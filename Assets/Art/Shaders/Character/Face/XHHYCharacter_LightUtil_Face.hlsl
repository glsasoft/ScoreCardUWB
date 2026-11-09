#pragma once

#include "XHHYCharacter_Input_Face.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "..\XHHYUtilityHLSL\XhhyAllUtilIncludes.hlsl" 
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

#include "XHHYCharacter_LightingEquationNew_Face.hlsl" 

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
    float2 stackingLayerUV = (uv + baseMapStackingLayerTexUVScaleOffset.zw + frac(_Time.yy * baseMapStackingLayer1TexUVAnimSpeed)) * baseMapStackingLayerTexUVScaleOffset.xy;
    //共享采样器以避免采样器的最大计数=16的限制
    half4 stackingLayerRGBAResult = SAMPLE_TEXTURE2D(baseMapStackingLayerTex, baseMapStackingLayerTexSamplerState, stackingLayerUV) * baseMapStackingLayerTintColor;

    // 对于遮罩纹理，我们重新使用_BaseMap的采样器以减少采样器的数量，因为在大多数情况下，设置应该是相同的 
    half stackingLayerMask = dot(SAMPLE_TEXTURE2D(baseMapStackingLayerMaskTex, sampler_BaseMap, uv), baseMapStackingLayerMaskTexChannel);
    stackingLayerRGBAResult.a *= stackingLayerMask;

    return stackingLayerRGBAResult;
}

