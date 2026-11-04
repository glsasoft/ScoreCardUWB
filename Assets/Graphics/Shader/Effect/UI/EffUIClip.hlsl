#ifndef EffUIClip
#define EffUIClip

#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
// #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

float4 _ClipRect;
float _UIMaskSoftnessX;
float _UIMaskSoftnessY;

float4 GetUIMask(float4 positionCS, float3 positionOS)
{
    float2 pixelSize = positionCS.w;
    pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

    float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
    float2 maskUV = (positionOS.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
    float4 mask = half4(positionOS.xy * 2 - clampedRect.xy - clampedRect.zw,
                        0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

    return mask;
}

void SetUIMask(inout float4 Color, float4 mask)
{
    #ifdef UNITY_UI_CLIP_RECT
    half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(mask.xy)) * mask.zw);
    Color.a *= m.x * m.y;
    #endif
}

#endif
