#pragma once

#include "XHHYCharacter_Input.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "..\XHHYUtilityHLSL\XhhyAllUtilIncludes.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"


#include "XHHYCharacter_LightingEquationNew.hlsl"

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//分割共享函数（第二步：为照明计算准备数据结构）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if AnyBaseMapStackingLayerEnabled
half4 GetStackingLayerRGBAResult(
    float2 uv,
    float4 baseMapStackingLayerTexUVScaleOffset,
    float2 baseMapStackingLayer1TexUVAnimSpeed,
    half4 baseMapStackingLayerTintColor,
    half4 baseMapStackingLayerMaskTexChannel,
    Texture2D baseMapStackingLayerTex,
    Texture2D baseMapStackingLayerMaskTex,
    SamplerState baseMapStackingLayerTexSamplerState)
{
    float2 stackingLayerUV = (uv + baseMapStackingLayerTexUVScaleOffset.zw + frac(
        _Time.yy * baseMapStackingLayer1TexUVAnimSpeed)) * baseMapStackingLayerTexUVScaleOffset.xy;
    //共享采样器以避免采样器的最大计数=16的限制
    half4 stackingLayerRGBAResult = SAMPLE_TEXTURE2D(baseMapStackingLayerTex, baseMapStackingLayerTexSamplerState,
                                                     stackingLayerUV) * baseMapStackingLayerTintColor;

    // 对于遮罩纹理，我们重新使用_BaseMap的采样器以减少采样器的数量，因为在大多数情况下，设置应该是相同的 
    half stackingLayerMask = dot(
        SAMPLE_TEXTURE2D(baseMapStackingLayerMaskTex, sampler_BaseMap, uv), baseMapStackingLayerMaskTexChannel);
    stackingLayerRGBAResult.a *= stackingLayerMask;

    return stackingLayerRGBAResult;
}

void ApplyStackingLayer(inout half4 colorRGBA, half4 layerColorRGBA)
{
    if (_IsAddColor == 0)
    {
        colorRGBA.rgb = lerp(colorRGBA.rgb, layerColorRGBA.rgb, layerColorRGBA.a);
        colorRGBA.a = max(colorRGBA.a, layerColorRGBA.a);
    }
    else
    {
        colorRGBA.rgb += layerColorRGBA.rgb * layerColorRGBA.a;
    }
}
#endif

half4 GetFinalBaseColor(Varyings input, ToonMap toon_map)
{
    half4 color = toon_map._BaseMap_Var;

    color *= _BaseColor; //编辑rgba（也包括alpha，不只是rgb），因为_BaseColor是每个材料，使用_BaseColor.a来控制alpha应该是用户有意的。

    //在BaseMap编辑完成后，添加10个Photoshop正常混合层。这是为脸部化妆、贴纸、标志、纹身等设计的。
    #if _BASEMAP_STACKING_LAYER1
    ApplyStackingLayer(color, GetStackingLayerRGBAResult(
                           input.uv,
                           _BaseMapStackingLayer1TexUVScaleOffset,
                           _BaseMapStackingLayer1TexUVAnimSpeed,
                           _BaseMapStackingLayer1TintColor,
                           _BaseMapStackingLayer1MaskTexChannel,
                           _BaseMapStackingLayer1Tex,
                           _BaseMapStackingLayer1MaskTex,
                           sampler_BaseMapStackingLayer1Tex)); // 只有前两层使用它自己的采样器，以便让用户有更多的控制。
    #endif
    #if _BASEMAP_STACKING_LAYER2
    ApplyStackingLayer(color, GetStackingLayerRGBAResult(
                           input.uv,
                           _BaseMapStackingLayer2TexUVScaleOffset,
                           _BaseMapStackingLayer2TexUVAnimSpeed,
                           _BaseMapStackingLayer2TintColor,
                           _BaseMapStackingLayer2MaskTexChannel,
                           _BaseMapStackingLayer2Tex,
                           _BaseMapStackingLayer2MaskTex,
                           sampler_BaseMapStackingLayer2Tex)); // 只有前两层使用它自己的采样器，以便让用户有更多的控制。
    #endif


    //每一个char的最终版本+全局编辑
  //  color.rgb *= _PerCharacterBaseColorTint * _GlobalVolumeBaseColorTintColor;
    // 只编辑RGB，因为它们不是按材料编辑的，按字符/全局编辑alpha应该不是用户故意的。
    #if _DECAL
    color.rgb = color.rgb*(1- toon_map._DecalMap_Var.a)+ toon_map._DecalMap_Var.a* toon_map._DecalMap_Var.rgb;
    #endif

    return color;
}

half4 GetFinalBaseColor(Varyings input)
{
    half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

    color *= _BaseColor; //编辑rgba（也包括alpha，不只是rgb），因为_BaseColor是每个材料，使用_BaseColor.a来控制alpha应该是用户有意的。

    //在BaseMap编辑完成后，添加10个Photoshop正常混合层。这是为脸部化妆、贴纸、标志、纹身等设计的。
    #if _BASEMAP_STACKING_LAYER1
    ApplyStackingLayer(color, GetStackingLayerRGBAResult(
                           input.uv,
                           _BaseMapStackingLayer1TexUVScaleOffset,
                           _BaseMapStackingLayer1TexUVAnimSpeed,
                           _BaseMapStackingLayer1TintColor,
                           _BaseMapStackingLayer1MaskTexChannel,
                           _BaseMapStackingLayer1Tex,
                           _BaseMapStackingLayer1MaskTex,
                           sampler_BaseMapStackingLayer1Tex)); // 只有前两层使用它自己的采样器，以便让用户有更多的控制。
    #endif
    #if _BASEMAP_STACKING_LAYER2
    ApplyStackingLayer(color, GetStackingLayerRGBAResult(
                           input.uv,
                           _BaseMapStackingLayer2TexUVScaleOffset,
                           _BaseMapStackingLayer2TexUVAnimSpeed,
                           _BaseMapStackingLayer2TintColor,
                           _BaseMapStackingLayer2MaskTexChannel,
                           _BaseMapStackingLayer2Tex,
                           _BaseMapStackingLayer2MaskTex,
                           sampler_BaseMapStackingLayer2Tex)); // 只有前两层使用它自己的采样器，以便让用户有更多的控制。
    #endif


    //每一个字符的最终版本+全局编辑
    color.rgb *= _PerCharacterBaseColorTint ;
    // 只编辑RGB，因为它们不是按材料编辑的，按字符/全局编辑alpha应该不是用户故意的。
    #if _DECAL
    //color.rgb = color.rgb*(1- toon_map._DecalMap_Var.a)+ toon_map._DecalMap_Var.a* toon_map._DecalMap_Var.rgb;
    #endif

    return color;
}

