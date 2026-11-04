/*
 * ========================================================================
 * XHHY角色光照方程系统 (新版本)
 * ========================================================================
 * 
 * 文件名: XHHYCharacter_LightingEquationNew.hlsl
 * 作用: XHHY角色渲染系统的高级光照计算和阴影处理
 * 作者: XHHY团队
 * 日期: 2024
 * 版本: 2.0 (新版光照方程)
 * 
 * 功能特性:
 * ========================================================================
 * 【阴影颜色计算】
 * - 基于HSV色彩空间的阴影颜色调整
 * - 支持色相偏移、饱和度增强、明度调节
 * - 低饱和度区域的回退颜色处理
 * - 光照到阴影过渡区域的特殊处理
 * 
 * 【皮肤材质特化】
 * - 专门的皮肤阴影色调处理
 * - 可选的简化皮肤着色模式
 * - 面部区域的特殊光照处理
 * 
 * 【高级光照特性】
 * - 自阴影区域计算
 * - 过渡区域的平滑处理
 * - 支持昼夜光照变化（预留接口）
 * - GPU纹理压缩伪影的抑制处理
 * 
 * 技术说明:
 * ========================================================================
 * - 使用HSV色彩空间进行更直观的颜色调整
 * - 针对GPU纹理压缩优化，减少色彩伪影
 * - 支持实时光照参数调整
 * - 兼容卡通渲染和写实渲染风格
 * 
 * 使用注意事项:
 * ========================================================================
 * - HSV调整参数需要谨慎设置，避免过度饱和
 * - 皮肤材质建议使用专门的皮肤着色参数
 * - 过渡区域强度参数影响光影边缘的柔和程度
 * ========================================================================
 */


#ifndef CHARACTERlIGHTINGEQUATIONNEW
#define CHARACTERlIGHTINGEQUATIONNEW


/**
 * 计算主光源的阴影区域颜色
 * 
 * 功能说明:
 * - 此函数专门处理阴影区域的颜色计算，不包含主光源颜色
 * - 使用HSV色彩空间进行颜色调整，然后与原始反照率混合
 * - 支持光照到阴影过渡区域的特殊处理
 * - 针对皮肤材质提供专门的着色选项
 * 
 * 技术细节:
 * - 计算光照到阴影过渡区域的强度
 * - 应用HSV调整（色相偏移、饱和度增强、明度调节）
 * - 处理低饱和度区域的颜色伪影问题
 * - 应用色调和过渡区域颜色调整
 */
