// 俯视角空气透视雾。全屏 Pass，读深度重建世界坐标，沿视线解析积分指数高度场。
// 数学推导见 AerialFog-design.md 第 7.1 / 7.2 节。
Shader "Piu/AerialFog"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "AerialFog"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _FogStart;
            float _FogEnd;
            float _FogCurve;
            float _Density;
            float _HeightFalloff;
            float _BaseHeight;
            float _MaxOpacity;
            float4 _ColorNear;
            float4 _ColorFar;
            float _SkyFogAmount;
            float _DebugMode;
            float4 _SunColor;
            float _SunInfluence;

            // 指数高度雾沿视线的光深闭式解。
            // density(h) = density * exp(-falloff * (h - baseH))
            // tau = ∫0^L density(h(s)) ds，h(s) 沿视线线性变化。
            float HeightFogOpticalDepth(float3 p0, float3 p1, float density, float falloff, float baseH)
            {
                float L = distance(p0, p1);
                if (L < 1e-4)
                    return 0.0;

                float dh = p1.y - p0.y;
                float k = falloff * dh;

                // 相机所在高度的密度。指数截断防止相机跑到基准高度以下时数值爆掉。
                float d0 = density * exp(min(-falloff * (p0.y - baseH), 10.0));

                // (1 - exp(-k)) / k，k 趋近 0 时极限为 1。
                float integral = abs(k) < 1e-4 ? 1.0 : (1.0 - exp(-k)) / k;

                return d0 * L * integral;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Blit.hlsl 的 Vert 已经把 _BlitScaleBias 乘进 texcoord，不要再乘一次。
                float2 uv = input.texcoord;

                float rawDepth = SampleSceneDepth(uv);
                float3 posWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                float3 camPos = _WorldSpaceCameraPos;

                float dist = distance(posWS, camPos);

                // 天空没有深度，Linear01Depth 判据平台无关；reversed-Z 下写死 0 判断会翻车。
                float linear01 = Linear01Depth(rawDepth, _ZBufferParams);
                bool isSky = linear01 >= 1.0 - 1e-4;
                if (isSky)
                    dist = _FogEnd;

                float u = saturate((dist - _FogStart) / max(_FogEnd - _FogStart, 1e-4));

                // pow(0, x) 在部分平台给 NaN 或 1，FogStart 内侧正好是 0，抬一手。
                float mask = pow(max(u, 1e-5), _FogCurve);
                if (isSky)
                    mask *= _SkyFogAmount;

                float tau = HeightFogOpticalDepth(camPos, posWS, _Density, _HeightFalloff, _BaseHeight);
                float t = (1.0 - exp(-tau * mask)) * _MaxOpacity;

                // 调试预览，_DebugMode = 0 是正常出图。
                if (_DebugMode > 0.5)
                {
                    float v;
                    if (_DebugMode < 1.5)      v = rawDepth;              // 原始深度
                    else if (_DebugMode < 2.5) v = linear01;              // 线性 0..1 深度
                    else if (_DebugMode < 3.5) v = saturate(dist / 50.0); // 视距，50 米封顶
                    else if (_DebugMode < 4.5) v = mask;                  // 距离裁切
                    else if (_DebugMode < 5.5) v = saturate(tau);         // 高度雾光深
                    else                       v = isSky ? 1.0 : 0.0;     // 天空遮罩
                    return half4(v, v, v, 1.0);
                }

                half4 src = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                half3 fogCol = lerp(_ColorNear.rgb, _ColorFar.rgb, u);

                // 让雾色吃一点主光颜色：influence = 0 时乘 1（不变），= 1 时雾色完全乘上光照色。
                // _SunColor 由 Material.SetColor 传入，线性色彩空间下 Unity 已自动转成线性值。
                fogCol *= lerp(float3(1.0, 1.0, 1.0), _SunColor.rgb, saturate(_SunInfluence));

                src.rgb = lerp(src.rgb, fogCol, t);
                return src;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
