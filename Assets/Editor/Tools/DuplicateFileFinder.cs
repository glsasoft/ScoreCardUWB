using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DuplicateFileFinder : EditorWindow
{
    private string targetFolderPath = "Assets";
    private Vector2 scrollPosition;
    private Dictionary<string, List<string>> duplicateFiles = new Dictionary<string, List<string>>();
    private bool hasScanned = false;
    private Dictionary<string, bool> fileSelection = new Dictionary<string, bool>();
    private const string PROTECTED_PATH = "Assets/Art/UIAtlas/PublicButtons";

    [MenuItem("Tools/美术资源/Duplicate File Finder", false, 4)]
    public static void ShowWindow()
    {
        GetWindow<DuplicateFileFinder>("Duplicate File Finder");
    }

    void OnGUI()
    {
        GUILayout.Label("Duplicate File Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 显示保护路径
        EditorGUILayout.LabelField($"Protected Path: {PROTECTED_PATH}", EditorStyles.helpBox);
        EditorGUILayout.Space();

        // 选择文件夹
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Folder Path:", GUILayout.Width(80));
        targetFolderPath = EditorGUILayout.TextField(targetFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", targetFolderPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                // 将绝对路径转换为相对于项目的路径
                if (path.StartsWith(Application.dataPath))
                {
                    targetFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a folder inside the project.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 扫描按钮
        if (GUILayout.Button("扫描"))
        {
            ScanForDuplicates();
        }

        EditorGUILayout.Space();

        // 显示结果
        if (hasScanned)
        {
            if (duplicateFiles.Count > 0)
            {
                GUILayout.Label($"Found {duplicateFiles.Count} duplicate file names:", EditorStyles.boldLabel);

                // 自动选择按钮
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Auto Select (Keep Protected)", GUILayout.Width(180)))
                {
                    AutoSelectFiles();
                }
                if (GUILayout.Button("Select All", GUILayout.Width(80)))
                {
                    SelectAllFiles(true);
                }
                if (GUILayout.Button("Deselect All", GUILayout.Width(80)))
                {
                    SelectAllFiles(false);
                }
                EditorGUILayout.EndHorizontal();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var pair in duplicateFiles)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(pair.Key, EditorStyles.boldLabel);

                    foreach (string path in pair.Value)
                    {
                        EditorGUILayout.BeginHorizontal();

                        // 检查是否是保护路径
                        bool isProtected = path.StartsWith(PROTECTED_PATH);

                        if (isProtected)
                        {
                            // 保护路径下的文件，显示保护标记
                            EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                            EditorGUILayout.LabelField(path, EditorStyles.boldLabel);
                            EditorGUILayout.LabelField("(Protected)", GUILayout.Width(80));
                        }
                        else
                        {
                            // 添加选择复选框
                            if (!fileSelection.ContainsKey(path))
                            {
                                fileSelection[path] = false;
                            }
                            fileSelection[path] = EditorGUILayout.Toggle(fileSelection[path], GUILayout.Width(20));

                            EditorGUILayout.LabelField(path, GUILayout.Width(1000));

                            if (GUILayout.Button("Select", GUILayout.Width(60)))
                            {
                                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.EndScrollView();

                // 删除选中文件按钮
                EditorGUILayout.Space();
                if (GUILayout.Button("Delete Selected Files", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Delete",
                        "Are you sure you want to delete the selected files? This action cannot be undone.",
                        "Delete", "Cancel"))
                    {
                        DeleteSelectedFiles();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No duplicate file names found.", MessageType.Info);
            }
        }
    }

    void ScanForDuplicates()
    {
        if (!AssetDatabase.IsValidFolder(targetFolderPath))
        {
            EditorUtility.DisplayDialog("Error", "Invalid folder path. Please select a valid folder in the project.", "OK");
            return;
        }

        duplicateFiles.Clear();
        fileSelection.Clear();

        // 获取所有文件
        string[] allFiles = Directory.GetFiles(targetFolderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".meta"))
            .ToArray();

        // 按文件名分组
        Dictionary<string, List<string>> fileGroups = new Dictionary<string, List<string>>();

        foreach (string filePath in allFiles)
        {
            string relativePath = filePath.Replace("\\", "/");
            if (relativePath.StartsWith(Application.dataPath))
            {
                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            }

            string fileName = Path.GetFileName(relativePath);

            if (!fileGroups.ContainsKey(fileName))
            {
                fileGroups[fileName] = new List<string>();
            }

            fileGroups[fileName].Add(relativePath);
        }

        // 找出重复的文件名
        foreach (var pair in fileGroups)
        {
            if (pair.Value.Count > 1)
            {
                duplicateFiles.Add(pair.Key, pair.Value);
            }
        }

        hasScanned = true;

        // 自动选择非保护路径的文件
        AutoSelectFiles();

        // 刷新界面
        Repaint();
    }

    void AutoSelectFiles()
    {
        foreach (var pair in duplicateFiles)
        {
            foreach (string path in pair.Value)
            {
                // 自动选择非保护路径的文件
                fileSelection[path] = !path.StartsWith(PROTECTED_PATH);
            }
        }
        Repaint();
    }

    void SelectAllFiles(bool select)
    {
        foreach (var pair in duplicateFiles)
        {
            foreach (string path in pair.Value)
            {
                // 跳过保护路径的文件
                if (!path.StartsWith(PROTECTED_PATH))
                {
                    fileSelection[path] = select;
                }
            }
        }
        Repaint();
    }

    void DeleteSelectedFiles()
    {
        List<string> filesToDelete = new List<string>();

        // 收集要删除的文件
        foreach (var pair in fileSelection)
        {
            if (pair.Value && !pair.Key.StartsWith(PROTECTED_PATH))
            {
                filesToDelete.Add(pair.Key);
            }
        }

        if (filesToDelete.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "No files selected for deletion.", "OK");
            return;
        }

        // 删除文件
        foreach (string filePath in filesToDelete)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);

                // 同时删除对应的.meta文件
                string metaFile = filePath + ".meta";
                if (File.Exists(metaFile))
                {
                    File.Delete(metaFile);
                }
            }
        }

        // 刷新AssetDatabase
        AssetDatabase.Refresh();

        // 重新扫描以更新显示
        ScanForDuplicates();

        EditorUtility.DisplayDialog("Success", $"Deleted {filesToDelete.Count} files.", "OK");
    }
}