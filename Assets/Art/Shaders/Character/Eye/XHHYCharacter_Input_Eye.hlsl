#pragma once


// because _DETAIL always sample detail normal map
// 因为_DETAIL总是对细节法线图进行采样
#if _NORMALMAP || _DETAIL || _KAJIYAKAY_SPECULAR || _DYNAMIC_EYE || _PARALLAXMAP
    #define VaryingsHasTangentWS 1
#endif

    #define AnyOcclusionEnabled 1

#if _BASEMAP_STACKING_LAYER1 || _BASEMAP_STACKING_LAYER2 || _BASEMAP_STACKING_LAYER3 || _BASEMAP_STACKING_LAYER4 || _BASEMAP_STACKING_LAYER5 || _BASEMAP_STACKING_LAYER6 || _BASEMAP_STACKING_LAYER7 || _BASEMAP_STACKING_LAYER8 || _BASEMAP_STACKING_LAYER9 || _BASEMAP_STACKING_LAYER10
    #define AnyBaseMapStackingLayerEnabled 1
#endif

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"


// all pass will share this Attributes struct (define data needed from Unity app to our vertex shader)
struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL; // GetVertexNormalInputs(...) 希望得到float3 normalOS的输入，不要写半3产生不必要的类型转换成本。
    float4 tangentOS : TANGENT; // GetVertexNormalInputs(...) 希望得到浮点4的正切输入，不要写成半4产生不需要的类型转换成本。
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1; // 详细地图可能需要uv2(TEXCOORD1)。
    float3 uv8 : TEXCOORD7; //用于存储uv8(TEXCOORD7)中轮廓的角度平滑的切线空间法线。
    float4 color : COLOR; //用于轮廓宽度掩码，而不是作为一种颜色，所以使用浮动而不是半数
    uint vertexID : SV_VertexID; // SV_VertexID需要是uint（https://docs.unity3d.com/Manual/SL-ShaderSemantics.html）。
};


struct Varyings
{
    float4 positionCS : SV_POSITION;

    float2 uv : TEXCOORD0;
    //需要float而不是一半的uv，如果纹理尺寸大于2048，一半是不够的（https://forum.unity.com/threads/why-does-unity-recommend-us-to-use-float-type-for-texture-coordinates-in-the-shader.732920/#post-4894619）。
    float4 positionWS_ZOffsetFinalSum : TEXCOORD1; // xyz = positionWS, w = ZOffsetFinalSum

    half4 SH_fogFactor : TEXCOORD2;
    // (4 half pack into 1 TEXCOORD)  xyz: SampleSH(normalWS) * multipliers, w: fog factor
    half4 normalWS_averageShadowAttenuation : TEXCOORD3;
    // (4 half pack into 1 TEXCOORD)  xyz: normalWS, w: averageShadowAttenuation

    half3 viewDirectionWS : TEXCOORD4;
    #if _DECAL
        float2 uv2                            : TEXCOORD5;
    #endif

float depth : TEXCOORD6;
    
    #if VaryingsHasTangentWS
    half4 tangentWS                             : TANGENT;      // xyz: tangent, w: sign. TODO: is TANGENT safe to use? In URP's LitForwardPass.hlsl, it is TEXCOORD4, not TANGENT.
    #endif

    half4 color : COLOR;
    // vertex color rgba, currently only for "control depth texture rimlight and shadow width by vertex color" or "debug shading"

    // 溶解用：局部 Y 轴归一化高度（0 = 头顶，1 = 脚底）
    half dissolveHeight : TEXCOORD9;

    // debug use
    //--------------------------------------------------------------------
    #if _NILOTOON_DEBUG_SHADING
    float3 uv8                                  : TEXCOORD8;
    #endif
    //--------------------------------------------------------------------

    // 为了支持GPU实例化和单通道立体渲染（VR），添加以下部分
    //------------------------------------------------------------------------------------------------------------------------------
    UNITY_VERTEX_INPUT_INSTANCE_ID //在非OpenGL和非PSL中会变成这样 -- uint instanceID : SV_InstanceID。
    UNITY_VERTEX_OUTPUT_STEREO
    //在非OpenGL和非PSL中会变成这样 -- uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex。
    //------------------------------------------------------------------------------------------------------------------------------
};

//一些复用的sampler
SAMPLER(sampler_linear_clamp);
SAMPLER(sampler_Point_clamp);

