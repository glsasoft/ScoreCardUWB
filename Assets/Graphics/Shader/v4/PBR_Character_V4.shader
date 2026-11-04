Shader "ShaderTemplate/PBR_Character_V4"
{
    Properties
    {
        _CullOff ("Two Side", Float) = 2.0
        _Transparent ("Transparent", Float) = 0.0
        _StencilOutline ("StencilOutline", Float) = 0.0
        _PreSSS_On ("_PreSSS_On", Range(0,1)) = 0.0
        _AnisoOn ("Aniso On", Float) = 0.0
        _SDF_On ("SDF On", Float) = 0.0
        _Unlit ("Unlit", Float) = 0.0
        _CullOutline ("Cull Outline", Float) = 1.0
        
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
        _Metallic("Metallic", Range(-1, 1)) = 0.0
        _Roughness("Roughness", Range(-1, 1)) = 0.0
        
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
        _BaseOutlineOffset ("Base Outline Offset", Range(0, 2)) = 0.1
        _StencilOutlineColor ("Stencil Outline Color", Color) = (0.5, 0.5, 0.5, 1)
        _StencilOutlineOffset ("Stencil Outline Offset", Range(0, 4)) = 0.0
        
        [Header(Ramp)]
        [Space(10)]
        _RampMap("Ramp Map", 2D) = "white"{}
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        
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
        [Toggle]_Dissolve_On ("Dissolve On", Range(0, 1)) = 0
        _DissolveFit ("Dissolve Fit", Range(-10, 10)) = 0
        _DissolveDirThreshold ("DissolveDir Threshold", Range(0, 1)) = 0
        _DissolveMapThreshold ("DissolveMap Threshold", Range(0, 1)) = 0
        [HDR]_DissolveColor ("Dissolve Color", Color) = (1, 1, 1, 1)
        _DissolveColorStrength ("Dissolve Color Strength", Range(0, 3)) = 1

        _DissolveDir ("Dissolve Dir", Vector) = (0, 1, 0, 0)
        _DissolveDirWidth ("DissolveDir Width", Range(0, 0.5)) = 0

        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _DissolveTexFlowAndBevel ("DissolveMap Flow&Bevel", Vector) = (0, 0, 0, 0)
        _DissolveMapEdge("DissolveMap Edge", Range(0,0.5)) = 0
        [Header(HideInInspecter)]
        _DissolveOrigin("Dissolve Origin", Vector) = (0,0,0,0)
        
        [Header(OverallColor)]
        _OverallColor("Overall Color", Color) = (0, 0, 0, 0)
        _OverallColorStrength("Overall Color Strength", Float) = 0
        _OverallColorExtent("Overall Color Extent", Float) = 0

        [Header(HideInInspecter)]
        _HeadForward ("Head Forward", Vector) = (0, 0, 0, 0)
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
        float _Unlit;
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float _BaseMapValue, _BaseMapSaturation, _BaseColorIntensity;
        float _EmissionIntensity;
        float3 _EmissionColor;
        float4 _FresnelColor;
        float _Fresnel;
        
        float _Metallic, _Roughness;

        float4 _RimColor;
        float _RimFac, _RimMin, _RimMax, _RimSmooth;

        float4 _BaseOutlineColor;
        float _BaseOutlineOffset;
        float4 _StencilOutlineColor;
        float _StencilOutlineOffset;

        float3 _ShadowColor;

        float _PreSSS_On;

        float _SDF_On;
        float3 _HeadForward;
        float3 _HeadCenter;
        float3 _HeadRight;

        float _AnisoOn;
        float4 _AnisoTint;
        float _RoughnessX;
        float _RoughnessY;
        float _IOR;

        float _ScatterNormalDistortion;
        float _ScatterPower;
        float _ScatterScale;
        float3 _ScatterColor;

        float _Dissolve_On;
        float _DissolveFit;
        float _DissolveDirThreshold;
        float _DissolveMapThreshold;
        float4 _DissolveColor;
        float _DissolveColorStrength;
        float4 _DissolveDir;
        float _DissolveDirWidth;
        float4 _DissolveMap_ST;
        float4 _DissolveTexFlowAndBevel;
        float _DissolveMapEdge;
        float4 _DissolveOrigin;

        float4 _OverallColor;
        float _OverallColorStrength;
        float _OverallColorExtent;
        
        CBUFFER_END
        
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
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

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap);
            SAMPLER(sampler_MaskMap);
            TEXTURE2D(_SDFMap);
            SAMPLER(sampler_SDFMap);
            TEXTURE2D(_ThicknessMap);
            SAMPLER(sampler_ThicknessMap);
            TEXTURE2D(_RampMap);
            SAMPLER(sampler_RampMap);
            // Ramp Debug
            TEXTURE2D(_RampMapDebug);
            SAMPLER(sampler_RampMapDebug);
            
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
                // 模型根位置
                // float3 rootPos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                // float3 rootPos = unity_ObjectToWorld[3].xyz;
                float3 rootPos = _DissolveOrigin.xyz;
                // 从“根位置”指向当前点
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
                // return IN.worldFactor;
                float3 worldPos = IN.worldPos;

                // 1.BaseColor
                float4 baseTexColor = _BaseColor ;
                baseTexColor *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float3 valueBaseTexColor = baseTexColor.rgb * _BaseMapValue;
                float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                baseTexColor.xyz = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation) * _BaseColorIntensity;
                
                // 2.Normal
                float3 worldNormal = 0;
                float3 worldBitTangent = normalize(cross(IN.worldNormal, IN.worldTangent));
                float3x3 tbn = float3x3(IN.worldTangent, worldBitTangent, IN.worldNormal);
                // float3 worldSmoothNormal = normalize(mul(IN.worldSmoothNormal, tbn));
                float3 normalTexColor = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                worldNormal = normalize(mul(normalTexColor, tbn));

                // 3.自发光、金属度、粗糙度
                float3 emColor = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).rgb * _EmissionColor.rgb * _EmissionIntensity;
                float4 maskTexColor = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, IN.uv);
                float metallic = saturate(maskTexColor.r + _Metallic);
                float roughness = saturate(maskTexColor.g + _Roughness);
                float ao = maskTexColor.b;

                // 4.PBR参数
                float smoothness = 1 - roughness;
                float perceptualRoughness = roughness;
                roughness = perceptualRoughness * perceptualRoughness;
                roughness = max(roughness, 0.01);
                float squareRoughness = roughness * roughness;
         
                // 5.光照参数
                Light light = GetMainLight(TransformWorldToShadowCoord(worldPos));
                float3 lightColor = light.color;
                float3 lightDir = light.direction;
                float3 viewDir = normalize(GetWorldSpaceViewDir(worldPos));
                float3 reflectDir = reflect(-normalize(viewDir), worldNormal);
                
                float nol = saturate(max(dot(worldNormal, lightDir), 1e-5));
                float lambert = dot(worldNormal, lightDir);
                float halfLambert = lambert * 0.5 + 0.5;
                // float lambertStep = smoothstep(0.423, 0.465, halfLambert);
                float lambertStep = smoothstep(0.323, 0.465, halfLambert);
                float3 halfDir = normalize(viewDir + lightDir);
                float noh = saturate(max(dot(worldNormal, halfDir), 1e-5));
                float nov = saturate(max(dot(worldNormal, viewDir), 1e-5));
                float hol = saturate(max(dot(halfDir, lightDir), 1e-5));

                float2 scrPos = IN.scrPos.xy / IN.scrPos.w;
                // float3 viewNormal = mul(unity_WorldToCamera, float4(IN.worldNormal, 0)).xyz;
                // float2 matcapUV = (viewNormal * 0.5 + 0.5).xy;
                float shadowMap = light.shadowAttenuation;

                // 6.环境光：探针 SH
                float3 ambientColor = IndirectDiffuse(IN.lightmapUVOrVertexSH.xy, worldNormal);
                MixRealtimeAndBakedGI(light, worldNormal, ambientColor);
                ambientColor = saturate(ambientColor);
                
                // 7.SDF
                float sdf = 1;
                {
                    float3 headForward = normalize(_HeadForward - _HeadCenter);
                    float3 headRight = normalize(_HeadRight - _HeadCenter);
                    float3 headUp = normalize(cross(headRight, headForward));

                    float3 lightDirectionProjHeadWS = normalize(lightDir - dot(lightDir, headUp) * headUp);

                    float sX = dot(lightDirectionProjHeadWS, headRight);
                    float sZ = dot(lightDirectionProjHeadWS, -headForward);

                    float angleThreshold = atan2(sX, sZ) / (2 * PI) + 0.5;

                    float exposeRight = smoothstep(0.45, 0.55, angleThreshold);
                    float value_R = pow(saturate((angleThreshold - 0.5) * 2), 3);   // 靠近0的时候更靠近0
                    float value_L = pow(saturate(1 - angleThreshold * 2), 3);
                    float mixThreshold = lerp(value_L, value_R, exposeRight);
                
                    float2 angleUV = IN.uv;
                    float sdfLeft = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, float2(1 - angleUV.x, angleUV.y)).x;
                    sdfLeft = smoothstep(0, 1, sdfLeft);
                    float sdfRight = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, angleUV).x;
                    sdfRight = smoothstep(0, 1, sdfRight);
                    float mixSDF = lerp(sdfLeft, sdfRight, exposeRight);
                    //sdf = step(0, mixThreshold - mixSDF);
                    sdf = smoothstep(0, 0.2, mixThreshold - mixSDF);
                    sdf = lerp(sdf, 1, step(_SDF_On, 0));
                }
                
                // 8. 次表面散射
                float3 lightScattering = 0.0;
                {
                    float3 translightDir = normalize(lightDir + worldNormal * _ScatterNormalDistortion);
                    float scattering = SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, IN.uv).x;  
                    float transDot = pow(clamp(dot(translightDir, viewDir), 0, 1) * scattering, _ScatterPower) * _ScatterScale;
                    lightScattering = transDot * _ScatterColor;
                    lightScattering *= step(0.5, _PreSSS_On);
                }
               
                // 9. PBR
                float3 pbrDiffuseColor = 0;
                float3 pbrSpecularColor = 0;
                float3 pbrIndirectColor = 0;        // 微乎其微
                {
                    float3 F0 = F0_Function(baseTexColor.rgb, metallic);
                    float3 Direct_F = F_Function(hol, F0);
                    float3 Kd = (1 - Direct_F) * (1 - metallic);
                    pbrDiffuseColor = Kd * baseTexColor.rgb * lightColor; // nol
                    
                    //ANISO Specular
                    float F = fresnel(lightDir, worldNormal, 1.0, _IOR);
                    float G = GeometrySmith_aniso(worldNormal, viewDir, lightDir, _RoughnessX, _RoughnessY, IN.worldTangent, worldBitTangent);
                    float D = DistributionGGX_Aniso(_RoughnessX, _RoughnessY, noh, halfDir, IN.worldTangent, worldBitTangent);
                    float3 ANISO_pbrBRDFSpecSection = D * G * F / (4 * nol * nov + 1e-5);
                    float3 ANISO_pbrSpecularColor = ANISO_pbrBRDFSpecSection * lightColor * baseTexColor.rgb * nol * PI;
                    //Specular
                    float Direct_D = D_Function(noh, squareRoughness);
                    float Direct_G = G_Function(nol, nov, roughness);
                    float3 Direct_pbrBRDFSpecSection = Direct_D * Direct_G * Direct_F / (4 * nol * nov + 1e-5);
                    float3 Direct_pbrSpecularColor = Direct_pbrBRDFSpecSection * lightColor * nol * PI;

                    pbrSpecularColor = lerp(Direct_pbrSpecularColor, ANISO_pbrSpecularColor, _AnisoOn);
                    float3 pbrBRDFSpecSection = lerp(Direct_pbrBRDFSpecSection, ANISO_pbrBRDFSpecSection, _AnisoOn);

                    //Indirect
                    float3 Indirect_Ks = Indirect_F_Function(nov, F0, roughness);
                    float3 Indirect_Kd = (1 - Indirect_Ks) * (1 - metallic);
                    float3 pbrIndirectDiffColor = ambientColor * Indirect_Kd * baseTexColor.rgb * saturate(halfLambert);
                    

                    // 反射探针
                    float3 IndirectSpeCubeColor = IndirectSpeCube(reflectDir, worldPos, scrPos, perceptualRoughness);
                    float3 IndirectSpeCubeFactor = IndirectSpeFactor(roughness, smoothness, pbrBRDFSpecSection, F0, nov);
                    float3 pbrIndirectSpeColor = IndirectSpeCubeColor * IndirectSpeCubeFactor;

                    pbrIndirectColor = pbrIndirectDiffColor + pbrIndirectSpeColor;
                }
                

                // 10. Ramp明暗控制
                // float3 rampColor = 0;
                float3 grayShadowColor = 0;
                float3 darkShadowColor = 0;
                {
                    // DEBUG : _RampMapDebug
                    // rampUV （Unity的UV原点在左下角）
                    // - U 左右，由亮到暗
                    // - V 下上，不同材质的明暗风格表现（暂未使用）

                    float rampClampMin = 0.05; // 避免采样到边界
                    float rampClampMax = 0.95;
                    // Toon风格，只将在中间的一段作为“阴影过渡区”，其余部分干净利落
                    float rampU = clamp(lerp(0.2, 0.4, halfLambert), rampClampMin, rampClampMax);
                    float rampV = 0;    // 可拓展：不同材质明暗过度

                    float2 rampGrayUV = float2(rampU, rampV);
                    float2 rampDarkUV = float2(rampClampMin, rampV);

                    // float3 rampSample = SAMPLE_TEXTURE2D(_RampMapDebug, sampler_RampMapDebug, rampUV).rgb;  // DEBUG - _RampMapDebug
                    float3 rampGrayColor = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampGrayUV).rgb;
                    float3 rampDarkColor = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampDarkUV).rgb;

                    rampGrayColor = clamp(rampGrayColor,0.4,1); // 控制ramp最深的颜色（后续手绘就会好一点）
                    rampDarkColor = clamp(rampDarkColor,0.4,1);
                    
                    grayShadowColor = pbrDiffuseColor * rampGrayColor * _ShadowColor.rgb;
                    darkShadowColor = pbrDiffuseColor * rampDarkColor * _ShadowColor.rgb;

                    // 插值过渡，控制强度
                    // rampColor = clamp(clamp(rampSample,0.4,1) * clamp(shadowMap, 0.4, 1), 0.2, 1);
                    // rampColor = clamp(rampSample * clamp(shadowMap, 0.4, 1), 0.4, 1);  // 统一 rampSample和shadowmap颜色
                }

                // 11.多光源
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
                        // addLightColor += (diffuse_Add + specular_Add) ;
                        float shadow_Add = addLight.distanceAttenuation * addLight.shadowAttenuation;   // 多光源衰减
                        
                        addLightColor += (diffuse_Add + specular_Add) * shadow_Add;                // 多光源 * 多光源衰减 * ao
                    }
                    LIGHT_LOOP_END
                }

                // 12.Rim
                float3 rimColor = 0;
                {
                    float fresnel = 1 - saturate(nov);
                    float rim = smoothstep(_RimMin, _RimMax, fresnel);
                    rim = smoothstep(0, _RimSmooth, rim);
                    rimColor += _RimColor.rgb * (_RimFac / 10) * lightColor * rim;
                    rimColor += _FresnelColor.rgb * _Fresnel * smoothstep(0.659, 1, 1 - nov);
                }
                

                // 13.溶解处理
                // 贴图溶解
                float2 dissolveUV = IN.uv * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
                dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z, dissolveUV.y + dissolveUV.x * _DissolveTexFlowAndBevel.w);
                dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;
                float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                float dissolveMapFactor = dissolveMap - strength;
                
                // 方向溶解
                float normDissolveFactor =  saturate(IN.worldFactor / _DissolveFit);
                normDissolveFactor = lerp(1 - normDissolveFactor, normDissolveFactor, step(0, _DissolveFit));
                // return normDissolveFactor;
                
                // - 方向溶解边缘
                float widthFactor = smoothstep(0.05, 0.5, _DissolveDirThreshold) * smoothstep(0.05, 0.5, 1 - _DissolveDirThreshold) + 0.2;
                float width = _DissolveDirWidth * saturate(widthFactor); // 边缘宽度
                float edgeStart = smoothstep(_DissolveDirThreshold - 0.5 * width, _DissolveDirThreshold, normDissolveFactor);
                // float edgeEnd = (1 - smoothstep(_DissolveDirThreshold, _DissolveDirThreshold + 0.5 * width, normDissolveFactor));
             
                // 剔除
                float dissolveAlpha = edgeStart * dissolveMapFactor;
                dissolveAlpha = lerp(0.01, dissolveAlpha, _Dissolve_On);
                clip(dissolveAlpha - 0.01);
                

                // 溶解颜色
                // - 方向溶解颜色
                float edgeColorRange = edgeStart;
                float3 dissolveDirColor = lerp(0, _DissolveColor.rgb, 1 - edgeColorRange) * _DissolveColorStrength;

                // return float4(dissolveDirColor, 1);

                // - 贴图溶解颜色
                float edge = smoothstep(strength - _DissolveMapEdge , strength, dissolveMapFactor) ;
                // float outer = 1 - smoothstep(strength, strength + _DissolveMapEdge, dissolveMapFactor);

                // 高亮因子（中心 0 → 中间 1 → 外层 0）
                float MapLightMask = saturate(1 - edge) ;
                
                // 多层过渡
                float3 dissolveMapColor = lerp(0, _DissolveColor.rgb, MapLightMask) * _DissolveColorStrength;
                
                // 14. 整体变色
                float3 overallColor = _OverallColor.rgb * _OverallColorStrength;

                // 颜色合成
                // Unlit Color
                float3 unlitFinalColor = baseTexColor.rgb + emColor;

                float3 diffuse = 0;
                {
                    diffuse = lerp(grayShadowColor, pbrDiffuseColor, lambertStep);
                    diffuse = lerp(darkShadowColor, diffuse, shadowMap * sdf);
                }
                ao = 1;

                // PBR Color
                float3 pbrFinalColor = clamp(
                                pbrIndirectColor
                                + (diffuse + pbrSpecularColor) * ao
                                + clamp(addLightColor, 0.0, 1.0) * ao
                                + rimColor
                                + lightScattering * baseTexColor.rgb * grayShadowColor * ao, 0, 0.75)
                                + emColor;
                
                // Final Color
                float3 finalColor = lerp(pbrFinalColor, unlitFinalColor, _Unlit); // _Unlit为1 无光
                float finalAlpha = 1;

                // 整体变色
                finalColor = lerp(finalColor, overallColor, _OverallColorExtent);
                
                // 溶解效果
                float3 finalColor_dDir = lerp(finalColor, dissolveDirColor, 1 - edgeStart);
                float3 finalColor_dDir_dMap = lerp(finalColor_dDir, dissolveMapColor, MapLightMask);
                finalColor = lerp(finalColor, finalColor_dDir_dMap, _Dissolve_On);
                
                
                
                return float4(finalColor, finalAlpha);
            }
            
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            Cull [_CullOff]
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
        // ===========测试用，最后删==============
        Pass{
            Name "OcclusionView"
            Tags { "LightMode" = "OcclusionView"} 

            ZWrite Off
            ZTest Greater      // 只有在后面才渲染
            Cull Back     // 避免前后同时渲染

            // Blend SrcAlpha OneMinusSrcAlpha   // *透明效果

            Stencil {
                Ref 10   //  模版参考值
                Comp Equal                      //  模版测试 比较方式
                Pass Keep                           //  测试通过对模版值的操作（使用模版参考值）（默认模版值为0）
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
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
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                // 轮廓光效果
                // float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.worldPos);
                // float3 normalWS = normalize(IN.worldNormal);
                // float rim = pow(1.0 - saturate(dot(normalize(viewDir), normalize(normalWS))), 2); // _RimPower = 2
                
                // 队友：纯色效果
                float3 color1 = float3(0.4, 1, 0.4);
                return float4(color1, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DrawBaseOutline"
            Tags { "LightMode" = "DrawBaseOutline" }
//            Cull Front
            Cull [_CullOutline]
            Stencil
            {
                Ref [_StencilOutlineValue]
                Pass [_StencilOutlinePass]
            }
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
                float4 worldFactor : TEXCOORD1;
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
                _BaseOutlineOffset = _BaseOutlineOffset * step(0.01, _BaseOutlineOffset);
                clipPos.xy += 0.01 * _BaseOutlineOffset * ndcNormal.xy;
                OUT.position = clipPos;
                OUT.uv = IN.uv;
                
                float3 rootPos = _DissolveOrigin.xyz;
                OUT.worldFactor = dot(normalize(_DissolveDir), vertex_position_inputs.positionWS - rootPos) ;
                
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                // ===描边颜色===
                float4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                
                float maxColor = max(max(baseTexColor.r, baseTexColor.g), baseTexColor.b);
                float edgeFactor = maxColor - 0.004;
                float mask = step(edgeFactor, maxColor);
                float mixFactor = 0.6;  
                float4 mixedColor = lerp(baseTexColor, baseTexColor * mask, mixFactor);

                // 最终输出：混合后的线 × 强度 × 描边颜色
                float lineStrength = 0.8;  // 强度，可做材质参数
                float4 finalColor = mixedColor * lineStrength * _BaseOutlineColor;
                
                
                // ==========
                 // =========溶解处理===========
                // 溶解贴图
                float2 dissolveUV = IN.uv * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
                dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z, dissolveUV.y + dissolveUV.x * _DissolveTexFlowAndBevel.w);
                dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;
                float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                float dissolveMapFactor = dissolveMap - strength;
                // 归一化溶解因子
                float normDissolveFactor = saturate(IN.worldFactor / _DissolveFit);
                normDissolveFactor = lerp(1 - normDissolveFactor, normDissolveFactor, step(0, _DissolveFit));
                // 边缘
                float widthFactor = smoothstep(0.05, 0.5, _DissolveDirThreshold) * smoothstep(0.05, 0.5, 1 - _DissolveDirThreshold) + 0.2;
                float width = _DissolveDirWidth * saturate(widthFactor);
                float edgeStart = smoothstep(_DissolveDirThreshold - 0.5 * width, _DissolveDirThreshold, normDissolveFactor);
                // float edgeEnd = (1 - smoothstep(_DissolveDirThreshold, _DissolveDirThreshold + 0.5 * width, normDissolveFactor));
                
                float dissolveAlpha = edgeStart * dissolveMapFactor;
                dissolveAlpha = lerp(0.01, dissolveAlpha, _Dissolve_On );
                clip(dissolveAlpha - 0.01);

             // =============================

             // 溶解颜色
                // - 方向溶解颜色
                float edgeColorRange = edgeStart;
                float3 dissolveDirColor = lerp(0, _DissolveColor.rgb, 1 - edgeColorRange) * _DissolveColorStrength;
                // - 贴图溶解颜色
                
                float edge = smoothstep(strength - _DissolveMapEdge , strength, dissolveMapFactor) ;
                // float outer = 1 - smoothstep(strength, strength + _DissolveMapEdge, dissolveMapFactor);

                // 高亮因子（中心 0 → 中间 1 → 外层 0）
                float MapLightMask = saturate(1 - edge) ;


                // 多层过渡：颜色 A（内），颜色 B（边）
                float3 dissolveMapColor = lerp(0, _DissolveColor.rgb, MapLightMask) * _DissolveColorStrength;

                // =======================

                // 溶解效果叠加
                float3 finalColor_dDir = lerp(finalColor.rgb, dissolveDirColor, 1 - edgeStart);
                float3 finalColor_dDir_dMap = lerp(finalColor_dDir, dissolveMapColor, MapLightMask);

                finalColor.rgb = lerp(finalColor.rgb, finalColor_dDir_dMap, _Dissolve_On);

                return float4(finalColor.rgb, 1);
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
            Name "DepthOnly"

            Tags {
                "LightMode" = "DepthOnly"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            #pragma vertex m_DepthOnlyVertex
            #pragma fragment m_DepthOnlyFragment


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
                // m_Varyings output = (m_Varyings)0;
                m_Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 rootPos = _DissolveOrigin.xyz;
                
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(input.position);
                output.worldFactor = dot(normalize(_DissolveDir), vertex_position_inputs.positionWS - rootPos);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half m_DepthOnlyFragment(m_Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

           
            // =========溶解处理===========
                // 溶解贴图
                float2 dissolveUV = input.uv * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
                dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z, dissolveUV.y + dissolveUV.x * _DissolveTexFlowAndBevel.w);
                dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;
                float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                float dissolveMapFactor = dissolveMap - strength;
                // 归一化溶解因子
                float normDissolveFactor = saturate(input.worldFactor / _DissolveFit);
                normDissolveFactor = lerp(1 - normDissolveFactor, normDissolveFactor, step(0, _DissolveFit));
                // return normDissolveFactor;
                // 边缘
                float widthFactor = smoothstep(0.05, 0.5, _DissolveDirThreshold) * smoothstep(0.05, 0.5, 1 - _DissolveDirThreshold) + 0.2;
                float width = _DissolveDirWidth * saturate(widthFactor);
                float edgeStart = smoothstep(_DissolveDirThreshold - 0.5 * width, _DissolveDirThreshold, normDissolveFactor);
                
                float dissolveAlpha = edgeStart * dissolveMapFactor;
                dissolveAlpha = lerp(0.01, dissolveAlpha, _Dissolve_On);
                clip(dissolveAlpha - 0.01);
                
             // =============================
             


                #if defined(LOD_FADE_CROSSFADE)
                    LODFadeCrossFade(input.positionCS);
                #endif

                return input.positionCS.z;
            }
            ENDHLSL
        }

    }
//    CustomEditor "PBRCharacterGUI_V4"
}
