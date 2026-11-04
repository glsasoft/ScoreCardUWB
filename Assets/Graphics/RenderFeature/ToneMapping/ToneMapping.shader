Shader "PostProcessingTemplate/GT_ToneMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	[Toggle]_IsTone("Is Tone", Float) = 1
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

			static const float e = 2.71828;

			float W_f(float x,float e0,float e1) {
				if (x <= e0)
					return 0;
				if (x >= e1)
					return 1;
				float a = (x - e0) / (e1 - e0);
				return a * a*(3 - 2 * a);
			}
			float H_f(float x, float e0, float e1) {
				if (x <= e0)
					return 0;
				if (x >= e1)
					return 1;
				return (x - e0) / (e1 - e0);
			}

			float GranTurismoTonemapper(float x) {
				float P = 1;
				float a = 1;
				float m = 0.22;
				float l = 0.4;
				float c = 1.33;
				float b = 0;
				float l0 = (P - m)*l / a;
				float L0 = m - m / a;
				float L1 = m + (1 - m) / a;
				float L_x = m + a * (x - m);
				float T_x = m * pow(x / m, c) + b;
				float S0 = m + l0;
				float S1 = m + a * l0;
				float C2 = a * P / (P - S1);
				float S_x = P - (P - S1)*pow(e,-(C2*(x-S0)/P));
				float w0_x = 1 - W_f(x, 0, m);
				float w2_x = H_f(x, m + l0, m + l0);
				float w1_x = 1 - w0_x - w2_x;
				float f_x = T_x * w0_x + L_x * w1_x + S_x * w2_x;
				return f_x;
			}

            float _IsTone;

            float4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv);
				float r = GranTurismoTonemapper(col.r);
				float g = GranTurismoTonemapper(col.g);
				float b = GranTurismoTonemapper(col.b);
				float4 colTone = float4(r,g,b,col.a);
            	
                return lerp(colTone, col, step(_IsTone, 0));
            }
            ENDHLSL
        }
    }
}