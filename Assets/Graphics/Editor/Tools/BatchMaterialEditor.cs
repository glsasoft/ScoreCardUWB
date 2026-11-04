using UnityEditor;
using UnityEngine;

public class BatchMaterialEditor : EditorWindow
{
    private string floatPropertyName = "";
    private float floatValue;
    private string texturePropertyName = "";
    private Texture textureValue;

    [MenuItem("Tools/批量材质属性修改")]
    public static void ShowWindow()
    {
        GetWindow<BatchMaterialEditor>("材质批量修改");
    }

    void OnGUI()
    {
        GUILayout.Label("当前选中的物体", EditorStyles.boldLabel);
        ShowSelectedObjects();

        EditorGUILayout.Space(15);
        DrawFloatSettings();

        EditorGUILayout.Space(15);
        DrawTextureSettings();
    }

    void ShowSelectedObjects()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorGUILayout.HelpBox("请选择场景中的物体", MessageType.Info);
            return;
        }

        foreach (GameObject go in Selection.gameObjects)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawFloatSettings()
    {
        GUILayout.Label("Float 属性设置", EditorStyles.boldLabel);
        floatPropertyName = EditorGUILayout.TextField("属性名称", floatPropertyName);
        floatValue = EditorGUILayout.FloatField("属性值", floatValue);

        if (GUILayout.Button("应用Float修改"))
        {
            ApplyFloatToMaterials();
        }
    }

    void DrawTextureSettings()
    {
        GUILayout.Label("Texture 属性设置", EditorStyles.boldLabel);
        texturePropertyName = EditorGUILayout.TextField("属性名称", texturePropertyName);
        textureValue = (Texture)EditorGUILayout.ObjectField("纹理贴图", textureValue, typeof(Texture), false);

        if (GUILayout.Button("应用Texture修改"))
        {
            ApplyTextureToMaterials();
        }
    }

    void ApplyFloatToMaterials()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("请先选择场景中的物体");
            return;
        }

        foreach (GameObject root in Selection.gameObjects)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null) continue;

                    if (material.HasProperty(floatPropertyName))
                    {
                        Undo.RecordObject(material, "Modify Material Float");
                        material.SetFloat(floatPropertyName, floatValue);
                        EditorUtility.SetDirty(material);
                        Debug.Log($"已修改 {material.name} 的 {floatPropertyName} 属性");
                    }
                    else
                    {
                        Debug.LogWarning($"材质 {material.name} 不存在属性: {floatPropertyName}");
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Float属性修改完成!");
    }

    void ApplyTextureToMaterials()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("请先选择场景中的物体");
            return;
        }

        if (textureValue == null)
        {
            Debug.LogWarning("请先指定纹理资源");
            return;
        }

        foreach (GameObject root in Selection.gameObjects)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null) continue;

                    if (material.HasProperty(texturePropertyName))
                    {
                        Undo.RecordObject(material, "Modify Material Texture");
                        material.SetTexture(texturePropertyName, textureValue);
                        EditorUtility.SetDirty(material);
                        Debug.Log($"已修改 {material.name} 的 {texturePropertyName} 属性");
                    }
                    else
                    {
                        Debug.LogWarning($"材质 {material.name} 不存在属性: {texturePropertyName}");
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Texture属性修改完成!");
    }
}