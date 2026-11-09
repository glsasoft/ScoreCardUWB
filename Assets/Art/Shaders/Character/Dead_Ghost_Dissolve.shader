// 合并 Dead_Ghost.shader 与 RoleDeadDissolve.shader，两种外观由「溶解进度」在同一件模型上分区呈现：
//
//   未溶解的部分（溶解贴图值 > _DissolveAmount）→ Dead 外观
//       主色 = 屏幕 UV 采样 _BaseMap × _DeadColor，吃流动和扭曲，实心输出，投影
//   已溶解的部分（溶解贴图值 <= _DissolveAmount）→ Ghost 外观
//       主色 = _GhostColor，菲涅尔叠边缘色，采样背景色 lerp 冒充透明，不投影
//
// _DissolveAmount 从 0 走到 1，就是「整件尸体慢慢腐蚀成灵魂」的过程。
// 扭曲 / 溶解 / 菲涅尔各自独立开关，互不干扰。两种外观都走 Blend Off + ZWrite On，
// 靠真实深度互相遮挡，与物体排序无关。
Shader "Universal Render Pipeline/Character/Dead_Ghost_Dissolve"
{
    Properties
    {
        [Header(Base Color)]
        [MainTexture]_BaseMap("主颜色贴图（Dead 部分，屏幕 UV 采样）", 2D) = "white" {}
        [MainColor][HDR]_DeadColor("Dead 主颜色", Color) = (1, 1, 1, 1)
        _GhostColor("Ghost 内部颜色 (A = 透明度)", Color) = (1, 1, 1, 1)
        _BaseAlpha("Ghost 整体不透明度", Range(0, 1)) = 1
        _FlowSpeedX("Dead 主色 UV 流动 X", Float) = 0
        _FlowSpeedY("Dead 主色 UV 流动 Y", Float) = 0

        [Header(UV Distort)]
        [Toggle]_DistortEnabled("开启扭曲", Float) = 0
        _DistortMap("扭曲贴图", 2D) = "gray" {}
        _DistortStrength("扭曲强度", Range(0, 1)) = 0.05
        _DistortSpeedX("扭曲流动速度 X", Float) = 0
        _DistortSpeedY("扭曲流动速度 Y", Float) = 0

        [Header(Dissolve)]
        [Toggle]_DissolveEnabled("开启溶解贴图（关掉时按进度整体切换）", Float) = 0
        _DissolveMap("溶解贴图（模型 UV）", 2D) = "white" {}
        _DissolveAmount("溶解进度（0 = 全是 Dead，1 = 全是 Ghost）", Range(0, 1)) = 0
        _DissolveEdgeWidth("溶解边缘宽度", Range(0, 1)) = 0.05
        [HDR]_DissolveEdgeColor("溶解边缘颜色", Color) = (1, 1, 1, 0)

        [Header(Fresnel)]
        [Toggle]_FresnelEnabled("开启菲涅尔", Float) = 1
        _RimColor("边缘颜色 (A = Ghost 边缘透明度)", Color) = (0.5, 0.8, 1.0, 1.0)
        _RimPower("边缘锐利度", Range(0.5, 8.0)) = 3.0
        _RimIntensity("边缘强度", Range(0.0, 5.0)) = 2.0

        [Header(Depth Fade)]
        _DepthFadeDistance("深度淡出距离", Float) = 1.0
        _DepthFadeRange("深度淡出范围", Float) = 2.0

        [Header(Render)]
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("剔除", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
        // 透明物在 450 那层后处理雾之后才画，吃不到雾；这里自己补一次，参数和雾同一份。
        #include "Assets/Graphics/RenderFeature/AerialFog/AerialFogCommon.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_DistortMap);
        SAMPLER(sampler_DistortMap);
        TEXTURE2D(_DissolveMap);
        SAMPLER(sampler_DissolveMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _DeadColor;
            float4 _GhostColor;
            float _BaseAlpha;
            float _FlowSpeedX;
            float _FlowSpeedY;

            float _DistortEnabled;
            float4 _DistortMap_ST;
            float _DistortStrength;
            float _DistortSpeedX;
            float _DistortSpeedY;

            float _DissolveEnabled;
            float4 _DissolveMap_ST;
            float _DissolveAmount;
            float _DissolveEdgeWidth;
            float4 _DissolveEdgeColor;

            float _FresnelEnabled;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            float _DepthFadeDistance;
            float _DepthFadeRange;
            float _Cull;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float4 screenPos : TEXCOORD2;
            float2 uv : TEXCOORD3;
        };

        Varyings vert(Attributes v)
        {
            Varyings o;
            VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
            o.positionCS = posInputs.positionCS;
            o.positionWS = posInputs.positionWS;
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.screenPos = ComputeScreenPos(o.positionCS);
            o.uv = v.uv;
            return o;
        }

        // ---- 菲涅尔：只读世界法线 ----
        half CalcFresnel(half3 normalWS, half3 positionWS)
        {
            if (_FresnelEnabled < 0.5)
            {
                return 0.0;
            }

            half3 N = normalize(normalWS);
            half3 V = normalize(_WorldSpaceCameraPos.xyz - positionWS);
            half NdotV = saturate(dot(N, V));
            return pow(1.0 - NdotV, _RimPower) * _RimIntensity;
        }

        // ---- 溶解边界：>0 = 未溶解（Dead），<=0 = 已溶解（Ghost） ----
        float CalcDissolveThreshold(Varyings i)
        {
            if (_DissolveEnabled < 0.5)
            {
                return 1.0 - _DissolveAmount;
            }

            half dissolveMask = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, TRANSFORM_TEX(i.uv, _DissolveMap)).r;
            return dissolveMask - _DissolveAmount;
        }

        // ---- Dead 外观：屏幕 UV 采样 _BaseMap，叠流动和扭曲 ----
        half3 SampleDeadColor(Varyings i, float2 screenUV)
        {
            float2 flow = float2(_FlowSpeedX, _FlowSpeedY) * _Time.y;

            float2 distortOffset = 0.0;
            if (_DistortEnabled > 0.5)
            {
                float2 distortFlow = float2(_DistortSpeedX, _DistortSpeedY) * _Time.y;
                float2 distortUV = screenUV * _DistortMap_ST.xy + _DistortMap_ST.zw + distortFlow;
                half4 distort = SAMPLE_TEXTURE2D(_DistortMap, sampler_DistortMap, distortUV);
                distortOffset = (distort.rg - 0.5) * 2.0 * _DistortStrength;
            }

            float2 baseUV = screenUV * _BaseMap_ST.xy + _BaseMap_ST.zw + flow + distortOffset;
            return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV).rgb * _DeadColor.rgb;
        }
        ENDHLSL

        Pass
        {
            Name "UnlitGhostDissolve"
            Tags { "LightMode" = "UniversalForward" }

            Blend Off
            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            half4 frag(Varyings i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                half fresnel = CalcFresnel(i.normalWS, i.positionWS);
                half rimWeight = saturate(fresnel);

                float threshold = CalcDissolveThreshold(i);

                // 溶解边缘：两侧都描一圈
                float edgeWidth = max(_DissolveEdgeWidth, 1e-3);
                float edge = 1.0 - smoothstep(0.0, edgeWidth, abs(threshold));

                half3 finalRGB;

                if (threshold >= 0.0)
                {
                    // ---- 未溶解：Dead，实心输出，不叠菲涅尔 ----
                    half3 deadRGB = SampleDeadColor(i, screenUV);
                    deadRGB += _DissolveEdgeColor.rgb * edge;
                    finalRGB = deadRGB;
                }
                else
                {
                    // ---- 已溶解：Ghost，拿背景色 lerp 冒充透明 ----
                    half3 ghostRGB = _GhostColor.rgb + _RimColor.rgb * fresnel;
                    ghostRGB += _DissolveEdgeColor.rgb * edge;

                    half innerAlpha = saturate(_BaseAlpha * _GhostColor.a);
                    half rimAlpha = saturate(_BaseAlpha * _RimColor.a);
                    half finalAlpha = lerp(innerAlpha, rimAlpha, rimWeight);

                    half3 sceneColor = SampleSceneColor(screenUV);
                    finalRGB = lerp(sceneColor, ghostRGB, finalAlpha);
                }

                // 自己吃一次雾：这个材质在 Transparent 队列，后处理雾（450）画在它之前，
                // 不补这一步它永远不跟远景一起淡下去。Ghost 部分拿的是 _CameraOpaqueTexture
                // 里的背景色，那张图在 AfterOpaques 拷的、不含雾，所以这里补完是对的。
                finalRGB = AerialFogApply(finalRGB, i.positionWS);

                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }

        // -------------------------------------------------------------
        // 阴影：只有未溶解的 Dead 部分投影，Ghost 部分整块丢弃
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
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #pragma target 2.0

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

            float4 GetShadowPositionHClip(float3 positionWS, float3 normalWS)
            {
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

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            ShadowVaryings shadowVert(Attributes v)
            {
                ShadowVaryings o;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = GetShadowPositionHClip(positionWS, normalWS);
                o.uv = v.uv;
                return o;
            }

            half4 shadowFrag(ShadowVaryings i) : SV_Target
            {
                float threshold;
                if (_DissolveEnabled < 0.5)
                {
                    threshold = 1.0 - _DissolveAmount;
                }
                else
                {
                    half dissolveMask = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, TRANSFORM_TEX(i.uv, _DissolveMap)).r;
                    threshold = dissolveMask - _DissolveAmount;
                }

                // 已溶解的 Ghost 部分不投影
                if (threshold < 0.0)
                {
                    clip(-1);
                }

                return 0;
            }
            ENDHLSL
        }

        // -------------------------------------------------------------
        // 深度：同样只写未溶解的 Dead 部分
        // -------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex depthVert
            #pragma fragment depthFrag
            #pragma target 2.0

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            DepthVaryings depthVert(Attributes v)
            {
                DepthVaryings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 depthFrag(DepthVaryings i) : SV_Target
            {
                float threshold;
                if (_DissolveEnabled < 0.5)
                {
                    threshold = 1.0 - _DissolveAmount;
                }
                else
                {
                    half dissolveMask = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, TRANSFORM_TEX(i.uv, _DissolveMap)).r;
                    threshold = dissolveMask - _DissolveAmount;
                }

                if (threshold < 0.0)
                {
                    clip(-1);
                }

                return i.positionCS.z;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
