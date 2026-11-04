using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct PassLightMode
{
    public string lightMode { get; set; }
    public bool status { get; set; }
}

public class ShaderPassControl : EditorWindow
{
    [MenuItem("Tools/Shader Pass Control")]
    private static void OpenWindows()
    {
        GetWindow<ShaderPassControl>(false, "Shader Pass Control", true).Show();
    }

    private static PassLightMode _passLightMode = new PassLightMode { lightMode = "DrawStencilOutline", status = false };
    private List<Material> _materials = new();

    private void OnEnable()
    {
        // 注册Selection变化时的回调
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        // 注销Selection变化时的回调
        Selection.selectionChanged -= OnSelectionChanged;
    }

    // 当选择发生变化时，更新材质列表
    private void OnSelectionChanged()
    {
        _materials.Clear();
        if (Selection.activeObject is null) return;

        foreach (var obj in Selection.objects)
        {
            if (obj is Material mat)
            {
                if (!_materials.Contains(mat)) _materials.Add(mat);
            }
            else if (obj is GameObject gobj)
            {
                AddMaterialsFromGameObject(gobj);
            }
        }

        Repaint(); // 更新UI
    }

    // 从GameObject中提取材质
    private void AddMaterialsFromGameObject(GameObject gobj)
    {
        MeshRenderer meshRenderer = gobj.GetComponent<MeshRenderer>();
        SkinnedMeshRenderer skinnedMeshRenderer = gobj.GetComponent<SkinnedMeshRenderer>();

        if (meshRenderer != null)
        {
            foreach (var mat in meshRenderer.sharedMaterials)
            {
                if (!_materials.Contains(mat)) _materials.Add(mat);
            }
        }
        else if (skinnedMeshRenderer != null)
        {
            foreach (var mat in skinnedMeshRenderer.sharedMaterials)
            {
                if (!_materials.Contains(mat)) _materials.Add(mat);
            }
        }
    }

    private void OnGUI()
    {
        if (_materials.Count == 0)
        {
            EditorGUILayout.LabelField("请选择目标材质");
            return;
        }

        // 显示选择的材质列表
        EditorGUILayout.BeginVertical();
        foreach (var mat in _materials)
        {
            EditorGUILayout.LabelField("已经选择的材质：" + mat.name);
        }

        // 设置LightMode
        EditorGUILayout.BeginHorizontal();
        _passLightMode.lightMode = EditorGUILayout.TextField("LightMode", _passLightMode.lightMode);
        _passLightMode.status = EditorGUILayout.Toggle("状态", _passLightMode.status);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("设置ShaderPass LightMode状态"))
        {
            foreach (var mat in _materials)
            {
                mat.SetShaderPassEnabled(_passLightMode.lightMode, _passLightMode.status);
            }
            Debug.Log("已成功设置 " + _passLightMode.lightMode + " 状态为 " + _passLightMode.status);
        }
        
        EditorGUILayout.EndVertical();
    }
}
