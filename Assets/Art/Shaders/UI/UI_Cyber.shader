Shader "UI/CyberpunkGlitchDissolve"
{
    Properties
    {
        // ===================== UI 标准属性 =====================
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // ===================== Stencil =====================
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // ===================== UI 裁剪 =====================
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)

        // ===================== 混合模式 =====================
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10

        // ===================== 总进度 =====================
        _Progress ("Progress (0=Dissolve, 1=Appear)", Range(0,1)) = 1

        // ===================== 溶解 =====================
        [KeywordEnum(Linear, Radial, Noise)] _DissolveMode ("Dissolve Mode", Float) = 0
        _DissolveSoftness ("Edge Softness", Range(0.001, 0.5)) = 0.1
        _DissolveLineWidth ("Edge Line Width", Range(0, 0.3)) = 0.05
        _DissolveNoiseScale ("Noise Scale", Range(0.1, 20)) = 5
        _DissolveNoiseStrength ("Noise Distortion", Range(0, 1)) = 0.3

        // ===================== 边缘发光 =====================
        [HDR] _EdgeColor ("Edge Color", Color) = (0, 2, 3, 1)
        _EdgeIntensity ("Edge Intensity", Range(0, 10)) = 2

        // ===================== 故障 =====================
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _GlitchSpeed ("Glitch Speed", Float) = 5
        _GlitchFrequency ("Glitch Frequency", Float) = 10
        _GlitchBlockSize ("Glitch Block Size", Range(1, 100)) = 20
        _ScanlineDensity ("Scanline Density", Range(1, 500)) = 100
        _ScanlineSpeed ("Scanline Speed", Float) = 1
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3

        _RandomSeed ("Random Seed", Float) = 0

        [Toggle(_BIND_GLITCH_TO_PROGRESS)] _BindGlitchToProgress ("Bind Glitch to Progress", Float) = 1

        // ===================== 色散 =====================
        [Toggle(_CHROMATIC_ABERRATION)] _EnableChromaticAberration ("Enable Chromatic Aberration", Float) = 1
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.05)) = 0.005
        _ChromaticDirection ("Chromatic Direction (Deg)", Range(0, 360)) = 0

        // ===================== Sprite Rect（C# 传入，Sprite Atlas 安全） =====================
        _SpriteRect ("Sprite Rect (UV)", Vector) = (0, 0, 1, 1)
        _UseSpriteRect ("Use Sprite Rect", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DstBlend]
        ColorMask [_ColorMask]

        Pass
        {
            Name "CyberpunkGlitchDissolve"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma shader_feature_local _DISSOLVEMODE_LINEAR _DISSOLVEMODE_RADIAL _DISSOLVEMODE_NOISE
            #pragma shader_feature_local _BIND_GLITCH_TO_PROGRESS
            #pragma shader_feature_local _CHROMATIC_ABERRATION
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float4 _ClipRect;
                float _Progress;
                float _DissolveSoftness;
                float _DissolveLineWidth;
                float _DissolveNoiseScale;
                float _DissolveNoiseStrength;
                half4 _EdgeColor;
                float _EdgeIntensity;
                float _GlitchIntensity;
                float _GlitchSpeed;
                float _GlitchFrequency;
                float _GlitchBlockSize;
                float _ScanlineDensity;
                float _ScanlineSpeed;
                float _ScanlineIntensity;
                float _RandomSeed;
                float _ChromaticAberration;
                float _ChromaticDirection;
                float4 _SpriteRect;
                float _UseSpriteRect;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float UnityGet2DClipping(float2 position, float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position) * step(position, clipRect.zw);
                return inside.x * inside.y;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.worldPosition = input.positionOS;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // ===================== 1. 故障参数提前算（色散需要） =====================
                float glitchAmount = _GlitchIntensity;
                #ifdef _BIND_GLITCH_TO_PROGRESS
                    glitchAmount *= saturate(1.0 - _Progress);
                #endif

                float time = _Time.y * _GlitchSpeed;

                // ===================== 2. 采样主纹理（含色散） =====================
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                #ifdef _CHROMATIC_ABERRATION
                    float caRad = _ChromaticDirection * 0.0174533; // PI / 180
                    float2 caDir = float2(cos(caRad), sin(caRad));
                    float2 caOffset = caDir * _ChromaticAberration * glitchAmount;

                    float2 uvR = input.uv + caOffset;
                    float2 uvB = input.uv - caOffset;

                    // Sprite Atlas 安全：clamp 到 Sprite 在 Atlas 中的 UV 范围
                    if (_UseSpriteRect > 0.5)
                    {
                        uvR = clamp(uvR, _SpriteRect.xy, _SpriteRect.zw);
                        uvB = clamp(uvB, _SpriteRect.xy, _SpriteRect.zw);
                    }

                    texColor.r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvR).r;
                    texColor.b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvB).b;
                #endif

                half4 color = texColor * input.color;

                // ===================== 3. UI 裁剪 =====================
                #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif

                // ===================== 4. 溶解值 =====================
                float dissolveValue = 0.0;

                #ifdef _DISSOLVEMODE_LINEAR
                    dissolveValue = 1.0 - input.uv.y;
                #elif defined(_DISSOLVEMODE_RADIAL)
                    float2 center = input.uv - 0.5;
                    dissolveValue = saturate(length(center) * 2.0);
                #else
                    dissolveValue = noise2D(input.uv * _DissolveNoiseScale + _RandomSeed);
                #endif

                #ifndef _DISSOLVEMODE_NOISE
                    float noiseVal = noise2D(input.uv * _DissolveNoiseScale * 2.0);
                    dissolveValue = saturate(dissolveValue + (noiseVal - 0.5) * _DissolveNoiseStrength);
                #endif

                // ===================== 5. 溶解遮罩 =====================
                float threshold = lerp(1.0 + _DissolveSoftness, -_DissolveSoftness, _Progress);
                float mask = smoothstep(
                    threshold - _DissolveSoftness,
                    threshold + _DissolveSoftness,
                    dissolveValue
                );

                // ===================== 6. 边缘发光 =====================
                float edgeDist = abs(dissolveValue - threshold);
                float edge = 1.0 - smoothstep(0.0, _DissolveLineWidth, edgeDist);
                edge *= step(0.001, _Progress) * step(_Progress, 0.999);
                edge *= step(0.001, _DissolveLineWidth);

                // ===================== 7. 其余故障 =====================
                float scanline = sin(input.uv.y * _ScanlineDensity + _Time.y * _ScanlineSpeed) * 0.5 + 0.5;
                float scanlineFactor = lerp(1.0, scanline, _ScanlineIntensity * glitchAmount);

                float2 blockUV = floor(input.uv * _GlitchBlockSize);
                float blockRand = hash21(blockUV + floor(time * _GlitchFrequency * 0.1) * 0.01);
                float blockThreshold = 1.0 - glitchAmount * 0.5;
                float blockGlitch = step(blockThreshold, blockRand);

                float flickerRand = hash21(floor(time * _GlitchFrequency) + _RandomSeed);
                float flicker = 1.0 + (flickerRand * 2.0 - 1.0) * glitchAmount * 0.3;

                // ===================== 8. 组合 =====================
                half3 finalColor = color.rgb;
                finalColor *= scanlineFactor;
                finalColor *= flicker;
                finalColor += _EdgeColor.rgb * _EdgeIntensity * edge * color.a;

                color.a *= mask;
                color.a *= 1.0 - blockGlitch * glitchAmount;
                color.rgb = finalColor;

                return color;
            }
            ENDHLSL
        }
    }

    Fallback "UI/Default"
}