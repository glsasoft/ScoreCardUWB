Shader "GLSA/Blender VAT URP Lit"
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
        [Toggle(_VAT_POSITION_OFFSET)] _VATPositionIsOffset("Position Map Is Offset", Float) = 0
        _VATOffsetScale("Offset Scale (match startSize)", Float) = 1
        _VATProgress("VAT Progress (Material)", Range(0, 1)) = 0

        [Header(VAT Particle Drive)]
        [Toggle(_VAT_PARTICLE_PROGRESS_ON)] _VATParticleProgress("Progress From Particle", Float) = 0
        [Enum(Material, 0, Custom1X, 1, VertexColorR, 2, AutoPlay, 3)] _VATProgressSource("Progress Source", Float) = 3
        _VATPlaySpeed("Auto Play Speed (loops/sec)", Float) = 1

        [Header(Soft Dissolve)]
        _DissolveMap("Dissolve Map", 2D) = "white" {}
        _DissolveProgress("Dissolve Progress", Range(0, 1)) = 0
        _DissolveSoftness("Dissolve Softness", Range(0.001, 1)) = 0.1
        [HDR] _DissolveEdgeColor("Dissolve Edge Color", Color) = (1, 0.5, 0, 1)
        _DissolveEdgeWidth("Dissolve Edge Width", Range(0.001, 0.5)) = 0.05

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
        TEXTURE2D(_DissolveMap);
        SAMPLER(sampler_DissolveMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _DissolveMap_ST;
            half4 _BaseColor;
            half4 _DissolveEdgeColor;
            half _Metallic;
            half _Smoothness;
            float _VATProgress;
            float _VATParticleProgress;
            float _VATProgressSource;
            float _VATPlaySpeed;
            float _VATPositionIsOffset;
            float _VATOffsetScale;
            float _UseAerialFog;
            half _DissolveProgress;
            half _DissolveSoftness;
            half _DissolveEdgeWidth;
            float4 _PositionTex_TexelSize;
        CBUFFER_END

        half GetDissolveThreshold()
        {
            return lerp(
                -max(_DissolveSoftness, 1e-4h),
                1.0h,
                saturate(_DissolveProgress)
            );
        }

        // Mesh 粒子的顶点布局：TEXCOORD0/1 是网格自己的 uv0/uv1（uv1 存 VAT 顶点行号），
        // 声明宽度必须和实际 float 数一致，写成 float4 会跨格读到隔壁属性。
        // Custom1 在实测中没有进入顶点缓冲，每粒子的进度改走顶点色，见 _VATProgressSource。
        float ResolveVATProgress(float custom1X, float vertexColorR)
        {
            #if defined(_VAT_PARTICLE_PROGRESS_ON)
                int source = (int)round(_VATProgressSource);
                if (source == 1) return saturate(custom1X);
                if (source == 2) return saturate(vertexColorR);
                if (source == 3)
                {
                    // 时间驱动，顶点色 R 当每粒子的相位偏移，鸟群就不会齐步走。
                    return frac(_Time.y * _VATPlaySpeed + vertexColorR);
                }
                return saturate(_VATProgress);
            #else
                return saturate(_VATProgress);
            #endif
        }

        void GetVATUVs(
            float vertexRow,
            float progress,
            out float2 currentUV,
            out float2 nextUV,
            out float frameBlend
        )
        {
            float frameCount = max(_PositionTex_TexelSize.z, 1.0);
            float framePosition = progress * (frameCount - 1.0);
            float currentFrame = floor(framePosition);
            float nextFrame = min(currentFrame + 1.0, frameCount - 1.0);
            frameBlend = frac(framePosition);
            currentUV = float2((currentFrame + 0.5) / frameCount, vertexRow);
            nextUV = float2((nextFrame + 0.5) / frameCount, vertexRow);
        }

        float3 BlenderToUnity(float3 value)
        {
            // Blender FBX preset: Bake Space Transform, Forward -Z, Up Y.
            // Unity's FBX import also changes handedness, hence the negative X.
            return float3(-value.x, value.z, -value.y);
        }

        void SampleVAT(
            float3 vertexPositionOS,
            float vertexRow,
            float progress,
            out float3 positionOS,
            out float3 normalOS
        )
        {
            float2 currentUV;
            float2 nextUV;
            float frameBlend;
            GetVATUVs(vertexRow, progress, currentUV, nextUV, frameBlend);

            float3 currentPosition = SAMPLE_TEXTURE2D_LOD(
                _PositionTex, sampler_PositionTex, currentUV, 0
            ).xyz;
            float3 nextPosition = SAMPLE_TEXTURE2D_LOD(
                _PositionTex, sampler_PositionTex, nextUV, 0
            ).xyz;
            float3 currentNormal = SAMPLE_TEXTURE2D_LOD(
                _NormalTex, sampler_NormalTex, currentUV, 0
            ).xyz;
            float3 nextNormal = SAMPLE_TEXTURE2D_LOD(
                _NormalTex, sampler_NormalTex, nextUV, 0
            ).xyz;

            float3 animatedPosition = BlenderToUnity(
                lerp(currentPosition, nextPosition, frameBlend)
            );

            // 第 0 帧烘的就是网格静止姿势。用「顶点位置 + 当前帧偏移」而不是直接拿贴图位置，
            // 顶点上带的变换（Mesh 粒子的位置/旋转/缩放）才会继续生效。
            #if defined(_VAT_POSITION_OFFSET)
                // 偏移图：贴图里存的就是「相对静止网格的偏移」，直接叠到顶点上，
                // 不会把 bind pose 和网格顶点的差带进形状里。
                // Mesh 粒子拿不到每粒子缩放矩阵（supportsMeshInstancing=False），
                // 偏移量靠 _VATOffsetScale 手动对齐 startSize。
                positionOS = vertexPositionOS + animatedPosition * _VATOffsetScale;
            #else
                float2 bindUV = float2(0.5 * _PositionTex_TexelSize.x, vertexRow);
                float3 bindPosition = BlenderToUnity(SAMPLE_TEXTURE2D_LOD(
                    _PositionTex, sampler_PositionTex, bindUV, 0
                ).xyz);
                positionOS = vertexPositionOS + (animatedPosition - bindPosition);
            #endif
            normalOS = normalize(BlenderToUnity(
                lerp(currentNormal, nextNormal, frameBlend)
            ));
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
            #pragma vertex VATLitVertex
            #pragma fragment VATLitFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma shader_feature_local _VAT_PARTICLE_PROGRESS_ON
            #pragma shader_feature_local _VAT_POSITION_OFFSET
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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 vatUV : TEXCOORD1;
                float4 customData : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                half3 vertexLighting : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings VATLitVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float progress = ResolveVATProgress(input.customData.x, input.color.r);

                float3 positionOS;
                float3 normalOS;
                SampleVAT(input.positionOS.xyz, input.vatUV.y, progress, positionOS, normalOS);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(normalOS);
                output.uv.xy = TRANSFORM_TEX(input.uv, _BaseMap);
                output.uv.zw = TRANSFORM_TEX(input.uv, _DissolveMap);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.vertexLighting = VertexLighting(positionInputs.positionWS, output.normalWS);
                return output;
            }

            half4 VATLitFragment(
                Varyings input,
                FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC
            ) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseSample = SAMPLE_TEXTURE2D(
                    _BaseMap, sampler_BaseMap, input.uv.xy
                );
                half dissolveNoise = SAMPLE_TEXTURE2D(
                    _DissolveMap, sampler_DissolveMap, input.uv.zw
                ).r;
                half softness = max(_DissolveSoftness, 1e-4h);
                half threshold = GetDissolveThreshold();
                half dissolveAlpha = smoothstep(
                    threshold, threshold + softness, dissolveNoise
                );
                half edgeCenter = threshold + softness * 0.5h;
                half edge = 1.0h - smoothstep(
                    0.0h,
                    max(_DissolveEdgeWidth, 1e-4h),
                    abs(dissolveNoise - edgeCenter)
                );
                edge *= step(1e-4h, _DissolveProgress);
                edge *= step(_DissolveProgress, 0.9999h);

                half faceSign = IS_FRONT_VFACE(isFrontFace, 1.0h, -1.0h);
                half3 normalWS = NormalizeNormalPerPixel(input.normalWS) * faceSign;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseSample.rgb * _BaseColor.rgb;
                surfaceData.alpha = (
                    baseSample.a * _BaseColor.a * dissolveAlpha
                );
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0, 0, 0);
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.emission = _DissolveEdgeColor.rgb * edge;
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

                // Transparent 队列画在 AerialFog(450) 之后，屏幕空间那层扫不到这里，
                // 所以用物体自身世界坐标补同一份雾；和内置雾二选一，不叠双份。
                #if defined(_AERIALFOG_ON)
                    color.rgb = AerialFogApply(color.rgb, input.positionWS);
                #else
                    color.rgb = MixFog(color.rgb, input.fogFactor);
                #endif

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex VATShadowVertex
            #pragma fragment VATShadowFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _VAT_PARTICLE_PROGRESS_ON
            #pragma shader_feature_local _VAT_POSITION_OFFSET
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 vatUV : TEXCOORD1;
                float4 customData : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ShadowVaryings VATShadowVertex(ShadowAttributes input)
            {
                ShadowVaryings output = (ShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float progress = ResolveVATProgress(input.customData.x, input.color.r);

                float3 positionOS;
                float3 normalOS;
                SampleVAT(input.positionOS.xyz, input.vatUV.y, progress, positionOS, normalOS);

                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(
                    ApplyShadowBias(positionWS, normalWS, lightDirectionWS)
                );
                output.uv = TRANSFORM_TEX(input.uv, _DissolveMap);
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(
                        output.positionCS.z,
                        UNITY_NEAR_CLIP_VALUE
                    );
                #else
                    output.positionCS.z = max(
                        output.positionCS.z,
                        UNITY_NEAR_CLIP_VALUE
                    );
                #endif
                return output;
            }

            half4 VATShadowFragment(ShadowVaryings input) : SV_Target
            {
                half dissolveNoise = SAMPLE_TEXTURE2D(
                    _DissolveMap, sampler_DissolveMap, input.uv
                ).r;
                clip(dissolveNoise - GetDissolveThreshold() - 1e-5h);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex VATDepthVertex
            #pragma fragment VATDepthFragment
            #pragma multi_compile_instancing
            #pragma shader_feature_local _VAT_PARTICLE_PROGRESS_ON
            #pragma shader_feature_local _VAT_POSITION_OFFSET

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 vatUV : TEXCOORD1;
                float4 customData : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthVaryings VATDepthVertex(DepthAttributes input)
            {
                DepthVaryings output = (DepthVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float progress = ResolveVATProgress(input.customData.x, input.color.r);

                float3 positionOS;
                float3 normalOS;
                SampleVAT(input.positionOS.xyz, input.vatUV.y, progress, positionOS, normalOS);
                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _DissolveMap);
                return output;
            }

            half4 VATDepthFragment(DepthVaryings input) : SV_Target
            {
                half dissolveNoise = SAMPLE_TEXTURE2D(
                    _DissolveMap, sampler_DissolveMap, input.uv
                ).r;
                clip(dissolveNoise - GetDissolveThreshold() - 1e-5h);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