SAMPLER(sampler_linear_repeat);

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_MaskMap);
TEXTURE2D(_DecalMap);
SAMPLER(sampler_DecalMap);


#if _BASEMAP_STACKING_LAYER1
    TEXTURE2D(_BaseMapStackingLayer1Tex); SAMPLER(sampler_BaseMapStackingLayer1Tex);// 只有前两层使用它自己的采样器，以便让用户有更多的控制。
    TEXTURE2D(_BaseMapStackingLayer1MaskTex);
#endif

#if _BASEMAP_STACKING_LAYER2
    TEXTURE2D(_BaseMapStackingLayer2Tex); SAMPLER(sampler_BaseMapStackingLayer2Tex);// 只有前两层使用它自己的采样器，以便让用户有更多的控制。
    TEXTURE2D(_BaseMapStackingLayer2MaskTex);
#endif

#if _EMISSION
    sampler2D _EmissionMap;
#endif

#if !_ISFACE
    sampler2D _MatCapAdditiveMap;
    TEXTURE2D(_MatCapAdditiveMaskMap);
#endif

TEXTURE2D(_RampLightingTex);            



#if !_ISFACE
    sampler2D _SpecularMap;
#endif


#if _ISFACE
    sampler2D _FaceShadowGradientMap;
    sampler2D _FaceShadowGradientMaskMap;
#endif

#if _NILOTOON_DISSOLVE //溶解
    sampler2D _DissolveThresholdMap;
#endif

#if _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
    TEXTURE2D(_PerCharacterBaseMapOverrideMap);
#endif
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// NiloToon的全局纹理
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////  
CBUFFER_START(UnityPerMaterial)
float _HairAlpha;
// 启用渲染

float _EnableRendering;

// 方向光染色
half _SunTintEnable;
half _SunTintAmount;

float4 _DecalColor;
float _LineNumber;
float _ColumnNumber;
float _Sequence;
// base color
float4 _BaseMap_ST;
half4 _BaseColor;
float  _Alpha;
half3 _PerCharacterBaseColorTint;

// 背面色调颜色
half3 _BackFaceTintColor;

// 阿尔法覆盖
half _AlphaOverrideStrength;
half _ApplyAlphaOverrideOnlyWhenFaceForwardIsPointingToCamera;
half4 _AlphaOverrideTexChannelMask;

// alpha test
half _Cutoff;

// Parallax
half _Parallax;

// BaseMap Alpha混合层（1-10）。
half4 _BaseMapStackingLayer1TintColor;
float2 _BaseMapStackingLayer1TexUVAnimSpeed;
float4 _BaseMapStackingLayer1TexUVScaleOffset;
half4 _BaseMapStackingLayer1MaskTexChannel;
float _BaseMapStackingLayer1TexUVToUV2;
half _IsAddColor;
half4 _BaseMapStackingLayer2TintColor;
float2 _BaseMapStackingLayer2TexUVAnimSpeed;
float4 _BaseMapStackingLayer2TexUVScaleOffset;
half4 _BaseMapStackingLayer2MaskTexChannel;

// 由于vulkan（android Mali GPU设备）的恒定缓冲区大小限制。
// 我们用硬编码删除了vulkan上的basemap堆叠层3-10，以使cbuffer更小。
// 有问题的设备列表。(vivo Y73s) (vivo Y72) (TAS_AN00) (mate40) (华为Honor 9x), 所有的设备都是Mali-G__ GPU + vulkan. 
// 虽然 "Adreno GPU + vulkan "是安全的（小米8）（三星A70），"OpenGLES + 任何GPU "也是安全的。

// emission
float4 _EmissionMapTilingXyOffsetZw;
half _EmissionIntensity;
half3 _EmissionColor;
half _MultiplyBaseColorToEmissionColor;
half _EmissionMapUseSingleChannelOnly;
half4 _EmissionMapSingleChannelMask;

// mat cap (alpha blend)
half _MatCapAlphaBlendUsage;
half4 _MatCapAlphaBlendTintColor;
float _MatCapAlphaBlendUvScale;
half _MatCapAlphaBlendMapAlphaAsMask;
half4 _MatCapAlphaBlendMaskMapChannelMask;
half _MatCapAlphaBlendMaskMapRemapStart;
half _MatCapAlphaBlendMaskMapRemapEnd;
half _MetallicColorIntensity;

