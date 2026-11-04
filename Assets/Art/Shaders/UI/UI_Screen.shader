Shader "UI/Screen"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // UGUI Mask/Stencil 支持（必须保留，否则 Mask 组件失效）
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // UGUI 裁剪支持
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
        ColorMask [_ColorMask]

        // ─────────────────────────────────────────
        // 滤色混合方程：result = src + dst - src*dst
        // 拆解为 GPU Blend：
        //   (1 - dst) * src  +  1 * dst
        //   = src - src*dst + dst
        //   = src + dst - src*dst  ✓
        // ─────────────────────────────────────────
        Blend OneMinusDstColor One

        Pass
        {
            Name "UI_Screen"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // UGUI RectMask2D 软裁剪支持
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            // Alpha Clip 支持
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                float4 _ClipRect;           // RectMask2D 裁剪区域
                float  _UIMaskSoftnessX;    // RectMask2D 软边缘 X
                float  _UIMaskSoftnessY;    // RectMask2D 软边缘 Y
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
                float4 worldPos    : TEXCOORD1;   // RectMask2D 需要世界坐标
                UNITY_VERTEX_OUTPUT_STEREO
            };

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
                // 采样纹理
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                // RectMask2D 软裁剪
                #ifdef UNITY_UI_CLIP_RECT
                    float2 m = saturate(
                        (_ClipRect.zw - _ClipRect.xy - abs(IN.worldPos.xy * 2.0
                        - _ClipRect.zw - _ClipRect.xy))
                        / (float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * 2.0 + 1e-4)
                    );
                    col.a *= m.x * m.y;
                #endif

                // Alpha Clip
                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif

                // 预乘 Alpha：让半透明区域按比例参与滤色，透明区域不影响底层
                col.rgb *= col.a;

                return col;
            }
            ENDHLSL
        }
    }

    // 不支持 URP 时 Fallback 到内置管线 UI/Default
    FallBack "UI/Default"
}
