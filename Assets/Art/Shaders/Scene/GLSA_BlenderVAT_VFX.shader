Shader "GLSA/Blender VAT VFX"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2

        [Header(VAT)]
        [NoScaleOffset] _PositionTex("VAT Position (EXR)", 2D) = "black" {}
        [NoScaleOffset] _NormalTex("VAT Normal (EXR)", 2D) = "bump" {}
        _VATPlaySpeed("Play Speed (loops/sec)", Float) = 0.5

        [Header(Fog)]
        [Toggle(_AERIALFOG_ON)] _UseAerialFog("AerialFog (Skip URP Fog)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_PositionTex);
        SAMPLER(sampler_PositionTex);
        TEXTURE2D(_NormalTex);
        SAMPLER(sampler_NormalTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Metallic;
            half _Smoothness;
            float _VATPlaySpeed;
            float _UseAerialFog;
            float4 _PositionTex_TexelSize;
        CBUFFER_END

        float3 BlenderToUnity(float3 value)
        {
            // Blender FBX preset: Bake Space Transform, Forward -Z, Up Y.
            // Unity's FBX import also changes handedness, hence the negative X.
            return float3(-value.x, value.z, -value.y);
        }

        // VFX Graph 给每个粒子一份完整的对象矩阵，所以这里直接输出对象空间的 VAT 位置，
        // 粒子的位置、旋转、size over lifetime 由矩阵统一作用，不需要再和网格顶点拼。
        void SampleVAT(float vertexRow, float progress, out float3 positionOS, out float3 normalOS)
        {
            float frameCount = max(_PositionTex_TexelSize.z, 1.0);
            float framePosition = saturate(progress) * (frameCount - 1.0);
            float currentFrame = floor(framePosition);
            float nextFrame = min(currentFrame + 1.0, frameCount - 1.0);
            float frameBlend = frac(framePosition);
            float2 currentUV = float2((currentFrame + 0.5) / frameCount, vertexRow);
            float2 nextUV = float2((nextFrame + 0.5) / frameCount, vertexRow);

            float3 currentPosition = SAMPLE_TEXTURE2D_LOD(_PositionTex, sampler_PositionTex, currentUV, 0).xyz;
            float3 nextPosition = SAMPLE_TEXTURE2D_LOD(_PositionTex, sampler_PositionTex, nextUV, 0).xyz;
            float3 currentNormal = SAMPLE_TEXTURE2D_LOD(_NormalTex, sampler_NormalTex, currentUV, 0).xyz;
            float3 nextNormal = SAMPLE_TEXTURE2D_LOD(_NormalTex, sampler_NormalTex, nextUV, 0).xyz;

            positionOS = BlenderToUnity(lerp(currentPosition, nextPosition, frameBlend));
            normalOS = normalize(BlenderToUnity(lerp(currentNormal, nextNormal, frameBlend)));
        }
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex VATVertex
            #pragma fragment VATFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma shader_feature_local_fragment _AERIALFOG_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Graphics/RenderFeature/AerialFog/AerialFogCommon.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 vatUV : TEXCOORD1;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                half3 vertexLighting : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings VATVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float progress = frac(_Time.y * _VATPlaySpeed + input.color.r);
                float3 positionOS;
                float3 normalOS;
                SampleVAT(input.vatUV.y, progress, positionOS, normalOS);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.vertexLighting = VertexLighting(positionInputs.positionWS, output.normalWS);
                return output;
            }

            half4 VATFragment(
                Varyings input,
                FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC
            ) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half faceSign = IS_FRONT_VFACE(isFrontFace, 1.0h, -1.0h);
                half3 normalWS = NormalizeNormalPerPixel(input.normalWS) * faceSign;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseSample.rgb * _BaseColor.rgb;
                surfaceData.alpha = baseSample.a * _BaseColor.a;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0, 0, 0);
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.emission = half3(0, 0, 0);
                surfaceData.occlusion = 1;
                clip(surfaceData.alpha - 1e-4h);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = input.vertexLighting;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                #if defined(_AERIALFOG_ON)
                    color.rgb = AerialFogApply(color.rgb, input.positionWS);
                #else
                    color.rgb = MixFog(color.rgb, input.fogFactor);
                #endif

                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
