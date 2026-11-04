using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class PBRCharacterGUI_V5 : ShaderGUI
{
    private Material targetMat;

    // 标准基础属性
    private MaterialProperty _CullOff, _Transparent, _Unlit, _SrcBlend, _DstBlend, _SrcBlendAlpha, _DstBlendAlpha, _ZWrite;

    // BaseColor
    private MaterialProperty _BaseMap, _BaseColor, _BaseMapValue, _BaseMapSaturation, _BaseColorIntensity;
    private MaterialProperty _EmissionMap, _EmissionColor, _EmissionIntensity;
    private MaterialProperty _FresnelColor, _Fresnel;

    // Normal
    private MaterialProperty _NormalMap;

    // Metallic / Roughness / AO
    private MaterialProperty _MaskMap, _Metallic, _Roughness;

    // Rim
    private MaterialProperty _RimColor, _RimMin, _RimMax, _RimSmooth, _RimFac;

    // Outline
    private MaterialProperty _BaseOutlineColor, _BaseOutlineOffset;
    private MaterialProperty _StencilOutlineColor, _StencilOutlineOffset;
    private MaterialProperty _StencilOutline_On, _CullOutline;

    // Ramp
    private MaterialProperty _RampMap, _ShadowColor;
    
    // Aniso
    private MaterialProperty _AnisoTint, _RoughnessX, _RoughnessY, _IOR;

    // SSS
    private MaterialProperty _ThicknessMap, _ScatterNormalDistortion, _ScatterPower, _ScatterScale, _ScatterColor;
    private MaterialProperty _PreSSS_On;

    // Dissolve
    private MaterialProperty _Dissolve_On, _DissolveFit, _DissolveDirThreshold, _DissolveMapThreshold;
    private MaterialProperty _DissolveColor, _DissolveColorStrength;
    private MaterialProperty _DissolveDir, _DissolveDirWidth;
    private MaterialProperty _DissolveMap, _DissolveTexFlowAndBevel, _DissolveMapEdge;

    // Overall Color
    private MaterialProperty _OverallColor, _OverallColorStrength, _OverallColorExtent;

    
    private bool _Base = false, _Outline = false, _Rim = false, _Ramp = false, _SSS = false, _Aniso = false, _Dissolve = false, _Overall = false;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        targetMat = materialEditor.target as Material;
        FindProps(props);

        Section_Base(materialEditor);
        Section_Outline(materialEditor);
        Section_Rim(materialEditor);
        Section_Ramp(materialEditor);
        Section_SSS(materialEditor);
        Section_Aniso(materialEditor);
        Section_Dissolve(materialEditor);
        Section_Overall(materialEditor);
    }

    ///////////////////////
    // Sections
    ///////////////////////
    void Section_Base(MaterialEditor materialEditor)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Base = ShaderGUI_Helper.Foldout(_Base, "Base");
        if (_Base)
        {
            EditorGUI.indentLevel++;
            BoolToggle("Two Side", _CullOff, 2.0f, 0.0f);
            BoolToggle("Cull Outline", _CullOutline, 1.0f, 0.0f);
            bool trans = FloatToggle("Transparent", _Transparent);
            SetupBlendMode(trans);
            FloatToggle("Unlit", _Unlit);

            materialEditor.TexturePropertySingleLine(new GUIContent("Albedo Map"), _BaseMap);
            materialEditor.ColorProperty(_BaseColor, "Base Color");
            materialEditor.RangeProperty(_BaseMapValue, "Base Map Value");
            materialEditor.RangeProperty(_BaseMapSaturation, "Base Map Saturation");
            materialEditor.RangeProperty(_BaseColorIntensity, "Base Color Intensity");

            materialEditor.TexturePropertySingleLine(new GUIContent("Emission Map"), _EmissionMap);
            materialEditor.RangeProperty(_EmissionIntensity, "Emission Intensity");
            materialEditor.ColorProperty(_EmissionColor, "Emission Color");

            materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), _NormalMap);
            materialEditor.TexturePropertySingleLine(new GUIContent("Mask Map"), _MaskMap);
            materialEditor.RangeProperty(_Metallic, "Metallic");
            materialEditor.RangeProperty(_Roughness, "Roughness");

            materialEditor.ColorProperty(_FresnelColor, "Fresnel Color");
            materialEditor.RangeProperty(_Fresnel, "Fresnel");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
    
    void Section_Outline(MaterialEditor materialEditor)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Outline = ShaderGUI_Helper.Foldout(_Outline, "Outline");
        if (_Outline)
        {
            EditorGUI.indentLevel++;
            materialEditor.ColorProperty(_BaseOutlineColor, "Base Outline Color");
            materialEditor.RangeProperty(_BaseOutlineOffset, "Base Outline Offset");
            FloatToggle("_StencilOutline_On", _StencilOutline_On);
            materialEditor.ColorProperty(_StencilOutlineColor, "Stencil Outline Color");
            materialEditor.RangeProperty(_StencilOutlineOffset, "Stencil Outline Offset");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
    
    void Section_Rim(MaterialEditor materialEditor)
    {
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
    }
    
    void Section_Ramp(MaterialEditor materialEditor)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Ramp = ShaderGUI_Helper.Foldout(_Ramp, "Ramp");
        if (_Ramp)
        {
            EditorGUI.indentLevel++;
            materialEditor.TexturePropertySingleLine(new GUIContent("Ramp Map"), _RampMap);
            materialEditor.ColorProperty(_ShadowColor, "Shadow Color");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
    
    void Section_SSS(MaterialEditor materialEditor)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _SSS = ShaderGUI_Helper.Foldout(_SSS, "SSS");
        if (_SSS)
        {
            EditorGUI.indentLevel++;
            FloatToggle("SSS Enabled", _PreSSS_On);
            materialEditor.TexturePropertySingleLine(new GUIContent("Thickness Map"), _ThicknessMap);
            materialEditor.RangeProperty(_ScatterNormalDistortion, "Normal Distortion");
            materialEditor.RangeProperty(_ScatterPower, "Scatter Power");
            materialEditor.RangeProperty(_ScatterScale, "Scatter Scale");
            materialEditor.ColorProperty(_ScatterColor, "Scatter Color");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
    
    void Section_Aniso(MaterialEditor materialEditor)
    {
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
    }
    
    void Section_Dissolve(MaterialEditor materialEditor)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Dissolve = ShaderGUI_Helper.Foldout(_Dissolve, "Dissolve");
        if (_Dissolve)
        {
            EditorGUI.indentLevel++;
            FloatToggle("Dissolve On", _Dissolve_On);
            materialEditor.RangeProperty(_DissolveFit, "Dissolve Fit");
            materialEditor.RangeProperty(_DissolveDirThreshold, "Dir Threshold");
            materialEditor.RangeProperty(_DissolveMapThreshold, "Map Threshold");
            materialEditor.ColorProperty(_DissolveColor, "Dissolve Color");
            materialEditor.RangeProperty(_DissolveColorStrength, "Color Strength");
            materialEditor.VectorProperty(_DissolveDir, "Dissolve Dir");
            materialEditor.RangeProperty(_DissolveDirWidth, "Dir Width");
            materialEditor.TexturePropertySingleLine(new GUIContent("Dissolve Map"), _DissolveMap);
            materialEditor.VectorProperty(_DissolveTexFlowAndBevel, "Flow & Bevel");
            materialEditor.RangeProperty(_DissolveMapEdge, "Map Edge");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
    
    void Section_Overall(MaterialEditor materialEditor)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Overall = ShaderGUI_Helper.Foldout(_Overall, "Overall Color");
        if (_Overall)
        {
            EditorGUI.indentLevel++;
            materialEditor.ColorProperty(_OverallColor, "Overall Color");
            materialEditor.FloatProperty(_OverallColorStrength, "Color Strength");
            materialEditor.FloatProperty(_OverallColorExtent, "Color Extent");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    
    ///////////////////////
    // 小工具
    ///////////////////////
    
    // Float开关
    private bool FloatToggle(string label, MaterialProperty prop)
    {
        bool ori = !Mathf.Approximately(prop.floatValue, 0.0f);
        bool result = EditorGUILayout.Toggle(label, ori);
        prop.floatValue = result ? 1.0f : 0.0f;
        return result;
    }

    // Bool开关
    private void BoolToggle(string label, MaterialProperty prop, float trueVal, float falseVal)
    {
        bool ori = !Mathf.Approximately(prop.floatValue, trueVal);
        bool result = EditorGUILayout.Toggle(label, ori);
        prop.floatValue = result ? falseVal : trueVal;
    }

    // 设置混合模式
    private void SetupBlendMode(bool transparent)
    {
        if (transparent)
        {
            _SrcBlend.floatValue = (float)BlendMode.SrcAlpha;
            _DstBlend.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            _SrcBlendAlpha.floatValue = (float)BlendMode.One;
            _DstBlendAlpha.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            _ZWrite.floatValue = 0.0f;
            targetMat.renderQueue = (int)RenderQueue.Transparent;
        }
        else
        {
            _SrcBlend.floatValue = (float)BlendMode.One;
            _DstBlend.floatValue = (float)BlendMode.Zero;
            _SrcBlendAlpha.floatValue = (float)BlendMode.One;
            _DstBlendAlpha.floatValue = (float)BlendMode.Zero;
            _ZWrite.floatValue = 1.0f;
            targetMat.renderQueue = (int)RenderQueue.Geometry;
        }
    }
    
    // 参数注册
    private void FindProps(MaterialProperty[] props)
    {
        _CullOff = FindProperty("_CullOff", props);
        _Transparent = FindProperty("_Transparent", props);
        _Unlit = FindProperty("_Unlit", props);
        _SrcBlend = FindProperty("_SrcBlend", props);
        _DstBlend = FindProperty("_DstBlend", props);
        _SrcBlendAlpha = FindProperty("_SrcBlendAlpha", props);
        _DstBlendAlpha = FindProperty("_DstBlendAlpha", props);
        _ZWrite = FindProperty("_ZWrite", props);

        _BaseMap = FindProperty("_BaseMap", props);
        _BaseColor = FindProperty("_BaseColor", props);
        _BaseMapValue = FindProperty("_BaseMapValue", props);
        _BaseMapSaturation = FindProperty("_BaseMapSaturation", props);
        _BaseColorIntensity = FindProperty("_BaseColorIntensity", props);

        _EmissionMap = FindProperty("_EmissionMap", props);
        _EmissionColor = FindProperty("_EmissionColor", props);
        _EmissionIntensity = FindProperty("_EmissionIntensity", props);

        _FresnelColor = FindProperty("_FresnelColor", props);
        _Fresnel = FindProperty("_Fresnel", props);

        _NormalMap = FindProperty("_NormalMap", props);
        _MaskMap = FindProperty("_MaskMap", props);
        _Metallic = FindProperty("_Metallic", props);
        _Roughness = FindProperty("_Roughness", props);

        _RimColor = FindProperty("_RimColor", props);
        _RimMin = FindProperty("_RimMin", props);
        _RimMax = FindProperty("_RimMax", props);
        _RimSmooth = FindProperty("_RimSmooth", props);
        _RimFac = FindProperty("_RimFac", props);

        _BaseOutlineColor = FindProperty("_BaseOutlineColor", props);
        _BaseOutlineOffset = FindProperty("_BaseOutlineOffset", props);
        _StencilOutlineColor = FindProperty("_StencilOutlineColor", props);
        _StencilOutlineOffset = FindProperty("_StencilOutlineOffset", props);
        _StencilOutline_On = FindProperty("_StencilOutline_On", props);
        _CullOutline = FindProperty("_CullOutline", props);

        _RampMap = FindProperty("_RampMap", props);
        _ShadowColor = FindProperty("_ShadowColor", props);

        _AnisoTint = FindProperty("_AnisoTint", props);
        _RoughnessX = FindProperty("_RoughnessX", props);
        _RoughnessY = FindProperty("_RoughnessY", props);
        _IOR = FindProperty("_IOR", props);

        _ThicknessMap = FindProperty("_ThicknessMap", props);
        _ScatterNormalDistortion = FindProperty("_ScatterNormalDistortion", props);
        _ScatterPower = FindProperty("_ScatterPower", props);
        _ScatterScale = FindProperty("_ScatterScale", props);
        _ScatterColor = FindProperty("_ScatterColor", props);
        _PreSSS_On = FindProperty("_PreSSS_On", props);

        _Dissolve_On = FindProperty("_Dissolve_On", props);
        _DissolveFit = FindProperty("_DissolveFit", props);
        _DissolveDirThreshold = FindProperty("_DissolveDirThreshold", props);
        _DissolveMapThreshold = FindProperty("_DissolveMapThreshold", props);
        _DissolveColor = FindProperty("_DissolveColor", props);
        _DissolveColorStrength = FindProperty("_DissolveColorStrength", props);
        _DissolveDir = FindProperty("_DissolveDir", props);
        _DissolveDirWidth = FindProperty("_DissolveDirWidth", props);
        _DissolveMap = FindProperty("_DissolveMap", props);
        _DissolveTexFlowAndBevel = FindProperty("_DissolveTexFlowAndBevel", props);
        _DissolveMapEdge = FindProperty("_DissolveMapEdge", props);

        _OverallColor = FindProperty("_OverallColor", props);
        _OverallColorStrength = FindProperty("_OverallColorStrength", props);
        _OverallColorExtent = FindProperty("_OverallColorExtent", props);
    }
}
