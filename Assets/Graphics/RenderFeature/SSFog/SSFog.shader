Shader "PostProcessingTemplate/test"
{
    Properties {
        _NoiseTex1("NoiseTex1", 2D) = "white"{}
        _NoiseTex2("NoiseTex2", 2D) = "white"{}
        
        _FogColor1("Fog Color1", Color) = (1,1,1,1)
        _FogColor2("Fog Color2", Color) = (1,1,1,1)
        
        _FogMinHeight("Fog Min Height", Float) = 0.0
        _FogMaxHeight("Fog Max Height", Float) = 2
        _FogSpeed("Fog Speed(speed1_xy,speed2_zw)", Vector) = (0,0,0,0)
        
        _FogDensity("Fog Density", Range(0,5)) = 1
        _FogFadeOut("Fog FadeOut", Range(0,5)) = 1
        _FogNear1("Fog Near1", Float) = 5
        _FogNearFadeOut1("Fog NearFadeOut1", Range(0,5)) = 1
        _FogNear2("Fog Near2", Float) = 7
        _FogNearFadeOut2("Fog NearFadeOut2", Range(0,5)) = 1
        
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "VolumetricLight"
            ZTest Always Cull Off ZWrite Off

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
            
            TEXTURE2D(_NoiseTex1);
            SAMPLER(sampler_NoiseTex1);
            TEXTURE2D(_NoiseTex2);
            SAMPLER(sampler_NoiseTex2);


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
            
            float _FogMinHeight;
            float _FogMaxHeight;
            float4 _FogSpeed;
            float _FogDensity;
            float4 _FogColor1;
            float4 _FogColor2;
            float _FogFadeOut;

            float4 _NoiseTex1_ST;
            float4 _NoiseTex2_ST;

            float _FogNear1;
            float _FogNearFadeOut1;
            float _FogNear2;
            float _FogNearFadeOut2;

            float4 frag (v2f i) : SV_Target
            {
                float4 originColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv);
                
                // 获取深度
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
                float linearEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float3 viewPos = ReconstructViewPos(i.uv, linearEyeDepth);
                float3 worldPos = viewPos + GetCameraPositionWS();
                
                // return float4(worldPos,1);

                // 雾高度分布 0~1
                float heightFog = smoothstep(_FogMaxHeight, _FogMinHeight, worldPos.y); // 越低越浓
                heightFog = pow(heightFog, _FogFadeOut);
                
                // 两层雾
                float2 noiseUV = worldPos.xz * 0.1;
                float2 noiseUV1 = TRANSFORM_TEX(noiseUV, _NoiseTex1);
                noiseUV1 = noiseUV1 + (_Time.y * _FogSpeed.xy);
                float2 noiseUV2 = TRANSFORM_TEX(noiseUV, _NoiseTex2);
                noiseUV2 = noiseUV2 + (_Time.y * _FogSpeed.zw);

                float noise1 = SAMPLE_TEXTURE2D(_NoiseTex1, sampler_NoiseTex1, noiseUV1).r;
                float noise2 = SAMPLE_TEXTURE2D(_NoiseTex2, sampler_NoiseTex2, noiseUV2).r;
                
                float camDist = distance(worldPos, GetCameraPositionWS());
                float nearFade1 = saturate((camDist - _FogNear1) / _FogNearFadeOut1);
                float nearFade2 = saturate((camDist - _FogNear2) / _FogNearFadeOut2);

                // 如果距离太远，完全不雾化
                float distFade = saturate((300 - camDist) / 20); 
                
                float fogFactor1 = saturate(noise1 * nearFade1 * heightFog * _FogDensity * distFade);
                float fogFactor2 = saturate(noise2 * nearFade2 * heightFog * _FogDensity * distFade);
                
                float4 final_Fog1 = lerp(originColor, _FogColor1, fogFactor1);
                float4 final_Fog1_Fog2 = lerp(final_Fog1, _FogColor2, fogFactor2);

                return final_Fog1_Fog2;
            }
            
            ENDHLSL
        }
    }
}
