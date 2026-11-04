using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;

enum PBRCharacterShaderTexTag_V2
{
    _BaseMap, _NormalMap, _MaskMap, _RoughnessMap, _EmissionMap, _AoMap, _MetallicMap, _SDFMap
}

public class PBRCharacterGUI_V2 : ShaderGUI
{
    private Material targetMat;
    
    private MaterialProperty _CullOff;
    private MaterialProperty _Transparent;
    private MaterialProperty _StencilOutline;
    private MaterialProperty _ScreenSpaceShadow;
    private MaterialProperty _Pre_SSS;
    private MaterialProperty _AnisoOn;
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
    
    private MaterialProperty _ShadowRatio;
    private MaterialProperty _ShadowThreshold;
    
    private MaterialProperty _RimColor;
    private MaterialProperty _RimMin;
    private MaterialProperty _RimMax;
    private MaterialProperty _RimSmooth;
    private MaterialProperty _RimFac;
    
    private MaterialProperty _BaseOutlineColor;
    private MaterialProperty _BaseOutlineOffset;
    private MaterialProperty _StencilOutlineColor;
    private MaterialProperty _StencilOutlineOffset;
    
    private MaterialProperty _RampSmoothness;
    private MaterialProperty _ShadowFadeColor;
    private MaterialProperty _ShadowColor;
    private MaterialProperty _ShallowFadeColor;
    private MaterialProperty _ShallowColor;
    private MaterialProperty _SSSColor;
    private MaterialProperty _FrontColor;
    private MaterialProperty _ForwardColor;

    private MaterialProperty _ScreenSpaceShadowWidth;
    private MaterialProperty _ScreenSpaceShadowFadeout;
    private MaterialProperty _ScreenSpaceShadowThreshold;

    private MaterialProperty _SDFMap;
    private MaterialProperty _SDFMaskMap;
    private MaterialProperty _NeckBaseColor;
    private MaterialProperty _SDFAoMap;
    private MaterialProperty _NoseMaskMap;
    private MaterialProperty _SDFRampSmoothness;
    private MaterialProperty _SDFShadowFadeColor;
    private MaterialProperty _SDFShadowColor;
    private MaterialProperty _SDFShallowFadeColor;
    private MaterialProperty _SDFShallowColor;
    private MaterialProperty _SDFScreenSpaceShadowWidth;
    private MaterialProperty _SDFScreenSpaceShadowFadeout;
    private MaterialProperty _SDFScreenSpaceShadowThreshold;

    private MaterialProperty _AnisoTint;
    private MaterialProperty _RoughnessX;
    private MaterialProperty _RoughnessY;
    private MaterialProperty _IOR;

    private MaterialProperty _ThicknessMap;
    private MaterialProperty _ScatterNormalDistortion;
    private MaterialProperty _ScatterPower;
    private MaterialProperty _ScatterScale;
    private MaterialProperty _ScatterColor;

    private bool _Base = true;
    private bool _Shadow = true;
    private bool _Rim = true;
    private bool _Outline = true;
    private bool _Ramp = true;
    private bool _ScreenSpaceShadowTitle = true;
    private bool _SDF = true;
    private bool _Aniso = true;
    private bool _SSS = true;

