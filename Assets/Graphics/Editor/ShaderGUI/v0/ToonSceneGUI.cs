using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;

enum ToonSceneShaderTexTag
{
    _BaseMap, _NormalMap, _MaskMap, _RoughnessMap, _EmissionMap, _AoMap, _MetallicMap
}

public class ToonSceneGUI : ShaderGUI
{
    private Material targetMat;
    
    private MaterialProperty _CullOff;
    private MaterialProperty _Transparent;
    private MaterialProperty _StencilOutline;
    private MaterialProperty _Unlit;
    private MaterialProperty _SrcBlend;
    private MaterialProperty _DstBlend;
    private MaterialProperty _SrcBlendAlpha;
    private MaterialProperty _DstBlendAlpha;
    private MaterialProperty _ZWrite;
    private MaterialProperty _StencilOutlineValue;
    private MaterialProperty _StencilOutlinePass;
    
    private MaterialProperty _BaseMap;
    private MaterialProperty _BaseColor;
    private MaterialProperty _BaseMapValue;
    private MaterialProperty _BaseMapSaturation;
    private MaterialProperty _BaseColorIntensity;
    private MaterialProperty _EmissionMap;
    private MaterialProperty _EmissionIntensity;
    private MaterialProperty _EmissionColor;
    private MaterialProperty _FresnelColor;
    private MaterialProperty _Fresnel;
    
    private MaterialProperty _NormalMap;
    
    private MaterialProperty _MaskMap;
    private MaterialProperty _MetallicMap;
    private MaterialProperty _RoughnessMap;
    private MaterialProperty _AoMap;
    private MaterialProperty _Metallic;
    private MaterialProperty _Roughness;
    
    private MaterialProperty _RimColor;
    private MaterialProperty _RimMin;
    private MaterialProperty _RimMax;
    private MaterialProperty _RimSmooth;
    private MaterialProperty _RimFac;
    
    private MaterialProperty _BaseOutlineColor;
    private MaterialProperty _BaseOutlineOffset;
    private MaterialProperty _StencilOutlineColor;
    private MaterialProperty _StencilOutlineOffset;

    private bool _Base = true;
    private bool _Rim = true;
    private bool _Outline = true;

    void FinProperties(MaterialProperty[] properties)
    {
        _CullOff = FindProperty("_CullOff", properties, true);
        _Transparent = FindProperty("_Transparent", properties, true);
        _StencilOutline = FindProperty("_StencilOutline", properties, true);
        _Unlit = FindProperty("_Unlit", properties, true);
        _SrcBlend = FindProperty("_SrcBlend", properties, true);
        _DstBlend = FindProperty("_DstBlend", properties, true);
        _SrcBlendAlpha = FindProperty("_SrcBlendAlpha", properties, true);
        _DstBlendAlpha = FindProperty("_DstBlendAlpha", properties, true);
        _ZWrite = FindProperty("_ZWrite", properties, true);
        _StencilOutlineValue = FindProperty("_StencilOutlineValue", properties, true);
        _StencilOutlinePass = FindProperty("_StencilOutlinePass", properties, true);
        
        _BaseMap = FindProperty("_BaseMap", properties, true);
        _BaseColor = FindProperty("_BaseColor", properties, true);
        _BaseMapValue = FindProperty("_BaseMapValue", properties, true);
        _BaseMapSaturation = FindProperty("_BaseMapSaturation", properties, true);
        _BaseColorIntensity = FindProperty("_BaseColorIntensity", properties, true);
        
        _EmissionMap = FindProperty("_EmissionMap", properties, true);
        _EmissionIntensity = FindProperty("_EmissionIntensity", properties, true);
        _EmissionColor = FindProperty("_EmissionColor", properties, true);
        
        _FresnelColor = FindProperty("_FresnelColor", properties, true);
        _Fresnel = FindProperty("_Fresnel", properties, true);
        
        _NormalMap = FindProperty("_NormalMap", properties, true);
        
        _MaskMap = FindProperty("_MaskMap", properties, true);
        _MetallicMap = FindProperty("_MetallicMap", properties, true);
        _RoughnessMap = FindProperty("_RoughnessMap", properties, true);
        _AoMap = FindProperty("_AoMap", properties, true);
        _Metallic = FindProperty("_Metallic", properties, true);
        _Roughness = FindProperty("_Roughness", properties, true);
        
        _RimColor = FindProperty("_RimColor", properties, true);
        _RimMin = FindProperty("_RimMin", properties, true);
        _RimMax = FindProperty("_RimMax", properties, true);
        _RimSmooth = FindProperty("_RimSmooth", properties, true);
        _RimFac = FindProperty("_RimFac", properties, true);
        
        _BaseOutlineColor = FindProperty("_BaseOutlineColor", properties, true);
        _BaseOutlineOffset = FindProperty("_BaseOutlineOffset", properties, true);
        _StencilOutlineColor = FindProperty("_StencilOutlineColor", properties, true);
        _StencilOutlineOffset = FindProperty("_StencilOutlineOffset", properties, true);
    }

