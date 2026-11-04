#ifndef XHHY_TOON_URP_COMMON
#define XHHY_TOON_URP_COMMON

//#include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Custom/Common.hlsl"

///应用透视矫正（保留透视效果）
float4 ApplyPerspectiveRemoval(float3 positionWS,float3 centerPointPosition )
{
   
    
    float3 positionVS = TransformWorldToView(positionWS);
    float3 a = TransformWorldToView(centerPointPosition);
    positionVS.x -= a.x;
    float4 b = TransformWViewToHClip(a);
    float4  positionCS = TransformWViewToHClip(positionVS);
    
    positionCS.xyz /= positionCS.w;
    positionCS.x += b.x/b.w;
    positionCS.w = 1;
   return positionCS;
}




#endif