    void FinProperties(MaterialProperty[] properties)
    {
        _CullOff = FindProperty("_CullOff", properties, true);
        _Transparent = FindProperty("_Transparent", properties, true);
        _StencilOutline = FindProperty("_StencilOutline", properties, true);
        _ScreenSpaceShadow = FindProperty("_ScreenSpaceShadow", properties, true);
        _Pre_SSS = FindProperty("_Pre_SSS", properties, true);
        _AnisoOn = FindProperty("_AnisoOn", properties, true);
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
        
        _ShadowRatio = FindProperty("_ShadowRatio", properties, true);
        _ShadowThreshold = FindProperty("_ShadowThreshold", properties, true);
        
        _RimColor = FindProperty("_RimColor", properties, true);
        _RimMin = FindProperty("_RimMin", properties, true);
        _RimMax = FindProperty("_RimMax", properties, true);
        _RimSmooth = FindProperty("_RimSmooth", properties, true);
        _RimFac = FindProperty("_RimFac", properties, true);
        
        _BaseOutlineColor = FindProperty("_BaseOutlineColor", properties, true);
        _BaseOutlineOffset = FindProperty("_BaseOutlineOffset", properties, true);
        _StencilOutlineColor = FindProperty("_StencilOutlineColor", properties, true);
        _StencilOutlineOffset = FindProperty("_StencilOutlineOffset", properties, true);
        
        _RampSmoothness = FindProperty("_RampSmoothness", properties, true);
        _ShadowFadeColor = FindProperty("_ShadowFadeColor", properties, true);
        _ShadowColor = FindProperty("_ShadowColor", properties, true);
        _ShallowFadeColor = FindProperty("_ShallowFadeColor", properties, true);
        _ShallowColor = FindProperty("_ShallowColor", properties, true);
        _SSSColor = FindProperty("_SSSColor", properties, true);
        _FrontColor = FindProperty("_FrontColor", properties, true);
        _ForwardColor = FindProperty("_ForwardColor", properties, true);
        
        _ScreenSpaceShadowWidth = FindProperty("_ScreenSpaceShadowWidth", properties, true);
        _ScreenSpaceShadowFadeout = FindProperty("_ScreenSpaceShadowFadeout", properties, true);
        _ScreenSpaceShadowThreshold = FindProperty("_ScreenSpaceShadowThreshold", properties, true);
        
        _SDFMap = FindProperty("_SDFMap", properties, true);
        _SDFMaskMap = FindProperty("_SDFMaskMap", properties, true);
        _NeckBaseColor = FindProperty("_NeckBaseColor", properties, true);
        _SDFAoMap = FindProperty("_SDFAoMap", properties, true);
        _NoseMaskMap = FindProperty("_NoseMaskMap", properties, true);
        _SDFRampSmoothness = FindProperty("_SDFRampSmoothness", properties, true);
        _SDFShadowFadeColor = FindProperty("_SDFShadowFadeColor", properties, true);
        _SDFShadowColor = FindProperty("_SDFShadowColor", properties, true);
        _SDFShallowFadeColor = FindProperty("_SDFShallowFadeColor", properties, true);
        _SDFShallowColor = FindProperty("_SDFShallowColor", properties, true);
        _SDFScreenSpaceShadowWidth = FindProperty("_SDFScreenSpaceShadowWidth", properties, true);
        _SDFScreenSpaceShadowFadeout = FindProperty("_SDFScreenSpaceShadowFadeout", properties, true);
        _SDFScreenSpaceShadowThreshold = FindProperty("_SDFScreenSpaceShadowThreshold", properties, true);
        
        _AnisoTint = FindProperty("_AnisoTint", properties, true);
        _RoughnessX = FindProperty("_RoughnessX", properties, true);
        _RoughnessY = FindProperty("_RoughnessY", properties, true);
        _IOR = FindProperty("_IOR", properties, true);
        
        _ThicknessMap = FindProperty("_ThicknessMap", properties, true);
        _ScatterNormalDistortion = FindProperty("_ScatterNormalDistortion", properties, true);
        _ScatterPower = FindProperty("_ScatterPower", properties, true);
        _ScatterScale = FindProperty("_ScatterScale", properties, true);
        _ScatterColor = FindProperty("_ScatterColor", properties, true);
    }

