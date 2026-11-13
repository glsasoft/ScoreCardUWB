Shader "Piu/Lit_Piu_TEST"
{
    Properties
    {

        [Main(Preset, _, off, off)] _PresetGroup ("表面选项", float) = 0
        [Preset(Preset,XHHY_BlendModePreset)] _BlendMode2 ("混合模式预设", float) = 0
        [SubEnum(Preset, UnityEngine.Rendering.CullMode)] _Cull ("剔除模式", Float) = 1
        [SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _SrcBlend ("源混合", Float) = 1
        [SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _DstBlend ("目标混合", Float) = 0
        [SubToggle(Preset)] _ZWrite ("深度写入", Float) = 1
        [SubEnum(Preset, UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试", Float) = 4 // 4 is LEqual
        [SubEnum(Preset, RGBA, 15, RGB, 14)] _ColorMask ("颜色遮罩", Float) = 15 // 15 is RGBA (binary 1111)
         [SubToggle(Preset,_RECEIVE_SHADOWS_OFF)] _RECEIVE_SHADOWS_OFF ("关闭阴影", Float) = 0
        //  [SubToggle(Preset,_ALPHATEST_ON)] _ALPHATEST_ON ("_AlphaTest ", Float) = 0
        [Sub(Preset)] _Cutoff("透明度裁剪", Range(0.0, 1.0)) = 0.5

        [Main(SurfaceInputs,_,off,off)] _SurfaceInputs("表面输入", float) = 0

        [Tex(SurfaceInputs,_BaseColor)][NoScaleOffset][MainTexture] _BaseMap("基础贴图 (Albedo)", 2D) = "white" {}
        [Sub(SurfaceInputs)][HideInInspector][HDR] _BaseColor("基础颜色", Color) = (1,1,1,1)
        [Sub(SurfaceInputs)]_ScaleOffset("UV 缩放偏移",Vector) = (1,1,0,0)

        //MSO
        [SubTitle(SurfaceInputs,Metal Smooth AO)]
        [Tex(SurfaceInputs)][NoScaleOffset] _MetallicGlossMap("金属度贴图", 2D) = "white" {}
        [Sub(SurfaceInputs)] _Metallic("金属度", Range(0.0, 1.0)) = 1
        [Sub(SurfaceInputs)] _Metallic_Rain("金属度 (雨天)", Range(0.0, 1.0)) = 1
        [Sub(SurfaceInputs)] _Smoothness("光滑度", Range(0.0, 1.0)) = 1
        [Sub(SurfaceInputs)] _Smoothness_Rain("光滑度 (雨天)", Range(0.0, 1.0)) = 1
        [Sub(SurfaceInputs)] _OcclusionStrength("环境光遮蔽 AO", Range(0.0, 1.0)) = 1.0
        [Sub(SurfaceInputs)] _OcclusionStrength_Rain("环境光遮蔽 AO (雨天)", Range(0.0, 1.0)) = 1


        //法线
        [SubTitle(SurfaceInputs,Normal)]
        [Tex(SurfaceInputs)][NoScaleOffset] _BumpMap("法线贴图", 2D) = "bump" {}
        [Sub(SurfaceInputs)] _BumpScale("法线强度", Range(0,5)) = 1.0


        //自发光
        [SubTitle(SurfaceInputs,Emission)]
        [SubToggle(SurfaceInputs, _EMISSION)]_EmissionToggle("开启自发光",float) = 0
        [Tex(SurfaceInputs,_EmissionColor)] _EmissionMap("自发光贴图", 2D) = "white" {}
        [Sub(SurfaceInputs)][HideInInspector] [HDR] _EmissionColor("自发光颜色", Color) = (0,0,0)


        [Main(Eff,_,off,off)] _Eff ("特殊效果", float) = 0
        //透明
        [SubTitle(Eff,Dither Fade)]
        [SubToggle(Eff,_NILOTOON_DITHER_FADEOUT)]_Dither("开启抖动淡出", Float) = 0
        [Sub(Eff)] _DitherFadeoutAmount("淡出程度", Range(0,1)) = 0

        // 溶解
        [SubTitle(Eff,Dissolve)]
        [SubToggle(Eff,_NILOTOON_DISSOLVE)]_NtloToonDissolve("开启溶解", Float) = 0
        [Sub(Eff)] _DissolveAmount("溶解进度", Range(-1,1)) = 0
        [Tex(Eff)] _DissolveThresholdMap("溶解阈值贴图", 2D) = "linearGrey" {}
        [SubToggle(Eff, _)] _DissolveByVertexColor("按顶点色溶解 (默认关闭)", Float) = 0
        [Sub(Eff)] _DissolveBorderRange("边缘宽度", Range(0.001,1)) = 0.02
        [Sub(Eff)] [HDR]_DissolveBorderTintColor("边缘颜色", Color) = (0,4,4,1)
        [Sub(Eff)] _DissolveTopBorderRange("顶部边缘宽度", Range(0.001,1)) = 0.02
        [Sub(Eff)] [HDR]_DissolveTopBorderTintColor("顶部边缘颜色", Color) = (0,4,4,1)

        //特殊效果
        [SubTitle(Eff,Per Character Color)]
        [Sub(Eff)] _PerCharEffectTintColor("整体染色", Color) = (1,1,1)
        [Sub(Eff)] _PerCharEffectAddColor("叠加颜色", Color) = (0,0,0)
        [Sub(Eff)] _PerCharEffectDesaturatePercentage("去色程度", Range(0,1)) = 0
        [Sub(Eff)] _PerCharEffectLerpColor("插值颜色", Color) = (1,1,0,0)
        [Sub(Eff)] _PerCharacterRimLightIntensity("边光强度", Range(0,100)) = 0
        [Sub(Eff)] _PerCharEffectRimColor("边光颜色", Color) = (0,0,0)
        [Sub(Eff)] _PerCharEffectRimSharpnessPower("边光锐度", Float) = 4

        
       [Main(Outline,_,off,on)][PassSwitch(NiloToonOutline)] _Outline ("描边", float) = 0
 
        [Sub(Outline)] _OutlineColor("描边颜色", Color) = (0,0,0)
         [Sub(Outline)]_OutlineWidth("描边宽度（默认 1）", Range(0,32)) = 1
         [Sub(Outline)]_OutlineWidthExtraMultiplier("描边宽度额外倍增（默认 1）", Range(0,256)) = 1

 //[Sub(OTHERWORLD)]_GIColor_OtherWorld("GIColor", Color) = (1,1,1,1)
        //  [Main(yuyuyu, _, off, off)] _PresetGroup("yuyuyuyu", float) = 0
        // [Sub(yuyuyu)] _BlendMode("Blend Mode Preset", float) = 0
        //[SubEnum(Preset, UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        //[SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        //[SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
        //[SubToggle(Preset), _)] _ZWrite ("ZWrite ", Float) = 1
        //[SubEnum(Preset, UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4 // 4 is LEqual
        //	[SubEnum(Preset, RGBA, 15, RGB, 14)] _ColorMask ("ColorMask", Float) = 15 // 15 is RGBA (binary 1111)





    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
            "Queue" = "Geometry"
        }
        LOD 300

        //
        Pass
        {

            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Stencil
            {
                Ref 1
                Comp [_StencilComp]
                //  Pass Replace
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull Off
            AlphaToMask[_AlphaToMask]


            HLSLPROGRAM
            #pragma target 2.0
            // -------------------------------------
            // Shader Stages
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            // -------------------------------------
            // Material Keywords
           // #pragma shader_feature_local _RECEIVE_SHADOWS_OFF   //关闭或开启接收阴影。
            #pragma shader_feature_local_fragment _ALPHATEST_ON     //alpha测试
            #pragma shader_feature_local_fragment _EMISSION     //是否开启自发光
            // 声明 _ENVIRONMENTREFLECTIONS_OFF 关键字：未定义时 GlobalIllumination.hlsl 走环境反射分支
            // 本 Shader 没有对应的 [ToggleOff] 属性，所以该关键字永远不启用 = 环境反射常开
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
           // #pragma multi_compile_fragment _ _LIGHT_COOKIES
          //  #define _REFLECTION_PROBE_BOX_PROJECTION
            // #pragma multi_compile _ _FORWARD_PLUS

              // Unity defined keywords
          //  #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
          //  #pragma multi_compile _ SHADOWS_SHADOWMASK
          //  #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
         //   #pragma multi_compile _ DYNAMICLIGHTMAP_ON

            
            //自定义效果
           // #pragma multi_compile _ _NILOTOON_DISSOLVE
           // #pragma multi_compile _ _NILOTOON_DITHER_FADEOUT
//#pragma multi_compile _ _OTHERWORLD
            // GPU Instancing
            //    #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #define ForwardLitPass 1
            #if defined(LIGHTMAP_ON) 
#define SHADOWS_SHADOWMASK
          #define  LIGHTMAP_SHADOW_MIXING
          #define     DIRLIGHTMAP_COMBINED
#endif
            #include "Library/XHHYLitTest/XHHYLitInput.hlsl"
            #include "Library/XHHYLitTest/XHHYLitForwardPass.hlsl"
            ENDHLSL

        }


         Pass //"描边"
        {
            Name "Outline"
            Tags
            {

                "LightMode" = "NiloToonOutline" // 这里我们将"NiloToonOutline"设置为自定义通道的"LightMode"值，但它可以是任何字符串，只需选择一个你喜欢的

            }

            // 渲染状态
            // TODO: 我们应该找到一种方法来从第一个通道翻转Cull，而不是硬编码“Cull Front”
            Cull Front // 对于额外的通道挤出网格描边方法，必须使用Cull Front。
           // ColorMask [_ColorMask]
           // ZTest Less
            /*Stencil
            {
                Ref 199
                // Comp [_StencilComp] // 通过深度测试 = 通过模板测试
                Pass [_StencilOp] // replace意味着写入
            }*/
          //  Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            // #pragma enable_d3d11_debug_symbols
          //  #pragma shader_feature_local _ALPHATEST_ON
           // #pragma multi_compile _ _NILOTOON_DEBUG_SHADING // multi_compile_fragment is not enough, because of uv8
           // #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT //(can use _fragment suffix)
           // #pragma multi_compile_local _ _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
            //     #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            // ---------------------------------------------------------------------------------------------
            //   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //(can use _fragment suffix)
           // #define _NILOTOON_ADDITIONAL_LIGHTS 1

            #pragma vertex vert
            #pragma fragment frag//FragmentShaderOutLine

             #include "Library/XHHYLitTest/XHHYLitInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

              struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float4 lightMapUV : TEXCOORD1;
            float3 uv8 : TEXCOORD7;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
               // float2 uv : TEXCOORD0;
               // float3 worldPos : TEXCOORD1;
              //  float3 worldNormal : TEXCOORD2;
              //  float3 worldTangent : TEXCOORD3;
              //  float4 scrPos : TEXCOORD4;
              //  float4 lightmapUVOrVertexSH : TEXCOORD5;
               // float4 directionFactor : TEXCOORD6;
            };
float3 ConvertNormalTSToNormalTargetSpace(float3 normalTS, float3 tangentTargetSpace, float3 bitangentTargetSpace, float3 normalTargetSpace)
{
    // TBN matrix, usually when you need to convert tangent space normal to other space(target space), you will need a TBN matrix of that target space
    float3x3 TBNMatrixTargetSpace = float3x3(tangentTargetSpace, bitangentTargetSpace, normalTargetSpace);
   
    // Usually 99% of the time you will see hlsl's mul() in this order
    // resultVector = mul(matrix, vector);

    // However doing the multiplication of the matrix and the vector in a different order; the matrix is transposed.
    // mul(matrix, vector) == mul(vector, transpose(matrix))
    // mul(transpose(matrix), vector) == mul(vector, matrix)

    // for more information about it you can read: https://forum.unity.com/threads/mul-function-and-matrices.445266/#post-2880535

    // 1.TBNMatrixTargetSpace is a rotation only 3x3 matrix
    // 2.The inverse of a rotation only matrix is its transpose, which is also a rotation only matrix
    // (https://en.wikipedia.org/wiki/Rotation_matrix)
    // 3.mul(A,B) is the transpose of mul(B,A)
    float3 resultNormalTargetSpace = mul(normalTS, TBNMatrixTargetSpace); // so this line means transform from Tangent space back to "Target space"
    resultNormalTargetSpace = normalize(resultNormalTargetSpace);
    return resultNormalTargetSpace;
}

             v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal.xyz, IN.tangent);

              float3   extrudeDirectionWS = ConvertNormalTSToNormalTargetSpace(IN.uv8, vertex_normal_inputs.tangentWS,
                                                              vertex_normal_inputs.bitangentWS,
                                                              vertex_normal_inputs.normalWS);
                 vertex_position_inputs.positionWS += extrudeDirectionWS.xyz * _OutlineWidth*_OutlineWidthExtraMultiplier   * 0.005    ;
                float3 ViewPos = TransformWorldToView(vertex_position_inputs.positionWS);
    OUT.position = TransformWViewToHClip(ViewPos);
           //     OUT.worldPos = vertex_position_inputs.positionWS;
            //    OUT.directionFactor.x = dot(_HollowDirection_X, IN.vertex);
           //     OUT.directionFactor.y = dot(_HollowDirection_Y, IN.vertex);
           //     OUT.worldNormal = vertex_normal_inputs.normalWS;
           //     OUT.worldTangent = vertex_normal_inputs.tangentWS;
          //      OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
             // //  OUT.scrPos = ComputeScreenPos(OUT.position);
             //   OUTPUT_LIGHTMAP_UV(IN.lightMapUV, unity_LightmapST, OUT.lightmapUVOrVertexSH.xy);
           //     OUTPUT_SH(OUT.worldNormal, OUT.lightmapUVOrVertexSH.xyz);
                return OUT;
            }
            
            float4 frag(v2f IN): SV_Target
            {

                return _OutlineColor;

                }

          
            ENDHLSL
        }



        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            //  #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            //  #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Library/XHHYLitTest/XHHYLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }



        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma multi_compile _ _NILOTOON_DITHER_FADEOUT
            // -------------------------------------
            // Unity defined keywords
            //  #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            //  #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Library/XHHYLitTest/XHHYLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }



        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // -------------------------------------
            // Render State Commands
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit_XHHY

            // -------------------------------------
            // Material Keywords
         //   #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
           // #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //    #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
        //    #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
              #include "Library/XHHYLitTest/XHHYLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            half4 UniversalFragmentMetaLit_XHHY(Varyings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData,float3(0,0,0),float3(0,0,0));

                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular,
                    surfaceData.smoothness, surfaceData.alpha, brdfData);

                MetaInput metaInput;
                metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
                metaInput.Emission = surfaceData.emission;
                return UniversalFragmentMeta(input, metaInput);
            }
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        /*Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            //     #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitDepthNormalsPass.hlsl"
            ENDHLSL
        }*/
    }


    CustomEditor "LWGUI.LWGUI" // TODO: check if this line is still required, because removing this line, LWGUI will still work
}