using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;
using NUnit.Framework.Internal;

enum PBRCharacterShaderTexTag_V4
{
    _BaseMap, _NormalMap, _MaskMap, _EmissionMap, _SDFMap, _DissolveMap, _RampMap
}

public class PBRCharacterGUI_V4 : ShaderGUI
{
    private Material targetMat;

    private MaterialProperty _CullOff;
    private MaterialProperty _Transparent;
    private MaterialProperty _StencilOutline;
    private MaterialProperty _PreSSS_On;
    private MaterialProperty _AnisoOn;
    private MaterialProperty _SDF_On;
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

    private MaterialProperty _ShadowColor;

    private MaterialProperty _RampMap;

    private MaterialProperty _SDFMap;

    private MaterialProperty _AnisoTint;
    private MaterialProperty _RoughnessX;
    private MaterialProperty _RoughnessY;
    private MaterialProperty _IOR;

    private MaterialProperty _ThicknessMap;
    private MaterialProperty _ScatterNormalDistortion;
    private MaterialProperty _ScatterPower;
    private MaterialProperty _ScatterScale;
    private MaterialProperty _ScatterColor;

    private MaterialProperty _Dissolve_On;
    private MaterialProperty _DissolveFit;

    private MaterialProperty _DissolveMapThreshold;
    private MaterialProperty _DissolveDirThreshold;
    private MaterialProperty  _DissolveColor;
    private MaterialProperty _DissolveColorStrength;
     private MaterialProperty _DissolveDir;
    private MaterialProperty _DissolveDirWidth;

    private MaterialProperty _DissolveMap;
    private MaterialProperty _DissolveTexFlowAndBevel;
    private MaterialProperty _DissolveMapEdge;


    private MaterialProperty _HeadForward;
    private MaterialProperty _HeadCenter;
    private MaterialProperty _HeadRight;

    private bool _Base = true;
    private bool _Shadow = true;
    private bool _Rim = true;
    private bool _Outline = true;
    private bool _Ramp = true;
    private bool _SDF = true;
    private bool _Aniso = true;
    private bool _SSS = true;
    private bool _Dissolve = true;

    void FindProperties(MaterialProperty[] properties)
    {
        _CullOff = FindProperty("_CullOff", properties, true);
        _Transparent = FindProperty("_Transparent", properties, true);
        _StencilOutline = FindProperty("_StencilOutline", properties, true);
        _PreSSS_On = FindProperty("_PreSSS_On", properties, true); // 统一定义
        _AnisoOn = FindProperty("_AnisoOn", properties, true);
        _SDF_On = FindProperty("_SDF_On", properties, true);
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

        _ShadowColor = FindProperty("_ShadowColor", properties, true);
        _RampMap = FindProperty("_RampMap", properties, true);

        _SDFMap = FindProperty("_SDFMap", properties, true);

        _AnisoTint = FindProperty("_AnisoTint", properties, true);
        _RoughnessX = FindProperty("_RoughnessX", properties, true);
        _RoughnessY = FindProperty("_RoughnessY", properties, true);
        _IOR = FindProperty("_IOR", properties, true);

        _ThicknessMap = FindProperty("_ThicknessMap", properties, true);
        _ScatterNormalDistortion = FindProperty("_ScatterNormalDistortion", properties, true);
        _ScatterPower = FindProperty("_ScatterPower", properties, true);
        _ScatterScale = FindProperty("_ScatterScale", properties, true);
        _ScatterColor = FindProperty("_ScatterColor", properties, true);

        _Dissolve_On = FindProperty("_Dissolve_On", properties, true);
        _DissolveFit = FindProperty("_DissolveFit", properties, true);
        _DissolveMapThreshold = FindProperty("_DissolveMapThreshold", properties, true);
        _DissolveDirThreshold = FindProperty("_DissolveDirThreshold", properties, true);
        _DissolveColor = FindProperty("_DissolveColor", properties, true);
        _DissolveColorStrength = FindProperty("_DissolveColorStrength", properties, true);
        _DissolveDir = FindProperty("_DissolveDir", properties, true);
        _DissolveDirWidth = FindProperty("_DissolveDirWidth", properties, true);
        _DissolveMap = FindProperty("_DissolveMap", properties, true);
        _DissolveTexFlowAndBevel = FindProperty("_DissolveTexFlowAndBevel", properties, true);
        _DissolveMapEdge = FindProperty("_DissolveMapEdge", properties, true);

        _HeadForward = FindProperty("_HeadForward", properties, true);
        _HeadCenter = FindProperty("_HeadCenter", properties, true);
        _HeadRight = FindProperty("_HeadRight", properties, true);
    }

