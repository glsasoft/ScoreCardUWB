using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;

public class PBRSceneGUI_V4 : ShaderGUI
{
    private Material targetMat;
    
    private MaterialProperty _CullOff;
    private MaterialProperty _Transparent;
    private MaterialProperty _Unlit;
    private MaterialProperty _Static;
    private MaterialProperty _SrcBlend;
    private MaterialProperty _DstBlend;
    private MaterialProperty _SrcBlendAlpha;
    private MaterialProperty _DstBlendAlpha;
    private MaterialProperty _ZWrite;
    private MaterialProperty _Queue;
    
    private MaterialProperty _BaseMap;
    private MaterialProperty _BaseColor;
    private MaterialProperty _BaseMapValue;
    private MaterialProperty _BaseMapSaturation;
    private MaterialProperty _BaseColorIntensity;
    private MaterialProperty _EmissionMap;
    private MaterialProperty _EmissionIntensity;
    private MaterialProperty _EmissionColor;
    
    private MaterialProperty _NormalMap;
    
    private MaterialProperty _MaskMap;
    private MaterialProperty _Metallic;
    private MaterialProperty _Roughness;

    private MaterialProperty _Hollow;
    private MaterialProperty _RisAlpha;
    private MaterialProperty _FlowSpeed;
    private MaterialProperty _HollowDirection_X;
    private MaterialProperty _HollowDirection_Y;
    private MaterialProperty _AmbientColorInt;

    private bool _Base = true;
    private bool _HollowPage = true;

    void FinProperties(MaterialProperty[] properties)
    {
        _CullOff = FindProperty("_CullOff", properties, true);
        _Transparent = FindProperty("_Transparent", properties, true);
        _Unlit = FindProperty("_Unlit", properties, true);
        _Static = FindProperty("_Static", properties, true);
        _SrcBlend = FindProperty("_SrcBlend", properties, true);
        _DstBlend = FindProperty("_DstBlend", properties, true);
        _SrcBlendAlpha = FindProperty("_SrcBlendAlpha", properties, true);
        _DstBlendAlpha = FindProperty("_DstBlendAlpha", properties, true);
        _ZWrite = FindProperty("_ZWrite", properties, true);
        _Queue = FindProperty("_Queue", properties, true);
        
        _BaseMap = FindProperty("_BaseMap", properties, true);
        _BaseColor = FindProperty("_BaseColor", properties, true);
        _BaseMapValue = FindProperty("_BaseMapValue", properties, true);
        _BaseMapSaturation = FindProperty("_BaseMapSaturation", properties, true);
        _BaseColorIntensity = FindProperty("_BaseColorIntensity", properties, true);
        
        _EmissionMap = FindProperty("_EmissionMap", properties, true);
        _EmissionIntensity = FindProperty("_EmissionIntensity", properties, true);
        _EmissionColor = FindProperty("_EmissionColor", properties, true);
        
        _NormalMap = FindProperty("_NormalMap", properties, true);
        
        _MaskMap = FindProperty("_MaskMap", properties, true);
        _Metallic = FindProperty("_Metallic", properties, true);
        _Roughness = FindProperty("_Roughness", properties, true);
        
        _Hollow = FindProperty("_Hollow", properties, true);
        _RisAlpha = FindProperty("_RisAlpha", properties, true);
        _FlowSpeed = FindProperty("_FlowSpeed", properties, true);
        _HollowDirection_X = FindProperty("_HollowDirection_X", properties, true);
        _HollowDirection_Y = FindProperty("_HollowDirection_Y", properties, true);
        _AmbientColorInt = FindProperty("_AmbientColorInt", properties, true);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        targetMat = materialEditor.target as Material;
        FinProperties(properties);
        
        materialEditor.FloatProperty(_Queue, "Queue Property");
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Base = ShaderGUI_Helper.Foldout(_Base, "Base");
        if (_Base)
        {
            EditorGUI.indentLevel++;
            bool cullOffOri = !Equals(_CullOff.floatValue, 2.0f);
            bool cullOffRes = EditorGUILayout.Toggle("Two Side", cullOffOri);
            _CullOff.floatValue = cullOffRes ? 0.0f : 2.0f;
            bool transparentOri = Equals(_Transparent.floatValue, 1.0f);
            bool transparentRes = EditorGUILayout.Toggle("Transparent", transparentOri);
            if (transparentRes)
            {
                _Transparent.floatValue = 1.0f;
                _SrcBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                _DstBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;;
                _SrcBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                _DstBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                _ZWrite.floatValue = 0.0f;
                targetMat.renderQueue = (int)RenderQueue.Transparent + (int)_Queue.floatValue;
            }
            else
            {
                _Transparent.floatValue = 0.0f;
                _SrcBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                _DstBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.Zero;;
                _SrcBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                _DstBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.Zero;
                _ZWrite.floatValue = 1.0f;
                targetMat.renderQueue = (int)RenderQueue.Geometry + (int)_Queue.floatValue;
            }
            
            bool unlitOri = !Equals(_Unlit.floatValue, 0.0f);
            bool unlitRes = EditorGUILayout.Toggle("Unlit", unlitOri);
            _Unlit.floatValue = unlitRes ? 1.0f : 0.0f;
            
            bool staticOri = !Equals(_Static.floatValue, 0.0f);
            bool staticRes = EditorGUILayout.Toggle("Static", staticOri);
            _Static.floatValue = staticRes ? 1.0f : 0.0f;
            
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_BaseMap), _BaseMap);
            
