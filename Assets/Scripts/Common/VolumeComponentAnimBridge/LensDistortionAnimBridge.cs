using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// LensDistortion 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class LensDistortionAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private LensDistortion _comp;

    [Header("Lens Distortion")]
    public float intensity;
    public float xMultiplier = 1f;
    public float yMultiplier = 1f;
    public float scale = 1f;

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
        if (_comp.xMultiplier.overrideState) _comp.xMultiplier.Override(xMultiplier);
        if (_comp.yMultiplier.overrideState) _comp.yMultiplier.Override(yMultiplier);
        if (_comp.scale.overrideState) _comp.scale.Override(scale);
        if (_comp.center.overrideState) _comp.center.Override(new Vector2(centerX, centerY));
    }
}
