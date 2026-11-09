#pragma once

// ---------------------------------------------------------------------------
//  头部 SDF 面部阴影
//
//  这段逻辑是从老项目搬过来的：
//      H:\UNITY\unity-2022\Shader\MainRoleRenderForwardLit.hlsl
//      - 第 112 行  SigmoidSharp()
//      - 第 174-194 行  SDF 面部阴影计算
//
//  改动原因：原来的实现是
//      smoothstep(threshold - softness, threshold + softness, sdf)
//  而 _FaceShadowGradientResultSoftness 默认只有 0.005，等于硬边，
//  只要动 _ShadowOffset，阈值一变阴影边缘就整块跳变。
//  SigmoidSharp 是连续可导的软阈值，sharp 控制软硬，参数连续变化时过渡是平滑的。
// ---------------------------------------------------------------------------

// 老项目原式：底数 100000 + (-3 * sharp * (x - center))，配合 _FaceShadowSmoothNdotL * 5 使用
float SigmoidSharp(float x, float center, float sharp)
{
    return 1.0 / (1.0 + pow(100000.0, (-3.0 * sharp * (x - center))));
}

half3 ShadeMainLight(inout ToonSurfaceData surfaceData, ToonLightingData lightingData, Light light, ToonMap toon_map)
{
    // 主光方向水平化。light.direction 来自 URP 的 GetMainLight()，自动跟随场景灯光，不需要外部传。
    half3 L = normalize(float3(light.direction.x, 0, light.direction.z));

    // _FaceForwardDirection / _FaceUpDirection 存的是【模型本地空间】方向（默认 +Z / +Y），
    // 转到世界空间后角色旋转会自动跟随。
    half3 faceForwardWS = normalize(TransformObjectToWorldDir(_FaceForwardDirection));
    half3 faceUpWS = normalize(TransformObjectToWorldDir(_FaceUpDirection));
    half3 faceRightWS = normalize(-cross(faceUpWS, faceForwardWS));

    // 光照方向分别投影到脸的右向 / 前向
    float2 faceLightDot;
    faceLightDot.x = dot(L.xz, faceRightWS.xz);
    faceLightDot.y = saturate(dot(-L.xz, faceForwardWS.xz) * 0.5 + _ShadowOffset);

    // 手动模式：不跟随灯光，直接用材质的两个滑块
    if (_FaceShadowNoFollowThrLight == 1)
    {
        faceLightDot.y = saturate(0.5 - _FaceShadowResult * 0.5);
        faceLightDot.x = _FaceShadowRool - 0.5;
    }

    // SDF 采样：光从另一侧来时镜像 UV
    float2 faceUV = lightingData.uv * _FaceShadowGradientMapUVScaleOffset.xy + _FaceShadowGradientMapUVScaleOffset.zw;
    float2 faceSDFUV = float2(faceLightDot.x < 0 ? faceUV.x : 1 - faceUV.x, faceUV.y);
    float faceSDF = tex2D(_FaceShadowGradientMap, faceSDFUV).r;

    // 应用区域复用 _MaskMap 的 a 通道，不需要单独一张阴影遮罩贴图。
    // _MaskMap 用 sampler_BaseMap 采样，和 GetTexture_Face 里保持一致。
    float faceShadowArea = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, lightingData.uv).a * lightingData.isFaceArea;

    if (_DebugFaceShadowGradientMap && faceShadowArea > 0)
        return faceSDF;

    // 软阈值：sharp 越大边缘越硬。下限 0.01 防止 sharp = 0 时 pow 参数退化。
    float sharpness = max(_FaceShadowSmoothNdotL * 5.0, 0.01);
    float faceMapShadow = SigmoidSharp(faceSDF, faceLightDot.y, sharpness) * faceShadowArea;

    // ========== ramp 采样 ==========
    float rampLightingTexSamplingUvY = 0.95 - _LightMode;
    float rampUvX = saturate(faceMapShadow);

    // 需要用钳形采样器，否则 uv.x == 0 时可能采样到 uv.x == 1 的位置
    half3 RampCol = SAMPLE_TEXTURE2D(_RampLightingTex, sampler_Point_clamp,
                                     half2(rampUvX, rampLightingTexSamplingUvY)).xyz;

    return surfaceData.albedo.rgb * RampCol;
}


half3 CompositeAllLightResults(half3 indirectResult, half3 mainLightResult, half3 additionalLightSumResult,
                               ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    half3 directLightResult = mainLightResult;

    half3 finalLightResult = max(indirectResult, directLightResult);

    return finalLightResult;
}
