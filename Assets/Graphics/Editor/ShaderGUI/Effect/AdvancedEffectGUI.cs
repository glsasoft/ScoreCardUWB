using UnityEngine;
using UnityEditor;
enum AdvancedEffectCull
{
    Both, Front, Back
}

public class AdvancedEffectGUI : ShaderGUI
{
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int MaskTex1 = Shader.PropertyToID("_MaskTex_1");
    private static readonly int MaskTex2 = Shader.PropertyToID("_MaskTex_2");
    private static readonly int TurbulenceTex = Shader.PropertyToID("_TurbulenceTex");
    private static readonly int DissolveTex = Shader.PropertyToID("_DissolveTex");
    private static readonly int DissolveTex2 = Shader.PropertyToID("_DissolveTex2");
    private static readonly int VertexOffsetMaskTex = Shader.PropertyToID("_VertexOffsetMaskTex");
    private Material targetMat;
    
    private MaterialProperty _CullOff;
    private MaterialProperty _AddMode;
    private MaterialProperty _ZWriteOn;
    private MaterialProperty _SrcBlend;
    private MaterialProperty _DstBlend;
    private MaterialProperty _SrcBlendAlpha;
    private MaterialProperty _DstBlendAlpha;
    private MaterialProperty _ZWrite;

    private MaterialProperty _SoftParticleOn;
    private MaterialProperty _SoftParticleFade;

    private MaterialProperty _MainTex;
    private MaterialProperty _MainColor;
    private MaterialProperty _MainColorIntensity;

    private MaterialProperty _MainTexRedIsAlphaOn;

    private MaterialProperty _MainTexRotation;
    private MaterialProperty _MainTexFlowAndBevel;

    private MaterialProperty _MainTexCustomDataOn_TillingX;
    private MaterialProperty _MainTexCustomDataOn_TillingY;

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
    private MaterialProperty _MaskTexCustomDataOn_1_TillingX;
    private MaterialProperty _MaskTexCustomDataOn_1_TillingY;
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
    private MaterialProperty _MaskTexCustomDataOn_2_TillingX;
    private MaterialProperty _MaskTexCustomDataOn_2_TillingY;
    private MaterialProperty _MaskTexCustomDataOn_2_OffsetX;
    private MaterialProperty _MaskTexPolarOn_2;
    private MaterialProperty _MaskTexPolarRotation_2;
    private MaterialProperty _MaskTexPolarRadialScale_2;
    private MaterialProperty _MaskTexPolarLengthScale_2;
    private MaterialProperty _MaskTexPolarCenter_U_2;
    private MaterialProperty _MaskTexPolarCenter_V_2;
    private MaterialProperty _MaskTexPolarRadialOffset_2;
    private MaterialProperty _MaskTexPolarLengthOffset_2;

    private MaterialProperty _TurbulenceOn;
    private MaterialProperty _TurbulenceTex;
    private MaterialProperty _TurbulenceFlow;
    private MaterialProperty _TurbulenceBasePoint;
    private MaterialProperty _TurbulenceStrength;
    private MaterialProperty _TurbulenceMask;
    private MaterialProperty _TurbulenceMask2;
    
    private MaterialProperty _DissolveOn;
    private MaterialProperty _DissolveTex;
    private MaterialProperty _DissolveColor;
    private MaterialProperty _DissolveTexRotation;
    private MaterialProperty _DissolveTexFlowAndBevel;

    private MaterialProperty _DissolveTex2On;
    private MaterialProperty _DissolveTex2;
    private MaterialProperty _DissolveTex2Rotation;
    private MaterialProperty _DissolveTex2Strength;
    private MaterialProperty _DissolveTex2Blend;
    private MaterialProperty _DissolveTex2Width;
    private MaterialProperty _DissolveTex2Offset;
    

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

    private MaterialProperty _DissolveDirectionOn;
    private MaterialProperty _DissolveDirectionAngle;
    private MaterialProperty _DissolveDirectionWidth;
    private MaterialProperty _DissolveDirectionOffset;

    private MaterialProperty _DepthIntersectOn;
    private MaterialProperty _DepthIntersectColor;
    private MaterialProperty _DepthIntersectWidth;
    private MaterialProperty _DepthIntersectSmooth;
    private MaterialProperty _DepthSensitivity;
    private MaterialProperty _DepthIntersectHardEdge;

