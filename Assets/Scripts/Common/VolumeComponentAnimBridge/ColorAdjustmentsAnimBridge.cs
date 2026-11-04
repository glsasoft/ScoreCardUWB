using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ColorAdjustments 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class ColorAdjustmentsAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private ColorAdjustments _comp;

    [Header("Color Adjustments")]
    public float postExposure;
    public float contrast;
    public float hueShift;
    public float saturation;

    [Header("Color Filter (RGBA)")]
    public float colorFilterR = 1f;
    public float colorFilterG = 1f;
    public float colorFilterB = 1f;
    public float colorFilterA = 1f;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.postExposure.overrideState) _comp.postExposure.Override(postExposure);
        if (_comp.contrast.overrideState) _comp.contrast.Override(contrast);
        if (_comp.hueShift.overrideState) _comp.hueShift.Override(hueShift);
        if (_comp.saturation.overrideState) _comp.saturation.Override(saturation);
        if (_comp.colorFilter.overrideState) _comp.colorFilter.Override(new Color(colorFilterR, colorFilterG, colorFilterB, colorFilterA));
    }
}