// mat cap (additive)
half _MatCapAdditiveMapAlphaAsMask;
half _MatCapAdditiveIntensity;
half4 _MatCapAdditiveColor;
half _MatCapAdditiveMixWithBaseMapColor;
float _MatCapAdditiveUvScale;
half4 _MatCapAdditiveMaskMapChannelMask;
half _MatCapAdditiveMaskMapRemapStart;
half _MatCapAdditiveMaskMapRemapEnd;
half _MetallicSpecularIntensity;
half4 _MetallicSpecularColor;
half _MetallicRoughness;
// mat cap (occlusion)
half _MatCapOcclusionIntensity;
half4 _MatCapOcclusionMapChannelMask;
half _MatCapOcclusionMapRemapStart;
half _MatCapOcclusionMapRemapEnd;
half _MatCapOcclusionMapAlphaAsMask;
float _MatCapOcclusionUvScale;
half4 _MatCapOcclusionMaskMapChannelMask;
half _MatCapOcclusionMaskMapRemapStart;
half _MatCapOcclusionMaskMapRemapEnd;
half _MatCapIntensity1;
half _MatCapIntensity2;
half _MatCapIntensity3;
half _MatCapIntensity4;
half _MatCapIntensity5;

half _Roughness1;
half _Roughness2;
half _Roughness3;
half _Roughness4;
half _Roughness5;

// occlusion
half _OcclusionStrength;
half _OcclusionStrengthIndirectMultiplier;
half4 _OcclusionMapChannelMask;
half _OcclusionRemapStart;
half _OcclusionRemapEnd;

// shading grade map
half _ShadingGradeMapStrength;
half _ShadingGradeMapApplyRange;
half _ShadingGradeMapMidPointOffset;
half4 _ShadingGradeMapChannelMask;
half _ShadingGradeMapInvertColor;
half _ShadingGradeMapRemapStart;
half _ShadingGradeMapRemapEnd;

// Face Shadow Gradient Map
//half4 _FaceShadowGradientMapChannel;
//half _FaceShadowGradientOffset;
float4 _FaceShadowGradientMapUVScaleOffset;
float _DebugFaceShadowGradientMap;
half _FaceShadowGradientIntensity;
half _FaceShadowGradientResultSoftness;
half4 _FaceShadowGradientMaskMapChannel;
float _FaceShadowNoFollowThrLight;
float _FaceShadowResult;
float _FaceShadowRool;

// specular
half4 _SpecularMapChannelMask;
half _SpecularMapRemapStart;
half _SpecularMapRemapEnd;
half _UseGGXDirectSpecular;
half _SpecularIntensity;
half3 _SpecularColor;
half _MultiplyBaseColorToSpecularColor;
half _GGXDirectSpecularSmoothnessMultiplier;
half _SpecularAreaRemapUsage;
half _SpecularAreaRemapMidPoint;
half _SpecularAreaRemapRange;
half _SpecularShowInShadowArea;
half _Multiply;


// kayjiya-kay hair specular
half _HairStrandSpecularMixWithBaseMapColor;
half _HairStrandSpecularOverallIntensity;
half _HairStrandSpecularShapeFrequency;
half _HairStrandSpecularShapeShift;
half _HairStrandSpecularShapePositionOffset;
half _HairStrandSpecularMainIntensity;
half _HairStrandSpecularSecondIntensity;
half3 _HairStrandSpecularMainColor;
half3 _HairStrandSpecularSecondColor;
half _HairStrandSpecularMainExponent;
half _HairStrandSpecularSecondExponent;


half _OverrideByFaceShadowTintColor;
half3 _FaceShadowTintColor;


// override shadow color by tex
half _OverrideShadowColorByTexIntensity;
half4 _OverrideShadowColorTexTintColor;
half _OverrideShadowColorTexIgnoreAlphaChannel;

// ramp lighting texture
half _RampLightingTexSampleUvY;
half _UseRampLightingSampleUvYTex;
half _UseRampLightingTex;


// lighting style
half _CelShadeMidPoint;
half _CelShadeSoftness;
half _MainLightIgnoreCelShade;
half _IndirectLightFlatten;
half _AdditionalLightCelShadeMidPoint;
half _AdditionalLightCelShadeSoftness;
half _AdditionalLightIgnoreCelShade;
half _AdditionalLightIgnoreOcclusion;
half _AdditionalLightDistanceAttenuationClamp;