/*half3 CalculateLightIndependentSelfShadowAlbedoColor(ToonSurfaceData surfaceData, ToonLightingData lightingData, half finalShadowArea)
{
    half3 rawAlbedo = surfaceData.albedo;
    half isFace = lightingData.isFaceArea;
    half isSkin = lightingData.isSkinArea;
    float2 uv = lightingData.uv;

     /*_SelfShadowAreaHueOffset = lerp(_SelfShadowAreaHueOffset,_SelfShadowAreaHueOffset_Night,_DayTime);
     _SelfShadowAreaSaturationBoost = lerp(_SelfShadowAreaSaturationBoost,_SelfShadowAreaSaturationBoost_Night,_DayTime);
     _SelfShadowAreaValueMul = lerp(_SelfShadowAreaValueMul,_SelfShadowAreaValueMul_Night,_DayTime);
     _SelfShadowTintColor = lerp(_SelfShadowTintColor,_SelfShadowTintColor_Night,_DayTime);
    
    _SkinShadowTintColor = lerp(_SkinShadowTintColor,_SkinShadowTintColor_Night,_DayTime);#1#
    
    //计算isLitToShadowTransitionArea。
    // 如果在LitToShadowTransitionArea内，稍后我们将编辑hsv并为这个区域染上额外的颜色。
    half isLitToShadowTransitionArea = saturate((1 - abs(finalShadowArea - 0.5) * 2) * _LitToShadowTransitionAreaIntensity);

    // [hsv]
    half HueOffset = _SelfShadowAreaHueOffset + _LitToShadowTransitionAreaHueOffset * isLitToShadowTransitionArea; // 1 MAD
    half SaturationBoost = _SelfShadowAreaSaturationBoost + _LitToShadowTransitionAreaSaturationBoost * isLitToShadowTransitionArea; // 1 MAD
    half ValueMul = _SelfShadowAreaValueMul * lerp(1, _LitToShadowTransitionAreaValueMul, isLitToShadowTransitionArea);

    half3 originalColorHSV; //用于接收来自ApplyHSVChange(...)的额外输出。
    half3 result = ApplyHSVChange(rawAlbedo, HueOffset, SaturationBoost, ValueMul, originalColorHSV);
    // [如果反照率饱和度较低，则替换为后备颜色，而不是色相转移，以抑制由于GPU纹理压缩造成的hsv结果的0/低饱和度颜色的随机色相伪影。］
    half3 fallbackColor = rawAlbedo * _LowSaturationFallbackColor.rgb;
    result = lerp(fallbackColor, result, lerp(1, saturate(originalColorHSV.y * 5), _LowSaturationFallbackColor.a)); //只影响0~20%的饱和度区域，0%的饱和度区域使用100%的回退。

    //return _LowSaturationFallbackColor.rgb;
    
    // [tint]
    result *= _SelfShadowTintColor;

    // [照明到阴影区的过渡色调]
    result *= lerp(1, _LitToShadowTransitionAreaTintColor, isLitToShadowTransitionArea);

    ////////////////////////////////////////////
    // 覆盖如果皮肤
    ////////////////////////////////////////////
    //皮肤可以选择完全覆盖到只是一个简单的单一颜色的色调
    result = lerp(result, rawAlbedo * _SkinShadowTintColor, isSkin);

    ////////////////////////////////////////////
    // 覆盖，如果脸
    ////////////////////////////////////////////
    #if _ISFACE
    // 脸部可以选择完全覆盖为只是一个简单的单一颜色的色调
    result = lerp(result, rawAlbedo * _SkinShadowTintColor, isFace);
    #endif

    return result;
}*/






/**
 * \brief 
 * \param surfaceData 
 * \param lightingData 
 * \param light 
 * \param toon_map 
 * \return 
 */
half3 ShadeMainLight(inout ToonSurfaceData surfaceData, ToonLightingData lightingData, Light light, ToonMap toon_map)
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 1、 Common data
    // 未使用的结果将被视为死代码并被编译器删除，如果你不使用它，不用担心性能。
    half3 N = lightingData.normalWS;
    half3 rawN = lightingData.PerPixelNormalizedPolygonNormalWS;
    half3 L = light.direction;

    //half3 V = lightingData.viewDirectionWS;
   float3 R = normalize(reflect (-L,N));
   half3 V = normalize (_WorldSpaceCameraPos - lightingData.positionWS);

    half3 H = normalize(L + V); //反射方向
