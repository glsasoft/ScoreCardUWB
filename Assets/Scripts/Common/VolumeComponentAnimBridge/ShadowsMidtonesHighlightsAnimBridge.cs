using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ShadowsMidtonesHighlights 后处理效果动画桥接
/// Shadows/Midtones/Highlights 为 Vector4 类型，拆分为 XYZW 四个 float
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class ShadowsMidtonesHighlightsAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private ShadowsMidtonesHighlights _comp;

    [Header("Shadows (XYZW)")]
    public float shadowsX = 1f;
    public float shadowsY = 1f;
    public float shadowsZ = 1f;
    public float shadowsW;

    [Header("Midtones (XYZW)")]
    public float midtonesX = 1f;
    public float midtonesY = 1f;
    public float midtonesZ = 1f;
    public float midtonesW;

    [Header("Highlights (XYZW)")]
    public float highlightsX = 1f;
    public float highlightsY = 1f;
    public float highlightsZ = 1f;
    public float highlightsW;

    [Header("Boundaries")]
    public float shadowsStart;
    public float shadowsEnd = 0.3f;
    public float highlightsStart = 0.55f;
    public float highlightsEnd = 1f;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.shadows.overrideState) _comp.shadows.Override(new Vector4(shadowsX, shadowsY, shadowsZ, shadowsW));
        if (_comp.midtones.overrideState) _comp.midtones.Override(new Vector4(midtonesX, midtonesY, midtonesZ, midtonesW));
        if (_comp.highlights.overrideState) _comp.highlights.Override(new Vector4(highlightsX, highlightsY, highlightsZ, highlightsW));
        if (_comp.shadowsStart.overrideState) _comp.shadowsStart.Override(shadowsStart);
        if (_comp.shadowsEnd.overrideState) _comp.shadowsEnd.Override(shadowsEnd);
        if (_comp.highlightsStart.overrideState) _comp.highlightsStart.Override(highlightsStart);
        if (_comp.highlightsEnd.overrideState) _comp.highlightsEnd.Override(highlightsEnd);
    }
}
