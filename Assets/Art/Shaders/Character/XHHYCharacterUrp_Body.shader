

Shader "Universal Render Pipeline/NiloToon/XHHY_CharacterURP_Body"
{
    Properties
    {
        // ====================================================================
        // 基础渲染控制
        // ====================================================================
        [HideInInspector]_EnableRendering("启用渲染 (默认开启)", Float) = 1

        [HideInInspector]_ShadowColor("_ShadowColor",Color) = (1,1,1,1)
        // 混合模式：Pass 里 Blend 已注释，属性留着
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("源混合模式 (SrcBlend)", Float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("目标混合模式 (DstBlend)", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("深度测试 (ZTest) (默认小于等于)", Float) = 4

        [Main(RenderStateGroup,_,off,off)]_RenderStateGroup("基础设置", Float) = 0
        [Sub(RenderStateGroup)]_CenterPointPosition("模型中心点", Vector) = (1,0,0,0)
        [Sub(RenderStateGroup)]_Alpha("透明度", Range(0,1)) = 1
        [SubEnum(RenderStateGroup, UnityEngine.Rendering.CullMode)]_Cull("剔除", Float) = 2
        [Sub(RenderStateGroup)][HDR]_BackFaceTintColor("背面颜色", Color) = (1,1,1,1)
        [SubToggle(RenderStateGroup, _)]_ZWrite("深度写入", Float) = 1
        [SubEnum(RenderStateGroup, RGBA, 15, RGB, 14)]_ColorMask("颜色通道", Float) = 15
        [Sub(RenderStateGroup)]_ExtraThickOutlineStencilID ("模板 ID", Range (0, 255)) = 199

        // 方向光染色：让角色跟随主方向光的颜色，和空气透视雾统一氛围。
        // 用 keyword 开关，关掉时整段代码被剥离，不产生任何指令。
        [Main(SunTintGroup,_,off,off)]_SunTintGroup("方向光染色", Float) = 0
        [SubToggle(SunTintGroup, _SUNTINT)]_SunTintEnable("受方向光影响", Float) = 0
        [Sub(SunTintGroup)]_SunTintAmount("影响程度", Range(0,1)) = 0.5

        [Main(TextureGroup,_,on,off)]_TextureGroup("主贴图", Float) = 0
        [Sub(TextureGroup)][MainTexture]_BaseMap("主贴图", 2D) = "white" {}
        [Sub(TextureGroup)][HDR][MainColor]_BaseColor("基础颜色", Color) = (1,1,1,1)
        [Sub(TextureGroup)]_MaskMap("遮罩贴图", 2D) = "white" {}
        [GradientRamp(TextureGroup)][NoScaleOffset]_RampLightingTex("渐变贴图", 2D) = "white" {}


        [Main(_NormalMapGroup,_NORMALMAP,on)]_UseNormalMap("法线贴图", Float) = 0



        [Sub(_NormalMapGroup)]_BumpMap("法线贴图", 2D) = "bump" {}
        [Sub(_NormalMapGroup)]_BumpScale("法线强度", Float) = 1.0


        [Main(_DEBUG,_DEBUG)]_DEBUG("调试", Float) = 0
        [SubEnum(_DEBUG, Mask0, 0, Mask025, 1, Mask05, 2, Mask075, 3, Mask1, 4, Metallic, 5)]_AlphaMask("调试通道", Float) = 1
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 非面部着色器特有属性
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Main(_SPECULARHIGHLIGHTS,_,off,off)]_UseSpecular("高光", Float) = 0

      


        // 遮罩 Alpha 从暗到亮：Mask0(0~0.2)、Mask025(0.2~0.4)、Mask05(0.4~0.6)、Mask075(0.6~0.85)、Mask1(0.85~1)
      
        [Title(_SPECULARHIGHLIGHTS, Mask0)]

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularIntensity("高光强度 (默认 1)", Range(0,100)) = 1
        [Sub(_SPECULARHIGHLIGHTS)][HDR]_SpecularColor("高光颜色 (默认白色)", Color) = (1,1,1)
        [Sub(_SPECULARHIGHLIGHTS)]_MultiplyBaseColorToSpecularColor("高光与基础色混合 (默认 1)", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_Multiply("高光形状强度 (默认 0)", Range(0,1)) = 0

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapMidPoint("高光范围（默认 1，越大越集中）", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapRange("高光边缘过渡 (默认 0.05)", Range(0,1)) = 0.05

         [Title(_MatCapAdditiveGroup, MatCapIntensity)]
        [Sub(_SPECULARHIGHLIGHTS)]_MatCapIntensity1("MatCap 强度 1（遮罩 0）", Range(0,100)) = 0
        [Sub(_SPECULARHIGHLIGHTS)]_Roughness1("粗糙度1 (默认 0)", Range(0,8)) = 0
        [Space(10)]
        [Title(_SPECULARHIGHLIGHTS, Mask025)]

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularIntensity2("高光强度 (默认 1)", Range(0,100)) = 1
        [Sub(_SPECULARHIGHLIGHTS)][HDR]_SpecularColor2("高光颜色 (默认白色)", Color) = (1,1,1)
        [Sub(_SPECULARHIGHLIGHTS)]_MultiplyBaseColorToSpecularColor2("高光与基础色混合 (默认 1)", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_Multiply2("高光形状强度 (默认 0)", Range(0,1)) = 0

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapMidPoint2("高光范围（默认 1，越大越集中）", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapRange2("高光边缘过渡 (默认 0.05)", Range(0,1)) = 0.05
 [Title(_MatCapAdditiveGroup, MatCapIntensity)]
        [Sub(_SPECULARHIGHLIGHTS)]_MatCapIntensity2("MatCap 强度 2（遮罩 0.25）", Range(0,100)) = 0
        [Sub(_SPECULARHIGHLIGHTS)]_Roughness2("粗糙度2 (默认 0)", Range(0,8)) = 0
        
        [Space(10)]
        [Title(_SPECULARHIGHLIGHTS, Mask05)]

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularIntensity3("高光强度 (默认 1)", Range(0,100)) = 1
        [Sub(_SPECULARHIGHLIGHTS)][HDR]_SpecularColor3("高光颜色 (默认白色)", Color) = (1,1,1)
        [Sub(_SPECULARHIGHLIGHTS)]_MultiplyBaseColorToSpecularColor3("高光与基础色混合 (默认 1)", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_Multiply3("高光形状强度 (默认 0)", Range(0,1)) = 0

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapMidPoint3("高光范围（默认 1，越大越集中）", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapRange3("高光边缘过渡 (默认 0.05)", Range(0,1)) = 0.05
 [Title(_MatCapAdditiveGroup, MatCapIntensity)]
        [Sub(_SPECULARHIGHLIGHTS)]_MatCapIntensity3("MatCap 强度 3（遮罩 0.5）", Range(0,100)) =0
        [Sub(_SPECULARHIGHLIGHTS)]_Roughness3("粗糙度3 (默认 0)", Range(0,8)) = 0

        [Space(10)]
        [Title(_SPECULARHIGHLIGHTS, Mask075)]

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularIntensity4("高光强度 (默认 1)", Range(0,100)) = 1
        [Sub(_SPECULARHIGHLIGHTS)][HDR]_SpecularColor4("高光颜色 (默认白色)", Color) = (1,1,1)
        [Sub(_SPECULARHIGHLIGHTS)]_MultiplyBaseColorToSpecularColor4("高光与基础色混合 (默认 1)", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_Multiply4("高光形状强度 (默认 0)", Range(0,1)) = 0

        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapMidPoint4("高光范围（默认 1，越大越集中）", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapRange4("高光边缘过渡 (默认 0.05)", Range(0,1)) = 0.05
 [Title(_MatCapAdditiveGroup, MatCapIntensity)]
        [Sub(_SPECULARHIGHLIGHTS)]_MatCapIntensity4("MatCap 强度 4（遮罩 0.75）", Range(0,100)) = 0
        [Sub(_SPECULARHIGHLIGHTS)]_Roughness4("粗糙度4 (默认 0)", Range(0,8)) = 0

        [Space(10)]
        [Title(_SPECULARHIGHLIGHTS, Mask1)]
 [Title(_MatCapAdditiveGroup, MatCapIntensity)]
        [Sub(_SPECULARHIGHLIGHTS)]_SpecularIntensity5("高光强度 (默认 1)", Range(0,100)) = 1
        [Sub(_SPECULARHIGHLIGHTS)][HDR]_SpecularColor5("高光颜色 (默认白色)", Color) = (1,1,1) // URP Lit.shader use _SpecColor, we ignore it and use our own name to avoid confusion
        [Sub(_SPECULARHIGHLIGHTS)]_MultiplyBaseColorToSpecularColor5("高光与基础色混合 (默认 1)", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_Multiply5("高光形状强度 (默认 0)", Range(0,1)) = 0

        //  [Title(_SPECULARHIGHLIGHTS,Style)]
        // [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapUsage5("_SpecularAreaRemapUsage (默认 0)", Range(0,1)) = 0.0
        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapMidPoint5("高光范围（默认 1，越大越集中）", Range(0,1)) = 1
        [Sub(_SPECULARHIGHLIGHTS)]_SpecularAreaRemapRange5("高光边缘过渡 (默认 0.05)", Range(0,1)) = 0.05
 [Title(_MatCapAdditiveGroup, MatCapIntensity)]
        [Sub(_SPECULARHIGHLIGHTS)]_MatCapIntensity5("MatCap 强度 5（遮罩 1）", Range(0,100)) = 0
        [Sub(_SPECULARHIGHLIGHTS)]_Roughness5("粗糙度5 (默认 0)", Range(0,8)) = 0

      

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // MatCap (Additive)
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [Main(_MatCapAdditiveGroup,_,off,off)]_UseMatCapAdditive("MatCap", Float) = 0

       

        [Title(_MatCapAdditiveGroup, MetallicMatCap)]
        [Sub(_MatCapAdditiveGroup)]_MatCapAdditiveMap("MatCap 贴图", 2D) = "white" {}
        [Sub(_MatCapAdditiveGroup)]_MatCapAdditiveIntensity("MatCap 强度", Range(0,100)) = 1
        [Sub(_MatCapAdditiveGroup)][HDR]_MatCapAdditiveColor("MatCap 颜色", Color) = (1,1,1,1)
        [Sub(_MatCapAdditiveGroup)]_MetallicRoughness("金属粗糙度", Range(0,8)) = 0
        [Sub(_MatCapAdditiveGroup)]_MetallicColorIntensity("金属颜色强度", Range(0,1)) = 0.1

        [Title(_MatCapAdditiveGroup, MetallicSpecular)]
        [Sub(_MatCapAdditiveGroup)]_MetallicSpecularIntensity("金属高光强度", Range(0,100)) = 1
        [Sub(_MatCapAdditiveGroup)][HDR]_MetallicSpecularColor("金属高光颜色", Color) = (1,1,1)
        [Sub(_MatCapAdditiveGroup)]_MetallicMultiplyBaseColorToSpecularColor("高光乘以基础色", Range(0,1)) = 1
        [Sub(_MatCapAdditiveGroup)]_MetallicMultiply("高光形状强度", Range(0,1)) = 0
        [Sub(_MatCapAdditiveGroup)]_MetallicSpecularAreaRemapMidPoint("高光范围", Range(0,1)) = 1
        [Sub(_MatCapAdditiveGroup)]_MetallicSpecularAreaRemapRange("高光过渡", Range(0,1)) = 0.05


     
        
       


       
      
        
       
       
        [Sub(_MatCapAdditiveGroup)]_MatCapAdditiveMixWithBaseMapColor("MatCap 混合基础色", Range(0,1)) = 0


        
        // [Main(_EMISSION,_EMISSION)]_UseEmission("自发光                                                            (默认 Off)", Float) = 0
        

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Fresnel（仅 MaskMap.a >= 0.85，即 Alpha 1 区域）
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [Main(_FresnelGroup,_,off,off)]_FresnelGroup("菲涅尔", Float) = 0
        [Sub(_FresnelGroup)][HDR]_FresnelColor("菲涅尔颜色", Color) = (1,0.1,0.1,1)
        [Sub(_FresnelGroup)]_FresnelIntensity("菲涅尔强度", Range(0,5)) = 0.3

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Depth Texture Rim Light And Shadow（光照里被写死关闭，属性留着给 SRP Batcher）
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [HideInInspector]_PerMaterialEnableDepthTextureRimLightAndShadow("Depth Texture Rim Light and Shadow(深度纹理边缘光和阴影)", Float) = 1
        [HideInInspector]_DepthTexRimLightAndShadowWidthMultiplier("_DepthTexRimLightAndShadowWidthMultiplier (默认 1)", Range(0,4)) = 1
        [HideInInspector]_DepthTexRimLightUsage("_DepthTexRimLightUsage (默认 1)", Range(0,1)) = 1
        [HideInInspector]_DepthTexRimLightIntensity("_DepthTexRimLightIntensity (默认 1)", Range(0,32)) = 1
        [HideInInspector][HDR]_DepthTexRimLightTintColor("_DepthTexRimLightTintColor (默认白色)", Color) = (1,1,1)
        [HideInInspector]_DepthTexRimLightMixWithBaseMapColor("_DepthTexRimLightMixWithBaseMapColor (默认 0)", Range(0,1)) = 0
        [HideInInspector]_DepthTexRimLightThresholdOffset("_DepthTexRimLightThresholdOffset (默认 0)", Range(-1,1)) = 0
        [HideInInspector]_DepthTexRimLightFadeoutRange("_DepthTexRimLightFadeoutRange (默认 1)", Range(0.01,10)) = 1



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Occlusion Map
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Group name added extra spaces to line up (默认 XXX) in material GUI
     
     

       
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 自发光
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 组名添加了额外的空格以在材质GUI中对齐 (默认 XXX)
        [Main(_EMISSION,_EMISSION)]_UseEmission("自发光", Float) = 0

        // 提示信息
        // TODO: 美化这个信息框
        [Sub(_EMISSION)]_EmissionMap("自发光贴图", 2D) = "white" {} // 与URP的Lit.shader相同
        [Sub(_EMISSION)]_EmissionMapTilingXyOffsetZw("自发光平铺偏移", Vector) = (1,1,0,0)

        [Title(_EMISSION, Channel)]
        [SubToggle(_EMISSION, _)]_EmissionMapUseSingleChannelOnly("仅使用单通道", Float) = 0
        [Channel(_EMISSION)]_EmissionMapSingleChannelMask("单通道遮罩", Vector) = (0,1,0,0)

        [Title(_EMISSION, IntensityAndColor)]
        [Sub(_EMISSION)]_EmissionIntensity("自发光强度", Range(0,100)) = 1
        [Sub(_EMISSION)][HDR]_EmissionColor("自发光颜色", Color) = (0,0,0) // 与URP的Lit.shader相同
        [Sub(_EMISSION)]_MultiplyBaseColorToEmissionColor("乘以基础色", Range(0,1)) = 0

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 传统描边
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 组名添加了额外的空格以在材质GUI中对齐 (默认 XXX)
       
        [Main(_RENDER_OUTLINE,_,on,off)]_RenderOutline("描边", Float) = 1

        [HideInInspector]_UnityCameraDepthTextureWriteOutlineExtrudedPosition("写入描边扩展位置到深度纹理 (如果出现2D边缘光白线瑕疵请关闭) (默认 On)", Float) = 1
        [HideInInspector]_OutlineCorrection("描边修正  (默认 On)", Float) = 1

        [Title(_RENDER_OUTLINE, Color)]
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor1("描边颜色 1（遮罩 1）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor2("描边颜色 2（遮罩 0.75）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor3("描边颜色 3（遮罩 0.5）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor4("描边颜色 4（遮罩 0.25）", Color) = (0,0,0,0)
        [Sub(_RENDER_OUTLINE)][HDR]_OutlineTintColor5("描边颜色 5（遮罩 0）", Color) = (0,0,0,0)


        [Title(_RENDER_OUTLINE, Width)]
        [Sub(_RENDER_OUTLINE)]_OutlineWidth("描边宽度", Range(0,32)) = 1
        [Sub(_RENDER_OUTLINE)]_OutlineWidthExtraMultiplier("描边宽度倍率", Range(0,256)) = 1

        [Title(_RENDER_OUTLINE, VertexWidth)]
        [SubToggle(_RENDER_OUTLINE,_)]_UseOutlineWidthMaskFromVertexColor("顶点色控制描边宽度", Float) = 0
        [Channel(_RENDER_OUTLINE)]_OutlineWidthMaskFromVertexColor("描边宽度通道", Vector) = (0,1,0,0)


        [HideInInspector]_OutlineZOffset("描边Z偏移 (视图空间单位) (默认 0.0001)", Range(0,1)) = 0.0001
        [HideInInspector]_OutlineZOffsetForFaceArea("面部描边Z偏移 (视图空间单位) (默认 0.02)", Range(0,1)) = 0.02

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 由脚本设置
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CBUFFER_START(UnityPerMaterial)内的所有内容都必须存在于此Properties{}部分中，以使SRP batcher正常工作
        // 即使是仅由脚本设置和[HideInInspector]的属性！
        // 您可以尝试在此处注释掉任何行并重新导入着色器，
        // 这样做会使SRP批处理失败，您可以通过在检查器中查看.shader来检查它

        // 平均阴影贴图
        [HideInInspector]_AverageShadowMapRTSampleIndex("平均阴影贴图RT采样索引", Float) = -1

        // 角色游戏效果
        [HideInInspector]_PerCharEffectTintColor("角色效果颜色", Color) = (1,1,1)
        [HideInInspector]_PerCharEffectAddColor("角色效果附加颜色", Color) = (0,0,0)
        [HideInInspector]_PerCharEffectDesaturatePercentage("角色效果去色百分比", Range(0,1)) = 0
        [HideInInspector]_PerCharEffectLerpColor("角色效果插值颜色", Color) = (1,1,0,0)
        [HideInInspector]_PerCharEffectRimColor("角色效果边缘光颜色", Color) = (0,0,0)
        [HideInInspector]_PerCharEffectRimSharpnessPower("角色效果边缘光锐利度", Float) = 4
        [HideInInspector]_PerCharacterOutlineColorLerp("角色描边颜色插值", Color) = (1,1,1,0)

        // 角色设置
        [HideInInspector]_PerCharacterBaseColorTint("角色基础颜色", Color) = (1,1,1)
        [HideInInspector]_PerCharacterOutlineWidthMultiply("角色描边宽度倍增", Float) = 1
        [HideInInspector]_PerCharacterOutlineColorTint("角色描边颜色", Color) = (1,1,1)

        // 超粗描边
        [HideInInspector]_ExtraThickOutlineWidth("超粗描边宽度", Range(0,100)) = 4
        [HideInInspector]_ExtraThickOutlineViewSpacePosOffset("超粗描边视图空间位置偏移", Vector) = (0,0,0)
        [HideInInspector]_ExtraThickOutlineColor("超粗描边颜色", Color) = (1,1,1,1)
        [HideInInspector]_ExtraThickOutlineZOffset("超粗描边Z偏移", Float) = -0.1
        [HideInInspector]_ExtraThickOutlineMaxFinalWidth("超粗描边最大最终宽度", Float) = 100

        // 面部
        [HideInInspector]_FaceForwardDirection("面部前向方向", Vector) = (0,0,1)
        [HideInInspector]_FaceUpDirection("面部上向方向", Vector) = (0,1,0)
        [HideInInspector]_FixFaceNormalAmount("修正面部法线强度", Range(0,1)) = 1
        [HideInInspector]_FixFaceNormalUseFlattenOrProxySphereMethod("修正面部法线使用平坦或代理球体方法", Range(0,1)) = 0

        // 角色中心
        [HideInInspector]_CharacterBoundCenterPosWS("角色包围盒中心位置(世界空间)", Vector) = (0,0,0)

        // EnableDepthTextureRimLightAndShadow
        [HideInInspector]_NiloToonEnableDepthTextureRimLightAndShadow("NiloToon启用深度纹理边缘光和阴影", Float) = 0

        // 受击闪白（独立一层菲涅尔，跟角色边缘光分开）
        [Main(HitFlashGroup,_,off,off)]_HitFlashGroup("受击闪白", Float) = 0
        [Sub(HitFlashGroup)][HDR]_HitFlashColor("闪白颜色", Color) = (0.2216981,0.9486833,1,1)
        [Sub(HitFlashGroup)]_HitFlashIntensity("闪白强度", Range(0,10)) = 0
        [Sub(HitFlashGroup)]_HitFlashPower("闪白菲涅尔指数", Range(0.1,16)) = 2

        // dither fadeout / dissolve
        [Main(DissolveGroup,_,off,off)]_DissolveGroup("溶解", Float) = 0
        [SubToggle(DissolveGroup, _NILOTOON_DISSOLVE)]_EnableDissolve("启用溶解", Float) = 0
        [Sub(DissolveGroup)]_DissolveAmount("溶解程度", Range(0,1)) = 0
        [Sub(DissolveGroup)]_DissolveMinY("溶解高度下限 (物体Y)", Float) = 0
        [Sub(DissolveGroup)]_DissolveMaxY("溶解高度上限 (物体Y)", Float) = 1.515
        [Sub(DissolveGroup)]_DissolveRootY("溶解基准世界Y", Float) = 0
        [HideInInspector]_DissolveThresholdMap("溶解阈值贴图", 2D) = "linearGrey" {}
        [Sub(DissolveGroup)]_DissolveBorderRange("溶解边缘范围", Range(0.01,1)) = 0.04
        [Sub(DissolveGroup)][HDR]_DissolveBorderTintColor("溶解边缘颜色", Color) = (0,4,4,1)
        [Sub(DissolveGroup)]_DissolveTopBorderRange("溶解顶部范围", Range(0.01,1)) = 0.02
        [Sub(DissolveGroup)][HDR]_DissolveTopBorderTintColor("溶解顶部颜色", Color) = (1,1,1,0)
        [Sub(DissolveGroup)]_DissolveEdgeDitherStrength("溶解边缘网点强度", Range(0,1)) = 0
        [SubToggle(DissolveGroup, _NILOTOON_DITHER_FADEOUT)]_EnableDitherFadeout("启用全局网点", Float) = 0
        [Sub(DissolveGroup)]_DitherFadeoutAmount("全局网点淡出", Range(0,1)) = 0

        // BaseMap Override
        [HideInInspector]_PerCharacterBaseMapOverrideAmount("角色基础贴图覆盖量", Range(0,1)) = 0
        [HideInInspector]_PerCharacterBaseMapOverrideMap("角色基础贴图覆盖贴图", 2D) = "White" {}
        [HideInInspector]_PerCharacterBaseMapOverrideTiling("角色基础贴图覆盖Tiling", Float) = 1
        [HideInInspector]_PerCharacterBaseMapOverrideTintColor("角色基础贴图覆盖颜色", Color) = (1,1,1)

        // perspective removal
        [Main(PerspectiveGroup,_,off,off)]_PerspectiveGroup("透视消除", Float) = 0
        [Sub(PerspectiveGroup)]_PerspectiveRemovalAmount("强度", Range(0,1)) = 0
        [HideInInspector]_PerspectiveRemovalRadius("透视移除半径", Float) = 1
        [HideInInspector]_HeadBonePositionWS("头部骨骼位置(世界空间)", Vector) = (0,0,0)
        [HideInInspector]_PerspectiveRemovalStartHeight("透视移除起始高度", Float) = 0 // 地面
        [HideInInspector]_PerspectiveRemovalEndHeight("透视移除结束高度", Float) = 1 // 地面和角色头部之间的点


        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 8
        [HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0

        [HideInInspector]_LightDirFromCameraDir("光照方向来自相机方向", Int) = 0
        [HideInInspector]_LightDirFromCameraDirOffSet("光照方向来自相机方向偏移", Vector) = (0,0,0,0)


    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "ShaderModel"="4.5"
            // 显式SubShader标签以避免混淆
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
            "Queue"="Geometry-2"
        }
        LOD 300
        HLSLINCLUDE
        //#pragma exclude_renderers gles gles3 glcore
        #pragma target 4.5
        //  #pragma enable_d3d11_debug_symbols

        //(can use _fragment suffix)
        #pragma multi_compile _ _NILOTOON_FORCE_MINIMUM_SHADER
        #pragma multi_compile_local _ _NILOTOON_DISSOLVE
        ENDHLSL













        Pass//"test"
        {
            Name "test"
            Tags
            {
                // "LightMode" = "UniversalPipeline"
            }

            // render state
            Cull [_Cull]
            ZWrite [_ZWrite]
            // Blend one Zero
            //   Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]
           // ZTest [_ZTest]




            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            //  #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            //#pragma exclude_renderers ... // no need to exclude renderers, all platforms will run the same Pass
            //  #pragma shader_feature_local _DECALCOVERAGEEMISSION
            #pragma shader_feature_local_fragment _EMISSION


            #pragma multi_compile_local_fragment _ _NILOTOON_DITHER_FADEOUT
            #pragma multi_compile_local _ _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING // multi_compile_fragment is not enough, because we need to transfer uv8 in vertex shader


          //  #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #pragma shader_feature_local _NORMALMAP
             #pragma shader_feature_local_fragment _DEBUG
            #pragma shader_feature_local _SUNTINT

            //#define ModelRenderer 1
            //  #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            //            #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正

            #pragma vertex VertexShaderForward
            #pragma fragment FragmentShaderForward

            #include "XHHYCharacter\XHHYCharacter_Input.hlsl"
            #include "XHHYCharacter\XHHYCharacter_ForwardPass.hlsl"
            ENDHLSL
        }





        Pass //"描边"
        {
            Name "Outline"
            Tags
            {

                "LightMode" = "NiloToonOutline"

            }

            // 渲染状态
            // TODO: 我们应该找到一种方法来从第一个通道翻转Cull，而不是硬编码“Cull Front”
            Cull Front // 对于额外的通道挤出网格描边方法，必须使用Cull Front。
            ColorMask [_ColorMask]
           // ZTest Less
            /*Stencil
            {
                Ref 199
                // Comp [_StencilComp] // 通过深度测试 = 通过模板测试
                Pass [_StencilOp] // replace意味着写入
            }*/
          //  Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            // #pragma enable_d3d11_debug_symbols
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile _ _NILOTOON_DEBUG_SHADING // multi_compile_fragment is not enough, because of uv8
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT //(can use _fragment suffix)
            #pragma multi_compile_local _ _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
            //     #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            // ---------------------------------------------------------------------------------------------
            //   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //(can use _fragment suffix)
            #define _NILOTOON_ADDITIONAL_LIGHTS 1

            #pragma vertex VertexShaderOutLine
            #pragma fragment FragmentShaderOutLineNoLight//FragmentShaderOutLine

            // all shader logic written inside this .hlsl, remember to write all #define BEFORE writing #include
            #include "XHHYCharacter\XHHYCharacter_Input.hlsl"
            #include "XHHYCharacter\XHHYCharacter_OutlineForwardPass.hlsl"
            ENDHLSL
        }





        // ShadowCaster pass. Used for rendering URP's shadow maps (see URP Lit.shader's "ShadowCaster" pass for reference)
        Pass //"ShadowCaster"
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // more explicit render state to avoid confusion
            ZWrite On // the only goal of this pass is to write depth (shadow map's light space depth)!
            ZTest LEqual // early exit at Early-Z stage if possible (only possible if clip() does not exist)            
            ColorMask 0 // we don't care about color, we just want to write depth, ColorMask 0 will save some write bandwidth
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            //  #pragma enable_d3d11_debug_symbols
            #pragma multi_compile_local _ _NILOTOON_DITHER_FADEOUT //(can use _fragment suffix)
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma vertex VertexShaderShadowCaster
            #pragma fragment BaseColorAlphaClipTest // we only need to do Clip(), we do not need rgba color result for shading



            #include "XHHYCharacter\XHHYCharacter_Input.hlsl"
            #include "XHHYCharacter\XHHYCharacter_ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass //"DepthOnly"
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // more explicit render state to avoid confusion
            ZWrite On // the only goal of this pass is to write depth!
            ZTest LEqual // early exit at Early-Z stage if possible (only possible if clip() does not exist)
            ColorMask 0 // we don't care about color, we just want to write depth, ColorMask 0 will save some write bandwidth
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            //   #pragma enable_d3d11_debug_symbols
            // for push back zoffset depth write of face vertices, to hide face depth texture self shadow artifact
            //   #pragma multi_compile _ _PERSPECTIVEREMOVAL //透视矫正
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest // we only need to do Clip(), no need color shading

            #include "XHHYCharacter\XHHYCharacter_Input.hlsl"
            #include "XHHYCharacter\XHHYCharacter_DepthOnlyOrDepthNormalPass.hlsl"
            ENDHLSL
        }

        Pass //"DepthNormals"
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // SSAO（Renderer Feature）要读深度法线图，角色所有部件都必须参与。
            // 之前 Body 少了这个 pass，深度法线图里只有眼睛和脸有数据，
            // 身体 / 头发 / 背心会被 SSAO 当成"没有表面"，AO 结果不对。
            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma vertex VertexShaderDepthOnlyOrDepthNormal
            #pragma fragment BaseColorAlphaClipTest_AndDepthNormalColorOutput

            #include "XHHYCharacter\XHHYCharacter_Input.hlsl"
            #include "XHHYCharacter\XHHYCharacter_DepthOnlyOrDepthNormalPass.hlsl"
            ENDHLSL
        }



    }




    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI" // TODO: check if this line is still required, because removing this line, LWGUI will still work
}