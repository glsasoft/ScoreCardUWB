using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// WhiteBalance 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class WhiteBalanceAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private WhiteBalance _comp;

    [Header("White Balance")]
    public float temperature;
    public float tint;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.temperature.overrideState) _comp.temperature.Override(temperature);
        if (_comp.tint.overrideState) _comp.tint.Override(tint);
    }
}
