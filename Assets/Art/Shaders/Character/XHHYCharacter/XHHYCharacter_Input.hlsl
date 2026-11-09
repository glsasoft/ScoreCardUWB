/*
 * 文件名: XHHYCharacter_Input.hlsl
 * 作用: XHHY角色渲染系统的输入数据结构和材质属性定义
 * 作者: XHHY团队
 * 日期: 2024
 * 
 * 描述: 
 * 该文件定义了角色渲染系统的核心数据结构，包括：
 * - 顶点属性输入结构体（Attributes）
 * - 顶点到片段着色器的插值数据结构体（Varyings）
 * - 材质属性常量缓冲区（CBUFFER）
 * - 纹理采样器和贴图声明
 * - 表面数据和光照数据结构体
 * - 工具函数和调试功能
 */


#ifndef CharacterInput
#define CharacterInput


// 因为_DETAIL总是对细节法线图进行采样
// 当启用以下任一功能时，需要切线空间数据
#if _NORMALMAP || _DETAIL || _KAJIYAKAY_SPECULAR || _DYNAMIC_EYE || _PARALLAXMAP
    #define VaryingsHasTangentWS 1 // 标记需要世界空间切线数据
#endif

    #define AnyOcclusionEnabled 1 // 启用遮挡功能

// 检查是否启用了任何基础贴图堆叠层
#if _BASEMAP_STACKING_LAYER1 || _BASEMAP_STACKING_LAYER2 || _BASEMAP_STACKING_LAYER3 || _BASEMAP_STACKING_LAYER4 || _BASEMAP_STACKING_LAYER5 || _BASEMAP_STACKING_LAYER6 || _BASEMAP_STACKING_LAYER7 || _BASEMAP_STACKING_LAYER8 || _BASEMAP_STACKING_LAYER9 || _BASEMAP_STACKING_LAYER10
    #define AnyBaseMapStackingLayerEnabled 1 // 标记启用了基础贴图堆叠
#endif

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

/**
 * 顶点属性输入结构体
 * 定义从Unity应用程序传递到顶点着色器的数据
 * 
 * 成员说明:
 * - positionOS: 对象空间顶点位置
 * - normalOS: 对象空间法线（使用float3以避免类型转换开销）
 * - tangentOS: 对象空间切线（使用float4以避免类型转换开销）
 * - uv: 主纹理坐标
 * - uv2: 第二套纹理坐标（用于细节贴图）
 * - uv8: 第八套纹理坐标（存储轮廓角度平滑的切线空间法线）
 * - color: 顶点颜色（主要用于轮廓宽度遮罩）
 * - vertexID: 顶点ID（必须为uint类型）
 */
struct Attributes
{
    float3 positionOS : POSITION;      // 对象空间位置
    float3 normalOS : NORMAL;          // 对象空间法线
    float4 tangentOS : TANGENT;        // 对象空间切线（w分量为符号位）
    float2 uv : TEXCOORD0;             // 主UV坐标
    float2 uv2 : TEXCOORD1;            // 第二套UV坐标，细节贴图可能需要
    float3 uv8 : TEXCOORD7;            // 用于存储轮廓角度平滑的切线空间法线
    float4 color : COLOR;              // 顶点颜色，用于轮廓宽度遮罩而非颜色显示
    uint vertexID : SV_VertexID;       // 系统顶点ID
};

/**
 * 顶点着色器到片段着色器的插值数据结构体
 * 
 * 成员说明:
 * - positionCS: 裁剪空间位置
 * - uv: 主纹理坐标
 * - positionWS_ZOffsetFinalSum: 世界空间位置和Z偏移总和
 * - SH_fogFactor: 球谐光照和雾效因子
 * - normalWS_averageShadowAttenuation: 世界空间法线和平均阴影衰减
 * - viewDirectionWS: 世界空间视线方向
 * - uv2: 第二套UV坐标
 * - depth: 深度值
 * - tangentWS: 世界空间切线（仅在启用法线贴图时）
 * - color: 顶点颜色
 * - uv8: 调试用的第八套UV坐标
 */
struct Varyings
{
    float4 positionCS : SV_POSITION;   // 裁剪空间位置

    float2 uv : TEXCOORD0;             // 主UV坐标
    // 需要使用float而不是half，当纹理尺寸大于2048时，half精度不够
    
    float4 positionWS_ZOffsetFinalSum : TEXCOORD1; // xyz = 世界空间位置, w = Z偏移总和

    half4 SH_fogFactor : TEXCOORD2;    // xyz: 球谐光照采样结果, w: 雾效因子
    // (4个half打包到1个TEXCOORD中)
    
    half4 normalWS_averageShadowAttenuation : TEXCOORD3; // xyz: 世界空间法线, w: 平均阴影衰减
    // (4个half打包到1个TEXCOORD中)

    half3 viewDirectionWS : TEXCOORD4; // 世界空间视线方向
    
    float2 uv2 : TEXCOORD5;            // 第二套UV坐标（用于贴花等）

    float depth : TEXCOORD6;           // 深度值
    
    #if VaryingsHasTangentWS
    half4 tangentWS : TANGENT;         // xyz: 切线, w: 符号位
    // TODO: 使用TANGENT语义是否安全？在URP的LitForwardPass.hlsl中使用的是TEXCOORD4
    #endif

    half4 color : COLOR;               // 顶点颜色RGBA，用于深度纹理边缘光和阴影宽度控制或调试着色

    // 调试用途
    float3 uv8 : TEXCOORD8;

    // 溶解用：局部 Y 轴归一化高度（0 = 头顶，1 = 脚底）
    half dissolveHeight : TEXCOORD9;

    // 为了支持GPU实例化和单通道立体渲染（VR），添加以下部分
    UNITY_VERTEX_INPUT_INSTANCE_ID     // 在非OpenGL和非PSL中会变成：uint instanceID : SV_InstanceID
    UNITY_VERTEX_OUTPUT_STEREO         // 在非OpenGL和非PSL中会变成：uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex
};

// 一些复用的采样器声明
SAMPLER(sampler_linear_clamp);         // 线性插值，边缘夹紧采样器
SAMPLER(sampler_Point_clamp);          // 点采样，边缘夹紧采样器
SAMPLER(sampler_linear_repeat);        // 线性插值，重复采样器

// 主要纹理和采样器声明
TEXTURE2D(_BaseMap);                   // 基础颜色贴图
SAMPLER(sampler_BaseMap);              // 基础颜色贴图采样器
TEXTURE2D(_MaskMap);                   // 遮罩贴图
TEXTURE2D(_DecalMap);                  // 贴花贴图
SAMPLER(sampler_DecalMap);             // 贴花贴图采样器
TEXTURE2D(_TESTTEX);                   // 测试纹理
SAMPLER(sampler_TESTTEX);              // 测试纹理采样器
TEXTURE2D(_BumpMap);                   // 法线贴图
SAMPLER(sampler_BumpMap);              // 法线贴图采样器

// 基础贴图堆叠层纹理声明
#if _BASEMAP_STACKING_LAYER1
    TEXTURE2D(_BaseMapStackingLayer1Tex);     // 堆叠层1纹理
    SAMPLER(sampler_BaseMapStackingLayer1Tex); // 只有前两层使用自己的采样器，给用户更多控制
    TEXTURE2D(_BaseMapStackingLayer1MaskTex); // 堆叠层1遮罩纹理
#endif

