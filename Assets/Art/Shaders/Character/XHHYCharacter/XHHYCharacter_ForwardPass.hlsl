/*
 * 文件名: XHHYCharacter_ForwardPass.hlsl
 * 作用: XHHY角色渲染系统的前向渲染通道实现
 * 作者: XHHY团队
 * 日期: 2024
 * 
 * 描述: 
 * 该文件实现了角色渲染的前向渲染通道，包含顶点着色器和片段着色器的实现。
 * 支持多种渲染特性：基础光照、高光反射、MatCap、溶解效果、雾效等。
 * 提供了三个不同的片段着色器变体以适应不同的渲染需求。
 */

#pragma once

#include "XHHYCharacter_LightUtil.hlsl" // 光照相关工具函数
#include "../XHHYUtilityHLSL/Common.hlsl" // 通用工具方法（原路径 "XHHYUtilityHLSL/Common.hlsl" 相对本目录找不到，改成显式相对路径）

//TEXTURE2D(_NiloToonAverageShadowMapRT); // 平均阴影贴图渲染纹理（已注释）

/**
 * 前向渲染顶点着色器
 * @param input 顶点属性输入结构体，包含位置、法线、切线、UV等信息
 * @return Varyings 输出到片段着色器的插值数据结构体
 * 
 * 功能说明:
 * - 计算顶点的世界空间位置和裁剪空间位置
 * - 处理法线和切线的世界空间变换
 * - 计算UV坐标变换
 * - 准备光照计算所需的各种向量
 */
Varyings VertexShaderForward(Attributes input)
{
    Varyings output = (Varyings)0; // 初始化输出结构体

    // 获取顶点位置相关的输入数据（世界空间、裁剪空间等）
    VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(input.positionOS);
    // 获取顶点法线相关的输入数据（世界空间法线、切线、副切线）
    VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    #if VaryingsHasTangentWS
    output.tangentWS = half4(vertexNormalInput.tangentWS, 1.0);
    #endif
    float3 positionWS = vertexPositionInput.positionWS; // 世界空间位置

    // 使用TRANSFORM_TEX宏进行UV坐标变换，应用贴图的平铺和偏移
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

    // 设置世界空间位置
    output.positionWS_ZOffsetFinalSum.xyz = positionWS;
    // 计算世界空间视线方向
    output.viewDirectionWS = GetWorldSpaceViewDir(positionWS);

    #if _NORMALMAP
    // 如果启用了法线贴图，计算切线空间相关数据
    // 获取切线的符号位，用于确定副切线的方向
    half sign = input.tangentOS.w * GetOddNegativeScale();
    output.tangentWS = half4(vertexNormalInput.tangentWS.xyz, sign);
    #endif

    // 设置裁剪空间位置
    output.positionCS = vertexPositionInput.positionCS;

    // 设置世界空间法线
    output.normalWS_averageShadowAttenuation.xyz = vertexNormalInput.normalWS;

    // 传递顶点颜色
    output.color = input.color;
    // 溶解用的局部 Y 轴高度（0 = 头顶，1 = 脚底）
    // 世界 Y 减去角色根节点的世界 Y，得到角色自身的高度。
    // 不要用 input.positionOS.y：在蒙皮网格上它和 bind pose 的模型高度对不上，方向会串。
    // 基准用 uniform 而不是每个渲染器自己的矩阵，否则头发、身体原点不同会互相错位。
    output.dissolveHeight = GetDissolveHeightOS(positionWS.y - _DissolveRootY);
    // 设置视空间深度值
    output.depth = vertexPositionInput.positionVS.z;

    // 平均阴影衰减初始化为1（浮点RT，使用r通道）
    output.normalWS_averageShadowAttenuation.w = 1;

    // 处理额外的法线数据（uv8通道）
    VertexNormalInputs vertexNormalInput2 = GetVertexNormalInputs(input.uv8, input.tangentOS);
    // 构建TBN矩阵（切线-副切线-法线矩阵）
    float3x3 TBNMatrixTargetSpace = float3x3(vertexNormalInput.tangentWS, vertexNormalInput.bitangentWS,
                                             vertexNormalInput.normalWS);
	// 消除透视相机的失真
    output.positionCS = NiloDoPerspectiveRemoval(output.positionCS, positionWS, _HeadBonePositionWS,
                                                 _PerspectiveRemovalRadius, _PerspectiveRemovalAmount,
                                                 _PerspectiveRemovalStartHeight, _PerspectiveRemovalEndHeight);										 

    // 将uv8从切线空间变换到世界空间，然后标准化并转换到视空间
    output.uv8 = mul(input.uv8, TBNMatrixTargetSpace);
    output.uv8 = normalize(output.uv8);
    output.uv8 = TransformWorldToViewNormal(output.uv8);
    
    return output;
}

