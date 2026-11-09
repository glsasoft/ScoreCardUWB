using UnityEngine;

/// <summary>
/// 一套天气预设。晴、薄雾、浓雾各是一个资产，由 AerialFogController 在它们之间过渡。
/// </summary>
[CreateAssetMenu(menuName = "Piu/Aerial Fog Preset", fileName = "AerialFog_Preset")]
public class AerialFogPreset : ScriptableObject
{
    [Tooltip("只用来在 Inspector 和日志里辨认，不参与计算。")]
    public string displayName;

    public AerialFogParams parameters = AerialFogParams.Default;
}