#if _BASEMAP_STACKING_LAYER2
    TEXTURE2D(_BaseMapStackingLayer2Tex);     // 堆叠层2纹理
    SAMPLER(sampler_BaseMapStackingLayer2Tex); // 只有前两层使用自己的采样器，给用户更多控制
    TEXTURE2D(_BaseMapStackingLayer2MaskTex); // 堆叠层2遮罩纹理
#endif

// 自发光贴图
#if _EMISSION
    sampler2D _EmissionMap;            // 自发光贴图
#endif

// MatCap相关纹理
sampler2D _MatCapAdditiveMap;          // MatCap添加模式贴图
TEXTURE2D(_MatCapAdditiveMaskMap);     // MatCap添加模式遮罩贴图

// 光照渐变纹理
TEXTURE2D(_RampLightingTex);           // 光照渐变纹理

// 高光贴图
sampler2D _SpecularMap;                // 高光贴图

// 溶解效果纹理
#if _NILOTOON_DISSOLVE
    sampler2D _DissolveThresholdMap;   // 溶解阈值贴图
#endif

// 每角色基础贴图覆盖
#if _NILOTOON_PERCHARACTER_BASEMAP_OVERRIDE
    TEXTURE2D(_PerCharacterBaseMapOverrideMap); // 每角色基础贴图覆盖贴图
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// NiloToon的全局纹理
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////  

/**
 * Unity材质属性常量缓冲区
 * 包含所有材质相关的属性和参数
 * 
 * 分类说明:
 * 1. 基础渲染控制
 * 2. 颜色和透明度
 * 3. 法线贴图
 * 4. 高光反射
 * 5. MatCap效果
 * 6. 遮挡和阴影
 * 7. 光照样式
 * 8. 轮廓渲染
 * 9. 特效和动画
 */
CBUFFER_START(UnityPerMaterial)

// 基础渲染控制
float _HairAlpha;                      // 头发透明度 [0,1]
float3 _ShadowColor;                   // 阴影颜色 RGB
float _EnableRendering;                // 启用渲染开关 [0,1]

// 方向光染色
half _SunTintEnable;                   // 是否受方向光染色 [0,1]
half _SunTintAmount;                   // 方向光染色程度 [0,1]

// 贴花相关
float4 _DecalColor;                    // 贴花颜色 RGBA
float _LineNumber;                     // 行数
float _ColumnNumber;                   // 列数
float _Sequence;                       // 序列

// 基础颜色属性
float4 _BaseMap_ST;                    // 基础贴图的平铺和偏移 (xy=平铺, zw=偏移)
half4 _BaseColor;                      // 基础颜色 RGBA
float _Alpha;                          // 全局透明度 [0,1]
half3 _PerCharacterBaseColorTint;      // 每角色基础颜色色调 RGB

// 背面着色
half3 _BackFaceTintColor;              // 背面色调颜色 RGB

// 透明度覆盖
half _AlphaOverrideStrength;           // 透明度覆盖强度 [0,1]
half _ApplyAlphaOverrideOnlyWhenFaceForwardIsPointingToCamera; // 仅当面朝摄像机时应用透明度覆盖 [0,1]
half4 _AlphaOverrideTexChannelMask;    // 透明度覆盖纹理通道遮罩 RGBA

// Alpha测试
half _Cutoff;                          // Alpha裁剪阈值 [0,1]

// 视差贴图
half _Parallax;                        // 视差强度

// 法线贴图
float _BumpScale;                      // 法线贴图强度

// 基础贴图Alpha混合层（1-10）
half4 _BaseMapStackingLayer1TintColor; // 堆叠层1色调颜色 RGBA
float2 _BaseMapStackingLayer1TexUVAnimSpeed; // 堆叠层1 UV动画速度 XY
float4 _BaseMapStackingLayer1TexUVScaleOffset; // 堆叠层1 UV缩放偏移 (xy=缩放, zw=偏移)
half4 _BaseMapStackingLayer1MaskTexChannel; // 堆叠层1遮罩纹理通道 RGBA
float _BaseMapStackingLayer1TexUVToUV2; // 堆叠层1纹理UV到UV2的映射
half _IsAddColor;                      // 是否为添加颜色模式 [0,1]

// 注意：由于vulkan（android Mali GPU设备）的常量缓冲区大小限制
// 我们在vulkan上硬编码移除了basemap堆叠层3-10，以使cbuffer更小
// 有问题的设备列表：(vivo Y73s) (vivo Y72) (TAS_AN00) (mate40) (华为Honor 9x)
// 所有设备都是Mali-G__ GPU + vulkan
// 而"Adreno GPU + vulkan"是安全的（小米8）（三星A70），"OpenGLES + 任何GPU"也是安全的

// 自发光属性
float4 _EmissionMapTilingXyOffsetZw;   // 自发光贴图平铺和偏移 (xy=平铺, zw=偏移)
half _EmissionIntensity;               // 自发光强度
half3 _EmissionColor;                  // 自发光颜色 RGB
half _MultiplyBaseColorToEmissionColor; // 基础颜色与自发光颜色的混合程度 [0,1]
half _EmissionMapUseSingleChannelOnly; // 自发光贴图仅使用单通道 [0,1]
half4 _EmissionMapSingleChannelMask;   // 自发光贴图单通道遮罩 RGBA

// MatCap (Alpha混合模式)
half _MatCapAlphaBlendUsage;           // MatCap Alpha混合使用程度 [0,1]
half4 _MatCapAlphaBlendTintColor;      // MatCap Alpha混合色调颜色 RGBA
float _MatCapAlphaBlendUvScale;        // MatCap Alpha混合UV缩放
half _MatCapAlphaBlendMapAlphaAsMask;  // MatCap Alpha混合贴图Alpha作为遮罩 [0,1]
half4 _MatCapAlphaBlendMaskMapChannelMask; // MatCap Alpha混合遮罩贴图通道遮罩 RGBA
half _MatCapAlphaBlendMaskMapRemapStart; // MatCap Alpha混合遮罩贴图重映射起始值 [0,1]
half _MatCapAlphaBlendMaskMapRemapEnd; // MatCap Alpha混合遮罩贴图重映射结束值 [0,1]
half _MetallicColorIntensity;          // 金属颜色强度 [0,1]

// MatCap (添加模式)
half _MatCapAdditiveMapAlphaAsMask;    // MatCap添加模式贴图Alpha作为遮罩 [0,1]
half _MatCapAdditiveIntensity;         // MatCap添加模式强度
half4 _MatCapAdditiveColor;            // MatCap添加模式颜色 RGBA
half _MatCapAdditiveMixWithBaseMapColor; // MatCap添加模式与基础贴图颜色混合程度 [0,1]
float _MatCapAdditiveUvScale;          // MatCap添加模式UV缩放
half4 _MatCapAdditiveMaskMapChannelMask; // MatCap添加模式遮罩贴图通道遮罩 RGBA
half _MatCapAdditiveMaskMapRemapStart; // MatCap添加模式遮罩贴图重映射起始值 [0,1]
half _MatCapAdditiveMaskMapRemapEnd;   // MatCap添加模式遮罩贴图重映射结束值 [0,1]

// 金属高光属性
half _MetallicSpecularIntensity;       // 金属高光强度
half4 _MetallicSpecularColor;          // 金属高光颜色 RGBA
half _MetallicRoughness;               // 金属粗糙度 [0,8]

