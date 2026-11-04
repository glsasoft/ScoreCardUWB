// Made with Amplify Shader Editor v1.9.9.12
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "URP/程序火焰"
{
	Properties
	{
		[Tooltip(Allows light to affect the surface, supporting one directional light and any number of additional lights)] _StartFoldoutLighting( "开始折叠-光照", Float ) = 0
		[Space(5)] _LambertLightingNormalfromHeight( "兰伯特光照法线取自高度", Float ) = 1
		[Header(Main Light)][Space(5)][Toggle( _MAINLIGHTENABLED_ON )] _MainLightEnabled( "启用主光", Float ) = 0
		[Space(5)] _MainLight( "主光", Range( 0, 1 ) ) = 1
		[Toggle( _MAINLIGHTFLATENABLED_ON )] _MainLightFlatEnabled( "启用主光平面光照", Float ) = 1
		[Toggle( _MAINLIGHTLAMBERTENABLED_ON )] _MainLightLambertEnabled( "启用主光兰伯特光照", Float ) = 0
		_MainLightFlatLambertBlend( "主光平面-兰伯特混合", Range( 0, 1 ) ) = 0.5
		_MainLightLambertTranslucency( "主光兰伯特透射", Range( 0, 1 ) ) = 1
		_MainLightLambertTranslucencyMask( "主光兰伯特透射遮罩", Range( 0, 10 ) ) = 0
		_MainLightAnisotropy( "主光各向异性", Range( 0, 1 ) ) = 0.8
		_MainLightAnisotropyBlend( "主光各向异性混合", Range( 0, 1 ) ) = 0
		[Space(5)] _MainLightShadow( "主光阴影", Range( 0, 1 ) ) = 1
		_MainLightShadowWorldNoise( "主光阴影世界噪波", Range( 0, 1 ) ) = 0
		_MainLightShadowDirectionalNoise( "主光阴影方向噪波", Range( 0, 1 ) ) = 0
		_MainLightShadowOrthogonalAlignmentWorldNoise( "主光阴影正交对齐世界噪波", Range( 0, 1 ) ) = 0
		_MainLightShadowDirectionalAlignmentNoise( "主光阴影方向对齐噪波", Range( 0, 1 ) ) = 0
		[Toggle( _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON )] _MainLightTransparencyShadowEnabled( "启用主光透明阴影", Float ) = 0
		_MainLightTransparencyShadow( "主光透明阴影", Range( 0, 1 ) ) = 1
		[Header(Additional Lights)][Space(5)][Toggle( _ADDITIONALLIGHTSENABLED_ON )] _AdditionalLightsEnabled( "启用附加光", Float ) = 0
		[Space(5)] _AdditionalLights( "附加光", Range( 0, 1 ) ) = 1
		[Toggle( _ADDITIONALLIGHTSFLATENABLED_ON )] _AdditionalLightsFlatEnabled( "启用附加光平面光照", Float ) = 1
		[Toggle( _ADDITIONALLIGHTSLAMBERTENABLED_ON )] _AdditionalLightsLambertEnabled( "启用附加光兰伯特光照", Float ) = 0
		[Space(5)] _AdditionalLightsFlatLambertBlend( "附加光平面-兰伯特混合", Range( 0, 1 ) ) = 0.5
		_AdditionalLightsLambertTranslucency( "附加光兰伯特透射", Range( 0, 1 ) ) = 1
		_AdditionalLightsLambertTranslucencyMask( "附加光兰伯特透射遮罩", Range( 0, 10 ) ) = 0
		[Header(Additional Lights Shadow)][Space(5)] _AdditionalLightsShadow( "附加光阴影", Range( 0, 1 ) ) = 1
		_AdditionalLightsShadowWorldNoise( "附加光阴影世界噪波", Range( 0, 1 ) ) = 0
		_AdditionalLightsShadowDirectionalNoise( "附加光阴影方向噪波", Range( 0, 1 ) ) = 0
		_AdditionalLightsShadowOrthogonalAlignmentWorldNoise( "附加光阴影正交对齐世界噪波", Range( 0, 1 ) ) = 0
		_AdditionalLightsShadowDirectionalAlignmentNoise( "附加光阴影方向对齐噪波", Range( 0, 1 ) ) = 0
		[Toggle( _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON )] _AdditionalLightsTransparencyShadowEnabled( "启用附加光透明阴影", Float ) = 0
		_AdditionalLightsTransparencyShadow( "附加光透明阴影", Range( 0, 1 ) ) = 1
		_EndFoldoutLighting( "结束折叠-光照", Float ) = 0
		_StartFoldoutBaseUVs( "开始折叠-基础 UV", Float ) = 0
		[Header(Base UVs)][Space(5)][Toggle( _SWAPUVXY_ON )] _SwapUVXY( "交换 UV XY", Float ) = 0
		[Toggle( _WORLDSPACEUVS_ON )] _WorldSpaceUVs( "世界空间 UV", Float ) = 0
		[Toggle( _OBJECTSPACEUVS_ON )] _ObjectSpaceUVs( "物体空间 UV", Float ) = 0
		[Space(5)] _UVSampleNoise( "UV 采样噪波", Range( 0, 1 ) ) = 0
		_EndFoldoutBaseUVs( "结束折叠-基础 UV", Float ) = 0
		_StartFoldoutParticleSettings( "开始折叠-粒子设置", Float ) = 0
		[Header(Particle Settings)][Space(5)] _ParticleRandomization( "粒子随机化", Range( 0, 1 ) ) = 1
		_ParticleSubtractNoiseoverLifetime( "粒子寿命内减去噪波", Range( 0, 1 ) ) = 0
		_EndFoldoutParticleSettings( "结束折叠-粒子设置", Float ) = 0
		[HideInInspector] _StartFoldoutColour( "开始折叠-颜色", Float ) = 0
		[HDR][Header(Colour)][Space(5)] _ColourA( "颜色 A", Color ) = ( 1, 0.1254902, 0, 1 )
		[HDR] _ColourB( "颜色 B", Color ) = ( 1, 0.02745098, 0, 1 )
		_ColourPower( "颜色幂次", Float ) = 1
		_ColourHueShift( "颜色色相偏移", Range( -1, 1 ) ) = 0
		_ColourSaturationShift( "颜色饱和度偏移", Range( -1, 1 ) ) = 0
		_ColourValueMultiplier( "颜色明度倍率", Float ) = 5
		_Alpha( "透明度", Range( 0, 1 ) ) = 1
		[HideInInspector] _EndFoldoutColour( "结束折叠-颜色", Float ) = 0
		[HideInInspector] _StartFoldoutVerticalColour( "开始折叠-垂直颜色", Float ) = 0
		[Header(Vertical Colour)][Space(5)][Toggle( _VERTICALCOLOUR_ON )] _VerticalColour( "垂直颜色", Float ) = 0
		[HDR] _VerticalColourA( "垂直颜色 A", Color ) = ( 0, 0.5019608, 1, 1 )
		[HDR] _VerticalColourB( "垂直颜色 B", Color ) = ( 0, 0, 1, 1 )
		_VerticalColourHueShift( "垂直颜色色相偏移", Range( -1, 1 ) ) = 0
		_VerticalColourSaturationShift( "垂直颜色饱和度偏移", Range( -1, 1 ) ) = 0
		_VerticalColourValueMultiplier( "垂直颜色明度倍率", Float ) = 5
		[Header(Vertical Colour Mask)][Space(5)] _VerticalColourMaskPower( "垂直颜色遮罩幂次", Float ) = 1
		_VerticalColourMaskRemapMin( "垂直颜色遮罩重映射最小值", Range( 0, 1 ) ) = 0.5
		_VerticalColourMaskRemapMax( "垂直颜色遮罩重映射最大值", Range( 0, 1 ) ) = 0.1
		[HideInInspector] _EndFoldoutVerticalColour( "结束折叠-垂直颜色", Float ) = 0
		[HideInInspector] _StartFoldoutNoise( "开始折叠-噪波", Float ) = 0
		[Header(Noise)][Space(5)] _Noise( "噪波", Range( 0, 1 ) ) = 1
		_NoiseScale( "噪波缩放", Float ) = 2
		_NoiseTiling( "噪波平铺", Vector ) = ( 1.5, 1, 1, 0 )
		_NoiseAnimation( "噪波动画", Vector ) = ( 0, 4, 1, 0 )
		_NoiseParticleAnimation( "噪波粒子动画", Vector ) = ( 0, 0, 0, 0 )
		_NoiseOffset( "噪波偏移", Vector ) = ( 0, 0, 0, 0 )
		[IntRange] _NoiseOctaves( "噪波倍频程", Range( 1, 8 ) ) = 1
		_NoiseDilation( "噪波膨胀", Range( 0, 0.1 ) ) = 0.004
		[Toggle( _NOISEDILATIONENABLED_ON )] _NoiseDilationEnabled( "启用噪波膨胀", Float ) = 0
		_NoisePower( "噪波幂次", Float ) = 0.5
		_NoiseRemapMin( "噪波重映射最小值", Range( 0, 1 ) ) = 0
		_NoiseRemapMax( "噪波重映射最大值", Range( 0, 1 ) ) = 1
		[Space(5)] _NoiseParallaxOffset( "噪波视差偏移", Float ) = 0
		[Space(5)] _NoiseXZTwist( "噪波 XZ 扭曲", Range( -360, 360 ) ) = 0
		[Toggle( _NOISEXZTWISTENABLED_ON )] _NoiseXZTwistEnabled( "启用噪波 XZ 扭曲", Float ) = 0
		[Space(5)] _NoiseUVYPreOffset( "噪波 UV-Y 预偏移", Float ) = 0
		_NoiseUVYPreScale( "噪波 UV-Y 预缩放", Float ) = 1
		_NoiseUVYPrePower( "噪波 UV-Y 预幂次", Float ) = 1
		[HideInInspector] _EndFoldoutNoise( "结束折叠-噪波", Float ) = 0
		[HideInInspector] _StartFoldoutNoiseDistortion( "开始折叠-噪波扭曲", Float ) = 0
		[Header(Noise Distortion)][Space(5)] _NoiseDistortion( "噪波扭曲", Range( 0, 1 ) ) = 0.05
		[Toggle( _NOISEDISTORTIONENABLED_ON )] _NoiseDistortionEnabled( "启用噪波扭曲", Float ) = 0
		_NoiseDistortionScale( "噪波扭曲缩放", Float ) = 1
		_NoiseDistortionTiling( "噪波扭曲平铺", Vector ) = ( 1.5, 1, 1, 0 )
		_NoiseDistortionAnimation( "噪波扭曲动画", Vector ) = ( 0, 1, 0, 0 )
		_NoiseDistortionParticleAnimation( "噪波扭曲粒子动画", Vector ) = ( 0, 0, 0, 0 )
		_NoiseDistortionOffset( "噪波扭曲偏移", Vector ) = ( 0, 0, 0, 0 )
		[IntRange] _NoiseDistortionOctaves( "噪波扭曲倍频程", Range( 1, 8 ) ) = 1
		_NoiseDistortionDilation( "噪波扭曲膨胀", Range( 0, 0.1 ) ) = 0.004
		[Toggle( _NOISEDISTORTIONDILATIONENABLED_ON )] _NoiseDistortionDilationEnabled( "启用噪波扭曲膨胀", Float ) = 0
		_NoiseDistortionPower( "噪波扭曲幂次", Float ) = 1
		[HideInInspector] _EndFoldoutNoiseDistortion( "结束折叠-噪波扭曲", Float ) = 0
		[HideInInspector] _StartFoldoutRadialMask( "开始折叠-径向遮罩", Float ) = 0
		[Header(Radial Mask)][Space(5)][Toggle( _RADIALMASK_ON )] _RadialMask( "径向遮罩", Float ) = 1
		[Toggle( _RADIALMASKSUBTRACTIVE_ON )] _RadialMaskSubtractive( "径向遮罩相减", Float ) = 1
		[Space(10)] _RadialMaskRadius( "径向遮罩半径", Range( 0, 1 ) ) = 1
		_RadialMaskRadiusOverParticleLifetime( "径向遮罩半径随粒子寿命变化", Range( 0, 1 ) ) = 0
		_RadialMaskFeather( "径向遮罩羽化", Range( 0, 2 ) ) = 1
		_RadialMaskPower( "径向遮罩幂次", Float ) = 1
		_RadialMaskTiling( "径向遮罩平铺", Vector ) = ( 1.5, 1, 0, 0 )
		_RadialMaskOffset( "径向遮罩偏移", Vector ) = ( 0, 0, 0, 0 )
		[HideInInspector] _EndFoldoutRadialMask( "结束折叠-径向遮罩", Float ) = 0
		[HideInInspector] _StartFoldoutRadialMaskDistortion( "开始折叠-径向遮罩扭曲", Float ) = 0
		[Header(Radial Mask Distortion)][Space(5)] _RadialMaskDistortion( "径向遮罩扭曲", Range( 0, 1 ) ) = 0.05
		[Toggle( _RADIALMASKDISTORTIONENABLED_ON )] _RadialMaskDistortionEnabled( "启用径向遮罩扭曲", Float ) = 0
		_RadialMaskDistortionScale( "径向遮罩扭曲缩放", Float ) = 2
		_RadialMaskDistortionTiling( "径向遮罩扭曲平铺", Vector ) = ( 1.5, 1, 1, 0 )
		_RadialMaskDistortionAnimation( "径向遮罩扭曲动画", Vector ) = ( 0, 2, 0, 0 )
		_RadialMaskDistortionParticleAnimation( "径向遮罩扭曲粒子动画", Vector ) = ( 0, 0, 0, 0 )
		_RadialMaskDistortionOffset( "径向遮罩扭曲偏移", Vector ) = ( 0, 0, 0, 0 )
		[IntRange] _RadialMaskDistortionOctaves( "径向遮罩扭曲倍频程", Range( 1, 8 ) ) = 1
		_RadialMaskDistortionDilation( "径向遮罩扭曲膨胀", Range( 0, 0.1 ) ) = 0.004
		[Toggle( _RADIALMASKDISTORTIONDILATIONENABLED_ON )] _RadialMaskDistortionDilationEnabled( "启用径向遮罩扭曲膨胀", Float ) = 0
		_RadialMaskDistortionPower( "径向遮罩扭曲幂次", Float ) = 1
		[HideInInspector] _EndFoldoutRadialMaskDistortion( "结束折叠-径向遮罩扭曲", Float ) = 0
		[HideInInspector] _StartFoldoutVerticalMasks( "开始折叠-垂直遮罩", Float ) = 0
		[Header(Vertical Mask)][Space(5)][Toggle( _VERTICALMASKSOBJECTSPACE_ON )] _VerticalMasksObjectSpace( "垂直遮罩物体空间", Float ) = 1
		[Header(Vertical Mask 1)][Space(5)][Toggle( _VERTICALMASK1_ON )] _VerticalMask1( "垂直遮罩 1", Float ) = 0
		[Toggle( _VERTICALMASK1SUBTRACTIVE_ON )] _VerticalMask1Subtractive( "垂直遮罩 1 相减", Float ) = 0
		[Space(5)] _VerticalMask1Power( "垂直遮罩 1 幂次", Float ) = 1
		_VerticalMask1RemapMin( "垂直遮罩 1 重映射最小值", Range( 0, 1 ) ) = 0
		_VerticalMask1RemapMax( "垂直遮罩 1 重映射最大值", Range( 0, 1 ) ) = 1
		_VerticalMask1ObjectSpaceScale( "垂直遮罩 1 物体空间缩放", Float ) = 2
		_VerticalMask1ObjectSpaceOffset( "垂直遮罩 1 物体空间偏移", Float ) = -1
		[Header(Vertical Mask 2)][Space(5)][Toggle( _VERTICALMASK2_ON )] _VerticalMask2( "垂直遮罩 2", Float ) = 0
		[Toggle( _VERTICALMASK2SUBTRACTIVE_ON )] _VerticalMask2Subtractive( "垂直遮罩 2 相减", Float ) = 0
		[Space(5)] _VerticalMask2Power( "垂直遮罩 2 幂次", Float ) = 1
		_VerticalMask2RemapMin( "垂直遮罩 2 重映射最小值", Range( 0, 1 ) ) = 0
		_VerticalMask2RemapMax( "垂直遮罩 2 重映射最大值", Range( 0, 1 ) ) = 1
		_VerticalMask2ObjectSpaceScale( "垂直遮罩 2 物体空间缩放", Float ) = 2
		_VerticalMask2ObjectSpaceOffset( "垂直遮罩 2 物体空间偏移", Float ) = -1
		[HideInInspector] _EndFoldoutVerticalMasks( "结束折叠-垂直遮罩", Float ) = 0
		[HideInInspector] _StartFoldoutSpherizeNoise( "开始折叠-球形化噪波", Float ) = 0
		[Header(Spherize Noise)][Space(5)][Toggle( _SPHERIZENOISE_ON )] _SpherizeNoise( "球形化噪波", Float ) = 0
		_SpherizeNoiseRadius( "球形化噪波半径", Float ) = 0.5
		_SpherizeNoiseStrength( "球形化噪波强度", Float ) = 1
		_SpherizeNoiseOffset( "球形化噪波偏移", Vector ) = ( 0, 0, 0, 0 )
		[HideInInspector] _EndFoldoutSpherizeNoise( "结束折叠-球形化噪波", Float ) = 0
		[HideInInspector] _StartFoldoutFresnelMask( "开始折叠-菲涅尔遮罩", Float ) = 0
		[Header(Fresnel Mask)][Space(5)] _FresnelMask( "菲涅尔遮罩", Range( 0, 1 ) ) = 0
		_FresnelMaskPower( "菲涅尔遮罩幂次", Float ) = 2
		_FresnelMaskRemapMin( "菲涅尔遮罩重映射最小值", Range( 0, 1 ) ) = 0
		_FresnelMaskRemapMax( "菲涅尔遮罩重映射最大值", Range( 0, 1 ) ) = 1
		[HideInInspector] _EndFoldoutFresnelMask( "结束折叠-菲涅尔遮罩", Float ) = 0
		[HideInInspector] _StartFoldoutDepthFade( "开始折叠-深度淡出", Float ) = 0
		[Header(Depth Fade)][Space(5)] _DepthFade( "深度淡出", Float ) = 0
		_DepthFadePower( "深度淡出幂次", Float ) = 1
		[Toggle( _INVERTDEPTHFADE_ON )] _InvertDepthFade( "反转深度淡出", Float ) = 0
		[Space(5)] _SubtractiveDepthFade( "相减深度淡出", Float ) = 0
		_SubtractiveDepthFadePower( "相减深度淡出幂次", Float ) = 1
		[Header(Camera Depth Fade)][Space(5)] _CameraDepthFadeLength( "相机深度淡出长度", Float ) = 0
		_CameraDepthFadeOffset( "相机深度淡出偏移", Float ) = 0
		_CameraDepthFadePower( "相机深度淡出幂次", Float ) = 1
		[HideInInspector] _EndFoldoutDepthFade( "结束折叠-深度淡出", Float ) = 0
		[HideInInspector] _StartFoldoutIntersectionHighlight( "开始折叠-交叉高光", Float ) = 0
		[Header(Intersection Highlight)][Space(5)] _IntersectionHighlight( "交叉高光", Float ) = 0
		_IntersectionHighlightPower( "交叉高光幂次", Float ) = 1
		_IntersectionHighlightRemapMin( "交叉高光重映射最小值", Range( 0, 1 ) ) = 0
		_IntersectionHighlightRemapMax( "交叉高光重映射最大值", Range( 0, 1 ) ) = 1
		_IntersectionHighlightShadowMaskPower( "交叉高光阴影遮罩幂次", Range( 0.01, 4 ) ) = 1
		[Header(Intersection Highlight Colour)][Space(5)] _IntersectionHighlightColour( "交叉高光颜色", Color ) = ( 1, 1, 1, 1 )
		_IntersectionHighlightColourHueShift( "交叉高光颜色色相偏移", Range( -1, 1 ) ) = 0
		_IntersectionHighlightColourSaturationShift( "交叉高光颜色饱和度偏移", Range( -1, 1 ) ) = 0
		_IntersectionHighlightColourValueMultiplier( "交叉高光颜色明度倍率", Float ) = 5
		[HideInInspector] _EndFoldoutIntersectionHighlight( "结束折叠-交叉高光", Float ) = 0
		[HideInInspector] _StartFoldoutVertexUVOffset( "开始折叠-顶点 UV 偏移", Float ) = 0
		[Header(Vertex UV Offset)][Space(5)] _VertexUVOffsetTop( "顶点 UV 顶部偏移", Range( -1, 1 ) ) = 0
		_VertexUVOffsetTopPower( "顶点 UV 顶部偏移幂次", Float ) = 1
		[Space(5)] _VertexUVOffsetBottom( "顶点 UV 底部偏移", Range( -1, 1 ) ) = 0
		_VertexUVOffsetBottomPower( "顶点 UV 底部偏移幂次", Float ) = 1
		[HideInInspector] _EndFoldoutVertexUVOffset( "结束折叠-顶点 UV 偏移", Float ) = 0
		[HideInInspector] _StartFoldoutVertexNormalOffset( "开始折叠-顶点法线偏移", Float ) = 0
		[Header(Vertex Normal Offset)][Space(5)] _VertexNormalOffset( "顶点法线偏移", Float ) = 0
		[Space(5)] _VertexNormalOffsetTop( "顶点法线顶部偏移", Float ) = 0
		_VertexNormalOffsetTopPower( "顶点法线顶部偏移幂次", Float ) = 1
		[Space(5)] _VertexNormalOffsetBottom( "顶点法线底部偏移", Float ) = 0
		_VertexNormalOffsetBottomPower( "顶点法线底部偏移幂次", Float ) = 1
		[HideInInspector] _EndFoldoutVertexNormalOffset( "结束折叠-顶点法线偏移", Float ) = 0
		[Header(Vertex Twist)][Space(5)] _VertexTwist( "顶点扭曲", Float ) = 0
		[HideInInspector] _StartFoldoutVertexWave( "开始折叠-顶点波动", Float ) = 0
		[Header(Vertex Wave)][Space(5)] _VertexWave( "顶点波动", Float ) = 0.1
		[Toggle( _VERTEXWAVEENABLED_ON )] _VertexWaveEnabled( "启用顶点波动", Float ) = 0
		_VertexWaveScale( "顶点波动缩放", Float ) = 2
		_VertexWaveAnimation( "顶点波动动画", Float ) = 4
		_VertexWaveOffset( "顶点波动偏移", Range( -1, 1 ) ) = 0
		[HideInInspector] _EndFoldoutVertexWave( "结束折叠-顶点波动", Float ) = 0
		[HideInInspector] _StartFoldoutVertexNoise( "开始折叠-顶点噪波", Float ) = 0
		[Header(Vertex Noise)][Space(5)] _VertexNoise( "顶点噪波", Float ) = 0.02
		[Toggle( _VERTEXNOISEENABLED_ON )] _VertexNoiseEnabled( "启用顶点噪波", Float ) = 0
		[Space(5)] _VertexNoiseScale( "顶点噪波缩放", Float ) = 2
		_VertexNoiseTiling( "顶点噪波平铺", Vector ) = ( 1, 1, 1, 0 )
		_VertexNoiseAnimation( "顶点噪波动画", Vector ) = ( 0, 2, 0, 0 )
		_VertexNoiseParticleAnimation( "顶点噪波粒子动画", Vector ) = ( 0, 0, 0, 0 )
		_VertexNoiseOffset( "顶点噪波偏移", Vector ) = ( 0, 0, 0, 0 )
		[IntRange] _VertexNoiseOctaves( "顶点噪波倍频程", Range( 1, 4 ) ) = 1
		[Space(5)] _VertexNoiseDilation( "顶点噪波膨胀", Range( -0.2, 0.2 ) ) = 0
		[Toggle( _VERTEXNOISEDILATIONENABLED_ON )] _VertexNoiseDilationEnabled( "启用顶点噪波膨胀", Float ) = 0
		[Space(5)] _VertexNoiseTwist( "顶点噪波扭曲", Range( -180, 180 ) ) = 0
		[HideInInspector] _EndFoldoutVertexNoise( "结束折叠-顶点噪波", Float ) = 0
		[HideInInspector] _StartFoldoutVertexWaveNoiseVerticalMask( "开始折叠-顶点波动-噪波垂直遮罩", Float ) = 0
		[Header(Vertex Wave Noise Vertical Mask)][Space(5)] _VertexWaveNoiseVerticalMaskPower( "顶点波动-噪波垂直遮罩幂次", Float ) = 1
		_VertexWaveNoiseVerticalMaskRemapMin( "顶点波动-噪波垂直遮罩重映射最小值", Range( 0, 1 ) ) = 0
		_VertexWaveNoiseVerticalMaskRemapMax( "顶点波动-噪波垂直遮罩重映射最大值", Range( 0, 1 ) ) = 1
		[HideInInspector] _EndFoldoutVertexWaveNoiseVerticalMask( "结束折叠-顶点波动-噪波垂直遮罩", Float ) = 0
		[HideInInspector] _StartFoldoutVertexOffsetoverY( "开始折叠-顶点 Y 向偏移", Float ) = 0
		[Header(Vertex Offset Over Y)][Space(5)] _VertexOffsetOverY1( "顶点 Y 向偏移 1", Vector ) = ( 0, 0, 0, 0 )
		_VertexOffsetOverY1Power( "顶点 Y 向偏移 1 幂次", Float ) = 2
		[Space(5)] _VertexOffsetOverY2( "顶点 Y 向偏移 2", Vector ) = ( 0, 0, 0, 0 )
		_VertexOffsetOverY2Power( "顶点 Y 向偏移 2 幂次", Float ) = 2
		[Header(Vertex Offset Over Circular Y)][Space(5)] _VertexOffsetOverCircularY( "顶点环形 Y 偏移", Vector ) = ( 0, 0, 0, 0 )
		_VertexOffsetOverCircularYPower( "顶点环形 Y 偏移幂次", Float ) = 1
		[HideInInspector] _EndFoldoutVertexOffsetoverY( "结束折叠-顶点 Y 向偏移", Float ) = 0
		[Header(Tessellation)][Space(5)] _Tessellation( "曲面细分", Range( 1, 64 ) ) = 1


		_TessPhongStrength( "Phong 细分强度", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		//[HideInInspector][ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

		//[HideInInspector] _AlphaClip("__clip", Float) = 0.0
	}

	SubShader
	{
		PackageRequirements
		{
			"com.unity.render-pipelines.universal": "[14.0,15.0]"
		}

		

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Unlit" }

	LOD 0

		ZWrite On
		Cull Off
		AlphaToMask Off
		ColorMask RGBA
		Blend One Zero, One Zero
		BlendOp Add, Add

		

		HLSLINCLUDE
		#pragma target 3.0
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#define ASE_ADJUST_CLIP_POSITION( x ) x

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			BlendOp Add, Add

			

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_FIXED_TESSELLATION
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _RECEIVE_SHADOWS_OFF
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif


			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Math.cginc"
			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Noise.cginc"
			#include "../../../Mirza/Solaris/Shaders/_Includes/Solaris.hlsl"

			
			
			

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _SWAPUVXY_ON
			#pragma shader_feature_local _VERTEXWAVEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEDILATIONENABLED_ON
			#pragma shader_feature_local _WORLDSPACEUVS_ON
			#pragma shader_feature_local _VERTICALCOLOUR_ON
			#pragma shader_feature_local _NOISEDILATIONENABLED_ON
			#pragma shader_feature_local _NOISEXZTWISTENABLED_ON
			#pragma shader_feature_local _SPHERIZENOISE_ON
			#pragma shader_feature_local _OBJECTSPACEUVS_ON
			#pragma shader_feature_local _NOISEDISTORTIONENABLED_ON
			#pragma shader_feature_local _NOISEDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _MAINLIGHTENABLED_ON
			#pragma shader_feature_local _MAINLIGHTLAMBERTENABLED_ON
			#pragma shader_feature_local _MAINLIGHTFLATENABLED_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSENABLED_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSLAMBERTENABLED_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSFLATENABLED_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _VERTICALMASK2_ON
			#pragma shader_feature_local _VERTICALMASK1_ON
			#pragma shader_feature_local _RADIALMASKSUBTRACTIVE_ON
			#pragma shader_feature_local _RADIALMASK_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONENABLED_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _VERTICALMASK1SUBTRACTIVE_ON
			#pragma shader_feature_local _VERTICALMASKSOBJECTSPACE_ON
			#pragma shader_feature_local _VERTICALMASK2SUBTRACTIVE_ON
			#pragma shader_feature_local _INVERTDEPTHFADE_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 positionWSAndFogFactor : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				half4 tangentWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ColourA;
			float4 _RadialMaskDistortionAnimation;
			float4 _VerticalColourA;
			float4 _VerticalColourB;
			float4 _RadialMaskDistortionParticleAnimation;
			float4 _RadialMaskDistortionOffset;
			float4 _NoiseDistortionOffset;
			float4 _ColourB;
			float4 _NoiseDistortionAnimation;
			float4 _NoiseDistortionParticleAnimation;
			float4 _IntersectionHighlightColour;
			float4 _VertexNoiseAnimation;
			float4 _NoiseAnimation;
			float4 _VertexNoiseParticleAnimation;
			float4 _NoiseOffset;
			float4 _NoiseParticleAnimation;
			float4 _VertexNoiseOffset;
			float3 _VertexOffsetOverY1;
			float3 _RadialMaskDistortionTiling;
			float3 _VertexOffsetOverCircularY;
			float3 _VertexOffsetOverY2;
			float3 _VertexNoiseTiling;
			float3 _NoiseTiling;
			float3 _NoiseDistortionTiling;
			float2 _RadialMaskTiling;
			float2 _RadialMaskOffset;
			float2 _SpherizeNoiseOffset;
			float _VerticalColourMaskRemapMax;
			float _MainLightShadowOrthogonalAlignmentWorldNoise;
			float _MainLightShadowDirectionalAlignmentNoise;
			float _Noise;
			float _MainLightShadow;
			float _MainLightAnisotropyBlend;
			float _MainLightAnisotropy;
			float _AdditionalLightsFlatLambertBlend;
			float _ParticleSubtractNoiseoverLifetime;
			float _NoisePower;
			float _AdditionalLightsLambertTranslucencyMask;
			float _MainLight;
			float _MainLightShadowDirectionalNoise;
			float _ColourPower;
			float _MainLightLambertTranslucency;
			float _ColourHueShift;
			float _MainLightLambertTranslucencyMask;
			float _ColourSaturationShift;
			float _ColourValueMultiplier;
			float _VerticalColourHueShift;
			float _VerticalColourSaturationShift;
			float _MainLightFlatLambertBlend;
			float _VerticalColourValueMultiplier;
			float _VerticalColourMaskRemapMin;
			float _LambertLightingNormalfromHeight;
			float _VerticalColourMaskPower;
			float _MainLightShadowWorldNoise;
			float _StartFoldoutRadialMask;
			float _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
			float _AdditionalLightsShadowWorldNoise;
			float _VerticalMask1Power;
			float _VerticalMask2RemapMin;
			float _VerticalMask2RemapMax;
			float _VerticalMask2ObjectSpaceOffset;
			float _VerticalMask2ObjectSpaceScale;
			float _VerticalMask2Power;
			float _FresnelMaskRemapMin;
			float _FresnelMaskRemapMax;
			float _FresnelMaskPower;
			float _FresnelMask;
			float _DepthFade;
			float _DepthFadePower;
			float _SubtractiveDepthFade;
			float _SubtractiveDepthFadePower;
			float _CameraDepthFadeLength;
			float _CameraDepthFadeOffset;
			float _CameraDepthFadePower;
			float _Alpha;
			float _MainLightTransparencyShadow;
			float _VerticalMask1ObjectSpaceScale;
			float _VerticalMask1ObjectSpaceOffset;
			float _VerticalMask1RemapMin;
			float _VerticalMask1RemapMax;
			float _AdditionalLightsShadowDirectionalNoise;
			float _AdditionalLightsShadowDirectionalAlignmentNoise;
			float _AdditionalLightsShadow;
			float _AdditionalLights;
			float _IntersectionHighlightColourHueShift;
			float _IntersectionHighlightColourSaturationShift;
			float _IntersectionHighlightColourValueMultiplier;
			float _IntersectionHighlightRemapMin;
			float _IntersectionHighlightRemapMax;
			float _AdditionalLightsLambertTranslucency;
			float _IntersectionHighlight;
			float _RadialMaskRadius;
			float _RadialMaskRadiusOverParticleLifetime;
			float _RadialMaskFeather;
			float _RadialMaskDistortionScale;
			float _RadialMaskDistortionOctaves;
			float _RadialMaskDistortionDilation;
			float _RadialMaskDistortionPower;
			float _RadialMaskDistortion;
			float _RadialMaskPower;
			float _IntersectionHighlightPower;
			float _NoiseDilation;
			float _NoiseDistortionDilation;
			float _NoiseScale;
			float _StartFoldoutFresnelMask;
			float _EndFoldoutFresnelMask;
			float _StartFoldoutDepthFade;
			float _EndFoldoutDepthFade;
			float _StartFoldoutIntersectionHighlight;
			float _EndFoldoutIntersectionHighlight;
			float _StartFoldoutVertexUVOffset;
			float _EndFoldoutVertexUVOffset;
			float _StartFoldoutVertexNormalOffset;
			float _EndFoldoutVertexNormalOffset;
			float _StartFoldoutVertexWave;
			float _EndFoldoutVertexWave;
			float _StartFoldoutVertexNoise;
			float _EndFoldoutVertexNoise;
			float _StartFoldoutVertexWaveNoiseVerticalMask;
			float _EndFoldoutVertexWaveNoiseVerticalMask;
			float _StartFoldoutVertexOffsetoverY;
			float _EndFoldoutSpherizeNoise;
			float _EndFoldoutVertexOffsetoverY;
			float _StartFoldoutSpherizeNoise;
			float _StartFoldoutVerticalMasks;
			float _StartFoldoutLighting;
			float _EndFoldoutLighting;
			float _StartFoldoutBaseUVs;
			float _EndFoldoutBaseUVs;
			float _StartFoldoutParticleSettings;
			float _EndFoldoutParticleSettings;
			float _StartFoldoutColour;
			float _EndFoldoutColour;
			float _StartFoldoutVerticalColour;
			float _EndFoldoutVerticalColour;
			float _StartFoldoutNoise;
			float _EndFoldoutNoise;
			float _StartFoldoutNoiseDistortion;
			float _EndFoldoutNoiseDistortion;
			float _EndFoldoutRadialMask;
			float _StartFoldoutRadialMaskDistortion;
			float _EndFoldoutRadialMaskDistortion;
			float _EndFoldoutVerticalMasks;
			float _Tessellation;
			float _VertexNormalOffset;
			float _VertexNormalOffsetTopPower;
			float _VertexOffsetOverY2Power;
			float _VertexOffsetOverCircularYPower;
			float _NoiseRemapMin;
			float _NoiseRemapMax;
			float _UVSampleNoise;
			float _SpherizeNoiseRadius;
			float _SpherizeNoiseStrength;
			float _NoiseXZTwist;
			float _NoiseUVYPreOffset;
			float _NoiseUVYPreScale;
			float _NoiseUVYPrePower;
			float _NoiseParallaxOffset;
			float _NoiseDistortionScale;
			float _NoiseDistortionOctaves;
			float _AdditionalLightsTransparencyShadow;
			float _NoiseDistortionPower;
			float _NoiseDistortion;
			float _VertexOffsetOverY1Power;
			float _VertexTwist;
			float _VertexUVOffsetBottom;
			float _VertexUVOffsetBottomPower;
			float _VertexNormalOffsetTop;
			float _VertexNormalOffsetBottomPower;
			float _VertexNormalOffsetBottom;
			float _VertexWaveScale;
			float _VertexWaveAnimation;
			float _VertexWaveOffset;
			float _VertexWave;
			float _VertexWaveNoiseVerticalMaskRemapMin;
			float _NoiseOctaves;
			float _VertexWaveNoiseVerticalMaskRemapMax;
			float _VertexNoiseScale;
			float _ParticleRandomization;
			float _VertexNoiseOctaves;
			float _VertexNoiseDilation;
			float _VertexNoiseTwist;
			float _VertexNoise;
			float _VertexUVOffsetTopPower;
			float _VertexUVOffsetTop;
			float _VertexWaveNoiseVerticalMaskPower;
			float _IntersectionHighlightShadowMaskPower;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float localTwistXZ_float11_g587 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float UV_2D_Y691 = (staticSwitch321).y;
				float3 Vertex_Normal_Offset670 = ( ( input.normalOS * _VertexNormalOffset ) + ( pow( UV_2D_Y691 , _VertexNormalOffsetTopPower ) * input.normalOS * _VertexNormalOffsetTop ) + ( pow( ( 1.0 - UV_2D_Y691 ) , _VertexNormalOffsetBottomPower ) * input.normalOS * _VertexNormalOffsetBottom ) );
				float2 UV_2D327 = staticSwitch321;
				float mulTime258 = _TimeParameters.x * _VertexWaveAnimation;
				float temp_output_7_0_g582 = _VertexWaveNoiseVerticalMaskRemapMin;
				float temp_output_23_0_g582 = _VertexWaveNoiseVerticalMaskRemapMax;
				float temp_output_20_0_g582 = UV_2D_Y691;
				float temp_output_4_0_g582 = _VertexWaveNoiseVerticalMaskPower;
				float smoothstepResult22_g582 = smoothstep( temp_output_7_0_g582 , temp_output_23_0_g582 , pow( temp_output_20_0_g582 , temp_output_4_0_g582 ));
				float Vertex_WaveNoise_Vertical_Mask280 = smoothstepResult22_g582;
				#ifdef _VERTEXWAVEENABLED_ON
				float3 staticSwitch448 = ( ( sin( ( ( UV_2D327.y * TWO_PI * _VertexWaveScale ) - ( mulTime258 + ( _VertexWaveOffset * TWO_PI ) ) ) ) * _VertexWave * Vertex_WaveNoise_Vertical_Mask280 ) * input.normalOS );
				#else
				float3 staticSwitch448 = float3( 0,0,0 );
				#endif
				float3 Vertex_Sine265 = staticSwitch448;
				float localTwistXZ_float11_g585 = ( 0.0 );
				float localSimplexNoise_float2_g584 = ( 0.0 );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.positionOS.xyz).yxz;
				#else
				float3 staticSwitch339 = input.positionOS.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch605 = UV_3D_World622;
				#else
				float3 staticSwitch605 = UV_3D340;
				#endif
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Vertex_Noise_Offset311 = ( _VertexNoiseOffset + Particle_Stable_Random_X147 + ( _VertexNoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g579 = ( float4( ( staticSwitch605 * _VertexNoiseScale * _VertexNoiseTiling ) , 0.0 ) - ( Vertex_Noise_Offset311 + ( _VertexNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_292_0 = (temp_output_10_0_g579).xyz;
				float3 position2_g584 = temp_output_292_0;
				float temp_output_292_15 = (temp_output_10_0_g579).w;
				float angle2_g584 = temp_output_292_15;
				float octaves2_g584 = _VertexNoiseOctaves;
				float noise2_g584 = 0.0;
				float3 gradient2_g584 = float3( 0,0,0 );
				SimplexNoise_float( position2_g584 , angle2_g584 , octaves2_g584 , noise2_g584 , gradient2_g584 );
				float localSimplexNoise_Caustics_float2_g583 = ( 0.0 );
				float3 position2_g583 = temp_output_292_0;
				float angle2_g583 = temp_output_292_15;
				float octaves2_g583 = _VertexNoiseOctaves;
				float gradientStrength2_g583 = _VertexNoiseDilation;
				float noise2_g583 = 0.0;
				float3 gradient2_g583 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g583 , angle2_g583 , octaves2_g583 , gradientStrength2_g583 , noise2_g583 , gradient2_g583 );
				#ifdef _VERTEXNOISEDILATIONENABLED_ON
				float3 staticSwitch490 = gradient2_g583;
				#else
				float3 staticSwitch490 = gradient2_g584;
				#endif
				float3 temp_output_10_0_g585 = staticSwitch490;
				float3 position11_g585 = temp_output_10_0_g585;
				float temp_output_9_0_g585 = _VertexNoiseTwist;
				float angle11_g585 = radians( temp_output_9_0_g585 );
				float3 output11_g585 = float3( 0,0,0 );
				TwistXZ_float( position11_g585 , angle11_g585 , output11_g585 );
				float3 temp_output_852_0 = output11_g585;
				#ifdef _VERTEXNOISEENABLED_ON
				float3 staticSwitch450 = ( temp_output_852_0 * _VertexNoise * Vertex_WaveNoise_Vertical_Mask280 );
				#else
				float3 staticSwitch450 = float3( 0,0,0 );
				#endif
				float3 Vertex_Noise298 = staticSwitch450;
				float2 break587 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_576_0 = ( ( break587.x * pow( break587.y , _VertexUVOffsetTopPower ) ) * ( _VertexUVOffsetTop * 0.5 ) );
				float3 appendResult575 = (float3(temp_output_576_0 , 0.0 , 0.0));
				float3 appendResult594 = (float3(0.0 , temp_output_576_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch593 = appendResult594;
				#else
				float3 staticSwitch593 = appendResult575;
				#endif
				float3 Vertex_Offset_Top579 = staticSwitch593;
				float2 break644 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_650_0 = ( ( break644.x * pow( ( 1.0 - break644.y ) , _VertexUVOffsetBottomPower ) ) * ( _VertexUVOffsetBottom * 0.5 ) );
				float3 appendResult653 = (float3(temp_output_650_0 , 0.0 , 0.0));
				float3 appendResult652 = (float3(0.0 , temp_output_650_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch654 = appendResult652;
				#else
				float3 staticSwitch654 = appendResult653;
				#endif
				float3 Vertex_Offset_Bottom655 = staticSwitch654;
				float3 temp_output_10_0_g587 = ( ( Vertex_Normal_Offset670 + Vertex_Sine265 + Vertex_Noise298 + Vertex_Offset_Top579 + Vertex_Offset_Bottom655 ) + input.positionOS.xyz );
				float3 position11_g587 = temp_output_10_0_g587;
				float temp_output_9_0_g587 = -_VertexTwist;
				float angle11_g587 = radians( temp_output_9_0_g587 );
				float3 output11_g587 = float3( 0,0,0 );
				TwistXZ_float( position11_g587 , angle11_g587 , output11_g587 );
				float3 worldToObjDir636 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY1, 0.0 ) ).xyz;
				float3 worldToObjDir780 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY2, 0.0 ) ).xyz;
				float UV_2D_Circular_Y897 = sin( ( UV_2D_Y691 * PI ) );
				float3 Vertex_Offset_over_Y639 = ( ( worldToObjDir636 * pow( UV_2D_Y691 , _VertexOffsetOverY1Power ) ) + ( worldToObjDir780 * pow( UV_2D_Y691 , _VertexOffsetOverY2Power ) ) + ( _VertexOffsetOverCircularY * pow( UV_2D_Circular_Y897 , _VertexOffsetOverCircularYPower ) ) );
				float3 Vertex_Offset247 = ( output11_g587 + Vertex_Offset_over_Y639 );
				
				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord6.x = eyeDepth;
				
				output.ase_texcoord3 = input.ase_texcoord;
				output.ase_texcoord4 = input.positionOS;
				output.ase_texcoord5 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord6.yzw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = Vertex_Offset247;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				float fogFactor = 0;
				#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
					fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWSAndFogFactor = float4( vertexInput.positionWS, fogFactor );
				output.normalWS = normalInput.normalWS;
				output.tangentWS = half4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _Tessellation; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined( _SURFACE_TYPE_TRANSPARENT )
					const bool isTransparent = true;
				#else
					const bool isTransparent = false;
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWSAndFogFactor.xyz );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWSAndFogFactor.xyz;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				half3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				float4 temp_output_22_0_g590 = _ColourB;
				float4 temp_output_22_0_g591 = _ColourA;
				float temp_output_7_0_g574 = _NoiseRemapMin;
				float temp_output_23_0_g574 = _NoiseRemapMax;
				float localSimplexNoise_float2_g507 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float2 UV_2D327 = staticSwitch321;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.ase_texcoord4.xyz).yxz;
				#else
				float3 staticSwitch339 = input.ase_texcoord4.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				#ifdef _OBJECTSPACEUVS_ON
				float3 staticSwitch223 = UV_3D340;
				#else
				float3 staticSwitch223 = float3( UV_2D327 ,  0.0 );
				#endif
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (PositionWS).yxz;
				#else
				float3 staticSwitch621 = PositionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch356 = UV_3D_World622;
				#else
				float3 staticSwitch356 = staticSwitch223;
				#endif
				float2 appendResult1146 = (float2(ScreenPosNorm.x , ScreenPosNorm.y));
				float2 Screen_UV1145 = appendResult1146;
				float2 appendResult1112 = (float2(_ScaledScreenParams.x , _ScaledScreenParams.y));
				float2 Screen_Resolution1113 = appendResult1112;
				float2 Screen_Position1254 = ( Screen_UV1145 * Screen_Resolution1113 );
				float2 screenPosition1243 = Screen_Position1254;
				float mulTime1247 = _TimeParameters.x * 60.0;
				float time1243 = mulTime1247;
				float3 localHash331243 = Hash33( screenPosition1243 , time1243 );
				float3 Sample_Noise1248 = ( (localHash331243*2.0 + -1.0) * _UVSampleNoise );
				float3 Noise_Base_UV220 = ( staticSwitch356 + Sample_Noise1248 );
				float localSpherize_float5_g491 = ( 0.0 );
				float2 uv5_g491 = (Noise_Base_UV220).xy;
				float2 center5_g491 = ( _SpherizeNoiseOffset + float2( 0.5,0.5 ) );
				float radius5_g491 = _SpherizeNoiseRadius;
				float strength5_g491 = _SpherizeNoiseStrength;
				float2 output5_g491 = float2( 0,0 );
				Spherize_float( uv5_g491 , center5_g491 , radius5_g491 , strength5_g491 , output5_g491 );
				float3 appendResult611 = (float3(output5_g491 , (Noise_Base_UV220).z));
				#ifdef _SPHERIZENOISE_ON
				float3 staticSwitch625 = appendResult611;
				#else
				float3 staticSwitch625 = Noise_Base_UV220;
				#endif
				float localTwistXZ_float11_g495 = ( 0.0 );
				float3 temp_output_10_0_g495 = staticSwitch625;
				float3 position11_g495 = temp_output_10_0_g495;
				float temp_output_9_0_g495 = _NoiseXZTwist;
				float angle11_g495 = radians( temp_output_9_0_g495 );
				float3 output11_g495 = float3( 0,0,0 );
				TwistXZ_float( position11_g495 , angle11_g495 , output11_g495 );
				#ifdef _NOISEXZTWISTENABLED_ON
				float3 staticSwitch801 = output11_g495;
				#else
				float3 staticSwitch801 = staticSwitch625;
				#endif
				float3 break728 = staticSwitch801;
				float temp_output_738_0 = ( ( break728.y - _NoiseUVYPreOffset ) * _NoiseUVYPreScale );
				float temp_output_7_0_g499 = abs( temp_output_738_0 );
				float temp_output_733_14 = ( pow( temp_output_7_0_g499 , _NoiseUVYPrePower ) * sign( temp_output_738_0 ) );
				float3 appendResult732 = (float3(break728.x , temp_output_733_14 , break728.z));
				float3 temp_output_872_0 = ( -ViewDirWS * _NoiseParallaxOffset );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch464 = (temp_output_872_0).yxz;
				#else
				float3 staticSwitch464 = temp_output_872_0;
				#endif
				float3 Parallax_Offset465 = staticSwitch464;
				float localSimplexNoise_float2_g493 = ( 0.0 );
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord3.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord3.z;
				float4 Distortion_Noise_Offset163 = ( _NoiseDistortionOffset + Particle_Stable_Random_X147 + ( _NoiseDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g489 = ( float4( ( ( Noise_Base_UV220 + Parallax_Offset465 ) * _NoiseDistortionScale * _NoiseDistortionTiling ) , 0.0 ) - ( Distortion_Noise_Offset163 + ( _NoiseDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_160_0 = (temp_output_10_0_g489).xyz;
				float3 position2_g493 = temp_output_160_0;
				float temp_output_160_15 = (temp_output_10_0_g489).w;
				float angle2_g493 = temp_output_160_15;
				float octaves2_g493 = _NoiseDistortionOctaves;
				float noise2_g493 = 0.0;
				float3 gradient2_g493 = float3( 0,0,0 );
				SimplexNoise_float( position2_g493 , angle2_g493 , octaves2_g493 , noise2_g493 , gradient2_g493 );
				float localSimplexNoise_Caustics_float2_g492 = ( 0.0 );
				float3 position2_g492 = temp_output_160_0;
				float angle2_g492 = temp_output_160_15;
				float octaves2_g492 = _NoiseDistortionOctaves;
				float gradientStrength2_g492 = _NoiseDistortionDilation;
				float noise2_g492 = 0.0;
				float3 gradient2_g492 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g492 , angle2_g492 , octaves2_g492 , gradientStrength2_g492 , noise2_g492 , gradient2_g492 );
				#ifdef _NOISEDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch440 = gradient2_g492;
				#else
				float3 staticSwitch440 = gradient2_g493;
				#endif
				float3 temp_output_7_0_g497 = abs( staticSwitch440 );
				float3 temp_cast_2 = (_NoiseDistortionPower).xxx;
				#ifdef _NOISEDISTORTIONENABLED_ON
				float3 staticSwitch442 = ( ( pow( temp_output_7_0_g497 , temp_cast_2 ) * sign( staticSwitch440 ) ) * _NoiseDistortion );
				#else
				float3 staticSwitch442 = float3( 0,0,0 );
				#endif
				float3 Noise_Distortion73 = staticSwitch442;
				float3 Noise_UV173 = ( appendResult732 + Parallax_Offset465 + Noise_Distortion73 );
				float4 Noise_Offset175 = ( _NoiseOffset + Particle_Stable_Random_X147 + ( _NoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g503 = ( float4( ( Noise_UV173 * _NoiseScale * _NoiseTiling ) , 0.0 ) - ( Noise_Offset175 + ( _NoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_159_0 = (temp_output_10_0_g503).xyz;
				float3 position2_g507 = temp_output_159_0;
				float temp_output_159_15 = (temp_output_10_0_g503).w;
				float angle2_g507 = temp_output_159_15;
				float octaves2_g507 = _NoiseOctaves;
				float noise2_g507 = 0.0;
				float3 gradient2_g507 = float3( 0,0,0 );
				SimplexNoise_float( position2_g507 , angle2_g507 , octaves2_g507 , noise2_g507 , gradient2_g507 );
				float localSimplexNoise_Caustics_float2_g506 = ( 0.0 );
				float3 position2_g506 = temp_output_159_0;
				float angle2_g506 = temp_output_159_15;
				float octaves2_g506 = _NoiseOctaves;
				float gradientStrength2_g506 = _NoiseDilation;
				float noise2_g506 = 0.0;
				float3 gradient2_g506 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g506 , angle2_g506 , octaves2_g506 , gradientStrength2_g506 , noise2_g506 , gradient2_g506 );
				#ifdef _NOISEDILATIONENABLED_ON
				float staticSwitch444 = noise2_g506;
				#else
				float staticSwitch444 = noise2_g507;
				#endif
				float temp_output_20_0_g574 = staticSwitch444;
				float temp_output_4_0_g574 = _NoisePower;
				float smoothstepResult22_g574 = smoothstep( temp_output_7_0_g574 , temp_output_23_0_g574 , pow( temp_output_20_0_g574 , temp_output_4_0_g574 ));
				float Particle_Subtract_Noise_over_Lifetime480 = ( input.ase_texcoord5.y * _ParticleSubtractNoiseoverLifetime );
				float temp_output_478_0 = ( smoothstepResult22_g574 - Particle_Subtract_Noise_over_Lifetime480 );
				float lerpResult569 = lerp( 1.0 , temp_output_478_0 , _Noise);
				float Noise11 = lerpResult569;
				float Colour_Power42 = pow( Noise11 , _ColourPower );
				float3 lerpResult31 = lerp( ( (temp_output_22_0_g590).rgb * (temp_output_22_0_g590).a ) , ( (temp_output_22_0_g591).rgb * (temp_output_22_0_g591).a ) , Colour_Power42);
				float3 hsvTorgb129 = RGBToHSV( lerpResult31 );
				float3 hsvTorgb132 = HSVToRGB( float3(( hsvTorgb129.x + _ColourHueShift ),( hsvTorgb129.y + _ColourSaturationShift ),( hsvTorgb129.z * _ColourValueMultiplier )) );
				float4 temp_output_22_0_g592 = _VerticalColourB;
				float4 temp_output_22_0_g593 = _VerticalColourA;
				float3 lerpResult387 = lerp( ( (temp_output_22_0_g592).rgb * (temp_output_22_0_g592).a ) , ( (temp_output_22_0_g593).rgb * (temp_output_22_0_g593).a ) , Colour_Power42);
				float3 hsvTorgb402 = RGBToHSV( lerpResult387 );
				float3 hsvTorgb401 = HSVToRGB( float3(( hsvTorgb402.x + _VerticalColourHueShift ),( hsvTorgb402.y + _VerticalColourSaturationShift ),( hsvTorgb402.z * _VerticalColourValueMultiplier )) );
				float temp_output_7_0_g598 = _VerticalColourMaskRemapMin;
				float temp_output_23_0_g598 = _VerticalColourMaskRemapMax;
				float UV_2D_Y691 = (staticSwitch321).y;
				float temp_output_20_0_g598 = UV_2D_Y691;
				float temp_output_4_0_g598 = _VerticalColourMaskPower;
				float smoothstepResult22_g598 = smoothstep( temp_output_7_0_g598 , temp_output_23_0_g598 , pow( temp_output_20_0_g598 , temp_output_4_0_g598 ));
				float Vertical_Colour_Mask369 = smoothstepResult22_g598;
				float3 lerpResult370 = lerp( hsvTorgb132 , hsvTorgb401 , Vertical_Colour_Mask369);
				#ifdef _VERTICALCOLOUR_ON
				float3 staticSwitch392 = lerpResult370;
				#else
				float3 staticSwitch392 = hsvTorgb132;
				#endif
				float3 Colour_Input393 = staticSwitch392;
				float4 Vertex_Colour951 = input.ase_color;
				float3 localGetMainLightColourFlat_float1118 = ( float3( 0,0,0 ) );
				float3 viewDirection1118 = ViewDirWS;
				float3 output1118 = float3( 0,0,0 );
				GetMainLightColourFlat_float( viewDirection1118 , output1118 );
				#ifdef _MAINLIGHTFLATENABLED_ON
				float3 staticSwitch1207 = output1118;
				#else
				float3 staticSwitch1207 = float3( 0,0,0 );
				#endif
				float3 localGetMainLightColourLambert_float1197 = ( float3( 0,0,0 ) );
				float3 viewDirection1197 = ViewDirWS;
				float3 temp_output_111_0_g605 = ddx( PositionWS );
				float3 temp_output_113_0_g605 = cross( ddy( PositionWS ) , NormalWS );
				float dotResult115_g605 = dot( temp_output_111_0_g605 , temp_output_113_0_g605 );
				float temp_output_20_0_g605 = Noise11;
				float3 normalizeResult130_g605 = normalize( ( ( abs( dotResult115_g605 ) * NormalWS ) - ( _LambertLightingNormalfromHeight * float3( 0.05,0.05,0.05 ) * sign( dotResult115_g605 ) * ( ( ddx( temp_output_20_0_g605 ) * temp_output_113_0_g605 ) + ( ddy( temp_output_20_0_g605 ) * cross( NormalWS , temp_output_111_0_g605 ) ) ) ) ) );
				float3 Normal_from_Height_Front1171 = normalizeResult130_g605;
				float3 normal1197 = Normal_from_Height_Front1171;
				float3 output1197 = float3( 0,0,0 );
				GetMainLightColourLambert_float( viewDirection1197 , normal1197 , output1197 );
				#ifdef _MAINLIGHTLAMBERTENABLED_ON
				float3 staticSwitch1208 = output1197;
				#else
				float3 staticSwitch1208 = float3( 0,0,0 );
				#endif
				float3 lerpResult1214 = lerp( staticSwitch1207 , staticSwitch1208 , _MainLightFlatLambertBlend);
				#ifdef _MAINLIGHTFLATENABLED_ON
				float3 staticSwitch1223 = lerpResult1214;
				#else
				float3 staticSwitch1223 = staticSwitch1208;
				#endif
				#ifdef _MAINLIGHTLAMBERTENABLED_ON
				float3 staticSwitch1215 = staticSwitch1223;
				#else
				float3 staticSwitch1215 = staticSwitch1207;
				#endif
				float3 Main_Light_Front1216 = staticSwitch1215;
				float3 localGetMainLightColourLambert_float1218 = ( float3( 0,0,0 ) );
				float3 viewDirection1218 = ViewDirWS;
				float3 temp_output_111_0_g604 = ddx( PositionWS );
				float3 temp_output_113_0_g604 = cross( ddy( PositionWS ) , NormalWS );
				float dotResult115_g604 = dot( temp_output_111_0_g604 , temp_output_113_0_g604 );
				float temp_output_20_0_g604 = ( ( 1.0 - Noise11 ) * 0.5 );
				float3 normalizeResult130_g604 = normalize( ( ( abs( dotResult115_g604 ) * NormalWS ) - ( _LambertLightingNormalfromHeight * float3( 0.05,0.05,0.05 ) * sign( dotResult115_g604 ) * ( ( ddx( temp_output_20_0_g604 ) * temp_output_113_0_g604 ) + ( ddy( temp_output_20_0_g604 ) * cross( NormalWS , temp_output_111_0_g604 ) ) ) ) ) );
				float3 Normal_from_Height_Back1172 = -normalizeResult130_g604;
				float3 normal1218 = Normal_from_Height_Back1172;
				float3 output1218 = float3( 0,0,0 );
				GetMainLightColourLambert_float( viewDirection1218 , normal1218 , output1218 );
				#ifdef _MAINLIGHTLAMBERTENABLED_ON
				float3 staticSwitch1206 = output1218;
				#else
				float3 staticSwitch1206 = float3( 0,0,0 );
				#endif
				float3 lerpResult1213 = lerp( float3( 0,0,0 ) , staticSwitch1206 , _MainLightFlatLambertBlend);
				#ifdef _MAINLIGHTFLATENABLED_ON
				float3 staticSwitch1220 = lerpResult1213;
				#else
				float3 staticSwitch1220 = staticSwitch1206;
				#endif
				float3 Main_Light_Back1217 = staticSwitch1220;
				float3 normalizeResult1228 = ASESafeNormalize( Main_Light_Back1217 );
				float lerpResult1230 = lerp( 1.0 , Noise11 , _MainLightLambertTranslucencyMask);
				#ifdef _MAINLIGHTLAMBERTENABLED_ON
				float3 staticSwitch1225 = ( Main_Light_Front1216 + ( normalizeResult1228 * length( Main_Light_Back1217 ) * ( lerpResult1230 * _MainLightLambertTranslucency ) ) );
				#else
				float3 staticSwitch1225 = Main_Light_Front1216;
				#endif
				float localGetMainLightShadow_float1080 = ( 0.0 );
				float3 position1080 = PositionWS;
				float2 screenPosition1080 = Screen_Position1254;
				float worldNoise1080 = _MainLightShadowWorldNoise;
				float lightDirectionNoise1080 = _MainLightShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1080 = _MainLightShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1080 = _MainLightShadowDirectionalAlignmentNoise;
				float3 normal1080 = NormalWS;
				float3x3 ase_worldToTangent = float3x3( TangentWS, BitangentWS, NormalWS );
				float3x3 worldToTangentMatrix1080 = ase_worldToTangent;
				float output1080 = 0.0;
				GetMainLightShadow_float( position1080 , screenPosition1080 , worldNoise1080 , lightDirectionNoise1080 , lightDirectionOrthogonalAlignmentNoise1080 , lightDirectionAlignmentNoise1080 , normal1080 , worldToTangentMatrix1080 , output1080 );
				float lerpResult963 = lerp( 1.0 , output1080 , _MainLightShadow);
				float Main_Light_Shadow1116 = lerpResult963;
				float3 localGetMainLightAnisotropy_float1203 = ( float3( 0,0,0 ) );
				float anisotropy1203 = _MainLightAnisotropy;
				float3 viewDirection1203 = ViewDirWS;
				float output1203 = 0.0;
				GetMainLightAnisotropy_float( anisotropy1203 , viewDirection1203 , output1203 );
				float lerpResult1205 = lerp( 1.0 , output1203 , _MainLightAnisotropyBlend);
				float3 Main_Light988 = ( staticSwitch1225 * Main_Light_Shadow1116 * _MainLight * lerpResult1205 );
				#ifdef _MAINLIGHTENABLED_ON
				float3 staticSwitch494 = Main_Light988;
				#else
				float3 staticSwitch494 = float3( 0,0,0 );
				#endif
				float localGetAdditionalLightsColourFlat_float1167 = ( 0.0 );
				float3 position1167 = PositionWS;
				float2 screenUV1167 = Screen_UV1145;
				float3 output1167 = float3( 0,0,0 );
				GetAdditionalLightsColourFlat_float( position1167 , screenUV1167 , output1167 );
				#ifdef _ADDITIONALLIGHTSFLATENABLED_ON
				float3 staticSwitch1179 = output1167;
				#else
				float3 staticSwitch1179 = float3( 0,0,0 );
				#endif
				float localGetAdditionalLightsColourLambert_float1135 = ( 0.0 );
				float3 position1135 = PositionWS;
				float2 screenUV1135 = Screen_UV1145;
				float3 normal1135 = Normal_from_Height_Front1171;
				float3 output1135 = float3( 0,0,0 );
				GetAdditionalLightsColourLambert_float( position1135 , screenUV1135 , normal1135 , output1135 );
				#ifdef _ADDITIONALLIGHTSLAMBERTENABLED_ON
				float3 staticSwitch1182 = output1135;
				#else
				float3 staticSwitch1182 = float3( 0,0,0 );
				#endif
				float3 lerpResult1176 = lerp( staticSwitch1179 , staticSwitch1182 , _AdditionalLightsFlatLambertBlend);
				#ifdef _ADDITIONALLIGHTSFLATENABLED_ON
				float3 staticSwitch1222 = lerpResult1176;
				#else
				float3 staticSwitch1222 = staticSwitch1182;
				#endif
				#ifdef _ADDITIONALLIGHTSLAMBERTENABLED_ON
				float3 staticSwitch1198 = staticSwitch1222;
				#else
				float3 staticSwitch1198 = staticSwitch1179;
				#endif
				float3 Additional_Lights_Front1175 = staticSwitch1198;
				float localGetAdditionalLightsColourLambert_float1137 = ( 0.0 );
				float3 position1137 = PositionWS;
				float2 screenUV1137 = Screen_UV1145;
				float3 normal1137 = Normal_from_Height_Back1172;
				float3 output1137 = float3( 0,0,0 );
				GetAdditionalLightsColourLambert_float( position1137 , screenUV1137 , normal1137 , output1137 );
				#ifdef _ADDITIONALLIGHTSLAMBERTENABLED_ON
				float3 staticSwitch1183 = output1137;
				#else
				float3 staticSwitch1183 = float3( 0,0,0 );
				#endif
				float3 lerpResult1199 = lerp( float3( 0,0,0 ) , staticSwitch1183 , _AdditionalLightsFlatLambertBlend);
				#ifdef _ADDITIONALLIGHTSFLATENABLED_ON
				float3 staticSwitch1221 = lerpResult1199;
				#else
				float3 staticSwitch1221 = staticSwitch1183;
				#endif
				float3 Additional_Lights_Back1180 = staticSwitch1221;
				float3 normalizeResult540 = ASESafeNormalize( Additional_Lights_Back1180 );
				float lerpResult1141 = lerp( 1.0 , Noise11 , _AdditionalLightsLambertTranslucencyMask);
				#ifdef _ADDITIONALLIGHTSLAMBERTENABLED_ON
				float3 staticSwitch1200 = ( Additional_Lights_Front1175 + ( normalizeResult540 * length( Additional_Lights_Back1180 ) * ( lerpResult1141 * _AdditionalLightsLambertTranslucency ) ) );
				#else
				float3 staticSwitch1200 = Additional_Lights_Front1175;
				#endif
				float localGetAdditionalLightsShadow_float1125 = ( 0.0 );
				float3 position1125 = PositionWS;
				float2 screenPosition1125 = Screen_Position1254;
				float2 screenUV1125 = Screen_UV1145;
				float worldNoise1125 = _AdditionalLightsShadowWorldNoise;
				float lightDirectionNoise1125 = _AdditionalLightsShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1125 = _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1125 = _AdditionalLightsShadowDirectionalAlignmentNoise;
				float3 normal1125 = NormalWS;
				float3x3 worldToTangentMatrix1125 = ase_worldToTangent;
				float output1125 = 0.0;
				GetAdditionalLightsShadow_float( position1125 , screenPosition1125 , screenUV1125 , worldNoise1125 , lightDirectionNoise1125 , lightDirectionOrthogonalAlignmentNoise1125 , lightDirectionAlignmentNoise1125 , normal1125 , worldToTangentMatrix1125 , output1125 );
				float lerpResult1241 = lerp( 1.0 , output1125 , _AdditionalLightsShadow);
				float Additional_Lights_Shadow1153 = lerpResult1241;
				float3 Additional_Lights552 = ( staticSwitch1200 * Additional_Lights_Shadow1153 * _AdditionalLights );
				#ifdef _ADDITIONALLIGHTSENABLED_ON
				float3 staticSwitch1079 = Additional_Lights552;
				#else
				float3 staticSwitch1079 = float3( 0,0,0 );
				#endif
				float3 Lighting495 = ( staticSwitch494 + staticSwitch1079 );
				float3 hsvTorgb1067 = RGBToHSV( Colour_Input393 );
				float3 hsvTorgb1073 = HSVToRGB( float3(( hsvTorgb1067.x + _IntersectionHighlightColourHueShift ),( hsvTorgb1067.y + _IntersectionHighlightColourSaturationShift ),( hsvTorgb1067.z * _IntersectionHighlightColourValueMultiplier )) );
				float temp_output_7_0_g581 = _IntersectionHighlightRemapMin;
				float temp_output_23_0_g581 = _IntersectionHighlightRemapMax;
				float screenDepth1005 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth1005 = saturate( abs( ( screenDepth1005 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _IntersectionHighlight ) ) );
				float temp_output_20_0_g581 = ( 1.0 - distanceDepth1005 );
				float temp_output_4_0_g581 = _IntersectionHighlightPower;
				float smoothstepResult22_g581 = smoothstep( temp_output_7_0_g581 , temp_output_23_0_g581 , pow( temp_output_20_0_g581 , temp_output_4_0_g581 ));
				float Intersection_Highlight1011 = smoothstepResult22_g581;
				float4 lerpResult1018 = lerp( ( ( float4( Colour_Input393 , 0.0 ) * Vertex_Colour951 ) + float4( Lighting495 , 0.0 ) ) , float4( ( hsvTorgb1073 * _IntersectionHighlightColour.rgb ) , 0.0 ) , pow( Intersection_Highlight1011 , 0.0001 ));
				float4 Colour50 = lerpResult1018;
				
				float Particle_Mask_Radius_over_Lifetime161 = input.ase_texcoord5.x;
				float lerpResult183 = lerp( 1.0 , Particle_Mask_Radius_over_Lifetime161 , _RadialMaskRadiusOverParticleLifetime);
				float temp_output_6_0_g500 = ( 1.0 - ( _RadialMaskRadius * lerpResult183 ) );
				float lerpResult5_g500 = lerp( temp_output_6_0_g500 , 1.0 , _RadialMaskFeather);
				float2 texCoord330 = input.ase_texcoord3.xy * float2( 2,2 ) + float2( -1,-1 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch329 = (texCoord330).yx;
				#else
				float2 staticSwitch329 = texCoord330;
				#endif
				float2 UV_2D_Centered328 = staticSwitch329;
				float localSimplexNoise_float2_g486 = ( 0.0 );
				float4 Mask_Distortion_Noise_Offset171 = ( _RadialMaskDistortionOffset + Particle_Stable_Random_X147 + ( _RadialMaskDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g483 = ( float4( ( Noise_Base_UV220 * _RadialMaskDistortionScale * _RadialMaskDistortionTiling ) , 0.0 ) - ( Mask_Distortion_Noise_Offset171 + ( _RadialMaskDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_158_0 = (temp_output_10_0_g483).xyz;
				float3 position2_g486 = temp_output_158_0;
				float temp_output_158_15 = (temp_output_10_0_g483).w;
				float angle2_g486 = temp_output_158_15;
				float octaves2_g486 = _RadialMaskDistortionOctaves;
				float noise2_g486 = 0.0;
				float3 gradient2_g486 = float3( 0,0,0 );
				SimplexNoise_float( position2_g486 , angle2_g486 , octaves2_g486 , noise2_g486 , gradient2_g486 );
				float localSimplexNoise_Caustics_float2_g485 = ( 0.0 );
				float3 position2_g485 = temp_output_158_0;
				float angle2_g485 = temp_output_158_15;
				float octaves2_g485 = _RadialMaskDistortionOctaves;
				float gradientStrength2_g485 = _RadialMaskDistortionDilation;
				float noise2_g485 = 0.0;
				float3 gradient2_g485 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g485 , angle2_g485 , octaves2_g485 , gradientStrength2_g485 , noise2_g485 , gradient2_g485 );
				#ifdef _RADIALMASKDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch441 = gradient2_g485;
				#else
				float3 staticSwitch441 = gradient2_g486;
				#endif
				float3 temp_output_7_0_g494 = abs( staticSwitch441 );
				float3 temp_cast_9 = (_RadialMaskDistortionPower).xxx;
				#ifdef _RADIALMASKDISTORTIONENABLED_ON
				float3 staticSwitch451 = ( ( pow( temp_output_7_0_g494 , temp_cast_9 ) * sign( staticSwitch441 ) ) * _RadialMaskDistortion );
				#else
				float3 staticSwitch451 = float3( 0,0,0 );
				#endif
				float3 Mask_Distortion117 = staticSwitch451;
				float temp_output_7_0_g500 = ( 1.0 - length( ( ( ( UV_2D_Centered328 + (Mask_Distortion117).xy ) - _RadialMaskOffset ) * _RadialMaskTiling ) ) );
				float smoothstepResult4_g500 = smoothstep( temp_output_6_0_g500 , lerpResult5_g500 , temp_output_7_0_g500);
				#ifdef _RADIALMASK_ON
				float staticSwitch601 = ( 1.0 - pow( smoothstepResult4_g500 , _RadialMaskPower ) );
				#else
				float staticSwitch601 = 0.0;
				#endif
				float Radial_Mask227 = staticSwitch601;
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch942 = Radial_Mask227;
				#else
				float staticSwitch942 = 0.0;
				#endif
				float temp_output_7_0_g505 = _VerticalMask1RemapMax;
				float temp_output_23_0_g505 = _VerticalMask1RemapMin;
				float UV_3D_Y719 = (staticSwitch339).y;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch716 = ( ( UV_3D_Y719 - _VerticalMask1ObjectSpaceOffset ) / _VerticalMask1ObjectSpaceScale );
				#else
				float staticSwitch716 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g505 = staticSwitch716;
				float smoothstepResult25_g505 = smoothstep( temp_output_7_0_g505 , temp_output_23_0_g505 , temp_output_20_0_g505);
				float temp_output_4_0_g505 = _VerticalMask1Power;
				float temp_output_862_0 = pow( smoothstepResult25_g505 , temp_output_4_0_g505 );
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch713 = ( 1.0 - temp_output_862_0 );
				#else
				float staticSwitch713 = temp_output_862_0;
				#endif
				float Vertical_Mask_1350 = staticSwitch713;
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch709 = ( staticSwitch942 + Vertical_Mask_1350 );
				#else
				float staticSwitch709 = staticSwitch942;
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch741 = staticSwitch709;
				#else
				float staticSwitch741 = staticSwitch942;
				#endif
				float temp_output_7_0_g508 = _VerticalMask2RemapMin;
				float temp_output_23_0_g508 = _VerticalMask2RemapMax;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch767 = ( ( UV_3D_Y719 - _VerticalMask2ObjectSpaceOffset ) / _VerticalMask2ObjectSpaceScale );
				#else
				float staticSwitch767 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g508 = staticSwitch767;
				float smoothstepResult25_g508 = smoothstep( temp_output_7_0_g508 , temp_output_23_0_g508 , temp_output_20_0_g508);
				float temp_output_4_0_g508 = _VerticalMask2Power;
				float temp_output_863_0 = pow( smoothstepResult25_g508 , temp_output_4_0_g508 );
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch758 = ( 1.0 - temp_output_863_0 );
				#else
				float staticSwitch758 = temp_output_863_0;
				#endif
				float Vertical_Mask_2759 = staticSwitch758;
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch760 = ( staticSwitch741 + Vertical_Mask_2759 );
				#else
				float staticSwitch760 = staticSwitch741;
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch766 = staticSwitch760;
				#else
				float staticSwitch766 = staticSwitch741;
				#endif
				float fresnelNdotV232 = dot( NormalWS, ViewDirWS );
				float fresnelNode232 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV232, _FresnelMaskPower ) );
				float smoothstepResult234 = smoothstep( _FresnelMaskRemapMin , _FresnelMaskRemapMax , fresnelNode232);
				float lerpResult242 = lerp( 1.0 , smoothstepResult234 , _FresnelMask);
				float Fresnel_Mask233 = lerpResult242;
				float temp_output_7_0_g576 = 0.0;
				float temp_output_23_0_g576 = 1.0;
				float screenDepth186 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth186 = saturate( abs( ( screenDepth186 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				#ifdef _INVERTDEPTHFADE_ON
				float staticSwitch218 = ( 1.0 - distanceDepth186 );
				#else
				float staticSwitch218 = distanceDepth186;
				#endif
				float temp_output_20_0_g576 = staticSwitch218;
				float temp_output_4_0_g576 = _DepthFadePower;
				float smoothstepResult22_g576 = smoothstep( temp_output_7_0_g576 , temp_output_23_0_g576 , pow( temp_output_20_0_g576 , temp_output_4_0_g576 ));
				float temp_output_7_0_g575 = 0.0;
				float temp_output_23_0_g575 = 1.0;
				float screenDepth208 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth208 = saturate( abs( ( screenDepth208 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _SubtractiveDepthFade ) ) );
				float temp_output_20_0_g575 = ( 1.0 - distanceDepth208 );
				float temp_output_4_0_g575 = _SubtractiveDepthFadePower;
				float smoothstepResult22_g575 = smoothstep( temp_output_7_0_g575 , temp_output_23_0_g575 , pow( temp_output_20_0_g575 , temp_output_4_0_g575 ));
				float Depth_Fade188 = saturate( ( smoothstepResult22_g576 - smoothstepResult22_g575 ) );
				float temp_output_7_0_g577 = 0.0;
				float temp_output_23_0_g577 = 1.0;
				float eyeDepth = input.ase_texcoord6.x;
				float cameraDepthFade191 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g577 = saturate( cameraDepthFade191 );
				float temp_output_4_0_g577 = _CameraDepthFadePower;
				float smoothstepResult22_g577 = smoothstep( temp_output_7_0_g577 , temp_output_23_0_g577 , pow( temp_output_20_0_g577 , temp_output_4_0_g577 ));
				float Camera_Depth_Fade195 = smoothstepResult22_g577;
				float Intersection_Highlight_Alpha1024 = _IntersectionHighlightColour.a;
				float lerpResult984 = lerp( 1.0 , output1080 , _MainLightTransparencyShadow);
				float Main_Light_Transparency_Shadow981 = lerpResult984;
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1076 = Main_Light_Transparency_Shadow981;
				#else
				float staticSwitch1076 = 1.0;
				#endif
				float lerpResult993 = lerp( 1.0 , output1125 , _AdditionalLightsTransparencyShadow);
				float Additional_Lights_Transparency_Shadow994 = lerpResult993;
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1077 = Additional_Lights_Transparency_Shadow994;
				#else
				float staticSwitch1077 = 1.0;
				#endif
				float temp_output_1030_0 = saturate( ( ( saturate( ( Noise11 - staticSwitch766 ) ) * Fresnel_Mask233 * (input.ase_color).a * Depth_Fade188 * Camera_Depth_Fade195 * _Alpha ) + ( Intersection_Highlight1011 * Intersection_Highlight_Alpha1024 * pow( ( staticSwitch1076 * staticSwitch1077 ) , _IntersectionHighlightShadowMaskPower ) ) ) );
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch943 = temp_output_1030_0;
				#else
				float staticSwitch943 = ( temp_output_1030_0 * ( 1.0 - Radial_Mask227 ) );
				#endif
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch711 = staticSwitch943;
				#else
				float staticSwitch711 = ( staticSwitch943 * Vertical_Mask_1350 );
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch744 = staticSwitch711;
				#else
				float staticSwitch744 = staticSwitch943;
				#endif
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch763 = staticSwitch744;
				#else
				float staticSwitch763 = ( staticSwitch744 * Vertical_Mask_2759 );
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch768 = staticSwitch763;
				#else
				float staticSwitch768 = staticSwitch744;
				#endif
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch985 = ( staticSwitch768 * Main_Light_Transparency_Shadow981 );
				#else
				float staticSwitch985 = staticSwitch768;
				#endif
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch998 = ( staticSwitch985 * Additional_Lights_Transparency_Shadow994 );
				#else
				float staticSwitch998 = staticSwitch985;
				#endif
				float Alpha49 = staticSwitch998;
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = Colour50.rgb;
				float3 Normal = float3(0, 0, 1);
				float Alpha = Alpha49;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif


				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.normalWS = NormalWS;
				inputData.viewDirectionWS = ViewDirWS;

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.positionWSAndFogFactor.w);
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(input.positionCS, Color);
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						Color.rgb = MixFogColor(Color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						Color.rgb = MixFog(Color.rgb, inputData.fogCoord);
					#endif
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				#if defined( ASE_OPAQUE_KEEP_ALPHA )
					return half4( Color, Alpha );
				#else
					return half4( Color, OutputAlpha( Alpha, isTransparent ) );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define ASE_FIXED_TESSELLATION
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _RECEIVE_SHADOWS_OFF
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif


			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Math.cginc"
			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Noise.cginc"
			#include "../../../Mirza/Solaris/Shaders/_Includes/Solaris.hlsl"

			
			
			

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#define ASE_NEEDS_VERT_TANGENT
			#pragma shader_feature_local _SWAPUVXY_ON
			#pragma shader_feature_local _VERTEXWAVEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEDILATIONENABLED_ON
			#pragma shader_feature_local _WORLDSPACEUVS_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _VERTICALMASK2_ON
			#pragma shader_feature_local _VERTICALMASK1_ON
			#pragma shader_feature_local _RADIALMASKSUBTRACTIVE_ON
			#pragma shader_feature_local _NOISEDILATIONENABLED_ON
			#pragma shader_feature_local _NOISEXZTWISTENABLED_ON
			#pragma shader_feature_local _SPHERIZENOISE_ON
			#pragma shader_feature_local _OBJECTSPACEUVS_ON
			#pragma shader_feature_local _NOISEDISTORTIONENABLED_ON
			#pragma shader_feature_local _NOISEDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _RADIALMASK_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONENABLED_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _VERTICALMASK1SUBTRACTIVE_ON
			#pragma shader_feature_local _VERTICALMASKSOBJECTSPACE_ON
			#pragma shader_feature_local _VERTICALMASK2SUBTRACTIVE_ON
			#pragma shader_feature_local _INVERTDEPTHFADE_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ColourA;
			float4 _RadialMaskDistortionAnimation;
			float4 _VerticalColourA;
			float4 _VerticalColourB;
			float4 _RadialMaskDistortionParticleAnimation;
			float4 _RadialMaskDistortionOffset;
			float4 _NoiseDistortionOffset;
			float4 _ColourB;
			float4 _NoiseDistortionAnimation;
			float4 _NoiseDistortionParticleAnimation;
			float4 _IntersectionHighlightColour;
			float4 _VertexNoiseAnimation;
			float4 _NoiseAnimation;
			float4 _VertexNoiseParticleAnimation;
			float4 _NoiseOffset;
			float4 _NoiseParticleAnimation;
			float4 _VertexNoiseOffset;
			float3 _VertexOffsetOverY1;
			float3 _RadialMaskDistortionTiling;
			float3 _VertexOffsetOverCircularY;
			float3 _VertexOffsetOverY2;
			float3 _VertexNoiseTiling;
			float3 _NoiseTiling;
			float3 _NoiseDistortionTiling;
			float2 _RadialMaskTiling;
			float2 _RadialMaskOffset;
			float2 _SpherizeNoiseOffset;
			float _VerticalColourMaskRemapMax;
			float _MainLightShadowOrthogonalAlignmentWorldNoise;
			float _MainLightShadowDirectionalAlignmentNoise;
			float _Noise;
			float _MainLightShadow;
			float _MainLightAnisotropyBlend;
			float _MainLightAnisotropy;
			float _AdditionalLightsFlatLambertBlend;
			float _ParticleSubtractNoiseoverLifetime;
			float _NoisePower;
			float _AdditionalLightsLambertTranslucencyMask;
			float _MainLight;
			float _MainLightShadowDirectionalNoise;
			float _ColourPower;
			float _MainLightLambertTranslucency;
			float _ColourHueShift;
			float _MainLightLambertTranslucencyMask;
			float _ColourSaturationShift;
			float _ColourValueMultiplier;
			float _VerticalColourHueShift;
			float _VerticalColourSaturationShift;
			float _MainLightFlatLambertBlend;
			float _VerticalColourValueMultiplier;
			float _VerticalColourMaskRemapMin;
			float _LambertLightingNormalfromHeight;
			float _VerticalColourMaskPower;
			float _MainLightShadowWorldNoise;
			float _StartFoldoutRadialMask;
			float _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
			float _AdditionalLightsShadowWorldNoise;
			float _VerticalMask1Power;
			float _VerticalMask2RemapMin;
			float _VerticalMask2RemapMax;
			float _VerticalMask2ObjectSpaceOffset;
			float _VerticalMask2ObjectSpaceScale;
			float _VerticalMask2Power;
			float _FresnelMaskRemapMin;
			float _FresnelMaskRemapMax;
			float _FresnelMaskPower;
			float _FresnelMask;
			float _DepthFade;
			float _DepthFadePower;
			float _SubtractiveDepthFade;
			float _SubtractiveDepthFadePower;
			float _CameraDepthFadeLength;
			float _CameraDepthFadeOffset;
			float _CameraDepthFadePower;
			float _Alpha;
			float _MainLightTransparencyShadow;
			float _VerticalMask1ObjectSpaceScale;
			float _VerticalMask1ObjectSpaceOffset;
			float _VerticalMask1RemapMin;
			float _VerticalMask1RemapMax;
			float _AdditionalLightsShadowDirectionalNoise;
			float _AdditionalLightsShadowDirectionalAlignmentNoise;
			float _AdditionalLightsShadow;
			float _AdditionalLights;
			float _IntersectionHighlightColourHueShift;
			float _IntersectionHighlightColourSaturationShift;
			float _IntersectionHighlightColourValueMultiplier;
			float _IntersectionHighlightRemapMin;
			float _IntersectionHighlightRemapMax;
			float _AdditionalLightsLambertTranslucency;
			float _IntersectionHighlight;
			float _RadialMaskRadius;
			float _RadialMaskRadiusOverParticleLifetime;
			float _RadialMaskFeather;
			float _RadialMaskDistortionScale;
			float _RadialMaskDistortionOctaves;
			float _RadialMaskDistortionDilation;
			float _RadialMaskDistortionPower;
			float _RadialMaskDistortion;
			float _RadialMaskPower;
			float _IntersectionHighlightPower;
			float _NoiseDilation;
			float _NoiseDistortionDilation;
			float _NoiseScale;
			float _StartFoldoutFresnelMask;
			float _EndFoldoutFresnelMask;
			float _StartFoldoutDepthFade;
			float _EndFoldoutDepthFade;
			float _StartFoldoutIntersectionHighlight;
			float _EndFoldoutIntersectionHighlight;
			float _StartFoldoutVertexUVOffset;
			float _EndFoldoutVertexUVOffset;
			float _StartFoldoutVertexNormalOffset;
			float _EndFoldoutVertexNormalOffset;
			float _StartFoldoutVertexWave;
			float _EndFoldoutVertexWave;
			float _StartFoldoutVertexNoise;
			float _EndFoldoutVertexNoise;
			float _StartFoldoutVertexWaveNoiseVerticalMask;
			float _EndFoldoutVertexWaveNoiseVerticalMask;
			float _StartFoldoutVertexOffsetoverY;
			float _EndFoldoutSpherizeNoise;
			float _EndFoldoutVertexOffsetoverY;
			float _StartFoldoutSpherizeNoise;
			float _StartFoldoutVerticalMasks;
			float _StartFoldoutLighting;
			float _EndFoldoutLighting;
			float _StartFoldoutBaseUVs;
			float _EndFoldoutBaseUVs;
			float _StartFoldoutParticleSettings;
			float _EndFoldoutParticleSettings;
			float _StartFoldoutColour;
			float _EndFoldoutColour;
			float _StartFoldoutVerticalColour;
			float _EndFoldoutVerticalColour;
			float _StartFoldoutNoise;
			float _EndFoldoutNoise;
			float _StartFoldoutNoiseDistortion;
			float _EndFoldoutNoiseDistortion;
			float _EndFoldoutRadialMask;
			float _StartFoldoutRadialMaskDistortion;
			float _EndFoldoutRadialMaskDistortion;
			float _EndFoldoutVerticalMasks;
			float _Tessellation;
			float _VertexNormalOffset;
			float _VertexNormalOffsetTopPower;
			float _VertexOffsetOverY2Power;
			float _VertexOffsetOverCircularYPower;
			float _NoiseRemapMin;
			float _NoiseRemapMax;
			float _UVSampleNoise;
			float _SpherizeNoiseRadius;
			float _SpherizeNoiseStrength;
			float _NoiseXZTwist;
			float _NoiseUVYPreOffset;
			float _NoiseUVYPreScale;
			float _NoiseUVYPrePower;
			float _NoiseParallaxOffset;
			float _NoiseDistortionScale;
			float _NoiseDistortionOctaves;
			float _AdditionalLightsTransparencyShadow;
			float _NoiseDistortionPower;
			float _NoiseDistortion;
			float _VertexOffsetOverY1Power;
			float _VertexTwist;
			float _VertexUVOffsetBottom;
			float _VertexUVOffsetBottomPower;
			float _VertexNormalOffsetTop;
			float _VertexNormalOffsetBottomPower;
			float _VertexNormalOffsetBottom;
			float _VertexWaveScale;
			float _VertexWaveAnimation;
			float _VertexWaveOffset;
			float _VertexWave;
			float _VertexWaveNoiseVerticalMaskRemapMin;
			float _NoiseOctaves;
			float _VertexWaveNoiseVerticalMaskRemapMax;
			float _VertexNoiseScale;
			float _ParticleRandomization;
			float _VertexNoiseOctaves;
			float _VertexNoiseDilation;
			float _VertexNoiseTwist;
			float _VertexNoise;
			float _VertexUVOffsetTopPower;
			float _VertexUVOffsetTop;
			float _VertexWaveNoiseVerticalMaskPower;
			float _IntersectionHighlightShadowMaskPower;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float localTwistXZ_float11_g587 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float UV_2D_Y691 = (staticSwitch321).y;
				float3 Vertex_Normal_Offset670 = ( ( input.normalOS * _VertexNormalOffset ) + ( pow( UV_2D_Y691 , _VertexNormalOffsetTopPower ) * input.normalOS * _VertexNormalOffsetTop ) + ( pow( ( 1.0 - UV_2D_Y691 ) , _VertexNormalOffsetBottomPower ) * input.normalOS * _VertexNormalOffsetBottom ) );
				float2 UV_2D327 = staticSwitch321;
				float mulTime258 = _TimeParameters.x * _VertexWaveAnimation;
				float temp_output_7_0_g582 = _VertexWaveNoiseVerticalMaskRemapMin;
				float temp_output_23_0_g582 = _VertexWaveNoiseVerticalMaskRemapMax;
				float temp_output_20_0_g582 = UV_2D_Y691;
				float temp_output_4_0_g582 = _VertexWaveNoiseVerticalMaskPower;
				float smoothstepResult22_g582 = smoothstep( temp_output_7_0_g582 , temp_output_23_0_g582 , pow( temp_output_20_0_g582 , temp_output_4_0_g582 ));
				float Vertex_WaveNoise_Vertical_Mask280 = smoothstepResult22_g582;
				#ifdef _VERTEXWAVEENABLED_ON
				float3 staticSwitch448 = ( ( sin( ( ( UV_2D327.y * TWO_PI * _VertexWaveScale ) - ( mulTime258 + ( _VertexWaveOffset * TWO_PI ) ) ) ) * _VertexWave * Vertex_WaveNoise_Vertical_Mask280 ) * input.normalOS );
				#else
				float3 staticSwitch448 = float3( 0,0,0 );
				#endif
				float3 Vertex_Sine265 = staticSwitch448;
				float localTwistXZ_float11_g585 = ( 0.0 );
				float localSimplexNoise_float2_g584 = ( 0.0 );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.positionOS.xyz).yxz;
				#else
				float3 staticSwitch339 = input.positionOS.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch605 = UV_3D_World622;
				#else
				float3 staticSwitch605 = UV_3D340;
				#endif
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Vertex_Noise_Offset311 = ( _VertexNoiseOffset + Particle_Stable_Random_X147 + ( _VertexNoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g579 = ( float4( ( staticSwitch605 * _VertexNoiseScale * _VertexNoiseTiling ) , 0.0 ) - ( Vertex_Noise_Offset311 + ( _VertexNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_292_0 = (temp_output_10_0_g579).xyz;
				float3 position2_g584 = temp_output_292_0;
				float temp_output_292_15 = (temp_output_10_0_g579).w;
				float angle2_g584 = temp_output_292_15;
				float octaves2_g584 = _VertexNoiseOctaves;
				float noise2_g584 = 0.0;
				float3 gradient2_g584 = float3( 0,0,0 );
				SimplexNoise_float( position2_g584 , angle2_g584 , octaves2_g584 , noise2_g584 , gradient2_g584 );
				float localSimplexNoise_Caustics_float2_g583 = ( 0.0 );
				float3 position2_g583 = temp_output_292_0;
				float angle2_g583 = temp_output_292_15;
				float octaves2_g583 = _VertexNoiseOctaves;
				float gradientStrength2_g583 = _VertexNoiseDilation;
				float noise2_g583 = 0.0;
				float3 gradient2_g583 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g583 , angle2_g583 , octaves2_g583 , gradientStrength2_g583 , noise2_g583 , gradient2_g583 );
				#ifdef _VERTEXNOISEDILATIONENABLED_ON
				float3 staticSwitch490 = gradient2_g583;
				#else
				float3 staticSwitch490 = gradient2_g584;
				#endif
				float3 temp_output_10_0_g585 = staticSwitch490;
				float3 position11_g585 = temp_output_10_0_g585;
				float temp_output_9_0_g585 = _VertexNoiseTwist;
				float angle11_g585 = radians( temp_output_9_0_g585 );
				float3 output11_g585 = float3( 0,0,0 );
				TwistXZ_float( position11_g585 , angle11_g585 , output11_g585 );
				float3 temp_output_852_0 = output11_g585;
				#ifdef _VERTEXNOISEENABLED_ON
				float3 staticSwitch450 = ( temp_output_852_0 * _VertexNoise * Vertex_WaveNoise_Vertical_Mask280 );
				#else
				float3 staticSwitch450 = float3( 0,0,0 );
				#endif
				float3 Vertex_Noise298 = staticSwitch450;
				float2 break587 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_576_0 = ( ( break587.x * pow( break587.y , _VertexUVOffsetTopPower ) ) * ( _VertexUVOffsetTop * 0.5 ) );
				float3 appendResult575 = (float3(temp_output_576_0 , 0.0 , 0.0));
				float3 appendResult594 = (float3(0.0 , temp_output_576_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch593 = appendResult594;
				#else
				float3 staticSwitch593 = appendResult575;
				#endif
				float3 Vertex_Offset_Top579 = staticSwitch593;
				float2 break644 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_650_0 = ( ( break644.x * pow( ( 1.0 - break644.y ) , _VertexUVOffsetBottomPower ) ) * ( _VertexUVOffsetBottom * 0.5 ) );
				float3 appendResult653 = (float3(temp_output_650_0 , 0.0 , 0.0));
				float3 appendResult652 = (float3(0.0 , temp_output_650_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch654 = appendResult652;
				#else
				float3 staticSwitch654 = appendResult653;
				#endif
				float3 Vertex_Offset_Bottom655 = staticSwitch654;
				float3 temp_output_10_0_g587 = ( ( Vertex_Normal_Offset670 + Vertex_Sine265 + Vertex_Noise298 + Vertex_Offset_Top579 + Vertex_Offset_Bottom655 ) + input.positionOS.xyz );
				float3 position11_g587 = temp_output_10_0_g587;
				float temp_output_9_0_g587 = -_VertexTwist;
				float angle11_g587 = radians( temp_output_9_0_g587 );
				float3 output11_g587 = float3( 0,0,0 );
				TwistXZ_float( position11_g587 , angle11_g587 , output11_g587 );
				float3 worldToObjDir636 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY1, 0.0 ) ).xyz;
				float3 worldToObjDir780 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY2, 0.0 ) ).xyz;
				float UV_2D_Circular_Y897 = sin( ( UV_2D_Y691 * PI ) );
				float3 Vertex_Offset_over_Y639 = ( ( worldToObjDir636 * pow( UV_2D_Y691 , _VertexOffsetOverY1Power ) ) + ( worldToObjDir780 * pow( UV_2D_Y691 , _VertexOffsetOverY2Power ) ) + ( _VertexOffsetOverCircularY * pow( UV_2D_Circular_Y897 , _VertexOffsetOverCircularYPower ) ) );
				float3 Vertex_Offset247 = ( output11_g587 + Vertex_Offset_over_Y639 );
				
				output.ase_texcoord2.xyz = ase_positionWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord4.xyz = ase_normalWS;
				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord2.w = eyeDepth;
				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord5.xyz = ase_tangentWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord6.xyz = ase_bitangentWS;
				
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.positionOS;
				output.ase_texcoord3 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord4.w = 0;
				output.ase_texcoord5.w = 0;
				output.ase_texcoord6.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = Vertex_Offset247;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _Tessellation; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				float temp_output_7_0_g574 = _NoiseRemapMin;
				float temp_output_23_0_g574 = _NoiseRemapMax;
				float localSimplexNoise_float2_g507 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float2 UV_2D327 = staticSwitch321;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.ase_texcoord1.xyz).yxz;
				#else
				float3 staticSwitch339 = input.ase_texcoord1.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				#ifdef _OBJECTSPACEUVS_ON
				float3 staticSwitch223 = UV_3D340;
				#else
				float3 staticSwitch223 = float3( UV_2D327 ,  0.0 );
				#endif
				float3 ase_positionWS = input.ase_texcoord2.xyz;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch356 = UV_3D_World622;
				#else
				float3 staticSwitch356 = staticSwitch223;
				#endif
				float2 appendResult1146 = (float2(ScreenPosNorm.x , ScreenPosNorm.y));
				float2 Screen_UV1145 = appendResult1146;
				float2 appendResult1112 = (float2(_ScaledScreenParams.x , _ScaledScreenParams.y));
				float2 Screen_Resolution1113 = appendResult1112;
				float2 Screen_Position1254 = ( Screen_UV1145 * Screen_Resolution1113 );
				float2 screenPosition1243 = Screen_Position1254;
				float mulTime1247 = _TimeParameters.x * 60.0;
				float time1243 = mulTime1247;
				float3 localHash331243 = Hash33( screenPosition1243 , time1243 );
				float3 Sample_Noise1248 = ( (localHash331243*2.0 + -1.0) * _UVSampleNoise );
				float3 Noise_Base_UV220 = ( staticSwitch356 + Sample_Noise1248 );
				float localSpherize_float5_g491 = ( 0.0 );
				float2 uv5_g491 = (Noise_Base_UV220).xy;
				float2 center5_g491 = ( _SpherizeNoiseOffset + float2( 0.5,0.5 ) );
				float radius5_g491 = _SpherizeNoiseRadius;
				float strength5_g491 = _SpherizeNoiseStrength;
				float2 output5_g491 = float2( 0,0 );
				Spherize_float( uv5_g491 , center5_g491 , radius5_g491 , strength5_g491 , output5_g491 );
				float3 appendResult611 = (float3(output5_g491 , (Noise_Base_UV220).z));
				#ifdef _SPHERIZENOISE_ON
				float3 staticSwitch625 = appendResult611;
				#else
				float3 staticSwitch625 = Noise_Base_UV220;
				#endif
				float localTwistXZ_float11_g495 = ( 0.0 );
				float3 temp_output_10_0_g495 = staticSwitch625;
				float3 position11_g495 = temp_output_10_0_g495;
				float temp_output_9_0_g495 = _NoiseXZTwist;
				float angle11_g495 = radians( temp_output_9_0_g495 );
				float3 output11_g495 = float3( 0,0,0 );
				TwistXZ_float( position11_g495 , angle11_g495 , output11_g495 );
				#ifdef _NOISEXZTWISTENABLED_ON
				float3 staticSwitch801 = output11_g495;
				#else
				float3 staticSwitch801 = staticSwitch625;
				#endif
				float3 break728 = staticSwitch801;
				float temp_output_738_0 = ( ( break728.y - _NoiseUVYPreOffset ) * _NoiseUVYPreScale );
				float temp_output_7_0_g499 = abs( temp_output_738_0 );
				float temp_output_733_14 = ( pow( temp_output_7_0_g499 , _NoiseUVYPrePower ) * sign( temp_output_738_0 ) );
				float3 appendResult732 = (float3(break728.x , temp_output_733_14 , break728.z));
				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - ase_positionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 temp_output_872_0 = ( -ase_viewDirWS * _NoiseParallaxOffset );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch464 = (temp_output_872_0).yxz;
				#else
				float3 staticSwitch464 = temp_output_872_0;
				#endif
				float3 Parallax_Offset465 = staticSwitch464;
				float localSimplexNoise_float2_g493 = ( 0.0 );
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Distortion_Noise_Offset163 = ( _NoiseDistortionOffset + Particle_Stable_Random_X147 + ( _NoiseDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g489 = ( float4( ( ( Noise_Base_UV220 + Parallax_Offset465 ) * _NoiseDistortionScale * _NoiseDistortionTiling ) , 0.0 ) - ( Distortion_Noise_Offset163 + ( _NoiseDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_160_0 = (temp_output_10_0_g489).xyz;
				float3 position2_g493 = temp_output_160_0;
				float temp_output_160_15 = (temp_output_10_0_g489).w;
				float angle2_g493 = temp_output_160_15;
				float octaves2_g493 = _NoiseDistortionOctaves;
				float noise2_g493 = 0.0;
				float3 gradient2_g493 = float3( 0,0,0 );
				SimplexNoise_float( position2_g493 , angle2_g493 , octaves2_g493 , noise2_g493 , gradient2_g493 );
				float localSimplexNoise_Caustics_float2_g492 = ( 0.0 );
				float3 position2_g492 = temp_output_160_0;
				float angle2_g492 = temp_output_160_15;
				float octaves2_g492 = _NoiseDistortionOctaves;
				float gradientStrength2_g492 = _NoiseDistortionDilation;
				float noise2_g492 = 0.0;
				float3 gradient2_g492 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g492 , angle2_g492 , octaves2_g492 , gradientStrength2_g492 , noise2_g492 , gradient2_g492 );
				#ifdef _NOISEDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch440 = gradient2_g492;
				#else
				float3 staticSwitch440 = gradient2_g493;
				#endif
				float3 temp_output_7_0_g497 = abs( staticSwitch440 );
				float3 temp_cast_2 = (_NoiseDistortionPower).xxx;
				#ifdef _NOISEDISTORTIONENABLED_ON
				float3 staticSwitch442 = ( ( pow( temp_output_7_0_g497 , temp_cast_2 ) * sign( staticSwitch440 ) ) * _NoiseDistortion );
				#else
				float3 staticSwitch442 = float3( 0,0,0 );
				#endif
				float3 Noise_Distortion73 = staticSwitch442;
				float3 Noise_UV173 = ( appendResult732 + Parallax_Offset465 + Noise_Distortion73 );
				float4 Noise_Offset175 = ( _NoiseOffset + Particle_Stable_Random_X147 + ( _NoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g503 = ( float4( ( Noise_UV173 * _NoiseScale * _NoiseTiling ) , 0.0 ) - ( Noise_Offset175 + ( _NoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_159_0 = (temp_output_10_0_g503).xyz;
				float3 position2_g507 = temp_output_159_0;
				float temp_output_159_15 = (temp_output_10_0_g503).w;
				float angle2_g507 = temp_output_159_15;
				float octaves2_g507 = _NoiseOctaves;
				float noise2_g507 = 0.0;
				float3 gradient2_g507 = float3( 0,0,0 );
				SimplexNoise_float( position2_g507 , angle2_g507 , octaves2_g507 , noise2_g507 , gradient2_g507 );
				float localSimplexNoise_Caustics_float2_g506 = ( 0.0 );
				float3 position2_g506 = temp_output_159_0;
				float angle2_g506 = temp_output_159_15;
				float octaves2_g506 = _NoiseOctaves;
				float gradientStrength2_g506 = _NoiseDilation;
				float noise2_g506 = 0.0;
				float3 gradient2_g506 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g506 , angle2_g506 , octaves2_g506 , gradientStrength2_g506 , noise2_g506 , gradient2_g506 );
				#ifdef _NOISEDILATIONENABLED_ON
				float staticSwitch444 = noise2_g506;
				#else
				float staticSwitch444 = noise2_g507;
				#endif
				float temp_output_20_0_g574 = staticSwitch444;
				float temp_output_4_0_g574 = _NoisePower;
				float smoothstepResult22_g574 = smoothstep( temp_output_7_0_g574 , temp_output_23_0_g574 , pow( temp_output_20_0_g574 , temp_output_4_0_g574 ));
				float Particle_Subtract_Noise_over_Lifetime480 = ( input.ase_texcoord3.y * _ParticleSubtractNoiseoverLifetime );
				float temp_output_478_0 = ( smoothstepResult22_g574 - Particle_Subtract_Noise_over_Lifetime480 );
				float lerpResult569 = lerp( 1.0 , temp_output_478_0 , _Noise);
				float Noise11 = lerpResult569;
				float Particle_Mask_Radius_over_Lifetime161 = input.ase_texcoord3.x;
				float lerpResult183 = lerp( 1.0 , Particle_Mask_Radius_over_Lifetime161 , _RadialMaskRadiusOverParticleLifetime);
				float temp_output_6_0_g500 = ( 1.0 - ( _RadialMaskRadius * lerpResult183 ) );
				float lerpResult5_g500 = lerp( temp_output_6_0_g500 , 1.0 , _RadialMaskFeather);
				float2 texCoord330 = input.ase_texcoord.xy * float2( 2,2 ) + float2( -1,-1 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch329 = (texCoord330).yx;
				#else
				float2 staticSwitch329 = texCoord330;
				#endif
				float2 UV_2D_Centered328 = staticSwitch329;
				float localSimplexNoise_float2_g486 = ( 0.0 );
				float4 Mask_Distortion_Noise_Offset171 = ( _RadialMaskDistortionOffset + Particle_Stable_Random_X147 + ( _RadialMaskDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g483 = ( float4( ( Noise_Base_UV220 * _RadialMaskDistortionScale * _RadialMaskDistortionTiling ) , 0.0 ) - ( Mask_Distortion_Noise_Offset171 + ( _RadialMaskDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_158_0 = (temp_output_10_0_g483).xyz;
				float3 position2_g486 = temp_output_158_0;
				float temp_output_158_15 = (temp_output_10_0_g483).w;
				float angle2_g486 = temp_output_158_15;
				float octaves2_g486 = _RadialMaskDistortionOctaves;
				float noise2_g486 = 0.0;
				float3 gradient2_g486 = float3( 0,0,0 );
				SimplexNoise_float( position2_g486 , angle2_g486 , octaves2_g486 , noise2_g486 , gradient2_g486 );
				float localSimplexNoise_Caustics_float2_g485 = ( 0.0 );
				float3 position2_g485 = temp_output_158_0;
				float angle2_g485 = temp_output_158_15;
				float octaves2_g485 = _RadialMaskDistortionOctaves;
				float gradientStrength2_g485 = _RadialMaskDistortionDilation;
				float noise2_g485 = 0.0;
				float3 gradient2_g485 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g485 , angle2_g485 , octaves2_g485 , gradientStrength2_g485 , noise2_g485 , gradient2_g485 );
				#ifdef _RADIALMASKDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch441 = gradient2_g485;
				#else
				float3 staticSwitch441 = gradient2_g486;
				#endif
				float3 temp_output_7_0_g494 = abs( staticSwitch441 );
				float3 temp_cast_5 = (_RadialMaskDistortionPower).xxx;
				#ifdef _RADIALMASKDISTORTIONENABLED_ON
				float3 staticSwitch451 = ( ( pow( temp_output_7_0_g494 , temp_cast_5 ) * sign( staticSwitch441 ) ) * _RadialMaskDistortion );
				#else
				float3 staticSwitch451 = float3( 0,0,0 );
				#endif
				float3 Mask_Distortion117 = staticSwitch451;
				float temp_output_7_0_g500 = ( 1.0 - length( ( ( ( UV_2D_Centered328 + (Mask_Distortion117).xy ) - _RadialMaskOffset ) * _RadialMaskTiling ) ) );
				float smoothstepResult4_g500 = smoothstep( temp_output_6_0_g500 , lerpResult5_g500 , temp_output_7_0_g500);
				#ifdef _RADIALMASK_ON
				float staticSwitch601 = ( 1.0 - pow( smoothstepResult4_g500 , _RadialMaskPower ) );
				#else
				float staticSwitch601 = 0.0;
				#endif
				float Radial_Mask227 = staticSwitch601;
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch942 = Radial_Mask227;
				#else
				float staticSwitch942 = 0.0;
				#endif
				float temp_output_7_0_g505 = _VerticalMask1RemapMax;
				float temp_output_23_0_g505 = _VerticalMask1RemapMin;
				float UV_2D_Y691 = (staticSwitch321).y;
				float UV_3D_Y719 = (staticSwitch339).y;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch716 = ( ( UV_3D_Y719 - _VerticalMask1ObjectSpaceOffset ) / _VerticalMask1ObjectSpaceScale );
				#else
				float staticSwitch716 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g505 = staticSwitch716;
				float smoothstepResult25_g505 = smoothstep( temp_output_7_0_g505 , temp_output_23_0_g505 , temp_output_20_0_g505);
				float temp_output_4_0_g505 = _VerticalMask1Power;
				float temp_output_862_0 = pow( smoothstepResult25_g505 , temp_output_4_0_g505 );
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch713 = ( 1.0 - temp_output_862_0 );
				#else
				float staticSwitch713 = temp_output_862_0;
				#endif
				float Vertical_Mask_1350 = staticSwitch713;
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch709 = ( staticSwitch942 + Vertical_Mask_1350 );
				#else
				float staticSwitch709 = staticSwitch942;
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch741 = staticSwitch709;
				#else
				float staticSwitch741 = staticSwitch942;
				#endif
				float temp_output_7_0_g508 = _VerticalMask2RemapMin;
				float temp_output_23_0_g508 = _VerticalMask2RemapMax;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch767 = ( ( UV_3D_Y719 - _VerticalMask2ObjectSpaceOffset ) / _VerticalMask2ObjectSpaceScale );
				#else
				float staticSwitch767 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g508 = staticSwitch767;
				float smoothstepResult25_g508 = smoothstep( temp_output_7_0_g508 , temp_output_23_0_g508 , temp_output_20_0_g508);
				float temp_output_4_0_g508 = _VerticalMask2Power;
				float temp_output_863_0 = pow( smoothstepResult25_g508 , temp_output_4_0_g508 );
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch758 = ( 1.0 - temp_output_863_0 );
				#else
				float staticSwitch758 = temp_output_863_0;
				#endif
				float Vertical_Mask_2759 = staticSwitch758;
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch760 = ( staticSwitch741 + Vertical_Mask_2759 );
				#else
				float staticSwitch760 = staticSwitch741;
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch766 = staticSwitch760;
				#else
				float staticSwitch766 = staticSwitch741;
				#endif
				float3 ase_normalWS = input.ase_texcoord4.xyz;
				float fresnelNdotV232 = dot( ase_normalWS, ase_viewDirWS );
				float fresnelNode232 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV232, _FresnelMaskPower ) );
				float smoothstepResult234 = smoothstep( _FresnelMaskRemapMin , _FresnelMaskRemapMax , fresnelNode232);
				float lerpResult242 = lerp( 1.0 , smoothstepResult234 , _FresnelMask);
				float Fresnel_Mask233 = lerpResult242;
				float temp_output_7_0_g576 = 0.0;
				float temp_output_23_0_g576 = 1.0;
				float screenDepth186 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth186 = saturate( abs( ( screenDepth186 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				#ifdef _INVERTDEPTHFADE_ON
				float staticSwitch218 = ( 1.0 - distanceDepth186 );
				#else
				float staticSwitch218 = distanceDepth186;
				#endif
				float temp_output_20_0_g576 = staticSwitch218;
				float temp_output_4_0_g576 = _DepthFadePower;
				float smoothstepResult22_g576 = smoothstep( temp_output_7_0_g576 , temp_output_23_0_g576 , pow( temp_output_20_0_g576 , temp_output_4_0_g576 ));
				float temp_output_7_0_g575 = 0.0;
				float temp_output_23_0_g575 = 1.0;
				float screenDepth208 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth208 = saturate( abs( ( screenDepth208 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _SubtractiveDepthFade ) ) );
				float temp_output_20_0_g575 = ( 1.0 - distanceDepth208 );
				float temp_output_4_0_g575 = _SubtractiveDepthFadePower;
				float smoothstepResult22_g575 = smoothstep( temp_output_7_0_g575 , temp_output_23_0_g575 , pow( temp_output_20_0_g575 , temp_output_4_0_g575 ));
				float Depth_Fade188 = saturate( ( smoothstepResult22_g576 - smoothstepResult22_g575 ) );
				float temp_output_7_0_g577 = 0.0;
				float temp_output_23_0_g577 = 1.0;
				float eyeDepth = input.ase_texcoord2.w;
				float cameraDepthFade191 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g577 = saturate( cameraDepthFade191 );
				float temp_output_4_0_g577 = _CameraDepthFadePower;
				float smoothstepResult22_g577 = smoothstep( temp_output_7_0_g577 , temp_output_23_0_g577 , pow( temp_output_20_0_g577 , temp_output_4_0_g577 ));
				float Camera_Depth_Fade195 = smoothstepResult22_g577;
				float temp_output_7_0_g581 = _IntersectionHighlightRemapMin;
				float temp_output_23_0_g581 = _IntersectionHighlightRemapMax;
				float screenDepth1005 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth1005 = saturate( abs( ( screenDepth1005 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _IntersectionHighlight ) ) );
				float temp_output_20_0_g581 = ( 1.0 - distanceDepth1005 );
				float temp_output_4_0_g581 = _IntersectionHighlightPower;
				float smoothstepResult22_g581 = smoothstep( temp_output_7_0_g581 , temp_output_23_0_g581 , pow( temp_output_20_0_g581 , temp_output_4_0_g581 ));
				float Intersection_Highlight1011 = smoothstepResult22_g581;
				float Intersection_Highlight_Alpha1024 = _IntersectionHighlightColour.a;
				float localGetMainLightShadow_float1080 = ( 0.0 );
				float3 position1080 = ase_positionWS;
				float2 screenPosition1080 = Screen_Position1254;
				float worldNoise1080 = _MainLightShadowWorldNoise;
				float lightDirectionNoise1080 = _MainLightShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1080 = _MainLightShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1080 = _MainLightShadowDirectionalAlignmentNoise;
				float3 normal1080 = ase_normalWS;
				float3 ase_tangentWS = input.ase_texcoord5.xyz;
				float3 ase_bitangentWS = input.ase_texcoord6.xyz;
				float3x3 ase_worldToTangent = float3x3( ase_tangentWS, ase_bitangentWS, ase_normalWS );
				float3x3 worldToTangentMatrix1080 = ase_worldToTangent;
				float output1080 = 0.0;
				GetMainLightShadow_float( position1080 , screenPosition1080 , worldNoise1080 , lightDirectionNoise1080 , lightDirectionOrthogonalAlignmentNoise1080 , lightDirectionAlignmentNoise1080 , normal1080 , worldToTangentMatrix1080 , output1080 );
				float lerpResult984 = lerp( 1.0 , output1080 , _MainLightTransparencyShadow);
				float Main_Light_Transparency_Shadow981 = lerpResult984;
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1076 = Main_Light_Transparency_Shadow981;
				#else
				float staticSwitch1076 = 1.0;
				#endif
				float localGetAdditionalLightsShadow_float1125 = ( 0.0 );
				float3 position1125 = ase_positionWS;
				float2 screenPosition1125 = Screen_Position1254;
				float2 screenUV1125 = Screen_UV1145;
				float worldNoise1125 = _AdditionalLightsShadowWorldNoise;
				float lightDirectionNoise1125 = _AdditionalLightsShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1125 = _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1125 = _AdditionalLightsShadowDirectionalAlignmentNoise;
				float3 normal1125 = ase_normalWS;
				float3x3 worldToTangentMatrix1125 = ase_worldToTangent;
				float output1125 = 0.0;
				GetAdditionalLightsShadow_float( position1125 , screenPosition1125 , screenUV1125 , worldNoise1125 , lightDirectionNoise1125 , lightDirectionOrthogonalAlignmentNoise1125 , lightDirectionAlignmentNoise1125 , normal1125 , worldToTangentMatrix1125 , output1125 );
				float lerpResult993 = lerp( 1.0 , output1125 , _AdditionalLightsTransparencyShadow);
				float Additional_Lights_Transparency_Shadow994 = lerpResult993;
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1077 = Additional_Lights_Transparency_Shadow994;
				#else
				float staticSwitch1077 = 1.0;
				#endif
				float temp_output_1030_0 = saturate( ( ( saturate( ( Noise11 - staticSwitch766 ) ) * Fresnel_Mask233 * (input.ase_color).a * Depth_Fade188 * Camera_Depth_Fade195 * _Alpha ) + ( Intersection_Highlight1011 * Intersection_Highlight_Alpha1024 * pow( ( staticSwitch1076 * staticSwitch1077 ) , _IntersectionHighlightShadowMaskPower ) ) ) );
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch943 = temp_output_1030_0;
				#else
				float staticSwitch943 = ( temp_output_1030_0 * ( 1.0 - Radial_Mask227 ) );
				#endif
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch711 = staticSwitch943;
				#else
				float staticSwitch711 = ( staticSwitch943 * Vertical_Mask_1350 );
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch744 = staticSwitch711;
				#else
				float staticSwitch744 = staticSwitch943;
				#endif
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch763 = staticSwitch744;
				#else
				float staticSwitch763 = ( staticSwitch744 * Vertical_Mask_2759 );
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch768 = staticSwitch763;
				#else
				float staticSwitch768 = staticSwitch744;
				#endif
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch985 = ( staticSwitch768 * Main_Light_Transparency_Shadow981 );
				#else
				float staticSwitch985 = staticSwitch768;
				#endif
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch998 = ( staticSwitch985 * Additional_Lights_Transparency_Shadow994 );
				#else
				float staticSwitch998 = staticSwitch985;
				#endif
				float Alpha49 = staticSwitch998;
				

				float Alpha = Alpha49;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_FIXED_TESSELLATION
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _RECEIVE_SHADOWS_OFF
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"


			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Math.cginc"
			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Noise.cginc"
			#include "../../../Mirza/Solaris/Shaders/_Includes/Solaris.hlsl"

			
			
			

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#define ASE_NEEDS_VERT_TANGENT
			#pragma shader_feature_local _SWAPUVXY_ON
			#pragma shader_feature_local _VERTEXWAVEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEDILATIONENABLED_ON
			#pragma shader_feature_local _WORLDSPACEUVS_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _VERTICALMASK2_ON
			#pragma shader_feature_local _VERTICALMASK1_ON
			#pragma shader_feature_local _RADIALMASKSUBTRACTIVE_ON
			#pragma shader_feature_local _NOISEDILATIONENABLED_ON
			#pragma shader_feature_local _NOISEXZTWISTENABLED_ON
			#pragma shader_feature_local _SPHERIZENOISE_ON
			#pragma shader_feature_local _OBJECTSPACEUVS_ON
			#pragma shader_feature_local _NOISEDISTORTIONENABLED_ON
			#pragma shader_feature_local _NOISEDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _RADIALMASK_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONENABLED_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _VERTICALMASK1SUBTRACTIVE_ON
			#pragma shader_feature_local _VERTICALMASKSOBJECTSPACE_ON
			#pragma shader_feature_local _VERTICALMASK2SUBTRACTIVE_ON
			#pragma shader_feature_local _INVERTDEPTHFADE_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ColourA;
			float4 _RadialMaskDistortionAnimation;
			float4 _VerticalColourA;
			float4 _VerticalColourB;
			float4 _RadialMaskDistortionParticleAnimation;
			float4 _RadialMaskDistortionOffset;
			float4 _NoiseDistortionOffset;
			float4 _ColourB;
			float4 _NoiseDistortionAnimation;
			float4 _NoiseDistortionParticleAnimation;
			float4 _IntersectionHighlightColour;
			float4 _VertexNoiseAnimation;
			float4 _NoiseAnimation;
			float4 _VertexNoiseParticleAnimation;
			float4 _NoiseOffset;
			float4 _NoiseParticleAnimation;
			float4 _VertexNoiseOffset;
			float3 _VertexOffsetOverY1;
			float3 _RadialMaskDistortionTiling;
			float3 _VertexOffsetOverCircularY;
			float3 _VertexOffsetOverY2;
			float3 _VertexNoiseTiling;
			float3 _NoiseTiling;
			float3 _NoiseDistortionTiling;
			float2 _RadialMaskTiling;
			float2 _RadialMaskOffset;
			float2 _SpherizeNoiseOffset;
			float _VerticalColourMaskRemapMax;
			float _MainLightShadowOrthogonalAlignmentWorldNoise;
			float _MainLightShadowDirectionalAlignmentNoise;
			float _Noise;
			float _MainLightShadow;
			float _MainLightAnisotropyBlend;
			float _MainLightAnisotropy;
			float _AdditionalLightsFlatLambertBlend;
			float _ParticleSubtractNoiseoverLifetime;
			float _NoisePower;
			float _AdditionalLightsLambertTranslucencyMask;
			float _MainLight;
			float _MainLightShadowDirectionalNoise;
			float _ColourPower;
			float _MainLightLambertTranslucency;
			float _ColourHueShift;
			float _MainLightLambertTranslucencyMask;
			float _ColourSaturationShift;
			float _ColourValueMultiplier;
			float _VerticalColourHueShift;
			float _VerticalColourSaturationShift;
			float _MainLightFlatLambertBlend;
			float _VerticalColourValueMultiplier;
			float _VerticalColourMaskRemapMin;
			float _LambertLightingNormalfromHeight;
			float _VerticalColourMaskPower;
			float _MainLightShadowWorldNoise;
			float _StartFoldoutRadialMask;
			float _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
			float _AdditionalLightsShadowWorldNoise;
			float _VerticalMask1Power;
			float _VerticalMask2RemapMin;
			float _VerticalMask2RemapMax;
			float _VerticalMask2ObjectSpaceOffset;
			float _VerticalMask2ObjectSpaceScale;
			float _VerticalMask2Power;
			float _FresnelMaskRemapMin;
			float _FresnelMaskRemapMax;
			float _FresnelMaskPower;
			float _FresnelMask;
			float _DepthFade;
			float _DepthFadePower;
			float _SubtractiveDepthFade;
			float _SubtractiveDepthFadePower;
			float _CameraDepthFadeLength;
			float _CameraDepthFadeOffset;
			float _CameraDepthFadePower;
			float _Alpha;
			float _MainLightTransparencyShadow;
			float _VerticalMask1ObjectSpaceScale;
			float _VerticalMask1ObjectSpaceOffset;
			float _VerticalMask1RemapMin;
			float _VerticalMask1RemapMax;
			float _AdditionalLightsShadowDirectionalNoise;
			float _AdditionalLightsShadowDirectionalAlignmentNoise;
			float _AdditionalLightsShadow;
			float _AdditionalLights;
			float _IntersectionHighlightColourHueShift;
			float _IntersectionHighlightColourSaturationShift;
			float _IntersectionHighlightColourValueMultiplier;
			float _IntersectionHighlightRemapMin;
			float _IntersectionHighlightRemapMax;
			float _AdditionalLightsLambertTranslucency;
			float _IntersectionHighlight;
			float _RadialMaskRadius;
			float _RadialMaskRadiusOverParticleLifetime;
			float _RadialMaskFeather;
			float _RadialMaskDistortionScale;
			float _RadialMaskDistortionOctaves;
			float _RadialMaskDistortionDilation;
			float _RadialMaskDistortionPower;
			float _RadialMaskDistortion;
			float _RadialMaskPower;
			float _IntersectionHighlightPower;
			float _NoiseDilation;
			float _NoiseDistortionDilation;
			float _NoiseScale;
			float _StartFoldoutFresnelMask;
			float _EndFoldoutFresnelMask;
			float _StartFoldoutDepthFade;
			float _EndFoldoutDepthFade;
			float _StartFoldoutIntersectionHighlight;
			float _EndFoldoutIntersectionHighlight;
			float _StartFoldoutVertexUVOffset;
			float _EndFoldoutVertexUVOffset;
			float _StartFoldoutVertexNormalOffset;
			float _EndFoldoutVertexNormalOffset;
			float _StartFoldoutVertexWave;
			float _EndFoldoutVertexWave;
			float _StartFoldoutVertexNoise;
			float _EndFoldoutVertexNoise;
			float _StartFoldoutVertexWaveNoiseVerticalMask;
			float _EndFoldoutVertexWaveNoiseVerticalMask;
			float _StartFoldoutVertexOffsetoverY;
			float _EndFoldoutSpherizeNoise;
			float _EndFoldoutVertexOffsetoverY;
			float _StartFoldoutSpherizeNoise;
			float _StartFoldoutVerticalMasks;
			float _StartFoldoutLighting;
			float _EndFoldoutLighting;
			float _StartFoldoutBaseUVs;
			float _EndFoldoutBaseUVs;
			float _StartFoldoutParticleSettings;
			float _EndFoldoutParticleSettings;
			float _StartFoldoutColour;
			float _EndFoldoutColour;
			float _StartFoldoutVerticalColour;
			float _EndFoldoutVerticalColour;
			float _StartFoldoutNoise;
			float _EndFoldoutNoise;
			float _StartFoldoutNoiseDistortion;
			float _EndFoldoutNoiseDistortion;
			float _EndFoldoutRadialMask;
			float _StartFoldoutRadialMaskDistortion;
			float _EndFoldoutRadialMaskDistortion;
			float _EndFoldoutVerticalMasks;
			float _Tessellation;
			float _VertexNormalOffset;
			float _VertexNormalOffsetTopPower;
			float _VertexOffsetOverY2Power;
			float _VertexOffsetOverCircularYPower;
			float _NoiseRemapMin;
			float _NoiseRemapMax;
			float _UVSampleNoise;
			float _SpherizeNoiseRadius;
			float _SpherizeNoiseStrength;
			float _NoiseXZTwist;
			float _NoiseUVYPreOffset;
			float _NoiseUVYPreScale;
			float _NoiseUVYPrePower;
			float _NoiseParallaxOffset;
			float _NoiseDistortionScale;
			float _NoiseDistortionOctaves;
			float _AdditionalLightsTransparencyShadow;
			float _NoiseDistortionPower;
			float _NoiseDistortion;
			float _VertexOffsetOverY1Power;
			float _VertexTwist;
			float _VertexUVOffsetBottom;
			float _VertexUVOffsetBottomPower;
			float _VertexNormalOffsetTop;
			float _VertexNormalOffsetBottomPower;
			float _VertexNormalOffsetBottom;
			float _VertexWaveScale;
			float _VertexWaveAnimation;
			float _VertexWaveOffset;
			float _VertexWave;
			float _VertexWaveNoiseVerticalMaskRemapMin;
			float _NoiseOctaves;
			float _VertexWaveNoiseVerticalMaskRemapMax;
			float _VertexNoiseScale;
			float _ParticleRandomization;
			float _VertexNoiseOctaves;
			float _VertexNoiseDilation;
			float _VertexNoiseTwist;
			float _VertexNoise;
			float _VertexUVOffsetTopPower;
			float _VertexUVOffsetTop;
			float _VertexWaveNoiseVerticalMaskPower;
			float _IntersectionHighlightShadowMaskPower;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float localTwistXZ_float11_g587 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float UV_2D_Y691 = (staticSwitch321).y;
				float3 Vertex_Normal_Offset670 = ( ( input.normalOS * _VertexNormalOffset ) + ( pow( UV_2D_Y691 , _VertexNormalOffsetTopPower ) * input.normalOS * _VertexNormalOffsetTop ) + ( pow( ( 1.0 - UV_2D_Y691 ) , _VertexNormalOffsetBottomPower ) * input.normalOS * _VertexNormalOffsetBottom ) );
				float2 UV_2D327 = staticSwitch321;
				float mulTime258 = _TimeParameters.x * _VertexWaveAnimation;
				float temp_output_7_0_g582 = _VertexWaveNoiseVerticalMaskRemapMin;
				float temp_output_23_0_g582 = _VertexWaveNoiseVerticalMaskRemapMax;
				float temp_output_20_0_g582 = UV_2D_Y691;
				float temp_output_4_0_g582 = _VertexWaveNoiseVerticalMaskPower;
				float smoothstepResult22_g582 = smoothstep( temp_output_7_0_g582 , temp_output_23_0_g582 , pow( temp_output_20_0_g582 , temp_output_4_0_g582 ));
				float Vertex_WaveNoise_Vertical_Mask280 = smoothstepResult22_g582;
				#ifdef _VERTEXWAVEENABLED_ON
				float3 staticSwitch448 = ( ( sin( ( ( UV_2D327.y * TWO_PI * _VertexWaveScale ) - ( mulTime258 + ( _VertexWaveOffset * TWO_PI ) ) ) ) * _VertexWave * Vertex_WaveNoise_Vertical_Mask280 ) * input.normalOS );
				#else
				float3 staticSwitch448 = float3( 0,0,0 );
				#endif
				float3 Vertex_Sine265 = staticSwitch448;
				float localTwistXZ_float11_g585 = ( 0.0 );
				float localSimplexNoise_float2_g584 = ( 0.0 );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.positionOS.xyz).yxz;
				#else
				float3 staticSwitch339 = input.positionOS.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch605 = UV_3D_World622;
				#else
				float3 staticSwitch605 = UV_3D340;
				#endif
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Vertex_Noise_Offset311 = ( _VertexNoiseOffset + Particle_Stable_Random_X147 + ( _VertexNoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g579 = ( float4( ( staticSwitch605 * _VertexNoiseScale * _VertexNoiseTiling ) , 0.0 ) - ( Vertex_Noise_Offset311 + ( _VertexNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_292_0 = (temp_output_10_0_g579).xyz;
				float3 position2_g584 = temp_output_292_0;
				float temp_output_292_15 = (temp_output_10_0_g579).w;
				float angle2_g584 = temp_output_292_15;
				float octaves2_g584 = _VertexNoiseOctaves;
				float noise2_g584 = 0.0;
				float3 gradient2_g584 = float3( 0,0,0 );
				SimplexNoise_float( position2_g584 , angle2_g584 , octaves2_g584 , noise2_g584 , gradient2_g584 );
				float localSimplexNoise_Caustics_float2_g583 = ( 0.0 );
				float3 position2_g583 = temp_output_292_0;
				float angle2_g583 = temp_output_292_15;
				float octaves2_g583 = _VertexNoiseOctaves;
				float gradientStrength2_g583 = _VertexNoiseDilation;
				float noise2_g583 = 0.0;
				float3 gradient2_g583 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g583 , angle2_g583 , octaves2_g583 , gradientStrength2_g583 , noise2_g583 , gradient2_g583 );
				#ifdef _VERTEXNOISEDILATIONENABLED_ON
				float3 staticSwitch490 = gradient2_g583;
				#else
				float3 staticSwitch490 = gradient2_g584;
				#endif
				float3 temp_output_10_0_g585 = staticSwitch490;
				float3 position11_g585 = temp_output_10_0_g585;
				float temp_output_9_0_g585 = _VertexNoiseTwist;
				float angle11_g585 = radians( temp_output_9_0_g585 );
				float3 output11_g585 = float3( 0,0,0 );
				TwistXZ_float( position11_g585 , angle11_g585 , output11_g585 );
				float3 temp_output_852_0 = output11_g585;
				#ifdef _VERTEXNOISEENABLED_ON
				float3 staticSwitch450 = ( temp_output_852_0 * _VertexNoise * Vertex_WaveNoise_Vertical_Mask280 );
				#else
				float3 staticSwitch450 = float3( 0,0,0 );
				#endif
				float3 Vertex_Noise298 = staticSwitch450;
				float2 break587 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_576_0 = ( ( break587.x * pow( break587.y , _VertexUVOffsetTopPower ) ) * ( _VertexUVOffsetTop * 0.5 ) );
				float3 appendResult575 = (float3(temp_output_576_0 , 0.0 , 0.0));
				float3 appendResult594 = (float3(0.0 , temp_output_576_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch593 = appendResult594;
				#else
				float3 staticSwitch593 = appendResult575;
				#endif
				float3 Vertex_Offset_Top579 = staticSwitch593;
				float2 break644 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_650_0 = ( ( break644.x * pow( ( 1.0 - break644.y ) , _VertexUVOffsetBottomPower ) ) * ( _VertexUVOffsetBottom * 0.5 ) );
				float3 appendResult653 = (float3(temp_output_650_0 , 0.0 , 0.0));
				float3 appendResult652 = (float3(0.0 , temp_output_650_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch654 = appendResult652;
				#else
				float3 staticSwitch654 = appendResult653;
				#endif
				float3 Vertex_Offset_Bottom655 = staticSwitch654;
				float3 temp_output_10_0_g587 = ( ( Vertex_Normal_Offset670 + Vertex_Sine265 + Vertex_Noise298 + Vertex_Offset_Top579 + Vertex_Offset_Bottom655 ) + input.positionOS.xyz );
				float3 position11_g587 = temp_output_10_0_g587;
				float temp_output_9_0_g587 = -_VertexTwist;
				float angle11_g587 = radians( temp_output_9_0_g587 );
				float3 output11_g587 = float3( 0,0,0 );
				TwistXZ_float( position11_g587 , angle11_g587 , output11_g587 );
				float3 worldToObjDir636 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY1, 0.0 ) ).xyz;
				float3 worldToObjDir780 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY2, 0.0 ) ).xyz;
				float UV_2D_Circular_Y897 = sin( ( UV_2D_Y691 * PI ) );
				float3 Vertex_Offset_over_Y639 = ( ( worldToObjDir636 * pow( UV_2D_Y691 , _VertexOffsetOverY1Power ) ) + ( worldToObjDir780 * pow( UV_2D_Y691 , _VertexOffsetOverY2Power ) ) + ( _VertexOffsetOverCircularY * pow( UV_2D_Circular_Y897 , _VertexOffsetOverCircularYPower ) ) );
				float3 Vertex_Offset247 = ( output11_g587 + Vertex_Offset_over_Y639 );
				
				output.ase_texcoord2.xyz = ase_positionWS;
				float4 ase_positionCS = TransformObjectToHClip( ( input.positionOS ).xyz );
				float4 screenPos = ComputeScreenPos( ase_positionCS );
				output.ase_texcoord3 = screenPos;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord5.xyz = ase_normalWS;
				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord2.w = eyeDepth;
				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord6.xyz = ase_tangentWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord7.xyz = ase_bitangentWS;
				
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.positionOS;
				output.ase_texcoord4 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord5.w = 0;
				output.ase_texcoord6.w = 0;
				output.ase_texcoord7.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = Vertex_Offset247;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _Tessellation; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float temp_output_7_0_g574 = _NoiseRemapMin;
				float temp_output_23_0_g574 = _NoiseRemapMax;
				float localSimplexNoise_float2_g507 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float2 UV_2D327 = staticSwitch321;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.ase_texcoord1.xyz).yxz;
				#else
				float3 staticSwitch339 = input.ase_texcoord1.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				#ifdef _OBJECTSPACEUVS_ON
				float3 staticSwitch223 = UV_3D340;
				#else
				float3 staticSwitch223 = float3( UV_2D327 ,  0.0 );
				#endif
				float3 ase_positionWS = input.ase_texcoord2.xyz;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch356 = UV_3D_World622;
				#else
				float3 staticSwitch356 = staticSwitch223;
				#endif
				float4 screenPos = input.ase_texcoord3;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 appendResult1146 = (float2(ase_positionSSNorm.x , ase_positionSSNorm.y));
				float2 Screen_UV1145 = appendResult1146;
				float2 appendResult1112 = (float2(_ScaledScreenParams.x , _ScaledScreenParams.y));
				float2 Screen_Resolution1113 = appendResult1112;
				float2 Screen_Position1254 = ( Screen_UV1145 * Screen_Resolution1113 );
				float2 screenPosition1243 = Screen_Position1254;
				float mulTime1247 = _TimeParameters.x * 60.0;
				float time1243 = mulTime1247;
				float3 localHash331243 = Hash33( screenPosition1243 , time1243 );
				float3 Sample_Noise1248 = ( (localHash331243*2.0 + -1.0) * _UVSampleNoise );
				float3 Noise_Base_UV220 = ( staticSwitch356 + Sample_Noise1248 );
				float localSpherize_float5_g491 = ( 0.0 );
				float2 uv5_g491 = (Noise_Base_UV220).xy;
				float2 center5_g491 = ( _SpherizeNoiseOffset + float2( 0.5,0.5 ) );
				float radius5_g491 = _SpherizeNoiseRadius;
				float strength5_g491 = _SpherizeNoiseStrength;
				float2 output5_g491 = float2( 0,0 );
				Spherize_float( uv5_g491 , center5_g491 , radius5_g491 , strength5_g491 , output5_g491 );
				float3 appendResult611 = (float3(output5_g491 , (Noise_Base_UV220).z));
				#ifdef _SPHERIZENOISE_ON
				float3 staticSwitch625 = appendResult611;
				#else
				float3 staticSwitch625 = Noise_Base_UV220;
				#endif
				float localTwistXZ_float11_g495 = ( 0.0 );
				float3 temp_output_10_0_g495 = staticSwitch625;
				float3 position11_g495 = temp_output_10_0_g495;
				float temp_output_9_0_g495 = _NoiseXZTwist;
				float angle11_g495 = radians( temp_output_9_0_g495 );
				float3 output11_g495 = float3( 0,0,0 );
				TwistXZ_float( position11_g495 , angle11_g495 , output11_g495 );
				#ifdef _NOISEXZTWISTENABLED_ON
				float3 staticSwitch801 = output11_g495;
				#else
				float3 staticSwitch801 = staticSwitch625;
				#endif
				float3 break728 = staticSwitch801;
				float temp_output_738_0 = ( ( break728.y - _NoiseUVYPreOffset ) * _NoiseUVYPreScale );
				float temp_output_7_0_g499 = abs( temp_output_738_0 );
				float temp_output_733_14 = ( pow( temp_output_7_0_g499 , _NoiseUVYPrePower ) * sign( temp_output_738_0 ) );
				float3 appendResult732 = (float3(break728.x , temp_output_733_14 , break728.z));
				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - ase_positionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 temp_output_872_0 = ( -ase_viewDirWS * _NoiseParallaxOffset );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch464 = (temp_output_872_0).yxz;
				#else
				float3 staticSwitch464 = temp_output_872_0;
				#endif
				float3 Parallax_Offset465 = staticSwitch464;
				float localSimplexNoise_float2_g493 = ( 0.0 );
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Distortion_Noise_Offset163 = ( _NoiseDistortionOffset + Particle_Stable_Random_X147 + ( _NoiseDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g489 = ( float4( ( ( Noise_Base_UV220 + Parallax_Offset465 ) * _NoiseDistortionScale * _NoiseDistortionTiling ) , 0.0 ) - ( Distortion_Noise_Offset163 + ( _NoiseDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_160_0 = (temp_output_10_0_g489).xyz;
				float3 position2_g493 = temp_output_160_0;
				float temp_output_160_15 = (temp_output_10_0_g489).w;
				float angle2_g493 = temp_output_160_15;
				float octaves2_g493 = _NoiseDistortionOctaves;
				float noise2_g493 = 0.0;
				float3 gradient2_g493 = float3( 0,0,0 );
				SimplexNoise_float( position2_g493 , angle2_g493 , octaves2_g493 , noise2_g493 , gradient2_g493 );
				float localSimplexNoise_Caustics_float2_g492 = ( 0.0 );
				float3 position2_g492 = temp_output_160_0;
				float angle2_g492 = temp_output_160_15;
				float octaves2_g492 = _NoiseDistortionOctaves;
				float gradientStrength2_g492 = _NoiseDistortionDilation;
				float noise2_g492 = 0.0;
				float3 gradient2_g492 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g492 , angle2_g492 , octaves2_g492 , gradientStrength2_g492 , noise2_g492 , gradient2_g492 );
				#ifdef _NOISEDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch440 = gradient2_g492;
				#else
				float3 staticSwitch440 = gradient2_g493;
				#endif
				float3 temp_output_7_0_g497 = abs( staticSwitch440 );
				float3 temp_cast_2 = (_NoiseDistortionPower).xxx;
				#ifdef _NOISEDISTORTIONENABLED_ON
				float3 staticSwitch442 = ( ( pow( temp_output_7_0_g497 , temp_cast_2 ) * sign( staticSwitch440 ) ) * _NoiseDistortion );
				#else
				float3 staticSwitch442 = float3( 0,0,0 );
				#endif
				float3 Noise_Distortion73 = staticSwitch442;
				float3 Noise_UV173 = ( appendResult732 + Parallax_Offset465 + Noise_Distortion73 );
				float4 Noise_Offset175 = ( _NoiseOffset + Particle_Stable_Random_X147 + ( _NoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g503 = ( float4( ( Noise_UV173 * _NoiseScale * _NoiseTiling ) , 0.0 ) - ( Noise_Offset175 + ( _NoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_159_0 = (temp_output_10_0_g503).xyz;
				float3 position2_g507 = temp_output_159_0;
				float temp_output_159_15 = (temp_output_10_0_g503).w;
				float angle2_g507 = temp_output_159_15;
				float octaves2_g507 = _NoiseOctaves;
				float noise2_g507 = 0.0;
				float3 gradient2_g507 = float3( 0,0,0 );
				SimplexNoise_float( position2_g507 , angle2_g507 , octaves2_g507 , noise2_g507 , gradient2_g507 );
				float localSimplexNoise_Caustics_float2_g506 = ( 0.0 );
				float3 position2_g506 = temp_output_159_0;
				float angle2_g506 = temp_output_159_15;
				float octaves2_g506 = _NoiseOctaves;
				float gradientStrength2_g506 = _NoiseDilation;
				float noise2_g506 = 0.0;
				float3 gradient2_g506 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g506 , angle2_g506 , octaves2_g506 , gradientStrength2_g506 , noise2_g506 , gradient2_g506 );
				#ifdef _NOISEDILATIONENABLED_ON
				float staticSwitch444 = noise2_g506;
				#else
				float staticSwitch444 = noise2_g507;
				#endif
				float temp_output_20_0_g574 = staticSwitch444;
				float temp_output_4_0_g574 = _NoisePower;
				float smoothstepResult22_g574 = smoothstep( temp_output_7_0_g574 , temp_output_23_0_g574 , pow( temp_output_20_0_g574 , temp_output_4_0_g574 ));
				float Particle_Subtract_Noise_over_Lifetime480 = ( input.ase_texcoord4.y * _ParticleSubtractNoiseoverLifetime );
				float temp_output_478_0 = ( smoothstepResult22_g574 - Particle_Subtract_Noise_over_Lifetime480 );
				float lerpResult569 = lerp( 1.0 , temp_output_478_0 , _Noise);
				float Noise11 = lerpResult569;
				float Particle_Mask_Radius_over_Lifetime161 = input.ase_texcoord4.x;
				float lerpResult183 = lerp( 1.0 , Particle_Mask_Radius_over_Lifetime161 , _RadialMaskRadiusOverParticleLifetime);
				float temp_output_6_0_g500 = ( 1.0 - ( _RadialMaskRadius * lerpResult183 ) );
				float lerpResult5_g500 = lerp( temp_output_6_0_g500 , 1.0 , _RadialMaskFeather);
				float2 texCoord330 = input.ase_texcoord.xy * float2( 2,2 ) + float2( -1,-1 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch329 = (texCoord330).yx;
				#else
				float2 staticSwitch329 = texCoord330;
				#endif
				float2 UV_2D_Centered328 = staticSwitch329;
				float localSimplexNoise_float2_g486 = ( 0.0 );
				float4 Mask_Distortion_Noise_Offset171 = ( _RadialMaskDistortionOffset + Particle_Stable_Random_X147 + ( _RadialMaskDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g483 = ( float4( ( Noise_Base_UV220 * _RadialMaskDistortionScale * _RadialMaskDistortionTiling ) , 0.0 ) - ( Mask_Distortion_Noise_Offset171 + ( _RadialMaskDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_158_0 = (temp_output_10_0_g483).xyz;
				float3 position2_g486 = temp_output_158_0;
				float temp_output_158_15 = (temp_output_10_0_g483).w;
				float angle2_g486 = temp_output_158_15;
				float octaves2_g486 = _RadialMaskDistortionOctaves;
				float noise2_g486 = 0.0;
				float3 gradient2_g486 = float3( 0,0,0 );
				SimplexNoise_float( position2_g486 , angle2_g486 , octaves2_g486 , noise2_g486 , gradient2_g486 );
				float localSimplexNoise_Caustics_float2_g485 = ( 0.0 );
				float3 position2_g485 = temp_output_158_0;
				float angle2_g485 = temp_output_158_15;
				float octaves2_g485 = _RadialMaskDistortionOctaves;
				float gradientStrength2_g485 = _RadialMaskDistortionDilation;
				float noise2_g485 = 0.0;
				float3 gradient2_g485 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g485 , angle2_g485 , octaves2_g485 , gradientStrength2_g485 , noise2_g485 , gradient2_g485 );
				#ifdef _RADIALMASKDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch441 = gradient2_g485;
				#else
				float3 staticSwitch441 = gradient2_g486;
				#endif
				float3 temp_output_7_0_g494 = abs( staticSwitch441 );
				float3 temp_cast_5 = (_RadialMaskDistortionPower).xxx;
				#ifdef _RADIALMASKDISTORTIONENABLED_ON
				float3 staticSwitch451 = ( ( pow( temp_output_7_0_g494 , temp_cast_5 ) * sign( staticSwitch441 ) ) * _RadialMaskDistortion );
				#else
				float3 staticSwitch451 = float3( 0,0,0 );
				#endif
				float3 Mask_Distortion117 = staticSwitch451;
				float temp_output_7_0_g500 = ( 1.0 - length( ( ( ( UV_2D_Centered328 + (Mask_Distortion117).xy ) - _RadialMaskOffset ) * _RadialMaskTiling ) ) );
				float smoothstepResult4_g500 = smoothstep( temp_output_6_0_g500 , lerpResult5_g500 , temp_output_7_0_g500);
				#ifdef _RADIALMASK_ON
				float staticSwitch601 = ( 1.0 - pow( smoothstepResult4_g500 , _RadialMaskPower ) );
				#else
				float staticSwitch601 = 0.0;
				#endif
				float Radial_Mask227 = staticSwitch601;
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch942 = Radial_Mask227;
				#else
				float staticSwitch942 = 0.0;
				#endif
				float temp_output_7_0_g505 = _VerticalMask1RemapMax;
				float temp_output_23_0_g505 = _VerticalMask1RemapMin;
				float UV_2D_Y691 = (staticSwitch321).y;
				float UV_3D_Y719 = (staticSwitch339).y;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch716 = ( ( UV_3D_Y719 - _VerticalMask1ObjectSpaceOffset ) / _VerticalMask1ObjectSpaceScale );
				#else
				float staticSwitch716 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g505 = staticSwitch716;
				float smoothstepResult25_g505 = smoothstep( temp_output_7_0_g505 , temp_output_23_0_g505 , temp_output_20_0_g505);
				float temp_output_4_0_g505 = _VerticalMask1Power;
				float temp_output_862_0 = pow( smoothstepResult25_g505 , temp_output_4_0_g505 );
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch713 = ( 1.0 - temp_output_862_0 );
				#else
				float staticSwitch713 = temp_output_862_0;
				#endif
				float Vertical_Mask_1350 = staticSwitch713;
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch709 = ( staticSwitch942 + Vertical_Mask_1350 );
				#else
				float staticSwitch709 = staticSwitch942;
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch741 = staticSwitch709;
				#else
				float staticSwitch741 = staticSwitch942;
				#endif
				float temp_output_7_0_g508 = _VerticalMask2RemapMin;
				float temp_output_23_0_g508 = _VerticalMask2RemapMax;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch767 = ( ( UV_3D_Y719 - _VerticalMask2ObjectSpaceOffset ) / _VerticalMask2ObjectSpaceScale );
				#else
				float staticSwitch767 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g508 = staticSwitch767;
				float smoothstepResult25_g508 = smoothstep( temp_output_7_0_g508 , temp_output_23_0_g508 , temp_output_20_0_g508);
				float temp_output_4_0_g508 = _VerticalMask2Power;
				float temp_output_863_0 = pow( smoothstepResult25_g508 , temp_output_4_0_g508 );
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch758 = ( 1.0 - temp_output_863_0 );
				#else
				float staticSwitch758 = temp_output_863_0;
				#endif
				float Vertical_Mask_2759 = staticSwitch758;
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch760 = ( staticSwitch741 + Vertical_Mask_2759 );
				#else
				float staticSwitch760 = staticSwitch741;
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch766 = staticSwitch760;
				#else
				float staticSwitch766 = staticSwitch741;
				#endif
				float3 ase_normalWS = input.ase_texcoord5.xyz;
				float fresnelNdotV232 = dot( ase_normalWS, ase_viewDirWS );
				float fresnelNode232 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV232, _FresnelMaskPower ) );
				float smoothstepResult234 = smoothstep( _FresnelMaskRemapMin , _FresnelMaskRemapMax , fresnelNode232);
				float lerpResult242 = lerp( 1.0 , smoothstepResult234 , _FresnelMask);
				float Fresnel_Mask233 = lerpResult242;
				float temp_output_7_0_g576 = 0.0;
				float temp_output_23_0_g576 = 1.0;
				float screenDepth186 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth186 = saturate( abs( ( screenDepth186 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				#ifdef _INVERTDEPTHFADE_ON
				float staticSwitch218 = ( 1.0 - distanceDepth186 );
				#else
				float staticSwitch218 = distanceDepth186;
				#endif
				float temp_output_20_0_g576 = staticSwitch218;
				float temp_output_4_0_g576 = _DepthFadePower;
				float smoothstepResult22_g576 = smoothstep( temp_output_7_0_g576 , temp_output_23_0_g576 , pow( temp_output_20_0_g576 , temp_output_4_0_g576 ));
				float temp_output_7_0_g575 = 0.0;
				float temp_output_23_0_g575 = 1.0;
				float screenDepth208 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth208 = saturate( abs( ( screenDepth208 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _SubtractiveDepthFade ) ) );
				float temp_output_20_0_g575 = ( 1.0 - distanceDepth208 );
				float temp_output_4_0_g575 = _SubtractiveDepthFadePower;
				float smoothstepResult22_g575 = smoothstep( temp_output_7_0_g575 , temp_output_23_0_g575 , pow( temp_output_20_0_g575 , temp_output_4_0_g575 ));
				float Depth_Fade188 = saturate( ( smoothstepResult22_g576 - smoothstepResult22_g575 ) );
				float temp_output_7_0_g577 = 0.0;
				float temp_output_23_0_g577 = 1.0;
				float eyeDepth = input.ase_texcoord2.w;
				float cameraDepthFade191 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g577 = saturate( cameraDepthFade191 );
				float temp_output_4_0_g577 = _CameraDepthFadePower;
				float smoothstepResult22_g577 = smoothstep( temp_output_7_0_g577 , temp_output_23_0_g577 , pow( temp_output_20_0_g577 , temp_output_4_0_g577 ));
				float Camera_Depth_Fade195 = smoothstepResult22_g577;
				float temp_output_7_0_g581 = _IntersectionHighlightRemapMin;
				float temp_output_23_0_g581 = _IntersectionHighlightRemapMax;
				float screenDepth1005 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth1005 = saturate( abs( ( screenDepth1005 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _IntersectionHighlight ) ) );
				float temp_output_20_0_g581 = ( 1.0 - distanceDepth1005 );
				float temp_output_4_0_g581 = _IntersectionHighlightPower;
				float smoothstepResult22_g581 = smoothstep( temp_output_7_0_g581 , temp_output_23_0_g581 , pow( temp_output_20_0_g581 , temp_output_4_0_g581 ));
				float Intersection_Highlight1011 = smoothstepResult22_g581;
				float Intersection_Highlight_Alpha1024 = _IntersectionHighlightColour.a;
				float localGetMainLightShadow_float1080 = ( 0.0 );
				float3 position1080 = ase_positionWS;
				float2 screenPosition1080 = Screen_Position1254;
				float worldNoise1080 = _MainLightShadowWorldNoise;
				float lightDirectionNoise1080 = _MainLightShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1080 = _MainLightShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1080 = _MainLightShadowDirectionalAlignmentNoise;
				float3 normal1080 = ase_normalWS;
				float3 ase_tangentWS = input.ase_texcoord6.xyz;
				float3 ase_bitangentWS = input.ase_texcoord7.xyz;
				float3x3 ase_worldToTangent = float3x3( ase_tangentWS, ase_bitangentWS, ase_normalWS );
				float3x3 worldToTangentMatrix1080 = ase_worldToTangent;
				float output1080 = 0.0;
				GetMainLightShadow_float( position1080 , screenPosition1080 , worldNoise1080 , lightDirectionNoise1080 , lightDirectionOrthogonalAlignmentNoise1080 , lightDirectionAlignmentNoise1080 , normal1080 , worldToTangentMatrix1080 , output1080 );
				float lerpResult984 = lerp( 1.0 , output1080 , _MainLightTransparencyShadow);
				float Main_Light_Transparency_Shadow981 = lerpResult984;
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1076 = Main_Light_Transparency_Shadow981;
				#else
				float staticSwitch1076 = 1.0;
				#endif
				float localGetAdditionalLightsShadow_float1125 = ( 0.0 );
				float3 position1125 = ase_positionWS;
				float2 screenPosition1125 = Screen_Position1254;
				float2 screenUV1125 = Screen_UV1145;
				float worldNoise1125 = _AdditionalLightsShadowWorldNoise;
				float lightDirectionNoise1125 = _AdditionalLightsShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1125 = _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1125 = _AdditionalLightsShadowDirectionalAlignmentNoise;
				float3 normal1125 = ase_normalWS;
				float3x3 worldToTangentMatrix1125 = ase_worldToTangent;
				float output1125 = 0.0;
				GetAdditionalLightsShadow_float( position1125 , screenPosition1125 , screenUV1125 , worldNoise1125 , lightDirectionNoise1125 , lightDirectionOrthogonalAlignmentNoise1125 , lightDirectionAlignmentNoise1125 , normal1125 , worldToTangentMatrix1125 , output1125 );
				float lerpResult993 = lerp( 1.0 , output1125 , _AdditionalLightsTransparencyShadow);
				float Additional_Lights_Transparency_Shadow994 = lerpResult993;
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1077 = Additional_Lights_Transparency_Shadow994;
				#else
				float staticSwitch1077 = 1.0;
				#endif
				float temp_output_1030_0 = saturate( ( ( saturate( ( Noise11 - staticSwitch766 ) ) * Fresnel_Mask233 * (input.ase_color).a * Depth_Fade188 * Camera_Depth_Fade195 * _Alpha ) + ( Intersection_Highlight1011 * Intersection_Highlight_Alpha1024 * pow( ( staticSwitch1076 * staticSwitch1077 ) , _IntersectionHighlightShadowMaskPower ) ) ) );
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch943 = temp_output_1030_0;
				#else
				float staticSwitch943 = ( temp_output_1030_0 * ( 1.0 - Radial_Mask227 ) );
				#endif
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch711 = staticSwitch943;
				#else
				float staticSwitch711 = ( staticSwitch943 * Vertical_Mask_1350 );
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch744 = staticSwitch711;
				#else
				float staticSwitch744 = staticSwitch943;
				#endif
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch763 = staticSwitch744;
				#else
				float staticSwitch763 = ( staticSwitch744 * Vertical_Mask_2759 );
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch768 = staticSwitch763;
				#else
				float staticSwitch768 = staticSwitch744;
				#endif
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch985 = ( staticSwitch768 * Main_Light_Transparency_Shadow981 );
				#else
				float staticSwitch985 = staticSwitch768;
				#endif
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch998 = ( staticSwitch985 * Additional_Lights_Transparency_Shadow994 );
				#else
				float staticSwitch998 = staticSwitch985;
				#endif
				float Alpha49 = staticSwitch998;
				

				surfaceDescription.Alpha = Alpha49;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define ASE_FIXED_TESSELLATION
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _RECEIVE_SHADOWS_OFF
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif


			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Math.cginc"
			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Noise.cginc"
			#include "../../../Mirza/Solaris/Shaders/_Includes/Solaris.hlsl"

			
			
			

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#define ASE_NEEDS_VERT_TANGENT
			#pragma shader_feature_local _SWAPUVXY_ON
			#pragma shader_feature_local _VERTEXWAVEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEDILATIONENABLED_ON
			#pragma shader_feature_local _WORLDSPACEUVS_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _VERTICALMASK2_ON
			#pragma shader_feature_local _VERTICALMASK1_ON
			#pragma shader_feature_local _RADIALMASKSUBTRACTIVE_ON
			#pragma shader_feature_local _NOISEDILATIONENABLED_ON
			#pragma shader_feature_local _NOISEXZTWISTENABLED_ON
			#pragma shader_feature_local _SPHERIZENOISE_ON
			#pragma shader_feature_local _OBJECTSPACEUVS_ON
			#pragma shader_feature_local _NOISEDISTORTIONENABLED_ON
			#pragma shader_feature_local _NOISEDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _RADIALMASK_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONENABLED_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _VERTICALMASK1SUBTRACTIVE_ON
			#pragma shader_feature_local _VERTICALMASKSOBJECTSPACE_ON
			#pragma shader_feature_local _VERTICALMASK2SUBTRACTIVE_ON
			#pragma shader_feature_local _INVERTDEPTHFADE_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ColourA;
			float4 _RadialMaskDistortionAnimation;
			float4 _VerticalColourA;
			float4 _VerticalColourB;
			float4 _RadialMaskDistortionParticleAnimation;
			float4 _RadialMaskDistortionOffset;
			float4 _NoiseDistortionOffset;
			float4 _ColourB;
			float4 _NoiseDistortionAnimation;
			float4 _NoiseDistortionParticleAnimation;
			float4 _IntersectionHighlightColour;
			float4 _VertexNoiseAnimation;
			float4 _NoiseAnimation;
			float4 _VertexNoiseParticleAnimation;
			float4 _NoiseOffset;
			float4 _NoiseParticleAnimation;
			float4 _VertexNoiseOffset;
			float3 _VertexOffsetOverY1;
			float3 _RadialMaskDistortionTiling;
			float3 _VertexOffsetOverCircularY;
			float3 _VertexOffsetOverY2;
			float3 _VertexNoiseTiling;
			float3 _NoiseTiling;
			float3 _NoiseDistortionTiling;
			float2 _RadialMaskTiling;
			float2 _RadialMaskOffset;
			float2 _SpherizeNoiseOffset;
			float _VerticalColourMaskRemapMax;
			float _MainLightShadowOrthogonalAlignmentWorldNoise;
			float _MainLightShadowDirectionalAlignmentNoise;
			float _Noise;
			float _MainLightShadow;
			float _MainLightAnisotropyBlend;
			float _MainLightAnisotropy;
			float _AdditionalLightsFlatLambertBlend;
			float _ParticleSubtractNoiseoverLifetime;
			float _NoisePower;
			float _AdditionalLightsLambertTranslucencyMask;
			float _MainLight;
			float _MainLightShadowDirectionalNoise;
			float _ColourPower;
			float _MainLightLambertTranslucency;
			float _ColourHueShift;
			float _MainLightLambertTranslucencyMask;
			float _ColourSaturationShift;
			float _ColourValueMultiplier;
			float _VerticalColourHueShift;
			float _VerticalColourSaturationShift;
			float _MainLightFlatLambertBlend;
			float _VerticalColourValueMultiplier;
			float _VerticalColourMaskRemapMin;
			float _LambertLightingNormalfromHeight;
			float _VerticalColourMaskPower;
			float _MainLightShadowWorldNoise;
			float _StartFoldoutRadialMask;
			float _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
			float _AdditionalLightsShadowWorldNoise;
			float _VerticalMask1Power;
			float _VerticalMask2RemapMin;
			float _VerticalMask2RemapMax;
			float _VerticalMask2ObjectSpaceOffset;
			float _VerticalMask2ObjectSpaceScale;
			float _VerticalMask2Power;
			float _FresnelMaskRemapMin;
			float _FresnelMaskRemapMax;
			float _FresnelMaskPower;
			float _FresnelMask;
			float _DepthFade;
			float _DepthFadePower;
			float _SubtractiveDepthFade;
			float _SubtractiveDepthFadePower;
			float _CameraDepthFadeLength;
			float _CameraDepthFadeOffset;
			float _CameraDepthFadePower;
			float _Alpha;
			float _MainLightTransparencyShadow;
			float _VerticalMask1ObjectSpaceScale;
			float _VerticalMask1ObjectSpaceOffset;
			float _VerticalMask1RemapMin;
			float _VerticalMask1RemapMax;
			float _AdditionalLightsShadowDirectionalNoise;
			float _AdditionalLightsShadowDirectionalAlignmentNoise;
			float _AdditionalLightsShadow;
			float _AdditionalLights;
			float _IntersectionHighlightColourHueShift;
			float _IntersectionHighlightColourSaturationShift;
			float _IntersectionHighlightColourValueMultiplier;
			float _IntersectionHighlightRemapMin;
			float _IntersectionHighlightRemapMax;
			float _AdditionalLightsLambertTranslucency;
			float _IntersectionHighlight;
			float _RadialMaskRadius;
			float _RadialMaskRadiusOverParticleLifetime;
			float _RadialMaskFeather;
			float _RadialMaskDistortionScale;
			float _RadialMaskDistortionOctaves;
			float _RadialMaskDistortionDilation;
			float _RadialMaskDistortionPower;
			float _RadialMaskDistortion;
			float _RadialMaskPower;
			float _IntersectionHighlightPower;
			float _NoiseDilation;
			float _NoiseDistortionDilation;
			float _NoiseScale;
			float _StartFoldoutFresnelMask;
			float _EndFoldoutFresnelMask;
			float _StartFoldoutDepthFade;
			float _EndFoldoutDepthFade;
			float _StartFoldoutIntersectionHighlight;
			float _EndFoldoutIntersectionHighlight;
			float _StartFoldoutVertexUVOffset;
			float _EndFoldoutVertexUVOffset;
			float _StartFoldoutVertexNormalOffset;
			float _EndFoldoutVertexNormalOffset;
			float _StartFoldoutVertexWave;
			float _EndFoldoutVertexWave;
			float _StartFoldoutVertexNoise;
			float _EndFoldoutVertexNoise;
			float _StartFoldoutVertexWaveNoiseVerticalMask;
			float _EndFoldoutVertexWaveNoiseVerticalMask;
			float _StartFoldoutVertexOffsetoverY;
			float _EndFoldoutSpherizeNoise;
			float _EndFoldoutVertexOffsetoverY;
			float _StartFoldoutSpherizeNoise;
			float _StartFoldoutVerticalMasks;
			float _StartFoldoutLighting;
			float _EndFoldoutLighting;
			float _StartFoldoutBaseUVs;
			float _EndFoldoutBaseUVs;
			float _StartFoldoutParticleSettings;
			float _EndFoldoutParticleSettings;
			float _StartFoldoutColour;
			float _EndFoldoutColour;
			float _StartFoldoutVerticalColour;
			float _EndFoldoutVerticalColour;
			float _StartFoldoutNoise;
			float _EndFoldoutNoise;
			float _StartFoldoutNoiseDistortion;
			float _EndFoldoutNoiseDistortion;
			float _EndFoldoutRadialMask;
			float _StartFoldoutRadialMaskDistortion;
			float _EndFoldoutRadialMaskDistortion;
			float _EndFoldoutVerticalMasks;
			float _Tessellation;
			float _VertexNormalOffset;
			float _VertexNormalOffsetTopPower;
			float _VertexOffsetOverY2Power;
			float _VertexOffsetOverCircularYPower;
			float _NoiseRemapMin;
			float _NoiseRemapMax;
			float _UVSampleNoise;
			float _SpherizeNoiseRadius;
			float _SpherizeNoiseStrength;
			float _NoiseXZTwist;
			float _NoiseUVYPreOffset;
			float _NoiseUVYPreScale;
			float _NoiseUVYPrePower;
			float _NoiseParallaxOffset;
			float _NoiseDistortionScale;
			float _NoiseDistortionOctaves;
			float _AdditionalLightsTransparencyShadow;
			float _NoiseDistortionPower;
			float _NoiseDistortion;
			float _VertexOffsetOverY1Power;
			float _VertexTwist;
			float _VertexUVOffsetBottom;
			float _VertexUVOffsetBottomPower;
			float _VertexNormalOffsetTop;
			float _VertexNormalOffsetBottomPower;
			float _VertexNormalOffsetBottom;
			float _VertexWaveScale;
			float _VertexWaveAnimation;
			float _VertexWaveOffset;
			float _VertexWave;
			float _VertexWaveNoiseVerticalMaskRemapMin;
			float _NoiseOctaves;
			float _VertexWaveNoiseVerticalMaskRemapMax;
			float _VertexNoiseScale;
			float _ParticleRandomization;
			float _VertexNoiseOctaves;
			float _VertexNoiseDilation;
			float _VertexNoiseTwist;
			float _VertexNoise;
			float _VertexUVOffsetTopPower;
			float _VertexUVOffsetTop;
			float _VertexWaveNoiseVerticalMaskPower;
			float _IntersectionHighlightShadowMaskPower;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float localTwistXZ_float11_g587 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float UV_2D_Y691 = (staticSwitch321).y;
				float3 Vertex_Normal_Offset670 = ( ( input.normalOS * _VertexNormalOffset ) + ( pow( UV_2D_Y691 , _VertexNormalOffsetTopPower ) * input.normalOS * _VertexNormalOffsetTop ) + ( pow( ( 1.0 - UV_2D_Y691 ) , _VertexNormalOffsetBottomPower ) * input.normalOS * _VertexNormalOffsetBottom ) );
				float2 UV_2D327 = staticSwitch321;
				float mulTime258 = _TimeParameters.x * _VertexWaveAnimation;
				float temp_output_7_0_g582 = _VertexWaveNoiseVerticalMaskRemapMin;
				float temp_output_23_0_g582 = _VertexWaveNoiseVerticalMaskRemapMax;
				float temp_output_20_0_g582 = UV_2D_Y691;
				float temp_output_4_0_g582 = _VertexWaveNoiseVerticalMaskPower;
				float smoothstepResult22_g582 = smoothstep( temp_output_7_0_g582 , temp_output_23_0_g582 , pow( temp_output_20_0_g582 , temp_output_4_0_g582 ));
				float Vertex_WaveNoise_Vertical_Mask280 = smoothstepResult22_g582;
				#ifdef _VERTEXWAVEENABLED_ON
				float3 staticSwitch448 = ( ( sin( ( ( UV_2D327.y * TWO_PI * _VertexWaveScale ) - ( mulTime258 + ( _VertexWaveOffset * TWO_PI ) ) ) ) * _VertexWave * Vertex_WaveNoise_Vertical_Mask280 ) * input.normalOS );
				#else
				float3 staticSwitch448 = float3( 0,0,0 );
				#endif
				float3 Vertex_Sine265 = staticSwitch448;
				float localTwistXZ_float11_g585 = ( 0.0 );
				float localSimplexNoise_float2_g584 = ( 0.0 );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.positionOS.xyz).yxz;
				#else
				float3 staticSwitch339 = input.positionOS.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch605 = UV_3D_World622;
				#else
				float3 staticSwitch605 = UV_3D340;
				#endif
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Vertex_Noise_Offset311 = ( _VertexNoiseOffset + Particle_Stable_Random_X147 + ( _VertexNoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g579 = ( float4( ( staticSwitch605 * _VertexNoiseScale * _VertexNoiseTiling ) , 0.0 ) - ( Vertex_Noise_Offset311 + ( _VertexNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_292_0 = (temp_output_10_0_g579).xyz;
				float3 position2_g584 = temp_output_292_0;
				float temp_output_292_15 = (temp_output_10_0_g579).w;
				float angle2_g584 = temp_output_292_15;
				float octaves2_g584 = _VertexNoiseOctaves;
				float noise2_g584 = 0.0;
				float3 gradient2_g584 = float3( 0,0,0 );
				SimplexNoise_float( position2_g584 , angle2_g584 , octaves2_g584 , noise2_g584 , gradient2_g584 );
				float localSimplexNoise_Caustics_float2_g583 = ( 0.0 );
				float3 position2_g583 = temp_output_292_0;
				float angle2_g583 = temp_output_292_15;
				float octaves2_g583 = _VertexNoiseOctaves;
				float gradientStrength2_g583 = _VertexNoiseDilation;
				float noise2_g583 = 0.0;
				float3 gradient2_g583 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g583 , angle2_g583 , octaves2_g583 , gradientStrength2_g583 , noise2_g583 , gradient2_g583 );
				#ifdef _VERTEXNOISEDILATIONENABLED_ON
				float3 staticSwitch490 = gradient2_g583;
				#else
				float3 staticSwitch490 = gradient2_g584;
				#endif
				float3 temp_output_10_0_g585 = staticSwitch490;
				float3 position11_g585 = temp_output_10_0_g585;
				float temp_output_9_0_g585 = _VertexNoiseTwist;
				float angle11_g585 = radians( temp_output_9_0_g585 );
				float3 output11_g585 = float3( 0,0,0 );
				TwistXZ_float( position11_g585 , angle11_g585 , output11_g585 );
				float3 temp_output_852_0 = output11_g585;
				#ifdef _VERTEXNOISEENABLED_ON
				float3 staticSwitch450 = ( temp_output_852_0 * _VertexNoise * Vertex_WaveNoise_Vertical_Mask280 );
				#else
				float3 staticSwitch450 = float3( 0,0,0 );
				#endif
				float3 Vertex_Noise298 = staticSwitch450;
				float2 break587 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_576_0 = ( ( break587.x * pow( break587.y , _VertexUVOffsetTopPower ) ) * ( _VertexUVOffsetTop * 0.5 ) );
				float3 appendResult575 = (float3(temp_output_576_0 , 0.0 , 0.0));
				float3 appendResult594 = (float3(0.0 , temp_output_576_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch593 = appendResult594;
				#else
				float3 staticSwitch593 = appendResult575;
				#endif
				float3 Vertex_Offset_Top579 = staticSwitch593;
				float2 break644 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_650_0 = ( ( break644.x * pow( ( 1.0 - break644.y ) , _VertexUVOffsetBottomPower ) ) * ( _VertexUVOffsetBottom * 0.5 ) );
				float3 appendResult653 = (float3(temp_output_650_0 , 0.0 , 0.0));
				float3 appendResult652 = (float3(0.0 , temp_output_650_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch654 = appendResult652;
				#else
				float3 staticSwitch654 = appendResult653;
				#endif
				float3 Vertex_Offset_Bottom655 = staticSwitch654;
				float3 temp_output_10_0_g587 = ( ( Vertex_Normal_Offset670 + Vertex_Sine265 + Vertex_Noise298 + Vertex_Offset_Top579 + Vertex_Offset_Bottom655 ) + input.positionOS.xyz );
				float3 position11_g587 = temp_output_10_0_g587;
				float temp_output_9_0_g587 = -_VertexTwist;
				float angle11_g587 = radians( temp_output_9_0_g587 );
				float3 output11_g587 = float3( 0,0,0 );
				TwistXZ_float( position11_g587 , angle11_g587 , output11_g587 );
				float3 worldToObjDir636 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY1, 0.0 ) ).xyz;
				float3 worldToObjDir780 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY2, 0.0 ) ).xyz;
				float UV_2D_Circular_Y897 = sin( ( UV_2D_Y691 * PI ) );
				float3 Vertex_Offset_over_Y639 = ( ( worldToObjDir636 * pow( UV_2D_Y691 , _VertexOffsetOverY1Power ) ) + ( worldToObjDir780 * pow( UV_2D_Y691 , _VertexOffsetOverY2Power ) ) + ( _VertexOffsetOverCircularY * pow( UV_2D_Circular_Y897 , _VertexOffsetOverCircularYPower ) ) );
				float3 Vertex_Offset247 = ( output11_g587 + Vertex_Offset_over_Y639 );
				
				output.ase_texcoord2.xyz = ase_positionWS;
				float4 ase_positionCS = TransformObjectToHClip( ( input.positionOS ).xyz );
				float4 screenPos = ComputeScreenPos( ase_positionCS );
				output.ase_texcoord3 = screenPos;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord5.xyz = ase_normalWS;
				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord2.w = eyeDepth;
				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord6.xyz = ase_tangentWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord7.xyz = ase_bitangentWS;
				
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.positionOS;
				output.ase_texcoord4 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord5.w = 0;
				output.ase_texcoord6.w = 0;
				output.ase_texcoord7.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = Vertex_Offset247;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _Tessellation; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float temp_output_7_0_g574 = _NoiseRemapMin;
				float temp_output_23_0_g574 = _NoiseRemapMax;
				float localSimplexNoise_float2_g507 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float2 UV_2D327 = staticSwitch321;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.ase_texcoord1.xyz).yxz;
				#else
				float3 staticSwitch339 = input.ase_texcoord1.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				#ifdef _OBJECTSPACEUVS_ON
				float3 staticSwitch223 = UV_3D340;
				#else
				float3 staticSwitch223 = float3( UV_2D327 ,  0.0 );
				#endif
				float3 ase_positionWS = input.ase_texcoord2.xyz;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch356 = UV_3D_World622;
				#else
				float3 staticSwitch356 = staticSwitch223;
				#endif
				float4 screenPos = input.ase_texcoord3;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 appendResult1146 = (float2(ase_positionSSNorm.x , ase_positionSSNorm.y));
				float2 Screen_UV1145 = appendResult1146;
				float2 appendResult1112 = (float2(_ScaledScreenParams.x , _ScaledScreenParams.y));
				float2 Screen_Resolution1113 = appendResult1112;
				float2 Screen_Position1254 = ( Screen_UV1145 * Screen_Resolution1113 );
				float2 screenPosition1243 = Screen_Position1254;
				float mulTime1247 = _TimeParameters.x * 60.0;
				float time1243 = mulTime1247;
				float3 localHash331243 = Hash33( screenPosition1243 , time1243 );
				float3 Sample_Noise1248 = ( (localHash331243*2.0 + -1.0) * _UVSampleNoise );
				float3 Noise_Base_UV220 = ( staticSwitch356 + Sample_Noise1248 );
				float localSpherize_float5_g491 = ( 0.0 );
				float2 uv5_g491 = (Noise_Base_UV220).xy;
				float2 center5_g491 = ( _SpherizeNoiseOffset + float2( 0.5,0.5 ) );
				float radius5_g491 = _SpherizeNoiseRadius;
				float strength5_g491 = _SpherizeNoiseStrength;
				float2 output5_g491 = float2( 0,0 );
				Spherize_float( uv5_g491 , center5_g491 , radius5_g491 , strength5_g491 , output5_g491 );
				float3 appendResult611 = (float3(output5_g491 , (Noise_Base_UV220).z));
				#ifdef _SPHERIZENOISE_ON
				float3 staticSwitch625 = appendResult611;
				#else
				float3 staticSwitch625 = Noise_Base_UV220;
				#endif
				float localTwistXZ_float11_g495 = ( 0.0 );
				float3 temp_output_10_0_g495 = staticSwitch625;
				float3 position11_g495 = temp_output_10_0_g495;
				float temp_output_9_0_g495 = _NoiseXZTwist;
				float angle11_g495 = radians( temp_output_9_0_g495 );
				float3 output11_g495 = float3( 0,0,0 );
				TwistXZ_float( position11_g495 , angle11_g495 , output11_g495 );
				#ifdef _NOISEXZTWISTENABLED_ON
				float3 staticSwitch801 = output11_g495;
				#else
				float3 staticSwitch801 = staticSwitch625;
				#endif
				float3 break728 = staticSwitch801;
				float temp_output_738_0 = ( ( break728.y - _NoiseUVYPreOffset ) * _NoiseUVYPreScale );
				float temp_output_7_0_g499 = abs( temp_output_738_0 );
				float temp_output_733_14 = ( pow( temp_output_7_0_g499 , _NoiseUVYPrePower ) * sign( temp_output_738_0 ) );
				float3 appendResult732 = (float3(break728.x , temp_output_733_14 , break728.z));
				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - ase_positionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 temp_output_872_0 = ( -ase_viewDirWS * _NoiseParallaxOffset );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch464 = (temp_output_872_0).yxz;
				#else
				float3 staticSwitch464 = temp_output_872_0;
				#endif
				float3 Parallax_Offset465 = staticSwitch464;
				float localSimplexNoise_float2_g493 = ( 0.0 );
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Distortion_Noise_Offset163 = ( _NoiseDistortionOffset + Particle_Stable_Random_X147 + ( _NoiseDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g489 = ( float4( ( ( Noise_Base_UV220 + Parallax_Offset465 ) * _NoiseDistortionScale * _NoiseDistortionTiling ) , 0.0 ) - ( Distortion_Noise_Offset163 + ( _NoiseDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_160_0 = (temp_output_10_0_g489).xyz;
				float3 position2_g493 = temp_output_160_0;
				float temp_output_160_15 = (temp_output_10_0_g489).w;
				float angle2_g493 = temp_output_160_15;
				float octaves2_g493 = _NoiseDistortionOctaves;
				float noise2_g493 = 0.0;
				float3 gradient2_g493 = float3( 0,0,0 );
				SimplexNoise_float( position2_g493 , angle2_g493 , octaves2_g493 , noise2_g493 , gradient2_g493 );
				float localSimplexNoise_Caustics_float2_g492 = ( 0.0 );
				float3 position2_g492 = temp_output_160_0;
				float angle2_g492 = temp_output_160_15;
				float octaves2_g492 = _NoiseDistortionOctaves;
				float gradientStrength2_g492 = _NoiseDistortionDilation;
				float noise2_g492 = 0.0;
				float3 gradient2_g492 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g492 , angle2_g492 , octaves2_g492 , gradientStrength2_g492 , noise2_g492 , gradient2_g492 );
				#ifdef _NOISEDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch440 = gradient2_g492;
				#else
				float3 staticSwitch440 = gradient2_g493;
				#endif
				float3 temp_output_7_0_g497 = abs( staticSwitch440 );
				float3 temp_cast_2 = (_NoiseDistortionPower).xxx;
				#ifdef _NOISEDISTORTIONENABLED_ON
				float3 staticSwitch442 = ( ( pow( temp_output_7_0_g497 , temp_cast_2 ) * sign( staticSwitch440 ) ) * _NoiseDistortion );
				#else
				float3 staticSwitch442 = float3( 0,0,0 );
				#endif
				float3 Noise_Distortion73 = staticSwitch442;
				float3 Noise_UV173 = ( appendResult732 + Parallax_Offset465 + Noise_Distortion73 );
				float4 Noise_Offset175 = ( _NoiseOffset + Particle_Stable_Random_X147 + ( _NoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g503 = ( float4( ( Noise_UV173 * _NoiseScale * _NoiseTiling ) , 0.0 ) - ( Noise_Offset175 + ( _NoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_159_0 = (temp_output_10_0_g503).xyz;
				float3 position2_g507 = temp_output_159_0;
				float temp_output_159_15 = (temp_output_10_0_g503).w;
				float angle2_g507 = temp_output_159_15;
				float octaves2_g507 = _NoiseOctaves;
				float noise2_g507 = 0.0;
				float3 gradient2_g507 = float3( 0,0,0 );
				SimplexNoise_float( position2_g507 , angle2_g507 , octaves2_g507 , noise2_g507 , gradient2_g507 );
				float localSimplexNoise_Caustics_float2_g506 = ( 0.0 );
				float3 position2_g506 = temp_output_159_0;
				float angle2_g506 = temp_output_159_15;
				float octaves2_g506 = _NoiseOctaves;
				float gradientStrength2_g506 = _NoiseDilation;
				float noise2_g506 = 0.0;
				float3 gradient2_g506 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g506 , angle2_g506 , octaves2_g506 , gradientStrength2_g506 , noise2_g506 , gradient2_g506 );
				#ifdef _NOISEDILATIONENABLED_ON
				float staticSwitch444 = noise2_g506;
				#else
				float staticSwitch444 = noise2_g507;
				#endif
				float temp_output_20_0_g574 = staticSwitch444;
				float temp_output_4_0_g574 = _NoisePower;
				float smoothstepResult22_g574 = smoothstep( temp_output_7_0_g574 , temp_output_23_0_g574 , pow( temp_output_20_0_g574 , temp_output_4_0_g574 ));
				float Particle_Subtract_Noise_over_Lifetime480 = ( input.ase_texcoord4.y * _ParticleSubtractNoiseoverLifetime );
				float temp_output_478_0 = ( smoothstepResult22_g574 - Particle_Subtract_Noise_over_Lifetime480 );
				float lerpResult569 = lerp( 1.0 , temp_output_478_0 , _Noise);
				float Noise11 = lerpResult569;
				float Particle_Mask_Radius_over_Lifetime161 = input.ase_texcoord4.x;
				float lerpResult183 = lerp( 1.0 , Particle_Mask_Radius_over_Lifetime161 , _RadialMaskRadiusOverParticleLifetime);
				float temp_output_6_0_g500 = ( 1.0 - ( _RadialMaskRadius * lerpResult183 ) );
				float lerpResult5_g500 = lerp( temp_output_6_0_g500 , 1.0 , _RadialMaskFeather);
				float2 texCoord330 = input.ase_texcoord.xy * float2( 2,2 ) + float2( -1,-1 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch329 = (texCoord330).yx;
				#else
				float2 staticSwitch329 = texCoord330;
				#endif
				float2 UV_2D_Centered328 = staticSwitch329;
				float localSimplexNoise_float2_g486 = ( 0.0 );
				float4 Mask_Distortion_Noise_Offset171 = ( _RadialMaskDistortionOffset + Particle_Stable_Random_X147 + ( _RadialMaskDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g483 = ( float4( ( Noise_Base_UV220 * _RadialMaskDistortionScale * _RadialMaskDistortionTiling ) , 0.0 ) - ( Mask_Distortion_Noise_Offset171 + ( _RadialMaskDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_158_0 = (temp_output_10_0_g483).xyz;
				float3 position2_g486 = temp_output_158_0;
				float temp_output_158_15 = (temp_output_10_0_g483).w;
				float angle2_g486 = temp_output_158_15;
				float octaves2_g486 = _RadialMaskDistortionOctaves;
				float noise2_g486 = 0.0;
				float3 gradient2_g486 = float3( 0,0,0 );
				SimplexNoise_float( position2_g486 , angle2_g486 , octaves2_g486 , noise2_g486 , gradient2_g486 );
				float localSimplexNoise_Caustics_float2_g485 = ( 0.0 );
				float3 position2_g485 = temp_output_158_0;
				float angle2_g485 = temp_output_158_15;
				float octaves2_g485 = _RadialMaskDistortionOctaves;
				float gradientStrength2_g485 = _RadialMaskDistortionDilation;
				float noise2_g485 = 0.0;
				float3 gradient2_g485 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g485 , angle2_g485 , octaves2_g485 , gradientStrength2_g485 , noise2_g485 , gradient2_g485 );
				#ifdef _RADIALMASKDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch441 = gradient2_g485;
				#else
				float3 staticSwitch441 = gradient2_g486;
				#endif
				float3 temp_output_7_0_g494 = abs( staticSwitch441 );
				float3 temp_cast_5 = (_RadialMaskDistortionPower).xxx;
				#ifdef _RADIALMASKDISTORTIONENABLED_ON
				float3 staticSwitch451 = ( ( pow( temp_output_7_0_g494 , temp_cast_5 ) * sign( staticSwitch441 ) ) * _RadialMaskDistortion );
				#else
				float3 staticSwitch451 = float3( 0,0,0 );
				#endif
				float3 Mask_Distortion117 = staticSwitch451;
				float temp_output_7_0_g500 = ( 1.0 - length( ( ( ( UV_2D_Centered328 + (Mask_Distortion117).xy ) - _RadialMaskOffset ) * _RadialMaskTiling ) ) );
				float smoothstepResult4_g500 = smoothstep( temp_output_6_0_g500 , lerpResult5_g500 , temp_output_7_0_g500);
				#ifdef _RADIALMASK_ON
				float staticSwitch601 = ( 1.0 - pow( smoothstepResult4_g500 , _RadialMaskPower ) );
				#else
				float staticSwitch601 = 0.0;
				#endif
				float Radial_Mask227 = staticSwitch601;
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch942 = Radial_Mask227;
				#else
				float staticSwitch942 = 0.0;
				#endif
				float temp_output_7_0_g505 = _VerticalMask1RemapMax;
				float temp_output_23_0_g505 = _VerticalMask1RemapMin;
				float UV_2D_Y691 = (staticSwitch321).y;
				float UV_3D_Y719 = (staticSwitch339).y;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch716 = ( ( UV_3D_Y719 - _VerticalMask1ObjectSpaceOffset ) / _VerticalMask1ObjectSpaceScale );
				#else
				float staticSwitch716 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g505 = staticSwitch716;
				float smoothstepResult25_g505 = smoothstep( temp_output_7_0_g505 , temp_output_23_0_g505 , temp_output_20_0_g505);
				float temp_output_4_0_g505 = _VerticalMask1Power;
				float temp_output_862_0 = pow( smoothstepResult25_g505 , temp_output_4_0_g505 );
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch713 = ( 1.0 - temp_output_862_0 );
				#else
				float staticSwitch713 = temp_output_862_0;
				#endif
				float Vertical_Mask_1350 = staticSwitch713;
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch709 = ( staticSwitch942 + Vertical_Mask_1350 );
				#else
				float staticSwitch709 = staticSwitch942;
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch741 = staticSwitch709;
				#else
				float staticSwitch741 = staticSwitch942;
				#endif
				float temp_output_7_0_g508 = _VerticalMask2RemapMin;
				float temp_output_23_0_g508 = _VerticalMask2RemapMax;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch767 = ( ( UV_3D_Y719 - _VerticalMask2ObjectSpaceOffset ) / _VerticalMask2ObjectSpaceScale );
				#else
				float staticSwitch767 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g508 = staticSwitch767;
				float smoothstepResult25_g508 = smoothstep( temp_output_7_0_g508 , temp_output_23_0_g508 , temp_output_20_0_g508);
				float temp_output_4_0_g508 = _VerticalMask2Power;
				float temp_output_863_0 = pow( smoothstepResult25_g508 , temp_output_4_0_g508 );
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch758 = ( 1.0 - temp_output_863_0 );
				#else
				float staticSwitch758 = temp_output_863_0;
				#endif
				float Vertical_Mask_2759 = staticSwitch758;
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch760 = ( staticSwitch741 + Vertical_Mask_2759 );
				#else
				float staticSwitch760 = staticSwitch741;
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch766 = staticSwitch760;
				#else
				float staticSwitch766 = staticSwitch741;
				#endif
				float3 ase_normalWS = input.ase_texcoord5.xyz;
				float fresnelNdotV232 = dot( ase_normalWS, ase_viewDirWS );
				float fresnelNode232 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV232, _FresnelMaskPower ) );
				float smoothstepResult234 = smoothstep( _FresnelMaskRemapMin , _FresnelMaskRemapMax , fresnelNode232);
				float lerpResult242 = lerp( 1.0 , smoothstepResult234 , _FresnelMask);
				float Fresnel_Mask233 = lerpResult242;
				float temp_output_7_0_g576 = 0.0;
				float temp_output_23_0_g576 = 1.0;
				float screenDepth186 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth186 = saturate( abs( ( screenDepth186 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				#ifdef _INVERTDEPTHFADE_ON
				float staticSwitch218 = ( 1.0 - distanceDepth186 );
				#else
				float staticSwitch218 = distanceDepth186;
				#endif
				float temp_output_20_0_g576 = staticSwitch218;
				float temp_output_4_0_g576 = _DepthFadePower;
				float smoothstepResult22_g576 = smoothstep( temp_output_7_0_g576 , temp_output_23_0_g576 , pow( temp_output_20_0_g576 , temp_output_4_0_g576 ));
				float temp_output_7_0_g575 = 0.0;
				float temp_output_23_0_g575 = 1.0;
				float screenDepth208 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth208 = saturate( abs( ( screenDepth208 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _SubtractiveDepthFade ) ) );
				float temp_output_20_0_g575 = ( 1.0 - distanceDepth208 );
				float temp_output_4_0_g575 = _SubtractiveDepthFadePower;
				float smoothstepResult22_g575 = smoothstep( temp_output_7_0_g575 , temp_output_23_0_g575 , pow( temp_output_20_0_g575 , temp_output_4_0_g575 ));
				float Depth_Fade188 = saturate( ( smoothstepResult22_g576 - smoothstepResult22_g575 ) );
				float temp_output_7_0_g577 = 0.0;
				float temp_output_23_0_g577 = 1.0;
				float eyeDepth = input.ase_texcoord2.w;
				float cameraDepthFade191 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g577 = saturate( cameraDepthFade191 );
				float temp_output_4_0_g577 = _CameraDepthFadePower;
				float smoothstepResult22_g577 = smoothstep( temp_output_7_0_g577 , temp_output_23_0_g577 , pow( temp_output_20_0_g577 , temp_output_4_0_g577 ));
				float Camera_Depth_Fade195 = smoothstepResult22_g577;
				float temp_output_7_0_g581 = _IntersectionHighlightRemapMin;
				float temp_output_23_0_g581 = _IntersectionHighlightRemapMax;
				float screenDepth1005 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth1005 = saturate( abs( ( screenDepth1005 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _IntersectionHighlight ) ) );
				float temp_output_20_0_g581 = ( 1.0 - distanceDepth1005 );
				float temp_output_4_0_g581 = _IntersectionHighlightPower;
				float smoothstepResult22_g581 = smoothstep( temp_output_7_0_g581 , temp_output_23_0_g581 , pow( temp_output_20_0_g581 , temp_output_4_0_g581 ));
				float Intersection_Highlight1011 = smoothstepResult22_g581;
				float Intersection_Highlight_Alpha1024 = _IntersectionHighlightColour.a;
				float localGetMainLightShadow_float1080 = ( 0.0 );
				float3 position1080 = ase_positionWS;
				float2 screenPosition1080 = Screen_Position1254;
				float worldNoise1080 = _MainLightShadowWorldNoise;
				float lightDirectionNoise1080 = _MainLightShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1080 = _MainLightShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1080 = _MainLightShadowDirectionalAlignmentNoise;
				float3 normal1080 = ase_normalWS;
				float3 ase_tangentWS = input.ase_texcoord6.xyz;
				float3 ase_bitangentWS = input.ase_texcoord7.xyz;
				float3x3 ase_worldToTangent = float3x3( ase_tangentWS, ase_bitangentWS, ase_normalWS );
				float3x3 worldToTangentMatrix1080 = ase_worldToTangent;
				float output1080 = 0.0;
				GetMainLightShadow_float( position1080 , screenPosition1080 , worldNoise1080 , lightDirectionNoise1080 , lightDirectionOrthogonalAlignmentNoise1080 , lightDirectionAlignmentNoise1080 , normal1080 , worldToTangentMatrix1080 , output1080 );
				float lerpResult984 = lerp( 1.0 , output1080 , _MainLightTransparencyShadow);
				float Main_Light_Transparency_Shadow981 = lerpResult984;
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1076 = Main_Light_Transparency_Shadow981;
				#else
				float staticSwitch1076 = 1.0;
				#endif
				float localGetAdditionalLightsShadow_float1125 = ( 0.0 );
				float3 position1125 = ase_positionWS;
				float2 screenPosition1125 = Screen_Position1254;
				float2 screenUV1125 = Screen_UV1145;
				float worldNoise1125 = _AdditionalLightsShadowWorldNoise;
				float lightDirectionNoise1125 = _AdditionalLightsShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1125 = _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1125 = _AdditionalLightsShadowDirectionalAlignmentNoise;
				float3 normal1125 = ase_normalWS;
				float3x3 worldToTangentMatrix1125 = ase_worldToTangent;
				float output1125 = 0.0;
				GetAdditionalLightsShadow_float( position1125 , screenPosition1125 , screenUV1125 , worldNoise1125 , lightDirectionNoise1125 , lightDirectionOrthogonalAlignmentNoise1125 , lightDirectionAlignmentNoise1125 , normal1125 , worldToTangentMatrix1125 , output1125 );
				float lerpResult993 = lerp( 1.0 , output1125 , _AdditionalLightsTransparencyShadow);
				float Additional_Lights_Transparency_Shadow994 = lerpResult993;
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1077 = Additional_Lights_Transparency_Shadow994;
				#else
				float staticSwitch1077 = 1.0;
				#endif
				float temp_output_1030_0 = saturate( ( ( saturate( ( Noise11 - staticSwitch766 ) ) * Fresnel_Mask233 * (input.ase_color).a * Depth_Fade188 * Camera_Depth_Fade195 * _Alpha ) + ( Intersection_Highlight1011 * Intersection_Highlight_Alpha1024 * pow( ( staticSwitch1076 * staticSwitch1077 ) , _IntersectionHighlightShadowMaskPower ) ) ) );
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch943 = temp_output_1030_0;
				#else
				float staticSwitch943 = ( temp_output_1030_0 * ( 1.0 - Radial_Mask227 ) );
				#endif
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch711 = staticSwitch943;
				#else
				float staticSwitch711 = ( staticSwitch943 * Vertical_Mask_1350 );
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch744 = staticSwitch711;
				#else
				float staticSwitch744 = staticSwitch943;
				#endif
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch763 = staticSwitch744;
				#else
				float staticSwitch763 = ( staticSwitch744 * Vertical_Mask_2759 );
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch768 = staticSwitch763;
				#else
				float staticSwitch768 = staticSwitch744;
				#endif
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch985 = ( staticSwitch768 * Main_Light_Transparency_Shadow981 );
				#else
				float staticSwitch985 = staticSwitch768;
				#endif
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch998 = ( staticSwitch985 * Additional_Lights_Transparency_Shadow994 );
				#else
				float staticSwitch998 = staticSwitch985;
				#endif
				float Alpha49 = staticSwitch998;
				

				surfaceDescription.Alpha = Alpha49;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = unity_SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			

        	#define _NORMAL_DROPOFF_TS 1
        	#pragma multi_compile_instancing
        	#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
        	#define ASE_FOG 1
        	#define ASE_FIXED_TESSELLATION
        	#define _SURFACE_TYPE_TRANSPARENT 1
        	#define _RECEIVE_SHADOWS_OFF
        	#define ASE_TESSELLATION 1
        	#pragma require tessellation tessHW
        	#pragma hull HullFunction
        	#pragma domain DomainFunction
        	#define ASE_PHONG_TESSELLATION
        	#define ASE_ABSOLUTE_VERTEX_POS 1
        	#define ASE_VERSION 19912
        	#define ASE_SRP_VERSION 140011
        	#define REQUIRE_DEPTH_TEXTURE 1


			

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS
			#define VARYINGS_NEED_TANGENT_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif


			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Math.cginc"
			#include "../../../Mirza/VFXToolkit/Shaders/_Includes/Noise.cginc"
			#include "../../../Mirza/Solaris/Shaders/_Includes/Solaris.hlsl"

			
			
			

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES1
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#pragma shader_feature_local _SWAPUVXY_ON
			#pragma shader_feature_local _VERTEXWAVEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEENABLED_ON
			#pragma shader_feature_local _VERTEXNOISEDILATIONENABLED_ON
			#pragma shader_feature_local _WORLDSPACEUVS_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
			#pragma shader_feature_local _VERTICALMASK2_ON
			#pragma shader_feature_local _VERTICALMASK1_ON
			#pragma shader_feature_local _RADIALMASKSUBTRACTIVE_ON
			#pragma shader_feature_local _NOISEDILATIONENABLED_ON
			#pragma shader_feature_local _NOISEXZTWISTENABLED_ON
			#pragma shader_feature_local _SPHERIZENOISE_ON
			#pragma shader_feature_local _OBJECTSPACEUVS_ON
			#pragma shader_feature_local _NOISEDISTORTIONENABLED_ON
			#pragma shader_feature_local _NOISEDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _RADIALMASK_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONENABLED_ON
			#pragma shader_feature_local _RADIALMASKDISTORTIONDILATIONENABLED_ON
			#pragma shader_feature_local _VERTICALMASK1SUBTRACTIVE_ON
			#pragma shader_feature_local _VERTICALMASKSOBJECTSPACE_ON
			#pragma shader_feature_local _VERTICALMASK2SUBTRACTIVE_ON
			#pragma shader_feature_local _INVERTDEPTHFADE_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				half3 normalWS : TEXCOORD0;
				float4 tangentWS : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ColourA;
			float4 _RadialMaskDistortionAnimation;
			float4 _VerticalColourA;
			float4 _VerticalColourB;
			float4 _RadialMaskDistortionParticleAnimation;
			float4 _RadialMaskDistortionOffset;
			float4 _NoiseDistortionOffset;
			float4 _ColourB;
			float4 _NoiseDistortionAnimation;
			float4 _NoiseDistortionParticleAnimation;
			float4 _IntersectionHighlightColour;
			float4 _VertexNoiseAnimation;
			float4 _NoiseAnimation;
			float4 _VertexNoiseParticleAnimation;
			float4 _NoiseOffset;
			float4 _NoiseParticleAnimation;
			float4 _VertexNoiseOffset;
			float3 _VertexOffsetOverY1;
			float3 _RadialMaskDistortionTiling;
			float3 _VertexOffsetOverCircularY;
			float3 _VertexOffsetOverY2;
			float3 _VertexNoiseTiling;
			float3 _NoiseTiling;
			float3 _NoiseDistortionTiling;
			float2 _RadialMaskTiling;
			float2 _RadialMaskOffset;
			float2 _SpherizeNoiseOffset;
			float _VerticalColourMaskRemapMax;
			float _MainLightShadowOrthogonalAlignmentWorldNoise;
			float _MainLightShadowDirectionalAlignmentNoise;
			float _Noise;
			float _MainLightShadow;
			float _MainLightAnisotropyBlend;
			float _MainLightAnisotropy;
			float _AdditionalLightsFlatLambertBlend;
			float _ParticleSubtractNoiseoverLifetime;
			float _NoisePower;
			float _AdditionalLightsLambertTranslucencyMask;
			float _MainLight;
			float _MainLightShadowDirectionalNoise;
			float _ColourPower;
			float _MainLightLambertTranslucency;
			float _ColourHueShift;
			float _MainLightLambertTranslucencyMask;
			float _ColourSaturationShift;
			float _ColourValueMultiplier;
			float _VerticalColourHueShift;
			float _VerticalColourSaturationShift;
			float _MainLightFlatLambertBlend;
			float _VerticalColourValueMultiplier;
			float _VerticalColourMaskRemapMin;
			float _LambertLightingNormalfromHeight;
			float _VerticalColourMaskPower;
			float _MainLightShadowWorldNoise;
			float _StartFoldoutRadialMask;
			float _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
			float _AdditionalLightsShadowWorldNoise;
			float _VerticalMask1Power;
			float _VerticalMask2RemapMin;
			float _VerticalMask2RemapMax;
			float _VerticalMask2ObjectSpaceOffset;
			float _VerticalMask2ObjectSpaceScale;
			float _VerticalMask2Power;
			float _FresnelMaskRemapMin;
			float _FresnelMaskRemapMax;
			float _FresnelMaskPower;
			float _FresnelMask;
			float _DepthFade;
			float _DepthFadePower;
			float _SubtractiveDepthFade;
			float _SubtractiveDepthFadePower;
			float _CameraDepthFadeLength;
			float _CameraDepthFadeOffset;
			float _CameraDepthFadePower;
			float _Alpha;
			float _MainLightTransparencyShadow;
			float _VerticalMask1ObjectSpaceScale;
			float _VerticalMask1ObjectSpaceOffset;
			float _VerticalMask1RemapMin;
			float _VerticalMask1RemapMax;
			float _AdditionalLightsShadowDirectionalNoise;
			float _AdditionalLightsShadowDirectionalAlignmentNoise;
			float _AdditionalLightsShadow;
			float _AdditionalLights;
			float _IntersectionHighlightColourHueShift;
			float _IntersectionHighlightColourSaturationShift;
			float _IntersectionHighlightColourValueMultiplier;
			float _IntersectionHighlightRemapMin;
			float _IntersectionHighlightRemapMax;
			float _AdditionalLightsLambertTranslucency;
			float _IntersectionHighlight;
			float _RadialMaskRadius;
			float _RadialMaskRadiusOverParticleLifetime;
			float _RadialMaskFeather;
			float _RadialMaskDistortionScale;
			float _RadialMaskDistortionOctaves;
			float _RadialMaskDistortionDilation;
			float _RadialMaskDistortionPower;
			float _RadialMaskDistortion;
			float _RadialMaskPower;
			float _IntersectionHighlightPower;
			float _NoiseDilation;
			float _NoiseDistortionDilation;
			float _NoiseScale;
			float _StartFoldoutFresnelMask;
			float _EndFoldoutFresnelMask;
			float _StartFoldoutDepthFade;
			float _EndFoldoutDepthFade;
			float _StartFoldoutIntersectionHighlight;
			float _EndFoldoutIntersectionHighlight;
			float _StartFoldoutVertexUVOffset;
			float _EndFoldoutVertexUVOffset;
			float _StartFoldoutVertexNormalOffset;
			float _EndFoldoutVertexNormalOffset;
			float _StartFoldoutVertexWave;
			float _EndFoldoutVertexWave;
			float _StartFoldoutVertexNoise;
			float _EndFoldoutVertexNoise;
			float _StartFoldoutVertexWaveNoiseVerticalMask;
			float _EndFoldoutVertexWaveNoiseVerticalMask;
			float _StartFoldoutVertexOffsetoverY;
			float _EndFoldoutSpherizeNoise;
			float _EndFoldoutVertexOffsetoverY;
			float _StartFoldoutSpherizeNoise;
			float _StartFoldoutVerticalMasks;
			float _StartFoldoutLighting;
			float _EndFoldoutLighting;
			float _StartFoldoutBaseUVs;
			float _EndFoldoutBaseUVs;
			float _StartFoldoutParticleSettings;
			float _EndFoldoutParticleSettings;
			float _StartFoldoutColour;
			float _EndFoldoutColour;
			float _StartFoldoutVerticalColour;
			float _EndFoldoutVerticalColour;
			float _StartFoldoutNoise;
			float _EndFoldoutNoise;
			float _StartFoldoutNoiseDistortion;
			float _EndFoldoutNoiseDistortion;
			float _EndFoldoutRadialMask;
			float _StartFoldoutRadialMaskDistortion;
			float _EndFoldoutRadialMaskDistortion;
			float _EndFoldoutVerticalMasks;
			float _Tessellation;
			float _VertexNormalOffset;
			float _VertexNormalOffsetTopPower;
			float _VertexOffsetOverY2Power;
			float _VertexOffsetOverCircularYPower;
			float _NoiseRemapMin;
			float _NoiseRemapMax;
			float _UVSampleNoise;
			float _SpherizeNoiseRadius;
			float _SpherizeNoiseStrength;
			float _NoiseXZTwist;
			float _NoiseUVYPreOffset;
			float _NoiseUVYPreScale;
			float _NoiseUVYPrePower;
			float _NoiseParallaxOffset;
			float _NoiseDistortionScale;
			float _NoiseDistortionOctaves;
			float _AdditionalLightsTransparencyShadow;
			float _NoiseDistortionPower;
			float _NoiseDistortion;
			float _VertexOffsetOverY1Power;
			float _VertexTwist;
			float _VertexUVOffsetBottom;
			float _VertexUVOffsetBottomPower;
			float _VertexNormalOffsetTop;
			float _VertexNormalOffsetBottomPower;
			float _VertexNormalOffsetBottom;
			float _VertexWaveScale;
			float _VertexWaveAnimation;
			float _VertexWaveOffset;
			float _VertexWave;
			float _VertexWaveNoiseVerticalMaskRemapMin;
			float _NoiseOctaves;
			float _VertexWaveNoiseVerticalMaskRemapMax;
			float _VertexNoiseScale;
			float _ParticleRandomization;
			float _VertexNoiseOctaves;
			float _VertexNoiseDilation;
			float _VertexNoiseTwist;
			float _VertexNoise;
			float _VertexUVOffsetTopPower;
			float _VertexUVOffsetTop;
			float _VertexWaveNoiseVerticalMaskPower;
			float _IntersectionHighlightShadowMaskPower;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float localTwistXZ_float11_g587 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float UV_2D_Y691 = (staticSwitch321).y;
				float3 Vertex_Normal_Offset670 = ( ( input.normalOS * _VertexNormalOffset ) + ( pow( UV_2D_Y691 , _VertexNormalOffsetTopPower ) * input.normalOS * _VertexNormalOffsetTop ) + ( pow( ( 1.0 - UV_2D_Y691 ) , _VertexNormalOffsetBottomPower ) * input.normalOS * _VertexNormalOffsetBottom ) );
				float2 UV_2D327 = staticSwitch321;
				float mulTime258 = _TimeParameters.x * _VertexWaveAnimation;
				float temp_output_7_0_g582 = _VertexWaveNoiseVerticalMaskRemapMin;
				float temp_output_23_0_g582 = _VertexWaveNoiseVerticalMaskRemapMax;
				float temp_output_20_0_g582 = UV_2D_Y691;
				float temp_output_4_0_g582 = _VertexWaveNoiseVerticalMaskPower;
				float smoothstepResult22_g582 = smoothstep( temp_output_7_0_g582 , temp_output_23_0_g582 , pow( temp_output_20_0_g582 , temp_output_4_0_g582 ));
				float Vertex_WaveNoise_Vertical_Mask280 = smoothstepResult22_g582;
				#ifdef _VERTEXWAVEENABLED_ON
				float3 staticSwitch448 = ( ( sin( ( ( UV_2D327.y * TWO_PI * _VertexWaveScale ) - ( mulTime258 + ( _VertexWaveOffset * TWO_PI ) ) ) ) * _VertexWave * Vertex_WaveNoise_Vertical_Mask280 ) * input.normalOS );
				#else
				float3 staticSwitch448 = float3( 0,0,0 );
				#endif
				float3 Vertex_Sine265 = staticSwitch448;
				float localTwistXZ_float11_g585 = ( 0.0 );
				float localSimplexNoise_float2_g584 = ( 0.0 );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.positionOS.xyz).yxz;
				#else
				float3 staticSwitch339 = input.positionOS.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				float3 ase_positionWS = TransformObjectToWorld( ( input.positionOS ).xyz );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch605 = UV_3D_World622;
				#else
				float3 staticSwitch605 = UV_3D340;
				#endif
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord.z;
				float4 Vertex_Noise_Offset311 = ( _VertexNoiseOffset + Particle_Stable_Random_X147 + ( _VertexNoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g579 = ( float4( ( staticSwitch605 * _VertexNoiseScale * _VertexNoiseTiling ) , 0.0 ) - ( Vertex_Noise_Offset311 + ( _VertexNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_292_0 = (temp_output_10_0_g579).xyz;
				float3 position2_g584 = temp_output_292_0;
				float temp_output_292_15 = (temp_output_10_0_g579).w;
				float angle2_g584 = temp_output_292_15;
				float octaves2_g584 = _VertexNoiseOctaves;
				float noise2_g584 = 0.0;
				float3 gradient2_g584 = float3( 0,0,0 );
				SimplexNoise_float( position2_g584 , angle2_g584 , octaves2_g584 , noise2_g584 , gradient2_g584 );
				float localSimplexNoise_Caustics_float2_g583 = ( 0.0 );
				float3 position2_g583 = temp_output_292_0;
				float angle2_g583 = temp_output_292_15;
				float octaves2_g583 = _VertexNoiseOctaves;
				float gradientStrength2_g583 = _VertexNoiseDilation;
				float noise2_g583 = 0.0;
				float3 gradient2_g583 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g583 , angle2_g583 , octaves2_g583 , gradientStrength2_g583 , noise2_g583 , gradient2_g583 );
				#ifdef _VERTEXNOISEDILATIONENABLED_ON
				float3 staticSwitch490 = gradient2_g583;
				#else
				float3 staticSwitch490 = gradient2_g584;
				#endif
				float3 temp_output_10_0_g585 = staticSwitch490;
				float3 position11_g585 = temp_output_10_0_g585;
				float temp_output_9_0_g585 = _VertexNoiseTwist;
				float angle11_g585 = radians( temp_output_9_0_g585 );
				float3 output11_g585 = float3( 0,0,0 );
				TwistXZ_float( position11_g585 , angle11_g585 , output11_g585 );
				float3 temp_output_852_0 = output11_g585;
				#ifdef _VERTEXNOISEENABLED_ON
				float3 staticSwitch450 = ( temp_output_852_0 * _VertexNoise * Vertex_WaveNoise_Vertical_Mask280 );
				#else
				float3 staticSwitch450 = float3( 0,0,0 );
				#endif
				float3 Vertex_Noise298 = staticSwitch450;
				float2 break587 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_576_0 = ( ( break587.x * pow( break587.y , _VertexUVOffsetTopPower ) ) * ( _VertexUVOffsetTop * 0.5 ) );
				float3 appendResult575 = (float3(temp_output_576_0 , 0.0 , 0.0));
				float3 appendResult594 = (float3(0.0 , temp_output_576_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch593 = appendResult594;
				#else
				float3 staticSwitch593 = appendResult575;
				#endif
				float3 Vertex_Offset_Top579 = staticSwitch593;
				float2 break644 = ( ( UV_2D327 * float2( 2,1 ) ) - float2( 1,0 ) );
				float temp_output_650_0 = ( ( break644.x * pow( ( 1.0 - break644.y ) , _VertexUVOffsetBottomPower ) ) * ( _VertexUVOffsetBottom * 0.5 ) );
				float3 appendResult653 = (float3(temp_output_650_0 , 0.0 , 0.0));
				float3 appendResult652 = (float3(0.0 , temp_output_650_0 , 0.0));
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch654 = appendResult652;
				#else
				float3 staticSwitch654 = appendResult653;
				#endif
				float3 Vertex_Offset_Bottom655 = staticSwitch654;
				float3 temp_output_10_0_g587 = ( ( Vertex_Normal_Offset670 + Vertex_Sine265 + Vertex_Noise298 + Vertex_Offset_Top579 + Vertex_Offset_Bottom655 ) + input.positionOS.xyz );
				float3 position11_g587 = temp_output_10_0_g587;
				float temp_output_9_0_g587 = -_VertexTwist;
				float angle11_g587 = radians( temp_output_9_0_g587 );
				float3 output11_g587 = float3( 0,0,0 );
				TwistXZ_float( position11_g587 , angle11_g587 , output11_g587 );
				float3 worldToObjDir636 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY1, 0.0 ) ).xyz;
				float3 worldToObjDir780 = mul( GetWorldToObjectMatrix(), float4( _VertexOffsetOverY2, 0.0 ) ).xyz;
				float UV_2D_Circular_Y897 = sin( ( UV_2D_Y691 * PI ) );
				float3 Vertex_Offset_over_Y639 = ( ( worldToObjDir636 * pow( UV_2D_Y691 , _VertexOffsetOverY1Power ) ) + ( worldToObjDir780 * pow( UV_2D_Y691 , _VertexOffsetOverY2Power ) ) + ( _VertexOffsetOverCircularY * pow( UV_2D_Circular_Y897 , _VertexOffsetOverCircularYPower ) ) );
				float3 Vertex_Offset247 = ( output11_g587 + Vertex_Offset_over_Y639 );
				
				output.ase_texcoord4.xyz = ase_positionWS;
				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord4.w = eyeDepth;
				
				output.ase_texcoord2 = input.ase_texcoord;
				output.ase_texcoord3 = input.positionOS;
				output.ase_texcoord5 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = Vertex_Offset247;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.ase_texcoord1;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _Tessellation; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			void frag(PackedVaryings input
						, out half4 outNormalWS : SV_Target0
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 NormalWS = input.normalWS * renormFactor;
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				float temp_output_7_0_g574 = _NoiseRemapMin;
				float temp_output_23_0_g574 = _NoiseRemapMax;
				float localSimplexNoise_float2_g507 = ( 0.0 );
				float2 texCoord322 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch321 = (texCoord322).yx;
				#else
				float2 staticSwitch321 = texCoord322;
				#endif
				float2 UV_2D327 = staticSwitch321;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch339 = (input.ase_texcoord3.xyz).yxz;
				#else
				float3 staticSwitch339 = input.ase_texcoord3.xyz;
				#endif
				float3 UV_3D340 = staticSwitch339;
				#ifdef _OBJECTSPACEUVS_ON
				float3 staticSwitch223 = UV_3D340;
				#else
				float3 staticSwitch223 = float3( UV_2D327 ,  0.0 );
				#endif
				float3 ase_positionWS = input.ase_texcoord4.xyz;
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch621 = (ase_positionWS).yxz;
				#else
				float3 staticSwitch621 = ase_positionWS;
				#endif
				float3 UV_3D_World622 = staticSwitch621;
				#ifdef _WORLDSPACEUVS_ON
				float3 staticSwitch356 = UV_3D_World622;
				#else
				float3 staticSwitch356 = staticSwitch223;
				#endif
				float2 appendResult1146 = (float2(ScreenPosNorm.x , ScreenPosNorm.y));
				float2 Screen_UV1145 = appendResult1146;
				float2 appendResult1112 = (float2(_ScaledScreenParams.x , _ScaledScreenParams.y));
				float2 Screen_Resolution1113 = appendResult1112;
				float2 Screen_Position1254 = ( Screen_UV1145 * Screen_Resolution1113 );
				float2 screenPosition1243 = Screen_Position1254;
				float mulTime1247 = _TimeParameters.x * 60.0;
				float time1243 = mulTime1247;
				float3 localHash331243 = Hash33( screenPosition1243 , time1243 );
				float3 Sample_Noise1248 = ( (localHash331243*2.0 + -1.0) * _UVSampleNoise );
				float3 Noise_Base_UV220 = ( staticSwitch356 + Sample_Noise1248 );
				float localSpherize_float5_g491 = ( 0.0 );
				float2 uv5_g491 = (Noise_Base_UV220).xy;
				float2 center5_g491 = ( _SpherizeNoiseOffset + float2( 0.5,0.5 ) );
				float radius5_g491 = _SpherizeNoiseRadius;
				float strength5_g491 = _SpherizeNoiseStrength;
				float2 output5_g491 = float2( 0,0 );
				Spherize_float( uv5_g491 , center5_g491 , radius5_g491 , strength5_g491 , output5_g491 );
				float3 appendResult611 = (float3(output5_g491 , (Noise_Base_UV220).z));
				#ifdef _SPHERIZENOISE_ON
				float3 staticSwitch625 = appendResult611;
				#else
				float3 staticSwitch625 = Noise_Base_UV220;
				#endif
				float localTwistXZ_float11_g495 = ( 0.0 );
				float3 temp_output_10_0_g495 = staticSwitch625;
				float3 position11_g495 = temp_output_10_0_g495;
				float temp_output_9_0_g495 = _NoiseXZTwist;
				float angle11_g495 = radians( temp_output_9_0_g495 );
				float3 output11_g495 = float3( 0,0,0 );
				TwistXZ_float( position11_g495 , angle11_g495 , output11_g495 );
				#ifdef _NOISEXZTWISTENABLED_ON
				float3 staticSwitch801 = output11_g495;
				#else
				float3 staticSwitch801 = staticSwitch625;
				#endif
				float3 break728 = staticSwitch801;
				float temp_output_738_0 = ( ( break728.y - _NoiseUVYPreOffset ) * _NoiseUVYPreScale );
				float temp_output_7_0_g499 = abs( temp_output_738_0 );
				float temp_output_733_14 = ( pow( temp_output_7_0_g499 , _NoiseUVYPrePower ) * sign( temp_output_738_0 ) );
				float3 appendResult732 = (float3(break728.x , temp_output_733_14 , break728.z));
				float3 ase_viewVectorWS = ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - ase_positionWS : UNITY_MATRIX_V[ 2 ].xyz );
				float3 ase_viewDirWS = normalize( ase_viewVectorWS );
				float3 temp_output_872_0 = ( -ase_viewDirWS * _NoiseParallaxOffset );
				#ifdef _SWAPUVXY_ON
				float3 staticSwitch464 = (temp_output_872_0).yxz;
				#else
				float3 staticSwitch464 = temp_output_872_0;
				#endif
				float3 Parallax_Offset465 = staticSwitch464;
				float localSimplexNoise_float2_g493 = ( 0.0 );
				float Particle_Stable_Random_X147 = ( ( input.ase_texcoord2.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float Particle_Age_Percent146 = input.ase_texcoord2.z;
				float4 Distortion_Noise_Offset163 = ( _NoiseDistortionOffset + Particle_Stable_Random_X147 + ( _NoiseDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g489 = ( float4( ( ( Noise_Base_UV220 + Parallax_Offset465 ) * _NoiseDistortionScale * _NoiseDistortionTiling ) , 0.0 ) - ( Distortion_Noise_Offset163 + ( _NoiseDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_160_0 = (temp_output_10_0_g489).xyz;
				float3 position2_g493 = temp_output_160_0;
				float temp_output_160_15 = (temp_output_10_0_g489).w;
				float angle2_g493 = temp_output_160_15;
				float octaves2_g493 = _NoiseDistortionOctaves;
				float noise2_g493 = 0.0;
				float3 gradient2_g493 = float3( 0,0,0 );
				SimplexNoise_float( position2_g493 , angle2_g493 , octaves2_g493 , noise2_g493 , gradient2_g493 );
				float localSimplexNoise_Caustics_float2_g492 = ( 0.0 );
				float3 position2_g492 = temp_output_160_0;
				float angle2_g492 = temp_output_160_15;
				float octaves2_g492 = _NoiseDistortionOctaves;
				float gradientStrength2_g492 = _NoiseDistortionDilation;
				float noise2_g492 = 0.0;
				float3 gradient2_g492 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g492 , angle2_g492 , octaves2_g492 , gradientStrength2_g492 , noise2_g492 , gradient2_g492 );
				#ifdef _NOISEDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch440 = gradient2_g492;
				#else
				float3 staticSwitch440 = gradient2_g493;
				#endif
				float3 temp_output_7_0_g497 = abs( staticSwitch440 );
				float3 temp_cast_2 = (_NoiseDistortionPower).xxx;
				#ifdef _NOISEDISTORTIONENABLED_ON
				float3 staticSwitch442 = ( ( pow( temp_output_7_0_g497 , temp_cast_2 ) * sign( staticSwitch440 ) ) * _NoiseDistortion );
				#else
				float3 staticSwitch442 = float3( 0,0,0 );
				#endif
				float3 Noise_Distortion73 = staticSwitch442;
				float3 Noise_UV173 = ( appendResult732 + Parallax_Offset465 + Noise_Distortion73 );
				float4 Noise_Offset175 = ( _NoiseOffset + Particle_Stable_Random_X147 + ( _NoiseParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g503 = ( float4( ( Noise_UV173 * _NoiseScale * _NoiseTiling ) , 0.0 ) - ( Noise_Offset175 + ( _NoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_159_0 = (temp_output_10_0_g503).xyz;
				float3 position2_g507 = temp_output_159_0;
				float temp_output_159_15 = (temp_output_10_0_g503).w;
				float angle2_g507 = temp_output_159_15;
				float octaves2_g507 = _NoiseOctaves;
				float noise2_g507 = 0.0;
				float3 gradient2_g507 = float3( 0,0,0 );
				SimplexNoise_float( position2_g507 , angle2_g507 , octaves2_g507 , noise2_g507 , gradient2_g507 );
				float localSimplexNoise_Caustics_float2_g506 = ( 0.0 );
				float3 position2_g506 = temp_output_159_0;
				float angle2_g506 = temp_output_159_15;
				float octaves2_g506 = _NoiseOctaves;
				float gradientStrength2_g506 = _NoiseDilation;
				float noise2_g506 = 0.0;
				float3 gradient2_g506 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g506 , angle2_g506 , octaves2_g506 , gradientStrength2_g506 , noise2_g506 , gradient2_g506 );
				#ifdef _NOISEDILATIONENABLED_ON
				float staticSwitch444 = noise2_g506;
				#else
				float staticSwitch444 = noise2_g507;
				#endif
				float temp_output_20_0_g574 = staticSwitch444;
				float temp_output_4_0_g574 = _NoisePower;
				float smoothstepResult22_g574 = smoothstep( temp_output_7_0_g574 , temp_output_23_0_g574 , pow( temp_output_20_0_g574 , temp_output_4_0_g574 ));
				float Particle_Subtract_Noise_over_Lifetime480 = ( input.ase_texcoord5.y * _ParticleSubtractNoiseoverLifetime );
				float temp_output_478_0 = ( smoothstepResult22_g574 - Particle_Subtract_Noise_over_Lifetime480 );
				float lerpResult569 = lerp( 1.0 , temp_output_478_0 , _Noise);
				float Noise11 = lerpResult569;
				float Particle_Mask_Radius_over_Lifetime161 = input.ase_texcoord5.x;
				float lerpResult183 = lerp( 1.0 , Particle_Mask_Radius_over_Lifetime161 , _RadialMaskRadiusOverParticleLifetime);
				float temp_output_6_0_g500 = ( 1.0 - ( _RadialMaskRadius * lerpResult183 ) );
				float lerpResult5_g500 = lerp( temp_output_6_0_g500 , 1.0 , _RadialMaskFeather);
				float2 texCoord330 = input.ase_texcoord2.xy * float2( 2,2 ) + float2( -1,-1 );
				#ifdef _SWAPUVXY_ON
				float2 staticSwitch329 = (texCoord330).yx;
				#else
				float2 staticSwitch329 = texCoord330;
				#endif
				float2 UV_2D_Centered328 = staticSwitch329;
				float localSimplexNoise_float2_g486 = ( 0.0 );
				float4 Mask_Distortion_Noise_Offset171 = ( _RadialMaskDistortionOffset + Particle_Stable_Random_X147 + ( _RadialMaskDistortionParticleAnimation * Particle_Age_Percent146 ) );
				float4 temp_output_10_0_g483 = ( float4( ( Noise_Base_UV220 * _RadialMaskDistortionScale * _RadialMaskDistortionTiling ) , 0.0 ) - ( Mask_Distortion_Noise_Offset171 + ( _RadialMaskDistortionAnimation * _TimeParameters.x ) ) );
				float3 temp_output_158_0 = (temp_output_10_0_g483).xyz;
				float3 position2_g486 = temp_output_158_0;
				float temp_output_158_15 = (temp_output_10_0_g483).w;
				float angle2_g486 = temp_output_158_15;
				float octaves2_g486 = _RadialMaskDistortionOctaves;
				float noise2_g486 = 0.0;
				float3 gradient2_g486 = float3( 0,0,0 );
				SimplexNoise_float( position2_g486 , angle2_g486 , octaves2_g486 , noise2_g486 , gradient2_g486 );
				float localSimplexNoise_Caustics_float2_g485 = ( 0.0 );
				float3 position2_g485 = temp_output_158_0;
				float angle2_g485 = temp_output_158_15;
				float octaves2_g485 = _RadialMaskDistortionOctaves;
				float gradientStrength2_g485 = _RadialMaskDistortionDilation;
				float noise2_g485 = 0.0;
				float3 gradient2_g485 = float3( 0,0,0 );
				SimplexNoise_Caustics_float( position2_g485 , angle2_g485 , octaves2_g485 , gradientStrength2_g485 , noise2_g485 , gradient2_g485 );
				#ifdef _RADIALMASKDISTORTIONDILATIONENABLED_ON
				float3 staticSwitch441 = gradient2_g485;
				#else
				float3 staticSwitch441 = gradient2_g486;
				#endif
				float3 temp_output_7_0_g494 = abs( staticSwitch441 );
				float3 temp_cast_5 = (_RadialMaskDistortionPower).xxx;
				#ifdef _RADIALMASKDISTORTIONENABLED_ON
				float3 staticSwitch451 = ( ( pow( temp_output_7_0_g494 , temp_cast_5 ) * sign( staticSwitch441 ) ) * _RadialMaskDistortion );
				#else
				float3 staticSwitch451 = float3( 0,0,0 );
				#endif
				float3 Mask_Distortion117 = staticSwitch451;
				float temp_output_7_0_g500 = ( 1.0 - length( ( ( ( UV_2D_Centered328 + (Mask_Distortion117).xy ) - _RadialMaskOffset ) * _RadialMaskTiling ) ) );
				float smoothstepResult4_g500 = smoothstep( temp_output_6_0_g500 , lerpResult5_g500 , temp_output_7_0_g500);
				#ifdef _RADIALMASK_ON
				float staticSwitch601 = ( 1.0 - pow( smoothstepResult4_g500 , _RadialMaskPower ) );
				#else
				float staticSwitch601 = 0.0;
				#endif
				float Radial_Mask227 = staticSwitch601;
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch942 = Radial_Mask227;
				#else
				float staticSwitch942 = 0.0;
				#endif
				float temp_output_7_0_g505 = _VerticalMask1RemapMax;
				float temp_output_23_0_g505 = _VerticalMask1RemapMin;
				float UV_2D_Y691 = (staticSwitch321).y;
				float UV_3D_Y719 = (staticSwitch339).y;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch716 = ( ( UV_3D_Y719 - _VerticalMask1ObjectSpaceOffset ) / _VerticalMask1ObjectSpaceScale );
				#else
				float staticSwitch716 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g505 = staticSwitch716;
				float smoothstepResult25_g505 = smoothstep( temp_output_7_0_g505 , temp_output_23_0_g505 , temp_output_20_0_g505);
				float temp_output_4_0_g505 = _VerticalMask1Power;
				float temp_output_862_0 = pow( smoothstepResult25_g505 , temp_output_4_0_g505 );
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch713 = ( 1.0 - temp_output_862_0 );
				#else
				float staticSwitch713 = temp_output_862_0;
				#endif
				float Vertical_Mask_1350 = staticSwitch713;
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch709 = ( staticSwitch942 + Vertical_Mask_1350 );
				#else
				float staticSwitch709 = staticSwitch942;
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch741 = staticSwitch709;
				#else
				float staticSwitch741 = staticSwitch942;
				#endif
				float temp_output_7_0_g508 = _VerticalMask2RemapMin;
				float temp_output_23_0_g508 = _VerticalMask2RemapMax;
				#ifdef _VERTICALMASKSOBJECTSPACE_ON
				float staticSwitch767 = ( ( UV_3D_Y719 - _VerticalMask2ObjectSpaceOffset ) / _VerticalMask2ObjectSpaceScale );
				#else
				float staticSwitch767 = UV_2D_Y691;
				#endif
				float temp_output_20_0_g508 = staticSwitch767;
				float smoothstepResult25_g508 = smoothstep( temp_output_7_0_g508 , temp_output_23_0_g508 , temp_output_20_0_g508);
				float temp_output_4_0_g508 = _VerticalMask2Power;
				float temp_output_863_0 = pow( smoothstepResult25_g508 , temp_output_4_0_g508 );
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch758 = ( 1.0 - temp_output_863_0 );
				#else
				float staticSwitch758 = temp_output_863_0;
				#endif
				float Vertical_Mask_2759 = staticSwitch758;
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch760 = ( staticSwitch741 + Vertical_Mask_2759 );
				#else
				float staticSwitch760 = staticSwitch741;
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch766 = staticSwitch760;
				#else
				float staticSwitch766 = staticSwitch741;
				#endif
				float fresnelNdotV232 = dot( NormalWS, ase_viewDirWS );
				float fresnelNode232 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV232, _FresnelMaskPower ) );
				float smoothstepResult234 = smoothstep( _FresnelMaskRemapMin , _FresnelMaskRemapMax , fresnelNode232);
				float lerpResult242 = lerp( 1.0 , smoothstepResult234 , _FresnelMask);
				float Fresnel_Mask233 = lerpResult242;
				float temp_output_7_0_g576 = 0.0;
				float temp_output_23_0_g576 = 1.0;
				float screenDepth186 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth186 = saturate( abs( ( screenDepth186 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				#ifdef _INVERTDEPTHFADE_ON
				float staticSwitch218 = ( 1.0 - distanceDepth186 );
				#else
				float staticSwitch218 = distanceDepth186;
				#endif
				float temp_output_20_0_g576 = staticSwitch218;
				float temp_output_4_0_g576 = _DepthFadePower;
				float smoothstepResult22_g576 = smoothstep( temp_output_7_0_g576 , temp_output_23_0_g576 , pow( temp_output_20_0_g576 , temp_output_4_0_g576 ));
				float temp_output_7_0_g575 = 0.0;
				float temp_output_23_0_g575 = 1.0;
				float screenDepth208 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth208 = saturate( abs( ( screenDepth208 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _SubtractiveDepthFade ) ) );
				float temp_output_20_0_g575 = ( 1.0 - distanceDepth208 );
				float temp_output_4_0_g575 = _SubtractiveDepthFadePower;
				float smoothstepResult22_g575 = smoothstep( temp_output_7_0_g575 , temp_output_23_0_g575 , pow( temp_output_20_0_g575 , temp_output_4_0_g575 ));
				float Depth_Fade188 = saturate( ( smoothstepResult22_g576 - smoothstepResult22_g575 ) );
				float temp_output_7_0_g577 = 0.0;
				float temp_output_23_0_g577 = 1.0;
				float eyeDepth = input.ase_texcoord4.w;
				float cameraDepthFade191 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g577 = saturate( cameraDepthFade191 );
				float temp_output_4_0_g577 = _CameraDepthFadePower;
				float smoothstepResult22_g577 = smoothstep( temp_output_7_0_g577 , temp_output_23_0_g577 , pow( temp_output_20_0_g577 , temp_output_4_0_g577 ));
				float Camera_Depth_Fade195 = smoothstepResult22_g577;
				float temp_output_7_0_g581 = _IntersectionHighlightRemapMin;
				float temp_output_23_0_g581 = _IntersectionHighlightRemapMax;
				float screenDepth1005 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth1005 = saturate( abs( ( screenDepth1005 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _IntersectionHighlight ) ) );
				float temp_output_20_0_g581 = ( 1.0 - distanceDepth1005 );
				float temp_output_4_0_g581 = _IntersectionHighlightPower;
				float smoothstepResult22_g581 = smoothstep( temp_output_7_0_g581 , temp_output_23_0_g581 , pow( temp_output_20_0_g581 , temp_output_4_0_g581 ));
				float Intersection_Highlight1011 = smoothstepResult22_g581;
				float Intersection_Highlight_Alpha1024 = _IntersectionHighlightColour.a;
				float localGetMainLightShadow_float1080 = ( 0.0 );
				float3 position1080 = ase_positionWS;
				float2 screenPosition1080 = Screen_Position1254;
				float worldNoise1080 = _MainLightShadowWorldNoise;
				float lightDirectionNoise1080 = _MainLightShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1080 = _MainLightShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1080 = _MainLightShadowDirectionalAlignmentNoise;
				float3 normal1080 = NormalWS;
				float3x3 ase_worldToTangent = float3x3( TangentWS, BitangentWS, NormalWS );
				float3x3 worldToTangentMatrix1080 = ase_worldToTangent;
				float output1080 = 0.0;
				GetMainLightShadow_float( position1080 , screenPosition1080 , worldNoise1080 , lightDirectionNoise1080 , lightDirectionOrthogonalAlignmentNoise1080 , lightDirectionAlignmentNoise1080 , normal1080 , worldToTangentMatrix1080 , output1080 );
				float lerpResult984 = lerp( 1.0 , output1080 , _MainLightTransparencyShadow);
				float Main_Light_Transparency_Shadow981 = lerpResult984;
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1076 = Main_Light_Transparency_Shadow981;
				#else
				float staticSwitch1076 = 1.0;
				#endif
				float localGetAdditionalLightsShadow_float1125 = ( 0.0 );
				float3 position1125 = ase_positionWS;
				float2 screenPosition1125 = Screen_Position1254;
				float2 screenUV1125 = Screen_UV1145;
				float worldNoise1125 = _AdditionalLightsShadowWorldNoise;
				float lightDirectionNoise1125 = _AdditionalLightsShadowDirectionalNoise;
				float lightDirectionOrthogonalAlignmentNoise1125 = _AdditionalLightsShadowOrthogonalAlignmentWorldNoise;
				float lightDirectionAlignmentNoise1125 = _AdditionalLightsShadowDirectionalAlignmentNoise;
				float3 normal1125 = NormalWS;
				float3x3 worldToTangentMatrix1125 = ase_worldToTangent;
				float output1125 = 0.0;
				GetAdditionalLightsShadow_float( position1125 , screenPosition1125 , screenUV1125 , worldNoise1125 , lightDirectionNoise1125 , lightDirectionOrthogonalAlignmentNoise1125 , lightDirectionAlignmentNoise1125 , normal1125 , worldToTangentMatrix1125 , output1125 );
				float lerpResult993 = lerp( 1.0 , output1125 , _AdditionalLightsTransparencyShadow);
				float Additional_Lights_Transparency_Shadow994 = lerpResult993;
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch1077 = Additional_Lights_Transparency_Shadow994;
				#else
				float staticSwitch1077 = 1.0;
				#endif
				float temp_output_1030_0 = saturate( ( ( saturate( ( Noise11 - staticSwitch766 ) ) * Fresnel_Mask233 * (input.ase_color).a * Depth_Fade188 * Camera_Depth_Fade195 * _Alpha ) + ( Intersection_Highlight1011 * Intersection_Highlight_Alpha1024 * pow( ( staticSwitch1076 * staticSwitch1077 ) , _IntersectionHighlightShadowMaskPower ) ) ) );
				#ifdef _RADIALMASKSUBTRACTIVE_ON
				float staticSwitch943 = temp_output_1030_0;
				#else
				float staticSwitch943 = ( temp_output_1030_0 * ( 1.0 - Radial_Mask227 ) );
				#endif
				#ifdef _VERTICALMASK1SUBTRACTIVE_ON
				float staticSwitch711 = staticSwitch943;
				#else
				float staticSwitch711 = ( staticSwitch943 * Vertical_Mask_1350 );
				#endif
				#ifdef _VERTICALMASK1_ON
				float staticSwitch744 = staticSwitch711;
				#else
				float staticSwitch744 = staticSwitch943;
				#endif
				#ifdef _VERTICALMASK2SUBTRACTIVE_ON
				float staticSwitch763 = staticSwitch744;
				#else
				float staticSwitch763 = ( staticSwitch744 * Vertical_Mask_2759 );
				#endif
				#ifdef _VERTICALMASK2_ON
				float staticSwitch768 = staticSwitch763;
				#else
				float staticSwitch768 = staticSwitch744;
				#endif
				#ifdef _MAINLIGHTTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch985 = ( staticSwitch768 * Main_Light_Transparency_Shadow981 );
				#else
				float staticSwitch985 = staticSwitch768;
				#endif
				#ifdef _ADDITIONALLIGHTSTRANSPARENCYSHADOWENABLED_ON
				float staticSwitch998 = ( staticSwitch985 * Additional_Lights_Transparency_Shadow994 );
				#else
				float staticSwitch998 = staticSwitch985;
				#endif
				float Alpha49 = staticSwitch998;
				

				float3 Normal = float3(0, 0, 1);
				float Alpha = Alpha49;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(NormalWS);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = NormalWS;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}
			ENDHLSL
		}

	
	}
	

	

	CustomEditor "Mirza.Solaris.SolarisGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19912
{"type":"AmplifyShaderEditor.ScreenPosInputsNode, AmplifyShaderEditor","id":1144,"pos":[-9984,3584],"params":["Float","False","0","False","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.ScreenParams, AmplifyShaderEditor","id":1111,"pos":[-9984,3840],"params":["Inherit","False","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":1146,"pos":[-9728,3584],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":1112,"pos":[-9728,3840],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1145,"pos":[-9472,3584],"params":["Inherit","False","Screen UV","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1113,"pos":[-9472,3840],"params":["Inherit","False","Screen Resolution","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1256,"pos":[-9088,3584],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1254,"pos":[-8832,3584],"params":["Inherit","False","Screen Position","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":322,"pos":[-7296,-2816],"params":["Inherit","False","0","-1","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor","id":145,"pos":[-3328,-3328],"params":["Inherit","False","0","4","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor","id":341,"pos":[-7296,-2176],"params":["Inherit","False","0","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":338,"pos":[-6912,-2048],"params":["Inherit","False","FLOAT3","1","0","2","3","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":316,"pos":[-6912,-2688],"params":["Inherit","False","FLOAT2","1","0","2","3","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1257,"pos":[-9088,-2816],"params":["Inherit","False","1254","Screen Position","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor","id":1247,"pos":[-9088,-2688],"params":["Inherit","False","1","0","FLOAT","60","False","5","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":156,"pos":[-2944,-3200],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":477,"pos":[-2944,-3072],"params":["Inherit","False","Property","_ParticleRandomization","粒子随机化","42","0","Create","False","0","0","0","False","2","Header(Particle Settings)","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":623,"pos":[-7296,-1920],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":620,"pos":[-6912,-1792],"params":["Inherit","False","FLOAT3","1","0","2","3","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":339,"pos":[-6656,-2176],"params":["Inherit","False","Property","_SwapUVXY2","交换 UV XY","36","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","321","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1243,"pos":[-8832,-2816],"params":["Inherit","False"," ","3","File","2","True","screenPosition","FLOAT2","0,0","In","","Inherit","False","True","time","FLOAT","0","In","","Inherit","False","Hash33","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","2","0","FLOAT2","0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":157,"pos":[-2560,-3200],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT","100","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":146,"pos":[-2944,-3328],"params":["Inherit","False","Particle Age Percent","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":168,"pos":[-5376,-1232],"params":["Inherit","False","146","Particle Age Percent","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":321,"pos":[-6656,-2816],"params":["Inherit","False","Property","_SwapUVXY","交换 UV XY","36","0","Create","False","0","0","0","False","2","Header(Base UVs)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":170,"pos":[-5376,-1408],"params":["Inherit","False","Property","_RadialMaskDistortionParticleAnimation","径向遮罩扭曲粒子动画","121","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":340,"pos":[-6016,-2176],"params":["Inherit","False","UV 3D","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":327,"pos":[-6016,-2816],"params":["Inherit","False","UV 2D","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":621,"pos":[-6656,-1920],"params":["Inherit","False","Property","_SwapUVXY4","交换 UV XY","36","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","321","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ScaleAndOffsetNode, AmplifyShaderEditor","id":1251,"pos":[-8448,-2816],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT","2","False","2","FLOAT","-1","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":147,"pos":[-2304,-3200],"params":["Inherit","False","Particle Stable Random X","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":151,"pos":[-5376,-1488],"params":["Inherit","False","147","Particle Stable Random X","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":169,"pos":[-4992,-1408],"params":["Inherit","False","2","2","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":165,"pos":[-5376,-2000],"params":["Inherit","False","146","Particle Age Percent","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1259,"pos":[-8448,-2688],"params":["Inherit","False","Property","_UVSampleNoise","UV 采样噪波","39","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":167,"pos":[-5376,-2176],"params":["Inherit","False","Property","_NoiseDistortionParticleAnimation","噪波扭曲粒子动画","98","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":109,"pos":[-5376,-1664],"params":["Inherit","False","Property","_RadialMaskDistortionOffset","径向遮罩扭曲偏移","122","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":622,"pos":[-5760,-1920],"params":["Inherit","False","UV 3D World","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":342,"pos":[-7296,-1536],"params":["Inherit","False","340","UV 3D","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":332,"pos":[-7296,-1664],"params":["Inherit","False","327","UV 2D","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1258,"pos":[-8064,-2816],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":150,"pos":[-4736,-1664],"params":["Inherit","False","3","3","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","2","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":149,"pos":[-5376,-2256],"params":["Inherit","False","147","Particle Stable Random X","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":166,"pos":[-4992,-2176],"params":["Inherit","False","2","2","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":82,"pos":[-5376,-2432],"params":["Inherit","False","Property","_NoiseDistortionOffset","噪波扭曲偏移","99","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":624,"pos":[-7040,-1536],"params":["Inherit","False","622","UV 3D World","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1248,"pos":[-7808,-2816],"params":["Inherit","False","Sample Noise","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":171,"pos":[-4480,-1664],"params":["Inherit","False","Mask Distortion Noise Offset","-1","True","1","0","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":148,"pos":[-4736,-2432],"params":["Inherit","False","3","3","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","2","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":223,"pos":[-7040,-1664],"params":["Inherit","False","Property","_ObjectSpaceUVs","物体空间 UV","38","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1268,"pos":[-6656,-1536],"params":["Inherit","False","1248","Sample Noise","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":172,"pos":[-4096,-1104],"params":["Inherit","False","171","Mask Distortion Noise Offset","1","0","OBJECT","","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":323,"pos":[-4096,-1664],"params":["Inherit","False","220","Noise Base UV","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":163,"pos":[-4480,-2432],"params":["Inherit","False","Distortion Noise Offset","-1","True","1","0","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":476,"pos":[-4096,-2352],"params":["Inherit","False","465","Parallax Offset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":324,"pos":[-4096,-2432],"params":["Inherit","False","220","Noise Base UV","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":356,"pos":[-6656,-1664],"params":["Inherit","False","Property","_WorldSpaceUVs","世界空间 UV","37","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":106,"pos":[-4096,-1536],"params":["Inherit","False","Property","_RadialMaskDistortionScale","径向遮罩扭曲缩放","118","0","Create","False","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":107,"pos":[-4096,-1456],"params":["Inherit","False","Property","_RadialMaskDistortionTiling","径向遮罩扭曲平铺","119","0","Create","False","0","0","0","False","0","False","Object","-1","","1.5,1,1","1,1,1","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":108,"pos":[-4096,-1280],"params":["Inherit","False","Property","_RadialMaskDistortionAnimation","径向遮罩扭曲动画","120","0","Create","False","0","0","0","False","0","False","Object","-1","","0,2,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1267,"pos":[-6272,-1664],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":158,"pos":[-3328,-1664],"params":["Inherit","False","Scale Tiling Offset Animation","-1","","483","650501f4d90f3194eb72a847e06cc2e3","1,21,0","6","4","FLOAT3","0,0,0","False","7","FLOAT","1","False","8","FLOAT3","1,1,1","False","9","FLOAT4","0,0,0,0","False","19","INT","0","False","12","FLOAT4","0,0,0,0","False","2","FLOAT3","0","FLOAT","15"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":475,"pos":[-3712,-2432],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":164,"pos":[-4096,-1840],"params":["Inherit","False","163","Distortion Noise Offset","1","0","OBJECT","","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":80,"pos":[-4096,-2272],"params":["Inherit","False","Property","_NoiseDistortionScale","噪波扭曲缩放","95","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":84,"pos":[-4096,-2192],"params":["Inherit","False","Property","_NoiseDistortionTiling","噪波扭曲平铺","96","0","Create","False","0","0","0","False","0","False","Object","-1","","1.5,1,1","1,1,1","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":83,"pos":[-4096,-2016],"params":["Inherit","False","Property","_NoiseDistortionAnimation","噪波扭曲动画","97","0","Create","False","0","0","0","False","0","False","Object","-1","","0,1,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":111,"pos":[-3328,-1488],"params":["Inherit","False","Property","_RadialMaskDistortionOctaves","径向遮罩扭曲倍频程","123","1","[IntRange]","Create","False","0","0","0","False","0","False","Object","-1","","1","1","1","8","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":112,"pos":[-3328,-1408],"params":["Inherit","False","Property","_RadialMaskDistortionDilation","径向遮罩扭曲膨胀","124","0","Create","False","0","0","0","False","0","False","Object","-1","","0.004","0.002","0","0.1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":137,"pos":[-9856,-512],"params":["Inherit","False","Property","_SpherizeNoiseOffset","球形化噪波偏移","149","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":220,"pos":[-6016,-1664],"params":["Inherit","False","Noise Base UV","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":118,"pos":[-2816,-1664],"params":["Inherit","False","Simplex Noise Caustics","-1","","485","477e7c249263854458b4f42934448d42","0","4","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","9","FLOAT","0.01","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":443,"pos":[-2816,-1792],"params":["Inherit","False","Simplex Noise","-1","","486","c68ae2e20c00ec54aaecd9d04797372e","0","3","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":612,"pos":[-9600,-768],"params":["Inherit","False","True","True","False","False","1","0","FLOAT3","0,0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":613,"pos":[-9600,-640],"params":["Inherit","False","False","False","True","False","1","0","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":629,"pos":[-9600,-512],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":160,"pos":[-3328,-2432],"params":["Inherit","False","Scale Tiling Offset Animation","-1","","489","650501f4d90f3194eb72a847e06cc2e3","1,21,0","6","4","FLOAT3","0,0,0","False","7","FLOAT","1","False","8","FLOAT3","1,1,1","False","9","FLOAT4","0,0,0,0","False","19","INT","0","False","12","FLOAT4","0,0,0,0","False","2","FLOAT3","0","FLOAT","15"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":86,"pos":[-3328,-2256],"params":["Inherit","False","Property","_NoiseDistortionOctaves","噪波扭曲倍频程","100","1","[IntRange]","Create","False","0","0","0","False","0","False","Object","-1","","1","1","1","8","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":87,"pos":[-3328,-2176],"params":["Inherit","False","Property","_NoiseDistortionDilation","噪波扭曲膨胀","101","0","Create","False","0","0","0","False","0","False","Object","-1","","0.004","0.002","0","0.1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":61,"pos":[-9600,-384],"params":["Inherit","False","Property","_SpherizeNoiseRadius","球形化噪波半径","147","0","Create","False","0","0","0","False","0","False","Object","-1","","0.5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":62,"pos":[-9600,-304],"params":["Inherit","False","Property","_SpherizeNoiseStrength","球形化噪波强度","148","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":447,"pos":[-9984,-768],"params":["Inherit","False","220","Noise Base UV","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":617,"pos":[-9104,-528],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":66,"pos":[-9216,-768],"params":["Inherit","False","Spherize","-1","","491","dce7577f44cbfeb4c822afd6b5c80507","0","4","7","FLOAT2","0,0","False","6","FLOAT2","0,0","False","8","FLOAT","1","False","9","FLOAT","0.5","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":89,"pos":[-2816,-2432],"params":["Inherit","False","Simplex Noise Caustics","-1","","492","477e7c249263854458b4f42934448d42","0","4","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","9","FLOAT","0.01","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":439,"pos":[-2816,-2560],"params":["Inherit","False","Simplex Noise","-1","","493","c68ae2e20c00ec54aaecd9d04797372e","0","3","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":441,"pos":[-2432,-1664],"params":["Inherit","False","Property","_RadialMaskDistortionDilationEnabled","启用径向遮罩扭曲膨胀","125","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":119,"pos":[-2432,-1536],"params":["Inherit","False","Property","_RadialMaskDistortionPower","径向遮罩扭曲幂次","126","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":437,"pos":[-1920,-1664],"params":["Inherit","False","Signed Power Smoothstep","-1","","494","3654d4d5f7b612d4085eb90cd7a60668","3,3,2,20,1,15,0","7","2","FLOAT","0","False","4","FLOAT2","0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT4","0,0,0,0","False","12","FLOAT","1","False","17","FLOAT","0","False","18","FLOAT","1","False","1","FLOAT3","14"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":611,"pos":[-8832,-768],"params":["Inherit","False","FLOAT3","4","0","FLOAT2","0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":628,"pos":[-9616,-912],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":798,"pos":[-8576,-768],"params":["Inherit","False","Property","_NoiseXZTwist","噪波 XZ 扭曲","83","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","-360","360","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":440,"pos":[-2432,-2432],"params":["Inherit","False","Property","_NoiseDistortionDilationEnabled","启用噪波扭曲膨胀","102","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":88,"pos":[-2432,-2304],"params":["Inherit","False","Property","_NoiseDistortionPower","噪波扭曲幂次","103","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":97,"pos":[-1920,-1536],"params":["Inherit","False","Property","_RadialMaskDistortion","径向遮罩扭曲","116","0","Create","False","0","0","0","False","2","Header(Radial Mask Distortion)","Space(5)","False","Object","-1","","0.05","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":116,"pos":[-1536,-1664],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":330,"pos":[-7296,-2432],"params":["Inherit","False","0","-1","2","3","2","SAMPLER2D","","False","0","FLOAT2","2,2","False","1","FLOAT2","-1,-1","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":851,"pos":[-8192,-768],"params":["Inherit","False","TwistXZ","-1","","495","9581222175ed3d74faf64569d7d97396","1,12,0","2","10","FLOAT3","0,0,0","False","9","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":436,"pos":[-1920,-2432],"params":["Inherit","False","Signed Power Smoothstep","-1","","497","3654d4d5f7b612d4085eb90cd7a60668","3,3,2,20,1,15,0","7","2","FLOAT","0","False","4","FLOAT2","0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT4","0,0,0,0","False","12","FLOAT","1","False","17","FLOAT","0","False","18","FLOAT","1","False","1","FLOAT3","14"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":71,"pos":[-7040,-1280],"params":["Inherit","False","Property","_NoiseParallaxOffset","噪波视差偏移","80","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":737,"pos":[-7680,-768],"params":["Inherit","False","Property","_NoiseUVYPreOffset","噪波 UV-Y 预偏移","85","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":103,"pos":[-1920,-2304],"params":["Inherit","False","Property","_NoiseDistortion","噪波扭曲","93","1","[Header]","Create","False","0","0","0","False","2","Header(Noise Distortion)","Space(5)","False","Object","-1","","0.05","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":625,"pos":[-8576,-896],"params":["Inherit","False","Property","_SpherizeNoise","球形化噪波","146","0","Create","False","0","0","0","False","2","Header(Spherize Noise)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":872,"pos":[-6784,-1280],"params":["Inherit","False","Parallax Offset","-1","","498","66d259709a71255489a93d3df825942b","3,20,0,16,1,9,1","1","13","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":463,"pos":[-6528,-1152],"params":["Inherit","False","FLOAT3","1","0","2","3","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor","id":331,"pos":[-6912,-2304],"params":["Inherit","False","FLOAT2","1","0","2","3","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":94,"pos":[-1536,-2432],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":736,"pos":[-7168,-896],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":801,"pos":[-7808,-896],"params":["Inherit","False","Property","_NoiseXZTwistEnabled","启用噪波 XZ 扭曲","84","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":740,"pos":[-7680,-608],"params":["Inherit","False","Property","_NoiseUVYPreScale","噪波 UV-Y 预缩放","86","0","Create","False","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":451,"pos":[-1280,-1664],"params":["Inherit","False","Property","_RadialMaskDistortionEnabled","启用径向遮罩扭曲","117","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":464,"pos":[-6144,-1280],"params":["Inherit","False","Property","_SwapUVXY3","交换 UV XY","36","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","321","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":117,"pos":[-896,-1664],"params":["Inherit","False","Mask Distortion","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":329,"pos":[-6656,-2432],"params":["Inherit","False","Property","_SwapUVXY1","交换 UV XY","36","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","321","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":728,"pos":[-7424,-896],"params":["Inherit","False","FLOAT3","1","0","FLOAT3","0,0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":738,"pos":[-6912,-896],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":730,"pos":[-7680,-688],"params":["Inherit","False","Property","_NoiseUVYPrePower","噪波 UV-Y 预幂次","87","0","Create","False","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":442,"pos":[-1280,-2432],"params":["Inherit","False","Property","_NoiseDistortionEnabled","启用噪波扭曲","94","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":465,"pos":[-5888,-1280],"params":["Inherit","False","Parallax Offset","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":328,"pos":[-6016,-2432],"params":["Inherit","False","UV 2D Centered","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":102,"pos":[-4736,2816],"params":["Inherit","False","117","Mask Distortion","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":718,"pos":[-6016,-2048],"params":["Inherit","False","False","True","False","True","1","0","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":177,"pos":[-5376,-464],"params":["Inherit","False","146","Particle Age Percent","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":73,"pos":[-896,-2432],"params":["Inherit","False","Noise Distortion","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":735,"pos":[-6672,-784],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":733,"pos":[-6656,-896],"params":["Inherit","False","Signed Power Smoothstep","-1","","499","3654d4d5f7b612d4085eb90cd7a60668","3,3,0,20,1,15,0","7","2","FLOAT","0","False","4","FLOAT2","0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT4","0,0,0,0","False","12","FLOAT","1","False","17","FLOAT","0","False","18","FLOAT","1","False","1","FLOAT","14"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":734,"pos":[-6672,-1040],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":179,"pos":[-5376,-640],"params":["Inherit","False","Property","_NoiseParticleAnimation","噪波粒子动画","72","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":161,"pos":[-2688,-2944],"params":["Inherit","False","Particle Mask Radius over Lifetime","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":343,"pos":[-4736,2688],"params":["Inherit","False","328","UV 2D Centered","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":122,"pos":[-4480,2816],"params":["Inherit","False","True","True","False","True","1","0","FLOAT3","0,0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":719,"pos":[-5760,-2048],"params":["Inherit","False","UV 3D Y","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":153,"pos":[-5376,-720],"params":["Inherit","False","147","Particle Stable Random X","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":178,"pos":[-4992,-640],"params":["Inherit","False","2","2","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":732,"pos":[-6272,-896],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":95,"pos":[-6272,-688],"params":["Inherit","False","73","Noise Distortion","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":466,"pos":[-6272,-768],"params":["Inherit","False","465","Parallax Offset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":19,"pos":[-5376,-896],"params":["Inherit","False","Property","_NoiseOffset","噪波偏移","73","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":99,"pos":[-4096,2688],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":181,"pos":[-4352,3024],"params":["Inherit","False","161","Particle Mask Radius over Lifetime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":690,"pos":[-6016,-2688],"params":["Inherit","False","False","True","True","True","1","0","FLOAT2","0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":717,"pos":[-8192,2304],"params":["Inherit","False","719","UV 3D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":152,"pos":[-4736,-896],"params":["Inherit","False","3","3","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","2","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":70,"pos":[-5888,-896],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":126,"pos":[-4096,2816],"params":["Inherit","False","Property","_RadialMaskOffset","径向遮罩偏移","113","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":722,"pos":[-8192,2384],"params":["Inherit","False","Property","_VerticalMask1ObjectSpaceOffset","垂直遮罩 1 物体空间偏移","136","0","Create","False","0","0","0","False","0","False","Object","-1","","-1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":184,"pos":[-4352,3104],"params":["Inherit","False","Property","_RadialMaskRadiusOverParticleLifetime","径向遮罩粒子生命周期半径","109","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":125,"pos":[-3840,2688],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":183,"pos":[-3872,3024],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":691,"pos":[-5760,-2688],"params":["Inherit","False","UV 2D Y","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":726,"pos":[-7808,2304],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":746,"pos":[-8192,2688],"params":["Inherit","False","719","UV 3D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":175,"pos":[-4480,-896],"params":["Inherit","False","Noise Offset","-1","True","1","0","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":173,"pos":[-5760,-896],"params":["Inherit","False","Noise UV","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":120,"pos":[-4352,2944],"params":["Inherit","False","Property","_RadialMaskRadius","径向遮罩半径","108","0","Create","False","0","0","0","False","1","Space(10)","False","Object","-1","","1","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":57,"pos":[-4352,3200],"params":["Inherit","False","Property","_RadialMaskFeather","径向遮罩羽化","110","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":123,"pos":[-3840,2816],"params":["Inherit","False","Property","_RadialMaskTiling","径向遮罩平铺","112","0","Create","False","0","0","0","False","0","False","Object","-1","","1.5,1","1,1","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":724,"pos":[-8192,2464],"params":["Inherit","False","Property","_VerticalMask1ObjectSpaceScale","垂直遮罩 1 物体空间缩放","135","0","Create","False","0","0","0","False","0","False","Object","-1","","2","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":747,"pos":[-8192,2768],"params":["Inherit","False","Property","_VerticalMask2ObjectSpaceOffset","垂直遮罩 2 物体空间偏移","143","0","Create","False","0","0","0","False","0","False","Object","-1","","-1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":121,"pos":[-3584,2688],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":180,"pos":[-3584,2944],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":185,"pos":[-3472,3056],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":699,"pos":[-7936,2176],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":725,"pos":[-7552,2304],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":749,"pos":[-7808,2688],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":176,"pos":[-4096,-336],"params":["Inherit","False","175","Noise Offset","1","0","OBJECT","","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":174,"pos":[-4096,-896],"params":["Inherit","False","173","Noise UV","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":16,"pos":[-4096,-768],"params":["Inherit","False","Property","_NoiseScale","噪波缩放","69","0","Create","False","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":72,"pos":[-4096,-688],"params":["Inherit","False","Property","_NoiseTiling","噪波平铺","70","0","Create","False","0","0","0","False","0","False","Object","-1","","1.5,1,1","1,1,1","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":18,"pos":[-4096,-512],"params":["Inherit","False","Property","_NoiseAnimation","噪波动画","71","0","Create","False","0","0","0","False","0","False","Object","-1","","0,4,1,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":748,"pos":[-8192,2848],"params":["Inherit","False","Property","_VerticalMask2ObjectSpaceScale","垂直遮罩 2 物体空间缩放","142","0","Create","False","0","0","0","False","0","False","Object","-1","","2","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor","id":162,"pos":[-3328,-2944],"params":["Inherit","False","1","4","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":53,"pos":[-3200,2688],"params":["Inherit","True","Radial Gradient 2","-1","","500","969db7e12a1ad8c4c8b8d89670372700","1,12,1","3","10","FLOAT2","0,0","False","8","FLOAT","1","False","9","FLOAT","0.2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":750,"pos":[-7936,2560],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":751,"pos":[-7552,2688],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":159,"pos":[-3328,-896],"params":["Inherit","False","Scale Tiling Offset Animation","-1","","503","650501f4d90f3194eb72a847e06cc2e3","1,21,0","6","4","FLOAT3","0,0,0","False","7","FLOAT","1","False","8","FLOAT3","1,1,1","False","9","FLOAT4","0,0,0,0","False","19","INT","0","False","12","FLOAT4","0,0,0,0","False","2","FLOAT3","0","FLOAT","15"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":13,"pos":[-3328,-720],"params":["Inherit","False","Property","_NoiseOctaves","噪波倍频程","74","1","[IntRange]","Create","False","0","0","0","False","0","False","Object","-1","","1","1","1","8","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":14,"pos":[-3328,-640],"params":["Inherit","False","Property","_NoiseDilation","噪波膨胀","75","0","Create","False","0","0","0","False","0","False","Object","-1","","0.004","0.002","0","0.1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":567,"pos":[-3200,2944],"params":["Inherit","False","Property","_RadialMaskPower","径向遮罩幂次","111","0","Create","False","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":716,"pos":[-7296,2176],"params":["Inherit","False","Property","_VerticalMasksObjectSpace","垂直遮罩物体空间","129","0","Create","False","0","0","0","False","2","Header(Vertical Mask)","Space(5)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":347,"pos":[-7296,2304],"params":["Inherit","False","Property","_VerticalMask1Power","垂直遮罩 1 幂次","132","0","Create","False","0","0","0","False","2","","Space(5)","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":345,"pos":[-7296,2384],"params":["Inherit","False","Property","_VerticalMask1RemapMin","垂直遮罩 1 重映射最小值","133","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":346,"pos":[-7296,2464],"params":["Inherit","False","Property","_VerticalMask1RemapMax","垂直遮罩 1 重映射最大值","134","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":485,"pos":[-3328,-2768],"params":["Inherit","False","Property","_ParticleSubtractNoiseoverLifetime","粒子随生命周期减去噪波","43","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":566,"pos":[-2816,2688],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":862,"pos":[-6784,2176],"params":["Inherit","True","Power Smoothstep","-1","","505","eaa8bfb6a4986cb418a1675cea297eed","1,24,1","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":742,"pos":[-6400,2304],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":767,"pos":[-7296,2560],"params":["Inherit","False","Property","_VerticalMaskObjectSpace1","垂直遮罩物体空间","129","0","Create","True","0","0","0","False","1","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","716","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":24,"pos":[-2816,-896],"params":["Inherit","False","Simplex Noise Caustics","-1","","506","477e7c249263854458b4f42934448d42","0","4","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","9","FLOAT","0.01","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":445,"pos":[-2816,-1024],"params":["Inherit","False","Simplex Noise","-1","","507","c68ae2e20c00ec54aaecd9d04797372e","0","3","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":484,"pos":[-2944,-2816],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":754,"pos":[-7296,2688],"params":["Inherit","False","Property","_VerticalMask2Power","垂直遮罩 2 幂次","139","0","Create","False","0","0","0","False","2","","Space(5)","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":753,"pos":[-7296,2848],"params":["Inherit","False","Property","_VerticalMask2RemapMax","垂直遮罩 2 重映射最大值","141","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":752,"pos":[-7296,2768],"params":["Inherit","False","Property","_VerticalMask2RemapMin","垂直遮罩 2 重映射最小值","140","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":187,"pos":[-7552,128],"params":["Inherit","False","Property","_DepthFade","深度淡出","158","0","Create","False","0","0","0","False","2","Header(Depth Fade)","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":55,"pos":[-2560,2688],"params":["Inherit","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":863,"pos":[-6784,2560],"params":["Inherit","True","Power Smoothstep","-1","","508","eaa8bfb6a4986cb418a1675cea297eed","1,24,1","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DepthFade, AmplifyShaderEditor","id":186,"pos":[-7152,128],"params":["Inherit","False","True","True","True","2","1","FLOAT3","0,0,0","False","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":480,"pos":[-2688,-2864],"params":["Inherit","False","Particle Subtract Noise over Lifetime","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":757,"pos":[-6400,2688],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":313,"pos":[-4608,6448],"params":["Inherit","False","146","Particle Age Percent","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":444,"pos":[-2432,-896],"params":["Inherit","False","Property","_NoiseDilationEnabled","启用噪波膨胀","76","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":22,"pos":[-2432,-768],"params":["Inherit","False","Property","_NoisePower","噪波幂次","77","0","Create","False","0","0","0","False","0","False","Object","-1","","0.5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":564,"pos":[-2432,-672],"params":["Inherit","False","Property","_NoiseRemapMin","噪波重映射最小值","78","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":565,"pos":[-2432,-592],"params":["Inherit","False","Property","_NoiseRemapMax","噪波重映射最大值","79","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":713,"pos":[-6144,2176],"params":["Inherit","False","Property","_VerticalMask1Subtractive","垂直遮罩 1 相减","131","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":210,"pos":[-7552,384],"params":["Inherit","False","Property","_SubtractiveDepthFade","相减深度淡出","161","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":314,"pos":[-4608,6272],"params":["Inherit","False","Property","_VertexNoiseParticleAnimation","顶点噪波粒子动画","206","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":350,"pos":[-5760,2176],"params":["Inherit","False","Vertical Mask 1","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":479,"pos":[-1920,-736],"params":["Inherit","False","480","Particle Subtract Noise over Lifetime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":219,"pos":[-6800,-16],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DepthFade, AmplifyShaderEditor","id":208,"pos":[-7152,384],"params":["Inherit","False","True","True","True","2","1","FLOAT3","0,0,0","False","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":217,"pos":[-6784,128],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":864,"pos":[-1920,-896],"params":["Inherit","False","Power Smoothstep","-1","","574","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":1081,"pos":[-9984,4864],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":1126,"pos":[-9984,6656],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":1115,"pos":[-9984,5488],"params":["Inherit","False","False","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.WorldToTangentMatrix, AmplifyShaderEditor","id":1088,"pos":[-9984,5648],"params":["Inherit","False","0","1","FLOAT3x3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1150,"pos":[-9984,5024],"params":["Inherit","False","1254","Screen Position","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1148,"pos":[-9984,6800],"params":["Inherit","False","1254","Screen Position","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1255,"pos":[-9984,6880],"params":["Inherit","False","1145","Screen UV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor","id":1133,"pos":[-9984,7280],"params":["Inherit","False","False","1","0","FLOAT3","0,0,1","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.WorldToTangentMatrix, AmplifyShaderEditor","id":1134,"pos":[-9984,7440],"params":["Inherit","False","0","1","FLOAT3x3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":310,"pos":[-4608,6192],"params":["Inherit","False","147","Particle Stable Random X","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":315,"pos":[-4224,6272],"params":["Inherit","False","2","2","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1085,"pos":[-9984,5120],"params":["Inherit","False","Property","_MainLightShadowWorldNoise","主光阴影世界噪波","12","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1089,"pos":[-9984,5200],"params":["Inherit","False","Property","_MainLightShadowDirectionalNoise","主光阴影方向噪波","13","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1123,"pos":[-9984,5280],"params":["Inherit","False","Property","_MainLightShadowOrthogonalAlignmentWorldNoise","主光阴影正交对齐世界噪波","14","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1110,"pos":[-9984,5360],"params":["Inherit","False","Property","_MainLightShadowDirectionalAlignmentNoise","主光阴影方向对齐噪波","15","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1129,"pos":[-9984,6960],"params":["Inherit","False","Property","_AdditionalLightsShadowWorldNoise","附加光阴影世界噪波","26","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1130,"pos":[-9984,7040],"params":["Inherit","False","Property","_AdditionalLightsShadowDirectionalNoise","附加光阴影方向噪波","27","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1131,"pos":[-9984,7120],"params":["Inherit","False","Property","_AdditionalLightsShadowOrthogonalAlignmentWorldNoise","附加光阴影正交对齐世界噪波","28","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1132,"pos":[-9984,7200],"params":["Inherit","False","Property","_AdditionalLightsShadowDirectionalAlignmentNoise","附加光阴影方向对齐噪波","29","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":601,"pos":[-2304,2688],"params":["Inherit","False","Property","_RadialMask","径向遮罩","106","0","Create","False","0","0","0","False","2","Header(Radial Mask)","Space(5)","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":758,"pos":[-6144,2560],"params":["Inherit","False","Property","_VerticalMask2Subtractive","垂直遮罩 2 相减","138","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":300,"pos":[-4608,6016],"params":["Inherit","False","Property","_VertexNoiseOffset","顶点噪波偏移","207","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":227,"pos":[-2048,2688],"params":["Inherit","False","Radial Mask","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":704,"pos":[-5120,4352],"params":["Inherit","False","350","Vertical Mask 1","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":478,"pos":[-1536,-896],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":211,"pos":[-6784,384],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":759,"pos":[-5760,2560],"params":["Inherit","False","Vertical Mask 2","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1125,"pos":[-9344,6656],"params":["Inherit","False","GetMainLightShadow_float","8","File","10","True","position","FLOAT3","0,0,0","In","","Inherit","False","True","screenPosition","FLOAT2","0,0","In","","Inherit","False","True","screenUV","FLOAT2","0,0","In","","Inherit","False","True","worldNoise","FLOAT","0","In","","Inherit","False","True","lightDirectionNoise","FLOAT","0","In","","Inherit","False","True","lightDirectionOrthogonalAlignmentNoise","FLOAT","0","In","","Inherit","False","True","lightDirectionAlignmentNoise","FLOAT","0","In","","Inherit","False","True","normal","FLOAT3","0,0,0","In","","Inherit","False","True","worldToTangentMatrix","FLOAT3x3","1,0,0,1,1,1,1,0,1","In","","Inherit","False","True","output","FLOAT","0","Out","","Inherit","False","GetAdditionalLightsShadow_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","11","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT3","0,0,0","False","9","FLOAT3x3","1,0,0,1,1,1,1,0,1","False","10","FLOAT","0","False","2","FLOAT","0","FLOAT","11"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1080,"pos":[-9344,4992],"params":["Inherit","False","GetMainLightShadow_float","8","File","9","True","position","FLOAT3","0,0,0","In","","Inherit","False","True","screenPosition","FLOAT2","0,0","In","","Inherit","False","True","worldNoise","FLOAT","0","In","","Inherit","False","True","lightDirectionNoise","FLOAT","0","In","","Inherit","False","True","lightDirectionOrthogonalAlignmentNoise","FLOAT","0","In","","Inherit","False","True","lightDirectionAlignmentNoise","FLOAT","0","In","","Inherit","False","True","normal","FLOAT3","0,0,0","In","","Inherit","False","True","worldToTangentMatrix","FLOAT3x3","1,0,0,1,1,1,1,0,1","In","","Inherit","False","True","output","FLOAT","0","Out","","Inherit","False","GetMainLightShadow_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","10","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT2","0,0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT3","0,0,0","False","8","FLOAT3x3","1,0,0,1,1,1,1,0,1","False","9","FLOAT","0","False","2","FLOAT","0","FLOAT","10"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":309,"pos":[-3968,6016],"params":["Inherit","False","3","3","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","2","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":980,"pos":[-9344,5424],"params":["Inherit","False","Property","_MainLightTransparencyShadow","主光透明阴影","17","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":996,"pos":[-9344,7168],"params":["Inherit","False","Property","_AdditionalLightsTransparencyShadow","附加光透明阴影","31","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":190,"pos":[-7152,256],"params":["Inherit","False","Property","_DepthFadePower","深度淡出幂次","159","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":218,"pos":[-6528,0],"params":["Inherit","False","Property","_InvertDepthFade","反转深度淡出","160","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":192,"pos":[-7168,640],"params":["Inherit","False","Property","_CameraDepthFadeLength","相机深度淡出长度","163","0","Create","False","0","0","0","False","2","Header(Camera Depth Fade)","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":193,"pos":[-7168,720],"params":["Inherit","False","Property","_CameraDepthFadeOffset","相机深度淡出偏移","164","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":209,"pos":[-7152,512],"params":["Inherit","False","Property","_SubtractiveDepthFadePower","相减深度淡出幂次","162","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":684,"pos":[-5120,4224],"params":["Inherit","False","227","Radial Mask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":703,"pos":[-4864,4352],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":571,"pos":[-1040,-912],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CameraDepthFade, AmplifyShaderEditor","id":191,"pos":[-6784,640],"params":["Inherit","False","3","2","FLOAT3","0,0,0","False","0","FLOAT","1","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":866,"pos":[-6144,384],"params":["Inherit","False","Power Smoothstep","-1","","575","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":865,"pos":[-6144,128],"params":["Inherit","False","Power Smoothstep","-1","","576","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":762,"pos":[-5120,4480],"params":["Inherit","False","759","Vertical Mask 2","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":984,"pos":[-8832,5376],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":993,"pos":[-8832,7040],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":606,"pos":[-3840,5840],"params":["Inherit","False","622","UV 3D World","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":311,"pos":[-3840,6016],"params":["Inherit","False","Vertex Noise Offset","-1","True","1","0","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":452,"pos":[-3840,5760],"params":["Inherit","False","340","UV 3D","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":570,"pos":[-1280,-640],"params":["Inherit","False","Property","_Noise","噪波","68","0","Create","False","0","0","0","False","2","Header(Noise)","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":235,"pos":[-4352,3456],"params":["Inherit","False","Property","_FresnelMaskPower","菲涅尔遮罩幂次","153","0","Create","False","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1007,"pos":[-7552,1024],"params":["Inherit","False","Property","_IntersectionHighlight","交叉高光","168","0","Create","False","0","0","0","False","2","Header(Intersection Highlight)","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":709,"pos":[-4608,4352],"params":["Inherit","False","Property","_Keyword2","关键字 2","131","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","713","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":569,"pos":[-892.5645,-770.2216],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":214,"pos":[-5760,128],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":568,"pos":[-6528,640],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DepthFade, AmplifyShaderEditor","id":1005,"pos":[-7168,1024],"params":["Inherit","False","True","True","True","2","1","FLOAT3","0,0,0","False","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FresnelNode, AmplifyShaderEditor","id":232,"pos":[-3968,3456],"params":["Inherit","True","Standard","WorldNormal","ViewDir","False","False","5","0","FLOAT3","0,0,1","False","4","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","1","False","3","FLOAT","5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":761,"pos":[-4736,4480],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":981,"pos":[-8576,5376],"params":["Inherit","False","Main Light Transparency Shadow","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":994,"pos":[-8576,7040],"params":["Inherit","False","Additional Lights Transparency Shadow","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":641,"pos":[-4480,7040],"params":["Inherit","False","327","UV 2D","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":588,"pos":[-4480,6656],"params":["Inherit","False","327","UV 2D","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TauNode, AmplifyShaderEditor","id":284,"pos":[-4480,5728],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":335,"pos":[-4480,5248],"params":["Inherit","False","327","UV 2D","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":605,"pos":[-3584,5760],"params":["Inherit","False","Property","_WorldSpaceUVs2","世界空间 UV","37","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","356","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":312,"pos":[-3584,6496],"params":["Inherit","False","311","Vertex Noise Offset","1","0","OBJECT","","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":942,"pos":[-4608,4224],"params":["Inherit","False","Property","_RadialMaskSubtractive","径向遮罩相减","107","0","Create","False","0","0","0","False","0","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":236,"pos":[-3968,3712],"params":["Inherit","False","Property","_FresnelMaskRemapMin","菲涅尔遮罩重映射最小值","154","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":237,"pos":[-3968,3792],"params":["Inherit","False","Property","_FresnelMaskRemapMax","菲涅尔遮罩重映射最大值","155","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":196,"pos":[-6784,768],"params":["Inherit","False","Property","_CameraDepthFadePower","相机深度淡出幂次","165","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":276,"pos":[-4480,5632],"params":["Inherit","False","Property","_VertexWaveOffset","顶点波动偏移","198","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":294,"pos":[-3584,6016],"params":["Inherit","False","Property","_VertexNoiseScale","顶点噪波缩放","203","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","2","4","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":296,"pos":[-3584,6144],"params":["Inherit","False","Property","_VertexNoiseTiling","顶点噪波平铺","204","0","Create","False","0","0","0","False","0","False","Object","-1","","1,1,1","1,1,1","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":295,"pos":[-3584,6304],"params":["Inherit","False","Property","_VertexNoiseAnimation","顶点噪波动画","205","0","Create","False","0","0","0","False","0","False","Object","-1","","0,2,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":260,"pos":[-4480,5536],"params":["Inherit","False","Property","_VertexWaveAnimation","顶点波波动画","197","0","Create","False","0","0","0","False","0","False","Object","-1","","4","4","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":11,"pos":[-640,-896],"params":["Inherit","False","Noise","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":216,"pos":[-5600,128],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":868,"pos":[-6144,640],"params":["Inherit","False","Power Smoothstep","-1","","577","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":1006,"pos":[-6912,1024],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":234,"pos":[-3456,3456],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":760,"pos":[-4608,4480],"params":["Inherit","False","Property","_Keyword5","关键字 2","138","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","758","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1078,"pos":[-4608,4608],"params":["Inherit","False","Constant","_Float1","Float 1","209","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1047,"pos":[-4608,4736],"params":["Inherit","False","981","Main Light Transparency Shadow","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1046,"pos":[-4608,4816],"params":["Inherit","False","994","Additional Lights Transparency Shadow","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":642,"pos":[-4288,7040],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","2,1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":590,"pos":[-4288,6656],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","2,1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":700,"pos":[-7296,3072],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TauNode, AmplifyShaderEditor","id":595,"pos":[-4480,5376],"params":["Inherit","False","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":336,"pos":[-4224,5248],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":285,"pos":[-4096,5632],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor","id":258,"pos":[-4096,5504],"params":["Inherit","False","1","0","FLOAT","1","False","5","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":292,"pos":[-3072,5888],"params":["Inherit","False","Scale Tiling Offset Animation","-1","","579","650501f4d90f3194eb72a847e06cc2e3","1,21,0","6","4","FLOAT3","0,0,0","False","7","FLOAT","1","False","8","FLOAT3","1,1,1","False","9","FLOAT4","0,0,0,0","False","19","INT","0","False","12","FLOAT4","0,0,0,0","False","2","FLOAT3","0","FLOAT","15"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":741,"pos":[-4224,4224],"params":["Inherit","False","Property","_VerticalMask1","垂直遮罩 1","130","0","Create","False","0","0","0","False","2","Header(Vertical Mask 1)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":243,"pos":[-3456,3584],"params":["Inherit","False","Property","_FresnelMask","菲涅尔遮罩","152","0","Create","False","0","0","0","False","2","Header(Fresnel Mask)","Space(5)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1010,"pos":[-7168,1152],"params":["Inherit","False","Property","_IntersectionHighlightPower","交叉高光幂次","170","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1016,"pos":[-7168,1280],"params":["Inherit","False","Property","_IntersectionHighlightRemapMin","交叉高光重映射最小值","171","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1017,"pos":[-7168,1360],"params":["Inherit","False","Property","_IntersectionHighlightRemapMax","交叉高光重映射最大值","172","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":254,"pos":[-4480,5456],"params":["Inherit","False","Property","_VertexWaveScale","顶点波动缩放","196","0","Create","False","0","0","0","False","0","False","Object","-1","","2","2","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":858,"pos":[-3072,6064],"params":["Inherit","False","Property","_VertexNoiseOctaves","顶点噪波倍频程","208","1","[IntRange]","Create","False","0","0","0","False","0","False","Object","-1","","1","0","1","4","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":303,"pos":[-3072,6144],"params":["Inherit","False","Property","_VertexNoiseDilation","顶点噪波膨胀","209","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","-0.2","0.2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":279,"pos":[-7296,3200],"params":["Inherit","False","Property","_VertexWaveNoiseVerticalMaskPower","顶点波动-噪波垂直遮罩幂次","215","0","Create","False","0","0","0","False","2","Header(Vertex Wave Noise Vertical Mask)","Space(5)","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":305,"pos":[-7296,3280],"params":["Inherit","False","Property","_VertexWaveNoiseVerticalMaskRemapMin","顶点波动-噪波垂直遮罩重映射最小值","216","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":306,"pos":[-7296,3360],"params":["Inherit","False","Property","_VertexWaveNoiseVerticalMaskRemapMax","顶点波动-噪波垂直遮罩重映射最大值","217","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":1064,"pos":[-3504,4816],"params":["Inherit","False","392.6667","131.3335","Adjust amount of intersection highlight shadow masking.","1","1063","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":195,"pos":[-5760,640],"params":["Inherit","False","Camera Depth Fade","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":188,"pos":[-5376,128],"params":["Inherit","False","Depth Fade","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":1008,"pos":[-6656,1024],"params":["Inherit","False","Power Smoothstep","-1","","581","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":242,"pos":[-3072,3584],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":41,"pos":[-4224,3968],"params":["Inherit","False","11","Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1076,"pos":[-4224,4608],"params":["Inherit","False","Property","_MainLightTransparencyShadowEnabled1","启用主光透明阴影","16","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","985","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1077,"pos":[-4224,4736],"params":["Inherit","False","Property","_AdditionalLightsTransparencyShadowEnabled1","启用附加光透明阴影","30","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","998","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":643,"pos":[-4096,7040],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","1,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":591,"pos":[-4096,6656],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","1,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":867,"pos":[-6784,3072],"params":["Inherit","True","Power Smoothstep","-1","","582","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":251,"pos":[-3968,5248],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT","16.13","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":583,"pos":[-3840,5504],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":657,"pos":[-3584,7168],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":302,"pos":[-2688,5888],"params":["Inherit","False","Simplex Noise Caustics","-1","","583","477e7c249263854458b4f42934448d42","0","4","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","9","FLOAT","0.01","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":489,"pos":[-2688,5760],"params":["Inherit","False","Simplex Noise","-1","","584","c68ae2e20c00ec54aaecd9d04797372e","0","3","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":766,"pos":[-4224,4352],"params":["Inherit","False","Property","_VerticalMask2","垂直遮罩 2","137","0","Create","False","0","0","0","False","2","Header(Vertical Mask 2)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":1019,"pos":[-3200,2080],"params":["Inherit","False","Property","_IntersectionHighlightColour","交叉高光颜色","174","0","Create","False","0","0","0","False","2","Header(Intersection Highlight Colour)","Space(5)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":661,"pos":[-4096,6912],"params":["Inherit","False","Property","_VertexUVOffsetTopPower","顶点 UV 顶部偏移幂次","181","0","Create","False","0","0","0","True","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":662,"pos":[-4096,7296],"params":["Inherit","False","Property","_VertexUVOffsetBottomPower","顶点 UV 底部偏移幂次","183","0","Create","False","0","0","0","True","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1011,"pos":[-6144,1024],"params":["Inherit","False","Intersection Highlight","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":142,"pos":[-3456,4128],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":199,"pos":[-3456,4384],"params":["Inherit","False","195","Camera Depth Fade","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":56,"pos":[-3712,3968],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":233,"pos":[-2816,3456],"params":["Inherit","False","Fresnel Mask","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":197,"pos":[-3456,4304],"params":["Inherit","False","188","Depth Fade","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1024,"pos":[-3200,2432],"params":["Inherit","False","Intersection Highlight Alpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1054,"pos":[-3456,4608],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":644,"pos":[-3840,7040],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":587,"pos":[-3840,6656],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":280,"pos":[-6400,3072],"params":["Inherit","False","Vertex WaveNoise Vertical Mask","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":584,"pos":[-3584,5248],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":646,"pos":[-3328,7168],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":597,"pos":[-3328,6784],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":694,"pos":[-3712,10240],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":894,"pos":[-3584,7552],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":128,"pos":[-3456,4480],"params":["Inherit","False","Property","_Alpha","透明度","54","0","Create","False","0","0","0","False","0","False","Object","-1","","1","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1063,"pos":[-3456,4864],"params":["Inherit","False","Property","_IntersectionHighlightShadowMaskPower","交叉高光阴影遮罩幂次","173","0","Create","False","0","0","0","False","0","False","Object","-1","","1","1","0.01","4","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":660,"pos":[-3328,6912],"params":["Inherit","False","Property","_VertexUVOffsetTop","顶点 UV 顶部偏移","180","0","Create","False","0","0","0","True","2","Header(Vertex UV Offset)","Space(5)","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":663,"pos":[-3328,7296],"params":["Inherit","False","Property","_VertexUVOffsetBottom","顶点 UV 底部偏移","182","0","Create","False","0","0","0","True","2","","Space(5)","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":490,"pos":[-2304,5888],"params":["Inherit","False","Property","_VertexNoiseDilationEnabled","启用顶点噪波膨胀","210","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":789,"pos":[-2304,6016],"params":["Inherit","False","Property","_VertexNoiseTwist","顶点噪波扭曲","211","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","-180","180","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":200,"pos":[-2960,4336],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":239,"pos":[-3456,4048],"params":["Inherit","False","233","Fresnel Mask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":143,"pos":[-3200,4128],"params":["Inherit","False","False","False","False","True","1","0","COLOR","0,0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":240,"pos":[-2960,4464],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":58,"pos":[-3456,3968],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":701,"pos":[-2960,4208],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":1061,"pos":[-2816,4608],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1028,"pos":[-2816,4304],"params":["Inherit","False","1024","Intersection Highlight Alpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1027,"pos":[-2816,4224],"params":["Inherit","False","1011","Intersection Highlight","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":249,"pos":[-3328,5248],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":281,"pos":[-3328,5456],"params":["Inherit","False","280","Vertex WaveNoise Vertical Mask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":648,"pos":[-3072,7040],"params":["Inherit","True","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":649,"pos":[-2944,7296],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":574,"pos":[-3072,6656],"params":["Inherit","True","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":604,"pos":[-2944,6912],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":682,"pos":[-3456,10240],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":693,"pos":[-3712,9728],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":895,"pos":[-3328,7552],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":668,"pos":[-3712,9856],"params":["Inherit","False","Property","_VertexNormalOffsetTopPower","顶点法线顶部偏移幂次","188","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":679,"pos":[-3712,10368],"params":["Inherit","False","Property","_VertexNormalOffsetBottomPower","顶点法线底部偏移幂次","190","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":267,"pos":[-3328,5376],"params":["Inherit","False","Property","_VertexWave","顶点波动","194","0","Create","False","0","0","0","False","2","Header(Vertex Wave)","Space(5)","False","Object","-1","","0.1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":852,"pos":[-1792,6016],"params":["Inherit","False","TwistXZ","-1","","585","9581222175ed3d74faf64569d7d97396","1,12,0","2","10","FLOAT3","0,0,0","False","9","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":127,"pos":[-2816,3968],"params":["Inherit","False","6","6","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1029,"pos":[-2432,4224],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":266,"pos":[-2944,5248],"params":["Inherit","False","3","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.NormalVertexDataNode, AmplifyShaderEditor","id":264,"pos":[-2944,5376],"params":["Inherit","False","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":650,"pos":[-2688,7040],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":576,"pos":[-2688,6656],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":297,"pos":[-1408,6224],"params":["Inherit","False","280","Vertex WaveNoise Vertical Mask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":856,"pos":[-1168,6000],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.NormalVertexDataNode, AmplifyShaderEditor","id":244,"pos":[-3712,9472],"params":["Inherit","False","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.NormalVertexDataNode, AmplifyShaderEditor","id":666,"pos":[-3712,9984],"params":["Inherit","False","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.NormalVertexDataNode, AmplifyShaderEditor","id":677,"pos":[-3712,10496],"params":["Inherit","False","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":669,"pos":[-3200,9728],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":680,"pos":[-3200,10240],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":896,"pos":[-3072,7552],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":246,"pos":[-3712,9616],"params":["Inherit","False","Property","_VertexNormalOffset","顶点法线偏移","186","0","Create","False","0","0","0","False","2","Header(Vertex Normal Offset)","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":667,"pos":[-3712,10144],"params":["Inherit","False","Property","_VertexNormalOffsetTop","顶点法线顶部偏移","187","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":681,"pos":[-3712,10656],"params":["Inherit","False","Property","_VertexNormalOffsetBottom","顶点法线底部偏移","189","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":290,"pos":[-1408,6144],"params":["Inherit","False","Property","_VertexNoise","顶点噪波","201","0","Create","False","0","0","0","False","2","Header(Vertex Noise)","Space(5)","False","Object","-1","","0.02","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":944,"pos":[-2176,4096],"params":["Inherit","False","227","Radial Mask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1012,"pos":[-2176,3968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":282,"pos":[-2688,5248],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":594,"pos":[-2432,6784],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":575,"pos":[-2432,6656],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":652,"pos":[-2432,7168],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":653,"pos":[-2432,7040],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":289,"pos":[-896,5888],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":245,"pos":[-3200,9472],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":676,"pos":[-2944,10368],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":665,"pos":[-2944,9856],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":897,"pos":[-2944,7552],"params":["Inherit","False","UV 2D Circular Y","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":948,"pos":[-1920,4096],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":1030,"pos":[-1920,3968],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":353,"pos":[-2176,4224],"params":["Inherit","False","350","Vertical Mask 1","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":593,"pos":[-2176,6656],"params":["Inherit","False","Property","_Keyword0","关键字 0","36","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","321","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":654,"pos":[-2176,7040],"params":["Inherit","False","Property","_Keyword1","关键字 0","36","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","321","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":674,"pos":[-2560,9856],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":692,"pos":[-3712,8064],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":899,"pos":[-3712,8576],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":902,"pos":[-3712,9088],"params":["Inherit","False","897","UV 2D Circular Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":448,"pos":[-2432,5248],"params":["Inherit","False","Property","_VertexWaveEnabled","启用顶点波动","195","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":450,"pos":[-640,5888],"params":["Inherit","False","Property","_VertexNoiseEnabled","启用顶点噪波","202","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":633,"pos":[-3712,7808],"params":["Inherit","False","Property","_VertexOffsetOverY1","顶点 Y 向偏移 1","220","0","Create","False","0","0","0","False","2","Header(Vertex Offset Over Y)","Space(5)","False","Object","-1","","0,0,0","0,0,0","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":631,"pos":[-3712,8192],"params":["Inherit","False","Property","_VertexOffsetOverY1Power","顶点 Y 向偏移 1 幂次","221","0","Create","False","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":777,"pos":[-3712,8320],"params":["Inherit","False","Property","_VertexOffsetOverY2","顶点 Y 向偏移 2","222","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0,0,0","0,0,0","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":775,"pos":[-3712,8704],"params":["Inherit","False","Property","_VertexOffsetOverY2Power","顶点 Y 向偏移 2 幂次","223","0","Create","False","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":886,"pos":[-3712,9216],"params":["Inherit","False","Property","_VertexOffsetOverCircularYPower","顶点环形 Y 偏移幂次","225","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":945,"pos":[-1664,3952],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":1033,"pos":[-1680,4048],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":712,"pos":[-1664,4224],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":765,"pos":[-2176,4352],"params":["Inherit","False","759","Vertical Mask 2","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":265,"pos":[-2048,5248],"params":["Inherit","False","Vertex Sine","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":579,"pos":[-1664,6656],"params":["Inherit","False","Vertex Offset Top","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":655,"pos":[-1664,7040],"params":["Inherit","False","Vertex Offset Bottom","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":298,"pos":[-256,5888],"params":["Inherit","False","Vertex Noise","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":670,"pos":[-2304,9856],"params":["Inherit","False","Vertex Normal Offset","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TransformDirectionNode, AmplifyShaderEditor","id":636,"pos":[-3328,7808],"params":["Inherit","False","World","Object","False","Fast","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":632,"pos":[-3200,8064],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TransformDirectionNode, AmplifyShaderEditor","id":780,"pos":[-3328,8320],"params":["Inherit","False","World","Object","False","Fast","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":776,"pos":[-3328,8576],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":888,"pos":[-3328,9088],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor","id":889,"pos":[-3712,8832],"params":["Inherit","False","Property","_VertexOffsetOverCircularY","顶点环形 Y 偏移","224","0","Create","False","0","0","0","False","2","Header(Vertex Offset Over Circular Y)","Space(5)","False","Object","-1","","0,0,0","0,0,0","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":943,"pos":[-1408,3968],"params":["Inherit","False","Property","_Keyword8","关键字 2","107","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","942","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":711,"pos":[-1408,4224],"params":["Inherit","False","Property","_Keyword3","关键字 2","131","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","713","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":764,"pos":[-1664,4352],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":671,"pos":[-3712,10880],"params":["Inherit","False","670","Vertex Normal Offset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":268,"pos":[-3712,10960],"params":["Inherit","False","265","Vertex Sine","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":299,"pos":[-3712,11040],"params":["Inherit","False","298","Vertex Noise","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":580,"pos":[-3712,11120],"params":["Inherit","False","579","Vertex Offset Top","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":638,"pos":[-2944,7936],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":893,"pos":[-2944,8960],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":782,"pos":[-2944,8448],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":656,"pos":[-3712,11200],"params":["Inherit","False","655","Vertex Offset Bottom","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":744,"pos":[-896,4224],"params":["Inherit","False","Property","_Keyword4","关键字 2","130","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","741","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":763,"pos":[-1408,4352],"params":["Inherit","False","Property","_Keyword6","关键字 2","138","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","758","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":983,"pos":[-896,4480],"params":["Inherit","False","981","Main Light Transparency Shadow","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":269,"pos":[-3200,10880],"params":["Inherit","False","5","5","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":785,"pos":[-2560,8448],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor","id":816,"pos":[-3200,11232],"params":["Inherit","False","0","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":818,"pos":[-3200,11136],"params":["Inherit","False","Property","_VertexTwist","顶点扭曲","192","0","Create","False","0","0","0","False","2","Header(Vertex Twist)","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":986,"pos":[-288,4032],"params":["Inherit","False","1275.315","190.7114","Separate from Lighting Enabled.","2","998","985","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":768,"pos":[-896,4352],"params":["Inherit","False","Property","_Keyword7","关键字 2","137","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","766","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":982,"pos":[-512,4352],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1000,"pos":[-256,4352],"params":["Inherit","False","994","Additional Lights Transparency Shadow","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":819,"pos":[-2944,10880],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":639,"pos":[-2304,8448],"params":["Inherit","False","Vertex Offset over Y","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.NegateNode, AmplifyShaderEditor","id":854,"pos":[-2944,11136],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":999,"pos":[256,4352],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":640,"pos":[-2688,11008],"params":["Inherit","False","639","Vertex Offset over Y","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":853,"pos":[-2688,10880],"params":["Inherit","False","TwistXZ","-1","","587","9581222175ed3d74faf64569d7d97396","1,12,0","2","10","FLOAT3","0,0,0","False","9","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":985,"pos":[-256,4096],"params":["Inherit","False","Property","_MainLightTransparencyShadowEnabled","启用主光透明阴影","16","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":857,"pos":[-2304,10880],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":998,"pos":[512,4096],"params":["Inherit","False","Property","_AdditionalLightsTransparencyShadowEnabled","启用附加光透明阴影","30","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":1187,"pos":[-8242,3022],"params":["Inherit","False","868","521.6667","Unsed.","6","1102","1103","1104","1106","1109","1105","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":1186,"pos":[-10032,3024],"params":["Inherit","False","964","433.3333","Unsed.","6","1100","1108","1098","1099","1101","1107","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":49,"pos":[1152,4096],"params":["Inherit","False","Alpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":247,"pos":[-2048,10880],"params":["Inherit","False","Vertex Offset","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.WorldToObjectMatrix, AmplifyShaderEditor","id":1269,"pos":[-9648,7344],"params":["Inherit","False","0","1","FLOAT4x4","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":560,"pos":[-1408,-3248],"params":["Inherit","False","Texture A","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":562,"pos":[-1536,-640],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":510,"pos":[-2432,-1152],"params":["Inherit","False","Property","_NoiseDilationEnabled1","启用噪波膨胀","32","0","Create","True","0","0","0","False","0","False","","0","1","1","True","","Toggle","2","Key0","Key1","Reference","444","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":509,"pos":[-1920,-1152],"params":["Inherit","False","Signed Power Smoothstep","-1","","589","3654d4d5f7b612d4085eb90cd7a60668","3,3,2,20,1,15,1","7","2","FLOAT","0","False","4","FLOAT2","0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT4","0,0,0,0","False","12","FLOAT","1","False","17","FLOAT","0","False","18","FLOAT","1","False","1","FLOAT3","14"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":511,"pos":[-1280,-1152],"params":["Inherit","False","Noise Gradient","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":559,"pos":[-1408,-3328],"params":["Inherit","False","Texture R","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":36,"pos":[-3840,192],"params":["Inherit","False","Colour RGB x A","-1","","590","034d6205f93eb7e4f9100dabf18de7c4","0","1","22","COLOR","1,1,1,0.5019608","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":37,"pos":[-3840,0],"params":["Inherit","False","Colour RGB x A","-1","","591","034d6205f93eb7e4f9100dabf18de7c4","0","1","22","COLOR","1,1,1,0.5019608","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":31,"pos":[-3328,0],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":43,"pos":[-3840,304],"params":["Inherit","False","42","Colour Power","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.HSVToRGBNode, AmplifyShaderEditor","id":132,"pos":[-2304,0],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":370,"pos":[-1920,256],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":44,"pos":[-2944,-256],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":33,"pos":[-3200,-256],"params":["Inherit","False","11","Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":42,"pos":[-2688,-256],"params":["Inherit","False","Colour Power","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":385,"pos":[-3840,704],"params":["Inherit","False","Colour RGB x A","-1","","592","034d6205f93eb7e4f9100dabf18de7c4","0","1","22","COLOR","1,1,1,0.5019608","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":386,"pos":[-3840,512],"params":["Inherit","False","Colour RGB x A","-1","","593","034d6205f93eb7e4f9100dabf18de7c4","0","1","22","COLOR","1,1,1,0.5019608","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":387,"pos":[-3328,512],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":398,"pos":[-2688,512],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":399,"pos":[-2688,640],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":400,"pos":[-2688,768],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":391,"pos":[-2304,672],"params":["Inherit","False","369","Vertical Colour Mask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":390,"pos":[-3840,816],"params":["Inherit","False","42","Colour Power","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":248,"pos":[-1664,672],"params":["Inherit","False","247","Vertex Offset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":769,"pos":[-7040,-512],"params":["Inherit","False","Signed Power Smoothstep","-1","","594","3654d4d5f7b612d4085eb90cd7a60668","3,3,0,20,1,15,1","7","2","FLOAT","0","False","4","FLOAT2","0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT4","0,0,0,0","False","12","FLOAT","1","False","17","FLOAT","0","False","18","FLOAT","1","False","1","FLOAT","14"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":805,"pos":[-8960,-1280],"params":["Inherit","False","Twirl","-1","","595","90936742ac32db8449cd21ab6dd337c8","0","4","1","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT","0","False","4","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":808,"pos":[-9344,-1152],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RadiansOpNode, AmplifyShaderEditor","id":809,"pos":[-9344,-1280],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":561,"pos":[-1920,-640],"params":["Inherit","False","560","Texture A","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RGBToHSVNode, AmplifyShaderEditor","id":402,"pos":[-3072,512],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.HSVToRGBNode, AmplifyShaderEditor","id":401,"pos":[-2304,512],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":52,"pos":[-1664,592],"params":["Inherit","False","49","Alpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":139,"pos":[-4096,1024],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":951,"pos":[-2688,1024],"params":["Inherit","False","Vertex Colour","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":698,"pos":[-7296,1664],"params":["Inherit","False","691","UV 2D Y","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":869,"pos":[-6784,1664],"params":["Inherit","True","Power Smoothstep","-1","","598","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":369,"pos":[-6400,1664],"params":["Inherit","False","Vertical Colour Mask","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":393,"pos":[-1280,0],"params":["Inherit","False","Colour Input","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RGBToHSVNode, AmplifyShaderEditor","id":129,"pos":[-3072,0],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":136,"pos":[-2688,256],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":135,"pos":[-2688,128],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":130,"pos":[-2688,0],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RGBToHSVNode, AmplifyShaderEditor","id":949,"pos":[-3840,1152],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":141,"pos":[-2944,1152],"params":["Inherit","False","True","True","True","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.HSVToRGBNode, AmplifyShaderEditor","id":950,"pos":[-3200,1152],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":394,"pos":[-4480,1664],"params":["Inherit","False","393","Colour Input","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":140,"pos":[-3840,1664],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RGBToHSVNode, AmplifyShaderEditor","id":1067,"pos":[-4096,1920],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1071,"pos":[-3712,2048],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1072,"pos":[-3712,1920],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1068,"pos":[-3712,2176],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.HSVToRGBNode, AmplifyShaderEditor","id":1073,"pos":[-3200,1920],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1020,"pos":[-3200,2304],"params":["Inherit","False","1011","Intersection Highlight","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":1041,"pos":[-2816,2304],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","0.0001","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1075,"pos":[-2816,1920],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":241,"pos":[-3072,3456],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TransformDirectionNode, AmplifyShaderEditor","id":891,"pos":[-3328,8832],"params":["Inherit","False","World","Object","False","Fast","False","1","0","FLOAT3","0,0,0","False","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":952,"pos":[-4480,1792],"params":["Inherit","False","951","Vertex Colour","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":955,"pos":[-3456,1536],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":956,"pos":[-3456,1408],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":492,"pos":[-3456,1664],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT3","0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":50,"pos":[-2048,1664],"params":["Inherit","False","Colour","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1018,"pos":[-2304,1664],"params":["Inherit","False","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":496,"pos":[-3840,1792],"params":["Inherit","False","495","Lighting","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":51,"pos":[-1664,512],"params":["Inherit","False","50","Colour","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RotateAboutAxisNode, AmplifyShaderEditor","id":1102,"pos":[-7680,3072],"params":["Inherit","False","False","4","0","FLOAT3","1,0,0","False","1","FLOAT","0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RotateAboutAxisNode, AmplifyShaderEditor","id":1103,"pos":[-7680,3216],"params":["Inherit","False","False","4","0","FLOAT3","0,1,0","False","1","FLOAT","0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RotateAboutAxisNode, AmplifyShaderEditor","id":1104,"pos":[-7680,3360],"params":["Inherit","False","False","4","0","FLOAT3","0,0,1","False","1","FLOAT","0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":1106,"pos":[-7936,3200],"params":["Inherit","False","FLOAT3","1","0","FLOAT3","0,0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1109,"pos":[-8192,3200],"params":["Inherit","False","1107","Particle Rotation 3D","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1105,"pos":[-8192,3072],"params":["Inherit","False","1101","Particle Center","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1172,"pos":[-6784,3968],"params":["Inherit","False","Normal from Height Back","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1171,"pos":[-6784,3712],"params":["Inherit","False","Normal from Height Front","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":528,"pos":[-7680,3968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":518,"pos":[-7936,3968],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":523,"pos":[-7424,3968],"params":["Inherit","False","Normal From Height","-1","","604","1942fe2c5f1a1f94881a33d532e4afeb","0","2","20","FLOAT","0","False","110","FLOAT","1","False","2","FLOAT3","40","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":507,"pos":[-7424,3712],"params":["Inherit","False","Normal From Height","-1","","605","1942fe2c5f1a1f94881a33d532e4afeb","0","2","20","FLOAT","0","False","110","FLOAT","1","False","2","FLOAT3","40","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":506,"pos":[-8192,3712],"params":["Inherit","False","11","Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.NegateNode, AmplifyShaderEditor","id":1138,"pos":[-7040,3968],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ViewDirInputsCoordNode, AmplifyShaderEditor","id":1124,"pos":[-8192,4224],"params":["Inherit","False","World","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1201,"pos":[-8192,4480],"params":["Inherit","False","1171","Normal from Height Front","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1219,"pos":[-8192,4608],"params":["Inherit","False","1172","Normal from Height Back","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1118,"pos":[-7680,4224],"params":["Inherit","False","GetMainLightShadow_float","8","File","2","True","viewDirection","FLOAT3","0,0,0","In","","Inherit","False","True","output","FLOAT3","0,0,0","Out","","Inherit","False","GetMainLightColourFlat_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","2","FLOAT3","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1197,"pos":[-7680,4480],"params":["Inherit","False","GetMainLightShadow_float","8","File","3","True","viewDirection","FLOAT3","0,0,0","In","","Inherit","False","True","normal","FLOAT3","0,0,0","In","","Inherit","False","True","output","FLOAT3","0,0,0","Out","","Inherit","False","GetMainLightColourLambert_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","4","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","2","FLOAT3","0","FLOAT3","4"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1218,"pos":[-7680,4608],"params":["Inherit","False","GetMainLightShadow_float","8","File","3","True","viewDirection","FLOAT3","0,0,0","In","","Inherit","False","True","normal","FLOAT3","0,0,0","In","","Inherit","False","True","output","FLOAT3","0,0,0","Out","","Inherit","False","GetMainLightColourLambert_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","4","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","2","FLOAT3","0","FLOAT3","4"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1206,"pos":[-7296,4624],"params":["Inherit","False","Property","_AdditionalLightsFlat6","附加光平面","5","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1208","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1213,"pos":[-6784,4736],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1220,"pos":[-6528,4608],"params":["Inherit","False","Property","_AdditionalLightsFlat9","附加光平面","4","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1207","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1223,"pos":[-6528,4352],"params":["Inherit","False","Property","_AdditionalLightsFlat12","附加光平面","4","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1207","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1214,"pos":[-6784,4480],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1216,"pos":[-5760,4224],"params":["Inherit","False","Main Light Front","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1217,"pos":[-5760,4608],"params":["Inherit","False","Main Light Back","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1227,"pos":[-8192,4992],"params":["Inherit","False","1216","Main Light Front","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor","id":1228,"pos":[-7552,5120],"params":["Inherit","False","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LengthOpNode, AmplifyShaderEditor","id":1229,"pos":[-7552,5248],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1230,"pos":[-7808,5376],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1231,"pos":[-7552,5376],"params":["Inherit","False","2","2","0","FLOAT","1","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1232,"pos":[-8192,5376],"params":["Inherit","False","11","Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1235,"pos":[-8192,5120],"params":["Inherit","False","1217","Main Light Back","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":1236,"pos":[-7296,5248],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1237,"pos":[-7040,5120],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":502,"pos":[-6272,4992],"params":["Inherit","False","4","4","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":988,"pos":[-6016,4992],"params":["Inherit","False","Main Light","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1225,"pos":[-6784,4992],"params":["Inherit","False","Property","_AdditionalLightsFlat7","附加光平面","5","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1208","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ViewDirInputsCoordNode, AmplifyShaderEditor","id":1204,"pos":[-7296,5632],"params":["Inherit","False","World","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1203,"pos":[-6784,5504],"params":["Inherit","False","GetMainLightShadow_float","8","File","3","True","anisotropy","FLOAT","0","In","","Inherit","False","True","viewDirection","FLOAT3","0,0,0","In","","Inherit","False","True","output","FLOAT","0","Out","","Inherit","False","GetMainLightAnisotropy_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","4","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT3","0,0,0","False","3","FLOAT","0","False","2","FLOAT3","0","FLOAT","4"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1205,"pos":[-6400,5504],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1117,"pos":[-6784,5120],"params":["Inherit","False","1116","Main Light Shadow","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1215,"pos":[-6144,4224],"params":["Inherit","False","Property","_AdditionalLightsFlat8","附加光平面","5","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1208","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":963,"pos":[-8832,4992],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1116,"pos":[-8576,4992],"params":["Inherit","False","Main Light Shadow","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":1152,"pos":[-8192,5888],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1174,"pos":[-8192,6272],"params":["Inherit","False","1172","Normal from Height Back","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1173,"pos":[-8192,6144],"params":["Inherit","False","1171","Normal from Height Front","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1183,"pos":[-7296,6288],"params":["Inherit","False","Property","_AdditionalLightsFlat2","附加光平面","21","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1182","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1199,"pos":[-6784,6400],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1221,"pos":[-6528,6272],"params":["Inherit","False","Property","_AdditionalLightsFlat10","附加光平面","20","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1179","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1198,"pos":[-6144,5888],"params":["Inherit","False","Property","_AdditionalLightsFlat5","附加光平面","21","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1182","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1175,"pos":[-5760,5888],"params":["Inherit","False","Additional Lights Front","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1176,"pos":[-6784,6144],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1222,"pos":[-6528,6016],"params":["Inherit","False","Property","_AdditionalLightsFlat11","附加光平面","20","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1179","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1180,"pos":[-5760,6272],"params":["Inherit","False","Additional Lights Back","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":989,"pos":[-8192,7424],"params":["Inherit","False","988","Main Light","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":500,"pos":[-7296,7424],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":495,"pos":[-7168,7424],"params":["Inherit","False","Lighting","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":553,"pos":[-8192,7552],"params":["Inherit","False","552","Additional Lights","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1200,"pos":[-6784,6656],"params":["Inherit","False","Property","_AdditionalLightsFlat4","附加光平面","21","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","1182","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1184,"pos":[-8192,6656],"params":["Inherit","False","1175","Additional Lights Front","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor","id":540,"pos":[-7552,6784],"params":["Inherit","False","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LengthOpNode, AmplifyShaderEditor","id":543,"pos":[-7552,6912],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1141,"pos":[-7808,7040],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":556,"pos":[-7552,7040],"params":["Inherit","False","2","2","0","FLOAT","1","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":549,"pos":[-8192,7040],"params":["Inherit","False","11","Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1185,"pos":[-8192,6784],"params":["Inherit","False","1180","Additional Lights Back","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":542,"pos":[-7296,6912],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":527,"pos":[-7040,6784],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":497,"pos":[-6272,6656],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":552,"pos":[-6016,6656],"params":["Inherit","False","Additional Lights","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1224,"pos":[-6784,6784],"params":["Inherit","False","1153","Additional Lights Shadow","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":992,"pos":[-9344,6400],"params":["Inherit","False"," ","8","File","3","True","screenPosition","FLOAT2","0,0","In","","Inherit","False","True","position","FLOAT3","0,0,0","In","","Inherit","False","True","output","FLOAT","0","Out","","Inherit","False","GetAdditionalLightsShadow_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","4","0","FLOAT","0","False","1","FLOAT2","0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT","0","False","2","FLOAT","0","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1153,"pos":[-8576,6656],"params":["Inherit","False","Additional Lights Shadow","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":1241,"pos":[-8832,6656],"params":["Inherit","False","3","0","FLOAT","1","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1147,"pos":[-9984,6400],"params":["Inherit","False","1145","Screen UV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":1100,"pos":[-9600,3072],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":1108,"pos":[-9600,3248],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor","id":1098,"pos":[-9984,3072],"params":["Inherit","False","1","4","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor","id":1099,"pos":[-9984,3248],"params":["Inherit","False","2","4","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1101,"pos":[-9344,3072],"params":["Inherit","False","Particle Center","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":1107,"pos":[-9344,3200],"params":["Inherit","False","Particle Rotation 3D","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":913,"pos":[-9984,880],"params":["Inherit","False","Property","_StartFoldoutRadialMask","开始折叠-径向遮罩","105","1","[HideInInspector]","Create","True","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1261,"pos":[-6656,-2048],"params":["Inherit","False","1248","Sample Noise","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1264,"pos":[-6656,-2304],"params":["Inherit","False","1248","Sample Noise","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1265,"pos":[-6656,-2688],"params":["Inherit","False","1248","Sample Noise","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1260,"pos":[-6656,-1792],"params":["Inherit","False","1248","Sample Noise","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1250,"pos":[-6272,-1792],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1262,"pos":[-6272,-2048],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1263,"pos":[-6272,-2304],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":1266,"pos":[-6272,-2688],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1135,"pos":[-7680,6144],"params":["Inherit","False"," ","8","File","4","True","position","FLOAT3","0,0,0","In","","Inherit","False","True","screenUV","FLOAT2","0,0","In","","Inherit","False","True","normal","FLOAT3","0,0,0","In","","Inherit","False","True","output","FLOAT3","0,0,0","Out","","Inherit","False","GetAdditionalLightsColourLambert_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","5","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT2","0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","2","FLOAT","0","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1137,"pos":[-7680,6288],"params":["Inherit","False"," ","8","File","4","True","position","FLOAT3","0,0,0","In","","Inherit","False","True","screenUV","FLOAT2","0,0","In","","Inherit","False","True","normal","FLOAT3","0,0,0","In","","Inherit","False","True","output","FLOAT3","0,0,0","Out","","Inherit","False","GetAdditionalLightsColourLambert_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","5","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT2","0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","2","FLOAT","0","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":1149,"pos":[-8192,6048],"params":["Inherit","False","1145","Screen UV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor","id":1167,"pos":[-7680,5888],"params":["Inherit","False"," ","8","File","3","True","position","FLOAT3","0,0,0","In","","Inherit","False","True","screenUV","FLOAT2","0,0","In","","Inherit","False","True","output","FLOAT3","0,0,0","Out","","Inherit","False","GetAdditionalLightsColourFlat_float","False","False","0","3e9fef90fa1fa8d4091eea0baa50ed09","False","4","0","FLOAT","0","False","1","FLOAT3","0,0,0","False","2","FLOAT2","0,0","False","3","FLOAT3","0,0,0","False","2","FLOAT","0","FLOAT3","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":508,"pos":[-7936,3840],"params":["Inherit","False","Property","_LambertLightingNormalfromHeight","兰伯特光照法线取自高度","1","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":494,"pos":[-7808,7424],"params":["Inherit","False","Property","_MainLightEnabled","启用主光","2","0","Create","False","0","0","0","False","2","Header(Main Light)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":505,"pos":[-6784,5248],"params":["Inherit","False","Property","_MainLight","主光","3","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1207,"pos":[-7296,4224],"params":["Inherit","False","Property","_MainLightFlatEnabled","启用主光平面光照","4","0","Create","False","0","0","0","False","0","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1208,"pos":[-7296,4480],"params":["Inherit","False","Property","_MainLightLambertEnabled","启用主光兰伯特光照","5","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1195,"pos":[-7296,4736],"params":["Inherit","False","Property","_MainLightFlatLambertBlend","主光平面-兰伯特混合","6","0","Create","False","0","0","0","False","0","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1234,"pos":[-8192,5632],"params":["Inherit","False","Property","_MainLightLambertTranslucency","主光兰伯特透射","7","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1233,"pos":[-8192,5504],"params":["Inherit","False","Property","_MainLightLambertTranslucencyMask","主光兰伯特透射遮罩","8","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","10","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1120,"pos":[-7296,5504],"params":["Inherit","False","Property","_MainLightAnisotropy","主光各向异性","9","0","Create","False","0","0","0","False","0","False","Object","-1","","0.8","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1121,"pos":[-6784,5632],"params":["Inherit","False","Property","_MainLightAnisotropyBlend","主光各向异性混合","10","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":965,"pos":[-9344,5344],"params":["Inherit","False","Property","_MainLightShadow","主光阴影","11","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1079,"pos":[-7808,7552],"params":["Inherit","False","Property","_AdditionalLightsEnabled","启用附加光","18","0","Create","False","0","0","0","False","2","Header(Additional Lights)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":498,"pos":[-6784,6912],"params":["Inherit","False","Property","_AdditionalLights","附加光","19","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1179,"pos":[-7296,5888],"params":["Inherit","False","Property","_AdditionalLightsFlatEnabled","启用附加光平面光照","20","0","Create","False","0","0","0","False","0","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":1182,"pos":[-7296,6144],"params":["Inherit","False","Property","_AdditionalLightsLambertEnabled","启用附加光兰伯特光照","21","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1169,"pos":[-7296,6400],"params":["Inherit","False","Property","_AdditionalLightsFlatLambertBlend","附加光平面-兰伯特混合","22","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":555,"pos":[-8192,7296],"params":["Inherit","False","Property","_AdditionalLightsLambertTranslucency","附加光兰伯特透射","23","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1142,"pos":[-8192,7168],"params":["Inherit","False","Property","_AdditionalLightsLambertTranslucencyMask","附加光兰伯特透射遮罩","24","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","10","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1242,"pos":[-9344,7040],"params":["Inherit","False","Property","_AdditionalLightsShadow","附加光阴影","25","0","Create","False","0","0","0","False","2","Header(Additional Lights Shadow)","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":905,"pos":[-9984,-128],"params":["Inherit","False","Property","_StartFoldoutLighting","开始折叠-光照","0","0","Create","False","0","0","0","True","1","Tooltip(Allows light to affect the surface, supporting one directional light and any number of additional lights)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":906,"pos":[-9984,-64],"params":["Inherit","False","Property","_EndFoldoutLighting","结束折叠-光照","32","0","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":557,"pos":[-1920,-3328],"params":["Inherit","True","Property","_Texture","贴图","33","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":558,"pos":[-1280,-768],"params":["Inherit","False","Property","_TextureEnabled","启用贴图","34","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":939,"pos":[-9984,16],"params":["Inherit","False","Property","_StartFoldoutBaseUVs","开始折叠-基础 UV","35","0","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":938,"pos":[-9984,80],"params":["Inherit","False","Property","_EndFoldoutBaseUVs","结束折叠-基础 UV","40","0","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":940,"pos":[-9984,160],"params":["Inherit","False","Property","_StartFoldoutParticleSettings","开始折叠-粒子设置","41","0","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":954,"pos":[-3840,1408],"params":["Inherit","False","Property","_VertexColourHueShift","顶点颜色色相偏移","44","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":953,"pos":[-3840,1488],"params":["Inherit","False","Property","_VertexColourSaturationShift","顶点颜色饱和度偏移","45","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":941,"pos":[-9984,224],"params":["Inherit","False","Property","_EndFoldoutParticleSettings","结束折叠-粒子设置","46","0","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":903,"pos":[-9984,304],"params":["Inherit","False","Property","_StartFoldoutColour","开始折叠-颜色","47","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":30,"pos":[-4096,192],"params":["Inherit","False","Property","_ColourB","颜色 B","49","1","[HDR]","Create","False","0","0","0","False","0","False","Object","-1","","1,0.02745098,0,1","1,1,1,1","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":29,"pos":[-4096,0],"params":["Inherit","False","Property","_ColourA","颜色 A","48","2","[HDR]","[Header]","Create","False","0","0","0","False","2","Header(Colour)","Space(5)","False","Object","-1","","1,0.1254902,0,1","1,1,1,1","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":35,"pos":[-3200,-176],"params":["Inherit","False","Property","_ColourPower","颜色幂次","50","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":131,"pos":[-3072,160],"params":["Inherit","False","Property","_ColourHueShift","颜色色相偏移","51","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":134,"pos":[-3072,256],"params":["Inherit","False","Property","_ColourSaturationShift","颜色饱和度偏移","52","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":68,"pos":[-3072,352],"params":["Inherit","False","Property","_ColourValueMultiplier","颜色明度倍率","53","0","Create","False","0","0","0","False","0","False","Object","-1","","5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":904,"pos":[-9984,368],"params":["Inherit","False","Property","_EndFoldoutColour","结束折叠-颜色","55","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":907,"pos":[-9984,448],"params":["Inherit","False","Property","_StartFoldoutVerticalColour","开始折叠-垂直颜色","56","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":392,"pos":[-1664,0],"params":["Inherit","False","Property","_VerticalColour","垂直颜色","57","0","Create","False","0","0","0","False","2","Header(Vertical Colour)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":388,"pos":[-4096,512],"params":["Inherit","False","Property","_VerticalColourA","垂直颜色 A","58","2","[HDR]","[Header]","Create","False","0","0","0","False","0","False","Object","-1","","0,0.5019608,1,1","1,1,1,1","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":389,"pos":[-4096,704],"params":["Inherit","False","Property","_VerticalColourB","垂直颜色 B","59","1","[HDR]","Create","False","0","0","0","False","0","False","Object","-1","","0,0,1,1","1,1,1,1","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":395,"pos":[-3072,672],"params":["Inherit","False","Property","_VerticalColourHueShift","垂直颜色色相偏移","60","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":396,"pos":[-3072,768],"params":["Inherit","False","Property","_VerticalColourSaturationShift","垂直颜色饱和度偏移","61","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":397,"pos":[-3072,864],"params":["Inherit","False","Property","_VerticalColourValueMultiplier","垂直颜色明度倍率","62","0","Create","False","0","0","0","False","0","False","Object","-1","","5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":364,"pos":[-7296,1792],"params":["Inherit","False","Property","_VerticalColourMaskPower","垂直颜色遮罩幂次","63","0","Create","False","0","0","0","False","2","Header(Vertical Colour Mask)","Space(5)","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":362,"pos":[-7296,1872],"params":["Inherit","False","Property","_VerticalColourMaskRemapMin","垂直颜色遮罩重映射最小值","64","0","Create","False","0","0","0","False","0","False","Object","-1","","0.5","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":363,"pos":[-7296,1952],"params":["Inherit","False","Property","_VerticalColourMaskRemapMax","垂直颜色遮罩重映射最大值","65","0","Create","False","0","0","0","False","0","False","Object","-1","","0.1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":908,"pos":[-9984,512],"params":["Inherit","False","Property","_EndFoldoutVerticalColour","结束折叠-垂直颜色","66","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":909,"pos":[-9984,592],"params":["Inherit","False","Property","_StartFoldoutNoise","开始折叠-噪波","67","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":806,"pos":[-9600,-1280],"params":["Inherit","False","Property","_NoiseXYTwist","噪波 XY 扭曲","81","0","Create","False","0","0","0","False","1","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":807,"pos":[-9600,-1152],"params":["Inherit","False","Property","_NoiseXYTwistOffset","噪波 XY 扭曲偏移","82","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":770,"pos":[-6656,-512],"params":["Inherit","False","Property","_NoiseUVPreRemap","噪波 UV 预重映射","88","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":771,"pos":[-7680,-512],"params":["Inherit","False","Property","_NoiseUVYPreRemapMin","噪波 UV-Y 预重映射最小值","89","0","Create","False","0","0","0","False","0","False","Object","-1","","0","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":773,"pos":[-7680,-432],"params":["Inherit","False","Property","_NoiseUVYPreRemapMax","噪波 UV-Y 预重映射最大值","90","0","Create","False","0","0","0","False","0","False","Object","-1","","1","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":910,"pos":[-9984,656],"params":["Inherit","False","Property","_EndFoldoutNoise","结束折叠-噪波","91","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":912,"pos":[-9984,736],"params":["Inherit","False","Property","_StartFoldoutNoiseDistortion","开始折叠-噪波扭曲","92","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":911,"pos":[-9984,800],"params":["Inherit","False","Property","_EndFoldoutNoiseDistortion","结束折叠-噪波扭曲","104","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":914,"pos":[-9984,944],"params":["Inherit","False","Property","_EndFoldoutRadialMask","结束折叠-径向遮罩","114","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":916,"pos":[-9984,1024],"params":["Inherit","False","Property","_StartFoldoutRadialMaskDistortion","开始折叠-径向遮罩扭曲","115","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":917,"pos":[-9984,1088],"params":["Inherit","False","Property","_EndFoldoutRadialMaskDistortion","结束折叠-径向遮罩扭曲","127","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":918,"pos":[-9984,1168],"params":["Inherit","False","Property","_StartFoldoutVerticalMasks","开始折叠-垂直遮罩","128","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":919,"pos":[-9984,1232],"params":["Inherit","False","Property","_EndFoldoutVerticalMasks","结束折叠-垂直遮罩","144","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":921,"pos":[-9984,1312],"params":["Inherit","False","Property","_StartFoldoutSpherizeNoise","开始折叠-球形化噪波","145","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":920,"pos":[-9984,1376],"params":["Inherit","False","Property","_EndFoldoutSpherizeNoise","结束折叠-球形化噪波","150","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":922,"pos":[-9984,1456],"params":["Inherit","False","Property","_StartFoldoutFresnelMask","开始折叠-菲涅尔遮罩","151","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":923,"pos":[-9984,1520],"params":["Inherit","False","Property","_EndFoldoutFresnelMask","结束折叠-菲涅尔遮罩","156","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":924,"pos":[-9984,1600],"params":["Inherit","False","Property","_StartFoldoutDepthFade","开始折叠-深度淡出","157","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":925,"pos":[-9984,1664],"params":["Inherit","False","Property","_EndFoldoutDepthFade","结束折叠-深度淡出","166","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1043,"pos":[-9984,1744],"params":["Inherit","False","Property","_StartFoldoutIntersectionHighlight","开始折叠-交叉高光","167","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1074,"pos":[-3712,2304],"params":["Inherit","False","Property","_IntersectionHighlightAlpha","交叉高光透明度","169","0","Create","False","0","0","0","False","0","False","Object","-1","","1","1","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1070,"pos":[-4096,2080],"params":["Inherit","False","Property","_IntersectionHighlightColourHueShift","交叉高光颜色色相偏移","175","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1069,"pos":[-4096,2176],"params":["Inherit","False","Property","_IntersectionHighlightColourSaturationShift","交叉高光颜色饱和度偏移","176","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","-1","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1044,"pos":[-9984,1808],"params":["Inherit","False","Property","_EndFoldoutIntersectionHighlight","结束折叠-交叉高光","178","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":1066,"pos":[-4096,2272],"params":["Inherit","False","Property","_IntersectionHighlightColourValueMultiplier","交叉高光颜色明度倍率","177","0","Create","False","0","0","0","False","0","False","Object","-1","","5","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":926,"pos":[-9984,1888],"params":["Inherit","False","Property","_StartFoldoutVertexUVOffset","开始折叠-顶点 UV 偏移","179","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":927,"pos":[-9984,1952],"params":["Inherit","False","Property","_EndFoldoutVertexUVOffset","结束折叠-顶点 UV 偏移","184","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":928,"pos":[-9984,2032],"params":["Inherit","False","Property","_StartFoldoutVertexNormalOffset","开始折叠-顶点法线偏移","185","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":929,"pos":[-9984,2096],"params":["Inherit","False","Property","_EndFoldoutVertexNormalOffset","结束折叠-顶点法线偏移","191","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":930,"pos":[-9984,2176],"params":["Inherit","False","Property","_StartFoldoutVertexWave","开始折叠-顶点波动","193","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":931,"pos":[-9984,2240],"params":["Inherit","False","Property","_EndFoldoutVertexWave","结束折叠-顶点波动","199","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":932,"pos":[-9984,2320],"params":["Inherit","False","Property","_StartFoldoutVertexNoise","开始折叠-顶点噪波","200","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":810,"pos":[-1408,5888],"params":["Inherit","False","Property","_VertexNoiseTwistEnabled","启用顶点噪波扭曲","212","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":933,"pos":[-9984,2384],"params":["Inherit","False","Property","_EndFoldoutVertexNoise","结束折叠-顶点噪波","213","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":934,"pos":[-9984,2464],"params":["Inherit","False","Property","_StartFoldoutVertexWaveNoiseVerticalMask","开始折叠-顶点波动-噪波垂直遮罩","214","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":935,"pos":[-9984,2528],"params":["Inherit","False","Property","_EndFoldoutVertexWaveNoiseVerticalMask","结束折叠-顶点波动-噪波垂直遮罩","218","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":936,"pos":[-9984,2608],"params":["Inherit","False","Property","_StartFoldoutVertexOffsetoverY","开始折叠-顶点 Y 向偏移","219","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":937,"pos":[-9984,2672],"params":["Inherit","False","Property","_EndFoldoutVertexOffsetoverY","结束折叠-顶点 Y 向偏移","226","1","[HideInInspector]","Create","False","0","0","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":301,"pos":[-1664,768],"params":["Inherit","False","Property","_Tessellation","曲面细分","227","0","Create","False","0","0","0","True","2","Header(Tessellation)","Space(5)","False","Object","-1","","1","0","1","64","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":2,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ShadowCaster","0","2","ShadowCaster","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","False","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=ShadowCaster","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":3,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthOnly","0","3","DepthOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","True","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","False","False","False","True","1","LightMode=DepthOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":4,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Meta","0","4","Meta","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Meta","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":5,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Universal2D","0","5","Universal2D","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=Universal2D","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":6,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","SceneSelectionPass","0","6","SceneSelectionPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=SceneSelectionPass","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":7,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ScenePickingPass","0","7","ScenePickingPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Picking","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":8,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormals","0","8","DepthNormals","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":9,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormalsOnly","0","9","DepthNormalsOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","True","9","d3d11","metal","vulkan","xboxone","xboxseries","playstation","ps4","ps5","switch","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":0,"pos":[-3456,288],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ExtraPrePass","0","0","ExtraPrePass","6","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","0","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":1,"pos":[-1280,512],"params":["Float","False","True","-1","2","Mirza.Solaris.SolarisGUI","0","19","URP/程序火焰","2992e84f91cbeb14eab234972e07ea9d","True","Forward","0","1","Forward","11","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Transparent=RenderType","Queue=Transparent=Queue=0","UniversalMaterialType=Unlit","True","2","True","12","all","0","False","True","1","5","False","","10","False","","1","1","False","","10","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","2","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalForwardOnly","False","False","0","","0","0","Standard","28","Surface","1","638678258758353849","  Keep Alpha","0","0","  Blend","0","0","Two Sided","0","638679950529090718","Alpha Clipping","0","638678277663027203","  Use Shadow Threshold","0","0","Fragment Normal Space","0","0","Forward Only","0","0","Cast Shadows","0","638678286371960716","Receive Shadows","0","638678286379007585","Receive SSAO","1","0","GPU Instancing","1","0","LOD CrossFade","1","0","Built-in Fog","1","0","Meta Pass","0","0","Extra Pre Pass","0","638682750905672820","Tessellation","1","638682750901786017","  Phong","1","638682750923282078","  Strength","0.5,False,","0","  Type","0","0","  Tess","16,True,_Tessellation","638751898865654623","  Min","10,False,","0","  Max","25,False,","0","  Edge Length","16,False,","0","  Max Displacement","25,False,","0","Write Depth","0","0","  Conservative","0","0","Vertex Position","0","638690283907091048","0","10","False","True","False","True","False","False","True","True","True","False","False","","False","0"]}
{"wire":[1146,0,1144,1]}
{"wire":[1146,1,1144,2]}
{"wire":[1112,0,1111,1]}
{"wire":[1112,1,1111,2]}
{"wire":[1145,0,1146,0]}
{"wire":[1113,0,1112,0]}
{"wire":[1256,0,1145,0]}
{"wire":[1256,1,1113,0]}
{"wire":[1254,0,1256,0]}
{"wire":[338,0,341,0]}
{"wire":[316,0,322,0]}
{"wire":[156,0,145,4]}
{"wire":[620,0,623,0]}
{"wire":[339,1,341,0]}
{"wire":[339,0,338,0]}
{"wire":[1243,0,1257,0]}
{"wire":[1243,1,1247,0]}
{"wire":[157,0,156,0]}
{"wire":[157,2,477,0]}
{"wire":[146,0,145,3]}
{"wire":[321,1,322,0]}
{"wire":[321,0,316,0]}
{"wire":[340,0,339,0]}
{"wire":[327,0,321,0]}
{"wire":[621,1,623,0]}
{"wire":[621,0,620,0]}
{"wire":[1251,0,1243,0]}
{"wire":[147,0,157,0]}
{"wire":[169,0,170,0]}
{"wire":[169,1,168,0]}
{"wire":[622,0,621,0]}
{"wire":[1258,0,1251,0]}
{"wire":[1258,1,1259,0]}
{"wire":[150,0,109,0]}
{"wire":[150,1,151,0]}
{"wire":[150,2,169,0]}
{"wire":[166,0,167,0]}
{"wire":[166,1,165,0]}
{"wire":[1248,0,1258,0]}
{"wire":[171,0,150,0]}
{"wire":[148,0,82,0]}
{"wire":[148,1,149,0]}
{"wire":[148,2,166,0]}
{"wire":[223,1,332,0]}
{"wire":[223,0,342,0]}
{"wire":[163,0,148,0]}
{"wire":[356,1,223,0]}
{"wire":[356,0,624,0]}
{"wire":[1267,0,356,0]}
{"wire":[1267,1,1268,0]}
{"wire":[158,4,323,0]}
{"wire":[158,7,106,0]}
{"wire":[158,8,107,0]}
{"wire":[158,9,108,0]}
{"wire":[158,12,172,0]}
{"wire":[475,0,324,0]}
{"wire":[475,1,476,0]}
{"wire":[220,0,1267,0]}
{"wire":[118,4,158,0]}
{"wire":[118,6,158,15]}
{"wire":[118,7,111,0]}
{"wire":[118,9,112,0]}
{"wire":[443,4,158,0]}
{"wire":[443,6,158,15]}
{"wire":[443,7,111,0]}
{"wire":[612,0,447,0]}
{"wire":[613,0,447,0]}
{"wire":[629,0,137,0]}
{"wire":[160,4,475,0]}
{"wire":[160,7,80,0]}
{"wire":[160,8,84,0]}
{"wire":[160,9,83,0]}
{"wire":[160,12,164,0]}
{"wire":[617,0,613,0]}
{"wire":[66,7,612,0]}
{"wire":[66,6,629,0]}
{"wire":[66,8,61,0]}
{"wire":[66,9,62,0]}
{"wire":[89,4,160,0]}
{"wire":[89,6,160,15]}
{"wire":[89,7,86,0]}
{"wire":[89,9,87,0]}
{"wire":[439,4,160,0]}
{"wire":[439,6,160,15]}
{"wire":[439,7,86,0]}
{"wire":[441,1,443,3]}
{"wire":[441,0,118,3]}
{"wire":[437,5,441,0]}
{"wire":[437,12,119,0]}
{"wire":[611,0,66,0]}
{"wire":[611,2,617,0]}
{"wire":[628,0,447,0]}
{"wire":[440,1,439,3]}
{"wire":[440,0,89,3]}
{"wire":[116,0,437,14]}
{"wire":[116,1,97,0]}
{"wire":[851,10,625,0]}
{"wire":[851,9,798,0]}
{"wire":[436,5,440,0]}
{"wire":[436,12,88,0]}
{"wire":[625,1,628,0]}
{"wire":[625,0,611,0]}
{"wire":[872,13,71,0]}
{"wire":[463,0,872,0]}
{"wire":[331,0,330,0]}
{"wire":[94,0,436,14]}
{"wire":[94,1,103,0]}
{"wire":[736,0,728,1]}
{"wire":[736,1,737,0]}
{"wire":[801,1,625,0]}
{"wire":[801,0,851,0]}
{"wire":[451,0,116,0]}
{"wire":[464,1,872,0]}
{"wire":[464,0,463,0]}
{"wire":[117,0,451,0]}
{"wire":[329,1,330,0]}
{"wire":[329,0,331,0]}
{"wire":[728,0,801,0]}
{"wire":[738,0,736,0]}
{"wire":[738,1,740,0]}
{"wire":[442,0,94,0]}
{"wire":[465,0,464,0]}
{"wire":[328,0,329,0]}
{"wire":[718,0,339,0]}
{"wire":[73,0,442,0]}
{"wire":[735,0,728,2]}
{"wire":[733,2,738,0]}
{"wire":[733,12,730,0]}
{"wire":[734,0,728,0]}
{"wire":[161,0,162,1]}
{"wire":[122,0,102,0]}
{"wire":[719,0,718,0]}
{"wire":[178,0,179,0]}
{"wire":[178,1,177,0]}
{"wire":[732,0,734,0]}
{"wire":[732,1,733,14]}
{"wire":[732,2,735,0]}
{"wire":[99,0,343,0]}
{"wire":[99,1,122,0]}
{"wire":[690,0,321,0]}
{"wire":[152,0,19,0]}
{"wire":[152,1,153,0]}
{"wire":[152,2,178,0]}
{"wire":[70,0,732,0]}
{"wire":[70,1,466,0]}
{"wire":[70,2,95,0]}
{"wire":[125,0,99,0]}
{"wire":[125,1,126,0]}
{"wire":[183,1,181,0]}
{"wire":[183,2,184,0]}
{"wire":[691,0,690,0]}
{"wire":[726,0,717,0]}
{"wire":[726,1,722,0]}
{"wire":[175,0,152,0]}
{"wire":[173,0,70,0]}
{"wire":[121,0,125,0]}
{"wire":[121,1,123,0]}
{"wire":[180,0,120,0]}
{"wire":[180,1,183,0]}
{"wire":[185,0,57,0]}
{"wire":[725,0,726,0]}
{"wire":[725,1,724,0]}
{"wire":[749,0,746,0]}
{"wire":[749,1,747,0]}
{"wire":[53,10,121,0]}
{"wire":[53,8,180,0]}
{"wire":[53,9,185,0]}
{"wire":[751,0,749,0]}
{"wire":[751,1,748,0]}
{"wire":[159,4,174,0]}
{"wire":[159,7,16,0]}
{"wire":[159,8,72,0]}
{"wire":[159,9,18,0]}
{"wire":[159,12,176,0]}
{"wire":[716,1,699,0]}
{"wire":[716,0,725,0]}
{"wire":[566,0,53,0]}
{"wire":[566,1,567,0]}
{"wire":[862,20,716,0]}
{"wire":[862,4,347,0]}
{"wire":[862,7,346,0]}
{"wire":[862,23,345,0]}
{"wire":[742,0,862,0]}
{"wire":[767,1,750,0]}
{"wire":[767,0,751,0]}
{"wire":[24,4,159,0]}
{"wire":[24,6,159,15]}
{"wire":[24,7,13,0]}
{"wire":[24,9,14,0]}
{"wire":[445,4,159,0]}
{"wire":[445,6,159,15]}
{"wire":[445,7,13,0]}
{"wire":[484,0,162,2]}
{"wire":[484,1,485,0]}
{"wire":[55,0,566,0]}
{"wire":[863,20,767,0]}
{"wire":[863,4,754,0]}
{"wire":[863,7,752,0]}
{"wire":[863,23,753,0]}
{"wire":[186,0,187,0]}
{"wire":[480,0,484,0]}
{"wire":[757,0,863,0]}
{"wire":[444,1,445,0]}
{"wire":[444,0,24,0]}
{"wire":[713,1,862,0]}
{"wire":[713,0,742,0]}
{"wire":[350,0,713,0]}
{"wire":[219,0,186,0]}
{"wire":[208,0,210,0]}
{"wire":[217,0,186,0]}
{"wire":[864,20,444,0]}
{"wire":[864,4,22,0]}
{"wire":[864,7,564,0]}
{"wire":[864,23,565,0]}
{"wire":[315,0,314,0]}
{"wire":[315,1,313,0]}
{"wire":[601,0,55,0]}
{"wire":[758,1,863,0]}
{"wire":[758,0,757,0]}
{"wire":[227,0,601,0]}
{"wire":[478,0,864,0]}
{"wire":[478,1,479,0]}
{"wire":[211,0,208,0]}
{"wire":[759,0,758,0]}
{"wire":[1125,1,1126,0]}
{"wire":[1125,2,1148,0]}
{"wire":[1125,3,1255,0]}
{"wire":[1125,4,1129,0]}
{"wire":[1125,5,1130,0]}
{"wire":[1125,6,1131,0]}
{"wire":[1125,7,1132,0]}
{"wire":[1125,8,1133,0]}
{"wire":[1125,9,1134,0]}
{"wire":[1080,1,1081,0]}
{"wire":[1080,2,1150,0]}
{"wire":[1080,3,1085,0]}
{"wire":[1080,4,1089,0]}
{"wire":[1080,5,1123,0]}
{"wire":[1080,6,1110,0]}
{"wire":[1080,7,1115,0]}
{"wire":[1080,8,1088,0]}
{"wire":[309,0,300,0]}
{"wire":[309,1,310,0]}
{"wire":[309,2,315,0]}
{"wire":[218,1,219,0]}
{"wire":[218,0,217,0]}
{"wire":[703,0,942,0]}
{"wire":[703,1,704,0]}
{"wire":[571,0,478,0]}
{"wire":[191,0,192,0]}
{"wire":[191,1,193,0]}
{"wire":[866,20,211,0]}
{"wire":[866,4,209,0]}
{"wire":[865,20,218,0]}
{"wire":[865,4,190,0]}
{"wire":[984,1,1080,10]}
{"wire":[984,2,980,0]}
{"wire":[993,1,1125,11]}
{"wire":[993,2,996,0]}
{"wire":[311,0,309,0]}
{"wire":[709,1,942,0]}
{"wire":[709,0,703,0]}
{"wire":[569,1,571,0]}
{"wire":[569,2,570,0]}
{"wire":[214,0,865,0]}
{"wire":[214,1,866,0]}
{"wire":[568,0,191,0]}
{"wire":[1005,0,1007,0]}
{"wire":[232,3,235,0]}
{"wire":[761,0,741,0]}
{"wire":[761,1,762,0]}
{"wire":[981,0,984,0]}
{"wire":[994,0,993,0]}
{"wire":[605,1,452,0]}
{"wire":[605,0,606,0]}
{"wire":[942,0,684,0]}
{"wire":[11,0,569,0]}
{"wire":[216,0,214,0]}
{"wire":[868,20,568,0]}
{"wire":[868,4,196,0]}
{"wire":[1006,0,1005,0]}
{"wire":[234,0,232,0]}
{"wire":[234,1,236,0]}
{"wire":[234,2,237,0]}
{"wire":[760,1,741,0]}
{"wire":[760,0,761,0]}
{"wire":[642,0,641,0]}
{"wire":[590,0,588,0]}
{"wire":[336,0,335,0]}
{"wire":[285,0,276,0]}
{"wire":[285,1,284,0]}
{"wire":[258,0,260,0]}
{"wire":[292,4,605,0]}
{"wire":[292,7,294,0]}
{"wire":[292,8,296,0]}
{"wire":[292,9,295,0]}
{"wire":[292,12,312,0]}
{"wire":[741,1,942,0]}
{"wire":[741,0,709,0]}
{"wire":[195,0,868,0]}
{"wire":[188,0,216,0]}
{"wire":[1008,20,1006,0]}
{"wire":[1008,4,1010,0]}
{"wire":[1008,7,1016,0]}
{"wire":[1008,23,1017,0]}
{"wire":[242,1,234,0]}
{"wire":[242,2,243,0]}
{"wire":[1076,1,1078,0]}
{"wire":[1076,0,1047,0]}
{"wire":[1077,1,1078,0]}
{"wire":[1077,0,1046,0]}
{"wire":[643,0,642,0]}
{"wire":[591,0,590,0]}
{"wire":[867,20,700,0]}
{"wire":[867,4,279,0]}
{"wire":[867,7,305,0]}
{"wire":[867,23,306,0]}
{"wire":[251,0,336,1]}
{"wire":[251,1,595,0]}
{"wire":[251,2,254,0]}
{"wire":[583,0,258,0]}
{"wire":[583,1,285,0]}
{"wire":[657,0,644,1]}
{"wire":[302,4,292,0]}
{"wire":[302,6,292,15]}
{"wire":[302,7,858,0]}
{"wire":[302,9,303,0]}
{"wire":[489,4,292,0]}
{"wire":[489,6,292,15]}
{"wire":[489,7,858,0]}
{"wire":[766,1,741,0]}
{"wire":[766,0,760,0]}
{"wire":[1011,0,1008,0]}
{"wire":[56,0,41,0]}
{"wire":[56,1,766,0]}
{"wire":[233,0,242,0]}
{"wire":[1024,0,1019,4]}
{"wire":[1054,0,1076,0]}
{"wire":[1054,1,1077,0]}
{"wire":[644,0,643,0]}
{"wire":[587,0,591,0]}
{"wire":[280,0,867,0]}
{"wire":[584,0,251,0]}
{"wire":[584,1,583,0]}
{"wire":[646,0,657,0]}
{"wire":[646,1,662,0]}
{"wire":[597,0,587,1]}
{"wire":[597,1,661,0]}
{"wire":[490,1,489,3]}
{"wire":[490,0,302,3]}
{"wire":[200,0,199,0]}
{"wire":[143,0,142,0]}
{"wire":[240,0,128,0]}
{"wire":[58,0,56,0]}
{"wire":[701,0,197,0]}
{"wire":[1061,0,1054,0]}
{"wire":[1061,1,1063,0]}
{"wire":[249,0,584,0]}
{"wire":[648,0,644,0]}
{"wire":[648,1,646,0]}
{"wire":[649,0,663,0]}
{"wire":[574,0,587,0]}
{"wire":[574,1,597,0]}
{"wire":[604,0,660,0]}
{"wire":[682,0,694,0]}
{"wire":[895,0,894,0]}
{"wire":[852,10,490,0]}
{"wire":[852,9,789,0]}
{"wire":[127,0,58,0]}
{"wire":[127,1,239,0]}
{"wire":[127,2,143,0]}
{"wire":[127,3,701,0]}
{"wire":[127,4,200,0]}
{"wire":[127,5,240,0]}
{"wire":[1029,0,1027,0]}
{"wire":[1029,1,1028,0]}
{"wire":[1029,2,1061,0]}
{"wire":[266,0,249,0]}
{"wire":[266,1,267,0]}
{"wire":[266,2,281,0]}
{"wire":[650,0,648,0]}
{"wire":[650,1,649,0]}
{"wire":[576,0,574,0]}
{"wire":[576,1,604,0]}
{"wire":[856,0,852,0]}
{"wire":[669,0,693,0]}
{"wire":[669,1,668,0]}
{"wire":[680,0,682,0]}
{"wire":[680,1,679,0]}
{"wire":[896,0,895,0]}
{"wire":[1012,0,127,0]}
{"wire":[1012,1,1029,0]}
{"wire":[282,0,266,0]}
{"wire":[282,1,264,0]}
{"wire":[594,1,576,0]}
{"wire":[575,0,576,0]}
{"wire":[652,1,650,0]}
{"wire":[653,0,650,0]}
{"wire":[289,0,856,0]}
{"wire":[289,1,290,0]}
{"wire":[289,2,297,0]}
{"wire":[245,0,244,0]}
{"wire":[245,1,246,0]}
{"wire":[676,0,680,0]}
{"wire":[676,1,677,0]}
{"wire":[676,2,681,0]}
{"wire":[665,0,669,0]}
{"wire":[665,1,666,0]}
{"wire":[665,2,667,0]}
{"wire":[897,0,896,0]}
{"wire":[948,0,944,0]}
{"wire":[1030,0,1012,0]}
{"wire":[593,1,575,0]}
{"wire":[593,0,594,0]}
{"wire":[654,1,653,0]}
{"wire":[654,0,652,0]}
{"wire":[674,0,245,0]}
{"wire":[674,1,665,0]}
{"wire":[674,2,676,0]}
{"wire":[448,0,282,0]}
{"wire":[450,0,289,0]}
{"wire":[945,0,1030,0]}
{"wire":[945,1,948,0]}
{"wire":[1033,0,1030,0]}
{"wire":[712,0,943,0]}
{"wire":[712,1,353,0]}
{"wire":[265,0,448,0]}
{"wire":[579,0,593,0]}
{"wire":[655,0,654,0]}
{"wire":[298,0,450,0]}
{"wire":[670,0,674,0]}
{"wire":[636,0,633,0]}
{"wire":[632,0,692,0]}
{"wire":[632,1,631,0]}
{"wire":[780,0,777,0]}
{"wire":[776,0,899,0]}
{"wire":[776,1,775,0]}
{"wire":[888,0,902,0]}
{"wire":[888,1,886,0]}
{"wire":[943,1,945,0]}
{"wire":[943,0,1033,0]}
{"wire":[711,1,712,0]}
{"wire":[711,0,943,0]}
{"wire":[764,0,744,0]}
{"wire":[764,1,765,0]}
{"wire":[638,0,636,0]}
{"wire":[638,1,632,0]}
{"wire":[893,0,889,0]}
{"wire":[893,1,888,0]}
{"wire":[782,0,780,0]}
{"wire":[782,1,776,0]}
{"wire":[744,1,943,0]}
{"wire":[744,0,711,0]}
{"wire":[763,1,764,0]}
{"wire":[763,0,744,0]}
{"wire":[269,0,671,0]}
{"wire":[269,1,268,0]}
{"wire":[269,2,299,0]}
{"wire":[269,3,580,0]}
{"wire":[269,4,656,0]}
{"wire":[785,0,638,0]}
{"wire":[785,1,782,0]}
{"wire":[785,2,893,0]}
{"wire":[768,1,744,0]}
{"wire":[768,0,763,0]}
{"wire":[982,0,768,0]}
{"wire":[982,1,983,0]}
{"wire":[819,0,269,0]}
{"wire":[819,1,816,0]}
{"wire":[639,0,785,0]}
{"wire":[854,0,818,0]}
{"wire":[999,0,985,0]}
{"wire":[999,1,1000,0]}
{"wire":[853,10,819,0]}
{"wire":[853,9,854,0]}
{"wire":[985,1,768,0]}
{"wire":[985,0,982,0]}
{"wire":[857,0,853,0]}
{"wire":[857,1,640,0]}
{"wire":[998,1,985,0]}
{"wire":[998,0,999,0]}
{"wire":[49,0,998,0]}
{"wire":[247,0,857,0]}
{"wire":[560,0,557,4]}
{"wire":[562,0,561,0]}
{"wire":[562,1,479,0]}
{"wire":[510,1,445,3]}
{"wire":[510,0,24,3]}
{"wire":[509,5,510,0]}
{"wire":[509,12,22,0]}
{"wire":[509,17,564,0]}
{"wire":[509,18,565,0]}
{"wire":[511,0,509,14]}
{"wire":[559,0,557,1]}
{"wire":[36,22,30,0]}
{"wire":[37,22,29,0]}
{"wire":[31,0,36,0]}
{"wire":[31,1,37,0]}
{"wire":[31,2,43,0]}
{"wire":[132,0,130,0]}
{"wire":[132,1,135,0]}
{"wire":[132,2,136,0]}
{"wire":[370,0,132,0]}
{"wire":[370,1,401,0]}
{"wire":[370,2,391,0]}
{"wire":[44,0,33,0]}
{"wire":[44,1,35,0]}
{"wire":[42,0,44,0]}
{"wire":[385,22,389,0]}
{"wire":[386,22,388,0]}
{"wire":[387,0,385,0]}
{"wire":[387,1,386,0]}
{"wire":[387,2,390,0]}
{"wire":[398,0,402,1]}
{"wire":[398,1,395,0]}
{"wire":[399,0,402,2]}
{"wire":[399,1,396,0]}
{"wire":[400,0,402,3]}
{"wire":[400,1,397,0]}
{"wire":[769,2,738,0]}
{"wire":[769,12,730,0]}
{"wire":[769,17,771,0]}
{"wire":[769,18,773,0]}
{"wire":[805,1,611,0]}
{"wire":[805,2,808,0]}
{"wire":[805,3,809,0]}
{"wire":[808,0,807,0]}
{"wire":[809,0,806,0]}
{"wire":[402,0,387,0]}
{"wire":[401,0,398,0]}
{"wire":[401,1,399,0]}
{"wire":[401,2,400,0]}
{"wire":[951,0,139,0]}
{"wire":[869,20,698,0]}
{"wire":[869,4,364,0]}
{"wire":[869,7,362,0]}
{"wire":[869,23,363,0]}
{"wire":[369,0,869,0]}
{"wire":[393,0,392,0]}
{"wire":[129,0,31,0]}
{"wire":[136,0,129,3]}
{"wire":[136,1,68,0]}
{"wire":[135,0,129,2]}
{"wire":[135,1,134,0]}
{"wire":[130,0,129,1]}
{"wire":[130,1,131,0]}
{"wire":[949,0,139,0]}
{"wire":[141,0,950,0]}
{"wire":[950,0,956,0]}
{"wire":[950,1,955,0]}
{"wire":[950,2,949,3]}
{"wire":[140,0,394,0]}
{"wire":[140,1,952,0]}
{"wire":[1067,0,394,0]}
{"wire":[1071,0,1067,2]}
{"wire":[1071,1,1069,0]}
{"wire":[1072,0,1067,1]}
{"wire":[1072,1,1070,0]}
{"wire":[1068,0,1067,3]}
{"wire":[1068,1,1066,0]}
{"wire":[1073,0,1072,0]}
{"wire":[1073,1,1071,0]}
{"wire":[1073,2,1068,0]}
{"wire":[1041,0,1020,0]}
{"wire":[1075,0,1073,0]}
{"wire":[1075,1,1019,5]}
{"wire":[241,0,234,0]}
{"wire":[891,0,889,0]}
{"wire":[955,0,949,2]}
{"wire":[955,1,953,0]}
{"wire":[956,0,949,1]}
{"wire":[956,1,954,0]}
{"wire":[492,0,140,0]}
{"wire":[492,1,496,0]}
{"wire":[50,0,1018,0]}
{"wire":[1018,0,492,0]}
{"wire":[1018,1,1075,0]}
{"wire":[1018,2,1041,0]}
{"wire":[1102,1,1106,0]}
{"wire":[1102,3,1105,0]}
{"wire":[1103,1,1106,0]}
{"wire":[1103,3,1102,0]}
{"wire":[1104,1,1106,0]}
{"wire":[1104,3,1103,0]}
{"wire":[1106,0,1109,0]}
{"wire":[1172,0,1138,0]}
{"wire":[1171,0,507,0]}
{"wire":[528,0,518,0]}
{"wire":[518,0,506,0]}
{"wire":[523,20,528,0]}
{"wire":[523,110,508,0]}
{"wire":[507,20,506,0]}
{"wire":[507,110,508,0]}
{"wire":[1138,0,523,0]}
{"wire":[1118,1,1124,0]}
{"wire":[1197,1,1124,0]}
{"wire":[1197,2,1201,0]}
{"wire":[1218,1,1124,0]}
{"wire":[1218,2,1219,0]}
{"wire":[1206,0,1218,4]}
{"wire":[1213,1,1206,0]}
{"wire":[1213,2,1195,0]}
{"wire":[1220,1,1206,0]}
{"wire":[1220,0,1213,0]}
{"wire":[1223,1,1208,0]}
{"wire":[1223,0,1214,0]}
{"wire":[1214,0,1207,0]}
{"wire":[1214,1,1208,0]}
{"wire":[1214,2,1195,0]}
{"wire":[1216,0,1215,0]}
{"wire":[1217,0,1220,0]}
{"wire":[1228,0,1235,0]}
{"wire":[1229,0,1235,0]}
{"wire":[1230,1,1232,0]}
{"wire":[1230,2,1233,0]}
{"wire":[1231,0,1230,0]}
{"wire":[1231,1,1234,0]}
{"wire":[1236,0,1228,0]}
{"wire":[1236,1,1229,0]}
{"wire":[1236,2,1231,0]}
{"wire":[1237,0,1227,0]}
{"wire":[1237,1,1236,0]}
{"wire":[502,0,1225,0]}
{"wire":[502,1,1117,0]}
{"wire":[502,2,505,0]}
{"wire":[502,3,1205,0]}
{"wire":[988,0,502,0]}
{"wire":[1225,1,1227,0]}
{"wire":[1225,0,1237,0]}
{"wire":[1203,1,1120,0]}
{"wire":[1203,2,1204,0]}
{"wire":[1205,1,1203,4]}
{"wire":[1205,2,1121,0]}
{"wire":[1215,1,1207,0]}
{"wire":[1215,0,1223,0]}
{"wire":[963,1,1080,10]}
{"wire":[963,2,965,0]}
{"wire":[1116,0,963,0]}
{"wire":[1183,0,1137,5]}
{"wire":[1199,1,1183,0]}
{"wire":[1199,2,1169,0]}
{"wire":[1221,1,1183,0]}
{"wire":[1221,0,1199,0]}
{"wire":[1198,1,1179,0]}
{"wire":[1198,0,1222,0]}
{"wire":[1175,0,1198,0]}
{"wire":[1176,0,1179,0]}
{"wire":[1176,1,1182,0]}
{"wire":[1176,2,1169,0]}
{"wire":[1222,1,1182,0]}
{"wire":[1222,0,1176,0]}
{"wire":[1180,0,1221,0]}
{"wire":[500,0,494,0]}
{"wire":[500,1,1079,0]}
{"wire":[495,0,500,0]}
{"wire":[1200,1,1184,0]}
{"wire":[1200,0,527,0]}
{"wire":[540,0,1185,0]}
{"wire":[543,0,1185,0]}
{"wire":[1141,1,549,0]}
{"wire":[1141,2,1142,0]}
{"wire":[556,0,1141,0]}
{"wire":[556,1,555,0]}
{"wire":[542,0,540,0]}
{"wire":[542,1,543,0]}
{"wire":[542,2,556,0]}
{"wire":[527,0,1184,0]}
{"wire":[527,1,542,0]}
{"wire":[497,0,1200,0]}
{"wire":[497,1,1224,0]}
{"wire":[497,2,498,0]}
{"wire":[552,0,497,0]}
{"wire":[992,1,1147,0]}
{"wire":[992,2,1126,0]}
{"wire":[1153,0,1241,0]}
{"wire":[1241,1,1125,11]}
{"wire":[1241,2,1242,0]}
{"wire":[1100,0,1098,3]}
{"wire":[1100,1,1098,4]}
{"wire":[1100,2,1099,1]}
{"wire":[1108,0,1099,2]}
{"wire":[1108,1,1099,3]}
{"wire":[1108,2,1099,4]}
{"wire":[1101,0,1100,0]}
{"wire":[1107,0,1108,0]}
{"wire":[1250,0,621,0]}
{"wire":[1250,1,1260,0]}
{"wire":[1262,0,339,0]}
{"wire":[1262,1,1261,0]}
{"wire":[1263,0,329,0]}
{"wire":[1263,1,1264,0]}
{"wire":[1266,0,321,0]}
{"wire":[1266,1,1265,0]}
{"wire":[1135,1,1152,0]}
{"wire":[1135,2,1149,0]}
{"wire":[1135,3,1173,0]}
{"wire":[1137,1,1152,0]}
{"wire":[1137,2,1149,0]}
{"wire":[1137,3,1174,0]}
{"wire":[1167,1,1152,0]}
{"wire":[1167,2,1149,0]}
{"wire":[494,0,989,0]}
{"wire":[1207,0,1118,3]}
{"wire":[1208,0,1197,4]}
{"wire":[1079,0,553,0]}
{"wire":[1179,0,1167,4]}
{"wire":[1182,0,1135,5]}
{"wire":[558,1,478,0]}
{"wire":[558,0,562,0]}
{"wire":[392,1,132,0]}
{"wire":[392,0,370,0]}
{"wire":[770,1,733,14]}
{"wire":[770,0,769,14]}
{"wire":[810,1,490,0]}
{"wire":[810,0,852,0]}
{"wire":[1,2,51,0]}
{"wire":[1,3,52,0]}
{"wire":[1,5,248,0]}
ASEEND*/

//CHKSM=5C8785CF64D9BE215B16BC3B70D9FAD069AA5D75



