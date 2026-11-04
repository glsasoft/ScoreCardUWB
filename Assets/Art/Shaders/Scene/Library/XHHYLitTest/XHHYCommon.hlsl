#ifndef XHHYCOMMON
#define XHHYCOMMON





float3 _MaskPosition;
float _MaskRadius;
float _MaskDeflate;
float4 _MaskUV;
float2 _MaskUV_Offset;
float4 _CloorTest;
float4 _MaskUVSpeed;
TEXTURE2D(_MapMask);
SAMPLER(sampler_MapMask);

TEXTURE2D(_MapTex);
SAMPLER(sampler_MapTex);

/// <summary>
/// 应用选关界面地图遮罩效果
/// </summary>
void ApplyOpenWorldMask(inout  float4 color , float3 positionWS ,float3 positionCS)
{
   // _MaskRadius = 0.7;
    float2 mapTexUV = saturate((positionWS.xz - _MaskUV.xy)/(_MaskUV.zw - _MaskUV.xy));
    float3 _MapTex_Var =   SAMPLE_TEXTURE2D(_MapTex, sampler_MapTex,mapTexUV).rgb;
  
    float2  mapMaskUV = positionCS.xy/_ScreenParams.xy;
    mapMaskUV = (mapMaskUV -0.5 + _MaskUV_Offset) * _MaskRadius +0.5 + _MaskUV_Offset;
 float2  mapMaskUV2 = (positionCS.xy/_ScreenParams.xy) *_MaskUVSpeed.z  + _MaskUVSpeed.xy * _Time.y;
   float noise = SAMPLE_TEXTURE2D(_MapMask, sampler_MapMask,mapMaskUV2).b;
    float2 _BaseMapMask_Var = 1-SAMPLE_TEXTURE2D(_MapMask, sampler_MapMask,mapMaskUV + (noise * 2 -1) * _MaskUVSpeed.w).rg;
   float3 c2 =  lerp(_MapTex_Var.rgb,_CloorTest.rgb,  _BaseMapMask_Var.g * _CloorTest.a );
 
    color.rgb = lerp(color.rgb,c2 , 1- _BaseMapMask_Var.r);
    //color.rgb = mapMaskUV.r;
}


void ApplyOpenWorldMask_lod(inout  float4 color , float3 positionWS ,float3 positionCS)
{
 float mdistance = distance(_MaskPosition.xz,positionWS.xz);
 float mask = smoothstep(_MaskRadius - _MaskDeflate,_MaskRadius + _MaskDeflate,mdistance);
 float2 MaskUV = saturate((positionWS.xz - _MaskUV.xy)/(_MaskUV.zw - _MaskUV.xy));
 float3 c2 =   SAMPLE_TEXTURE2D(_MapTex, sampler_MapTex,MaskUV).rgb;
    
 color.rgb = lerp(color.rgb,c2,mask);
}

float4 _MinMaxHeightZ;
float4 _worldMinMaxHeightZ;
float4 _MinMaxMapPos;
TEXTURE2D(_heightMap);
SAMPLER(sampler_heightMap);
/// <summary>
/// 应用地形混合效果
/// </summary>
 float4 ApplyTerrainBlend(float4 color,float3 positionWS,float _LocalFadeSize )
{
     
     float2 uv;
     uv.x = saturate((positionWS.x - _MinMaxMapPos.x) / (_MinMaxMapPos.z - _MinMaxMapPos.x));
     uv.y = saturate((positionWS.z - _MinMaxMapPos.y) / (_MinMaxMapPos.w - _MinMaxMapPos.y));
     // sample the texture
     float4 depth = SAMPLE_TEXTURE2D(_heightMap, sampler_heightMap,   uv).r;
     depth = (_worldMinMaxHeightZ.y - _worldMinMaxHeightZ.x) * depth + _worldMinMaxHeightZ.x;
  #if _TerrainBlend_DEBUG
  color.rgb = saturate(abs(positionWS.y - depth.x) > 0.1 ? 0 : abs(positionWS.y - depth.x)/0.1  );
  color.a = 1;
  return color;
  #endif
     float mask =  saturate((positionWS.y - depth.x)* _LocalFadeSize  * 5) ;
     uv = smoothstep(0,0.05,uv) * smoothstep(1,0.95,uv);
     mask = lerp(1,mask, uv.x*uv.y);
     color.a *= mask;
     return  color;
     
}

#endif
