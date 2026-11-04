#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "ImportSettingsPreset", menuName = "Import Settings/Import Preset")]
public class ImportSettingsPreset : ScriptableObject
{
    public string[] targetFolders;

    // ====== Texture ======
    public bool setTextureType;
    public TextureImporterType textureType = TextureImporterType.Sprite;

    public bool setSRGB;
    public bool sRGB = true;

    public bool setMipMap;
    public bool mipmapEnabled = false;

    public bool setWrapMode;
    public TextureWrapMode wrapMode = TextureWrapMode.Repeat;

    public bool setFilterMode;
    public FilterMode filterMode = FilterMode.Bilinear;

    [System.Serializable]
    public struct TexturePlatform
    {
        public bool enabled;
        public bool overridden;
        public int maxTextureSize;
        public TextureResizeAlgorithm resizeAlgorithm;
        public TextureImporterFormat textureFormat;
        public int compressionQuality;
        public bool allowsAlphaSplitting;
    }

    public TexturePlatform platformDefault = new TexturePlatform
    {
        enabled = false,
        overridden = true,
        maxTextureSize = 2048,
        resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
        textureFormat = TextureImporterFormat.Automatic,
        compressionQuality = 50,
        allowsAlphaSplitting = false
    };

    public TexturePlatform platformAndroid = new TexturePlatform
    {
        enabled = false,
        overridden = true,
        maxTextureSize = 2048,
        resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
        textureFormat = TextureImporterFormat.RGBA32,
        compressionQuality = 50,
        allowsAlphaSplitting = false
    };

    public TexturePlatform platformiOS = new TexturePlatform
    {
        enabled = false,
        overridden = true,
        maxTextureSize = 2048,
        resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
        textureFormat = TextureImporterFormat.RGBA32,
        compressionQuality = 50,
        allowsAlphaSplitting = false
    };

    // ====== Model ======
    public bool setModelNormals;
    public ModelImporterNormals modelNormals = ModelImporterNormals.Import;

    public bool setModelTangents;
    public ModelImporterTangents modelTangents = ModelImporterTangents.CalculateMikk;

    public bool setMaterialImportMode;
    public ModelImporterMaterialImportMode materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

    public bool setReadable;
    public bool modelReadable;

    // ====== Audio ======
    public bool setAudioForceToMono;
    public bool audioForceToMono;

    public bool setAudioLoadInBackground;
    public bool audioLoadInBackground;

    public bool setAudioAmbisonic;
    public bool audioAmbisonic;

    public bool setAudioLoadType;
    public AudioClipLoadType audioLoadType = AudioClipLoadType.DecompressOnLoad;

    public bool setAudioCompressionFormat;
    public AudioCompressionFormat audioCompressionFormat = AudioCompressionFormat.Vorbis;

    public bool setAudioQuality;
    [Range(0f, 1f)] public float audioQuality = 1.0f;

    public bool setSampleRateSetting;
    public AudioSampleRateSetting sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;

    [System.Serializable]
    public struct AudioPlatform
    {
        public bool enabled;
        public AudioCompressionFormat format;
        [Range(0f, 1f)] public float quality;
    }

    public AudioPlatform audioAndroid = new AudioPlatform
    {
        enabled = false,
        format = AudioCompressionFormat.Vorbis,
        quality = 0.5f
    };

    public AudioPlatform audioiOS = new AudioPlatform
    {
        enabled = false,
        format = AudioCompressionFormat.Vorbis,
        quality = 0.5f
    };
}
#endif