    private MaterialProperty _BulletOn;
    private MaterialProperty _BulletCol;
    private MaterialProperty _BulletRow;
    private MaterialProperty _BulletNumber;

    private MaterialProperty _VertexOffsetOn;
    private MaterialProperty _VertexOffsetTex;
    private MaterialProperty _VertexOffsetMaskTex;
    private MaterialProperty _VertexOffsetMaskRIsAlphaOn;
    private MaterialProperty _VertexOffsetStrength;
    private MaterialProperty _VertexOffsetFlowU;
    private MaterialProperty _VertexOffsetFlowV;
    private MaterialProperty _VertexOffsetCustomDataOn_Strength;
    private MaterialProperty _VertexOffsetCustomDataOn_MaskTex_ST_Z;
    private MaterialProperty _VertexOffsetCustomDataOn_MaskTex_ST_W;
    
    private bool _Main = true;
    private bool _Mask = true;
    private bool _Turbulence = true;
    private bool _DepthIntersect = true;

    private bool _BulletDisplay = true;
    private bool _VertexOffset = true;
    private bool _Dissolve = true;
    private void FinProperties(MaterialProperty[] properties)
    {
        _CullOff = FindProperty("_CullOff", properties, true);
        _AddMode = FindProperty("_AddMode", properties, true);
        _ZWriteOn = FindProperty("_ZWriteOn", properties, true);
        _SrcBlend = FindProperty("_SrcBlend", properties, true);
        _DstBlend = FindProperty("_DstBlend", properties, true);
        _SrcBlendAlpha = FindProperty("_SrcBlendAlpha", properties, true);
        _DstBlendAlpha = FindProperty("_DstBlendAlpha", properties, true);
        
        _SoftParticleOn = FindProperty("_SoftParticleOn", properties, true);
        _SoftParticleFade = FindProperty("_SoftParticleFade", properties, true);
        
        _MainTex = FindProperty("_MainTex", properties, true);
        _MainColor = FindProperty("_MainColor", properties, true);
        _MainColorIntensity = FindProperty("_MainColorIntensity", properties, true);
        
        _MainTexRedIsAlphaOn = FindProperty("_MainTexRedIsAlphaOn", properties, true);
        
        _MainTexRotation = FindProperty("_MainTexRotation", properties, true);
        _MainTexFlowAndBevel = FindProperty("_MainTexFlowAndBevel", properties, true);
        
        _MainTexCustomDataOn_TillingX = FindProperty("_MainTexCustomDataOn_TillingX", properties, true);
        _MainTexCustomDataOn_TillingY = FindProperty("_MainTexCustomDataOn_TillingY", properties, true);
        
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
        _MaskTexCustomDataOn_1_TillingX = FindProperty("_MaskTexCustomDataOn_1_TillingX", properties, true);
        _MaskTexCustomDataOn_1_TillingY = FindProperty("_MaskTexCustomDataOn_1_TillingY", properties, true);
        
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
        _MaskTexCustomDataOn_2_TillingX = FindProperty("_MaskTexCustomDataOn_2_TillingX", properties, true);
        _MaskTexCustomDataOn_2_TillingY = FindProperty("_MaskTexCustomDataOn_2_TillingY", properties, true);
        _MaskTexCustomDataOn_2_OffsetX = FindProperty("_MaskTexCustomDataOn_2_OffsetX", properties, true);
        
        _MaskTexPolarOn_2 = FindProperty("_MaskTexPolarOn_2", properties, true);
        _MaskTexPolarRotation_2 = FindProperty("_MaskTexPolarRotation_2", properties, true);
        _MaskTexPolarRadialScale_2 = FindProperty("_MaskTexPolarRadialScale_2", properties, true);
        _MaskTexPolarLengthScale_2 = FindProperty("_MaskTexPolarLengthScale_2", properties, true);
        _MaskTexPolarCenter_U_2 = FindProperty("_MaskTexPolarCenter_U_2", properties, true);
        _MaskTexPolarCenter_V_2 = FindProperty("_MaskTexPolarCenter_V_2", properties, true);
        _MaskTexPolarRadialOffset_2 = FindProperty("_MaskTexPolarRadialOffset_2", properties, true);
        _MaskTexPolarLengthOffset_2 = FindProperty("_MaskTexPolarLengthOffset_2", properties, true);
        
        _TurbulenceOn = FindProperty("_TurbulenceOn", properties, true);
        _TurbulenceTex = FindProperty("_TurbulenceTex", properties, true);
        _TurbulenceFlow = FindProperty("_TurbulenceFlow", properties, true);
        _TurbulenceBasePoint = FindProperty("_TurbulenceBasePoint", properties, true);
        _TurbulenceStrength = FindProperty("_TurbulenceStrength", properties, true);
        _TurbulenceMask = FindProperty("_TurbulenceMask", properties, true);
        _TurbulenceMask2 = FindProperty("_TurbulenceMask2", properties, true);
        
        _DissolveOn = FindProperty("_DissolveOn", properties, true);
        _DissolveTex = FindProperty("_DissolveTex", properties, true);
        _DissolveTexRotation = FindProperty("_DissolveTexRotation", properties, true);
        _DissolveTexFlowAndBevel = FindProperty("_DissolveTexFlowAndBevel", properties, true);
        _DissolveColor = FindProperty("_DissolveColor", properties, true);

        _DissolveTex2On = FindProperty("_DissolveTex2On", properties, true);
        _DissolveTex2 = FindProperty("_DissolveTex2", properties, true);
        _DissolveTex2Rotation = FindProperty("_DissolveTex2Rotation", properties, true);
        _DissolveTex2Offset = FindProperty("_DissolveTex2Offset", properties, true);
        _DissolveTex2Strength = FindProperty("_DissolveTex2Strength", properties, true);
        _DissolveTex2Blend = FindProperty("_DissolveTex2Blend", properties, true);
        _DissolveTex2Width = FindProperty("_DissolveTex2Width", properties, true);

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
        
        _DissolveDirectionOn = FindProperty("_DissolveDirectionOn", properties, true);
        _DissolveDirectionAngle = FindProperty("_DissolveDirectionAngle", properties, true);
        _DissolveDirectionWidth = FindProperty("_DissolveDirectionWidth", properties, true);
        _DissolveDirectionOffset = FindProperty("_DissolveDirectionOffset", properties, true);

        _BulletOn = FindProperty("_BulletOn", properties, true);
        _BulletCol = FindProperty("_BulletCol", properties, true);
        _BulletRow = FindProperty("_BulletRow", properties, true);
        _BulletNumber = FindProperty("_BulletNumber", properties, true);
        
        _VertexOffsetOn = FindProperty("_VertexOffsetOn", properties, true);
        _VertexOffsetTex = FindProperty("_VertexOffsetTex", properties, true);
        _VertexOffsetMaskTex = FindProperty("_VertexOffsetMaskTex", properties, true);
        _VertexOffsetMaskRIsAlphaOn = FindProperty("_VertexOffsetMaskRIsAlphaOn", properties, true);
        _VertexOffsetStrength = FindProperty("_VertexOffsetStrength", properties, true);
        _VertexOffsetFlowU = FindProperty("_VertexOffsetFlowU", properties, true);
        _VertexOffsetFlowV = FindProperty("_VertexOffsetFlowV", properties, true);
        _VertexOffsetCustomDataOn_Strength = FindProperty("_VertexOffsetCustomDataOn_Strength", properties, true);
        _VertexOffsetCustomDataOn_MaskTex_ST_Z = FindProperty("_VertexOffsetCustomDataOn_MaskTex_ST_Z", properties, true);
        _VertexOffsetCustomDataOn_MaskTex_ST_W = FindProperty("_VertexOffsetCustomDataOn_MaskTex_ST_W", properties, true);

        _DepthIntersectOn = FindProperty("_DepthIntersectOn", properties, true);
        _DepthIntersectColor = FindProperty("_DepthIntersectColor", properties, true);
        _DepthIntersectWidth = FindProperty("_DepthIntersectWidth", properties, true);
        _DepthIntersectSmooth = FindProperty("_DepthIntersectSmooth", properties, true);
        _DepthSensitivity = FindProperty("_DepthSensitivity", properties, true);
        _DepthIntersectHardEdge = FindProperty("_DepthIntersectHardEdge", properties, true);
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

            AdvancedEffectCull cullOri = (AdvancedEffectCull)_CullOff.floatValue;
            AdvancedEffectCull cullRes = (AdvancedEffectCull)EditorGUILayout.EnumPopup("剔除模式", cullOri);
            _CullOff.floatValue = (float)cullRes;
            
            bool zWriteOri = Equals(_ZWriteOn.floatValue, 1.0f);
            bool zWriteRes = EditorGUILayout.Toggle("是否写入深度", zWriteOri);
            _ZWriteOn.floatValue = zWriteRes ? 1.0f : 0.0f;
            
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
            
            bool softParticleOnOri = Equals(_SoftParticleOn.floatValue, 1.0f);
            bool softParticleOnRes = EditorGUILayout.Toggle("启用软粒子", softParticleOnOri);
            if (softParticleOnRes)
            {
                EditorGUI.indentLevel++;
                _SoftParticleOn.floatValue = 1.0f;
                materialEditor.RangeProperty(_SoftParticleFade, "软粒子过渡");
                targetMat!.EnableKeyword("_SOFT_PARTICLE_ON");
                EditorGUI.indentLevel--;
            }
            else
            {
                _SoftParticleOn.floatValue = 0.0f;
                targetMat!.DisableKeyword("_SOFT_PARTICLE_ON");
            }

            materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_MainTex), _MainTex);
            
            Vector2 scale = targetMat.GetTextureScale(MainTex);
            Vector2 offset = targetMat.GetTextureOffset(MainTex);
            
            bool mainTexCustomDataOnOri_TillingX = Equals(_MainTexCustomDataOn_TillingX.floatValue, 1.0f);
            bool mainTexCustomDataOnRes_TillingX = EditorGUILayout.Toggle("主贴图 Tilling X 启用自定义数据", mainTexCustomDataOnOri_TillingX);
            if (mainTexCustomDataOnRes_TillingX) _MainTexCustomDataOn_TillingX.floatValue = 1.0f;
            else _MainTexCustomDataOn_TillingX.floatValue = 0.0f;
            
            bool mainTexCustomDataOnOri_TillingY = Equals(_MainTexCustomDataOn_TillingY.floatValue, 1.0f);
            bool mainTexCustomDataOnRes_TillingY = EditorGUILayout.Toggle("主贴图 Tilling Y 启用自定义数据", mainTexCustomDataOnOri_TillingY);
            if (mainTexCustomDataOnRes_TillingY) _MainTexCustomDataOn_TillingY.floatValue = 1.0f;
            else _MainTexCustomDataOn_TillingY.floatValue = 0.0f;
            
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

                if (targetMat != null)
                {
                    Vector2 scale = targetMat.GetTextureScale(MaskTex1);
                    Vector2 offset = targetMat.GetTextureOffset(MaskTex1);
                
                    bool maskTexCustomDataOnOri_1_TillingX = Equals(_MaskTexCustomDataOn_1_TillingX.floatValue, 1.0f);
                    bool maskTexCustomDataOnRes_1_TillingX = EditorGUILayout.Toggle("遮罩图 1 Tilling X 启用自定义数据", maskTexCustomDataOnOri_1_TillingX);
                    if (maskTexCustomDataOnRes_1_TillingX) _MaskTexCustomDataOn_1_TillingX.floatValue = 1.0f;
                    else _MaskTexCustomDataOn_1_TillingX.floatValue = 0.0f;
                
                    bool maskTexCustomDataOnOri_1_TillingY = Equals(_MaskTexCustomDataOn_1_TillingY.floatValue, 1.0f);
                    bool maskTexCustomDataOnRes_1_TillingY = EditorGUILayout.Toggle("遮罩图 1 Tilling Y 启用自定义数据", maskTexCustomDataOnOri_1_TillingY);
                    if (maskTexCustomDataOnRes_1_TillingY) _MaskTexCustomDataOn_1_TillingY.floatValue = 1.0f;
                    else _MaskTexCustomDataOn_1_TillingY.floatValue = 0.0f;
                
                    EditorGUI.BeginChangeCheck();
                    scale = EditorGUILayout.Vector2Field("Tiling", scale);
                    offset = EditorGUILayout.Vector2Field("Offset", offset);
                    if (EditorGUI.EndChangeCheck())
                        _MaskTex_1.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                }

                materialEditor.VectorProperty(_MaskTexFlowAndBevel_1, "遮罩图 1 流动 & 斜切");
                materialEditor.RangeProperty(_MaskTexRotation_1, "遮罩图 1 旋转");
                
                bool maskTexRedIsAlphaOnOri_1 = Equals(_MaskTexRedIsAlphaOn_1.floatValue, 1.0f);
                bool maskTexRedIsAlphaOnRes_1 = EditorGUILayout.Toggle("遮罩图 1 R是透明通道", maskTexRedIsAlphaOnOri_1);
                if (maskTexRedIsAlphaOnRes_1) _MaskTexRedIsAlphaOn_1.floatValue = 1.0f;
                else _MaskTexRedIsAlphaOn_1.floatValue = 0.0f;
                
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
                    targetMat?.EnableKeyword("_MASK_TEX_POLAR_ON_1");
                    EditorGUI.indentLevel--;
                }
                else
                {
                    _MaskTexPolarOn_1.floatValue = 0.0f;
                    targetMat?.DisableKeyword("_MASK_TEX_POLAR_ON_1");
                }
                targetMat?.EnableKeyword("_MASK_TEX_ON_1");
                EditorGUI.indentLevel--;
            }
            else
            {
                _MaskTexOn_1.floatValue = 0.0f;
                targetMat?.DisableKeyword("_MASK_TEX_ON_1");
                targetMat?.DisableKeyword("_MASK_TEX_POLAR_ON_1");
            }
            
            bool maskTexOnOri_2 = Equals(_MaskTexOn_2.floatValue, 1.0f);
            bool maskTexOnRes_2 = EditorGUILayout.Toggle("启用遮罩图 2", maskTexOnOri_2);
            if (maskTexOnRes_2)
            {
                EditorGUI.indentLevel++;
                _MaskTexOn_2.floatValue = 1.0f;
                materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_MaskTex_2), _MaskTex_2);

                if (targetMat != null)
                {
                    Vector2 scale = targetMat.GetTextureScale(MaskTex2);
                    Vector2 offset = targetMat.GetTextureOffset(MaskTex2);
                
                    bool maskTexCustomDataOnOri_2_TillingX = Equals(_MaskTexCustomDataOn_2_TillingX.floatValue, 1.0f);
                    bool maskTexCustomDataOnRes_2_TillingX = EditorGUILayout.Toggle("遮罩图 2 Tilling X 启用自定义数据", maskTexCustomDataOnOri_2_TillingX);
                    if (maskTexCustomDataOnRes_2_TillingX) _MaskTexCustomDataOn_2_TillingX.floatValue = 1.0f;
                    else _MaskTexCustomDataOn_2_TillingX.floatValue = 0.0f;
                
                    bool maskTexCustomDataOnOri_2_TillingY = Equals(_MaskTexCustomDataOn_2_TillingY.floatValue, 1.0f);
                    bool maskTexCustomDataOnRes_2_TillingY = EditorGUILayout.Toggle("遮罩图 2 Tilling Y 启用自定义数据", maskTexCustomDataOnOri_2_TillingY);
                    if (maskTexCustomDataOnRes_2_TillingY) _MaskTexCustomDataOn_2_TillingY.floatValue = 1.0f;
                    else _MaskTexCustomDataOn_2_TillingY.floatValue = 0.0f;
                
                    bool maskTexCustomDataOnOri_2_OffsetX = Equals(_MaskTexCustomDataOn_2_OffsetX.floatValue, 1.0f);
                    bool maskTexCustomDataOnRes_2_OffsetX = EditorGUILayout.Toggle("遮罩图 2 Offset X 启用自定义数据", maskTexCustomDataOnOri_2_OffsetX);
                    if (maskTexCustomDataOnRes_2_OffsetX) _MaskTexCustomDataOn_2_OffsetX.floatValue = 1.0f;
                    else _MaskTexCustomDataOn_2_OffsetX.floatValue = 0.0f;
                
                    EditorGUI.BeginChangeCheck();
                    scale = EditorGUILayout.Vector2Field("Tiling", scale);
                    offset = EditorGUILayout.Vector2Field("Offset", offset);
                    if (EditorGUI.EndChangeCheck())
                        _MaskTex_2.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                }

                materialEditor.VectorProperty(_MaskTexFlowAndBevel_2, "遮罩图 2 流动 & 斜切");
                materialEditor.RangeProperty(_MaskTexRotation_2, "遮罩图 2 旋转");
                
                bool maskTexRedIsAlphaOnOri_2 = Equals(_MaskTexRedIsAlphaOn_2.floatValue, 1.0f);
                bool maskTexRedIsAlphaOnRes_2 = EditorGUILayout.Toggle("遮罩图 2 R是透明通道", maskTexRedIsAlphaOnOri_2);
                if (maskTexRedIsAlphaOnRes_2) _MaskTexRedIsAlphaOn_2.floatValue = 1.0f;
                else _MaskTexRedIsAlphaOn_2.floatValue = 0.0f;
                
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
                    targetMat?.EnableKeyword("_MASK_TEX_POLAR_ON_2");
                    EditorGUI.indentLevel--;
                }
                else
                {
                    _MaskTexPolarOn_2.floatValue = 0.0f;
                    targetMat?.DisableKeyword("_MASK_TEX_POLAR_ON_2");
                }
                targetMat?.EnableKeyword("_MASK_TEX_ON_2");
                EditorGUI.indentLevel--;
            }
            else
            {
                _MaskTexOn_2.floatValue = 0.0f;
                targetMat?.DisableKeyword("_MASK_TEX_ON_2");
                targetMat?.DisableKeyword("_MASK_TEX_POLAR_ON_2");
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
         EditorGUILayout.BeginVertical(EditorStyles.helpBox);
         _Turbulence = ShaderGUI_Helper.Foldout(_Turbulence, "UV扭曲");
        if (_Turbulence)
        {
            EditorGUI.indentLevel++;
            bool turbulenceOnOri = Equals(_TurbulenceOn.floatValue, 1.0f);
            bool turbulenceOnOriOnRes = EditorGUILayout.Toggle("启用UV扭曲", turbulenceOnOri);
            if (turbulenceOnOriOnRes)
            {
                _TurbulenceOn.floatValue = 1.0f;
                materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_TurbulenceTex), _TurbulenceTex);
                
                Vector2 scale = targetMat.GetTextureScale(TurbulenceTex);
                Vector2 offset = targetMat.GetTextureOffset(TurbulenceTex);
                EditorGUI.BeginChangeCheck();
                scale = EditorGUILayout.Vector2Field("Tiling", scale);
                offset = EditorGUILayout.Vector2Field("Offset", offset);
                if (EditorGUI.EndChangeCheck())
                    _TurbulenceTex.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                
                materialEditor.VectorProperty(_TurbulenceFlow, "UV扭曲流动速度");
                materialEditor.RangeProperty(_TurbulenceBasePoint, "扭曲基准值");
               
                
                materialEditor.VectorProperty(_TurbulenceStrength, "扭曲强度");
                
                bool turbulenceMaskOnOri = Equals(_TurbulenceMask.floatValue, 1.0f);
                bool turbulenceMaskOnRes = EditorGUILayout.Toggle("UV扭曲影响遮罩图1", turbulenceMaskOnOri);
                if (turbulenceMaskOnRes) _TurbulenceMask.floatValue = 1.0f;
                else _TurbulenceMask.floatValue = 0.0f;
                
                bool turbulenceMaskOnOri_2 = Equals(_TurbulenceMask2.floatValue, 1.0f);
                bool turbulenceMaskOnRes_2 = EditorGUILayout.Toggle("UV扭曲影响遮罩图2", turbulenceMaskOnOri_2);
                if (turbulenceMaskOnRes_2) _TurbulenceMask2.floatValue = 1.0f;
                else _TurbulenceMask2.floatValue = 0.0f;
                
              
                
                targetMat.EnableKeyword("_TURBULENCE_ON");
            }
            else
            {
                _TurbulenceOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_TURBULENCE_ON");
              
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
                
                Vector2 scale = targetMat.GetTextureScale(DissolveTex);
                Vector2 offset = targetMat.GetTextureOffset(DissolveTex);
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

                bool dissolveTex2OnOri = Equals(_DissolveTex2On.floatValue, 1.0f);
                bool dissolveTex2OnRes = EditorGUILayout.Toggle("启用附加溶解图", dissolveTex2OnOri);
                if(dissolveTex2OnRes){
                    EditorGUI.indentLevel++;
                    _DissolveTex2On.floatValue = 1.0f;

                    materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_DissolveTex2), _DissolveTex2);
                

                    Vector2 scale1 = targetMat.GetTextureScale(DissolveTex2);
                    Vector2 offset1 = targetMat.GetTextureOffset(DissolveTex2);
                    EditorGUI.BeginChangeCheck();
                    scale1 = EditorGUILayout.Vector2Field("Tiling", scale1);
                    offset1 = EditorGUILayout.Vector2Field("Offset", offset1);
                    if (EditorGUI.EndChangeCheck())
                        _DissolveTex2.textureScaleAndOffset = new Vector4(scale1.x, scale1.y, offset1.x, offset1.y);
                    
                    materialEditor.RangeProperty(_DissolveTex2Rotation, "附加溶解图 旋转");
                    materialEditor.RangeProperty(_DissolveTex2Strength, "附加溶解图 强度");
                    materialEditor.RangeProperty(_DissolveTex2Blend, "附加溶解图 混合");
                    materialEditor.RangeProperty(_DissolveTex2Width,"附加溶解图 过渡宽度");
                    materialEditor.RangeProperty(_DissolveTex2Offset, "附加溶解图 偏移");

                    targetMat.EnableKeyword("_DISSOLVE_TEX2_ON");
                    EditorGUI.indentLevel--;
                }
                else{
                    _DissolveTex2On.floatValue = 0.0f;
                    targetMat.DisableKeyword("_DISSOLVE_TEX2_ON");
                }


                materialEditor.RangeProperty(_DissolveEmissionWidth, "亮边宽度");
                materialEditor.RangeProperty(_DissolveSmooth, "软化程度");
                materialEditor.RangeProperty(_DissolveThreshold, "溶解程度");

                bool dissolveDirectionOnOri = Equals(_DissolveDirectionOn.floatValue, 1.0f);
                bool dissolveDirectionOnRes = EditorGUILayout.Toggle("溶解 启用方向控制", dissolveDirectionOnOri);
                if(dissolveDirectionOnRes){
                    EditorGUI.indentLevel++;
                    _DissolveDirectionOn.floatValue = 1.0f;

                    materialEditor.RangeProperty(_DissolveDirectionAngle, "溶解方向(角度)");
                    materialEditor.RangeProperty(_DissolveDirectionWidth, "方向过渡宽度");
                    materialEditor.RangeProperty(_DissolveDirectionOffset, "方向偏移(特定角度覆盖不全)");

                    targetMat.EnableKeyword("_DISSOLVE_DIRECTION_ON");
                    EditorGUI.indentLevel--;
                }
                else{
                    _DissolveDirectionOn.floatValue = 0.0f;
                    targetMat.DisableKeyword("_DISSOLVE_DIRECTION_ON");
                }
                
                
                targetMat.EnableKeyword("_DISSOLVE_ON");
            }
            else
            {
                _DissolveOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_DISSOLVE_ON");
                targetMat.DisableKeyword("_DISSOLVE_POLAR_ON");
                targetMat.DisableKeyword("_DISSOLVE_DIRECTION_ON");
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _DepthIntersect = ShaderGUI_Helper.Foldout(_DepthIntersect, "相交亮边");
        if (_DepthIntersect)
        {
            EditorGUI.indentLevel++;
            bool depthIntersectOnOri = Equals(_DepthIntersectOn.floatValue, 1.0f);
            bool depthIntersectOnRes = EditorGUILayout.Toggle("启用相切亮边", depthIntersectOnOri);
            if (depthIntersectOnRes)
            {
                _DepthIntersectOn.floatValue = 1.0f;
                materialEditor.ColorProperty(_DepthIntersectColor, "相切边缘颜色");
                materialEditor.RangeProperty(_DepthIntersectWidth, "相切宽度");
                materialEditor.RangeProperty(_DepthIntersectSmooth, "相切边缘颜色过渡");
                materialEditor.RangeProperty(_DepthSensitivity, "相切深度灵敏度");
                materialEditor.RangeProperty(_DepthIntersectHardEdge, "相切边缘硬边程度");
                targetMat.EnableKeyword("_DEPTH_INTERSECT");
            }
            else
            {
                _DepthIntersectHardEdge.floatValue = 0.0f;
                _DepthIntersectOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_DEPTH_INTERSECT");
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _BulletDisplay = ShaderGUI_Helper.Foldout(_BulletDisplay, "子弹HUD");
        if (_BulletDisplay)
        {
            EditorGUI.indentLevel++;
            bool bulletOnOri = Equals(_BulletOn.floatValue, 1.0f);
            bool bulletOnRes = EditorGUILayout.Toggle("启用子弹HUD", bulletOnOri);
            if (bulletOnRes)
            {
                _BulletOn.floatValue = 1.0f;
                materialEditor.FloatProperty(_BulletRow, "子弹贴图行数");
                materialEditor.FloatProperty(_BulletCol, "子弹贴图列数");
                materialEditor.FloatProperty(_BulletNumber, "当前子弹数量");

                targetMat.EnableKeyword("_BULLET_ON");
            }
            else
            {
                _BulletOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_BULLET_ON");
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _VertexOffset = ShaderGUI_Helper.Foldout(_VertexOffset, "顶点偏移");
        if (_VertexOffset)
        {
            EditorGUI.indentLevel++;
            bool vertexoffsetOnOri = Equals(_VertexOffsetOn.floatValue, 1.0f);
            bool vertexoffsetOnRes = EditorGUILayout.Toggle("启用 顶点偏移", vertexoffsetOnOri);
            if (vertexoffsetOnRes)
            {
                _VertexOffsetOn.floatValue = 1.0f;
                materialEditor.TextureProperty(_VertexOffsetTex, "顶点偏移贴图");

                materialEditor.TexturePropertySingleLine(ShaderGUI_Helper.GetGUIContext(_VertexOffsetMaskTex), _VertexOffsetMaskTex);
                Vector2 scale = targetMat.GetTextureScale(VertexOffsetMaskTex);
                Vector2 offset = targetMat.GetTextureOffset(VertexOffsetMaskTex);
                bool vertexOffsetCustomDataOn_MaskTex_ST_Z_OnOri = Equals(_VertexOffsetCustomDataOn_MaskTex_ST_Z.floatValue, 1.0f);
                bool vertexOffsetCustomDataOn_MaskTex_ST_Z_OnRes = EditorGUILayout.Toggle("遮罩贴图OffsetX 启用自定义数据", vertexOffsetCustomDataOn_MaskTex_ST_Z_OnOri);
                if(vertexOffsetCustomDataOn_MaskTex_ST_Z_OnRes) _VertexOffsetCustomDataOn_MaskTex_ST_Z.floatValue = 1.0f; 
                else _VertexOffsetCustomDataOn_MaskTex_ST_Z.floatValue = 0.0f;
                
                bool vertexOffsetCustomDataOn_MaskTex_ST_W_OnOri = Equals(_VertexOffsetCustomDataOn_MaskTex_ST_W.floatValue, 1.0f);
                bool vertexOffsetCustomDataOn_MaskTex_ST_W_OnRes = EditorGUILayout.Toggle("遮罩贴图OffsetY 启用自定义数据", vertexOffsetCustomDataOn_MaskTex_ST_W_OnOri);
                if(vertexOffsetCustomDataOn_MaskTex_ST_W_OnRes) _VertexOffsetCustomDataOn_MaskTex_ST_W.floatValue = 1.0f; 
                else _VertexOffsetCustomDataOn_MaskTex_ST_W.floatValue = 0.0f;
                EditorGUI.BeginChangeCheck();
                scale = EditorGUILayout.Vector2Field("Tiling", scale);
                offset = EditorGUILayout.Vector2Field("Offset", offset);
                if (EditorGUI.EndChangeCheck())
                    _VertexOffsetMaskTex.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                
                
                bool vertexOffsetMaskRIsAlphaOnOri = Equals(_VertexOffsetMaskRIsAlphaOn.floatValue, 1.0f);
                bool vertexOffsetMaskRIsAlphaOnRes = EditorGUILayout.Toggle("R是透明通道", vertexOffsetMaskRIsAlphaOnOri);
                if(vertexOffsetMaskRIsAlphaOnRes) _VertexOffsetMaskRIsAlphaOn.floatValue = 1.0f; 
                else _VertexOffsetMaskRIsAlphaOn.floatValue = 0.0f;
                
                bool vertexOffsetStrengthCustomData_OnOri = Equals(_VertexOffsetCustomDataOn_Strength.floatValue, 1.0f);
                bool vertexOffsetStrengthCustomData_OnRes = EditorGUILayout.Toggle("顶点偏移程度 启用自定义数据", vertexOffsetStrengthCustomData_OnOri);
                if(vertexOffsetStrengthCustomData_OnRes) _VertexOffsetCustomDataOn_Strength.floatValue = 1.0f; 
                else _VertexOffsetCustomDataOn_Strength.floatValue = 0.0f;
                materialEditor.FloatProperty(_VertexOffsetStrength, "顶点偏移程度");
                
                materialEditor.FloatProperty(_VertexOffsetFlowU, "U流动");
                materialEditor.FloatProperty(_VertexOffsetFlowV, "V流动");
                
                targetMat.EnableKeyword("_VERTEXOFFSET_ON");
            }
            else
            {
                _VertexOffsetOn.floatValue = 0.0f;
                targetMat.DisableKeyword("_VERTEXOFFSET_ON");
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        materialEditor.RenderQueueField();
    }
}