// lighting style for face area
float _OverrideCelShadeParamForFaceArea;
half _CelShadeMidPointForFaceArea;
half _CelShadeSoftnessForFaceArea;
half _MainLightIgnoreCelShadeForFaceArea;
float _OverrideAdditionalLightCelShadeParamForFaceArea;
half _AdditionalLightCelShadeMidPointForFaceArea;
half _AdditionalLightCelShadeSoftnessForFaceArea;
half _AdditionalLightIgnoreCelShadeForFaceArea;


// depth texture rim light and shadow
float _PerMaterialEnableDepthTextureRimLightAndShadow;
float _NiloToonEnableDepthTextureRimLightAndShadow;

float _DepthTexRimLightAndShadowWidthMultiplier;

float _UseDepthTexRimLightAndShadowWidthMultiplierFromVertexColor;
float4 _DepthTexRimLightAndShadowWidthMultiplierFromVertexColorChannelMask;
float4 _DepthTexRimLightAndShadowWidthTexChannelMask;


half _DepthTexRimLightUsage;
half _DepthTexRimLightIntensity;
half3 _DepthTexRimLightTintColor;
half _DepthTexRimLightMixWithBaseMapColor;
float _DepthTexRimLightThresholdOffset;
float _DepthTexRimLightFadeoutRange;


float _FaceAreaCameraDepthTextureZWriteOffset;
// 当把深度写入_CameraDepthTexture时，脸部区域有足够的ZOffset。好的ZOffset范围大约是0.04到0.06，这使深度纹理的阴影更容易出现。

// average shadow mapping
float _AverageShadowMapRTSampleIndex;


// outline
//float _RenderOutline;
float _OutlineUseBakedSmoothNormal;
float _UnityCameraDepthTextureWriteOutlineExtrudedPosition;
half3 _OutlineTintColor;
half4 _OutlineTintColorSkinAreaOverride;
half3 _OutlineOcclusionAreaTintColor;
half _OutlineUseReplaceColor;
half3 _OutlineReplaceColor;
int _OutlineCorrection;

half3 _OutlineTintColor1;
half3 _OutlineTintColor2;
half3 _OutlineTintColor3;
half3 _OutlineTintColor4;
half3 _OutlineTintColor5;

float _OutlineWidth;
float _OutlineWidthExtraMultiplier;

float _OutlineZOffset;
float _OutlineZOffsetForFaceArea;
float _UseOutlineZOffsetTex;
float _OutlineZOffsetMaskRemapStart;
float _OutlineZOffsetMaskRemapEnd;
float _UseOutlineWidthMaskFromVertexColor;
float4 _OutlineWidthMaskFromVertexColor;
//float4 _OutlineWidthTexChannelMask;

// override outline color by tex
half4 _OverrideOutlineColorTexTintColor;
half _OverrideOutlineColorTexIgnoreAlphaChannel;
half _OverrideOutlineColorByTexIntensity;

// [screen space outline]
// 由于vulkan（android Mali GPU设备）的恒定缓冲区大小限制。
// 我们用硬编码删除了vulkan上的basemap堆叠层3-10，以使cbuffer更小。
// 有问题的设备列表。(vivo Y73s) (vivo Y72) (TAS_AN00) (mate40) (华为Honor 9x), 所有的设备都是Mali-G__ GPU + vulkan. 
//虽然 "Adreno GPU + vulkan "是安全的（小米8）（三星A70），"OpenGLES + 任何GPU "也是安全的。
#ifndef SHADER_API_VULKAN
float _ScreenSpaceOutlineWidth;
float _ScreenSpaceOutlineWidthIfFace;
float _ScreenSpaceOutlineDepthSensitivity;
float _ScreenSpaceOutlineDepthSensitivityIfFace;
float _ScreenSpaceOutlineNormalsSensitivity;
float _ScreenSpaceOutlineNormalsSensitivityIfFace;
half3 _ScreenSpaceOutlineTintColor;
half3 _ScreenSpaceOutlineOcclusionAreaTintColor;
half _ScreenSpaceOutlineUseReplaceColor;
half3 _ScreenSpaceOutlineReplaceColor;
half4 _ScreenSpaceOutlineDepthSensitivityTexChannelMask;
half4 _ScreenSpaceOutlineNormalsSensitivityTexChannelMask;
half _ScreenSpaceOutlineDepthSensitivityTexRemapStart;
half _ScreenSpaceOutlineDepthSensitivityTexRemapEnd;
half _ScreenSpaceOutlineNormalsSensitivityTexRemapStart;
half _ScreenSpaceOutlineNormalsSensitivityTexRemapEnd;
#endif

