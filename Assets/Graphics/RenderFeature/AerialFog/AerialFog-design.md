# AerialFog 设计文档

俯视角空气透视：自定义 URP 全屏雾 + 指数高度雾 + 剧情式天气过渡。

- 日期：2026-11-05
- 工程：`H:\unity_cundang\Render`（URP 14.0.11 / Unity 2022.3.53f1c1）
- 状态：设计待审

---

## 1. 意图

给俯视/上帝视角的场景做大气透视。目标画面：整片场景统一往远处压淡，同时低处的雾比高处浓，能看出平地和楼顶的差别；雾的状态可以由剧情切换，从"晴"平滑过渡到"浓雾"。

替代现在开着的 Unity 内置 Linear 雾。内置雾有三个绕不过去的短板：

1. 只对 Shader 里写了 `ComputeFogFactor` 的材质生效。这个工程的描边 Pass、部分特效、贴花都不吃它，远景压不下去，画面会分层。
2. 只有一组 color/start/end，雾色不能随距离变化，也没有高度概念。
3. 只按物体着色器里的顶点/片元距离算，透明物件的排序和覆盖不可控。

要的是：**一个全屏后处理，读深度重建世界坐标，按指数高度场积分出光深，把整幅画面往雾色上压**。所有像素一视同仁，不管它是什么 Shader 画的。

## 2. 目标

- 一个全屏 Pass，把画面按视距和高度从清晰过渡到雾色。
- **高度雾**：密度随世界高度指数衰减，低处浓、高处淡，能在同一屏里区分平地和大楼。
- **天气过渡**：几套预设之间按时间平滑切换，某一帧的雾是两套预设的插值结果。剧情式手动触发，不做区域触发、不做自动循环。
- 距离区间、最大浓度、曲线、近远雾色、高度参数全部可调，运行时可改。
- 不依赖物体 Shader，透明物体、描边、粒子、贴花统一处理。
- 单 Pass，无 compute，无额外 RT 链。

## 3. 非目标

- 不做真正的体积雾（froxel、光轴、雾中阴影）。
- 不做太阳方向散射 / 单次散射的相位函数。
- 不做风的流动、噪声扰动、雾的形态动画。
- 不做按区域的雾（走进 Volume 范围变浓）。相机是上帝视角，不跟角色走，这套用不上。
- 不做昼夜循环 / 自动天气轮换。
- 不做相机缩放时的自动参数标定（第一版固定标定，留扩展点，见第 10 节）。

## 4. 反目标（做到了也算失败）

- 为了"看起来像雾"把参数做成十几个看不懂的系数。
- 依赖 URP 包里被改过的私有接口。这个工程的 URP 是标准 registry 包，不许碰。
- 雾把 Preview、Reflection、Overlay 相机一起糊掉。
- 天气过渡在切换瞬间出现跳变、色彩断层或浓度尖峰。
- 屏幕空间雾的固有缺陷（见 7.4）被藏起来不写在文档里。

## 5. 约束（实测，不是估计）

### 5.1 相机与分辨率

`Main Camera`（场景里的俯视相机，`Untagged`，depth 0）：

| 项 | 值 |
| --- | --- |
| 投影 | 透视，FOV 40 |
| 朝向 | 欧拉 (80, 270, 0)，即俯角 80° |
| 位置 | (3.22, 18.06, 0) |
| near / far | 0.3 / 1000 |
| 抗锯齿 | SMAA |
| 深度图 | 需要（`requiresDepthTexture = true`） |

分辨率目标 **3840×2160**（编辑器 Game View 当前是 2048×2048，那是缩放预览，不是发布尺寸）。HDR 开，MSAA 1，项目 `colorSpace = Linear`，DX11，reversed-Z。

`RoleCamera`（`MainCamera` tag，FOV 20，贴角色）是测试用相机，**被雾误伤无所谓**，不为它做任何过滤。

### 5.2 这台相机下的距离分布

沿视线到地面的距离（相机高度没变，这几个数仍然有效）：

