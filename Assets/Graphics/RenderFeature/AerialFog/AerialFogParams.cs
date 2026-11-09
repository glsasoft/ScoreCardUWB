using System;
using UnityEngine;

/// <summary>
/// AerialFog 的全部可调参数。
/// 预设资产、天气控制器、Renderer Feature 共用同一份定义，字段顺序即 Inspector 显示顺序。
/// </summary>
[Serializable]
public struct AerialFogParams
{
    [Tooltip("雾开始出现的视距（米）。屏幕底部视距大约 18.1，从这里起雾。")]
    public float fogStart;

    [Tooltip("雾达到标称密度的视距（米）。必须大于 fogStart。")]
    public float fogEnd;

    [Tooltip("距离裁切曲线。1 = 线性，越大越靠远处才起雾。")]
    public float fogCurve;

    [Tooltip("基准高度处的雾密度（1/米）。")]
    public float density;

    [Tooltip("高度衰减（1/米）。越大雾越贴地，相机越高看见的雾越少。")]
    public float heightFalloff;

    [Tooltip("基准高度（世界 Y）。地面一般填 0。")]
    public float baseHeight;

    [Tooltip("浓度上限，0..1。留一点底透可以避免远景死白。")]
    public float maxOpacity;

    [Tooltip("雾起点的颜色。近处一般带地面反射的暖灰。")]
    public Color colorNear;

    [Tooltip("雾终点的颜色。远处向天光的冷蓝靠。")]
    public Color colorFar;

    [Tooltip("天空被雾化的比例，0..1。俯视角相机看不到天空时可忽略。")]
    public float skyFogAmount;

    [Tooltip("方向光对雾色的影响程度，0..1。0 = 只用雾色，1 = 雾色完全乘上主光颜色。")]
    public float sunInfluence;

    /// <summary>
    /// 按当前工程标定的一组默认值：18 米俯视相机，场景深度 15.6~26.4 米。
    /// 对应"薄雾"预设。换场景或换相机要重标。
    /// </summary>
    public static AerialFogParams Default => new AerialFogParams
    {
        fogStart = 15.5f,
        fogEnd = 26.0f,
        fogCurve = 1.5f,
        density = 0.0475f,
        heightFalloff = 0.06f,
        baseHeight = 0.0f,
        maxOpacity = 0.85f,
        colorNear = new Color(0.58f, 0.56f, 0.55f, 1f),
        colorFar = new Color(0.62f, 0.70f, 0.82f, 1f),
        skyFogAmount = 1.0f,
        sunInfluence = 0.5f,
    };

    /// <summary>
    /// 两个参数组之间插值。t 不做钳制，由调用方保证落在 0..1。
    /// </summary>
    public static AerialFogParams Lerp(in AerialFogParams a, in AerialFogParams b, float t)
    {
        return new AerialFogParams
        {
            fogStart = Mathf.Lerp(a.fogStart, b.fogStart, t),
            fogEnd = Mathf.Lerp(a.fogEnd, b.fogEnd, t),
            fogCurve = Mathf.Lerp(a.fogCurve, b.fogCurve, t),
            density = Mathf.Lerp(a.density, b.density, t),
            heightFalloff = Mathf.Lerp(a.heightFalloff, b.heightFalloff, t),
            baseHeight = Mathf.Lerp(a.baseHeight, b.baseHeight, t),
            maxOpacity = Mathf.Lerp(a.maxOpacity, b.maxOpacity, t),
            colorNear = Color.Lerp(a.colorNear, b.colorNear, t),
            colorFar = Color.Lerp(a.colorFar, b.colorFar, t),
            skyFogAmount = Mathf.Lerp(a.skyFogAmount, b.skyFogAmount, t),
            sunInfluence = Mathf.Lerp(a.sunInfluence, b.sunInfluence, t),
        };
    }
}
