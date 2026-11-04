Shader "ShaderTemplate/Toon_SmoothNormal"
{
    Properties
    {
        _CullOff ("Two Side", Float) = 2.0
        _Transparent ("Transparent", Float) = 0.0
        _StencilOutline ("Stencil Outline", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _StencilOutlineValue("__stencilOutlineValue", Float) = 0.0
        [HideInInspector] _StencilOutlinePass("__stencilOutlinePass", Float) = 0.0
        
        [Header(BaseColor)]
        [Space(10)]
        _BaseMap ("Albedo Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMapValue("Base Map Value", Range(0, 10)) = 1
        _BaseMapSaturation("Base Map Saturation", Range(0, 5)) = 1
        _EmissionMap ("Emission Map", 2D) = "white" {}
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1
        _FresnelColor("Fresnel Color", Color) = (1, 1, 1, 1)
        _Fresnel("Fresnel", Range(0, 5)) = 0
        
        [Header(Normal)]
        [Space(10)]
        _NormalMap("Normal Map", 2D) = "bump" { }
        
        [Header(Metallic Roughness)]
        [Space(10)]
        _MaskMap ("Metallic Roughness AO Map", 2D) = "black" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _AoMap ("AO Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Roughness("Roughness", Range(0, 1)) = 0.0
        
        [Header(Shadow)]
        [Space(10)]
        _ShadowRatio("阴影浓度", Range(0.01, 1)) = 0.7
        _ShadowThreshold("阴影过渡阈值", Range(0.03, 1)) = 0.8
        
        [Header(Rim)]
        [Space(10)]
        [HDR]_RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimMin ("Rim Min", Range(0, 1)) = 0
        _RimMax ("Rim Max", Range(0, 1)) = 1
        _RimSmooth("Rim Smooth", Range(0, 1)) = 1
        _RimFac ("Rim Fac", Range(0, 100)) = 0
        
        [Header(Outline)]
        [Space(10)]
        _BaseOutlineColor ("Base Outline Color", Color) = (0, 0, 0, 1)
        _BaseOutlineOffset ("Base Outline Offset", Range(0, 2)) = 0.5
        _StencilOutlineColor ("Stencil Outline Color", Color) = (0.5, 0.5, 0.5, 1)
        _StencilOutlineOffset ("Stencil Outline Offset", Range(0, 4)) = 2
        
        [Header(Ramp)]
        [Space(10)]
        _RampSmoothness ("Ramp Smoothness", Range(0, 1)) = 0.0
        _ShadowFadeColor ("Shadow Fade Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (1, 1, 1, 1)
        _ShallowFadeColor ("Shallow Fade Color", Color) = (1, 1, 1, 1)
        _ShallowColor ("Shadow Color", Color) = (1, 1, 1, 1)
        _SSSColor ("SSS Color", Color) = (1, 1, 1, 1)
        _FrontColor ("Front Color", Color) = (1, 1, 1, 1)
        _ForwardColor ("Forward Color", Color) = (1, 1, 1, 1)
        
        [Header(Screen Space Shadow)]
        _ScreenSpaceShadowWidth ("Screen Space Shadow Width", Float) = 1
        _ScreenSpaceShadowFadeout ("Screen Space Shadow Fadeout", Float) = 1
        _ScreenSpaceShadowThreshold ("Screen Space Shadow Threshold", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }
        
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float _BaseMapValue, _BaseMapSaturation;
        float _EmissionIntensity;
        float4 _FresnelColor;
        float _Fresnel;
        
        float _Metallic, _Roughness;
        float _ShadowRatio, _ShadowThreshold;
        
        float4 _RimColor;
        float _RimFac, _RimMin, _RimMax, _RimSmooth;

        float4 _BaseOutlineColor;
        float _BaseOutlineOffset;
        float4 _StencilOutlineColor;
        float _StencilOutlineOffset;

        float _RampSmoothness;
        float3 _ShadowFadeColor;
        float3 _ShadowColor;
        float3 _ShallowFadeColor;
        float3 _ShallowColor;
        float3 _SSSColor;
        float3 _FrontColor;
        float3 _ForwardColor;

        float _ScreenSpaceShadowWidth;
        float _ScreenSpaceShadowFadeout;
        float _ScreenSpaceShadowThreshold;
        CBUFFER_END
        
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        
        ENDHLSL
        
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            Stencil
            {
                Ref [_StencilOutlineValue]
                Pass [_StencilOutlinePass]
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

            #pragma multi_compile _SCREEN_SPACE_SHADOW
            
            #pragma shader_feature _ALPHATEST_ON

            #pragma shader_feature _BaseMapOn
            #pragma shader_feature _EmissionMapOn
            #pragma shader_feature _NormalMapOn
            #pragma shader_feature _MaskMapOn
            #pragma shader_feature _RoughnessMapOn
            #pragma shader_feature _AoMapOn

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap);
            SAMPLER(sampler_MaskMap);
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
            };

            float3 IndirectDiffuse( float2 uvStaticLightmap, float3 normalWS )
            {
                #ifdef LIGHTMAP_ON
                return SampleLightmap( uvStaticLightmap, normalWS );
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
            float D_Function(float NdotH, float roughness)
            {
                float a2 = roughness * roughness;
                float NdotH2 = NdotH * NdotH;
                float d = (NdotH2 * (a2 - 1) + 1);
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
            float3 IndirectSpeCube(float3 reflectDir, float3 worldPos, float2 scrPos, float roughness)
            {
                //roughness = roughness * (1.7 - 0.7 * roughness); // unity 内部不是线性 调整下 拟合曲线求近似，可以再 GGB 可视化曲线
                //float mipmapLevel = roughness * 6; // 把粗糙度 remap 到 0~6 的 7个阶段，然后进行 texture lod 采样
                float3 specularColor = GlossyEnvironmentReflection(reflectDir, worldPos, roughness, 1.0, scrPos);
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
                float4 baseTexColor = _BaseColor;
                #if _BaseMapOn
                    baseTexColor *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                #endif
                float3 valueBaseTexColor = baseTexColor * _BaseMapValue;
                float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                baseTexColor.xyz = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation);
                float3 worldNormal = 0;
                #if _NormalMapOn
                float3 normalTexColor = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                float3 worldBitTangent = cross(IN.worldNormal, IN.worldTangent);
                float3x3 tbn = float3x3(IN.worldTangent, worldBitTangent, IN.worldNormal);
                worldNormal = normalize(mul(normalTexColor, tbn));
                #else
                worldNormal = normalize(IN.worldNormal);
                #endif

                float3 emColor = 0;
                float ao = 1;
                float metallic = _Metallic;
                float roughness = _Roughness;
                float smoothness = 0;

                #if _EmissionMapOn
                emColor = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv) * _EmissionIntensity;
                #endif
                
                #if _MaskMapOn
                float4 maskTexColor = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, IN.uv);
                metallic = maskTexColor.r;
                roughness = maskTexColor.g + _Roughness;
                ao = smoothstep(0, 0.6, maskTexColor.b);
                #elif  _RoughnessMapOn
                float roughnessTexColor = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, IN.uv).x;
                roughness = roughnessTexColor + _Roughness;
                #endif
                smoothness = 1 - roughness;
                roughness = pow(roughness, 2);

                #if _AoMapOn
                float aoTexColor = SAMPLE_TEXTURE2D(_AoMap, sampler_AoMap, IN.uv).x;
                ao = smoothstep(0, 1, aoTexColor);
                #endif
                //return ao;

                //ShadowMask
                //阴影遮罩
                //烘焙时额外烘焙的一张贴图，用作烘焙阴影
                //记录了阴影信息，当在Distance范围内时使用实时阴影，在范围外使用烘焙阴影
                
                float4 shadowMask = float4(1, 1, 1, 1);
                float4 rawShadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH);
                #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
                shadowMask = rawShadowMask;
                #elif !defined (LIGHTMAP_ON)
                shadowMask = unity_ProbesOcclusion;
                #else
                shadowMask = half4(1, 1, 1, 1);
                #endif

                //Content
                //一些变量
                float2 scrPos = IN.scrPos.xy / IN.scrPos.w;
                float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
                Light light = GetMainLight(shadowCoord, worldPos, shadowMask);
                float3 lightColor = light.color;
                float3 lightDir = light.direction;
                float3 viewDir = normalize(GetWorldSpaceViewDir(worldPos));
                float3 viewNormal = mul(unity_WorldToCamera, float4(IN.worldNormal, 0)).xyz;
                float3 reflectDir = reflect(-normalize(viewDir), worldNormal);

                //正常的MatcapUV
                float2 matcapUV = (viewNormal * 0.5 + 0.5).xy;
                
                float nol = saturate(max(dot(worldNormal, lightDir), 1e-5));
                float lambert = dot(worldNormal, lightDir);
                float halfLambert = lambert * 0.5 + 0.5;
                float lambertStep = smoothstep(0.423, 0.465, halfLambert);
                float3 halfDir = normalize(viewDir + lightDir);
                float noh = saturate(max(dot(worldNormal, halfDir), 1e-5));

                float nov = saturate(max(dot(worldNormal, viewDir), 1e-5));
                float hol = saturate(max(dot(halfDir, lightDir), 1e-5));

                //Screen Space Shadow
                float screenSpaceShadowAttenuation = 1;
                {
                    #if _SCREEN_SPACE_SHADOW
                    {
                        float linearEyeDepth = IN.position.w;
                        float perspective = 1.0 / linearEyeDepth;
                        float offsetMul = _ScreenSpaceShadowWidth * 5.0 * perspective / 100.0;
                        
                        float3 lightDirectionVS = normalize(TransformWorldToViewDir(lightDir));
                        float2 offset = lightDirectionVS.xy * offsetMul;
                        int2 coord = IN.position.xy + offset * _ScaledScreenParams.xy;
                        coord = min(max(0, coord), _ScaledScreenParams.xy - 1);
                        float offsetSceneDepth = LoadSceneDepth(coord);
                        float offsetSceneLinearEyeDepth = LinearEyeDepth(offsetSceneDepth, _ZBufferParams);
                        float fadeout = max(1e-5, _ScreenSpaceShadowFadeout);
                        screenSpaceShadowAttenuation = saturate((offsetSceneLinearEyeDepth - (linearEyeDepth - _ScreenSpaceShadowThreshold)) * 8 / fadeout);
                    }
                    #endif
                }

                //Ramp
                float3 rampColor = 0;
                {
                    _RampSmoothness = max(1e-5, _RampSmoothness);
                    float rampAttenuation = lambert * 1.5;
                    float s0 = _RampSmoothness * 1.5;
                    float s1 = 1 - s0;
                    
                    float rampShadowFade = 1;
                    float rampShadow = 1;
                    float rampShallowFade = 1;
                    float rampShallow = 1;
                    float rampSSS = 1;
                    float rampFront = 1;
                    float rampForward = 1;

                    float rampValue[6] =
                        {
                            (rampAttenuation + 1.5) / s1 + 0.0,
                            (rampAttenuation + 0.5) / s0 + 0.5,
                            (rampAttenuation + 0.0) / s1 + 0.5,
                            (rampAttenuation - 0.5) / s0 + 0.5,
                            (rampAttenuation - 0.5) / s0 - 0.5,
                            (rampAttenuation - 2.0) / s1 + 1.5,
                        };
                    
                    rampShadowFade = saturate(1 - rampValue[0]);
                    rampShadow = saturate(min(1 - rampValue[1], rampValue[0]));
                    rampShallowFade = saturate(min(1 - rampValue[2], rampValue[1]));
                    rampShallow = saturate(min(1 - rampValue[3], rampValue[2]));
                    rampSSS = saturate(min(1 - rampValue[4], rampValue[3]));
                    rampFront = saturate(min(1 - rampValue[5], rampValue[4]));
                    rampForward = saturate(rampValue[5]);

                    float screenSpaceShadowRampValue[2] = {
                        2 * screenSpaceShadowAttenuation, 2 * screenSpaceShadowAttenuation - 1
                    };
                    rampShallowFade *= saturate(screenSpaceShadowRampValue[0]);
                    rampShallowFade += (1 - rampShadowFade - rampShadow) * saturate(1 - screenSpaceShadowRampValue[0]);
                    rampShallow *= saturate(min(screenSpaceShadowRampValue[0], 1 - screenSpaceShadowRampValue[1])) + saturate(screenSpaceShadowRampValue[1]);
                    rampSSS *= saturate(min(screenSpaceShadowRampValue[0], 1 - screenSpaceShadowRampValue[1])) + saturate(screenSpaceShadowRampValue[1]);
                    rampSSS += (rampFront + rampForward) * saturate(min(screenSpaceShadowRampValue[0], 1 - screenSpaceShadowRampValue[1]));
                    rampFront *= saturate(screenSpaceShadowRampValue[1]);
                    rampForward *= saturate(screenSpaceShadowRampValue[1]);
                    

                    rampColor = (rampForward * _ForwardColor + rampFront * _FrontColor + rampSSS * _SSSColor) * lightColor;
                    rampColor += (rampShadowFade * _ShadowFadeColor + rampShadow * _ShadowColor + rampShallowFade * _ShallowFadeColor + rampShallow * _ShallowColor) * lightColor;
                }
                

                //Gamma
                float3 gammaColor = baseTexColor;
                {
                    float luminance = Luminance(baseTexColor);
                    float fac = lambert * ao;
                    float gamma = lerp(luminance * 0.2875 + 1.4375, 1, fac);

                    gammaColor = pow(max(1e-5, baseTexColor), gamma);
                    float3 gammaColorHalf = lerp(baseTexColor, gammaColor, 0.5);
                    gammaColor = lerp(gammaColorHalf, gammaColor, fac);
                }

                //PBR
                float3 pbrDiffuseColor = lerp(0.96 * gammaColor, 0, metallic);
                float3 pbrSpecularColor = lerp(0.04, gammaColor, metallic);
                {
                    
                }

                //Ambient
                float3 ambientColor = gammaColor;
                {
                    ambientColor = SampleSH(worldNormal) * gammaColor;
                }
                
                float3 finalColorOutput = pbrDiffuseColor * rampColor
                                    + ambientColor;
                return float4(finalColorOutput, 1);
                

                //Shadow
                float shadow = light.shadowAttenuation;
                float shadowRamp = smoothstep(0, _ShadowThreshold, nol);
                shadow *= shadowRamp;

                //LightMap
                //烘焙贴图 或 光照探针贴图
                float3 lightmapColor = IndirectDiffuse(IN.lightmapUVOrVertexSH.xy, worldNormal);
                MixRealtimeAndBakedGI(light, worldNormal, lightmapColor);

                //BaseColor
                float3 baseColor = baseTexColor;
                baseColor = _ShadowRatio * baseColor * shadow + (1 - _ShadowRatio) * baseColor;
                
                //Diffuse
                float3 F0 = F0_Function(baseColor, metallic);
                float3 Direct_F = F_Function(hol, F0);
                float3 Ks = Direct_F;
                float3 Kd = (1 - Ks) * (1 - metallic);
                float3 diffuseColor = Kd * baseColor * lightColor * nol / PI;
                
                //Specular
                float Direct_D = D_Function(noh, roughness);
                float Direct_G = G_Function(nol, nov, roughness);
                
                float3 BRDFSpecSection = Direct_D * Direct_G * Direct_F / (4 * nol * nov);
                float3 DirectSpecColor = Ks * BRDFSpecSection * lightColor * nol;
                
                float3 specularColor = DirectSpecColor;

                float3 albedo = diffuseColor + specularColor;
                
                //多光源
                //使用Forward+的写法
                uint addLightsCount = GetAdditionalLightsCount();
                float nol_all = nol;
                float3 pointLightColor = 0;
                float3 BRDFSpecSection_AllAdd = 0;
                InputData inputData = InitializeInputData(IN);
                LIGHT_LOOP_BEGIN(addLightsCount)
                {
                    //Context
                    Light addLight = GetAdditionalLight(lightIndex, worldPos, shadowMask);
                    float3 lightDir_Add = addLight.direction;
                    float3 lightColor_Add = addLight.color;
                    float3 halfDir_Add = normalize(lightDir_Add + viewDir);
                    float hol_Add = saturate(max(dot(halfDir_Add, lightDir_Add), 1e-5));
                    float nol_Add = saturate(max(dot(worldNormal, lightDir_Add), 1e-5));
                    nol_all += nol_Add;
                    float noh_Add = saturate(max(dot(worldNormal, halfDir_Add), 1e-5));
                    float shadow_Add = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    float shadowRamp_Add = smoothstep(0, _ShadowThreshold, nol_Add);
                    //shadow_Add *= shadowRamp_Add;
                    float3 baseColor_Add = baseTexColor;
                    //Diffuse
                    float3 F0_Add = F0_Function(baseColor_Add, metallic);
                    float3 Direct_F_Add = F_Function(hol_Add, F0_Add);
                    float3 Ks_Add = Direct_F_Add;
                    float3 Kd_Add = (1 - Ks_Add) * (1 - metallic);
                    float3 diffuse_Add = Kd_Add * baseColor_Add * lightColor_Add * shadow_Add * nol_Add;

                    //Specular
                    float Direct_D_Add = D_Function(noh_Add, roughness);
                    float Direct_G_Add = G_Function(nol_Add, nov, roughness);
                    float3 BRDFSpecSection_Add = Direct_D_Add * Direct_G_Add * Direct_F_Add / (4 * nol_Add * nov);
                    BRDFSpecSection_AllAdd += BRDFSpecSection_Add;
                    float3 specular_Add = Ks_Add * BRDFSpecSection_Add * lightColor_Add * shadow_Add * nol_Add;
                    pointLightColor += diffuse_Add + specular_Add;
                }
                LIGHT_LOOP_END

                //Indirect
                float3 Indirect_Ks = Indirect_F_Function(nov, F0, roughness);
                float3 Indirect_Kd = (1 - Indirect_Ks) * (1 - metallic);
                float3 IndirectDiffColor = lightmapColor * Indirect_Kd * baseColor;
                

                float3 IndirectSpeCubeColor = IndirectSpeCube(reflectDir, worldPos, scrPos, roughness);
                float3 IndirectSpeCubeFactor = IndirectSpeFactor(roughness, smoothness, BRDFSpecSection, F0, nov);
                float3 IndirectSpeColor = IndirectSpeCubeColor * IndirectSpeCubeFactor;

                float3 IndirectColor = IndirectDiffColor + IndirectSpeColor;

                float3 finalColor = albedo + pointLightColor + IndirectColor;
                finalColor *= ao;
                
                //Rim
                float fresnel = 1 - saturate(nov);
                float rim = smoothstep(_RimMin, _RimMax, fresnel);
                rim = smoothstep(0, _RimSmooth, rim);
                float3 rimBRDFSpecSection = (BRDFSpecSection + BRDFSpecSection_AllAdd) * 4;
                finalColor += _RimColor * _RimFac * baseColor * light.color * rim * rimBRDFSpecSection;

                finalColor += _FresnelColor * _Fresnel * smoothstep(0.659, 1, 1 - nov);

                finalColor += emColor;
                
                return float4(finalColor, baseTexColor.a);
            }
            
            ENDHLSL
        }

        Pass
        {
            Name "DrawBaseOutline"
            Tags { "LightMode" = "DrawBaseOutline" }
            
            Stencil
            {
                Ref [_StencilOutlineValue]
                Pass [_StencilOutlinePass]
            }
            
            Cull Front
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 uv7 : TEXCOORD7;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float3 OctToUnitVector(float2 oct)
            {
                float3 N = float3(oct, 1 - dot(1, abs(oct)));
                float t = max(-N.z, 0);
                N.x += N.x >= 0 ? (-t) : t;
                N.y += N.y >= 0 ? (-t) : t;
                return normalize(N);
            }
            
            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                float4 clipPos = vertex_position_inputs.positionCS;
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal, IN.tangent);
                float2 oct =  IN.uv1;
                float3 smoothNormal = OctToUnitVector(oct);
                float3x3 tbn = float3x3(vertex_normal_inputs.tangentWS, vertex_normal_inputs.bitangentWS, vertex_normal_inputs.normalWS);
                smoothNormal = mul(smoothNormal, tbn);
                float3 worldNormal = mul(IN.uv7.xyz, tbn);
                worldNormal = smoothNormal;
                float3 viewNormal = TransformWorldToViewNormal(worldNormal);
                float3 ndcNormal = normalize(TransformWViewToHClip(viewNormal.xyz)) * clamp(clipPos.w, 1.0, 5.0);
                float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
                float aspect = abs(nearUpperRight.y / nearUpperRight.x);
                ndcNormal.x *= aspect;
                clipPos.xy += 0.01 * _BaseOutlineOffset * ndcNormal.xy;
                OUT.position = clipPos;
                OUT.uv = IN.uv;
                return OUT;
            }
            float4 frag(v2f IN) : SV_Target
            {
                float4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                return baseTexColor * _BaseOutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DrawStencilOutline"
            Cull Front
            Stencil
            {
                Ref 1
                Comp NotEqual
            }
            
            Tags
            {
                "LightMode" = "DrawStencilOutline"
            }
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv1 : TEXCOORD1;
                float4 uv7 : TEXCOORD7;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
            };

            float3 OctToUnitVector(float2 oct)
            {
                float3 N = float3(oct, 1 - dot(1, abs(oct)));
                float t = max(-N.z, 0);
                N.x += N.x >= 0 ? (-t) : t;
                N.y += N.y >= 0 ? (-t) : t;
                return normalize(N);
            }

            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                float4 clipPos = vertex_position_inputs.positionCS;
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal, IN.tangent);
                float3 smoothNormal =  OctToUnitVector(IN.uv1);
                float3x3 tbn = float3x3(vertex_normal_inputs.tangentWS, vertex_normal_inputs.bitangentWS, vertex_normal_inputs.normalWS);
                smoothNormal = mul(smoothNormal, tbn);
                float3 worldNormal = mul(IN.uv7.xyz, tbn);
                worldNormal = smoothNormal;
                float3 viewNormal = TransformWorldToViewNormal(worldNormal);
                float3 ndcNormal = normalize(TransformWViewToHClip(viewNormal.xyz)) * clamp(clipPos.w, 1.0, 5.0);
                float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
                float aspect = abs(nearUpperRight.y / nearUpperRight.x);
                ndcNormal.x *= aspect;
                clipPos.xy += 0.01 * _StencilOutlineOffset * ndcNormal.xy;
                OUT.position = clipPos;
                
                return OUT;
            }

            float4 frag(v2f IN): SV_Target
            {
                return _StencilOutlineColor;
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
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }
            real4 frag(v2f i) : SV_Target
            {
                #if _ALPHATEST_ON
                float4 col = tex2D(_MainTex, i.uv);
                clip(col.a - 0.001);
                #endif
                return 0;
            }
            ENDHLSL
        }

        

        Pass 
        {
            Name "DepthNormals"
            Tags {
                "LightMode" = "DepthNormals"
            }
            
            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
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
                float4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                MetaInput metaInput;
                metaInput.Albedo = baseTexColor * _BaseColor;
                metaInput.Emission = baseTexColor * _BaseColor;
                return UniversalFragmentMeta(i, metaInput);
            }
            ENDHLSL
        }

    }
    CustomEditor "ToonGUI"
}
