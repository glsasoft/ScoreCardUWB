/**
 * 文件名: SceneViewToolsData.cs
 * 作用: 场景视图工具数据容器，存储游戏对象和资源的查找配置
 * 作者: ArtTools Team
 * 日期: 2024
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 场景视图工具数据容器
    /// 用于存储和管理工具中的游戏对象和资源查找配置
    /// </summary>
    [CreateAssetMenu(fileName = "SceneViewToolsData", menuName = "ArtTools/Scene View Tools Data")]
    public class SceneViewToolsData : ScriptableObject
    {
        #region 字段定义
        
        /// <summary>
        /// 游戏对象查找配置列表
        /// 存储需要快速查找的游戏对象名称和显示标签
        /// </summary>
        [SerializeField]
        [Tooltip("游戏对象查找配置列表")]
        public List<GameObjectSearchItem> gameObjectItems = new List<GameObjectSearchItem>();
        
        /// <summary>
        /// 资源对象查找配置列表
        /// 存储需要快速访问的资源对象和显示标签
        /// </summary>
        [SerializeField]
        [Tooltip("资源对象查找配置列表")]
        public List<AssetSearchItem> assetItems = new List<AssetSearchItem>();
        
        #endregion
        
        #region Unity回调
        
        /// <summary>
        /// 当Inspector中的值发生变化时调用
        /// 用于通知工具窗口更新显示
        /// </summary>
        private void OnValidate()
        {
            // 通知工具窗口需要重新调整大小
            SceneViewToolsManager.RequestWindowResize();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 添加游戏对象搜索项
        /// </summary>
        /// <param name="item">要添加的游戏对象搜索项</param>
        /// <returns>是否添加成功</returns>
        public bool AddGameObjectItem(GameObjectSearchItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.objectName))
            {
                Debug.LogWarning("[SceneViewToolsData] 无法添加空的游戏对象搜索项");
                return false;
            }
            
            // 检查是否已存在相同名称的项
            if (gameObjectItems.Exists(x => x.objectName == item.objectName))
            {
                Debug.LogWarning($"[SceneViewToolsData] 游戏对象 '{item.objectName}' 已存在于列表中");
                return false;
            }
            
            gameObjectItems.Add(item);
            EditorUtility.SetDirty(this);
            return true;
        }
        
        /// <summary>
        /// 添加资源搜索项
        /// </summary>
        /// <param name="item">要添加的资源搜索项</param>
        /// <returns>是否添加成功</returns>
        public bool AddAssetItem(AssetSearchItem item)
        {
            if (item == null || item.asset == null)
            {
                Debug.LogWarning("[SceneViewToolsData] 无法添加空的资源搜索项");
                return false;
            }
            
            // 检查是否已存在相同资源的项
            if (assetItems.Exists(x => x.asset == item.asset))
            {
                Debug.LogWarning($"[SceneViewToolsData] 资源 '{item.asset.name}' 已存在于列表中");
                return false;
            }
            
            assetItems.Add(item);
            EditorUtility.SetDirty(this);
            return true;
        }
        
        /// <summary>
        /// 移除游戏对象搜索项
        /// </summary>
        /// <param name="objectName">要移除的游戏对象名称</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveGameObjectItem(string objectName)
        {
            int removedCount = gameObjectItems.RemoveAll(x => x.objectName == objectName);
            if (removedCount > 0)
            {
                EditorUtility.SetDirty(this);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 移除资源搜索项
        /// </summary>
        /// <param name="asset">要移除的资源对象</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveAssetItem(Object asset)
        {
            int removedCount = assetItems.RemoveAll(x => x.asset == asset);
            if (removedCount > 0)
            {
                EditorUtility.SetDirty(this);
                return true;
            }
            return false;
        }
        
        #endregion
    }
    
    #region 数据结构定义
    
    /// <summary>
    /// 游戏对象搜索项
    /// 包含游戏对象名称和显示标签
    /// </summary>
    [System.Serializable]
    public class GameObjectSearchItem
    {
        /// <summary>
        /// 显示标签，用于在UI中显示的友好名称
        /// </summary>
        [Tooltip("显示标签，用于在UI中显示的友好名称")]
        public string displayLabel;
        
        /// <summary>
        /// 游戏对象名称，用于实际查找的对象名称
        /// </summary>
        [Tooltip("游戏对象名称，用于实际查找的对象名称")]
        public string objectName;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="objectName">游戏对象名称</param>
        /// <param name="displayLabel">显示标签，如果为空则使用对象名称</param>
        public GameObjectSearchItem(string objectName, string displayLabel = null)
        {
            this.objectName = objectName;
            this.displayLabel = string.IsNullOrEmpty(displayLabel) ? objectName : displayLabel;
        }
        
        /// <summary>
        /// 获取有效的显示标签
        /// </summary>
        /// <returns>显示标签，如果为空则返回对象名称</returns>
        public string GetDisplayLabel()
        {
            return string.IsNullOrEmpty(displayLabel) ? objectName : displayLabel;
        }
    }
    
    /// <summary>
    /// 资源搜索项
    /// 包含资源对象引用和显示标签
    /// </summary>
    [System.Serializable]
    public class AssetSearchItem
    {
        /// <summary>
        /// 显示标签，用于在UI中显示的友好名称
        /// </summary>
        [Tooltip("显示标签，用于在UI中显示的友好名称")]
        public string displayLabel;
        
        /// <summary>
        /// 资源对象引用
        /// </summary>
        [Tooltip("资源对象引用")]
        public Object asset;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="asset">资源对象</param>
        /// <param name="displayLabel">显示标签，如果为空则使用资源名称</param>
        public AssetSearchItem(Object asset, string displayLabel = null)
        {
            this.asset = asset;
            this.displayLabel = string.IsNullOrEmpty(displayLabel) && asset != null ? asset.name : displayLabel;
        }
        
        /// <summary>
        /// 获取有效的显示标签
        /// </summary>
        /// <returns>显示标签，如果为空则返回资源名称</returns>
        public string GetDisplayLabel()
        {
            if (!string.IsNullOrEmpty(displayLabel))
                return displayLabel;
            
            return asset != null ? asset.name : "<Missing Asset>";
        }
    }
    
    #endregion
}