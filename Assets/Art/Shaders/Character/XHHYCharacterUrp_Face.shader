Shader "Universal Render Pipeline/NiloToon/XHHY_CharacterURP_Face"
{
    Properties
    {
        [HideInInspector]_group2 ("Enable Pass ModelTransparent", float) = 0

        _CenterPointPosition("模型中心点",Vector) = (1,0,0,0)
        [Main(BaseMapGroup,_,on,off)]_BaseMapGroup("主贴图", Float) = 0
        [Sub(BaseMapGroup)][MainTexture]_BaseMap("主贴图", 2D) = "white" {}
        [Sub(BaseMapGroup)][HDR][MainColor]_BaseColor("基础颜色", Color) = (1,1,1,1)
        [HideInInspector]_ShadowColor("_ShadowColor",Color) = (1,1,1,1)
        [Sub(BaseMapGroup)]_Alpha("透明度",Range(0,1)) = 1
        [Sub(BaseMapGroup)]_MaskMap("遮罩贴图", 2D) = "white" {}
        [GradientRamp(BaseMapGroup)][NoScaleOffset]_RampLightingTex("渐变贴图", 2D) = "white" {}
        [Sub(BaseMapGroup)]_LightMode("渐变行", Float) = 0

        [Main(RenderStateGroup,_,off,off)]_RenderStateGroup("渲染状态", Float) = 0
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.CullMode)]_Cull("剔除", Float) = 2
        [Sub(RenderStateGroup)][HDR]_BackFaceTintColor("背面颜色", Color) = (1,1,1,1)
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.BlendMode)]_SrcBlend("源混合", Float) = 1
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.BlendMode)]_DstBlend("目标混合", Float) = 0
        [SubToggle(RenderStateGroup, _)]_ZWrite("深度写入", Float) = 1
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.CompareFunction)]_ZTest("深度测试", Float) = 4 // 4 is LEqual
        [SubEnum(RenderStateGroup, RGBA, 15, RGB, 14)]_ColorMask("颜色通道", Float) = 15
        [Sub(RenderStateGroup)]_ExtraThickOutlineStencilID ("模板 ID", Range (0, 255)) = 199


        [Main(_DecalGroup,_Decal)]_Decal("贴花", Float) = 0
        [Sub(_DecalGroup)]_DecalMap("贴花贴图", 2D) = "white" {} //
        [HDR][Sub(_DecalGroup)]_DecalColor("贴花颜色", Color) = (1,1,1,1)
        [Sub(_DecalGroup)]_DecalInt("贴花强度",Range(0,1)) = 0


        [Main(_IsFaceGroup,_,off,off)]_IsFace("面部阴影", Float) = 0

        [Title(_IsFaceGroup, Map)]
        [Sub(_IsFaceGroup)]_FaceShadowGradientMap("面部阴影贴图（白=正面光，黑=右侧光）", 2D) = "gray" {}
        // [Channel(_IsFaceGroup)]_FaceShadowGradientMapChannel("_FaceShadowGradientMapChannel (默认 G)", Vector) = (0,1,0,0) // use G as default value because if input is rgb grayscale texture, g is 1 bit better

        [SubToggle(_IsFaceGroup,_)]_FaceShadowNoFollowThrLight("阴影不跟随灯光", Float) = 0
        [Sub(_IsFaceGroup)]_FaceShadowResult("正面阴影", Range(-1,1)) = 1
        [Sub(_IsFaceGroup)]_FaceShadowRool("侧面阴影", Range(-1,1)) = 0.5
        [Sub(_IsFaceGroup)]_ShadowOffset("阴影偏移", Range(-1,1)) = 0.5
        [Sub(_IsFaceGroup)]_FaceShadowSmoothNdotL("阴影平滑度", Range(0, 5)) = 1


        [HideInInspector]_FaceShadowGradientMapUVScaleOffset("阴影贴图缩放偏移", Vector) = (1,1,0,0)

        [HideInInspector]_FaceShadowGradientIntensity("_FaceShadowGradientIntensity (默认 1)", Range(0,1)) = 1

        [Title(_IsFaceGroup, Debug)]
        [SubToggle(_IsFaceGroup, _)]_DebugFaceShadowGradientMap("调试阴影贴图", Float) = 0


        [HideInInspector]_UseRampLightingTex("Ramp Texture (渐变纹理)(Lighting)", Float) = 0
        [HideInInspector]_RampLightingTexSampleUvY("_RampLightingTexSampleUvY (默认 0.5)", Range(0,1)) = 0.5
        [HideInInspector]_UseRampLightingSampleUvYTex("_UseRampLightingSampleUvYTex (默认 Off)", Float) = 0

        [Main(_RENDER_OUTLINE,_,on)]_RenderOutline("描边", Float) = 1
        [SubToggle(_RENDER_OUTLINE,_)]_EnableRendering("启用渲染", Float) = 1
        [HideInInspector]_OutlineCorrection("_OutlineCorrection  (默认 On)", Float) = 1
        [HideInInspector]_UnityCameraDepthTextureWriteOutlineExtrudedPosition("_UnityCameraDepthTextureWriteOutlineExtrudedPosition (Turn it off if you are having 2D rim light white line artifact) (默认 On)", Float) = 1


        [Title(_RENDER_OUTLINE, Color)]
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor1("描边颜色 1（遮罩 1）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor2("描边颜色 2（遮罩 0.75）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor3("描边颜色 3（遮罩 0.5）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor4("描边颜色 4（遮罩 0.25）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor5("描边颜色 5（遮罩 0）", Color) = (0,0,0,0)


        [Title(_RENDER_OUTLINE, Width)]
        [Sub(_RENDER_OUTLINE)]_OutlineWidth("描边宽度", Range(0,32)) = 1
        [Sub(_RENDER_OUTLINE)]_OutlineWidthExtraMultiplier("描边宽度倍率", Range(0,256)) = 1

        [Title(_RENDER_OUTLINE, VertexWidth)]
        [SubToggle(_RENDER_OUTLINE,_)]_UseOutlineWidthMaskFromVertexColor("顶点色控制描边宽度", Float) = 0
        [Channel(_RENDER_OUTLINE)]_OutlineWidthMaskFromVertexColor("描边宽度通道", Vector) = (0,1,0,0)

        [HideInInspector]_OutlineZOffset("_OutlineZOffset (View Space unit push away) (默认 0.0001)", Range(0,1)) = 0.0001
        [HideInInspector]_OutlineZOffsetForFaceArea("_OutlineZOffsetForFaceArea (View Space unit push away) (默认 0.02)", Range(0,1)) = 0.02

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Set by script
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // everything inside your CBUFFER_START(UnityPerMaterial) must exist in this Properties{} section in order to make SRP batcher works
        // even properties that are only set by script and [HideInInspector]!
        // You can try comment out any line here and reimport the shader,
        // doing this will make SRP batching fail, you can check it by viewing .shader in the inspector

        // average shadow mapping
        [HideInInspector]_AverageShadowMapRTSampleIndex("_AverageShadowMapRTSampleIndex", Float) = -1

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

        // extra thick outline
        [HideInInspector]_ExtraThickOutlineWidth("_ExtraThickOutlineWidth", Range(0,100)) = 4
        [HideInInspector]_ExtraThickOutlineViewSpacePosOffset("_ExtraThickOutlineViewSpacePosOffset", Vector) = (0,0,0)
        [HideInInspector]_ExtraThickOutlineColor("_ExtraThickOutlineColor", Color) = (1,1,1,1)
        [HideInInspector]_ExtraThickOutlineZOffset("_ExtraThickOutlineZOffset", Float) = -0.1
        [HideInInspector]_ExtraThickOutlineMaxFinalWidth("_ExtraThickOutlineMaxFinalWidth", Float) = 100

        // face
        [HideInInspector]_FaceForwardDirection("_FaceForwardDirection", Vector) = (0,0,1)
        [HideInInspector]_FaceUpDirection("_FaceUpDirection", Vector) = (0,1,0)
        [HideInInspector]_FixFaceNormalAmount("_FixFaceNormalAmount", Range(0,1)) = 1
        [HideInInspector]_FixFaceNormalUseFlattenOrProxySphereMethod("_FixFaceNormalUseFlattenOrProxySphereMethod", Range(0,1)) = 0

        // per char center
        [HideInInspector]_CharacterBoundCenterPosWS("_CharacterBoundCenterPosWS", Vector) = (0,0,0)

        // EnableDepthTextureRimLightAndShadow
        [HideInInspector]_NiloToonEnableDepthTextureRimLightAndShadow("_NiloToonEnableDepthTextureRimLightAndShadow", Float) = 0

        // 受击闪白（独立一层菲涅尔，跟角色边缘光分开）
        [Main(HitFlashGroup,_,off,off)]_HitFlashGroup("受击闪白", Float) = 0
        [Sub(HitFlashGroup)][HDR]_HitFlashColor("闪白颜色", Color) = (0.2216981,0.9486833,1,1)
        [Sub(HitFlashGroup)]_HitFlashIntensity("闪白强度", Range(0,10)) = 0
        [Sub(HitFlashGroup)]_HitFlashPower("闪白菲涅尔指数", Range(0.1,16)) = 2

        // dissolve
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


        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 8
        [HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0 // 0 = keep, 2 = replace

        //ThickOut

        [HideInInspector]_LightDirFromCameraDir("_LightDirFromCameraDir", Int) = 0

        [HideInInspector]_LightDirFromCameraDirOffSet("_LightDirFromCameraDirOffSet", Vector) = (0,0,0,0)
        [HideInInspector]_LightDir("_LightDir",Vector) = (0,0,0,0)
        [HideInInspector]_TimeStooped("_TimeStooped",int) = 0
          [HideInInspector]_M_CombinedRotation0("_M_CombinedRotation0", Vector) = (0,0,0,0)
          [HideInInspector]_M_CombinedRotation1("_M_CombinedRotation1", Vector) = (0,0,0,0)
         [HideInInspector] _M_CombinedRotation2("_M_CombinedRotation2", Vector) = (0,0,0,0)

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
            "Queue"="Geometry-2"
        }
        LOD 300
        HLSLINCLUDE
        //#pragma exclude_renderers gles gles3 glcore
        #pragma target 4.5
        //  #pragma enable_d3d11_debug_symbols
        #pragma multi_compile _ _NILOTOON_FORCE_MINIMUM_SHADER
        #pragma multi_compile_local _ _NILOTOON_DISSOLVE
        ENDHLSL

        /*Pass//"EyeSpecialEffects"
        {
            Name "EyeSpecialEffects"
            Tags
            {
                "LightMode" = "EyeSpecialEffects"
            }
            //ZClip Off
            // render state
            //    Cull [_Cull]
            ZWrite [_ZWrite]
            //   Blend one one
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]
            //   ZTest off
            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                //   Comp Less // passing depth test = passing stencil
                // Ref 225
                // Comp NotEqual // passing depth test = passing stencil
                Pass [_StencilOp] // replace means write
            }

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            //#pragma exclude_renderers ... 

            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正

            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward2

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_ForwardPass_Face.hlsl"
            ENDHLSL
        }*/


      

       
        Pass//"test"
        {
            Name "Facetest"
          

            // render state
            Cull [_Cull]
            ZWrite [_ZWrite]
            //Blend one one
          //  Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]
            ZTest [_ZTest]
         


            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            //#pragma exclude_renderers ... // no need to exclude renderers, all platforms will run the same Pass
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING
           // #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma shader_feature_local _DECAL
            //   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //   #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma multi_compile_local _ _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
            //   #pragma multi_compile_local_fragment _ _NILOTOON_DITHER_FADEOUT
            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_ForwardPass_Face.hlsl"
            ENDHLSL
        }


        Pass //"Outline"
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "NiloToonOutline"
            }

            // render state
            // TODO: we should find a way to flip Cull from the first pass, instead of hardcode "Cull Front"
            Cull Front
            ColorMask [_ColorMask]
            ZTest Less
            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                Comp [_StencilComp]
                Pass [_StencilOp]
            }
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            // #pragma enable_d3d11_debug_symbols
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            //   #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            //  #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #pragma vertex VertexShaderOutLine
            #pragma fragment FragmentShaderOutLineNoLight

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_OutlineForwardPass_Face.hlsl"
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
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma vertex VertexShaderShadowCaster
            #pragma fragment BaseColorAlphaClipTest

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_ShadowCasterPass_Face.hlsl"
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
            #pragma shader_feature_local _ALPHATEST_ON
            //     #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_DepthOnlyOrDepthNormalPass_Face.hlsl"
            ENDHLSL
        }


        /*
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
    //        #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest_AndDepthNormalColorOutput 

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_DepthOnlyOrDepthNormalPass_Face.hlsl"
            ENDHLSL
        }
*/

        //        Pass //"NiloToonSelfShadowCaster"
        //        {
        //            Name "NiloToonSelfShadowCaster"
        //            Tags
        //            {
        //                "LightMode" = "NiloToonSelfShadowCaster"
        //            }
        //
        //            ZWrite On 
        //            ZTest LEqual            
        //            ColorMask 0 
        //            Cull [_Cull]
        //
        //            HLSLPROGRAM
        //            #pragma target 4.5
        //            //  #pragma enable_d3d11_debug_symbols
        //            #pragma vertex VertexShaderSelfShadowCaster
        //            #pragma fragment BaseColorAlphaClipTest 
        //
        //            #include "Face\XHHYCharacter_Input_Face.hlsl"
        //            #include "Face\XHHYCharacter_SelfShadowCasterPass_Face.hlsl"
        //            ENDHLSL
        //        }

        /*Pass // "NiloToonPrepassBuffer"
        {
            Name "NiloToonPrepassBuffer"
            Tags
            {
                "LightMode" = "NiloToonPrepassBuffer"
            }

            ZWrite On 
            ZTest LEqual           
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            //    #pragma enable_d3d11_debug_symbols
            #pragma vertex VertexShaderPrepassBuffer
            #pragma fragment BaseColorAlphaClipTest_AndNiloToonPrepassBufferColorOutput 

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_PrepassBufferPass_Face.hlsl"
            ENDHLSL
        }*/
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
            "Queue"="Geometry-2"
        }
        LOD 200
        HLSLINCLUDE
        //#pragma exclude_renderers gles gles3 glcore
        #pragma target 4.5
        //  #pragma enable_d3d11_debug_symbols
        ENDHLSL

        /*Pass//"EyeSpecialEffects"
        {
            Name "EyeSpecialEffects"
            Tags
            {
                "LightMode" = "EyeSpecialEffects"
            }
            //ZClip Off
            // render state
            //    Cull [_Cull]
            ZWrite [_ZWrite]
            //   Blend one one
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]
            //   ZTest off
            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                //   Comp Less // passing depth test = passing stencil
                // Ref 225
                // Comp NotEqual // passing depth test = passing stencil
                Pass [_StencilOp] // replace means write
            }

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            //#pragma exclude_renderers ... 

            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正

            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward2

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_ForwardPass_Face.hlsl"
            ENDHLSL
        }*/


        Pass
        {
            Name "UniversalForwardLodAlpha"
            Tags
            {
                "LightMode"="UniversalForwardLodAlpha"
            }

            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "Face\XHHYCharacter_Input_Face.hlsl"

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


        /*Pass//"UniversalForward"
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "Facetest"
            }

            // render state
            Cull [_Cull]
            ZWrite [_ZWrite]
            //Blend one one
            Blend [_SrcBlend] [_DstBlend]
            ColorMask 0
            //ZTest [_ZTest]
            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                Comp [_StencilComp] // passing depth test = passing stencil
                // Ref 225
                // Comp NotEqual // passing depth test = passing stencil
                Pass [_StencilOp] // replace means write
            }


            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            //#pragma exclude_renderers ... // no need to exclude renderers, all platforms will run the same Pass

            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING

            //  #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //  #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正

            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward3

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_ForwardPass_Face.hlsl"
            ENDHLSL
        }*/


        Pass//"test"
        {
            Name "Facetest"
            Tags
            {
                "LightMode" = "UniversalForwardLod"
            }

            // render state
            Cull [_Cull]
            ZWrite [_ZWrite]
            //Blend one one
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]
            // ZTest [_ZTest]
            Stencil
            {
                Ref 199

                Comp [_StencilComp] // passing depth test = passing stencil


                // Ref 225
                // Comp NotEqual // passing depth test = passing stencil

                Pass Replace // replace means write
            }


            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            //#pragma exclude_renderers ... // no need to exclude renderers, all platforms will run the same Pass
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING
            #pragma shader_feature_local _DECAL
            //   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //   #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma multi_compile_local _ _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
            //   #pragma multi_compile_local_fragment _ _NILOTOON_DITHER_FADEOUT

            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward
            
            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_ForwardPass_Face.hlsl"
            ENDHLSL
        }


        Pass //"Outline"
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "NiloToonOutline"
            }

            // render state
            // TODO: we should find a way to flip Cull from the first pass, instead of hardcode "Cull Front"
            Cull Front
            ColorMask [_ColorMask]
            ZTest Less
            Stencil
            {
                Ref [_ExtraThickOutlineStencilID]
                Comp [_StencilComp]
                Pass [_StencilOp]
            }
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            // #pragma enable_d3d11_debug_symbols
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT
            //   #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            //  #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #pragma vertex VertexShaderOutLine
            #pragma fragment FragmentShaderOutLineNoLight

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_OutlineForwardPass_Face.hlsl"
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
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma vertex VertexShaderShadowCaster
            #pragma fragment BaseColorAlphaClipTest

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_ShadowCasterPass_Face.hlsl"
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
            #pragma shader_feature_local _ALPHATEST_ON
            //     #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_DepthOnlyOrDepthNormalPass_Face.hlsl"
            ENDHLSL
        }


        /*
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
    //        #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest_AndDepthNormalColorOutput 

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_DepthOnlyOrDepthNormalPass_Face.hlsl"
            ENDHLSL
        }
*/

        //        Pass //"NiloToonSelfShadowCaster"
        //        {
        //            Name "NiloToonSelfShadowCaster"
        //            Tags
        //            {
        //                "LightMode" = "NiloToonSelfShadowCaster"
        //            }
        //
        //            ZWrite On 
        //            ZTest LEqual            
        //            ColorMask 0 
        //            Cull [_Cull]
        //
        //            HLSLPROGRAM
        //            #pragma target 4.5
        //            //  #pragma enable_d3d11_debug_symbols
        //            #pragma vertex VertexShaderSelfShadowCaster
        //            #pragma fragment BaseColorAlphaClipTest 
        //
        //            #include "Face\XHHYCharacter_Input_Face.hlsl"
        //            #include "Face\XHHYCharacter_SelfShadowCasterPass_Face.hlsl"
        //            ENDHLSL
        //        }

        /*Pass // "NiloToonPrepassBuffer"
        {
            Name "NiloToonPrepassBuffer"
            Tags
            {
                "LightMode" = "NiloToonPrepassBuffer"
            }

            ZWrite On 
            ZTest LEqual           
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            //    #pragma enable_d3d11_debug_symbols
            #pragma vertex VertexShaderPrepassBuffer
            #pragma fragment BaseColorAlphaClipTest_AndNiloToonPrepassBufferColorOutput 

            #include "Face\XHHYCharacter_Input_Face.hlsl"
            #include "Face\XHHYCharacter_PrepassBufferPass_Face.hlsl"
            ENDHLSL
        }*/
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI" // TODO: check if this line is still required, because removing this line, LWGUI will still work
}