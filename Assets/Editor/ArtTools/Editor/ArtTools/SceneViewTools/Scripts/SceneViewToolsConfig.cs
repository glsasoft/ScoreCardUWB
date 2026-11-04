/**
 * 文件名: SceneViewToolsConfig.cs
 * 作用: 场景视图工具配置管理器，统一管理工具的配置信息和常量
 * 作者: ArtTools Team
 * 日期: 2024
 */

using UnityEngine;
using UnityEditor;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 场景视图工具配置管理器
    /// 负责管理工具的配置信息、数据路径和编辑器偏好设置
    /// </summary>
    public static class SceneViewToolsConfig
    {
        #region 常量定义
        
        /// <summary>
        /// 工具数据文件的相对路径
        /// </summary>
        public const string DATA_ASSET_PATH = "Assets/Editor/ArtTools/Editor/ArtTools/SceneViewTools/Data/SceneViewToolsData.asset";
        
        /// <summary>
        /// GUI皮肤文件路径
        /// </summary>
        public const string GUI_SKIN_PATH = "Assets/Editor/ArtTools/Editor/MyToolsStyle.guiskin";
        
        /// <summary>
        /// 工具开关的EditorPrefs键名
        /// </summary>
        public const string TOOLS_ENABLED_KEY = "SceneViewTools_ON";
        
        /// <summary>
        /// 工具窗口位置的EditorPrefs键名
        /// </summary>
        public const string WINDOW_LEFT_POSITION_KEY = "SceneViewTools_LeftPosition";
        
        /// <summary>
        /// 工具窗口展开状态的EditorPrefs键名
        /// </summary>
        public const string WINDOW_EXPANDED_KEY = "SceneViewTools_Expanded";
        
        #endregion
        
        #region 窗口配置
        
        /// <summary>
        /// 默认窗口大小
        /// </summary>
        public static readonly Vector2 DEFAULT_WINDOW_SIZE = new Vector2(200, 50);
        
        /// <summary>
        /// 时间轴窗口大小
        /// </summary>
        public static readonly Vector2 TIMELINE_WINDOW_SIZE = new Vector2(500, 200);
        
        /// <summary>
        /// 相机列表窗口最大尺寸
        /// </summary>
        public static readonly Vector2 CAMERA_LIST_MAX_SIZE = new Vector2(250, 400);
        
        #endregion
        
        #region 编辑器偏好设置管理
        
        /// <summary>
        /// 获取工具是否启用
        /// </summary>
        /// <returns>工具启用状态</returns>
        public static bool IsToolsEnabled
        {
            get => EditorPrefs.GetBool(TOOLS_ENABLED_KEY, false);
            set => EditorPrefs.SetBool(TOOLS_ENABLED_KEY, value);
        }
        
        /// <summary>
        /// 获取窗口是否在左侧显示
        /// </summary>
        /// <returns>窗口位置状态</returns>
        public static bool IsWindowOnLeft
        {
            get => EditorPrefs.GetBool(WINDOW_LEFT_POSITION_KEY, false);
            set => EditorPrefs.SetBool(WINDOW_LEFT_POSITION_KEY, value);
        }
        
        /// <summary>
        /// 获取窗口是否展开
        /// </summary>
        /// <returns>窗口展开状态</returns>
        public static bool IsWindowExpanded
        {
            get => EditorPrefs.GetBool(WINDOW_EXPANDED_KEY, true);
            set => EditorPrefs.SetBool(WINDOW_EXPANDED_KEY, value);
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化配置设置
        /// 确保所有必要的EditorPrefs键都存在默认值
        /// </summary>
        public static void InitializeSettings()
        {
            if (!EditorPrefs.HasKey(TOOLS_ENABLED_KEY))
            {
                EditorPrefs.SetBool(TOOLS_ENABLED_KEY, false);
            }
            
            if (!EditorPrefs.HasKey(WINDOW_LEFT_POSITION_KEY))
            {
                EditorPrefs.SetBool(WINDOW_LEFT_POSITION_KEY, false);
            }
            
            if (!EditorPrefs.HasKey(WINDOW_EXPANDED_KEY))
            {
                EditorPrefs.SetBool(WINDOW_EXPANDED_KEY, true);
            }
        }
        
        #endregion
    }
}