void ApplyStackingLayer(inout half4 colorRGBA, half4 layerColorRGBA)
{
    if(_IsAddColor == 0)
    {
        colorRGBA.rgb = lerp(colorRGBA.rgb, layerColorRGBA.rgb, layerColorRGBA.a);
        colorRGBA.a = max(colorRGBA.a, layerColorRGBA.a);
    }else
    {
        colorRGBA.rgb +=  layerColorRGBA.rgb * layerColorRGBA.a;
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
 //   color.rgb *= _PerCharacterBaseColorTint * _GlobalVolumeBaseColorTintColor;
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
    color.rgb *= _PerCharacterBaseColorTint * _GlobalVolumeBaseColorTintColor;
    // 只编辑RGB，因为它们不是按材料编辑的，按字符/全局编辑alpha应该不是用户故意的。
    #if _DECAL
    //color.rgb = color.rgb*(1- toon_map._DecalMap_Var.a)+ toon_map._DecalMap_Var.a* toon_map._DecalMap_Var.rgb;
    #endif

    return color;
}

half3 GetFinalEmissionColor(Varyings input, half3 baseColor,ToonMap toonmap)
{
    #if _EMISSION
    float2 uv = input.uv * _EmissionMapTilingXyOffsetZw.xy + _EmissionMapTilingXyOffsetZw.zw;

    half4 emissionMapSampleValue = tex2D(_EmissionMap, uv);
    half3 emissionResult =  emissionMapSampleValue.rgb; // 如果使用RGB模式，alpha会被忽略

    emissionResult *= _EmissionColor.rgb * _EmissionIntensity; 
    emissionResult *= lerp(1,baseColor,_MultiplyBaseColorToEmissionColor); //让用户选择性地将基色与发射色混合在一起
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



half GetFinalSmoothness(Varyings input)
{
    half smoothnessResult = 0.5;
    return smoothnessResult;
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

// 类似于URP的LitForwardPass.hlsl -> InitializeStandardLitSurfaceData()
ToonSurfaceData InitializeSurfaceData(Varyings input, ToonMap toonMap)
{
    ToonSurfaceData output = (ToonSurfaceData)0;

    half4 baseColorFinal = GetFinalBaseColor(input, toonMap);
    output.albedo = baseColorFinal.rgb;
    output.alpha = baseColorFinal.a;
    // 阿尔法剪辑和抖动淡出
    DoClipTestToTargetAlphaValue(output.alpha); // 一旦知道阿尔法值，就让clip()提前退出。
    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif

    // normalTS
    output.normalTS = GetFinalNormalTS(input);

    output.metallicMask = 0;//面部光照

    return output;
}

//类似于URP的LitForwardPass.hlsl -> InitializeInputData()
ToonLightingData InitializeLightingData(Varyings input,inout  ToonSurfaceData surfaceData, ToonMap toonMap)
{
    ToonLightingData lightingData;

    lightingData.uv = input.uv;
    lightingData.positionWS = input.positionWS_ZOffsetFinalSum.xyz;

    lightingData.viewDirectionWS = SafeNormalize(input.viewDirectionWS);

    half3 normalWS = input.normalWS_averageShadowAttenuation.xyz;

    normalWS = normalize(normalWS);

    lightingData.normalWS = normalWS;
    lightingData.PerPixelNormalizedPolygonNormalWS = normalWS; // 额外的：将归一化的插值顶点法线保存到lightingData中，不要让法线图影响这个向量。
    lightingData.isFaceArea = 1;

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
    toonmap._BaseMap_Var = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    toonmap._MaskMap_Var = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, input.uv);
    toonmap._DecalMap_Var = 0;
    #if _DECAL
    toonmap._DecalMap_Var = SAMPLE_TEXTURE2D(_DecalMap, sampler_DecalMap, input.uv2) * _DecalColor;
    #endif

    return toonmap;
}

ToonMap GetTexture_Face(Varyings input)
{
    ToonMap toonmap;
    toonmap._BaseMap_Var = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    toonmap._MaskMap_Var = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, input.uv);
    toonmap._DecalMap_Var = 0;
    #if _DECAL
    toonmap._DecalMap_Var = SAMPLE_TEXTURE2D(_DecalMap, sampler_DecalMap, input.uv) * _DecalColor;
     toonmap._DecalMap_Var.a *= _DecalInt;
    #endif

    return toonmap;
}


Mask GetMask(ToonMap toonmap)
{
    Mask mask;
     mask.colormask1  = step(0.0f, toonmap._MaskMap_Var.a) * step( toonmap._MaskMap_Var.a,0.2f);
     mask.colormask2  = step(0.2f, toonmap._MaskMap_Var.a) * step(toonmap._MaskMap_Var.a,0.4f);
     mask.colormask3  = step(0.4f, toonmap._MaskMap_Var.a) * step( toonmap._MaskMap_Var.a,0.6f);
     mask.colormask4  = step(0.6f, toonmap._MaskMap_Var.a) * step( toonmap._MaskMap_Var.a,0.85f);
     mask.colormask5  = step(0.85f, toonmap._MaskMap_Var.a) * step( toonmap._MaskMap_Var.a,1.01f);

    return mask;

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
  // originalSurfaceColor =  texValue.rgb ;
    #endif
}

// [Mat Cap(alpha blend)]
void ApplyMatCapAlphaBlend(inout ToonSurfaceData surfaceData, Varyings varyings, ToonLightingData lightingData)
{
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
    half3 indirectResult = 0;
    float3 center = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w + 0.5, unity_ObjectToWorld[2].w);
    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS_ZOffsetFinalSum.xyz);
    Light mainLight = GetMainLight(shadowCoord);

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.light_distanceAttenua = mainLight.distanceAttenuation; //debug
    #endif
    lightingData.rawURPShadowAttenuation = mainLight.shadowAttenuation;

    half3 mainLightResult = ShadeMainLight(surfaceData, lightingData, mainLight, toonmap);
    //return mainLightResult;

    // additional light
    half3 additionalLightResult = 0;

    // 综合所有的照明结果
    return CompositeAllLightResults(indirectResult, mainLightResult, additionalLightResult, surfaceData,
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
        float finalOutlineWidth = lerp(_ScreenSpaceOutlineWidth, _ScreenSpaceOutlineWidthIfFace, lightingData.isFaceArea) * _GlobalScreenSpaceOutlineWidthMultiplierForChar;

        // depth sensitivity
        float finalDepthSensitivity = max(0,lerp(_ScreenSpaceOutlineDepthSensitivity, _ScreenSpaceOutlineDepthSensitivityIfFace, lightingData.isFaceArea) + _GlobalScreenSpaceOutlineDepthSensitivityOffsetForChar);
        // depth sensitivity texture apply
        half4 depthSensitivityTexValue = tex2D(_ScreenSpaceOutlineDepthSensitivityTex, lightingData.uv);
        half depthSensitivityMultiplierByTex = dot(depthSensitivityTexValue, _ScreenSpaceOutlineDepthSensitivityTexChannelMask);
        depthSensitivityMultiplierByTex = invLerpClamp(_ScreenSpaceOutlineDepthSensitivityTexRemapStart, _ScreenSpaceOutlineDepthSensitivityTexRemapEnd, depthSensitivityMultiplierByTex); // should remap first,
        finalDepthSensitivity *= depthSensitivityMultiplierByTex;

        // normals sensitivity
        float finalNormalsSensitivity = max(0,lerp(_ScreenSpaceOutlineNormalsSensitivity, _ScreenSpaceOutlineNormalsSensitivityIfFace, lightingData.isFaceArea) + _GlobalScreenSpaceOutlineNormalsSensitivityOffsetForChar);
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
        
        isScreenSpaceOutlineArea *= _GlobalScreenSpaceOutlineIntensityForChar; // intensity control
       // originalSurfaceColor = isScreenSpaceOutlineArea;
       // return isScreenSpaceOutlineArea;
        originalSurfaceColor *= lerp(1,_ScreenSpaceOutlineTintColor * _GlobalScreenSpaceOutlineTintColorForChar,isScreenSpaceOutlineArea);
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
    color *= _GlobalVolumeMulColor;
    color = lerp(color, _GlobalVolumeLerpColor.rgb, _GlobalVolumeLerpColor.a);
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
    half rim = 1-NdotV;

    rim = pow(rim,_PerCharEffectRimSharpnessPower);

    // apply(additive)
    color.rgb += (rim * (1-lightingData.isFaceArea)) * _PerCharEffectRimColor; // 边缘光不能显示在脸上，因为它看起来很难看
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


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 碎片化的共享函数（步骤9：覆盖最终输出的alpha）。
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void ApplyOverrideOutputAlpha(inout half outputAlpha)
{
    //这里不值得用if()，所以用a?b:c
    outputAlpha = outputAlpha  ;// _EditFinalOutputAlphaEnable ? (_ForceFinalOutputAlphaEqualsOne ? 1.0 : outputAlpha) : outputAlpha;
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


float4 BaseColorAlphaClipTest_AndDepthNormalColorOutput(Varyings input) : SV_TARGET
{
    BaseColorAlphaClipTest(input);

    return float4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS_averageShadowAttenuation.xyz, true)),
                  0.0, 0.0);
}


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
    return float4(0,1, 0, 1);
}