| 屏幕位置 | 视距 |
| --- | --- |
| 顶部（NDC y=0.95） | 20.35 m |
| 中间 | 18.95 m |
| 底部（NDC y=0.05） | 18.10 m |

**场景内容换过一次。** 2026-11-05 时是个半径 1.28 米、高 3.5 米的展示台，深度只有 0.69 米，高度雾基本看不出来。2026-11-06 换成了一张 33×58 米的地图（312 个 renderer），沿相机前向的深度 **15.6 → 26.4 米**，世界高度 **−13.4 → 4.5 米**。这份文档里的标定值以新场景为准，旧值在 8.1 里留了记录。

### 5.3 渲染管线现状

- URP 14.0.11 标准包，`Piu*` 私有接口不存在，`UrpBlitCompat.FinalBlit` 可用。
- Renderer `Piu_URP_Forward+_Renderer`，模式 Forward，Intermediate Texture = **Always**。
- 列表里有 `SwapAB`（`PiuFinalBlitRenderFeature`）在 `AfterRenderingPostProcessing` (600) 做唯一的 Final Blit。**关掉它画面会停在中间 RT 上。**
- `m_RendererFeatures` 里已有 `BlindFogRenderFeature`（索引 5）和 `SSFogRenderFeature`（索引 7），都 `active = false`，材质为空，工程里没有配套 Shader。**用户已确认这是旧项目的方法，可以随便改或删。**
- `Piu_URP_UI_Renderer` 上有一个 URP 自带的 `FullScreenPassRendererFeature`，关着。
- 场景里有一个 Global Volume：Bloom、Tonemapping、ChromaticAberration 三个 override 是开的。
- `_CameraDepthTexture` 已启用，拷贝时机 After Opaques。

### 5.4 现在开着的内置雾

运行时 `RenderSettings.fog = true`，Linear，color (0.32, 0.325, 0.406)，start −11.7，end 27.01，density 0.14。

但 `SampleScene.unity` 的 YAML 里是 `m_Fog: 0`，说明编辑器里有未存盘的改动。**上线前必须把 `RenderSettings.fog` 关掉**，两套雾叠加会互相污染。

## 6. 方案

### 6.1 手写 Renderer Feature + HLSL（采用）

新写 `AerialFogRenderFeature` + `AerialFog.shader`，参考 URP 14 官方 `FullScreenPassRendererFeature` 的做法：`ResetTarget()` → 拷贝当前颜色到临时 RT → `CoreUtils.SetRenderTarget(cameraColorTargetHandle)` → `DrawProcedural` 画一个全屏三角形。

优点：参数、数学、插入点全在手里；不受 Shader Graph 的节点组织方式限制；插在 SwapAB 之前能拿到后处理后的颜色。

### 6.2 Shader Graph Fullscreen + URP Full Screen Pass（未采用）

零 C#，节点连线，调参直观。放弃原因：指数积分 + 高度解析解 + 天空分支 + 近远色插值在节点图里会连成一团；而且它和现有 `SwapAB` 抢 Final Blit 的顺序，出问题难查。

### 6.3 调内置 Linear 雾（未采用，保留为对照基线）

零代码，但只影响写了雾的 Shader，也没有高度概念。任何时刻把 `RenderSettings.fog` 打开当对照组，一眼能看出自定义版多了什么。

### 6.4 高度雾的两种模型（选了 6.4.1）

**6.4.1 一个密度场 + 距离裁切（采用）**

```hlsl
float2 密度随高度指数衰减；
沿视线解析积分出光深 τ；
距离区间只缩放 τ，不参与密度计算。
```

参数十个，语义硬：高度管"雾在哪里浓"，距离管"从哪开始出现、到哪达到标称浓度"。天气预设写起来就是一组密度/衰减/基准加颜色。

**6.4.2 两层叠加（未采用）**

距离雾、高度雾各一套参数、各自有颜色，按 `1-(1-a)(1-b)` 合成。物理上两种介质确实该分开算，调的时候也不打架，但参数翻倍，两层颜色怎么合还要额外定规则，对"整片统一"的诉求来说收益不明显。

### 6.5 天气过渡的三种驱动方式（选了 6.5.1）

