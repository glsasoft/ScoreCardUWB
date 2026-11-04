using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LetsPiu.Graphics;

[CustomEditor(typeof(MapMaterialGroupProfile))]
public class MapMaterialGroupProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapMaterialGroupProfile profile = (MapMaterialGroupProfile)target;

        GUILayout.Space(10);

        if (GUILayout.Button("CollectMat（自动收集三档材质）"))
        {
            CollectMaterials(profile);
        }
    }

    private void CollectMaterials(MapMaterialGroupProfile profile)
    {
        profile.lowQualityMaterials.Clear();
        profile.mediumQualityMaterials.Clear();
        profile.highQualityMaterials.Clear();

        CollectFromFolder(profile.materialLowFolder, profile.lowQualityMaterials);
        CollectFromFolder(profile.materialMediumFolder, profile.mediumQualityMaterials);
        CollectFromFolder(profile.materialHighFolder, profile.highQualityMaterials);

        EditorUtility.SetDirty(profile);
        Debug.Log("材质收集完成！");
    }

    private void CollectFromFolder(string folderPath, List<Material> targetList)
    {
        if (!folderPath.StartsWith("Assets"))
        {
            Debug.LogWarning($"无效路径：{folderPath}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                targetList.Add(mat);
            }
        }
    }
}