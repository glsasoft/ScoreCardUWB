Shader "GLSA/UI/HealthBarBillboard"
{
    Properties
    {
        [MainTexture] _BaseMap("血条贴图 (可选)", 2D) = "white" {}
        [MainColor] _BaseColor("血条前景色", Color) = (0.15, 0.85, 0.25, 1)
        _BgColor("血条背景色", Color) = (0.1, 0.1, 0.1, 0.75)
        _FillAmount("当前血量比例", Range(0, 1)) = 1.0
        [Toggle] _VerticalBillboard("锁定Y轴(仅水平旋转)", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "DisableBatching" = "True" // 防止静态合批破坏本地原点
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // 声明 CBUFFER 以兼容 URP 的 SRP Batcher
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _BgColor;
                half _FillAmount;
                half _VerticalBillboard;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 1. 获取模型原点在观察空间(View Space)的位置
                float3 centerVS = TransformWorldToView(TransformObjectToWorld(float3(0, 0, 0)));

                // 2. 提取模型的世界缩放（包含父物体缩放）
                float3 scale = float3(
                    length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                    length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                    length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z))
                );

                float3 offsetOS = input.positionOS.xyz * scale;

                if (_VerticalBillboard > 0.5)
                {
                    // 模式B：仅水平朝向相机（适合角色上下俯视时不倾斜）
                    float3 worldCenter = TransformObjectToWorld(float3(0, 0, 0));
                    float3 forward = normalize(_WorldSpaceCameraPos - worldCenter);
                    forward.y = 0;
                    forward = normalize(forward + float3(0, 0, 0.0001));
                    float3 up = float3(0, 1, 0);
                    float3 right = cross(up, forward);

                    float3 worldPos = worldCenter + right * offsetOS.x + up * offsetOS.y;
                    output.positionCS = TransformWorldToHClip(worldPos);
                }
                else
                {
                    // 模式A：完全面向相机（与粒子系统默认 Billboard 行为一致）
                    // 注意：观察空间下，面向相机的 Quad 顶点只需在 XY 平面直接叠加偏移
                    float3 posVS = centerVS + float3(offsetOS.x, offsetOS.y, 0);
                    output.positionCS = TransformWViewToHClip(posVS);
                }

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // 根据 UV.x 裁剪血量前景与背景
                half mask = step(input.uv.x, _FillAmount);
                half4 finalColor = lerp(_BgColor, _BaseColor, mask) * texColor;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
