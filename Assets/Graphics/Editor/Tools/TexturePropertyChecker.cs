using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LetsPiu
{
    public class TexturePropertyChecker : EditorWindowWithAssetSelector
    {
        private Vector2 scrollPosition;

        public bool keepLargeAlbedoTexture = true;
        public List<Texture2D> checkedAssets = new();
        
        [MenuItem("Tools/贴图属性检查器")]
        private static void OpenWindow()
        {
            GetWindow<TexturePropertyChecker>("贴图属性检查器").Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height));
            
            DrawAssetsGUI();
            if (GUILayout.Button("打开资产选择器")) AssetSelector.OpenWindow();
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(keepLargeAlbedoTexture)),
                new GUIContent("保留大尺寸 Albedo 贴图"));
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("检查贴图属性"))
            {
                Undo.RecordObject(this, "检查贴图属性");
                
                checkedAssets.Clear();
                foreach (Object asset in assets)
                {
                    if (asset is not Texture2D texture) continue;
                    if (ValidateTextureProperty(texture)) continue;
                    checkedAssets.Add(texture);
                }
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(checkedAssets)),
                new GUIContent("属性不正确的贴图资产"));
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("修正贴图属性"))
            {
                if (EditorUtility.DisplayDialog("修正贴图属性", "你确定要根据修正列表中所有贴图的属性吗？该操作无法被撤销", "确认", "取消"))
                {
                    for (int i = 0; i < checkedAssets.Count; i++)
                    {
                        Texture2D texture = checkedAssets[i];
                        if (!texture) continue;
                        EditorUtility.DisplayProgressBar("修正贴图属性中...",
                            $"{AssetDatabase.GetAssetPath(texture)} ({i}/{checkedAssets.Count})",
                            (float)i / checkedAssets.Count);
                        FixTextureProperty(texture);
                    }
                    EditorUtility.ClearProgressBar();
                }
            }

            if (GUILayout.Button("清空列表"))
            {
                Undo.RecordObject(this, "清空列表");
                checkedAssets.Clear();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private bool ValidateTextureProperty(Texture2D texture)
        {
            if (texture is null) return false;
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            TextureImporterPlatformSettings settings =
                importer?.GetPlatformTextureSettings(NamedBuildTarget.Android.TargetName);
            if (settings is null) return false;

            bool isAlbedo = texture.name.Contains("Albedo");
            bool isNormal = texture.name.Contains("Normal");
            bool isEmission = texture.name.Contains("Emission");

            if (!((!isNormal && importer.textureType == TextureImporterType.Default) ||
                  (isNormal && importer.textureType == TextureImporterType.NormalMap))) return false;
            if ((isAlbedo || isEmission) ^ importer.sRGBTexture) return false;
            if (!settings.overridden)  return false;
            if (!(settings.maxTextureSize == 1024 ||
                  (keepLargeAlbedoTexture && isAlbedo && settings.maxTextureSize == 2048))) return false;
            if (settings.format != TextureImporterFormat.ASTC_6x6) return false;
            if (!((isAlbedo && settings.compressionQuality == (int)TextureCompressionQuality.Normal) ||
                  (!isAlbedo && settings.compressionQuality == (int)TextureCompressionQuality.Fast))) return false;
            return true;
        }

        private void FixTextureProperty(Texture2D texture)
        {
            if (texture is null) return;
            
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            if (importer is null) return;
            
            TextureImporterPlatformSettings settings =
                importer.GetPlatformTextureSettings(NamedBuildTarget.Android.TargetName);
            if (settings is null)
            {
                settings = new TextureImporterPlatformSettings();
                importer.GetDefaultPlatformTextureSettings().CopyTo(settings);
                settings.name = NamedBuildTarget.Android.TargetName;
            }

            bool isAlbedo = texture.name.Contains("Albedo");
            bool isNormal = texture.name.Contains("Normal");
            bool isEmission = texture.name.Contains("Emission");

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = isAlbedo || isEmission;
            settings.overridden = true;
            if (!(settings.maxTextureSize == 1024 ||
                  (keepLargeAlbedoTexture && isAlbedo && settings.maxTextureSize == 2048)))
                settings.maxTextureSize = 1024;
            settings.format = TextureImporterFormat.ASTC_6x6;
            settings.compressionQuality =
                (int)(isAlbedo ? TextureCompressionQuality.Normal : TextureCompressionQuality.Fast);
            
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
        }

        protected override bool ValidateAsset(Object asset, out string error)
        {
            if (!base.ValidateAsset(asset, out error)) return false;
            
            if (asset is not Texture2D)
            {
                error = "资产类型需要为 Texture 2D";
                return false;
            }
            
            if (!Regex.IsMatch(asset.name,
                    @"^T_.+_(Albedo|Normal|Metallic|Roughness|Ambient Occlusion|AO|Emission|MRO|Thickness|SDF)_\d+$"))
            {
                error = "贴图名称不符合规范";
                return false;
            }
            
            error = null;
            return true;
        }
    }
}