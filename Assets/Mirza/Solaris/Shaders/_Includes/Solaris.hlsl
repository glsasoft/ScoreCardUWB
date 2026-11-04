#ifndef SOLARIS_HLSL
#define SOLARIS_HLSL

// Includes.

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

// Required for CornetteShanksPhaseFunction.

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/VolumeRendering.hlsl"

// ...

#pragma multi_compile _ _FORWARD_PLUS

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

//half4 GetShadowMask(float2 lightmapUV)
//{
//#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
//    return SAMPLE_SHADOWMASK(lightmapUV);
//#elif !defined (LIGHTMAP_ON)
//    return unity_ProbesOcclusion;
//#else
//    return half4(1.0, 1.0, 1.0, 1.0);
//#endif
//}

// TO-DO: Get rid of this and import VFX Toolkit's hash include from Vivid/VHS project.

float3 hash33(float3 p3)
{
    p3 = frac(p3 * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yxz + 33.33);
    return frac((p3.xxy + p3.yxx) * p3.zyx);
}

float3 Hash33(float2 screenPosition, float time)
{
    // XY, and Z.
    
    return hash33(float3(screenPosition, time));
}

// Generates independent noise for world space and light direction alignment.

float3 ShadowNoise(float2 screenPosition, float3 normal, float3 lightDirection, float3x3 worldToTangentMatrix, float worldNoise, float lightDirectionNoise, float lightDirectionOrthogonalAlignmentNoise, float lightDirectionAlignmentNoise)
{
    const int fps = 60;
    
    // Generate independent noise samples.
        
    float3 hashNoise1 = Hash33(screenPosition, _Time.y * fps);
    float3 hashNoise2 = Hash33(screenPosition * 1.37, (_Time.y + 13.27) * fps);
    
    //return hashNoise1 * worldNoise;

    // Center.

    hashNoise1 = (hashNoise1 * 2.0) - 1.0;
    hashNoise2 = (hashNoise2 * 2.0) - 1.0;

    // Light direction in tangent space.

    float3 lightTangentDirection = mul(lightDirection, worldToTangentMatrix);
    lightTangentDirection = normalize(lightTangentDirection);

    // Offset: independent noise samples to prevent unintended correlation.

    float3 randLightDir = hashNoise1;
    float3 randPlane = hashNoise2;

    // Project plane offset onto plane intersection.

    float3 planeAxis = normalize(cross(normal, lightTangentDirection) + 1e-6);
    
    randPlane -= dot(randPlane, normal) * normal;
    randPlane = dot(randPlane, planeAxis) * planeAxis;
    
    // Light direction alignment noise.
    
    float normalLightDirectionAlignment = abs(dot(lightDirection, normal));
    
    // To the power of 8.0, because I felt like it. It's nice and sharp.
    
    normalLightDirectionAlignment = pow(normalLightDirectionAlignment, 8.0);
    float inverseNormalLightDirectionAlignment = 1.0 - normalLightDirectionAlignment;
    
    worldNoise += lightDirectionOrthogonalAlignmentNoise * inverseNormalLightDirectionAlignment;
    lightDirectionNoise += lightDirectionAlignmentNoise * normalLightDirectionAlignment;
    
    // Compute final noise offset.
    
    return (randPlane * worldNoise) + (lightTangentDirection * (randLightDir * lightDirectionNoise));
}

void GetMainLightShadow_float(float3 position, float2 screenPosition, float worldNoise, float lightDirectionNoise, float lightDirectionOrthogonalAlignmentNoise, float lightDirectionAlignmentNoise, float3 normal, float3x3 worldToTangentMatrix, out float output)
{
    Light light = GetMainLight();
    
    // Apply noise offset.
    
    position += ShadowNoise(screenPosition, normal, light.direction, worldToTangentMatrix, worldNoise, lightDirectionNoise, lightDirectionOrthogonalAlignmentNoise, lightDirectionAlignmentNoise);

    // Output.

    output = MainLightRealtimeShadow(TransformWorldToShadowCoord(position));
}

// ...

//void GetMainLightDirection_float(out float output)
//{
//    output = GetMainLight().direction;
//}
float LightAnisotropy(float anisotropy, float3 lightDirection, float3 viewDirection)
{
    float cosTheta = dot(lightDirection, normalize(viewDirection));
    return CornetteShanksPhaseFunction(anisotropy, cosTheta);
}
void GetMainLightAnisotropy_float(float anisotropy, float3 viewDirection, out float output)
{
    output = LightAnisotropy(-anisotropy, GetMainLight().direction, normalize(viewDirection));
}

// Main light colour with anisotropy.

void GetMainLightColourFlat_float(float3 viewDirection, out float3 output)
{
    Light light = GetMainLight();    
    output = light.color * light.distanceAttenuation;
}
void GetMainLightColourLambert_float(float3 viewDirection, float3 normal, out float3 output)
{
    Light light = GetMainLight();    
    output = LightingLambert(light.color * light.distanceAttenuation, light.direction, normal);
}

