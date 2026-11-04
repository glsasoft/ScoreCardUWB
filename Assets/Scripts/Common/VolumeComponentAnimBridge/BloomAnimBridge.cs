using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Bloom 后处理效果动画桥接
/// 挂载到带有 Volume 组件的 GameObject 上，通过 Animator 驱动 Bloom 参数
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class BloomAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private Bloom _comp;

    [Header("Bloom")]
    public float threshold = 0.9f;
    public float intensity;
    public float scatter = 0.7f;
    public float clamp = 65472f;
    public float dirtIntensity;

    [Header("Tint (RGBA)")]
    public float tintR = 1f;
    public float tintG = 1f;
    public float tintB = 1f;
    public float tintA = 1f;

    public bool highQualityFiltering;
    public int maxIterations = 6;

    [Header("Dirt Texture")]
    public Texture dirtTexture;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.threshold.overrideState) _comp.threshold.Override(threshold);
        if (_comp.intensity.overrideState) _comp.intensity.Override(intensity);
        if (_comp.scatter.overrideState) _comp.scatter.Override(scatter);
        if (_comp.clamp.overrideState) _comp.clamp.Override(clamp);
        if (_comp.dirtIntensity.overrideState) _comp.dirtIntensity.Override(dirtIntensity);
        if (_comp.tint.overrideState) _comp.tint.Override(new Color(tintR, tintG, tintB, tintA));
        if (_comp.highQualityFiltering.overrideState) _comp.highQualityFiltering.Override(highQualityFiltering);
        if (_comp.maxIterations.overrideState) _comp.maxIterations.Override(maxIterations);
        if (_comp.dirtTexture.overrideState && dirtTexture != null) _comp.dirtTexture.Override(dirtTexture);
    }
}
