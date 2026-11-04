#pragma once

half3 ShadeMainLight(inout ToonSurfaceData surfaceData, ToonLightingData lightingData, Light light, ToonMap toon_map)
{
    // 1、 Common data
    // 未使用的结果将被视为死代码并被编译器删除，如果你不使用它，不用担心性能。
    half3 L = normalize(float3(light.direction.x,0, light.direction.z));


   


    // L = L = _LightDirFromCameraDir? normalize(lightingData.viewDirectionWS +  _LightDirFromCameraDirOffSet) :L; 
    // ========== 2.5、 处理Face_Shadow_Map的阴影 ========== //
    // TODO：将整个部分移到C#中进行优化，当它被证明是稳定和没有错误的时候。
    half3 faceRightDirWS = -cross(_FaceUpDirection, _FaceForwardDirection);
    half3x3 worldSpaceToHeadSpaceMatrix = half3x3(faceRightDirWS, _FaceUpDirection, _FaceForwardDirection);

    // [将所有方向转换为头部空间]
    // 在 "仅旋转 "矩阵mul之后，单位向量仍然是单位向量，不需要归一化（）。
    half3 faceForwardDirectionHeadSpace = mul(worldSpaceToHeadSpaceMatrix, _FaceForwardDirection);
    half3 faceRightDirectionHeadSpace = mul(worldSpaceToHeadSpaceMatrix, faceRightDirWS);
    half3 lightDirectionHeadSpace = mul(worldSpaceToHeadSpaceMatrix, L);

    lightDirectionHeadSpace.y = 0;
    float2 lightDirectionHeadSpaceXZ = SafeNormalize(lightDirectionHeadSpace).xz;

    float2 faceForwardDirectionHeadSpaceXZ = normalize(faceForwardDirectionHeadSpace.xz);
    float2 faceRightDirectionHeadSpaceXZ = normalize(faceRightDirectionHeadSpace.xz);

    float FaceForwardDotL = dot(faceForwardDirectionHeadSpaceXZ, lightDirectionHeadSpaceXZ);
    float FaceRightDotL = dot(faceRightDirectionHeadSpaceXZ, lightDirectionHeadSpaceXZ);

    if (_FaceShadowNoFollowThrLight == 1)
    {
        FaceForwardDotL = _FaceShadowResult;
        FaceRightDotL = _FaceShadowRool - 0.5;
    }

    FaceForwardDotL = max(-0.99 , FaceForwardDotL);
    float2 faceUV = lightingData.uv;
    faceUV = faceUV * _FaceShadowGradientMapUVScaleOffset.xy + _FaceShadowGradientMapUVScaleOffset.zw;
    float2 faceShadowGraientMapValueUV = faceUV * float2(-sign(FaceRightDotL), 1);
    //----------------------------------------------------------------------------------------
    half4 faceShadowGradientMapSampledValue = tex2D(_FaceShadowGradientMap, faceShadowGraientMapValueUV).r;
   // return faceShadowGradientMapSampledValue.r;
    half faceShadowGradientMapValue = faceShadowGradientMapSampledValue.x;
    //找到应用区域
    half faceShadowGradientMask = tex2D(_FaceShadowGradientMaskMap, lightingData.uv).r;
    half faceShadowGradientApplyArea = faceShadowGradientMask * lightingData.isFaceArea;
    // 用户调试
    if (_DebugFaceShadowGradientMap && faceShadowGradientApplyArea > 0)
    {
        return faceShadowGradientMapValue;
    }
    float resultThresholdValue = 1 - (FaceForwardDotL * 0.5 + 0.5);
    half faceShadowResult = smoothstep(resultThresholdValue - _FaceShadowGradientResultSoftness,
                                       resultThresholdValue + _FaceShadowGradientResultSoftness,
                                       faceShadowGradientMapValue); //TODO: provide smoothstep option?
//return faceShadowResult;
    half faceShadowResult2 = faceShadowResult;
    // ========== 处理Face_Shadow_Map的阴影 ENd ========== //

    // [ramp照明覆盖]
    float shadow = 1;

    float rampLightingTexSamplingUvY = 0.95  - _LightMode;
    float rampUvX = faceShadowResult2;

   // rampUvX *= shadow;

    //我们需要使用钳形采样器对斜面纹理进行采样，否则uv.x == 0可能还会对uv.x == 1的位置进行采样。
    half3 _RampLightingTex_Day = SAMPLE_TEXTURE2D(_RampLightingTex, sampler_Point_clamp,
                                                  half2(rampUvX, rampLightingTexSamplingUvY)).xyz;
    //_RampLightingTex_Day = lerp(_RampLightingTex_Day * _ShadowColor,_RampLightingTex_Day,rampUvX);
   // _RampLightingTex_Day = lerp(1, _RampLightingTex_Day, lightingData.vertexColor.b);
    half3 RampCol = _RampLightingTex_Day;
    half3 lightColorIndependentLitColor = surfaceData.albedo.rgb * RampCol;

    return lightColorIndependentLitColor;
}


half3 CompositeAllLightResults(half3 indirectResult, half3 mainLightResult, half3 additionalLightSumResult,
                               ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    half3 directLightResult = mainLightResult;

    half3 finalLightResult = max(indirectResult, directLightResult);

    return finalLightResult;
}
