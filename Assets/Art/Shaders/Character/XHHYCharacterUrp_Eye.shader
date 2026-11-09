Shader "Universal Render Pipeline/NiloToon/XHHY_CharacterURP_Eye"
{
    Properties
    {
        _CenterPointPosition("模型中心点",Vector) = (1,0,0,0)

        [Main(BaseMapGroup,_,on,off)]_BaseMapGroup("主贴图", Float) = 0
        [Sub(BaseMapGroup)][MainTexture]_BaseMap("主贴图", 2D) = "white" {}
        [Sub(BaseMapGroup)][HDR][MainColor]_BaseColor("基础颜色", Color) = (1,1,1,1)
        [Sub(BaseMapGroup)]_Alpha("透明度",Range(0,1)) = 1

        [Main(RenderStateGroup,_,off,off)]_RenderStateGroup("渲染状态", Float) = 0
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.CullMode)]_Cull("剔除", Float) = 2
        [HideInInspector][HDR]_BackFaceTintColor("背面颜色", Color) = (1,1,1,1)
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.BlendMode)]_SrcBlend("源混合", Float) = 1
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.BlendMode)]_DstBlend("目标混合", Float) = 0
        [SubToggle(RenderStateGroup, _)]_ZWrite("深度写入", Float) = 1
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.CompareFunction)]_ZTest("深度测试", Float) = 4 // 4 is LEqual
        [SubEnum(RenderStateGroup, RGBA, 15, RGB, 14)]_ColorMask("颜色通道", Float) = 15
        [Sub(RenderStateGroup)]_ExtraThickOutlineStencilID ("模板 ID", Range (0, 255)) = 199

        // 方向光染色：让角色跟随主方向光的颜色，和空气透视雾统一氛围。
        // 用 keyword 开关，关掉时整段代码被剥离，不产生任何指令。
        [Main(SunTintGroup,_,off,off)]_SunTintGroup("方向光染色", Float) = 0
        [SubToggle(SunTintGroup, _SUNTINT)]_SunTintEnable("受方向光影响", Float) = 0
        [Sub(SunTintGroup)]_SunTintAmount("影响程度", Range(0,1)) = 0.5

        [Main(g2,_,off,on)]
        [PassSwitch(ModelTransparent)]
        _group2 ("半透明", float) = 0

        [HideInInspector]_EnableRendering("_EnableRendering(默认启用) ", Float) = 1

        // face
        [HideInInspector]_FaceForwardDirection("_FaceForwardDirection", Vector) = (0,0,1)
        [HideInInspector]_FaceUpDirection("_FaceUpDirection", Vector) = (0,1,0)
        [HideInInspector]_FixFaceNormalAmount("_FixFaceNormalAmount", Range(0,1)) = 1
        [HideInInspector]_FixFaceNormalUseFlattenOrProxySphereMethod("_FixFaceNormalUseFlattenOrProxySphereMethod", Range(0,1)) = 0

        // per char center
        [HideInInspector]_CharacterBoundCenterPosWS("_CharacterBoundCenterPosWS", Vector) = (0,0,0)

        // 受击闪白（独立一层菲涅尔，跟角色边缘光分开）
        [Main(HitFlashGroup,_,off,off)]_HitFlashGroup("受击闪白", Float) = 0
        [Sub(HitFlashGroup)][HDR]_HitFlashColor("闪白颜色", Color) = (0.2216981,0.9486833,1,1)
        [Sub(HitFlashGroup)]_HitFlashIntensity("闪白强度", Range(0,10)) = 0
        [Sub(HitFlashGroup)]_HitFlashPower("闪白菲涅尔指数", Range(0.1,16)) = 2

        [Main(DissolveGroup,_,off,off)]_DissolveGroup("溶解", Float) = 0
        [SubToggle(DissolveGroup, _NILOTOON_DISSOLVE)]_EnableDissolve("启用溶解", Float) = 0
        [Sub(DissolveGroup)]_DissolveAmount("溶解程度", Range(0,1)) = 0
        [Sub(DissolveGroup)]_DissolveMinY("溶解高度下限 (物体Y)", Float) = 0
        [Sub(DissolveGroup)]_DissolveMaxY("溶解高度上限 (物体Y)", Float) = 1.515
        [Sub(DissolveGroup)]_DissolveRootY("溶解基准世界Y", Float) = 0
        [HideInInspector]_DissolveThresholdMap("溶解阈值贴图", 2D) = "linearGrey" {}
        [Sub(DissolveGroup)]_DissolveBorderRange("溶解边缘范围", Range(0.01,1)) = 0.04
        [Sub(DissolveGroup)][HDR]_DissolveBorderTintColor("溶解边缘颜色", Color) = (0,4,4,1)
        [Sub(DissolveGroup)]_DissolveTopBorderRange("溶解顶部范围", Range(0.01,1)) = 0.02
        [Sub(DissolveGroup)][HDR]_DissolveTopBorderTintColor("溶解顶部颜色", Color) = (1,1,1,0)
        [Sub(DissolveGroup)]_DissolveEdgeDitherStrength("溶解边缘网点强度", Range(0,1)) = 0
        [SubToggle(DissolveGroup, _NILOTOON_DITHER_FADEOUT)]_EnableDitherFadeout("启用全局网点", Float) = 0
        [Sub(DissolveGroup)]_DitherFadeoutAmount("全局网点淡出", Range(0,1)) = 0

        [Main(PerspectiveGroup,_,off,off)]_PerspectiveGroup("透视消除", Float) = 0
        [Sub(PerspectiveGroup)]_PerspectiveRemovalAmount("强度", Range(0,1)) = 0
        [HideInInspector]_PerspectiveRemovalRadius("_PerspectiveRemovalRadius", Float) = 1
        [HideInInspector]_HeadBonePositionWS("_HeadBonePositionWS", Vector) = (0,0,0)
        [HideInInspector]_PerspectiveRemovalStartHeight("_PerspectiveRemovalStartHeight", Float) = 0 // ground
        [HideInInspector]_PerspectiveRemovalEndHeight("_PerspectiveRemovalEndHeight", Float) = 1 // a point above ground and below character head

          // per character gameplay effect
        [HideInInspector]_PerCharEffectTintColor("_PerCharEffectTintColor", Color) = (1,1,1)
        [HideInInspector]_PerCharEffectAddColor("_PerCharEffectAddColor", Color) = (0,0,0)
        [HideInInspector]_PerCharEffectDesaturatePercentage("_PerCharEffectDesaturatePercentage", Range(0,1)) = 0
        [HideInInspector]_PerCharEffectLerpColor("_PerCharEffectLerpColor", Color) = (1,1,0,0)
        [HideInInspector]_PerCharEffectRimColor("_PerCharEffectRimColor", Color) = (0,0,0)
        [HideInInspector]_PerCharEffectRimSharpnessPower("_PerCharEffectRimSharpnessPower", Float) = 4
        [HideInInspector]_PerCharacterOutlineColorLerp("_PerCharacterOutlineColorLerp", Color) = (1,1,1,0)
        
          // per character set up
        [HideInInspector]_PerCharacterBaseColorTint("_PerCharacterBaseColorTint", Color) = (1,1,1)
        [HideInInspector]_PerCharacterOutlineWidthMultiply("_PerCharacterOutlineWidthMultiply", Float) = 1
        [HideInInspector]_PerCharacterOutlineColorTint("_PerCharacterOutlineColorTint", Color) = (1,1,1)
        
        // BaseMap Override
        [HideInInspector]_PerCharacterBaseMapOverrideAmount("_PerCharacterBaseMapOverrideAmount", Range(0,1)) = 0
        [HideInInspector]_PerCharacterBaseMapOverrideMap("_PerCharacterBaseMapOverrideMap", 2D) = "White" {}
        [HideInInspector]_PerCharacterBaseMapOverrideTiling("_PerCharacterBaseMapOverrideTiling", Float) = 1
        [HideInInspector]_PerCharacterBaseMapOverrideTintColor("_PerCharacterBaseMapOverrideTintColor", Color) = (1,1,1)

        
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 8
        [HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0 // 0 = keep, 2 = replace

        //ThickOut

        [HideInInspector]_TimeStooped("_TimeStooped",int) = 0

        [Main(g1,_,off,off)]
        _group ("头发", float) = 0
        [Sub(g1)]_HairAlpha("头发透明度",Range(0,1)) = 0.85
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "ShaderModel"="4.5"
            // explicit SubShader tag to avoid confusion
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
            "Queue"="Geometry-3"
        }

        HLSLINCLUDE
        //#pragma exclude_renderers gles gles3 glcore
        #pragma target 4.5
        //  #pragma enable_d3d11_debug_symbols
        #pragma shader_feature_local _ALPHATEST_ON
        #pragma multi_compile _ _NILOTOON_FORCE_MINIMUM_SHADER
        #pragma multi_compile_local _ _NILOTOON_DISSOLVE
        ENDHLSL

