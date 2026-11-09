#ifndef AERIAL_FOG_COMMON_INCLUDED
#define AERIAL_FOG_COMMON_INCLUDED

// ---------------------------------------------------------------------------------------------
// 给「需要自己吃雾」的物体 shader 用，主要针对 Transparent 队列。
//
// 后处理的 AerialFog 挂在 BeforeRenderingTransparents (450)，透明物在它之后才画，
// 所以透明物本来整片免疫。想让某个透明物也跟远景一起变淡，在它的片元里调
// AerialFogApply(color, positionWS) 就行，参数和那层后处理雾是同一份。
//
// 调用约定：
//   1. 先 include URP 的 Core.hlsl（需要 half3 / _WorldSpaceCameraPos）
//   2. 再 include 本文件
//   3. 片元里用物体自己的世界坐标，不要用屏幕 UV 去重建，透明物本来就没有深度
//
// 参数由 AerialFogRenderFeature 每帧 Shader.SetGlobalXxx 写入；Feature 没开时这些值是 0，
// 遮罩算出来 t = 0，等于不加雾，不会把画面弄黑。
// ---------------------------------------------------------------------------------------------

float _AerialFogStart;
float _AerialFogEnd;
float _AerialFogCurve;
float _AerialFogDensity;
float _AerialFogHeightFalloff;
float _AerialFogBaseHeight;
float _AerialFogMaxOpacity;
float4 _AerialFogColorNear;
float4 _AerialFogColorFar;

// 把颜色按视距和高度雾压向雾色。positionWS 传物体自己的世界坐标。
half3 AerialFogApply(half3 color, float3 positionWS)
{
    float3 camPos = _WorldSpaceCameraPos;
    float dist = distance(positionWS, camPos);

    // 距离裁切：和 AerialFog.shader 第 7.1 节完全一致
    float u = saturate((dist - _AerialFogStart) / max(_AerialFogEnd - _AerialFogStart, 1e-4));
    float mask = pow(max(u, 1e-5), _AerialFogCurve);

    // 指数高度雾沿视线的光深闭式解，和 AerialFog.shader 第 7.2 节同一条公式
    float dh = positionWS.y - camPos.y;
    float k = _AerialFogHeightFalloff * dh;
    float d0 = _AerialFogDensity * exp(min(-_AerialFogHeightFalloff * (camPos.y - _AerialFogBaseHeight), 10.0));
    float integral = abs(k) < 1e-4 ? 1.0 : (1.0 - exp(-k)) / k;
    float tau = d0 * dist * integral;

    float t = (1.0 - exp(-tau * mask)) * _AerialFogMaxOpacity;

    half3 fogCol = lerp(_AerialFogColorNear.rgb, _AerialFogColorFar.rgb, u);
    return lerp(color, fogCol, t);
}

#endif // AERIAL_FOG_COMMON_INCLUDED
