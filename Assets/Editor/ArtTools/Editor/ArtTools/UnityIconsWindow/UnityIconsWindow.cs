/*
 * 文件名: UnityIconsWindow.cs
 * 作用: Unity编辑器窗口，用于显示和管理Unity内置图标
 * 作者: ArtTools团队
 * 创建日期: 2024
 * 描述: 提供一个可视化界面来浏览Unity内置的所有图标资源，支持点击复制图标调用代码
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtTools
{
    /// <summary>
    /// Unity内置图标浏览窗口
    /// 继承自EditorWindow，提供图标浏览和复制功能
    /// </summary>
    /// <remarks>
    /// 主要功能:
    /// 1. 显示Unity内置的所有图标
    /// 2. 支持点击图标复制对应的代码到剪贴板
    /// 3. 提供网格布局的图标展示界面
    /// </remarks>
    public class UnityIconsWindow : EditorWindow
    {
        /// <summary>
        /// 显示Unity内置图标窗口的菜单项
        /// </summary>
        /// <remarks>
        /// 通过Unity编辑器菜单"工具/Unity内置图标"调用
        /// 创建并显示图标浏览窗口，设置窗口大小和标题
        /// </remarks>
        [MenuItem(("工具/Unity内置图标"))]
        private static void ShowWindow()
        {
            // 获取或创建UnityIconsWindow实例
            var window = GetWindow<UnityIconsWindow>();
            // 设置窗口固定大小为1280x720
            window.minSize = window.maxSize = new Vector2(1280, 720);
            // 设置窗口标题和图标
            window.titleContent = new GUIContent("Unity内置图标", EditorGUIUtility.IconContent("d_UnityLogo").image);
        }

        /// <summary>
        /// 创建窗口的用户界面
        /// </summary>
        /// <remarks>
        /// Unity编辑器窗口的UI创建方法，使用UIElements系统
        /// 加载样式表，创建图标网格布局，设置点击事件处理
        /// </remarks>
        public void CreateGUI()
        {
            // 加载USS样式表文件
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/ArtTools/Editor/ArtTools/UnityIconsWindow/UnityIconsWindow.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            // 查找所有Texture2D资源（包括Unity内置图标）
            var texture2Ds = Resources.FindObjectsOfTypeAll<Texture2D>();

            // 创建图标容器，用于网格布局显示
            var container = new VisualElement();
            container.AddToClassList("container");

            // 遍历所有找到的纹理资源
            for (int i = 0; i < texture2Ds.Length; i++)
            {
                // 获取图标名称
                var iconName = texture2Ds[i].name;
                // 尝试通过EditorGUIUtility获取图标
                var texture = EditorGUIUtility.IconContent(iconName).image;
                if (texture is Texture2D texture2D)
                {
                    // 创建图标显示元素
                    var image = new VisualElement();
                    image.style.backgroundImage = new StyleBackground(texture2D);
                    image.AddToClassList("container-item");
                    container.Add(image);

                    // 注册点击事件：复制图标调用代码到剪贴板
                    image.RegisterCallback<ClickEvent>(evt =>
                    {
                        // 生成获取图标的代码字符串
                        var copyStr = $"EditorGUIUtility.IconContent({iconName}).image";
                        // 复制到系统剪贴板
                        GUIUtility.systemCopyBuffer = copyStr;
                        // 在控制台输出提示信息
                        Debug.Log($"已复制到剪切板:{iconName}");
                    });
                }
            }

            // 创建滚动视图容器，支持大量图标的滚动浏览
            var scrollView = new ScrollView();
            scrollView.Add(container);

            // 将滚动视图添加到根视觉元素
            rootVisualElement.Add(scrollView);
        }
    }
}