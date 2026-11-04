#pragma once

#include "XHHYCharacter_LightUtil_Face.hlsl" //光照相关
#include "XHHYUtilityHLSL/Common.hlsl"//通用方法

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
    output.depth = vertexPositionInput.positionVS.z;
    // 消除透视相机的失真
    output.positionCS = NiloDoPerspectiveRemoval(output.positionCS, positionWS, _HeadBonePositionWS,
                                                 _PerspectiveRemovalRadius, _PerspectiveRemovalAmount,
                                                 _PerspectiveRemovalStartHeight, _PerspectiveRemovalEndHeight);
    
    #if _PERSPECTIVEREMOVAL
    output.positionCS = ApplyPerspectiveRemoval(positionWS,_CenterPointPosition);
    #endif
    return output;
}
half3 LightingPhysicallyBased_Nilo(float3 diffuse,
    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
    half3 normalWS)
{
    
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
     NdotL = smoothstep(0.4,0.5, NdotL *0.5 +0.5);
     NdotL = 1;
    half3 radiance = lightColor * (lightAttenuation * NdotL) * 0.75;

    half3 brdf = diffuse;
   

    return brdf * radiance;
}

half4 FragmentShaderForward(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    input.normalWS_averageShadowAttenuation.xyz *= facing;
    
    ToonMap toonMap = GetTexture_Face(input);
    //为照明功能准备所有的数据结构
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap);
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);
    
    //应用所有的照明计算
    half3 color = ShadeAllLights(surfaceData, lightingData, input, toonMap);



    


    
    ApplyPerCharacterBaseMapOverride(color, input, lightingData);
    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData);
 //   ApplyVolumeEffectColorEdit(color);

    //额外灯光计算
    float3 addLight = 0;
    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, i.worldPos);
        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
        {
            addLight += light.color * light.distanceAttenuation ;
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, lightingData.positionWS);
    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
    {
        addLight +=  LightingPhysicallyBased_Nilo(toonMap._BaseMap_Var.rgb,light.color,light.direction,light.distanceAttenuation * light.shadowAttenuation,normalize(input.normalWS_averageShadowAttenuation.xyz));
    }
   
    LIGHT_LOOP_END
    #endif
//return float4(addLight,1) ;
    color += addLight ;

    //应用溶解
    ApplyDissolve(color, input);
    //应用雾（雾前和雾后有扩展逻辑功能）
//    color = MyFog(color,lightingData.positionWS);
    // 覆盖输出阿尔法
    ApplyOverrideOutputAlpha(surfaceData.alpha);
//return float4(_PerCharEffectRimColor,1);
  // return _Alpha;
    return half4(color, _Alpha);
}

half4 FragmentShaderForward2(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 qwe = _FaceForwardDirection;
    input.normalWS_averageShadowAttenuation.xyz *= facing;

    ToonMap toonMap = GetTexture(input);
    //为照明功能准备所有的数据结构
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap);
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);
    
    //应用所有的照明计算
    half3 color = ShadeAllLights(surfaceData, lightingData, input, toonMap);

    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData);
    ApplyVolumeEffectColorEdit(color);

   
    
    //应用溶解
    ApplyDissolve(color, input);
    //应用雾（雾前和雾后有扩展逻辑功能）
//    color = MyFog(color,lightingData.positionWS);
    // 覆盖输出阿尔法
    ApplyOverrideOutputAlpha(surfaceData.alpha);

    float3 e = mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2].xyz);
    float _Asd = saturate(dot(normalize(qwe),normalize(e)));
    _Asd = smoothstep(0.5,1,_Asd);
   // return _Asd;
    float alpha = lerp(1,  _HairAlpha,_Asd);
    return half4(color, alpha );
}


half4 FragmentShaderForward3(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

  
    input.normalWS_averageShadowAttenuation.xyz *= facing;
    
    ToonMap toonMap = GetTexture(input);
    //为照明功能准备所有的数据结构
    /*ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap);
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);*/
      ApplyDissolve_th( input);
    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif
    return 1;
    //应用所有的照明计算
    /*half3 color = ShadeAllLights(surfaceData, lightingData, input, toonMap);

    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData);
    ApplyVolumeEffectColorEdit(color);
    //应用溶解
    ApplyDissolve(color, input);
    //应用雾（雾前和雾后有扩展逻辑功能）
    color = MyFog(color,lightingData.positionWS);
    // 覆盖输出阿尔法
    ApplyOverrideOutputAlpha(surfaceData.alpha);

    return half4(color, surfaceData.alpha  * _Alpha);*/
}

