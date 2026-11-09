// =============================================================================================================================
// XHHY 角色渲染 —— DepthOnly / DepthNormals 通道顶点着色器（Character / Face / Eye 三套共用）
//
// 合并自：
//   XHHYCharacter\XHHYCharacter_DepthOnlyOrDepthNormalPass.hlsl
//   Face\XHHYCharacter_DepthOnlyOrDepthNormalPass_Face.hlsl
//   Eye\XHHYCharacter_DepthOnlyOrDepthNormalPass_Eye.hlsl
//
// 三份原本只差两处：
//   1. Character 那份没有调用 NiloDoPerspectiveRemoval（旧版漏了）
//   2. Character 那份多一句 output.color = input.color（本通道只写深度/法线，用不到）
// 这里统一以 Face / Eye 版为准。
//
// 使用约定：本文件只放函数体，不 include 任何东西。调用方（各组的薄壳 .hlsl）负责先 include
// 自己那一组的 XHHYCharacter_LightUtil[_Face|_Eye].hlsl 和 XHHYUtilityHLSL/Common.hlsl。
// =============================================================================================================================
#pragma once

Varyings VertexShaderDepthOnlyOrDepthNormal(Attributes input)
{
    Varyings output = (Varyings)0;

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
    output.dissolveHeight = GetDissolveHeightOS(positionWS.y - _DissolveRootY);
    output.positionCS = TransformWorldToHClip(positionWS);

    half4 lightingNormalWS_faceArea = GetLightingNormalWS_FaceArea(vertexNormalInput.normalWS, positionWS, output.uv);
    output.normalWS_averageShadowAttenuation.xyz = lightingNormalWS_faceArea.xyz;
    //已经被GetLightingNormalWS_FaceArea(...)规范化。

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 消除透视相机的失真
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    output.positionCS = NiloDoPerspectiveRemoval(output.positionCS, positionWS, _HeadBonePositionWS,
                                                 _PerspectiveRemovalRadius, _PerspectiveRemovalAmount,
                                                 _PerspectiveRemovalStartHeight, _PerspectiveRemovalEndHeight);
    #if _PERSPECTIVEREMOVAL
    output.positionCS = ApplyPerspectiveRemoval(positionWS,_CenterPointPosition);
    #endif
    return output;
}
