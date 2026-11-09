#pragma once

#include "XHHYCharacter_LightUtil_Eye.hlsl" //光照相关
#include "../XHHYUtilityHLSL/Common.hlsl"//通用方法（原路径相对本目录找不到，改成显式相对路径）

//TEXTURE2D(_NiloToonAverageShadowMapRT);

Varyings VertexShaderForward(Attributes input)
{
    Varyings output = (Varyings)0;

    /*bool shouldDisableRendering = !_EnableRendering;
    shouldDisableRendering = shouldDisableRendering || (_DitherFadeoutAmount == 1) || (_DissolveAmount == 1);

    //执行 "禁用渲染"。
    if (shouldDisableRendering)
    {
        output.positionCS.w = 0;
        return output;
    }*/

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    
 #if _NILOTOON_FORCE_MINIMUM_SHADER //调试用 ,shader编译流程会自动剔除
    output.positionCS = TransformObjectToHClip(input.positionOS);
    //返回最小的可变结构数据，只渲染_BaseMap（与Unlit相同）.
    output.uv = input.uv;
    return output;    
#endif

    //ApplyExternalAssetSupportLogicToVertexAttributeAtVertexShaderStart(input);

    VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    float3 positionWS = vertexPositionInput.positionWS;
    
    // TRANSFORM_TEX与旧版着色器库相同。
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

#if _NILOTOON_DEBUG_SHADING
    output.uv8 = input.uv8; // 在片段着色器中显示切线空间平滑的法线作为调试颜色。
#endif
  
    output.positionWS_ZOffsetFinalSum.xyz = positionWS;
    output.viewDirectionWS = GetWorldSpaceViewDir(positionWS);
    
#if VaryingsHasTangentWS
    half sign = input.tangentOS.w * GetOddNegativeScale();
    output.tangentWS = half4(vertexNormalInput.tangentWS.xyz, sign);
#endif

    output.positionCS =vertexPositionInput.positionCS; // TransformWorldToHClip(positionWS);
  
    //找出面的区域，如果顶点在面的区域内，就编辑normalWS。
    half4 lightingNormalWS_faceArea = GetLightingNormalWS_FaceArea(vertexNormalInput.normalWS, positionWS, output.uv);
    output.normalWS_averageShadowAttenuation.xyz = lightingNormalWS_faceArea.xyz;
    
    output.color = input.color;
    output.dissolveHeight = GetDissolveHeightOS(positionWS.y - _DissolveRootY);
    output.depth = vertexPositionInput.positionVS.z;
    //SH
    half3 normalWSForSH = lightingNormalWS_faceArea.xyz;
    //normalWSForSH *= 1-_IndirectLightFlatten; // make the normal become 0 when _IndirectLightFlatten is 1
    //half3 SH = SampleSH(normalWSForSH) * _GlobalIndirectLightMultiplier;
    //output.SH_fogFactor.rgb = SH;

    // 平均阴影衰减
    //output.normalWS_averageShadowAttenuation.w = 1; // floating point RT, so r channel

    // 消除透视相机的失真
    output.positionCS = NiloDoPerspectiveRemoval(output.positionCS, positionWS, _HeadBonePositionWS,
                                                 _PerspectiveRemovalRadius, _PerspectiveRemovalAmount,
                                                 _PerspectiveRemovalStartHeight, _PerspectiveRemovalEndHeight);
    
    #if _PERSPECTIVEREMOVAL
    output.positionCS = ApplyPerspectiveRemoval(positionWS,_CenterPointPosition);
    #endif
    // output.positionCS = TransformObjectToHClip(input.positionOS);
    return output;
}


