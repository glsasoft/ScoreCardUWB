using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ChannelMixer 后处理效果动画桥接
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class ChannelMixerAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private ChannelMixer _comp;

    [Header("Red Output")]
    public float redOutRedIn = 100f;
    public float redOutGreenIn;
    public float redOutBlueIn;

    [Header("Green Output")]
    public float greenOutRedIn;
    public float greenOutGreenIn = 100f;
    public float greenOutBlueIn;

    [Header("Blue Output")]
    public float blueOutRedIn;
    public float blueOutGreenIn;
    public float blueOutBlueIn = 100f;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.redOutRedIn.overrideState) _comp.redOutRedIn.Override(redOutRedIn);
        if (_comp.redOutGreenIn.overrideState) _comp.redOutGreenIn.Override(redOutGreenIn);
        if (_comp.redOutBlueIn.overrideState) _comp.redOutBlueIn.Override(redOutBlueIn);
        if (_comp.greenOutRedIn.overrideState) _comp.greenOutRedIn.Override(greenOutRedIn);
        if (_comp.greenOutGreenIn.overrideState) _comp.greenOutGreenIn.Override(greenOutGreenIn);
        if (_comp.greenOutBlueIn.overrideState) _comp.greenOutBlueIn.Override(greenOutBlueIn);
        if (_comp.blueOutRedIn.overrideState) _comp.blueOutRedIn.Override(blueOutRedIn);
        if (_comp.blueOutGreenIn.overrideState) _comp.blueOutGreenIn.Override(blueOutGreenIn);
        if (_comp.blueOutBlueIn.overrideState) _comp.blueOutBlueIn.Override(blueOutBlueIn);
    }
}
