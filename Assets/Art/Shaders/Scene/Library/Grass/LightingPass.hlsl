//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//#define DEBUG_BEND_AREA
//#define DEBUG_BEND_VECTORS

#if UNITY_VERSION >= 202120
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#endif
#include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Custom/Common.hlsl"
#include "Assets/GameData/Shader/Scene/Library/Grass/Wind.hlsl"
#include "Assets/GameData/Shader/Scene/Library/XHHYLitTest/XHHYCommon.hlsl"
//Attributes shared per pass, varyings declared separately per pass


CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float4 _BaseColor_Down;
    float4 _HueVariation;
    float _MaskMap_Scale;
    half _Smoothness;
    float4 _EmissionColor;
    float _ColorMapHeight;

    float _Alpha;
    //Wind
    half4 _WindDirection;
    half _WindAmbientStrength;
    float _WindSpeed;
    half _WindVertexRand;
    half _WindObjectRand;
    half _WindRandStrength;
    half _WindSwinging;
    half _WindGustStrength;
    half _WindGustFreq;
    float _WindGustSpeed;
    half _WindGustTint;
    float _SpecularInt;

    half4 _BaseColor_OtherWorld;
    half4 _BaseColor_Down_OtherWorld;
    half4 _HueVariation_OtherWorld;

CBUFFER_END


// _NILOTOON_DISSOLVE
float _DissolveAmount;
//float _DissolveSource;
float _DissolveBorderRange;
half4 _DissolveBorderTintColor;
float _DissolveTopBorderRange;
half4 _DissolveTopBorderTintColor;
float _MaskRadius_OtherWorld;
float _MaskDeflate_OtherWorld;
float3 _MaskPosition_OtherWorld;
float _ColorMapHeight_OtherWorld;
TEXTURE2D(_DissolveThresholdMap);
TEXTURE2D(_DissolveThresholdMap2);
SAMPLER(sampler_linear_repeat);


TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(TerrainShadowMask);
SAMPLER(sampler_TerrainShadowMask);
float4 TerrainShadowMaskTex_Offset;
float4 TerrainPosition;

TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);

struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct Varyings
{
    float4 uv : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 1);
    //#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR) //Always needed
    float3 positionWS : TEXCOORD2;

    float4 positionCS : SV_POSITION;
    float4 color : COLOR0;
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	float4 shadowCoord              : TEXCOORD10;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

float ObjectPosRand01()
{
    #if defined(UNITY_DOTS_INSTANCING_ENABLED)
	return saturate(frac(UNITY_MATRIX_M[0][2] + UNITY_MATRIX_M[1][2] + UNITY_MATRIX_M[2][2]));
    #else
    return frac(UNITY_MATRIX_M[0][3] + UNITY_MATRIX_M[1][3] + UNITY_MATRIX_M[2][3]);
    #endif
}

struct BendSettings
{
    uint mode;
    float mask;
    float pushStrength;
    float flattenStrength;
    float perspectiveCorrection;
};


BendSettings PopulateBendSettings(uint mode, float mask, float pushStrength, float flattenStrength,
                                  float perspCorrection)
{
    BendSettings s = (BendSettings)0;

    s.mode = mode;
    s.mask = mask;
    s.pushStrength = pushStrength;
    s.flattenStrength = flattenStrength;
    s.perspectiveCorrection = perspCorrection;

    return s;
}


Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float posOffset = ObjectPosRand01();

    WindSettings wind = PopulateWindSettings(_WindAmbientStrength, _WindSpeed, _WindDirection, _WindSwinging,
                                             input.color.g, _WindObjectRand, _WindVertexRand, _WindRandStrength,
                                             _WindGustStrength, _WindGustFreq, _WindGustSpeed);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float4 windVec = GetWindOffset(input.positionOS.xyz, output.positionWS, posOffset, wind);
    //Less wind on shorter grass

    float3 positionWS = output.positionWS;

    positionWS.xz += windVec.xz;
    positionWS.y -= windVec.y;


    output.color.rgb = input.color.rgb;
    output.uv.xy = input.uv;
    output.positionCS = TransformWorldToHClip(positionWS);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)


    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
               
	output.shadowCoord = ComputeScreenPos(output.positionCS);
    #else
	output.shadowCoord = TransformWorldToShadowCoord( positionWS);
    #endif
    #endif

    OUTPUT_SH(float3(0,1,0), output.vertexSH);
    return output;
}