**6.5.1 剧情式手动切换（采用）**

`AerialFogController` 挂在场景里，持有一个预设数组，代码或 Inspector 按钮调用 `TransitionTo(索引, 时长)`，在时长内做 smoothstep 插值。触发时机由上层（关卡脚本、Timeline、对话系统）决定。

**6.5.2 自动循环（未采用）**：按时间轴首尾相接循环，适合昼夜，与"剧情式"需求不符。

**6.5.3 按区域 Volume（未采用）**：做成 `VolumeComponent`，走进范围自动变浓。用户明确说相机是上帝视角、不跟角色走，这个场景不成立。

## 7. 设计

### 7.1 雾模型

像素的视距记为 `d`（相机到该像素世界点的直线距离，米）。

```hlsl
float u  = saturate((d - _FogStart) / max(_FogEnd - _FogStart, 1e-4));
float mask = pow(max(u, 1e-5), _FogCurve);       // 距离裁切，0..1
float tau  = HeightFogOpticalDepth(camPos, posWS, _Density, _HeightFalloff, _BaseHeight);
float t    = (1.0 - exp(-tau * mask)) * _MaxOpacity;
float3 fogCol = lerp(_FogColorNear, _FogColorFar, u);
color = lerp(color, fogCol, t);
```

`mask` 乘在 τ 上而不是乘在最终颜色上，含义是"雾从 `FogStart` 开始出现，到 `FogEnd` 达到标称密度"。物理上可解释成雾层的边界，美术上就两个数。

`pow` 的第一个参数必须避开 0。`pow(0, x)` 在部分平台上返回 NaN 或 1，`u` 在 `FogStart` 内侧正好是 0，所以套一层 `max(u, 1e-5)`——`1e-5` 的 1.5 次方是 3e-8，视觉上没有差别。

`_FogEnd <= _FogStart` 时 `u` 恒等于 1，画面直接糊死。Feature 的 `OnValidate` 里把 `FogEnd` 夹到 `FogStart + 0.01` 以上。

### 7.2 高度雾解析积分

密度场：

```
density(h) = _Density * exp(-_HeightFalloff * (h - _BaseHeight))
```

沿视线从相机 `p0` 积分到目标点 `p1`。高度沿视线线性变化，所以闭式可解：

```hlsl
float HeightFogOpticalDepth(float3 p0, float3 p1, float density, float falloff, float baseH)
{
    float L  = distance(p0, p1);
    if (L < 1e-4) return 0.0;

    float dh = p1.y - p0.y;
    float k  = falloff * dh;

    // 相机所在高度的密度，指数截断防止低洼处数值爆掉
    float d0 = density * exp(min(-falloff * (p0.y - baseH), 10.0));

    // ∫0^L exp(-falloff*(p0.y + dh*t/L - baseH)) dt 的闭式解
    float integral = abs(k) < 1e-4 ? 1.0 : (1.0 - exp(-k)) / k;

    return d0 * L * integral;
}
```

推导：令 `s = t/L`，`h(s) = p0.y + dh*s`，

```
∫0^L exp(-falloff*(h(s) - baseH)) ds
  = exp(-falloff*(p0.y - baseH)) * L * ∫0^1 exp(-k*s) ds
  = exp(-falloff*(p0.y - baseH)) * L * (1 - exp(-k)) / k
```

`k → 0` 时 `(1 - exp(-k))/k → 1`，用 `abs(k) < 1e-4` 分支兜住除零。

三种情形的行为都对：

| 情形 | k | integral | 结果 |
| --- | --- | --- | --- |
| 平视 | ≈ 0 | 1 | `τ = d0 · L`，均匀密度 |
| 俯视（往下看，雾更浓） | < 0 | > 1 | 平均密度高于相机处 |
| 仰视（往上看，雾更淡） | > 0 | < 1 | 平均密度低于相机处 |

**这台相机上的量级**：`p0.y = 18.06`、`p1.y ≈ 0`、`dh ≈ -18.06`。取新场景"薄雾"档的 `density = 0.0475`、`falloff = 0.06`、`baseHeight = 0`，边缘点 `L ≈ 26`：