            Vector2 scale = targetMat.GetTextureScale("_BaseMap");
            Vector2 offset = targetMat.GetTextureOffset("_BaseMap");
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Vector2Field("Tiling", scale);
            offset = EditorGUILayout.Vector2Field("Offset", offset);
            if (EditorGUI.EndChangeCheck())
            {
                // 更新缩放和偏移
                _BaseMap.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
            }
            
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_EmissionMap), _EmissionMap);
            materialEditor.RangeProperty(_EmissionIntensity, "Emission Intensity");
            materialEditor.ColorProperty(_EmissionColor, "Emission Color");
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_NormalMap), _NormalMap);
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_MaskMap), _MaskMap);
            
            materialEditor.ColorProperty(_BaseColor, "Base Color");
            materialEditor.RangeProperty(_BaseMapValue, "Base Map Value");
            materialEditor.RangeProperty(_BaseMapSaturation, "Base Map Saturation");
            materialEditor.RangeProperty(_BaseColorIntensity, "Base Color Intensity");
            
            materialEditor.RangeProperty(_Metallic, "Metallic");
            materialEditor.RangeProperty(_Roughness, "Roughness");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _HollowPage = ShaderGUI_Helper.Foldout(_HollowPage, "Hollow");
        if (_HollowPage)
        {
            EditorGUI.indentLevel++;
            bool hollowOri = !Equals(_Hollow.floatValue, 0.0f);
            bool hollowRes = EditorGUILayout.Toggle("Hollow", hollowOri);
            _Hollow.floatValue = hollowRes ? 1.0f : 0.0f;
            
            bool rIsAlphaOri = !Equals(_RisAlpha.floatValue, 0.0f);
            bool rIsAlphaRes = EditorGUILayout.Toggle("RisAlpha", rIsAlphaOri);
            _RisAlpha.floatValue = rIsAlphaRes ? 1.0f : 0.0f;

            materialEditor.RangeProperty(_FlowSpeed, "Flow Speed");
            materialEditor.VectorProperty(_HollowDirection_X, "Hollow Direction X");
            materialEditor.VectorProperty(_HollowDirection_Y, "Hollow Direction Y");
            materialEditor.FloatProperty(_AmbientColorInt, "AmbientColorInt");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
}