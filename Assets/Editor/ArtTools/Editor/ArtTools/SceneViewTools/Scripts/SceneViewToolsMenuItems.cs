/**
 * 文件名: SceneViewToolsMenuItems.cs
 * 作用: 场景视图工具菜单项，提供右键菜单功能来添加对象到查找列表
 * 作者: ArtTools Team
 * 日期: 2024
 */

using UnityEngine;
using UnityEditor;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 场景视图工具菜单项
    /// 提供Unity编辑器菜单项，用于将选中的游戏对象和资源添加到快速查找列表
    /// </summary>
    public static class SceneViewToolsMenuItems
    {
        #region 游戏对象菜单项
        
        /// <summary>
        /// 将选中的游戏对象添加到查找列表
        /// 通过GameObject菜单访问
        /// </summary>
        [MenuItem("GameObject/ArtTools/添加到查找列表", false, 0)]
        public static void AddGameObjectToSearchList()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择一个游戏对象", "确定");
                return;
            }
            
            GameObject selectedObject = Selection.activeGameObject;
            
            // 创建游戏对象搜索项
            var searchItem = new GameObjectSearchItem(selectedObject.name, selectedObject.name);
            
            // 打开编辑窗口
            GameObjectNameWindow.Open(searchItem);
        }
        
        /// <summary>
        /// 验证游戏对象菜单项是否可用
        /// </summary>
        /// <returns>菜单项是否可用</returns>
        [MenuItem("GameObject/ArtTools/添加到查找列表", true)]
        public static bool ValidateAddGameObjectToSearchList()
        {
            return Selection.activeGameObject != null;
        }
        
        #endregion
        
        #region 资源菜单项
        
        /// <summary>
        /// 将选中的资源添加到查找列表
        /// 通过Assets菜单访问
        /// </summary>
        [MenuItem("Assets/ArtTools/添加到查找列表", false, 0)]
        public static void AddAssetToSearchList()
        {
            if (Selection.activeObject == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择一个资源对象", "确定");
                return;
            }
            
            Object selectedAsset = Selection.activeObject;
            
            // 创建资源搜索项
            var searchItem = new AssetSearchItem(selectedAsset, selectedAsset.name);
            
            // 打开编辑窗口
            ObjectNameWindow.Open(searchItem);
        }
        
        /// <summary>
        /// 验证资源菜单项是否可用
        /// </summary>
        /// <returns>菜单项是否可用</returns>
        [MenuItem("Assets/ArtTools/添加到查找列表", true)]
        public static bool ValidateAddAssetToSearchList()
        {
            return Selection.activeObject != null && !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
        }
        
        #endregion
        
        #region 工具菜单项
        
        /// <summary>
        /// 打开场景视图工具设置窗口
        /// </summary>
        [MenuItem("Tools/ArtTools/场景视图工具设置", false, 100)]
        public static void OpenToolsSettings()
        {
            var toolsData = GetOrCreateToolsData();
            if (toolsData != null)
            {
                Selection.activeObject = toolsData;
                EditorGUIUtility.PingObject(toolsData);
            }
        }
        
        /// <summary>
        /// 切换场景视图工具的启用状态
        /// </summary>
        [MenuItem("Tools/ArtTools/启用/禁用场景视图工具", false, 101)]
        public static void ToggleSceneViewTools()
        {
            bool currentState = SceneViewToolsConfig.IsToolsEnabled;
            SceneViewToolsConfig.IsToolsEnabled = !currentState;
            
            string message = SceneViewToolsConfig.IsToolsEnabled ? "场景视图工具已启用" : "场景视图工具已禁用";
            Debug.Log($"[SceneViewToolsMenuItems] {message}");
            
            // 刷新场景视图
            SceneView.RepaintAll();
        }
        
        /// <summary>
        /// 验证工具启用状态菜单项
        /// </summary>
        /// <returns>菜单项是否被选中</returns>
        [MenuItem("Tools/ArtTools/启用/禁用场景视图工具", true)]
        public static bool ValidateToggleSceneViewTools()
        {
            Menu.SetChecked("Tools/ArtTools/启用/禁用场景视图工具", SceneViewToolsConfig.IsToolsEnabled);
            return true;
        }
        
        /// <summary>
        /// 重置场景视图工具设置
        /// </summary>
        [MenuItem("Tools/ArtTools/重置场景视图工具设置", false, 102)]
        public static void ResetToolsSettings()
        {
            if (EditorUtility.DisplayDialog("重置设置", "确定要重置所有场景视图工具设置吗？这将清除所有保存的查找项。", "确定", "取消"))
            {
                // 重置EditorPrefs设置
                SceneViewToolsConfig.IsToolsEnabled = false;
                SceneViewToolsConfig.IsWindowOnLeft = false;
                SceneViewToolsConfig.IsWindowExpanded = true;
                
                // 清空工具数据
                var toolsData = GetOrCreateToolsData();
                if (toolsData != null)
                {
                    toolsData.gameObjectItems.Clear();
                    toolsData.assetItems.Clear();
                    EditorUtility.SetDirty(toolsData);
                    AssetDatabase.SaveAssets();
                }
                
                Debug.Log("[SceneViewToolsMenuItems] 场景视图工具设置已重置");
                SceneView.RepaintAll();
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取或创建工具数据资源
        /// </summary>
        /// <returns>工具数据对象</returns>
        private static SceneViewToolsData GetOrCreateToolsData()
        {
            var toolsData = AssetDatabase.LoadAssetAtPath<SceneViewToolsData>(SceneViewToolsConfig.DATA_ASSET_PATH);
            
            if (toolsData == null)
            {
                // 确保目录存在
                string directory = System.IO.Path.GetDirectoryName(SceneViewToolsConfig.DATA_ASSET_PATH);
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    string[] pathParts = directory.Replace('\\', '/').Split('/');
                    string currentPath = pathParts[0];
                    
                    for (int i = 1; i < pathParts.Length; i++)
                    {
                        string newPath = currentPath + "/" + pathParts[i];
                        if (!AssetDatabase.IsValidFolder(newPath))
                        {
                            AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                        }
                        currentPath = newPath;
                    }
                }
                
                // 创建新的工具数据资源
                toolsData = ScriptableObject.CreateInstance<SceneViewToolsData>();
                toolsData.name = "SceneViewToolsData";
                AssetDatabase.CreateAsset(toolsData, SceneViewToolsConfig.DATA_ASSET_PATH);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"[SceneViewToolsMenuItems] 已创建新的工具数据文件: {SceneViewToolsConfig.DATA_ASSET_PATH}");
            }
            
            return toolsData;
        }
        
        #endregion
    }
}