Shader "Effect/ErrorEffect"
{
    Properties
    {
        _ErrorTex ("故障贴图", 2D) = "white"{}
        [HDR]_ErrorColor ("颜色", Color) = (1, 1, 1, 1)
        _ErrorColorIntensity ("颜色强度", Float) = 1
        _ErrorTexRedIsAlphaOn ("R是透明通道", Float) = 0
        _ErrorTexRotation ("故障贴图旋转", Range(-360, 360)) = 0
        _ErrorTexFlowAndBevel ("故障贴图 流动 & 斜切", Vector) = (0, 0, 0, 0)
        
        _ErrorRange ("偏移范围", Range(0, 3)) = 0
        _ErrorDirectionRed ("红色偏移方向", Range(0, 1)) = 0
        _ErrorDirectionBlue ("蓝色偏移方向", Range(0, 1)) = 0
        _ErrorDistortGrid ("扰动格子", Range(1, 16)) = 0
        _ErrorDistortStrength ("扰动强度", Range(0, 0.1)) = 0
        _ErrorDistortSpeed ("扰动速度", Range(-1, 1)) = 0
        
        _ErrorStrobeInterval ("频闪间隔时间", Range(0, 2)) = 2.0
        _ErrorStrobeDuration ("频闪持续时间", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
                "Queue" = "Transparent"
            }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            float4 _ErrorColor;
            float4 _ErrorTex_ST;
            float _ErrorTexRedIsAlphaOn;
            float _ErrorColorIntensity;
            float _ErrorTexRotation;
            float4 _ErrorTexFlowAndBevel;

            float _ErrorRange;
            float _ErrorDirectionRed;
            float _ErrorDirectionBlue;
            float _ErrorDistortGrid;
            float _ErrorDistortStrength;
            float _ErrorDistortSpeed;

            float _ErrorStrobeInterval;
            float _ErrorStrobeDuration;
            CBUFFER_END
            
            TEXTURE2D(_ErrorTex);
            SAMPLER(sampler_ErrorTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertex_position_inputs.positionCS;
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float cycle = _ErrorStrobeInterval + _ErrorStrobeDuration;
                float timeInCycle = fmod(_Time.y, cycle);
                float strobeActive = step(timeInCycle, _ErrorStrobeDuration);
                
                float2 errorTexUV = input.uv.xy;
                
                //故障贴图 缩放，偏移，斜切
                errorTexUV = errorTexUV * _ErrorTex_ST.xy + _ErrorTex_ST.zw;
                
                float2 errorTexBevel = _ErrorTexFlowAndBevel.zw;
                errorTexUV = float2(errorTexUV.x + errorTexUV.y * errorTexBevel.x, errorTexUV.y + errorTexUV.x * errorTexBevel.y);

                //故障贴图 旋转
                errorTexUV -= float2(0.5f, 0.5f);
                float errorRotateAngle = _ErrorTexRotation * (PI * 2 / 360);
                errorTexUV = float2(errorTexUV.x * cos(errorRotateAngle) - errorTexUV.y * sin(errorRotateAngle),
                                    errorTexUV.x * sin(errorRotateAngle) + errorTexUV.y * cos(errorRotateAngle));
                errorTexUV += float2(0.5f, 0.5f);
                
                //故障贴图 流动
                float2 errorTexFlow = _ErrorTexFlowAndBevel.xy;
                errorTexUV += _Time.y * errorTexFlow;

                //故障贴图 故障效果处理
                float gridSize = (int)_ErrorDistortGrid;
                float2 discreteUV = floor(errorTexUV * gridSize) / gridSize;
                float random = frac(sin(dot(discreteUV, float2(12.9898, 78.233)) + _Time.y * _ErrorDistortSpeed * 0.0001) * 43758.5453);
                float2 offset = float2(random, frac(random * 3.1415)) * _ErrorDistortStrength * strobeActive;
                //return float4(offset, 0, 1);
                _ErrorRange *= strobeActive;
                // 计算红蓝偏移方向
                float angleRed = _ErrorDirectionRed * PI * 2; // 将0-1映射到0-360度
                float2 dirRed = float2(cos(angleRed), sin(angleRed));
                float2 redOffset = dirRed * _ErrorRange;

                float angleBlue = _ErrorDirectionBlue * PI * 2;
                float2 dirBlue = float2(cos(angleBlue), sin(angleBlue));
                float2 blueOffset = dirBlue * _ErrorRange;
                float2 errorTexUV1 = errorTexUV + offset * redOffset;
                float2 errorTexUV2 = errorTexUV - offset * blueOffset;
                //errorTexUV += offset;
                //故障贴图 采样
                float4 errorTexColor = SAMPLE_TEXTURE2D(_ErrorTex, sampler_ErrorTex, errorTexUV);
                float4 errorTexColorRed = SAMPLE_TEXTURE2D(_ErrorTex, sampler_ErrorTex, errorTexUV1);
                float4 errorTexColorGreen = errorTexColor;
                float4 errorTexColorBlue = SAMPLE_TEXTURE2D(_ErrorTex, sampler_ErrorTex, errorTexUV2);

                float grayRed = saturate(length(errorTexColorRed.rgb));
                float grayGreen = saturate(length(errorTexColorGreen.rgb));
                float grayBlue = saturate(length(errorTexColorBlue.rgb));
                float3 errorColor = lerp(float3(grayRed, grayGreen, grayBlue), errorTexColor.rgb, step(strobeActive, 0));
                //主贴图 R是透明通道

                float otherAlpha = lerp(saturate(grayRed + grayGreen + grayBlue), grayGreen, step(strobeActive, 0));
                float rightAlpha = saturate(errorTexColorRed.a + errorTexColorGreen.a + errorTexColorBlue.a);
                float errorAlpha = lerp(otherAlpha, rightAlpha, step(_ErrorTexRedIsAlphaOn, 0));
                float3 finalColor = errorColor * _ErrorColor.rgb * _ErrorColorIntensity;
                float finalAlpha = errorAlpha * _ErrorColor.a;
                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "ErrorEffectGUI"
}