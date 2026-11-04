using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

public class BuiltInResViewTool : EditorWindow
{
    [MenuItem("Tools/显示内置资源")]
    static void ShowWin()
    {
        BuiltInResViewTool _instance = GetWindow<BuiltInResViewTool>("Unity内置资源");
        _instance.Show();
    }

    #region 变量
    // 通用
    private int titleSelectIndex = 0;
    private string[] titleStrs = new string[] { "图标 (Icons)", "风格 (Styles)" };

    // Icon
    private string unityEditorResourcesPath = "";
    private List<GUIContent> iconContentList;
    private List<GUIContent> showIconContentList;
    private Vector2 iconScrollPosition = Vector2.zero;
    private int iconSize = 40;
    private int rowCount = 18;
    private string iconFilterStr = "";
    private int builtInIconListExist = -1; // -1: 未检查, 0: 不存在, 1: 存在

    // Style
    private List<GUIStyle> guiStyles;
    private List<GUIStyle> showGuiStyleList;
    private Vector2 stylesScrollPosition = Vector2.zero;
    private string styleFilterStr = "";
    #endregion

    private void OnGUI()
    {
        // 顶部标签页
        GUILayout.Space(10);
        titleSelectIndex = GUILayout.Toolbar(titleSelectIndex, titleStrs, GUILayout.Height(30));
        GUILayout.Space(10);

        // 根据选择绘制不同界面
        switch (titleSelectIndex)
        {
            case 0:
                DrawIcons();
                break;
            case 1:
                DrawStyle();
                break;
        }
    }

