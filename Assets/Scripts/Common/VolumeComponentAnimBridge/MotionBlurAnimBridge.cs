using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// MotionBlur 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class MotionBlurAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private MotionBlur _comp;

    [Header("Motion Blur")]
    public float intensity;
    public float clamp = 0.05f;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.intensity.overrideState) _comp.intensity.Override(intensity);
        if (_comp.clamp.overrideState) _comp.clamp.Override(clamp);
    }
}
