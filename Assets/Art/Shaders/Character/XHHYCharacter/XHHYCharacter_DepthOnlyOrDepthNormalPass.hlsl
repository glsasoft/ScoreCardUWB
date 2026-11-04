#pragma once
#include "XHHYCharacter_LightUtil.hlsl" //光照相关
#include "XHHYUtilityHLSL/Common.hlsl"//通用方法
Varyings VertexShaderDepthOnlyOrDepthNormal(Attributes input)
{
    Varyings output = (Varyings)0;

    /*bool shouldDisableRendering = !_EnableRendering;

    shouldDisableRendering = shouldDisableRendering || (_DitherFadeoutAmount == 1) || (_DissolveAmount == 1);

    #if NiloToonDepthOnlyOrDepthNormalPass
    shouldDisableRendering = shouldDisableRendering || !_AllowRenderDepthOnlyOrDepthNormalsPass;
    #endif

    //执行 "禁用渲染"。
    if (shouldDisableRendering)
    {
        output.positionCS.w = 0;
        return output;
    }*/

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    //调试用 ,shader编译流程会自动剔除
    #if _NILOTOON_FORCE_MINIMUM_SHADER 
    output.positionCS = TransformObjectToHClip(input.positionOS);
    //返回最小的可变结构数据，只渲染_BaseMap（与Unlit相同）.
    output.uv = input.uv;
    return output;    
    #endif

    //ApplyExternalAssetSupportLogicToVertexAttributeAtVertexShaderStart(input);

    VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    // TRANSFORM_TEX与旧版着色器库相同。
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

    #if _NILOTOON_DEBUG_SHADING
    output.uv8 = input.uv8; // 在片段着色器中显示切线空间平滑的法线作为调试颜色。
    #endif

    #if VaryingsHasTangentWS
    half sign = input.tangentOS.w * GetOddNegativeScale();
    output.tangentWS = half4(vertexNormalInput.tangentWS.xyz, sign);
    #endif

    float3 positionWS = vertexPositionInput.positionWS;
    output.positionCS = TransformWorldToHClip(positionWS);
    
    half4 lightingNormalWS_faceArea = GetLightingNormalWS_FaceArea(vertexNormalInput.normalWS, positionWS, output.uv);
    output.normalWS_averageShadowAttenuation.xyz = lightingNormalWS_faceArea.xyz;
    //已经被GetLightingNormalWS_FaceArea(...)规范化。
    

    
    #if _PERSPECTIVEREMOVAL
    output.positionCS = ApplyPerspectiveRemoval(positionWS,_CenterPointPosition);
    #endif
    return output;
}