```
d0     = 0.0475 * exp(-0.06*18.06) = 0.0160
k      = 0.06 * (-18.06)          = -1.084
integral = (1 - exp(1.084)) / (-1.084) = 1.805
τ      = 0.0160 * 26 * 1.805      = 0.751
```

`(1 - exp(-0.751)) * maxOpacity(0.85) = 45%`，正是 7.8.1 表里薄雾档的边缘覆盖率。中心点 `L ≈ 18.3`、距离 mask 只有 0.116，覆盖率降到个位数，层次就出来了。

这个 `density` 是按覆盖率反推的，不是拍的：`τ` 和 `density` 成正比，所以换场景时先定目标覆盖率，再按 `τ = -ln(1 - t/max)` 反解 `density`。旧展示台场景标的是 0.14（见 8.1），不要混用。

### 7.3 世界坐标重建

```hlsl
float rawDepth = SampleSceneDepth(uv);
float3 posWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
```

- `ComputeWorldSpacePosition` 在 `Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl`（第 1327 行），三参数版本存在，内部调 `ComputeClipSpacePosition(positionNDC, deviceDepth)`，接受 **0..1 的 UV**，并在 `UNITY_UV_STARTS_AT_TOP` 下处理 Y 翻转。直接传 `uv`，不要再自己 `*2-1`。
- `UNITY_MATRIX_I_VP` 在 `Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl` 第 196 行定义（`#define UNITY_MATRIX_I_VP unity_MatrixInvVP`），URP 自己的 Decal 就是这么用的。
- reversed-Z 由反投影矩阵本身处理，不用自己判。

### 7.4 天空、透明、主角

**天空**：天空盒不写深度，重建出的点会落在 far 平面（1000 米），τ 直接顶满，天空变成一片纯色。

```hlsl
float linear01 = Linear01Depth(rawDepth, _ZBufferParams);
bool isSky = (linear01 >= 1.0 - 1e-4);
if (isSky) { d = _FogEnd; }
```

别用 `rawDepth <= 1e-6` 判天空。reversed-Z 下远处是 0，看着没问题，但换到不支持 reversed-Z 的平台（GLES 系）方向就反了。`Linear01Depth` 平台无关。

天空单独乘 `_SkyFogAmount`（0..1）。补一句实测推论：俯角 80°、垂直半角 20°，最上方那条视线也是向下 60°，离地平线还很远——**这台相机理论上拍不到天空**，`isSky` 分支在当前配置下不会触发。保留它是为了搬到别的相机上能用。

**透明物体**：屏幕空间雾的固有缺陷。透明物件不写深度，`SampleSceneDepth` 拿到的是它**背后**的不透明物的深度，所以透明特效的雾会比周围略淡。这是这套方案的已知边界，不是 bug。要更准只能上 per-object 雾或体积雾，超出本次范围。

**主角**：按默认参数，画面中央（视距 18.95）的雾是十几到二十几的百分比，站在那里的角色会一起吃。轻度雾化通常是对的，角色在场景里就该同一个大气。如果调重之后角色糊了，把 `FogStart` 提到角色视距之上就行，不预留角色 Mask。

**相机过滤**：跳过 `CameraType.Preview`、`CameraType.Reflection`，以及 `renderType == CameraRenderType.Overlay` 的相机。**Scene View 要保留**，不然在编辑器里调参得反复进 Play 模式。

Overlay 必须跳：它叠加在 base 相机的结果上，再雾一遍就是双重雾。当前工程两个相机的 stack 都是空的，但 `UniversalAdditionalCameraData` 支持 stack，不防一手迟早踩。

### 7.5 插入点

`RenderPassEvent = BeforeRenderingTransparents`（450）。

**这个事件是实测定下来的，不是为了好看才选的。** 它是唯一一个能同时满足两件事的时机：

- **在不透明物体之后**（URP 主不透明在 250，Copy Depth 在 300），`_CameraDepthTexture` 已经有地面和角色的深度，雾能算对距离；
- **在透明物体之前**（URP 透明在 500，`MainPlayTransparentRenderer` 在 550，`PandaPass` 在 500），所有 Transparent 队列的东西都画在雾**之上**，不会被雾色覆盖。

