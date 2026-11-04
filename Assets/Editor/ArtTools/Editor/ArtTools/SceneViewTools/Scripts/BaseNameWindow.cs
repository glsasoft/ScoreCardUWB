/**
 * 文件名: BaseNameWindow.cs
 * 作用: 名称编辑窗口基类，提供通用的名称编辑功能
 * 作者: ArtTools Team
 * 日期: 2024
 */

using UnityEngine;
using UnityEditor;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 名称编辑窗口基类
    /// 提供通用的名称编辑功能，子类可以继承并实现具体的保存逻辑
    /// </summary>
    public abstract class BaseNameWindow : EditorWindow
    {
        #region 保护字段
        
        /// <summary>
        /// 当前编辑的标签文本
        /// </summary>
        protected string currentLabel = string.Empty;
        
        /// <summary>
        /// 工具数据引用
        /// </summary>
        protected SceneViewToolsData toolsData;
        
        /// <summary>
        /// 窗口标题
        /// </summary>
        protected string windowTitle = "编辑名称";
        
        #endregion
        
        #region 抽象方法
        
        /// <summary>
        /// 保存数据的抽象方法，由子类实现具体的保存逻辑
        /// </summary>
        /// <param name="label">编辑后的标签</param>
        /// <returns>是否保存成功</returns>
        protected abstract bool SaveData(string label);
        
        /// <summary>
        /// 验证输入的抽象方法，由子类实现具体的验证逻辑
        /// </summary>
        /// <param name="label">要验证的标签</param>
        /// <returns>验证结果和错误信息</returns>
        protected abstract (bool isValid, string errorMessage) ValidateInput(string label);
        
        #endregion
        
        #region 保护方法
        
        /// <summary>
        /// 初始化窗口
        /// </summary>
        /// <param name="initialLabel">初始标签文本</param>
        /// <param name="title">窗口标题</param>
        protected void InitializeWindow(string initialLabel, string title = "编辑名称")
        {
            currentLabel = initialLabel ?? string.Empty;
            windowTitle = title;
            
            // 设置窗口属性
            GUIContent content = new GUIContent(windowTitle);
            titleContent = content;
            maxSize = SceneViewToolsConfig.DEFAULT_WINDOW_SIZE;
            
            // 加载工具数据
            LoadToolsData();
            
            Show();
        }
        
        /// <summary>
        /// 加载工具数据
        /// </summary>
        protected virtual void LoadToolsData()
        {
            toolsData = AssetDatabase.LoadAssetAtPath<SceneViewToolsData>(SceneViewToolsConfig.DATA_ASSET_PATH);
            
            if (toolsData == null)
            {
                Debug.LogError($"[BaseNameWindow] 无法加载工具数据文件: {SceneViewToolsConfig.DATA_ASSET_PATH}");
            }
        }
        
        /// <summary>
        /// 执行保存操作
        /// </summary>
        protected virtual void PerformSave()
        {
            if (toolsData == null)
            {
                Debug.LogError("[BaseNameWindow] 工具数据未加载，无法保存");
                return;
            }
            
            // 验证输入
            var (isValid, errorMessage) = ValidateInput(currentLabel);
            if (!isValid)
            {
                EditorUtility.DisplayDialog("输入错误", errorMessage, "确定");
                return;
            }
            
            // 保存数据
            if (SaveData(currentLabel))
            {
                // 标记数据已修改并保存
                EditorUtility.SetDirty(toolsData);
                AssetDatabase.SaveAssets();
                
                // 关闭窗口
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("保存失败", "保存数据时发生错误，请重试", "确定");
            }
        }
        
        #endregion
        
        #region Unity回调
        
        /// <summary>
        /// 绘制GUI
        /// </summary>
        protected virtual void OnGUI()
        {
            // 检查选择状态
            if (!IsValidSelection())
            {
                EditorGUILayout.HelpBox("请选择有效的对象", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.BeginVertical();
            {
                // 标签输入框
                EditorGUILayout.LabelField("显示标签:");
                currentLabel = EditorGUILayout.TextField(currentLabel);
                
                EditorGUILayout.Space();
                
                // 按钮区域
                EditorGUILayout.BeginHorizontal();
                {
                    // 确认按钮
                    if (GUILayout.Button("确认"))
                    {
                        PerformSave();
                    }
                    
                    // 取消按钮
                    if (GUILayout.Button("取消"))
                    {
                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region 虚方法
        
        /// <summary>
        /// 检查当前选择是否有效
        /// 子类可以重写此方法来实现自定义的选择验证逻辑
        /// </summary>
        /// <returns>选择是否有效</returns>
        protected virtual bool IsValidSelection()
        {
            return Selection.activeObject != null;
        }
        
        #endregion
    }
}