/**
 * 基于物理的光照计算函数（Nilo风格的卡通化版本）
 * @param diffuse 漫反射颜色
 * @param lightColor 光源颜色
 * @param lightDirectionWS 世界空间光照方向
 * @param lightAttenuation 光照衰减系数
 * @param normalWS 世界空间法线
 * @return half3 计算后的光照颜色
 * 
 * 功能说明:
 * - 计算法线与光照方向的点积
 * - 使用smoothstep函数实现卡通化的光照过渡
 * - 应用光照衰减和颜色混合
 */
half3 LightingPhysicallyBased_Nilo(float3 diffuse,
                                   half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
                                   half3 normalWS)
{
    // 计算法线与光照方向的点积，并限制在[0,1]范围内
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    
    // 使用smoothstep实现卡通化的光照过渡效果
    // 将NdotL从[-1,1]映射到[0,1]，然后应用平滑过渡
    NdotL = smoothstep(0.4, 0.5, NdotL * 0.5 + 0.5);
    
    // 计算光照辐射度（考虑光照颜色、衰减和法线点积）
    half3 radiance = lightColor * (lightAttenuation * NdotL) * 0.5;

    // BRDF（双向反射分布函数）简化为漫反射颜色
    // 在卡通渲染中，通常使用简化的BRDF模型
    half3 brdf = diffuse;

    return brdf * radiance;
}

/**
 * 前向渲染片段着色器（主要版本）
 * @param input 从顶点着色器传入的插值数据结构
 * @param facing 面朝向标识（VFACE语义，正值表示正面，负值表示背面）
 * @return half4 最终的像素颜色（RGBA格式）
 * 
 * 功能说明:
 * - 处理双面渲染时的法线方向调整
 * - 采样和处理各种材质贴图数据
 * - 执行主光源和附加光源的完整光照计算
 * - 应用高级渲染效果（MatCap反射、溶解效果、雾效等）
 * - 支持多种调试和可视化模式
 */
half4 FragmentShaderForward(Varyings input, half facing : VFACE) : SV_TARGET
{
    // Unity实例化和立体渲染设置
    UNITY_SETUP_INSTANCE_ID(input); // 设置GPU实例化ID
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input); // 设置VR/AR立体渲染眼部索引
    
    
    // 根据面朝向调整法线方向（双面渲染支持）
    // facing > 0 表示正面，facing < 0 表示背面
    input.normalWS_averageShadowAttenuation.xyz *= facing;

    // ====================================================================
    // 第一阶段：采样和预处理贴图数据
    // ====================================================================
    ToonMap toonMap = GetTexture(input); // 获取所有纹理采样结果
  //  return step(0.13,toonMap._MaskMap_Var.a);
    Mask mask = SetMask(toonMap); // 设置遮罩数据结构

    setSpecularData(mask); // 根据遮罩区域设置高光属性

    // ====================================================================
    // 第二阶段：初始化光照计算所需的数据结构
    // ====================================================================
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap, mask);
    // return  _BumpScale;