一开始放在 600（`AfterRenderingPostProcessing`），结果所有 queue 3000 的物体——烟雾、打击特效、角色死亡溶解——都被雾压没了。原因见 7.3：透明物体不写深度，雾读到的是它们**背后**的距离；背后一空，雾就按远平面算，等于把透明物整片涂成雾色。挪到 450 之后，透明物体画在雾之后，天然免疫。

**代价，两条，都是真的：**

**透明物完全不吃雾。** 不是说"少一点"，是一点都没有。远处的烟、飞在半空的打击特效，不会跟着远景一起变淡，可能显得"贴"在画面上。这是用空间换正确性的取舍——先保住所有透明物可见，再谈远处的氛围。真需要某些透明物也吃雾，只能走 7.4 说的"透明物改不透明队列 + AlphaTest"那条路。

**雾会进 URP 后处理。** Bloom、色调映射、SMAA 都会作用在雾上。卡通管线里 Bloom 会把雾边晕开，色调映射会改雾色——**Inspector 里填的雾色不再等于最终画面上的颜色**，这一点和放在 600 时相反。调色的时候得连着后处理一起看。

**不能放在 600 的原因**（留个记录，免得以后有人改回去）：`SwapAB`（`PiuFinalBlitRenderFeature`）在 600 做唯一的 Final Blit。放在 600 时必须排在它前面，否则雾会画在 back buffer 上、被中间 RT 覆盖，等于没做，而且 Console 什么都不报。450 比 600 早，和 SwapAB 不冲突，没有这个顺序问题。

### 7.6 参数

雾参数打成一个可序列化结构体，预设、Controller、Feature 三处共用同一份定义：

```csharp
[Serializable]
public struct AerialFogParams
{
    public float fogStart;        // 雾开始的视距（米）
    public float fogEnd;          // 达到标称密度的视距（米）
    public float fogCurve;        // 距离裁切曲线，1 = 线性
    public float density;         // 基准高度处的密度（1/米）
    public float heightFalloff;   // 高度衰减（1/米），越大雾越贴地
    public float baseHeight;      // 基准高度（世界 Y）
    public float maxOpacity;      // 浓度上限
    public Color colorNear;       // fogStart 处的雾色
    public Color colorFar;        // fogEnd 处的雾色
    public float skyFogAmount;    // 天空被雾化的比例
}
```

默认值（按当前 33×58 米地图标定，等于"薄雾"那档）：

| 字段 | 默认 | 说明 |
| --- | --- | --- |
| `fogStart` | 15.5 | 场景近端深度约 15.6，从这里开始起雾 |
| `fogEnd` | 26.0 | 场景最远约 26.4，到这里接近满额 |
| `fogCurve` | 1.5 | 中心和四周的过渡斜率 |
| `density` | 0.0475 | 按 45% 边缘覆盖率反推，见 7.2 和 7.8.1 |
| `heightFalloff` | 0.06 | 匹配 18 米相机高度，值太大雾全贴地看不见 |
| `baseHeight` | 0.0 | 场景地面在 y≈0 |
| `maxOpacity` | 0.85 | 留一点底透，避免远景死白 |
| `colorNear` | (0.58, 0.56, 0.55) | 中性暖灰，别偏黄 |
| `colorFar` | (0.62, 0.70, 0.82) | 提亮偏冷，拉开空气感 |
| `skyFogAmount` | 1.0 | 背景（无深度像素）也跟着雾化 |

Feature 上还有一个不属于 `AerialFogParams` 的字段：

| 字段 | 默认 | 说明 |
| --- | --- | --- |
| `RenderPassEvent` | `BeforeRenderingTransparents` | 插入时机，见 7.5。改它会直接决定透明物看不看得见，不是随便调的 |
| `debugMode` | 0 | 0 正常出图；1~6 出灰度调试图，见 8.1 |