    #region Icon
    /// <summary>
    /// 绘制Icon列表
    /// </summary>
    private void DrawIcons()
    {
        DrawHeader("内置图标浏览器 (Built-in Icons)");

        // 路径选择区域
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle helpTextStyle = new GUIStyle(EditorStyles.label);
        helpTextStyle.wordWrap = true;
        helpTextStyle.normal.textColor = Color.yellow;
        GUILayout.Label("如果下方未显示任何图标，请先选择您Unity安装目录下的 'Editor/Data/Resources/unity editor resources' 文件，然后点击“生成”按钮。此操作只需执行一次。", helpTextStyle);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("资源文件路径:", GUILayout.Width(100));
        EditorGUILayout.SelectableLabel(unityEditorResourcesPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon"), GUILayout.Width(40), GUILayout.Height(20)))
        {
            unityEditorResourcesPath = EditorUtility.OpenFilePanel("请选择 unity editor resources 文件", "", "");
        }
        if (!string.IsNullOrEmpty(unityEditorResourcesPath))
        {
            if (GUILayout.Button("生成", GUILayout.Width(80), GUILayout.Height(20)))
            {
                CreateBuiltInIconList();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        // 检查Icon列表文件是否存在
        if (builtInIconListExist < 0)
        {
            CheckBuiltInIconList();
        }

        if (builtInIconListExist == 0)
        {
            EditorGUILayout.HelpBox("请先选择 'unity editor resources' 文件路径并点击“生成”按钮来创建图标索引。", MessageType.Info);
            return;
        }

        if (iconContentList == null)
        {
            GetAllIcons();
        }
        
        // 搜索和刷新区域
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("搜索:", GUILayout.Width(40));
        EditorGUI.BeginChangeCheck();
        iconFilterStr = EditorGUILayout.TextField(iconFilterStr, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
        if (EditorGUI.EndChangeCheck())
        {
            FilterIconList();
        }
        if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            GetAllIcons();
        }
        if (showIconContentList != null)
        {
             GUILayout.FlexibleSpace();
            GUILayout.Label($"找到: {showIconContentList.Count} 个");
        }
        GUILayout.EndHorizontal();

        if (showIconContentList == null || showIconContentList.Count == 0)
        {
            EditorGUILayout.HelpBox("没有找到匹配的图标。", MessageType.Info);
            return;
        }

        // 图标显示区域
        iconScrollPosition = GUILayout.BeginScrollView(iconScrollPosition);
        // 使用 flexibleSpace 替代复杂的循环计算，实现自动换行
        EditorGUILayout.BeginVertical();
        int columnCount = Mathf.FloorToInt(position.width / (iconSize + 5));
        if (columnCount < 1) columnCount = 1;
        for (int i = 0; i < showIconContentList.Count; i += columnCount)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < columnCount && i + j < showIconContentList.Count; j++)
            {
                if (GUILayout.Button(showIconContentList[i + j], GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                {
                    string iconName = showIconContentList[i + j].image.name;
                    CopyValue(iconName);
                    ShowNotification(new GUIContent($"'{iconName}' 已复制到剪切板"));
                    Debug.Log($"Icon Name: '{iconName}' has been copied to the clipboard.");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 通过搜索过滤内置icon的显示列表
    /// </summary>
    private void FilterIconList()
    {
        if (iconContentList == null) return;
        
        showIconContentList = new List<GUIContent>();
        if (string.IsNullOrEmpty(iconFilterStr))
        {
            showIconContentList.AddRange(iconContentList);
        }
        else
        {
            string tempStr = iconFilterStr.ToLower();
            showIconContentList = iconContentList.Where(icon => icon.image != null && icon.image.name.ToLower().Contains(tempStr)).ToList();
        }
    }

    /// <summary>
    /// 检查内置icon名字列表是否已经生成
    /// </summary>
    private void CheckBuiltInIconList()
    {
        string path = GetIconListPath();
        builtInIconListExist = File.Exists(path) ? 1 : 0;
    }

    /// <summary>
    /// 通过unity editor resource生成内置icon名字列表文件
    /// </summary>
    private void CreateBuiltInIconList()
    {
        if (string.IsNullOrEmpty(unityEditorResourcesPath))
        {
            EditorUtility.DisplayDialog("错误", "请先选择 'unity editor resources' 文件路径！", "确定");
            return;
        }

        int count = 0;
        bool success = false;
        try
        {
            EditorUtility.DisplayProgressBar("生成图标列表", "正在读取资源文件...", 0.1f);
            string content = ReadFileText(unityEditorResourcesPath);
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("读取文件失败: " + unityEditorResourcesPath);
                builtInIconListExist = 0;
                return;
            }
            
            EditorUtility.DisplayProgressBar("生成图标列表", "正在解析图标名称...", 0.4f);
            HashSet<string> iconSet = new HashSet<string>();
            // 正则表达式匹配 Unity 2019+
            string regexStr = @"([a-zA-Z0-9\._\- ]+)\.png";
            MatchCollection matches = Regex.Matches(content, regexStr);
            foreach (Match match in matches)
            {
                iconSet.Add(Path.GetFileNameWithoutExtension(match.Groups[1].Value));
            }

            // 旧版Unity的匹配规则
            regexStr = @"(?<=icons/)[^\.]+(?=.png)";
            string[] oldMatches = GetAllMatchs(content, regexStr);
            if (oldMatches != null)
            {
                foreach(var m in oldMatches) iconSet.Add(m.Trim());
            }

            EditorUtility.DisplayProgressBar("生成图标列表", "正在写入索引文件...", 0.8f);
            string saveContent = string.Join("\n", iconSet);
            string savePath = GetIconListPath();
            CheckFileSavePath(savePath);
            File.WriteAllText(savePath, saveContent);
            builtInIconListExist = 1;
            
            // 生成后立即刷新
            GetAllIcons();
            count = iconSet.Count;
            success = true;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (success)
        {
            EditorUtility.DisplayDialog("成功", $"已成功生成图标列表，共找到 {count} 个图标。", "确定");
        }
    }

    /// <summary>
    /// 从内置icon名字列表读取所有可用的icon
    /// </summary>
    private void GetAllIcons()
    {
        iconContentList = new List<GUIContent>();
        string path = GetIconListPath();
        if (!File.Exists(path)) return;

        try
        {
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                // 每100个图标更新一次进度条，避免UI调用过于频繁
                if (i % 100 == 0)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("加载图标", $"正在加载 {i}/{lines.Length}...", (float)i / lines.Length))
                    {
                        break; // 用户取消
                    }
                }
                
                GUIContent icon = EditorGUIUtility.IconContent(lines[i].Trim());
                if (icon != null && icon.image != null)
                {
                    iconContentList.Add(icon);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        FilterIconList();
    }

    /// <summary>
    /// 获取内置icon名字列表的文件路径
    /// </summary>
    private string GetIconListPath()
    {
        return "Library/BuiltinIconList.txt"; // 存放在Library下，避免影响项目资源
    }
    #endregion

    #region Style
    /// <summary>
    /// 绘制风格列表
    /// </summary>
    private void DrawStyle()
    {
        DrawHeader("内置GUI风格浏览器 (Built-in GUIStyles)");
        if (guiStyles == null)
        {
            GetAllEditorStyles();
        }
        
        // 搜索和刷新区域
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("搜索:", GUILayout.Width(40));
        EditorGUI.BeginChangeCheck();
        styleFilterStr = EditorGUILayout.TextField(styleFilterStr, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
        if (EditorGUI.EndChangeCheck())
        {
            FilterStyleList();
        }
        if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            GetAllEditorStyles();
        }
        GUILayout.EndHorizontal();

        if (showGuiStyleList == null || showGuiStyleList.Count == 0)
        {
             EditorGUILayout.HelpBox("没有找到匹配的Style。", MessageType.Info);
            return;
        }

        // 风格列表显示
        stylesScrollPosition = GUILayout.BeginScrollView(stylesScrollPosition);
        for (int i = 0; i < showGuiStyleList.Count; i++)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            // 显示Style名称，并提供复制按钮
            EditorGUILayout.SelectableLabel(showGuiStyleList[i].name, GUILayout.Height(30), GUILayout.Width(250));
            if(GUILayout.Button("复制名称", GUILayout.Width(70), GUILayout.Height(20)))
            {
                 CopyValue(showGuiStyleList[i].name);
                 ShowNotification(new GUIContent($"'{showGuiStyleList[i].name}' 已复制"));
            }
            GUILayout.FlexibleSpace();
            // 样例展示
            GUILayout.Label("这是一个测试范例", showGuiStyleList[i], GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 通过搜索过滤风格列表
    /// </summary>
    private void FilterStyleList()
    {
        if (guiStyles == null) return;
        
        showGuiStyleList = new List<GUIStyle>();
        if (string.IsNullOrEmpty(styleFilterStr))
        {
            showGuiStyleList.AddRange(guiStyles);
        }
        else
        {
            string tempStr = styleFilterStr.ToLower();
            showGuiStyleList = guiStyles.Where(style => style.name.ToLower().Contains(tempStr)).ToList();
        }
    }

    /// <summary>
    /// 获取所有内置的风格
    /// </summary>
    private void GetAllEditorStyles()
    {
        guiStyles = new List<GUIStyle>();
        // 通过反射获取EditorStyles所有静态GUIStyle属性
        PropertyInfo[] properties = typeof(EditorStyles).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType == typeof(GUIStyle))
            {
                guiStyles.Add((GUIStyle)property.GetValue(null, null));
            }
        }
        // 添加当前皮肤的所有style
        if (GUI.skin != null)
        {
            foreach (GUIStyle style in GUI.skin)
            {
                if (!guiStyles.Contains(style))
                {
                    guiStyles.Add(style);
                }
            }
        }
        FilterStyleList(); // 初始化显示列表
    }
    #endregion

    #region 辅助方法 (Helper Methods)
    /// <summary>
    /// 绘制标题和分割线
    /// </summary>
    private void DrawHeader(string title)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);
    }
    
    /// <summary>
    /// 复制字符串到系统剪切板
    /// </summary>
    private void CopyValue(string value)
    {
        EditorGUIUtility.systemCopyBuffer = value;
    }

    /// <summary>
    /// 从文件中读取文本
    /// </summary>
    private string ReadFileText(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return File.ReadAllText(path);
        }
        catch(System.Exception e)
        {
            Debug.LogError($"读取文件失败: {path}, 错误: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 获取所有正则匹配项
    /// </summary>
    private string[] GetAllMatchs(string content, string regexStr)
    {
        if (string.IsNullOrEmpty(content)) return null;
        MatchCollection matches = Regex.Matches(content, regexStr);
        string[] result = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
        {
            result[i] = matches[i].Value;
        }
        return result;
    }
    
    /// <summary>
    /// 检查并创建文件保存路径的目录
    /// </summary>
    private void CheckFileSavePath(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
    #endregion
}

