Shader "Universal Render Pipeline/NiloToon/Dead_Ghost"
{
    Properties
    {
        _RimColor ("边缘颜色", Color) = (0.5, 0.8, 1.0, 1.0)
        _RimPower ("边缘锐利度", Range(0.5, 8.0)) = 3.0
        _RimIntensity ("边缘强度", Range(0.0, 5.0)) = 2.0

        _BaseColor ("内部颜色", Color) = (0.2, 0.5, 0.8, 0.15)
        _BaseAlpha ("基础透明度", Range(0, 1)) = 0.15

        [Header(Depth Fade)]
        _DepthFadeDistance ("深度淡出距离", Float) = 1.0
        _DepthFadeRange ("深度淡出范围", Float) = 2.0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            // 走 Transparent 队列，但输出的是不透明像素（Blend Off + 输出 alpha 1）。
            //
            // 原理：不做真正的 alpha 混合，而是把"背景色"采样回来，和菲涅尔色 lerp，
            // 当成不透明颜色盖上去 —— 用场景色冒充透明。
            //
            // 好处：
            //   1) 像素是实心写入的，ZWrite On + ZTest LEqual 让 10 个 mesh 按真实深度
            //      互相遮挡，跟物体排序无关，转镜头也不会跳层级；
            //   2) 头发 / 脸 / 面具谁在前面完全由逐像素深度决定；
            //   3) _BaseAlpha = 0 时 lerp 结果就是背景色本身，画面上彻底看不见，
            //      也不会像之前那样把地面深度顶掉、露出清屏色。
            //
            // 背景色来自 _CameraOpaqueTexture（URP 在 opaque 阶段结束后 copy），
            // 本 shader 在 Transparent 队列渲染，所以这张图里不含自己。
            // 需要在 URP Asset 里打开 Opaque Texture。
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "UnlitGhostFresnel"
            Tags { "LightMode" = "UniversalForward" }

            // 实心写入，不做混合
            Blend Off
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;
                half4 _BaseColor;
                half _BaseAlpha;
                float _DepthFadeDistance;
                float _DepthFadeRange;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // --- 菲涅尔 ---
                half3 N = normalize(i.normalWS);
                half3 V = normalize(_WorldSpaceCameraPos.xyz - i.positionWS);
                half NdotV = saturate(dot(N, V));
                half fresnel = pow(1.0 - NdotV, _RimPower) * _RimIntensity;

                // 菲涅尔同时充当"内部 → 边缘"的权重：
                //   0 = 正对相机的内部区域，1 = 边缘区域
                half rimWeight = saturate(fresnel);

                // --- 内部 / 边缘各自用自己的 A 通道 ---
                //   _BaseColor.a 控制内部透明度，_RimColor.a 控制边缘透明度，
                //   _BaseAlpha 是两者的总倍率。
                half innerAlpha = saturate(_BaseAlpha * _BaseColor.a);
                half rimAlpha   = saturate(_BaseAlpha * _RimColor.a);
                half finalAlpha = lerp(innerAlpha, rimAlpha, rimWeight);

                // --- 颜色仍然是叠加式（内部色 + 边缘色 * 菲涅尔）---
                half3 tint = _BaseColor.rgb + _RimColor.rgb * fresnel;

                // --- 取背景色，冒充透明 ---
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                half3 sceneColor = SampleSceneColor(screenUV);

                // 等价于原来的 SrcAlpha OneMinusSrcAlpha，只是换成不透明输出
                half3 finalRGB = lerp(sceneColor, tint, finalAlpha);

                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
}