half3 GetFinalEmissionColor(Varyings input, half3 baseColor, ToonMap toonmap)
{
    #if _EMISSION
    float2 uv = input.uv * _EmissionMapTilingXyOffsetZw.xy + _EmissionMapTilingXyOffsetZw.zw;

    half4 emissionMapSampleValue = tex2D(_EmissionMap, uv);
    // RGB 模式忽略 alpha；单通道模式用遮罩点出 R/G/B/A 之一，再铺成强度
    half singleChannel = dot(emissionMapSampleValue, _EmissionMapSingleChannelMask);
    half3 emissionResult = lerp(emissionMapSampleValue.rgb, singleChannel.xxx, _EmissionMapUseSingleChannelOnly);

    emissionResult *= _EmissionColor.rgb * _EmissionIntensity;
    emissionResult *= lerp(1, baseColor, _MultiplyBaseColorToEmissionColor); //让用户选择性地将基色与发射色混合在一起
    #if _DECAL
    #if _DECALCOVERAGEEMISSION
    emissionResult *=1-toonmap._DecalMap_Var.a;
    #endif
    #endif

    return emissionResult;
    #else
    return 0; // 关闭时默认的发射值为黑色
    #endif
}

//默认使用
half GetFinalOcculsion(Varyings input, half4 _MaskMap_Var)
{
    // #if /*!_ISFACE*/ _OCCLUSIONMAP

   // half occlusionValue = _MaskMap_Var.g / 0.4;
    // occlusionValue = invLerpClamp(_OcclusionRemapStart, _OcclusionRemapEnd, occlusionValue); //应该先重新映射。
   // occlusionValue = lerp(1, occlusionValue, _OcclusionStrength); //然后应用每个材料和每个体积的淡出。
    return  _MaskMap_Var.g;
    /*#else
    return 1; // 关闭时默认的闭塞值为1
    #endif*/
}

half GetFinalSmoothness(Varyings input)
{
    half smoothnessResult = 0.5;
    return smoothnessResult;
}

//返回.rgb是镜面RGB，返回.a是镜面遮罩 
half4 GetFinalSpecularRGBA(Varyings input, half3 baseColor, half4 _MaskMap_Var)
{
    // #if !_ISFACE /*_SPECULARHIGHLIGHTS*/

    half specularMask = _MaskMap_Var.r;
    half metallicMask = step(0.95, _MaskMap_Var.r); //找到金属区域
    //specularMask = invLerpClamp(_SpecularMapRemapStart, _SpecularMapRemapEnd, specularMask); //应该先重新映射。
    half3 specularRGBResult = _SpecularColor * (specularMask * _SpecularIntensity); // 然后应用强度/颜色
    // half3 MetallicSpecularRGBResult2 = _MetallicSpecularColor * _MetallicSpecularIntensity;// 然后应用金属强度/颜色
    //  specularRGBResult = metallicMask == 1? MetallicSpecularRGBResult2 : specularRGBResult;
    specularRGBResult *= lerp(1, baseColor, _MultiplyBaseColorToSpecularColor); //让用户有选择地将底色与镜面色相混合

    return half4(specularRGBResult, specularMask);
    /*#else
    return 0; // 关闭时默认的镜面值为0
    #endif*/
}

half4 GetFinalSpecularRGBA(Varyings input, half3 baseColor, half4 _MaskMap_Var, ToonSurfaceData surfaceData)
{
    //#if !_ISFACE /*_SPECULARHIGHLIGHTS*/

    half specularMask = _MaskMap_Var.b;
    //specularMask = invLerpClamp(_SpecularMapRemapStart, _SpecularMapRemapEnd, specularMask); //应该先重新映射。
    half3 specularRGBResult = _SpecularColor * (specularMask * _SpecularIntensity); // 然后应用强度/颜色
    specularRGBResult *= lerp(1, baseColor, max(_MultiplyBaseColorToSpecularColor, surfaceData.metallicMask));
    //让用户有选择地将底色与镜面色相混合

    return half4(specularRGBResult, specularMask);
    /*#else
    return 0; // 关闭时默认的镜面值为0
    #endif*/
}

half3 GetFinalNormalTS(Varyings input)
{
    #if _NORMALMAP
    // Not using UnpackNormal(...) for mobile & switch now
    // because we want to unify mobile and non mobile detail normalmap rendering, since performance differernce is small
    // honestly no one will care 1 more MUL in 2021, even for mobile
    return UnpackNormalScale(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv), _BumpScale);
    #else
    return half3(0,0,1); //default value of normal map when turn off, pointing out in tangent space, if converted back to world space it will be the same as raw vertex normal
    #endif
}

