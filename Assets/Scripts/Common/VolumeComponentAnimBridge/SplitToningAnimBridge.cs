using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// SplitToning 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class SplitToningAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private SplitToning _comp;

    [Header("Split Toning")]
    public float balance;

    [Header("Shadows (RGBA)")]
    public float shadowsR = 0.5f;
    public float shadowsG = 0.5f;
    public float shadowsB = 0.5f;
    public float shadowsA = 1f;

    [Header("Highlights (RGBA)")]
    public float highlightsR = 0.5f;
    public float highlightsG = 0.5f;
    public float highlightsB = 0.5f;
    public float highlightsA = 1f;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.shadows.overrideState) _comp.shadows.Override(new Color(shadowsR, shadowsG, shadowsB, shadowsA));
        if (_comp.highlights.overrideState) _comp.highlights.Override(new Color(highlightsR, highlightsG, highlightsB, highlightsA));
        if (_comp.balance.overrideState) _comp.balance.Override(balance);
    }
}
