using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ChromaticAberration 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class ChromaticAberrationAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private ChromaticAberration _comp;

    [Header("Chromatic Aberration")]
    public float intensity;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.intensity.overrideState) _comp.intensity.Override(intensity);
    }
}