half _MetallicMultiplyBaseColorToSpecularColor; // 金属高光与基础颜色混合程度 [0,1]
half _MetallicMultiply;                // 金属高光形状强度 [0,1]
half _MetallicSpecularAreaRemapMidPoint; // 金属高光区域重映射中点 [0,1]
half _MetallicSpecularAreaRemapRange;  // 金属高光区域重映射范围 [0,1]


// MatCap (遮挡模式)
half _MatCapOcclusionIntensity;        // MatCap遮挡强度
half4 _MatCapOcclusionMapChannelMask;  // MatCap遮挡贴图通道遮罩 RGBA
half _MatCapOcclusionMapRemapStart;    // MatCap遮挡贴图重映射起始值 [0,1]
half _MatCapOcclusionMapRemapEnd;      // MatCap遮挡贴图重映射结束值 [0,1]
half _MatCapOcclusionMapAlphaAsMask;   // MatCap遮挡贴图Alpha作为遮罩 [0,1]
float _MatCapOcclusionUvScale;         // MatCap遮挡UV缩放
half4 _MatCapOcclusionMaskMapChannelMask; // MatCap遮挡遮罩贴图通道遮罩 RGBA
half _MatCapOcclusionMaskMapRemapStart; // MatCap遮挡遮罩贴图重映射起始值 [0,1]
half _MatCapOcclusionMaskMapRemapEnd;  // MatCap遮挡遮罩贴图重映射结束值 [0,1]

// 分区域MatCap强度控制（对应遮罩贴图的不同Alpha值）
half _MatCapIntensity1;                // 区域1 MatCap强度 (Alpha = 1.0)
half _MatCapIntensity2;                // 区域2 MatCap强度 (Alpha = 0.75)
half _MatCapIntensity3;                // 区域3 MatCap强度 (Alpha = 0.5)
half _MatCapIntensity4;                // 区域4 MatCap强度 (Alpha = 0.25)
half _MatCapIntensity5;                // 区域5 MatCap强度 (Alpha = 0.0)

// 分区域粗糙度控制（对应遮罩贴图的不同Alpha值）
half _Roughness1;                      // 区域1粗糙度 (Alpha = 1.0) [0,1]
half _Roughness2;                      // 区域2粗糙度 (Alpha = 0.75) [0,1]
half _Roughness3;                      // 区域3粗糙度 (Alpha = 0.5) [0,1]
half _Roughness4;                      // 区域4粗糙度 (Alpha = 0.25) [0,1]
half _Roughness5;                      // 区域5粗糙度 (Alpha = 0.0) [0,1]

// 环境遮挡属性
half _OcclusionStrength;               // 遮挡强度 [0,1]
half _OcclusionStrengthIndirectMultiplier; // 间接光遮挡强度倍数 [0,1]
half4 _OcclusionMapChannelMask;        // 遮挡贴图通道遮罩 RGBA
half _OcclusionRemapStart;             // 遮挡重映射起始值 [0,1]
half _OcclusionRemapEnd;               // 遮挡重映射结束值 [0,1]

// 着色等级贴图属性
half _ShadingGradeMapStrength;         // 着色等级贴图强度 [0,1]
half _ShadingGradeMapApplyRange;       // 着色等级贴图应用范围 [0,1]
half _ShadingGradeMapMidPointOffset;   // 着色等级贴图中点偏移 [-1,1]
half4 _ShadingGradeMapChannelMask;     // 着色等级贴图通道遮罩 RGBA
half _ShadingGradeMapInvertColor;      // 着色等级贴图反转颜色 [0,1]
half _ShadingGradeMapRemapStart;       // 着色等级贴图重映射起始值 [0,1]
half _ShadingGradeMapRemapEnd;         // 着色等级贴图重映射结束值 [0,1]

// 面部阴影渐变贴图属性
float4 _FaceShadowGradientMapUVScaleOffset; // 面部阴影渐变贴图UV缩放偏移 (xy=缩放, zw=偏移)
float _DebugFaceShadowGradientMap;     // 调试面部阴影渐变贴图 [0,1]
half _FaceShadowGradientIntensity;     // 面部阴影渐变强度 [0,1]
half _FaceShadowGradientResultSoftness; // 面部阴影渐变结果柔和度 [0,1]
half4 _FaceShadowGradientMaskMapChannel; // 面部阴影渐变遮罩贴图通道 RGBA
float _FaceShadowNoFollowThrLight;     // 面部阴影不跟随透射光 [0,1]
float _FaceShadowResult;               // 面部阴影结果 [0,1]
float _FaceShadowRool;                 // 面部阴影滚动 [0,1]

// 高光反射属性 - 区域1 (Alpha = 1.0)
half _SpecularIntensity;               // 高光强度
half3 _SpecularColor;                  // 高光颜色 RGB
half _MultiplyBaseColorToSpecularColor; // 基础颜色与高光颜色混合程度 [0,1]
half _SpecularAreaRemapUsage;          // 高光区域重映射使用程度 [0,1]
half _SpecularAreaRemapMidPoint;       // 高光区域重映射中点 [0,1]
half _SpecularAreaRemapRange;          // 高光区域重映射范围 [0,1]
half _Multiply;                        // 高光形状强度 [0,1]

// 高光反射属性 - 区域2 (Alpha = 0.75)
half _SpecularIntensity2;              // 高光强度
half3 _SpecularColor2;                 // 高光颜色 RGB
half _MultiplyBaseColorToSpecularColor2; // 基础颜色与高光颜色混合程度 [0,1]
half _SpecularAreaRemapUsage2;         // 高光区域重映射使用程度 [0,1]
half _SpecularAreaRemapMidPoint2;      // 高光区域重映射中点 [0,1]
half _SpecularAreaRemapRange2;         // 高光区域重映射范围 [0,1]
half _Multiply2;                       // 高光形状强度 [0,1]

// 高光反射属性 - 区域3 (Alpha = 0.5)
half _SpecularIntensity3;              // 高光强度
half3 _SpecularColor3;                 // 高光颜色 RGB
half _MultiplyBaseColorToSpecularColor3; // 基础颜色与高光颜色混合程度 [0,1]
half _SpecularAreaRemapUsage3;         // 高光区域重映射使用程度 [0,1]
half _SpecularAreaRemapMidPoint3;      // 高光区域重映射中点 [0,1]
half _SpecularAreaRemapRange3;         // 高光区域重映射范围 [0,1]
half _Multiply3;                       // 高光形状强度 [0,1]

// 高光反射属性 - 区域4 (Alpha = 0.25)
half _SpecularIntensity4;              // 高光强度
half3 _SpecularColor4;                 // 高光颜色 RGB
half _MultiplyBaseColorToSpecularColor4; // 基础颜色与高光颜色混合程度 [0,1]
half _SpecularAreaRemapUsage4;         // 高光区域重映射使用程度 [0,1]
half _SpecularAreaRemapMidPoint4;      // 高光区域重映射中点 [0,1]
half _SpecularAreaRemapRange4;         // 高光区域重映射范围 [0,1]
half _Multiply4;                       // 高光形状强度 [0,1]

