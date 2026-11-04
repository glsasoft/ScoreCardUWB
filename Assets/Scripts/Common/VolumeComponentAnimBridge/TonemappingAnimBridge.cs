using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Tonemapping 后处理效果动画桥接
/// 注意：mode 等枚举参数未暴露，需要时可用 int 间接映射
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class TonemappingAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private Tonemapping _comp;

    [Header("Tonemapping")]
    public float hueShiftAmount;
    public float paperWhite = 300f;
    public float minNits = 0.005f;
    public float maxNits = 1000f;

    [Header("Detect")]
    public bool detectPaperWhite;
    public bool detectBrightnessLimits = true;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.hueShiftAmount.overrideState) _comp.hueShiftAmount.Override(hueShiftAmount);
        if (_comp.paperWhite.overrideState) _comp.paperWhite.Override(paperWhite);
        if (_comp.minNits.overrideState) _comp.minNits.Override(minNits);
        if (_comp.maxNits.overrideState) _comp.maxNits.Override(maxNits);
        if (_comp.detectPaperWhite.overrideState) _comp.detectPaperWhite.Override(detectPaperWhite);
        if (_comp.detectBrightnessLimits.overrideState) _comp.detectBrightnessLimits.Override(detectBrightnessLimits);
    }
}
