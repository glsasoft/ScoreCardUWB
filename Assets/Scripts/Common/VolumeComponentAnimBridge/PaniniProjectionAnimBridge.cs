using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// PaniniProjection 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class PaniniProjectionAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private PaniniProjection _comp;

    [Header("Panini Projection")]
    public float distance;
    public float cropToFit = 1f;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.distance.overrideState) _comp.distance.Override(distance);
        if (_comp.cropToFit.overrideState) _comp.cropToFit.Override(cropToFit);
    }
}