half4 FragmentShaderForward(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float3 qwe = _FaceForwardDirection;
    input.normalWS_averageShadowAttenuation.xyz *= facing;
/*#if _NILOTOON_FORCE_MINIMUM_SHADER
    return Get_NILOTOON_FORCE_MINIMUM_FRAGMENT_SHADER_result(input,1);
#endif*/

    ToonMap toonMap = GetTexture(input);
    //为照明功能准备所有的数据结构
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap);
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);
    
    half3 color = toonMap._BaseMap_Var.xyz * _BaseColor.rgb;

    //Debug
    #if _NILOTOON_DEBUG_SHADING
    return Get_NILOTOON_DEBUG_SHADING_result(input, surfaceData, lightingData,1);
    #endif
    ApplyPerCharacterBaseMapOverride(color, input, lightingData);
    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData);
    // 受击闪白（独立一层菲涅尔）
    ApplyHitFlash(color, lightingData);
    //应用溶解
    ApplyDissolve(color, input);
    //应用雾（雾前和雾后有扩展逻辑功能）
   // ApplyCustomUserLogicAfterFog(color, surfaceData, lightingData);
//    color = MyFog(color,lightingData.positionWS);
    // 覆盖输出阿尔法
    ApplyOverrideOutputAlpha(surfaceData.alpha);

    // 方向光染色：把角色整体往主光颜色上拉，和空气透视雾保持同一套光照氛围。
    // 用 keyword 剥离，没开时这里一行指令都不生成。
    // _MainLightColor 是 URP 每帧推的主光色（线性空间、已乘 intensity），不用自己传。
    #if defined(_SUNTINT)
    {
        half3 sunCol = _MainLightColor.rgb;
        // 归一化到最大分量为 1，只保留色相，避免光强调高时把角色打爆。
        sunCol /= max(max(sunCol.r, max(sunCol.g, sunCol.b)), 1e-4);
        color *= lerp(half3(1, 1, 1), sunCol, (half)saturate(_SunTintAmount));
    }
    #endif

    return half4(color,  _Alpha);
}


half4 FragmentShaderForward2(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 qwe = _FaceForwardDirection;
    input.normalWS_averageShadowAttenuation.xyz *= facing;
    /*#if _NILOTOON_FORCE_MINIMUM_SHADER
        return Get_NILOTOON_FORCE_MINIMUM_FRAGMENT_SHADER_result(input,1);
    #endif*/

    ToonMap toonMap = GetTexture(input);
    //为照明功能准备所有的数据结构
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap);
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);
    
    //应用所有的照明计算
    half3 color = surfaceData.albedo;
    
    //Debug
    #if _NILOTOON_DEBUG_SHADING
    return Get_NILOTOON_DEBUG_SHADING_result(input, surfaceData, lightingData,1);
    #endif
    
    //适用于每个体积/人物/材料的颜色编辑.应用顺序=从本地到全球
    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData);
    
    //应用溶解
    ApplyDissolve(color, input);
    //应用雾（雾前和雾后有扩展逻辑功能）
   // color = MyFog(color,lightingData.positionWS);
  
    // 覆盖输出阿尔法
    ApplyOverrideOutputAlpha(surfaceData.alpha);

    float3 e = mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2].xyz);
    float _Asd = saturate(dot(normalize(qwe),normalize(e)));
    _Asd = smoothstep(0.5,1,_Asd);
    float alpha = lerp(1,  _HairAlpha,_Asd);
    
    return half4(color, alpha );
}

half4 FragmentShaderForward3(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

   
    float3 qwe = _FaceForwardDirection;
    input.normalWS_averageShadowAttenuation.xyz *= facing;
    /*#if _NILOTOON_FORCE_MINIMUM_SHADER
        return Get_NILOTOON_FORCE_MINIMUM_FRAGMENT_SHADER_result(input,1);
    #endif*/
    ApplyDissolve_th(input);
    ToonMap toonMap = GetTexture(input);
    //为照明功能准备所有的数据结构
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap);
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);
    return 1;
    half3 color = toonMap._BaseMap_Var.xyz;

    //Debug
    #if _NILOTOON_DEBUG_SHADING
    return Get_NILOTOON_DEBUG_SHADING_result(input, surfaceData, lightingData,1);
    #endif
    
    //应用溶解
    ApplyDissolve(color, input);
    //应用雾（雾前和雾后有扩展逻辑功能）
    // ApplyCustomUserLogicAfterFog(color, surfaceData, lightingData);
  //  color = MyFog(color,lightingData.positionWS);
    // 覆盖输出阿尔法
    ApplyOverrideOutputAlpha(surfaceData.alpha);

    return half4(color, surfaceData.alpha  * _Alpha);
}

