using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ColorLookup 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class ColorLookupAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private ColorLookup _comp;

    [Header("Color Lookup")]
    public float contribution;

    [Header("LUT Texture")]
    public Texture texture;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.contribution.overrideState) _comp.contribution.Override(contribution);
        if (_comp.texture.overrideState && texture != null) _comp.texture.Override(texture);
    }
}
