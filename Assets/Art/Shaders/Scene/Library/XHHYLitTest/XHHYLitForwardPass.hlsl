#ifndef XHHY_LIT_FORWARDPASS
#define XHHY_LIT_FORWARDPASS

//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Custom/BlendGround.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
//#include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Custom/Common.hlsl"
// GLES2 has limited amount of interpolators


#if defined(_TERRAINBLEND) 
#define SCENE_SHADOW 1
#endif

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    float3 positionWS               : TEXCOORD1;
    float3 normalWS                 : TEXCOORD2;
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
#else
    half  fogFactor                 : TEXCOORD5;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                : TEXCOORD7;
#endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
#endif

    //************** CUSTOM ADD START ***************//
    float3 uv2AndEyeZ  :   TEXCOORD10;
    float4 screenPos  :   TEXCOORD11;
    //************** CUSTOM ADD END ***************//
    
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData_XHHY inputData)
{
    inputData = (InputData_XHHY)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
//#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

  //  #if defined(_NORMALMAP)
    inputData.tangentToWorld = tangentToWorld;
   // #endif
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
//#else
//    inputData.normalWS = input.normalWS;
//#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;
    inputData.screenUV = input.screenPos.xy/input.screenPos.w;
    inputData.offsetDepth = SAMPLE_TEXTURE2D_LOD(_CameraDepthTexture, sampler_CameraDepthTexture,
                                                          inputData.screenUV,0).r;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)

    #if SCENE_SHADOW || BlendGroundPass
    
    float2 screenUV = inputData.screenUV;
    float offsetDepth = inputData.offsetDepth;

    float3 positionWS =  ComputeWorldSpacePosition(input.screenPos.xy/input.screenPos.w, offsetDepth, unity_MatrixInvVP);

    
    inputData.shadowCoord = TransformWorldToShadowCoord(positionWS);
    #else
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #endif
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#else
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif


   
    
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
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




///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

   
   
    
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
 
    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
  //  normalInput.normalWS = lerp(normalInput.normalWS,float3(0,0,1),mask);
   // normalInput.normalWS = float3(0,0,1);
  //  half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
   output.uv.xy = SetBaseMapUV(input.texcoord.xy);
    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
//#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
//#endif
//#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
//#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
    output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;
    //************** CUSTOM ADD START ***************//
    output.uv2AndEyeZ.z = -vertexInput.positionVS.z;//. -TransformWorldToView(vertexInput.positionWS).z;
    output.screenPos = ComputeScreenPos(output.positionCS);
    //************** CUSTOM ADD END ***************//

    
    return output;
}


//float4 _MaskUV;

