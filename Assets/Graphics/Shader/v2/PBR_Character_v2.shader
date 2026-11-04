Shader "ShaderTemplate/PBR_Character_V2"
{
    Properties
    {
        _CullOff ("Two Side", Float) = 2.0
        _Transparent ("Transparent", Float) = 0.0
        _StencilOutline ("Stencil Outline", Float) = 0.0
        _ScreenSpaceShadow ("Screen Space Shadow", Float) = 0.0
        _Pre_SSS("Pre SSS", Float) = 0.0
        _AnisoOn ("Aniso On", Float) = 0.0
        _Unlit ("Unlit", Float) = 0.0
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
        [HDR]_BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMapValue("Base Map Value", Range(0, 10)) = 1
        _BaseMapSaturation("Base Map Saturation", Range(0, 5)) = 1
        _BaseColorIntensity ("Base Color Intensity", Range(0, 5)) = 1
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1
        _FresnelColor("Fresnel Color", Color) = (1, 1, 1, 1)
        _Fresnel("Fresnel", Range(0, 5)) = 0
        
        [Header(Normal)]
        [Space(10)]
        _NormalMap("Normal Map", 2D) = "bump" { }
        
        [Header(Metallic Roughness)]
        [Space(10)]
        _MaskMap ("Metallic Roughness AO Map", 2D) = "black" {}
        _MetallicMap ("Metallic Map", 2D) = "black" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _AoMap ("AO Map", 2D) = "white" {}
        _Metallic("Metallic", Range(-1, 1)) = 0.0
        _Roughness("Roughness", Range(-1, 1)) = 0.0
        
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
        _ShallowColor ("Shallow Color", Color) = (1, 1, 1, 1)
        _SSSColor ("SSS Color", Color) = (1, 1, 1, 1)
        _FrontColor ("Front Color", Color) = (1, 1, 1, 1)
        _ForwardColor ("Forward Color", Color) = (1, 1, 1, 1)
        
        [Header(Screen Space Shadow)]
        _ScreenSpaceShadowWidth ("Screen Space Shadow Width", Float) = 0.2
        _ScreenSpaceShadowFadeout ("Screen Space Shadow Fadeout", Float) = 0.2
        _ScreenSpaceShadowThreshold ("Screen Space Shadow Threshold", Range(0, 1)) = 0.015
        
        [Header(SDF)]
        _SDFMap ("SDF Map", 2D) = "black" {}
        _SDFMaskMap ("SDF Mask Map", 2D) = "black" {}
        _NeckBaseColor ("NecK Base Color", Color) = (1, 1, 1, 1)
        _SDFAoMap ("SDF AO Map", 2D) = "white" {}
        _NoseMaskMap ("Nose Mask Map", 2D) = "black" {}
        
        _SDFRampSmoothness ("SDF Ramp Smoothness", Range(0, 1)) = 0.0
        _SDFShadowFadeColor ("SDF Shadow Fade Color", Color) = (1, 1, 1, 1)
        _SDFShadowColor ("SDF Shadow Color", Color) = (1, 1, 1, 1)
        _SDFShallowFadeColor ("SDF Shallow Fade Color", Color) = (1, 1, 1, 1)
        _SDFShallowColor ("SDF Shadow Color", Color) = (1, 1, 1, 1)
        _SDFScreenSpaceShadowWidth ("SDF Screen Space Shadow Width", Float) = 0.2
        _SDFScreenSpaceShadowFadeout ("SDF Screen Space Shadow Fadeout", Float) = 0.2
        _SDFScreenSpaceShadowThreshold ("SDF Screen Space Shadow Threshold", Range(0, 1)) = 0.015
        
        [Header(Aniso)]
        _AnisoTint ("Aniso Tint", Color) = (1, 1, 1, 1)
        _RoughnessX ("Roughness X", Range(0, 1)) = 0.5
        _RoughnessY ("Roughness Y", Range(0, 1)) = 0.5
        _IOR ("IOR", Range(1.0, 30.0)) = 1.5
        
        [Header(SSS)]
        _ThicknessMap ("Thickness Map", 2D) = "black" {}
        _ScatterNormalDistortion ("Normal Distortion", Range(0, 5)) = 1
        _ScatterPower ("Scatter Power", Range(0, 5)) = 1
        _ScatterScale ("Scatter Scale", Range(0, 10)) = 1
        _ScatterColor ("Scatter Color", Color) = (1, 1, 1, 1)
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
        float4 _BaseColor;
        float _BaseMapValue, _BaseMapSaturation, _BaseColorIntensity;
        float _EmissionIntensity;
        float3 _EmissionColor;
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

        float3 _HeadForward;
        float3 _HeadCenter;
        float3 _HeadRight;
        float _SDFRampSmoothness;
        float4 _NeckBaseColor;
        float3 _SDFShadowFadeColor;
        float3 _SDFShadowColor;
        float3 _SDFShallowFadeColor;
        float3 _SDFShallowColor;
        float _SDFScreenSpaceShadowWidth;
        float _SDFScreenSpaceShadowFadeout;
        float _SDFScreenSpaceShadowThreshold;

        float4 _AnisoTint;
        float _RoughnessX;
        float _RoughnessY;
        float _IOR;

        float _ScatterNormalDistortion;
        float _ScatterPower;
        float _ScatterScale;
        float3 _ScatterColor;
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
            
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _Unlit_ON
            #pragma shader_feature_local _SCREEN_SPACE_SHADOW
            #pragma shader_feature_local _PRE_SSS
            #pragma shader_feature_local _ANISO_ON
            #pragma shader_feature_local _BaseMapOn
            #pragma shader_feature_local _EmissionMapOn
            #pragma shader_feature_local _NormalMapOn
            #pragma shader_feature_local _MaskMapOn
            #pragma shader_feature_local _RoughnessMapOn
            #pragma shader_feature_local _AoMapOn
            #pragma shader_feature_local _MetallicMapOn
            #pragma shader_feature_local _SDFMapOn

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
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
            TEXTURE2D(_SDFMap);
            SAMPLER(sampler_SDFMap);
            TEXTURE2D(_SDFMaskMap);
            SAMPLER(sampler_SDFMaskMap);
            TEXTURE2D(_SDFAoMap);
            SAMPLER(sampler_SDFAoMap);
            TEXTURE2D(_NoseMaskMap);
            SAMPLER(sampler_NoseMaskMap);
            TEXTURE2D(_ThicknessMap);
            SAMPLER(sampler_ThicknessMap);
            
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

            //Anisotropy
            //菲涅尔项
            float fresnel(float3 I,float3 N,float IOR1,float IOR2){
                float R0 = pow( (IOR1-IOR2) / (IOR1 + IOR2) , 2 );
                float cos_theta = dot(I,N);
                return R0 + (1-R0)*pow(1-cos_theta,5);
            }
            //各向异性的GGX函数，这里直接搬了UE4的代码实现
            float DistributionGGX_Aniso( float ax, float ay, float NoH, float3 H, float3 X, float3 Y )
            {
                float XoH = dot( X, H );
                float YoH = dot( Y, H );
                float d = XoH*XoH / (ax*ax) + YoH*YoH / (ay*ay) + NoH*NoH;
                return 1 / ( PI * ax*ay * d*d );
            }
            //G1
            float smithG_GGX_aniso(float dotVN, float dotVX, float dotVY, float ax, float ay)
            {
                return 1.0 / (1.0 + sqrt( (pow(dotVX * ax, 2.0) + pow(dotVY * ay, 2.0)) / pow(dotVN, 2.0)));
            }
            //G2
            float GeometrySmith_aniso(float3 N, float3 V, float3 L, float roughnessX,float roughnessY,float3 X,float3 Y)
            {
                float dotVN = dot(N,V);
                float dotVX = dot(V,X);
                float dotVY = dot(V,Y);
                float dotLN = dot(L,N);
                float dotLX = dot(L,X);
                float dotLY = dot(L,Y);
                float ggx2 = smithG_GGX_aniso(dotVN,dotVX,dotVY,roughnessX,roughnessY);
                float ggx1 = smithG_GGX_aniso(dotLN,dotLX,dotLY,roughnessX,roughnessY);
                if(dotVN < 0.0 || dotLN < 0.0) return 0.0;
                return ggx1 * ggx2;
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
                OUT.worldSmoothNormal = IN.uv7;
                OUTPUT_LIGHTMAP_UV(IN.lightMapUV, unity_LightmapST, OUT.lightmapUVOrVertexSH.xy);
                OUTPUT_SH(OUT.worldNormal, OUT.lightmapUVOrVertexSH.xyz);
                return OUT;
            }

            float4 frag(v2f IN): SV_Target
            {
                float3 worldPos = IN.worldPos;
                
                //BaseColor
                float4 baseTexColor = _BaseColor;
                float sdfMask = 1;
                #if _SDFMapOn
                    sdfMask = SAMPLE_TEXTURE2D(_SDFMaskMap, sampler_SDFMaskMap, IN.uv).x;
                    if (sdfMask > 0.95) baseTexColor = _NeckBaseColor;
                #endif
                {
                    #if _BaseMapOn
                        baseTexColor *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                    #endif
                        float3 valueBaseTexColor = baseTexColor * _BaseMapValue;
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
                float3 emColor = _EmissionColor * _EmissionIntensity;
                float ao = 1;
                float metallic = saturate(_Metallic);
                float roughness = saturate(_Roughness);
                float smoothness = 0;
                {
                    #if _EmissionMapOn
                        emColor = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv) * _EmissionColor * _EmissionIntensity;
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
                    roughness = saturate(pow(roughness, 2));

                    #if _AoMapOn
                        float aoTexColor = SAMPLE_TEXTURE2D(_AoMap, sampler_AoMap, IN.uv).x;
                        ao = smoothstep(0, 1, aoTexColor);
                    #endif
                }

                //ShadowMask
                float4 shadowMask = float4(1, 1, 1, 1);
                {
                    float4 rawShadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH);
                    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
                        shadowMask = rawShadowMask;
                    #elif !defined (LIGHTMAP_ON)
                        shadowMask = unity_ProbesOcclusion;
                    #else
                        shadowMask = half4(1, 1, 1, 1);
                    #endif
                }

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
                    shadowMap = 1 + (shadowMap - _ShadowRatio);
                }

                //LightMap
                //烘焙贴图 或 光照探针贴图
                float3 ambientColor = 0;
                {
                    ambientColor = IndirectDiffuse(IN.lightmapUVOrVertexSH.xy, worldNormal);
                    MixRealtimeAndBakedGI(light, worldNormal, ambientColor);
                }
                
                //SDF
                float sdf = 1;
                {
                    #if _SDFMapOn
                        
                        if (sdfMask < 0.95)
                        {
                            float3 headForward = normalize(_HeadForward - _HeadCenter);
                            float3 headRight = normalize(_HeadRight - _HeadCenter);
                            float3 headUp = normalize(cross(headRight, headForward));

                            float3 lightDirectionProjHeadWS = normalize(lightDir - dot(lightDir, headUp) * headUp);

                            float sX = dot(lightDirectionProjHeadWS, headRight);
                            float sZ = dot(lightDirectionProjHeadWS, -headForward);

                            float angleThreshold = atan2(sX, sZ) / (2 * PI) + 0.5;

                            float exposeRight = smoothstep(0.45, 0.55, angleThreshold);
                            float value_R = pow(saturate((angleThreshold - 0.5) * 2), 3);
                            float value_L = pow(saturate(1 - angleThreshold * 2), 3);
                            float mixThreshold = lerp(value_L, value_R, exposeRight);
                        
                            float2 angleUV = IN.uv;
                            float sdfLeft = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, float2(1 - angleUV.x, angleUV.y)).x;
                            sdfLeft = smoothstep(0, 1, sdfLeft);
                            float sdfRight = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, angleUV).x;
                            sdfRight = smoothstep(0, 1, sdfRight);
                            float mixSDF = lerp(sdfLeft, sdfRight, exposeRight);
                            sdf = saturate(mixThreshold - mixSDF);
                        }
                        else
                        {
                            ao = SAMPLE_TEXTURE2D(_SDFAoMap, sampler_SDFAoMap, IN.uv).x;
                            ao = smoothstep(0, 1, ao);
                            _RampSmoothness = _SDFRampSmoothness;
                            _ShadowFadeColor = _SDFShadowFadeColor;
                            _ShadowColor = _SDFShadowColor;
                            _ShallowFadeColor = _SDFShallowFadeColor;
                            _ShallowColor = _SDFShallowColor;
                            _ScreenSpaceShadowWidth = _SDFScreenSpaceShadowWidth;
                            _ScreenSpaceShadowFadeout = _SDFScreenSpaceShadowFadeout;
                            _ScreenSpaceShadowThreshold = _SDFScreenSpaceShadowThreshold;
                        }
                        //sdf = smoothstep(-1, 2, sdf);
                        // angleThreshold = angleThreshold > 0 ? (1 - angleThreshold) : (1 + angleThreshold);
                        //
                        // if (dot(lightDirectionProjHeadWS, headRight) > 0)
                        // {
                        //     angleUV.x = 1.0 - angleUV.x;
                        // }
                        // sdf = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, angleUV);
                        // sdf = smoothstep(0, 1, saturate(sdf - angleThreshold));
                    #endif
                }

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

                        float noseMask = SAMPLE_TEXTURE2D(_NoseMaskMap, sampler_NoseMaskMap, IN.uv).x;
                        if (noseMask > 0) screenSpaceShadowAttenuation = 1;
                    }
                    #endif
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

                float3 lightScattering = 0;
                {
                    #if _PRE_SSS
                        float3 translightDir = lightDir + worldNormal * _ScatterNormalDistortion;
                        float scattering = SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, IN.uv).x;  
                        float transDot = pow(saturate(dot(-translightDir, viewDir)) * scattering * scattering, _ScatterPower) * _ScatterScale;
                        lightScattering = transDot * _ScatterColor;
                    #endif
                }

                //Ramp
                float3 rampColor = 0;
                {
                    _RampSmoothness = max(1e-5, _RampSmoothness);
                    float rampAttenuation = 1.5;
                    #if _SDFMapOn
                        if (sdfMask < 0.95) rampAttenuation *= sdf;
                        else rampAttenuation *= lambert;
                    #else
                        rampAttenuation *= lambert;
                    #endif
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
                        2 * (shadowMap * screenSpaceShadowAttenuation), 2 * (shadowMap * screenSpaceShadowAttenuation) - 1
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

                //PBR
                float3 pbrDiffuseColor = 0;
                float3 pbrSpecularColor = 0;
                float3 pbrIndirectColor = 0;
                float3 pbrIndirectDiffColor = 0;
                float3 pbrIndirectSpeColor = 0;
                float3 pbrBRDFSpecSection = 0;
                {
                    float3 F0 = F0_Function(gammaColor, metallic);
                    //Diffuse
                    float3 Direct_F = F_Function(hol, F0);
                    float3 Ks = Direct_F;
                    float3 Kd = (1 - Ks) * (1 - metallic);
                    pbrDiffuseColor = Kd * lightColor * nol / PI;
                    
                    #if _ANISO_ON
                        //Specular
                        float F = fresnel(lightDir, worldNormal, 1.0, _IOR);
                        float G = GeometrySmith_aniso(worldNormal, viewDir, lightDir, _RoughnessX, _RoughnessY, IN.worldTangent, worldBitTangent);
                        float D = DistributionGGX_Aniso(_RoughnessX, _RoughnessY, noh, halfDir, IN.worldTangent, worldBitTangent);

                        pbrBRDFSpecSection = D * G * F / (4 * nol * nov);
                        pbrSpecularColor = Ks * pbrBRDFSpecSection * lightColor * nol * PI;
                    #else
                        //Specular
                        float Direct_D = D_Function(noh, roughness);
                        float Direct_G = G_Function(nol, nov, roughness);
                        pbrBRDFSpecSection = Direct_D * Direct_G * Direct_F / (4 * nol * nov);
                        
                        pbrSpecularColor = Ks * pbrBRDFSpecSection * lightColor * nol;
                    #endif

                    //Indirect
                    float3 Indirect_Ks = Indirect_F_Function(nov, F0, roughness);
                    float3 Indirect_Kd = (1 - Indirect_Ks) * (1 - metallic);
                    pbrIndirectDiffColor = ambientColor * Indirect_Kd;
                    
                    float3 IndirectSpeCubeColor = IndirectSpeCube(reflectDir, worldPos, scrPos, roughness);
                    float3 IndirectSpeCubeFactor = IndirectSpeFactor(roughness, smoothness, pbrBRDFSpecSection, F0, nov);
                    pbrIndirectSpeColor = IndirectSpeCubeColor * IndirectSpeCubeFactor;

                    pbrIndirectColor = pbrIndirectDiffColor + pbrIndirectSpeColor;
                }
                
                //多光源
                float3 addLightColor = 0;
                float nol_addAll = nol;
                float3 BRDFSpecSection_addAll = 0;
                {
                    uint addLightsCount = GetAdditionalLightsCount();
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
                        nol_addAll += nol_Add;
                        float noh_Add = saturate(max(dot(worldNormal, halfDir_Add), 1e-5));
                        float shadow_Add = addLight.distanceAttenuation * addLight.shadowAttenuation;

                        //Ramp
                        float3 addRampColor = 0;
                        {
                            _RampSmoothness = max(1e-5, _RampSmoothness);
                            float rampAttenuation = nol_Add * 1.5;
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
                                2 * shadow_Add, 2 * shadow_Add - 1
                            };
                            rampShallowFade *= saturate(screenSpaceShadowRampValue[0]);
                            rampShallowFade += (1 - rampShadowFade - rampShadow) * saturate(1 - screenSpaceShadowRampValue[0]);
                            rampShallow *= saturate(min(screenSpaceShadowRampValue[0], 1 - screenSpaceShadowRampValue[1])) + saturate(screenSpaceShadowRampValue[1]);
                            rampSSS *= saturate(min(screenSpaceShadowRampValue[0], 1 - screenSpaceShadowRampValue[1])) + saturate(screenSpaceShadowRampValue[1]);
                            rampSSS += (rampFront + rampForward) * saturate(min(screenSpaceShadowRampValue[0], 1 - screenSpaceShadowRampValue[1]));
                            rampFront *= saturate(screenSpaceShadowRampValue[1]);
                            rampForward *= saturate(screenSpaceShadowRampValue[1]);
                            
                            addRampColor = (rampForward * _ForwardColor + rampFront * _FrontColor + rampSSS * _SSSColor) * lightColor;
                            addRampColor += (rampShadowFade * _ShadowFadeColor + rampShadow * _ShadowColor + rampShallowFade * _ShallowFadeColor + rampShallow * _ShallowColor) * lightColor;
                        }
                        
                        float3 baseColor_Add = gammaColor;
                        //Diffuse
                        float3 F0_Add = F0_Function(baseColor_Add, metallic);
                        float3 Direct_F_Add = F_Function(hol_Add, F0_Add);
                        float3 Ks_Add = Direct_F_Add;
                        float3 Kd_Add = (1 - Ks_Add) * (1 - metallic);
                        float3 diffuse_Add = Kd_Add * lightColor_Add * nol_Add;

                        //Specular
                        float Direct_D_Add = D_Function(noh_Add, roughness);
                        float Direct_G_Add = G_Function(nol_Add, nov, roughness);
                        float3 BRDFSpecSection_Add = Direct_D_Add * Direct_G_Add * Direct_F_Add / (4 * nol_Add * nov);
                        BRDFSpecSection_addAll += BRDFSpecSection_Add;
                        float3 specular_Add = Ks_Add * BRDFSpecSection_Add * lightColor_Add * nol_Add;
                        addLightColor += (diffuse_Add + specular_Add) * shadow_Add * addRampColor;
                    }
                    LIGHT_LOOP_END
                }

                //Rim
                float3 rimColor = 0;
                {
                    float fresnel = 1 - saturate(nov);
                    float rim = smoothstep(_RimMin, _RimMax, fresnel);
                    rim = smoothstep(0, _RimSmooth, rim);
                    float3 rimBRDFSpecSection = (pbrBRDFSpecSection + BRDFSpecSection_addAll) * 4;
                    rimColor += _RimColor * _RimFac * light.color * rim * rimBRDFSpecSection;
                    rimColor += _FresnelColor * _Fresnel * smoothstep(0.659, 1, 1 - nov);
                }
                
                //return float4(rampColor, 1);
                float3 pbrSection = (pbrDiffuseColor + pbrSpecularColor + addLightColor) * gammaColor * rampColor;
                pbrSection = lerp(pbrSection, gammaColor * rampColor, 0.7);

                #if _SDFMapOn
                {
                    if (sdfMask < 0.95)
                    {
                        ao = saturate(ao * 2);
                        return float4((pbrIndirectColor * ao * gammaColor * rampColor)
                                        + (ao * pbrSection) + lightScattering * gammaColor * rampColor
                                        + (rimColor * rampColor), baseTexColor.a);
                    }
                    return float4(pbrIndirectColor * screenSpaceShadowAttenuation * ao * gammaColor * rampColor +
                        pbrIndirectColor * screenSpaceShadowAttenuation * ao * pbrSection
                        + emColor + lightScattering * gammaColor * rampColor
                        + rimColor * rampColor, baseTexColor.a);
                }
                #endif
                
                //Unlit
                #if _Unlit_ON
                {
                    return float4(pbrIndirectColor * gammaColor + emColor, baseTexColor.a);
                }
                #endif
                return float4(pbrIndirectColor * screenSpaceShadowAttenuation * ao * gammaColor * rampColor +
                            pbrIndirectColor * screenSpaceShadowAttenuation * ao * pbrSection
                            + emColor + lightScattering * gammaColor * rampColor
                            + rimColor * rampColor, baseTexColor.a);

                
                return float4(pbrIndirectColor * screenSpaceShadowAttenuation * ao * gammaColor * rampColor + pbrIndirectColor * screenSpaceShadowAttenuation * ao * pbrSection + emColor + rimColor * rampColor, baseTexColor.a);
            }
            
            ENDHLSL
        }

        Pass
        {
            Name "DrawBaseOutline"
            Tags { "LightMode" = "DrawBaseOutline" }
            Cull Front
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
                float4 uv7 : TEXCOORD7;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                float4 clipPos = vertex_position_inputs.positionCS;
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal, IN.tangent);
                float3x3 tbn = float3x3(vertex_normal_inputs.tangentWS, vertex_normal_inputs.bitangentWS, vertex_normal_inputs.normalWS);
                float3 worldNormal = mul(IN.uv7.xyz, tbn);
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
                float4 uv7 : TEXCOORD7;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
            };

            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                float4 clipPos = vertex_position_inputs.positionCS;
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal, IN.tangent);
                float3x3 tbn = float3x3(vertex_normal_inputs.tangentWS, vertex_normal_inputs.bitangentWS, vertex_normal_inputs.normalWS);
                float3 worldNormal = mul(IN.uv7.xyz, tbn);
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
    CustomEditor "PBRCharacterGUI_V2"
}
