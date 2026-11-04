using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LetsPiu
{
    public class RendererFeatureManagerEditor : EditorWindow, ISerializationCallbackReceiver
    {
        private List<string> featureNames = new();
        [SerializeField] private List<bool> featureActives = new();

        private RendererFeatureManager manager;
        private SerializedObject serializedObject;
        private SerializedProperty featureActivesProperty;

        [MenuItem("Tools/Renderer Feature 管理器")]
        public static void OpenWindow()
        {
            GetWindow<RendererFeatureManagerEditor>("Renderer Feature 管理器").Show();
        }

        private void OnEnable()
        {
            manager = RendererFeatureManager.Instance;
            serializedObject = new SerializedObject(this);
            featureActivesProperty = serializedObject.FindProperty(nameof(featureActives));
        }

        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (!manager.Initialize())
            {
                EditorGUILayout.HelpBox(manager.errorMessage, MessageType.Warning);
                return;
            }

            serializedObject.Update();

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 25;
            for (int i = 0; i < featureNames.Count; i++)
                EditorGUILayout.PropertyField(featureActivesProperty.GetArrayElementAtIndex(i),
                    new GUIContent(RegulateFeatureName(featureNames[i])));
            EditorGUIUtility.labelWidth = labelWidth;

            if (serializedObject.ApplyModifiedProperties()) AssetDatabase.SaveAssets();
        }

        private string RegulateFeatureName(string featureName)
        {
            featureName = Regex.Replace(featureName, @"^[a-z]", @"\U$0");
            featureName = Regex.Replace(featureName, @"(?<!^)[A-Z](?![A-Z])", @" $0");
            return featureName;
        }

        public void OnBeforeSerialize()
        {
            if (manager is null) return;
            if (!manager.initialized) return;

            featureNames.Clear();
            featureActives.Clear();

            featureNames.Capacity = manager.rendererFeatures.Count;
            featureActives.Capacity = manager.rendererFeatures.Count;

            foreach (KeyValuePair<string, ScriptableRendererFeature> kvp in manager.rendererFeatures)
            {
                featureNames.Add(kvp.Key);
                featureActives.Add(kvp.Value.isActive);
            }
        }

        public void OnAfterDeserialize()
        {
            if (manager is null) return;
            if (!manager.initialized) return;
            
            for (int i = 0; i < featureNames.Count; i++)
                manager.rendererFeatures[featureNames[i]].SetActive(featureActives[i]);
        }
    }
}