//void GetMainLightSolaris(float3 flatColour, float3 lambertColour, float flatLambertBlend, float shadow, float anisotropy, float anisotropyBlend, float3 viewDirection, out float3 output)
//{
//    Light light = GetMainLight();
    
//    float3 colour = lerp(flatColour, lambertColour, flatLambertBlend);
//    float scattering = LightAnisotropy(anisotropy, light.direction, viewDirection);
    
//    colour *= shadow;
//    colour *= lerp(1.0f, scattering, anisotropyBlend);
    
//    output = colour;
//}

// Get specific light's shadow.

float GetAdditionalLightRealtimeShadow(float3 position, Light light, uint index)
{
    return AdditionalLightRealtimeShadow(index, position, light.direction);
    //return AdditionalLightRealtimeShadow(index, position, light.direction, GetAdditionalLightShadowParams(index), GetAdditionalLightShadowSamplingData(index));
}

InputData InitializeInputData(float2 screenPosition, float3 position)
{
    InputData inputData = (InputData) 0;
    
    inputData.positionWS = position;
    inputData.normalizedScreenSpaceUV = screenPosition;
    
    return inputData;
}

// Get light colour with shadows mixed in.

void GetAdditionalLights_float(float3 position, float2 screenUV, out float3 output)
{
    float3 colour = 0.0;
    
    // Forward+ rendering path needs this data before light loop.
    
    InputData inputData = InitializeInputData(screenUV, position);
    
    LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, position);        
        light.shadowAttenuation = GetAdditionalLightRealtimeShadow(position, light, lightIndex);
        
        colour += light.color * (light.distanceAttenuation * light.shadowAttenuation);
    }
    LIGHT_LOOP_END
    
    output = colour;
}

// Get both light colour and shadow separately.

void GetAdditionalLights_float(float3 position, float2 screenUV, out float3 outputColour, out float outputShadow)
{
    float3 colour = 0.0;
    float shadow = 1.0;
    
    // Forward+ rendering path needs this data before light loop.
    
    InputData inputData = InitializeInputData(screenUV, position);
    
    LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, position);
        
        colour += light.color * light.distanceAttenuation;
        shadow *= GetAdditionalLightRealtimeShadow(position, light, lightIndex);
    }
    LIGHT_LOOP_END
    
    outputColour = colour;
    outputShadow = shadow;
}

// Get light colour only.

void GetAdditionalLightsColour_float(float3 position, float2 screenUV, out float3 output)
{
    float3 colour = 0.0;
    
    // Forward+ rendering path needs this data before light loop.
    
    InputData inputData = InitializeInputData(screenUV, position);
    
    LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, position);        
        colour += light.color * light.distanceAttenuation;
    }
    LIGHT_LOOP_END
    
    output = colour;
}

// Get additional lights colour with lambert shading.

void GetAdditionalLightsColourFlat_float(float3 position, float2 screenUV, out float3 output)
{
    float3 colour = 0.0;
    
    // Forward+ rendering path needs this data before light loop.
    
    InputData inputData = InitializeInputData(screenUV, position);
    
    LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, position);
        colour += light.color * light.distanceAttenuation;
    }
    LIGHT_LOOP_END
    
    output = colour;
}
void GetAdditionalLightsColourLambert_float(float3 position, float2 screenUV, float3 normal, out float3 output)
{
    float3 colour = 0.0;
    
    // Forward+ rendering path needs this data before light loop.
    
    InputData inputData = InitializeInputData(screenUV, position);
        
    LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, position);             
        colour += LightingLambert(light.color * light.distanceAttenuation, light.direction, normal);
        
    }
    LIGHT_LOOP_END
    
    output = colour;
}

// Get light shadows only.

void GetAdditionalLightsShadow_float(float2 screenUV, float3 position, out float output)
{
    float shadow = 1.0;
    
    // Forward+ rendering path needs this data before light loop.
    
    InputData inputData = InitializeInputData(screenUV, position);
    
    LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, position);
        shadow *= GetAdditionalLightRealtimeShadow(position, light, lightIndex);
    }
    LIGHT_LOOP_END
    
    output = shadow;
}

// Get additional light shadows with noise diffusion.

void GetAdditionalLightsShadow_float(float3 position, float2 screenPosition, float2 screenUV, float worldNoise, float lightDirectionNoise, float lightDirectionOrthogonalAlignmentNoise, float lightDirectionAlignmentNoise, float3 normal, float3x3 worldToTangentMatrix, out float output)
{
    float shadow = 1.0;
    
    // Forward+ rendering path needs this data before light loop.
    
    InputData inputData = InitializeInputData(screenUV, position);        
    
    LIGHT_LOOP_BEGIN(GetAdditionalLightsCount())
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, position);
        
        float3 shadowSamplePosition = position;
        shadowSamplePosition += ShadowNoise(screenPosition, normal, light.direction, worldToTangentMatrix, worldNoise, lightDirectionNoise, lightDirectionOrthogonalAlignmentNoise, lightDirectionAlignmentNoise);
        
        shadow *= GetAdditionalLightRealtimeShadow(shadowSamplePosition, light, lightIndex);
    }
    LIGHT_LOOP_END
    
    output = shadow;
}

#endif