**注意这台相机的距离分布是径向的，不是上下。** 俯角 80° 往下看，画面中心是最近的地面，四周才是远处，所以雾呈现为"中心清晰、四周发白"的环形渐变，屏幕上方略强于下方。这和一般第三人称的"上下渐变"直觉不同，但物理上是对的。7.1.1 那张按 NDC 上下沿算的表只是估算屏幕跨度，实际分布以径向为准。

### 7.7 天气系统

四个文件：

**`AerialFogPreset : ScriptableObject`** —— 一套天气就是一个资产，持有 `AerialFogParams` 和一个显示名。`CreateAssetMenu` 挂在 `Piu/Aerial Fog Preset`。

**`AerialFogController : MonoBehaviour`** —— 挂在场景里：

```csharp
public AerialFogPreset[] presets;
public int startIndex;
public float defaultTransitionDuration = 3f;

public void TransitionTo(int index, float duration = -1f);  // 开始过渡，-1 = 用 defaultTransitionDuration
public void SnapTo(int index);                              // 立即切换，不走过渡
public AerialFogParams CurrentParams { get; }               // 当前插值结果
```

`TransitionTo` 把当前值记为 `_from`、目标预设记为 `_to`、`_elapsed = 0`，`duration` 传 -1 时取 `defaultTransitionDuration`，目标索引和当前索引相同时直接 return。`Update` 里：

```csharp
if (_elapsed >= _duration) { CurrentParams = _to; return; }   // 过渡结束，别每帧重复插值
_elapsed += Time.deltaTime;
float raw = _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f;
float s = raw * raw * (3f - 2f * raw);                       // smoothstep，起止都缓
CurrentParams = Lerp(_from, _to, s);
```

用 smoothstep 而不是线性：线性的启动和收尾都有可见的"折角"，尤其是浓度从 0.03 跳到 0.08 这种幅度。smoothstep 两端导数为 0，观感上不会有顿挫。

**`AerialFogRenderFeature`** —— 每帧从 Controller 取参数，没有 Controller 就用自己 Inspector 上的默认值：

```csharp
var p = AerialFogController.Instance != null
    ? AerialFogController.Instance.CurrentParams
    : inspectorParams;
```

**`AerialFogControllerEditor`** —— `Assets/Editor/` 下的自定义 Inspector。默认字段照旧显示，下面加一排预设按钮：Play 模式走 `TransitionTo`，非 Play 模式走 `SnapTo`，点完 `SceneView.RepaintAll()`。这样在编辑器里调天气不用进 Play、不用写代码。

`Instance` 的 getter 是惰性的：`null` 时用 `FindObjectOfType` 找一次（每帧最多一次），因为编辑器里 static 会随 domain reload 丢失。找到后如果 `_initialized` 还是 false，补一次 `SnapTo(startIndex)`，否则 `CurrentParams` 全是 0，画面会没雾。

不用 Volume：项目虽然有 Global Volume 在跑 Bloom/Tonemapping，但 Volume 解决的是"按空间位置和优先级混合"，换成剧情式的"按时间插值"反而要绕着它走。

### 7.8 交付物

```
Assets/Graphics/RenderFeature/AerialFog/
    AerialFogParams.cs              参数结构体 + 插值
    AerialFogPreset.cs              预设资产
    AerialFogController.cs          天气过渡控制器
    AerialFogRenderFeature.cs       Renderer Feature
    AerialFog.shader                Piu/AerialFog
    AerialFog.mat
    Presets/
        AerialFog_None.asset        通透
        AerialFog_Clear.asset       晴
        AerialFog_Mist.asset        薄雾
        AerialFog_Heavy.asset       浓雾
        AerialFog_Total.asset       全遮
    AerialFog-design.md             本文档

Assets/Editor/
    AerialFogControllerEditor.cs    预设按钮面板
```

### 7.8.1 五档预设

预设数组按雾量从弱到强排列，`TransitionTo` 的索引就是数组下标。所有档共用同一组 `fogStart = 15.5` / `fogEnd = 26.0`（按当前场景的深度范围 15.6~26.4 米标定），差异只在 `density` 和 `maxOpacity`。