    private void EnableToonTextureKeywords()
    {
        foreach (var texTag in Enum.GetValues(typeof(PBRCharacterShaderTexTag_V4)))
        {
            string tag = texTag.ToString();
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
        FindProperties(properties);
        EnableToonTextureKeywords();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Base = ShaderGUI_Helper.Foldout(_Base, "Base");
        if (_Base)
        {
            EditorGUI.indentLevel++;
            bool cullOffOri = !Mathf.Approximately(_CullOff.floatValue, 2.0f);
            bool cullOffRes = EditorGUILayout.Toggle("Two Side", cullOffOri);
            _CullOff.floatValue = cullOffRes ? 0.0f : 2.0f;

            bool transparentOri = Mathf.Approximately(_Transparent.floatValue, 1.0f);
            bool transparentRes = EditorGUILayout.Toggle("Transparent", transparentOri);
            if (transparentRes)
            {
                _Transparent.floatValue = 1.0f;
                _SrcBlend.floatValue = (float)BlendMode.SrcAlpha;
                _DstBlend.floatValue = (float)BlendMode.OneMinusSrcAlpha;
                _SrcBlendAlpha.floatValue = (float)BlendMode.One;
                _DstBlendAlpha.floatValue = (float)BlendMode.OneMinusSrcAlpha;
                _ZWrite.floatValue = 0.0f;
                targetMat.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                _Transparent.floatValue = 0.0f;
                _SrcBlend.floatValue = (float)BlendMode.One;
                _DstBlend.floatValue = (float)BlendMode.Zero;
                _SrcBlendAlpha.floatValue = (float)BlendMode.One;
                _DstBlendAlpha.floatValue = (float)BlendMode.Zero;
                _ZWrite.floatValue = 1.0f;
                targetMat.renderQueue = (int)RenderQueue.Geometry;
            }

            bool stencilOutlineOri = !Mathf.Approximately(_StencilOutline.floatValue, 0.0f);
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


            bool anisoOnOri = !Mathf.Approximately(_AnisoOn.floatValue, 0.0f);
            bool anisoOnRes = EditorGUILayout.Toggle("Aniso On", anisoOnOri);
            _AnisoOn.floatValue = anisoOnRes ? 1.0f : 0.0f;
            if (anisoOnRes) targetMat.EnableKeyword("_ANISO_ON");
            else targetMat.DisableKeyword("_ANISO_ON");
            
            bool sdfOnOri = !Mathf.Approximately(_SDF_On.floatValue, 0.0f);
            bool sdfOnRes = EditorGUILayout.Toggle("SDF On", sdfOnOri);
            _SDF_On.floatValue = sdfOnRes ? 1.0f : 0.0f;

            bool unlitOri = !Mathf.Approximately(_Unlit.floatValue, 0.0f);
            bool unlitRes = EditorGUILayout.Toggle("Unlit", unlitOri);
            _Unlit.floatValue = unlitRes ? 1.0f : 0.0f;
            if (unlitRes) targetMat.EnableKeyword("_UNLIT_ON");
            else targetMat.DisableKeyword("_UNLIT_ON");

            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_BaseMap), _BaseMap);

            Vector2 scale = targetMat.GetTextureScale("_BaseMap");
            Vector2 offset = targetMat.GetTextureOffset("_BaseMap");
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Vector2Field("Tiling", scale);
            offset = EditorGUILayout.Vector2Field("Offset", offset);
            if (EditorGUI.EndChangeCheck())
            {
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

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Ramp = ShaderGUI_Helper.Foldout(_Ramp, "Ramp");
        if (_Ramp)
        {
            EditorGUI.indentLevel++;
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_RampMap), _RampMap);
            materialEditor.ColorProperty(_ShadowColor, "Shadow Color");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _SDF = ShaderGUI_Helper.Foldout(_SDF, "SDF");
        if (_SDF)
        {
            EditorGUI.indentLevel++;
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_SDFMap), _SDFMap);
            materialEditor.VectorProperty(_HeadForward, "Head Forward");
            materialEditor.VectorProperty(_HeadCenter, "Head Center");
            materialEditor.VectorProperty(_HeadRight, "Head Right");
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
            materialEditor.RangeProperty(_PreSSS_On, "PreSSS On"); // 移动到 SSS 部分
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Dissolve = ShaderGUI_Helper.Foldout(_Dissolve, "Dissolve");
        if (_Dissolve)
        {
            EditorGUI.indentLevel++;

            bool dissolveOnOri = !Mathf.Approximately(_Dissolve_On.floatValue, 0.0f);
            bool dissolveOnRes = EditorGUILayout.Toggle("Dissolve On", dissolveOnOri);
            _Dissolve_On.floatValue = dissolveOnRes ? 1.0f : 0.0f;
            if (dissolveOnRes) targetMat.EnableKeyword("_DISSOLVE_ON");
            else targetMat.DisableKeyword("_DISSOLVE_ON");

            materialEditor.RangeProperty(_DissolveFit, "溶解匹配");
            materialEditor.RangeProperty(_DissolveDirThreshold, "方向溶解 - 阈值");
            materialEditor.RangeProperty(_DissolveMapThreshold, "贴图溶解 - 阈值");
            materialEditor.ColorProperty(_DissolveColor, "溶解颜色");
            materialEditor.RangeProperty(_DissolveColorStrength, "溶解颜色强度");
            materialEditor.VectorProperty(_DissolveDir, "方向溶解 - 方向");
            materialEditor.RangeProperty(_DissolveDirWidth, "方向溶解 - 过渡");
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_DissolveMap), _DissolveMap);
            Vector2 scale = targetMat.GetTextureScale("_DissolveMap");
            Vector2 offset = targetMat.GetTextureOffset("_DissolveMap");
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Vector2Field("Tiling", scale);
            offset = EditorGUILayout.Vector2Field("Offset", offset);
            if (EditorGUI.EndChangeCheck())
            {
                _DissolveMap.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
            }
            materialEditor.VectorProperty(_DissolveTexFlowAndBevel, "贴图溶解 - 流动&斜切");

            materialEditor.RangeProperty(_DissolveMapEdge, "贴图溶解 - 过渡");
           
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
}