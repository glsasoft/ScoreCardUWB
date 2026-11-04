Shader "ShaderTemplate/PBR_Scene_V4"
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
        
        [Header(Dissolve)]
        [Toggle]_Dissolve_On ("Dissolve On", Range(0, 1)) = 0
        _DissolveMapThreshold ("DissolveMap Threshold", Range(0, 1)) = 0
        [HDR]_DissolveColor ("Dissolve Color", Color) = (1, 1, 1, 1)
        _DissolveColorStrength ("Dissolve Color Strength", Range(0, 3)) = 1
        
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _DissolveTexFlowAndBevel ("DissolveMap Flow&Bevel", Vector) = (0, 0, 0, 0)
        _DissolveMapEdge("DissolveMap Edge", Range(0,0.5)) = 0
        
        [Header(Hollow)]
        _Hollow ("Hollow", Float) = 0
        _RisAlpha ("R is Alpha", Float) = 0
        _FlowSpeed ("Flow Speed", Range(0, 100)) = 1
        _HollowDirection_X ("Hollow Direction X", Vector) = (1, 0, 0, 0)
        _HollowDirection_Y ("Hollow Direction Y", Vector) = (0, 1, 0, 0)
        _AmbientColorInt ("_AmbientColorInt",Float) = 1
        
        
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

        float _Dissolve_On;
        float _DissolveMapThreshold;
        float4 _DissolveColor;
        float _DissolveColorStrength;
        float4 _DissolveMap_ST;
        float4 _DissolveTexFlowAndBevel;
        float _DissolveMapEdge;
        float _AmbientColorInt;
        CBUFFER_END
        
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(_DissolveMap);
        SAMPLER(sampler_DissolveMap);
        
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
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
            
            InputData InitializeInputData(v2f IN)
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.normalizedScreenSpaceUV = float2(IN.scrPos.xy / IN.scrPos.w);
                return inputData;
            }
            ////////////////////////
            // PBR
            ////////////////////////
            // D - 法线分布
            float D_Function(float NdotH, float roughness)
            {
                float a = roughness * roughness;
                float a2 = a * a;
                float d = (NdotH * NdotH * (a2 - 1.0f) + 1.0f);
                return a2 / (d * d * PI);
            }
            // G - 几何遮蔽
            inline float G_subSection(float dot, float k)
            {
                // lerp(a, b, k) = a(1-k) + bk
	            return dot / lerp(dot, 1, k);
            }
            float G_Function(float NdotL, float NdotV, float roughness)
            {
                float k = pow(1 + roughness, 2) * 0.5;
                return G_subSection(NdotL, k) * G_subSection(NdotV, k);
            }
            // F - 菲涅尔
            inline float3 F0_Function(float3 albedo, float metallic)
            {
                return lerp(0.04, albedo, metallic);
            }
            float3 F_Function(float HdotL, float3 F)
            {
                float Fre = exp2((-5.55473 * HdotL - 6.98316) * HdotL);
                return lerp(Fre, 1, F);
            }
            
            ////////////////////////
            // 间接光 Indirect
            ////////////////////////
            float3 Indirect_F_Function(float NdotV, float3 F0, float roughness)
            {
                float fre = exp2((-5.55473 * NdotV - 6.98316) * NdotV);
                return F0 + fre * saturate(1 - roughness - F0);
            }
            float3 IndirectSpeCube(float3 reflectDir, float3 worldPos, float2 scrPos, float roughness, float ao)
            {
                float3 specularColor = GlossyEnvironmentReflection(reflectDir, worldPos, roughness, ao, scrPos);
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
            float3 IndirectDiffuse( float2 uvStaticLightmap, float3 normalWS )
            {
                #ifdef LIGHTMAP_ON
                return SampleLightmap(uvStaticLightmap, normalWS);
                #else
                return SampleSH(normalWS);
                #endif
            }
            
            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal.xyz, IN.tangent);
                OUT.position = vertex_position_inputs.positionCS;
                OUT.worldPos = vertex_position_inputs.positionWS;
                OUT.directionFactor.x = dot(_HollowDirection_X, IN.vertex);
                OUT.directionFactor.y = dot(_HollowDirection_Y, IN.vertex);
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

               // return 0.5;
                float3 worldPos = IN.worldPos;
                
                float2 uv = lerp(float2(IN.directionFactor.x - _Time.y * _FlowSpeed, IN.directionFactor.y), IN.uv, step(_Hollow, 0));
                
                
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
                float roughness = max(saturate(maskTexColor.g + _Roughness), 0.01);
                float ao = maskTexColor.b;
                float smoothness = 1 - roughness;
                
                //一些变量
                float2 scrPos = IN.scrPos.xy / IN.scrPos.w;
                float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
                Light light = GetMainLight(shadowCoord);
                float3 lightColor = light.color;
                float3 lightDir = light.direction;
                
                //Shadow
                float shadowMap = light.shadowAttenuation;

                //LightMap
                //烘焙贴图 或 光照探针贴图
                float3 ambientColor = 0;
                {
                    ambientColor = IndirectDiffuse(IN.lightmapUVOrVertexSH.xy, worldNormal);
                    MixRealtimeAndBakedGI(light, worldNormal, ambientColor);
                    ambientColor = saturate(ambientColor);
                }
                ambientColor *= _AmbientColorInt;
                float4 dominantDir = SAMPLE_TEXTURE2D(unity_LightmapInd, sampler_LinearClamp, IN.lightmapUVOrVertexSH);
                float4 dominantLightDir = float4(dominantDir * 2 - 1);
                float mainLightFactor = length(dominantLightDir.xyz);
                float3 mainLightDir = normalize(dominantLightDir.xyz / max(0.001, mainLightFactor));
                
                lightDir = lerp(mainLightDir, lightDir, step(_Static, 0));
                lightColor = lerp(mainLightFactor * ambientColor / dominantLightDir.w, lightColor, step(_Static, 0));
                
                float3 viewDir = normalize(GetWorldSpaceViewDir(worldPos));
                float3 reflectDir = reflect(-normalize(viewDir), worldNormal);
                float nol = saturate(max(dot(worldNormal, lightDir), 1e-5));
                float lambert = dot(worldNormal, lightDir);
                float halfLambert = lambert * 0.5 + 0.5;
                float3 halfDir = normalize(viewDir + lightDir);
                float noh = saturate(max(dot(worldNormal, halfDir), 1e-5));
                float nov = saturate(max(dot(worldNormal, viewDir), 1e-5));
                float hol = saturate(max(dot(halfDir, lightDir), 1e-5));
                
                //PBR
                float3 pbrDiffuseColor = 0;
                float3 pbrSpecularColor = 0;
                float3 pbrIndirectColor = 0;
                {
                    // 01 直接光 Direct
                    // diffuse
                    float3 F0 = F0_Function(baseColor.rgb, metallic);
                    float3 Direct_F = F_Function(hol, F0);
                    float3 Kd = (1 - Direct_F) * (1 - metallic);
                    pbrDiffuseColor = Kd * baseColor.rgb * nol * lightColor;
                    // specular
                    float Direct_D = D_Function(noh, roughness);
                    float Direct_G = max(G_Function(nol, nov, roughness), 1e-5);
                    float3 pbrBRDFSpecSection = Direct_D * Direct_G * Direct_F / (4 * nol * nov + 1e-5);
                    pbrSpecularColor = pbrBRDFSpecSection * lightColor * nol * PI;

                    // 02 间接光 Indirect
                    // diffuse
                    float3 Indirect_Ks = Indirect_F_Function(nov, F0, roughness);
                    float3 Indirect_Kd = (1 - Indirect_Ks) * (1 - metallic);
                    float3 pbrIndirectDiffColor = ambientColor * Indirect_Kd * baseColor.rgb * saturate(halfLambert);
                    // specular
                    float3 IndirectSpeCubeColor = IndirectSpeCube(reflectDir, worldPos, scrPos, roughness, ao);
                    float3 IndirectSpeCubeFactor = IndirectSpeFactor(roughness, smoothness, pbrBRDFSpecSection, F0, nov);
                    float3 pbrIndirectSpeColor = IndirectSpeCubeColor * IndirectSpeCubeFactor;

                    pbrIndirectColor = pbrIndirectDiffColor + pbrIndirectSpeColor;
                }
                
                //多光源
                float3 addLightColor = 0;
                {
                    uint addLightsCount = GetAdditionalLightsCount();
                    // InputData inputData = InitializeInputData(IN);
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
                        float3 baseColor_Add = baseColor.rgb;
                        //Diffuse
                        float3 F0_Add = F0_Function(baseColor_Add, metallic);
                        float3 Direct_F_Add = F_Function(hol_Add, F0_Add);
                        float3 Kd_Add = (1 - Direct_F_Add) * (1 - metallic);
                        float3 diffuse_Add = Kd_Add * baseColor_Add * lightColor_Add * nol_Add;

                        //Specular
                        float Direct_D_Add = D_Function(noh_Add, roughness);
                        float Direct_G_Add = G_Function(nol_Add, nov, roughness);
                        float3 BRDFSpecSection_Add = Direct_D_Add * Direct_G_Add * Direct_F_Add / (4 * nol_Add * nov + 1e-5);
                        float3 specular_Add = BRDFSpecSection_Add * lightColor_Add * nol_Add * PI;
                        addLightColor += (diffuse_Add + specular_Add) * shadow_Add;
                    }
                    LIGHT_LOOP_END
                }

                // TODO：溶解完善
                // 特殊1. 溶解处理
                float3 dissolveMapColor = 0;
                float MapLightMask = 0;
                {
                    // ====== step.1 Alpha计算 ======
                    // 贴图溶解
                    float2 dissolveUV = IN.uv * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
                    dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z, dissolveUV.y + dissolveUV.x * _DissolveTexFlowAndBevel.w);
                    dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;
                    float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                    float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                    float dissolveMapFactor = dissolveMap - strength;
                    
                    // 剔除
                    float dissolveAlpha = dissolveMapFactor;
                    dissolveAlpha = lerp(1, dissolveAlpha, _Dissolve_On);
                    clip(dissolveAlpha);

                    // ====== step.2 溶解颜色 ======
                    float edge = smoothstep(strength - _DissolveMapEdge , strength, dissolveMapFactor) ;    
                    MapLightMask = saturate(1 - edge) ;// 高亮因子（中心 0 → 中间 1 → 外层 0）
                    dissolveMapColor = lerp(0, _DissolveColor.rgb, MapLightMask) * _DissolveColorStrength;
                    
                }
                
                //PBR Color
                float3 pbrFinalColor = saturate(pbrIndirectColor + (pbrDiffuseColor + pbrSpecularColor)  * ao * shadowMap + addLightColor * ao)+ emColor;
                
                float finalAlpha = baseTexColor.a * _BaseColor.a;
                finalAlpha = lerp(baseTexColor.r * _BaseColor.a, finalAlpha, step(_RisAlpha, 0));

                //Unlit Color
                float3 unlitFinalColor = saturate(baseColor.rgb) + emColor;

                //Hollow Color
                float3 hollowFinalColor = saturate(baseColor.rgb) + emColor;
                
                //Final Color
                float3 finalColor = lerp(unlitFinalColor, pbrFinalColor, step(_Unlit, 0));
                finalColor = lerp(hollowFinalColor, finalColor, step(_Hollow, 0));

                // 溶解效果
                float3 finalColor_dDir = lerp(finalColor, dissolveMapColor, MapLightMask);
                finalColor = lerp(finalColor, finalColor_dDir, _Dissolve_On);
                
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

        Pass {
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

            #pragma vertex m_DepthOnlyVertex
            #pragma fragment m_DepthOnlyFragment
            
            struct m_Varyings
            {
                float2 uv       : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            m_Varyings m_DepthOnlyVertex(Attributes input)
            {
                // m_Varyings output = (m_Varyings)0;
                m_Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                return output;
            }
            
            half m_DepthOnlyFragment(m_Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // 特殊1. 溶解处理
                float2 dissolveUV = input.uv * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
                dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z, dissolveUV.y + dissolveUV.x * _DissolveTexFlowAndBevel.w);
                dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;
                float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                float dissolveMapFactor = dissolveMap - strength;
                // 剔除
                float dissolveAlpha = dissolveMapFactor;
                dissolveAlpha = lerp(1, dissolveAlpha, _Dissolve_On);
                clip(dissolveAlpha);

                #if defined(LOD_FADE_CROSSFADE)
                    LODFadeCrossFade(input.positionCS);
                #endif
                
                return input.positionCS.z;
            }
            
            ENDHLSL
        }
        Pass
        {
            Name "Meta"
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
