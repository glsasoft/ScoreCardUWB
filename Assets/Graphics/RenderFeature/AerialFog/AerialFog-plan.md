# AerialFog Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 URP 14 工程里做出可调距离 + 指数高度 + 剧情式天气过渡的全屏空气透视雾。

**Architecture:** 一个 `ScriptableRendererFeature` 在 `AfterRenderingPostProcessing` 入队，把当前颜色拷到临时 RT，再用 `DrawProcedural` 画一个全屏三角形，片元里由深度重建世界坐标、解析积分指数高度场得到光深、与近远双色混合后写回。雾参数集中在 `AerialFogParams` 结构体，由 `AerialFogController` 在预设之间做 smoothstep 插值，Feature 每帧读取。

**Tech Stack:** Unity 2022.3.53f1c1、URP 14.0.11、HLSL、`Blitter` / `RTHandle` / `CommandBuffer.DrawProcedural`。

**Spec:** `Assets/Graphics/RenderFeature/AerialFog/AerialFog-design.md`

**Note on testing:** 这个工程没有测试基础设施（无 asmdef、无 Test Framework 配置、`Assets` 下零测试文件）。不为此引入一套，验证走编辑器内实测：编译通过 + Console 无报错 + 用 MCP 驱动 `execute_code` 检查运行时状态 + 截图目视。每条验证步骤都写明了预期输出。

## Global Constraints

- URP 是标准 registry 包 `com.unity.render-pipelines.universal@14.0.11`，不改包、不用反射、不依赖任何 `Piu*` 私有接口。
- Shader 名固定 `Piu/AerialFog`，材质 `AerialFog.mat` 放在 `Assets/Graphics/RenderFeature/AerialFog/`。
- 材质属性名（shader 全局）：`_FogStart` `_FogEnd` `_FogCurve` `_Density` `_HeightFalloff` `_BaseHeight` `_MaxOpacity` `_ColorNear` `_ColorFar` `_SkyFogAmount`。
- C# 字段名（结构体）：`fogStart` `fogEnd` `fogCurve` `density` `heightFalloff` `baseHeight` `maxOpacity` `colorNear` `colorFar` `skyFogAmount`。
- 默认值：`18.0 / 20.5 / 1.5 / 0.03 / 0.06 / 0.0 / 0.85 / (0.55,0.50,0.45) / (0.42,0.52,0.68) / 1.0`。
- Renderer Feature 必须排在 `SwapABRenderFeature` **之前**，事件 `AfterRenderingPostProcessing`。
- 代码不带 asmdef，进 `Assembly-CSharp`，可以 `using UnityEngine.Rendering.Universal;`。
- 所有面向使用者的注释和文档用中文。

## Review Focus

1. **`FogEnd` 被设得小于 `FogStart`** —— 用户手滑把两个数填反，期望画面不至于整片糊死，实际 `u` 恒为 1 直接全雾。`OnValidate` 必须把它夹回来。
2. **`u = 0` 处的 `pow(0, curve)`** —— 用户把 `FogCurve` 调到非整数（比如 1.5），期望 `FogStart` 内侧是干净的，实际部分平台给 NaN 或 1，画出黑块或横条。
3. **场景里没有 Controller** —— 用户只挂了 Feature 没搭天气系统，期望 Inspector 上的默认值直接生效。这时 Feature 不能因为 `Instance == null` 就跳过不画。
4. **相机叠了 Overlay** —— 用户接了 UI 相机做 stack，期望 UI 不被雾压两遍。Overlay 必须跳过。
5. **快速连切天气** —— 用户在 0→2 还没走完时又调 1，期望不卡死、不错乱、不累积漂移。`TransitionTo` 必须以当前插值结果为新起点。

---

### Task 1: 雾参数结构体与插值

**Files:**
- Create: `Assets/Graphics/RenderFeature/AerialFog/AerialFogParams.cs`

**Interfaces:**
- Consumes: 无。
- Produces:
  - `[Serializable] public struct AerialFogParams`，10 个 public 字段（名字见 Global Constraints，类型依次 `float`×7、`Color`×2、`float`×1，顺序：`fogStart, fogEnd, fogCurve, density, heightFalloff, baseHeight, maxOpacity, colorNear, colorFar, skyFogAmount`）
  - `public static AerialFogParams Lerp(in AerialFogParams a, in AerialFogParams b, float t)`
  - `public static AerialFogParams Default => ...`（Global Constraints 里的那组值）

- [ ] **Step 1: 写结构体与两个静态方法**

