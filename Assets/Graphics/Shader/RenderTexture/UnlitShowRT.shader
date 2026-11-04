Shader "URP/UnlitShowRT"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        [Toggle] _FlipX ("Flip UV X", Float) = 0
        _Brightness ("Brightness", Range(-1, 1)) = 0
        _Contrast ("Contrast", Range(0, 2)) = 1
        _Saturation ("Saturation", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  "Queue"="Geometry"}
        Pass
        {
            Name "UnlitShowRT"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Brightness;
            float _Contrast;
            float _Saturation;
            float _FlipX;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                o.uv.x = lerp(i.uv.x, 1.0 - i.uv.x, step(0.5, _FlipX));
                
                return o;
            }
            
            half3 AdjustColor(half3 color)
            {
                // 基本颜色调整
                color += _Brightness;
                color = (color - 0.5) * _Contrast + 0.5;
                half gray = dot(color, half3(0.299, 0.587, 0.114));
                color = lerp(gray.xxx, color, _Saturation);
                
                return saturate(color);
            }
            
            half4 frag(Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                col.rgb = AdjustColor(col.rgb);
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags {
                "LightMode" = "DepthOnly"
            }
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            ENDHLSL
        }
    }
}



