#ifndef XHHY_LIT_INPUT
#define XHHY_LIT_INPUT

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"


// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
float4 _ScaleOffset;
    //half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Smoothness;
    half _Metallic;
    half _BumpScale;
    half _OcclusionStrength;


half _Smoothness_Rain;
half _Metallic_Rain;
half _OcclusionStrength_Rain;
  

    half _DitherFadeoutAmount;

    // _NILOTOON_DISSOLVE
    half _DissolveAmount;
    half _DissolveSource;
    half _DissolveBorderRange;
    half4 _DissolveBorderTintColor;
    half _DissolveTopBorderRange;
    half4 _DissolveTopBorderTintColor;
    half _DissolveByVertexColor;

    // gameplay effect
    half4 _PerCharEffectTintColor;
    half4 _PerCharEffectAddColor;
    half4 _PerCharEffectLerpColor;
    float _PerCharacterRimLightIntensity;
    half4 _PerCharEffectRimColor;
    half _PerCharEffectRimSharpnessPower;
    half _PerCharEffectDesaturatePercentage;
    float _LocalFadeSize;
float4 _BaseColor_OtherWorld;

float4 _OutlineColor;
float _OutlineWidth;
float _OutlineWidthExtraMultiplier;

    CBUFFER_END
float4 _GIColor_OtherWorld;
float _MaskRadius_OtherWorld;
float _MaskDeflate_OtherWorld;
float3 _MaskPosition_OtherWorld;
float _GlobalMetallic;
float _GlobalSmoothness;
float4 _GlobalColor;
float _GlobalOcclusion;
float _BakedGIInt;

float _waterHeight;



TEXTURE2D(_MetallicGlossMap);
SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_DissolveThresholdMap);
//一些复用的sampler
SAMPLER(sampler_linear_clamp);
SAMPLER(sampler_Point_clamp);
SAMPLER(sampler_linear_repeat);


#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)


struct InputData_XHHY
{
    float3  positionWS;
    float4  positionCS;
    float3  normalWS;
    half3   viewDirectionWS;
    float4  shadowCoord;
    half    fogCoord;
    half3   vertexLighting;
    half3   bakedGI;
    float2  normalizedScreenSpaceUV;
    half4   shadowMask;
    half3x3 tangentToWorld;
    float2 screenUV;
    float offsetDepth;
    
    #if defined(DEBUG_DISPLAY)
    half2   dynamicLightmapUV;
    half2   staticLightmapUV;
    float3  vertexSH;

    half3 brdfDiffuse;
    half3 brdfSpecular;
    float2 uv;
    uint mipCount;

    // texelSize :
    // x = 1 / width
    // y = 1 / height
    // z = width
    // w = height
    float4 texelSize;

    // mipInfo :
    // x = quality settings minStreamingMipLevel
    // y = original mip count for texture
    // z = desired on screen mip level
    // w = loaded mip level
    float4 mipInfo;
    #endif
};

float2 SetBaseMapUV(float2 texcoord)
{
    texcoord.xy = texcoord.xy * _ScaleOffset.xy + _ScaleOffset.zw;
    return texcoord;
}



half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

    specGloss = half4(SAMPLE_METALLICSPECULAR(uv));

    return specGloss;
}




half2 SampleClearCoat(float2 uv)
{
    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoatMaskSmoothness = half2(_ClearCoatMask, _ClearCoatSmoothness);

    #if defined(_CLEARCOATMAP)
    clearCoatMaskSmoothness *= SAMPLE_TEXTURE2D(_ClearCoatMap, sampler_ClearCoatMap, uv).rg;
    #endif

    return clearCoatMaskSmoothness;
    #else
    return half2(0.0, 1.0);
    #endif  // _CLEARCOAT
}

void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
{
    #if defined(_PARALLAXMAP)
    uv += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv);
    #endif
}

