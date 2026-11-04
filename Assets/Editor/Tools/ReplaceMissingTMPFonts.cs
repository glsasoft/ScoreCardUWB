/*
[使用方法]
把脚本放到 Editor 文件夹（例如 Assets/Editor/ReplaceMissingTMPFonts.cs）。
在菜单栏点击：Tools → TMP → Replace Missing TMP Fonts
选择一个 默认 TMP_FontAsset。
勾选是否扫描 场景 和 预制体。
点击 Scan and Replace → 自动修复所有 Missing 字体。
 */
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class ReplaceMissingTMPFonts : EditorWindow
{
    private TMP_FontAsset defaultFont;
    private bool searchInPrefabs = true;
    private bool searchInScenes = true;

    [MenuItem("Tools/字体相关/(TMP)Replace Missing TMP Fonts &2", false, 1)]
    public static void ShowWindow()
    {
        GetWindow<ReplaceMissingTMPFonts>("Replace Missing TMP Fonts");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Missing TMP Fonts", EditorStyles.boldLabel);

        defaultFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Default TMP Font:", defaultFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);
        GUILayout.Label("Search Options", EditorStyles.boldLabel);
        searchInScenes = EditorGUILayout.Toggle("Search in Open Scenes", searchInScenes);
        searchInPrefabs = EditorGUILayout.Toggle("Search in Project Prefabs", searchInPrefabs);

        GUILayout.Space(20);
        if (GUILayout.Button("Scan and Replace"))
        {
            if (defaultFont == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Default TMP Font first.", "OK");
                return;
            }

            int fixedCount = 0;

            if (searchInScenes)
                fixedCount += FixFontsInScenes();

            if (searchInPrefabs)
                fixedCount += FixFontsInPrefabs();

            EditorUtility.DisplayDialog("Process Complete", $"Replaced missing fonts in {fixedCount} TMP Text components.", "OK");
        }
    }

    private int FixFontsInScenes()
    {
        int count = 0;
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
                foreach (var text in texts)
                {
                    if (text.font == null)
                    {
                        Undo.RecordObject(text, "Replace Missing TMP Font");
                        text.font = defaultFont;
                        count++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }
        return count;
    }

    private int FixFontsInPrefabs()
    {
        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool changed = false;
            TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in texts)
            {
                if (text.font == null)
                {
                    text.font = defaultFont;
                    changed = true;
                    count++;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
        return count;
    }
}
