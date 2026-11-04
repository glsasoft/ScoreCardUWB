using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LetsPiu
{
    public class RendererFeatureManager
    {
        public static RendererFeatureManager Instance
        {
            get
            {
                _instance ??= new RendererFeatureManager();
                return _instance;
            }
        }
        
        private static RendererFeatureManager _instance;
        
        public bool initialized { get; private set; }
        public string errorMessage { get; private set; }

        public IReadOnlyDictionary<string, ScriptableRendererFeature> rendererFeatures => _rendererFeatures;
        private Dictionary<string, ScriptableRendererFeature> _rendererFeatures;

        private Stack<Dictionary<string, bool>> activeStatesStack = new();

        private RendererFeatureManager()
        {
            Initialize();
        }

        public bool Initialize()
        {
            initialized = false;

            UniversalRenderPipelineAsset pipelineAsset =
                GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipelineAsset is null)
            {
                errorMessage = "URP Asset 为空";
                return false;
            }

            FieldInfo rendererDataListFieldInfo = typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            ScriptableRendererData[] rendererDataList =
                (ScriptableRendererData[]) rendererDataListFieldInfo!.GetValue(pipelineAsset);
            if (rendererDataList.Length == 0)
            {
                errorMessage = "Renderer Data 列表为空";
                return false;
            }

            ScriptableRendererData rendererData = rendererDataList[0];
            if (rendererData is null)
            {
                errorMessage = "Renderer Data 为空";
                return false;
            }

            _rendererFeatures = new Dictionary<string, ScriptableRendererFeature>(rendererData.rendererFeatures.Count);
            foreach (ScriptableRendererFeature rendererFeature in rendererData.rendererFeatures)
            {
                if (!_rendererFeatures.TryAdd(rendererFeature.name, rendererFeature))
                    Debug.LogWarning($"Renderer Feature 名称重复: {rendererFeature.name}");
            }

            initialized = true;
            return true;
        }

        public void Push()
        {
            Dictionary<string, bool> activeStates = new(rendererFeatures.Count);
            foreach (KeyValuePair<string, ScriptableRendererFeature> kvp in rendererFeatures)
                activeStates.Add(kvp.Key, kvp.Value.isActive);
            activeStatesStack.Push(activeStates);
        }

        public void Pop()
        {
            Dictionary<string, bool> activeStates = activeStatesStack.Pop();
            foreach (KeyValuePair<string, bool> kvp in activeStates)
                rendererFeatures[kvp.Key].SetActive(kvp.Value);
        }
    }
}