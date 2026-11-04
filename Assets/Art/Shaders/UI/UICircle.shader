// UICircle.shader
// 将 UGUI Image 裁剪为圆形，支持自定义圆心、半径、边缘羽化，兼容 URP。
// 用法：Material 使用本 Shader，调整 _Center / _Radius / _Feather 即可。
// 注意：圆心与半径均在 UV 空间（0~1）下定义，(0.5,0.5) 为纹理中心。
Shader "UI/UICircle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // —— 圆形裁剪参数 ——
        _Center ("圆心 (UV空间)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("半径 (UV空间)", Float) = 0.5
        _Feather ("边缘羽化宽度", Range(0, 0.5)) = 0.02
        [Toggle(_CORRECT_ASPECT)] _CorrectAspect ("按纹理宽高比修正 (防止椭圆)", Float) = 0

        // —— UGUI Mask / Stencil 支持（必须保留，否则 Mask 组件失效）——
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // —— UGUI 裁剪支持 ——
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

        // 预乘 Alpha 混合（与 Unity2022 UI/Default 一致）
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "UICircle"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0

            // 按纹理宽高比修正开关（防止非正方形纹理产生椭圆）
            #pragma shader_feature_local _CORRECT_ASPECT

            // UGUI RectMask2D 软裁剪支持
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            // Alpha Clip 支持
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                float4 _ClipRect;           // RectMask2D 裁剪区域
                float  _UIMaskSoftnessX;    // RectMask2D 软边缘 X
                float  _UIMaskSoftnessY;    // RectMask2D 软边缘 Y
                // 圆形裁剪参数
                float2 _Center;
                float  _Radius;
                float  _Feather;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            // URP 下 TEXTURE2D 不会自动声明 _MainTex_TexelSize，需手动声明
            // 约定：(1/width, 1/height, width, height)，由引擎自动填充
            float4 _MainTex_TexelSize;

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

                // —— RectMask2D 软裁剪 ——
                #ifdef UNITY_UI_CLIP_RECT
                    float2 m = saturate(
                        (_ClipRect.zw - _ClipRect.xy - abs(IN.worldPos.xy * 2.0
                        - _ClipRect.zw - _ClipRect.xy))
                        / (float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * 2.0 + 1e-4)
                    );
                    col.a *= m.x * m.y;
                #endif

                // —— 圆形遮罩计算 ——
                // 在 UV 空间计算到圆心的距离
                float2 d = IN.uv - _Center;

                // 按纹理宽高比修正，避免非正方形纹理导致圆形被拉成椭圆
                #if _CORRECT_ASPECT
                    // _MainTex_TexelSize.z = 宽, .w = 高
                    float aspect = _MainTex_TexelSize.z / max(_MainTex_TexelSize.w, 1e-5);
                    d.x *= aspect;
                    // 修正后半径也需按同一比例换算，这里保持 _Radius 以高度方向为基准
                #endif

                float dist = length(d);

                // smoothstep(edge0, edge1, x)：以 _Radius 为中心，_Feather 为过渡总宽度
                //   dist <= _Radius - _Feather/2  -> 1（完全显示）
                //   dist >= _Radius + _Feather/2  -> 0（完全裁掉）
                float circleAlpha = smoothstep(_Radius + _Feather * 0.5,
                                               _Radius - _Feather * 0.5,
                                               dist);
                col.a *= circleAlpha;

                // —— Alpha Clip ——
                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif

                // 预乘 Alpha（配合 Blend One OneMinusSrcAlpha）
                col.rgb *= col.a;

                return col;
            }
            ENDHLSL
        }
    }

    // 不支持 URP 时回退到内置 UI/Default
    FallBack "UI/Default"
}
