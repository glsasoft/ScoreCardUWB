using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;

public class ReflectEffectGUI : ShaderGUI
{
    private Material targetMat;

    private MaterialProperty _ZTest;
    
    private MaterialProperty _ReflectTex;
    private MaterialProperty _NormalScale;
    private MaterialProperty _DistortTimeFactor_U;
    private MaterialProperty _DistortTimeFactor_V;
    private MaterialProperty _ReflectTexCustomDataOn;
    private MaterialProperty _DistortStrength;
    
    private MaterialProperty _MaskTexOn_1;
    private MaterialProperty _MaskTex_1;
    private MaterialProperty _MaskTexRedIsAlphaOn_1;
    private MaterialProperty _MaskTexRotation_1;
    private MaterialProperty _MaskTexFlowAndBevel_1;
    private MaterialProperty _MaskTexCustomDataOn_1;
    
    private MaterialProperty _MaskTexOn_2;
    private MaterialProperty _MaskTex_2;
    private MaterialProperty _MaskTexRotation_2;
    private MaterialProperty _MaskTexFlowAndBevel_2;
    private MaterialProperty _MaskTexRedIsAlphaOn_2;
    private MaterialProperty _MaskTexCustomDataOn_2;
    
    private bool _Reflect = true;
    private bool _Mask = true;

