using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LetsPiu
{
    public class TextureSetter : EditorWindowWithAssetSelector
    {
        // 检查贴图名称
        [Serializable]
        public class TextureNamePair
        {
            public Texture2D texture;
            public string name;

            [CustomPropertyDrawer(typeof(TextureNamePair))]
            private class AssetNamePairDrawer : PropertyDrawer
            {
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    position = EditorGUI.PrefixLabel(position, label);
                    position.width = (position.width - EditorGUIUtility.standardVerticalSpacing) / 2;
                    EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(texture)), GUIContent.none);
                    position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(name)), GUIContent.none);
                }
            }
        }
        
        private Vector2 scrollPosition;

        // 贴图名称管理
        public List<TextureNamePair> checkedNameAssets = new();
        private SerializedProperty checkedAssetProperty;
        private ReorderableList checkedAssetList;
        private bool showCheckedName = false;
        private bool showCheckedAssetList;
        private Vector2 checkedAssetListScrollPosition;
        
        // 贴图类型管理
        private bool showCheckedProperty = false;
        public List<Texture2D> checkedPropertyAssets = new();
        public bool keepLargeAlbedoTexture = true;
        
        
        [MenuItem("Tools/Graphics/资产选择器_贴图设置器")]
        private static void OpenWindow()
        {
            GetWindow<TextureSetter>("贴图设置器").Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // checkedNameAssets字段的绘制
            checkedAssetProperty = serializedObject.FindProperty(nameof(checkedNameAssets));
            checkedAssetList = new ReorderableList(serializedObject, checkedAssetProperty)
            {
                headerHeight = 0,
                elementHeight = EditorGUIUtility.singleLineHeight,
                multiSelect = true,
                drawHeaderCallback = rect => { },
                drawElementBackgroundCallback = (rect, index, active, focused) =>
                {
                    if (checkedAssetProperty.arraySize == 0) return;
                    if (active) EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
                    if (!ValidateTextureName(checkedAssetProperty.GetArrayElementAtIndex(index)
                            .FindPropertyRelative(nameof(TextureNamePair.name)).stringValue))
                        EditorGUI.DrawRect(rect, new Color(0.53f, 0.36f, 0.17f));
                    if (focused) EditorGUI.DrawRect(rect, new Color(0.17f, 0.36f, 0.53f));
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    SerializedProperty property = checkedAssetProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, property, new GUIContent($"Element {index}"));
                }
            };
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height));
            
            DrawAssetsGUI();
            if (GUILayout.Button("打开资产选择器")) AssetSelector.OpenWindow();
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            TextureNameManager();
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            TexturePropertyManager();

            EditorGUILayout.EndScrollView();
        }

        // 贴图名称管理
        private void TextureNameManager()
        {
            showCheckedName = EditorGUILayout.Toggle("1. 贴图名称管理", showCheckedName);
            if (showCheckedName)
            {
                EditorGUILayout.BeginVertical("box"); // 包一层灰框视觉分组
                GUILayout.Space(4);

                if (GUILayout.Button("检查贴图名称"))
                {
                    Undo.RecordObject(this, "检查贴图名称");

                    checkedNameAssets.Clear();
                    foreach (Object asset in assets)
                    {
                        if (asset is not Texture2D texture) continue;
                        if (ValidateTextureName(texture.name)) continue;
                        checkedNameAssets.Add(new TextureNamePair
                        {
                            texture = texture,
                            name = SuggestedTextureName(texture)
                        });
                    }
                }
                serializedObject.Update();
                
                showCheckedAssetList = EditorGUILayout.BeginFoldoutHeaderGroup(showCheckedAssetList, "需重命名的贴图资产");
                if (showCheckedAssetList)
                {
                    float height = Mathf.Min(checkedAssetList.GetHeight(), 320);
                    checkedAssetListScrollPosition =
                        EditorGUILayout.BeginScrollView(checkedAssetListScrollPosition,
                            GUILayout.Height(height + 10));
                    Rect rect = EditorGUILayout.GetControlRect(false, checkedAssetList.GetHeight());
                    Rect visibleRect = new(rect.x, rect.y + checkedAssetListScrollPosition.y, rect.width, height);
                    checkedAssetList.DoList(rect, visibleRect);
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
                serializedObject.ApplyModifiedProperties();

                if (GUILayout.Button("重命名贴图"))
                {
                    if (EditorUtility.DisplayDialog("重命名贴图", "你确定要根据列表信息重命名贴图吗？该操作无法被撤销", "确认", "取消"))
                    {
                        foreach (TextureNamePair pair in checkedNameAssets)
                            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(pair.texture), pair.name);
                    }
                }

                if (GUILayout.Button("清空重命名列表"))
                {
                    Undo.RecordObject(this, "清空重命名列表");
                    checkedNameAssets.Clear();
                }
                
                EditorGUILayout.EndVertical(); // end box
            }
        }
        private bool ValidateTextureName(string textureName)
                {
                    if (textureName is null) return false;
                    return Regex.IsMatch(textureName,
                        @"^T_.+_(Albedo|Normal|Metallic|Roughness|Ambient Occlusion|AO|Emission|MRO|Thickness|SDF)_\d+$");
                }
        private string SuggestedTextureName(Texture2D texture)
                {
                    string suggestedName = texture.name;
        
                    Match match = Regex.Match(suggestedName,
                        @"^(T_)?(.*?)_?(Albedo|Normal|Metallic|Roughness|Ambient Occlusion|AO|Emission|MRO|Thickness|SDF)_?(\d*)(.*)$");
                    
                    if (match.Success)
                    {
                        string mat1 = match.Groups[2].Value;
                        string mat2 = match.Groups[5].Value;
                        string material = string.IsNullOrEmpty(mat1) ? mat2 :
                            string.IsNullOrEmpty(mat2) ? mat1 : $"{mat1}_{mat2}";
                        
                        string type = match.Groups[3].Value;
                        
                        string size = match.Groups[4].Value;
                        if (string.IsNullOrEmpty(size)) size = texture.width.ToString();
        
                        suggestedName = $"T_{material}_{type}_{size}";
                    }
        
                    return suggestedName;
                }
        
        // 贴图属性管理
        private void TexturePropertyManager()
        {
            showCheckedProperty = EditorGUILayout.Toggle("2. 贴图属性管理", showCheckedProperty);
            if (showCheckedProperty)
            {
                EditorGUILayout.BeginVertical("box"); 
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(keepLargeAlbedoTexture)), new GUIContent("保留大尺寸Albedo贴图"));
                serializedObject.ApplyModifiedProperties();

                if (GUILayout.Button("检查贴图属性"))
                {
                    Undo.RecordObject(this, "检查贴图属性");
                    checkedPropertyAssets.Clear();
                    foreach (Object asset in assets)
                    {
                        if(asset is not Texture2D texture) continue;
                        if(ValidateTextureProperty(texture)) continue;
                        checkedPropertyAssets.Add(texture);
                    }
                }
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(checkedPropertyAssets)), new GUIContent("属性不正确的贴图资产"));
                serializedObject.ApplyModifiedProperties();

                if (GUILayout.Button("修正贴图属性"))
                {
                    if (EditorUtility.DisplayDialog("修正贴图属性", "你确定要根据修正列表中所有贴图的属性吗？该操作无法被撤销", "确认", "取消"))
                    {
                        for (int i = 0; i < checkedPropertyAssets.Count; i++)
                        {
                            Texture2D texture = checkedPropertyAssets[i];
                            if(!texture) continue;
                            EditorUtility.DisplayProgressBar("修正贴图属性中...",
                                $"{AssetDatabase.GetAssetPath(texture)} ({i}/{checkedPropertyAssets.Count})",
                                (float)i / checkedPropertyAssets.Count);
                            FixTextureProperty(texture);
                        }
                        EditorUtility.ClearProgressBar();
                    }
                }
                
                if (GUILayout.Button("清空列表"))
                {
                    Undo.RecordObject(this, "清空列表");
                    checkedPropertyAssets.Clear();
                }
                
                EditorGUILayout.EndVertical(); // end box
            }
        }
        private bool ValidateTextureProperty(Texture2D texture)
        {
            if(texture is null) return false;
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            TextureImporterPlatformSettings settings = importer?.GetPlatformTextureSettings(NamedBuildTarget.Android.TargetName);
            if(settings is null) return false;
            
            bool isAlbedo = texture.name.Contains("Albedo");
            bool isNormal = texture.name.Contains("Normal");
            bool isEmission = texture.name.Contains("Emission");
            
            if(!((!isNormal && importer.textureType == TextureImporterType.Default) || 
                 (isNormal && importer.textureType == TextureImporterType.NormalMap))) return false;
            if((isAlbedo || isEmission) ^ importer.sRGBTexture) return false;
            if(!(settings.maxTextureSize == 1024 || (keepLargeAlbedoTexture && isAlbedo && settings.maxTextureSize == 2048))) return false;
            if(settings.format != TextureImporterFormat.ASTC_6x6) return false;
            if (!((isAlbedo && settings.compressionQuality == (int)TextureCompressionQuality.Normal) ||
                  (!isAlbedo && settings.compressionQuality == (int)TextureCompressionQuality.Fast))) return false;
            
            return true;
        }
        private void FixTextureProperty(Texture2D texture)
        {
            if (texture is null) return;
            
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            if (importer is null) return;
            
            TextureImporterPlatformSettings settings = importer?.GetPlatformTextureSettings(NamedBuildTarget.Android.TargetName);
            if (settings is null)
            {
                settings = new TextureImporterPlatformSettings();
                importer.GetDefaultPlatformTextureSettings().CopyTo(settings);
                settings.name = NamedBuildTarget.Android.TargetName;
            }
            
            bool isAlbedo = texture.name.Contains("Albedo");
            bool isNormal = texture.name.Contains("Normal");
            bool isEmission = texture.name.Contains("Emission");
            
            importer.textureType = isNormal? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = isAlbedo || isEmission;
            settings.overridden = true;
            if(!(settings.maxTextureSize == 1024 || (keepLargeAlbedoTexture && isAlbedo && settings.maxTextureSize == 2048)))
                settings.maxTextureSize = 1024;
            settings.format = TextureImporterFormat.ASTC_6x6;
            settings.compressionQuality = (int)(isAlbedo ? TextureCompressionQuality.Normal : TextureCompressionQuality.Fast);
            
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

            if (showCheckedProperty)
            {
                if (!Regex.IsMatch(asset.name,
                        @"^T_.+_(Albedo|Normal|Metallic|Roughness|Ambient Occlusion|AO|Emission|MRO|Thickness|SDF)_\d+$"))
                {
                    error = "贴图名称不符合规范";
                    return false;
                }
            }
            
            error = null;
            return true;
        }
    }
}