//return float4(surfaceData.normalTS,1);
   //
   //
   // return toonMap._MaskMap_Var.g;
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);

    // ====================================================================
    // 第三阶段：执行主要光照计算
    // ====================================================================
    half3 color = ShadeAllLights(surfaceData, lightingData, input, toonMap);

    // ====================================================================
    // 第四阶段：应用高级渲染效果
    // ====================================================================
    // 应用MatCap金属反射效果
    ApplyMatCapAdditive(color, input, lightingData, toonMap, surfaceData);
    
    // 调试模式输出（开发时使用）
    #if _NILOTOON_DEBUG_SHADING
    return Get_NILOTOON_DEBUG_SHADING_result(input, surfaceData, lightingData, 1);
    #endif
    
    // ====================================================================
    // 第五阶段：应用角色和材质特定的颜色调整
    // ====================================================================
    // 颜色编辑应用顺序：从局部到全局
    ApplyPerCharacterBaseMapOverride(color, input, lightingData); // 每角色基础贴图覆盖
    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData); // 每角色效果颜色编辑
     ApplyVolumeEffectColorEdit(color); // 体积效果颜色编辑（已禁用）

    // ====================================================================
    // 第六阶段：处理附加光源
    // ====================================================================
    float3 addLight = 0; // 附加光源累积颜色
    
    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount(); // 获取影响当前像素的附加光源数量

    #if USE_FORWARD_PLUS
    // Forward+渲染路径：处理定向光源
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK // Forward+光源检查宏

        Light light = GetAdditionalLight(lightIndex, i.worldPos);
        
        #ifdef _LIGHT_LAYERS
        // 检查光源层级匹配（如果启用了光源层级功能）
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            // 简化的附加光源计算（仅考虑颜色和距离衰减）
            addLight += light.color * light.distanceAttenuation;
        }
    }
    #endif

    // 传统的附加光源循环处理（点光源、聚光灯等）
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, lightingData.positionWS);
        
        #ifdef _LIGHT_LAYERS
        // 检查光源层级匹配
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            // 使用自定义的卡通化物理光照函数计算附加光源贡献
            addLight += LightingPhysicallyBased_Nilo(
                toonMap._BaseMap_Var.rgb,                                    // 漫反射颜色
                light.color,                                                 // 光源颜色
                light.direction,                                             // 光源方向
                light.distanceAttenuation * light.shadowAttenuation,        // 距离衰减 * 阴影衰减
                normalize(input.normalWS_averageShadowAttenuation.xyz)       // 标准化的世界空间法线
            );
        }
    LIGHT_LOOP_END
    #endif

    // 添加附加光源贡献（排除幽灵模型渲染器）
    #ifndef ModelRenderer_YouLing
    color += addLight;
    #endif

    // 受击闪白（独立一层菲涅尔）
    ApplyHitFlash(color, lightingData);

    // 应用溶解效果
    ApplyDissolve(color, input);
    
    // 应用雾效（雾前和雾后有扩展逻辑功能）
    // color = MyFog(color, lightingData.positionWS); // 已注释
    
    // 覆盖输出Alpha值
    ApplyOverrideOutputAlpha(surfaceData.alpha);
   
   // return _AlphaMask/5;
   #if _DEBUG

float    b =  1- mask.metallicmask;
if (_AlphaMask == 0)
{
    color= lerp(float3(0.5,0,0),color,mask.colormask1 * b);
}
else if (_AlphaMask == 1)
{
    color= lerp(float3(0.5,0,0),color,mask.colormask2 * b);
}else if (_AlphaMask == 2)
{
    color= lerp(float3(0.5,0,0),color,mask.colormask3 * b);
}else if (_AlphaMask == 3)
{
    color= lerp(float3(0.5,0,0),color,mask.colormask4 * b);
}else if (_AlphaMask == 4)
{
    color= lerp(float3(0.5,0,0),color,mask.colormask5 * b);
}else if (_AlphaMask == 5)
{
    color= lerp(float3(0.5,0,0),color, mask.metallicmask );
}
   // return   half4(lerp(float3(0.5,0,0),1,mask.colormask4 * b),1);
    /*if (_AlphaMask == 0)
    {
        color= mask.colormask1;
    }
    else if (_AlphaMask == 1)
    {
        color= mask.colormask2;
    }else if (_AlphaMask == 2)
    {
        color= mask.colormask3;
    }else if (_AlphaMask == 3)
    {
        color= mask.colormask4;
    }else if (_AlphaMask == 4)
    {
        color=mask.colormask5;
    }*/
    #endif
    
    // 方向光染色：把角色整体往主光颜色上拉，和空气透视雾保持同一套光照氛围。
    // 用 keyword 剥离，没开时这里一行指令都不生成。
    // _MainLightColor 是 URP 每帧推的主光色（线性空间、已乘 intensity），不用自己传。
    #if defined(_SUNTINT)
    {
        half3 sunCol = _MainLightColor.rgb;
        // 归一化到最大分量为 1，只保留色相，避免光强调高时把角色打爆。
        sunCol /= max(max(sunCol.r, max(sunCol.g, sunCol.b)), 1e-4);
        color *= lerp(half3(1, 1, 1), sunCol, (half)saturate(_SunTintAmount));
    }
    #endif

    return half4(min(color,2), _Alpha); // 返回最终颜色和透明度
}