// Used for scaling detail albedo. Main features:
// - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
// - No effect is applied if detailAlbedo is 0.5.
half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
{
    // detailAlbedo = detailAlbedo * 2.0h - 1.0h;
    // detailAlbedo *= _DetailAlbedoMapScale;
    // detailAlbedo = detailAlbedo * 0.5h + 0.5h;
    // return detailAlbedo * 2.0f;

    // A bit more optimized
    return half(2.0) * detailAlbedo * scale - scale + half(1.0);
}

half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
{
    #if defined(_DETAIL)
    half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;

    // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
    #if defined(_DETAIL_SCALED)
    detailAlbedo = ScaleDetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
    #else
    detailAlbedo = half(2.0) * detailAlbedo;
    #endif

    return albedo * LerpWhiteTo(detailAlbedo, detailMask);
    #else
    return albedo;
    #endif
}

half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask)
{
    #if defined(_DETAIL)
    #if BUMP_SCALE_NOT_SUPPORTED
    half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
    #else
    half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), _DetailNormalMapScale);
    #endif

    // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
    // For visual consistancy we going to do in all cases
    detailNormalTS = normalize(detailNormalTS);

    return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
    #else
    return normalTS;
    #endif
}


half3 ApplyNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = half(1.0))
{
  //  #ifdef _NORMALMAP
    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    #if BUMP_SCALE_NOT_SUPPORTED
    return UnpackNormal(n);
    #else
    return UnpackNormalScale(n, scale);
    #endif
    /*#else
    return half3(0.0h, 0.0h, 1.0h);
    #endif*/
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) /** _GlobalColor*/;
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 _MetallicGlossMap_Var = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = _MetallicGlossMap_Var.r * _Metallic  + _GlobalMetallic;
    outSurfaceData.metallic = clamp(0,1,outSurfaceData.metallic);
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif

    outSurfaceData.smoothness = _MetallicGlossMap_Var.g * _Smoothness + _GlobalSmoothness;
    outSurfaceData.smoothness = clamp(0,1,outSurfaceData.smoothness);
    outSurfaceData.normalTS = ApplyNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = _MetallicGlossMap_Var.b * _OcclusionStrength;
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask       = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
    #else
    outSurfaceData.clearCoatMask = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    #endif

    #if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
    #endif

  
}


inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData, float3 postionWS, float3 normolWS)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) /** _GlobalColor*/;
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 _MetallicGlossMap_Var = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = _MetallicGlossMap_Var.r * _Metallic  + _GlobalMetallic * _Metallic_Rain;
    outSurfaceData.metallic = clamp(0,1,outSurfaceData.metallic);
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif

    outSurfaceData.smoothness = _MetallicGlossMap_Var.g * _Smoothness + _GlobalSmoothness * _Smoothness_Rain;
    outSurfaceData.smoothness = clamp(0,1,outSurfaceData.smoothness);
    outSurfaceData.normalTS = ApplyNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = _MetallicGlossMap_Var.b * _OcclusionStrength + _GlobalOcclusion * _OcclusionStrength_Rain;
    
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask       = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
    #else
    outSurfaceData.clearCoatMask = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    #endif

    #if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
    #endif

    #if defined(_Layer_MULX2)
    half layerMask ;
    normolWS = normalize(normolWS);
    float temp_cast_5 = ((0.001 + (_LayerThreshold - 0.0) * (1.0 - 0.001) / (1.0 - 0.0)));
    float BlendAlpha85 = pow( saturate( normolWS.y  + _LayerPower  ) , temp_cast_5 );
    //layerMask =   pow( saturate( _LayerPower + normolWS.y  ) , 0.001 + _LayerThreshold );
    layerMask = BlendAlpha85  ;

    float2 layerUV = postionWS.xz * _LayerAlbedoMap_ST.xy + _LayerAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyLayerAlbedo(layerUV,outSurfaceData.albedo,layerMask);
    outSurfaceData.normalTS = ApplyLayerNormal(layerUV,outSurfaceData.normalTS,layerMask);
    half3 MSO = half3(outSurfaceData.metallic,outSurfaceData.smoothness,outSurfaceData.occlusion);
    MSO = ApplyLayerMSO(layerUV,MSO,layerMask);
    outSurfaceData.metallic = MSO.r;
    outSurfaceData.smoothness = MSO.g;
    outSurfaceData.occlusion = MSO.b;
    #endif
}