void ModifySurfaceData(Varyings input, out SurfaceData surfaceData)
{
    float4 albedoAlpha = 1;

    //If MSAA is enabled an issue occurs where any vertex interpolated values are abnormal if the geometry occupies 1px on screen
    //Clamping the value resolves these artefacts for the most part, otherwise causes fireflies
    half mask = input.uv.y;
    float ColorMask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.positionWS.xz * 0.1 * _MaskMap_Scale).r;
    _BaseColor = lerp(_BaseColor, _HueVariation, ColorMask);
    half heightGradient = saturate(sqrt(mask));
    heightGradient = pow(heightGradient, _ColorMapHeight);

    albedoAlpha.rgb = lerp(_BaseColor_Down.rgb, _BaseColor.rgb, heightGradient);
    //Apply ambient occlusion and random darkening from vertex stage

    #if _BASEMAP
    albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    clip(albedoAlpha.a - 0.5);
    #endif


    surfaceData.albedo = albedoAlpha.rgb;


    surfaceData.specular = float3(0, 0, 0);
    surfaceData.metallic = 0.0;
    surfaceData.smoothness = lerp(0.0, _Smoothness, mask);
    surfaceData.smoothness = _Smoothness;
    #ifdef _NORMALMAP
	surfaceData.normalTS = SampleNormal(input.uv.zw, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    #else
    surfaceData.normalTS = float3(0.5, 0.5, 1.0);
    #endif
    surfaceData.emission = lerp(0.0, _EmissionColor.rgb, mask);
    surfaceData.emission = _EmissionColor.rgb;
    surfaceData.occlusion = 1.0;
    surfaceData.alpha = albedoAlpha.a;

    #if UNITY_VERSION >= 202030
	surfaceData.clearCoatMask = 0.0h;
	surfaceData.clearCoatSmoothness = 0.0h;
    #endif

    //Debug
    //surfaceData.albedo = surfaceData.smoothness.xxx;
}

//This function is a testament to how convoluted cross-compatibility between difference URP versions has become
void PopulateLightingInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    //#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS.xyz;
    //#endif

    //Using GetWorldSpaceViewDir returns a constant vector for orthographic camera's, which isn't useful
    half3 viewDirWS = normalize(_WorldSpaceCameraPos - (input.positionWS.xyz));

    half3x3 tangentToWorld = 0;
    #if defined(_NORMALMAP)
	float sgn = input.tangentWS.w; // should be either +1 or -1
	float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
	tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);;
    #endif


    inputData.normalWS = NormalizeNormalPerPixel(float3(0, 1, 0));

    inputData.viewDirectionWS = viewDirWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) //No shadow cascades
	inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    #if UNITY_VERSION >= 202120
	inputData.positionCS = input.positionCS;
	inputData.tangentToWorld = tangentToWorld; //Not actually using this value of the InputData struct, but URP12+ dictates it
    #endif

    //inputData.fogCoord = input.fogFactorAndVertexLight.x;
    //inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;

    #if defined(DYNAMICLIGHTMAP_ON) && UNITY_VERSION >= 202120
	inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV.xy, input.vertexSH, inputData.normalWS);
    #elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
	inputData.bakedGI = SAMPLE_GI(input.vertexSH, GetAbsolutePositionWS(inputData.positionWS), inputData.normalWS, inputData.viewDirectionWS, input.positionCS.xy);
    #else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    #endif


    #if UNITY_VERSION >= 202030
	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
	inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
    #endif

    #if defined(DEBUG_DISPLAY) && UNITY_VERSION >= 202120//URP 12+
    #if defined(DYNAMICLIGHTMAP_ON)
	inputData.dynamicLightmapUV = input.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
	inputData.staticLightmapUV = input.staticLightmapUV;
    #else
	inputData.vertexSH = input.vertexSH;
    #endif
    #endif
}

struct TranslucencyData
{
    float strengthDirect;
    float strengthIndirect;
    float exponent;
    float thickness; //Actually comes in reversed and represents "thinness"
    float offset;

    Light light;
};


