#pragma once

#include "XHHYCharacter_Input_Eye.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "..\XHHYUtilityHLSL\XhhyAllUtilIncludes.hlsl" 
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"


half4 GetFinalBaseColor(Varyings input, ToonMap toon_map)
{
    half4 color = toon_map._BaseMap_Var;
    color *= _BaseColor; 

    color.rgb *= _PerCharacterBaseColorTint * _GlobalVolumeBaseColorTintColor;
    return color;
}

half4 GetFinalBaseColor(Varyings input)
{
    half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

    color *= _BaseColor;
    color.rgb *= _PerCharacterBaseColorTint * _GlobalVolumeBaseColorTintColor;
    return color;
}

half3 GetFinalNormalTS(Varyings input)
{
    return half3(0, 0, 1); //关闭时法线图的默认值，指向切线空间，如果转换回世界空间，将与原始顶点法线相同。
}

void DoClipTestToTargetAlphaValue(half alpha)
{
    #if _ALPHATEST_ON
    clip(alpha - _Cutoff);
    #endif
}


// 类似于URP的LitForwardPass.hlsl -> InitializeStandardLitSurfaceData()
ToonSurfaceData InitializeSurfaceData(Varyings input, ToonMap toonMap)
{
    ToonSurfaceData output = (ToonSurfaceData)0;

    // albedo(Base Color) & alpha
    half4 baseColorFinal = GetFinalBaseColor(input, toonMap);
    output.albedo = baseColorFinal.rgb;
    output.alpha = baseColorFinal.a;
    
    // 阿尔法剪辑和抖动淡出
    DoClipTestToTargetAlphaValue(output.alpha); // 一旦知道阿尔法值，就让clip()提前退出。
    
    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif
    
    // smoothness
    output.smoothness = 0.5;
    // normalTS
    output.normalTS = GetFinalNormalTS(input);
    return output;
}

//类似于URP的LitForwardPass.hlsl -> InitializeInputData()
ToonLightingData InitializeLightingData(Varyings input,inout  ToonSurfaceData surfaceData, ToonMap toonMap)
{
    ToonLightingData lightingData;
    lightingData.uv = input.uv;
    lightingData.positionWS = input.positionWS_ZOffsetFinalSum.xyz;

    // 注意：是否需要SafeNormalize()？我们可以直接使用normalize()吗？
    // 但是LitForwardPass.hlsl -> InitializeInputData() 使用SafeNormalized()
    // 我们就按照官方着色器的实现来做吧
    lightingData.viewDirectionWS = SafeNormalize(input.viewDirectionWS);

    half3 normalWS = input.normalWS_averageShadowAttenuation.xyz;
    normalWS = normalize(normalWS);

    lightingData.normalWS = normalWS;
    lightingData.PerPixelNormalizedPolygonNormalWS = normalWS; // 额外的：将归一化的插值顶点法线保存到lightingData中，不要让法线图影响这个向量。
    
    lightingData.selfLinearEyeDepth = Convert_SV_PositionZ_ToLinearViewSpaceDepth(input.positionCS.z);
    lightingData.ZOffsetFinalSum = input.positionWS_ZOffsetFinalSum.w;
    
    lightingData.normalVS = mul((half3x3)UNITY_MATRIX_V, lightingData.normalWS).xyz;

    lightingData.reflectionVectorVS = reflect(-lightingData.viewDirectionWS, lightingData.normalWS);
    // see URP Lighting.hlsl

    lightingData.NdotV = saturate(dot(lightingData.normalWS, lightingData.viewDirectionWS));
    lightingData.PolygonNdotV = saturate(dot(lightingData.PerPixelNormalizedPolygonNormalWS,
                                             lightingData.viewDirectionWS));

    lightingData.vertexColor = input.color;

    lightingData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    // in case we need it in the future

    return lightingData;
}

ToonMap GetTexture(Varyings input)
{
    ToonMap toonmap;
    toonmap._BaseMap_Var = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

    return toonmap;
}


// [Per Character script]
void ApplyPerCharacterEffectColorEditForwardPass(inout half3 color, ToonLightingData lightingData)
{
    color.rgb = color.rgb * _PerCharEffectTintColor + _PerCharEffectAddColor;
    // desaturate
    color = lerp(color, Luminance(color), _PerCharEffectDesaturatePercentage);

    // lerp
    color.rgb = lerp(color.rgb, _PerCharEffectLerpColor.rgb, _PerCharEffectLerpColor.a);

    // rim light (outline pass don't need this)
    // 使用PolygonNdotV而不是NdotV。
    // 因为ToonLightingData的NdotV是应用了法线图并进行了标准化处理的，我们不希望法线图在这里影响到边缘光。
    half NdotV = lightingData.PolygonNdotV;
    half rim = 1-NdotV;

    rim = pow(rim,_PerCharEffectRimSharpnessPower);

    // apply(additive)
   // color.rgb += (rim ) * _PerCharEffectRimColor; // 边缘光不能显示在脸上，因为它看起来很难看
}