inline void InitializeStandardLitSurfaceData_WaterTablets(float2 uv, out SurfaceData outSurfaceData, float3 postionWS, float3 normolWS)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) /** _GlobalColor*/;
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 _MetallicGlossMap_Var = 1;
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = _MetallicGlossMap_Var.r * _Metallic  + _GlobalMetallic * _Metallic_Rain;
    outSurfaceData.metallic = clamp(0,1,outSurfaceData.metallic);
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif

    outSurfaceData.smoothness = _MetallicGlossMap_Var.g * _Smoothness + _GlobalSmoothness * _Smoothness_Rain;
    outSurfaceData.smoothness = clamp(0,1,outSurfaceData.smoothness);
    outSurfaceData.normalTS = ApplyNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = _MetallicGlossMap_Var.b * _OcclusionStrength + _GlobalOcclusion * _OcclusionStrength_Rain;
    
    outSurfaceData.emission = 0;

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask       = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
    #else
    outSurfaceData.clearCoatMask = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    #endif

    #if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
    #endif

    #if defined(_Layer_MULX2)
    half layerMask ;
    normolWS = normalize(normolWS);
    float temp_cast_5 = ((0.001 + (_LayerThreshold - 0.0) * (1.0 - 0.001) / (1.0 - 0.0)));
    float BlendAlpha85 = pow( saturate( normolWS.y  + _LayerPower  ) , temp_cast_5 );
    //layerMask =   pow( saturate( _LayerPower + normolWS.y  ) , 0.001 + _LayerThreshold );
    layerMask = BlendAlpha85  ;

    float2 layerUV = postionWS.xz * _LayerAlbedoMap_ST.xy + _LayerAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyLayerAlbedo(layerUV,outSurfaceData.albedo,layerMask);
    outSurfaceData.normalTS = ApplyLayerNormal(layerUV,outSurfaceData.normalTS,layerMask);
    half3 MSO = half3(outSurfaceData.metallic,outSurfaceData.smoothness,outSurfaceData.occlusion);
    MSO = ApplyLayerMSO(layerUV,MSO,layerMask);
    outSurfaceData.metallic = MSO.r;
    outSurfaceData.smoothness = MSO.g;
    outSurfaceData.occlusion = MSO.b;
    #endif
}

inline void InputData_XHHYToInputData(InputData_XHHY input_data_xhhy, out InputData outSurfaceData)
{
    outSurfaceData.positionWS = input_data_xhhy.positionWS;
    outSurfaceData.positionCS = input_data_xhhy.positionCS;
   outSurfaceData.normalWS = input_data_xhhy.normalWS ;
   outSurfaceData.viewDirectionWS = input_data_xhhy.viewDirectionWS;
   outSurfaceData.shadowCoord = input_data_xhhy.shadowCoord;
    outSurfaceData.fogCoord =  input_data_xhhy.fogCoord;
   outSurfaceData.vertexLighting =  input_data_xhhy.vertexLighting;
   outSurfaceData.bakedGI = input_data_xhhy.bakedGI;
   outSurfaceData.normalizedScreenSpaceUV = input_data_xhhy.normalizedScreenSpaceUV;
   outSurfaceData.shadowMask = input_data_xhhy.shadowMask;
   outSurfaceData.tangentToWorld = input_data_xhhy.tangentToWorld;

    #if defined(DEBUG_DISPLAY)
  outSurfaceData.dynamicLightmapUV = input_data_xhhy.dynamicLightmapUV;
   outSurfaceData.staticLightmapUV =  input_data_xhhy.staticLightmapUV;
   outSurfaceData.vertexSH = input_data_xhhy.vertexSH;

   outSurfaceData.brdfDiffuse = input_data_xhhy.brdfDiffuse;
   outSurfaceData.brdfSpecular = input_data_xhhy.brdfSpecular;
   outSurfaceData.uv = input_data_xhhy.uv;
   outSurfaceData.mipCount = input_data_xhhy.mipCount;

    // texelSize :
    // x = 1 / width
    // y = 1 / height
    // z = width
    // w = height
   outSurfaceData.texelSize = input_data_xhhy.texelSize;

    // mipInfo :
    // x = quality settings minStreamingMipLevel
    // y = original mip count for texture
    // z = desired on screen mip level
    // w = loaded mip level
   outSurfaceData.mipInfo =  input_data_xhhy.mipInfo;
    #endif
}



