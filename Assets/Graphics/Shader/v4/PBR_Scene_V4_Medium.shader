Shader "ShaderTemplate/PBR_Scene_V4_Medium"
{
    Properties
    {
        _CullOff ("Two Side", Float) = 2.0
        _Transparent ("Transparent", Float) = 0.0
        _Unlit ("Unlit", Float) = 0.0
        _Static ("Static", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Queue("__queue", Float) = 0.0
        
        [Header(BaseColor)]
        [Space(10)]
        _BaseMap ("Albedo Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMapValue("Base Map Value", Range(0, 10)) = 1
        _BaseMapSaturation("Base Map Saturation", Range(0, 5)) = 1
        _BaseColorIntensity ("Base Color Intensity", Range(0, 5)) = 1
        _EmissionMap ("Emission Map", 2D) = "black" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 0
        
        [Header(Normal)]
        [Space(10)]
        _NormalMap("Normal Map", 2D) = "bump" { }
        
        [Header(Metallic Roughness)]
        [Space(10)]
        _MaskMap ("Metallic Roughness AO Map", 2D) = "white" {}
        _Metallic("Metallic", Range(-1, 1)) = 0.0
        _Roughness("Roughness", Range(-1, 1)) = 0.0
        
        [Header(Hollow)]
        _Hollow ("Hollow", Float) = 0
        _RisAlpha ("R is Alpha", Float) = 0
        _FlowSpeed ("Flow Speed", Range(0, 30)) = 1
        _HollowDirection_X ("Hollow Direction X", Vector) = (1, 0, 0, 0)
        _HollowDirection_Y ("Hollow Direction Y", Vector) = (0, 1, 0, 0)
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

        CBUFFER_START(UnityPerMaterial)
        float _Unlit;
        float _Queue;
        float _Static;
        
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float _BaseMapValue, _BaseMapSaturation, _BaseColorIntensity;
        float _EmissionIntensity;
        float4 _EmissionColor;
        
        float _Metallic, _Roughness;

        float _Hollow;
        float _RisAlpha;
        float _FlowSpeed;
        float4 _HollowDirection_X;
        float4 _HollowDirection_Y;
        CBUFFER_END
        
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        
        ENDHLSL
        
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            Stencil
            {
                Ref 10          //  模版参考值
                Comp Always     //  模版测试 比较方式
                Pass Replace    //  测试通过对模版值的操作（使用模版参考值）
            }
            
            Cull [_CullOff]
            ZWrite[_ZWrite]
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _USE_FORWARD_PLUS
            // 中画质不开启阴影
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS                  // 主光阴影
            // #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS        // 主光投射阴影
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE          // 级联阴影
            // #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS            // 多光源阴影
            // #pragma multi_compile _ _SHADOWS_SOFT                        // PCF阴影
            // #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING               // 混合 实时阴影 + 烘焙光照
            // #pragma multi_compile _ SHADOWS_SHADOWMASK                   // 混合 实时阴影 + 烘焙阴影
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap);
            SAMPLER(sampler_MaskMap);
            
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float4 lightMapUV : TEXCOORD1;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float4 scrPos : TEXCOORD4;
                float4 lightmapUVOrVertexSH : TEXCOORD5;
                float4 directionFactor : TEXCOORD6;
            };

            //PBR
            float3 IndirectDiffuse( float2 uvStaticLightmap, float3 normalWS )
            {
                #ifdef LIGHTMAP_ON
                return SampleLightmap(uvStaticLightmap, normalWS);
                #else
                return SampleSH(normalWS);
                #endif
            }
            InputData InitializeInputData(v2f IN)
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.normalizedScreenSpaceUV = float2(IN.scrPos.xy / IN.scrPos.w);
                return inputData;
            }
            float D_Function(float NdotH, float squareRoughness)
            {
                float a2 = squareRoughness;
                float NdotH2 = NdotH * NdotH;
                float d = NdotH2 * (a2 - 1) + 1;
                d = d * d * PI;
                return a2 / d;
            }
            inline float G_subSection(float dot, float k)
            {
	            return dot / lerp(dot, 1, k);
            }
            float G_Function(float NdotL, float NdotV, float roughness)
            {
                float k = pow(1 + roughness, 2) * 0.5;
                return G_subSection(NdotL, k) * G_subSection(NdotV, k);
            }
            inline float3 F0_Function(float3 albedo, float metallic)
            {
                return lerp(0.04, albedo, metallic);
            }
            float3 F_Function(float HdotL, float3 F)
            {
                float Fre = exp2((-5.55473 * HdotL - 6.98316) * HdotL);
                return lerp(Fre, 1, F);
            }
            float3 Indirect_F_Function(float NdotV, float3 F0, float roughness)
            {
                float fre = exp2((-5.55473 * NdotV - 6.98316) * NdotV);
                return F0 + fre * saturate(1 - roughness - F0);
            }
            float3 IndirectSpeCube(float3 reflectDir, float3 worldPos, float2 scrPos, float perceptualRoughness, float ao)
            {
                float3 specularColor = GlossyEnvironmentReflection(reflectDir, worldPos, perceptualRoughness, ao, scrPos);
                return specularColor;
            }
            float3 IndirectSpeFactor(float roughness, float smoothness, float3 BRDFspe, float3 F0, float NdotV)
            {
                #ifdef UNITY_COLORSPACE_GAMMA
                float SurReduction = 1 - 0.28 * roughness * roughness;
                #else
                float SurReduction = 1 / (roughness * roughness + 1);
                #endif
                #if defined(SHADER_API_GLES) // Lighting.hlsl 261 行
                float Reflectivity = BRDFspe.x;
                #else
                float Reflectivity = max(max(BRDFspe.x, BRDFspe.y), BRDFspe.z);
                #endif
                float GrazingTSection = saturate(Reflectivity + smoothness);
                float fre = Pow4(1 - NdotV); // Lighting.hlsl 第 501 行
                // float fre = exp2((-5.55473 * NdotV - 6.98316) * NdotV); // Lighting.hlsl 第 501 行，他是 4 次方，我们是 5 次方
                return lerp(F0, GrazingTSection, fre) * SurReduction;
            }
            
            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal.xyz, IN.tangent);
                OUT.position = vertex_position_inputs.positionCS;
                OUT.worldPos = vertex_position_inputs.positionWS;
                OUT.directionFactor.x = dot(_HollowDirection_X, IN.vertex * 100);
                OUT.directionFactor.y = dot(_HollowDirection_Y, IN.vertex * 100);
                OUT.worldNormal = vertex_normal_inputs.normalWS;
                OUT.worldTangent = vertex_normal_inputs.tangentWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.scrPos = ComputeScreenPos(OUT.position);
                OUTPUT_LIGHTMAP_UV(IN.lightMapUV, unity_LightmapST, OUT.lightmapUVOrVertexSH.xy);
                OUTPUT_SH(OUT.worldNormal, OUT.lightmapUVOrVertexSH.xyz);
                return OUT;
            }

            float4 frag(v2f IN): SV_Target
            {
                float3 worldPos = IN.worldPos;
                float2 uv = lerp(float2(IN.directionFactor.x - _Time.y * _FlowSpeed, IN.directionFactor.y) * 0.01, IN.uv, step(_Hollow, 0));
                
                float4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                float3 valueBaseTexColor = baseTexColor.rgb * _BaseMapValue;
                float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                float3 baseColor = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation) * _BaseColorIntensity * _BaseColor;

                //Normal
                float3 worldBitTangent = normalize(cross(IN.worldNormal, IN.worldTangent));
                float3x3 tbn = float3x3(IN.worldTangent, worldBitTangent, IN.worldNormal);
                float3 normalTexColor = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));
                float3 worldNormal = normalize(mul(normalTexColor, tbn));
                
                //PBR Params
                float3 emColor = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor.rgb * _EmissionIntensity;
                float4 maskTexColor = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
                float metallic = saturate(maskTexColor.r + _Metallic);
                // float roughness = saturate(maskTexColor.g + _Roughness);
                float ao = maskTexColor.b;

                // float smoothness = 1 - roughness;
                // float perceptualRoughness = roughness;
                // roughness = perceptualRoughness * perceptualRoughness;
                // roughness = max(roughness, 0.01);
                // float squareRoughness = roughness * roughness;
                
                //Content
                //一些变量
                // float2 scrPos = IN.scrPos.xy / IN.scrPos.w;
                float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
                Light light = GetMainLight(shadowCoord);
                float3 lightColor = light.color;
                float3 lightDir = light.direction;
                
                //Shadow
                // float shadowMap = light.shadowAttenuation;

                //LightMap
                //烘焙贴图 或 光照探针贴图
                float3 ambientColor;
                {
                    ambientColor = IndirectDiffuse(IN.lightmapUVOrVertexSH.xy, worldNormal);
                    MixRealtimeAndBakedGI(light, worldNormal, ambientColor);
                    ambientColor = saturate(ambientColor);
                }
                
                float4 dominantDir = SAMPLE_TEXTURE2D(unity_LightmapInd, sampler_LinearClamp, IN.lightmapUVOrVertexSH);
                float4 dominantLightDir = float4(dominantDir * 2 - 1);
                float mainLightFactor = length(dominantLightDir.xyz);
                float3 mainLightDir = normalize(dominantLightDir.xyz / max(0.001, mainLightFactor));
                
                lightDir = lerp(mainLightDir, lightDir, step(_Static, 0));
                lightColor = lerp(mainLightFactor * ambientColor / dominantLightDir.w, lightColor, step(_Static, 0));
                
                float3 viewDir = normalize(GetWorldSpaceViewDir(worldPos));
                // float3 reflectDir = reflect(-normalize(viewDir), worldNormal);
                float nol = saturate(max(dot(worldNormal, lightDir), 1e-5));
                float lambert = dot(worldNormal, lightDir);
                float halfLambert = lambert * 0.5 + 0.5;
                float3 halfDir = normalize(viewDir + lightDir);
                // float noh = saturate(max(dot(worldNormal, halfDir), 1e-5));
                // float nov = saturate(max(dot(worldNormal, viewDir), 1e-5));
                // float hol = saturate(max(dot(halfDir, lightDir), 1e-5));

                // PBR 简化版
                float3 pbrDiffuseColor;
                float3 pbrSpecularColor;
                float3 pbrIndirectColor;
                {
                    float3 F0 = F0_Function(baseColor.rgb, metallic);
                    float voh = saturate(max(dot(viewDir, halfDir), 1e-5));

                    // 中画质 - 简化菲涅尔
                    // float3 Direct_F = F_Function(hol, F0);
                    // float3 Kd = (1 - Direct_F) * (1 - metallic);
                    float fresnel = Pow4(1.0 - voh);
                    float3 Direct_F_simplified = lerp(F0, 1.0, fresnel);
                    float3 Kd = (1.0 - Direct_F_simplified) * (1.0 - metallic);
                    pbrDiffuseColor = Kd * baseColor.rgb * nol * lightColor;
                    // 中画质 - 简化PbrSpecular
                    pbrSpecularColor = Direct_F_simplified * lightColor * nol;
                    
                    // 中画质 - 简化间接高光
                    float3 Indirect_Kd = (1.0 - Direct_F_simplified) * (1.0 - metallic);
                    float3 pbrIndirectDiffColor = ambientColor * Indirect_Kd * baseColor.rgb * saturate(halfLambert);
                    // 中画质 - 移除IBL间接高光
                    pbrIndirectColor = pbrIndirectDiffColor;    
                }
                
                // 中画质 - 移除多光源（实际也没用到）

                //PBR Color
                float3 pbrFinalColor = saturate(pbrIndirectColor + (pbrDiffuseColor + pbrSpecularColor)  * ao )+ emColor;
                
                float finalAlpha = baseTexColor.a * _BaseColor.a;
                finalAlpha = lerp(baseTexColor.r * _BaseColor.a, finalAlpha, step(_RisAlpha, 0));

                //Unlit Color
                float3 unlitFinalColor = saturate(baseColor.rgb) + emColor;

                //Hollow Color
                float3 hollowFinalColor = saturate(baseColor.rgb) + emColor;
                
                //Final Color
                float3 finalColor = lerp(unlitFinalColor, pbrFinalColor, step(_Unlit, 0));
                finalColor = lerp(hollowFinalColor, finalColor, step(_Hollow, 0));
                
                return float4(finalColor, finalAlpha);
            }
            
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            Cull [_CullOff]
            ZWrite[_ZWrite]
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            struct a2v {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float3 _LightDirection;
            
            v2f vert(a2v v)
            {
                v2f o;
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
            // 深度测试也需要剔除
            Cull [_CullOff]
            ZWrite[_ZWrite]
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            // Meta 光照烘焙通道(unity原生烘焙需要用）
            Tags
            {
                "LightMode" = "Meta"
            }
            Cull [_CullOff]
            ZWrite[_ZWrite]
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

            #pragma vertex UniversalVertexMeta
            #pragma fragment CartoonMetaFragment
            
            half4 CartoonMetaFragment(Varyings i) : SV_Target
            {
                float4 baseTexColor = _BaseColor;
                baseTexColor *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float3 valueBaseTexColor = baseTexColor.rgb * _BaseMapValue;
                float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                baseTexColor.xyz = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation) * _BaseColorIntensity;
                
                float3 emColor = _EmissionColor.rgb * _EmissionIntensity;
                emColor *= SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv).rgb;
                
                MetaInput metaInput;
                metaInput.Albedo = baseTexColor.rgb;
                metaInput.Emission = emColor;
                return UniversalFragmentMeta(i, metaInput);
            }
            ENDHLSL
        }
    }
    CustomEditor "PBRSceneGUI_V4"
}
