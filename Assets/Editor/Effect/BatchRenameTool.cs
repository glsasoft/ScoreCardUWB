using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

public class BatchRenameTool : EditorWindow
{
    // 重命名配置
    private string prefix = "";
    private string middleContent = "";
    private string suffix = "";
    private int startNumber = 0;
    private int digitCount = 3; // 序列号位数，如001
    
    // 移动文件配置
    private bool enableMove = false;
    private string targetFolder = "";

    // 是否自动解决冲突（自动排序）
    private bool autoResolveConflict = true;

    // 中间部分模式
    private enum MiddleMode
    {
        KeepOriginalName,   // 保持原名
        CustomText,         // 自定义文本
        SequenceNumber,     // 序列号
        Remove              // 移除中间部分（仅使用前后缀）
    }
    private MiddleMode middleMode = MiddleMode.KeepOriginalName;

    // 预览列表滚动位置
    private Vector2 scrollPosition;

    // 选中的对象
    private List<Object> selectedObjects = new List<Object>();
    
    // 缓存预览名字结果 [Path -> NewName]
    private Dictionary<string, string> previewNames = new Dictionary<string, string>();

    [MenuItem("Tools/批量重命名工具")]
    public static void ShowWindow()
    {
        BatchRenameTool window = GetWindow<BatchRenameTool>("批量重命名");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    private void OnEnable()
    {
        // 窗口打开或重新聚焦时更新选中列表
        UpdateSelection();
        Selection.selectionChanged += UpdateSelection;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= UpdateSelection;
    }

    private void UpdateSelection()
    {
        selectedObjects = Selection.objects.ToList();
        // 过滤掉不可重命名的资源（如脚本自身在编译时可能无法重命名，或者非Asset资源）
        selectedObjects = selectedObjects.Where(obj => AssetDatabase.Contains(obj)).OrderBy(obj => obj.name).ToList();
        CalculatePreviewNames();
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("批量重命名工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. 设置区域
        DrawSettings();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("选中的文件 (" + selectedObjects.Count + ")", EditorStyles.boldLabel);
        
        // 2. 预览列表区域
        DrawPreviewList();

        EditorGUILayout.Space();

        // 3. 操作区域
        DrawActionButtons();
    }

    private void DrawSettings()
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("命名规则", EditorStyles.boldLabel);
        prefix = EditorGUILayout.TextField("前缀 (Prefix)", prefix);
        
        EditorGUILayout.Space();
        middleMode = (MiddleMode)EditorGUILayout.EnumPopup("中间部分模式", middleMode);

        if (middleMode == MiddleMode.CustomText)
        {
            middleContent = EditorGUILayout.TextField("中间文本", middleContent);
        }
        else if (middleMode == MiddleMode.SequenceNumber)
        {
            EditorGUILayout.BeginHorizontal();
            startNumber = EditorGUILayout.IntField("起始序号", startNumber);
            digitCount = EditorGUILayout.IntSlider("序号位数", digitCount, 1, 6);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("示例: " + startNumber.ToString("D" + digitCount), MessageType.Info);
        }

        EditorGUILayout.Space();
        suffix = EditorGUILayout.TextField("后缀 (Suffix)", suffix);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("冲突处理", EditorStyles.boldLabel);
        autoResolveConflict = EditorGUILayout.Toggle("自动解决重名冲突", autoResolveConflict);
        if (autoResolveConflict)
        {
            EditorGUILayout.HelpBox("如果名字冲突：\n1. 若以数字结尾，数字+1\n2. 否则追加 _1", MessageType.None);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("文件移动", EditorStyles.boldLabel);
        enableMove = EditorGUILayout.Toggle("同时移动文件", enableMove);
        if (enableMove)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标文件夹:", GUILayout.Width(70));
            targetFolder = EditorGUILayout.TextField(targetFolder);
            if (GUILayout.Button("选择", GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFolderPanel("选择目标文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        targetFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "请选择 Assets 目录下的文件夹！", "确定");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
        {
            CalculatePreviewNames();
        }

        EditorGUILayout.EndVertical();
        
        if (GUILayout.Button("清除设置"))
        {
            prefix = "";
            middleMode = MiddleMode.KeepOriginalName;
            middleContent = "";
            suffix = "";
            startNumber = 0;
            autoResolveConflict = true;
            enableMove = false;
            targetFolder = "";
            CalculatePreviewNames();
            GUI.FocusControl(null);
        }
    }

    private void CalculatePreviewNames()
    {
        previewNames.Clear();
        HashSet<string> usedPaths = new HashSet<string>();
        
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            Object obj = selectedObjects[i];
            if (obj == null) continue;

            string path = AssetDatabase.GetAssetPath(obj);
            // 如果启用移动且目标文件夹有效，则使用目标文件夹作为基础目录
            string dir = Path.GetDirectoryName(path);
            if (enableMove && !string.IsNullOrEmpty(targetFolder))
            {
                dir = targetFolder;
            }
            string ext = Path.GetExtension(path);
            
            // 计算基础名字
            string baseName = GetBaseNewName(obj, i);
            string finalName = baseName;

            if (autoResolveConflict)
            {
                int safeGuard = 0;
                while (safeGuard < 1000)
                {
                    string fullNewPath = Path.Combine(dir, finalName + ext).Replace("\\", "/");
                    
                    if (!usedPaths.Contains(fullNewPath))
                    {
                        usedPaths.Add(fullNewPath);
                        break;
                    }
                    
                    // 冲突，生成下一个名字
                    finalName = GetNextName(finalName);
                    safeGuard++;
                }
            }
            else
            {
                string fullNewPath = Path.Combine(dir, finalName + ext).Replace("\\", "/");
                usedPaths.Add(fullNewPath);
            }

            previewNames[path] = finalName;
        }
    }

    /// <summary>
    /// 获取下一个名字：
    /// 如果末尾有数字，则数字+1
    /// 否则追加 _1
    /// </summary>
    private string GetNextName(string currentName)
    {
        // 匹配末尾的数字
        // 格式：任何字符 + 数字 + 结束
        Match match = Regex.Match(currentName, @"^(.*?)(\d+)$");
        
        if (match.Success)
        {
            string prefixPart = match.Groups[1].Value;
            string numberPart = match.Groups[2].Value;
            
            // 尝试解析数字
            long number;
            if (long.TryParse(numberPart, out number))
            {
                number++;
                // 保持原来的位数（如果有前导零）
                // 如果原来的数字是 '01' (长度2)，现在的数字是 2，那么应该变成 '02'
                string newNumberStr = number.ToString().PadLeft(numberPart.Length, '0');
                return prefixPart + newNumberStr;
            }
        }
        
        // 如果没有匹配到末尾数字，或者解析失败，直接追加 _1
        return currentName + "_1";
    }

    private string GetBaseNewName(Object obj, int index)
    {
        string middle = "";
        switch (middleMode)
        {
            case MiddleMode.KeepOriginalName:
                middle = obj.name;
                break;
            case MiddleMode.CustomText:
                middle = middleContent;
                break;
            case MiddleMode.SequenceNumber:
                middle = (startNumber + index).ToString("D" + digitCount);
                break;
            case MiddleMode.Remove:
                middle = "";
                break;
        }

        return prefix + middle + suffix;
    }

    private void DrawPreviewList()
    {
        if (selectedObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("请在 Project 窗口中选中需要重命名的文件。", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("预览 (原名 -> 新名)", EditorStyles.miniLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 检测显示用的冲突集合（仅用于标红）
        HashSet<string> uniqueCheck = new HashSet<string>();
        HashSet<string> conflictNames = new HashSet<string>();

        // 预先收集一遍看有没有冲突（如果未开启自动解决）
        foreach (var kvp in previewNames)
        {
            string ext = Path.GetExtension(kvp.Key);
            
            string dir;
            if (enableMove && !string.IsNullOrEmpty(targetFolder))
                dir = targetFolder;
            else
                dir = Path.GetDirectoryName(kvp.Key);

            string fullPath = dir + "/" + kvp.Value + ext;
            
            if (uniqueCheck.Contains(fullPath))
            {
                conflictNames.Add(fullPath);
            }
            else
            {
                uniqueCheck.Add(fullPath);
            }
        }

        for (int i = 0; i < selectedObjects.Count; i++)
        {
            Object obj = selectedObjects[i];
            if (obj == null) continue;

            string path = AssetDatabase.GetAssetPath(obj);
            if (!previewNames.ContainsKey(path)) continue;

            string originalName = obj.name;
            string newName = previewNames[path];
            string extension = Path.GetExtension(path);
            
            // 构建显示路径
            string newDisplayPath = newName + extension;
            if (enableMove && !string.IsNullOrEmpty(targetFolder))
            {
                // 如果启用了移动，预览显示目标文件夹+文件名
                // 为了显示简洁，如果文件夹路径太长，可以简化显示，但这里直接显示
                // 或者我们可以只显示 "-> [Move] NewName"
                string folderName = Path.GetFileName(targetFolder); // 只取文件夹名
                newDisplayPath = string.Format("[{0}]/{1}", folderName, newName + extension);
            }

            // 完整路径用于冲突检测
            string dir;
            if (enableMove && !string.IsNullOrEmpty(targetFolder))
                dir = targetFolder;
            else
                dir = Path.GetDirectoryName(path);
            string fullNewPath = dir + "/" + newName + extension;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(originalName + extension, GUILayout.Width(position.width * 0.45f));
            EditorGUILayout.LabelField("->", GUILayout.Width(20));
            
            bool isConflict = conflictNames.Contains(fullNewPath);
            
            GUIStyle nameStyle = isConflict ? new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } } : EditorStyles.label;
            EditorGUILayout.LabelField(newDisplayPath, nameStyle);
            
            if (isConflict)
            {
                GUILayout.Label("(冲突)", EditorStyles.miniLabel, GUILayout.Width(40));
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawActionButtons()
    {
        EditorGUI.BeginDisabledGroup(selectedObjects.Count == 0);
        
        string btnText = enableMove ? "执行重命名并移动" : "执行重命名";
        
        if (GUILayout.Button(btnText, GUILayout.Height(30)))
        {
            RenameAssets();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void RenameAssets()
    {
        if (selectedObjects.Count == 0) return;
        
        // 简单校验移动目标
        if (enableMove)
        {
            if (string.IsNullOrEmpty(targetFolder))
            {
                EditorUtility.DisplayDialog("错误", "请选择目标文件夹！", "确定");
                return;
            }
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                EditorUtility.DisplayDialog("错误", "目标文件夹不存在！请检查路径。", "确定");
                return;
            }
        }

        // 重新计算一次确保最新
        CalculatePreviewNames();

        int count = 0;
        var validItems = selectedObjects.Where(o => o != null).Select(o => AssetDatabase.GetAssetPath(o)).ToList();
        
        for (int i = 0; i < validItems.Count; i++)
        {
            string path = validItems[i];
            if (!previewNames.ContainsKey(path)) continue;
            
            string newName = previewNames[path];
            string ext = Path.GetExtension(path);
            string oldName = Path.GetFileNameWithoutExtension(path);
            
            // 目标路径
            string targetDir = enableMove ? targetFolder : Path.GetDirectoryName(path);
            string targetPath = targetDir + "/" + newName + ext;

            // 如果没改名也没移动，就跳过
            // 注意 MoveAsset 即使路径一样也会执行，但这里我们优化一下
            // 需要标准化路径分隔符比较
            if (path.Replace("\\", "/") == targetPath.Replace("\\", "/")) continue;

            // 使用 MoveAsset，它既可以移动也可以重命名
            string error = AssetDatabase.MoveAsset(path, targetPath);
            if (string.IsNullOrEmpty(error))
            {
                count++;
            }
            else
            {
                Debug.LogError(string.Format("操作失败: {0} -> {1}. 错误: {2}", path, targetPath, error));
            }
        }

        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            string msg = enableMove ? "成功重命名并移动 {0} 个文件。" : "成功重命名 {0} 个文件。";
            Debug.Log(string.Format(msg, count));
            UpdateSelection(); 
        }
    }
}
