using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LetsPiu
{
    public class CharacterMaterialAnimationGenerator : EditorWindow
    {
        public Transform characterRoot;
        public AnimationClip animationClip;
        public string propertyPattern;
        
        [MenuItem("Tools/Character Material Animation Generator")]
        public static void OpenWindow()
        {
            CreateWindow<CharacterMaterialAnimationGenerator>();
        }

        private void OnGUI()
        {
            SerializedObject serializedObject = new(this);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(characterRoot)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(animationClip)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(propertyPattern)));
            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button("Generate")) Generate();
        }

        private void Generate()
        {
            if (!animationClip)
            {
                string path = EditorUtility.SaveFilePanel("Save Animation Clip", "", "New Animation", "anim");
                if (string.IsNullOrEmpty(path)) return;
                int index = path.IndexOf("Assets", StringComparison.Ordinal);
                if (index == -1) return;
                animationClip = new AnimationClip();
                AssetDatabase.CreateAsset(animationClip, path.Substring(index));
            }
            
            animationClip.ClearCurves();
            CollectProperties(characterRoot);
            AssetDatabase.SaveAssets();
        }

        private void CollectProperties(Transform trans, string path = "")
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform child = trans.GetChild(i);
                CollectProperties(child, (string.IsNullOrEmpty(path) ? path : path + "/") + child.name);
            }

            SkinnedMeshRenderer renderer = trans.GetComponent<SkinnedMeshRenderer>();
            if (!renderer) return;

            Material material = renderer.sharedMaterial;
            
            string[] floats = material.GetPropertyNames(MaterialPropertyType.Float);
            foreach (string property in floats)
            {
                if (!Regex.IsMatch(property, propertyPattern)) continue;
                animationClip.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + property,
                    new AnimationCurve(new Keyframe(0, material.GetFloat(property))));
            }
            
            string[] vectors = material.GetPropertyNames(MaterialPropertyType.Vector);
            foreach (string property in vectors)
            {
                if (!Regex.IsMatch(property, propertyPattern)) continue;
                Vector4 vector = material.GetVector(property);
                animationClip.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + property + ".x",
                    new AnimationCurve(new Keyframe(0, vector.x)));
                animationClip.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + property + ".y",
                    new AnimationCurve(new Keyframe(0, vector.y)));
                animationClip.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + property + ".z",
                    new AnimationCurve(new Keyframe(0, vector.z)));
                animationClip.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + property + ".w",
                    new AnimationCurve(new Keyframe(0, vector.w)));
            }
        }
    }
}
