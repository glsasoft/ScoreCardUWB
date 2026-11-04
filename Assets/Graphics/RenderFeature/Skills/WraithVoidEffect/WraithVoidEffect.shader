Shader "SkillsEffect/WraithVoidEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Distortion)]
        _Distortion("Distortion", Range(-3,3)) = 0
        _Scale("Scale", Range(0,2)) = 1

        [Header(Color Process)]
        _Brightness("Brightness", Range(0,1)) = 0
        _Saturation("Saturation", Range(0,2)) = 1
        _Contrast("Contrast", Range(0,2)) = 1
        _ColorReverse("Color Reverse", Range(0,1)) = 0
        _Tint("Color", Color) = (1,1,1,1)

        [Header(Edge)]
        _EdgeStart("Edge Start", Range(0,1)) = 0.1
        _EdgeFade("Edge Fade", Range(0,1)) = 0.5
        _EdgeStrength("Edge Strength", Range(0,1)) = 0.5
        _EdgeColor("Edge Color", Color) = (1,1,1,1)

        [Header(Void)]
        _voidRange("Void Range(m)", Range(0, 500)) = 10
        _voidFade("Void Fade", Range(0, 50)) = 0

        [Header(Grid)]
        _gridDensity("Grid Density", Range(0, 1.5)) = 0.5
        _gridLineWidth("Grid Line Width", Range(0, 0.2)) = 0.015
        _gridColor("Grid Color", Color) = (1,1,1,1)
        _gridRange("Grid Range(m)", Range(0, 15)) = 5
        _gridFade("Grid Fade", Range(0, 50)) = 0
        
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        HLSLINCLUDE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Assets/Graphics/Shader/Library/Input.hlsl"

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

        ENDHLSL

        Pass
        {
        	
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;

            half _Distortion;
            half _Scale;

            float _Brightness;
            float _Saturation;
            float _Contrast;
            float _ColorReverse;
            float4 _Tint;

            float _EdgeStart;
            float _EdgeFade;
            float _EdgeStrength;
            float4 _EdgeColor;

            float _voidRange;
            float _voidFade;

            float _gridDensity;
            float _gridLineWidth;
            float4 _gridColor;
            float _gridRange;
            float _gridFade;

            // sampler2D _Noise;


            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);

            float2 GetDistortedUV(float2 uv)
            {
                float time = _Time.y * 1;
                uv.x += sin(uv.y * 30.0 + time * 2.0) * 2;
                uv.y += cos(uv.x * 25.0 + time * 1.5) * 2;
                return uv;
            }

            float Random01(float2 uv)
            {
                float seed = dot(uv, float2(12.9898, 78.233));
                return frac(sin(seed) * 43758.5453);
            }

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

            float4 frag (v2f i) : SV_Target
            {

                float4 originColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv);

                // 镜头扭曲
                float4 distortionColor = 0;
                {
                    float2 center = i.uv - 0.5;
                    float radius2 = pow(center.x, 2) + pow(center.y, 2);
                    float distortion = 1 + sqrt(radius2) * radius2 * _Distortion;
                    float2 uvScreen = center * distortion * _Scale + 0.5;
                    distortionColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uvScreen);
                }

                // 颜色处理
                float4 colorProcess = distortionColor;
                {
                    colorProcess += _Brightness;                                // 亮度
                    float4 luminance = Luminance(colorProcess.rgb).xxxx;        // 饱和度
                    colorProcess = lerp(luminance, colorProcess, _Saturation);
                    float4 gray = float4(0.5, 0.5, 0.5, 1);                     // 对比度
                    colorProcess = lerp(gray, colorProcess, _Contrast);
                    colorProcess = abs(_ColorReverse - colorProcess);           // 颜色反向     
                    colorProcess *= _Tint;  
                }

                // 暗角
                float EdgeCoef = 0; // 要乘
                {
                    float EdgeStart = 0.2;   // 开始距离
                    float EdgeWidth = 0.5;   // 过渡范围

                    float2 uv = i.uv;
                    float2 center = float2(0.5, 0.5);
                    float dist = distance(uv, center);

                    EdgeCoef = saturate((dist - _EdgeStart) / _EdgeFade) * _EdgeStrength;
                }

                // voidRange: 基于深度图
                
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
                float linearEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float3 viewPos = ReconstructViewPos(i.uv, linearEyeDepth);
                float voidRange = 1.0 - smoothstep(_voidRange - _voidFade, _voidRange, length(viewPos));

                // grid
                float gridRange = smoothstep(_gridRange - _gridFade, _gridRange, length(viewPos));
                float3 worldPos = viewPos + GetCameraPositionWS();
                float3 grid = abs(frac(worldPos * _gridDensity) - 0.5);
                float3 gridDist = grid;
                // 线条强度 = 越接近轴线越亮（用 smoothstep 得到线宽范围内的渐变）
                float lineX = smoothstep(0.0, _gridLineWidth, 0.5 - gridDist.x);
                float lineY = smoothstep(0.0, _gridLineWidth, 0.5 - gridDist.y);
                float lineZ = smoothstep(0.0, _gridLineWidth, 0.5 - gridDist.z);
                // 多轴靠近时值会叠加 → 交点更亮
                float gridMask = lineX * lineY * lineZ;
                // 衰减
                float fade = smoothstep(_gridRange, _gridRange + _gridFade, length(viewPos));
                // 最终颜色遮罩
                float finalMask = saturate((1 - gridMask) * fade);

               
                float3 finalColor = colorProcess + EdgeCoef * _EdgeColor.rgb; 
                finalColor = lerp(finalColor, finalColor + finalMask * _gridColor, gridRange);
                finalColor = lerp(originColor, finalColor, voidRange);

                return float4(finalColor,1); 
            }
            ENDHLSL
        }
    
       
    }
}