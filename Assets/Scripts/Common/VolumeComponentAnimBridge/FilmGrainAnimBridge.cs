using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// FilmGrain 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class FilmGrainAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private FilmGrain _comp;

    [Header("Film Grain")]
    public float intensity;
    public float response = 0.8f;

    [Header("Texture")]
    public Texture texture;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.intensity.overrideState) _comp.intensity.Override(intensity);
        if (_comp.response.overrideState) _comp.response.Override(response);
        if (_comp.texture.overrideState && texture != null) _comp.texture.Override(texture);
    }
}
