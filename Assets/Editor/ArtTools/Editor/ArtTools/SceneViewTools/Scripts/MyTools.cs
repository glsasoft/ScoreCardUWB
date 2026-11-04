/**
 * 文件名: MyTools.cs
 * 作用: 场景视图工具的兼容性包装器和菜单项
 * 作者: ArtTools Team
 * 日期: 2024
 * 说明: 此文件保持向后兼容性，主要功能已迁移到SceneViewToolsManager
 */

using UnityEngine;
using UnityEditor;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 场景视图工具的兼容性包装器
    /// 保持与旧版本的兼容性，同时提供菜单项访问
    /// 主要功能已迁移到SceneViewToolsManager类
    /// </summary>
    public class MyTools : Editor
    {
        #region 静态字段（保持兼容性）
        
        /// <summary>
        /// 是否需要重置窗口大小（向后兼容）
        /// </summary>
        public static bool ISRes
        {
            get => false; // 不再使用，但保持兼容性
            set
            {
                if (value)
                {
                    SceneViewToolsManager.RequestWindowResize();
                }
            }
        }
        
        /// <summary>
        /// 窗口是否在左侧（向后兼容）
        /// </summary>
        public static bool ISleft
        {
            get => SceneViewToolsConfig.IsWindowOnLeft;
            set => SceneViewToolsConfig.IsWindowOnLeft = value;
        }
        
        #endregion
        
        #region 菜单项
        
        /// <summary>
        /// 关闭场景视图工具
        /// </summary>
        [MenuItem("工具/Scene视口工具/关闭Scene视口工具", false, 3)]
        public static void SceneViewToolsOFF()
        {
            SceneViewToolsConfig.IsToolsEnabled = false;
            Debug.Log("[MyTools] 场景视图工具已关闭");
        }
        
        /// <summary>
        /// 打开场景视图工具
        /// </summary>
        [MenuItem("工具/Scene视口工具/打开Scene视口工具", false, 3)]
        public static void SceneViewToolsON()
        {
            SceneViewToolsConfig.IsToolsEnabled = true;
            Debug.Log("[MyTools] 场景视图工具已打开");
        }
        
        /// <summary>
        /// 验证关闭菜单项
        /// </summary>
        [MenuItem("工具/Scene视口工具/关闭Scene视口工具", true)]
        public static bool ValidateSceneViewToolsOFF()
        {
            return SceneViewToolsConfig.IsToolsEnabled;
        }
        
        /// <summary>
        /// 验证打开菜单项
        /// </summary>
        [MenuItem("工具/Scene视口工具/打开Scene视口工具", true)]
        public static bool ValidateSceneViewToolsON()
        {
            return !SceneViewToolsConfig.IsToolsEnabled;
        }
        
        /// <summary>
        /// 打开工具设置
        /// </summary>
        [MenuItem("工具/Scene视口工具/工具设置", false, 4)]
        public static void OpenToolsSettings()
        {
            SceneViewToolsMenuItems.OpenToolsSettings();
        }
        
        /// <summary>
        /// 重置工具设置
        /// </summary>
        [MenuItem("工具/Scene视口工具/重置设置", false, 5)]
        public static void ResetToolsSettings()
        {
            SceneViewToolsMenuItems.ResetToolsSettings();
        }
        
        #endregion
        
        #region 向后兼容性方法
        
        /// <summary>
        /// 初始化切换器（向后兼容）
        /// 注意：实际初始化现在由SceneViewToolsManager处理
        /// </summary>
        [System.Obsolete("此方法已弃用，初始化现在由SceneViewToolsManager自动处理", false)]
        public static void initSwitcher()
        {
            Debug.LogWarning("[MyTools] initSwitcher方法已弃用，初始化现在由SceneViewToolsManager自动处理");
        }
        
        /// <summary>
        /// 绘制窗口（向后兼容）
        /// 注意：窗口绘制现在由SceneViewToolsManager处理
        /// </summary>
        /// <param name="windowId">窗口ID</param>
        [System.Obsolete("此方法已弃用，窗口绘制现在由SceneViewToolsManager处理", false)]
        public static void DoMyWindow(int windowId)
        {
            Debug.LogWarning("[MyTools] DoMyWindow方法已弃用，窗口绘制现在由SceneViewToolsManager处理");
        }
        
        /// <summary>
        /// 绘制第二个窗口（向后兼容）
        /// 注意：窗口绘制现在由SceneViewToolsManager处理
        /// </summary>
        /// <param name="windowId">窗口ID</param>
        [System.Obsolete("此方法已弃用，窗口绘制现在由SceneViewToolsManager处理", false)]
        public static void DoMyWindow2(int windowId)
        {
            Debug.LogWarning("[MyTools] DoMyWindow2方法已弃用，窗口绘制现在由SceneViewToolsManager处理");
        }
        
        #endregion
        
        #region 迁移提示
        
        /// <summary>
        /// 显示迁移信息
        /// </summary>
        [MenuItem("工具/Scene视口工具/关于重构", false, 100)]
        public static void ShowMigrationInfo()
        {
            string message = "场景视图工具已重构！\n\n" +
                           "主要改进：\n" +
                           "• 更好的代码组织和可维护性\n" +
                           "• 改进的命名规范\n" +
                           "• 增强的错误处理\n" +
                           "• 完整的文档注释\n" +
                           "• 统一的配置管理\n\n" +
                           "新的类结构：\n" +
                           "• SceneViewToolsManager - 主要功能管理\n" +
                           "• SceneViewToolsConfig - 配置管理\n" +
                           "• SceneViewToolsData - 数据存储\n" +
                           "• BaseNameWindow - 窗口基类\n" +
                           "• SceneViewToolsMenuItems - 菜单项\n\n" +
                           "旧的MyTools类保持向后兼容性。";
            
            EditorUtility.DisplayDialog("场景视图工具重构信息", message, "确定");
        }
        
        #endregion
    }
}

// 保持旧命名空间的兼容性
namespace ArtTools
{
    /// <summary>
    /// 向后兼容性类型别名
    /// </summary>
    [System.Obsolete("请使用 ArtTools.SceneViewTools.SceneViewToolsData 替代", false)]
    public class MyToolsWindowData : ArtTools.SceneViewTools.SceneViewToolsData
    {
        // 空实现，仅用于向后兼容
    }
    
    /// <summary>
    /// 向后兼容性类型别名
    /// </summary>
    [System.Obsolete("请使用 ArtTools.SceneViewTools.GameObjectSearchItem 替代", false)]
    public struct myGameobject
    {
        public string label;
        public string name;
        
        /// <summary>
        /// 转换为新的数据结构
        /// </summary>
        /// <returns>新的游戏对象搜索项</returns>
        public ArtTools.SceneViewTools.GameObjectSearchItem ToNewFormat()
        {
            return new ArtTools.SceneViewTools.GameObjectSearchItem(name, label);
        }
    }
    
    /// <summary>
    /// 向后兼容性类型别名
    /// </summary>
    [System.Obsolete("请使用 ArtTools.SceneViewTools.AssetSearchItem 替代", false)]
    public struct myObject
    {
        public string label;
        public UnityEngine.Object myobject;
        
        /// <summary>
        /// 转换为新的数据结构
        /// </summary>
        /// <returns>新的资源搜索项</returns>
        public ArtTools.SceneViewTools.AssetSearchItem ToNewFormat()
        {
            return new ArtTools.SceneViewTools.AssetSearchItem(myobject, label);
        }
    }
}