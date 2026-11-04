/**
 * 文件名: ObjectNameWindow.cs
 * 作用: 资源对象名称编辑窗口，用于编辑和保存资源对象的显示标签
 * 作者: ArtTools Team
 * 日期: 2024
 */

using UnityEngine;
using UnityEditor;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 资源对象名称编辑窗口
    /// 继承自BaseNameWindow，专门用于编辑资源对象的显示标签
    /// </summary>
    public class ObjectNameWindow : BaseNameWindow
    {
        #region 私有字段
        
        /// <summary>
        /// 当前编辑的资源搜索项
        /// </summary>
        private AssetSearchItem currentItem;
        
        #endregion
        
        #region 静态方法
        
        /// <summary>
        /// 打开资源对象名称编辑窗口
        /// </summary>
        /// <param name="item">要编辑的资源搜索项</param>
        public static void Open(AssetSearchItem item)
        {
            if (item == null || item.asset == null)
            {
                Debug.LogError("[ObjectNameWindow] 无法打开窗口：资源搜索项为空或资源对象为空");
                return;
            }
            
            ObjectNameWindow window = GetWindow<ObjectNameWindow>();
            window.currentItem = item;
            window.InitializeWindow(item.GetDisplayLabel(), "编辑资源标签");
        }
        
        /// <summary>
        /// 为当前选中的资源对象打开编辑窗口
        /// </summary>
        /// <param name="asset">资源对象</param>
        /// <param name="initialLabel">初始标签，如果为空则使用资源名称</param>
        public static void OpenForAsset(Object asset, string initialLabel = null)
        {
            if (asset == null)
            {
                Debug.LogError("[ObjectNameWindow] 无法打开窗口：资源对象为空");
                return;
            }
            
            var item = new AssetSearchItem(asset, initialLabel);
            Open(item);
        }
        
        #endregion
        
        #region 重写的抽象方法
        
        /// <summary>
        /// 保存资源搜索项数据
        /// </summary>
        /// <param name="label">编辑后的标签</param>
        /// <returns>是否保存成功</returns>
        protected override bool SaveData(string label)
        {
            if (currentItem == null || currentItem.asset == null)
            {
                Debug.LogError("[ObjectNameWindow] 保存失败：当前项或资源对象为空");
                return false;
            }
            
            if (toolsData == null)
            {
                Debug.LogError("[ObjectNameWindow] 保存失败：工具数据未加载");
                return false;
            }
            
            // 更新标签
            currentItem.displayLabel = label;
            
            // 添加到工具数据中
            bool success = toolsData.AddAssetItem(currentItem);
            
            if (success)
            {
                Debug.Log($"[ObjectNameWindow] 成功添加资源搜索项：{currentItem.GetDisplayLabel()}");
            }
            else
            {
                Debug.LogWarning($"[ObjectNameWindow] 资源搜索项可能已存在：{currentItem.GetDisplayLabel()}");
                // 即使项已存在，我们也认为操作成功，因为数据是一致的
                success = true;
            }
            
            return success;
        }
        
        /// <summary>
        /// 验证输入的标签
        /// </summary>
        /// <param name="label">要验证的标签</param>
        /// <returns>验证结果和错误信息</returns>
        protected override (bool isValid, string errorMessage) ValidateInput(string label)
        {
            // 检查标签是否为空
            if (string.IsNullOrWhiteSpace(label))
            {
                return (false, "标签不能为空或只包含空白字符");
            }
            
            // 检查标签长度
            if (label.Length > 50)
            {
                return (false, "标签长度不能超过50个字符");
            }
            
            // 检查是否包含非法字符
            if (label.Contains("\n") || label.Contains("\r") || label.Contains("\t"))
            {
                return (false, "标签不能包含换行符或制表符");
            }
            
            return (true, string.Empty);
        }
        
        #endregion
        
        #region 重写的虚方法
        
        /// <summary>
        /// 检查当前选择是否有效
        /// 对于资源对象窗口，需要检查是否有选中的资源对象
        /// </summary>
        /// <returns>选择是否有效</returns>
        protected override bool IsValidSelection()
        {
            return Selection.activeObject != null && currentItem != null && currentItem.asset != null;
        }
        
        /// <summary>
        /// 绘制GUI
        /// </summary>
        protected override void OnGUI()
        {
            if (currentItem == null || currentItem.asset == null)
            {
                EditorGUILayout.HelpBox("资源对象丢失或未设置", MessageType.Error);
                if (GUILayout.Button("关闭"))
                {
                    Close();
                }
                return;
            }
            
            EditorGUILayout.BeginVertical();
            {
                // 显示资源信息
                EditorGUILayout.LabelField("资源对象:", currentItem.asset.name);
                EditorGUILayout.LabelField("资源类型:", currentItem.asset.GetType().Name);
                
                EditorGUILayout.Space();
                
                // 调用基类的GUI绘制
                base.OnGUI();
            }
            EditorGUILayout.EndVertical();
        }
        
        #endregion
    }
}