字段全部 public、`[Tooltip]` 用中文写。`Lerp` 里 float 用 `Mathf.Lerp`、Color 用 `Color.Lerp`，`t` 不做钳制（由调用方保证 0..1）。`Default` 用属性返回字面量构造的结构体。

- [ ] **Step 2: 让 Unity 编译并确认无报错**

用 MCP `mcp__unity__refresh_unity`（`compile: request`），然后 `mcp__unity__read_console` 取 error 级消息。
Expected: 0 条 error，Console 里能看到 `AerialFogParams` 编译通过（无 error 即通过）。

- [ ] **Step 3: 验证 `Default` 与 `Lerp` 的数值**

用 `mcp__unity__execute_code` 执行：

```csharp
var d = AerialFogParams.Default;
var mid = AerialFogParams.Lerp(d, d, 0.5f);
return $"{d.fogStart} {d.fogEnd} {d.fogCurve} {d.density} {d.heightFalloff} {d.baseHeight} {d.maxOpacity} {d.skyFogAmount} | mid.density={mid.density} color={mid.colorFar}";
```

Expected: 前八个值与 Global Constraints 的默认值逐位一致；`mid.density` 等于 `d.density`；`mid.colorFar` 等于 `d.colorFar`。

---

### Task 2: 雾 Shader

**Files:**
- Create: `Assets/Graphics/RenderFeature/AerialFog/AerialFog.shader`

**Interfaces:**
- Consumes: 无（纯 shader，参数全走 material 属性）。
- Produces: `Shader "Piu/AerialFog"`，单个 Pass（index 0），无 Cull/ZTest 需求，`ZWrite Off` `ZTest Always` `Cull Off`。片元读 `_BlitTexture`、`_BlitScaleBias`、`_CameraDepthTexture`、`_WorldSpaceCameraPos`、`unity_MatrixInvVP`。

- [ ] **Step 1: 写 shader 骨架与顶点**

`#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"`、`/ShaderLibrary/DeclareDepthTexture.hlsl`、`Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl`。顶点直接用 `Blit.hlsl` 的 `Vert`，UV 是 `varyings.texcoord`（**已经乘过 `_BlitScaleBias`，不要重复做**）。

- [ ] **Step 2: 写 `HeightFogOpticalDepth` 函数**

签名与实现照抄设计文档 7.2 的代码块：`float HeightFogOpticalDepth(float3 p0, float3 p1, float density, float falloff, float baseH)`。三处不能改：`d0` 的 `exp(min(..., 10.0))` 截断、`abs(k) < 1e-4` 分支、`(1.0 - exp(-k)) / k` 的符号。

- [ ] **Step 3: 写片元**

先 `SampleSceneDepth(uv)` → `ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP)` → `d = distance(posWS, _WorldSpaceCameraPos)`。天空分支用 `Linear01Depth(rawDepth, _ZBufferParams) >= 1.0 - 1e-4`，命中时把 `d` 置为 `_FogEnd` 并把 `mask` 乘上 `_SkyFogAmount`。然后按设计文档 7.1 的公式算 `u` / `mask` / `tau` / `t` / `fogCol`，最后 `lerp` 源色与雾色返回。源色用 `SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv)`。

- [ ] **Step 4: 确认 shader 编译通过**

`mcp__unity__refresh_unity`（`compile: request`）后 `mcp__unity__read_console` 过滤 `error`。
Expected: 0 条 error；Console 里没有 "Shader error in 'Piu/AerialFog'"。

- [ ] **Step 5: 建材质**

用 `mcp__unity__manage_material`（`action: create`, `path: Assets/Graphics/RenderFeature/AerialFog/AerialFog.mat`, `shader: Piu/AerialFog`），再反射确认 `AerialFog.mat` 的 shader 名是 `Piu/AerialFog` 且 `HasProperty("_Density")` 为 true。
Expected: 材质创建成功；`HasProperty` 对十个属性全返回 true。

---

### Task 3: Renderer Feature 接通

**Files:**
- Create: `Assets/Graphics/RenderFeature/AerialFog/AerialFogRenderFeature.cs`

**Interfaces:**
- Consumes: `AerialFogParams`（Task 1）、`Piu/AerialFog` 材质（Task 2）。
- Produces: `public class AerialFogRenderFeature : ScriptableRendererFeature`，Inspector 字段 `public Material material;` 与 `public AerialFogParams parameters;`，`public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;`。

- [ ] **Step 1: 写 Feature 与 Pass 类**

