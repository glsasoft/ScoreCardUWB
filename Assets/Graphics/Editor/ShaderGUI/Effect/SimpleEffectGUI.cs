using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;
enum SimpleEffectCull
{
    Both, Front, Back
}

public class SimpleEffectGUI : ShaderGUI
{
    private Material targetMat;
    
    private MaterialProperty _CullOff;
    private MaterialProperty _AddMode;
    private MaterialProperty _ZWriteOn;
    private MaterialProperty _ZTestOn;
    private MaterialProperty _SrcBlend;
    private MaterialProperty _DstBlend;
    private MaterialProperty _SrcBlendAlpha;
    private MaterialProperty _DstBlendAlpha;

    private MaterialProperty _SoftParticleOn;
    private MaterialProperty _SceneUVOn;
    private MaterialProperty _SoftParticleFade;

    private MaterialProperty _MainTex;
    private MaterialProperty _MainColor;
    private MaterialProperty _MainColorIntensity;

    private MaterialProperty _MainTexRedIsAlphaOn;

    private MaterialProperty _MainTexRotation;
    private MaterialProperty _MainTexFlowAndBevel;

    private MaterialProperty _MainTexCustomDataOn;

    private MaterialProperty _MainTexPolarOn;
    private MaterialProperty _MainTexPolarRotation;
    private MaterialProperty _MainTexPolarRadialScale;
    private MaterialProperty _MainTexPolarLengthScale;
    private MaterialProperty _MainTexPolarCenter_U;
    private MaterialProperty _MainTexPolarCenter_V;
    private MaterialProperty _MainTexPolarRadialOffset;
    private MaterialProperty _MainTexPolarLengthOffset;
    
    private MaterialProperty _MaskTexOn_1;
    private MaterialProperty _MaskTex_1;
    private MaterialProperty _MaskTexRedIsAlphaOn_1;
    private MaterialProperty _MaskTexRotation_1;
    private MaterialProperty _MaskTexFlowAndBevel_1;
    private MaterialProperty _MaskTexCustomDataOn_1;
    private MaterialProperty _MaskTexPolarOn_1;
    private MaterialProperty _MaskTexPolarRotation_1;
    private MaterialProperty _MaskTexPolarRadialScale_1;
    private MaterialProperty _MaskTexPolarLengthScale_1;
    private MaterialProperty _MaskTexPolarCenter_U_1;
    private MaterialProperty _MaskTexPolarCenter_V_1;
    private MaterialProperty _MaskTexPolarRadialOffset_1;
    private MaterialProperty _MaskTexPolarLengthOffset_1;
    
    private MaterialProperty _MaskTexOn_2;
    private MaterialProperty _MaskTex_2;
    private MaterialProperty _MaskTexRotation_2;
    private MaterialProperty _MaskTexFlowAndBevel_2;
    private MaterialProperty _MaskTexRedIsAlphaOn_2;
    private MaterialProperty _MaskTexCustomDataOn_2;
    private MaterialProperty _MaskTexPolarOn_2;
    private MaterialProperty _MaskTexPolarRotation_2;
    private MaterialProperty _MaskTexPolarRadialScale_2;
    private MaterialProperty _MaskTexPolarLengthScale_2;
    private MaterialProperty _MaskTexPolarCenter_U_2;
    private MaterialProperty _MaskTexPolarCenter_V_2;
    private MaterialProperty _MaskTexPolarRadialOffset_2;
    private MaterialProperty _MaskTexPolarLengthOffset_2;

    private MaterialProperty _DissolveOn;
    private MaterialProperty _DissolveTex;
    private MaterialProperty _DissolveColor;
    private MaterialProperty _DissolveTexRotation;
    private MaterialProperty _DissolveTexFlowAndBevel;
    private MaterialProperty _DissolveCustomDataOn;
    private MaterialProperty _DissolveTexPolarOn;
    private MaterialProperty _DissolveTexPolarRotation;
    private MaterialProperty _DissolveTexPolarRadialScale;
    private MaterialProperty _DissolveTexPolarLengthScale;
    private MaterialProperty _DissolveTexPolarCenter_U;
    private MaterialProperty _DissolveTexPolarCenter_V;
    private MaterialProperty _DissolveTexPolarRadialOffset;
    private MaterialProperty _DissolveTexPolarLengthOffset;
    private MaterialProperty _DissolveEmissionWidth;
    private MaterialProperty _DissolveSmooth;
    private MaterialProperty _DissolveThreshold;
    
