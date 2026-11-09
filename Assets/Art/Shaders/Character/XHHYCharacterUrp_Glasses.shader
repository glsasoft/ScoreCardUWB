Shader "Universal Render Pipeline/NiloToon/XHHY_CharacterURP_Glasses"
{
    Properties
    {
        [Header(Glass)]
        [MainTexture]_BaseMap("基础色贴图", 2D) = "white" {}
        [MainColor][HDR]_BaseColor("镜片颜色", Color) = (0.75, 0.85, 0.95, 0.25)

        [Header(Fake Highlight)]
        _HighlightMap("高光贴图", 2D) = "black" {}
        [HDR]_HighlightColor("高光颜色", Color) = (1, 1, 1, 1)
        _HighlightIntensity("高光强度", Range(0, 8)) = 1
        _HighlightSlide("左右滑动", Range(-1, 1)) = 0.25

        [Header(Fresnel)]
        _FresnelMin("侧面消隐", Range(0, 1)) = 0.15
        _FresnelMax("正面保留", Range(0, 1)) = 0.5

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

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
            #include "Assets/Graphics/RenderFeature/AerialFog/AerialFogCommon.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_HighlightMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _HighlightMap_ST;
                float4 _HighlightColor;
                float _HighlightIntensity;
                float _HighlightSlide;
                float _FresnelMin;
                float _FresnelMax;
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
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv.xy = TRANSFORM_TEX(input.uv, _BaseMap);
                output.uv.zw = TRANSFORM_TEX(input.uv, _HighlightMap);

                // 相机在网格空间绕 Y 的左右转角。正前方（局部 +Z）为 0，右侧为正。
                float3 camOS = TransformWorldToObject(GetCameraPositionWS());
                output.uv.w += atan2(camOS.x, camOS.z) * _HighlightSlide;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv.xy);
                half4 hi = SAMPLE_TEXTURE2D(_HighlightMap, sampler_LinearClamp, input.uv.zw);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float facing = saturate(dot(normalWS, viewDirWS));
                // 低于消隐：高光没了。高于保留：高光全在。中间平滑过渡。
                float keep = smoothstep(_FresnelMin, max(_FresnelMax, _FresnelMin + 1e-3), facing);

                float mask = hi.a * keep;
                half3 spec = hi.rgb * _HighlightColor.rgb * (_HighlightIntensity * mask);
                half streak = max(spec.r, max(spec.g, spec.b));
                half3 rgb = baseMap.rgb * _BaseColor.rgb + spec;
                half alpha = saturate(baseMap.a * _BaseColor.a + streak);

                // 透明物画在屏幕空间雾之后，本来整片免疫；这里用自己的世界坐标补一遍，
                // 远的镜片/面罩才能和周围一起变淡。
                rgb = AerialFogApply(rgb, input.positionWS);

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
