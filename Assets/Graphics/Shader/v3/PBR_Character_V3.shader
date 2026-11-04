Shader "ShaderTemplate/PBR_Character_V3"
{
    Properties
    {
        _CullOff ("Two Side", Float) = 2.0
        _Transparent ("Transparent", Float) = 0.0
        _StencilOutline ("StencilOutline", Float) = 0.0
        _ScreenSpaceShadow ("ScreenSpaceShadow", Float) = 0.0
        _Pre_SSS ("Pre SSS", Float) = 0.0
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
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
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
        _RampSmoothness ("Ramp Smoothness", Range(0, 1)) = 0.15
        _ShadowFadeColor ("Shadow Fade Color", Color) = (0, 0, 0, 1)
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        _ShallowFadeColor ("Shallow Fade Color", Color) = (0.5, 0.5, 0.5, 1)
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
        
        [Header(Aniso)]
        _AnisoTint ("Aniso Tint", Color) = (1, 1, 1, 1)
        _RoughnessX ("Roughness X", Range(0, 1)) = 0.5
        _RoughnessY ("Roughness Y", Range(0, 1)) = 0.5
        _IOR ("IOR", Range(1.0, 100.0)) = 1.5
        
        [Header(SSS)]
        _ThicknessMap ("Thickness Map", 2D) = "black" {}
        _ScatterNormalDistortion ("Normal Distortion", Range(0, 5)) = 1
        _ScatterPower ("Scatter Power", Range(0, 5)) = 1
        _ScatterScale ("Scatter Scale", Range(0, 10)) = 1
        _ScatterColor ("Scatter Color", Color) = (1, 1, 1, 1)
        
        [Header(Dissolve)]
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        [HDR]_DissolveColor ("Dissolve Color", Color) = (1, 1, 1, 1)
        _DissolveEmissionWidth ("Dissolve Emission Width", Range(0, 1)) = 0
        _DissolveSmooth ("Dissolve Smooth", Range(0, 1)) = 0
        _DissolveThreshold ("Dissolve Threshold", Range(0, 10)) = 0
        _DissolveWorldSpaceScale ("Dissolve World Space Scale", Range(0, 5)) = 1
        _DissolveDir ("Dissolve Dir", Vector) = (0, 1, 0, 0)
        
        [Header(HideInInspecter)]
        _HeadForward ("Head Forawrd", Vector) = (0, 0, 0, 0)
        _HeadCenter ("Head Center", Vector) = (0, 0, 0, 0)
        _HeadRight ("Head Right", Vector) = (0, 0, 0, 0)
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

        float4 _AnisoTint;
        float _RoughnessX;
        float _RoughnessY;
        float _IOR;

        float _ScatterNormalDistortion;
        float _ScatterPower;
        float _ScatterScale;
        float3 _ScatterColor;

        float4 _DissolveColor;
        float _DissolveEmissionWidth;
        float _DissolveSmooth;
        float _DissolveThreshold;
        float _DissolveWorldSpaceScale;
        float4 _DissolveDir;
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
            #pragma shader_feature_local _DissolveMapOn

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
            TEXTURE2D(_ThicknessMap);
            SAMPLER(sampler_ThicknessMap);
            TEXTURE2D(_DissolveMap);
            SAMPLER(sampler_DissolveMap);
            
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
                float4 worldFactor : TEXCOORD7;
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
            float3 IndirectSpeCube(float3 reflectDir, float3 worldPos, float2 scrPos, float perceptualRoughness)
            {
                //roughness = roughness * (1.7 - 0.7 * roughness); // unity 内部不是线性 调整下 拟合曲线求近似，可以再 GGB 可视化曲线
                //float mipmapLevel = roughness * 6; // 把粗糙度 remap 到 0~6 的 7个阶段，然后进行 texture lod 采样
                float3 specularColor = GlossyEnvironmentReflection(reflectDir, worldPos, perceptualRoughness, 1.0, scrPos);
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
                float3 rootPos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                OUT.worldFactor = dot(normalize(_DissolveDir.xyz), OUT.worldPos - rootPos);
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

                float dissolveEdge = 0;
                float dissolveAlpha = 1;
                #if _DissolveMapOn
                    float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, IN.uv);
                    dissolveAlpha = (dissolveMap - _DissolveThreshold) + IN.worldFactor * _DissolveWorldSpaceScale;
                #endif
                
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
                float3 emColor = _EmissionColor * _EmissionIntensity;
                float ao = 1;
                float metallic = saturate(_Metallic);
                float roughness = saturate(_Roughness);
                float perceptualRoughness = roughness;
                float squareRoughness = roughness * roughness;
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
                
                //SDF
                float sdf = 1;
                {
                    #if _SDFMapOn
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
                        sdf = step(0, mixThreshold - mixSDF);
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
                    }
                    #endif
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
                    float rampAttenuation = 1.5 * lambert;
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
                    
                    rampColor = (rampForward * _ForwardColor + rampFront * _FrontColor + rampSSS * _SSSColor);
                    rampColor += (rampShadowFade * _ShadowFadeColor + rampShadow * _ShadowColor + rampShallowFade * _ShallowFadeColor + rampShallow * _ShallowColor);
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
                    pbrDiffuseColor = Kd * baseTexColor.rgb * lightColor; // nol
                    
                    #if _ANISO_ON
                        //Specular
                        float F = fresnel(lightDir, worldNormal, 1.0, _IOR);
                        float G = GeometrySmith_aniso(worldNormal, viewDir, lightDir, _RoughnessX, _RoughnessY, IN.worldTangent, worldBitTangent);
                        float D = DistributionGGX_Aniso(_RoughnessX, _RoughnessY, noh, halfDir, IN.worldTangent, worldBitTangent);

                        pbrBRDFSpecSection = D * G * F / (4 * nol * nov + 1e-5);
                        pbrSpecularColor = pbrBRDFSpecSection * lightColor * baseTexColor.rgb * nol * PI;
                    #else
                        //Specular
                        float Direct_D = D_Function(noh, squareRoughness);
                        float Direct_G = G_Function(nol, nov, roughness);
                        pbrBRDFSpecSection = Direct_D * Direct_G * Direct_F / (4 * nol * nov + 1e-5);
                        
                        pbrSpecularColor = pbrBRDFSpecSection * lightColor * nol * PI;
                    #endif

                    //Indirect
                    float3 Indirect_Ks = Indirect_F_Function(nov, F0, roughness);
                    float3 Indirect_Kd = (1 - Indirect_Ks) * (1 - metallic);
                    pbrIndirectDiffColor = ambientColor * Indirect_Kd * baseTexColor.rgb * saturate(halfLambert) * _ShadowRatio;
                    
                    float3 IndirectSpeCubeColor = IndirectSpeCube(reflectDir, worldPos, scrPos, perceptualRoughness);
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
                        
                        float3 baseColor_Add = baseTexColor.rgb;
                        //Diffuse
                        float3 F0_Add = F0_Function(baseColor_Add, metallic);
                        float3 Direct_F_Add = F_Function(hol_Add, F0_Add);
                        float3 Kd_Add = (1 - Direct_F_Add) * (1 - metallic);
                        float3 diffuse_Add = baseColor_Add * Kd_Add * lightColor_Add * nol_Add;

                        //Specular
                        float Direct_D_Add = D_Function(noh_Add, squareRoughness);
                        float Direct_G_Add = G_Function(nol_Add, nov, roughness);
                        float3 BRDFSpecSection_Add = Direct_D_Add * Direct_G_Add * Direct_F_Add / (4 * nol_Add * nov + 1e-5);
                        float3 specular_Add = BRDFSpecSection_Add * lightColor_Add * nol_Add * PI;
                        addLightColor += (diffuse_Add + specular_Add);
                    }
                    LIGHT_LOOP_END
                }

                //Rim
                float3 rimColor = 0;
                {
                    _RimFac /= 10;
                    float fresnel = 1 - saturate(nov);
                    float rim = smoothstep(_RimMin, _RimMax, fresnel);
                    rim = smoothstep(0, _RimSmooth, rim);
                    rimColor += _RimColor.rgb * _RimFac * lightColor * rim;
                    rimColor += _FresnelColor.rgb * _Fresnel * smoothstep(0.659, 1, 1 - nov);
                }
                
                float3 finalColor = float3(0, 0, 0);
                float finalAlpha = 1;
                
                //Unlit
                #if _Unlit_ON
                    finalColor = baseTexColor.rgb + emColor;
                    finalAlpha = baseTexColor.a;
                #else
                    finalColor = clamp(pbrIndirectColor
                                + (pbrDiffuseColor + pbrSpecularColor) * ao * rampColor
                                + addLightColor * ao
                                + rimColor
                                + lightScattering * baseTexColor.rgb * ao * rampColor, 0, 0.8)
                                + emColor;
                    finalAlpha = baseTexColor.a;
                #endif
                
                //溶解亮边颜色
                //finalColor = lerp(finalColor, _DissolveColor.rgb, _DissolveColor.a * (1 - dissolveAlpha));
                //finalColor += dissolveEdge * _DissolveColor.rgb * _DissolveColor.a;
                
                return float4(finalColor, finalAlpha);
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
                float3 rootPos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                output.worldFactor = dot(normalize(_DissolveDir.xyz), TransformObjectToWorld(input.position) - rootPos);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half m_DepthOnlyFragment(m_Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float dissolveAlpha = 1;
                #if _DissolveMapOn
                    float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, input.uv);
                    dissolveAlpha = (dissolveMap - _DissolveThreshold) + input.worldFactor * _DissolveWorldSpaceScale;
                #endif
                clip(dissolveAlpha - 0.01);

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
            Cull Off
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

            #pragma vertex UniversalVertexMeta
            #pragma fragment CartoonMetaFragment
            
            half4 CartoonMetaFragment(Varyings i) : SV_Target
            {
                float4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                MetaInput metaInput;
                metaInput.Albedo = baseTexColor.rgb * _BaseColor.rgb;
                metaInput.Emission = baseTexColor.rgb * _BaseColor.rgb;
                return UniversalFragmentMeta(i, metaInput);
            }
            ENDHLSL
        }
    }
    CustomEditor "PBRCharacterGUI_V3"
}