    private bool _Main = true;
    private bool _Mask = true;
    private bool _Dissolve = true;

    void FinProperties(MaterialProperty[] properties)
    {
        _CullOff = FindProperty("_CullOff", properties, true);
        _AddMode = FindProperty("_AddMode", properties, true);
        _ZWriteOn = FindProperty("_ZWriteOn", properties, true);
        _ZTestOn = FindProperty("_ZTestOn", properties, true);
        _SrcBlend = FindProperty("_SrcBlend", properties, true);
        _DstBlend = FindProperty("_DstBlend", properties, true);
        _SrcBlendAlpha = FindProperty("_SrcBlendAlpha", properties, true);
        _DstBlendAlpha = FindProperty("_DstBlendAlpha", properties, true);
        
        _SoftParticleOn = FindProperty("_SoftParticleOn", properties, true);
        _SceneUVOn = FindProperty("_SceneUVOn", properties, true);
        
        _SoftParticleFade = FindProperty("_SoftParticleFade", properties, true);
        
        _MainTex = FindProperty("_MainTex", properties, true);
        _MainColor = FindProperty("_MainColor", properties, true);
        _MainColorIntensity = FindProperty("_MainColorIntensity", properties, true);
        
        _MainTexRedIsAlphaOn = FindProperty("_MainTexRedIsAlphaOn", properties, true);
        
        _MainTexRotation = FindProperty("_MainTexRotation", properties, true);
        _MainTexFlowAndBevel = FindProperty("_MainTexFlowAndBevel", properties, true);
        
        _MainTexCustomDataOn = FindProperty("_MainTexCustomDataOn", properties, true);
        
        _MainTexPolarOn = FindProperty("_MainTexPolarOn", properties, true);
        _MainTexPolarRotation = FindProperty("_MainTexPolarRotation", properties, true);
        _MainTexPolarRadialScale = FindProperty("_MainTexPolarRadialScale", properties, true);
        _MainTexPolarLengthScale = FindProperty("_MainTexPolarLengthScale", properties, true);
        _MainTexPolarCenter_U = FindProperty("_MainTexPolarCenter_U", properties, true);
        _MainTexPolarCenter_V = FindProperty("_MainTexPolarCenter_V", properties, true);
        _MainTexPolarRadialOffset = FindProperty("_MainTexPolarRadialOffset", properties, true);
        _MainTexPolarLengthOffset = FindProperty("_MainTexPolarLengthOffset", properties, true);
        
        _MaskTexOn_1 = FindProperty("_MaskTexOn_1", properties, true);
        _MaskTex_1 = FindProperty("_MaskTex_1", properties, true);
        
        _MaskTexRotation_1 = FindProperty("_MaskTexRotation_1", properties, true);
        _MaskTexFlowAndBevel_1 = FindProperty("_MaskTexFlowAndBevel_1", properties, true);
        
        _MaskTexRedIsAlphaOn_1 = FindProperty("_MaskTexRedIsAlphaOn_1", properties, true);
        _MaskTexCustomDataOn_1 = FindProperty("_MaskTexCustomDataOn_1", properties, true);
        
        _MaskTexPolarOn_1 = FindProperty("_MaskTexPolarOn_1", properties, true);
        _MaskTexPolarRotation_1 = FindProperty("_MaskTexPolarRotation_1", properties, true);
        _MaskTexPolarRadialScale_1 = FindProperty("_MaskTexPolarRadialScale_1", properties, true);
        _MaskTexPolarLengthScale_1 = FindProperty("_MaskTexPolarLengthScale_1", properties, true);
        _MaskTexPolarCenter_U_1 = FindProperty("_MaskTexPolarCenter_U_1", properties, true);
        _MaskTexPolarCenter_V_1 = FindProperty("_MaskTexPolarCenter_V_1", properties, true);
        _MaskTexPolarRadialOffset_1 = FindProperty("_MaskTexPolarRadialOffset_1", properties, true);
        _MaskTexPolarLengthOffset_1 = FindProperty("_MaskTexPolarLengthOffset_1", properties, true);
        
        _MaskTexOn_2 = FindProperty("_MaskTexOn_2", properties, true);
        _MaskTex_2 = FindProperty("_MaskTex_2", properties, true);
        _MaskTexRotation_2 = FindProperty("_MaskTexRotation_2", properties, true);
        _MaskTexFlowAndBevel_2 = FindProperty("_MaskTexFlowAndBevel_2", properties, true);
        
        _MaskTexRedIsAlphaOn_2 = FindProperty("_MaskTexRedIsAlphaOn_2", properties, true);
        _MaskTexCustomDataOn_2 = FindProperty("_MaskTexCustomDataOn_2", properties, true);
        
        _MaskTexPolarOn_2 = FindProperty("_MaskTexPolarOn_2", properties, true);
        _MaskTexPolarRotation_2 = FindProperty("_MaskTexPolarRotation_2", properties, true);
        _MaskTexPolarRadialScale_2 = FindProperty("_MaskTexPolarRadialScale_2", properties, true);
        _MaskTexPolarLengthScale_2 = FindProperty("_MaskTexPolarLengthScale_2", properties, true);
        _MaskTexPolarCenter_U_2 = FindProperty("_MaskTexPolarCenter_U_2", properties, true);
        _MaskTexPolarCenter_V_2 = FindProperty("_MaskTexPolarCenter_V_2", properties, true);
        _MaskTexPolarRadialOffset_2 = FindProperty("_MaskTexPolarRadialOffset_2", properties, true);
        _MaskTexPolarLengthOffset_2 = FindProperty("_MaskTexPolarLengthOffset_2", properties, true);
        
        _DissolveOn = FindProperty("_DissolveOn", properties, true);
        _DissolveTex = FindProperty("_DissolveTex", properties, true);
        _DissolveTexRotation = FindProperty("_DissolveTexRotation", properties, true);
        _DissolveTexFlowAndBevel = FindProperty("_DissolveTexFlowAndBevel", properties, true);
        _DissolveColor = FindProperty("_DissolveColor", properties, true);
        
        _DissolveCustomDataOn = FindProperty("_DissolveCustomDataOn", properties, true);
        
        _DissolveTexPolarOn = FindProperty("_DissolveTexPolarOn", properties, true);
        _DissolveTexPolarRotation = FindProperty("_DissolveTexPolarRotation", properties, true);
        _DissolveTexPolarRadialScale = FindProperty("_DissolveTexPolarRadialScale", properties, true);
        _DissolveTexPolarLengthScale = FindProperty("_DissolveTexPolarLengthScale", properties, true);
        _DissolveTexPolarCenter_U = FindProperty("_DissolveTexPolarCenter_U", properties, true);
        _DissolveTexPolarCenter_V = FindProperty("_DissolveTexPolarCenter_V", properties, true);
        _DissolveTexPolarRadialOffset = FindProperty("_DissolveTexPolarRadialOffset", properties, true);
        _DissolveTexPolarLengthOffset = FindProperty("_DissolveTexPolarLengthOffset", properties, true);
        
        _DissolveEmissionWidth = FindProperty("_DissolveEmissionWidth", properties, true);
        _DissolveSmooth = FindProperty("_DissolveSmooth", properties, true);
        _DissolveThreshold = FindProperty("_DissolveThreshold", properties, true);
    }
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        targetMat = materialEditor.target as Material;
        FinProperties(properties);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Main = ShaderGUI_Helper.Foldout(_Main, "主贴图");
        if (_Main)
        {
            EditorGUI.indentLevel++;
            // bool cullOffOri = !Equals(_CullOff.floatValue, 2.0f);
            // bool cullOffRes = EditorGUILayout.Toggle("双面", cullOffOri);
            // _CullOff.floatValue = cullOffRes ? 0.0f : 2.0f;

            SimpleEffectCull cullOri = (SimpleEffectCull)_CullOff.floatValue;
            SimpleEffectCull cullRes = (SimpleEffectCull)EditorGUILayout.EnumPopup("剔除模式", cullOri);
            _CullOff.floatValue = (float)cullRes;
            
            bool zWriteOri = Equals(_ZWriteOn.floatValue, 1.0f);
            bool zWriteRes = EditorGUILayout.Toggle("是否写入深度", zWriteOri);
            _ZWriteOn.floatValue = zWriteRes ? 1.0f : 0.0f;
            
            bool zTestOri = Equals(_ZTestOn.floatValue, 2.0f);
            bool zTestRes = EditorGUILayout.Toggle("开启深度测试", zTestOri);
            _ZTestOn.floatValue = zTestRes ? 2.0f : 0.0f;
            
            bool addModeOri = Equals(_AddMode.floatValue, 1.0f);
            bool addModeRes = EditorGUILayout.Toggle("相加模式", addModeOri);
            if (addModeRes)
            {
                _AddMode.floatValue = 1.0f;
                _SrcBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                _DstBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                _SrcBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                _DstBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
            }
            else
            {
                _AddMode.floatValue = 0.0f;
                _SrcBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                _DstBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                _SrcBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                _DstBlendAlpha.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
            }
            bool sceneUVOnOri = Equals(_SceneUVOn.floatValue, 1.0f);
            bool sceneUVOn = EditorGUILayout.Toggle("启用屏幕UV", sceneUVOnOri);
            _SceneUVOn.floatValue = sceneUVOn ? 1.0f : 0.0f;
            
            bool softParticleOnOri = Equals(_SoftParticleOn.floatValue, 1.0f);
            bool softParticleOnRes = EditorGUILayout.Toggle("启用软粒子", softParticleOnOri);
            if (softParticleOnRes)
            {
                EditorGUI.indentLevel++;
                _SoftParticleOn.floatValue = 1.0f;
                materialEditor.RangeProperty(_SoftParticleFade, "软粒子过渡");
                targetMat.EnableKeyword("_SOFT_PARTICLE_ON");
                EditorGUI.indentLevel--;
            }
            else
            {
                _SoftParticleOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_SOFT_PARTICLE_ON");
            }
            
            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_MainTex), _MainTex);
            
            Vector2 scale = targetMat.GetTextureScale("_MainTex");
            Vector2 offset = targetMat.GetTextureOffset("_MainTex");
            EditorGUI.BeginChangeCheck();
            scale = EditorGUILayout.Vector2Field("Tiling", scale);
            offset = EditorGUILayout.Vector2Field("Offset", offset);
            if (EditorGUI.EndChangeCheck())
                _MainTex.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
            
            bool mainTexRedIsAlphaOnOri = Equals(_MainTexRedIsAlphaOn.floatValue, 1.0f);
            bool mainTexRedIsAlphaOnRes = EditorGUILayout.Toggle("R是透明通道", mainTexRedIsAlphaOnOri);
            if (mainTexRedIsAlphaOnRes) _MainTexRedIsAlphaOn.floatValue = 1.0f;
            else _MainTexRedIsAlphaOn.floatValue = 0.0f;
                
            materialEditor.ColorProperty(_MainColor, "颜色");
            materialEditor.FloatProperty(_MainColorIntensity, "颜色强度");
            materialEditor.RangeProperty(_MainTexRotation, "主贴图旋转");
            materialEditor.VectorProperty(_MainTexFlowAndBevel, "主贴图 流动 & 斜切");
            
            bool mainTexCustomDataOnOri = Equals(_MainTexCustomDataOn.floatValue, 1.0f);
            bool mainTexCustomDataOnRes = EditorGUILayout.Toggle("主贴图启用自定义数据", mainTexCustomDataOnOri);
            if (mainTexCustomDataOnRes) _MainTexCustomDataOn.floatValue = 1.0f;
            else _MainTexCustomDataOn.floatValue = 0.0f;
            
            bool mainTexPolarOnOri = Equals(_MainTexPolarOn.floatValue, 1.0f);
            bool mainTexPolarOnRes = EditorGUILayout.Toggle("主贴图启用极坐标UV", mainTexPolarOnOri);
            if (mainTexPolarOnRes)
            {
                EditorGUI.indentLevel++;
                _MainTexPolarOn.floatValue = 1.0f;
                materialEditor.RangeProperty(_MainTexPolarRotation, "主贴图极坐标 旋转");
                materialEditor.FloatProperty(_MainTexPolarRadialScale, "主贴图极坐标半径 缩放");
                materialEditor.FloatProperty(_MainTexPolarLengthScale, "主贴图极坐标长度 缩放");
                Vector2 polarCenter = new Vector2(_MainTexPolarCenter_U.floatValue, _MainTexPolarCenter_V.floatValue);
                EditorGUI.BeginChangeCheck();
                polarCenter = EditorGUILayout.Vector2Field("主贴图极坐标中心", polarCenter);
                if (EditorGUI.EndChangeCheck())
                {
                    _MainTexPolarCenter_U.floatValue = polarCenter.x;
                    _MainTexPolarCenter_V.floatValue = polarCenter.y;
                }
                materialEditor.FloatProperty(_MainTexPolarRadialOffset, "主贴图极坐标半径 偏移");
                materialEditor.FloatProperty(_MainTexPolarLengthOffset, "主贴图极坐标长度 偏移");
                targetMat.EnableKeyword("_POLAR_ON");
                EditorGUI.indentLevel--;
            }
            else
            {
                _MainTexPolarOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_POLAR_ON");
            }
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
                
                bool maskTexPolarOnOri_1 = Equals(_MaskTexPolarOn_1.floatValue, 1.0f);
                bool maskTexPolarOnRes_1 = EditorGUILayout.Toggle("遮罩图 1 启用极坐标UV", maskTexPolarOnOri_1);
                if (maskTexPolarOnRes_1)
                {
                    EditorGUI.indentLevel++;
                    _MaskTexPolarOn_1.floatValue = 1.0f;
                    materialEditor.RangeProperty(_MaskTexPolarRotation_1, "遮罩图 1 极坐标 旋转");
                    materialEditor.FloatProperty(_MaskTexPolarRadialScale_1, "遮罩图 1 极坐标半径 缩放");
                    materialEditor.FloatProperty(_MaskTexPolarLengthScale_1, "遮罩图 1 极坐标长度 缩放");
                    Vector2 polarCenter = new Vector2(_MaskTexPolarCenter_U_1.floatValue, _MaskTexPolarCenter_V_1.floatValue);
                    EditorGUI.BeginChangeCheck();
                    polarCenter = EditorGUILayout.Vector2Field("遮罩图 1 极坐标中心", polarCenter);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _MaskTexPolarCenter_U_1.floatValue = polarCenter.x;
                        _MaskTexPolarCenter_V_1.floatValue = polarCenter.y;
                    }
                    materialEditor.FloatProperty(_MaskTexPolarRadialOffset_1, "遮罩图 1 极坐标半径 偏移");
                    materialEditor.FloatProperty(_MaskTexPolarLengthOffset_1, "遮罩图 1 极坐标长度 偏移");
                    targetMat.EnableKeyword("_MASK_TEX_POLAR_ON_1");
                    EditorGUI.indentLevel--;
                }
                else
                {
                    _MaskTexPolarOn_1.floatValue = 0.0f;
                    targetMat.DisableKeyword("_MASK_TEX_POLAR_ON_1");
                }
                targetMat.EnableKeyword("_MASK_TEX_ON_1");
                EditorGUI.indentLevel--;
            }
            else
            {
                _MaskTexOn_1.floatValue = 0.0f;
                targetMat.DisableKeyword("_MASK_TEX_ON_1");
                targetMat.DisableKeyword("_MASK_TEX_POLAR_ON_1");
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
                
                bool maskTexPolarOnOri_2 = Equals(_MaskTexPolarOn_2.floatValue, 1.0f);
                bool maskTexPolarOnRes_2 = EditorGUILayout.Toggle("遮罩图 2 启用极坐标UV", maskTexPolarOnOri_2);
                if (maskTexPolarOnRes_2)
                {
                    EditorGUI.indentLevel++;
                    _MaskTexPolarOn_2.floatValue = 1.0f;
                    materialEditor.RangeProperty(_MaskTexPolarRotation_2, "遮罩图 2 极坐标 旋转");
                    materialEditor.FloatProperty(_MaskTexPolarRadialScale_2, "遮罩图 2 极坐标半径 缩放");
                    materialEditor.FloatProperty(_MaskTexPolarLengthScale_2, "遮罩图 2 极坐标长度 缩放");
                    Vector2 polarCenter = new Vector2(_MaskTexPolarCenter_U_2.floatValue, _MaskTexPolarCenter_V_2.floatValue);
                    EditorGUI.BeginChangeCheck();
                    polarCenter = EditorGUILayout.Vector2Field("遮罩图 2 极坐标中心", polarCenter);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _MaskTexPolarCenter_U_2.floatValue = polarCenter.x;
                        _MaskTexPolarCenter_V_2.floatValue = polarCenter.y;
                    }
                    materialEditor.FloatProperty(_MaskTexPolarRadialOffset_2, "遮罩图 2 极坐标半径 偏移");
                    materialEditor.FloatProperty(_MaskTexPolarLengthOffset_2, "遮罩图 2 极坐标长度 偏移");
                    targetMat.EnableKeyword("_MASK_TEX_POLAR_ON_2");
                    EditorGUI.indentLevel--;
                }
                else
                {
                    _MaskTexPolarOn_2.floatValue = 0.0f;
                    targetMat.DisableKeyword("_MASK_TEX_POLAR_ON_2");
                }
                targetMat.EnableKeyword("_MASK_TEX_ON_2");
                EditorGUI.indentLevel--;
            }
            else
            {
                _MaskTexOn_2.floatValue = 0.0f;
                targetMat.DisableKeyword("_MASK_TEX_ON_2");
                targetMat.DisableKeyword("_MASK_TEX_POLAR_ON_2");
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Dissolve = ShaderGUI_Helper.Foldout(_Dissolve, "溶解");
        if (_Dissolve)
        {
            EditorGUI.indentLevel++;
            bool dissolveTexOnOri = Equals(_DissolveOn.floatValue, 1.0f);
            bool dissolveTexOnRes = EditorGUILayout.Toggle("启用溶解", dissolveTexOnOri);
            if (dissolveTexOnRes)
            {
                _DissolveOn.floatValue = 1.0f;
                materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_DissolveTex), _DissolveTex);
                
                Vector2 scale = targetMat.GetTextureScale("_DissolveTex");
                Vector2 offset = targetMat.GetTextureOffset("_DissolveTex");
                EditorGUI.BeginChangeCheck();
                scale = EditorGUILayout.Vector2Field("Tiling", scale);
                offset = EditorGUILayout.Vector2Field("Offset", offset);
                if (EditorGUI.EndChangeCheck())
                    _DissolveTex.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                
                materialEditor.VectorProperty(_DissolveTexFlowAndBevel, "溶解图 流动 & 斜切");
                materialEditor.RangeProperty(_DissolveTexRotation, "溶解图 旋转");
                
                materialEditor.ColorProperty(_DissolveColor, "溶解颜色");
                
                bool dissolveCustomDataOnOri = Equals(_DissolveCustomDataOn.floatValue, 1.0f);
                bool dissolveCustomDataOnRes = EditorGUILayout.Toggle("溶解 启用自定义数据", dissolveCustomDataOnOri);
                if (dissolveCustomDataOnRes) _DissolveCustomDataOn.floatValue = 1.0f;
                else _DissolveCustomDataOn.floatValue = 0.0f;
                
                bool dissolveTexPolarOnOri = Equals(_DissolveTexPolarOn.floatValue, 1.0f);
                bool dissolveTexPolarOnRes = EditorGUILayout.Toggle("溶解图 启用极坐标UV", dissolveTexPolarOnOri);
                if (dissolveTexPolarOnRes)
                {
                    EditorGUI.indentLevel++;
                    _DissolveTexPolarOn.floatValue = 1.0f;

                    materialEditor.RangeProperty(_DissolveTexPolarRotation, "溶解图 极坐标 旋转");
                    materialEditor.FloatProperty(_DissolveTexPolarRadialScale, "溶解图 极坐标半径 缩放");
                    materialEditor.FloatProperty(_DissolveTexPolarLengthScale, "溶解图 极坐标长度 缩放");
                    Vector2 polarCenter = new Vector2(_DissolveTexPolarCenter_U.floatValue, _DissolveTexPolarCenter_V.floatValue);
                    EditorGUI.BeginChangeCheck();
                    polarCenter = EditorGUILayout.Vector2Field("溶解图 极坐标中心", polarCenter);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _DissolveTexPolarCenter_U.floatValue = polarCenter.x;
                        _DissolveTexPolarCenter_V.floatValue = polarCenter.y;
                    }
                    materialEditor.FloatProperty(_DissolveTexPolarRadialOffset, "溶解图 极坐标半径 偏移");
                    materialEditor.FloatProperty(_DissolveTexPolarLengthOffset, "溶解图 极坐标长度 偏移");
                    
                    targetMat.EnableKeyword("_DISSOLVE_POLAR_ON");
                    EditorGUI.indentLevel--;
                }
                else
                {
                    _DissolveTexPolarOn.floatValue = 0.0f;
                    targetMat.DisableKeyword("_DISSOLVE_POLAR_ON");
                }

                materialEditor.RangeProperty(_DissolveEmissionWidth, "亮边宽度");
                materialEditor.RangeProperty(_DissolveSmooth, "软化程度");
                materialEditor.RangeProperty(_DissolveThreshold, "溶解程度");
                
                targetMat.EnableKeyword("_DISSOLVE_ON");
            }
            else
            {
                _DissolveOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_DISSOLVE_ON");
                targetMat.DisableKeyword("_DISSOLVE_POLAR_ON");
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        materialEditor.RenderQueueField();
    }
}