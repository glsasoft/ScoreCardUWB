Shader "ShaderTemplate/OccusionViewChar_V1"
{
    Properties
    {
        _StencilOcclusionViewValue("StencilOcclusionViewValue", Float) = 10.0

        _BaseMap ("Albedo Map", 2D) = "white" {}

        [Header(TeamColor)]
        _TeamColor("队友透视", Color) = (1, 1, 1, 1)
        [Toggle]_IsTeam("IsTeam", Float) = 1

        [Header(ScanColor)]
        _ScanColor("扫描透视", Color) = (1, 1, 1, 1)
        _RimPower(" Rim Power", Range(0, 5)) = 1
        _FlickerSpeed("闪烁速度", Range(0, 10)) = 1.0
        [Toggle]_IsScan("IsScan", Float) = 0
        
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        CBUFFER_START(UnityPerMaterial)

        float4 _BaseMap_ST;
        float4 _TeamColor;
        float _IsTeam;

        float4 _ScanColor;
        float _RimPower;
        float _FlickerSpeed;
        float _IsScan;


        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        ENDHLSL

        Pass{
            Name "OccusionViewChar"
            Tags { "LightMode" = "UniversalForward"} 

            ZWrite Off
            ZTest Greater       // 只有在后面才渲染
            Cull Back           // 避免前后同时渲染

            // Blend SrcAlpha OneMinusSrcAlpha   // *透明效果

            Stencil {
                Ref [_StencilOcclusionViewValue]    //  模版参考值
                Comp Equal                      //  模版测试 比较方式
                Pass Keep                           //  测试通过对模版值的操作（使用模版参考值）（默认模版值为0）
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal, IN.tangent);
                OUT.position = vertex_position_inputs.positionCS;
                OUT.worldPos = vertex_position_inputs.positionWS;
                OUT.worldNormal = vertex_normal_inputs.normalWS;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.worldPos);
                float3 normalWS = normalize(IN.worldNormal);
                float rim = pow(1.0 - saturate(dot(normalize(viewDir), normalize(normalWS))), _RimPower); 
             
                // 队友：纯色效果
                float3 color1 = _TeamColor;

                // 扫描效果
                float flicker = abs(sin(_Time.y * _FlickerSpeed)) + 0.2;  
                float3 color2 = _ScanColor * flicker * rim;  

                float3 finalColor = color1 * _IsTeam + color2 * _IsScan;
                
                return float4(finalColor,1);
            }
            ENDHLSL
        }

        
        Pass
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _DissolveMapOn

            TEXTURE2D(_DissolveMap);
            SAMPLER(sampler_DissolveMap);
            
            struct a2v {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldFactor : TEXCOORD1;
            };
            
            float3 _LightDirection;
            
            v2f vert(a2v v)
            {
                v2f o = (v2f)0;
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normal);
                worldPos = ApplyShadowBias(worldPos, normalWS, _LightDirection);
                o.vertex = TransformWorldToHClip(worldPos);
    			//参考 cat like coding 博主的处理方式
                #if UNITY_REVERSED_Z
    			o.vertex.z = min(o.vertex.z, o.vertex.w * UNITY_NEAR_CLIP_VALUE);
                #else
    			o.vertex.z = max(o.vertex.z, o.vertex.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                o.uv = v.uv;
                return o;
            }
            real4 frag(v2f i) : SV_Target
            {
                float dissolveAlpha = 1;
                #if _DissolveMapOn
                    
                #endif
                clip(dissolveAlpha - 0.01);
                
                return 0;
            }
            ENDHLSL
        }

        Pass 
        {
            Name "DepthOnly"

            Tags {
                "LightMode" = "DepthOnly"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            #pragma vertex m_DepthOnlyVertex
            #pragma fragment m_DepthOnlyFragment

            #pragma shader_feature_local _DissolveMapOn

            TEXTURE2D(_DissolveMap);
            SAMPLER(sampler_DissolveMap);

            struct m_Varyings
            {
                float2 uv       : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                float4 worldFactor : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            m_Varyings m_DepthOnlyVertex(Attributes input)
            {
                m_Varyings output = (m_Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                // float3 rootPos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                // output.worldFactor = dot(normalize(_DissolveDir.xyz), TransformObjectToWorld(input.position) - rootPos);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half m_DepthOnlyFragment(m_Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // float dissolveAlpha = 1;
                // #if _DissolveMapOn
                //     float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, input.uv);
                //     dissolveAlpha = (dissolveMap - _DissolveThreshold) + input.worldFactor * _DissolveWorldSpaceScale;
                // #endif
                // clip(dissolveAlpha - 0.01);

                // #if defined(LOD_FADE_CROSSFADE)
                //     LODFadeCrossFade(input.positionCS);
                // #endif

                return input.positionCS.z;
            }
            ENDHLSL
        }

    }
}