// 高光反射属性 - 区域5 (Alpha = 0.0)
half _SpecularIntensity5;              // 高光强度
half3 _SpecularColor5;                 // 高光颜色 RGB
half _MultiplyBaseColorToSpecularColor5; // 基础颜色与高光颜色混合程度 [0,1]
half _SpecularAreaRemapUsage5;         // 高光区域重映射使用程度 [0,1]
half _SpecularAreaRemapMidPoint5;      // 高光区域重映射中点 [0,1]
half _SpecularAreaRemapRange5;         // 高光区域重映射范围 [0,1]
half _Multiply5;                       // 高光形状强度 [0,1]

// Kajiya-Kay头发高光模型属性
half _HairStrandSpecularMixWithBaseMapColor; // 头发高光与基础贴图颜色混合程度 [0,1]
half _HairStrandSpecularOverallIntensity; // 头发高光整体强度
half _HairStrandSpecularShapeFrequency; // 头发高光形状频率
half _HairStrandSpecularShapeShift;    // 头发高光形状偏移
half _HairStrandSpecularShapePositionOffset; // 头发高光形状位置偏移
half _HairStrandSpecularMainIntensity; // 头发主高光强度
half _HairStrandSpecularSecondIntensity; // 头发次高光强度
half3 _HairStrandSpecularMainColor;    // 头发主高光颜色 RGB
half3 _HairStrandSpecularSecondColor;  // 头发次高光颜色 RGB
half _HairStrandSpecularMainExponent;  // 头发主高光指数
half _HairStrandSpecularSecondExponent; // 头发次高光指数

// 面部阴影色调覆盖
half _OverrideByFaceShadowTintColor;   // 使用面部阴影色调覆盖 [0,1]
half3 _FaceShadowTintColor;            // 面部阴影色调颜色 RGB

// 纹理阴影颜色覆盖
half _OverrideShadowColorByTexIntensity; // 纹理阴影颜色覆盖强度 [0,1]
half4 _OverrideShadowColorTexTintColor; // 纹理阴影颜色色调 RGBA
half _OverrideShadowColorTexIgnoreAlphaChannel; // 纹理阴影颜色忽略Alpha通道 [0,1]

// 渐变光照纹理属性
half _RampLightingTexSampleUvY;        // 渐变光照纹理采样UV Y坐标 [0,1]
half _UseRampLightingSampleUvYTex;     // 使用渐变光照采样UV Y纹理 [0,1]
half _UseRampLightingTex;              // 使用渐变光照纹理 [0,1]


// lighting style
half _CelShadeMidPoint;
half _CelShadeSoftness;
half _MainLightIgnoreCelShade;
half _IndirectLightFlatten;
half _AdditionalLightCelShadeMidPoint;
half _AdditionalLightCelShadeSoftness;
half _AdditionalLightIgnoreCelShade;
half _AdditionalLightIgnoreOcclusion;
half _AdditionalLightDistanceAttenuationClamp;

// lighting style for face area
float _OverrideCelShadeParamForFaceArea;
half _CelShadeMidPointForFaceArea;
half _CelShadeSoftnessForFaceArea;
half _MainLightIgnoreCelShadeForFaceArea;
float _OverrideAdditionalLightCelShadeParamForFaceArea;
half _AdditionalLightCelShadeMidPointForFaceArea;
half _AdditionalLightCelShadeSoftnessForFaceArea;
half _AdditionalLightIgnoreCelShadeForFaceArea;


// fresnel (body, colormask5)
half3 _FresnelColor;
half _FresnelIntensity;

// depth texture rim light and shadow
float _PerMaterialEnableDepthTextureRimLightAndShadow;
float _NiloToonEnableDepthTextureRimLightAndShadow;

float _DepthTexRimLightAndShadowWidthMultiplier;

float _UseDepthTexRimLightAndShadowWidthMultiplierFromVertexColor;
float4 _DepthTexRimLightAndShadowWidthMultiplierFromVertexColorChannelMask;
float4 _DepthTexRimLightAndShadowWidthTexChannelMask;


half _DepthTexRimLightUsage;
half _DepthTexRimLightIntensity;
half3 _DepthTexRimLightTintColor;
half _DepthTexRimLightMixWithBaseMapColor;
float _DepthTexRimLightThresholdOffset;
float _DepthTexRimLightFadeoutRange;


float _FaceAreaCameraDepthTextureZWriteOffset;
// 当把深度写入_CameraDepthTexture时，脸部区域有足够的ZOffset。好的ZOffset范围大约是0.04到0.06，这使深度纹理的阴影更容易出现。

// average shadow mapping
float _AverageShadowMapRTSampleIndex;


// outline
float _RenderOutline;
float _OutlineUseBakedSmoothNormal;
float _UnityCameraDepthTextureWriteOutlineExtrudedPosition;
half3 _OutlineTintColor;
half4 _OutlineTintColorSkinAreaOverride;
half3 _OutlineOcclusionAreaTintColor;
half _OutlineUseReplaceColor;
half3 _OutlineReplaceColor;
int _OutlineCorrection;

half3 _OutlineTintColor1;
half3 _OutlineTintColor2;
half3 _OutlineTintColor3;
half3 _OutlineTintColor4;
half3 _OutlineTintColor5;

float _OutlineWidth;
float _OutlineWidthExtraMultiplier;

float _OutlineZOffset;
float _OutlineZOffsetForFaceArea;
float _UseOutlineZOffsetTex;
float _OutlineZOffsetMaskRemapStart;
float _OutlineZOffsetMaskRemapEnd;
float _UseOutlineWidthMaskFromVertexColor;
float4 _OutlineWidthMaskFromVertexColor;
//float4 _OutlineWidthTexChannelMask;

// override outline color by tex
half4 _OverrideOutlineColorTexTintColor;
half _OverrideOutlineColorTexIgnoreAlphaChannel;
half _OverrideOutlineColorByTexIntensity;

// 屏幕空间轮廓属性
// 注意：由于vulkan（android Mali GPU设备）的常量缓冲区大小限制
// 我们在vulkan上硬编码移除了basemap堆叠层3-10，以使cbuffer更小
// 有问题的设备列表：(vivo Y73s) (vivo Y72) (TAS_AN00) (mate40) (华为Honor 9x)
// 所有设备都是Mali-G__ GPU + vulkan
// 而"Adreno GPU + vulkan"是安全的（小米8）（三星A70），"OpenGLES + 任何GPU"也是安全的
#ifndef SHADER_API_VULKAN
float _ScreenSpaceOutlineWidth;         // 屏幕空间轮廓宽度
float _ScreenSpaceOutlineWidthIfFace;   // 面部区域屏幕空间轮廓宽度
float _ScreenSpaceOutlineDepthSensitivity; // 屏幕空间轮廓深度敏感度
float _ScreenSpaceOutlineDepthSensitivityIfFace; // 面部区域屏幕空间轮廓深度敏感度
float _ScreenSpaceOutlineNormalsSensitivity; // 屏幕空间轮廓法线敏感度
float _ScreenSpaceOutlineNormalsSensitivityIfFace; // 面部区域屏幕空间轮廓法线敏感度
half3 _ScreenSpaceOutlineTintColor;     // 屏幕空间轮廓色调颜色 RGB
half3 _ScreenSpaceOutlineOcclusionAreaTintColor; // 屏幕空间轮廓遮挡区域色调颜色 RGB
half _ScreenSpaceOutlineUseReplaceColor; // 屏幕空间轮廓使用替换颜色 [0,1]
half3 _ScreenSpaceOutlineReplaceColor;  // 屏幕空间轮廓替换颜色 RGB
half4 _ScreenSpaceOutlineDepthSensitivityTexChannelMask; // 屏幕空间轮廓深度敏感度纹理通道遮罩 RGBA
half4 _ScreenSpaceOutlineNormalsSensitivityTexChannelMask; // 屏幕空间轮廓法线敏感度纹理通道遮罩 RGBA
half _ScreenSpaceOutlineDepthSensitivityTexRemapStart; // 屏幕空间轮廓深度敏感度纹理重映射起始值 [0,1]
half _ScreenSpaceOutlineDepthSensitivityTexRemapEnd; // 屏幕空间轮廓深度敏感度纹理重映射结束值 [0,1]
half _ScreenSpaceOutlineNormalsSensitivityTexRemapStart; // 屏幕空间轮廓法线敏感度纹理重映射起始值 [0,1]
half _ScreenSpaceOutlineNormalsSensitivityTexRemapEnd; // 屏幕空间轮廓法线敏感度纹理重映射结束值 [0,1]
#endif

