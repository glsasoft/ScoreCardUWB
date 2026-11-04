using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LetsPiu.Graphics.Editor.Tools
{
    public class AnimationOverrideControllerClipReplacer : EditorWindow
    {
        public AnimatorOverrideController controller;
        public string original;
        public string replaced;
        
        [MenuItem("Tools/动画控制器切片替换器")]
        public static void OpenWindow()
        {
            GetWindow<AnimationOverrideControllerClipReplacer>("动画控制器切片替换器").Show();
        }

        private void OnGUI()
        {
            SerializedObject serializedObject = new(this);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(controller)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(original)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(replaced)));
            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button("Replace")) Replace();
        }

        private void Replace()
        {
            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new();
            controller.GetOverrides(overrides);

            for (int i = 0; i < overrides.Count; i++)
            {
                KeyValuePair<AnimationClip, AnimationClip> originalPair = overrides[i];
                if (!originalPair.Value) continue;
                
                string originalName = originalPair.Value.name;
                if (!originalName.Contains(original)) continue;
                
                string replacedName = originalName.Replace(original, replaced);
                string[] replacedClipGUID = AssetDatabase.FindAssets($"{replacedName} t:AnimationClip");
                if (replacedClipGUID.Length == 0)
                {
                    Debug.LogWarning(
                        $"[Animation Override Controller Clip Replacer] Animation Clip Not Found: {replacedName}");
                    continue;
                }
                
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(originalPair.Key,
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(replacedClipGUID[0])));
            }
            
            controller.ApplyOverrides(overrides);
            AssetDatabase.SaveAssets();
        }
    }
}