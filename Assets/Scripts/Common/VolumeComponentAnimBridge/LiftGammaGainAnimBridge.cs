using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// LiftGammaGain 后处理效果动画桥接
/// Lift/Gamma/Gain 为 Vector4 类型，拆分为 XYZW 四个 float
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Volume))]
public class LiftGammaGainAnimBridge : MonoBehaviour
{
    private Volume _volume;
    private LiftGammaGain _comp;

    [Header("Lift (XYZW)")]
    public float liftX = 1f;
    public float liftY = 1f;
    public float liftZ = 1f;
    public float liftW;

    [Header("Gamma (XYZW)")]
    public float gammaX = 1f;
    public float gammaY = 1f;
    public float gammaZ = 1f;
    public float gammaW;

    [Header("Gain (XYZW)")]
    public float gainX = 1f;
    public float gainY = 1f;
    public float gainZ = 1f;
    public float gainW;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _comp);
    }

    private void LateUpdate()
    {
        if (_comp == null) return;
        if (_comp.lift.overrideState) _comp.lift.Override(new Vector4(liftX, liftY, liftZ, liftW));
        if (_comp.gamma.overrideState) _comp.gamma.Override(new Vector4(gammaX, gammaY, gammaZ, gammaW));
        if (_comp.gain.overrideState) _comp.gain.Override(new Vector4(gainX, gainY, gainZ, gainW));
    }
}
