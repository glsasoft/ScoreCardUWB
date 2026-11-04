using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public class PreviewTonemapping : EditorWindow
{
    //private static MatchinMaterial _window;

    //[MenuItem("Tools/开启预览窗口调色", true)]
    //public static bool OpenValidation()
    //{
    //    UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
    //    FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
    //    UniversalRendererData URPData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);
    //    bool isOpen = (URPData?.rendererFeatures[URPData.rendererFeatures.Count - 1] as ToneMappingRenderFeature)?.Setting.Material
    //        .name == "ToneMapping";
    //    return !isOpen;
    //}

    //[MenuItem("Tools/开启预览窗口调色")]
    //public static void Open()
    //{
    //    UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
    //    FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
    //    UniversalRendererData URPData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);
    //    (URPData.rendererFeatures[URPData.rendererFeatures.Count - 1] as ToneMappingRenderFeature).Setting.Material =
    //        AssetDatabase.LoadAssetAtPath<Material>("Assets/Graphics/RenderFeature/ToneMapping/ToneMapping.mat");
    //}

    //[MenuItem("Tools/关闭预览窗口调色", true)]
    //public static bool CloseValidation()
    //{
    //    UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
    //    FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
    //    UniversalRendererData URPData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);
    //    bool isOpen = (URPData?.rendererFeatures[URPData.rendererFeatures.Count - 1] as ToneMappingRenderFeature)?.Setting.Material
    //        .name == "ToneMapping";
    //    return isOpen;
    //}

    //[MenuItem("Tools/关闭预览窗口调色")]
    //public static void CloseX()
    //{
    //    UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
    //    FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
    //    UniversalRendererData URPData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);
    //    (URPData.rendererFeatures[URPData.rendererFeatures.Count - 1] as ToneMappingRenderFeature).Setting.Material =
    //        AssetDatabase.LoadAssetAtPath<Material>("Assets/Graphics/RenderFeature/ToneMapping/BlitSource.mat");
    //}
}