//return pow(dot(R,V),10);
    half NoL = dot(N, L); //光照对表面的强度 //不要使用saturate()，因为我们稍后将通过smoothstep()重新映射NoL。
   // return NoL *0.5+0.5;
    half saturateNoL = saturate(NoL);
    half NoV = saturate(dot(N, V)); //normal,view
    half NoH = max(0,(dot(N, H))); //normal,refrect
    half LoH = saturate(dot(L, H));
    half VoV = saturate(dot(V, V));
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//return normalize(N);

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 2、照明（NoL cel Diffuse，occlution，URP shadowMap）。
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    half celShadeMidPointOffset = 0;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 2.1、 计算常规赛璐璐光照 smoothstep(a,b, (Dot(N,L)))
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    half finalCelShadeMidPoint = _CelShadeMidPoint + celShadeMidPointOffset;
    half smoothstepNoL = smoothstep(finalCelShadeMidPoint - _CelShadeSoftness,
                                    finalCelShadeMidPoint + _CelShadeSoftness, NoL);

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 2.2、 定义Cel light 强度控制 (selfLightAttenuation)
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    half selfLightAttenuation = lerp(smoothstepNoL, 1, _MainLightIgnoreCelShade);

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.celShade = selfLightAttenuation;//赛璐璐光照结果
    #endif


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 2.4、 遮挡(LightMap.g ,face通常是1的可调控默认值)，迫使目标区域变成阴影. 对直接/间接遮挡的分离控制
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #if AnyOcclusionEnabled
    half directOcclusion = surfaceData.occlusion;
    // hardcode 100% usage, unlike indirectOcclusion in ShadeGI(), it is not adjustable
    selfLightAttenuation *= directOcclusion;

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.celShadeOcclusion = selfLightAttenuation;//赛璐璐光照,TexAo叠加结果
    #endif

    //return  surfaceData.celShadeOcclusion;
    #endif


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 2.7、 "_CameraDepthTexture深度 "与 "fragment self深度"，线性视图空间深度差异作为2D边缘光和2D阴影的显示区域。
    // 只有在启用了enableDepthTextureRimLightAndShadow的情况下才会渲染。
    // 如果enableDepthTextureRimLightAndShadow被关闭，2D边缘光将回到经典的NoV边缘光）。
    // 如果enableDepthTextureRimLightAndShadow是关闭的，2D阴影就不会有任何回退。
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // light 默认只有forward 使用， outline 不做光照计算
    half rimAttenuation = 0;

    // 所以我们现在依靠这个bool enableDepthTextureRimLightAndShadow来控制开/关。
    // 只有当 "CharacterScript && Material && Global " 都允许深度纹理边缘光时，我们才能安全地渲染深度纹理边缘光。
    bool enableDepthTextureRimLightAndShadow = _PerMaterialEnableDepthTextureRimLightAndShadow;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 2.7.1、 深度纹理边缘光和阴影
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    #ifndef  ModelRenderer
    enableDepthTextureRimLightAndShadow = false;
    if (enableDepthTextureRimLightAndShadow)
    {
        //return half3(1,0,0);

        //[ 2.7.1.1、获取_CameraDepthTexture在偏移的屏幕位置的线性视图空间深度（屏幕空间uv偏移）。]
        // 用于透视摄像机。
        // 当摄像机移开时，减少宽度（屏幕空间uv偏移量）。
        // 但是，如果摄像机非常接近顶点（此时distanceToCamera接近0），则使用1/(1+distanceToCamera)的方法将宽度锁定在max = 1。
        // 对于正交相机。
        // 禁用摄像机距离固定（只返回一个常数0.7以匹配透视的结果），因为距离/深度不会影响多边形NDC在正交摄像机中的xy位置。
        float cameraDistanceFix = unity_OrthoParams.w ? 0.7 : rcp(1 + lightingData.selfLinearEyeDepth);
        // no need to care orthographicCameraAmount, this line is already correct

        // [2.7.1.2、 计算finalDepthTexRimLightAndShadowWidthMultiplier] 。
        // 允许通过全局/每个材料/每个顶点/每个片段来编辑宽度
        // (*全局*每个材料的宽度乘数)
        float finalDepthTexRimLightAndShadowWidthMultiplier = _DepthTexRimLightAndShadowWidthMultiplier *
             lightingData.vertexColor.g;
        // (* vertex color width multiplier)
        float depthTexRimLightAndShadowWidthMultiplierFromVertexColor = lightingData.vertexColor.g;
        finalDepthTexRimLightAndShadowWidthMultiplier *= depthTexRimLightAndShadowWidthMultiplierFromVertexColor;

        //return finalDepthTexRimLightAndShadowWidthMultiplier;

        // [2.7.1.3、 将所有的float1分组，先进行计算，以提高性能]
        float2 UvOffsetMultiplier =  (finalDepthTexRimLightAndShadowWidthMultiplier *
             cameraDistanceFix);

        // [2.7.1.4、根据视图空间的光线方向进行偏移]
        float2 finalUVOffset = normalize(surfaceData.uv8).xy * UvOffsetMultiplier * abs(normalize(surfaceData.uv8).x);

        // [[2.7.1.5、获得_CameraDepthTexture的相机深度数据，以找到与角色有较大深度差异的边缘]
        // 这里我们只加载一次_CameraDepthTexture，并将其作为边缘光和阴影的输入。
        // 如果与加载_CameraDepthTexture两次相比，我们在性能上有很大的优势，但是我们失去了单独编辑宽度的能力（例如，边缘和阴影分开的宽度）。
        // 使用LOAD而不是sample来获得更好的性能，因为我们不需要深度纹理的mipmap，也不需要对深度数据进行双线性过滤，这毫无意义。
        int2 loadTexPos = lightingData.SV_POSITIONxy + finalUVOffset * _CameraDepthTexture_TexelSize.zw;
        loadTexPos = min(loadTexPos, _CameraDepthTexture_TexelSize.zw - 1);
        // clamp loadTexPos to prevent loading outside of _CameraDepthTexture's valid area

        //  loadTexPos = min(max(loadTexPos,0),1);

        float depthTextureRawSampleValue = LoadSceneDepth(loadTexPos);
        // using URP provided LoadSceneDepth(pos), this will make rendering correct in VR also  
        float depthTextureLinearDepthVS = Convert_SV_PositionZ_ToLinearViewSpaceDepth(depthTextureRawSampleValue);

        // [2.7.1.6.计算深度纹理屏幕空间的边缘光]
        // 注意：为什么要在selfLinearEyeDepth中加入lightingData.ZOffsetFinalSum？
        // 这是因为我们要对抗顶点着色器的 "ZOffsetFinalSum改变selfLinearEyeDepth"。(颜色通道已经应用了ZOffset，但深度通道却没有)
        // 如果我们不把lightingData.ZOffsetFinalSum添加到selfLinearEyeDepth中，ZOffset就会使边缘光的结果出错（例如，眉毛有ZOffset）。
        float selfLinearEyeDepthForDepthTexRimLight = lightingData.selfLinearEyeDepth + lightingData.ZOffsetFinalSum;

        float depthTexRimLightDepthDiffThreshold = 0.05;

        //给用户每个材料的控制权（默认为0）。
        depthTexRimLightDepthDiffThreshold += _DepthTexRimLightThresholdOffset;

        float rimLightdepthDiffThreshold = saturate(
            depthTexRimLightDepthDiffThreshold );

        //计数器/取消脸部区域ZOffset渲染在_CameraDepthTexture(DepthOnly pass)
        rimLightdepthDiffThreshold += /*_FaceAreaCameraDepthTextureZWriteOffset **/ lightingData.isFaceArea;

        // 当深度差过小时，让边缘光柔和而平滑地淡出。
        // _DepthTexRimLightFadeoutRange将允许用户控制淡出的方式。
        rimAttenuation = saturate(
            (depthTextureLinearDepthVS - (selfLinearEyeDepthForDepthTexRimLight + rimLightdepthDiffThreshold)) * 10 /
            _DepthTexRimLightFadeoutRange) * lightingData.vertexColor.g;
        half _a = loadTexPos.x > 1 ? 1 : 0;


        // rimAttenuation = a * b * c * d * rimAttenuation;
        rimAttenuation *= _a;
        #if _NILOTOON_DEBUG_SHADING
        surfaceData.rimAttenuation = rimAttenuation; //depthRimLight_Shadow
        #endif
    }
    #endif


    /*float2 screenUV =  lightingData.SV_POSITIONxy/_ScreenParams.xy;
    float rawDepth = SampleSceneDepth(screenUV);
    float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
    float3 positionVS = normalize(surfaceData.uv8);
    float2 scren0ffset = float2(lerp(-1, 1, step(0, positionVS.x)) * 0.003 /#1# _ScreenParams.x#1# /#1# max(1, pow(linearDepth,2))#1#, 0);
    float offsetDepth = SampleSceneDepth(screenUV + scren0ffset);
    float offsetLinearDepth = LinearEyeDepth(offsetDepth, _ZBufferParams);
    float rim = saturate(offsetLinearDepth - linearDepth);
    rim = step(0.5, rim);
    float fresnelPower = 6;
    float fresnelClamp = 0.8;
    float fresnel = 1 - saturate(dot(lightingData.normalWS , V));
    fresnel = pow(fresnel, fresnelPower);
    fresnel = fresnel * fresnelClamp + (1 - fresnelClamp);*/


    //当角色离摄像机很远时，让边光柔和地淡出，以避免边光闪烁的假象（由于像素数不够）。
    // 这类似于线性雾化，雾化量(0~1)被替换为边缘光的淡出量(0~1)
   // rimAttenuation = lerp(0, rimAttenuation, smoothstep(_GlobalDepthTexRimLightCameraDistanceFadeoutEndDistance,
   //                                                     _GlobalDepthTexRimLightCameraDistanceFadeoutStartDistance,
   //                                                     lightingData.selfLinearEyeDepth));
    //   rimAttenuation = rim ;
    #if _NILOTOON_DEBUG_SHADING
    surfaceData.rimAttenuation_FadeOut = rimAttenuation;//depthRimLight_Shadow 深度淡入淡出
    #endif
    //return rimAttenuation; //litter diff


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  2.9、Lit ramp light , 高光     
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////[ 2.9.1、 ramp light]

    half finalShadowArea = selfLightAttenuation;

    //return finalShadowArea;

    //**********************************************************************//
    // [ramp照明覆盖]
    //**********************************************************************//
    // 如果启用斜面照明纹理
    float lambert = (dot(N, normalize(float3(L.x,0.2,L.z))) * 0.5 + 0.5);
   //return lambert;
    float rampUvX = saturate((lambert - 0.25)/0.5);
    rampUvX = lambert;
    float shadow = smoothstep(0, 1, lightingData.averageShadowAttenuation);
    shadow = lightingData.averageShadowAttenuation;

    #if AnyOcclusionEnabled
    shadow = surfaceData.occlusion;
    #endif

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.averageShadowAttenuation = shadow;//平均过渡阴影
    #endif


    float rampLightingTexSamplingUvY;

    //rampLightingTexSamplingUvY =_UseRampLightingSampleUvYTex == 1 ? toon_map._MaskMap_Var.a :_RampLightingTexSampleUvY  ;
    rampLightingTexSamplingUvY = surfaceData.colormask1 * 0 + surfaceData.colormask2 * 0.25 + surfaceData.colormask3
        * 0.5 + surfaceData.colormask4 * 0.75 + surfaceData.colormask5 ;
    shadow = clamp(0,1,shadow);
    rampUvX *= shadow;