void ApplyPerCharacterEffectColorEdit(inout half3 color, float NdotV)
{
    //////////////////////////////////////////////////////////////////////////////////
    // （不是照明，通常是为了游戏的需要，如选择角色）。
    //////////////////////////////////////////////////////////////////////////////////
    // mul add
    color.rgb = color.rgb * _PerCharEffectTintColor.rgb + _PerCharEffectAddColor.rgb;

    // desaturate
    color = lerp(color, dot(color, real3(0.2126729, 0.7151522, 0.0721750)), _PerCharEffectDesaturatePercentage);

    // lerp
    color.rgb = lerp(color.rgb, _PerCharEffectLerpColor.rgb, _PerCharEffectLerpColor.a);

    // rim light (outline pass don't need this)

   
    half rim = 1 - NdotV;
    rim = abs(rim);
    rim = pow(rim, _PerCharEffectRimSharpnessPower);

    // apply(additive)
    color.rgb += rim * _PerCharacterRimLightIntensity * _PerCharEffectRimColor.rgb; // 
  
}

void ApplyDissolve(inout half3 color, float2 uv)
{
    #if _NILOTOON_DISSOLVE
                // read black and white map
                half dissolveMapThresholdMapValue = SAMPLE_TEXTURE2D(_DissolveThresholdMap,sampler_linear_repeat, uv).g;
              //  dissolveMapThresholdMapValue =  _DissolveByVertexColor == 0?dissolveMapThresholdMapValue:vertexcolor.z;
                half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;

                // clip threshold
                clip(dissolve);
                float dissolveBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveBorderRange, _DissolveAmount, dissolveMapThresholdMapValue);
                dissolveBorderTintColorMask = lerp(0,dissolveBorderTintColorMask,_DissolveBorderTintColor.a);

                float dissolveTopBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveTopBorderRange, _DissolveAmount, dissolveMapThresholdMapValue);
                dissolveTopBorderTintColorMask = lerp(0,dissolveTopBorderTintColorMask,_DissolveTopBorderTintColor.a);
                //对 "近阈值区域 "进行HDR色彩调剂

                float3 DissolveBorderTintColor = dissolveBorderTintColorMask * _DissolveBorderTintColor.rgb;
                DissolveBorderTintColor = lerp(DissolveBorderTintColor,_DissolveTopBorderTintColor.rgb,dissolveTopBorderTintColorMask);
                color += DissolveBorderTintColor;

    
               // color = dissolveTopBorderTintColorMask;
                //color += dissolveTopBorderTintColorMask * _DissolveTopBorderTintColor.rgb ;
    #endif
}


void NiloDoDitherFadeoutClip(float2 SV_POSITIONxy, float ditherOpacity)
{
    // copy from https://docs.unity3d.com/Packages/com.unity.shadergraph@10.3/manual/Dither-Node.html?q=dither
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
    };
    uint index = (uint(SV_POSITIONxy.x) % 4) * 4 + uint(SV_POSITIONxy.y) % 4;
    clip(ditherOpacity - DITHER_THRESHOLDS[index]);
}




#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