// dynamic eye
float _DynamicEyeSize;
half _DynamicEyeFinalBrightness;
half3 _DynamicEyeFinalTintColor;
half4 _DynamicEyePupilMaskTexChannelMask;
half3 _DynamicEyePupilColor;
float _DynamicEyePupilDepthScale;
float _DynamicEyePupilSize;
float _DynamicEyePupilMaskSoftness;
float4 _DynamicEyeWhiteMap_ST;

// extra thick outline
float _ExtraThickOutlineWidth;
float _ExtraThickOutlineMaxFinalWidth;
float3 _ExtraThickOutlineViewSpacePosOffset;
//half4 _ExtraThickOutlineColor;
float _ExtraThickOutlineZOffset;

// gameplay effect
half3 _PerCharEffectTintColor;
half3 _PerCharEffectAddColor;
half4 _PerCharEffectLerpColor;
half3 _PerCharEffectRimColor;
half _PerCharEffectRimSharpnessPower;
half _PerCharEffectDesaturatePercentage;

// 受击闪白：独立一层菲涅尔，不跟角色边缘光共用参数
half3 _HitFlashColor;
half _HitFlashIntensity;
half _HitFlashPower;

// skin
//float _IsSkin;
half4 _SkinMaskMapChannelMask;

// face
half4 _FaceMaskMapChannelMask;
half3 _FaceForwardDirection;
half3 _FaceUpDirection;
half _FixFaceNormalAmount;
half _FixFaceNormalUseFlattenOrProxySphereMethod;

// per char center
float3 _CharacterBoundCenterPosWS;

// dither fadeout
float _DitherFadeoutAmount; // clip() only accept float

// perspective removal
float _PerspectiveRemovalAmount; // total amount
// perspective removal(sphere)
float _PerspectiveRemovalRadius;
float3 _HeadBonePositionWS;
// perspective removal(world height)
float _PerspectiveRemovalStartHeight; // usually is world space pos.y 0
float _PerspectiveRemovalEndHeight;

// ZWrite (for disabling all _CameraDepthTexture related effect when _ZWrite = off)
float _ZWrite;
// Cull (for fixing outline problem when rendering planar reflection pass)
float _Cull;


// _NILOTOON_DISSOLVE
float _DissolveAmount;
//float _DissolveSource;
float _DissolveBorderRange;
half4 _DissolveBorderTintColor;
float _DissolveTopBorderRange;
half4 _DissolveTopBorderTintColor;
float _DissolveMinY;
float _DissolveMaxY;
float _DissolveRootY;
float _DissolveEdgeDitherStrength;
float _EnableDissolve;
float _EnableDitherFadeout;


// per character basemap override
half _PerCharacterBaseMapOverrideAmount;
float _PerCharacterBaseMapOverrideTiling;
half3 _PerCharacterBaseMapOverrideTintColor;



float3 _CenterPointPosition;

CBUFFER_END