//return rampUvX;

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.rampUvX = rampUvX;                              //ramp光照 UV 映射X (是否合并映射)
    surfaceData.rampLightingTexSamplingUvY = rampLightingTexSamplingUvY; //ramp光照 UV 映射Y (是否合并映射)
    #endif
    // surfaceData.albedo.rgb = lerp(surfaceData.albedo.rgb * _ShadowColor,surfaceData.albedo.rgb,rampUvX);
    //我们需要使用钳形采样器对斜面纹理进行采样，否则uv.x == 0可能还会对uv.x == 1的位置进行采样。
    half3 _RampLightingTex_Day = SAMPLE_TEXTURE2D(_RampLightingTex, sampler_Point_clamp,
                                                  half2(rampUvX, rampLightingTexSamplingUvY )).xyz;
    // half3 _RampLightingTex_Night = SAMPLE_TEXTURE2D(_RampLightingTex, sampler_Point_clamp, half2(rampUvX, rampLightingTexSamplingUvY - 0.5));

    // half3 RampCol = lerp(_RampLightingTex_Day,_RampLightingTex_Night,_DayTime);
    half3 RampCol = _RampLightingTex_Day;

  //  return RampCol;
    half3 lightColorIndependentLitColor = surfaceData.albedo.rgb * RampCol;

    lightColorIndependentLitColor += surfaceData.albedo.rgb * _FresnelColor * pow(1 - max(0, dot(N, V)), 2) * _FresnelIntensity * surfaceData.colormask5;

   // return lightColorIndependentLitColor;
    //return surfaceData.albedo.rgb * float3(0.3,0.1,0.1) * pow(1- max(0,dot(N,V)),3) * 0.5;
    #if _NILOTOON_DEBUG_SHADING
    surfaceData.RampCol = RampCol;                             //采样的ramp光照颜色
    surfaceData.RampColAlbedo = lightColorIndependentLitColor; //融合贴图的rampCol
    #endif


    #if _RAMPTEXCOLORDEBUG
    lightColorIndependentLitColor = _RampTexLevel==rampLightingTexSamplingUvY ? surfaceData.albedo *_RampTexColor : lightColorIndependentLitColor;
    #endif

    float rampValue;
    rampValue = step(0.55, lambert) * shadow;
    rampValue = smoothstep(0.54, 0.56, lambert) * shadow;

    #if AnyOcclusionEnabled
    rampValue = min(surfaceData.occlusion, rampValue);
    #endif

    //*  abode  *********************************//

    //**********************************************************************//
    //2.9.2、 specularLitAdd
    //**********************************************************************//

    //用于所有加光逻辑，在以下代码中使用（specular,rim light, hair strand specular....）。
    //减少（25%），但不完全消除阴影中的加光结果
    half inShadow25PercentMul = lerp(lightingData.averageShadowAttenuation, 1, 0.25);

    //[ Specular]

    half3 specularLitAdd = surfaceData.specular;
    //return  surfaceData.specular;
    //允许用户选择简单的镜面反射或GGX镜面反射方法
    half SpecularRawArea;
    SpecularRawArea = min(1, dot(R,V));
   // _SpecularAreaRemapMidPoint -= toon_map._MaskMap_Var.b * _Multiply;
    half specularRemapStartPoint = max(0, _SpecularAreaRemapMidPoint - _SpecularAreaRemapRange);
    half specularRemapEndPoint = min(1, _SpecularAreaRemapMidPoint + _SpecularAreaRemapRange);
    half remappedSpecularRawArea = smoothstep(specularRemapStartPoint, specularRemapEndPoint, SpecularRawArea);
    //  remappedSpecularRawArea = lerp(SpecularRawArea, remappedSpecularRawArea, _SpecularAreaRemapUsage); // allow user to choose apply specular remap or not
    half3 specularResultRGBMultiplier = remappedSpecularRawArea;


     H = normalize(L + V);
    float3 T_shifted = ShiftTangent(lightingData.tangentWS, N, (surfaceData.albedo.r) );
    float3 H_shifted = normalize(L + V);

    
    /*float3 kk =  D_KajiyaKay(T_shifted,H_shifted,150) * surfaceData.albedo ;
    specularResultRGBMultiplier = lerp(specularResultRGBMultiplier,kk,surfaceData.colormask1);

    float aaa =  pow(max(0,dot(N,V)),2) /*+ pow(1-pow(max(0,dot(N,V)),1),5)#1#;
   // return ;
    aaa = lerp(1,aaa,surfaceData.colormask1);
    lightColorIndependentLitColor *= lerp(float3(0.15,0.15,0.15),1  ,aaa  );

    float rim = pow(1 - dot(normalize(V), N), 6 / 10);   //边缘光
    float fresnel = pow(1.0 - max(0,dot(normalize(V), N)),1.63);    //菲涅尔
    float denier = (5 - 5) / 115;    //丹尼尔参数
    float density = max(rim, (denier * (1 - tex2D(_DenierTex, IN.uv_DenierTex))));  //lerp参数*/

    
   // return lightColorIndependentLitColor;
    //toon_map._MaskMap_Var.r = _MultiplyBaseColorToSpecularColor;
    // float Ks = 0.04;
    //float SpecularPow = exp2(0.5*toon_map._MaskMap_Var.r * 11+2);
    // float SpecularNorm = (SpecularPow+8)/8;
    // float3 SpecularCol = surfaceData.albedo * toon_map._MaskMap_Var.b;
    //float SpecularColid = surfaceData.albedo * (SpecularNorm * pow(NoH,SpecularPow));