// 动态眼部属性
float _DynamicEyeSize;                  // 动态眼部尺寸
half _DynamicEyeFinalBrightness;        // 动态眼部最终亮度 [0,1]
half3 _DynamicEyeFinalTintColor;        // 动态眼部最终色调颜色 RGB
half4 _DynamicEyePupilMaskTexChannelMask; // 动态眼部瞳孔遮罩纹理通道遮罩 RGBA
half3 _DynamicEyePupilColor;            // 动态眼部瞳孔颜色 RGB
float _DynamicEyePupilDepthScale;       // 动态眼部瞳孔深度缩放
float _DynamicEyePupilSize;             // 动态眼部瞳孔尺寸
float _DynamicEyePupilMaskSoftness;     // 动态眼部瞳孔遮罩柔和度
float4 _DynamicEyeWhiteMap_ST;          // 动态眼部白色贴图平铺偏移 (xy=平铺, zw=偏移)

// 额外厚轮廓属性
float _ExtraThickOutlineWidth;          // 额外厚轮廓宽度
float _ExtraThickOutlineMaxFinalWidth;  // 额外厚轮廓最大最终宽度
float3 _ExtraThickOutlineViewSpacePosOffset; // 额外厚轮廓视空间位置偏移 XYZ
float _ExtraThickOutlineZOffset;        // 额外厚轮廓Z偏移

// 游戏玩法效果属性
half3 _PerCharEffectTintColor;          // 每角色效果色调颜色 RGB
half3 _PerCharEffectAddColor;           // 每角色效果添加颜色 RGB
half4 _PerCharEffectLerpColor;          // 每角色效果插值颜色 RGBA
half3 _PerCharEffectRimColor;           // 每角色效果边缘光颜色 RGB
half _PerCharEffectRimSharpnessPower;   // 每角色效果边缘光锐度幂次
half _PerCharEffectDesaturatePercentage; // 每角色效果去饱和度百分比 [0,1]

// 受击闪白：独立一层菲涅尔，不跟角色边缘光共用参数
half3 _HitFlashColor;
half _HitFlashIntensity;
half _HitFlashPower;

// 皮肤属性
half4 _SkinMaskMapChannelMask;          // 皮肤遮罩贴图通道遮罩 RGBA

// 面部属性
half4 _FaceMaskMapChannelMask;          // 面部遮罩贴图通道遮罩 RGBA
half3 _FaceForwardDirection;            // 面部前向方向 XYZ
half3 _FaceUpDirection;                 // 面部上向方向 XYZ
half _FixFaceNormalAmount;              // 修正面部法线数量 [0,1]
half _FixFaceNormalUseFlattenOrProxySphereMethod; // 修正面部法线使用平坦或代理球体方法 [0,1]

// 每角色中心属性
float3 _CharacterBoundCenterPosWS;      // 角色边界中心世界空间位置 XYZ

// 抖动淡出属性
float _DitherFadeoutAmount;             // 抖动淡出数量（clip()只接受float类型）

// 透视移除属性
float _PerspectiveRemovalAmount;        // 透视移除总量
// 透视移除（球体）
float _PerspectiveRemovalRadius;        // 透视移除半径
float3 _HeadBonePositionWS;             // 头部骨骼世界空间位置 XYZ
// 透视移除（世界高度）
float _PerspectiveRemovalStartHeight;   // 透视移除起始高度（通常是世界空间pos.y为0）
float _PerspectiveRemovalEndHeight;     // 透视移除结束高度

// 深度写入（用于在_ZWrite = off时禁用所有_CameraDepthTexture相关效果）
float _ZWrite;                          // 深度写入开关 [0,1]
// 剔除（用于修复平面反射通道渲染时的轮廓问题）
float _Cull;                            // 剔除模式

// 溶解效果属性
float _DissolveAmount;                  // 溶解数量 [0,1]
float _DissolveBorderRange;             // 溶解边界范围
float _DissolveEdgeDitherStrength;      // 溶解边缘网点强度 [0,1]，0 = 关
float _EnableDissolve;                  // 溶解总开关（绑 _NILOTOON_DISSOLVE keyword）
float _EnableDitherFadeout;             // 全局网点开关（绑 _NILOTOON_DITHER_FADEOUT keyword）
float _DissolveMinY;                    // 溶解高度下限（物体空间 Y）
float _DissolveMaxY;                    // 溶解高度上限（物体空间 Y）
float _DissolveRootY;                   // 角色根节点的世界 Y，用来把世界高度换算成角色局部高度
half4 _DissolveBorderTintColor;         // 溶解边界色调颜色 RGBA
float _DissolveTopBorderRange;          // 溶解顶部边界范围
half4 _DissolveTopBorderTintColor;      // 溶解顶部边界色调颜色 RGBA

// 每角色基础贴图覆盖属性
half _PerCharacterBaseMapOverrideAmount; // 每角色基础贴图覆盖数量 [0,1]
float _PerCharacterBaseMapOverrideTiling; // 每角色基础贴图覆盖平铺
half3 _PerCharacterBaseMapOverrideTintColor; // 每角色基础贴图覆盖色调颜色 RGB

// 测试和调试变量
//float _ASDASD;                          // 测试变量
//float _TestInt;                         // 测试整数
float3 _CenterPointPosition;            // 中心点位置 XYZ
int _LightDirFromCameraDir;             // 光照方向来自摄像机方向 [0,1]
float3 _LightDirFromCameraDirOffSet;    // 光照方向来自摄像机方向偏移 XYZ
float4 _M_CombinedRotation0, _M_CombinedRotation1, _M_CombinedRotation2; // 组合旋转矩阵行
half _LightMode;
float _AlphaMask; 
CBUFFER_END