// 把物体空间 Y 转成溶解高度：0 = 头顶（先消失），1 = 脚底（最后消失）。
half GetDissolveHeightOS(float positionOSY)
{
    half h = saturate((positionOSY - _DissolveMinY) / max(_DissolveMaxY - _DissolveMinY, 1e-5));
    return 1.0 - h;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 全局性的制服
// 如果一个制服不是每个材质的制服。
// 在CBUFFER_START(UnityPerMaterial)之外写它是可以的。
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// a special uniform for applyShadowBiasFixToHClipPos() only
float3 _LightDirection;
float _GlobalOutlineWidth;
// global outline uniforms
float _GlobalShouldRenderOutline;
float _GlobalOutlineWidthMultiplier;
half3 _GlobalOutlineTintColor;
float _GlobalOutlineWidthAutoAdjustToCameraDistanceAndFOV;

// global shadow mapping uniforms
float _GlobalShouldReceiveShadowMapping;
half _GlobalReceiveShadowMappingAmount;
float _GlobalToonShaderNormalBiasMultiplier;
half3 _GlobalMainLightURPShadowAsDirectResultTintColor;
half _GlobalURPShadowAsDirectLightTintIgnoreMaterialURPUsageSetting;
half _GlobalNiloToonReceiveURPShadowblurriness;

// self shadow mapping
float4x4 _NiloToonSelfShadowWorldToClip;
float4 _NiloToonSelfShadowParam;
float _NiloToonSelfShadowRange;
float _NiloToonGlobalSelfShadowDepthBias;
half3 _NiloToonSelfShadowLightDirection;
half _NiloToonSelfShadowUseNdotLFix;
float _GlobalReceiveSelfShadowMappingPosOffset;

// global occlusion uniforms
half _GlobalOcclusionStrength;

// global lighting uniforms
half _GlobalIndirectLightMultiplier;
half3 _GlobalIndirectLightMinColor;

// global depth diff rim light and shadow uniforms
float _GlobalDepthTexRimLightAndShadowWidthMultiplier;
float _GlobalDepthTexRimLightDepthDiffThresholdOffset;
float _GlobalDepthTexRimLightCameraDistanceFadeoutStartDistance;
float _GlobalDepthTexRimLightCameraDistanceFadeoutEndDistance;

// global light uniforms
half3 _GlobalUserOverridedMainLightDirVS;
half3 _GlobalUserOverridedMainLightDirWS;

// global camera uniforms
float _CurrentCameraFOV;

// global camera fix
float2 _GlobalAspectFix;
float _GlobalFOVorOrthoSizeFix;

// global volume
half3 _GlobalVolumeMulColor;
half4 _GlobalVolumeLerpColor;
half3 _GlobalRimLightMultiplier;
half3 _GlobalMainDirectionalLightMaxContribution;
half3 _GlobalAdditionalLightMaxContribution;
half3 _GlobalSpecularTintColor;
half _GlobalSpecularMinIntensity;
half _GlobalSpecularReactToLightDirectionChange;
half3 _GlobalMainDirectionalLightMultiplier;
half3 _GlobalAdditionalLightMultiplier;
half3 _GlobalVolumeBaseColorTintColor;
half3 _GlobalCharacterOverallShadowTintColor;
half _GlobalCharacterOverallShadowStrength;

// global screen space outline settings
float _GlobalScreenSpaceOutlineIntensityForChar;
float _GlobalScreenSpaceOutlineWidthMultiplierForChar;
float _GlobalScreenSpaceOutlineNormalsSensitivityOffsetForChar;
float _GlobalScreenSpaceOutlineDepthSensitivityOffsetForChar;
float _GlobalScreenSpaceOutlineDepthSensitivityDistanceFadeoutStrengthForChar;
half3 _GlobalScreenSpaceOutlineTintColorForChar;

// global utility
float _GlobalShouldDisableNiloToonDepthTextureRimLightAndShadow;

// global debug
float _GlobalToonShadeDebugPipline;
float _GlobalToonShadeDebugCase;
float _DayTime;


//float _OutlineSca;
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Data Structs
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 这个结构类似于URP的SurfaceData.hlsl -> "SurfaceData" 结构
// 注意：不要将其命名为 "SurfaceData"，因为它将与URP的SurfaceData.hlsl中的 "SurfaceData "结构冲突。
struct ToonSurfaceData
{
    half3 albedo;
    half alpha;
    half3 emission;
    half occlusion; 
    half3 specular;
    half specularMask; // store user's remapped specular mask texture, for ramp specular
    half3 normalTS;
    half smoothness;
    half metallicMask;
    half colormask1;
    half colormask2;
    half colormask3;
    half colormask4;
    half colormask5;

#if _NILOTOON_DEBUG_SHADING
    //ShadeMainLight  灯光结果debug
    half celShade; //赛璐璐光照结果
    half celShadeOcclusion; //赛璐璐光照,TexAo叠加结果
    half faceShadow; // 面部阴影
    half rimAttenuation;  // depthRimLight_Shadow 深度边缘光和阴影 强度
    half rimAttenuation_FadeOut; // depthRimLight_Shadow 深度淡入淡出
    half averageShadowAttenuation; // 平均过渡阴影
    half rampUvX; //ramp光照 UV 映射X (是否合并映射)
    half rampLightingTexSamplingUvY; //ramp光照 UV 映射Y (是否合并映射)

    half3 RampCol; //采样的ramp光照颜色
    half3 RampColAlbedo; //融合贴图的rampCol
    
    half3 specularLitAdd; //Specular 高光部分
    
    half3 Resut_RampCol_Albede_Specular_FinalShadow; // 融合光照
    
    half3 rimLightColor; // 边缘光颜色调控
    half3 Resut_add_rimLightColor; // 叠加边缘光颜色调控结果
    
    half4 MatCapColor; // 金属高光颜色
    half3 MatCapAddColor; // 金属高光混合光照
    half light_distanceAttenua; // 金属高光混合光照
    
#endif
    
};

// 这个结构与URP的Input.hlsl->"InputData "结构相似。
// 任何对照明有用的数据都会被放到这个结构中。
struct ToonLightingData
{
    float2 uv;
    half3 normalWS;
    half3 PerPixelNormalizedPolygonNormalWS;
    float3 positionWS;
    half3 viewDirectionWS;
    float selfLinearEyeDepth;
    //half averageShadowAttenuation;
   // half3 SH;
   // half isFaceArea; // default 0, = default not face, see InitializeLightingData(...)
    //half isSkinArea; // default 0, = default not skin, see InitializeLightingData(...)
    float2 SV_POSITIONxy;
    half3 normalVS;
    half3 reflectionVectorVS;
    half NdotV;
    half PolygonNdotV;
    half rawURPShadowAttenuation;

    #if VaryingsHasTangentWS
    half3x3 TBN_WS;
    half3   viewDirectionTS;
    #endif

    half4 vertexColor;

    float ZOffsetFinalSum;

    float2 normalizedScreenSpaceUV;
    
};


struct ToonMap
{
    half4 _BaseMap_Var;
    /*half4 _MaskMap_Var;
    half4 _DecalMap_Var;*/
};

struct Mask
{
    half colormask1;
    half colormask2;
    half colormask3;
    half colormask4;
    half colormask5;
};


//output: .xyz  = lightingNormalWS (脸部正常编辑应用)   .w    = isFaceArea
half4 GetLightingNormalWS_FaceArea(half3 normalWS, float3 positionWS, float2 uv)
{
    half3 resultNormalWS = normalWS; // 默认使用原始的normalWS
   
    half isFaceArea = 1;              // 如果没有面罩，当_ISFACE开启时，我们假定整个材料是面罩。

    // [应用面部正常编辑]
    //---------------------------------------------------------------------------------------------
    // 方法A：通过强迫脸部法线成为脸部的前进方向矢量
    half3 flatDirectionFixFaceNormalWS = _FaceForwardDirection;

    // 方法B：通过强制脸部法线成为球体代理法线，使用_HeadBonePositionWS作为球体代理中心。
    half3 proxySphereFixFaceNormalWS = positionWS - _HeadBonePositionWS;

    //让用户选择方法A或方法B
    half3 fixedFaceNormalWS = lerp(flatDirectionFixFaceNormalWS, proxySphereFixFaceNormalWS,
                                   _FixFaceNormalUseFlattenOrProxySphereMethod);

    resultNormalWS = normalize(lerp(resultNormalWS, fixedFaceNormalWS, _FixFaceNormalAmount * isFaceArea));
    //在最后只进行一次normalize()以提高性能
    //---------------------------------------------------------------------------------------------
    return half4(resultNormalWS, isFaceArea);
}

#include "..\XHHYUtilityHLSL\XhhyNormalVectorUtil.hlsl" //.hlsl 合并include

float3 TransformPositionWSToOutlinePositionWS(float width, VertexPositionInputs vertexPositionInputs,
                                              VertexNormalInputs vertexNormalInputs, float3 smoothedNormalTS)
{
    //width *= GetOutlineCameraFovAndDistanceFixMultiplier(vertexPositionInputs.positionVS.z, _CurrentCameraFOV, _GlobalOutlineWidthAutoAdjustToCameraDistanceAndFOV);

    float Outlinemask = length(vertexPositionInputs.positionWS.xyz - _WorldSpaceCameraPos.xyz);
    Outlinemask = saturate(Outlinemask / 6);
    Outlinemask = pow(Outlinemask, 0.9) * 6;
    Outlinemask = min(5, Outlinemask);
    width *= Outlinemask * 0.001;

    // 默认情况下，我们将使用照明法线（常规顶点法线）作为挤出方向。它是可用的，但由于平滑组的分割法线，它并不完美。
    // 对于硬边的多边形（例如立方体的角，头发的末端顶点），它将产生不连续的轮廓。
    float3 extrudeDirectionWS = vertexNormalInputs.normalWS;

    // 但是 "世界空间平滑法线 "是一个比简单使用照明法线（常规顶点法线）更好的挤出轮廓的方向。
    // 因为平滑法线没有任何分割法线，所以轮廓总是连续的，这看起来更好。
    // 如果我们在模型的uv8中烘焙了 "切线空间平滑法线"。我们可以在这里将其转换回世界空间，并将其作为一个更好的挤出轮廓的方向。
    extrudeDirectionWS = ConvertNormalTSToNormalTargetSpace(smoothedNormalTS, vertexNormalInputs.tangentWS,
                                                            vertexNormalInputs.bitangentWS,
                                                            vertexNormalInputs.normalWS);
    // [这部分是可选的] 。
    //你可以使挤压方向在屏幕空间中归一化，这将产生屏幕空间恒定宽度的轮廓。
    // https://www.videopoetics.com/tutorials/pixel-perfect-outline-shaders-unity/
    // https://github.com/Santarh/MToon/blob/master/MToon/Resources/Shaders/MToonCore.cginc#L90
    // TODO: 我们是否需要支持这个选项？

    //产生并返回世界空间中的轮廓顶点位置 
    float3 outlinePositionWS = vertexPositionInputs.positionWS + extrudeDirectionWS * width;
    return vertexPositionInputs.positionWS;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//片段共享函数（步骤1：如果是DEBUG，运行这些DEBUG函数，并在片段函数的早期启动点提前退出）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
half4 Get_NILOTOON_FORCE_MINIMUM_FRAGMENT_SHADER_result(Varyings input,int toonIsAnyOutlinePass)
{
    half3 debugColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
    if( toonIsAnyOutlinePass)
    {
        debugColor *= 0.25; // 任何值都是有效的，我们只是想让轮廓变暗一点。
    }
    return half4(debugColor, 1);
}

half4 Get_NILOTOON_DEBUG_SHADING_result(Varyings input, ToonSurfaceData surfaceData, ToonLightingData lightingData , int toonForwardLitPass)
{
    #if _NILOTOON_DEBUG_SHADING
    // 性能在这里并不重要，因为它是一个调试着色器的函数。

    // 如果_NILOTOON_DEBUG_SHADING被激活，我们只想渲染NiloToonForwardLitPass
    if( !toonForwardLitPass)
    {
        clip(-1);
        return 0;
    }

    // hlsl中switch case的例子。
    // https://github.com/SickheadGames/HL2GLSL/blob/master/tests/HL2GLSL/FlowControl.hlsl
    switch(_GlobalToonShadeDebugCase)
    {
        case 0:     return half4(surfaceData.albedo, surfaceData.alpha);
        case 1:     return 1;
        case 2:     return surfaceData.occlusion;
        case 3:     return half4(surfaceData.emission,1);
        case 4:     return half4(lightingData.normalWS * 0.5 + 0.5,1);
        case 5:     return half4(input.uv,0,1);
        case 6:     return input.color.r;
        case 7:     return input.color.g;
        case 8:     return input.color.b;
        case 9:     return input.color.a;
        case 10:    return half4(input.color.rgb,1);
        case 11:    return half4(surfaceData.specular.rgb,1);
        case 12:    return half4(input.uv8,1);
        case 13:    return half4(surfaceData.albedo * dot(lightingData.normalWS,lightingData.viewDirectionWS), surfaceData.alpha);

        case 14:    return surfaceData.celShade;
        case 15:    return surfaceData.celShadeOcclusion;
        case 16:    return surfaceData.faceShadow;
        case 17:    return surfaceData.rimAttenuation;
        case 18:    return surfaceData.rimAttenuation_FadeOut;
        case 19:    return surfaceData.averageShadowAttenuation;
        case 20:    return surfaceData.rampUvX;
        case 21:    return surfaceData.rampLightingTexSamplingUvY;
        case 22:    return half4(surfaceData.RampCol,1);
        case 23:    return half4(surfaceData.RampColAlbedo,1);
        case 24:    return half4(surfaceData.specularLitAdd,1);
        case 25:    return half4(surfaceData.Resut_RampCol_Albede_Specular_FinalShadow,1);
        case 26:    return half4(surfaceData.rimLightColor,1);
        case 27:    return half4(surfaceData.Resut_add_rimLightColor,1);
        case 28:    return surfaceData.MatCapColor;
        case 29:    return half4(surfaceData.MatCapAddColor,1);
        case 30:    return surfaceData.light_distanceAttenua;
    }
    #endif

    return 1;
}