Pass 内部字段：`Material _material`、`AerialFogParams _params`、`RTHandle _copiedColor`、`ProfilingSampler`。`OnCameraSetup` 里 `ResetTarget()` 并 `RenderingUtils.ReAllocateIfNeeded(ref _copiedColor, desc, ...)`（`desc.msaaSamples = 1; desc.depthBufferBits = 0;`）。`Execute` 里按顺序：`Blitter.BlitTexture` 把 `cameraColorTargetHandle` 拷进 `_copiedColor`；`CoreUtils.SetRenderTarget(cmd, cameraColorTargetHandle)`；把十个参数 `SetFloat`/`SetColor` 到 `_material`；`cmd.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, 3, 1)`。`Dispose` 里 `_copiedColor?.Release()`。

`AddRenderPasses` 的过滤照设计文档 7.4：跳过 `CameraType.Preview`、`CameraType.Reflection`、`renderType == CameraRenderType.Overlay`；`material == null` 时 `Debug.LogWarning` 一次并 return。**不要**因为 `AerialFogController.Instance == null` 就 return——这时用 `parameters` 字段的值。

- [ ] **Step 2: 挂进 renderer 并确认排在 SwapAB 之前**

用 `mcp__unity__manage_graphics`（`action: feature_add`, `target: Piu_URP_Forward+_Renderer`, `feature_type: AerialFogRenderFeature`），然后反射读 `m_RendererFeatures` 列出顺序。
Expected: 列表里出现 `AerialFogRenderFeature`；若它在 `SwapABRenderFeature` 之后，用 `feature_reorder` 把它移到前面。

- [ ] **Step 3: 勾上 Active 并把材质拖进去**

用 MCP 设置该 Feature 的 `m_Active = 1` 与 `material` 指向 `Assets/Graphics/RenderFeature/AerialFog/AerialFog.mat`。先用默认 `parameters`，暂不接 Controller。

- [ ] **Step 4: 关掉内置雾，确认画面出雾**

用 `mcp__unity__manage_graphics`（`action: skybox_set_fog`, `fog_enabled: false`）关掉 `RenderSettings.fog`。截图对比开 / 关 Feature 两帧。
Expected: 关掉 Feature 时画面清晰；打开后画面顶部明显偏冷蓝、底部接近原色，渐变连续，没有横向色带，没有整屏糊死。

- [ ] **Step 5: 验证参数确实在生效**

`execute_code` 里把 `fogCurve` 从 1.5 改成 1.0 再截图，然后把 `fogStart` 改成 5.0 再截图。
Expected: `fogCurve = 1` 时中段雾明显变重；`fogStart = 5` 时整屏雾都变重（因为整条视线都超过起点）。

---

### Task 4: 天气预设与过渡

**Files:**
- Create: `Assets/Graphics/RenderFeature/AerialFog/AerialFogPreset.cs`
- Create: `Assets/Graphics/RenderFeature/AerialFog/AerialFogController.cs`
- Create: `Assets/Graphics/RenderFeature/AerialFog/Presets/AerialFog_Clear.asset`
- Create: `Assets/Graphics/RenderFeature/AerialFog/Presets/AerialFog_Mist.asset`
- Create: `Assets/Graphics/RenderFeature/AerialFog/Presets/AerialFog_Heavy.asset`
- Modify: `Assets/Graphics/RenderFeature/AerialFog/AerialFogRenderFeature.cs`（取参数的来源）

**Interfaces:**
- Consumes: `AerialFogParams`（Task 1）。
- Produces:
  - `public class AerialFogPreset : ScriptableObject`，字段 `public string displayName;` `public AerialFogParams parameters;`，`[CreateAssetMenu(menuName = "Piu/Aerial Fog Preset")]`
  - `public class AerialFogController : MonoBehaviour`，字段 `public AerialFogPreset[] presets;` `public int startIndex;` `public float defaultTransitionDuration = 3f;`，属性 `public AerialFogParams CurrentParams { get; }`，方法 `public void TransitionTo(int index, float duration = -1f)` `public void SnapTo(int index)`，静态 `public static AerialFogController Instance { get; }`

- [ ] **Step 1: 写 Preset 与 Controller**

