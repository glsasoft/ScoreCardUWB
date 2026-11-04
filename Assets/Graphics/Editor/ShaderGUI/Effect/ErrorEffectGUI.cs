using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;


public class ErrorEffectGUI : ShaderGUI
{
    private Material targetMat;
    
    private MaterialProperty _ErrorTex;
    private MaterialProperty _ErrorColor;
    private MaterialProperty _ErrorColorIntensity;
    private MaterialProperty _ErrorTexRedIsAlphaOn;
    private MaterialProperty _ErrorTexRotation;
    private MaterialProperty _ErrorTexFlowAndBevel;

    private MaterialProperty _ErrorRange;
    private MaterialProperty _ErrorDirectionRed;
    private MaterialProperty _ErrorDirectionBlue;
    private MaterialProperty _ErrorDistortGrid;
    private MaterialProperty _ErrorDistortStrength;
    private MaterialProperty _ErrorDistortSpeed;

    private MaterialProperty _ErrorStrobeInterval;
    private MaterialProperty _ErrorStrobeDuration;
    
    private bool _Error = true;

    void FinProperties(MaterialProperty[] properties)
    {
        _ErrorTex = FindProperty("_ErrorTex", properties, true);
        _ErrorColor = FindProperty("_ErrorColor", properties, true);
        _ErrorColorIntensity = FindProperty("_ErrorColorIntensity", properties, true);
        _ErrorTexRedIsAlphaOn = FindProperty("_ErrorTexRedIsAlphaOn", properties, true);
        _ErrorTexRotation = FindProperty("_ErrorTexRotation", properties, true);
        _ErrorTexFlowAndBevel = FindProperty("_ErrorTexFlowAndBevel", properties, true);
        
        _ErrorRange = FindProperty("_ErrorRange", properties, true);
        _ErrorDirectionRed = FindProperty("_ErrorDirectionRed", properties, true);
        _ErrorDirectionBlue = FindProperty("_ErrorDirectionBlue", properties, true);
        _ErrorDistortGrid = FindProperty("_ErrorDistortGrid", properties, true);
        _ErrorDistortStrength = FindProperty("_ErrorDistortStrength", properties, true);
        _ErrorDistortSpeed = FindProperty("_ErrorDistortSpeed", properties, true);
        
        _ErrorStrobeInterval = FindProperty("_ErrorStrobeInterval", properties, true);
        _ErrorStrobeDuration = FindProperty("_ErrorStrobeDuration", properties, true);
    }
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        targetMat = materialEditor.target as Material;
        targetMat.renderQueue = (int)RenderQueue.Transparent;
        FinProperties(properties);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Error = ShaderGUI_Helper.Foldout(_Error, "故障贴图");
        if (_Error)
        {
            EditorGUI.indentLevel++;
            
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_ErrorTex), _ErrorTex);
            
            Vector2 scale = targetMat.GetTextureScale("_ErrorTex");
            Vector2 offset = targetMat.GetTextureOffset("_ErrorTex");
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Vector2Field("Tiling", scale);
            offset = EditorGUILayout.Vector2Field("Offset", offset);
            if (EditorGUI.EndChangeCheck())
                _ErrorTex.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
            
            bool errorTexRedIsAlphaOnOri = Equals(_ErrorTexRedIsAlphaOn.floatValue, 1.0f);
            bool errorTexRedIsAlphaOnRes = EditorGUILayout.Toggle("R是透明通道", errorTexRedIsAlphaOnOri);
            if (errorTexRedIsAlphaOnRes) _ErrorTexRedIsAlphaOn.floatValue = 1.0f;
            else _ErrorTexRedIsAlphaOn.floatValue = 0.0f;
                
            materialEditor.ColorProperty(_ErrorColor, "颜色");
            materialEditor.FloatProperty(_ErrorColorIntensity, "颜色强度");
            materialEditor.RangeProperty(_ErrorTexRotation, "故障贴图旋转");
            materialEditor.VectorProperty(_ErrorTexFlowAndBevel, "故障贴图 流动 & 斜切");

            materialEditor.RangeProperty(_ErrorRange, "偏移范围");
            
            materialEditor.RangeProperty(_ErrorDirectionRed, "红色偏移方向");
            materialEditor.RangeProperty(_ErrorDirectionBlue, "蓝色偏移方向");

            int errorDistortGridOri = Mathf.RoundToInt(_ErrorDistortGrid.floatValue);
            int errorDistortGridRes = EditorGUILayout.IntSlider("扰动格子", errorDistortGridOri,
                (int)_ErrorDistortGrid.rangeLimits.x, (int)_ErrorDistortGrid.rangeLimits.y);
            _ErrorDistortGrid.floatValue = errorDistortGridRes;
            
            materialEditor.RangeProperty(_ErrorDistortStrength, "扰动强度");
            materialEditor.RangeProperty(_ErrorDistortSpeed, "扰动速度");

            materialEditor.RangeProperty(_ErrorStrobeInterval, "频闪间隔时间");
            materialEditor.RangeProperty(_ErrorStrobeDuration, "频闪持续时间");
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
}