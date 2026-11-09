# AerialFog 踩坑记录

这份文档只记「试过、翻车、最后怎么定」的部分，设计推导见 `AerialFog-design.md`，参数标定见 `AerialFog-plan.md`。

所有坑都围绕同一件事：**AerialFog 是全屏后处理，靠 `_CameraDepthTexture` 反推每个像素的世界坐标。谁没写进这张深度图，雾就会拿它背后的深度去算。**

---

## 1. Scene View 的深度图来自 depth prepass，不是 CopyDepth

**现象**：场景视图里，眼睛上半截（背后是天空）被雾涂成雾色，下半截（背后有地面）正常。Game 视图却完全正常。

**原因**：Scene View 相机的 `_CameraDepthTexture` 由 `DepthOnly` prepass 生成；Game 相机走的是 `CopyDepth`（从 depth attachment 拷）。

**翻车点**：`M_Soldier_Eye` 的 `disabledShaderPasses` 里禁用了 `DepthOnly`，眼睛永远进不了 Scene View 的深度图。

**结论**：角色材质不要随手禁用 `DepthOnly`。眼睛材质把 `DepthOnly` 重新启用后，Scene View 恢复正常。

---

## 2. 描边在 Event 350 画，深度进不了深度图

**现象**：body 描边背后是天空时，整条描边被涂成雾色；背后有地面时正常。

**原因**：`NiloToonOutlineRenderObjects`（RenderObjects，`Event = 350 BeforeRenderingSkybox`）画描边。URP 的 `CopyDepth` 在 300 就把深度快照了，描边 350 写进 depth buffer 也进不了 `_CameraDepthTexture`。

**翻车点**：试过把描边 pass 的 `LightMode` 改成 `SRPDefaultUnlit` 塞进不透明队列——薄物体（耳朵、袖口）的背面外扩直接包住整个模型，耳朵被描边色糊满。

**结论**：350 不是上一个项目的残留，是「所有不透明物之后、天空盒之前」这个唯一可用的位置。描边不能进不透明队列。

---

## 3. 描边挪到 AfterRendering(1000) → 过曝

**现象**：描边画在 1000（后处理之后），颜色没经过 tonemap / color grading，和画面其它部分对不上，显得过亮。

**结论**：描边必须落在后处理之前（`BeforeRenderingPostProcessing` 附近）。

---

## 4. 描边自己算雾 + lerp 到亮雾色 → Bloom 光晕

**现象**：给描边加 `AerialFogApply` 后，角色下半身出现大面积白色光晕。

**原因**：描边在 `BeforeRenderingPostProcessing` 之前画，颜色会进 Bloom；`AerialFogApply` 把描边 lerp 向雾色，而当时雾色是接近白青的高亮值，高亮部分被 Bloom 直接放大。

**结论**：描边不要直接 lerp 到高亮雾色。要么不吃雾，要么用单独的低强度计算，不要和 Bloom 抢亮度。

---

## 5. 自己 copy depth attachment 给雾用 → 画面花

**尝试**：在雾之前用 `Hidden/Universal Render Pipeline/CopyDepth` 再拷一份 depth attachment，让雾读最新深度。

**结果**：MSAA 采样 / 格式没对齐，脸和耳朵出现粉色、蓝色斑块，比原问题还严重。

**结论**：URP 的 `CopyDepthPass` 内部处理了 MSAA resolve 和 y-flip，自己在 feature 里重写很容易翻车。要改深度来源，优先用 URP 的 `CopyDepthPass` 而不是手写 Blit。

---

## 6. 雾的 Event 挪到 440 → 全白

**现象**：把雾的 `renderPassEvent` 从 450 改成 440，整幅画面发白，角色像被光晕包住。

**原因**：440 时颜色 / 深度目标的状态不对，雾读到全远深度，等于把整个屏幕当天空涂满。

**结论**：雾的 Event 不要乱调。450（`BeforeRenderingTransparents`）能用就用，动它前先看 `AerialFog-design.md` 7.5。

---

## 7. 角色材质 renderQueue 用了 Geometry-2 / Geometry-3

**现状（改前）**：Body / Face 是 1998（`Geometry-2`），Eye 是 1997（`Geometry-3`）。

**问题**：这是上个项目为了压 z-fighting 留的后门，数字不直观，而且和描边、深度的排序互相牵连。

**结论**：已统一改回 `Geometry`(2000)。脸和眼睛的深度冲突用 `_OutlineZOffset`、`_OutlineZOffsetForFaceArea` 这类深度偏移解决，不要用 renderQueue 偏移。

---

## 8. 透明物免疫屏幕空间雾，要自己算

**原因**：雾在透明物之前画（450 之前），透明物画在雾之上，天然不吃雾。

**解法**：透明物 shader include `AerialFogCommon.hlsl`，片元里：

```hlsl
color.rgb = AerialFogApply(color.rgb, positionWS);
```

`AerialFogApply` 用物体自己的世界坐标算，和屏幕空间雾共用同一组 `_AerialFog*` 全局参数，不依赖深度。

---

## 当前确定的顺序

| 阶段 | Event | 内容 |
|------|-------|------|
| 不透明 | 250 | 角色主体（`UniversalForward` / 无 LightMode） |
| CopyDepth | 300 | URP 快照深度 |
| 雾 | 450 | `AerialFogRenderFeature`，`BeforeRenderingTransparents` |
| 透明 | 450 | 透明物，不吃屏幕空间雾，自己在 shader 里算 |
| 描边 | 500 | `NiloToonOutlineRenderObjects`，后处理之前，只保留自身颜色 |
| 后处理 | 600 | Bloom / tonemap |

**注意**：描边在 500 会盖在透明物上面。角色描边是细线，一般可以接受；如果以后要把描边压到透明特效下面，就得回到 350，同时接受「描边深度进不了深度图」。

---

## 一句话总结

- 深度图缺谁，雾就按谁背后的深度算。
- 描边、透明物这类「没有可靠深度」的东西，别让屏幕空间雾管，自己在 shader 里算。
- 描边不要进不透明队列，不要放到后处理之后，不要把亮雾色直接 lerp 上去。
- 雾的 Event、描边的 Event、材质的 Queue，这三个是连动的，改一个先想另外两个。