half4 UniversalFragmentBlinnPhong_Grass(InputData inputData, SurfaceData surfaceData)
{
    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
    {
        return debugColor;
    }
    #endif

    uint meshRenderingLayers = GetMeshRenderingLayer();
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);

    float2 TerrainShadowMaskUV;
    TerrainShadowMaskUV.x = 1 - saturate(
        (inputData.positionWS.x - TerrainPosition.x) / (TerrainPosition.z - TerrainPosition.x));
    TerrainShadowMaskUV.y = saturate(
        (inputData.positionWS.z - TerrainPosition.y) / (TerrainPosition.w - TerrainPosition.y));
    TerrainShadowMaskUV.xy = TerrainShadowMaskUV * TerrainShadowMaskTex_Offset.xy + TerrainShadowMaskTex_Offset.zw;
    float TerrainShadowMask_Var = SAMPLE_TEXTURE2D(TerrainShadowMask, sampler_TerrainShadowMask, TerrainShadowMaskUV).r;
    mainLight.shadowAttenuation = min(mainLight.shadowAttenuation, TerrainShadowMask_Var);


    inputData.bakedGI *= surfaceData.albedo;
    surfaceData.specular = _SpecularInt;
    LightingData lightingData = CreateLightingData(inputData, surfaceData);
    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    #endif
    {
        lightingData.mainLightColor += CalculateBlinnPhong(mainLight, inputData, surfaceData);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            lightingData.additionalLightsColor += CalculateBlinnPhong(light, inputData, surfaceData);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            lightingData.additionalLightsColor += CalculateBlinnPhong(light, inputData, surfaceData);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * surfaceData.albedo;
    #endif

    return CalculateFinalColor(lightingData, surfaceData.alpha);
}


// General function to apply lighting based on the configured mode
half3 ApplyLighting(SurfaceData surfaceData, InputData inputData)
{
    half3 color = 0;


    color = UniversalFragmentBlinnPhong_Grass(inputData, surfaceData).rgb;
    //#endif


    return color;
}

int _OtherWorld2;

void ApplyDissolve(inout half3 color, float2 uv, float3 positionWS)
{
    #if _OTHERWORLD


    if (_OtherWorld2)
    {
        _DissolveBorderTintColor = 0;
        _DissolveTopBorderTintColor = 0;
    }
    // read black and white map
    half noise = SAMPLE_TEXTURE2D(_DissolveThresholdMap, sampler_linear_repeat, uv).g;
    half dissolveMapThresholdMapValue = uv.y;

    half noise2 = SAMPLE_TEXTURE2D(_DissolveThresholdMap2, sampler_linear_repeat, positionWS.xz*0.3).g;
    _DissolveAmount -= noise;
    _DissolveAmount *= noise2 * noise2;
    _DissolveTopBorderRange *= noise2 * noise2;
    _DissolveBorderRange *= noise2 * noise2;
    half dissolve = dissolveMapThresholdMapValue - _DissolveAmount;
    // clip threshold
    clip(dissolve);
    float dissolveBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveBorderRange, _DissolveAmount,
                                                   dissolveMapThresholdMapValue);
    dissolveBorderTintColorMask = lerp(0, dissolveBorderTintColorMask, _DissolveBorderTintColor.a);

    float dissolveTopBorderTintColorMask = smoothstep(_DissolveAmount + _DissolveTopBorderRange, _DissolveAmount,
                                                      dissolveMapThresholdMapValue);
    dissolveTopBorderTintColorMask = lerp(0, dissolveTopBorderTintColorMask, _DissolveTopBorderTintColor.a);
    //对 "近阈值区域 "进行HDR色彩调剂

    float3 DissolveBorderTintColor = dissolveBorderTintColorMask * _DissolveBorderTintColor.rgb;
    DissolveBorderTintColor = lerp(DissolveBorderTintColor, _DissolveTopBorderTintColor.rgb,
                                   dissolveTopBorderTintColorMask);
    color += DissolveBorderTintColor;

    //color = _DissolveAmount;
    // color = dissolveTopBorderTintColorMask;
    //color += dissolveTopBorderTintColorMask * _DissolveTopBorderTintColor.rgb ;
    #endif
}


