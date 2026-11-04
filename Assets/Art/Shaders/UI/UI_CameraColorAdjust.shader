Shader "UI/CameraColorAdjust"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Brightness  ("Brightness",  Range(-1, 1)) = 0
        _Contrast    ("Contrast",    Range(0, 3))  = 1
        _Saturation  ("Saturation",  Range(0, 3))  = 1
        _HueShift    ("Hue Shift",   Range(0, 1))  = 0

        // UGUI Mask / Stencil 支持（必须保留）
        _StencilComp     ("Stencil Comparison", Float) = 8
        _Stencil         ("Stencil ID",         Float) = 0
        _StencilOp       ("Stencil Operation",  Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask",  Float) = 255
        _ColorMask       ("Color Mask",         Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline"    = "UniversalPipeline"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]
        Blend    SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_CameraColorAdjust"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half   _Brightness;
                half   _Contrast;
                half   _Saturation;
                half   _HueShift;
                float4 _ClipRect;
                float  _UIMaskSoftnessX;
                float  _UIMaskSoftnessY;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4  color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half4  color       : COLOR;
                float2 uv          : TEXCOORD0;
                float4 worldPos    : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // RGB → HSV
            half3 RGBtoHSV(half3 c)
            {
                half4 K = half4(0.0h, -1.0h/3.0h, 2.0h/3.0h, -1.0h);
                half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
                half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));
                half  d = q.x - min(q.w, q.y);
                half  e = 1.0e-10h;
                return half3(abs(q.z + (q.w - q.y) / (6.0h * d + e)), d / (q.x + e), q.x);
            }

            // HSV → RGB
            half3 HSVtoRGB(half3 c)
            {
                half4 K = half4(1.0h, 2.0h/3.0h, 1.0h/3.0h, 3.0h);
                half3 p = abs(frac(c.xxx + K.xyz) * 6.0h - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos    = IN.positionOS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color       = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                // 1. 亮度偏移
                col.rgb += _Brightness;

                // 2. 对比度（以 0.5 为中心缩放）
                col.rgb = (col.rgb - 0.5h) * _Contrast + 0.5h;

                // 3. 饱和度（向灰度插值）
                half lum = dot(col.rgb, half3(0.299h, 0.587h, 0.114h));
                col.rgb  = lerp(half3(lum, lum, lum), col.rgb, _Saturation);

                // 4. 色相偏移（仅在非零时计算）
                [branch]
                if (_HueShift > 0.001h)
                {
                    half3 hsv = RGBtoHSV(saturate(col.rgb));
                    hsv.x     = frac(hsv.x + _HueShift);
                    col.rgb   = HSVtoRGB(hsv);
                }

                col.rgb = saturate(col.rgb);

                // UGUI RectMask2D 软裁剪
                #ifdef UNITY_UI_CLIP_RECT
                    float2 m = saturate(
                        (_ClipRect.zw - _ClipRect.xy
                         - abs(IN.worldPos.xy * 2.0 - _ClipRect.zw - _ClipRect.xy))
                        / (float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * 2.0 + 1e-4)
                    );
                    col.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "UI/Default"
}
