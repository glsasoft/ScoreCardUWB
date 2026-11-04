using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;

public class ErrorEffectGUI1 : ShaderGUI
{
    private Material targetMat;

    // Base
    private MaterialProperty _ErrorTex;
    private MaterialProperty _ErrorColor;
    private MaterialProperty _ErrorColorIntensity;
    private MaterialProperty _ErrorTexRedIsAlphaOn;
    private MaterialProperty _ErrorTexRotation;
    private MaterialProperty _ErrorTexFlowAndBevel;

    // Core
    private MaterialProperty _Freq;
    private MaterialProperty _Fade;

    // Offset
    private MaterialProperty _ErrorOffsetMode;
    private MaterialProperty _ErrorOffsetInterval;
    private MaterialProperty _ErrorOffsetDuration;
    private MaterialProperty _ErrorOffsetSpeed;
    private MaterialProperty _ErrorOffsetMax;
    private MaterialProperty _RGBSplit_Indensity;

    // Block
    private MaterialProperty _BlockLayer;
    private MaterialProperty _BlockLayer1_Indensity;
    private MaterialProperty _BlockLayer2_Indensity;
    private MaterialProperty _Offset;

    // Foldouts
    private bool _BaseFold = true;
    private bool _CoreFold = true;
    private bool _OffsetFold = true;
    private bool _BlockFold = true;

    void FindProperties(MaterialProperty[] properties)
    {
        _ErrorTex = FindProperty("_ErrorTex", properties);
        _ErrorColor = FindProperty("_ErrorColor", properties);
        _ErrorColorIntensity = FindProperty("_ErrorColorIntensity", properties);
        _ErrorTexRedIsAlphaOn = FindProperty("_ErrorTexRedIsAlphaOn", properties);
        _ErrorTexRotation = FindProperty("_ErrorTexRotation", properties);
        _ErrorTexFlowAndBevel = FindProperty("_ErrorTexFlowAndBevel", properties);

        _Freq = FindProperty("_Freq", properties);
        _Fade = FindProperty("_Fade", properties);

        _ErrorOffsetMode = FindProperty("_ErrorOffsetMode", properties);
        _ErrorOffsetInterval = FindProperty("_ErrorOffsetInterval", properties);
        _ErrorOffsetDuration = FindProperty("_ErrorOffsetDuration", properties);
        _ErrorOffsetSpeed = FindProperty("_ErrorOffsetSpeed", properties);
        _ErrorOffsetMax = FindProperty("_ErrorOffsetMax", properties);
        _RGBSplit_Indensity = FindProperty("_RGBSplit_Indensity", properties);

        _BlockLayer = FindProperty("_BlockLayer", properties);
        _BlockLayer1_Indensity = FindProperty("_BlockLayer1_Indensity", properties);
        _BlockLayer2_Indensity = FindProperty("_BlockLayer2_Indensity", properties);
        _Offset = FindProperty("_Offset", properties);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        targetMat = materialEditor.target as Material;
        targetMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        FindProperties(properties);

        // Base
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _BaseFold = EditorGUILayout.Foldout(_BaseFold, "基础参数 Base", true);
        if (_BaseFold)
        {
            EditorGUI.indentLevel++;
            materialEditor.TexturePropertySingleLine(new GUIContent("故障贴图"), _ErrorTex);

            Vector2 scale = targetMat.GetTextureScale("_ErrorTex");
            Vector2 offset = targetMat.GetTextureOffset("_ErrorTex");
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Vector2Field("Tiling", scale);
            offset = EditorGUILayout.Vector2Field("Offset", offset);
            if (EditorGUI.EndChangeCheck())
                _ErrorTex.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);

            bool redAsAlpha = _ErrorTexRedIsAlphaOn.floatValue == 1.0f;
            redAsAlpha = EditorGUILayout.Toggle("R是透明通道", redAsAlpha);
            _ErrorTexRedIsAlphaOn.floatValue = redAsAlpha ? 1.0f : 0.0f;

            materialEditor.ColorProperty(_ErrorColor, "颜色");
            materialEditor.FloatProperty(_ErrorColorIntensity, "颜色强度");
            materialEditor.RangeProperty(_ErrorTexRotation, "故障贴图旋转");
            materialEditor.VectorProperty(_ErrorTexFlowAndBevel, "故障贴图 流动 & 斜切");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        // Core
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _CoreFold = EditorGUILayout.Foldout(_CoreFold, "核心参数 Core", true);
        if (_CoreFold)
        {
            EditorGUI.indentLevel++;
            materialEditor.RangeProperty(_Freq, "噪声频率");
            materialEditor.FloatProperty(_Fade, "褪色程度");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        // Offset
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _OffsetFold = EditorGUILayout.Foldout(_OffsetFold, "偏移设置 Offset", true);
        if (_OffsetFold)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(_ErrorOffsetMode, "偏移类型 (sin，三角，锯齿)");
            materialEditor.RangeProperty(_ErrorOffsetInterval, "偏移间隔时间");
            materialEditor.RangeProperty(_ErrorOffsetDuration, "偏移持续时间");
            materialEditor.RangeProperty(_ErrorOffsetSpeed, "偏移速度");
            materialEditor.RangeProperty(_ErrorOffsetMax, "偏移最大值");
            materialEditor.RangeProperty(_RGBSplit_Indensity, "偏移强度");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        // Block
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _BlockFold = EditorGUILayout.Foldout(_BlockFold, "故障块设置 Error Block", true);
        if (_BlockFold)
        {
            EditorGUI.indentLevel++;
            materialEditor.VectorProperty(_BlockLayer, "故障块数量(xy内层，zw表层)");
            materialEditor.RangeProperty(_BlockLayer1_Indensity, "内层故障块强度");
            materialEditor.RangeProperty(_BlockLayer2_Indensity, "表层故障块强度");
            materialEditor.RangeProperty(_Offset, "表层故障块贡献");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
}
