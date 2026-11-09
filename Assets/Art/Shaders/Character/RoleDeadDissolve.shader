Shader "Universal Render Pipeline/Character/DeadDissolve"
{
    Properties
    {
        [Header(BaseColor ScreenUV)]
        [MainTexture]_BaseMap("基础色贴图(屏幕UV)", 2D) = "white" {}
        [MainColor][HDR]_BaseColor("颜色", Color) = (1, 1, 1, 1)
        _FlowSpeedX("UV流动速度X", Float) = 0
        _FlowSpeedY("UV流动速度Y", Float) = 0

        [Header(UV Distort)]
        _DistortMap("扭曲贴图", 2D) = "gray" {}
        _DistortStrength("扭曲强度", Range(0, 1)) = 0.05
        _DistortSpeedX("扭曲流动速度X", Float) = 0
        _DistortSpeedY("扭曲流动速度Y", Float) = 0

        [Header(Dissolve ModelUV)]
        _DissolveMap("溶解贴图(模型UV)", 2D) = "white" {}
        _DissolveAmount("溶解进度", Range(0, 1)) = 0
        _DissolveEdgeWidth("溶解边缘宽度", Range(0, 1)) = 0.05
        [HDR]_DissolveEdgeColor("溶解边缘颜色", Color) = (1, 1, 1, 0)

        [Header(Render)]
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("剔除", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
        }

        // -------------------------------------------------------------
        // 主渲染 Pass：unlit + 屏幕UV基础色(流动/扭曲) + 溶解
        // -------------------------------------------------------------
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DistortMap);
            SAMPLER(sampler_DistortMap);
            TEXTURE2D(_DissolveMap);
            SAMPLER(sampler_DissolveMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _FlowSpeedX;
                float _FlowSpeedY;
                float _DistortStrength;
                float _DistortSpeedX;
                float _DistortSpeedY;
                float4 _DistortMap_ST;
                float4 _DissolveMap_ST;
                float _DissolveAmount;
                float _DissolveEdgeWidth;
                float4 _DissolveEdgeColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;   // 屏幕空间齐次坐标，片元里做透视除法
                float2 uv : TEXCOORD1;          // 模型UV，供溶解贴图
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.screenPos = ComputeScreenPos(pos.positionCS);
                output.uv = TRANSFORM_TEX(input.uv, _DissolveMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 屏幕UV：0~1，随屏幕坐标变化
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // 基础UV流动：X / Y 独立速度，不再共用一个标量
                float2 flow = float2(_FlowSpeedX, _FlowSpeedY) * _Time.y;

                // 扭曲贴图流动：X / Y 独立速度，不再跟基础 flow 绑在一起
                float2 distortFlow = float2(_DistortSpeedX, _DistortSpeedY) * _Time.y;
                // UV扭曲：用扭曲贴图偏移屏幕UV，同时支持扭曲贴图自身的 tiling/offset
                float2 distortUV = screenUV * _DistortMap_ST.xy + _DistortMap_ST.zw + distortFlow;
                half4 distort = SAMPLE_TEXTURE2D(_DistortMap, sampler_DistortMap, distortUV);
                float2 offset = (distort.rg - 0.5) * 2.0 * _DistortStrength;
                // 基础色：屏幕UV 套上材质面板的 Tiling/Offset，再叠加流动与扭曲
                float2 baseUV = screenUV * _BaseMap_ST.xy + _BaseMap_ST.zw + flow + offset;

                half4 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV);
                half3 rgb = base.rgb * _BaseColor.rgb;

                // 溶解：按模型UV采样溶解贴图，超出进度就裁掉
                half dissolve = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, input.uv).r;
                float t = dissolve - _DissolveAmount;
                clip(t);

                // 溶解边缘发光
                float edge = 1.0 - smoothstep(0.0, max(_DissolveEdgeWidth, 1e-3), t);
                rgb += _DissolveEdgeColor.rgb * edge;

                return half4(rgb, 1.0);
            }
            ENDHLSL
        }

        // -------------------------------------------------------------
        // 阴影 Pass：阴影随溶解一起消失
        // -------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;
            float4 _ShadowBias;

            float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
            {
                float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
                float scale = invNdotL * _ShadowBias.y;
                positionWS = lightDirection * _ShadowBias.xxx + positionWS;
                positionWS = normalWS * scale.xxx + positionWS;
                return positionWS;
            }

            TEXTURE2D(_DissolveMap);
            SAMPLER(sampler_DissolveMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _FlowSpeedX;
                float _FlowSpeedY;
                float _DistortStrength;
                float _DistortSpeedX;
                float _DistortSpeedY;
                float4 _DistortMap_ST;
                float4 _DissolveMap_ST;
                float _DissolveAmount;
                float _DissolveEdgeWidth;
                float4 _DissolveEdgeColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);

                output.uv = TRANSFORM_TEX(input.uv, _DissolveMap);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                half dissolve = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, input.uv).r;
                clip(dissolve - _DissolveAmount);
                return 0;
            }
            ENDHLSL
        }

        // -------------------------------------------------------------
        // 深度 Pass：深度纹理里的角色也随溶解消失
        // -------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_DissolveMap);
            SAMPLER(sampler_DissolveMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _FlowSpeedX;
                float _FlowSpeedY;
                float _DistortStrength;
                float _DistortSpeedX;
                float _DistortSpeedY;
                float4 _DistortMap_ST;
                float4 _DissolveMap_ST;
                float _DissolveAmount;
                float _DissolveEdgeWidth;
                float4 _DissolveEdgeColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _DissolveMap);
                return output;
            }

            half DepthOnlyFragment(Varyings input) : SV_Target
            {
                half dissolve = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, input.uv).r;
                clip(dissolve - _DissolveAmount);
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
}
