#pragma once
//#include "XHHYCharacter_LightUtil.hlsl" //光照相关

// all pass will share this Attributes struct (define data needed from Unity app to our vertex shader)
struct ThickOutlineAttributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL; // GetVertexNormalInputs(...) 希望得到float3 normalOS的输入，不要写半3产生不必要的类型转换成本。
    float4 tangentOS : TANGENT; // GetVertexNormalInputs(...) 希望得到浮点4的正切输入，不要写成半4产生不需要的类型转换成本。
    float2 uv : TEXCOORD0;
    float3 uv8 : TEXCOORD7; //用于存储uv8(TEXCOORD7)中轮廓的角度平滑的切线空间法线。
};

struct ThickOutlineVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
};


// 做常规的MVP变换 + 轮廓挤出顶点 + 所有与轮廓有关的任务
ThickOutlineVaryings VertexShaderOutLine(ThickOutlineAttributes input)
{
    ThickOutlineVaryings output = (ThickOutlineVaryings)0;
    /*bool shouldDisableRendering = !_EnableRendering;
    // 不渲染，不会污染URP的shadowmap和depth texture/depth normal texture
    shouldDisableRendering = shouldDisableRendering || (_DitherFadeoutAmount == 1) || (_DissolveAmount == 1);
    shouldDisableRendering = shouldDisableRendering ; // 因为只有少量的材质希望不渲染轮廓，比如眼球、眉毛、牙齿....
    if (shouldDisableRendering)
    {
        output.positionCS.w = 0;
        return output;
    }*/

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    
    #if _NILOTOON_FORCE_MINIMUM_SHADER //调试用 ,shader编译流程会自动剔除
     output.positionCS = TransformObjectToHClip(input.positionOS + input.normalOS * 0.005); // any visible debug outline width is ok
    
    //返回最小的可变结构数据，只渲染_BaseMap（与Unlit相同）.
   // output.uv = input.uv;

    return output;    
    #endif

   // ApplyExternalAssetSupportLogicToVertexAttributeAtVertexShaderStartForOutLine(input);

   // VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

   
    
    float finalOutlineWidth = _OutlineWidth;
   
    finalOutlineWidth = _ThickOutWidth /*+ 2 * abs(frac(_Time.y)-0.5)*2*/;
   
   
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

   

   
    
    #if VaryingsHasTangentWS
    half sign = input.tangentOS.w * GetOddNegativeScale();
    #endif

    output.positionCS = TransformObjectToHClip(input.positionOS);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz).xyz;
    //处理Outline 宽度
    float3 extrudeDirectionWS = ConvertNormalTSToNormalTargetSpace(input.uv8, vertexNormalInput.tangentWS,
                                                            vertexNormalInput.bitangentWS,
                                                            vertexNormalInput.normalWS);
    
    extrudeDirectionWS = TransformWorldToHClipDir(extrudeDirectionWS);
    float4 nearUpperRight = mul(UNITY_MATRIX_I_VP,float4(1,1,UNITY_NEAR_CLIP_VALUE,_ProjectionParams.y));
    float sapect = abs(nearUpperRight.y / nearUpperRight.x);
    output.positionCS.xy += extrudeDirectionWS.xy * finalOutlineWidth*0.001 * output.positionCS.w * _CurrentCameraFOV/50;
    
    #if _PERSPECTIVEREMOVAL
    output.positionCS = ApplyPerspectiveRemoval( output.positionWS,_CenterPointPosition);
    #endif

    
    return output;
}


half4 FragmentShaderOutLine(ThickOutlineVaryings input, half facing : VFACE) : SV_TARGET
{
  //  _CenterPointPosition;
    float a = abs(input.positionWS.y - _CenterPointPosition.y);
    a= smoothstep(-0.1,_ThickOutAlphaHeight,a);
   // a = 1;
   // _ThickOutColor = float4(0.5,0,0,1);
    return float4(_ThickOutColor.rgb,a);
    }