    private void EnableToonTextureKeywords()
    {
        foreach (var texTag in Enum.GetValues(typeof(PBRCharacterShaderTexTag_V2)))
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

            bool screenSpaceShadowOri = !Equals(_ScreenSpaceShadow.floatValue, 0.0f);
            bool screenSpaceShadowRes = EditorGUILayout.Toggle("Screen Space Shadow", screenSpaceShadowOri);
            _ScreenSpaceShadow.floatValue = screenSpaceShadowRes ? 1.0f : 0.0f;
            if (screenSpaceShadowRes) targetMat.EnableKeyword("_SCREEN_SPACE_SHADOW");
            else targetMat.DisableKeyword("_SCREEN_SPACE_SHADOW");
            
            bool preSSSOri = !Equals(_Pre_SSS.floatValue, 0.0f);
            bool preSSSRes = EditorGUILayout.Toggle("Pre SSS", preSSSOri);
            _Pre_SSS.floatValue = preSSSRes ? 1.0f : 0.0f;
            if (preSSSRes) targetMat.EnableKeyword("_PRE_SSS");
            else targetMat.DisableKeyword("_PRE_SSS");
            
            bool anisoOnOri = !Equals(_AnisoOn.floatValue, 0.0f);
            bool anisoOnRes = EditorGUILayout.Toggle("Aniso On", anisoOnOri);
            _AnisoOn.floatValue = anisoOnRes ? 1.0f : 0.0f;
            if (anisoOnRes) targetMat.EnableKeyword("_ANISO_ON");
            else targetMat.DisableKeyword("_ANISO_ON");
            
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
        _Shadow = ShaderGUI_Helper.Foldout(_Shadow, "Shadow");
        if (_Shadow)
        {
            EditorGUI.indentLevel++;
            materialEditor.RangeProperty(_ShadowRatio, "Shadow Ratio");
            materialEditor.RangeProperty(_ShadowThreshold, "Shadow Threshold");
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

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Ramp = ShaderGUI_Helper.Foldout(_Ramp, "Ramp");
        if (_Ramp)
        {
            EditorGUI.indentLevel++;
            materialEditor.RangeProperty(_RampSmoothness, "Ramp Smoothness");
            materialEditor.ColorProperty(_ShadowFadeColor, "Shadow Fade Color");
            materialEditor.ColorProperty(_ShadowColor, "Shadow Color");
            materialEditor.ColorProperty(_ShallowFadeColor, "Shallow Fade Color");
            materialEditor.ColorProperty(_ShallowColor, "Shadow Color");
            materialEditor.ColorProperty(_SSSColor, "SSS Color");
            materialEditor.ColorProperty(_FrontColor, "Front Color");
            materialEditor.ColorProperty(_ForwardColor, "Forward Color");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _ScreenSpaceShadowTitle = ShaderGUI_Helper.Foldout(_ScreenSpaceShadowTitle, "Screen Space Shadow");
        if (_ScreenSpaceShadowTitle)
        {
            EditorGUI.indentLevel++;
            materialEditor.FloatProperty(_ScreenSpaceShadowWidth, "Screen Space Shadow Width");
            materialEditor.FloatProperty(_ScreenSpaceShadowFadeout, "Screen Space Shadow Fadeout");
            materialEditor.RangeProperty(_ScreenSpaceShadowThreshold, "Screen Space Shadow Threshold");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _SDF = ShaderGUI_Helper.Foldout(_SDF, "SDF");
        if (_SDF)
        {
            EditorGUI.indentLevel++;
            materialEditor.RangeProperty(_SDFRampSmoothness, "SDF Ramp Smoothness");
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_SDFMap), _SDFMap);
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_SDFMaskMap), _SDFMaskMap);
            materialEditor.ColorProperty(_NeckBaseColor, "NecK Base Color");
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_SDFAoMap), _SDFAoMap);
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_NoseMaskMap), _NoseMaskMap);
            materialEditor.ColorProperty(_SDFShadowFadeColor, "SDF Shadow Fade Color");
            materialEditor.ColorProperty(_SDFShadowColor, "SDF Shadow Color");
            materialEditor.ColorProperty(_SDFShallowFadeColor, "SDF Shallow Fade Color");
            materialEditor.ColorProperty(_SDFShallowColor, "SDF Shadow Color");
            materialEditor.FloatProperty(_SDFScreenSpaceShadowWidth, "SDF Screen Space Shadow Width");
            materialEditor.FloatProperty(_SDFScreenSpaceShadowFadeout, "SDF Screen Space Shadow Fadeout");
            materialEditor.RangeProperty(_SDFScreenSpaceShadowThreshold, "SDF Screen Space Shadow Threshold");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Aniso = ShaderGUI_Helper.Foldout(_Aniso, "Aniso");
        if (_Aniso)
        {
            EditorGUI.indentLevel++;
            materialEditor.ColorProperty(_AnisoTint, "Aniso Tint");
            materialEditor.RangeProperty(_RoughnessX, "Roughness X");
            materialEditor.RangeProperty(_RoughnessY, "Roughness Y");
            materialEditor.RangeProperty(_IOR, "IOR");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _SSS = ShaderGUI_Helper.Foldout(_SSS, "SSS");
        if (_SSS)
        {
            EditorGUI.indentLevel++;
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_ThicknessMap), _ThicknessMap);
            materialEditor.RangeProperty(_ScatterNormalDistortion, "Normal Distortion");
            materialEditor.RangeProperty(_ScatterPower, "Scatter Power");
            materialEditor.RangeProperty(_ScatterScale, "Scatter Scale");
            materialEditor.ColorProperty(_ScatterColor, "Scatter Color");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
}