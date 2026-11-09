// =============================================================================================================================
// XHHY 角色渲染 —— 描边（挤出）通道顶点/片段着色器（Character / Face 两套共用）
//
// 合并自：
//   XHHYCharacter\XHHYCharacter_OutlineForwardPass.hlsl
//   Face\XHHYCharacter_OutlineForwardPass_Face.hlsl
//
// 两份原本几乎完全一致，差异只有三处，这里统一以 Face 版为准：
//   1. Character 版的 "禁用渲染" 判断整段被注释掉了（_EnableRendering / _RenderOutline 不生效），Face 版是启用的
//   2. Character 版末尾多一句 color.a = _Alpha（Face 版把它注释掉了）
//   3. _NILOTOON_FORCE_MINIMUM_SHADER 与 _NILOTOON_DITHER_FADEOUT 两段的先后顺序不同
//
// 使用约定：本文件只放函数体，不 include 任何东西。调用方（各组的薄壳 .hlsl）负责先 include
// 自己那一组的 XHHYCharacter_LightUtil[_Face|_Eye].hlsl 和 XHHYUtilityHLSL/Common.hlsl。
// =============================================================================================================================
#pragma once

// 描边挤出方向。
// 首选烘焙在 uv8(TEXCOORD7) 里的切线空间平滑法线，它是连续性最好的挤出方向；
// 但模型如果没做过平滑法线烘焙，uv8 全是 0，normalize(0) 得到零向量，壳不会往外挤、
// 会被本体深度挡住，表现为「完全没有描边」。这种情况退回顶点法线，至少保证描边出得来。
float3 GetOutlineExtrudeDirectionWS(Attributes input, VertexNormalInputs vertexNormalInput)
{
    if (dot(input.uv8, input.uv8) > 1e-6)
    {
        return ConvertNormalTSToNormalTargetSpace(input.uv8,
                                                  vertexNormalInput.tangentWS,
                                                  vertexNormalInput.bitangentWS,
                                                  vertexNormalInput.normalWS);
    }
    return normalize(vertexNormalInput.normalWS);
}

