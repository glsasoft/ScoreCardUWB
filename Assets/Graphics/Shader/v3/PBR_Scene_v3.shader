Shader "ShaderTemplate/PBR_Scene_V3"
{
    Properties
    {
        _CullOff ("Two Side", Float) = 2.0
        _Transparent ("Transparent", Float) = 0.0
        _Unlit ("Unlit", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        
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
        _MaskMap ("Metallic Roughness AO Map", 2D) = "black" {}
        _MetallicMap ("Metallic Map", 2D) = "black" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _AoMap ("AO Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Roughness("Roughness", Range(0, 1)) = 0.0
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
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float _BaseMapValue, _BaseMapSaturation, _BaseColorIntensity;
        float _EmissionIntensity;
        float4 _EmissionColor;
        
        float _Metallic, _Roughness;
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
            
            Cull [_CullOff]
            ZWrite[_ZWrite]
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _USE_FORWARD_PLUS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            
            #pragma shader_feature_local _Unlit_ON
            #pragma shader_feature_local _BaseMapOn
            #pragma shader_feature_local _EmissionMapOn
            #pragma shader_feature_local _NormalMapOn
            #pragma shader_feature_local _MaskMapOn
            #pragma shader_feature_local _RoughnessMapOn
            #pragma shader_feature_local _AoMapOn
            #pragma shader_feature_local _MetallicMapOn
            
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap);
            SAMPLER(sampler_MaskMap);
            TEXTURE2D(_MetallicMap);
            SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_RoughnessMap);
            SAMPLER(sampler_RoughnessMap);
            TEXTURE2D(_AoMap);
            SAMPLER(sampler_AoMap);
            
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float4 lightMapUV : TEXCOORD1;
                float4 uv7 : TEXCOORD7;
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
                float3 worldSmoothNormal : TEXCOORD6;
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
                float3 rootPos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                OUT.worldNormal = vertex_normal_inputs.normalWS;
                OUT.worldTangent = vertex_normal_inputs.tangentWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.scrPos = ComputeScreenPos(OUT.position);
                OUT.worldSmoothNormal = IN.uv7.rgb;
                OUTPUT_LIGHTMAP_UV(IN.lightMapUV, unity_LightmapST, OUT.lightmapUVOrVertexSH.xy);
                OUTPUT_SH(OUT.worldNormal, OUT.lightmapUVOrVertexSH.xyz);
                return OUT;
            }

            float4 frag(v2f IN): SV_Target
            {
                float3 worldPos = IN.worldPos;
                
                //BaseColor
                float4 baseTexColor = _BaseColor;
                {
                    #if _BaseMapOn
                        baseTexColor *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                    #endif
                        float3 valueBaseTexColor = baseTexColor.rgb * _BaseMapValue;
                        float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                        baseTexColor.xyz = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation) * _BaseColorIntensity;
                }
                
                //Normal
                float3 worldNormal = 0;
                float3 worldBitTangent = normalize(cross(IN.worldNormal, IN.worldTangent));
                float3x3 tbn = float3x3(IN.worldTangent, worldBitTangent, IN.worldNormal);
                float3 worldSmoothNormal = normalize(mul(IN.worldSmoothNormal, tbn));
                {
                    #if _NormalMapOn
                        float3 normalTexColor = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                        worldNormal = normalize(mul(normalTexColor, tbn));
                    #else
                    worldNormal = normalize(IN.worldNormal);
                    #endif
                }
                
                //Params
                float3 emColor = _EmissionColor.rgb * _EmissionIntensity;
                float ao = 1;
                float metallic = saturate(_Metallic);
                float roughness = saturate(_Roughness);
                float perceptualRoughness = roughness;
                float squareRoughness = 0;
                float smoothness = 0;
                {
                    #if _EmissionMapOn
                        emColor = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).rgb * _EmissionColor.rgb * _EmissionIntensity;
                    #endif

                    #if _MaskMapOn
                        float4 maskTexColor = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, IN.uv);
                        metallic = saturate(maskTexColor.r + _Metallic);
                        roughness = saturate(maskTexColor.g + _Roughness);
                        ao = smoothstep(0, 1, maskTexColor.b);
                    #endif

                    #if _MetallicMapOn
                        float metallicMapColor = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, IN.uv).x;
                        metallic = saturate(metallicMapColor + _Metallic);
                    #endif
                    
                    #if  _RoughnessMapOn
                        float roughnessTexColor = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, IN.uv).x;
                        roughness = saturate(roughnessTexColor + _Roughness);
                    #endif
                    smoothness = 1 - roughness;
                    perceptualRoughness = roughness;
                    roughness = perceptualRoughness * perceptualRoughness;
                    roughness = max(roughness, 0.01);
                    squareRoughness = roughness * roughness;

                    #if _AoMapOn
                        ao = SAMPLE_TEXTURE2D(_AoMap, sampler_AoMap, IN.uv).x;
                    #endif
                }

                //Content
                //一些变量
                float2 scrPos = IN.scrPos.xy / IN.scrPos.w;
                float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
                Light light = GetMainLight(shadowCoord);
                float3 lightColor = light.color;
                float3 lightDir = light.direction;
                float3 viewDir = normalize(GetWorldSpaceViewDir(worldPos));
                float3 viewNormal = mul(unity_WorldToCamera, float4(IN.worldNormal, 0)).xyz;
                float3 reflectDir = reflect(-normalize(viewDir), worldNormal);
                float2 matcapUV = (viewNormal * 0.5 + 0.5).xy;
                float nol = saturate(max(dot(worldNormal, lightDir), 1e-5));
                float lambert = dot(worldNormal, lightDir);
                float halfLambert = lambert * 0.5 + 0.5;
                float lambertStep = smoothstep(0.423, 0.465, halfLambert);
                float3 halfDir = normalize(viewDir + lightDir);
                float noh = saturate(max(dot(worldNormal, halfDir), 1e-5));
                float nov = saturate(max(dot(worldNormal, viewDir), 1e-5));
                float hol = saturate(max(dot(halfDir, lightDir), 1e-5));
                
                //Shadow
                float shadowMap = 1;
                {
                    shadowMap = light.shadowAttenuation;
                }

                //LightMap
                //烘焙贴图 或 光照探针贴图
                float3 ambientColor = 0;
                {
                    ambientColor = IndirectDiffuse(IN.lightmapUVOrVertexSH.xy, worldNormal);
                    MixRealtimeAndBakedGI(light, worldNormal, ambientColor);
                    ambientColor = saturate(ambientColor);
                }
                
                //PBR
                float3 pbrDiffuseColor = 0;
                float3 pbrSpecularColor = 0;
                float3 pbrIndirectColor = 0;
                float3 pbrIndirectDiffColor = 0;
                float3 pbrIndirectSpeColor = 0;
                float3 pbrBRDFSpecSection = 0;
                {
                    float3 F0 = F0_Function(baseTexColor.rgb, metallic);
                    float3 Direct_F = F_Function(hol, F0);
                    float3 Kd = (1 - Direct_F) * (1 - metallic);
                    pbrDiffuseColor = Kd * baseTexColor.rgb * nol * lightColor;
                    
                    //Specular
                    float Direct_D = D_Function(noh, squareRoughness);
                    float Direct_G = G_Function(nol, nov, roughness);
                    pbrBRDFSpecSection = Direct_D * Direct_G * Direct_F / (4 * nol * nov + 1e-5);
                    
                    pbrSpecularColor = pbrBRDFSpecSection * lightColor * nol * PI;

                    //Indirect
                    float3 Indirect_Ks = Indirect_F_Function(nov, F0, roughness);
                    float3 Indirect_Kd = (1 - Indirect_Ks) * (1 - metallic);
                    pbrIndirectDiffColor = ambientColor * Indirect_Kd * baseTexColor.rgb * saturate(halfLambert);
                    
                    float3 IndirectSpeCubeColor = IndirectSpeCube(reflectDir, worldPos, scrPos, perceptualRoughness, ao);
                    float3 IndirectSpeCubeFactor = IndirectSpeFactor(roughness, smoothness, pbrBRDFSpecSection, F0, nov);
                    pbrIndirectSpeColor = IndirectSpeCubeColor * IndirectSpeCubeFactor;

                    pbrIndirectColor = pbrIndirectDiffColor + pbrIndirectSpeColor;
                }
                
                
                //多光源
                float3 addLightColor = 0;
                {
                    uint addLightsCount = GetAdditionalLightsCount();
                    InputData inputData = InitializeInputData(IN);
                    LIGHT_LOOP_BEGIN(addLightsCount)
                    {
                        //Context
                        Light addLight = GetAdditionalLight(lightIndex, worldPos);
                        float3 lightDir_Add = addLight.direction;
                        float3 lightColor_Add = addLight.color;
                        float3 halfDir_Add = normalize(lightDir_Add + viewDir);
                        float hol_Add = saturate(max(dot(halfDir_Add, lightDir_Add), 1e-5));
                        float nol_Add = saturate(max(dot(worldNormal, lightDir_Add), 1e-5));
                        float noh_Add = saturate(max(dot(worldNormal, halfDir_Add), 1e-5));
                        float shadow_Add = addLight.distanceAttenuation * addLight.shadowAttenuation;
                        
                        float3 baseColor_Add = baseTexColor.rgb;
                        //Diffuse
                        float3 F0_Add = F0_Function(baseColor_Add, metallic);
                        float3 Direct_F_Add = F_Function(hol_Add, F0_Add);
                        float3 Kd_Add = (1 - Direct_F_Add) * (1 - metallic);
                        float3 diffuse_Add = Kd_Add * baseColor_Add * lightColor_Add * nol_Add;

                        //Specular
                        float Direct_D_Add = D_Function(noh_Add, squareRoughness);
                        float Direct_G_Add = G_Function(nol_Add, nov, roughness);
                        float3 BRDFSpecSection_Add = Direct_D_Add * Direct_G_Add * Direct_F_Add / (4 * nol_Add * nov + 1e-5);
                        float3 specular_Add = BRDFSpecSection_Add * lightColor_Add * nol_Add * PI;
                        addLightColor += (diffuse_Add + specular_Add) * shadow_Add;
                    }
                    LIGHT_LOOP_END
                }
                float3 finalColor = float3(1, 0, 0);
                float finalAlpha = 1;
                #if _Unlit_ON
                finalColor = baseTexColor.rgb + emColor;
                finalAlpha = baseTexColor.a;
                #else
                finalColor = saturate(pbrIndirectColor + (pbrDiffuseColor + pbrSpecularColor) * shadowMap * ao + addLightColor * ao) + emColor;
                finalAlpha = baseTexColor.a;
                #endif
                
                return float4(finalColor, finalAlpha);
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
                return 0;
            }
            ENDHLSL
        }

        Pass {
            Name "DepthOnly"

            Tags {
                "LightMode" = "DepthOnly"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
            Cull Off
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

            #pragma vertex UniversalVertexMeta
            #pragma fragment CartoonMetaFragment
            
            half4 CartoonMetaFragment(Varyings i) : SV_Target
            {
                float4 baseTexColor = _BaseColor;
                #if _BaseMapOn
                    baseTexColor *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                #endif
                float3 valueBaseTexColor = baseTexColor.rgb * _BaseMapValue;
                float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                baseTexColor.xyz = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation) * _BaseColorIntensity;
                
                float3 emColor = _EmissionColor.rgb * _EmissionIntensity;
                #if _EmissionMapOn
                    emColor *= SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).rgb;
                #endif
                
                MetaInput metaInput;
                metaInput.Albedo = baseTexColor.rgb;
                metaInput.Emission = emColor;
                return UniversalFragmentMeta(i, metaInput);
            }
            ENDHLSL
        }
    }
    CustomEditor "PBRSceneGUI_V3"
}