#if defined(SHADERPASS_DEFERRED) && UNITY_VERSION >= 202120
FragmentOutput LightingPassFragment(Varyings input)
#else
void LightingPassFragment(Varyings input, out half4 outColor : SV_Target0
                          #ifdef _WRITE_RENDERING_LAYERS
	, out float4 outRenderingLayers : SV_Target1
                          #endif
)
#endif
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


    #if _OTHERWORLD

    float mdistance = distance(_MaskPosition_OtherWorld.xz, input.positionWS.xz);
    float mask = smoothstep(_MaskRadius_OtherWorld - _MaskDeflate_OtherWorld,
                            _MaskRadius_OtherWorld + _MaskDeflate_OtherWorld, mdistance);
    _BaseColor = lerp(_BaseColor_OtherWorld, _BaseColor, mask);
    _BaseColor_Down = lerp(_BaseColor_Down_OtherWorld, _BaseColor_Down, mask);
    _HueVariation = lerp(_HueVariation_OtherWorld, _HueVariation, mask);
    _DissolveAmount = lerp(_DissolveAmount,-0.5,mask);
    _ColorMapHeight = lerp(_ColorMapHeight_OtherWorld, _ColorMapHeight, mask);
    #endif


    SurfaceData surfaceData;
    //Can't use standard function, since including LitInput.hlsl breaks the SRP batcher
    ModifySurfaceData(input, surfaceData);

    //surfaceData.alpha = ComposeAlpha(surfaceData.alpha, _Cutoff, input.positionCS.xyz, input.positionWS.xyz, _FadeNear, _FadeFar, _FadeAngleThreshold);
    //return float4(surfaceData.alpha.xxx, 1.0);

    InputData inputData;
    //Standard URP function barely changes, but adds things like clear coat and detail normals
    PopulateLightingInputData(input, surfaceData.normalTS, inputData);

    #if UNITY_VERSION >= 202120
    #if defined(DEBUG_DISPLAY)
	SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv.xy, _BaseMap);
    #endif
    #endif

    //Debugging
    //return float4(AngleFadeFactor(input.positionWS, _FadeAngleThreshold).xxx, 1.0);
    //return float4(DistanceFadeFactor(input.positionWS, _FadeNear, _FadeFar).xxx, 1.0);
    //return float4(HeightDistanceWeight(input.positionWS.y, GetBendVector(input.positionWS).y).xxx * GetBendVector(input.positionWS).a, 1.0);
    //return float4(InterleavedNoise(input.positionCS.xy, PlayerFaceFactor(input.positionWS)).xxx, 1.0);

    #ifdef DEBUG_BEND_AREA
	float2 bendUV = GetBendMapUV(input.positionWS);
	return float4(any(bendUV.xy) ? bendUV.xy : float2(0,0), 0, 1.0);

	float edgeMask = BoundsEdgeMask(input.positionWS.xz);
	return float4(edgeMask.xxx, 1.0);
	return float4(lerp(float3(1,0,0), float3(0,1,0), edgeMask > 0 ? 1 : 0), 1.0);
    #endif

    //Get main light first, need attenuation to mask wind gust
    Light mainLight = GetMainLight(inputData.shadowCoord);


    #ifdef DEBUG_BEND_VECTORS
	float4 bendVector = GetBendVector(input.positionWS).xyzw;

	float dist = HeightDistanceWeight(input.positionWS, bendVector.xyz);
	//return float4(bendVector.aaa, 1.0);
	//return float4(saturate(bendVector.yyy), 1.0);
	surfaceData.albedo = saturate(bendVector * 0.5 + 0.5);
    #endif

    #if UNITY_VERSION >= 202120
    #ifdef _DBUFFER
	ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif
    #endif


    float3 finalColor = ApplyLighting(surfaceData, inputData);
    finalColor = MyFog(finalColor, inputData.positionWS);
    //finalColor = a;
    //finalColor = 	TerrainShadowMask_Var;
    float colorMapMask = clamp(0, 1, input.uv.y);
    colorMapMask = saturate(colorMapMask / 0.3);
    //	albedoAlpha.rgb = lerp(_BaseColor_Down.rgb, albedoAlpha.rgb, colorMapMask );
    ApplyDissolve(finalColor, 1 - input.uv, input.positionWS);
    surfaceData.alpha = colorMapMask;
    outColor = half4(finalColor, 1);
    outColor = ApplyTerrainBlend(outColor, input.positionWS, _Alpha);
    //outColor = float4(surfaceData.albedo,1);
    #ifdef _WRITE_RENDERING_LAYERS
	uint renderingLayers = GetMeshRenderingLayer();
	outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif

    //return half4(finalColor, surfaceData.alpha);
}