| 索引 | 名字 | density | maxOpacity | 场景边缘覆盖率 |
| --- | --- | --- | --- | --- |
| 0 | 通透 | 0.0066 | 0.20 | 2.0% |
| 1 | 晴 | 0.0285 | 0.55 | 20.0% |
| 2 | 薄雾 | 0.0475 | 0.85 | 45.0%（默认） |
| 3 | 浓雾 | 0.0980 | 0.95 | 75.0% |
| 4 | 全遮 | 0.3150 | 1.00 | 99.3% |

覆盖率按 `(1 - exp(-τ)) · maxOpacity` 算，`τ = density · exp(-falloff · 18.06) · L · 1.805`，`L = 26`，`falloff = 0.06`。这张表是标定换算的依据，换场景后照同样方式反推。

Feature 挂进 `Piu_URP_Forward+_Renderer`，位置在 `SwapABRenderFeature` 之前。

实现完成后清理旧壳：从 `m_RendererFeatures` 移除 `BlindFogRenderFeature`（索引 5）和 `SSFogRenderFeature`（索引 7），删掉这两个脚本，同步改 `Piu_URP_Forward+_Renderer.md` 的 5.4 / 5.6 节。**清理放在 AerialFog 验证通过之后单独做**，不要和新功能混在一次提交里。

## 8. 验证

这台相机的相机高度和朝向没变（18.06 米、俯角 80°、FOV 40），但场景已经换成了一张 33×58 米的地图，深度范围 15.6~26.4 米、高度范围 −13.4~4.5 米（312 个 renderer）。验证分三步。

**第一步，静态标定**：Feature 打开、把 Controller 停用（用 Inspector 默认值）。画面中心应比边缘明显清晰，渐变连续无带状，描边和透明特效跟着变淡。

**第二步，高度雾**：场景现在有 18 米高差，理论上够验了——找一处地势最低的坑和最高的建筑，比较两者在同一屏上的雾浓度。预期：低处的雾明显浓于高处。这一步同时暴露 `heightFalloff` 和 `baseHeight` 调错的情况。

**第三步，天气过渡**：Controller 上放五套预设，代码里调 `TransitionTo(4, 3f)`。预期：三秒内平滑变浓，无跳变、无色彩断层、无浓度尖峰；再调 `TransitionTo(0, 3f)` 能平滑回来。连续快速切换（0→4→2→0）不累积误差、不卡在中间状态。

另外两项：

- `RenderSettings.fog` 关掉，画面不出现两层雾叠加的脏感。
- 4K 下用 Profiler 量这一帧的 GPU 时间，记下来。超过 2ms 就要考虑半分辨率（见第 9 节）。

### 8.1 已完成的验证

**2026-11-05（旧展示台场景，18 米相机 / 0.69 米场景深度）**

| 项 | 结果 |
| --- | --- |
| Shader 编译 | 通过，Console 无 error |
| 距离重建 | `_DebugMode = 3` 画面中心 ≈ 18 米、边角 ≈ 20 米，与实测相机动线一致 |
| 高度雾积分 | `_DebugMode = 5` 整屏 τ ≈ 0.35（当时 density 0.03），与 7.2 手算 0.348 吻合 |
| 距离裁切 | `_DebugMode = 4` 呈径向渐变，中心 0、边角 1 |
| 天气过渡 | `TransitionTo` 3 秒走完，中途打断无跳变，最终准确回到目标值 |
| 折线 | `pow(0, x)` 与 `FogEnd < FogStart` 两个坑都按设计挡住了 |

**2026-11-06（换成 33×58 米地图后重标）**

场景内容换掉了，相机参数没变。旧参数（`fogStart 18` / `fogEnd 20.5` / `density 0.14`）在新场景里直接失效——`end` 只覆盖到 20.5 米，而场景最远到 26.4 米，导致画面一大片全是满额雾。重新标定为 `start 15.5` / `end 26.0`，五档密度按边缘覆盖率 2% / 20% / 45% / 75% / 99% 反推。