    private void EnableToonTextureKeywords()
    {
        foreach (var texTag in Enum.GetValues(typeof(ToonSceneShaderTexTag)))
        {
            String tag = texTag.ToString();
            Texture mainTex = targetMat.GetTexture(tag);
            if (mainTex != null)
                targetMat.EnableKeyword(tag + "On");
            else
                targetMat.DisableKeyword(tag + "On");
        }
    }
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        targetMat = materialEditor.target as Material;
        FinProperties(properties);
        EnableToonTextureKeywords();
        
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
                targetMat.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                _Transparent.floatValue = 0.0f;
                _SrcBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                _DstBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.Zero;;
                _SrcBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                _DstBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.Zero;
                _ZWrite.floatValue = 1.0f;
                targetMat.renderQueue = (int)RenderQueue.Geometry;
            }

            bool stencilOutlineOri = !Equals(_StencilOutline.floatValue, 0.0f);
            bool stencilOutlineRes = EditorGUILayout.Toggle("Stencil Outline", stencilOutlineOri);
            _StencilOutline.floatValue = stencilOutlineRes ? 1.0f : 0.0f;
            if (stencilOutlineRes)
            {
                _StencilOutlineValue.floatValue = 1.0f;
                _StencilOutlinePass.floatValue = 2.0f;
            }
            else
            {
                _StencilOutlineValue.floatValue = 0.0f;
                _StencilOutlinePass.floatValue = 0.0f;
            }
            
            bool unlitOri = !Equals(_Unlit.floatValue, 0.0f);
            bool unlitRes = EditorGUILayout.Toggle("Unlit", unlitOri);
            _Unlit.floatValue = unlitRes ? 1.0f : 0.0f;
            if (unlitRes) targetMat.EnableKeyword("_Unlit_ON");
            else targetMat.DisableKeyword("_Unlit_ON");
            
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
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_MetallicMap), _MetallicMap);
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_RoughnessMap), _RoughnessMap);
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_AoMap), _AoMap);
            
            materialEditor.ColorProperty(_BaseColor, "Base Color");
            materialEditor.RangeProperty(_BaseMapValue, "Base Map Value");
            materialEditor.RangeProperty(_BaseMapSaturation, "Base Map Saturation");
            materialEditor.RangeProperty(_BaseColorIntensity, "Base Color Intensity");
            
            materialEditor.ColorProperty(_FresnelColor, "Fresnel Color");
            materialEditor.RangeProperty(_Fresnel, "Fresnel");
            
            materialEditor.RangeProperty(_Metallic, "Metallic");
            materialEditor.RangeProperty(_Roughness, "Roughness");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Rim = ShaderGUI_Helper.Foldout(_Rim, "Rim");
        if (_Rim)
        {
            EditorGUI.indentLevel++;
            materialEditor.ColorProperty(_RimColor, "Rim Color");
            materialEditor.RangeProperty(_RimMin, "Rim Min");
            materialEditor.RangeProperty(_RimMax, "Rim Max");
            materialEditor.RangeProperty(_RimSmooth, "Rim Smooth");
            materialEditor.RangeProperty(_RimFac, "Rim Fac");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Outline = ShaderGUI_Helper.Foldout(_Outline, "Outline");
        if (_Outline)
        {
            EditorGUI.indentLevel++;
            materialEditor.ColorProperty(_BaseOutlineColor, "Base Outline Color");
            materialEditor.RangeProperty(_BaseOutlineOffset, "Base Outline Offset");
            materialEditor.ColorProperty(_StencilOutlineColor, "Stencil Outline Color");
            materialEditor.RangeProperty(_StencilOutlineOffset, "Stencil Outline Offset");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
}