// 做常规的MVP变换 + 轮廓挤出顶点 + 所有与轮廓有关的任务
Varyings VertexShaderOutLine(Attributes input)
{
    Varyings output = (Varyings)0;
    bool shouldDisableRendering = !_EnableRendering;
    // 不渲染，不会污染URP的shadowmap和depth texture/depth normal texture
    //shouldDisableRendering = shouldDisableRendering || (_DitherFadeoutAmount == 1) || (_DissolveAmount == 1);
    shouldDisableRendering = shouldDisableRendering || !_RenderOutline; // 因为只有少量的材质希望不渲染轮廓，比如眼球、眉毛、牙齿....
    if (shouldDisableRendering)
    {
        output.positionCS.w = 0;
        return output;
    }

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    #if _NILOTOON_FORCE_MINIMUM_SHADER //调试用 ,shader编译流程会自动剔除
     output.positionCS = TransformObjectToHClip(input.positionOS + input.normalOS * 0.005); // any visible debug outline width is ok

    //返回最小的可变结构数据，只渲染_BaseMap（与Unlit相同）.
    output.uv = input.uv;

    return output;
    #endif

   // ApplyExternalAssetSupportLogicToVertexAttributeAtVertexShaderStartForOutLine(input);

    VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    float3 positionWS = 0;
    float3 positionVS = vertexPositionInput.positionVS;
    _OutlineWidth *= _OutlineWidthExtraMultiplier;
    float finalOutlineWidth = _OutlineWidth;
    {
        // outline宽度与顶点颜色相乘
        finalOutlineWidth = _UseOutlineWidthMaskFromVertexColor
                                ? finalOutlineWidth * dot(input.color, _OutlineWidthMaskFromVertexColor)
                                : finalOutlineWidth;

        /*
        //在平面反射过程中，_GlobalShouldDisableNiloToonZOffset将被设置为true（=ZOffset被禁用）。
        if (_GlobalShouldDisableNiloToonZOffset)
        {
            finalOutlineWidth = _Cull == 0 ? 0 : finalOutlineWidth;//在Cull Off时才禁用轮廓。
        }
        */

        //对轮廓进行定位挤出
        positionWS = TransformPositionWSToOutlinePositionWS(10, vertexPositionInput, vertexNormalInput,
                                                            input.uv8);
    }

    //decal///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    #if _DECAL
     //UV的起始位置（左上角的格子）
    float2 uv = input.uv2;
    float Sequence = _Sequence-1;
     uv.x = uv.x + 1/_LineNumber * Sequence;
    uv.y = uv.y - 1/_ColumnNumber * floor(Sequence/_LineNumber);
    output.uv2 = uv;
    #endif
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #if _NILOTOON_DEBUG_SHADING  //编译阶段自动剔除
    output.uv8 = input.uv8;      // 在片段着色器中显示切线空间平滑的法线作为调试颜色。
    #endif

    output.positionWS_ZOffsetFinalSum.xyz = positionWS;
    output.viewDirectionWS = GetWorldSpaceViewDir(positionWS); // this will support orthographic camera view direction fix

    #if VaryingsHasTangentWS
    half sign = input.tangentOS.w * GetOddNegativeScale();
    output.tangentWS = half4(vertexNormalInput.tangentWS.xyz, sign);
    #endif

    positionWS = vertexPositionInput.positionWS; //如果使用了 Extrude posWS outlint ，ws就被编辑过了，cs需要重新计算
   // output.positionCS = TransformWorldToHClip(positionWS);

    //处理Outline 宽度
    float3 extrudeDirectionWS = GetOutlineExtrudeDirectionWS(input, vertexNormalInput);
    float3 extrudeDirectionWS2 = extrudeDirectionWS;
    extrudeDirectionWS = TransformWorldToHClipDir(extrudeDirectionWS);
    float4 nearUpperRight = mul(UNITY_MATRIX_I_VP,float4(1,1,UNITY_NEAR_CLIP_VALUE,_ProjectionParams.y));
    float sapect = abs(nearUpperRight.y / nearUpperRight.x);
    //extrudeDirectionWS = normalize(extrudeDirectionWS) ;
    // extrudeDirectionWS.x *= sapect;
    float mask = saturate((output.positionCS.w )/10);

   // output.positionCS.xy += extrudeDirectionWS.xy * finalOutlineWidth * 0.001 *pow(mask,1.1)* 10   * _CurrentCameraFOV/50 ;


   // output.positionCS = TransformWorldToHClip(positionWS);
    //找出面的区域，如果顶点在面的区域内，就编辑normalWS。///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    half4 lightingNormalWS_faceArea = GetLightingNormalWS_FaceArea(vertexNormalInput.normalWS, positionWS, output.uv);
    output.normalWS_averageShadowAttenuation.xyz = lightingNormalWS_faceArea.xyz;
    //已经被GetLightingNormalWS_FaceArea(...)规范化。

    //Vertex  color
    output.color = input.color;
    output.dissolveHeight = GetDissolveHeightOS(positionWS.y - _DissolveRootY);

    // 平均阴影衰减
    //使用LOAD_TEXTURE2D而不是tex2dLOD来简化uv代码，同时也是为了更好的性能。_NiloToonAverageShadowMapRT是一个 "N x 1 "的纹理，所以LOAD索引是(charID,0)
    output.normalWS_averageShadowAttenuation.w = 1; // floating point RT, so r channel
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    extrudeDirectionWS = GetOutlineExtrudeDirectionWS(input, vertexNormalInput);


    //  extrudeDirectionWS = TransformWorldToHClipDir(extrudeDirectionWS);
    nearUpperRight = mul(UNITY_MATRIX_I_VP,float4(1,1,UNITY_NEAR_CLIP_VALUE,_ProjectionParams.y));
    sapect = abs(nearUpperRight.y / nearUpperRight.x);
    //extrudeDirectionWS = normalize(extrudeDirectionWS) ;
    // extrudeDirectionWS.x *= sapect;

    // 摄像机距离 / FOV 补偿：让描边在屏幕上保持恒定宽度，角色离相机远近都长一样粗。
    // 透视相机按 |viewZ| * FOV 放大，正交相机按 orthoSize 换算。
    //
    // 注意：_CurrentCameraFOV 和 _GlobalOutlineWidthAutoAdjustToCameraDistanceAndFOV 这两个全局量
    // 在本工程里没有任何 C# 在写，GPU 上恒为 0；直接传它们会让补偿结果乘成 0，描边整条消失。
    // 所以 FOV 改成从投影矩阵现算（GetCameraFOV），补偿百分比固定成全量 1.0。
    float outlineFixFov = GetCameraFOV();
    float outlineDistFix = GetOutlineCameraFovAndDistanceFixMultiplier(
        vertexPositionInput.positionVS.z,
        outlineFixFov,
        1.0);

    positionWS.xyz += extrudeDirectionWS.xyz * finalOutlineWidth * outlineDistFix;


    // 消除透视相机的失真
    output.positionCS = NiloDoPerspectiveRemoval(output.positionCS, positionWS, _HeadBonePositionWS,
                                                 _PerspectiveRemovalRadius, _PerspectiveRemovalAmount,
                                                 _PerspectiveRemovalStartHeight, _PerspectiveRemovalEndHeight);

    #if _PERSPECTIVEREMOVAL
   // output.positionCS = ApplyPerspectiveRemoval(positionWS,_CenterPointPosition);
    #endif


    return output;
}