// 受击闪白：独立一层菲涅尔，不跟角色边缘光共用参数。
// 用 PolygonNdotV（几何法线）而不是带法线图的 NdotV，闪白不会被法线图带歪。
void ApplyHitFlash(inout half3 color, ToonLightingData lightingData)
{
    half fresnel = 1.0 - saturate(lightingData.PolygonNdotV);
    fresnel = pow(fresnel, max(_HitFlashPower, 0.001));
    // 纯菲涅尔：只有边缘一圈闪，中间不加白，power 拉大边缘就越窄
    color.rgb += fresnel * _HitFlashColor.rgb * _HitFlashIntensity;
}


void ApplyDissolve(inout half3 color, Varyings input)
{
    #if _NILOTOON_DISSOLVE
    // 阈值来源：物体空间 Y 的归一化高度，在顶点阶段算好传过来
    half dissolveMapThresholdMapValue = input.dissolveHeight;
    half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;

    if (_DissolveEdgeDitherStrength > 0)
    {
        half edgeDist = saturate(abs(dissolve) / max(_DissolveBorderRange, 1e-4));
        half edgeMask = 1 - edgeDist;
        half edgeDither = (1 - edgeMask * _DissolveEdgeDitherStrength)
                        - NiloGetDitherThreshold(input.positionCS.xy);
        dissolve = min(dissolve, edgeDither);
    }

    // clip threshold
    clip(dissolve);

    // smoothstep 两个 edge 相等是未定义行为，会整块闪
    float borderRange = max(_DissolveBorderRange, 0.001);
    float topBorderRange = max(_DissolveTopBorderRange, 0.001);

    float dissolveBorderTintColorMask = smoothstep(_DissolveAmount + borderRange, _DissolveAmount, dissolveMapThresholdMapValue);
    dissolveBorderTintColorMask = lerp(0, dissolveBorderTintColorMask, _DissolveBorderTintColor.a);

    float dissolveTopBorderTintColorMask = smoothstep(_DissolveAmount + topBorderRange, _DissolveAmount, dissolveMapThresholdMapValue);
    dissolveTopBorderTintColorMask = lerp(0, dissolveTopBorderTintColorMask, _DissolveTopBorderTintColor.a);

    //对 "近阈值区域 "进行HDR色彩调剂 
    color += dissolveBorderTintColorMask * _DissolveBorderTintColor.rgb;
    color += dissolveTopBorderTintColorMask * _DissolveTopBorderTintColor.rgb ;
    #endif
}

void ApplyDissolve_th( Varyings input)
{
    #if _NILOTOON_DISSOLVE
    half dissolveMapThresholdMapValue = input.dissolveHeight;
    half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;

    if (_DissolveEdgeDitherStrength > 0)
    {
        half edgeDist = saturate(abs(dissolve) / max(_DissolveBorderRange, 1e-4));
        half edgeMask = 1 - edgeDist;
        half edgeDither = (1 - edgeMask * _DissolveEdgeDitherStrength)
                        - NiloGetDitherThreshold(input.positionCS.xy);
        dissolve = min(dissolve, edgeDither);
    }

    // clip threshold
    clip(dissolve);
    #endif
}

void ApplyOverrideOutputAlpha(inout half outputAlpha)
{
    //这里不值得用if()，所以用a?b:c
    outputAlpha = outputAlpha  ;
}

void BaseColorAlphaClipTest(Varyings input)
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if _ALPHATEST_ON
    DoClipTestToTargetAlphaValue(GetFinalBaseColor(input).a);
    #endif

    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif
    
    // 位置高度溶解统一走 ApplyDissolve，跟主 pass 同一套逻辑
    half3 dummy = half3(0, 0, 0);
    ApplyDissolve(dummy, input);
}

float4 BaseColorAlphaClipTest_AndDepthNormalColorOutput(Varyings input) : SV_TARGET
{
    BaseColorAlphaClipTest(input);

    return float4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS_averageShadowAttenuation.xyz, true)),
                  0.0, 0.0);
}

float4 BaseColorAlphaClipTest_AndNiloToonPrepassBufferColorOutput(Varyings input) : SV_TARGET
{
    BaseColorAlphaClipTest(input);

    float sceneLinearDepth = Convert_SV_PositionZ_ToLinearViewSpaceDepth(LoadSceneDepth(input.positionCS.xy));
    float selfLinearDepth = Convert_SV_PositionZ_ToLinearViewSpaceDepth(input.positionCS.z);
    if (sceneLinearDepth < selfLinearDepth - 0.001)
        clip(-1);

    return float4(0,1, 0, 1);
}
void ApplyPerCharacterBaseMapOverride(inout half3 originalSurfaceColor, Varyings varyings,
                                      ToonLightingData lightingData)
{
    #if _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
    half4 texValue = SAMPLE_TEXTURE2D(_PerCharacterBaseMapOverrideMap, sampler_BaseMap, varyings.uv *_PerCharacterBaseMapOverrideTiling);
    texValue.rgb *= _PerCharacterBaseMapOverrideTintColor;
    originalSurfaceColor = lerp(originalSurfaceColor,texValue.rgb, _PerCharacterBaseMapOverrideAmount * texValue.a);
    //    originalSurfaceColor = texValue.rgb;
    #endif
}