Shader "ShaderTemplate/Toon_Face_V1"
{
    Properties
    {
        _CullOff ("Two Side", Float) = 2.0
        _Transparent ("Transparent", Float) = 0.0
        _StencilOutline ("StencilOutline", Float) = 0.0

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
        _ambientColorStrength("Ambient Color Strength", Range(0, 2)) = 0.8

        [HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1
        _FresnelColor("Fresnel Color", Color) = (1, 1, 1, 1)
        _Fresnel("Fresnel", Range(0, 5)) = 0

        [Header(Normal)]
        [Space(10)]
        _NormalMap("Normal Map", 2D) = "bump" { }

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

        [Header(SSS)]
        _ThicknessMap ("Thickness Map", 2D) = "black" {}
        _ScatterNormalDistortion ("Normal Distortion", Range(0, 5)) = 1
        _ScatterPower ("Scatter Power", Range(0, 5)) = 1
        _ScatterScale ("Scatter Scale", Range(0, 10)) = 1
        _ScatterColor ("Scatter Color", Color) = (1, 1, 1, 1)
        _PreSSS_On ("Pre SSS On", Range(0,1)) = 0

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
        _HeadForward ("Head Forawrd", Vector) = (0, 0, 0, 0)
        _HeadCenter ("Head Center", Vector) = (0, 0, 0, 0)
        _HeadRight ("Head Right", Vector) = (0, 0, 0, 0)

       _FaceForwardDirection("_FaceForwardDirection", Vector) = (0,0,1)
       _FaceUpDirection("_FaceUpDirection", Vector) = (0,1,0)
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
            float _ambientColorStrength;

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

            float3 _HeadForward;
            float3 _HeadCenter;
            float3 _HeadRight;

            float _ScatterNormalDistortion;
            float _ScatterPower;
            float _ScatterScale;
            float3 _ScatterColor;
            float _PreSSS_On;

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
            half3 _FaceForwardDirection;
            half3 _FaceUpDirection;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_RampMap);
        SAMPLER(sampler_RampMap);
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

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

            TEXTURE2D(_SDFMap);
            SAMPLER(sampler_SDFMap);
            TEXTURE2D(_ThicknessMap);
            SAMPLER(sampler_ThicknessMap);
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
                float4 worldFactor : TEXCOORD6;
            };

            //PBR
            float3 IndirectDiffuse(float2 uvStaticLightmap, float3 normalWS)
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

            v2f vert(a2v IN)
            {
                v2f OUT;

                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal.xyz, IN.tangent);
                OUT.position = vertex_position_inputs.positionCS;
                OUT.worldPos = vertex_position_inputs.positionWS;

                float3 rootPos = _DissolveOrigin.xyz;
                OUT.worldFactor = dot(normalize(_DissolveDir), OUT.worldPos - rootPos);

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

                // 1.BaseColor
                float4 baseTexColor = _BaseColor;
                baseTexColor *= SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float3 valueBaseTexColor = baseTexColor.rgb * _BaseMapValue;
                float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                baseTexColor.xyz = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation) *
                    _BaseColorIntensity;

                // 2.Normal
                float3 worldNormal = 0;
                float3 worldBitTangent = normalize(cross(IN.worldNormal, IN.worldTangent));
                float3x3 tbn = float3x3(IN.worldTangent, worldBitTangent, IN.worldNormal);
                float3 normalTexColor = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                worldNormal = normalize(mul(normalTexColor, tbn));

                // 3.光照参数
                Light light = GetMainLight(TransformWorldToShadowCoord(worldPos));
                float3 lightColor = light.color;
                float3 lightDir = light.direction;
                float3 viewDir = normalize(GetWorldSpaceViewDir(worldPos));
                float nov = saturate(max(dot(worldNormal, viewDir), 1e-5));

                // 4.环境光：探针 SH
                float3 ambientColor = 0;
                {
                    ambientColor = IndirectDiffuse(IN.lightmapUVOrVertexSH.xy, worldNormal);
                    MixRealtimeAndBakedGI(light, worldNormal, ambientColor);
                    ambientColor = saturate(ambientColor);
                }

                // 5.SDF
                float sdf = 1;
                {
                    half3 faceRightDirWS = -cross(_FaceUpDirection, _FaceForwardDirection);
                    half3x3 worldSpaceToHeadSpaceMatrix = half3x3(faceRightDirWS, _FaceUpDirection,
                                                                 _FaceForwardDirection);

                    half3 faceForwardDirectionHeadSpace = mul(worldSpaceToHeadSpaceMatrix, _FaceForwardDirection);
                    half3 faceRightDirectionHeadSpace = mul(worldSpaceToHeadSpaceMatrix, faceRightDirWS);
                    half3 lightDirectionHeadSpace = mul(worldSpaceToHeadSpaceMatrix, light.direction);


                    lightDirectionHeadSpace.y = 0;
                    float2 lightDirectionHeadSpaceXZ = SafeNormalize(lightDirectionHeadSpace).xz;

                    float2 faceForwardDirectionHeadSpaceXZ = normalize(faceForwardDirectionHeadSpace.xz);
                    float2 faceRightDirectionHeadSpaceXZ = normalize(faceRightDirectionHeadSpace.xz);

                    float FaceForwardDotL = dot(faceForwardDirectionHeadSpaceXZ, lightDirectionHeadSpaceXZ);
                    float FaceRightDotL = dot(faceRightDirectionHeadSpaceXZ, lightDirectionHeadSpaceXZ);

                    float2 faceUV = IN.uv;
                    float2 faceShadowGraientMapValueUV = faceUV * float2(-sign(FaceRightDotL), 1);
                    //----------------------------------------------------------------------------------------
                    // assume texture _FaceShadowGradientMap can be x-axis mirrored, so we just flip uv.x to get another side's gradient data
                    half4 faceShadowGradientMapSampledValue =
                        SAMPLE_TEXTURE2D(_SDFMap,sampler_SDFMap, faceShadowGraientMapValueUV);
                    half faceShadowGradientMapValue = faceShadowGradientMapSampledValue.r;
                    // find apply area
                   float resultThresholdValue = 1 - (FaceForwardDotL * 0.5 + 0.5);
   
    half faceShadowResult = smoothstep(resultThresholdValue-0.005,resultThresholdValue+0.005, faceShadowGradientMapValue); //TODO: provide smoothstep option?
    //----------------------------------------------------------------------------------------

    // intensity control (mask included)
   // faceShadowResult = lerp(1,faceShadowResult,_FaceShadowGradientIntensity * faceShadowGradientApplyArea);


                    /*// 构建 头部局部坐标系
                    float3 headForward = normalize(_HeadForward - _HeadCenter);
                    float3 headRight = normalize(_HeadRight - _HeadCenter);
                    float3 headUp = normalize(cross(headRight, headForward));
                    // 投影光照方向到头部平面
                    float3 lightDirectionProjHeadWS = normalize(lightDir - dot(lightDir, headUp) * headUp);
                    // 光照方向在头部平面的“角度”
                    float sX = dot(lightDirectionProjHeadWS, headRight);
                    float sZ = dot(lightDirectionProjHeadWS, -headForward);
                    float angleThreshold = atan2(sX, sZ) / (2 * PI) + 0.5; // 0.5 表示正前方
                    // 根据方向混合左右权重
                    float exposeRight = smoothstep(0.323, 0.55, angleThreshold);
                    float value_R = pow(saturate((angleThreshold - 0.5) * 2), 3); // 从 0.5 向右，靠近0越弱
                    float value_L = pow(saturate(1 - angleThreshold * 2), 3); // 从 0.5 向左，靠近0越弱
                    float mixThreshold = lerp(value_L, value_R, exposeRight);

                    float2 angleUV = IN.uv;
                    float sdfLeft = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, float2(1 - angleUV.x, angleUV.y)).x;
                    sdfLeft = smoothstep(0, 1, sdfLeft);
                    float sdfRight = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, angleUV).x;
                    sdfRight = smoothstep(0, 1, sdfRight);
                    float mixSDF = lerp(sdfLeft, sdfRight, exposeRight);
                    sdf = smoothstep(0, 0.2, mixThreshold - mixSDF);*/
                    sdf = faceShadowResult;
                }