Controller 的 `OnEnable` 检测 `Instance != null && Instance != this` 时 `Debug.LogWarning`，然后赋值；`OnDisable` 清空。`Awake` 里 `SnapTo(startIndex)` 初始化。`TransitionTo` 以 `CurrentParams` 作为 `_from`（这条是 Review Focus 第 5 条）、目标预设为 `_to`、`_elapsed = 0`、`_duration = duration < 0 ? defaultTransitionDuration : duration`；索引越界或缺预设时 `Debug.LogWarning` 并 return。`Update` 里按设计文档 7.7 的代码块插值，`_elapsed >= _duration` 之后直接置为 `_to` 并 return。

- [ ] **Step 2: 编译并确认无报错**

`mcp__unity__refresh_unity` + `read_console`（error）。
Expected: 0 条 error。

- [ ] **Step 3: 改 Feature 取参数的地方**

`SetupRenderPasses` 里改成：`var p = AerialFogController.Instance != null ? AerialFogController.Instance.CurrentParams : parameters;`，再传给 Pass。

- [ ] **Step 4: 创建三套预设并填值**

用 `mcp__unity__manage_scriptable_object`（`action: create`, `type_name: AerialFogPreset`）建三个资产，再 `modify` 分别写 `displayName` 和 `parameters`。取值：
- `Clear`（晴）：`density 0.012`，`maxOpacity 0.55`，其余同默认。
- `Mist`（薄雾）：即 Global Constraints 的默认值。
- `Heavy`（浓雾）：`density 0.08`，`maxOpacity 0.95`，`heightFalloff 0.04`，其余同默认。

- [ ] **Step 5: 场景里搭 Controller**

`manage_gameobject create` 一个空物体 `AerialFogController`，`manage_components add` 挂 `AerialFogController`，把三个预设按 Clear / Mist / Heavy 顺序填进 `presets`，`startIndex = 0`，`defaultTransitionDuration = 3`。

- [ ] **Step 6: 验证过渡平滑且可打断**

Play 模式，`execute_code` 依次执行并各截一帧：

```csharp
var c = AerialFogController.Instance;
c.TransitionTo(2, 3f);                 // 晴 -> 浓雾
// 等 1.5 秒后
c.TransitionTo(1, 3f);                 // 中途改主意切薄雾
```

Expected: 切换瞬间浓度不跳变；从浓雾回薄雾的方向正确；连续三次快速切（0→2→1→0，每次间隔 0.2 秒）结束后停在晴，`CurrentParams` 与 `Clear` 预设逐字段相等。

---

### Task 5: 清理旧壳与文档同步

**Files:**
- Modify: `Assets/Graphics/Setting/Piu_URP_Forward+_Renderer.asset`（移除两个 Feature 引用）
- Delete: `Assets/Graphics/RenderFeature/BlindFog/`（整个目录）
- Delete: `Assets/Graphics/RenderFeature/SSFog/`（整个目录）
- Modify: `Assets/Graphics/Setting/Piu_URP_Forward+_Renderer.md`（5.4 / 5.6 两节）
- Modify: `Assets/Graphics/RenderFeature/AerialFog/AerialFog-design.md`（第 10 节收尾）

**Interfaces:**
- Consumes: 无。
- Produces: 无（纯删除与文档）。

- [ ] **Step 1: 先从 renderer 列表移除**

`mcp__unity__manage_graphics`（`action: feature_remove`）移除 `BlindFogRenderFeature` 和 `SSFogRenderFeature`。反射确认 `m_RendererFeatures` 里两者都不再出现。
Expected: 列表里无这两个名字；`AerialFogRenderFeature` 仍在 `SwapABRenderFeature` 之前。

- [ ] **Step 2: 删掉两个脚本目录**

删 `Assets/Graphics/RenderFeature/BlindFog/` 和 `Assets/Graphics/RenderFeature/SSFog/`（含 `.meta`）。`mcp__unity__refresh_unity` 后 `read_console` 查 error。
Expected: 0 条 error；没有 "missing script" 警告。

- [ ] **Step 3: 改管线文档**

`Piu_URP_Forward+_Renderer.md` 的 5.4、5.6 两节改成"已被 `AerialFogRenderFeature` 取代，脚本与材质已删除"，并删掉第 2 节里提到 BlindFog、SSFog 的那半句。

- [ ] **Step 4: 设计文档收尾**

`AerialFog-design.md` 第 10 节删掉"BlindFog / SSFog 保留不动"那条，改成已清理；若有实现期改动（参数默认值、插入点顺序），同步回文档。

- [ ] **Step 5: 全量回归**

关掉 Feature 再打开、切一遍三套天气、4K 分辨率下跑 60 帧看帧时间。
Expected: 无报错；切换正常；帧时间记录在设计文档第 8 节补上。
