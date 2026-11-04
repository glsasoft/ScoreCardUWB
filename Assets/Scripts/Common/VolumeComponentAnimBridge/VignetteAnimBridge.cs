using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Vignette 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class VignetteAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private Vignette _comp;

    [Header("Vignette")]
    public float intensity;
    public float smoothness = 0.2f;
    public bool rounded;

    [Header("Color (RGBA)")]
    public float colorR;
    public float colorG;
    public float colorB;
    public float colorA = 1f;

    [Header("Center (XY)")]
    public float centerX = 0.5f;
    public float centerY = 0.5f;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.intensity.overrideState) _comp.intensity.Override(intensity);
        if (_comp.smoothness.overrideState) _comp.smoothness.Override(smoothness);
        if (_comp.rounded.overrideState) _comp.rounded.Override(rounded);
        if (_comp.color.overrideState) _comp.color.Override(new Color(colorR, colorG, colorB, colorA));
        if (_comp.center.overrideState) _comp.center.Override(new Vector2(centerX, centerY));
    }
}
