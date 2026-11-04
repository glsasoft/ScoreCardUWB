using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ColorCurves 后处理效果动画桥接
/// 注意：TextureCurveParameter 无法通过 Animator 驱动，此脚本仅提供基础框架
/// 如需动画控制曲线，建议使用脚本动态修改曲线数据
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class ColorCurvesAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private ColorCurves _comp;

    // TextureCurveParameter 无法通过 Animator 数值驱动
    // 如需运行时修改曲线，可通过脚本操作 _comp.master.value 等曲线数据

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    /// <summary>
    /// 获取 ColorCurves 组件引用，用于脚本中动态修改曲线
    /// </summary>
    public ColorCurves GetComponent() => _comp;
}