void DoClipTestToTargetAlphaValue(half alpha)
{
    #if _ALPHATEST_ON
    clip(alpha - _Cutoff);
    #endif
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//片段共享函数（步骤1.5：编辑视差uv）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 注意：本节不会影响细节uv。
// 直接从URP的LitInput.hlsl中复制。
void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
{
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////
// 从URP10.2.2的LitInput.hlsl的细节图逻辑中复制和简化 - START
// 本节只存在 #if _DETAIL
#if _DETAIL
    // 用于缩放细节反照率。主要特点。
    // - 取决于detailAlbedo是变亮还是变暗，比例放大的效果。
    // - 如果detailAlbedo为0.5，则没有效果。
    half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
    {
        // A bit more optimized
        return 2.0h * detailAlbedo * scale - scale + 1.0h;
    }
 

#endif
// 从URP10.2.2的LitInput.hlsl的细节图逻辑中复制和简化 - END
///////////////////////////////////////////////////////////////////////////////////////////////////////////

Mask SetMask(ToonMap toonmap)
{
    Mask mask;
    mask.colormask1 =  step(toonmap._MaskMap_Var.a, 0.2f);
    mask.colormask2 = step(0.2f, toonmap._MaskMap_Var.a) * step(toonmap._MaskMap_Var.a, 0.4f);
    mask.colormask3 = step(0.4f, toonmap._MaskMap_Var.a) * step(toonmap._MaskMap_Var.a, 0.6f);
    mask.colormask4 = step(0.6f, toonmap._MaskMap_Var.a) * step(toonmap._MaskMap_Var.a, 0.85f);
    mask.colormask5 = step(0.85f, toonmap._MaskMap_Var.a) ;

    //合并计算

    float4 MaskMap_Var_a = toonmap._MaskMap_Var.a;
    float4 a = float4(0, 0.2, 0.4, 0.6);
    float4 b = float4(0.2, 0.4, 0.6, 0.85);
    float4 mask1 = step(a, MaskMap_Var_a) * step(MaskMap_Var_a, b);
    float4 mask2 = mask.colormask5;

    mask.colormask1 = mask1.x;
    mask.colormask2 = mask1.y;
    mask.colormask3 = mask1.z;
    mask.colormask4 = mask1.w;
    mask.colormask5 = step(0.85f, toonmap._MaskMap_Var.a) ;
    mask.metallicmask = smoothstep(0.9f, 0.95f, toonmap._MaskMap_Var.r);
    return mask;
}

//分层处理数据
void setSpecularData(Mask mask)
{
    float4 specularIntensity1 = float4(_SpecularIntensity, _MultiplyBaseColorToSpecularColor, _Multiply,
                                       _SpecularAreaRemapRange);
    float4 specularIntensityColor_SpecularAreaRemapMidPoint1 = float4(_SpecularColor, _SpecularAreaRemapMidPoint);

    float4 specularIntensity2 = float4(_SpecularIntensity2, _MultiplyBaseColorToSpecularColor2, _Multiply2,
                                       _SpecularAreaRemapRange2);
    float4 specularIntensityColor_SpecularAreaRemapMidPoint2 = float4(_SpecularColor2, _SpecularAreaRemapMidPoint2);

    float4 specularIntensity3 = float4(_SpecularIntensity3, _MultiplyBaseColorToSpecularColor3, _Multiply3,
                                       _SpecularAreaRemapRange3);
    float4 specularIntensityColor_SpecularAreaRemapMidPoint3 = float4(_SpecularColor3, _SpecularAreaRemapMidPoint3);

    float4 specularIntensity4 = float4(_SpecularIntensity4, _MultiplyBaseColorToSpecularColor4, _Multiply4,
                                       _SpecularAreaRemapRange4);
    float4 specularIntensityColor_SpecularAreaRemapMidPoint4 = float4(_SpecularColor4, _SpecularAreaRemapMidPoint4);

    float4 specularIntensity5 = float4(_SpecularIntensity5, _MultiplyBaseColorToSpecularColor5, _Multiply5,
                                       _SpecularAreaRemapRange5);
    float4 specularIntensityColor_SpecularAreaRemapMidPoint5 = float4(_SpecularColor5, _SpecularAreaRemapMidPoint5);

    float4 specularIntensity = mask.colormask1 * specularIntensity1 + mask.colormask2 * specularIntensity2 + mask.
        colormask3 * specularIntensity3 + mask.colormask4 * specularIntensity4 + mask.colormask5 * specularIntensity5;
    float4 specularIntensityColor_SpecularAreaRemapMidPoint = mask.colormask1 *
        specularIntensityColor_SpecularAreaRemapMidPoint1 + mask.colormask2 *
        specularIntensityColor_SpecularAreaRemapMidPoint2 + mask.colormask3 *
        specularIntensityColor_SpecularAreaRemapMidPoint3 + mask.colormask4 *
        specularIntensityColor_SpecularAreaRemapMidPoint4 + mask.colormask5 *
        specularIntensityColor_SpecularAreaRemapMidPoint5;

    float4 metallicspecularIntensity5 = float4(_MetallicSpecularIntensity, _MetallicMultiplyBaseColorToSpecularColor,
                                               _MetallicMultiply, _MetallicSpecularAreaRemapRange);
    float4 metallicspecularIntensityColor_SpecularAreaRemapMidPoint1 = float4(
        _MetallicSpecularColor.rgb, _MetallicSpecularAreaRemapMidPoint);

    specularIntensity = lerp(specularIntensity, metallicspecularIntensity5, mask.metallicmask);
    specularIntensityColor_SpecularAreaRemapMidPoint = lerp(specularIntensityColor_SpecularAreaRemapMidPoint,
                                                            metallicspecularIntensityColor_SpecularAreaRemapMidPoint1,
                                                            mask.metallicmask);
    // _SpecularIntensity = specularIntensity.x;
    _SpecularIntensity = specularIntensity.x;
    //  _SpecularIntensity = mask.colormask4;
    _SpecularColor = specularIntensityColor_SpecularAreaRemapMidPoint.rgb;
    _MultiplyBaseColorToSpecularColor = specularIntensity.y;
    _Multiply = specularIntensity.z;
    // _SpecularAreaRemapUsage = specularIntensity.w;
    _SpecularAreaRemapMidPoint = specularIntensityColor_SpecularAreaRemapMidPoint.w;
    _SpecularAreaRemapRange = specularIntensity.w;
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

// 类似于URP的LitForwardPass.hlsl -> InitializeStandardLitSurfaceData()
ToonSurfaceData InitializeSurfaceData(Varyings input, ToonMap toonMap, Mask mask)
{
    ToonSurfaceData output = (ToonSurfaceData)0;

    // albedo(Base Color) & alpha
    half4 baseColorFinal = GetFinalBaseColor(input, toonMap);
    output.albedo = baseColorFinal.rgb;
    output.metallicMask = mask.metallicmask;
   // output.albedo *= (1 - output.metallicMask) * (1 - _MetallicColorIntensity) + _MetallicColorIntensity;
   // output.albedo *= max(1-mask.metallicmask,0.5);
    output.alpha = baseColorFinal.a;
    //应用金属度
    // output.albedo = baseColorFinal.rgb * (1-toonMap._MaskMap_Var.r);
    // 阿尔法剪辑和抖动淡出
    DoClipTestToTargetAlphaValue(output.alpha); // 一旦知道阿尔法值，就让clip()提前退出。
    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif

    // occlusion
    output.occlusion = GetFinalOcculsion(input, toonMap._MaskMap_Var);

    // smoothness
    output.smoothness = 0.5;

    // normalTS
   // output.normalTS = GetFinalNormalTS(input);
    output.normalTS.rg = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap,input.uv).rg *2 -1 ;
    output.normalTS.b = 1;
    output.normalTS = ApplyNormal(input.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    output.uv8 = input.uv8;
    ///////////////////////////////////////////////////////////
    //在详细反照率和法线之后。
    //做所有需要详细反照率和法线的函数
    ///////////////////////////////////////////////////////////
    // emission
    output.emission = GetFinalEmissionColor(input, output.albedo, toonMap);

    // specular & roughness
    half4 specularResult = GetFinalSpecularRGBA(input, output.albedo, toonMap._MaskMap_Var);
    output.specular = specularResult.rgb * toonMap._MaskMap_Var.g * toonMap._MaskMap_Var.r /* * (1-mask.metallicmask) */ ;
    output.specularMask = specularResult.a;

    /*#if _ISFACE
    output.metallicMask = 0;
    #endif*/


    output.colormask1 = mask.colormask1;
    output.colormask2 = mask.colormask2;
    output.colormask3 = mask.colormask3;
    output.colormask4 = mask.colormask4;
    output.colormask5 = mask.colormask5;
    return output;
}

//类似于URP的LitForwardPass.hlsl -> InitializeInputData()
ToonLightingData InitializeLightingData(Varyings input, inout ToonSurfaceData surfaceData, ToonMap toonMap)
{
    ToonLightingData lightingData;

    lightingData.uv = input.uv;
    lightingData.positionWS = input.positionWS_ZOffsetFinalSum.xyz;

    lightingData.viewDirectionWS = SafeNormalize(input.viewDirectionWS);
    #if VaryingsHasTangentWS
    lightingData.tangentWS = SafeNormalize(input.tangentWS.xyz);
    #endif
    half3 normalWS = input.normalWS_averageShadowAttenuation.xyz;

    normalWS = normalize(normalWS);

    lightingData.normalWS = normalWS;
    lightingData.PerPixelNormalizedPolygonNormalWS = normalWS; // 额外的：将归一化的插值顶点法线保存到lightingData中，不要让法线图影响这个向量。

    /*
    #if VaryingsHasTangentWS
    // [你可以从URP的LitForwardPass.hlsl -> InitializeInputData()中引用这一部分]
    half sgn = input.tangentWS.w; //应该是+1或-1。这里不需要应用unity_WorldTransformParams，因为它已经在顶点着色器中应用过了。

    // 如果你只使用 normalWS，就不需要对 bitangentWS 进行规范化处理。
    // 因为当我们把normalWS的结果分配给lightingData时，我们还是会在最后对normalWS进行归一化处理。
    half3 bitangentWS = sgn * cross(normalWS, input.tangentWS.xyz);

    lightingData.TBN_WS = half3x3(input.tangentWS.xyz, bitangentWS, normalWS);

    // TODO：参见LitForwardPass.hlsl中的 "half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS); "。
    //看看下面一行是否是正确的实现
    lightingData.viewDirectionTS = TransformWorldToTangent(lightingData.viewDirectionWS,lightingData.TBN_WS);
    #endif
    */

    #if _NORMALMAP 
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS_averageShadowAttenuation.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS_averageShadowAttenuation.xyz);

    //  #if defined(_NORMALMAP)
   // inputData.tangentToWorld = tangentToWorld;
    // #endif
    lightingData.normalWS = TransformTangentToWorld(surfaceData.normalTS, tangentToWorld);
    lightingData.normalWS = NormalizeNormalPerPixel(lightingData.normalWS);
    #endif

    
    // 材料的默认值不是脸
    lightingData.isFaceArea = 0;

    //切换"_ISFACE "将影响脸部照明和阴影。
    // 如果是脸部：在顶点着色器中通过用户定义的脸部前进方向和脸部区域遮罩来覆盖法线。
    // 如果不是脸部：不要编辑法线，直接使用网格的法线。
    /*#if _ISFACE
    //在顶点着色器中已经完成了normalWS面部区域的编辑。
    // 所以在这里我们不需要编辑normalWS，把isFaceArea传给ToonLightingData结构就足够了。
        lightingData.isFaceArea = 1;
    #endif*/

    // 拨动"_IsSkin "将影响皮肤的照明颜色
    //lightingData.isSkinArea = _IsSkin;
    lightingData.isSkinArea = step(0.85f, toonMap._MaskMap_Var.a) * step(toonMap._MaskMap_Var.a, 1.01f);
    // reuse sampler_BaseMap to save sampler count              

    // input.positionCS(: SV_POSITION)在片段着色器中是在窗口空间中。
    // x = [0, RT width]
    // y = [0, RT高度]
    lightingData.SV_POSITIONxy = input.positionCS.xy;

    // [为什么不直接从顶点着色器中传递abs(positionVS.z)呢，似乎它也是LinearEyeDepth]
    // 因为我们希望lightingData.selfLinearEyeDepth与Convert_SV_PositionZ_ToLinearViewSpaceDepth(tex2D(_CameraDepthTexture))具有相同的格式。
    //从positionCS.z重新计算selfLinearEyeDepth，在进行深度纹理阴影深度比较逻辑时，提供了更好的精度。
    // 三星A70的移动精度测试@ 2021-3-23证实了使用这一行而不是顶点着色器的abs(positionVS.z)，深度纹理阴影的精度提高了很多。  
    lightingData.selfLinearEyeDepth = Convert_SV_PositionZ_ToLinearViewSpaceDepth(input.positionCS.z);

    lightingData.ZOffsetFinalSum = input.positionWS_ZOffsetFinalSum.w;

    lightingData.SH = input.SH_fogFactor.xyz;

    lightingData.averageShadowAttenuation = input.normalWS_averageShadowAttenuation.w;

    // 如果没有人使用lightingData.normalVS，unity的着色器将删除这一行的计算。
    // 与URP的VertexPositionInputs和VertexNormalInputs结构相同。
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
    toonmap._BaseMap_Var = SAMPLE_TEXTURE2D(_BaseMap, sampler_linear_clamp, input.uv);
    toonmap._MaskMap_Var = SAMPLE_TEXTURE2D(_MaskMap, sampler_linear_clamp, input.uv);
    // toonmap._MaskMap_Var = SAMPLE_TEXTURE2D_LOD(_MaskMap, sampler_linear_clamp, input.uv,0);
   // toonmap._MaskMap_Var.b = toonmap._MaskMap_Var.b;
    toonmap._DecalMap_Var = 0;
    #if _DECAL
    toonmap._DecalMap_Var = SAMPLE_TEXTURE2D(_DecalMap, sampler_DecalMap, input.uv2) * _DecalColor;
    #endif

    return toonmap;
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//片段共享函数（Step3：覆盖surfaceData.albedo）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ApplyDynamicEye(inout ToonSurfaceData surfaceData, Varyings varyings, ToonLightingData lightingData)
{
}

void ApplyPerCharacterBaseMapOverride(inout half3 originalSurfaceColor, Varyings varyings,
                                      ToonLightingData lightingData)
{
    #if _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
    half4 texValue = SAMPLE_TEXTURE2D(_PerCharacterBaseMapOverrideMap, sampler_BaseMap, varyings.uv *_PerCharacterBaseMapOverrideTiling);
    texValue.rgb *= _PerCharacterBaseMapOverrideTintColor;
    originalSurfaceColor = lerp(originalSurfaceColor,texValue.rgb, _PerCharacterBaseMapOverrideAmount * texValue.a);
    #endif
}



// [Mat Cap(alpha blend)]
void ApplyMatCapAlphaBlend(inout ToonSurfaceData surfaceData, Varyings varyings, ToonLightingData lightingData)
{
}

// [Mat Cap(additive)]
void ApplyMatCapAdditive(inout ToonSurfaceData surfaceData, Varyings varyings, ToonLightingData lightingData,
                         ToonMap toonmap)
{
    //#if !_ISFACE /*_MATCAP_ADD*/
    // 读取matcap纹理
    float2 matCapAddUV = lightingData.normalVS.xy * 0.5 + 0.5;
    half4 matCapTextureReadRGBA = tex2D(_MatCapAdditiveMap, matCapAddUV);

    half4 matCapAdditiveRGBA = _MatCapAdditiveColor; // contains alpha also

    // intensity slider
    matCapAdditiveRGBA.rgb *= _MatCapAdditiveIntensity; // only affect rgb

    //让用户选择是否与基色混合
    matCapAdditiveRGBA.rgb *= lerp(1, surfaceData.albedo, _MatCapAdditiveMixWithBaseMapColor);

    //通过一个可选的遮罩纹理进行遮罩
    // （决定在这个遮罩部分不使用shader_feature，否则会使用太多的shader_feature）。
    half matCapAdditiveMask = step(0.95f, toonmap._MaskMap_Var.r); // reuse sampler_BaseMap to save sampler count
    matCapAdditiveMask = toonmap._MaskMap_Var.r;
    // matCapAdditiveMask = invLerpClamp(_MatCapAdditiveMaskMapRemapStart, _MatCapAdditiveMaskMapRemapEnd, matCapAdditiveMask);
    matCapAdditiveRGBA.rgb *= matCapAdditiveMask; // only affect rgb

    //将RGBA与matcap纹理混合
    half4 result = matCapTextureReadRGBA * matCapAdditiveRGBA;

    surfaceData.albedo += result.rgb;
    // #endif
}

void ApplyMatCapAdditive(inout half3 color, Varyings varyings, ToonLightingData lightingData,
                         ToonMap toonmap, inout ToonSurfaceData surfaceData)
{
    //#if !_ISFACE /*_MATCAP_ADD*/
    // 读取matcap纹理
    float2 matCapAddUV = lightingData.normalVS.xy * 0.5 + 0.5;

    float mip = surfaceData.colormask1 * _Roughness1 + surfaceData.colormask2 * _Roughness2 + surfaceData.colormask3 *
        _Roughness3 + surfaceData.colormask4 * _Roughness4 + surfaceData.colormask5 * _Roughness5;
    mip = lerp(mip, _MetallicRoughness, surfaceData.metallicMask);
    half4 matCapTextureReadRGBA = tex2Dlod(_MatCapAdditiveMap, float4(matCapAddUV, 1, mip)).r;
   // matCapTextureReadRGBA = min(matCapTextureReadRGBA,toonmap._MaskMap_Var.g);
    
   // _MatCapAdditiveColor.rgb *= lerp(1,surfaceData.albedo,_MatCapAdditiveMixWithBaseMapColor);

    
    half4 matCapAdditiveRGBA = lerp(1, _MatCapAdditiveColor, surfaceData.metallicMask); // contains alpha also

    float _MatCapAdditiveIntensity2 = surfaceData.colormask1 * _MatCapIntensity1 + surfaceData.colormask2 *
        _MatCapIntensity2 + surfaceData.colormask3 * _MatCapIntensity3 + surfaceData.colormask4 * _MatCapIntensity4 +
        surfaceData.colormask5 * _MatCapIntensity5;
    _MatCapAdditiveIntensity = lerp(_MatCapAdditiveIntensity2, _MatCapAdditiveIntensity, surfaceData.metallicMask);

    // intensity slider
    matCapAdditiveRGBA.rgb *= _MatCapAdditiveIntensity; // only affect rgb
    //让用户选择是否与基色混合
    matCapAdditiveRGBA.rgb *=  surfaceData.albedo;
                                  

    //将RGBA与matcap纹理混合
    half4 result = matCapTextureReadRGBA * matCapAdditiveRGBA * toonmap._MaskMap_Var.g * toonmap._MaskMap_Var.r;
   // color = result;
   // color *=  clamp(0.5,1,1- _MatCapAdditiveIntensity);
    color += result.rgb;
 //color=  1- _MatCapAdditiveIntensity;
    #if _NILOTOON_DEBUG_SHADING
    surfaceData.MatCapColor = result;  // 金属高光颜色
    surfaceData.MatCapAddColor = color;   // 金属高光混合光照
    #endif


    //#endif
}


// [back face color tint]
void ApplyBackFaceColorTint(inout half3 originalSurfaceColor, Varyings varyings, ToonLightingData lightingData,
                            half facing)
{
    originalSurfaceColor *= facing < 0 ? _BackFaceTintColor : 1;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//片段共享函数（第四步：计算照明和最终颜色）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// 类似于URP的Lighting.hlsl -> UniversalFragmentPBR()。
// 这个函数不包含任何光照逻辑，它只是传递光照结果数据。
// 在这个函数中完成的工作是 "做阴影贴图深度测试定位WS偏移"
half3 ShadeAllLights(inout ToonSurfaceData surfaceData, ToonLightingData lightingData, Varyings input, ToonMap toonmap)
{
    ///////////////////////////////////////////////////////////////////
    // Indirect light
    ///////////////////////////////////////////////////////////////////
    half3 indirectResult = 0;


    // 主光是最亮的方向性光线。
    // 它在量灯光循环之外进行阴影处理，它有一套特定的变和阴影路径
    // 所以我们可以在只有一个方向性光线的情况下尽可能快地完成。
    // 你可以选择性地传递一个shadowCoord。如果是这样的话，将计算出shadowAttenuation。
    float3 center = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w + 0.5, unity_ObjectToWorld[2].w);
    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS_ZOffsetFinalSum.xyz);
    Light mainLight = GetMainLight(shadowCoord);

    // distanceAttenuation对于定向光来说是无用的。
    // 但乘以它可以使光的Cull Mask发挥作用。
    // 这意味着乘以unity_LightData.z; // unity_LightData.z在没有被Cull Mask remove的时候是1，否则是0。
    // *见URP的Lighting.hlsl的GetMainLight()。

    //TODO  bug 修复  Min(2,light.Color) 使用distanceAttenuation 会有闪烁 ,是否是精度问题 夏勇固定值
    // mainLight.color *= /*0.3f;//*/ mainLight.distanceAttenuation;

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.light_distanceAttenua = mainLight.distanceAttenuation; //debug
    #endif


    // 在编辑前复制
    lightingData.rawURPShadowAttenuation = mainLight.shadowAttenuation;


    half3 mainLightResult = ShadeMainLight(surfaceData, lightingData, mainLight, toonmap);

    // additional light
    half3 additionalLightResult = 0;

    // Emission
    half3 emissionResult = 0;
    #if _EMISSION
    emissionResult = ShadeEmission(surfaceData, lightingData);
    #endif

    // 综合所有的照明结果
    return CompositeAllLightResults(indirectResult, mainLightResult, additionalLightResult, emissionResult, surfaceData,
                                    lightingData);
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//片段共享函数（步骤5：轮廓颜色编辑）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ApplyOutlineColorOverrideByTexture(inout half3 originalSurfaceColor, ToonLightingData lightingData)
{
}

void ApplySurfaceToScreenSpaceOutlineColorEditIfSurfacePass(inout half3 originalSurfaceColor,
                                                            ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    // 这个函数只影响ForwardLit通道的表面像素
    // 而Outline pass只会使用ApplySurfaceToOutlineColorEditIfOutlinePass(...)，而不是这个函数。
    #if NiloToonForwardLitPass && _SCREENSPACE_OUTLINE && _NILOTOON_GLOBAL_ENABLE_SCREENSPACE_OUTLINE && !SHADER_API_VULKAN

    // 如果_ZWrite是关闭的(=0)，屏幕空间的轮廓被禁用。
    // 因为如果_ZWrite是关闭的（=0），这个材料应该是一个透明的材料。
    // 而透明材质不会写入_CameraDepthTexture。
    // 这使得屏幕空间的轮廓不正确（屏幕空间的轮廓依赖于_CameraDepthTexture和_CameraNormalsTexture）。
    // 因此，如果_Zwrite是关闭的，就自动禁用屏幕空间轮廓。
    if(_ZWrite)
    {
        // outline width
        float finalOutlineWidth = lerp(_ScreenSpaceOutlineWidth, _ScreenSpaceOutlineWidthIfFace, lightingData.isFaceArea) * ;

        // depth sensitivity
        float finalDepthSensitivity = max(0,lerp(_ScreenSpaceOutlineDepthSensitivity, _ScreenSpaceOutlineDepthSensitivityIfFace, lightingData.isFaceArea) + );
        // depth sensitivity texture apply
        half4 depthSensitivityTexValue = tex2D(_ScreenSpaceOutlineDepthSensitivityTex, lightingData.uv);
        half depthSensitivityMultiplierByTex = dot(depthSensitivityTexValue, _ScreenSpaceOutlineDepthSensitivityTexChannelMask);
        depthSensitivityMultiplierByTex = invLerpClamp(_ScreenSpaceOutlineDepthSensitivityTexRemapStart, _ScreenSpaceOutlineDepthSensitivityTexRemapEnd, depthSensitivityMultiplierByTex); // should remap first,
        finalDepthSensitivity *= depthSensitivityMultiplierByTex;

        // normals sensitivity
        float finalNormalsSensitivity = max(0,lerp(_ScreenSpaceOutlineNormalsSensitivity, _ScreenSpaceOutlineNormalsSensitivityIfFace, lightingData.isFaceArea) );
        // normals sensitivity texture apply
        half4 normalsSensitivityTexValue = tex2D(_ScreenSpaceOutlineNormalsSensitivityTex, lightingData.uv);
        half normalsSensitivityMultiplierByTex = dot(normalsSensitivityTexValue, _ScreenSpaceOutlineNormalsSensitivityTexChannelMask);
        normalsSensitivityMultiplierByTex = invLerpClamp(_ScreenSpaceOutlineNormalsSensitivityTexRemapStart, _ScreenSpaceOutlineNormalsSensitivityTexRemapEnd, normalsSensitivityMultiplierByTex); // should remap first,
        finalNormalsSensitivity *= normalsSensitivityMultiplierByTex;
        //----------------------------------------------------------------------------------------------------------------------------------------
        float isScreenSpaceOutlineArea = IsScreenSpaceOutline(lightingData.SV_POSITIONxy, 
            finalOutlineWidth, 
            finalDepthSensitivity, 
            finalNormalsSensitivity, 
            lightingData.selfLinearEyeDepth, 
            _GlobalScreenSpaceOutlineDepthSensitivityDistanceFadeoutStrengthForChar,
            _CurrentCameraFOV);
        
       // isScreenSpaceOutlineArea *= _GlobalScreenSpaceOutlineIntensityForChar; // intensity control
       // originalSurfaceColor = isScreenSpaceOutlineArea;
       // return isScreenSpaceOutlineArea;
        originalSurfaceColor *= lerp(1,_ScreenSpaceOutlineTintColor ,isScreenSpaceOutlineArea);
        originalSurfaceColor *= lerp(_ScreenSpaceOutlineOcclusionAreaTintColor, 1, surfaceData.occlusion);

        originalSurfaceColor = lerp(originalSurfaceColor, _ScreenSpaceOutlineReplaceColor, _ScreenSpaceOutlineUseReplaceColor * isScreenSpaceOutlineArea);
        
        // override by texture
        half3 originalSurfaceColorBeforeOverrideByTexture = originalSurfaceColor;
        ApplyOutlineColorOverrideByTexture(originalSurfaceColor, lightingData);
        originalSurfaceColor = lerp(originalSurfaceColorBeforeOverrideByTexture, originalSurfaceColor, isScreenSpaceOutlineArea);       
    }
    #endif
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//片段共享函数（步骤6：每卷和每个字符的颜色编辑）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// [Volume]
void ApplyVolumeEffectColorEdit(inout half3 color)
{
    //////////////////////////////////////////////////////////////////////////////////
    // （不是照明，通常是为了电影的需要，如剪接场景）。
    //////////////////////////////////////////////////////////////////////////////////
  //  color *= _GlobalVolumeMulColor;
   // color = lerp(color, _GlobalVolumeLerpColor.rgb, _GlobalVolumeLerpColor.a);
}

// [Per Character script]
void ApplyPerCharacterEffectColorEditForwardPass(inout half3 color, ToonLightingData lightingData)
{
    //////////////////////////////////////////////////////////////////////////////////
    // （不是照明，通常是为了游戏的需要，如选择角色）。
    //////////////////////////////////////////////////////////////////////////////////
    // mul add
    color.rgb = color.rgb * _PerCharEffectTintColor + _PerCharEffectAddColor;

    // desaturate
    color = lerp(color, Luminance(color), _PerCharEffectDesaturatePercentage);

    // lerp
    color.rgb = lerp(color.rgb, _PerCharEffectLerpColor.rgb, _PerCharEffectLerpColor.a);

    // rim light (outline pass don't need this)
    // 使用PolygonNdotV而不是NdotV。
    // 因为ToonLightingData的NdotV是应用了法线图并进行了标准化处理的，我们不希望法线图在这里影响到边缘光。
    half NdotV = lightingData.PolygonNdotV;
    half rim = 1 - NdotV;

    rim = pow(rim, _PerCharEffectRimSharpnessPower);

    // apply(additive)
    color.rgb += (rim * (1 - lightingData.isFaceArea)) * _PerCharEffectRimColor; // 边缘光不能显示在脸上，因为它看起来很难看
}

void ApplyPerCharacterEffectColorEditForwardPass(inout half3 color)
{
    //////////////////////////////////////////////////////////////////////////////////
    // （不是照明，通常是为了游戏的需要，如选择角色）。
    //////////////////////////////////////////////////////////////////////////////////
    // mul add
    color.rgb = color.rgb * _PerCharEffectTintColor + _PerCharEffectAddColor;

    // desaturate
    color = lerp(color, Luminance(color), _PerCharEffectDesaturatePercentage);

    // lerp
    color.rgb = lerp(color.rgb, _PerCharEffectLerpColor.rgb, _PerCharEffectLerpColor.a);

    // rim light (outline pass don't need this)
    // 使用PolygonNdotV而不是NdotV。
    // 因为ToonLightingData的NdotV是应用了法线图并进行了标准化处理的，我们不希望法线图在这里影响到边缘光。
    
   

 

    // apply(additive)
    // 边缘光不能显示在脸上，因为它看起来很难看
}


void ApplyPerCharacterEffectColorEdit(inout half3 color, ToonLightingData lightingData)
{
    //////////////////////////////////////////////////////////////////////////////////
    // （不是照明，通常是为了游戏的需要，如选择角色）。
    //////////////////////////////////////////////////////////////////////////////////
    // mul add
    color.rgb = color.rgb * _PerCharEffectTintColor + _PerCharEffectAddColor;

    // desaturate
    color = lerp(color, Luminance(color), _PerCharEffectDesaturatePercentage);

    // lerp
    color.rgb = lerp(color.rgb, _PerCharEffectLerpColor.rgb, _PerCharEffectLerpColor.a);
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 碎片化的共享函数（步骤7：溶解）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ApplyDissolve(inout half3 color, Varyings input)
{
    #if _NILOTOON_DISSOLVE
    // read black and white map
    half dissolveMapThresholdMapValue = tex2D(_DissolveThresholdMap, input.uv).g;
  // dissolveMapThresholdMapValue =  _DissolveSource == 0?dissolveMapThresholdMapValue:input.color.z;
    half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;

    // clip threshold
    clip(dissolve);
    float dissolveBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveBorderRange, _DissolveAmount, dissolveMapThresholdMapValue);
    dissolveBorderTintColorMask = lerp(0,dissolveBorderTintColorMask,_DissolveBorderTintColor.a);

    float dissolveTopBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveTopBorderRange, _DissolveAmount, dissolveMapThresholdMapValue);
    dissolveBorderTintColorMask = lerp(0,dissolveBorderTintColorMask,_DissolveTopBorderTintColor.a);
    //对 "近阈值区域 "进行HDR色彩调剂 
    color += dissolveBorderTintColorMask * _DissolveBorderTintColor.rgb;
    color += dissolveTopBorderTintColorMask * _DissolveTopBorderTintColor.rgb ;
    #endif
}

void ApplyDissolve_th( Varyings input)
{
    #if _NILOTOON_DISSOLVE
    // read black and white map
    half dissolveMapThresholdMapValue = tex2D(_DissolveThresholdMap, input.uv).g;
    //dissolveMapThresholdMapValue =  _DissolveSource == 0?dissolveMapThresholdMapValue:input.color.z;
    half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;

    // clip threshold
    clip(dissolve);
   
    #endif
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 碎片化的共享函数（步骤9：覆盖最终输出的alpha）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ApplyOverrideOutputAlpha(inout half outputAlpha)
{
    //这里不值得用if()，所以用a?b:c
    outputAlpha = outputAlpha;
    // _EditFinalOutputAlphaEnable ? (_ForceFinalOutputAlphaEqualsOne ? 1.0 : outputAlpha) : outputAlpha;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 片段共享函数 
// （只用于ShadowCaster通道，DepthOnly和NiloToonSelfShadowCaster通道只使用）。
// (不用于其他通道)
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void BaseColorAlphaClipTest(Varyings input)
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if _NILOTOON_DISSOLVE
    // read black and white map
    half dissolveMapThresholdMapValue = tex2D(_DissolveThresholdMap, input.uv).g;
   // dissolveMapThresholdMapValue =  _DissolveSource == 0?dissolveMapThresholdMapValue:input.color.z;
    half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;

    // clip threshold
    clip(dissolve);
    #endif

    // 如果_AlphaClip是关闭的，就没有理由调用GetFinalBaseColor()。
    // 所以这里我们使用#if _ALPHATEST_ON来避免调用GetFinalBaseColor()作为优化。
    #if _ALPHATEST_ON
    DoClipTestToTargetAlphaValue(GetFinalBaseColor(input).a);
    #endif

    // 衰减应该影响URP的阴影图和深度纹理/深度法线纹理。
    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif

    // 溶解应该影响URP的阴影图和深度纹理/深度法线纹理。
    half3 dummy = half3(0, 0, 0);
    ApplyDissolve(dummy, input);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 片段共享函数 
// （仅用于DepthNormal通道，仅用于）。
// (不用于其他通道)
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
float4 BaseColorAlphaClipTest_AndDepthNormalColorOutput(Varyings input) : SV_TARGET
{
    BaseColorAlphaClipTest(input);

    return float4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS_averageShadowAttenuation.xyz, true)),
                  0.0, 0.0);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 片段共享函数 
//（只在NiloToonPrepassBuffer传递中使用）。
// (不用于其他传递)
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
float4 BaseColorAlphaClipTest_AndNiloToonPrepassBufferColorOutput(Varyings input) : SV_TARGET
{
    BaseColorAlphaClipTest(input);

    //对深度纹理进行采样，如果被阻断，则进行剪辑（）。
    float sceneLinearDepth = Convert_SV_PositionZ_ToLinearViewSpaceDepth(LoadSceneDepth(input.positionCS.xy));
    float selfLinearDepth = Convert_SV_PositionZ_ToLinearViewSpaceDepth(input.positionCS.z);
    if (sceneLinearDepth < selfLinearDepth - 0.001)
        clip(-1);

    // 如果像素仍然可见，则绘制到_NiloToonPrepassBufferTex中。
    //return float4(0, _AllowNiloToonBloomCharacterAreaOverride * _AllowedNiloToonBloomOverrideStrength, 0, 1);
    return float4(0, 1, 0, 1);
}