| 项 | 结果 |
| --- | --- |
| 五档预设 | 0 通透几乎看不出雾，4 全遮基本只剩中心一小块，中间三档层次可辨 |
| 薄雾档（默认） | 中心保留细节与色彩，左右边缘和上下背景发白褪色，过渡连续无带状 |
| 编辑器内切档 | 非 Play 模式下 Inspector 按钮即点即生效，不需要进 Play |

**还没验证**：高度雾的实际形态。新场景有 18 米高差，条件够了，但还没找低洼处和高楼做对比。`_DebugMode = 6` 的天空遮罩仍恒为 0。

**调试入口**：`AerialFogRenderFeature.debugMode` 取值 0～6，0 正常出图，1 原始深度、2 线性深度、3 视距、4 距离裁切、5 光深、6 天空遮罩。调参卡住时先看这几张灰度图，比猜参数快。

## 9. 留给实现的决定

下面这些不改变做什么，只影响怎么写，由实现时定：

- 绘制方式用 `CoreUtils.SetRenderTarget(cmd, cameraColorTargetHandle)` + `cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3, 1)`，和 URP 14 官方 `FullScreenPassRendererFeature` 一致（`Runtime/RendererFeatures/FullScreenPassRendererFeature.cs` 第 164~194 行）。不要走 `Blitter.BlitCameraTexture` 来回拷两次，那个写法在 `Intermediate Texture = Always` 下多一次全屏拷贝，而且和官方路径不一致、出问题不好对照。
- 颜色拷贝用 `RTHandle` + `RenderingUtils.ReAllocateIfNeeded`，和 `BlindFog` 现在的写法一致，已验证可用。
- Shader 里 `uv` 要用 `_BlitScaleBias` 变换（`uv * _BlitScaleBias.xy + _BlitScaleBias.zw`），因为拷贝到临时 RT 时 scale-bias 可能不是 (1,1,0,0)。
- 参数每帧 `SetFloat` / `SetColor` 推到 material 上，不依赖 `.mat` 里存的值，这样 Inspector 和 Controller 改了都立刻生效。
- 半分辨率：雾是低频信号，半分辨率做再双线性放大完全够用，4K 下能省一半带宽。第一版全分辨率，量完 GPU 时间再决定。
- `AerialFogPreset` 里的 `params` 用 `[SerializeField]` 私有字段加公开属性，还是直接 public 字段。选 public，和项目里现有代码风格一致（`BlindFogSetting` 就是 public）。

## 10. 遗留问题

- **`BlindFog` / `SSFog` 还没清理**。两个 Feature 仍在 `m_RendererFeatures` 索引 5 和 7 上，`active = false`，脚本和材质都还在。用户已确认可以删，但删除会改 `Piu_URP_Forward+_Renderer.asset` 并波及 `Piu_URP_Forward+_Renderer.md` 的 5.4 / 5.6 两节，所以留到 AerialFog 效果验收通过之后单独做一次。
- **`AerialFog.mat` 的 Shader 属性没有落盘**。所有参数由 Feature 每帧 `SetFloat` / `SetColor` 推过去，`.mat` 里存的是全零。这是刻意的（Inspector 改了立刻生效），但也意味着**单独把这个材质挂到别处不会有任何效果**，必须走 Feature。
- **高度雾未做形态验证**，见 8.1。
- **相机缩放要重标参数**。`FogStart` / `FogEnd` 是绝对米数，标定前提是相机离地 18 米。用户确认第一版按固定机位做，观战系统如果哪天加了推拉缩放，这两个数就全废。到时候给 Feature 加 `AutoFitToCameraHeight`。
- **高度雾在高层建筑场景要重标**。`heightFalloff = 0.06` 配 18 米相机合适，换到有 50 米高楼的地图，雾会全部压在 30 米以下。`baseHeight` 和 `falloff` 都是场景相关的。
- 内置 `RenderSettings.fog` 已在场景保存时关掉，`SampleScene.unity` 现在存的是 `m_Fog: 0`，和 YAML 一致了。
- 把 Feature 加进 renderer 列表时是**追加到末尾**。这次是脚本插到 `SwapABRenderFeature` 之前的（索引 8，SwapAB 在 9），手工加的话必须在 Inspector 里拖顺序。
