Shader "ShaderTemplate/ScreenUV_Unlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        
    }

    SubShader
    {
        Tags
        {
            
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        // -------------------------------------
        // Render State Commands
      

        Pass
        {
           

            // -------------------------------------
            // Render State Commands
//Cull Off

            HLSLPROGRAM
            
            // -------------------------------------
            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
        
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            //    #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            //    #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitForwardPass.hlsl"

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

               CBUFFER_START(UnityPerMaterial)
              half4 _BaseColor;

            CBUFFER_END


            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);


            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings

            {
                float4 screenPos : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(appdata input)
            {
                Varyings output = (Varyings)0;


                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);

                return output;
            }

            float4 frag(Varyings i) : SV_Target

            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, screenUV);
              //  return 1;
                return texColor * _BaseColor;
            }
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

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }


    }

   // FallBack "Hidden/Universal Render Pipeline/FallbackError"
   // CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}