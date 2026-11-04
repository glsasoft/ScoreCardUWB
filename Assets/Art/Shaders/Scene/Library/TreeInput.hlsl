// #include <HLSLSupport.cginc>
// CBUFFER_START(UnityPerMaterial)
// float _FrePow;
// float _FreInt;
// float _AOInt;
// float4 _AOCol;
// float4 _MainCol, _MainCol2;
// float4 _FreCol, _FreCol2;
// float _AoPow;
// float4 _YunDong;
// float _ShadowInt;
// float _FanWei;
// float _OffsetMul, _Threshold;
// float _SpeInt, _SpePow;
// float4 _SpeCol;
// float _A;
//
// float3 _FogSet;
// float _FogHeight, _FogDensity, _FogMaxDensit, _FogHeightDis, _RunFogpow, _RunFogMaxDensit, _FogStart;
// half3 _FogHeigheCol, _FogColor2, _RunColor;
//
// CBUFFER_END
#ifndef TREEINPUT
#define TREEINPUT


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MyFog.hlsl"

// #if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
//         #define _DETAIL
// #endif

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
//CBUFFER_START(UnityPerMaterial)

float4 _BaseMap_ST;
float _FrePow;
float _FreInt;
float _AOInt;
float4 _AOCol;
float4 _MainCol, _MainCol2;
float4 _FreCol, _FreCol2;
float _AoPow;
float4 _YunDong;
float _ShadowInt;
float _FanWei;
float _OffsetMul, _Threshold;
float _SpeInt, _SpePow;
float4 _SpeCol;
float _A;
float _DitherInt;
float _minClip;
float _Clip;
float _minDis;
float _maxDis;
float _OutLineMask;


float3 _MainCol3;
float _ColorLambertFactor,_MainCol3Int,_Clip2,_ShadowInt2;
float _H,_S,_V;
float _IsSSS,_SSSPoW,_SSSInt,_BackSubsurfaceDistortion;
float _AOInt2;
float _C,_B,_D,_LambertInt;
float4 _AOCol1;

float _DitherFadeoutAmount;

float _Light_H;
float _Light_S;
float _Light_V;
float _MinLightInt;
//CBUFFER_END

//float _LightInt;
float _AnimatorSpeed;
float _AnimatorRange;
float _TimeAnimator;
half Dither8x8_Arry(float2 cs, half x,float A8x8[64])
{
    
    uint2 uv = (uint2)cs % 8;
    return step(A8x8[uv.x * 8 + uv.y] / 65, x);
}

half3 RGB2HSV(half3 c)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

    half d = q.x - min(q.w, q.y);
    half e = 1.0e-10;
    return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half3 HSV2RGB(half3 c)
{
    half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}
// float3 _FogSet;
// float _FogHeight, _FogDensity, _FogMaxDensit, _FogHeightDis, _RunFogpow, _RunFogMaxDensit, _FogStart;
// half3 _FogHeigheCol, _FogColor2, _RunColor;

// NOTE: Do not ifdef the properties for dots instancing, but ifdef the actual usage.
// Otherwise you might break CPU-side as property constant-buffer offsets change per variant.
// NOTE: Dots instancing is orthogonal to the constant buffer above.


// TEXTURE2D(_ParallaxMap);
// SAMPLER(sampler_ParallaxMap);
// TEXTURE2D(_OcclusionMap);
// SAMPLER(sampler_OcclusionMap);
// TEXTURE2D(_DetailMask);
// SAMPLER(sampler_DetailMask);
// TEXTURE2D(_DetailAlbedoMap);
// SAMPLER(sampler_DetailAlbedoMap);
// TEXTURE2D(_DetailNormalMap);
// SAMPLER(sampler_DetailNormalMap);
// TEXTURE2D(_MetallicGlossMap);
// SAMPLER(sampler_MetallicGlossMap);
// TEXTURE2D(_SpecGlossMap);
// SAMPLER(sampler_SpecGlossMap);
// TEXTURE2D(_ClearCoatMap);
// SAMPLER(sampler_ClearCoatMap);
// TEXTURE2D(_FogMap);
// SAMPLER(sampler_FogMap);
// TEXTURE2D(_FogHeightNoise);
// SAMPLER(sampler_FogHeightNoise);

// #ifdef _SPECULAR_SETUP
//         #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
// #else
// #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
// #endif

