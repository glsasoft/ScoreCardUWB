using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class LargeTextureChecker : EditorWindow
{
    private int maxWidth = 2048;
    private int maxHeight = 2048;
    private int maxSizeMB = 5; // 超过 5MB 提示
    private string searchFolder = "Assets"; // 默认检测整个项目
    private float minActualMB = 1f; // 过滤小纹理
    private bool sortByActual = true; // 默认按实际占用排序
    private List<TextureInfo> results = new List<TextureInfo>();
    private Vector2 scrollPos;

    private class TextureInfo
    {
        public string path;
        public int width;
        public int height;
        public float estimatedMB; // 未压缩估算
        public float actualMB;    // 实际运行时占用
        public string compression; // 压缩格式
    }

    [MenuItem("Tools/检测工具/检测大纹理")]
    public static void ShowWindow()
    {
        GetWindow<LargeTextureChecker>("大纹理检测");
    }

    void OnGUI()
    {
        GUILayout.Label("大纹理检测工具", EditorStyles.boldLabel);

        maxWidth = EditorGUILayout.IntField("最大宽度(px)", maxWidth);
        maxHeight = EditorGUILayout.IntField("最大高度(px)", maxHeight);
        maxSizeMB = EditorGUILayout.IntField("最大内存(MB)", maxSizeMB);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("检测目录:");
        searchFolder = EditorGUILayout.TextField(searchFolder);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择检测目录", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                    searchFolder = "Assets" + selected.Substring(Application.dataPath.Length);
                else
                    EditorUtility.DisplayDialog("错误", "必须选择Assets目录下的文件夹", "确定");
            }
        }
        EditorGUILayout.EndHorizontal();

        minActualMB = EditorGUILayout.FloatField("过滤实际占用小于(MB)", minActualMB);

        sortByActual = EditorGUILayout.Toggle("按实际占用排序", sortByActual);

        if (GUILayout.Button("开始检测", GUILayout.Height(30)))
        {
            CheckLargeTextures();
        }

        GUILayout.Space(10);
        if (results.Count > 0)
        {
            GUILayout.Label($"检测到 {results.Count} 个大纹理：", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("选中", GUILayout.Width(40));
            GUILayout.Label("路径", GUILayout.Width(600));
            GUILayout.Label("尺寸", GUILayout.Width(90));
            GUILayout.Label("压缩格式", GUILayout.Width(100));
            GUILayout.Label("估算MB", GUILayout.Width(70));
            GUILayout.Label("实际MB", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var texInfo in results)
            {
                if (texInfo.actualMB < minActualMB) continue; // 过滤小纹理

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("选中", GUILayout.Width(40)))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Texture2D>(texInfo.path);
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }

                GUILayout.Label(texInfo.path, GUILayout.Width(600));
                GUILayout.Label($"{texInfo.width}x{texInfo.height}", GUILayout.Width(90));
                GUILayout.Label(texInfo.compression, GUILayout.Width(100));
                GUILayout.Label($"{texInfo.estimatedMB:F2}", GUILayout.Width(70));

                // 根据实际占用设置颜色
                Color defaultColor = GUI.color;
                if (texInfo.actualMB > 10f) GUI.color = Color.red;
                else if (texInfo.actualMB > 5f) GUI.color = Color.yellow;

                GUILayout.Label($"{texInfo.actualMB:F2}", GUILayout.Width(70));

                GUI.color = defaultColor;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("导出结果到文件", GUILayout.Height(25)))
            {
                string filePath = EditorUtility.SaveFilePanel("保存结果", "", "LargeTextureReport.txt", "txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    File.WriteAllLines(filePath, results.ConvertAll(r =>
                        $"{r.path} | {r.width}x{r.height} | 压缩:{r.compression} | 估算:{r.estimatedMB:F2}MB | 实际:{r.actualMB:F2}MB"));
                    EditorUtility.RevealInFinder(filePath);
                }
            }
        }
    }

    private void CheckLargeTextures()
    {
        results.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { searchFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                int width, height;
                importer.GetSourceTextureWidthAndHeight(out width, out height);

                // 估算未压缩内存占用
                int memorySize = width * height * 4;
                float memoryMB = memorySize / (1024f * 1024f);

                // 获取压缩格式
                string compression = importer.textureCompression.ToString();

                // 加载纹理计算实际运行时占用
                float actualMB = 0;
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    long runtimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
                    actualMB = runtimeSize / (1024f * 1024f);
                }

                if (width > maxWidth || height > maxHeight || memoryMB > maxSizeMB)
                {
                    results.Add(new TextureInfo
                    {
                        path = path,
                        width = width,
                        height = height,
                        estimatedMB = memoryMB,
                        actualMB = actualMB,
                        compression = compression
                    });
                }
            }
        }

        // 排序
        if (sortByActual)
            results.Sort((a, b) => b.actualMB.CompareTo(a.actualMB));
        else
            results.Sort((a, b) => b.estimatedMB.CompareTo(a.estimatedMB));

        Debug.Log($"检测完成，共发现 {results.Count} 个大纹理");
    }
}
