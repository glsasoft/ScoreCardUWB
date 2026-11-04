Shader "Universal Render Pipeline/NiloToon/XHHY_CharacterURP_EyeShadow"
{
    Properties
    {


        [Header(Enable Rendering)]
        _CenterPointPosition("_CenterPointPosition",Vector) = (1,0,0,0)

        [Header(BaseMap and Color)]
        [HDR]_BaseColor("BaseColor",Color) = (0,0,0,0.5)
        _Alpha("Alpha",Range(0,1)) = 1



        _ExtraThickOutlineStencilID ("StencilID", Range (0, 255)) = 199
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0 // 0 = keep, 2 = replace



        // dither fadeout
        [HideInInspector]_DitherFadeoutAmount("_DitherFadeoutAmount", Range(0,1)) = 0.5

        // dissolve
        [HideInInspector]_DissolveAmount("_DissolveAmount", Range(0,1)) = 0
        [HideInInspector]_DissolveThresholdMap("_DissolveThresholdMap", 2D) = "linearGrey" {}
        [HideInInspector]_DissolveBorderRange("_DissolveBorderRange", Range(0,1)) = 0.02
        [HideInInspector]_DissolveBorderTintColor("_DissolveBorderTintColor", Color) = (0,4,4,1)
        [HideInInspector]_DissolveTopBorderRange("_DissolveTopBorderRange", Float) = 1
        [HideInInspector]_DissolveTopBorderTintColor("_DissolveTopBorderTintColor", Color) = (1,1,1)

        // perspective removal
        [HideInInspector]_PerspectiveRemovalAmount("_PerspectiveRemovalAmount", Range(0,1)) = 0
        [HideInInspector]_PerspectiveRemovalRadius("_PerspectiveRemovalRadius", Float) = 1
        [HideInInspector]_HeadBonePositionWS("_HeadBonePositionWS", Vector) = (0,0,0)
        [HideInInspector]_PerspectiveRemovalStartHeight("_PerspectiveRemovalStartHeight", Float) = 0 // ground
        [HideInInspector]_PerspectiveRemovalEndHeight("_PerspectiveRemovalEndHeight", Float) = 1 // a point above ground and below character head

        [HideInInspector]_PerCharEffectLerpColor("_PerCharEffectLerpColor", Color) = (1,1,0,0)

        [HideInInspector]_PerCharEffectTintColor("_PerCharEffectTintColor", Color) = (1,1,1)
        [HideInInspector]_PerCharEffectAddColor("_PerCharEffectAddColor", Color) = (0,0,0)






    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "ShaderModel"="4.5"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
            "Queue"="Geometry-3"
        }

        HLSLINCLUDE
        #pragma target 4.5
        ENDHLSL


        Pass//"test"
        {
            Name "Eyetest"
            Tags
            {
                "LightMode" = "EyeShadow"
            }


            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                Comp [_StencilComp]
                // Ref 225
                // Comp NotEqual // passing depth test = passing stencil
                Pass [_StencilOp] // replace means write
            }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT

            //  #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma multi_compile_local _ _NILOTOON_DISSOLVE
            #pragma vertex VertexShaderForward_EyeShadow
            #pragma fragment FragmentShaderForward_EyeShadow

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      //      #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Custom/Common.hlsl"

            CBUFFER_START(UnityPerMaterial)
                // perspective removal
                float4 _BaseColor;
                float _Alpha;
                float _PerspectiveRemovalAmount; // total amount
                // perspective removal(sphere)
                float _PerspectiveRemovalRadius;
                float3 _HeadBonePositionWS;
                // perspective removal(world height)
                float _PerspectiveRemovalStartHeight; // usually is world space pos.y 0
                float _PerspectiveRemovalEndHeight;
                float3 _CenterPointPosition;
                // _NILOTOON_DISSOLVE
                float _DissolveAmount;
                float _DissolveSource;
                float _DissolveBorderRange;
                half4 _DissolveBorderTintColor;
                float _DissolveTopBorderRange;
                half4 _DissolveTopBorderTintColor;
                float _DitherFadeoutAmount;

                float4 _PerCharEffectLerpColor;

                float4 _PerCharEffectTintColor;
                float4 _PerCharEffectAddColor;
                // float test;
            CBUFFER_END

            TEXTURE2D(_DissolveThresholdMap);

            SAMPLER(sampler_DissolveThresholdMap);

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; //用于轮廓宽度掩码，而不是作为一种颜色，所以使用浮动而不是半数
                // SV_VertexID需要是uint（https://docs.unity3d.com/Manual/SL-ShaderSemantics.html）。
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float2 uv : TEXCOORD0;
                //需要float而不是一半的uv，如果纹理尺寸大于2048，一半是不够的（https://forum.unity.com/threads/why-does-unity-recommend-us-to-use-float-type-for-texture-coordinates-in-the-shader.732920/#post-4894619）。
                float3 positionWS : TEXCOORD1; // xyz = positionWS, w = ZOffsetFinalSum


                half4 color : COLOR;
                // vertex color rgba, currently only for "control depth texture rimlight and shadow width by vertex color" or "debug shading"
            };


            float4 NiloDoPerspectiveRemoval(float4 originalPositionCS, float3 positionWS,
                                                    float3 removalCenterPositionWS, float removalRadius,
                                                    float removalAmount, float removalStartHeight,
                                                    float removalEndHeight)
            {
                /*// only do perspective removal if is perspective camera
                // high level function contain global disable logic, to reduce code complexity of this .hlsl's user code
                if(_GlobalShouldDisableNiloToonPerspectiveRemoval || unity_OrthoParams.w == 1)
                    return originalPositionCS;
            
                float perspectiveRemovalAreaSphere = saturate(removalRadius - distance(positionWS,removalCenterPositionWS) / removalRadius);
                float perspectiveRemovalAreaWorldHeight = saturate(invLerp(removalStartHeight, removalEndHeight, positionWS.y));
                float perspectiveRemovalFinalAmount = removalAmount * perspectiveRemovalAreaSphere * perspectiveRemovalAreaWorldHeight;
                float centerPosVSz = mul(UNITY_MATRIX_V, float4(removalCenterPositionWS,1)).z;
            
                return NiloDoPerspectiveRemoval(originalPositionCS, perspectiveRemovalFinalAmount, centerPosVSz);*/


                float3 CenterPointPositionVS = TransformWorldToView(_CenterPointPosition);
                float3 positionVS = TransformWorldToView(positionWS);
                positionVS.z -= CenterPointPositionVS.z;
                positionVS.z *= max(0.05, 1 - removalAmount);
                positionVS.z += CenterPointPositionVS.z;
                return TransformWViewToHClip(positionVS);
            }

            Varyings VertexShaderForward_EyeShadow(Attributes input)
            {
                Varyings output = (Varyings)0;


                float3 positionWS = TransformObjectToWorld(input.positionOS);

                // TRANSFORM_TEX与旧版着色器库相同。
                output.uv = input.uv;


                output.positionWS = positionWS;


                //     output.positionCS = vertexPositionInput.positionCS; // TransformWorldToHClip(positionWS);
                output.color = input.color;

                // 消除透视相机的失真
                output.positionCS = NiloDoPerspectiveRemoval(output.positionCS, positionWS, _HeadBonePositionWS,
                                                                              _PerspectiveRemovalRadius,
                                                                              _PerspectiveRemovalAmount,
                                                                              _PerspectiveRemovalStartHeight,
                                                                              _PerspectiveRemovalEndHeight);

                #if _PERSPECTIVEREMOVAL
                    output.positionCS = ApplyPerspectiveRemoval(positionWS,_CenterPointPosition);
                #endif
                // output.positionCS = TransformObjectToHClip(input.positionOS);
                return output;
            }


            void ApplyDissolve(inout half3 color, Varyings input)
            {
                #if _NILOTOON_DISSOLVE
                // read black and white map
                half dissolveMapThresholdMapValue = SAMPLE_TEXTURE2D(_DissolveThresholdMap,
       sampler_DissolveThresholdMap, input.uv).g;
                dissolveMapThresholdMapValue = _DissolveSource == 0 ? dissolveMapThresholdMapValue : input.color.z;
                half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;

                // clip threshold
                clip(dissolve);
                float dissolveBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveBorderRange, _DissolveAmount,
                                                  dissolveMapThresholdMapValue);
                dissolveBorderTintColorMask = lerp(0, dissolveBorderTintColorMask, _DissolveBorderTintColor.a);

                float dissolveTopBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveTopBorderRange,
                _DissolveAmount,
                dissolveMapThresholdMapValue);
                dissolveBorderTintColorMask = lerp(0, dissolveBorderTintColorMask, _DissolveTopBorderTintColor.a);
                //对 "近阈值区域 "进行HDR色彩调剂 
                color += dissolveBorderTintColorMask * _DissolveBorderTintColor.rgb;
                color += dissolveTopBorderTintColorMask * _DissolveTopBorderTintColor.rgb;
                #endif
            }

            void NiloDoDitherFadeoutClip(float2 SV_POSITIONxy, float ditherOpacity)
            {
                // copy from https://docs.unity3d.com/Packages/com.unity.shadergraph@10.3/manual/Dither-Node.html?q=dither
                float DITHER_THRESHOLDS[16] =
                {
                    1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
                };
                uint index = (uint(SV_POSITIONxy.x) % 4) * 4 + uint(SV_POSITIONxy.y) % 4;
                clip(ditherOpacity - DITHER_THRESHOLDS[index]);
            }

            half4 FragmentShaderForward_EyeShadow(Varyings input, half facing : VFACE) : SV_TARGET
            {
                half3 color = _BaseColor.rgb;
                #if _NILOTOON_DITHER_FADEOUT
                NiloDoDitherFadeoutClip(input.positionCS.xy, 1 - _DitherFadeoutAmount);
                #endif
                ApplyDissolve(color, input);
                color.rgb = color.rgb * _PerCharEffectTintColor.rgb + _PerCharEffectAddColor.rgb;
                color.rgb = lerp(color.rgb, _PerCharEffectLerpColor.rgb, _PerCharEffectLerpColor.a);
            //    color = MyFog(color, input.positionWS.xyz);
                return half4(color, _BaseColor.a * _Alpha);
            }
            ENDHLSL
        }



        // NiloToonSelfShadowCaster pass. Used for rendering NiloToon system's character self shadow map (not related to URP's shadow map system)
        //        Pass //"NiloToonSelfShadowCaster"
        //        {
        //            Name "NiloToonSelfShadowCaster"
        //            Tags
        //            {
        //                "LightMode" = "NiloToonSelfShadowCaster"
        //            }
        //
        //            // more explicit render state to avoid confusion
        //            ZWrite On
        //            ZTest LEqual
        //            ColorMask 0
        //            Cull [_Cull]
        //
        //            HLSLPROGRAM
        //            #pragma target 4.5
        //            //  #pragma enable_d3d11_debug_symbols
        //            #pragma vertex VertexShaderSelfShadowCaster
        //            #pragma fragment BaseColorAlphaClipTest // we only need to do Clip(), no need color shading
        //
        //            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
        //            #include "Eye\XHHYCharacter_SelfShadowCasterPass_Eye.hlsl"
        //            ENDHLSL
        //        }

        //        // [This pass is used when drawing to _NiloToonPrepassBufferTex texture]
        //        Pass // "NiloToonPrepassBuffer"
        //        {
        //            Name "NiloToonPrepassBuffer"
        //            Tags
        //            {
        //                "LightMode" = "NiloToonPrepassBuffer"
        //            }
        //
        //            // more explicit render state to avoid confusion
        //            ZWrite On
        //            ZTest LEqual
        //            Cull [_Cull]
        //
        //            HLSLPROGRAM
        //            #pragma target 4.5
        //            //    #pragma enable_d3d11_debug_symbols
        //
        //            #pragma vertex VertexShaderPrepassBuffer
        //            #pragma fragment BaseColorAlphaClipTest_AndNiloToonPrepassBufferColorOutput // we need to do Clip(), and output normal as color
        //
        //
        //
        //            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
        //            #include "Eye\XHHYCharacter_PrepassBufferPass_Eye.hlsl"
        //            ENDHLSL
        //        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI" // TODO: check if this line is still required, because removing this line, LWGUI will still work
}