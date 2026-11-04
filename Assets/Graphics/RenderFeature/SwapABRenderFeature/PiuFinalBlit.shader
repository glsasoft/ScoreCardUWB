Shader "PostProcessingTemplate/PiuFinalBlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
        	ZWrite Off ZTest Always Blend Off Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            struct appdata
            {
                #if SHADER_API_GLES
			        float4 positionOS : POSITION;
			        float2 uv : TEXCOORD0;
				#else
				    uint vertexID : SV_VertexID;
				#endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
            	#if SHADER_API_GLES
			        float4 pos = v.positionOS;
			        float2 uv  = v.uv;
				#else
			        float4 pos = GetFullScreenTriangleVertexPosition(v.vertexID);
			        float2 uv = GetFullScreenTriangleTexCoord(v.vertexID);
				#endif
                o.vertex = pos;
				o.uv = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv);

                return col;
            }
            ENDHLSL
        }
    }
}