//return specularResultRGBMultiplier;
    specularLitAdd *= specularResultRGBMultiplier;
   // return specularLitAdd;
    // if ramp specular enabled, no need to do the this section, because ramp specular don't respect light color anyway
    // max(0.25,x), to prevent 0 specular if Directional Light is off
    // saturate(x), to prevent over bright
  //  specularLitAdd *= saturate(max(0.25, dot(light.color, float3(0.299, 0.587, 0.114))));

   
    // keep 25% specular in 0% direct light(in shadow) enviroment
    specularLitAdd *= (lightingData.averageShadowAttenuation);
   // return NoH;

    #if _NILOTOON_DEBUG_SHADING
   //Light color diff
    surfaceData.specularLitAdd = specularLitAdd; //Specular 高光部分
    #endif

    // return specularLitAdd;


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 2.10、 合成最终的主灯颜色  //在最后阶段将光照颜色应用于整个身体，这更适合卡通/动漫阴影风格的需要。
    // min(LightMaxContribution,light.color)以防止过亮，理想情况下，我们想把它做成地图，但由于性能原因，只用min就够了。
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    half3 result = lightColorIndependentLitColor;
    // return result;

    //将所有的镜面光应用于光照通道（忽略轮廓通道）。
    result += specularLitAdd /** finalShadowArea*/;

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.Resut_RampCol_Albede_Specular_FinalShadow = result;  // 融合光照
    #endif


    //#if NiloToonForwardLitPass
    // (1):
    // min(2,light.color)，以防止灯光颜色过亮。
    // (2):
    // _ZWrite是*到_DepthTexRimLightUsage。
    // 因为如果_ZWrite是关闭的(=0)，这个材料应该是一个透明的材料。
    // 而透明材料不会写入_CameraDepthTexture。
    // 这使得深度纹理的边缘光不正确。
    // 因此，如果_Zwrite是关闭的，就会自动禁用深度纹理的边缘光。
    //TODO 压下边缘光强度 lightColor 异常值
    half3 rimLightColor = min(2, light.color) * _DepthTexRimLightTintColor *
        lerp(1, surfaceData.albedo, _DepthTexRimLightMixWithBaseMapColor) * ((_DepthTexRimLightUsage * _ZWrite) *
            _DepthTexRimLightIntensity); // 组（浮动 * 浮动）。
    rimLightColor = (0.05 + rimLightColor / 6.0); // 边缘光使用灯光的颜色，加上一个小的常数5%，以防止在没有灯光时出现0%的边缘光。
  //  rimLightColor *= _GlobalRimLightMultiplier;

    //return rimLightColor;

    rimAttenuation *= inShadow25PercentMul; // 如果在阴影区，减少边缘光。
    //return rimAttenuation;
    float3 rimAttenuationColor = lerp(rimAttenuation, rimAttenuation * surfaceData.albedo,
                                      _DepthTexRimLightMixWithBaseMapColor);
    result += rimAttenuationColor * rimLightColor;

    #if _NILOTOON_DEBUG_SHADING
    surfaceData.rimLightColor = rimLightColor;  // 边缘光颜色调控
    surfaceData.Resut_add_rimLightColor = result; // 叠加边缘光颜色调控结果
    #endif

    return result;
}

