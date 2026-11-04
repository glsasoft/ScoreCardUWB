using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LetsPiu
{
    public class AssetSelector : EditorWindowWithAssetSelector
    {
        private Vector2 scrollPosition;
        
        private bool foldoutNameFilter;     
        private string namePattern;

        private bool foldoutTypeFilter;
        private string typePattern;
        private string[] availableTypeNames = new string[0];
        private int selectedTypeIndex = 0;

        private bool foldoutTexSelector;
        private bool texSelector;
        private Object prefabForTexSelector;

        private bool foldoutMoveAssets;
        private Object targetFold;
        
        
        [MenuItem("Tools/Graphics/资产选择器")]
        public static void OpenWindow()
        {
            GetWindow<AssetSelector>("资产选择器").Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height));
            
            DrawAssetsGUI();
            // PerformDragEvent();  
            
            foldoutNameFilter = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutNameFilter, "名称筛选器");
            if (foldoutNameFilter)
            {
                namePattern = EditorGUILayout.TextField("名称模式", namePattern);
                if (GUILayout.Button("筛选名称")) FilterName(namePattern, false);
                if (GUILayout.Button("剔除名称")) FilterName(namePattern, true);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.Space();
            foldoutTypeFilter = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTypeFilter, "类型筛选器");
            if (foldoutTypeFilter)
            {
                // 自动提取类型列表
                UpdateAvailableTypes();
                if (availableTypeNames.Length > 0)
                {
                    selectedTypeIndex = EditorGUILayout.Popup("选择类型", selectedTypeIndex, availableTypeNames);
                    string selectedType = availableTypeNames[selectedTypeIndex];
                    
                    if(GUILayout.Button("筛选类型")) FilterType(selectedType, false);
                    if(GUILayout.Button("剔除类型")) FilterType(selectedType, true);
                }
                else
                {
                    EditorGUILayout.HelpBox("当前资产列表为空，无法提取类型", MessageType.Info);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            
            EditorGUILayout.Space();
            foldoutTexSelector = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTexSelector, "依赖贴图提取器");
            if (foldoutTexSelector)
            {
                prefabForTexSelector = EditorGUILayout.ObjectField("需提取tex的prefab", prefabForTexSelector, typeof(Object), true);
                if (GUILayout.Button("提取tex"))
                {
                    TexSelector();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.Space();
            foldoutMoveAssets = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMoveAssets, "移动Assets源文件");
            if (foldoutMoveAssets)
            {
                targetFold = EditorGUILayout.ObjectField("目标文件夹", targetFold, typeof(Object), false);
                if (GUILayout.Button("移动Assets源文件"))
                {
                    MoveAssets();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            
            
            EditorGUILayout.EndScrollView();
        }

        private void MoveAssets()
        {
            if (targetFold == null)
            {
                Debug.LogError("未指定目标文件夹！");
                return;
            }
            
            string targetPath = AssetDatabase.GetAssetPath(targetFold);

            if (!AssetDatabase.IsValidFolder(targetPath))
            {
                Debug.LogError("目标路径不是一个有效文件夹");
                return;
            }

            foreach (Object obj in assets)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"跳过无效资源:{obj}");
                    continue;
                }
                
                string fileName = System.IO.Path.GetFileName(assetPath);
                string newPath = System.IO.Path.Combine(targetPath, fileName).Replace('\\', '/');
                
                //同名处理
                if (AssetDatabase.MoveAsset(assetPath, newPath) != string.Empty)
                {
                    Debug.LogWarning($"移动失败或目标已存在:{assetPath}");
                }
                else
                {
                    Debug.Log($"已移动：{fileName} >>> {targetPath}");
                }
            }
            AssetDatabase.Refresh();
        }

        private void TexSelector()
        {
            if (prefabForTexSelector == null)
            {
                Debug.LogWarning("请指定 Prefab！");
                return;
            }

            // 获取所有依赖资源
            Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { prefabForTexSelector });

            HashSet<Object> textureSet = new();
            foreach (var obj in dependencies)
            {
                if (obj is Texture || obj is RenderTexture)
                {
                    textureSet.Add(obj);
                }
            }

            assets = new List<Object>(textureSet);
            Debug.Log($"收集完成，共找到 {assets.Count} 张贴图。");
        }

        private void PerformDragEvent()
        {
            Event e = Event.current;
            // 相比基类而言，条件更宽松，只要在窗口内，任意处都可以
            switch (e.type)
            {
                case EventType.DragUpdated:
                {
                    if (mouseOverWindow != this) break;
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    
                    break;
                }
                case EventType.DragPerform:
                {
                    if (mouseOverWindow != this) break;
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    DragAndDrop.AcceptDrag();
        
                    AddAssets(DragAndDrop.objectReferences);
                    
                    break;
                }
            }
        }

        private void FilterName(string pattern, bool keepOrRemove)
        {
            List<Object> list = new();

            foreach (Object asset in assets)
                // 正则匹配 ^ keepOrRemove
                if (Regex.IsMatch(asset.name, pattern) ^ keepOrRemove)
                    list.Add(asset);
            assets = list;
        }

        private void UpdateAvailableTypes()
        {
            HashSet<string> typeSet = new();    // HashSet自动去重
            foreach (Object asset in assets)
            {
                if(asset == null) continue;
                typeSet.Add(asset.GetType().Name);
            }
            // availableTypeNames = new List<string>(typeSet).ToArray();   //HashSet<string> -> List<string> -> String[]
            availableTypeNames = typeSet.ToArray();     // 来自 System.Linq
            
            if(selectedTypeIndex >= availableTypeNames.Length)
                selectedTypeIndex = 0;
        }
        private void FilterType(string pattern, bool keepOrRemove)
        {
            List<Object> list = new();
            foreach (Object asset in assets)
            {
                if (asset == null) continue;
                
                string typeName = asset.GetType().Name;
                if(Regex.IsMatch(typeName, pattern) ^ keepOrRemove)
                    list.Add(asset);
            }
            assets = list;
        }
        
    }
}