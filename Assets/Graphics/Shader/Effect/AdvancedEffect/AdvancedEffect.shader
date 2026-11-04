Shader "Effect/AdvancedEffect"
{
    Properties
    {
        _CullOff ("双面", Float) = 2.0
        _AddMode ("相加模式", Float) = 0.0
        _ZWriteOn ("是否写入深度", Float) = 0.0
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

        _MainTexCustomDataOn_TillingX ("主贴图 Tilling X 启用自定义数据", Float) = 0
        _MainTexCustomDataOn_TillingY ("主贴图 Tilling Y 启用自定义数据", Float) = 0

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
        _MaskTexCustomDataOn_1_TillingX ("遮罩图 1 Tilling X 启用自定义数据", Float) = 0
        _MaskTexCustomDataOn_1_TillingY ("遮罩图 1 Tilling Y 启用自定义数据", Float) = 0

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
        _MaskTexCustomDataOn_2_TillingX ("遮罩图 2 Tilling X 启用自定义数据", Float) = 0
        _MaskTexCustomDataOn_2_TillingY ("遮罩图 2 Tilling Y 启用自定义数据", Float) = 0
        _MaskTexCustomDataOn_2_OffsetX ("遮罩图 2 Offset X 启用自定义数据", Float) = 0

        _MaskTexPolarOn_2 ("遮罩图 2 启用极坐标UV", Float) = 0
        _MaskTexPolarRotation_2 ("遮罩图 2 极坐标 旋转", Range(-360, 360)) = 0
        _MaskTexPolarRadialScale_2 ("遮罩图 2 极坐标半径 缩放", Float) = 1
        _MaskTexPolarLengthScale_2 ("遮罩图 2 极坐标长度 缩放", Float) = 1
        _MaskTexPolarCenter_U_2 ("遮罩图 2 极坐标中心 U", Float) = 0.5
        _MaskTexPolarCenter_V_2 ("遮罩图 2 极坐标中心 V", Float) = 0.5
        _MaskTexPolarRadialOffset_2 ("遮罩图 2 极坐标半径 偏移", Float) = 0
        _MaskTexPolarLengthOffset_2 ("遮罩图 2 极坐标长度 偏移", Float) = 0

        _TurbulenceOn ("启用扭曲", Float) = 0
        _TurbulenceTex ("扭曲贴图", 2D) = "black" {}
        _TurbulenceFlow ("遮罩图  流动 ", Vector) = (0, 0, 0, 0)
        _TurbulenceBasePoint ("扭曲基准值", Range(0,1)) = 0
        _TurbulenceStrength ("扭曲强度", Vector ) = (0, 0, 0, 0)
        _TurbulenceMask("扭曲遮罩1", Float ) = 0
        _TurbulenceMask2("扭曲遮罩2", Float ) = 0



        _DissolveOn ("启用溶解", Float) = 0
        _DissolveTex ("溶解图", 2D) = "black" {}
        _DissolveTexRotation ("溶解图 旋转", Range(-360, 360)) = 0
        _DissolveTexFlowAndBevel ("溶解图 流动 & 斜切", Vector) = (0, 0, 0, 0)
        [HDR]_DissolveColor ("溶解颜色", Color) = (1, 1, 1, 1)

        // 附加溶解图
        _DissolveTex2On("启用附加溶解图", Float) = 0
        _DissolveTex2("附加溶解图", 2D) = "black" {}
        _DissolveTex2Rotation ("附加溶解图 旋转", Range(-360, 360)) = 0
        _DissolveTex2Strength("附加溶解图 强度", Range(0, 2)) = 1
        _DissolveTex2Blend("附加溶解图 混合系数", Range(0, 1)) = 0
        _DissolveTex2Width("附加溶解图 过渡宽度", Range(0, 5)) = 1
        _DissolveTex2Offset("附加溶解图 偏移", Range(0, 0.5)) = 0
        // ===

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

        // 方向溶解
        _DissolveDirectionOn("溶解 启用方向控制", Float) = 0
        _DissolveDirectionAngle ("方向角度", Range(0, 360)) = 0
        _DissolveDirectionWidth ("方向过渡宽度", Range(0, 5)) = 1
        _DissolveDirectionOffset("方向过渡偏移", Range(0, 0.5)) = 0

        _DepthIntersectOn ("启用相切亮边", Float) = 0
        [HDR]_DepthIntersectColor ("相切边缘颜色", Color) = (1, 1, 1, 1)
        _DepthIntersectWidth ("相切宽度", Range(0, 1)) = 0.1
        _DepthIntersectSmooth ("相切边缘颜色过渡", Range(0, 1)) = 1
        _DepthSensitivity ("相切深度灵敏度", Range(0, 10)) = 0.1
        _DepthIntersectHardEdge ("相切边缘硬边程度", Range(0, 1)) = 0

        // 枪械子弹显示
        _BulletOn ("启用子弹显示", Float) = 0
        _BulletCol("子弹贴图列", Float) = 0
        _BulletRow("子弹贴图行", Float) = 0
        _BulletNumber("当前数字", Float) = 0

        // 顶点偏移效果
        _VertexOffsetOn("启用顶点偏移", Float) = 0
        _VertexOffsetTex("顶点偏移贴图", 2D) = "black"{}
        _VertexOffsetMaskTex("顶点遮罩贴图", 2D) = "white"{}
        _VertexOffsetMaskRIsAlphaOn("R是透明通道", Float) = 0
        _VertexOffsetStrength("顶点偏移程度", Float) = 0.5
        _VertexOffsetCustomDataOn_Strength("顶点偏移程度 启用自定义数据", Float) = 0
        _VertexOffsetFlowU("U流动", Float) = 0
        _VertexOffsetCustomDataOn_MaskTex_ST_Z("FlowU 启用自定义数据", Float) = 0
        _VertexOffsetFlowV("V流动", Float) = 0
        _VertexOffsetCustomDataOn_MaskTex_ST_W("FlowV 启用自定义数据", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
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
            Blend [_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma shader_feature_local _POLAR_ON
            #pragma shader_feature_local _SOFT_PARTICLE_ON
            #pragma shader_feature_local _MASK_TEX_ON_1
            #pragma shader_feature_local _MASK_TEX_POLAR_ON_1
            #pragma shader_feature_local _MASK_TEX_ON_2
            #pragma shader_feature_local _MASK_TEX_POLAR_ON_2
            #pragma shader_feature_local _DISSOLVE_ON
            #pragma shader_feature_local _DISSOLVE_POLAR_ON
            #pragma shader_feature_local _DEPTH_INTERSECT
            #pragma shader_feature_local _DISSOLVE_DIRECTION_ON
            #pragma shader_feature_local _DISSOLVE_TEX2_ON
            #pragma shader_feature_local _BULLET_ON
            #pragma shader_feature_local _VERTEXOFFSET_ON
            #pragma shader_feature_local _VERTEXOFFSET_ON
            #pragma shader_feature_local _TURBULENCE_ON

            // 打包用，否则会丢失
            // #pragma multi_compile _ _POLAR_ON
            // #pragma multi_compile _ _SOFT_PARTICLE_ON
            // #pragma multi_compile _ _MASK_TEX_ON_1
            // #pragma multi_compile _ _MASK_TEX_POLAR_ON_1
            // #pragma multi_compile _ _MASK_TEX_ON_2
            // #pragma multi_compile _ _MASK_TEX_POLAR_ON_2
            // #pragma multi_compile _ _DISSOLVE_ON
            // #pragma multi_compile _ _DISSOLVE_POLAR_ON
            // #pragma multi_compile _ _DEPTH_INTERSECT
            // #pragma multi_compile _ _DISSOLVE_DIRECTION_ON
            // #pragma multi_compile _ _DISSOLVE_TEX2_ON
            // #pragma multi_compile _ _BULLET_ON
            // #pragma multi_compile _ _VERTEXOFFSET_ON
            //  #pragma multi_compile _ _TURBULENCE_ON

            CBUFFER_START(UnityPerMaterial)
                float _SoftParticleFade;

                float4 _MainColor;
                float4 _MainTex_ST;
                float _MainTexRedIsAlphaOn;
                float _MainColorIntensity;
                float _MainTexRotation;
                float4 _MainTexFlowAndBevel;

                float _MainTexCustomDataOn_TillingX;
                float _MainTexCustomDataOn_TillingY;

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
                float _MaskTexCustomDataOn_1_TillingX;
                float _MaskTexCustomDataOn_1_TillingY;
                float _MaskTexCustomDataOn_2_OffsetX;

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
                float _MaskTexCustomDataOn_2_TillingX;
                float _MaskTexCustomDataOn_2_TillingY;

                float _MaskTexPolarRotation_2;
                float _MaskTexPolarRadialScale_2;
                float _MaskTexPolarLengthScale_2;
                float _MaskTexPolarCenter_U_2;
                float _MaskTexPolarCenter_V_2;
                float _MaskTexPolarRadialOffset_2;
                float _MaskTexPolarLengthOffset_2;

                float4 _TurbulenceFlow;
                float _TurbulenceBasePoint;
                float4 _TurbulenceStrength;
                float _TurbulenceMask;
                float _TurbulenceMask2;
                float4 _TurbulenceTex_ST;

                float4 _DissolveTex_ST;
                float4 _DissolveColor;
                float _DissolveTexRotation;
                float4 _DissolveTexFlowAndBevel;

                float4 _DissolveTex2_ST;
                float _DissolveTex2Rotation;
                float _DissolveTex2Strength;
                float _DissolveTex2Blend;
                float _DissolveTex2Width;
                float _DissolveTex2Offset;

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

                float _DissolveDirectionAngle;
                float _DissolveDirectionWidth;
                float _DissolveDirectionOffset;

                float _BulletCol;
                float _BulletRow;
                float _BulletNumber;

                float4 _VertexOffsetTex_ST;
                float4 _VertexOffsetMaskTex_ST;
                float _VertexOffsetStrength;
                float _VertexOffsetFlowU;
                float _VertexOffsetFlowV;
                float _VertexOffsetMaskRIsAlphaOn;
                float _VertexOffsetCustomDataOn_Strength;
                float _VertexOffsetCustomDataOn_MaskTex_ST_Z;
                float _VertexOffsetCustomDataOn_MaskTex_ST_W;

                float4 _DepthIntersectColor;
                float _DepthIntersectWidth;
                float _DepthIntersectSmooth;
                float _DepthSensitivity;
                float _DepthIntersectHardEdge;


            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex_1);
            SAMPLER(sampler_MaskTex_1);
            TEXTURE2D(_MaskTex_2);
            SAMPLER(sampler_MaskTex_2);
            TEXTURE2D(_DissolveTex);
            SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_DissolveTex2);
            SAMPLER(sampler_DissolveTex2);
            TEXTURE2D(_TurbulenceTex);
            SAMPLER(sampler_TurbulenceTex);

            // 顶点偏移
            TEXTURE2D(_VertexOffsetTex);
            SAMPLER(sampler_VertexOffsetTex);
            TEXTURE2D(_VertexOffsetMaskTex);
            SAMPLER(sampler_VertexOffsetMaskTex);


            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0; //CustomData zw
                float4 uv1 : TEXCOORD1; //CustomData
                float4 uv2 : TEXCOORD2; //CustomData
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0; //CustomData zw
                float4 uv1 : TEXCOORD1; //CustomData
                float4 uv2 : TEXCOORD2; //CustomData
                float4 scrPos : TEXCOORD3;

                float3 positionWS : TEXCOORD4;
                float4 VertexOffset : TEXCOORD5;
            };

            float rand1(float x)
            {
                return frac(sin(x * 12.9898) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                float customData[8] = {
                    input.uv.z, input.uv.w, input.uv1.x, input.uv1.y, input.uv1.z, input.uv1.w, input.uv2.x, input.uv2.y
                };
                int customDataIndex = 0;
                Varyings output;
                output.color = input.color;
                #if _VERTEXOFFSET_ON
                    float2 uv = input.uv;
                    // 顶点偏移程度
                    _VertexOffsetStrength = lerp(_VertexOffsetStrength, customData[customDataIndex], _VertexOffsetCustomDataOn_Strength);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _VertexOffsetCustomDataOn_Strength);
                    // 强度贴图
                    float2 flowUV = uv + float2(_VertexOffsetFlowU, _VertexOffsetFlowV) * _Time.y;  // UV流动
                    flowUV = TRANSFORM_TEX(flowUV, _VertexOffsetTex);
                    float offsetSample = SAMPLE_TEXTURE2D_LOD(_VertexOffsetTex, sampler_VertexOffsetTex, flowUV, 0).r;
                    offsetSample = offsetSample * 2.0 - 1.0;
                    // 遮罩贴图
                    float _VertexOffsetMaskTex_ST_X = lerp(_VertexOffsetMaskTex_ST.z, customData[customDataIndex], _VertexOffsetCustomDataOn_MaskTex_ST_Z);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _VertexOffsetCustomDataOn_MaskTex_ST_Z);
                    float _VertexOffsetMaskTex_ST_Y = lerp(_VertexOffsetMaskTex_ST.w, customData[customDataIndex], _VertexOffsetCustomDataOn_MaskTex_ST_W);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _VertexOffsetCustomDataOn_MaskTex_ST_W);
                    float2 maskUV = uv * _VertexOffsetMaskTex_ST.xy + float2(_VertexOffsetMaskTex_ST_X, _VertexOffsetMaskTex_ST_Y);
                    float mask = SAMPLE_TEXTURE2D_LOD(_VertexOffsetMaskTex, sampler_VertexOffsetMaskTex, maskUV, 0).r;
                    // 偏移方向：顶点方向
                    float3 offsetDir =  input.normalOS * offsetSample * mask * _VertexOffsetStrength ;
                    input.positionOS.xyz += offsetDir;
                    // 顶点偏移的专属值
                    output.VertexOffset.yz = maskUV;
                #endif

                // 顶点偏移的专属值
                output.VertexOffset.x = customDataIndex;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertex_position_inputs.positionCS;
                // output.color = input.color;
                output.uv = input.uv;
                output.uv1 = input.uv1;
                output.uv2 = input.uv2;
                output.scrPos = ComputeScreenPos(output.vertex);
                output.positionWS = vertex_position_inputs.positionWS;
                return output;
            }

            void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale,
                           out float2 Out)
            {
                float2 delta = UV - Center;
                float radius = length(delta) * 2 * RadialScale;
                float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
                Out = float2(radius, angle);
            }

            void frag(Varyings input, out float4 Color0 : SV_Target0)
            {
                float customData[8] = {
                    input.uv.z, input.uv.w, input.uv1.x, input.uv1.y, input.uv1.z, input.uv1.w, input.uv2.x, input.uv2.y
                };
                int customDataIndex = (int)round(input.VertexOffset.x);

                //UV扭曲
                float2 turbulenceUV = 0;

                #if _TURBULENCE_ON
                 turbulenceUV = input.uv.xy;
                float2 turbulenceTexUV = TRANSFORM_TEX(turbulenceUV, _TurbulenceTex);
                turbulenceTexUV.xy = (turbulenceTexUV.xy + _Time.y * _TurbulenceFlow.xy);
                float turbulenceSample = SAMPLE_TEXTURE2D(_TurbulenceTex, sampler_TurbulenceTex, turbulenceTexUV).x;
                 turbulenceUV = (turbulenceSample - _TurbulenceBasePoint) / (1-_TurbulenceBasePoint) ;
                turbulenceUV *= _TurbulenceStrength;
                #endif


                float2 mainTexUV = input.uv.xy;

                float mainTexTillingX = lerp(_MainTex_ST.x, customData[customDataIndex], _MainTexCustomDataOn_TillingX);
                customDataIndex = lerp(customDataIndex, customDataIndex + 1, _MainTexCustomDataOn_TillingX);
                float mainTexTillingY = lerp(_MainTex_ST.y, customData[customDataIndex], _MainTexCustomDataOn_TillingY);
                customDataIndex = lerp(customDataIndex, customDataIndex + 1, _MainTexCustomDataOn_TillingY);

                //主贴图 极坐标
                #if _POLAR_ON
                    //主贴图 旋转
                    mainTexUV -= float2(0.5f, 0.5f);
                    float mainRotateAngle = _MainTexRotation * (PI * 2 / 360);
                    mainTexUV = float2(mainTexUV.x * cos(mainRotateAngle) - mainTexUV.y * sin(mainRotateAngle),
                                        mainTexUV.x * sin(mainRotateAngle) + mainTexUV.y * cos(mainRotateAngle));
                    mainTexUV += float2(0.5f, 0.5f);
                
                    Unity_PolarCoordinates_float(mainTexUV, float2(_MainTexPolarCenter_U, _MainTexPolarCenter_V),
                                                _MainTexPolarRadialScale, _MainTexPolarLengthScale, mainTexUV);
                    mainTexUV += float2(_MainTexPolarRadialOffset, _MainTexPolarLengthOffset);

                    //主贴图 极坐标 缩放，偏移，斜切
                    mainTexUV = mainTexUV * float2(mainTexTillingX, mainTexTillingY) + _MainTex_ST.zw;
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
                mainTexUV = mainTexUV * float2(mainTexTillingX, mainTexTillingY) + _MainTex_ST.zw;
                float2 mainTexBevel = _MainTexFlowAndBevel.zw;
                mainTexUV = float2(mainTexUV.x + mainTexUV.y * mainTexBevel.x,
                                 mainTexUV.y + mainTexUV.x * mainTexBevel.y);

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
                float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV + turbulenceUV);

                //主贴图 R是透明通道
                mainTexColor.a = lerp(mainTexColor.a, mainTexColor.r, _MainTexRedIsAlphaOn);

                //遮罩图1
                float maskFac_1 = 1;
                float maskAlpha_1 = 1;
                #if _MASK_TEX_ON_1
                    float2 maskTexUV_1 = input.uv;

                    float maskTexTilling_1_TillingX = lerp(_MaskTex_1_ST.x, customData[customDataIndex], _MaskTexCustomDataOn_1_TillingX);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _MaskTexCustomDataOn_1_TillingX);
                    float maskTexTilling_1_TillingY = lerp(_MaskTex_1_ST.y, customData[customDataIndex], _MaskTexCustomDataOn_1_TillingY);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _MaskTexCustomDataOn_1_TillingY);
                
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
                        maskTexUV_1 = maskTexUV_1 * float2(maskTexTilling_1_TillingX, maskTexTilling_1_TillingY) + _MaskTex_1_ST.zw;
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
                        maskTexUV_1 = maskTexUV_1 * float2(maskTexTilling_1_TillingX, maskTexTilling_1_TillingY) + _MaskTex_1_ST.zw;
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

                    float4 maskTexColor_1 = SAMPLE_TEXTURE2D(_MaskTex_1, sampler_MaskTex_1, maskTexUV_1 + turbulenceUV * _TurbulenceMask);
                    maskFac_1 = saturate(maskTexColor_1.r);
                    maskAlpha_1 = lerp(saturate(maskTexColor_1.a), maskFac_1, _MaskTexRedIsAlphaOn_1);
                #endif

                //遮罩图2
                float maskFac_2 = 1;
                float maskAlpha_2 = 1;
                #if _MASK_TEX_ON_2
                    float2 maskTexUV_2 = input.uv;

                    float maskTexTilling_2_TillingX = lerp(_MaskTex_2_ST.x, customData[customDataIndex], _MaskTexCustomDataOn_2_TillingX);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _MaskTexCustomDataOn_2_TillingX);
                    float maskTexTilling_2_TillingY = lerp(_MaskTex_2_ST.y, customData[customDataIndex], _MaskTexCustomDataOn_2_TillingY);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _MaskTexCustomDataOn_2_TillingY);
                 
                    float2 maskTexOffset_2 = lerp(_MaskTex_2_ST.zw, float2(customData[customDataIndex], _MaskTex_2_ST.w), _MaskTexCustomDataOn_2_OffsetX);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _MaskTexCustomDataOn_2_OffsetX);
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
                        maskTexUV_2 = maskTexUV_2 * float2(maskTexTilling_2_TillingX, maskTexTilling_2_TillingY) + maskTexOffset_2;
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
                        maskTexUV_2 = maskTexUV_2 * float2(maskTexTilling_2_TillingX, maskTexTilling_2_TillingY) + maskTexOffset_2;
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

                    float4 maskTexColor_2 = SAMPLE_TEXTURE2D(_MaskTex_2, sampler_MaskTex_2, maskTexUV_2 + turbulenceUV * _TurbulenceMask2);
                    maskFac_2 = saturate(maskTexColor_2.r);
                    maskAlpha_2 = lerp(maskTexColor_2.a, maskFac_2, _MaskTexRedIsAlphaOn_2);
                #endif

                //溶解
                float dissolveEdge = 0;
                float dissolveAlpha = 1;
                #if _DISSOLVE_ON
                    float2 dissolveTexUV = input.uv.xy;
                    
                    float dissolveThreshold = lerp(_DissolveThreshold, customData[customDataIndex], _DissolveCustomDataOn);
                    customDataIndex = lerp(customDataIndex, customDataIndex + 1, _DissolveCustomDataOn);
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

                    float dissolveNoise = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, dissolveTexUV).r;  
                    
                    // ==== 附加溶解图 ====
                #if _DISSOLVE_TEX2_ON

                        float2 dissolveTex2UV = input.uv.xy;
                        // 缩放，偏移
                        dissolveTex2UV = dissolveTex2UV * _DissolveTex2_ST.xy + _DissolveTex2_ST.zw;
                        // 旋转
                        dissolveTex2UV -= float2(0.5f, 0.5f);
                        float dissolveTex2RotateAngle  = _DissolveTex2Rotation * (PI * 2 / 360);
                        dissolveTex2UV = float2(dissolveTex2UV.x * cos(dissolveTex2RotateAngle) - dissolveTex2UV.y * sin(dissolveTex2RotateAngle), 
                                            dissolveTex2UV.x * sin(dissolveTex2RotateAngle) + dissolveTex2UV.y * cos(dissolveTex2RotateAngle));
                        dissolveTex2UV += float2(0.5f, 0.5f);

                        // 应用
                        float dissolveTex2 = SAMPLE_TEXTURE2D(_DissolveTex2, sampler_DissolveTex2, dissolveTex2UV).r;

                        float adjusted = pow(saturate(dissolveTex2), _DissolveTex2Strength); // 非线性增强
                        float mixNoise = lerp(1, adjusted, _DissolveTex2Blend);           // 控制混合程度
                        dissolveNoise *= mixNoise;
                        
                        float edge = dissolveThreshold - dissolveTex2 + _DissolveTex2Offset;
                        edge = saturate(edge * _DissolveTex2Width + 0.5);
                        dissolveThreshold *= edge;

                #endif


                // ==== 定向溶解 =============
                #if _DISSOLVE_DIRECTION_ON

                        
                        float dissolveDirectionAngle = _DissolveDirectionAngle * (PI * 2 / 360); // 转换为弧度
                        float2 dissolveDirection = float2(cos(dissolveDirectionAngle), sin(dissolveDirectionAngle)); // 方向向量

                        // v1.0508：“伪方向溶解” - 基于 UV 偏移
                        // dissolveTexUV += dissolveDirection * dissolveThreshold * _DissolveDirectionWidth;    // 这里的_DissolveDirectionWidth是强度，控制速度

                        // v2.0509：“真方向溶解”
                        float3 positionWS = input.positionWS;
                        float3 direction = dot(positionWS, dissolveDirection);
                        direction = (direction + 1) * 0.5;
                        float DirectionEdge = dissolveThreshold - direction + _DissolveDirectionOffset;   // 偏移 _DissolveDirectionOffset（避免特殊角度覆盖不全，对角线大于边长）
                        DirectionEdge = saturate(DirectionEdge * _DissolveDirectionWidth + 0.5);   // 溶解交接宽度 _DissolveDirectionWidth（遮罩图斜率）
                        dissolveNoise *= (1 - DirectionEdge); 
                        dissolveThreshold *= DirectionEdge;  // 确保只有遮罩部分在溶解，其余部分不溶解

                #endif
                
                    float baseThreshold = (-_DissolveEmissionWidth + (dissolveThreshold - 0.0) * (1.0 - -_DissolveEmissionWidth) / (1.0 - 0.0));
                    float softRange = _DissolveSmooth + 0.0001;

                    // 主溶解区域计算
                    float dissolveRange = baseThreshold + _DissolveEmissionWidth;
                    float dissolveUpper = (-softRange + (dissolveRange - 0.0) * (1.0 - -softRange) / (1.0 - 0.0));
                    dissolveAlpha = smoothstep(dissolveUpper, dissolveUpper + softRange , dissolveNoise);
                    
                    // 边缘发光区域计算
                    float edgeThreshold = baseThreshold;
                    float edgeUpper = (-softRange + (edgeThreshold - 0.0) * (1.0 - -softRange) / (1.0 - 0.0));
                    dissolveEdge = 1.0 - smoothstep(edgeUpper, edgeUpper + softRange, dissolveNoise);

                #endif

                // 子弹显示
                #if _BULLET_ON
                    float HUDu = (mainTexUV.x / _BulletCol) + fmod(_BulletNumber, _BulletCol) / _BulletCol;
                    float HUDv = (mainTexUV.y / _BulletRow) + ((_BulletRow - 1) - floor(_BulletNumber / _BulletCol))/ _BulletRow;
                    float4 BulletCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(HUDu, HUDv));
                    clip(BulletCol.r - 0.05);
                    mainTexColor = BulletCol;
                #endif

                // 相切亮边
                float depthIntersect = 0;
                #if _DEPTH_INTERSECT
                {
                    float2 scrPos = input.scrPos.xy / input.scrPos.w;           // 屏幕空间uv
                    float depth_01 = SampleSceneDepth(scrPos);            
                    float depth_eye = LinearEyeDepth(depth_01, _ZBufferParams); // 线性场景深度
                    float depth_self = input.scrPos.w;                          // 当前片元自身深度
                    float delta = abs(depth_eye - depth_self);                  // 深度差距
                    float x = 1.0 - saturate(delta / _DepthSensitivity);        // 接近系数（相切灵敏度）
                    x = pow(x, 1.0 / max(_DepthIntersectWidth, 0.001));                 // 相切边缘宽度
                    depthIntersect = smoothstep(1 - _DepthIntersectSmooth, 1.0, x);     // 平滑控制边缘
                    depthIntersect = lerp(depthIntersect, 0, _DepthIntersectSmooth);    // 再次平滑
                    //depthIntersect = smoothstep(0, _DepthIntersectWidth, saturate((depth_eye - depth_self) / _DepthSensitivity));
                }
                #endif


                //软粒子
                float softParticleFade = 1;
                #if _SOFT_PARTICLE_ON
                {
                    float2 scrPos = input.scrPos.xy / input.scrPos.w;
                    float depth_01 = SampleSceneDepth(scrPos);
                    float depth_eye = LinearEyeDepth(depth_01, _ZBufferParams);
                    softParticleFade = saturate((10 - _SoftParticleFade) * (depth_eye - input.scrPos.w));
                }
                #endif

                float vertexOffsetAlpha = 1;
                #if _VERTEXOFFSET_ON
                    float2 maskUV = input.VertexOffset.yz;
                    float mask = SAMPLE_TEXTURE2D_LOD(_VertexOffsetMaskTex, sampler_VertexOffsetMaskTex, maskUV, 0).r;
                    vertexOffsetAlpha = lerp(vertexOffsetAlpha, mask, _VertexOffsetMaskRIsAlphaOn);
                #endif

                //粒子系统参数
                float4 particleColor = input.color;
                //粒子颜色 * 主贴图 * 颜色 * 颜色强度
                float3 finalColor = particleColor.xyz * mainTexColor.xyz * _MainColor.xyz * _MainColorIntensity;
                //溶解亮边颜色
                finalColor = lerp(finalColor, _DissolveColor.rgb, _DissolveColor.a * (1 - dissolveAlpha));
                finalColor += dissolveEdge * _DissolveColor.rgb * _DissolveColor.a;
                //相交亮边颜色
                finalColor = lerp(finalColor, _DepthIntersectColor.rgb,
               lerp(depthIntersect, step(0.01, depthIntersect), _DepthIntersectHardEdge));
                //粒子Alpha * 主贴图Alpha * 颜色的Alpha * 遮罩图 1 Alpha * 遮罩图 2 Alpha * 软粒子Alpha * 溶解Alpha
                float finalAlpha = particleColor.a * mainTexColor.a * _MainColor.a
                    * maskAlpha_1 * maskAlpha_2
                    * vertexOffsetAlpha
                    * softParticleFade;

                finalAlpha = lerp(finalAlpha, _DepthIntersectColor.a,
                          lerp(depthIntersect, step(0.01, depthIntersect), _DepthIntersectHardEdge)) * dissolveAlpha;

                Color0 = float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "AdvancedEffectGUI"
}