half3 CalculateAdditiveSingleAdditionalLight(Light light, ToonLightingData lightingData)
{
    half NoL = dot(lightingData.normalWS, light.direction); // TODO：允许用户在原始法线和球体代理法线之间进行混合？

    // 重置的N点L 最简单的1行cel shade，你可以用你的方法替换这一行，就像灰度斜面纹理。
    // celShadeResult: 0是阴影，1是光。
    half smoothstepNoL = smoothstep(_AdditionalLightCelShadeMidPoint - _AdditionalLightCelShadeSoftness,
                                    _AdditionalLightCelShadeMidPoint + _AdditionalLightCelShadeSoftness, NoL);

    // 如果你不希望直接照明的NdotL cel shade效果看起来太强，请将_AdditionalLightIgnoreCelShade设置为一个较高的值。
    half lightAttenuation = lerp(smoothstepNoL, 1, _AdditionalLightIgnoreCelShade);

    // 距离和阴影
    //距离衰减钳，以防止在光线离顶点太近时出现过亮现象
    lightAttenuation *= min(_AdditionalLightDistanceAttenuationClamp, light.distanceAttenuation) * light.
        shadowAttenuation;

    //先完成所有的浮点乘法，然后再做float3乘法。(性能)
    return light.color * lightAttenuation;
}

half3 ShadeEmission(ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    //什么都不做，只是返回。
    //创建这个函数是为了防止我们将来需要编辑排放的方程式。
    half3 emissionResult = surfaceData.emission;
    return emissionResult;
}

half3 CompositeAllLightResults(half3 indirectResult, half3 mainLightResult, half3 additionalLightSumResult,
                               half3 emissionResult, ToonSurfaceData surfaceData, ToonLightingData lightingData)
{
    // [在间接光和直接光之间选取最高值 = max(direct light,indirect light)]
    // 动漫人物艺术家通常不希望有间接光和直接光的概念。
    // 因为它很容易破坏最终的颜色。
    // 角色艺术家想要的是。
    // 如果光照足够明亮--保持角色的结果与他们所画的反照率纹理相同。
    // -- 如果在黑暗的环境中 -- 转而使用探照灯，使人物融入环境中
    half3 directLightResult = mainLightResult;

    half3 finalLightResult = max(indirectResult, directLightResult);

    #if _EMISSION
    finalLightResult += emissionResult;
    #endif

    return finalLightResult;
}


#endif
