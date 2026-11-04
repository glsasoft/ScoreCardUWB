// =============================================================================================
//  后处理故障效果 · 七合一可叠加版  Glitch Combined
//  由 Assets/GlitchShaderPack/ 下的 7 个独立 shader 合并而成，单 Pass + 关键字变体开关。
//
//  ── 开关方式 ──────────────────────────────────────────────────────────────────────────────
//  每个效果由一个 shader_feature_local 关键字控制，材质面板上就是对应的勾选框：
//      _GLITCH_IMAGE_BLOCK           错位图块故障
//      _GLITCH_ANALOG_NOISE          模拟噪点故障
//      _GLITCH_SCAN_LINE_JITTER      扫描线抖动故障
//      _GLITCH_SCREEN_SHAKE          屏幕抖动故障
//      _GLITCH_WAVE_JITTER           波动抖动故障
//      _GLITCH_RGB_SPLIT             RGB 颜色分离
//      _GLITCH_LINE_BLOCK            错位线条故障
//      _GLITCH_LINE_BLOCK_INFINITE   错位线条的"持续抖动"附加开关
//  默认全部关闭（材质初始就是原画面），按需勾选，可以任意多个同时叠加。
//
//  ── 叠加顺序（重要） ──────────────────────────────────────────────────────────────────────
//  原工程是 7 个独立的 Renderer 依次 Blit，优先级升序执行，顺序为：
//      模拟噪点(10) → 错位图块(30) → 错位线条(70) → RGB分离(80)
//      → 扫描线(130) → 屏幕抖动(150) → 波动抖动(170)
//
//  单 Pass 无法保存中间结果，所以这里做了必要的重排：
//      第一段（位移类，累积到 uv 上，依次串联）
//          屏幕抖动 → 扫描线抖动 → 波动抖动
//          这三个本质是"整幅画面的 UV 偏移"，把偏移叠加到同一个 uv 上。
//          但只有在前后两个位移互不相关（典型是分属不同轴）时才与逐个 Blit 严格等价：
//          扫描线抖动与波动抖动的偏移量本身是被位移坐标的函数，同轴串联时后一个会在
//          "已被前一个位移过的坐标"上求值，结果会有细微差异。
//      采样一次
//      第二段（颜色类，依次作用在同一份颜色上）
//          波动抖动的通道分离 → 模拟噪点 → 错位图块 → 错位线条 → RGB分离
//          相对顺序与原工程的优先级顺序一致。
//
//  两点说明：
//    1. 位移类效果原本排在颜色类之后，这里必须提前（否则无法串联），因此
//       "先错位图块再屏幕抖动"与原工程"先屏幕抖动再错位图块"的像素结果不完全一致。
//       两者都是故障效果，观感相同；需要 100% 复刻叠加结果请继续用 7 个独立 shader。
//    2. 波动抖动的通道分离在原工程里排最后，它会用源画面重建 R/B 通道。
//       这里把它放在颜色段最前面，好让后面的效果能叠在它之上。
//
//  ── 变体数量 ──────────────────────────────────────────────────────────────────────────────
//  用的是 shader_feature_local：只编译材质实际用到的组合，打包体积最小，适合在材质面板上勾选。
//  如果你需要在运行时用脚本开启"从未在材质上勾选过"的组合，把下面所有
//  #pragma shader_feature_local 改成 #pragma multi_compile_local 即可（会编译全部 2^8 个变体）。
//
//  ── 用法：把材质挂到一个网格上（不是全屏后处理） ──────────────────────────────────────────
//  这个 shader 不是全屏 Blit，而是普通网格着色器：建一个片（Quad / Plane），把材质挂上去，
//  **这个片在屏幕上遮挡到的区域就会被叠加故障效果**，片没盖到的地方完全保持原样。
//
//  因为 uv 取的是像素的屏幕坐标，所以片的形状、朝向、甚至换成球体都无所谓，
//  效果永远贴着屏幕对齐 —— 片只起"划定区域"的作用。
//
//  源画面取自 URP 的 _CameraOpaqueTexture（不透明场景图），
//  前提：URP Asset 里必须勾选 "Opaque Texture"，否则这张图是空的、什么都看不到。
//
//  建议摆放：把片放在相机前面（或直接设成相机的子物体），用它的位置/缩放来框选出
//  要出现故障的屏幕区域。渲染参数见文件末尾 SubShader 的注释。
// =============================================================================================
Shader "Eff/EffPostProcessing"
{
    Properties
    {
        // 注意：中文不能出现在属性参数里 —— [Header("中文")] 同样会解析报错。
        // 因此所有分组标题改用"空属性"实现（下面的 _SectionXX），下面每个分组同理。
        // [Header("勾选开关启用对应效果 可以多个同时叠加")]
        
        _Mask("_Mask", 2D) = "white" {}
        [Toggle] _FilpMask("反转Mask", Int) = 0
        [Space(10)]
        _SectionHint("==== 勾选开关启用对应效果 可以多个同时叠加 ====", Float) = 0

        // [Header("1 错位图块故障 Image Block")]
        [Space(10)]
        _Section1("==== 1 错位图块故障 Image Block ====", Float) = 0
        [Toggle(_GLITCH_IMAGE_BLOCK)] _EnableImageBlock("_EnableImageBlock 启用错位图块", Float) = 0
        _IB_Fade("_IB_Fade 混合强度", Range(0, 1)) = 1
        _IB_Speed("_IB_Speed 变化速度", Range(0, 1)) = 0.5
        _IB_Amount("_IB_Amount 错位强度", Range(0, 10)) = 1
        _IB_BlockLayer1_U("_IB_BlockLayer1_U 第一层图块 U 数量", Range(0, 50)) = 9
        _IB_BlockLayer1_V("_IB_BlockLayer1_V 第一层图块 V 数量", Range(0, 50)) = 9
        _IB_BlockLayer1_Indensity("_IB_BlockLayer1_Indensity 第一层随机强度", Range(0, 50)) = 8
        _IB_BlockLayer2_U("_IB_BlockLayer2_U 第二层图块 U 数量", Range(0, 50)) = 5
        _IB_BlockLayer2_V("_IB_BlockLayer2_V 第二层图块 V 数量", Range(0, 50)) = 5
        _IB_BlockLayer2_Indensity("_IB_BlockLayer2_Indensity 第二层随机强度", Range(0, 50)) = 4
        _IB_RGBSplitIndensity("_IB_RGBSplitIndensity RGB 分离强度", Range(0, 50)) = 0.5
        [Enum(RG,0,GB,1,RB,2)]_IB_RGBEnum(" RGB 分离通道选择 RG  ，GB  ，RB", int) = 0.5

        // [Header("2 模拟噪点故障 Analog Noise")]
        [Space(10)]
        _Section2("==== 2 模拟噪点故障 Analog Noise ====", Float) = 0
        [Toggle(_GLITCH_ANALOG_NOISE)] _EnableAnalogNoise("_EnableAnalogNoise 启用模拟噪点", Float) = 0
        _AN_Fading("_AN_Fading 噪点混合强度", Range(0, 1)) = 1
        _AN_Speed("_AN_Speed 噪点变化速度", Range(0, 1)) = 0.5
        _AN_LuminanceJitterThreshold("_AN_LuminanceJitterThreshold 亮度跳变阈值", Range(0, 1)) = 0.8

        // [Header("3 扫描线抖动故障 Scan Line Jitter")]
        [Space(10)]
        _Section3("==== 3 扫描线抖动故障 Scan Line Jitter ====", Float) = 0
        [Toggle(_GLITCH_SCAN_LINE_JITTER)] _EnableScanLineJitter("_EnableScanLineJitter 启用扫描线抖动", Float) = 0
        [Enum(Horizontal, 0, Vertical, 1)] _SLJ_Direction("_SLJ_Direction 抖动方向 水平/垂直", Float) = 0
        _SLJ_Intensity("_SLJ_Intensity 抖动强度", Range(0, 1)) = 0.1
        _SLJ_Frequency("_SLJ_Frequency 变化频率 设 0 即为持续抖动", Range(0, 25)) = 0

        // [Header("4 屏幕抖动故障 Screen Shake")]
        [Space(10)]
        _Section4("==== 4 屏幕抖动故障 Screen Shake ====", Float) = 0
        [Toggle(_GLITCH_SCREEN_SHAKE)] _EnableScreenShake("_EnableScreenShake 启用屏幕抖动", Float) = 0
        [Enum(Horizontal, 0, Vertical, 1)] _SS_Direction("_SS_Direction 抖动方向 水平/垂直", Float) = 0
        _SS_Intensity("_SS_Intensity 抖动强度", Range(0, 1)) = 0.3

        // [Header("5 波动抖动故障 Wave Jitter")]
        [Space(10)]
        _Section5("==== 5 波动抖动故障 Wave Jitter ====", Float) = 0
        [Toggle(_GLITCH_WAVE_JITTER)] _EnableWaveJitter("_EnableWaveJitter 启用波动抖动", Float) = 0
        [Enum(Horizontal, 0, Vertical, 1)] _WJ_Direction("_WJ_Direction 波动方向 水平/垂直", Float) = 0
        _WJ_Frequency("_WJ_Frequency 波动频率 设 0 即为持续抖动", Range(0, 50)) = 0
        _WJ_Amount("_WJ_Amount 波动幅度", Range(0, 2)) = 1
        _WJ_Speed("_WJ_Speed 波动速度", Range(0, 1)) = 0.25
        _WJ_RGBSplit("_WJ_RGBSplit 颜色分离强度", Range(0, 50)) = 20
        _WJ_UseCustomResolution("_WJ_UseCustomResolution 使用自定义分辨率", Range(0, 1)) = 0
        _WJ_Resolution("_WJ_Resolution 自定义分辨率", Vector) = (640, 480, 0, 0)

        // [Header("6 RGB 颜色分离 RGB Split")]
        [Space(10)]
        _Section6("==== 6 RGB 颜色分离 RGB Split ====", Float) = 0
        [Toggle(_GLITCH_RGB_SPLIT)] _EnableRGBSplit("_EnableRGBSplit 启用颜色分离", Float) = 0
        [Enum(Horizontal, 0, Vertical, 1, Both, 2)] _RGBS_Direction("_RGBS_Direction 分离方向 水平/垂直/双向", Float) = 0
        _RGBS_Fading("_RGBS_Fading 混合强度", Range(0, 1)) = 1
        _RGBS_Amount("_RGBS_Amount 分离强度", Range(0, 1)) = 0.5
        _RGBS_Speed("_RGBS_Speed 分离速度", Range(0, 10)) = 1
        _RGBS_CenterFading("_RGBS_CenterFading 中心衰减", Range(0, 1)) = 0
        _RGBS_AmountR("_RGBS_AmountR 红色通道倍数", Range(0, 5)) = 1
        _RGBS_AmountB("_RGBS_AmountB 蓝色通道倍数", Range(0, 5)) = 1

        // [Header("7 错位线条故障 Line Block")]
        [Space(10)]
        _Section7("==== 7 错位线条故障 Line Block ====", Float) = 0
        [Toggle(_GLITCH_LINE_BLOCK)] _EnableLineBlock("_EnableLineBlock 启用错位线条", Float) = 0
        [Toggle(_GLITCH_LINE_BLOCK_INFINITE)] _LB_Infinite("_LB_Infinite 持续抖动", Float) = 0
        [Enum(Horizontal, 0, Vertical, 1)] _LB_Direction("_LB_Direction 线条方向 横线条/竖线条", Float) = 0
        _LB_Frequency("_LB_Frequency 线条变化频率", Range(0, 25)) = 0
        _LB_Amount("_LB_Amount 线条阈值", Range(0, 1)) = 0.5
        _LB_LinesWidth("_LB_LinesWidth 线条宽度", Range(0.1, 10)) = 1
        _LB_Speed("_LB_Speed 变化速度", Range(0, 1)) = 0.8
        _LB_Offset("_LB_Offset 错位幅度", Range(0, 13)) = 1
        _LB_Alpha("_LB_Alpha 线条不透明度", Range(0, 1)) = 1
    }

    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

    // ---- 每个效果的开关变体 ----
    #pragma shader_feature_local _ _GLITCH_IMAGE_BLOCK
    #pragma shader_feature_local _ _GLITCH_ANALOG_NOISE
    #pragma shader_feature_local _ _GLITCH_SCAN_LINE_JITTER
    #pragma shader_feature_local _ _GLITCH_SCREEN_SHAKE
    #pragma shader_feature_local _ _GLITCH_WAVE_JITTER
    #pragma shader_feature_local _ _GLITCH_RGB_SPLIT
    #pragma shader_feature_local _ _GLITCH_LINE_BLOCK
    #pragma shader_feature_local _ _GLITCH_LINE_BLOCK_INFINITE

    // =========================================================================================
    //  公共：顶点着色器 + 源画面采样
    // =========================================================================================
    // 这是一个"贴在网格上"的着色器，不是全屏 Blit。把它挂到一个片（Quad / Plane / 任意网格）上，
    // 这个片在屏幕上遮挡到的区域就会被叠加故障效果，片没盖到的地方保持原样。
    //
    // 源画面取 URP 的 _CameraOpaqueTexture（不透明场景图），前提是 URP Asset 里勾选了
    // Opaque Texture。
    //
    // uv 用的是"当前像素的屏幕坐标"，不是网格自身的 uv —— 所以片的形状、旋转、
    // 甚至换成球体之类的网格都无所谓，效果永远贴着屏幕对齐。
    struct Attributes
    {
        float4 positionOS : POSITION;
        float2	texcoord0		: TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        output.uv = input.texcoord0;
        return output;
    }

    // 当前像素对应的屏幕 uv (0~1)。这就是"这个片在屏幕上所占的那块 uv"。
    // GetNormalizedScreenSpaceUV 是 URP 官方辅助函数（ShaderVariablesFunctions.hlsl），
    // 内部用 _ScaledScreenParams 归一化，并处理 UNITY_UV_STARTS_AT_TOP 的翻转；
    // ShaderGraph 的 Scene Color 节点在 Mesh 模式下用的就是它。
    float2 ScreenUV(float4 positionCS)
    {
        return GetNormalizedScreenSpaceUV(positionCS);
    }

    TEXTURE2D_X(_ScreenCopyTexture);
SAMPLER(sampler_ScreenCopyTexture);
    
     TEXTURE2D_X(_Mask);
SAMPLER(sampler_Mask);
    int _FilpMask;
    // 直接采样 RGBA。URP 自带的 SampleSceneColor 只返回 RGB 会把 alpha 丢掉，
    // 而错位图块 / 波动抖动原本会用到 alpha，所以这里直接采纹理。
    half4 GetScreenColor(float2 uv)
    {
        return SAMPLE_TEXTURE2D_X(_ScreenCopyTexture, sampler_ScreenCopyTexture, uv);
    }
    
     // 直接采样 RGBA。URP 自带的 SampleSceneColor 只返回 RGB 会把 alpha 丢掉，
    // 而错位图块 / 波动抖动原本会用到 alpha，所以这里直接采纹理。
    half4 GetMaskTex(float2 uv)
    {
        return SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv);
    }

    // =========================================================================================
    //  1  错位图块故障  Image Block
    // =========================================================================================
    uniform half _IB_Fade;
    uniform half _IB_Speed;
    uniform half _IB_Amount;
    uniform half _IB_BlockLayer1_U;
    uniform half _IB_BlockLayer1_V;
    uniform half _IB_BlockLayer1_Indensity;
    uniform half _IB_BlockLayer2_U;
    uniform half _IB_BlockLayer2_V;
    uniform half _IB_BlockLayer2_Indensity;
    uniform half _IB_RGBSplitIndensity;
    uniform int _IB_RGBEnum;
    // 原工程 _TimeX = 每帧累加 deltaTime（超过 100 由 C# 归零）再乘 Speed
    float IB_Time()
    {
        return fmod(_Time.y, 100.0) * _IB_Speed;
    }

    float IB_RandomNoise(float2 seed)
    {
        return frac(sin(dot(seed * floor(IB_Time() * 30.0), float2(127.1, 311.7))) * 43758.5453123);
    }

    float IB_RandomNoise(float seed)
    {
        return IB_RandomNoise(float2(seed, 1.0));
    }

    half4 IB_Apply(float2 uv, half4 incoming)
    {
        // 保持原工程 BlockLayer1/2 的 U/V 交叉绑定，保证参数含义与独立 shader 一致
        float2 blockLayer1 = floor(uv * float2(_IB_BlockLayer2_V, _IB_BlockLayer1_U));
        float2 blockLayer2 = floor(uv * float2(_IB_BlockLayer1_V, _IB_BlockLayer2_U));

        float lineNoise1 = pow(IB_RandomNoise(blockLayer1), _IB_BlockLayer1_Indensity);
        float lineNoise2 = pow(IB_RandomNoise(blockLayer2), _IB_BlockLayer2_Indensity);
        float RGBSplitNoise = pow(IB_RandomNoise(5.1379), 7.1) * _IB_RGBSplitIndensity;
        float lineNoise = lineNoise1 * lineNoise2 * _IB_Amount - RGBSplitNoise;

        // colorR 用上一层的结果而不是重新采样源画面，这样前面效果的颜色不会被丢
        // （只开这一个效果时 incoming 就等于 GetScreenColor(uv)，与独立 shader 完全一致）
        //half4 colorR = incoming;
        half4 colorA = GetScreenColor(uv + float2(lineNoise * 0.05 * IB_RandomNoise(7.0), 0));
        half4 colorB = GetScreenColor(uv - float2(lineNoise * 0.05 * IB_RandomNoise(23.0), 0));

        
        half4 result;
        if (_IB_RGBEnum == 0 )
         result = half4(colorA.x, colorB.y, incoming.z, (colorA.a + colorB.a + incoming.a));
         if (_IB_RGBEnum == 1 )
         result = half4(incoming.x, colorA.y, colorB.z, (incoming.a + colorA.a + colorB.a));
         if (_IB_RGBEnum == 2 )
         result = half4(colorA.x, incoming.y, colorB.z, (colorA.a + incoming.a + colorB.a));
        
        return lerp(incoming, result, _IB_Fade);
    }

    // =========================================================================================
    //  2  模拟噪点故障  Analog Noise
    // =========================================================================================
    uniform half _AN_Fading;
    uniform half _AN_Speed;
    uniform half _AN_LuminanceJitterThreshold;

    float AN_RandomNoise(float2 c)
    {
        return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
    }

    half4 AN_Apply(float2 uv, half4 incoming)
    {
        float timeX = fmod(_Time.y, 100.0) * _AN_Speed;

        half4 noiseColor = incoming;
        half luminance = dot(noiseColor.rgb, half3(0.22, 0.707, 0.071));
        if (AN_RandomNoise(float2(timeX, timeX)) > _AN_LuminanceJitterThreshold)
        {
            noiseColor = half4(luminance, luminance, luminance, luminance);
        }

        float noiseX = AN_RandomNoise(timeX + uv / float2(-213, 5.53));
        float noiseY = AN_RandomNoise(timeX - uv / float2(213, -5.53));
        float noiseZ = AN_RandomNoise(timeX + uv / float2(213, 5.53));
        noiseColor.rgb += 0.25 * float3(noiseX, noiseY, noiseZ) - 0.125;

        return lerp(incoming, noiseColor, _AN_Fading);
    }

    // =========================================================================================
    //  3  扫描线抖动故障  Scan Line Jitter（位移类：返回偏移后的 uv）
    // =========================================================================================
    uniform half _SLJ_Direction;
    uniform half _SLJ_Intensity;
    uniform half _SLJ_Frequency;

    float SLJ_RandomNoise(float x, float y)
    {
        return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
    }

    float2 SLJ_Warp(float2 uv)
    {
        
        float2 uv_lod = uv;
        // 频率设 0 时 cos(0)=1，强度恒为 1，等价于原工程的"持续抖动"模式
        half strength = 0.5 + 0.5 * cos(_Time.y * _SLJ_Frequency);

        // 原工程由 C# 完成的参数换算
        float amount = 0.005 + pow(_SLJ_Intensity, 3.0) * 0.1;
        float threshold = saturate(1.0 - _SLJ_Intensity * 1.2);

        bool vertical = _SLJ_Direction > 0.5;
        float r = vertical ? SLJ_RandomNoise(uv.x, _Time.x) : SLJ_RandomNoise(uv.y, _Time.x);

        float jitter = r * 2 - 1;
        jitter *= step(threshold, abs(jitter)) * amount * strength;

         uv= frac(uv + (vertical ? float2(0.0, jitter) : float2(jitter, 0.0)));
       uv=  _SLJ_Intensity < 0.001? uv_lod: uv;
        return uv;
    }

    // =========================================================================================
    //  4  屏幕抖动故障  Screen Shake（位移类）
    // =========================================================================================
    uniform half _SS_Direction;
    uniform half _SS_Intensity;

    float SS_RandomNoise(float x, float y)
    {
        return frac(sin(dot(float2(x, y), float2(127.1, 311.7))) * 43758.5453);
    }

    float2 SS_Warp(float2 uv)
    {
        // 原工程 C# 把强度乘了 0.25
        float shake = (SS_RandomNoise(_Time.x, 2) - 0.5) * (_SS_Intensity * 0.25);
        return frac(uv + (_SS_Direction > 0.5 ? float2(0.0, shake) : float2(shake, 0.0)));
    }

    // =========================================================================================
    //  5  波动抖动故障  Wave Jitter（位移 + 通道分离）
    // =========================================================================================
    uniform half _WJ_Direction;
    uniform half _WJ_Frequency;
    uniform half _WJ_Amount;
    uniform half _WJ_Speed;
    uniform half _WJ_RGBSplit;
    uniform half _WJ_UseCustomResolution;
    uniform float4 _WJ_Resolution;

    // ---- 2D Simplex Noise（自 NoiseLibrary.hlsl 内联，加 WJ_ 前缀避免与其它效果重名）----
    #define WJ_NOISE_SIMPLEX_1_DIV_289 0.00346020761245674740484429065744f

    float3 WJ_Mod289(float3 x)
    {
        return x - floor(x * WJ_NOISE_SIMPLEX_1_DIV_289) * 289.0;
    }

    float2 WJ_Mod289(float2 x)
    {
        return x - floor(x * WJ_NOISE_SIMPLEX_1_DIV_289) * 289.0;
    }

    float3 WJ_Permute(float3 x)
    {
        return WJ_Mod289(x * x * 34.0 + x);
    }

    float3 WJ_TaylorInvSqrt(float3 r)
    {
        return 1.79284291400159 - 0.85373472095314 * r;
    }

    float WJ_Snoise(float2 v)
    {
        const float4 C = float4(0.211324865405187,
                                0.366025403784439,
                               -0.577350269189626,
                                0.024390243902439);
        float2 i = floor(v + dot(v, C.yy));
        float2 x0 = v - i + dot(i, C.xx);

        float2 i1;
        i1.x = step(x0.y, x0.x);
        i1.y = 1.0 - i1.x;

        float2 x1 = x0 + C.xx - i1;
        float2 x2 = x0 + C.zz;

        i = WJ_Mod289(i);
        float3 p = WJ_Permute(WJ_Permute(i.y + float3(0.0, i1.y, 1.0))
                                       + i.x + float3(0.0, i1.x, 1.0));

        float3 m = max(0.5 - float3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
        m = m * m;
        m = m * m;

        float3 x = 2.0 * frac(p * C.www) - 1.0;
        float3 h = abs(x) - 0.5;
        float3 ox = floor(x + 0.5);
        float3 a0 = x - ox;

        m *= WJ_TaylorInvSqrt(a0 * a0 + h * h);

        float3 g;
        g.x = a0.x * x0.x + h.x * x0.y;
        g.y = a0.y * x1.x + h.y * x1.y;
        g.z = a0.z * x2.x + h.z * x2.y;
        return 130.0 * dot(m, g);
    }

    // 原工程: _Resolution = customResolution ? 自定义值 : Screen.width/height
    float2 WJ_Resolution()
    {
        return lerp(_ScreenParams.xy, _WJ_Resolution.xy, _WJ_UseCustomResolution);
    }

    // 返回需要叠加到 uv 上的位移，同时输出通道分离偏移
    float2 WJ_WaveOffset(float2 uv, out float2 rgbSplitOffset)
    {
        // 频率设 0 时 strength 恒为 1，等价于原工程的"持续抖动"
        half strength = 0.5 + 0.5 * cos(_Time.y * _WJ_Frequency);
        float2 res = WJ_Resolution();

        if (_WJ_Direction > 0.5)
        {
            float uv_x = uv.x * res.x;
            float wave1 = WJ_Snoise(float2(uv_x * 0.01, _Time.y * _WJ_Speed * 20)) * (strength * _WJ_Amount * 32.0);
            float wave2 = WJ_Snoise(float2(uv_x * 0.02, _Time.y * _WJ_Speed * 10)) * (strength * _WJ_Amount * 4.0);
            // 原工程此处除的是 _Resolution.x，保持一致
            float wave_y = wave1 * wave2 / res.x;
            rgbSplitOffset = float2(0.0, (_WJ_RGBSplit * 50 + (20.0 * strength + 1.0)) * wave_y / res.y);
            return float2(0.0, wave_y);
        }
        else
        {
            float uv_y = uv.y * res.y;
            float wave1 = WJ_Snoise(float2(uv_y * 0.01, _Time.y * _WJ_Speed * 20)) * (strength * _WJ_Amount * 32.0);
            float wave2 = WJ_Snoise(float2(uv_y * 0.02, _Time.y * _WJ_Speed * 10)) * (strength * _WJ_Amount * 4.0);
            float wave_x = wave1 * wave2 / res.x;
            rgbSplitOffset = float2((_WJ_RGBSplit * 50 + (20.0 * strength + 1.0)) * wave_x / res.x, 0.0);
            return float2(wave_x, 0.0);
        }
    }

    // 原版: colorG 取波后的画面, colorRB 再偏移一个分离量, 然后重组成 RGB
    half4 WJ_ApplyRGBSplit(float2 uv, float2 rgbSplitOffset, half4 incoming)
    {
        half4 colorG = incoming;
        half4 colorRB = GetScreenColor(uv + rgbSplitOffset);
        return half4(colorRB.r, colorG.g, colorRB.b, colorRB.a + colorG.a);
    }

    // =========================================================================================
    //  6  RGB 颜色分离  RGB Split
    // =========================================================================================
    uniform half _RGBS_Direction;
    uniform half _RGBS_Fading;
    uniform half _RGBS_Amount;
    uniform half _RGBS_Speed;
    uniform half _RGBS_CenterFading;
    uniform half _RGBS_AmountR;
    uniform half _RGBS_AmountB;

    half RGBS_BaseSplitAmount()
    {
        // 原工程 _TimeX 每帧累加（超过 100 归零），着色器里再乘 6 * Speed
        float time = fmod(_Time.y, 100.0) * 6 * _RGBS_Speed;

        half splitAmount = (1.0 + sin(time)) * 0.5;
        splitAmount *= 1.0 + sin(time * 2) * 0.5;
        splitAmount = pow(splitAmount, 3.0);
        splitAmount *= 0.05;
        return splitAmount;
    }

    half4 RGBS_Apply(float2 uv, half4 incoming)
    {
        half splitAmount = RGBS_BaseSplitAmount();
        half3 colorR = incoming.rgb;
        half3 colorB = incoming.rgb;

        if (_RGBS_Direction < 0.5)
        {
            // 水平：应用 _CenterFading
            float centerDistance = length(uv - float2(0.5, 0.5));
            splitAmount *= _RGBS_Fading * _RGBS_Amount;
            splitAmount *= lerp(1.0, centerDistance, _RGBS_CenterFading);

            colorR = GetScreenColor(float2(uv.x + splitAmount * _RGBS_AmountR, uv.y)).rgb;
            colorB = GetScreenColor(float2(uv.x - splitAmount * _RGBS_AmountB, uv.y)).rgb;
        }
        else
        {
            // 垂直 / 双向：原工程这两个 Pass 重复乘了一次 _Fading * _Amount，
            // 且没有应用 _CenterFading，这里保持完全一致
            splitAmount *= _RGBS_Fading * _RGBS_Amount;
            splitAmount *= _RGBS_Fading * _RGBS_Amount;

            if (_RGBS_Direction < 1.5)
            {
                colorR = GetScreenColor(float2(uv.x, uv.y + splitAmount * _RGBS_AmountR)).rgb;
                colorB = GetScreenColor(float2(uv.x, uv.y - splitAmount * _RGBS_AmountB)).rgb;
            }
            else
            {
                float splitR = splitAmount * _RGBS_AmountR;
                float splitB = splitAmount * _RGBS_AmountB;
                colorR = GetScreenColor(float2(uv.x + splitR, uv.y + splitR)).rgb;
                colorB = GetScreenColor(float2(uv.x - splitB, uv.y - splitB)).rgb;
            }
        }

        // G 通道与混合底色都沿用 incoming，保证前面效果能叠加进来。
        // alpha 仍按独立版写死 1.0，这样"只开这一个"时与 GlitchRGBSplit.shader 完全一致
        // （独立版三个 Pass 返回的都是 1.0）
        half3 splitColor = half3(colorR.r, incoming.g, colorB.b);
        return half4(lerp(incoming.rgb, splitColor, _RGBS_Fading), 1.0);
    }

    // =========================================================================================
    //  7  错位线条故障  Line Block
    // =========================================================================================
    uniform half _LB_Direction;
    uniform half _LB_Frequency;
    uniform half _LB_Amount;
    uniform half _LB_LinesWidth;
    uniform half _LB_Speed;
    uniform half _LB_Offset;
    uniform half _LB_Alpha;

    float LB_RandomNoise(float2 c)
    {
        return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
    }

    // 按层级量化（重载 HLSL 内置 trunc，加前缀避免歧义）
    float LB_Trunc(float x, float num_levels)
    {
        return floor(x * num_levels) / num_levels;
    }

    float2 LB_Trunc(float2 x, float2 num_levels)
    {
        return floor(x * num_levels) / num_levels;
    }

    float3 LB_Rgb2Yuv(float3 rgb)
    {
        float3 yuv;
        yuv.x = dot(rgb, float3(0.299, 0.587, 0.114));
        yuv.y = dot(rgb, float3(-0.14713, -0.28886, 0.436));
        yuv.z = dot(rgb, float3(0.615, -0.51499, -0.10001));
        return yuv;
    }

    float3 LB_Yuv2Rgb(float3 yuv)
    {
        float3 rgb;
        rgb.r = yuv.x + yuv.z * 1.13983;
        rgb.g = yuv.x + dot(float2(-0.39465, -0.58060), yuv.yz);
        rgb.b = yuv.x + yuv.y * 2.03211;
        return rgb;
    }

    // 原工程: _TimeX = 每帧累加 deltaTime（超过 100 归零）* Speed * 0.2
    float LB_Time()
    {
        return fmod(_Time.y, 100.0) * _LB_Speed * 0.2;
    }

    // 生成错位线条，返回带符号的错位量（lineCoord 传 uv.yy 或 uv.xx）
    float LB_BlockLineRandom(float2 lineCoord, float timeX)
    {
        // [1] 生成随机强度梯度线条
        float truncTime = LB_Trunc(timeX, 4.0);
        float uv_trunc = LB_RandomNoise(LB_Trunc(lineCoord, float2(8, 8)) + 100.0 * truncTime);
        float uv_randomTrunc = 6.0 * LB_Trunc(timeX, 24.0 * uv_trunc);

        // [2] 生成随机非均匀宽度线条（原工程 C# 传入的是 1 / LinesWidth）
        float invLinesWidth = 1.0 / _LB_LinesWidth;
        float blockLine_random = 0.5 * LB_RandomNoise(
            LB_Trunc(lineCoord + uv_randomTrunc, float2(8 * invLinesWidth, 8 * invLinesWidth)));
        blockLine_random += 0.5 * LB_RandomNoise(LB_Trunc(lineCoord + uv_randomTrunc, float2(7, 7)));
        blockLine_random = blockLine_random * 2.0 - 1.0;
        blockLine_random = sign(blockLine_random) * saturate((abs(blockLine_random) - _LB_Amount) / 0.4);
        blockLine_random = lerp(0.0, blockLine_random, _LB_Offset);
        return blockLine_random;
    }

    // [4] RGB -> YUV 并做色调偏移
    float3 LB_AdjustChroma(float3 srcColor, float blockLine_random)
    {
        float3 colorYuv = LB_Rgb2Yuv(srcColor);
        colorYuv.y /= 1.0 - 3.0 * abs(blockLine_random) * saturate(0.5 - blockLine_random);
        colorYuv.z += 0.125 * blockLine_random * saturate(blockLine_random - 0.5);
        return LB_Yuv2Rgb(colorYuv);
    }

    half4 LB_Apply(float2 uv, half4 incoming)
    {
        float timeX = LB_Time();

        half strength = 0;
        #if defined(_GLITCH_LINE_BLOCK_INFINITE)
            strength = 10;
        #else
            strength = 0.5 + 0.5 * cos(timeX * _LB_Frequency);
        #endif
        timeX *= strength;

        bool vertical = _LB_Direction > 0.5;
        float2 lineCoord = vertical ? uv.xx : uv.yy;
        float2 axis = vertical ? float2(0.0, 1.0) : float2(1.0, 0.0);

        float blockLine_random = LB_BlockLineRandom(lineCoord, timeX);

        // [3] 生成源色调的 blockLine Glitch
        float2 uv_blockLine = saturate(uv + axis * (0.1 * blockLine_random));
        half4 blockLineColor = GetScreenColor(abs(uv_blockLine));

        float3 blockLineColor_rgb = LB_AdjustChroma(blockLineColor.rgb, blockLine_random);

        // [5] 与上一层结果混合
        return lerp(incoming, half4(blockLineColor_rgb, blockLineColor.a), _LB_Alpha);
    }

    // =========================================================================================
    //  主片元
    // =========================================================================================
    half4 Frag_Combined(Varyings i) : SV_Target
    {
        // 网格自身不提供 uv，用当前像素的屏幕坐标 —— 这就是"这个片在屏幕上所占的那块 uv"
        float2 uv = ScreenUV(i.positionCS);
 half4 col_lod = GetScreenColor(uv);
        // ---------- 第一段：位移类效果，依次累积到 uv 上 ----------
        #if defined(_GLITCH_SCREEN_SHAKE)
            uv = SS_Warp(uv);
        #endif

        #if defined(_GLITCH_SCAN_LINE_JITTER)
            uv = SLJ_Warp(uv);
        #endif

        #if defined(_GLITCH_WAVE_JITTER)
            float2 wjRGBSplitOffset;
            uv += WJ_WaveOffset(uv, wjRGBSplitOffset);
        #endif

        // ---------- 采样 ----------
        half4 col = GetScreenColor(uv);
            
        // ---------- 第二段：颜色类效果，依次叠加 ----------
        #if defined(_GLITCH_WAVE_JITTER)
            col = WJ_ApplyRGBSplit(uv, wjRGBSplitOffset, col);
        #endif

        #if defined(_GLITCH_ANALOG_NOISE)
            col = AN_Apply(uv, col);
        #endif

        #if defined(_GLITCH_IMAGE_BLOCK)
            col = IB_Apply(uv, col);
        #endif

        #if defined(_GLITCH_LINE_BLOCK)
            col = LB_Apply(uv, col);
        #endif

        #if defined(_GLITCH_RGB_SPLIT)
            col = RGBS_Apply(uv, col);
        #endif
       float mask = GetMaskTex(i.uv);
       mask =  lerp(mask,1-mask,_FilpMask) ;
      // col = lerp(col_lod,col,mask);
        col.a *= mask;
        return col;
    }

    ENDHLSL

    SubShader
    {
        // 挂在网格上的"区域遮罩"式设置：
        //   Transparent 队列 + ZWrite Off + ZTest Always
        //   —— 片永远画在已渲染画面的上层，所以"片挡住哪里，哪里就有故障"，
        //      不受片自身在世界空间里的深度影响。
        //   如果你希望片像普通物体一样被前面的物体遮挡，改成 ZTest LEqual + Queue Geometry。
        //   Cull Off：单面 Quad 从正反两面都可见。
        //   Blend Off：直接覆盖，不做混合。
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "GlitchCombined"
 Tags
            {
                "LightMode" = "Eff"
            }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag_Combined

            ENDHLSL
        }
    }
}
