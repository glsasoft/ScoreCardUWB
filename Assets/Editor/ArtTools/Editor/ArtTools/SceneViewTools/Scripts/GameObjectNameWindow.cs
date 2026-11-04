/**
 * 文件名: GameObjectNameWindow.cs
 * 作用: 游戏对象名称编辑窗口，用于编辑和保存游戏对象的显示标签
 * 作者: ArtTools Team
 * 日期: 2024
 */

using UnityEngine;
using UnityEditor;

namespace ArtTools.SceneViewTools
{
    /// <summary>
    /// 游戏对象名称编辑窗口
    /// 继承自BaseNameWindow，专门用于编辑游戏对象的显示标签
    /// </summary>
    public class GameObjectNameWindow : BaseNameWindow
    {
        #region 私有字段
        
        /// <summary>
        /// 当前编辑的游戏对象搜索项
        /// </summary>
        private GameObjectSearchItem currentItem;
        
        #endregion
        
        #region 静态方法
        
        /// <summary>
        /// 打开游戏对象名称编辑窗口
        /// </summary>
        /// <param name="item">要编辑的游戏对象搜索项</param>
        public static void Open(GameObjectSearchItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.objectName))
            {
                Debug.LogError("[GameObjectNameWindow] 无法打开窗口：游戏对象搜索项为空或对象名称为空");
                return;
            }
            
            GameObjectNameWindow window = GetWindow<GameObjectNameWindow>();
            window.currentItem = item;
            window.InitializeWindow(item.GetDisplayLabel(), "编辑游戏对象标签");
        }
        
        /// <summary>
        /// 为当前选中的游戏对象打开编辑窗口
        /// </summary>
        /// <param name="gameObject">游戏对象</param>
        /// <param name="initialLabel">初始标签，如果为空则使用游戏对象名称</param>
        public static void OpenForGameObject(GameObject gameObject, string initialLabel = null)
        {
            if (gameObject == null)
            {
                Debug.LogError("[GameObjectNameWindow] 无法打开窗口：游戏对象为空");
                return;
            }
            
            var item = new GameObjectSearchItem(gameObject.name, initialLabel);
            Open(item);
        }
        
        #endregion
        
        #region 重写的抽象方法
        
        /// <summary>
        /// 保存游戏对象搜索项数据
        /// </summary>
        /// <param name="label">编辑后的标签</param>
        /// <returns>是否保存成功</returns>
        protected override bool SaveData(string label)
        {
            if (currentItem == null || string.IsNullOrEmpty(currentItem.objectName))
            {
                Debug.LogError("[GameObjectNameWindow] 保存失败：当前项或对象名称为空");
                return false;
            }
            
            if (toolsData == null)
            {
                Debug.LogError("[GameObjectNameWindow] 保存失败：工具数据未加载");
                return false;
            }
            
            // 更新标签
            currentItem.displayLabel = label;
            
            // 添加到工具数据中
            bool success = toolsData.AddGameObjectItem(currentItem);
            
            if (success)
            {
                Debug.Log($"[GameObjectNameWindow] 成功添加游戏对象搜索项：{currentItem.GetDisplayLabel()}");
            }
            else
            {
                Debug.LogWarning($"[GameObjectNameWindow] 游戏对象搜索项可能已存在：{currentItem.GetDisplayLabel()}");
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
        /// 对于游戏对象窗口，需要检查是否有选中的游戏对象
        /// </summary>
        /// <returns>选择是否有效</returns>
        protected override bool IsValidSelection()
        {
            return Selection.activeGameObject != null && currentItem != null && !string.IsNullOrEmpty(currentItem.objectName);
        }
        
        /// <summary>
        /// 绘制GUI
        /// </summary>
        protected override void OnGUI()
        {
            if (currentItem == null || string.IsNullOrEmpty(currentItem.objectName))
            {
                EditorGUILayout.HelpBox("游戏对象信息丢失或未设置", MessageType.Error);
                if (GUILayout.Button("关闭"))
                {
                    Close();
                }
                return;
            }
            
            EditorGUILayout.BeginVertical();
            {
                // 显示游戏对象信息
                EditorGUILayout.LabelField("游戏对象名称:", currentItem.objectName);
                
                // 尝试查找当前场景中的对象
                GameObject foundObject = GameObject.Find(currentItem.objectName);
                if (foundObject != null)
                {
                    EditorGUILayout.LabelField("场景中状态:", "已找到");
                    EditorGUILayout.LabelField("激活状态:", foundObject.activeInHierarchy ? "激活" : "未激活");
                }
                else
                {
                    EditorGUILayout.LabelField("场景中状态:", "未找到或已删除");
                }
                
                EditorGUILayout.Space();
                
                // 调用基类的GUI绘制
                base.OnGUI();
            }
            EditorGUILayout.EndVertical();
        }
        
        #endregion
    }
}

