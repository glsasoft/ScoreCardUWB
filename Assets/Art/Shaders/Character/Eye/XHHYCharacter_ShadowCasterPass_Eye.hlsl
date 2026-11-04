#pragma once
#include "XHHYCharacter_LightUtil_Eye.hlsl" //光照相关


// 如果 "NiloToonIsAnyOutlinePass "被定义为 = 做常规的MVP变换 + 轮廓挤出顶点 + 所有与轮廓有关的任务
Varyings VertexShaderShadowCaster(Attributes input)
{
    Varyings output = (Varyings)0;

    /*bool shouldDisableRendering = !_EnableRendering;
    // 不渲染，不会污染URP的shadowmap和depth texture/depth normal texture
    shouldDisableRendering = shouldDisableRendering || (_DitherFadeoutAmount == 1) || (_DissolveAmount == 1);
    //shouldDisableRendering = shouldDisableRendering || !_AllowRenderURPShadowCasterPass;
    
    //执行 "禁用渲染"。
    if (shouldDisableRendering)
    {
        output.positionCS.w = 0;
        return output;
    }*/

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    
#if _NILOTOON_FORCE_MINIMUM_SHADER //调试用 ,shader编译流程会自动剔除
    output.positionCS = TransformObjectToHClip(input.positionOS);
    //返回最小的可变结构数据，只渲染_BaseMap（与Unlit相同）.
    output.uv = input.uv;
    return output;    
#endif

    //ApplyExternalAssetSupportLogicToVertexAttributeAtVertexShaderStart(input);
    
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    
    // TRANSFORM_TEX与旧版着色器库相同。
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

    #if _NILOTOON_DEBUG_SHADING
    output.uv8 = input.uv8; // 在片段着色器中显示切线空间平滑的法线作为调试颜色。
    #endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS * _GlobalToonShaderNormalBiasMultiplier, _LightDirection));
    #if UNITY_REVERSED_Z
        positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif
    
    output.positionCS = positionCS;
    return output;
}