/**
 * 前向渲染片段着色器（第二版本）
 * @param input 从顶点着色器传入的插值数据
 * @param facing 面朝向标识（VFACE语义）
 * @return half4 最终的像素颜色（RGBA）
 * 
 * 功能说明:
 * - 与主版本类似，但包含头发透明度处理
 * - 基于面部朝向计算头发的透明度变化
 * - 适用于需要特殊透明度处理的材质
 */
half4 FragmentShaderForward2(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 qwe = _FaceForwardDirection; // 面部朝向方向
    input.normalWS_averageShadowAttenuation.xyz *= facing;

    // 获取贴图和遮罩数据
    ToonMap toonMap = GetTexture(input);
    Mask mask = SetMask(toonMap);
    setSpecularData(mask);
    
    // 初始化表面数据和光照数据
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap, mask);
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);

    // 应用所有的光照计算
    half3 color = ShadeAllLights(surfaceData, lightingData, input, toonMap);

    // 调试模式输出
    #if _NILOTOON_DEBUG_SHADING
    return Get_NILOTOON_DEBUG_SHADING_result(input, surfaceData, lightingData, 1);
    #endif

    // 应用颜色编辑效果
    ApplyPerCharacterBaseMapOverride(color, input, lightingData);
    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData);
    ApplyVolumeEffectColorEdit(color);

    // 应用溶解效果
    ApplyDissolve(color, input);
    
    // 屏幕空间UV计算（用于深度比较等）
    float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
    
    // 覆盖输出Alpha值
    ApplyOverrideOutputAlpha(surfaceData.alpha);

    // 计算基于面部朝向的透明度
    float3 e = mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2].xyz);
    float _Asd = saturate(dot(normalize(qwe), normalize(e)));
    _Asd = smoothstep(0.5, 1, _Asd); // 平滑过渡

    // 根据朝向计算头发透明度
    float alpha = lerp(1, _HairAlpha, _Asd);
    
    return half4(color, alpha * _Alpha);
}

/**
 * 前向渲染片段着色器（第三版本）
 * @param input 从顶点着色器传入的插值数据
 * @param facing 面朝向标识（VFACE语义）
 * @return half4 最终的像素颜色（RGBA）
 * 
 * 功能说明:
 * - 包含抖动淡出效果处理
 * - 支持溶解阈值处理
 * - 提供调试返回选项
 * - 使用表面数据的alpha值作为最终透明度
 */
half4 FragmentShaderForward3(Varyings input, half facing : VFACE) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    // 应用溶解阈值处理
    ApplyDissolve_th(input);
    
    #if _NILOTOON_DITHER_FADEOUT
    // 应用抖动淡出裁剪
    NiloDoDitherFadeoutClip(input.positionCS.xy, 1 - _DitherFadeoutAmount);
    #endif

    float3 qwe = _FaceForwardDirection; // 面部朝向方向
    input.normalWS_averageShadowAttenuation.xyz *= facing;

    // 获取贴图数据
    ToonMap toonMap = GetTexture(input);
    
    // 根据mask贴图的alpha通道分出不同区域
    Mask mask = SetMask(toonMap);

    // 根据区域设置高光属性
    setSpecularData(mask);
    
    // 为光照功能准备所有的数据结构
    ToonSurfaceData surfaceData = InitializeSurfaceData(input, toonMap, mask);
    
    // 调试返回（可用于测试）
    return 0.5; // 注意：这里直接返回了0.5，可能是调试代码
    
    ToonLightingData lightingData = InitializeLightingData(input, surfaceData, toonMap);

    // 应用所有的光照计算
    half3 color = ShadeAllLights(surfaceData, lightingData, input, toonMap);

    // 应用MatCap添加效果
    ApplyMatCapAdditive(color, input, lightingData, toonMap, surfaceData);

    // 调试模式输出
    #if _NILOTOON_DEBUG_SHADING
    return Get_NILOTOON_DEBUG_SHADING_result(input, surfaceData, lightingData, 1);
    #endif

    // 应用颜色编辑效果（从局部到全局）
    ApplyPerCharacterBaseMapOverride(color, input, lightingData);
    ApplyPerCharacterEffectColorEditForwardPass(color, lightingData);
    ApplyVolumeEffectColorEdit(color);
    
    // 应用溶解效果
    ApplyDissolve(color, input);
    
    // 覆盖输出Alpha值
    ApplyOverrideOutputAlpha(surfaceData.alpha);

    return half4(color, surfaceData.alpha * _Alpha);
}
