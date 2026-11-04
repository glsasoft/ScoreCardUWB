Shader "Effect/AirDistortion"
{
    Properties
    {
        _ZTest ("深度测试", Float) = 2
        _ReflectTexCustomDataOn ("折射贴图启用自定义数据", Float) = 0
        _ReflectTex ("折射贴图", 2D) = "white"{}
        _NormalScale ("法线缩放", Float) = 1
        _DistortTimeFactor_U ("流速 U", Float) = 0
        _DistortTimeFactor_V ("流速 V", Float) = 0
        _DistortStrength ("扰动强度", Float) = 1
        
        _MaskTexOn_1 ("启用遮罩图 1", Float) = 0
        _MaskTex_1 ("遮罩图 1", 2D) = "white"{}
        _MaskTexRotation_1 ("遮罩图 1 旋转", Range(-360, 360)) = 0
        _MaskTexFlowAndBevel_1 ("遮罩图 1 流动 & 斜切", Vector) = (0, 0, 0, 0)
        _MaskTexRedIsAlphaOn_1 ("R是透明通道", Float) = 0
        _MaskTexCustomDataOn_1 ("遮罩图 1 启用自定义数据", Float) = 0
        
        _MaskTexOn_2 ("启用遮罩图 2", Float) = 0
        _MaskTex_2 ("遮罩图 2", 2D) = "white"{}
        _MaskTexRotation_2 ("遮罩图 2 旋转", Range(-360, 360)) = 0
        _MaskTexFlowAndBevel_2 ("遮罩图 2 流动 & 斜切", Vector) = (0, 0, 0, 0)
        _MaskTexRedIsAlphaOn_2 ("R是透明通道", Float) = 0
        _MaskTexCustomDataOn_2 ("遮罩图 2 启用自定义数据", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        }
        Pass
        {
            Tags
            {
                "LightMode" = "AirDistortion"
            }
            ZWrite Off
            ZTest [_ZTest]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma shader_feature_local _MASK_TEX_ON_1
            #pragma shader_feature_local _MASK_TEX_ON_2
            
            CBUFFER_START(UnityPerMaterial)
            float _ReflectTexCustomDataOn;
            float _NormalScale;
            float4 _ReflectTex_ST;
            float _DistortTimeFactor_U;
            float _DistortTimeFactor_V;
            float _DistortStrength;

            float4 _MaskTex_1_ST;
            float _MaskTexRotation_1;
            float4 _MaskTexFlowAndBevel_1;
            float _MaskTexRedIsAlphaOn_1;
            float _MaskTexCustomDataOn_1;

            float4 _MaskTex_2_ST;
            float _MaskTexRotation_2;
            float4 _MaskTexFlowAndBevel_2;
            float _MaskTexRedIsAlphaOn_2;
            float _MaskTexCustomDataOn_2;
            CBUFFER_END
            
            TEXTURE2D(_CustomCameraColorTexture);
            SAMPLER(sampler_CustomCameraColorTexture);
            TEXTURE2D(_ReflectTex);
            SAMPLER(sampler_ReflectTex);
            TEXTURE2D(_MaskTex_1);
            SAMPLER(sampler_MaskTex_1);
            TEXTURE2D(_MaskTex_2);
            SAMPLER(sampler_MaskTex_2);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;     //CustomData
                float4 uv2 : TEXCOORD2;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;     //CustomData
                float4 uv2 : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertex_position_inputs.positionCS;
                output.uv = input.uv;
                output.uv1 = input.uv1;
                output.uv2 = input.uv2;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = GetNormalizedScreenSpaceUV(input.vertex);
                float2 mainTexUV = input.uv.xy;
                float2 tilling = lerp(input.uv.zw, _ReflectTex_ST.xy, step(_ReflectTexCustomDataOn, 0));
                float strength = lerp(input.uv2.x, _DistortStrength, step(_ReflectTexCustomDataOn, 0));
                
                mainTexUV = mainTexUV * tilling + _ReflectTex_ST.zw;
                
                float3 noise = UnpackNormalScale(SAMPLE_TEXTURE2D(_ReflectTex, sampler_ReflectTex,
                            mainTexUV - _Time.y * float2(_DistortTimeFactor_U, _DistortTimeFactor_V)), _NormalScale);
                
                float2 offset = noise.xy * strength * 0.005;

                //遮罩图1
                float maskFac_1 = 1;
                float maskAlpha_1 = 1;
                #if _MASK_TEX_ON_1
                    float2 maskTexUV_1 = input.uv;

                    float2 maskTexTilling_1 = lerp(input.uv1.xy, _MaskTex_1_ST, step(_MaskTexCustomDataOn_1, 0));
                    //遮罩图1 缩放，偏移，斜切
                    maskTexUV_1 = maskTexUV_1 * maskTexTilling_1 + _MaskTex_1_ST.zw;
                    float2 maskTexBevel_1 = _MaskTexFlowAndBevel_1.zw;
                    maskTexUV_1 = float2(maskTexUV_1.x + maskTexUV_1.y * maskTexBevel_1.x, maskTexUV_1.y + maskTexUV_1.x * maskTexBevel_1.y);

                    //遮罩图1 旋转
                    maskTexUV_1 -= float2(0.5f, 0.5f);
                    float maskTexRotateAngle_1 = _MaskTexRotation_1 * (PI * 2 / 360);
                    maskTexUV_1 = float2(maskTexUV_1.x * cos(maskTexRotateAngle_1) - maskTexUV_1.y * sin(maskTexRotateAngle_1),
                                        maskTexUV_1.x * sin(maskTexRotateAngle_1) + maskTexUV_1.y * cos(maskTexRotateAngle_1));
                    maskTexUV_1 += float2(0.5f, 0.5f);

                    //遮罩图1 流动
                    float2 maskTexFlow_1 = _MaskTexFlowAndBevel_1.xy;
                    maskTexUV_1 += _Time.y * maskTexFlow_1;

                    float4 maskTexColor_1 = SAMPLE_TEXTURE2D(_MaskTex_1, sampler_MaskTex_1, maskTexUV_1);
                    maskFac_1 = saturate(maskTexColor_1.r);
                    maskAlpha_1 = lerp(maskFac_1, saturate(maskTexColor_1.a), step(_MaskTexRedIsAlphaOn_1, 0));
                #endif

                //遮罩图2
                float maskFac_2 = 1;
                float maskAlpha_2 = 1;
                #if _MASK_TEX_ON_2
                    float2 maskTexUV_2 = input.uv;

                    float2 maskTexTilling_2 = lerp(input.uv1.zw, _MaskTex_2_ST.xy, step(_MaskTexCustomDataOn_2, 0));

                    //遮罩图2 缩放，偏移，斜切
                    maskTexUV_2 = maskTexUV_2 * maskTexTilling_2 + _MaskTex_2_ST.zw;
                    float2 maskTexBevel_2 = _MaskTexFlowAndBevel_2.zw;
                    maskTexUV_2 = float2(maskTexUV_2.x + maskTexUV_2.y * maskTexBevel_2.x, maskTexUV_2.y + maskTexUV_2.x * maskTexBevel_2.y);

                    //遮罩图2 旋转
                    maskTexUV_2 -= float2(0.5f, 0.5f);
                    float maskTexRotateAngle_2 = _MaskTexRotation_2 * (PI * 2 / 360);
                    maskTexUV_2 = float2(maskTexUV_2.x * cos(maskTexRotateAngle_2) - maskTexUV_2.y * sin(maskTexRotateAngle_2),
                                        maskTexUV_2.x * sin(maskTexRotateAngle_2) + maskTexUV_2.y * cos(maskTexRotateAngle_2));
                    maskTexUV_2 += float2(0.5f, 0.5f);
                
                    //遮罩图2 流动
                    float2 maskTexFlow_2 = _MaskTexFlowAndBevel_2.xy;
                    maskTexUV_2 += _Time.y * maskTexFlow_2;

                    float4 maskTexColor_2 = SAMPLE_TEXTURE2D(_MaskTex_2, sampler_MaskTex_2, maskTexUV_2);
                    maskFac_2 = saturate(maskTexColor_2.r);
                    maskAlpha_2 = lerp(maskFac_2, maskTexColor_2.a, step(_MaskTexRedIsAlphaOn_2, 0));
                #endif
                
                //像素采样时偏移offset，用Mask权重进行修改
                screenUV += offset * maskAlpha_1 * maskAlpha_2;
                return SAMPLE_TEXTURE2D(_CustomCameraColorTexture, sampler_CustomCameraColorTexture, screenUV);
            }
            ENDHLSL
        }
    }
    CustomEditor "ReflectEffectGUI"
}