// [第二遍挤出的Cull前部轮廓] //应用着色
void ApplySurfaceToOutlineColorEditIfOutlinePass(inout half3 originalSurfaceColor, ToonMap toon_map)
{

    float OutlinerampLightingTexSamplingUvY;
    OutlinerampLightingTexSamplingUvY = toon_map._MaskMap_Var.a;
    half Outlinecolormask1 = step(0.0f, OutlinerampLightingTexSamplingUvY) * step(OutlinerampLightingTexSamplingUvY, 0.2);
    half Outlinecolormask2 = step(0.2f, OutlinerampLightingTexSamplingUvY) * step(OutlinerampLightingTexSamplingUvY, 0.4f);
    half Outlinecolormask3 = step(0.4f, OutlinerampLightingTexSamplingUvY) * step(OutlinerampLightingTexSamplingUvY, 0.6f);
    half Outlinecolormask4 = step(0.6f, OutlinerampLightingTexSamplingUvY) * step(OutlinerampLightingTexSamplingUvY, 0.85f);
    half Outlinecolormask5 = step(0.85f, OutlinerampLightingTexSamplingUvY) * step(OutlinerampLightingTexSamplingUvY, 1.01f);

    originalSurfaceColor = Outlinecolormask1 * _OutlineTintColor5 + Outlinecolormask2 * _OutlineTintColor4 +
        Outlinecolormask3 * _OutlineTintColor3 + Outlinecolormask4 * _OutlineTintColor2 + Outlinecolormask5 *
        _OutlineTintColor1;
}

half4 FragmentShaderOutLineNoLight(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    input.normalWS_averageShadowAttenuation.xyz *= facing;

    #if _NILOTOON_FORCE_MINIMUM_SHADER
    return Get_NILOTOON_FORCE_MINIMUM_FRAGMENT_SHADER_result(input,1);
    #endif
    #if _NILOTOON_DITHER_FADEOUT
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1-_DitherFadeoutAmount);
    #endif
    ToonMap toonMap = GetTexture(input);

    half4 color = toonMap._BaseMap_Var;
    color *= _BaseColor;

    ApplySurfaceToOutlineColorEditIfOutlinePass(color.xyz, toonMap);
    ApplyPerCharacterEffectColorEditForwardPass(color.rgb);
    ApplyDissolve(color.xyz, input);

    // 描边画在 BeforeRenderingPostProcessing，会跟着角色一起进 Bloom / 色调映射。
    // 这里如果再把描边 lerp 向雾色（尤其是高亮雾色），会直接被 Bloom 放大成过曝光晕，
    // 所以描边只保留自身颜色，不吃雾。
    return color;
}