// 把物体空间 Y 转成溶解高度：0 = 头顶（先消失），1 = 脚底（最后消失）。
// 用物体空间而不是世界空间，角色跑动、跳跃时溶解面跟着角色走，不会横穿身体。
// 注意 positionOS 是 bind pose 的顶点位置，蒙皮动画不会改变它，
// 所以这个高度轴是固定的，溶解线不会随骨骼动画抖动。
half GetDissolveHeightOS(float positionOSY)
{
    half h = saturate((positionOSY - _DissolveMinY) / max(_DissolveMaxY - _DissolveMinY, 1e-5));
    return 1.0 - h;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 全局统一变量
// 如果一个统一变量不是每个材质的统一变量，
// 在CBUFFER_START(UnityPerMaterial)之外声明是可以的。
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// applyShadowBiasFixToHClipPos()专用的特殊统一变量
float3 _LightDirection;                 // 光照方向 XYZ

// 全局轮廓统一变量
float _GlobalOutlineWidth;              // 全局轮廓宽度
float _GlobalShouldRenderOutline;       // 全局是否应该渲染轮廓 [0,1]
float _GlobalOutlineWidthMultiplier;    // 全局轮廓宽度倍数
half3 _GlobalOutlineTintColor;          // 全局轮廓色调颜色 RGB
float _GlobalOutlineWidthAutoAdjustToCameraDistanceAndFOV; // 全局轮廓宽度自动调整到摄像机距离和FOV

// 全局阴影映射统一变量
float _GlobalShouldReceiveShadowMapping; // 全局是否应该接收阴影映射 [0,1]
half _GlobalReceiveShadowMappingAmount; // 全局接收阴影映射数量 [0,1]
float _GlobalToonShaderNormalBiasMultiplier; // 全局卡通着色器法线偏移倍数
half3 _GlobalMainLightURPShadowAsDirectResultTintColor; // 全局主光源URP阴影作为直接结果色调颜色 RGB
half _GlobalURPShadowAsDirectLightTintIgnoreMaterialURPUsageSetting; // 全局URP阴影作为直接光色调忽略材质URP使用设置 [0,1]
half _GlobalNiloToonReceiveURPShadowblurriness; // 全局NiloToon接收URP阴影模糊度 [0,1]

// 自阴影映射
float4x4 _NiloToonSelfShadowWorldToClip; // NiloToon自阴影世界到裁剪矩阵
float4 _NiloToonSelfShadowParam;        // NiloToon自阴影参数 XYZW
float _NiloToonSelfShadowRange;         // NiloToon自阴影范围
float _NiloToonGlobalSelfShadowDepthBias; // NiloToon全局自阴影深度偏移
half3 _NiloToonSelfShadowLightDirection; // NiloToon自阴影光照方向 XYZ
half _NiloToonSelfShadowUseNdotLFix;    // NiloToon自阴影使用NdotL修正 [0,1]
float _GlobalReceiveSelfShadowMappingPosOffset; // 全局接收自阴影映射位置偏移

// 全局遮挡统一变量
half _GlobalOcclusionStrength;          // 全局遮挡强度 [0,1]

// 全局光照统一变量
half _GlobalIndirectLightMultiplier;    // 全局间接光倍数
half3 _GlobalIndirectLightMinColor;     // 全局间接光最小颜色 RGB

// 全局深度差异边缘光和阴影统一变量
float _GlobalDepthTexRimLightAndShadowWidthMultiplier; // 全局深度纹理边缘光和阴影宽度倍数
float _GlobalDepthTexRimLightDepthDiffThresholdOffset; // 全局深度纹理边缘光深度差异阈值偏移
float _GlobalDepthTexRimLightCameraDistanceFadeoutStartDistance; // 全局深度纹理边缘光摄像机距离淡出起始距离
float _GlobalDepthTexRimLightCameraDistanceFadeoutEndDistance; // 全局深度纹理边缘光摄像机距离淡出结束距离

// 全局光照统一变量
half3 _GlobalUserOverridedMainLightDirVS; // 全局用户覆盖主光源方向视空间 XYZ
half3 _GlobalUserOverridedMainLightDirWS; // 全局用户覆盖主光源方向世界空间 XYZ

// 全局摄像机统一变量
float _CurrentCameraFOV;                // 当前摄像机视野角度

// 全局摄像机修正
float2 _GlobalAspectFix;                // 全局宽高比修正 XY
float _GlobalFOVorOrthoSizeFix;         // 全局FOV或正交尺寸修正

// 全局体积
half3 _GlobalVolumeMulColor;            // 全局体积乘法颜色 RGB
half4 _GlobalVolumeLerpColor;           // 全局体积插值颜色 RGBA
half3 _GlobalRimLightMultiplier;        // 全局边缘光倍数 RGB
half3 _GlobalMainDirectionalLightMaxContribution; // 全局主方向光最大贡献 RGB
half3 _GlobalAdditionalLightMaxContribution; // 全局附加光最大贡献 RGB
half3 _GlobalSpecularTintColor;         // 全局高光色调颜色 RGB
half _GlobalSpecularMinIntensity;       // 全局高光最小强度 [0,1]
half _GlobalSpecularReactToLightDirectionChange; // 全局高光对光照方向变化的反应 [0,1]
half3 _GlobalMainDirectionalLightMultiplier; // 全局主方向光倍数 RGB
half3 _GlobalAdditionalLightMultiplier; // 全局附加光倍数 RGB
half3 _GlobalVolumeBaseColorTintColor;  // 全局体积基础颜色色调颜色 RGB
half3 _GlobalCharacterOverallShadowTintColor; // 全局角色整体阴影色调颜色 RGB
half _GlobalCharacterOverallShadowStrength; // 全局角色整体阴影强度 [0,1]

// 全局屏幕空间轮廓设置
float _GlobalScreenSpaceOutlineIntensityForChar; // 全局角色屏幕空间轮廓强度
float _GlobalScreenSpaceOutlineWidthMultiplierForChar; // 全局角色屏幕空间轮廓宽度倍数
float _GlobalScreenSpaceOutlineNormalsSensitivityOffsetForChar; // 全局角色屏幕空间轮廓法线敏感度偏移
float _GlobalScreenSpaceOutlineDepthSensitivityOffsetForChar; // 全局角色屏幕空间轮廓深度敏感度偏移
float _GlobalScreenSpaceOutlineDepthSensitivityDistanceFadeoutStrengthForChar; // 全局角色屏幕空间轮廓深度敏感度距离淡出强度
half3 _GlobalScreenSpaceOutlineTintColorForChar; // 全局角色屏幕空间轮廓色调颜色 RGB

// 全局实用工具
float _GlobalShouldDisableNiloToonDepthTextureRimLightAndShadow; // 全局是否应该禁用NiloToon深度纹理边缘光和阴影 [0,1]

// 全局调试
float _GlobalToonShadeDebugPipline;     // 全局卡通着色调试管线
float _GlobalToonShadeDebugCase;        // 全局卡通着色调试案例
float _DayTime;                         // 日间时间

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 数据结构体
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/**
 * 卡通表面数据结构体
 * 类似于URP的SurfaceData.hlsl中的"SurfaceData"结构
 * 注意：不要将其命名为"SurfaceData"，因为它将与URP的SurfaceData.hlsl中的"SurfaceData"结构冲突
 * 
 * 成员说明:
 * - albedo: 反照率颜色
 * - alpha: 透明度
 * - emission: 自发光颜色
 * - occlusion: 遮挡值
 * - specular: 高光颜色
 * - specularMask: 高光遮罩（存储用户重映射的高光遮罩纹理，用于渐变高光）
 * - normalTS: 切线空间法线
 * - smoothness: 光滑度
 * - metallicMask: 金属遮罩
 * - colormask1-5: 颜色遮罩1-5（对应不同区域）
 * - uv8: 第八套UV坐标
 */
struct ToonSurfaceData
{
    half3 albedo;                       // 反照率颜色 RGB
    half alpha;                         // 透明度 [0,1]
    half3 emission;                     // 自发光颜色 RGB
    half occlusion;                     // 遮挡值 [0,1]
    half3 specular;                     // 高光颜色 RGB
    half specularMask;                  // 高光遮罩，存储用户重映射的高光遮罩纹理，用于渐变高光
    half3 normalTS;                     // 切线空间法线 XYZ
    half smoothness;                    // 光滑度 [0,1]
    half metallicMask;                  // 金属遮罩 [0,1]
    half colormask1;                    // 颜色遮罩1 (Alpha = 1.0) [0,1]
    half colormask2;                    // 颜色遮罩2 (Alpha = 0.75) [0,1]
    half colormask3;                    // 颜色遮罩3 (Alpha = 0.5) [0,1]
    half colormask4;                    // 颜色遮罩4 (Alpha = 0.25) [0,1]
    half colormask5;                    // 颜色遮罩5 (Alpha = 0.0) [0,1]
    float3 uv8;                         // 第八套UV坐标 XYZ
    
#if _NILOTOON_DEBUG_SHADING
    // 主光源着色结果调试变量
    half celShade;                      // 赛璐璐光照结果 [0,1]
    half celShadeOcclusion;             // 赛璐璐光照纹理AO叠加结果 [0,1]
    half faceShadow;                    // 面部阴影 [0,1]
    half rimAttenuation;                // 深度边缘光和阴影强度 [0,1]
    half rimAttenuation_FadeOut;        // 深度边缘光和阴影淡入淡出 [0,1]
    half averageShadowAttenuation;      // 平均过渡阴影 [0,1]
    half rampUvX;                       // 渐变光照UV映射X（是否合并映射）[0,1]
    half rampLightingTexSamplingUvY;    // 渐变光照UV映射Y（是否合并映射）[0,1]

    half3 RampCol;                      // 采样的渐变光照颜色 RGB
    half3 RampColAlbedo;                // 融合贴图的渐变光照颜色 RGB
    
    half3 specularLitAdd;               // 高光部分 RGB
    
    half3 Resut_RampCol_Albede_Specular_FinalShadow; // 融合光照结果 RGB
    
    half3 rimLightColor;                // 边缘光颜色调控 RGB
    half3 Resut_add_rimLightColor;      // 叠加边缘光颜色调控结果 RGB
    
    half4 MatCapColor;                  // 金属高光颜色 RGBA
    half3 MatCapAddColor;               // 金属高光混合光照 RGB
    half light_distanceAttenua;         // 光照距离衰减 [0,1]
#endif
};

/**
 * 卡通光照数据结构体
 * 类似于URP的Input.hlsl中的"InputData"结构
 * 任何对光照有用的数据都会被放到这个结构中
 * 
 * 成员说明:
 * - uv: UV坐标
 * - normalWS: 世界空间法线
 * - PerPixelNormalizedPolygonNormalWS: 每像素标准化多边形法线
 * - positionWS: 世界空间位置
 * - viewDirectionWS: 世界空间视线方向
 * - selfLinearEyeDepth: 自身线性眼部深度
 * - averageShadowAttenuation: 平均阴影衰减
 * - SH: 球谐光照
 * - isFaceArea: 是否为面部区域（默认0，表示不是面部）
 * - isSkinArea: 是否为皮肤区域（默认0，表示不是皮肤）
 * - SV_POSITIONxy: 屏幕位置XY
 * - normalVS: 视空间法线
 * - reflectionVectorVS: 视空间反射向量
 * - NdotV: 法线与视线方向点积
 * - PolygonNdotV: 多边形法线与视线方向点积
 * - rawURPShadowAttenuation: 原始URP阴影衰减
 * - TBN_WS: 世界空间TBN矩阵（仅在有切线时）
 * - viewDirectionTS: 切线空间视线方向（仅在有切线时）
 * - vertexColor: 顶点颜色
 * - ZOffsetFinalSum: Z偏移最终总和
 * - normalizedScreenSpaceUV: 标准化屏幕空间UV
 */
struct ToonLightingData
{
    float2 uv;                          // UV坐标 XY
    half3 normalWS;                     // 世界空间法线 XYZ
    half3 PerPixelNormalizedPolygonNormalWS; // 每像素标准化多边形法线 XYZ
    float3 positionWS;                  // 世界空间位置 XYZ
    half3 viewDirectionWS;              // 世界空间视线方向 XYZ
    float selfLinearEyeDepth;           // 自身线性眼部深度
    half averageShadowAttenuation;      // 平均阴影衰减 [0,1]
    half3 SH;                           // 球谐光照 XYZ
    half isFaceArea;                    // 是否为面部区域，默认0表示不是面部，参见InitializeLightingData(...)
    half isSkinArea;                    // 是否为皮肤区域，默认0表示不是皮肤，参见InitializeLightingData(...)
    float2 SV_POSITIONxy;               // 屏幕位置XY
    half3 normalVS;                     // 视空间法线 XYZ
    half3 reflectionVectorVS;           // 视空间反射向量 XYZ
    half NdotV;                         // 法线与视线方向点积 [0,1]
    half PolygonNdotV;                  // 多边形法线与视线方向点积 [0,1]
    half rawURPShadowAttenuation;       // 原始URP阴影衰减 [0,1]

    #if VaryingsHasTangentWS
    half3x3 TBN_WS;                     // 世界空间TBN矩阵（切线-副切线-法线）
    half3 viewDirectionTS;              // 切线空间视线方向 XYZ
    #endif

    half4 vertexColor;                  // 顶点颜色 RGBA

    float ZOffsetFinalSum;              // Z偏移最终总和

    float2 normalizedScreenSpaceUV;     // 标准化屏幕空间UV XY
    float3 tangentWS;
};

/**
 * 卡通贴图结构体
 * 存储各种贴图的采样结果
 */
struct ToonMap
{
    half4 _BaseMap_Var;                 // 基础贴图变量 RGBA
    half4 _MaskMap_Var;                 // 遮罩贴图变量 RGBA
    half4 _DecalMap_Var;                // 贴花贴图变量 RGBA
};

/**
 * 遮罩结构体
 * 存储不同区域的遮罩值
 */
struct Mask
{
    half colormask1;                    // 颜色遮罩1 (Alpha = 1.0) [0,1]
    half colormask2;                    // 颜色遮罩2 (Alpha = 0.75) [0,1]
    half colormask3;                    // 颜色遮罩3 (Alpha = 0.5) [0,1]
    half colormask4;                    // 颜色遮罩4 (Alpha = 0.25) [0,1]
    half colormask5;                    // 颜色遮罩5 (Alpha = 0.0) [0,1]
    half metallicmask;                  // 金属遮罩 [0,1]
};

/**
 * 获取光照法线和面部区域信息
 * @param normalWS 世界空间法线
 * @param positionWS 世界空间位置
 * @param uv UV坐标
 * @return half4 输出：xyz = 光照法线WS（应用面部法线编辑），w = 是否为面部区域
 * 
 * 功能说明:
 * - 处理面部法线修正
 * - 支持两种修正方法：平坦方向和代理球体
 * - 返回修正后的法线和面部区域标识
 */
half4 GetLightingNormalWS_FaceArea(half3 normalWS, float3 positionWS, float2 uv)
{
    half3 resultNormalWS = normalWS;    // 默认使用原始的normalWS
    half isFaceArea = 0;                // 默认不是面部区域

    #if _ISFACE
    isFaceArea = 1;                     // 如果没有面罩，当_ISFACE开启时，我们假定整个材质是面部

    // 应用面部法线编辑
    // 方法A：通过强制面部法线成为面部的前进方向向量
    half3 flatDirectionFixFaceNormalWS = _FaceForwardDirection;

    // 方法B：通过强制面部法线成为球体代理法线，使用_HeadBonePositionWS作为球体代理中心
    half3 proxySphereFixFaceNormalWS = positionWS - _HeadBonePositionWS;

    // 让用户选择方法A或方法B
    half3 fixedFaceNormalWS = lerp(flatDirectionFixFaceNormalWS, proxySphereFixFaceNormalWS,
                                   _FixFaceNormalUseFlattenOrProxySphereMethod);

    resultNormalWS = normalize(lerp(resultNormalWS, fixedFaceNormalWS, _FixFaceNormalAmount * isFaceArea));
    // 在最后只进行一次normalize()以提高性能
    #endif
    
    return half4(resultNormalWS, isFaceArea);
}

#include "..\XHHYUtilityHLSL\XhhyNormalVectorUtil.hlsl" // HLSL合并包含

/**
 * 将世界空间位置变换为轮廓位置
 * @param width 轮廓宽度
 * @param vertexPositionInputs 顶点位置输入数据
 * @param vertexNormalInputs 顶点法线输入数据
 * @param smoothedNormalTS 平滑的切线空间法线
 * @return float3 轮廓位置（世界空间）
 * 
 * 功能说明:
 * - 计算基于距离的轮廓宽度调整
 * - 使用平滑法线作为挤出方向以获得更好的轮廓连续性
 * - 支持屏幕空间恒定宽度轮廓（可选）
 */
float3 TransformPositionWSToOutlinePositionWS(float width, VertexPositionInputs vertexPositionInputs,
                                              VertexNormalInputs vertexNormalInputs, float3 smoothedNormalTS)
{
    // 计算基于摄像机距离的轮廓遮罩
    float Outlinemask = length(vertexPositionInputs.positionWS.xyz - _WorldSpaceCameraPos.xyz);
    Outlinemask = saturate(Outlinemask / 6);
    Outlinemask = pow(Outlinemask, 0.9) * 6;
    Outlinemask = min(5, Outlinemask);
  //  width *= Outlinemask * 0.001;

    // 默认情况下，我们将使用光照法线（常规顶点法线）作为挤出方向
    // 它是可用的，但由于平滑组的分割法线，它并不完美
    // 对于硬边的多边形（例如立方体的角，头发的末端顶点），它将产生不连续的轮廓
    float3 extrudeDirectionWS = vertexNormalInputs.normalWS;

    // 但是"世界空间平滑法线"是一个比简单使用光照法线更好的挤出轮廓的方向
    // 因为平滑法线没有任何分割法线，所以轮廓总是连续的，这看起来更好
    // 如果我们在模型的uv8中烘焙了"切线空间平滑法线"，我们可以在这里将其转换回世界空间
    extrudeDirectionWS = ConvertNormalTSToNormalTargetSpace(smoothedNormalTS, vertexNormalInputs.tangentWS,
                                                            vertexNormalInputs.bitangentWS,
                                                            vertexNormalInputs.normalWS);
    
    // 注意：这里返回的是原始位置而不是轮廓位置，可能是为了调试或其他目的
        return vertexPositionInputs.positionWS + extrudeDirectionWS * width;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 片段共享函数（步骤1：如果是DEBUG，运行这些DEBUG函数，并在片段函数的早期启动点提前退出）
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/**
 * 获取强制最小片段着色器结果（调试用）
 * @param input 插值数据
 * @param toonIsAnyOutlinePass 是否为轮廓通道
 * @return half4 调试颜色结果
 */
half4 Get_NILOTOON_FORCE_MINIMUM_FRAGMENT_SHADER_result(Varyings input, int toonIsAnyOutlinePass)
{
    half3 debugColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
    if(toonIsAnyOutlinePass)
    {
        debugColor *= 0.25; // 任何值都是有效的，我们只是想让轮廓变暗一点
    }
    return half4(debugColor, 1);
}

/**
 * 获取调试着色结果
 * @param input 插值数据
 * @param surfaceData 表面数据
 * @param lightingData 光照数据
 * @param toonForwardLitPass 是否为前向光照通道
 * @return half4 调试着色结果
 * 
 * 功能说明:
 * - 提供多种调试模式的着色结果
 * - 支持查看反照率、遮挡、自发光、法线、UV、顶点颜色等
 * - 支持查看各种光照计算的中间结果
 */
half4 Get_NILOTOON_DEBUG_SHADING_result(Varyings input, ToonSurfaceData surfaceData, ToonLightingData lightingData, int toonForwardLitPass)
{
    #if _NILOTOON_DEBUG_SHADING
    // 性能在这里并不重要，因为它是一个调试着色器函数

    // 如果_NILOTOON_DEBUG_SHADING被激活，我们只想渲染NiloToonForwardLitPass
    if(!toonForwardLitPass)
    {
        clip(-1);
        return 0;
    }

    // HLSL中switch case的示例
    switch(_GlobalToonShadeDebugCase)
    {
        case 0:     return half4(surfaceData.albedo, surfaceData.alpha);                    // 反照率
        case 1:     return 1;                                                               // 纯白
        case 2:     return surfaceData.occlusion;                                           // 遮挡
        case 3:     return half4(surfaceData.emission, 1);                                  // 自发光
        case 4:     return half4(lightingData.normalWS * 0.5 + 0.5, 1);                   // 世界空间法线
        case 5:     return half4(input.uv, 0, 1);                                          // UV坐标
        case 6:     return input.color.r;                                                  // 顶点颜色R通道
        case 7:     return input.color.g;                                                  // 顶点颜色G通道
        case 8:     return input.color.b;                                                  // 顶点颜色B通道
        case 9:     return input.color.a;                                                  // 顶点颜色A通道
        case 10:    return half4(input.color.rgb, 1);                                      // 顶点颜色RGB
        case 11:    return half4(surfaceData.specular.rgb, 1);                             // 高光颜色
        case 12:    return half4(input.uv8, 1);                                            // UV8坐标
        case 13:    return half4(surfaceData.albedo * dot(lightingData.normalWS, lightingData.viewDirectionWS), surfaceData.alpha); // 反照率*NdotV

        case 14:    return surfaceData.celShade;                                           // 赛璐璐着色
        case 15:    return surfaceData.celShadeOcclusion;                                  // 赛璐璐着色遮挡
        case 16:    return surfaceData.faceShadow;
        case 17:    return surfaceData.rimAttenuation;
        case 18:    return surfaceData.rimAttenuation_FadeOut;
        case 19:    return surfaceData.averageShadowAttenuation;
        case 20:    return surfaceData.rampUvX;
        case 21:    return surfaceData.rampLightingTexSamplingUvY;
        case 22:    return half4(surfaceData.RampCol,1);
        case 23:    return half4(surfaceData.RampColAlbedo,1);
        case 24:    return half4(surfaceData.specularLitAdd,1);
        case 25:    return half4(surfaceData.Resut_RampCol_Albede_Specular_FinalShadow,1);
        case 26:    return half4(surfaceData.rimLightColor,1);
        case 27:    return half4(surfaceData.Resut_add_rimLightColor,1);
        case 28:    return surfaceData.MatCapColor;
        case 29:    return half4(surfaceData.MatCapAddColor,1);
        case 30:    return surfaceData.light_distanceAttenua;
    }
    #endif

    return 1;
}



#endif