//        Pass//"EyeSpecialEffects"
//        {
//            Name "EyeSpecialEffects"
//            Tags
//            {
//                "LightMode" = "EyeSpecialEffects"
//            }
//            //ZClip Off
//            // render state
//            //    Cull [_Cull]
//            ZWrite [_ZWrite]
//            //   Blend one one
//            Blend SrcAlpha OneMinusSrcAlpha
//            ColorMask [_ColorMask]
//            //   ZTest off
//            Stencil
//            {
//                Ref [_ExtraThickOutlineStencilID]
//                //   Comp Less // passing depth test = passing stencil
//                // Ref 225
//                // Comp NotEqual // passing depth test = passing stencil
//                Pass [_StencilOp] // replace means write
//            }
//
//            HLSLPROGRAM
//            //#pragma exclude_renderers gles gles3 glcore
//            #pragma target 4.5
//            //#pragma enable_d3d11_debug_symbols
//            //#pragma exclude_renderers ... // no need to exclude renderers, all platforms will run the same Pass
//
//            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
//            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING
//
//            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
//            #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
//
//            #pragma vertex VertexShaderForward
//            #pragma fragment FragmentShaderForward2
//
//            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
//            #include "Eye\XHHYCharacter_ForwardPass_Eye.hlsl"
//            ENDHLSL
//        }


        Pass
        {
            Name "ModelTransparent"
            Tags
            {
                "LightMode"="ModelTransparent"
            }

            // https://docs.unity3d.com/Manual/SL-Stencil.html
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "Eye\XHHYCharacter_Input_Eye.hlsl"

            struct Varyings2
            {
                float4 positionCS : SV_POSITION;
            };


            Varyings2 Vertex(Attributes input)
            {
                Varyings2 output = (Varyings2)0;

                float3 positionWS = TransformObjectToWorld(input.positionOS);
                float3 positionVS = TransformWorldToView(positionWS);
                output.positionCS = TransformWViewToHClip(positionVS);

                return output;
            };

            half4 Fragment(Varyings2 input, half facing : VFACE) : SV_TARGET
            {
                return 1;
            }
            ENDHLSL
        }


        Pass//"test"
        {
            Name "Eyetest"
            Tags
            {
            //    "LightMode" = "ModelRenderer"
            }

            // render state
            Cull [_Cull]
            ZWrite On
            //Blend one one
            //Blend SrcAlpha OneMinusSrcAlpha   // 眼睛改成不透明，关掉 alpha 混合
            ColorMask [_ColorMask]
            ZTest [_ZTest]
            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                Comp [_StencilComp]
                // Ref 225
                // Comp NotEqual // passing depth test = passing stencil
                Pass [_StencilOp] // replace means write
            }

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //#pragma enable_d3d11_debug_symbols
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING
#pragma multi_compile_local _ _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
            #pragma shader_feature_local _SUNTINT
         //   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //(can use _fragment suffix)
       //     #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正

            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward

            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
            #include "Eye\XHHYCharacter_ForwardPass_Eye.hlsl"
            ENDHLSL
        }


        Pass//"UniversalForward"
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "Eyetest"
            }

            // render state
            //  Cull [_Cull]
            ZWrite On
            //Blend one one
            Blend [_SrcBlend] [_DstBlend]
            ColorMask 0
            //  ZTest [_ZTest]
            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                Comp [_StencilComp]
                // Ref 225
                // Comp NotEqual // passing depth test = passing stencil
                Pass [_StencilOp] // replace means write
            }

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //#pragma enable_d3d11_debug_symbols
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING

         //   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //(can use _fragment suffix)
          //  #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正

            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward3

            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
            #include "Eye\XHHYCharacter_ForwardPass_Eye.hlsl"
            ENDHLSL
        }





        Pass //"ShadowCaster"
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT //(can use _fragment suffix)

            #pragma vertex VertexShaderShadowCaster
            #pragma fragment BaseColorAlphaClipTest

            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
            #include "Eye\XHHYCharacter_ShadowCasterPass_Eye.hlsl"
            ENDHLSL
        }

        Pass //"DepthOnly"
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // more explicit render state to avoid confusion
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            //   #pragma enable_d3d11_debug_symbols

         //   #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest // we only need to do Clip(), no need color shading

            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
            #include "Eye\XHHYCharacter_DepthOnlyOrDepthNormalPass_Eye.hlsl"
            ENDHLSL
        }


        Pass //"DepthNormals"
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols

           // #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest_AndDepthNormalColorOutput // we need to do Clip(), and output normal as color



            #include "Eye\XHHYCharacter_Input_Eye.hlsl"
            #include "Eye\XHHYCharacter_DepthOnlyOrDepthNormalPass_Eye.hlsl"
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