Shader "Skybox/Piu_Cubemap"
{
    Properties
    {
        _Cubemap("Cubemap", CUBE) = "" {}
        _Exposure("Exposure", Range(0, 8)) = 1.0
        _HorizonalRotation("Horizonal Rotation", Range(0, 360)) = 0
        _VerticalRotation("Vertical Rotation", Range(0, 360)) = 0

        
        [Header(Color)]
        _Brightness("Brightness", Range(-1, 1)) = 0.0
        _Contrast("Contrast", Range(0,2)) = 1.0
        _Saturation("Saturation", Range(0,1)) = 1.0
        
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }

        Pass
        {
            Name "Piu_Cubemap"
            ZWrite Off
            Cull Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_Cubemap);
            SAMPLER(sampler_Cubemap);

            CBUFFER_START(UnityPerMaterial)
                float _Exposure;
                float _HorizonalRotation;
                float _VerticalRotation;
                float _Brightness;
                float _Contrast;
                float _Saturation;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 texcoord   : TEXCOORD;
            };

            float3 RotateDirection(float3 dir, float yawAngle, float pitchAngle)
            {
                float yawRad = radians(yawAngle);
                float pitchRad = radians(pitchAngle);

                float sinY = sin(yawRad);
                float cosY = cos(yawRad);
                float sinP = sin(pitchRad);
                float cosP = cos(pitchRad);

                // Yaw: rotate around Y axis
                float3x3 yawMatrix = float3x3(
                    cosY, 0, -sinY,
                    0,    1,     0,
                    sinY, 0,  cosY
                );

                // Pitch: rotate around X axis
                float3x3 pitchMatrix = float3x3(
                    1,    0,     0,
                    0, cosP, -sinP,
                    0, sinP,  cosP
                );

                return mul(pitchMatrix, mul(yawMatrix, dir));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 dir = TransformObjectToWorld(IN.positionOS);
                OUT.texcoord = RotateDirection(dir, _HorizonalRotation, _VerticalRotation);
                OUT.positionCS = TransformWorldToHClip(dir * 10000.0); // push far enough
                return OUT;
            }

            float3 AdjustColor(float3 color, float brightness, float contrast, float saturation)
            {
                color += brightness;
                color = (color - 0.5f) * contrast + 0.5f;

                // 转灰度再混合
                float luminance = dot(color, float3(0.2126, 0.7152, 0.0722));
                color = lerp(float3(luminance, luminance, luminance), color, saturation);
                
                return saturate(color);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURECUBE(_Cubemap, sampler_Cubemap, normalize(IN.texcoord));
                col.rgb *= _Exposure;
                col.rgb = AdjustColor(col, _Brightness, _Contrast, _Saturation);

               
                return col;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
