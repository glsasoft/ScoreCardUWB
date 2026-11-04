Shader "PostProcessingTemplate/BlindFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogStart ("Fog Start", Float) = 0
        _FogEnd ("Fog End", Float) = 1
        _FogDensity ("Fog Density", Range(0, 3)) = 1
        _FogColor ("Fog Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Assets/Graphics/Shader/Library/Input.hlsl"

            struct appdata
            {
                #if SHADER_API_GLES
			        float4 positionOS : POSITION;
			        float2 uv : TEXCOORD0
				#else
				    uint vertexID : SV_VertexID;
				#endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _FogStart;
            float _FogEnd;
            float _FogDensity;
            float4 _FogColor;
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float3 ReconstructViewPos(float2 uv, float linearEyeDepth) {  
                //翻转屏幕
                #ifdef UNITY_UV_STARTS_AT_TOP
                    uv.y = 1 - uv.y;
                #endif

                float zScale = linearEyeDepth / _ProjectionParams.y; // 除以近平面
                float3 viewPos = Piu_CameraViewTopLeftCorner.xyz + Piu_CameraViewXExtent.xyz * uv.x + Piu_CameraViewYExtent.xyz * uv.y;  
                viewPos *= zScale;
                return viewPos;  
            }

            v2f vert(appdata v)
            {
                v2f o;
            	#if SHADER_API_GLES
			        float4 pos = v.positionOS;
			        float2 uv  = v.u
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
                //重建世界坐标
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
                float linearEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float3 viewPos = ReconstructViewPos(i.uv, linearEyeDepth);

                float3 fogDensity = (_FogEnd - length(viewPos)) / (_FogEnd - _FogStart);
                fogDensity = saturate(fogDensity * _FogDensity);
                // float4 finalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 finalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv);
                
                finalColor.rgb = lerp(finalColor, _FogColor.rgb, fogDensity);
                
                return float4(finalColor);
            }
            ENDHLSL
        }
    }
}