//return sdf;
                // 6.Ramp明暗控制
                float3 rampShadowColor = 0;
                {
                    // DEBUG : _RampMapDebug
                    // rampUV （Unity的UV原点在左下角）
                    // - U 左右，由亮到暗
                    // - V 下上，不同材质的明暗风格表现（暂未使用）
                    float rampClampMin = 0.05; // 避免采样到边界
                    float rampClampMax = 0.95;
                    // Toon风格，只将在中间的一段作为“阴影过渡区”，其余部分干净利落
                    float rampU = clamp(sdf, rampClampMin, rampClampMax);
                    float rampV = 0; // 可拓展：不同材质明暗过度
                    float2 rampGrayUV = float2(rampU, rampV);

                    rampShadowColor = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampGrayUV).rgb;
                }

                // 7.次表面散射
                float3 lightScattering = 0.0;
                {
                    float3 translightDir = normalize(lightDir + worldNormal * _ScatterNormalDistortion);
                    float scattering = SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, IN.uv).x;
                    float transDot = pow(clamp(dot(translightDir, viewDir), 0, 1) * scattering, _ScatterPower) *
                        _ScatterScale;
                    lightScattering = transDot * _ScatterColor;
                    lightScattering *= step(0.5, _PreSSS_On);
                }


                // 8.多光源
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
                            float nol_Add = max(dot(worldNormal, lightDir_Add), 1e-5);
                            float shadow_Add = addLight.distanceAttenuation * addLight.shadowAttenuation; // 多光源衰减
                            addLightColor += nol_Add * lightColor_Add * shadow_Add; // 多光源 * 多光源衰减 * ao
                        }
                    LIGHT_LOOP_END
                }

                // 9.Rim
                float3 rimColor = 0;
                {
                    float fresnel = 1 - saturate(nov);
                    float rim = smoothstep(_RimMin, _RimMax, fresnel);
                    rim = smoothstep(0, _RimSmooth, rim);
                    rimColor += _RimColor.rgb * (_RimFac / 10) * lightColor * rim;
                    rimColor += _FresnelColor.rgb * _Fresnel * smoothstep(0.659, 1, 1 - nov);
                }

                // 10.溶解
                // 10.1 贴图溶解
                float2 dissolveUV = IN.uv * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
                dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z,
                                                                     dissolveUV.y + dissolveUV.x *
                                                                     _DissolveTexFlowAndBevel.w);
                dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;

                float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                float dissolveMapFactor = dissolveMap - strength;

                // 10.2 方向溶解
                float normDissolveFactor = saturate(IN.worldFactor / _DissolveFit);
                normDissolveFactor = lerp(1 - normDissolveFactor, normDissolveFactor, step(0, _DissolveFit));
                //方向溶解边缘
                float widthFactor = smoothstep(0.05, 0.5, _DissolveDirThreshold) * smoothstep(
                    0.05, 0.5, 1 - _DissolveDirThreshold) + 0.2;
                float width = _DissolveDirWidth * saturate(widthFactor); // 边缘宽度
                float edgeStart = smoothstep(_DissolveDirThreshold - 0.5 * width, _DissolveDirThreshold,
                   normDissolveFactor);
                // float edgeEnd = (1 - smoothstep(_DissolveDirThreshold, _DissolveDirThreshold + 0.5 * width, normDissolveFactor));

                // 剔除
                float dissolveAlpha = edgeStart * dissolveMapFactor;
                dissolveAlpha = lerp(0.01, dissolveAlpha, _Dissolve_On);
                clip(dissolveAlpha - 0.01);

                // 10.3 溶解颜色
                // =方向溶解颜色
                float edgeColorRange = edgeStart;
                float3 dissolveDirColor = lerp(0, _DissolveColor.rgb, 1 - edgeColorRange) * _DissolveColorStrength;

                // =贴图溶解颜色
                float edge = smoothstep(strength - _DissolveMapEdge, strength, dissolveMapFactor);
                // 高亮因子（中心 0 → 中间 1 → 外层 0）
                float MapLightMask = saturate(1 - edge);
                // 多层过渡：颜色 A（内），颜色 B（边）
                float3 dissolveMapColor = lerp(0, _DissolveColor.rgb, MapLightMask) * _DissolveColorStrength;

                // 11.整体变色
                float3 overallColor = _OverallColor.rgb * _OverallColorStrength;

                // 颜色合成
                float3 finalColor = clamp(
                    lightScattering * baseTexColor.rgb * lightColor
                    + baseTexColor.rgb * rampShadowColor * lerp(baseTexColor.rgb, ambientColor, _ambientColorStrength)
                    + baseTexColor.rgb * ambientColor
                    + addLightColor * baseTexColor.rgb
                    + rimColor,
                    0, 0.75);
                float finalAlpha = 1;

                // 整体变色
                finalColor.rgb = lerp(finalColor, overallColor, _OverallColorExtent);

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
            Name "DrawBaseOutline"
            Tags
            {
                "LightMode" = "DrawBaseOutline"
            }
            Cull Front
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
                float3x3 tbn = float3x3(vertex_normal_inputs.tangentWS, vertex_normal_inputs.bitangentWS,
                            vertex_normal_inputs.normalWS);
                float3 worldNormal = mul(IN.uv7.xyz, tbn);
                float3 viewNormal = TransformWorldToViewNormal(worldNormal);
                float3 ndcNormal = normalize(TransformWViewToHClip(viewNormal.xyz)) * clamp(clipPos.w, 1.0, 5.0);
                float4 nearUpperRight = mul(unity_CameraInvProjection,
                                                            float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
                float aspect = abs(nearUpperRight.y / nearUpperRight.x);
                ndcNormal.x *= aspect;
                _BaseOutlineOffset = _BaseOutlineOffset * step(0.01, _BaseOutlineOffset);
                clipPos.xy += 0.01 * _BaseOutlineOffset * ndcNormal.xy;
                OUT.position = clipPos;
                OUT.uv = IN.uv;

                float3 rootPos = _DissolveOrigin.xyz;
                OUT.worldFactor = dot(normalize(_DissolveDir), vertex_position_inputs.positionWS - rootPos);

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
                float lineStrength = 0.8; // 强度，可做材质参数
                float4 finalColor = mixedColor * lineStrength * _BaseOutlineColor;


                // ==========
                // =========溶解处理===========
                // 溶解贴图
                float2 dissolveUV = IN.uv * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
                dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z,
                                                             dissolveUV.y + dissolveUV.x * _DissolveTexFlowAndBevel.w);
                dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;
                float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                float dissolveMapFactor = dissolveMap - strength;
                // 归一化溶解因子
                float normDissolveFactor = saturate(IN.worldFactor / _DissolveFit);
                normDissolveFactor = lerp(1 - normDissolveFactor, normDissolveFactor, step(0, _DissolveFit));
                // 边缘
                float widthFactor = smoothstep(0.05, 0.5, _DissolveDirThreshold) * smoothstep(
                    0.05, 0.5, 1 - _DissolveDirThreshold) + 0.2;
                float width = _DissolveDirWidth * saturate(widthFactor);
                float edgeStart = smoothstep(_DissolveDirThreshold - 0.5 * width, _DissolveDirThreshold,
                          normDissolveFactor);
                float edgeEnd = (1 - smoothstep(_DissolveDirThreshold, _DissolveDirThreshold + 0.5 * width,
                                  normDissolveFactor));

                float dissolveAlpha = edgeStart * dissolveMapFactor;
                dissolveAlpha = lerp(0.01, dissolveAlpha, _Dissolve_On);
                clip(dissolveAlpha - 0.01);

                // =============================

                // =========溶解颜色===========
                // - 方向溶解颜色
                float edgeColorRange = edgeStart;
                float3 dissolveDirColor = lerp(0, _DissolveColor.rgb, 1 - edgeColorRange) * _DissolveColorStrength;
                // - 贴图溶解颜色
                float edge = smoothstep(strength - _DissolveMapEdge, strength, dissolveMapFactor);

                // 高亮因子（中心 0 → 中间 1 → 外层 0）
                float MapLightMask = saturate(1 - edge);

                // 多层过渡：颜色 A（内），颜色 B（边）
                float3 dissolveMapColor = lerp(0, _DissolveColor.rgb, MapLightMask) * _DissolveColorStrength;

                // =======================

                // 溶解效果叠加
                float3 finalColor_dDir = lerp(finalColor.rgb, dissolveDirColor, 1 - edgeStart);
                float3 finalColor_dDir_dMap = lerp(finalColor_dDir, dissolveMapColor, MapLightMask);

                finalColor.rgb = lerp(finalColor.rgb, finalColor_dDir_dMap, _Dissolve_On);

                return finalColor;
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
                float3x3 tbn = float3x3(vertex_normal_inputs.tangentWS, vertex_normal_inputs.bitangentWS,
                            vertex_normal_inputs.normalWS);
                float3 worldNormal = mul(IN.uv7.xyz, tbn);
                float3 viewNormal = TransformWorldToViewNormal(worldNormal);
                float3 ndcNormal = normalize(TransformWViewToHClip(viewNormal.xyz)) * clamp(clipPos.w, 1.0, 5.0);
                float4 nearUpperRight = mul(unity_CameraInvProjection,
                                                            float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
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

            Tags
            {
                "LightMode" = "DepthOnly"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            #pragma vertex m_DepthOnlyVertex
            #pragma fragment m_DepthOnlyFragment

            struct m_Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
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
                dissolveUV = float2(dissolveUV.x + dissolveUV.y * _DissolveTexFlowAndBevel.z,
                                                                             dissolveUV.y + dissolveUV.x *
                                                                             _DissolveTexFlowAndBevel.w);
                dissolveUV += _Time.y * _DissolveTexFlowAndBevel.xy;
                float dissolveMap = SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, dissolveUV);
                float strength = pow(_DissolveMapThreshold, 2.2); // 2.2 可调节
                float dissolveMapFactor = dissolveMap - strength;
                // 归一化溶解因子
                float normDissolveFactor = saturate(input.worldFactor / _DissolveFit);
                normDissolveFactor = lerp(1 - normDissolveFactor, normDissolveFactor, step(0, _DissolveFit));

                // 边缘
                float widthFactor = smoothstep(0.05, 0.5, _DissolveDirThreshold) * smoothstep(
                    0.05, 0.5, 1 - _DissolveDirThreshold) + 0.2;
                float width = _DissolveDirWidth * saturate(widthFactor);
                float edgeStart = smoothstep(_DissolveDirThreshold - 0.5 * width, _DissolveDirThreshold,
              normDissolveFactor);

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
}