// Used in Standard (Physically Based) shader
void LitPassFragment(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif
    
#if _OTHERWORLD
   
    float mdistance = distance(_MaskPosition_OtherWorld.xz, input.positionWS.xz);
    float otherMask = smoothstep(_MaskRadius_OtherWorld - _MaskDeflate_OtherWorld,
                            _MaskRadius_OtherWorld + _MaskDeflate_OtherWorld, mdistance);
    _BaseColor = lerp(_BaseColor_OtherWorld,_BaseColor,otherMask);
    #endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData,input.positionWS.xyz,input.normalWS);



    InputData_XHHY inputData;

  //  InputData a;
    InitializeInputData(input, surfaceData.normalTS, inputData);


    
    #if _OTHERWORLD
    inputData.bakedGI = lerp(inputData.bakedGI * _GIColor_OtherWorld,inputData.bakedGI,otherMask);
    #endif


    
    InputData inputData_lod;
    InputData_XHHYToInputData(inputData,inputData_lod);
    SETUP_DEBUG_TEXTURE_DATA(inputData_lod, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

   
    
    half4 color = UniversalFragmentPBR(inputData_lod, surfaceData);

    
    half NdotV = dot( inputData.normalWS,normalize(_WorldSpaceCameraPos - input.positionWS));
    ApplyPerCharacterEffectColorEdit(color.rgb,NdotV);

    ApplyDissolve(color.rgb,input.uv);
    
    //地图遮罩
    #if _OPENWORLD
    ApplyOpenWorldMask_lod(color ,input.positionWS.xyz,input.positionCS.xyz );
    #endif
    
   // color.rgb = MyFog(color.rgb, inputData.positionWS);
   // color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

   



   


    #if _TerrainBlend 
    {
        //   half3 blendTex = SAMPLE_TEXTURE2D(_TerrainBlendTexture,sampler_TerrainBlendTexture,inputData.screenUV).rgb;
    
        color =   ApplyTerrainBlend(color , input.positionWS,_LocalFadeSize);
      //  color = 0.5;
    }
    #endif

    #if _OPENWORLD
    float mdistance = distance(_MaskPosition.xz,inputData.positionWS.xz);
    float mask = smoothstep(_MaskRadius - _MaskDeflate,_MaskRadius + _MaskDeflate,mdistance);
    float2 MaskUV = saturate((inputData.positionWS.xz - _MaskUV.xy)/(_MaskUV.zw - _MaskUV.xy));
    float3 c2 =   SAMPLE_TEXTURE2D(_MapTex, sampler_MapTex,MaskUV).xyz;
    
    color.rgb = lerp(color.rgb,c2,mask);
    //  color.rgb = 1;
    #endif
    
    
 outColor =  color;
    #if BACKNORMOL
    outColor.rgb = step(_waterHeight, inputData.positionWS.y);
    outColor.a = 1;
    #endif
    
/*#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif*/
}



/*void LitPassFragment_Low(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif
    
    #if _OTHERWORLD
   
    float mdistance = distance(_MaskPosition_OtherWorld.xz, input.positionWS.xz);
    float otherMask = smoothstep(_MaskRadius_OtherWorld - _MaskDeflate_OtherWorld,
                            _MaskRadius_OtherWorld + _MaskDeflate_OtherWorld, mdistance);
    _BaseColor = lerp(_BaseColor_OtherWorld,_BaseColor,otherMask);
    #endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData,input.positionWS.xyz,input.normalWS);



    InputData_XHHY inputData;

    //  InputData a;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    InputData inputData_lod;
    InputData_XHHYToInputData(inputData,inputData_lod);
    SETUP_DEBUG_TEXTURE_DATA(inputData_lod, input.uv, _BaseMap);

    #ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif

    #if _OTHERWORLD
    inputData.bakedGI = lerp(inputData.bakedGI * _GIColor_OtherWorld,inputData.bakedGI,otherMask);
    #endif
 
    
    half4 color = UniversalFragmentBlinnPhong2(inputData_lod, surfaceData);

    
    half NdotV = dot( inputData.normalWS,normalize(_WorldSpaceCameraPos - input.positionWS));
  //  ApplyPerCharacterEffectColorEdit(color.rgb,NdotV);

    ApplyDissolve(color.rgb,input.uv);
    
    //地图遮罩
    #if _OPENWORLD
    ApplyOpenWorldMask_lod(color ,input.positionWS.xyz,input.positionCS.xyz );
    #endif
    
    color.rgb = MyFog(color.rgb, inputData.positionWS);
    // color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

   



   


    #if _TerrainBlend 
    {
        //   half3 blendTex = SAMPLE_TEXTURE2D(_TerrainBlendTexture,sampler_TerrainBlendTexture,inputData.screenUV).rgb;
    
        color =   ApplyTerrainBlend(color , input.positionWS,_LocalFadeSize);
        //  color = 0.5;
    }
    #endif
 
    outColor =  color;
    
    /*#ifdef _WRITE_RENDERING_LAYERS
        uint renderingLayers = GetMeshRenderingLayer();
        outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif#1#
}*/






// Used in Standard (Physically Based) shader
void LitPassFragment2(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif
    


    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData,input.positionWS.xyz,input.normalWS);



    InputData_XHHY inputData;

    //  InputData a;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    InputData inputData_lod;
    InputData_XHHYToInputData(inputData,inputData_lod);
    SETUP_DEBUG_TEXTURE_DATA(inputData_lod, input.uv, _BaseMap);

    #ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif

   
    
    half4 color = UniversalFragmentPBR(inputData_lod, surfaceData);

    
    half NdotV = dot( inputData.normalWS,normalize(_WorldSpaceCameraPos - input.positionWS));
  //  ApplyPerCharacterEffectColorEdit(color.rgb,NdotV);

    ApplyDissolve(color.rgb,input.uv);
    
    //地图遮罩
    #if _OPENWORLD
    ApplyOpenWorldMask_lod(color ,input.positionWS.xyz,input.positionCS.xyz );
    #endif
    
   // color.rgb = MyFog(color.rgb, inputData.positionWS);
    // color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

   



   


    #if _TerrainBlend 
    {
        //   half3 blendTex = SAMPLE_TEXTURE2D(_TerrainBlendTexture,sampler_TerrainBlendTexture,inputData.screenUV).rgb;
    
        color =   ApplyTerrainBlend(color , input.positionWS,_LocalFadeSize);
        //  color = 0.5;
    }
    #endif
 
    outColor =  color;
    outColor = GetMainLight(inputData_lod.shadowCoord).shadowAttenuation;
    
    /*#ifdef _WRITE_RENDERING_LAYERS
        uint renderingLayers = GetMeshRenderingLayer();
        outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif*/
}






#endif
