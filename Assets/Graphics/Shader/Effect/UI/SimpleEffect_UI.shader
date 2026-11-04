Shader "Effect/UI/SimpleEffect"
{
    Properties
    {
        _CullOff ("双面", Float) = 2.0
        _AddMode ("相加模式", Float) = 0.0
        _ZWriteOn ("是否写入深度", Float) = 0.0
        _ZTestOn ("开启深度测试", Float) = 2.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        _SoftParticleOn ("启用软粒子", Float) = 0
        _SoftParticleFade ("软粒子过渡", Range(0, 10)) = 0.25
        
        _MainTex ("主贴图", 2D) = "white"{}
        [HDR]_MainColor ("颜色", Color) = (1, 1, 1, 1)
        _MainColorIntensity ("颜色强度", Float) = 1
        
        _MainTexRedIsAlphaOn ("R是透明通道", Float) = 0
        
        _MainTexRotation ("主贴图旋转", Range(-360, 360)) = 0
        _MainTexFlowAndBevel ("主贴图 流动 & 斜切", Vector) = (0, 0, 0, 0)
        
        _MainTexCustomDataOn ("主贴图启用自定义数据", Float) = 0
        
        _MainTexPolarOn ("主贴图启用极坐标UV", Float) = 0
        _MainTexPolarRotation ("主贴图极坐标 旋转", Range(-360, 360)) = 0
        _MainTexPolarRadialScale ("主贴图极坐标半径 缩放", Float) = 1
        _MainTexPolarLengthScale ("主贴图极坐标长度 缩放", Float) = 1
        _MainTexPolarCenter_U ("主贴图极坐标中心 U", Float) = 0.5
        _MainTexPolarCenter_V ("主贴图极坐标中心 V", Float) = 0.5
        _MainTexPolarRadialOffset ("主贴图极坐标半径 偏移", Float) = 0
        _MainTexPolarLengthOffset ("主贴图极坐标长度 偏移", Float) = 0
        
        _MaskTexOn_1 ("启用遮罩图 1", Float) = 0
        _MaskTex_1 ("遮罩图 1", 2D) = "white"{}
        _MaskTexRotation_1 ("遮罩图 1 旋转", Range(-360, 360)) = 0
        _MaskTexFlowAndBevel_1 ("遮罩图 1 流动 & 斜切", Vector) = (0, 0, 0, 0)
        
        _MaskTexRedIsAlphaOn_1 ("R是透明通道", Float) = 0
        _MaskTexCustomDataOn_1 ("遮罩图 1 启用自定义数据", Float) = 0
        
        _MaskTexPolarOn_1 ("遮罩图 1 启用极坐标UV", Float) = 0
        _MaskTexPolarRotation_1 ("遮罩图 1 极坐标 旋转", Range(-360, 360)) = 0
        _MaskTexPolarRadialScale_1 ("遮罩图 1 极坐标半径 缩放", Float) = 1
        _MaskTexPolarLengthScale_1 ("遮罩图 1 极坐标长度 缩放", Float) = 1
        _MaskTexPolarCenter_U_1 ("遮罩图 1 极坐标中心 U", Float) = 0.5
        _MaskTexPolarCenter_V_1 ("遮罩图 1 极坐标中心 V", Float) = 0.5
        _MaskTexPolarRadialOffset_1 ("遮罩图 1 极坐标半径 偏移", Float) = 0
        _MaskTexPolarLengthOffset_1 ("遮罩图 1 极坐标长度 偏移", Float) = 0
        
        _MaskTexOn_2 ("启用遮罩图 2", Float) = 0
        _MaskTex_2 ("遮罩图 2", 2D) = "white"{}
        _MaskTexRotation_2 ("遮罩图 2 旋转", Range(-360, 360)) = 0
        _MaskTexFlowAndBevel_2 ("遮罩图 2 流动 & 斜切", Vector) = (0, 0, 0, 0)
        
        _MaskTexRedIsAlphaOn_2 ("R是透明通道", Float) = 0
        _MaskTexCustomDataOn_2 ("遮罩图 2 启用自定义数据", Float) = 0
        
        _MaskTexPolarOn_2 ("遮罩图 2 启用极坐标UV", Float) = 0
        _MaskTexPolarRotation_2 ("遮罩图 2 极坐标 旋转", Range(-360, 360)) = 0
        _MaskTexPolarRadialScale_2 ("遮罩图 2 极坐标半径 缩放", Float) = 1
        _MaskTexPolarLengthScale_2 ("遮罩图 2 极坐标长度 缩放", Float) = 1
        _MaskTexPolarCenter_U_2 ("遮罩图 2 极坐标中心 U", Float) = 0.5
        _MaskTexPolarCenter_V_2 ("遮罩图 2 极坐标中心 V", Float) = 0.5
        _MaskTexPolarRadialOffset_2 ("遮罩图 2 极坐标半径 偏移", Float) = 0
        _MaskTexPolarLengthOffset_2 ("遮罩图 2 极坐标长度 偏移", Float) = 0
        
        _DissolveOn ("启用溶解", Float) = 0
        _DissolveTex ("溶解图", 2D) = "black" {}
        _DissolveTexRotation ("溶解图 旋转", Range(-360, 360)) = 0
        _DissolveTexFlowAndBevel ("溶解图 流动 & 斜切", Vector) = (0, 0, 0, 0)
        [HDR]_DissolveColor ("溶解颜色", Color) = (1, 1, 1, 1)
        
        _DissolveCustomDataOn ("溶解 启用自定义数据", Float) = 0
        
        _DissolveTexPolarOn("溶解图 启用极坐标UV", Float) = 0
        _DissolveTexPolarRotation ("溶解图 极坐标 旋转", Range(-360, 360)) = 0
        _DissolveTexPolarRadialScale ("溶解图 极坐标半径 缩放", Float) = 1
        _DissolveTexPolarLengthScale ("溶解图 极坐标长度 缩放", Float) = 1
        _DissolveTexPolarCenter_U ("溶解图 极坐标中心 U", Float) = 0.5
        _DissolveTexPolarCenter_V ("溶解图 极坐标中心 V", Float) = 0.5
        _DissolveTexPolarRadialOffset ("溶解图 极坐标半径 偏移", Float) = 0
        _DissolveTexPolarLengthOffset ("溶解图 极坐标长度 偏移", Float) = 0
        
        _DissolveEmissionWidth ("亮边宽度", Range(0, 1)) = 0
        _DissolveSmooth ("软化程度", Range(0, 1)) = 0
        _DissolveThreshold ("溶解程度", Range(0, 1)) = 0
        
        
          _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue"="Transparent"
        }
         Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            
            Name "SimpleEffect"
            ColorMask RGBA
            Cull [_CullOff]
            ZWrite [_ZWriteOn]
            ZTest [_ZTestOn]
            Blend [_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "EffUIClip.hlsl"
            #pragma shader_feature_local _POLAR_ON
            #pragma shader_feature_local _SOFT_PARTICLE_ON
            #pragma shader_feature_local _MASK_TEX_ON_1
            #pragma shader_feature_local _MASK_TEX_POLAR_ON_1
            #pragma shader_feature_local _MASK_TEX_ON_2
            #pragma shader_feature_local _MASK_TEX_POLAR_ON_2
            #pragma shader_feature_local _DISSOLVE_ON
            #pragma shader_feature_local _DISSOLVE_POLAR_ON

            // #pragma multi_compile _ _POLAR_ON
            // #pragma multi_compile _ _SOFT_PARTICLE_ON
            // #pragma multi_compile _ _MASK_TEX_ON_1
            // #pragma multi_compile _ _MASK_TEX_POLAR_ON_1
            // #pragma multi_compile _ _MASK_TEX_ON_2
            // #pragma multi_compile _ _MASK_TEX_POLAR_ON_2
            // #pragma multi_compile _ _DISSOLVE_ON
            // #pragma multi_compile _ _DISSOLVE_POLAR_ON

            

            CBUFFER_START(UnityPerMaterial)
            float _SoftParticleFade;
            
            float4 _MainColor;
            float4 _MainTex_ST;
            float _MainTexRedIsAlphaOn;
            float _MainColorIntensity;
            float _MainTexRotation;
            float4 _MainTexFlowAndBevel;

            float _MainTexCustomDataOn;

            float _MainTexPolarRotation;
            float _MainTexPolarRadialScale;
            float _MainTexPolarLengthScale;
            float _MainTexPolarCenter_U;
            float _MainTexPolarCenter_V;
            float _MainTexPolarRadialOffset;
            float _MainTexPolarLengthOffset;

            float4 _MaskTex_1_ST;
            float _MaskTexRotation_1;
            float4 _MaskTexFlowAndBevel_1;
            float _MaskTexRedIsAlphaOn_1;
            float _MaskTexCustomDataOn_1;

            float _MaskTexPolarRotation_1;
            float _MaskTexPolarRadialScale_1;
            float _MaskTexPolarLengthScale_1;
            float _MaskTexPolarCenter_U_1;
            float _MaskTexPolarCenter_V_1;
            float _MaskTexPolarRadialOffset_1;
            float _MaskTexPolarLengthOffset_1;

            float4 _MaskTex_2_ST;
            float _MaskTexRotation_2;
            float4 _MaskTexFlowAndBevel_2;

            float _MaskTexRedIsAlphaOn_2;
            float _MaskTexCustomDataOn_2;

            float _MaskTexPolarRotation_2;
            float _MaskTexPolarRadialScale_2;
            float _MaskTexPolarLengthScale_2;
            float _MaskTexPolarCenter_U_2;
            float _MaskTexPolarCenter_V_2;
            float _MaskTexPolarRadialOffset_2;
            float _MaskTexPolarLengthOffset_2;

            float4 _DissolveTex_ST;
            float4 _DissolveColor;
            float _DissolveTexRotation;
            float4 _DissolveTexFlowAndBevel;

            float _DissolveCustomDataOn;

            float _DissolveTexPolarRotation;
            float _DissolveTexPolarRadialScale;
            float _DissolveTexPolarLengthScale;
            float _DissolveTexPolarCenter_U;
            float _DissolveTexPolarCenter_V;
            float _DissolveTexPolarRadialOffset;
            float _DissolveTexPolarLengthOffset;
            float _DissolveEmissionWidth;
            float4 _DissolveEmissionColor;
            float _DissolveSmooth;
            float _DissolveThreshold;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex_1);
            SAMPLER(sampler_MaskTex_1);
            TEXTURE2D(_MaskTex_2);
            SAMPLER(sampler_MaskTex_2);
            TEXTURE2D(_DissolveTex);
            SAMPLER(sampler_DissolveTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;      //CustomData zw
                float4 uv1 : TEXCOORD1;     //CustomData
                float4 uv2 : TEXCOORD2;     //CustomData
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;      //CustomData zw
                float4 uv1 : TEXCOORD1;     //CustomData
                float4 uv2 : TEXCOORD2;     //CustomData
                float4 scrPos : TEXCOORD3;
                 float4 mask : TEXCOORD4;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertex_position_inputs.positionCS;
                output.color = input.color;
                output.uv = input.uv;
                output.uv1 = input.uv1;
                output.uv2 = input.uv2;
                output.scrPos = ComputeScreenPos(output.vertex);
                 output.mask = GetUIMask(output.vertex, input.positionOS);
                return output;
            }

            void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
            {
                float2 delta = UV - Center;
                float radius = length(delta) * 2 * RadialScale;                 // 半径 * 缩放系数
                float angle = atan2(delta.x, delta.y) * 1.0/6.28 * LengthScale; // 角度 * 归一化 * 缩放系数
                Out = float2(radius, angle);
            }

            float4 frag(Varyings input) : SV_Target0
            {
                float2 mainTexUV = input.uv.xy;

                float2 mainTexTilling = lerp(_MainTex_ST.xy, input.uv.zw, _MainTexCustomDataOn);
                
                //主贴图 极坐标
                #if _POLAR_ON
                    //主贴图 旋转

                    // 对变换前的图像进行旋转处理
                    mainTexUV -= float2(0.5f, 0.5f);                            // uv中心
                    float mainRotateAngle = _MainTexRotation * (PI * 2 / 360);  // 角度转换
                    mainTexUV = float2(mainTexUV.x * cos(mainRotateAngle) - mainTexUV.y * sin(mainRotateAngle),
                                        mainTexUV.x * sin(mainRotateAngle) + mainTexUV.y * cos(mainRotateAngle));   // 二维旋转矩阵的应用
                    mainTexUV += float2(0.5f, 0.5f);
                
                    Unity_PolarCoordinates_float(mainTexUV, float2(_MainTexPolarCenter_U, _MainTexPolarCenter_V),
                                                _MainTexPolarRadialScale, _MainTexPolarLengthScale, mainTexUV);
                    mainTexUV += float2(_MainTexPolarRadialOffset, _MainTexPolarLengthOffset);
        
                    //主贴图 极坐标 缩放，偏移，斜切
                    mainTexUV = mainTexUV * mainTexTilling + _MainTex_ST.zw;
                    float2 mainTexBevel = _MainTexFlowAndBevel.zw;
                    mainTexUV = float2(mainTexUV.x + mainTexUV.y * mainTexBevel.x, mainTexUV.y + mainTexUV.x * mainTexBevel.y);

                    //主贴图 极坐标 二次旋转
                    mainTexUV -= float2(0.5f, 0.5f);
                    float mainPolarRotateAngle = _MainTexPolarRotation * (PI * 2 / 360);
                    mainTexUV = float2(mainTexUV.x * cos(mainPolarRotateAngle) - mainTexUV.y * sin(mainPolarRotateAngle),
                                        mainTexUV.x * sin(mainPolarRotateAngle) + mainTexUV.y * cos(mainPolarRotateAngle));
                    mainTexUV += float2(0.5f, 0.5f);
                #else 
                    //主贴图 缩放，偏移，斜切
                    mainTexUV = mainTexUV * mainTexTilling + _MainTex_ST.zw;
                    float2 mainTexBevel = _MainTexFlowAndBevel.zw;
                    mainTexUV = float2(mainTexUV.x + mainTexUV.y * mainTexBevel.x, mainTexUV.y + mainTexUV.x * mainTexBevel.y);

                    //主贴图 旋转
                    mainTexUV -= float2(0.5f, 0.5f);
                    float mainRotateAngle = _MainTexRotation * (PI * 2 / 360);
                    mainTexUV = float2(mainTexUV.x * cos(mainRotateAngle) - mainTexUV.y * sin(mainRotateAngle),
                                        mainTexUV.x * sin(mainRotateAngle) + mainTexUV.y * cos(mainRotateAngle));
                    mainTexUV += float2(0.5f, 0.5f);
                #endif
                
                //主贴图 流动
                float2 mainTexFlow = _MainTexFlowAndBevel.xy;
                mainTexUV += _Time.y * mainTexFlow;

                //主贴图 采样
                float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV);

                //主贴图 R是透明通道
                mainTexColor.a = lerp(mainTexColor.a, mainTexColor.r, _MainTexRedIsAlphaOn);
                
                //遮罩图1
                float maskFac_1 = 1;
                float maskAlpha_1 = 1;
                #if _MASK_TEX_ON_1
                    float2 maskTexUV_1 = input.uv;

                    float2 maskTexTilling_1 = lerp(_MaskTex_1_ST, input.uv1.xy, _MaskTexCustomDataOn_1);
                    //遮罩图1 极坐标
                    #if _MASK_TEX_POLAR_ON_1
                        //遮罩图1 旋转
                        maskTexUV_1 -= float2(0.5f, 0.5f);
                        float maskTexRotateAngle_1 = _MaskTexRotation_1 * (PI * 2 / 360);
                        maskTexUV_1 = float2(maskTexUV_1.x * cos(maskTexRotateAngle_1) - maskTexUV_1.y * sin(maskTexRotateAngle_1),
                                            maskTexUV_1.x * sin(maskTexRotateAngle_1) + maskTexUV_1.y * cos(maskTexRotateAngle_1));
                        maskTexUV_1 += float2(0.5f, 0.5f);

                        //遮罩图1 极坐标
                        Unity_PolarCoordinates_float(maskTexUV_1, float2(_MaskTexPolarCenter_U_1, _MaskTexPolarCenter_V_1),
                                                _MaskTexPolarRadialScale_1, _MaskTexPolarLengthScale_1, maskTexUV_1);
                        maskTexUV_1 += float2(_MaskTexPolarRadialOffset_1, _MaskTexPolarLengthOffset_1);

                        //遮罩图1 缩放，偏移，斜切
                        maskTexUV_1 = maskTexUV_1 * maskTexTilling_1 + _MaskTex_1_ST.zw;
                        float2 maskTexBevel_1 = _MaskTexFlowAndBevel_1.zw;
                        maskTexUV_1 = float2(maskTexUV_1.x + maskTexUV_1.y * maskTexBevel_1.x, maskTexUV_1.y + maskTexUV_1.x * maskTexBevel_1.y);

                        //遮罩图1 旋转
                        maskTexUV_1 -= float2(0.5f, 0.5f);
                        float maskTexPolarRotateAngle_1 = _MaskTexPolarRotation_1 * (PI * 2 / 360);
                        maskTexUV_1 = float2(maskTexUV_1.x * cos(maskTexPolarRotateAngle_1) - maskTexUV_1.y * sin(maskTexPolarRotateAngle_1),
                                            maskTexUV_1.x * sin(maskTexPolarRotateAngle_1) + maskTexUV_1.y * cos(maskTexPolarRotateAngle_1));
                        maskTexUV_1 += float2(0.5f, 0.5f);
                    #else
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
                    #endif

                    //遮罩图1 流动
                    float2 maskTexFlow_1 = _MaskTexFlowAndBevel_1.xy;
                    maskTexUV_1 += _Time.y * maskTexFlow_1;

                    float4 maskTexColor_1 = SAMPLE_TEXTURE2D(_MaskTex_1, sampler_MaskTex_1, maskTexUV_1);
                    maskFac_1 = saturate(maskTexColor_1.r);
                    maskAlpha_1 = lerp(saturate(maskTexColor_1.a), maskFac_1, _MaskTexRedIsAlphaOn_1);
                #endif

                //遮罩图2
                float maskFac_2 = 1;
                float maskAlpha_2 = 1;
                #if _MASK_TEX_ON_2
                    float2 maskTexUV_2 = input.uv;

                    float2 maskTexTilling_2 = lerp(_MaskTex_2_ST.xy, input.uv1.zw, _MaskTexCustomDataOn_2);
                    float2 maskTexOffset_2 = lerp(_MaskTex_2_ST.zw, float2(input.uv2.y, _MaskTex_2_ST.w), _MaskTexCustomDataOn_2);
                    //遮罩图2 极坐标
                    #if _MASK_TEX_POLAR_ON_2
                        //遮罩图2 旋转
                        maskTexUV_2 -= float2(0.5f, 0.5f);
                        float maskTexRotateAngle_2 = _MaskTexRotation_2 * (PI * 2 / 360);
                        maskTexUV_2 = float2(maskTexUV_2.x * cos(maskTexRotateAngle_2) - maskTexUV_2.y * sin(maskTexRotateAngle_2),
                                            maskTexUV_2.x * sin(maskTexRotateAngle_2) + maskTexUV_2.y * cos(maskTexRotateAngle_2));
                        maskTexUV_2 += float2(0.5f, 0.5f);
                
                        //遮罩图2 极坐标
                        Unity_PolarCoordinates_float(maskTexUV_2, float2(_MaskTexPolarCenter_U_2, _MaskTexPolarCenter_V_2),
                                                _MaskTexPolarRadialScale_2, _MaskTexPolarLengthScale_2, maskTexUV_2);
                        maskTexUV_2 += float2(_MaskTexPolarRadialOffset_2, _MaskTexPolarLengthOffset_2);

                        //遮罩图2 缩放，偏移，斜切
                        maskTexUV_2 = maskTexUV_2 * maskTexTilling_2 + maskTexOffset_2;
                        float2 maskTexBevel_2 = _MaskTexFlowAndBevel_2.zw;
                        maskTexUV_2 = float2(maskTexUV_2.x + maskTexUV_2.y * maskTexBevel_2.x, maskTexUV_2.y + maskTexUV_2.x * maskTexBevel_2.y);

                        //遮罩图2 极坐标 旋转
                        maskTexUV_2 -= float2(0.5f, 0.5f);
                        float maskTexPolarRotateAngle_2 = _MaskTexPolarRotation_2 * (PI * 2 / 360);
                        maskTexUV_2 = float2(maskTexUV_2.x * cos(maskTexPolarRotateAngle_2) - maskTexUV_2.y * sin(maskTexPolarRotateAngle_2),
                                            maskTexUV_2.x * sin(maskTexPolarRotateAngle_2) + maskTexUV_2.y * cos(maskTexPolarRotateAngle_2));
                        maskTexUV_2 += float2(0.5f, 0.5f);
                    #else
                        //遮罩图2 缩放，偏移，斜切
                        maskTexUV_2 = maskTexUV_2 * maskTexTilling_2 + maskTexOffset_2;
                        float2 maskTexBevel_2 = _MaskTexFlowAndBevel_2.zw;
                        maskTexUV_2 = float2(maskTexUV_2.x + maskTexUV_2.y * maskTexBevel_2.x, maskTexUV_2.y + maskTexUV_2.x * maskTexBevel_2.y);

                        //遮罩图2 旋转
                        maskTexUV_2 -= float2(0.5f, 0.5f);
                        float maskTexRotateAngle_2 = _MaskTexRotation_2 * (PI * 2 / 360);
                        maskTexUV_2 = float2(maskTexUV_2.x * cos(maskTexRotateAngle_2) - maskTexUV_2.y * sin(maskTexRotateAngle_2),
                                            maskTexUV_2.x * sin(maskTexRotateAngle_2) + maskTexUV_2.y * cos(maskTexRotateAngle_2));
                        maskTexUV_2 += float2(0.5f, 0.5f);
                    #endif
                
                    //遮罩图2 流动
                    float2 maskTexFlow_2 = _MaskTexFlowAndBevel_2.xy;
                    maskTexUV_2 += _Time.y * maskTexFlow_2;

                    float4 maskTexColor_2 = SAMPLE_TEXTURE2D(_MaskTex_2, sampler_MaskTex_2, maskTexUV_2);
                    maskFac_2 = saturate(maskTexColor_2.r);
                    maskAlpha_2 = lerp(maskTexColor_2.a, maskFac_2, _MaskTexRedIsAlphaOn_2);
                #endif

                //溶解
                float dissolveEdge = 0;
                float dissolveAlpha = 1;
                //float dissolveEdgeGlow = 0;
                #if _DISSOLVE_ON
                    float2 dissolveTexUV = input.uv;

                    float dissolveThreshold = lerp(_DissolveThreshold, input.uv2.x, _DissolveCustomDataOn);
                    //溶解图 极坐标
                    #if _DISSOLVE_POLAR_ON
                        //溶解图 旋转
                        dissolveTexUV -= float2(0.5f, 0.5f);
                        float dissolveTexRotateAngle = _DissolveTexRotation * (PI * 2 / 360);
                        dissolveTexUV = float2(dissolveTexUV.x * cos(dissolveTexRotateAngle) - dissolveTexUV.y * sin(dissolveTexRotateAngle),
                                            dissolveTexUV.x * sin(dissolveTexRotateAngle) + dissolveTexUV.y * cos(dissolveTexRotateAngle));
                        dissolveTexUV += float2(0.5f, 0.5f);

                        //溶解图 极坐标 旋转
                        Unity_PolarCoordinates_float(dissolveTexUV, float2(_DissolveTexPolarCenter_U, _DissolveTexPolarCenter_V),
                                                _DissolveTexPolarRadialScale, _DissolveTexPolarLengthScale, dissolveTexUV);
                        dissolveTexUV += float2(_DissolveTexPolarRadialOffset, _DissolveTexPolarLengthOffset);

                        //溶解图 缩放，偏移，斜切
                        dissolveTexUV = dissolveTexUV * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
                        float2 dissolveTexBevel = _DissolveTexFlowAndBevel.zw;
                        dissolveTexUV = float2(dissolveTexUV.x + dissolveTexUV.y * dissolveTexBevel.x, dissolveTexUV.y + dissolveTexUV.x * dissolveTexBevel.y);

                        //溶解图 旋转
                        dissolveTexUV -= float2(0.5f, 0.5f);
                        float dissolveTexPolarRotateAngle = _DissolveTexPolarRotation * (PI * 2 / 360);
                        dissolveTexUV = float2(dissolveTexUV.x * cos(dissolveTexPolarRotateAngle) - dissolveTexUV.y * sin(dissolveTexPolarRotateAngle),
                                            dissolveTexUV.x * sin(dissolveTexPolarRotateAngle) + dissolveTexUV.y * cos(dissolveTexPolarRotateAngle));
                        dissolveTexUV += float2(0.5f, 0.5f);
                    #else
                        //溶解图 缩放，偏移，斜切
                        dissolveTexUV = dissolveTexUV * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
                        float2 dissolveTexBevel = _DissolveTexFlowAndBevel.zw;
                        dissolveTexUV = float2(dissolveTexUV.x + dissolveTexUV.y * dissolveTexBevel.x, dissolveTexUV.y + dissolveTexUV.x * dissolveTexBevel.y);

                        //溶解图 旋转
                        dissolveTexUV -= float2(0.5f, 0.5f);
                        float dissolveTexRotateAngle = _DissolveTexRotation * (PI * 2 / 360);
                        dissolveTexUV = float2(dissolveTexUV.x * cos(dissolveTexRotateAngle) - dissolveTexUV.y * sin(dissolveTexRotateAngle),
                                            dissolveTexUV.x * sin(dissolveTexRotateAngle) + dissolveTexUV.y * cos(dissolveTexRotateAngle));
                        dissolveTexUV += float2(0.5f, 0.5f);
                    #endif

                    //溶解图 流动
                    float2 dissolveTexFlow = _DissolveTexFlowAndBevel.xy;
                    dissolveTexUV += _Time.y * dissolveTexFlow;

                    float dissolveNoise= SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, dissolveTexUV).r;

                    float baseThreshold = (-_DissolveEmissionWidth + (dissolveThreshold - 0.0) * (1.0 - -_DissolveEmissionWidth) / (1.0 - 0.0));
                    float softRange = _DissolveSmooth + 0.0001;

                    // 主溶解区域计算
                    float dissolveRange = baseThreshold + _DissolveEmissionWidth;
                    float dissolveUpper = (-softRange + (dissolveRange - 0.0) * (1.0 - -softRange) / (1.0 - 0.0));
                    dissolveAlpha = smoothstep(dissolveUpper, dissolveUpper + softRange, dissolveNoise);

                    // 边缘发光区域计算
                    float edgeThreshold = baseThreshold;
                    float edgeUpper = (-softRange + (edgeThreshold - 0.0) * (1.0 - -softRange) / (1.0 - 0.0));
                    dissolveEdge = 1.0 - smoothstep(edgeUpper, edgeUpper + softRange, dissolveNoise);
                #endif
                
                //软粒子
                float softParticleFade = 1;
                #if _SOFT_PARTICLE_ON
                    float2 scrPos = input.scrPos.xy / input.scrPos.w;   // 屏幕uv
                    float depth_01 = SampleSceneDepth(scrPos);          // 当前像素深度
                    float depth_eye = LinearEyeDepth(depth_01, _ZBufferParams); // 线性深度（相机出发）
                    softParticleFade = saturate((10 - _SoftParticleFade) * (depth_eye - input.scrPos.w));   
                #endif

                //粒子系统参数
                float4 particleColor = input.color;
                
                //粒子颜色 * 主贴图 * 颜色 * 颜色强度
                float3 finalColor = particleColor.xyz * mainTexColor.xyz * _MainColor.xyz * _MainColorIntensity;
                //溶解亮边颜色
                finalColor = lerp(finalColor, _DissolveColor.rgb, _DissolveColor.a * (1 - dissolveAlpha));
                finalColor += dissolveEdge * _DissolveColor.rgb * _DissolveColor.a;
                
                //粒子Alpha * 主贴图Alpha * 颜色的Alpha * 遮罩图 1 Alpha * 遮罩图 2 Alpha * 软粒子Alpha
                float finalAlpha = particleColor.a * mainTexColor.a * _MainColor.a
                                * maskAlpha_1 * maskAlpha_2
                                * softParticleFade * dissolveAlpha;
                  SetUIMask(float4(finalColor, finalAlpha),input.mask);
                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "SimpleEffectGUI"
}