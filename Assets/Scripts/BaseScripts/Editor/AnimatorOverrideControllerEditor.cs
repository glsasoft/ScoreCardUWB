using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LetsPiu
{
    [CustomEditor(typeof(AnimatorOverrideController))]
    public class AnimatorOverrideControllerEditor : Editor
    {
        private class EditorBuffer
        {
            public string pattern;
            public string replacement;
        }

        private static readonly Dictionary<AnimatorOverrideController, EditorBuffer> buffers = new();
        
        private AnimatorOverrideController controller;
        private EditorBuffer buffer;
        private Editor defaultEditor;

        private void OnEnable()
        {
            controller = (AnimatorOverrideController)target;
                
            if (!buffers.TryGetValue(controller, out buffer))
            {
                buffer = new EditorBuffer();
                buffers.Add(controller, buffer);
            }
            
            defaultEditor = CreateEditor(targets,
                Type.GetType("UnityEditor.AnimatorOverrideControllerInspector, UnityEditor"));
        }

        private void OnDisable()
        {
            MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (disableMethod != null)
                disableMethod.Invoke(defaultEditor,null);
            DestroyImmediate(defaultEditor);
        }

        public override void OnInspectorGUI()
        {
            defaultEditor.OnInspectorGUI();
            
            buffer.pattern = EditorGUILayout.TextField("Pattern", buffer.pattern);
            buffer.replacement = EditorGUILayout.TextField("Replacement", buffer.replacement);

            if (GUILayout.Button("Replace Clips"))
            {
                Undo.RecordObject(controller, "Replace Clips");
                
                List<KeyValuePair<AnimationClip, AnimationClip>> pairs = new(controller.overridesCount);
                controller.GetOverrides(pairs);
                
                for (int i = 0; i < pairs.Count; i++)
                {
                    KeyValuePair<AnimationClip, AnimationClip> origin = pairs[i];
                    string clipName = origin.Key.name;
                    
                    if (Regex.IsMatch(clipName, buffer.pattern))
                    {
                        clipName = Regex.Replace(clipName, buffer.pattern, buffer.replacement);
                        
                        string[] newClipGUIDs = AssetDatabase.FindAssets($"{clipName} t:AnimationClip");
                        if (newClipGUIDs.Length == 0)
                        {
                            Debug.LogWarning($"Clip {clipName} does not exist.");
                            continue;
                        }
                        if (newClipGUIDs.Length > 1)
                            Debug.LogWarning($"Clip {clipName} exists more than once.");
                        
                        string newClipPath = AssetDatabase.GUIDToAssetPath(newClipGUIDs[0]);
                        AnimationClip newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath);
                        pairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(origin.Key, newClip);
                    }
                }
                
                controller.ApplyOverrides(pairs);
                AssetDatabase.SaveAssets();
            }
        }
    }
}