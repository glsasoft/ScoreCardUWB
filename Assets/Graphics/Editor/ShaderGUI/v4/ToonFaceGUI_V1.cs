using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ToonFaceGUI_V1 : ShaderGUI
{
    private Material targetMat;

    private MaterialProperty _CullOff;
    private MaterialProperty _Transparent;
    private MaterialProperty _StencilOutline;

    private MaterialProperty _SrcBlend;
    private MaterialProperty _DstBlend;
    private MaterialProperty _SrcBlendAlpha;
    private MaterialProperty _DstBlendAlpha;
    private MaterialProperty _ZWrite;
    private MaterialProperty _StencilOutlineValue;
    private MaterialProperty _StencilOutlinePass;

    // Base
    private MaterialProperty _BaseMap;
    private MaterialProperty _BaseColor;
    private MaterialProperty _BaseMapValue;
    private MaterialProperty _BaseMapSaturation;
    private MaterialProperty _BaseColorIntensity;
    private MaterialProperty _EmissionMap;
    private MaterialProperty _EmissionIntensity;
    private MaterialProperty _EmissionColor;
    private MaterialProperty _ambientColorStrength;
    private MaterialProperty _FresnelColor;
    private MaterialProperty _Fresnel;

    private MaterialProperty _NormalMap;

    // Rim
    private MaterialProperty _RimColor;
    private MaterialProperty _RimMin;
    private MaterialProperty _RimMax;
    private MaterialProperty _RimSmooth;
    private MaterialProperty _RimFac;

    // Outline
    private MaterialProperty _BaseOutlineColor;
    private MaterialProperty _BaseOutlineOffset;
    private MaterialProperty _StencilOutlineColor;
    private MaterialProperty _StencilOutlineOffset;

    // Ramp
    private MaterialProperty _RampMap;
    private MaterialProperty _ShadowColor;

    // SDF
    private MaterialProperty _SDFMap;

    // SSS
    private MaterialProperty _ThicknessMap;
    private MaterialProperty _ScatterNormalDistortion;
    private MaterialProperty _ScatterPower;
    private MaterialProperty _ScatterScale;
    private MaterialProperty _ScatterColor;
    private MaterialProperty _PreSSS_On;

    // Dissolve
    private MaterialProperty _Dissolve_On;
    private MaterialProperty _DissolveFit;
    private MaterialProperty _DissolveDirThreshold;
    private MaterialProperty _DissolveMapThreshold;
    private MaterialProperty _DissolveColor;
    private MaterialProperty _DissolveColorStrength;
    private MaterialProperty _DissolveDir;
    private MaterialProperty _DissolveDirWidth;
    private MaterialProperty _DissolveMap;
    private MaterialProperty _DissolveTexFlowAndBevel;
    private MaterialProperty _DissolveMapEdge;

    // SDF Head Direction
    private MaterialProperty _HeadForward;
    private MaterialProperty _HeadCenter;
    private MaterialProperty _HeadRight;

    void FindProperties(MaterialProperty[] props)
    {
        _CullOff = FindProperty("_CullOff", props);
        _Transparent = FindProperty("_Transparent", props);
        _StencilOutline = FindProperty("_StencilOutline", props);

        _SrcBlend = FindProperty("_SrcBlend", props);
        _DstBlend = FindProperty("_DstBlend", props);
        _SrcBlendAlpha = FindProperty("_SrcBlendAlpha", props);
        _DstBlendAlpha = FindProperty("_DstBlendAlpha", props);
        _ZWrite = FindProperty("_ZWrite", props);
        _StencilOutlineValue = FindProperty("_StencilOutlineValue", props);
        _StencilOutlinePass = FindProperty("_StencilOutlinePass", props);

        _BaseMap = FindProperty("_BaseMap", props);
        _BaseColor = FindProperty("_BaseColor", props);
        _BaseMapValue = FindProperty("_BaseMapValue", props);
        _BaseMapSaturation = FindProperty("_BaseMapSaturation", props);
        _BaseColorIntensity = FindProperty("_BaseColorIntensity", props);
        _EmissionMap = FindProperty("_EmissionMap", props);
        _EmissionIntensity = FindProperty("_EmissionIntensity", props);
        _EmissionColor = FindProperty("_EmissionColor", props);
        _ambientColorStrength = FindProperty("_ambientColorStrength", props);
        _FresnelColor = FindProperty("_FresnelColor", props);
        _Fresnel = FindProperty("_Fresnel", props);

        _NormalMap = FindProperty("_NormalMap", props);

        _RimColor = FindProperty("_RimColor", props);
        _RimMin = FindProperty("_RimMin", props);
        _RimMax = FindProperty("_RimMax", props);
        _RimSmooth = FindProperty("_RimSmooth", props);
        _RimFac = FindProperty("_RimFac", props);

        _BaseOutlineColor = FindProperty("_BaseOutlineColor", props);
        _BaseOutlineOffset = FindProperty("_BaseOutlineOffset", props);
        _StencilOutlineColor = FindProperty("_StencilOutlineColor", props);
        _StencilOutlineOffset = FindProperty("_StencilOutlineOffset", props);

        _RampMap = FindProperty("_RampMap", props);
        _ShadowColor = FindProperty("_ShadowColor", props);

        _SDFMap = FindProperty("_SDFMap", props);

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

        _HeadForward = FindProperty("_HeadForward", props);
        _HeadCenter = FindProperty("_HeadCenter", props);
        _HeadRight = FindProperty("_HeadRight", props);
    }

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] props)
    {
        targetMat = editor.target as Material;
        FindProperties(props);

        // === 基础属性 ===
        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
        DrawRenderingOptions();

        // === 各模块 ===
        DrawBase(editor);
        DrawRim(editor);
        DrawOutline(editor);
        DrawRamp(editor);
        DrawSDF(editor);
        DrawSSS(editor);
        DrawDissolve(editor);
    }

    void DrawRenderingOptions()
    {
        _CullOff.floatValue = EditorGUILayout.Toggle("Two Side", !Mathf.Approximately(_CullOff.floatValue, 2)) ? 0 : 2;

        bool transparent = EditorGUILayout.Toggle("Transparent", Mathf.Approximately(_Transparent.floatValue, 1));
        if (transparent)
        {
            _Transparent.floatValue = 1;
            _SrcBlend.floatValue = (float)BlendMode.SrcAlpha;
            _DstBlend.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            _SrcBlendAlpha.floatValue = 1;
            _DstBlendAlpha.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            _ZWrite.floatValue = 0;
            targetMat.renderQueue = (int)RenderQueue.Transparent;
        }
        else
        {
            _Transparent.floatValue = 0;
            _SrcBlend.floatValue = 1;
            _DstBlend.floatValue = 0;
            _SrcBlendAlpha.floatValue = 1;
            _DstBlendAlpha.floatValue = 0;
            _ZWrite.floatValue = 1;
            targetMat.renderQueue = (int)RenderQueue.Geometry;
        }

        _StencilOutline.floatValue = EditorGUILayout.Toggle("Stencil Outline", !Mathf.Approximately(_StencilOutline.floatValue, 0)) ? 1 : 0;
        if (_StencilOutline.floatValue > 0)
        {
            _StencilOutlineValue.floatValue = 1;
            _StencilOutlinePass.floatValue = 2;
        }
        else
        {
            _StencilOutlineValue.floatValue = 0;
            _StencilOutlinePass.floatValue = 0;
        }
    }

    void DrawBase(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);
        editor.TexturePropertySingleLine(new GUIContent("Albedo Map"), _BaseMap, _BaseColor);
        editor.RangeProperty(_BaseMapValue, "Base Map Value");
        editor.RangeProperty(_BaseMapSaturation, "Base Map Saturation");
        editor.RangeProperty(_BaseColorIntensity, "Base Color Intensity");
        editor.TexturePropertySingleLine(new GUIContent("Emission Map"), _EmissionMap);
        editor.ColorProperty(_EmissionColor, "Emission Color");
        editor.RangeProperty(_EmissionIntensity, "Emission Intensity");
        editor.RangeProperty(_ambientColorStrength, "Ambient Color Strength");
        editor.ColorProperty(_FresnelColor, "Fresnel Color");
        editor.RangeProperty(_Fresnel, "Fresnel");
        editor.TexturePropertySingleLine(new GUIContent("Normal Map"), _NormalMap);
    }

    void DrawRim(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rim", EditorStyles.boldLabel);
        editor.ColorProperty(_RimColor, "Rim Color");
        editor.RangeProperty(_RimMin, "Rim Min");
        editor.RangeProperty(_RimMax, "Rim Max");
        editor.RangeProperty(_RimSmooth, "Rim Smooth");
        editor.RangeProperty(_RimFac, "Rim Fac");
    }

    void DrawOutline(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);
        editor.ColorProperty(_BaseOutlineColor, "Base Outline Color");
        editor.RangeProperty(_BaseOutlineOffset, "Base Outline Offset");
        editor.ColorProperty(_StencilOutlineColor, "Stencil Outline Color");
        editor.RangeProperty(_StencilOutlineOffset, "Stencil Outline Offset");
    }

    void DrawRamp(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ramp", EditorStyles.boldLabel);
        editor.TexturePropertySingleLine(new GUIContent("Ramp Map"), _RampMap);
        editor.ColorProperty(_ShadowColor, "Shadow Color");
    }

    void DrawSDF(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("SDF", EditorStyles.boldLabel);
        editor.TexturePropertySingleLine(new GUIContent("SDF Map"), _SDFMap);
        editor.VectorProperty(_HeadForward, "Head Forward");
        editor.VectorProperty(_HeadCenter, "Head Center");
        editor.VectorProperty(_HeadRight, "Head Right");
    }

    void DrawSSS(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("SSS", EditorStyles.boldLabel);
        editor.TexturePropertySingleLine(new GUIContent("Thickness Map"), _ThicknessMap);
        editor.RangeProperty(_ScatterNormalDistortion, "Normal Distortion");
        editor.RangeProperty(_ScatterPower, "Scatter Power");
        editor.RangeProperty(_ScatterScale, "Scatter Scale");
        editor.ColorProperty(_ScatterColor, "Scatter Color");
        editor.RangeProperty(_PreSSS_On, "PreSSS On");
    }

    void DrawDissolve(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dissolve", EditorStyles.boldLabel);
        bool on = EditorGUILayout.Toggle("Dissolve On", Mathf.Approximately(_Dissolve_On.floatValue, 1));
        _Dissolve_On.floatValue = on ? 1 : 0;
        editor.RangeProperty(_DissolveFit, "Dissolve Fit");
        editor.RangeProperty(_DissolveDirThreshold, "Dir Threshold");
        editor.RangeProperty(_DissolveMapThreshold, "Map Threshold");
        editor.ColorProperty(_DissolveColor, "Dissolve Color");
        editor.RangeProperty(_DissolveColorStrength, "Color Strength");
        editor.VectorProperty(_DissolveDir, "Dissolve Dir");
        editor.RangeProperty(_DissolveDirWidth, "Dir Width");
        editor.TexturePropertySingleLine(new GUIContent("Dissolve Map"), _DissolveMap);
        editor.VectorProperty(_DissolveTexFlowAndBevel, "Flow & Bevel");
        editor.RangeProperty(_DissolveMapEdge, "Map Edge");
    }
}