// half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
// {
//     half4 specGloss;
//
//     #ifdef _METALLICSPECGLOSSMAP
//             specGloss = half4(SAMPLE_METALLICSPECULAR(uv));
//     #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//                 specGloss.a = albedoAlpha * _Smoothness;
//     #else
//                 specGloss.a *= _Smoothness;
//     #endif
//     #else // _METALLICSPECGLOSSMAP
//     #if _SPECULAR_SETUP
//                 specGloss.rgb = _SpecColor.rgb;
//     #else
//     specGloss.rgb = _Metallic.rrr;
//     #endif
//
//     #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//                 specGloss.a = albedoAlpha * _Smoothness;
//     #else
//     specGloss.a = _Smoothness;
//     #endif
//     #endif
//
//     return specGloss;
// }
//
// half SampleOcclusion(float2 uv)
// {
//     #ifdef _OCCLUSIONMAP
//     // TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
//     #if defined(SHADER_API_GLES)
//                 return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
//     #else
//                 half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
//                 return LerpWhiteTo(occ, _OcclusionStrength);
//     #endif
//     #else
//     return half(1.0);
//     #endif
// }
//
//
// // Returns clear coat parameters
// // .x/.r == mask
// // .y/.g == smoothness
// half2 SampleClearCoat(float2 uv)
// {
//     #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
//             half2 clearCoatMaskSmoothness = half2(_ClearCoatMask, _ClearCoatSmoothness);
//
//     #if defined(_CLEARCOATMAP)
//                 clearCoatMaskSmoothness *= SAMPLE_TEXTURE2D(_ClearCoatMap, sampler_ClearCoatMap, uv).rg;
//     #endif
//
//             return clearCoatMaskSmoothness;
//     #else
//     return half2(0.0, 1.0);
//     #endif  // _CLEARCOAT
// }
//
// void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
// {
//     #if defined(_PARALLAXMAP)
//             uv += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv);
//     #endif
// }
//
// // Used for scaling detail albedo. Main features:
// // - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
// // - No effect is applied if detailAlbedo is 0.5.
// half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
// {
//     // detailAlbedo = detailAlbedo * 2.0h - 1.0h;
//     // detailAlbedo *= _DetailAlbedoMapScale;
//     // detailAlbedo = detailAlbedo * 0.5h + 0.5h;
//     // return detailAlbedo * 2.0f;
//
//     // A bit more optimized
//     return half(2.0) * detailAlbedo * scale - scale + half(1.0);
// }
//
// half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
// {
//     #if defined(_DETAIL)
//             half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;
//
//             // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
//     #if defined(_DETAIL_SCALED)
//                 detailAlbedo = ScaleDetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
//     #else
//                 detailAlbedo = half(2.0) * detailAlbedo;
//     #endif
//
//             return albedo * LerpWhiteTo(detailAlbedo, detailMask);
//     #else
//     return albedo;
//     #endif
// }
//
// half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask)
// {
//     #if defined(_DETAIL)
//     #if BUMP_SCALE_NOT_SUPPORTED
//                 half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
//     #else
//                 half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), _DetailNormalMapScale);
//     #endif
//
//             // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
//             // For visual consistancy we going to do in all cases
//             detailNormalTS = normalize(detailNormalTS);
//
//             return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
//     #else
//     return normalTS;
//     #endif
// }
//
// inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
// {
//     half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
//     outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
//
//     half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
//     outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
//
//     #if _SPECULAR_SETUP
//             outSurfaceData.metallic = half(1.0);
//             outSurfaceData.specular = specGloss.rgb;
//     #else
//     outSurfaceData.metallic = specGloss.r;
//     outSurfaceData.specular = half3(0.0, 0.0, 0.0);
//     #endif
//
//     outSurfaceData.smoothness = specGloss.a;
//     outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
//     outSurfaceData.occlusion = SampleOcclusion(uv);
//     outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
//
//     #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
//             half2 clearCoat = SampleClearCoat(uv);
//             outSurfaceData.clearCoatMask       = clearCoat.r;
//             outSurfaceData.clearCoatSmoothness = clearCoat.g;
//     #else
//     outSurfaceData.clearCoatMask = half(0.0);
//     outSurfaceData.clearCoatSmoothness = half(0.0);
//     #endif
//
//     #if defined(_DETAIL)
//             half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
//             float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
//             outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
//             outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
//     #endif
// }

#endif