    void FinProperties(MaterialProperty[] properties)
    {
        _ZTest = FindProperty("_ZTest", properties, true);
        
        _ReflectTex = FindProperty("_ReflectTex", properties, true);
        _NormalScale = FindProperty("_NormalScale", properties, true);
        _DistortTimeFactor_U = FindProperty("_DistortTimeFactor_U", properties, true);
        _DistortTimeFactor_V = FindProperty("_DistortTimeFactor_V", properties, true);
        _ReflectTexCustomDataOn = FindProperty("_ReflectTexCustomDataOn", properties, true);
        _DistortStrength = FindProperty("_DistortStrength", properties, true);
        
        _MaskTexOn_1 = FindProperty("_MaskTexOn_1", properties, true);
        _MaskTex_1 = FindProperty("_MaskTex_1", properties, true);
        _MaskTexRotation_1 = FindProperty("_MaskTexRotation_1", properties, true);
        _MaskTexFlowAndBevel_1 = FindProperty("_MaskTexFlowAndBevel_1", properties, true);
        _MaskTexRedIsAlphaOn_1 = FindProperty("_MaskTexRedIsAlphaOn_1", properties, true);
        _MaskTexCustomDataOn_1 = FindProperty("_MaskTexCustomDataOn_1", properties, true);
        
        _MaskTexOn_2 = FindProperty("_MaskTexOn_2", properties, true);
        _MaskTex_2 = FindProperty("_MaskTex_2", properties, true);
        _MaskTexRotation_2 = FindProperty("_MaskTexRotation_2", properties, true);
        _MaskTexFlowAndBevel_2 = FindProperty("_MaskTexFlowAndBevel_2", properties, true);
        _MaskTexRedIsAlphaOn_2 = FindProperty("_MaskTexRedIsAlphaOn_2", properties, true);
        _MaskTexCustomDataOn_2 = FindProperty("_MaskTexCustomDataOn_2", properties, true);
    }
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        targetMat = materialEditor.target as Material;
        targetMat.renderQueue = (int)RenderQueue.Transparent;
        FinProperties(properties);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Reflect = ShaderGUI_Helper.Foldout(_Reflect, "空间扰动");
        if (_Reflect)
        {
            EditorGUI.indentLevel++;
            
            bool zTestOri = Equals(_ZTest.floatValue, 2.0f);
            bool zTestRes = EditorGUILayout.Toggle("深度测试", zTestOri);
            _ZTest.floatValue = zTestRes ? 2.0f : 0.0f;
            
            bool reflectCustomDataOnOri = Equals(_ReflectTexCustomDataOn.floatValue, 1.0f);
            bool reflectCustomDataOnRes = EditorGUILayout.Toggle("折射贴图启用自定义数据", reflectCustomDataOnOri);
            if (reflectCustomDataOnRes) _ReflectTexCustomDataOn.floatValue = 1.0f;
            else _ReflectTexCustomDataOn.floatValue = 0.0f;
            
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_ReflectTex), _ReflectTex);
            
            Vector2 scale = targetMat.GetTextureScale("_ReflectTex");
            Vector2 offset = targetMat.GetTextureOffset("_ReflectTex");
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Vector2Field("Tiling", scale);
            offset = EditorGUILayout.Vector2Field("Offset", offset);
            if (EditorGUI.EndChangeCheck())
            {
                // 更新缩放和偏移
                _ReflectTex.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
            }
            materialEditor.FloatProperty(_NormalScale, "法线缩放");
            materialEditor.FloatProperty(_DistortTimeFactor_U, "流速 U");
            materialEditor.FloatProperty(_DistortTimeFactor_V, "流速 V");
            materialEditor.FloatProperty(_DistortStrength, "扰动强度");

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Mask = ShaderGUI_Helper.Foldout(_Mask, "遮罩");
        if (_Mask)
        {
            EditorGUI.indentLevel++;
            bool maskTexOnOri_1 = Equals(_MaskTexOn_1.floatValue, 1.0f);
            bool maskTexOnRes_1 = EditorGUILayout.Toggle("启用遮罩图 1", maskTexOnOri_1);
            if (maskTexOnRes_1)
            {
                EditorGUI.indentLevel++;
                _MaskTexOn_1.floatValue = 1.0f;
                materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_MaskTex_1), _MaskTex_1);
                
                Vector2 scale = targetMat.GetTextureScale("_MaskTex_1");
                Vector2 offset = targetMat.GetTextureOffset("_MaskTex_1");
                EditorGUI.BeginChangeCheck();
                scale = EditorGUILayout.Vector2Field("Tiling", scale);
                offset = EditorGUILayout.Vector2Field("Offset", offset);
                if (EditorGUI.EndChangeCheck())
                    _MaskTex_1.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                
                materialEditor.VectorProperty(_MaskTexFlowAndBevel_1, "遮罩图 1 流动 & 斜切");
                materialEditor.RangeProperty(_MaskTexRotation_1, "遮罩图 1 旋转");
                
                bool maskTexRedIsAlphaOnOri_1 = Equals(_MaskTexRedIsAlphaOn_1.floatValue, 1.0f);
                bool maskTexRedIsAlphaOnRes_1 = EditorGUILayout.Toggle("遮罩图 1 R是透明通道", maskTexRedIsAlphaOnOri_1);
                if (maskTexRedIsAlphaOnRes_1) _MaskTexRedIsAlphaOn_1.floatValue = 1.0f;
                else _MaskTexRedIsAlphaOn_1.floatValue = 0.0f;
                
                bool maskTexCustomDataOnOri_1 = Equals(_MaskTexCustomDataOn_1.floatValue, 1.0f);
                bool maskTexCustomDataOnRes_1 = EditorGUILayout.Toggle("遮罩图 1 启用自定义数据", maskTexCustomDataOnOri_1);
                if (maskTexCustomDataOnRes_1) _MaskTexCustomDataOn_1.floatValue = 1.0f;
                else _MaskTexCustomDataOn_1.floatValue = 0.0f;
                
                targetMat.EnableKeyword("_MASK_TEX_ON_1");
                EditorGUI.indentLevel--;
            }
            else
            {
                _MaskTexOn_1.floatValue = 0.0f;
                targetMat.DisableKeyword("_MASK_TEX_ON_1");
            }

            bool maskTexOnOri_2 = Equals(_MaskTexOn_2.floatValue, 1.0f);
            bool maskTexOnRes_2 = EditorGUILayout.Toggle("启用遮罩图 2", maskTexOnOri_2);
            if (maskTexOnRes_2)
            {
                EditorGUI.indentLevel++;
                _MaskTexOn_2.floatValue = 1.0f;
                materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_MaskTex_2), _MaskTex_2);
                
                Vector2 scale = targetMat.GetTextureScale("_MaskTex_2");
                Vector2 offset = targetMat.GetTextureOffset("_MaskTex_2");
                EditorGUI.BeginChangeCheck();
                scale = EditorGUILayout.Vector2Field("Tiling", scale);
                offset = EditorGUILayout.Vector2Field("Offset", offset);
                if (EditorGUI.EndChangeCheck())
                    _MaskTex_2.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                materialEditor.VectorProperty(_MaskTexFlowAndBevel_2, "遮罩图 2 流动 & 斜切");
                materialEditor.RangeProperty(_MaskTexRotation_2, "遮罩图 2 旋转");
                
                bool maskTexRedIsAlphaOnOri_2 = Equals(_MaskTexRedIsAlphaOn_2.floatValue, 1.0f);
                bool maskTexRedIsAlphaOnRes_2 = EditorGUILayout.Toggle("遮罩图 2 R是透明通道", maskTexRedIsAlphaOnOri_2);
                if (maskTexRedIsAlphaOnRes_2) _MaskTexRedIsAlphaOn_2.floatValue = 1.0f;
                else _MaskTexRedIsAlphaOn_2.floatValue = 0.0f;
                
                bool maskTexCustomDataOnOri_2 = Equals(_MaskTexCustomDataOn_2.floatValue, 1.0f);
                bool maskTexCustomDataOnRes_2 = EditorGUILayout.Toggle("遮罩图 2 启用自定义数据", maskTexCustomDataOnOri_2);
                if (maskTexCustomDataOnRes_2) _MaskTexCustomDataOn_2.floatValue = 1.0f;
                else _MaskTexCustomDataOn_2.floatValue = 0.0f;
                
                targetMat.EnableKeyword("_MASK_TEX_ON_2");
                EditorGUI.indentLevel--;
            }
            else
            {
                _MaskTexOn_2.floatValue = 0.0f;
                targetMat.DisableKeyword("_MASK_TEX_ON_2");
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
}