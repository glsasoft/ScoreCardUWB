Shader "Effect/ErrorEffect1"
{
    Properties
    {
        // 基础参数
        [Header(Base)]
        [Space(20)]
        _ErrorTex ("故障贴图", 2D) = "white"{}
        [HDR]_ErrorColor ("颜色", Color) = (1, 1, 1, 1)
        _ErrorColorIntensity ("颜色强度", Float) = 1
        _ErrorTexRedIsAlphaOn ("R是透明通道", Float) = 0
        _ErrorTexRotation ("故障贴图旋转", Range(-360, 360)) = 0
        _ErrorTexFlowAndBevel ("故障贴图 流动 & 斜切", Vector) = (0, 0, 0, 0)
 
        [Header(ErrorEffect)]
        [Space(20)]

        [Header(Core)]
        [Space(10)]
        _Freq ("噪声频率", Range(0,1)) = 0.1
        _Fade ("褪色程度", Float) = 1

        [Header(Offset)]
        [Space(10)]
        [KeywordEnum(Sin, Triangle, Sawtooth)] _ErrorOffsetMode ("偏移类型(sin，三角，锯齿)", Float) = 0
        _ErrorOffsetInterval ("偏移间隔时间", Range(0, 2)) = 0.3
        _ErrorOffsetDuration ("偏移持续时间", Range(0, 2)) = 1
        _ErrorOffsetSpeed("偏移速度", Range(0, 5)) = 1
        [Space(10)]
        _ErrorOffsetMax("偏移最大值", Range(-50,50)) = 10
        _RGBSplit_Indensity ("偏移强度", Range(-2,2)) = 0.2
        

        [Header(ErrorBlock)]
        [Space(10)]
         _BlockLayer ("故障块数量(xy内层，zw表层)", Vector) = (8, 5, 4.9, 9.9)
         _BlockLayer1_Indensity ("内层故障块强度", Range(-1,20)) = 8
         _BlockLayer2_Indensity ("表层故障块强度", Range(-1,20)) = 4
         
         _Offset ("表层故障块贡献", Range(0,5)) = 1

         
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
            #pragma shader_feature _ERROROFFSETMODE_SIN _ERROROFFSETMODE_TRIANGLE _ERROROFFSETMODE_SAWTOOTH
            

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            float4 _ErrorColor;
            float4 _ErrorTex_ST;
            float _ErrorTexRedIsAlphaOn;
            float _ErrorColorIntensity;
            float _ErrorTexRotation;
            float4 _ErrorTexFlowAndBevel;

            float _ErrorOffsetInterval;
            float _ErrorOffsetDuration;
            
            float _ErrorOffsetMax, _ErrorOffsetSpeed;

            float _Freq,_Offset,_Fade;
           
            float4 _BlockLayer;

            float _RGBSplit_Indensity, _BlockLayer1_Indensity, _BlockLayer2_Indensity;
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

            float randomNoise(float2 seed)
            {
                return frac(sin(dot(seed * floor(_Freq * 30.0), float2(127.1, 311.7))) * 43758.5453123);
            }
	
            float randomNoise(float seed)
            {
                return randomNoise(float2(seed, 1.0));
            }


            float4 frag(Varyings input) : SV_Target
            {
                // 【基本功能】
                float2 errorTexUV = input.uv.xy;
                //- 故障贴图 缩放，偏移，斜切
                errorTexUV = errorTexUV * _ErrorTex_ST.xy + _ErrorTex_ST.zw;
                float2 errorTexBevel = _ErrorTexFlowAndBevel.zw;
                errorTexUV = float2(errorTexUV.x + errorTexUV.y * errorTexBevel.x, errorTexUV.y + errorTexUV.x * errorTexBevel.y);
                //- 故障贴图 旋转
                errorTexUV -= float2(0.5f, 0.5f);
                float errorRotateAngle = _ErrorTexRotation * (PI * 2 / 360);
                errorTexUV = float2(errorTexUV.x * cos(errorRotateAngle) - errorTexUV.y * sin(errorRotateAngle),
                                    errorTexUV.x * sin(errorRotateAngle) + errorTexUV.y * cos(errorRotateAngle));
                errorTexUV += float2(0.5f, 0.5f);
                //- 故障贴图 流动
                float2 errorTexFlow = _ErrorTexFlowAndBevel.xy;
                errorTexUV += _Time.y * errorTexFlow;

                // 【故障特效】
                // - 分块
                float2 blockLayer1 = floor(errorTexUV * float2(_BlockLayer.x, _BlockLayer.y));
                float2 blockLayer2 = floor(errorTexUV * float2(_BlockLayer.z, _BlockLayer.w));

                float lineNoise1 = pow(randomNoise(blockLayer1), _BlockLayer1_Indensity);
                float lineNoise2 = pow(randomNoise(blockLayer2), _BlockLayer2_Indensity);
                float RGBSplitNoise = pow(randomNoise(5.1379), 7.1) + _RGBSplit_Indensity;
                float lineNoise = lineNoise1 + lineNoise2 * _Offset - RGBSplitNoise;
                
                // 波形节奏
                float timeCycle = frac(_Time.y * _ErrorOffsetSpeed);

                // 周期控制
                float cycleLength = _ErrorOffsetDuration + _ErrorOffsetInterval;
                float fullTime = fmod(_Time.y, cycleLength);
                float isActive = step(fullTime, _ErrorOffsetDuration); // 1 = 启用，0 = 静止

                // - 偏移类型
                #if defined(_ERROROFFSETMODE_SIN)
                    //sin波
                    float offset = sin(timeCycle * PI) * _ErrorOffsetMax;
                #elif defined(_ERROROFFSETMODE_TRIANGLE)
                    // 三角波
                    float offset = (1.0 - abs(timeCycle * 2.0 - 1.0)) * _ErrorOffsetMax;
                #elif defined(_ERROROFFSETMODE_SAWTOOTH)
                    // 锯齿波
                    float offset = timeCycle * _ErrorOffsetMax;
                #endif

                lineNoise *=  offset * isActive;
                
                float4 colorR = SAMPLE_TEXTURE2D(_ErrorTex, sampler_ErrorTex, errorTexUV);
                float4 colorG = SAMPLE_TEXTURE2D(_ErrorTex, sampler_ErrorTex, errorTexUV + float2(lineNoise * 0.05 * randomNoise(7.0), 0));
                float4 colorB = SAMPLE_TEXTURE2D(_ErrorTex, sampler_ErrorTex, errorTexUV - float2(lineNoise * 0.05 * randomNoise(23.0), 0));

                float4 result = float4(float3(colorR.r, colorG.g, colorB.b), colorR.a + colorG.a + colorB.a);

                result = lerp(colorR, result, _Fade * isActive);

                // 颜色叠加
                result.rgb *= _ErrorColor.rgb * _ErrorColorIntensity;
                result.a *= _ErrorColor.a;

                return result;
            }
            ENDHLSL
        }
    }
CustomEditor "ErrorEffectGUI1"
}