using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

namespace LetsPiu.Graphics
{
    public class RenderFeatureControl : MonoBehaviour
    {
        private static MatchinMaterial _window;

        [MenuItem("Tools/后效开关/VoidEffect")]
        public static void OpenVoidEffect()
        {
            UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            UniversalRendererData URPData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);
            if (URPData != null)
                for (int i = 0; i < URPData.rendererFeatures.Count; i++)
                {
                    if (URPData.rendererFeatures[i] is WraithVoidEffectFeature)
                    {
                        var feature = URPData.rendererFeatures[i];
                        bool isOpen = feature.isActive;
                        feature.SetActive(!isOpen);
                    }
                }
        }

        [MenuItem("Tools/后效开关/BlindFog")]
        public static void OpenBlindFog()
        {
            UniversalRenderPipelineAsset URPAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            FieldInfo propertyInfo = URPAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            UniversalRendererData URPData = (UniversalRendererData)(((ScriptableRendererData[])propertyInfo?.GetValue(URPAsset))?[0]);
            if (URPData != null)
                for (int i = 0; i < URPData.rendererFeatures.Count; i++)
                {
                    if (URPData.rendererFeatures[i] is BlindFogRenderFeature)
                    {
                        var feature = URPData.rendererFeatures[i];
                        bool isOpen = feature.isActive;
                        feature.SetActive(!isOpen);
                    }
                }
        }
    }
}
