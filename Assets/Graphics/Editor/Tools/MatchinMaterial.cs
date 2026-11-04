using System.IO;
using UnityEngine;
using UnityEditor;

public class MatchinMaterial : EditorWindow
{
    public string modelPath = "Assets";
    private static string materialPath = "null";
    public Rect modelRect;
    public string AlbedoMap = "_BaseMap";
    public string MaskMap = "_MaskMap";
    public string NormalMap = "_NormalMap";
    public string EmissionMap = "_EmissionMap";
    public string RoughnessMap = "_RoughnessMap";
    public string MetallicMap = "_MetallicMap";
    public string AoMap = "_AoMap";

    private Shader useShader;
    private static MatchinMaterial _window;
    
    [SerializeField]
    public string m_AlbedoMap = "_Albedo", 
    m_MaskMap = "_Mask", 
    m_NormalMap = "_Normal", 
    m_EmissionMap = "_Emission", 
    m_RoughnessMap = "_Roughness", 
    m_MetallicMap = "_Metallic",
    m_AoMap = "_AO";
    
    public string m_Shader = "ShaderTemplate/PBR_Character_V3";
    public string sizeSuffix = "_4096";
    public string texturePath = "Assets";
    public Rect textureRect;

    [MenuItem("Tools/材质匹配")]
    public static void showWindow()
    {
        Rect wr = new Rect(0, 0, 600, 400);     // 增加窗口高度
        _window = (MatchinMaterial)GetWindowWithRect(typeof(MatchinMaterial), wr, true, "材质匹配");
        _window.Show();
    }

    public void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("模型路径 (鼠标拖拽文件夹到这里)");
        modelRect = EditorGUILayout.GetControlRect();
        GUI.SetNextControlName("input1");
        modelPath = EditorGUI.TextField(modelRect, modelPath);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("贴图路径 (鼠标拖拽文件夹到这里)");
        textureRect = EditorGUILayout.GetControlRect();
        GUI.SetNextControlName("input2");
        texturePath = EditorGUI.TextField(textureRect, texturePath);

        EditorGUILayout.Space();
        m_AlbedoMap = EditorGUILayout.TextField("Albedo贴图后缀", m_AlbedoMap);
        m_MaskMap = EditorGUILayout.TextField("Mask贴图后缀", m_MaskMap);
        m_MetallicMap = EditorGUILayout.TextField("Metallic贴图后缀", m_MetallicMap);
        m_RoughnessMap = EditorGUILayout.TextField("Roughness贴图后缀", m_RoughnessMap);
        m_NormalMap = EditorGUILayout.TextField("Normal贴图后缀", m_NormalMap);
        m_EmissionMap = EditorGUILayout.TextField("Emission贴图后缀", m_EmissionMap);
        sizeSuffix = EditorGUILayout.TextField("贴图尺寸后缀", sizeSuffix);  // 新增尺寸后缀输入
        m_Shader = EditorGUILayout.TextField("材质shader", m_Shader);
        useShader = Shader.Find(m_Shader);
        
        DragFolder();

        EditorGUILayout.Space();
        if (GUILayout.Button("导出材质球")) ForEachModels();
        if (GUILayout.Button("设置材质")) ForEachMaterials();
    }

    void DragFolder()
    {
        if (mouseOverWindow == this)
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (modelRect.Contains(Event.current.mousePosition))
                    GUI.FocusControl("input1");
                else if (textureRect.Contains(Event.current.mousePosition))
                    GUI.FocusControl("input2");
            }
            else if (Event.current.type == EventType.DragExited)
            {
                string dragPath = string.Join("", DragAndDrop.paths);
                if (modelRect.Contains(Event.current.mousePosition))
                {
                    modelPath = dragPath;
                    materialPath = Path.GetDirectoryName(modelPath) + "/Material";
                }
                else if (textureRect.Contains(Event.current.mousePosition))
                    texturePath = dragPath;
                GUI.FocusControl(null);
            }
        }
    }

    void ForEachModels()
    {
        string[] allPath = AssetDatabase.FindAssets("t:GameObject", new[] { modelPath });
        if (allPath.Length == 0 && Path.GetExtension(modelPath) == ".fbx")
        {
            ExtractMaterialsFromFBX(modelPath);
        }
        else
        {
            foreach (var path in allPath)
            {
                ExtractMaterialsFromFBX(AssetDatabase.GUIDToAssetPath(path));
            }
        }
    }

    public static void ExtractMaterialsFromFBX(string assetPath)
    {
        if (!AssetDatabase.IsValidFolder(materialPath))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(assetPath), "Material");

        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (fbx == null)
        {
            Debug.LogWarning($"无法加载 FBX：{assetPath}");
            return;
        }

        // 查找所有带有 Renderer 的子对象
        Renderer[] renderers = fbx.GetComponentsInChildren<Renderer>(true);

        foreach (var renderer in renderers)
        {
            string matName = renderer.name;
            string path = Path.Combine(materialPath, $"{matName}.mat");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            // 创建材质并使用默认 shader
            Material mat = new Material(Shader.Find("Standard")); // 或替换为默认 shader
            AssetDatabase.CreateAsset(mat, path);
            Debug.Log($"创建材质：{matName}.mat");

            // 将材质赋值到 FBX 实例上的 renderer（可选，需实例化 prefab）
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    void ForEachMaterials()
    {
        foreach (var guid in GetObjPath("Material", materialPath))
        {
            setMaterialShader(AssetDatabase.GUIDToAssetPath(guid));
        }
        Debug.Log($"材质设置完成，共处理{GetObjPath("Material", materialPath).Length}个材质");
    }

    void setMaterialShader(string path)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        mat.shader = useShader;

        foreach (var texGuid in GetObjPath("Texture2D", texturePath))
        {
            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(
                AssetDatabase.GUIDToAssetPath(texGuid));

            string fullSuffix = sizeSuffix;  // 组合尺寸后缀
            string baseName = $"T_{mat.name}";

            CheckAndAssign(t, baseName + m_AlbedoMap + fullSuffix, AlbedoMap, mat);
            CheckAndAssign(t, baseName + m_MaskMap + fullSuffix, MaskMap, mat);
            CheckAndAssign(t, baseName + m_NormalMap + fullSuffix, NormalMap, mat);
            CheckAndAssign(t, baseName + m_EmissionMap + fullSuffix, EmissionMap, mat);
            CheckAndAssign(t, baseName + m_MetallicMap + fullSuffix, MetallicMap, mat);
            CheckAndAssign(t, baseName + m_RoughnessMap + fullSuffix, RoughnessMap, mat);
            CheckAndAssign(t, baseName + m_AoMap + fullSuffix, AoMap, mat);
        }
    }

    void CheckAndAssign(Texture2D tex, string matchName, string property, Material mat)
    {
        if (tex.name == matchName)
        {
            mat.SetTexture(property, tex);
            Debug.Log($"已匹配 {property}: {tex.name}");
        }
    }

    string[] GetObjPath(string mType, string searchPath)
    {
        return AssetDatabase.FindAssets($"t:{mType}", new[] { searchPath });
    }
}