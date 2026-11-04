using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// DepthOfField 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class DepthOfFieldAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private DepthOfField _comp;

    [Header("Gaussian")]
    public float gaussianStart = 10f;
    public float gaussianEnd = 30f;
    public float gaussianMaxRadius = 1f;

    [Header("Bokeh")]
    public float focusDistance = 10f;
    public float aperture = 5.6f;
    public float focalLength = 50f;
    public int bladeCount = 5;
    public float bladeCurvature = 1f;
    public float bladeRotation;

    [Header("Common")]
    public bool highQualitySampling;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.gaussianStart.overrideState) _comp.gaussianStart.Override(gaussianStart);
        if (_comp.gaussianEnd.overrideState) _comp.gaussianEnd.Override(gaussianEnd);
        if (_comp.gaussianMaxRadius.overrideState) _comp.gaussianMaxRadius.Override(gaussianMaxRadius);
        if (_comp.focusDistance.overrideState) _comp.focusDistance.Override(focusDistance);
        if (_comp.aperture.overrideState) _comp.aperture.Override(aperture);
        if (_comp.focalLength.overrideState) _comp.focalLength.Override(focalLength);
        if (_comp.bladeCount.overrideState) _comp.bladeCount.Override(bladeCount);
        if (_comp.bladeCurvature.overrideState) _comp.bladeCurvature.Override(bladeCurvature);
        if (_comp.bladeRotation.overrideState) _comp.bladeRotation.Override(bladeRotation);
        if (_comp.highQualitySampling.overrideState) _comp.highQualitySampling.Override(highQualitySampling);
    }
}
