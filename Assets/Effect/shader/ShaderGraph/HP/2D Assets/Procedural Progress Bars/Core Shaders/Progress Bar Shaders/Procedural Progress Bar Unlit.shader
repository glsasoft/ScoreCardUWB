Shader "Effect/程序化进度条 Unlit"
{
    Properties
    {
        [HideInInspector] _MainTex("主贴图", 2D) = "white" {}
        _MainAlpha("总透明度", Range(0.0, 1.0)) = 1.0
        _Desaturate("去色", Range(0.0, 1.0)) = 0.0
        _Fill_Amount("填充量", Range(0.0, 1.0)) = 0.800000011920929
        _Second_Bar_Main_Blend("第二进度到主条混合", Range(0.0, 1.0)) = 0.0
        _Second_Bar_Invisible_Blend("第二进度到隐去混合", Range(0.0, 1.0)) = 0.0
        _Main_Bar_Fill_Amount("主条填充量", Range(0.0, 1.0)) = 0.5
        _End_Line_Visibility("末端线可见度", Range(0.0, 1.0)) = 1.0
        _General_Shape_Height("基础形状 高度", Range(0.0, 0.20000000298023224)) = 0.10000000149011612
        _General_Shape_Size_Offset("基础形状 尺寸偏移", Vector) = (0.05000000074505806, 0.02500000037252903, 0, 0)
        _General_Shape_Skew_Left_Top("基础形状 左上倾斜", Range(0.0, 1.0)) = 0.0
        _General_Shape_Skew_Left_Bottom("基础形状 左下倾斜", Range(0.0, 1.0)) = 0.0
        _General_Shape_Skew_Left_Edge_Place("基础形状 左侧倾斜 边缘位置", Range(0.0, 1.0)) = 0.5
        [Toggle] _General_Shape_Skew_Left_Invert("基础形状 左侧倾斜反转", Float) = 0
        _General_Shape_Skew_Right_Top("基础形状 右上倾斜", Range(0.0, 1.0)) = 0.0
        _General_Shape_Skew_Right_Bottom("基础形状 右下倾斜", Range(0.0, 1.0)) = 0.0
        _General_Shape_Skew_Right_Edge_Place("基础形状 右侧倾斜 边缘位置", Range(0.0, 1.0)) = 0.5
        [Toggle] _General_Shape_Skew_Right_Invert("基础形状 右侧倾斜反转", Float) = 1
        _General_Shape_Roundness("基础形状 圆角", Range(0.0, 0.25)) = 0.0
        _General_Shape_Roudness_Bottom_Left("基础形状 左下圆角", Range(0.0, 1.0)) = 0.0
        _General_Shape_Roudness_Top_Left("基础形状 左上圆角", Range(0.0, 1.0)) = 0.0
        _General_Shape_Roudness_Bottom_Right("基础形状 右下圆角", Range(0.0, 1.0)) = 0.0
        _General_Shape_Roudness_Top_Right("基础形状 右上圆角", Range(0.0, 1.0)) = 0.0
        [Toggle(_CUSTOM_SHAPE_ENABLE)] _CUSTOM_SHAPE_ENABLE("自定义形状 启用", Float) = 0
        _Custom_Shape_Noise_Guide_1("自定义形状 噪声引导 1", 2D) = "white" {}
        _Custom_Shape_Noise_Guide_1_Strength("自定义形状 噪声引导 1 强度", Range(-0.5, 0.5)) = 0.20000000298023224
        _Custom_Shape_Noise_Guide_1_Scroll_Speed("自定义形状 噪声引导 1 滚动速度", Vector) = (0.0, 0.0, 0, 0)
        [Toggle] _Custom_Shape_Use_Second_Texture("自定义形状 使用第二张贴图", Float) = 1
        _Custom_Shape_Noise_Guide_2("自定义形状 噪声引导 2", 2D) = "white" {}
        _Custom_Shape_Noise_Guide_2_Strength("自定义形状 噪声引导 2 强度", Range(-0.5, 0.5)) = 0.10000000149011612
        _Custom_Shape_Noise_Guide_2_Scroll_Speed("自定义形状 噪声引导 2 滚动速度", Vector) = (0.0, 0.0, 0, 0)
        _Simple_Outline_Thickness("简单描边 粗细", Range(0.0, 0.10000000149011612)) = 0.009999999776482582
        [HDR] _Simple_Outline_Color("简单描边 颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        [Toggle] _Simple_Outline_Use_Color_Alpha_For_Color_Blending("简单描边 用颜色透明度混合", Float) = 1
        [Toggle(_CUSTOM_OUTLINE_ENABLE)] _CUSTOM_OUTLINE_ENABLE("自定义描边 启用", Float) = 0
        [Toggle] _Custom_Outline_Depends_On_Fill_Amount("自定义描边 跟随填充量", Float) = 0
        _Custom_Outline_Size("自定义描边 大小", Range(0.949999988079071, 1.149999976158142)) = 0.9900000095367432
        _Custom_Outline_Offset("自定义描边 偏移", Range(-0.009999999776482582, 0.009999999776482582)) = 0.0
        _Custom_Outline_Smoothness("自定义描边 平滑", Range(0.0, 0.10000000149011612)) = 0.019999999552965164
        [HDR] _Custom_Outline_Color("自定义描边 颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        _Custom_Outline_Power("自定义描边 幂", Range(1.0, 5.0)) = 2.0
        _Custom_Outline_Strength("自定义描边 强度", Range(0.0, 1.0)) = 1.0
        [Toggle] _Custom_Outline_Use_Noise_Guides("自定义描边 使用噪声引导", Float) = 1
        _Custom_Outline_Noise_Guide_1("自定义描边 噪声引导 1", 2D) = "white" {}
        _Custom_Outline_Noise_Guide_1_Strength("自定义描边 噪声引导 1 强度", Range(-0.5, 0.5)) = 0.5
        _Custom_Outline_Noise_Guide_1_Scroll_Speed("自定义描边 噪声引导 1 滚动速度", Vector) = (-0.029999999329447746, -0.029999999329447746, 0, 0)
        _Custom_Outline_Noise_Guide_2("自定义描边 噪声引导 2", 2D) = "white" {}
        _Custom_Outline_Noise_Guide_2_Strength("自定义描边 噪声引导 2 强度", Range(-0.5, 0.5)) = 0.10000000149011612
        _Custom_Outline_Noise_Guide_2_Scroll_Speed("自定义描边 噪声引导 2 滚动速度", Vector) = (-0.019999999552965164, -0.019999999552965164, 0, 0)
        [Toggle] _Shadow_Enable("阴影 启用", Float) = 1
        _Shadow_Size("阴影 大小", Range(0.949999988079071, 1.149999976158142)) = 1.0
        _Shadow_Smoothness("阴影 平滑", Range(0.0, 0.10000000149011612)) = 0.029999999329447746
        [HDR] _Shadow_Color("阴影 颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        _Shadow_Strength("阴影 强度", Range(0.0, 1.0)) = 1.0
        _Shadow_Power("阴影 幂", Range(1.0, 5.0)) = 2.0
        _Fill_Skew("填充 倾斜", Range(-1.0, 1.0)) = -0.25
        [Toggle] _Fill_Use_Only_Background_Alpha("填充 仅用背景透明度", Float) = 0
        [HDR] _Fill_Background_color("填充 背景颜色", Color) = (0.07036010921001434, 0.00856812484562397, 0.004776953253895044, 0.800000011920929)
        [HDR] _Fill_Color_No_Gradient("填充 颜色（无渐变）", Color) = (0.679542601108551, 0.07618539035320282, 0.04817183315753937, 1.0)
        [Toggle] _Fill_Use_Horizontal_Gradient("填充 使用水平渐变", Float) = 1
        [HDR] _Fill_Gradient_Color_Left("填充 左侧渐变色", Color) = (0.07618539035320282, 0.020288562402129173, 0.020288562402129173, 1.0)
        [HDR] _Fill_Gradient_Color_Right("填充 右侧渐变色", Color) = (0.679542601108551, 0.07618539035320282, 0.04817183315753937, 1.0)
        [Toggle] _Fill_Second_Bar_Enable("第二进度 启用", Float) = 0
        [HDR] _Fill_Second_Bar_Color("第二进度 颜色", Color) = (0.623960554599762, 0.14126332104206085, 0.14126332104206085, 1.0)
        [Toggle] _Fill_Vertical_Darken_Enable("填充 纵向压暗 启用", Float) = 0
        [HDR] _Fill_Vertical_Darken_Color("填充 纵向压暗 颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        _Fill_Vertical_Darken_Amount("填充 纵向压暗 数量", Range(0.0, 1.0)) = 0.30000001192092896
        _Fill_Vertical_Darken_Power("填充 纵向压暗 幂", Range(1.0, 20.0)) = 7.0
        [Toggle] _Fill_Vertical_Darken_Use_Flat("填充 纵向压暗 使用平面", Float) = 0
        [Toggle] _Fill_Vertical_Darken_Flip("填充 纵向压暗 翻转", Float) = 0
        [Toggle(_FILL_OVERLAY_ENABLE)] _FILL_OVERLAY_ENABLE("填充叠加 启用", Float) = 0
        _Fill_Overlay_Strength("填充叠加 强度", Range(0.0, 1.0)) = 0.5
        [HDR] _Fill_Overlay_Fill_Color("填充叠加 填充 颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        _Fill_Overlay_Fill_Color_Impact("填充叠加 填充颜色影响", Range(0.0, 1.0)) = 1.0
        _Fill_Overlay_Fill_Alpha_Impact("填充叠加 填充透明度影响", Range(0.0, 1.0)) = 1.0
        [Toggle(_FILL_OVERLAY_USE_GUIDES)] _FILL_OVERLAY_USE_GUIDES("填充叠加 使用引导", Float) = 1
        [HDR] _Fill_Overlay_Background_Color("填充叠加 背景颜色", Color) = (0.0, 0.0, 0.0, 0.9019607901573181)
        _Fill_Overlay_Background_Color_Impact("填充叠加 背景颜色影响", Range(0.0, 1.0)) = 0.0
        _Fill_Overlay_Background_Alpha_Impact("填充叠加 背景透明度影响", Range(0.0, 1.0)) = 1.0
        _Fill_Overlay_Step_Offset("填充叠加 阶跃偏移", Range(0.0, 1.0)) = 0.07999999821186066
        _Fill_Overlay_Step_Smoothness("填充叠加 阶跃平滑", Range(0.0, 1.0)) = 0.20000000298023224
        _Fill_Overlay_Guide_Rotation("填充叠加 引导旋转", Range(0.0, 180.0)) = 0.0
        _Fill_Overlay_Inverse_Overlay("填充叠加 反转叠加", Range(0.0, 1.0)) = 0.0
        _Fill_Overlay_Noise_Guide_1("填充叠加 噪声引导 1", 2D) = "white" {}
        _Fill_Overlay_Noise_Guide_1_Strength("填充叠加 噪声引导 1 强度", Range(0.0, 1.0)) = 1.0
        _Fill_Overlay_Noise_Guide_1_Scroll_Speed("填充叠加 噪声引导 1 滚动速度", Vector) = (0.0, 0.0, 0, 0)
        _Fill_Overlay_Noise_Guide_2("填充叠加 噪声引导 2", 2D) = "white" {}
        _Fill_Overlay_Noise_Guide_2_Strength("填充叠加 噪声引导 2 强度", Range(0.0, 1.0)) = 1.0
        _Fill_Overlay_Noise_Guide_2_Scroll_Speed("填充叠加 噪声引导 2 滚动速度", Vector) = (0.0, 0.0, 0, 0)
        [Toggle(_FILL_SEGMENTS_ENABLE)] _FILL_SEGMENTS_ENABLE("填充分段 启用", Float) = 0
        _Fill_Segments_Segments_Number("填充分段 段数", Range(0.0, 1.0)) = 5.0
        _Fill_Segments_Skew("填充分段 倾斜", Range(-1.0, 1.0)) = 0.0
        _Fill_Segments_Width("填充分段 宽度", Range(0.0, 0.10000000149011612)) = 0.02500000037252903
        _Fill_Segments_Edge_Smoothness("填充分段 边缘平滑", Range(0.0, 0.10000000149011612)) = 0.0
        [Toggle] _Fill_Segments_Use_Edge_Step("填充分段 使用边缘阶跃", Float) = 1
        _Fill_Segments_Custom_Shape_Strength("填充分段 自定义形状强度", Range(0.0, 4.0)) = 0.0
        _Fill_Segments_Top_Strength("填充分段 顶部强度", Range(-1.0, 1.0)) = -0.30000001192092896
        _Fill_Segments_Top_Smoothness("填充分段 顶部平滑", Range(0.0, 0.10000000149011612)) = 0.019999999552965164
        _Fill_Segments_Top_Offset("填充分段 顶部偏移", Range(0.0, 0.10000000149011612)) = 0.009999999776482582
        _Fill_Segments_Add_Alpha_To_1("填充分段 透明度补到 1", Range(0.0, 1.0)) = 1.0
        _Fill_Segments_Multiply_Alpha_To_0("填充分段 透明度乘到 0", Range(0.0, 1.0)) = 0.0
        [HDR] _Fill_Segments_Color("填充分段 颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        _Fill_Segments_Strength_On_Color("填充分段 作用于颜色的强度", Range(0.0, 1.0)) = 1.0
        [Toggle] _Fill_Enable_Smooth_Edge("填充 启用平滑边缘", Float) = 0
        _Fill_Smooth_Edge_Value("填充 平滑边缘值", Range(0.0, 0.5)) = 0.009999999776482582
        [Toggle] _Fill_Enable_Edge_Guide("填充 启用边缘引导", Float) = 0
        _Fill_Texture_Guide("填充 贴图引导", 2D) = "white" {}
        _Fill_Guide_Strength("填充 引导强度", Range(0.0, 1.0)) = 0.20000000298023224
        _Fill_Scroll_speed_XY("填充 滚动速度（横纵）", Vector) = (0.0, 0.0, 0, 0)
        [Toggle] _Fill_Flip_Guide_Texture_90_degrees_rotation("填充 翻转引导贴图（旋转90度）", Float) = 0
        [Toggle] _Fill_Flip_Guide_Texture_Value("填充 翻转引导贴图数值", Float) = 0
        [Toggle] _Fill_Shadow_Enable("填充阴影 启用", Float) = 0
        [Toggle] _Fill_Shadow_Use_Only_Color_Alpha("填充阴影 仅用颜色透明度", Float) = 0
        [HDR] _Fill_Shadow_Color("填充阴影 颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        [Toggle] _Fill_Shadow_Use_On_Background("填充阴影 作用于背景", Float) = 1
        [Toggle] _Fill_Shadow_Use_Only_Background_Color_Alpha("填充阴影 仅用背景颜色透明度", Float) = 0
        [HDR] _Fill_Shadow_Background_Color("填充阴影 背景颜色", Color) = (0.07036010921001434, 0.00856812484562397, 0.004776953253895044, 1.0)
        _Fill_Shadow_Power_Hardness("填充阴影 幂（硬度）", Range(0.0, 5.0)) = 1.0
        _Fill_Shadow_Multiplier_Shadow_Amount("填充阴影 数量倍率", Range(2.0, 10.0)) = 3.0
        [Toggle] _Fill_Shadow_Use_Hard_Step("填充阴影 使用硬边阶跃", Float) = 0
        [Toggle] _Fill_Shadow_Use_Texture_Guide("填充阴影 使用贴图引导", Float) = 0
        _Fill_Shadow_Texture_Guide("填充阴影 贴图引导", 2D) = "white" {}
        [Toggle] _Fill_Shadow_Flip_Guide_Texture("填充阴影 翻转引导贴图", Float) = 0
        _Fill_Shadow_Guide_Strength("填充阴影 引导强度", Range(0.0, 2.0)) = 2.0
        _Fill_Shadow_Scroll_Speed_XY("填充阴影 滚动速度（横纵）", Vector) = (0.0, 0.0, 0, 0)
        [Toggle(_END_LINE_ENABLE)] _END_LINE_ENABLE("末端线 启用", Float) = 0
        [HDR] _End_Line_Color("末端线 颜色", Color) = (0.9473068714141846, 0.12743772566318512, 0.12743772566318512, 1.0)
        _End_Line_Width_Left("末端线 左侧宽度", Range(0.0, 0.10000000149011612)) = 0.02500000037252903
        _End_Line_Smoothness_Left("末端线 左侧平滑", Range(0.0, 0.4000000059604645)) = 0.029999999329447746
        _End_Line_Width_Right("末端线 右侧宽度", Range(0.0, 0.10000000149011612)) = 0.0
        _End_Line_Smoothness_Right("末端线 右侧平滑", Range(0.0, 0.4000000059604645)) = 0.10000000149011612
        _End_Line_Offset("末端线 偏移", Range(-0.10000000149011612, 0.10000000149011612)) = 0.0
        _End_Line_Multiplier("末端线 倍率", Float) = 2.0
        _End_Line_Power("末端线 幂", Float) = 2.0
        [Toggle] _End_Line_Multiply_With_Fill_Shadow("末端线 与填充阴影相乘", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ _FILL_OVERLAY_USE_GUIDES
            #pragma shader_feature_local _ _FILL_SEGMENTS_ENABLE
            #pragma shader_feature_local _ _CUSTOM_SHAPE_ENABLE
            #pragma shader_feature_local _ _CUSTOM_OUTLINE_ENABLE
            #pragma shader_feature_local _ _END_LINE_ENABLE
            #pragma shader_feature_local _ _FILL_OVERLAY_ENABLE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "../../../Common/Subgraphs/2D Sdfs/Core/2D_SDFs.hlsl"

            bool IsGammaSpace()
            {
                #ifdef UNITY_COLORSPACE_GAMMA
                    return true;
                #else
                    return false;
                #endif
            }
            

            struct UnityTexture2D
            {
                Texture2D tex;
                SamplerState samplerstate;
                float4 scaleTranslate;
            };

            UnityTexture2D UnityBuildTexture2DStruct(Texture2D tex, SamplerState sampl, float4 st)
            {
                UnityTexture2D r;
                r.tex = tex;
                r.samplerstate = sampl;
                r.scaleTranslate = st;
                return r;
            }
TEXTURE2D(_Custom_Shape_Noise_Guide_1);
            SAMPLER(sampler_Custom_Shape_Noise_Guide_1);
            TEXTURE2D(_Custom_Shape_Noise_Guide_2);
            SAMPLER(sampler_Custom_Shape_Noise_Guide_2);
            TEXTURE2D(_Custom_Outline_Noise_Guide_1);
            SAMPLER(sampler_Custom_Outline_Noise_Guide_1);
            TEXTURE2D(_Custom_Outline_Noise_Guide_2);
            SAMPLER(sampler_Custom_Outline_Noise_Guide_2);
            TEXTURE2D(_Fill_Overlay_Noise_Guide_1);
            SAMPLER(sampler_Fill_Overlay_Noise_Guide_1);
            TEXTURE2D(_Fill_Overlay_Noise_Guide_2);
            SAMPLER(sampler_Fill_Overlay_Noise_Guide_2);
            TEXTURE2D(_Fill_Texture_Guide);
            SAMPLER(sampler_Fill_Texture_Guide);
            TEXTURE2D(_Fill_Shadow_Texture_Guide);
            SAMPLER(sampler_Fill_Shadow_Texture_Guide);

            CBUFFER_START(UnityPerMaterial)
    float _MainAlpha;
    float _Desaturate;
    float _Fill_Amount;
    float _Second_Bar_Main_Blend;
    float _Second_Bar_Invisible_Blend;
    float _Main_Bar_Fill_Amount;
    float _End_Line_Visibility;
    float _General_Shape_Height;
    float4 _General_Shape_Size_Offset;
    float _General_Shape_Skew_Left_Top;
    float _General_Shape_Skew_Left_Bottom;
    float _General_Shape_Skew_Left_Edge_Place;
    float _General_Shape_Skew_Left_Invert;
    float _General_Shape_Skew_Right_Top;
    float _General_Shape_Skew_Right_Bottom;
    float _General_Shape_Skew_Right_Edge_Place;
    float _General_Shape_Skew_Right_Invert;
    float _General_Shape_Roundness;
    float _General_Shape_Roudness_Bottom_Left;
    float _General_Shape_Roudness_Top_Left;
    float _General_Shape_Roudness_Bottom_Right;
    float _General_Shape_Roudness_Top_Right;
    float4 _Custom_Shape_Noise_Guide_1_ST;
    float _Custom_Shape_Noise_Guide_1_Strength;
    float4 _Custom_Shape_Noise_Guide_1_Scroll_Speed;
    float _Custom_Shape_Use_Second_Texture;
    float4 _Custom_Shape_Noise_Guide_2_ST;
    float _Custom_Shape_Noise_Guide_2_Strength;
    float4 _Custom_Shape_Noise_Guide_2_Scroll_Speed;
    float _Simple_Outline_Thickness;
    float4 _Simple_Outline_Color;
    float _Simple_Outline_Use_Color_Alpha_For_Color_Blending;
    float _Custom_Outline_Depends_On_Fill_Amount;
    float _Custom_Outline_Size;
    float _Custom_Outline_Offset;
    float _Custom_Outline_Smoothness;
    float4 _Custom_Outline_Color;
    float _Custom_Outline_Power;
    float _Custom_Outline_Strength;
    float _Custom_Outline_Use_Noise_Guides;
    float4 _Custom_Outline_Noise_Guide_1_ST;
    float _Custom_Outline_Noise_Guide_1_Strength;
    float4 _Custom_Outline_Noise_Guide_1_Scroll_Speed;
    float4 _Custom_Outline_Noise_Guide_2_ST;
    float _Custom_Outline_Noise_Guide_2_Strength;
    float4 _Custom_Outline_Noise_Guide_2_Scroll_Speed;
    float _Shadow_Enable;
    float _Shadow_Size;
    float _Shadow_Smoothness;
    float4 _Shadow_Color;
    float _Shadow_Strength;
    float _Shadow_Power;
    float _Fill_Skew;
    float _Fill_Use_Only_Background_Alpha;
    float4 _Fill_Background_color;
    float4 _Fill_Color_No_Gradient;
    float _Fill_Use_Horizontal_Gradient;
    float4 _Fill_Gradient_Color_Left;
    float4 _Fill_Gradient_Color_Right;
    float _Fill_Second_Bar_Enable;
    float4 _Fill_Second_Bar_Color;
    float _Fill_Vertical_Darken_Enable;
    float4 _Fill_Vertical_Darken_Color;
    float _Fill_Vertical_Darken_Amount;
    float _Fill_Vertical_Darken_Power;
    float _Fill_Vertical_Darken_Use_Flat;
    float _Fill_Vertical_Darken_Flip;
    float _Fill_Overlay_Strength;
    float4 _Fill_Overlay_Fill_Color;
    float _Fill_Overlay_Fill_Color_Impact;
    float _Fill_Overlay_Fill_Alpha_Impact;
    float4 _Fill_Overlay_Background_Color;
    float _Fill_Overlay_Background_Color_Impact;
    float _Fill_Overlay_Background_Alpha_Impact;
    float _Fill_Overlay_Step_Offset;
    float _Fill_Overlay_Step_Smoothness;
    float _Fill_Overlay_Guide_Rotation;
    float _Fill_Overlay_Inverse_Overlay;
    float4 _Fill_Overlay_Noise_Guide_1_ST;
    float _Fill_Overlay_Noise_Guide_1_Strength;
    float4 _Fill_Overlay_Noise_Guide_1_Scroll_Speed;
    float4 _Fill_Overlay_Noise_Guide_2_ST;
    float _Fill_Overlay_Noise_Guide_2_Strength;
    float4 _Fill_Overlay_Noise_Guide_2_Scroll_Speed;
    float _Fill_Segments_Segments_Number;
    float _Fill_Segments_Skew;
    float _Fill_Segments_Width;
    float _Fill_Segments_Edge_Smoothness;
    float _Fill_Segments_Use_Edge_Step;
    float _Fill_Segments_Custom_Shape_Strength;
    float _Fill_Segments_Top_Strength;
    float _Fill_Segments_Top_Smoothness;
    float _Fill_Segments_Top_Offset;
    float _Fill_Segments_Add_Alpha_To_1;
    float _Fill_Segments_Multiply_Alpha_To_0;
    float4 _Fill_Segments_Color;
    float _Fill_Segments_Strength_On_Color;
    float _Fill_Enable_Smooth_Edge;
    float _Fill_Smooth_Edge_Value;
    float _Fill_Enable_Edge_Guide;
    float4 _Fill_Texture_Guide_ST;
    float _Fill_Guide_Strength;
    float4 _Fill_Scroll_speed_XY;
    float _Fill_Flip_Guide_Texture_90_degrees_rotation;
    float _Fill_Flip_Guide_Texture_Value;
    float _Fill_Shadow_Enable;
    float _Fill_Shadow_Use_Only_Color_Alpha;
    float4 _Fill_Shadow_Color;
    float _Fill_Shadow_Use_On_Background;
    float _Fill_Shadow_Use_Only_Background_Color_Alpha;
    float4 _Fill_Shadow_Background_Color;
    float _Fill_Shadow_Power_Hardness;
    float _Fill_Shadow_Multiplier_Shadow_Amount;
    float _Fill_Shadow_Use_Hard_Step;
    float _Fill_Shadow_Use_Texture_Guide;
    float4 _Fill_Shadow_Texture_Guide_ST;
    float _Fill_Shadow_Flip_Guide_Texture;
    float _Fill_Shadow_Guide_Strength;
    float4 _Fill_Shadow_Scroll_Speed_XY;
    float4 _End_Line_Color;
    float _End_Line_Width_Left;
    float _End_Line_Smoothness_Left;
    float _End_Line_Width_Right;
    float _End_Line_Smoothness_Right;
    float _End_Line_Offset;
    float _End_Line_Multiplier;
    float _End_Line_Power;
    float _End_Line_Multiply_With_Fill_Shadow;
            CBUFFER_END

            struct SGInput
            {
                float4 uv0;
                float3 time;
                float3 positionOS;
                float3 positionWS;
                float3 normalWS;
            };

            void sg_Anti_Aliased_Step(float _Edge, bool _Edge_IsConnected, float _In, bool _In_IsConnected, SGInput IN, out float Out_1)
            {
                float v0_Out = _Edge;
                bool v0_Out_IsConnected = _Edge_IsConnected;
                float v1_Out = (v0_Out * -1.0);
                float v2_Out = _In;
                bool v2_Out_IsConnected = _In_IsConnected;
                float v3_Out = (v1_Out + v2_Out);
                float v4_Out = (abs(ddx(v3_Out)) + abs(ddy(v3_Out)));
                float v5_Out = (v3_Out / v4_Out);
                float v6_Out = saturate(v5_Out);
                Out_1 = v6_Out;
            }

            void sg_Box_SDF(float2 _Uv, bool _Uv_IsConnected, float2 _Size, bool _Size_IsConnected, float4 _Roudness, bool _Roudness_IsConnected, float2 _Uv_Offset, bool _Uv_Offset_IsConnected, SGInput IN, out float Out_1)
            {
                float2 v0_Out = _Uv;
                bool v0_Out_IsConnected = _Uv_IsConnected;
                float4 v1_Out = IN.uv0;
                float2 v2_Out = v0_Out_IsConnected ? v0_Out : (v1_Out.xy);
                float2 v3_Out = _Uv_Offset;
                bool v3_Out_IsConnected = _Uv_Offset_IsConnected;
                float2 v4_Out = (v2_Out + v3_Out);
                float2 v5_Out = _Size;
                bool v5_Out_IsConnected = _Size_IsConnected;
                float4 v6_Out = _Roudness;
                bool v6_Out_IsConnected = _Roudness_IsConnected;
                float v7_Out;
                RoundedBox_float(v4_Out, v5_Out, v6_Out, v7_Out);
                Out_1 = v7_Out;
            }

            void sg_2D_Box(float _Roundess, bool _Roundess_IsConnected, float4 _Per_Angle_Roundess, bool _Per_Angle_Roundess_IsConnected, float2 _Size, bool _Size_IsConnected, float _Outline_Size, bool _Outline_Size_IsConnected, float _Glow_Power, bool _Glow_Power_IsConnected, float _Fill_Shadow_Multiplier, bool _Fill_Shadow_Multiplier_IsConnected, float _Fill_Shadow_Power, bool _Fill_Shadow_Power_IsConnected, float2 _Uv, bool _Uv_IsConnected, float2 _Uv_Offset, bool _Uv_Offset_IsConnected, float _Custom_Shape_Offset, bool _Custom_Shape_Offset_IsConnected, SGInput IN, out float Fill_0, out float Fill_Hard_Step_5, out float Fill_Shadow_3, out float Outline_1, out float Glow_2, out float SDF_4)
            {
                float v0_Out = _Fill_Shadow_Multiplier;
                bool v0_Out_IsConnected = _Fill_Shadow_Multiplier_IsConnected;
                float v1_Out = _Fill_Shadow_Power;
                bool v1_Out_IsConnected = _Fill_Shadow_Power_IsConnected;
                float v2_Out = _Outline_Size;
                bool v2_Out_IsConnected = _Outline_Size_IsConnected;
                float v3_Out = (v2_Out * -1.0);
                float v4_Out = _Glow_Power;
                bool v4_Out_IsConnected = _Glow_Power_IsConnected;
                float2 v5_Out = _Uv;
                bool v5_Out_IsConnected = _Uv_IsConnected;
                float4 v6_Out = IN.uv0;
                float2 v7_Out = v5_Out_IsConnected ? v5_Out : (v6_Out.xy);
                float2 v8_Out = _Size;
                bool v8_Out_IsConnected = _Size_IsConnected;
                float2 v9_Out = _Size;
                bool v9_Out_IsConnected = _Size_IsConnected;
                float v10_R = (v9_Out)[0];
                float v11_G = (v9_Out)[1];
                float v12_B = 0.0;
                float v13_A = 0.0;
                float v14_Out = min(v10_R, v11_G);
                float4 v15_Out = _Per_Angle_Roundess;
                bool v15_Out_IsConnected = _Per_Angle_Roundess_IsConnected;
                float4 v16_Out = lerp(float4(0.0, 0.0, 0.0, 0.0), (v14_Out.xxxx), v15_Out);
                float2 v17_Out = _Uv_Offset;
                bool v17_Out_IsConnected = _Uv_Offset_IsConnected;
                float2 v18_Out = (v17_Out + float2(-0.5, -0.5));
                float v19_Out;
                sg_Box_SDF(v7_Out, true, v8_Out, true, v16_Out, true, v18_Out, true, IN, v19_Out);
                float v20_Out = _Roundess;
                bool v20_Out_IsConnected = _Roundess_IsConnected;
                float v21_Out = (v19_Out - v20_Out);
                float v22_Out = _Custom_Shape_Offset;
                bool v22_Out_IsConnected = _Custom_Shape_Offset_IsConnected;
                float v23_Out = (v21_Out + v22_Out);
                float v24_o = v23_Out;
                float v25_Out = step(v24_o, 0.0);
                float v26_o = v25_Out;
                float v27_Out = (v24_o * -1.0);
                float v28_Out = (v27_Out * v0_Out);
                float v29_Out = saturate(v28_Out);
                float v30_Out = pow(abs(v29_Out), v1_Out);
                float v31_Out;
                sg_Anti_Aliased_Step(v24_o, true, 0.0, false, IN, v31_Out);
                float v32_o = v31_Out;
                float v33_Out;
                sg_Anti_Aliased_Step(v24_o, true, v3_Out, true, IN, v33_Out);
                float v34_Out = (v31_Out - v33_Out);
                float v35_Out = (1.0 - v24_o);
                float v36_Out = saturate(v35_Out);
                float v37_Out = pow(abs(v36_Out), v4_Out);
                Fill_0 = v32_o;
                Fill_Hard_Step_5 = v26_o;
                Fill_Shadow_3 = v30_Out;
                Outline_1 = v34_Out;
                Glow_2 = v37_Out;
                SDF_4 = v24_o;
            }

            void sg_General_Shape(float _General_Shape_Skew_Left_Top, bool _General_Shape_Skew_Left_Top_IsConnected, float _General_Shape_Skew_Left_Bottom, bool _General_Shape_Skew_Left_Bottom_IsConnected, float _General_Shape_Skew_Right_Top, bool _General_Shape_Skew_Right_Top_IsConnected, float _General_Shape_Skew_Right_Bottom, bool _General_Shape_Skew_Right_Bottom_IsConnected, float _General_Shape_Skew_Right_Invert, bool _General_Shape_Skew_Right_Invert_IsConnected, float _General_Shape_Skew_Left_Invert, bool _General_Shape_Skew_Left_Invert_IsConnected, float _General_Shape_Skew_Right_Edge_Place, bool _General_Shape_Skew_Right_Edge_Place_IsConnected, float _General_Shape_Height, bool _General_Shape_Height_IsConnected, float _General_Shape_Skew_Left_Edge_Place, bool _General_Shape_Skew_Left_Edge_Place_IsConnected, SGInput IN, out float2 OutVector2_1)
            {
                float v0_Out = _General_Shape_Skew_Left_Top;
                bool v0_Out_IsConnected = _General_Shape_Skew_Left_Top_IsConnected;
                float v1_Out = v0_Out;
                float v2_Out = v1_Out;
                float v3_Out = _General_Shape_Skew_Left_Bottom;
                bool v3_Out_IsConnected = _General_Shape_Skew_Left_Bottom_IsConnected;
                float v4_Out = (v3_Out * -1.0);
                float v5_Out = v4_Out;
                float v6_Out = v5_Out;
                float v7_Out = 0.0;
                float v8_Out = _General_Shape_Height;
                bool v8_Out_IsConnected = _General_Shape_Height_IsConnected;
                float v9_Out = (v7_Out + v8_Out);
                float v10_Out = (v7_Out - v8_Out);
                float v11_Out = _General_Shape_Skew_Left_Edge_Place;
                bool v11_Out_IsConnected = _General_Shape_Skew_Left_Edge_Place_IsConnected;
                float v12_Out = lerp(v9_Out, v10_Out, v11_Out);
                float v13_Out = (v12_Out * -1.0);
                float v14_Out = v13_Out;
                float v15_Out = _General_Shape_Skew_Left_Invert;
                bool v15_Out_IsConnected = _General_Shape_Skew_Left_Invert_IsConnected;
                float v16_Out = ((bool)v15_Out) ? -1.0 : 1.0;
                float v17_Out = _General_Shape_Skew_Right_Top;
                bool v17_Out_IsConnected = _General_Shape_Skew_Right_Top_IsConnected;
                float v18_Out = v17_Out;
                float v19_Out = v18_Out;
                float v20_Out = _General_Shape_Skew_Right_Bottom;
                bool v20_Out_IsConnected = _General_Shape_Skew_Right_Bottom_IsConnected;
                float v21_Out = (v20_Out * -1.0);
                float v22_Out = v21_Out;
                float v23_Out = v22_Out;
                float v24_Out = 0.0;
                float v25_Out = _General_Shape_Height;
                bool v25_Out_IsConnected = _General_Shape_Height_IsConnected;
                float v26_Out = (v24_Out + v25_Out);
                float v27_Out = (v24_Out - v25_Out);
                float v28_Out = _General_Shape_Skew_Right_Edge_Place;
                bool v28_Out_IsConnected = _General_Shape_Skew_Right_Edge_Place_IsConnected;
                float v29_Out = lerp(v26_Out, v27_Out, v28_Out);
                float v30_Out = (v29_Out * -1.0);
                float v31_Out = v30_Out;
                float v32_Out = _General_Shape_Skew_Right_Invert;
                bool v32_Out_IsConnected = _General_Shape_Skew_Right_Invert_IsConnected;
                float v33_Out = ((bool)v32_Out) ? -1.0 : 1.0;
                float4 v34_Out = IN.uv0;
                float2 v35_Out = (v34_Out).xy;
                float v36_R = (v35_Out)[0];
                float v37_G = (v35_Out)[1];
                float v38_B = 0.0;
                float v39_A = 0.0;
                float v40_Out = (v37_G + v14_Out);
                float v41_Out = (v2_Out * v40_Out);
                float v42_Out = step(v36_R, 0.5);
                float v43_Out = (v41_Out * v42_Out);
                float v44_Out = saturate(v43_Out);
                float v45_Out = (v37_G + v14_Out);
                float v46_Out = (v6_Out * v45_Out);
                float v47_Out = step(v36_R, 0.5);
                float v48_Out = (v46_Out * v47_Out);
                float v49_Out = saturate(v48_Out);
                float v50_Out = (v44_Out + v49_Out);
                float v51_Out = (v50_Out * v16_Out);
                float v52_Out = (v36_R - v51_Out);
                float v53_Out = (v37_G + v31_Out);
                float v54_Out = (v19_Out * v53_Out);
                float v55_Out = step(0.5, v36_R);
                float v56_Out = (v54_Out * v55_Out);
                float v57_Out = saturate(v56_Out);
                float v58_Out = (v37_G + v31_Out);
                float v59_Out = (v23_Out * v58_Out);
                float v60_Out = step(0.5, v36_R);
                float v61_Out = (v59_Out * v60_Out);
                float v62_Out = saturate(v61_Out);
                float v63_Out = (v57_Out + v62_Out);
                float v64_Out = (v63_Out * v33_Out);
                float v65_Out = (v52_Out - v64_Out);
                float2 v66_Out = float2(v65_Out, v37_G);
                OutVector2_1 = v66_Out;
            }

            void sg_Fill_Amount(float _Fill_Amount, bool _Fill_Amount_IsConnected, float _Fill_Skew, bool _Fill_Skew_IsConnected, float _General_Shape_Height, bool _General_Shape_Height_IsConnected, float _Fill_Enable_Smooth_Edge, bool _Fill_Enable_Smooth_Edge_IsConnected, float _Fill_Smooth_Edge_Value, bool _Fill_Smooth_Edge_Value_IsConnected, float _Fill_Enable_Edge_Guide, bool _Fill_Enable_Edge_Guide_IsConnected, float _Fill_Guide_Strength, bool _Fill_Guide_Strength_IsConnected, float2 _General_Shape_Size_Offset, bool _General_Shape_Size_Offset_IsConnected, SGInput IN, out float OutVector1_1, out float2 OutVector2_2)
            {
                float v0_Out = _Fill_Amount;
                bool v0_Out_IsConnected = _Fill_Amount_IsConnected;
                float v1_Out = _General_Shape_Height;
                bool v1_Out_IsConnected = _General_Shape_Height_IsConnected;
                float v2_Out = (v1_Out * -1.0);
                float v3_Out = _Fill_Skew;
                bool v3_Out_IsConnected = _Fill_Skew_IsConnected;
                float v4_Out = sign(v3_Out);
                float v5_Out = (v4_Out * v2_Out);
                float v6_Out = (v5_Out * v3_Out);
                float v7_Out = (v4_Out * v1_Out);
                float v8_Out = (v7_Out * v3_Out);
                float v9_Out = _Fill_Enable_Smooth_Edge;
                bool v9_Out_IsConnected = _Fill_Enable_Smooth_Edge_IsConnected;
                float v10_Out = _Fill_Smooth_Edge_Value;
                bool v10_Out_IsConnected = _Fill_Smooth_Edge_Value_IsConnected;
                float v11_Out = ((bool)v9_Out) ? v10_Out : 0.0;
                float v12_Out = (v6_Out - v11_Out);
                float v13_Out = _Fill_Enable_Edge_Guide;
                bool v13_Out_IsConnected = _Fill_Enable_Edge_Guide_IsConnected;
                float v14_Out = _Fill_Guide_Strength;
                bool v14_Out_IsConnected = _Fill_Guide_Strength_IsConnected;
                float v15_Out = abs(v14_Out);
                float v16_Out = ((bool)v13_Out) ? v15_Out : 0.0;
                float v17_Out = (v11_Out + v16_Out);
                float v18_Out = (1.0 + v17_Out);
                float v19_Out = (v8_Out + v18_Out);
                float2 v20_Out = float2(v12_Out, v19_Out);
                float2 v21_Out = _General_Shape_Size_Offset;
                bool v21_Out_IsConnected = _General_Shape_Size_Offset_IsConnected;
                float v22_R = (v21_Out)[0];
                float v23_G = (v21_Out)[1];
                float v24_B = 0.0;
                float v25_A = 0.0;
                float v26_Out = (v22_R * 1.0);
                float v27_Out = (v22_R * -1.0);
                float2 v28_Out = float2(v26_Out, v27_Out);
                float2 v29_Out = (v20_Out + v28_Out);
                float v30_Out = (v29_Out.x + (v0_Out - float2(1.0, 0.0).x) * (v29_Out.y - v29_Out.x) / (float2(1.0, 0.0).y - float2(1.0, 0.0).x));
                OutVector1_1 = v30_Out;
                OutVector2_2 = v29_Out;
            }

            void sg_Smoothed_Step(float _Edge, bool _Edge_IsConnected, float _In, bool _In_IsConnected, float _Smooth_Value, bool _Smooth_Value_IsConnected, SGInput IN, out float Out_1)
            {
                float v0_Out = _Edge;
                bool v0_Out_IsConnected = _Edge_IsConnected;
                float v1_Out = v0_Out;
                float v2_Out = _Smooth_Value;
                bool v2_Out_IsConnected = _Smooth_Value_IsConnected;
                float v3_Out = v2_Out;
                float v4_Out = (v1_Out - v3_Out);
                float v5_Out = (v1_Out + v3_Out);
                float v6_Out = _In;
                bool v6_Out_IsConnected = _In_IsConnected;
                float v7_Out = smoothstep(v4_Out, v5_Out, v6_Out);
                Out_1 = v7_Out;
            }

            void sg_Sample_Scrolling_Texture(float2 _Scroll_Speed, bool _Scroll_Speed_IsConnected, UnityTexture2D _Texture, bool _Texture_IsConnected, float2 _UVs, bool _UVs_IsConnected, SGInput IN, out float OutVector1_1)
            {
                UnityTexture2D v0_Out = _Texture;
                bool v0_Out_IsConnected = _Texture_IsConnected;
                float2 v1_Out = _UVs;
                bool v1_Out_IsConnected = _UVs_IsConnected;
                float4 v2_Out = IN.uv0;
                float2 v3_Out = v1_Out_IsConnected ? v1_Out : (v2_Out.xy);
                float v4_Time = IN.time.x;
                float v5_Sine_Time = IN.time.y;
                float v6_Cosine_Time = IN.time.z;
                float v7_Delta_Time = unity_DeltaTime.x;
                float v8_Smooth_Delta = unity_DeltaTime.z;
                float2 v9_Out = _Scroll_Speed;
                bool v9_Out_IsConnected = _Scroll_Speed_IsConnected;
                float2 v10_Out = ((v4_Time.xx) * v9_Out);
                float2 v11_Out = (v3_Out * float2(1.0, 1.0) + v10_Out);
                UnityTexture2D v12_RGBA_tex = v0_Out;
                float2 v12_RGBA_uv = v12_RGBA_tex.scaleTranslate.xy * v11_Out + v12_RGBA_tex.scaleTranslate.zw;
                float4 v12_RGBA = SAMPLE_TEXTURE2D(v12_RGBA_tex.tex, v12_RGBA_tex.samplerstate, v12_RGBA_uv);
                float v13_R = v12_RGBA.r;
                float v14_G = v12_RGBA.g;
                float v15_B = v12_RGBA.b;
                float v16_A = v12_RGBA.a;
                OutVector1_1 = v13_R;
            }

            void sg_Fill_Overlay(UnityTexture2D _Fill_Overlay_Noise_Guide_1, bool _Fill_Overlay_Noise_Guide_1_IsConnected, float2 _Fill_Overlay_Noise_Guide_1_Scroll_Speed, bool _Fill_Overlay_Noise_Guide_1_Scroll_Speed_IsConnected, UnityTexture2D _Fill_Overlay_Noise_Guide_2, bool _Fill_Overlay_Noise_Guide_2_IsConnected, float2 _Fill_Overlay_Noise_Guide_2_Scroll_Speed, bool _Fill_Overlay_Noise_Guide_2_Scroll_Speed_IsConnected, float _Fill_Overlay_Noise_Guide_1_Strength, bool _Fill_Overlay_Noise_Guide_1_Strength_IsConnected, float _Fill_Overlay_Noise_Guide_2_Strength, bool _Fill_Overlay_Noise_Guide_2_Strength_IsConnected, float _Fill_Overlay_Step_Offset, bool _Fill_Overlay_Step_Offset_IsConnected, float _Fill_Overlay_Step_Smoothness, bool _Fill_Overlay_Step_Smoothness_IsConnected, float _Fill_Overlay_Inverse_Overlay, bool _Fill_Overlay_Inverse_Overlay_IsConnected, float _Fill_Overlay_Guide_Rotation, bool _Fill_Overlay_Guide_Rotation_IsConnected, SGInput IN, out float OutVector1_1)
            {
                float v0_Out = _Fill_Overlay_Inverse_Overlay;
                bool v0_Out_IsConnected = _Fill_Overlay_Inverse_Overlay_IsConnected;
                float v1_Out = _Fill_Overlay_Step_Offset;
                bool v1_Out_IsConnected = _Fill_Overlay_Step_Offset_IsConnected;
                float2 v2_Out = _Fill_Overlay_Noise_Guide_1_Scroll_Speed;
                bool v2_Out_IsConnected = _Fill_Overlay_Noise_Guide_1_Scroll_Speed_IsConnected;
                UnityTexture2D v3_Out = _Fill_Overlay_Noise_Guide_1;
                bool v3_Out_IsConnected = _Fill_Overlay_Noise_Guide_1_IsConnected;
                float v4_Out = _Fill_Overlay_Noise_Guide_1_Strength;
                bool v4_Out_IsConnected = _Fill_Overlay_Noise_Guide_1_Strength_IsConnected;
                float2 v5_Out = _Fill_Overlay_Noise_Guide_2_Scroll_Speed;
                bool v5_Out_IsConnected = _Fill_Overlay_Noise_Guide_2_Scroll_Speed_IsConnected;
                UnityTexture2D v6_Out = _Fill_Overlay_Noise_Guide_2;
                bool v6_Out_IsConnected = _Fill_Overlay_Noise_Guide_2_IsConnected;
                float4 v7_Out = IN.uv0;
                float v8_Out = _Fill_Overlay_Guide_Rotation;
                bool v8_Out_IsConnected = _Fill_Overlay_Guide_Rotation_IsConnected;
                float v9_Out_rot = v8_Out * (3.1415926 / 180.0);
                float2 v9_Out_uv = (v7_Out.xy) - float2(0.5, 0.5);
                float v9_Out_s = sin(v9_Out_rot);
                float v9_Out_c = cos(v9_Out_rot);
                float2x2 v9_Out_m = float2x2(v9_Out_c, -v9_Out_s, v9_Out_s, v9_Out_c);
                v9_Out_m *= 0.5;
                v9_Out_m += 0.5;
                v9_Out_m = v9_Out_m * 2.0 - 1.0;
                v9_Out_uv = mul(v9_Out_uv, v9_Out_m);
                float2 v9_Out = v9_Out_uv + float2(0.5, 0.5);
                float v10_OutVector1;
                sg_Sample_Scrolling_Texture(v2_Out, true, v3_Out, true, v9_Out, true, IN, v10_OutVector1);
                float v11_Out = lerp(1.0, v10_OutVector1, v4_Out);
                float v12_OutVector1;
                sg_Sample_Scrolling_Texture(v5_Out, true, v6_Out, true, v9_Out, true, IN, v12_OutVector1);
                float v13_Out = _Fill_Overlay_Noise_Guide_2_Strength;
                bool v13_Out_IsConnected = _Fill_Overlay_Noise_Guide_2_Strength_IsConnected;
                float v14_Out = lerp(1.0, v12_OutVector1, v13_Out);
                float v15_Out = (v11_Out * v14_Out);
                float v16_Out = _Fill_Overlay_Step_Smoothness;
                bool v16_Out_IsConnected = _Fill_Overlay_Step_Smoothness_IsConnected;
                float v17_Out;
                sg_Smoothed_Step(v1_Out, true, v15_Out, true, v16_Out, true, IN, v17_Out);
                float v18_Out = (v0_Out - v17_Out);
                float v19_Out;
                #if defined(_FILL_OVERLAY_USE_GUIDES)
                    v19_Out = v18_Out;
                #else
                    v19_Out = 1.0;
                #endif
                float v20_Out = abs(v19_Out);
                OutVector1_1 = v20_Out;
            }

            void sg_Uv_Skew(float _Strength, bool _Strength_IsConnected, SGInput IN, out float2 Uv_1)
            {
                float v0_Out = _Strength;
                bool v0_Out_IsConnected = _Strength_IsConnected;
                float v1_Out = v0_Out;
                float4 v2_Out = IN.uv0;
                float2 v3_Out = (v2_Out).xy;
                float v4_R = (v3_Out)[0];
                float v5_G = (v3_Out)[1];
                float v6_B = 0.0;
                float v7_A = 0.0;
                float v8_Out = (v5_G + -0.5);
                float v9_Out = (v1_Out * v8_Out);
                float v10_Out = (v9_Out + v4_R);
                float2 v11_Out = float2(v10_Out, v5_G);
                Uv_1 = v11_Out;
            }

            void sg_Fill_Segments(float _Fill_Segments_Segments_Number, bool _Fill_Segments_Segments_Number_IsConnected, float _Fill_Segments_Width, bool _Fill_Segments_Width_IsConnected, float _Fill_Segments_Edge_Smoothness, bool _Fill_Segments_Edge_Smoothness_IsConnected, float2 _General_Shape_Size_Offset, bool _General_Shape_Size_Offset_IsConnected, float _Fill_Segments_Skew, bool _Fill_Segments_Skew_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Fill_Segments_Custom_Shape_Strength, bool _Fill_Segments_Custom_Shape_Strength_IsConnected, float _Fill_Segments_Add_Alpha_To_1, bool _Fill_Segments_Add_Alpha_To_1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _Fill_Segments_Multiply_Alpha_To_0, bool _Fill_Segments_Multiply_Alpha_To_0_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, float _Fill_Segments_Top_Offset, bool _Fill_Segments_Top_Offset_IsConnected, float _Fill_Segments_Top_Smoothness, bool _Fill_Segments_Top_Smoothness_IsConnected, float _Fill_Segments_Top_Strength, bool _Fill_Segments_Top_Strength_IsConnected, float _Fill_Segments_Use_Edge_Step, bool _Fill_Segments_Use_Edge_Step_IsConnected, float4 _Fill_Segments_Color, bool _Fill_Segments_Color_IsConnected, SGInput IN, out float OutVector1_1, out float OutVector11_2, out float OutVector12_3)
            {
                float v0_Out = _Fill_Segments_Add_Alpha_To_1;
                bool v0_Out_IsConnected = _Fill_Segments_Add_Alpha_To_1_IsConnected;
                float v1_Out = _Vector1_1;
                bool v1_Out_IsConnected = _Vector1_1_IsConnected;
                float v2_Out = v1_Out;
                float v3_Out = _Fill_Segments_Multiply_Alpha_To_0;
                bool v3_Out_IsConnected = _Fill_Segments_Multiply_Alpha_To_0_IsConnected;
                float v4_Out = _Fill_Segments_Use_Edge_Step;
                bool v4_Out_IsConnected = _Fill_Segments_Use_Edge_Step_IsConnected;
                float v5_Out = _Fill_Segments_Width;
                bool v5_Out_IsConnected = _Fill_Segments_Width_IsConnected;
                float v6_Out = _Fill_Segments_Skew;
                bool v6_Out_IsConnected = _Fill_Segments_Skew_IsConnected;
                float2 v7_Uv;
                sg_Uv_Skew(v6_Out, true, IN, v7_Uv);
                float v8_R = (v7_Uv)[0];
                float v9_G = (v7_Uv)[1];
                float v10_B = 0.0;
                float v11_A = 0.0;
                float v12_Out = (1.0 - v8_R);
                float v13_Out = _Vector1;
                bool v13_Out_IsConnected = _Vector1_IsConnected;
                float v14_Out = v13_Out;
                float v15_Out = _Fill_Segments_Custom_Shape_Strength;
                bool v15_Out_IsConnected = _Fill_Segments_Custom_Shape_Strength_IsConnected;
                float v16_Out = (v14_Out * v15_Out);
                float v17_Out = (v12_Out + v16_Out);
                float2 v18_Out = float2(v17_Out, 0.0);
                float v19_R = (v18_Out)[0];
                float v20_G = (v18_Out)[1];
                float v21_B = 0.0;
                float v22_A = 0.0;
                float2 v23_Out = _General_Shape_Size_Offset;
                bool v23_Out_IsConnected = _General_Shape_Size_Offset_IsConnected;
                float2 v24_Out = (v23_Out * float2(-1.0, -1.0));
                float v25_R = (v24_Out)[0];
                float v26_G = (v24_Out)[1];
                float v27_B = 0.0;
                float v28_A = 0.0;
                float v29_Out = (1.0 - v25_R);
                float2 v30_Out = float2(v25_R, v29_Out);
                float v31_Out = (v30_Out.x + (v19_R - float2(0.0, 1.0).x) * (v30_Out.y - v30_Out.x) / (float2(0.0, 1.0).y - float2(0.0, 1.0).x));
                float v32_Out = (1.0 - v31_Out);
                float v33_Out = _Fill_Segments_Segments_Number;
                bool v33_Out_IsConnected = _Fill_Segments_Segments_Number_IsConnected;
                float v34_Out = v33_Out;
                float v35_Out = (v31_Out * v34_Out);
                float v36_Out = frac(v35_Out);
                float v37_Out = (v32_Out * v34_Out);
                float v38_Out = frac(v37_Out);
                float v39_Out = (v36_Out * v38_Out);
                float v40_Out = _Fill_Segments_Top_Offset;
                bool v40_Out_IsConnected = _Fill_Segments_Top_Offset_IsConnected;
                float v41_Out = _Vector1_2;
                bool v41_Out_IsConnected = _Vector1_2_IsConnected;
                float v42_Out = v41_Out;
                float v43_Out = abs(v42_Out);
                float v44_Out = _Fill_Segments_Top_Smoothness;
                bool v44_Out_IsConnected = _Fill_Segments_Top_Smoothness_IsConnected;
                float v45_Out;
                sg_Smoothed_Step(v40_Out, true, v43_Out, true, v44_Out, true, IN, v45_Out);
                float v46_Out = (1.0 - v45_Out);
                float v47_Out = _Fill_Segments_Top_Strength;
                bool v47_Out_IsConnected = _Fill_Segments_Top_Strength_IsConnected;
                float v48_Out = lerp(0.0, v46_Out, v47_Out);
                float v49_Out = (v39_Out + v48_Out);
                float v50_Out;
                sg_Anti_Aliased_Step(v5_Out, true, v49_Out, true, IN, v50_Out);
                float v51_Out = _Fill_Segments_Edge_Smoothness;
                bool v51_Out_IsConnected = _Fill_Segments_Edge_Smoothness_IsConnected;
                float v52_Out;
                sg_Smoothed_Step(v5_Out, true, v49_Out, true, v51_Out, true, IN, v52_Out);
                float v53_Out = ((bool)v4_Out) ? v50_Out : v52_Out;
                float v54_o = v53_Out;
                float v55_Out = (1.0 - v54_o);
                float v56_Out = (v55_Out * v2_Out);
                float v57_Out = (v0_Out * v56_Out);
                float v58_Out;
                #if defined(_FILL_SEGMENTS_ENABLE)
                    v58_Out = v57_Out;
                #else
                    v58_Out = 0.0;
                #endif
                float v59_Out = lerp(1.0, v54_o, v3_Out);
                float v60_Out;
                #if defined(_FILL_SEGMENTS_ENABLE)
                    v60_Out = v59_Out;
                #else
                    v60_Out = 1.0;
                #endif
                float v61_o = v54_o;
                OutVector1_1 = v58_Out;
                OutVector11_2 = v60_Out;
                OutVector12_3 = v61_o;
            }

            void sg_Custom_Shape(UnityTexture2D _Custom_Shape_Noise_Guide_1, bool _Custom_Shape_Noise_Guide_1_IsConnected, float2 _Custom_Shape_Noise_Guide_1_Scroll_Speed, bool _Custom_Shape_Noise_Guide_1_Scroll_Speed_IsConnected, UnityTexture2D _Custom_Shape_Noise_Guide_2, bool _Custom_Shape_Noise_Guide_2_IsConnected, float _Custom_Shape_Noise_Guide_2_Strength, bool _Custom_Shape_Noise_Guide_2_Strength_IsConnected, float2 _Custom_Shape_Noise_Guide_2_Scroll_Speed, bool _Custom_Shape_Noise_Guide_2_Scroll_Speed_IsConnected, float _Custom_Shape_Noise_Guide_1_Strength, bool _Custom_Shape_Noise_Guide_1_Strength_IsConnected, float _Custom_Shape_Use_Second_Texture, bool _Custom_Shape_Use_Second_Texture_IsConnected, SGInput IN, out float OutVector1_1, out float OutVector11_2)
            {
                float2 v0_Out = _Custom_Shape_Noise_Guide_1_Scroll_Speed;
                bool v0_Out_IsConnected = _Custom_Shape_Noise_Guide_1_Scroll_Speed_IsConnected;
                UnityTexture2D v1_Out = _Custom_Shape_Noise_Guide_1;
                bool v1_Out_IsConnected = _Custom_Shape_Noise_Guide_1_IsConnected;
                float v2_OutVector1;
                sg_Sample_Scrolling_Texture(v0_Out, true, v1_Out, true, float2(0.0, 0.0), false, IN, v2_OutVector1);
                float v3_Out = _Custom_Shape_Noise_Guide_1_Strength;
                bool v3_Out_IsConnected = _Custom_Shape_Noise_Guide_1_Strength_IsConnected;
                float v4_Out = (v2_OutVector1 * v3_Out);
                float v5_Out = _Custom_Shape_Use_Second_Texture;
                bool v5_Out_IsConnected = _Custom_Shape_Use_Second_Texture_IsConnected;
                float2 v6_Out = _Custom_Shape_Noise_Guide_2_Scroll_Speed;
                bool v6_Out_IsConnected = _Custom_Shape_Noise_Guide_2_Scroll_Speed_IsConnected;
                UnityTexture2D v7_Out = _Custom_Shape_Noise_Guide_2;
                bool v7_Out_IsConnected = _Custom_Shape_Noise_Guide_2_IsConnected;
                float v8_OutVector1;
                sg_Sample_Scrolling_Texture(v6_Out, true, v7_Out, true, float2(0.0, 0.0), false, IN, v8_OutVector1);
                float v9_Out = _Custom_Shape_Noise_Guide_2_Strength;
                bool v9_Out_IsConnected = _Custom_Shape_Noise_Guide_2_Strength_IsConnected;
                float v10_Out = (v8_OutVector1 * v9_Out);
                float v11_Out = ((bool)v5_Out) ? v10_Out : 11.0;
                float v12_Out = (v4_Out * v11_Out);
                float v13_Out;
                #if defined(_CUSTOM_SHAPE_ENABLE)
                    v13_Out = v12_Out;
                #else
                    v13_Out = 0.0;
                #endif
                float v14_o = v13_Out;
                OutVector1_1 = v14_o;
                OutVector11_2 = v13_Out;
            }

            void sg_2D_Box_Properties(float _General_Shape_Height, bool _General_Shape_Height_IsConnected, float _General_Shape_Roundness, bool _General_Shape_Roundness_IsConnected, float _General_Shape_Roudness_Top_Right, bool _General_Shape_Roudness_Top_Right_IsConnected, float _General_Shape_Roudness_Bottom_Right, bool _General_Shape_Roudness_Bottom_Right_IsConnected, float _General_Shape_Roudness_Top_Left, bool _General_Shape_Roudness_Top_Left_IsConnected, float _General_Shape_Roudness_Bottom_Left, bool _General_Shape_Roudness_Bottom_Left_IsConnected, float2 _Vector2, bool _Vector2_IsConnected, float _General_Shape_Skew_Left_Invert, bool _General_Shape_Skew_Left_Invert_IsConnected, float _General_Shape_Skew_Left_Edge_Place, bool _General_Shape_Skew_Left_Edge_Place_IsConnected, float _General_Shape_Skew_Left_Bottom, bool _General_Shape_Skew_Left_Bottom_IsConnected, float _General_Shape_Skew_Left_Top, bool _General_Shape_Skew_Left_Top_IsConnected, float _General_Shape_Skew_Right_Invert, bool _General_Shape_Skew_Right_Invert_IsConnected, float _General_Shape_Skew_Right_Edge_Place, bool _General_Shape_Skew_Right_Edge_Place_IsConnected, float _General_Shape_Skew_Right_Bottom, bool _General_Shape_Skew_Right_Bottom_IsConnected, float _General_Shape_Skew_Right_Top, bool _General_Shape_Skew_Right_Top_IsConnected, float2 _General_Shape_Size_Offset, bool _General_Shape_Size_Offset_IsConnected, float _Fill_Shadow_Power_Hardness, bool _Fill_Shadow_Power_Hardness_IsConnected, float _Fill_Shadow_Multiplier_Shadow_Amount, bool _Fill_Shadow_Multiplier_Shadow_Amount_IsConnected, float _Vector1, bool _Vector1_IsConnected, SGInput IN, out float4 OutVector4_1, out float2 OutVector2_2, out float2 OutVector21_3, out float2 OutVector22_4, out float OutVector1_5, out float OutVector11_6, out float OutVector12_7)
            {
                float v0_Out = _General_Shape_Roudness_Top_Right;
                bool v0_Out_IsConnected = _General_Shape_Roudness_Top_Right_IsConnected;
                float v1_Out = _General_Shape_Roudness_Bottom_Right;
                bool v1_Out_IsConnected = _General_Shape_Roudness_Bottom_Right_IsConnected;
                float v2_Out = _General_Shape_Roudness_Top_Left;
                bool v2_Out_IsConnected = _General_Shape_Roudness_Top_Left_IsConnected;
                float v3_Out = _General_Shape_Roudness_Bottom_Left;
                bool v3_Out_IsConnected = _General_Shape_Roudness_Bottom_Left_IsConnected;
                float4 v4_Out = float4(v0_Out, v1_Out, v2_Out, v3_Out);
                float v5_Out = _General_Shape_Skew_Left_Invert;
                bool v5_Out_IsConnected = _General_Shape_Skew_Left_Invert_IsConnected;
                float v6_Out = ((bool)v5_Out) ? -1.0 : 0.0;
                float v7_Out = _General_Shape_Height;
                bool v7_Out_IsConnected = _General_Shape_Height_IsConnected;
                float v8_Out = (v6_Out * v7_Out);
                float v9_Out = _General_Shape_Skew_Right_Invert;
                bool v9_Out_IsConnected = _General_Shape_Skew_Right_Invert_IsConnected;
                float v10_Out = ((bool)v9_Out) ? 0.0 : 1.0;
                float v11_Out = _General_Shape_Height;
                bool v11_Out_IsConnected = _General_Shape_Height_IsConnected;
                float v12_Out = (v10_Out * v11_Out);
                float2 v13_Out = _Vector2;
                bool v13_Out_IsConnected = _Vector2_IsConnected;
                float2 v14_o = v13_Out;
                float v15_Out = _General_Shape_Height;
                bool v15_Out_IsConnected = _General_Shape_Height_IsConnected;
                float2 v16_Out = float2(0.0, v15_Out);
                float v17_Out = _General_Shape_Skew_Left_Invert;
                bool v17_Out_IsConnected = _General_Shape_Skew_Left_Invert_IsConnected;
                float v18_Out = ((bool)v17_Out) ? 1.0 : 0.0;
                float v19_Out = _General_Shape_Skew_Left_Edge_Place;
                bool v19_Out_IsConnected = _General_Shape_Skew_Left_Edge_Place_IsConnected;
                float v20_Out = _General_Shape_Skew_Left_Top;
                bool v20_Out_IsConnected = _General_Shape_Skew_Left_Top_IsConnected;
                float v21_Out = (v19_Out * v20_Out);
                float v22_Out = _General_Shape_Skew_Left_Bottom;
                bool v22_Out_IsConnected = _General_Shape_Skew_Left_Bottom_IsConnected;
                float v23_Out = _General_Shape_Skew_Left_Edge_Place;
                bool v23_Out_IsConnected = _General_Shape_Skew_Left_Edge_Place_IsConnected;
                float v24_Out = (1.0 - v23_Out);
                float v25_Out = (v22_Out * v24_Out);
                float v26_Out = max(v21_Out, v25_Out);
                float v27_Out = (v26_Out * v8_Out);
                float v28_Out = (v18_Out * v26_Out);
                float v29_Out = _General_Shape_Skew_Right_Invert;
                bool v29_Out_IsConnected = _General_Shape_Skew_Right_Invert_IsConnected;
                float v30_Out = ((bool)v29_Out) ? 0.0 : 1.0;
                float v31_Out = _General_Shape_Skew_Right_Edge_Place;
                bool v31_Out_IsConnected = _General_Shape_Skew_Right_Edge_Place_IsConnected;
                float v32_Out = _General_Shape_Skew_Right_Top;
                bool v32_Out_IsConnected = _General_Shape_Skew_Right_Top_IsConnected;
                float v33_Out = (v31_Out * v32_Out);
                float v34_Out = _General_Shape_Skew_Right_Bottom;
                bool v34_Out_IsConnected = _General_Shape_Skew_Right_Bottom_IsConnected;
                float v35_Out = _General_Shape_Skew_Right_Edge_Place;
                float v36_Out = (1.0 - v35_Out);
                float v37_Out = (v34_Out * v36_Out);
                float v38_Out = max(v33_Out, v37_Out);
                float v39_Out = (v38_Out * v12_Out);
                float v40_Out = (v27_Out + v39_Out);
                float2 v41_Out = float2(v40_Out, 0.0);
                float v42_Out = (v30_Out * v38_Out);
                float v43_Out = (v28_Out + v42_Out);
                float v44_Out = _General_Shape_Height;
                bool v44_Out_IsConnected = _General_Shape_Height_IsConnected;
                float v45_Out = (v44_Out * -1.0);
                float v46_Out = (v43_Out * v45_Out);
                float2 v47_Out = float2(v46_Out, 0.0);
                float2 v48_Out = (v16_Out + v47_Out);
                float v49_Out = _General_Shape_Roundness;
                bool v49_Out_IsConnected = _General_Shape_Roundness_IsConnected;
                float2 v50_Out = (v48_Out - (v49_Out.xx));
                float2 v51_Out = _General_Shape_Size_Offset;
                bool v51_Out_IsConnected = _General_Shape_Size_Offset_IsConnected;
                float2 v52_Out = (v50_Out - v51_Out);
                float v53_Out = _Fill_Shadow_Power_Hardness;
                bool v53_Out_IsConnected = _Fill_Shadow_Power_Hardness_IsConnected;
                float v54_Out = pow(abs(v53_Out), 2.0);
                float v55_Out = _Fill_Shadow_Multiplier_Shadow_Amount;
                bool v55_Out_IsConnected = _Fill_Shadow_Multiplier_Shadow_Amount_IsConnected;
                float v56_Out = pow(abs(v55_Out), 3.0);
                float v57_Out = _Vector1;
                bool v57_Out_IsConnected = _Vector1_IsConnected;
                OutVector4_1 = v4_Out;
                OutVector2_2 = v41_Out;
                OutVector21_3 = v14_o;
                OutVector22_4 = v52_Out;
                OutVector1_5 = v54_Out;
                OutVector11_6 = v56_Out;
                OutVector12_7 = v57_Out;
            }

            void sg_Shadow(float _Shadow_Strength, bool _Shadow_Strength_IsConnected, float _Shadow_Size, bool _Shadow_Size_IsConnected, float _Shadow_Smoothness, bool _Shadow_Smoothness_IsConnected, float _Shadow_Power, bool _Shadow_Power_IsConnected, float4 _Shadow_Color, bool _Shadow_Color_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Shadow_Enable, bool _Shadow_Enable_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, SGInput IN, out float4 OutVector4_1, out float OutVector1_2)
            {
                float4 v0_Out = _Vector4;
                bool v0_Out_IsConnected = _Vector4_IsConnected;
                float4 v1_Out = v0_Out;
                float4 v2_Out = _Shadow_Color;
                bool v2_Out_IsConnected = _Shadow_Color_IsConnected;
                float v3_Out = _Vector1_1;
                bool v3_Out_IsConnected = _Vector1_1_IsConnected;
                float v4_Out = v3_Out;
                float v5_Out = (1.0 - v4_Out);
                float v6_o = v5_Out;
                float4 v7_Out = _Shadow_Color;
                bool v7_Out_IsConnected = _Shadow_Color_IsConnected;
                float v8_R = (v7_Out)[0];
                float v9_G = (v7_Out)[1];
                float v10_B = (v7_Out)[2];
                float v11_A = (v7_Out)[3];
                float v12_Out = _Shadow_Enable;
                bool v12_Out_IsConnected = _Shadow_Enable_IsConnected;
                float v13_Out = _Shadow_Size;
                bool v13_Out_IsConnected = _Shadow_Size_IsConnected;
                float v14_Out = _Shadow_Size;
                bool v14_Out_IsConnected = _Shadow_Size_IsConnected;
                float v15_Out = (1.0 - v14_Out);
                float v16_Out = _Shadow_Smoothness;
                bool v16_Out_IsConnected = _Shadow_Smoothness_IsConnected;
                float v17_Out = (v15_Out + v16_Out);
                float v18_Out = (v17_Out * -1.0);
                float v19_Out = _Shadow_Strength;
                bool v19_Out_IsConnected = _Shadow_Strength_IsConnected;
                float v20_Out = lerp(v18_Out, 0.0, v19_Out);
                float v21_Out = _Vector1;
                bool v21_Out_IsConnected = _Vector1_IsConnected;
                float v22_o = v21_Out;
                float v23_Out = abs(v22_o);
                float v24_Out = (1.0 - v23_Out);
                float v25_Out = (v20_Out + v24_Out);
                float v26_Out = _Shadow_Smoothness;
                bool v26_Out_IsConnected = _Shadow_Smoothness_IsConnected;
                float v27_Out;
                sg_Smoothed_Step(v13_Out, true, v25_Out, true, v26_Out, true, IN, v27_Out);
                float v28_Out = _Shadow_Power;
                bool v28_Out_IsConnected = _Shadow_Power_IsConnected;
                float v29_Out = pow(abs(v27_Out), v28_Out);
                float v30_Out = ((bool)v12_Out) ? v29_Out : 0.0;
                float v31_Out = (v30_Out * v6_o);
                float4 v32_Out = lerp(v1_Out, v2_Out, (v31_Out.xxxx));
                float v33_Out = lerp(0.0, v11_A, v30_Out);
                OutVector4_1 = v32_Out;
                OutVector1_2 = v33_Out;
            }

            void sg_Custom_Outline(float _Custom_Outline_Noise_Guide_1_Strength, bool _Custom_Outline_Noise_Guide_1_Strength_IsConnected, UnityTexture2D _Custom_Outline_Noise_Guide_1, bool _Custom_Outline_Noise_Guide_1_IsConnected, float2 _Custom_Outline_Noise_Guide_2_Scroll_Speed, bool _Custom_Outline_Noise_Guide_2_Scroll_Speed_IsConnected, UnityTexture2D _Custom_Outline_Noise_Guide_2, bool _Custom_Outline_Noise_Guide_2_IsConnected, float _Custom_Outline_Noise_Guide_2_Strength, bool _Custom_Outline_Noise_Guide_2_Strength_IsConnected, float _Custom_Outline_Size, bool _Custom_Outline_Size_IsConnected, float _Custom_Outline_Smoothness, bool _Custom_Outline_Smoothness_IsConnected, float4 _Custom_Outline_Color, bool _Custom_Outline_Color_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Custom_Outline_Depends_On_Fill_Amount, bool _Custom_Outline_Depends_On_Fill_Amount_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Custom_Outline_Use_Noise_Guides, bool _Custom_Outline_Use_Noise_Guides_IsConnected, float _Custom_Outline_Power, bool _Custom_Outline_Power_IsConnected, float _Custom_Outline_Strength, bool _Custom_Outline_Strength_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _Custom_Outline_Offset, bool _Custom_Outline_Offset_IsConnected, float2 _Custom_Outline_Noise_Guide_1_Scroll_Speed, bool _Custom_Outline_Noise_Guide_1_Scroll_Speed_IsConnected, float4 _Vector4_1, bool _Vector4_1_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, SGInput IN, out float4 OutVector4_1, out float OutVector1_2, out float4 OutVector41_3, out float OutVector11_4)
            {
                float4 v0_Out = _Vector4;
                bool v0_Out_IsConnected = _Vector4_IsConnected;
                float4 v1_Out = v0_Out;
                float4 v2_Out = _Custom_Outline_Color;
                bool v2_Out_IsConnected = _Custom_Outline_Color_IsConnected;
                float4 v3_Out = _Custom_Outline_Color;
                bool v3_Out_IsConnected = _Custom_Outline_Color_IsConnected;
                float v4_R = (v3_Out)[0];
                float v5_G = (v3_Out)[1];
                float v6_B = (v3_Out)[2];
                float v7_A = (v3_Out)[3];
                float v8_Out = _Custom_Outline_Size;
                bool v8_Out_IsConnected = _Custom_Outline_Size_IsConnected;
                float v9_Out = _Custom_Outline_Size;
                bool v9_Out_IsConnected = _Custom_Outline_Size_IsConnected;
                float v10_Out = (1.0 - v9_Out);
                float v11_Out = _Custom_Outline_Smoothness;
                bool v11_Out_IsConnected = _Custom_Outline_Smoothness_IsConnected;
                float v12_Out = (v10_Out + v11_Out);
                float v13_Out = (v12_Out * -1.0);
                float v14_Out = _Custom_Outline_Strength;
                bool v14_Out_IsConnected = _Custom_Outline_Strength_IsConnected;
                float v15_Out = lerp(v13_Out, 0.0, v14_Out);
                float v16_Out = _Custom_Outline_Use_Noise_Guides;
                bool v16_Out_IsConnected = _Custom_Outline_Use_Noise_Guides_IsConnected;
                float2 v17_Out = _Custom_Outline_Noise_Guide_1_Scroll_Speed;
                bool v17_Out_IsConnected = _Custom_Outline_Noise_Guide_1_Scroll_Speed_IsConnected;
                UnityTexture2D v18_Out = _Custom_Outline_Noise_Guide_1;
                bool v18_Out_IsConnected = _Custom_Outline_Noise_Guide_1_IsConnected;
                float v19_OutVector1;
                sg_Sample_Scrolling_Texture(v17_Out, true, v18_Out, true, float2(0.0, 0.0), false, IN, v19_OutVector1);
                float v20_Out = _Custom_Outline_Noise_Guide_1_Strength;
                bool v20_Out_IsConnected = _Custom_Outline_Noise_Guide_1_Strength_IsConnected;
                float v21_Out = (v19_OutVector1 * v20_Out);
                float2 v22_Out = _Custom_Outline_Noise_Guide_2_Scroll_Speed;
                bool v22_Out_IsConnected = _Custom_Outline_Noise_Guide_2_Scroll_Speed_IsConnected;
                UnityTexture2D v23_Out = _Custom_Outline_Noise_Guide_2;
                bool v23_Out_IsConnected = _Custom_Outline_Noise_Guide_2_IsConnected;
                float v24_OutVector1;
                sg_Sample_Scrolling_Texture(v22_Out, true, v23_Out, true, float2(0.0, 0.0), false, IN, v24_OutVector1);
                float v25_Out = _Custom_Outline_Noise_Guide_2_Strength;
                bool v25_Out_IsConnected = _Custom_Outline_Noise_Guide_2_Strength_IsConnected;
                float v26_Out = (v24_OutVector1 * v25_Out);
                float v27_Out = (v21_Out * v26_Out);
                float v28_Out = ((bool)v16_Out) ? v27_Out : 0.0;
                float v29_Out = (v15_Out + v28_Out);
                float v30_Out = _Vector1_1;
                bool v30_Out_IsConnected = _Vector1_1_IsConnected;
                float v31_o = v30_Out;
                float v32_Out = _Custom_Outline_Offset;
                bool v32_Out_IsConnected = _Custom_Outline_Offset_IsConnected;
                float v33_Out = (v31_o - v32_Out);
                float v34_Out = abs(v33_Out);
                float v35_Out = (1.0 - v34_Out);
                float v36_Out = (v29_Out + v35_Out);
                float v37_Out = _Custom_Outline_Smoothness;
                float v38_Out;
                sg_Smoothed_Step(v8_Out, true, v36_Out, true, v37_Out, true, IN, v38_Out);
                float v39_Out = _Custom_Outline_Depends_On_Fill_Amount;
                bool v39_Out_IsConnected = _Custom_Outline_Depends_On_Fill_Amount_IsConnected;
                float v40_Out = _Vector1;
                bool v40_Out_IsConnected = _Vector1_IsConnected;
                float v41_Out = v40_Out;
                float v42_Out = ((bool)v39_Out) ? v41_Out : 1.0;
                float v43_Out = (v38_Out * v42_Out);
                float v44_Out = _Custom_Outline_Power;
                bool v44_Out_IsConnected = _Custom_Outline_Power_IsConnected;
                float v45_Out = pow(abs(v43_Out), v44_Out);
                float v46_Out;
                #if defined(_CUSTOM_OUTLINE_ENABLE)
                    v46_Out = v45_Out;
                #else
                    v46_Out = 0.0;
                #endif
                float4 v47_Out = lerp(v1_Out, v2_Out, (v46_Out.xxxx));
                float v48_Out = lerp(0.0, v7_A, v46_Out);
                float4 v49_Out = _Vector4_1;
                bool v49_Out_IsConnected = _Vector4_1_IsConnected;
                float v50_Out = _Vector1_2;
                bool v50_Out_IsConnected = _Vector1_2_IsConnected;
                OutVector4_1 = v47_Out;
                OutVector1_2 = v48_Out;
                OutVector41_3 = v49_Out;
                OutVector11_4 = v50_Out;
            }

            void sg_End_Line(float _End_Line_Smoothness_Left, bool _End_Line_Smoothness_Left_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _End_Line_Smoothness_Right, bool _End_Line_Smoothness_Right_IsConnected, float _End_Line_Power, bool _End_Line_Power_IsConnected, float _End_Line_Multiply_With_Fill_Shadow, bool _End_Line_Multiply_With_Fill_Shadow_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _End_Line_Width_Right, bool _End_Line_Width_Right_IsConnected, float _End_Line_Multiplier, bool _End_Line_Multiplier_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, float _End_Line_Width_Left, bool _End_Line_Width_Left_IsConnected, float _Vector1_3, bool _Vector1_3_IsConnected, float _End_Line_Offset, bool _End_Line_Offset_IsConnected, float _Vector1_4, bool _Vector1_4_IsConnected, SGInput IN, out float OutVector1_1, out float OutVector11_2)
            {
                float v0_Out = _Vector1_2;
                bool v0_Out_IsConnected = _Vector1_2_IsConnected;
                float v1_o = v0_Out;
                float v2_Out = _End_Line_Multiply_With_Fill_Shadow;
                bool v2_Out_IsConnected = _End_Line_Multiply_With_Fill_Shadow_IsConnected;
                float v3_Out = _Vector1_1;
                bool v3_Out_IsConnected = _Vector1_1_IsConnected;
                float v4_Out = ((bool)v2_Out) ? v3_Out : 1.0;
                float v5_Out = _End_Line_Width_Right;
                bool v5_Out_IsConnected = _End_Line_Width_Right_IsConnected;
                float v6_Out = _End_Line_Smoothness_Right;
                bool v6_Out_IsConnected = _End_Line_Smoothness_Right_IsConnected;
                float v7_Out = _Vector1;
                bool v7_Out_IsConnected = _Vector1_IsConnected;
                float v8_Out = v7_Out;
                float v9_Out = _Vector1_3;
                bool v9_Out_IsConnected = _Vector1_3_IsConnected;
                float v10_o = v9_Out;
                float v11_Out = _End_Line_Offset;
                bool v11_Out_IsConnected = _End_Line_Offset_IsConnected;
                float v12_Out = (v10_o + v11_Out);
                float v13_Out = v12_Out;
                float v14_Out = (v13_Out + v5_Out);
                float v15_Out;
                sg_Smoothed_Step(v8_Out, true, v14_Out, true, v6_Out, true, IN, v15_Out);
                float v16_Out = _End_Line_Width_Left;
                bool v16_Out_IsConnected = _End_Line_Width_Left_IsConnected;
                float v17_Out = (v16_Out * -1.0);
                float v18_Out = (v13_Out + v17_Out);
                float v19_Out = _End_Line_Smoothness_Left;
                bool v19_Out_IsConnected = _End_Line_Smoothness_Left_IsConnected;
                float v20_Out;
                sg_Smoothed_Step(v8_Out, true, v18_Out, true, v19_Out, true, IN, v20_Out);
                float v21_Out = (v15_Out - v20_Out);
                float v22_Out = saturate(v21_Out);
                float v23_Out = _End_Line_Multiplier;
                bool v23_Out_IsConnected = _End_Line_Multiplier_IsConnected;
                float v24_Out = (v22_Out * v23_Out);
                float v25_Out = _End_Line_Power;
                bool v25_Out_IsConnected = _End_Line_Power_IsConnected;
                float v26_Out = pow(abs(v24_Out), v25_Out);
                float v27_Out = (v4_Out * v26_Out);
                float v28_Out = saturate(v27_Out);
                float v29_Out = (v1_o * v28_Out);
                float v30_Out;
                #if defined(_END_LINE_ENABLE)
                    v30_Out = v29_Out;
                #else
                    v30_Out = 0.0;
                #endif
                float v31_Out = _Vector1_4;
                bool v31_Out_IsConnected = _Vector1_4_IsConnected;
                OutVector1_1 = v30_Out;
                OutVector11_2 = v31_Out;
            }

            void sg_Simple_Outline(float4 _Simple_Outline_Color, bool _Simple_Outline_Color_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Simple_Outline_Thickness, bool _Simple_Outline_Thickness_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Simple_Outline_Use_Color_Alpha_For_Color_Blending, bool _Simple_Outline_Use_Color_Alpha_For_Color_Blending_IsConnected, SGInput IN, out float4 OutVector4_1, out float OutVector1_2)
            {
                float4 v0_Out = _Vector4;
                bool v0_Out_IsConnected = _Vector4_IsConnected;
                float4 v1_o = v0_Out;
                float4 v2_Out = _Simple_Outline_Color;
                bool v2_Out_IsConnected = _Simple_Outline_Color_IsConnected;
                float v3_Out = _Simple_Outline_Use_Color_Alpha_For_Color_Blending;
                bool v3_Out_IsConnected = _Simple_Outline_Use_Color_Alpha_For_Color_Blending_IsConnected;
                float v4_Out = _Simple_Outline_Thickness;
                bool v4_Out_IsConnected = _Simple_Outline_Thickness_IsConnected;
                float v5_Out = (v4_Out == 0.0) ? 1.0 : 0.0;
                float v6_Out = v5_Out ? 0.0 : 0.10000000149011612;
                float4 v7_Out = _Simple_Outline_Color;
                bool v7_Out_IsConnected = _Simple_Outline_Color_IsConnected;
                float v8_R = (v7_Out)[0];
                float v9_G = (v7_Out)[1];
                float v10_B = (v7_Out)[2];
                float v11_A = (v7_Out)[3];
                float v12_Out = ((bool)v3_Out) ? v11_A : 1.0;
                float v13_Out = _Simple_Outline_Thickness;
                bool v13_Out_IsConnected = _Simple_Outline_Thickness_IsConnected;
                float v14_Out = (-1.0 * v13_Out);
                float v15_Out = _Vector1;
                bool v15_Out_IsConnected = _Vector1_IsConnected;
                float v16_o = v15_Out;
                float v17_Out;
                sg_Anti_Aliased_Step(v16_o, true, v6_Out, true, IN, v17_Out);
                float v18_Out;
                sg_Anti_Aliased_Step(v16_o, true, v14_Out, true, IN, v18_Out);
                float v19_Out = (v17_Out - v18_Out);
                float v20_Out = (v12_Out * v19_Out);
                float4 v21_Out = lerp(v1_o, v2_Out, (v20_Out.xxxx));
                float4 v22_o = v21_Out;
                float v23_Out;
                sg_Anti_Aliased_Step(v16_o, true, 0.0, false, IN, v23_Out);
                float v24_Out;
                sg_Anti_Aliased_Step(v16_o, true, v14_Out, true, IN, v24_Out);
                float v25_Out = (v23_Out - v24_Out);
                float v26_Out;
                sg_Anti_Aliased_Step(v14_Out, true, v16_o, true, IN, v26_Out);
                float v27_Out;
                sg_Anti_Aliased_Step(0.0, false, v16_o, true, IN, v27_Out);
                float v28_Out = (v26_Out - v27_Out);
                float v29_Out = (v25_Out * v28_Out);
                float v30_o = v29_Out;
                float v31_Out = (v11_A * v30_o);
                OutVector4_1 = v22_o;
                OutVector1_2 = v31_Out;
            }

            void sg_Fill_Vertical_Darken(float _Fill_Vertical_Darken_Power, bool _Fill_Vertical_Darken_Power_IsConnected, float _Fill_Vertical_Darken_Use_Flat, bool _Fill_Vertical_Darken_Use_Flat_IsConnected, float _Fill_Vertical_Darken_Flip, bool _Fill_Vertical_Darken_Flip_IsConnected, float _Fill_Vertical_Darken_Amount, bool _Fill_Vertical_Darken_Amount_IsConnected, float _Fill_Vertical_Darken_Enable, bool _Fill_Vertical_Darken_Enable_IsConnected, float _Vector1, bool _Vector1_IsConnected, SGInput IN, out float OutVector1_1, out float OutVector11_2)
            {
                float v0_Out = _Fill_Vertical_Darken_Enable;
                bool v0_Out_IsConnected = _Fill_Vertical_Darken_Enable_IsConnected;
                float v1_Out = _Fill_Vertical_Darken_Use_Flat;
                bool v1_Out_IsConnected = _Fill_Vertical_Darken_Use_Flat_IsConnected;
                float v2_Out = _Fill_Vertical_Darken_Power;
                bool v2_Out_IsConnected = _Fill_Vertical_Darken_Power_IsConnected;
                float v3_Out = _Fill_Vertical_Darken_Flip;
                bool v3_Out_IsConnected = _Fill_Vertical_Darken_Flip_IsConnected;
                float4 v4_Out = IN.uv0;
                float v5_Out = (v4_Out).y;
                float v6_Out = (1.0 - v5_Out);
                float v7_Out = ((bool)v3_Out) ? v6_Out : v5_Out;
                float v8_o = v7_Out;
                float v9_Out;
                sg_Anti_Aliased_Step(v8_o, true, 0.5, false, IN, v9_Out);
                float v10_Out = (v8_o * 2.0);
                float v11_Out = pow(abs(v10_Out), v2_Out);
                float v12_Out;
                sg_Anti_Aliased_Step(v8_o, true, 0.5, false, IN, v12_Out);
                float v13_Out = (v11_Out * v12_Out);
                float v14_Out = ((bool)v1_Out) ? v9_Out : v13_Out;
                float v15_Out = _Fill_Vertical_Darken_Amount;
                bool v15_Out_IsConnected = _Fill_Vertical_Darken_Amount_IsConnected;
                float v16_Out = lerp(0.0, v14_Out, v15_Out);
                float v17_Out = ((bool)v0_Out) ? v16_Out : 0.0;
                float v18_Out = _Vector1;
                bool v18_Out_IsConnected = _Vector1_IsConnected;
                OutVector1_1 = v17_Out;
                OutVector11_2 = v18_Out;
            }

            void sg_Sample_Texture_Ui(UnityTexture2D _Texture, bool _Texture_IsConnected, float _Flip_Uvs, bool _Flip_Uvs_IsConnected, float _Flow_Speed, bool _Flow_Speed_IsConnected, float2 _Flow_Speed_XY, bool _Flow_Speed_XY_IsConnected, float2 _Uv, bool _Uv_IsConnected, SGInput IN, out float Texture_R_1)
            {
                float2 v0_Out = _Flow_Speed_XY;
                bool v0_Out_IsConnected = _Flow_Speed_XY_IsConnected;
                UnityTexture2D v1_Out = _Texture;
                bool v1_Out_IsConnected = _Texture_IsConnected;
                float v2_Out = _Flip_Uvs;
                bool v2_Out_IsConnected = _Flip_Uvs_IsConnected;
                float2 v3_Out = _Uv;
                bool v3_Out_IsConnected = _Uv_IsConnected;
                float2 v4_Out = _Uv;
                bool v4_Out_IsConnected = _Uv_IsConnected;
                float4 v5_Out = IN.uv0;
                float2 v6_Out = v3_Out_IsConnected ? v4_Out : (v5_Out.xy);
                float2 v7_Out = (v6_Out).yx;
                float2 v8_Out = (v6_Out).xy;
                float2 v9_Out = ((bool)v2_Out) ? v7_Out : v8_Out;
                float v10_OutVector1;
                sg_Sample_Scrolling_Texture(v0_Out, true, v1_Out, true, v9_Out, true, IN, v10_OutVector1);
                Texture_R_1 = v10_OutVector1;
            }

            void sg_Fill_Skew_and_Edge_Guide(float _Fill_Skew, bool _Fill_Skew_IsConnected, float _Fill_Guide_Strength, bool _Fill_Guide_Strength_IsConnected, float _Fill_Enable_Edge_Guide, bool _Fill_Enable_Edge_Guide_IsConnected, UnityTexture2D _Fill_Texture_Guide, bool _Fill_Texture_Guide_IsConnected, float _Fill_Flip_Guide_Texture_90_degrees_rotation, bool _Fill_Flip_Guide_Texture_90_degrees_rotation_IsConnected, float2 _Fill_Scroll_speed_XY, bool _Fill_Scroll_speed_XY_IsConnected, float _Fill_Flip_Guide_Texture_Value, bool _Fill_Flip_Guide_Texture_Value_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, SGInput IN, out float OutVector1_1, out float OutVector11_2, out float OutVector12_3, out float OutVector13_4, out float OutVector14_5)
            {
                float v0_Out = _Fill_Enable_Edge_Guide;
                bool v0_Out_IsConnected = _Fill_Enable_Edge_Guide_IsConnected;
                float v1_Out = _Fill_Flip_Guide_Texture_Value;
                bool v1_Out_IsConnected = _Fill_Flip_Guide_Texture_Value_IsConnected;
                UnityTexture2D v2_Out = _Fill_Texture_Guide;
                bool v2_Out_IsConnected = _Fill_Texture_Guide_IsConnected;
                float v3_Out = _Fill_Flip_Guide_Texture_90_degrees_rotation;
                bool v3_Out_IsConnected = _Fill_Flip_Guide_Texture_90_degrees_rotation_IsConnected;
                float2 v4_Out = _Fill_Scroll_speed_XY;
                bool v4_Out_IsConnected = _Fill_Scroll_speed_XY_IsConnected;
                float v5_Texture_R;
                sg_Sample_Texture_Ui(v2_Out, true, v3_Out, true, 0.0, false, v4_Out, true, float2(0.0, 0.0), false, IN, v5_Texture_R);
                float v6_Out = (1.0 - v5_Texture_R);
                float v7_Out = ((bool)v1_Out) ? v6_Out : v5_Texture_R;
                float v8_Out = _Fill_Guide_Strength;
                bool v8_Out_IsConnected = _Fill_Guide_Strength_IsConnected;
                float v9_Out = (v7_Out * v8_Out);
                float v10_Out = ((bool)v0_Out) ? v9_Out : 0.0;
                float v11_Out = _Fill_Skew;
                bool v11_Out_IsConnected = _Fill_Skew_IsConnected;
                float2 v12_Uv;
                sg_Uv_Skew(v11_Out, true, IN, v12_Uv);
                float v13_R = (v12_Uv)[0];
                float v14_G = (v12_Uv)[1];
                float v15_B = 0.0;
                float v16_A = 0.0;
                float v17_Out = (1.0 - v13_R);
                float2 v18_Out = float2(v17_Out, 0.0);
                float v19_R = (v18_Out)[0];
                float v20_G = (v18_Out)[1];
                float v21_B = 0.0;
                float v22_A = 0.0;
                float v23_o = v19_R;
                float v24_Out = (v23_o + v10_Out);
                float v25_Out = _Vector1;
                bool v25_Out_IsConnected = _Vector1_IsConnected;
                float v26_Out = _Vector1_1;
                bool v26_Out_IsConnected = _Vector1_1_IsConnected;
                float v27_Out = _Vector1_2;
                bool v27_Out_IsConnected = _Vector1_2_IsConnected;
                OutVector1_1 = v24_Out;
                OutVector11_2 = v23_o;
                OutVector12_3 = v25_Out;
                OutVector13_4 = v26_Out;
                OutVector14_5 = v27_Out;
            }

            void sg_Fill_Second_Bar(float2 _Vector2, bool _Vector2_IsConnected, float _Fill_Smooth_Edge_Value, bool _Fill_Smooth_Edge_Value_IsConnected, float _Fill_Enable_Smooth_Edge, bool _Fill_Enable_Smooth_Edge_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Main_Bar_Fill_Amount, bool _Main_Bar_Fill_Amount_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Second_Bar_Blend_To_Main_Bar, bool _Second_Bar_Blend_To_Main_Bar_IsConnected, float4 _Fill_Second_Bar_Color, bool _Fill_Second_Bar_Color_IsConnected, float _Second_Bar_Blend_To_Invisible, bool _Second_Bar_Blend_To_Invisible_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, float _Vector1_3, bool _Vector1_3_IsConnected, float _Vector1_4, bool _Vector1_4_IsConnected, float _Vector1_5, bool _Vector1_5_IsConnected, SGInput IN, out float OutVector1_1, out float OutVector11_2, out float OutVector12_3, out float4 OutVector4_4, out float OutVector13_5, out float OutVector14_6, out float OutVector15_7, out float OutVector16_8, out float OutVector17_9)
            {
                float v0_Out = _Fill_Enable_Smooth_Edge;
                bool v0_Out_IsConnected = _Fill_Enable_Smooth_Edge_IsConnected;
                float v1_Out = _Fill_Smooth_Edge_Value;
                bool v1_Out_IsConnected = _Fill_Smooth_Edge_Value_IsConnected;
                float v2_Out = _Main_Bar_Fill_Amount;
                bool v2_Out_IsConnected = _Main_Bar_Fill_Amount_IsConnected;
                float v3_Out = _Fill_Enable_Smooth_Edge;
                bool v3_Out_IsConnected = _Fill_Enable_Smooth_Edge_IsConnected;
                float v4_Out = _Fill_Smooth_Edge_Value;
                bool v4_Out_IsConnected = _Fill_Smooth_Edge_Value_IsConnected;
                float v5_Out = ((bool)v3_Out) ? v4_Out : 0.0;
                float v6_Out = (v2_Out + v5_Out);
                float2 v7_Out = _Vector2;
                bool v7_Out_IsConnected = _Vector2_IsConnected;
                float v8_Out = (v7_Out.x + (v6_Out - float2(1.0, 0.0).x) * (v7_Out.y - v7_Out.x) / (float2(1.0, 0.0).y - float2(1.0, 0.0).x));
                float v9_o = v8_Out;
                float v10_Out = _Vector1;
                bool v10_Out_IsConnected = _Vector1_IsConnected;
                float v11_o = v10_Out;
                float v12_Out;
                sg_Smoothed_Step(v9_o, true, v11_o, true, v1_Out, true, IN, v12_Out);
                float v13_Out;
                sg_Anti_Aliased_Step(v9_o, true, v11_o, true, IN, v13_Out);
                float v14_Out = ((bool)v0_Out) ? v12_Out : v13_Out;
                float v15_Out = _Second_Bar_Blend_To_Main_Bar;
                bool v15_Out_IsConnected = _Second_Bar_Blend_To_Main_Bar_IsConnected;
                float v16_Out = (1.0 - v15_Out);
                float v17_Out = lerp(1.0, v14_Out, v16_Out);
                float v18_Out = (1.0 - v17_Out);
                float4 v19_Out = _Fill_Second_Bar_Color;
                bool v19_Out_IsConnected = _Fill_Second_Bar_Color_IsConnected;
                float v20_R = (v19_Out)[0];
                float v21_G = (v19_Out)[1];
                float v22_B = (v19_Out)[2];
                float v23_A = (v19_Out)[3];
                float v24_Out = _Second_Bar_Blend_To_Invisible;
                bool v24_Out_IsConnected = _Second_Bar_Blend_To_Invisible_IsConnected;
                float v25_Out = (1.0 - v24_Out);
                float v26_Out = (v23_A * v25_Out);
                float4 v27_Out = float4(v20_R, v21_G, v22_B, v26_Out);
                float v28_Out = _Vector1_2;
                bool v28_Out_IsConnected = _Vector1_2_IsConnected;
                float v29_Out = _Vector1_3;
                bool v29_Out_IsConnected = _Vector1_3_IsConnected;
                float v30_Out = _Vector1_4;
                bool v30_Out_IsConnected = _Vector1_4_IsConnected;
                float v31_Out = _Vector1_1;
                bool v31_Out_IsConnected = _Vector1_1_IsConnected;
                float v32_o = v31_Out;
                float v33_Out = _Vector1_5;
                bool v33_Out_IsConnected = _Vector1_5_IsConnected;
                OutVector1_1 = v11_o;
                OutVector11_2 = v32_o;
                OutVector12_3 = v18_Out;
                OutVector4_4 = v27_Out;
                OutVector13_5 = v28_Out;
                OutVector14_6 = v29_Out;
                OutVector15_7 = v30_Out;
                OutVector16_8 = v31_Out;
                OutVector17_9 = v33_Out;
            }

            void sg_Fill_Shadow(float _Fill_Shadow_Enable, bool _Fill_Shadow_Enable_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Fill_Shadow_Use_Hard_Step, bool _Fill_Shadow_Use_Hard_Step_IsConnected, SGInput IN, out float OutVector1_1)
            {
                float v0_Out = _Fill_Shadow_Enable;
                bool v0_Out_IsConnected = _Fill_Shadow_Enable_IsConnected;
                float v1_Out = _Fill_Shadow_Use_Hard_Step;
                bool v1_Out_IsConnected = _Fill_Shadow_Use_Hard_Step_IsConnected;
                float v2_Out = _Vector1;
                bool v2_Out_IsConnected = _Vector1_IsConnected;
                float v3_o = v2_Out;
                float v4_Out;
                sg_Anti_Aliased_Step(0.5, false, v3_o, true, IN, v4_Out);
                float v5_o = v3_o;
                float v6_Out = ((bool)v1_Out) ? v4_Out : v5_o;
                float v7_o = v6_Out;
                float v8_Out = ((bool)v0_Out) ? v7_o : 1.0;
                OutVector1_1 = v8_Out;
            }

            void sg_Edge_Value(float _Fill_Smooth_Edge_Value, bool _Fill_Smooth_Edge_Value_IsConnected, float _Fill_Enable_Smooth_Edge, bool _Fill_Enable_Smooth_Edge_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, SGInput IN, out float OutVector1_1)
            {
                float v0_Out = _Fill_Enable_Smooth_Edge;
                bool v0_Out_IsConnected = _Fill_Enable_Smooth_Edge_IsConnected;
                float v1_Out = _Fill_Smooth_Edge_Value;
                bool v1_Out_IsConnected = _Fill_Smooth_Edge_Value_IsConnected;
                float v2_Out = _Vector1_1;
                bool v2_Out_IsConnected = _Vector1_1_IsConnected;
                float v3_o = v2_Out;
                float v4_Out = _Vector1;
                bool v4_Out_IsConnected = _Vector1_IsConnected;
                float v5_o = v4_Out;
                float v6_Out;
                sg_Smoothed_Step(v3_o, true, v5_o, true, v1_Out, true, IN, v6_Out);
                float v7_Out;
                sg_Anti_Aliased_Step(v3_o, true, v5_o, true, IN, v7_Out);
                float v8_Out = ((bool)v0_Out) ? v6_Out : v7_Out;
                OutVector1_1 = v8_Out;
            }

            void sg_Fill_Shadow_Guide(float _Fill_Shadow_Use_Texture_Guide, bool _Fill_Shadow_Use_Texture_Guide_IsConnected, float _Fill_Shadow_Guide_Strength, bool _Fill_Shadow_Guide_Strength_IsConnected, UnityTexture2D _Fill_Shadow_Texture_Guide, bool _Fill_Shadow_Texture_Guide_IsConnected, float _Fill_Shadow_Flip_Guide_Texture, bool _Fill_Shadow_Flip_Guide_Texture_IsConnected, float2 _Fill_Shadow_Scroll_Speed_XY, bool _Fill_Shadow_Scroll_Speed_XY_IsConnected, float _Fill_Shadow_Enable, bool _Fill_Shadow_Enable_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, float _Vector1_3, bool _Vector1_3_IsConnected, SGInput IN, out float OutVector1_1, out float OutVector11_2, out float OutVector12_3, out float OutVector13_4, out float OutVector14_5)
            {
                float v0_Out = _Fill_Shadow_Use_Texture_Guide;
                bool v0_Out_IsConnected = _Fill_Shadow_Use_Texture_Guide_IsConnected;
                float v1_Out = _Fill_Shadow_Enable;
                bool v1_Out_IsConnected = _Fill_Shadow_Enable_IsConnected;
                UnityTexture2D v2_Out = _Fill_Shadow_Texture_Guide;
                bool v2_Out_IsConnected = _Fill_Shadow_Texture_Guide_IsConnected;
                float v3_Out = _Fill_Shadow_Flip_Guide_Texture;
                bool v3_Out_IsConnected = _Fill_Shadow_Flip_Guide_Texture_IsConnected;
                float2 v4_Out = _Fill_Shadow_Scroll_Speed_XY;
                bool v4_Out_IsConnected = _Fill_Shadow_Scroll_Speed_XY_IsConnected;
                float v5_Texture_R;
                sg_Sample_Texture_Ui(v2_Out, true, v3_Out, true, 0.0, false, v4_Out, true, float2(0.0, 0.0), false, IN, v5_Texture_R);
                float v6_Out = _Fill_Shadow_Guide_Strength;
                bool v6_Out_IsConnected = _Fill_Shadow_Guide_Strength_IsConnected;
                float v7_Out = (v5_Texture_R * v6_Out);
                float v8_Out = ((bool)v1_Out) ? v7_Out : 0.0;
                float v9_Out = ((bool)v0_Out) ? v8_Out : 0.0;
                float v10_Out = _Vector1;
                bool v10_Out_IsConnected = _Vector1_IsConnected;
                float v11_Out = _Vector1_1;
                bool v11_Out_IsConnected = _Vector1_1_IsConnected;
                float v12_Out = _Vector1_2;
                bool v12_Out_IsConnected = _Vector1_2_IsConnected;
                float v13_Out = _Vector1_3;
                bool v13_Out_IsConnected = _Vector1_3_IsConnected;
                OutVector1_1 = v9_Out;
                OutVector11_2 = v10_Out;
                OutVector12_3 = v11_Out;
                OutVector13_4 = v12_Out;
                OutVector14_5 = v13_Out;
            }

            void sg_Fill_Overlay_Coloring(float4 _Vector4, bool _Vector4_IsConnected, float4 _Fill_Overlay_Fill_Color, bool _Fill_Overlay_Fill_Color_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Fill_Overlay_Strength, bool _Fill_Overlay_Strength_IsConnected, float _Fill_Overlay_Fill_Color_Impact, bool _Fill_Overlay_Fill_Color_Impact_IsConnected, float _Fill_Overlay_Fill_Alpha_Impact, bool _Fill_Overlay_Fill_Alpha_Impact_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, SGInput IN, out float4 OutVector4_1, out float OutVector1_2, out float OutVector11_3)
            {
                float4 v0_Out = _Fill_Overlay_Fill_Color;
                bool v0_Out_IsConnected = _Fill_Overlay_Fill_Color_IsConnected;
                float3 v1_Out = (v0_Out).xyz;
                float v2_Out = _Fill_Overlay_Strength;
                bool v2_Out_IsConnected = _Fill_Overlay_Strength_IsConnected;
                float v3_Out = _Fill_Overlay_Fill_Color_Impact;
                bool v3_Out_IsConnected = _Fill_Overlay_Fill_Color_Impact_IsConnected;
                float v4_Out = (v2_Out * v3_Out);
                float4 v5_Out = _Fill_Overlay_Fill_Color;
                bool v5_Out_IsConnected = _Fill_Overlay_Fill_Color_IsConnected;
                float v6_Out = (v5_Out).w;
                float v7_Out = _Fill_Overlay_Strength;
                bool v7_Out_IsConnected = _Fill_Overlay_Strength_IsConnected;
                float v8_Out = _Fill_Overlay_Fill_Alpha_Impact;
                bool v8_Out_IsConnected = _Fill_Overlay_Fill_Alpha_Impact_IsConnected;
                float v9_Out = (v7_Out * v8_Out);
                float4 v10_Out = _Vector4;
                bool v10_Out_IsConnected = _Vector4_IsConnected;
                float3 v11_Out = (v10_Out).xyz;
                float v12_Out = (v10_Out).w;
                float v13_Out = _Vector1;
                bool v13_Out_IsConnected = _Vector1_IsConnected;
                float v14_o = v13_Out;
                float v15_Out = (v14_o * v4_Out);
                float3 v16_Out = lerp(v11_Out, v1_Out, (v15_Out.xxx));
                float v17_R = (v16_Out)[0];
                float v18_G = (v16_Out)[1];
                float v19_B = (v16_Out)[2];
                float v20_A = 0.0;
                float v21_Out = (v14_o * v9_Out);
                float v22_Out = lerp(v12_Out, v6_Out, v21_Out);
                float4 v23_Out = float4(v17_R, v18_G, v19_B, v22_Out);
                float4 v24_Out;
                #if defined(_FILL_OVERLAY_ENABLE)
                    v24_Out = v23_Out;
                #else
                    v24_Out = v10_Out;
                #endif
                float v25_Out = _Vector1_1;
                bool v25_Out_IsConnected = _Vector1_1_IsConnected;
                OutVector4_1 = v24_Out;
                OutVector1_2 = v13_Out;
                OutVector11_3 = v25_Out;
            }

            void sg_Fill_Overlay_Coloring_Background(float4 _Fill_Background_color, bool _Fill_Background_color_IsConnected, float4 _Fill_Overlay_Background_Color, bool _Fill_Overlay_Background_Color_IsConnected, float _Vector1, bool _Vector1_IsConnected, float _Fill_Overlay_Strength, bool _Fill_Overlay_Strength_IsConnected, float _Fill_Overlay_Background_Color_Impact, bool _Fill_Overlay_Background_Color_Impact_IsConnected, float _Fill_Overlay_Background_Alpha_Impact, bool _Fill_Overlay_Background_Alpha_Impact_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, float _Vector1_3, bool _Vector1_3_IsConnected, SGInput IN, out float4 OutVector4_1, out float OutVector1_2, out float OutVector11_3, out float OutVector12_4)
            {
                float4 v0_Out = _Fill_Background_color;
                bool v0_Out_IsConnected = _Fill_Background_color_IsConnected;
                float3 v1_Out = (v0_Out).xyz;
                float4 v2_Out = _Fill_Overlay_Background_Color;
                bool v2_Out_IsConnected = _Fill_Overlay_Background_Color_IsConnected;
                float3 v3_Out = (v2_Out).xyz;
                float v4_Out = _Fill_Overlay_Strength;
                bool v4_Out_IsConnected = _Fill_Overlay_Strength_IsConnected;
                float v5_Out = _Fill_Overlay_Background_Color_Impact;
                bool v5_Out_IsConnected = _Fill_Overlay_Background_Color_Impact_IsConnected;
                float v6_Out = (v4_Out * v5_Out);
                float4 v7_Out = _Fill_Background_color;
                bool v7_Out_IsConnected = _Fill_Background_color_IsConnected;
                float v8_Out = (v7_Out).w;
                float4 v9_Out = _Fill_Overlay_Background_Color;
                bool v9_Out_IsConnected = _Fill_Overlay_Background_Color_IsConnected;
                float v10_Out = (v9_Out).w;
                float v11_Out = _Vector1;
                bool v11_Out_IsConnected = _Vector1_IsConnected;
                float v12_o = v11_Out;
                float v13_Out = (v12_o * v6_Out);
                float3 v14_Out = lerp(v1_Out, v3_Out, (v13_Out.xxx));
                float v15_R = (v14_Out)[0];
                float v16_G = (v14_Out)[1];
                float v17_B = (v14_Out)[2];
                float v18_A = 0.0;
                float v19_Out = _Fill_Overlay_Strength;
                bool v19_Out_IsConnected = _Fill_Overlay_Strength_IsConnected;
                float v20_Out = _Fill_Overlay_Background_Alpha_Impact;
                bool v20_Out_IsConnected = _Fill_Overlay_Background_Alpha_Impact_IsConnected;
                float v21_Out = (v19_Out * v20_Out);
                float v22_Out = (v12_o * v21_Out);
                float v23_Out = lerp(v8_Out, v10_Out, v22_Out);
                float4 v24_Out = float4(v15_R, v16_G, v17_B, v23_Out);
                float v25_Out = _Vector1_1;
                bool v25_Out_IsConnected = _Vector1_1_IsConnected;
                float v26_Out = _Vector1_2;
                bool v26_Out_IsConnected = _Vector1_2_IsConnected;
                float v27_Out = _Vector1_3;
                bool v27_Out_IsConnected = _Vector1_3_IsConnected;
                OutVector4_1 = v24_Out;
                OutVector1_2 = v25_Out;
                OutVector11_3 = v26_Out;
                OutVector12_4 = v27_Out;
            }

            void sg_End_Line_Coloring(float4 _Vector4, bool _Vector4_IsConnected, float4 _End_Line_Color, bool _End_Line_Color_IsConnected, float _End_Line_Visibility, bool _End_Line_Visibility_IsConnected, float _Vector1, bool _Vector1_IsConnected, SGInput IN, out float4 OutVector4_1)
            {
                float4 v0_Out = _Vector4;
                bool v0_Out_IsConnected = _Vector4_IsConnected;
                float4 v1_Out = _End_Line_Color;
                bool v1_Out_IsConnected = _End_Line_Color_IsConnected;
                float v2_R = (v1_Out)[0];
                float v3_G = (v1_Out)[1];
                float v4_B = (v1_Out)[2];
                float v5_A = (v1_Out)[3];
                float4 v6_Out = float4(v2_R, v3_G, v4_B, 0.0);
                float4 v7_Out = _End_Line_Color;
                bool v7_Out_IsConnected = _End_Line_Color_IsConnected;
                float v8_R = (v7_Out)[0];
                float v9_G = (v7_Out)[1];
                float v10_B = (v7_Out)[2];
                float v11_A = (v7_Out)[3];
                float v12_Out = _End_Line_Visibility;
                bool v12_Out_IsConnected = _End_Line_Visibility_IsConnected;
                float v13_Out = (v11_A * v12_Out);
                float v14_Out = _Vector1;
                bool v14_Out_IsConnected = _Vector1_IsConnected;
                float v15_o = v14_Out;
                float v16_Out = (v13_Out * v15_o);
                float4 v17_Out = lerp(v0_Out, v6_Out, (v16_Out.xxxx));
                OutVector4_1 = v17_Out;
            }

            void sg_Final_Coloring(float _Vector1, bool _Vector1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, float _Vector1_3, bool _Vector1_3_IsConnected, float _Vector1_4, bool _Vector1_4_IsConnected, float _Shadow_Enable, bool _Shadow_Enable_IsConnected, float4 _Custom_Outline_Color, bool _Custom_Outline_Color_IsConnected, float4 _Shadow_Color, bool _Shadow_Color_IsConnected, SGInput IN, out float4 OutVector4_1, out float OutVector1_2)
            {
                float v0_Out = _Shadow_Enable;
                bool v0_Out_IsConnected = _Shadow_Enable_IsConnected;
                float4 v1_Out = _Custom_Outline_Color;
                bool v1_Out_IsConnected = _Custom_Outline_Color_IsConnected;
                float4 v2_Out = _Vector4;
                bool v2_Out_IsConnected = _Vector4_IsConnected;
                float4 v3_o = v2_Out;
                float4 v4_Out = ((bool)v0_Out) ? v3_o : v1_Out;
                float4 v5_Out = _Shadow_Color;
                bool v5_Out_IsConnected = _Shadow_Color_IsConnected;
                float v6_Out = _Vector1_1;
                bool v6_Out_IsConnected = _Vector1_1_IsConnected;
                float v7_Out = (1.0 - v6_Out);
                float4 v8_Out = lerp(v3_o, v4_Out, (v7_Out.xxxx));
                float4 v9_Out = lerp(v3_o, v5_Out, (v7_Out.xxxx));
                float4 v10_Out;
                #if defined(_CUSTOM_OUTLINE_ENABLE)
                    v10_Out = v8_Out;
                #else
                    v10_Out = v9_Out;
                #endif
                float4 v11_o = v10_Out;
                float v12_Out = _Vector1_4;
                bool v12_Out_IsConnected = _Vector1_4_IsConnected;
                float v13_Out = _Vector1_2;
                bool v13_Out_IsConnected = _Vector1_2_IsConnected;
                float v14_o = v13_Out;
                float v15_Out = _Vector1;
                bool v15_Out_IsConnected = _Vector1_IsConnected;
                float v16_Out = max(v14_o, v15_Out);
                float v17_Out = _Vector1_3;
                bool v17_Out_IsConnected = _Vector1_3_IsConnected;
                float v18_Out = max(v16_Out, v17_Out);
                float v19_o = v18_Out;
                float v20_Out = max(v12_Out, v19_o);
                float v21_o = v20_Out;
                OutVector4_1 = v11_o;
                OutVector1_2 = v21_o;
            }

            void sg_Fill_Segments_Coloring(float4 _Fill_Segments_Color, bool _Fill_Segments_Color_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Fill_Segments_Strength_On_Color, bool _Fill_Segments_Strength_On_Color_IsConnected, float _Vector1, bool _Vector1_IsConnected, SGInput IN, out float4 OutVector4_1)
            {
                float4 v0_Out = _Fill_Segments_Color;
                bool v0_Out_IsConnected = _Fill_Segments_Color_IsConnected;
                float4 v1_Out = _Vector4;
                bool v1_Out_IsConnected = _Vector4_IsConnected;
                float v2_Out = _Vector1;
                bool v2_Out_IsConnected = _Vector1_IsConnected;
                float v3_Out = (1.0 * v2_Out);
                float v4_Out = _Fill_Segments_Strength_On_Color;
                bool v4_Out_IsConnected = _Fill_Segments_Strength_On_Color_IsConnected;
                float v5_Out = lerp(1.0, v3_Out, v4_Out);
                float v6_Out;
                #if defined(_FILL_SEGMENTS_ENABLE)
                    v6_Out = v5_Out;
                #else
                    v6_Out = 1.0;
                #endif
                float4 v7_Out = lerp(v0_Out, v1_Out, (v6_Out.xxxx));
                float4 v8_o = v7_Out;
                OutVector4_1 = v8_o;
            }

            void sg_Background_Coloring(float _Vector1, bool _Vector1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, float _Fill_Shadow_Use_On_Background, bool _Fill_Shadow_Use_On_Background_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Vector1_3, bool _Vector1_3_IsConnected, float4 _Fill_Shadow_Background_Color, bool _Fill_Shadow_Background_Color_IsConnected, float _Fill_Shadow_Use_Only_Background_Color_Alpha, bool _Fill_Shadow_Use_Only_Background_Color_Alpha_IsConnected, float4 _Vector4_1, bool _Vector4_1_IsConnected, SGInput IN, out float OutVector1_1, out float4 OutVector4_2)
            {
                float v0_Out = _Vector1_1;
                bool v0_Out_IsConnected = _Vector1_1_IsConnected;
                float v1_o = v0_Out;
                float v2_Out = _Fill_Shadow_Use_On_Background;
                bool v2_Out_IsConnected = _Fill_Shadow_Use_On_Background_IsConnected;
                float v3_Out = _Fill_Shadow_Use_Only_Background_Color_Alpha;
                bool v3_Out_IsConnected = _Fill_Shadow_Use_Only_Background_Color_Alpha_IsConnected;
                float4 v4_Out = _Fill_Shadow_Background_Color;
                bool v4_Out_IsConnected = _Fill_Shadow_Background_Color_IsConnected;
                float v5_R = (v4_Out)[0];
                float v6_G = (v4_Out)[1];
                float v7_B = (v4_Out)[2];
                float v8_A = (v4_Out)[3];
                float4 v9_Out = _Fill_Shadow_Background_Color;
                bool v9_Out_IsConnected = _Fill_Shadow_Background_Color_IsConnected;
                float v10_Out = _Vector1_3;
                bool v10_Out_IsConnected = _Vector1_3_IsConnected;
                float4 v11_Out = _Vector4;
                bool v11_Out_IsConnected = _Vector4_IsConnected;
                float v12_R = (v11_Out)[0];
                float v13_G = (v11_Out)[1];
                float v14_B = (v11_Out)[2];
                float v15_A = (v11_Out)[3];
                float4 v16_Out = float4(v12_R, v13_G, v14_B, v8_A);
                float4 v17_Out = ((bool)v3_Out) ? v16_Out : v9_Out;
                float4 v18_Out = lerp(v17_Out, v11_Out, (v10_Out.xxxx));
                float4 v19_Out = ((bool)v2_Out) ? v18_Out : v11_Out;
                float v20_Out = _Vector1;
                bool v20_Out_IsConnected = _Vector1_IsConnected;
                float4 v21_Out = (v19_Out * (v20_Out.xxxx));
                float v22_R = (v21_Out)[0];
                float v23_G = (v21_Out)[1];
                float v24_B = (v21_Out)[2];
                float v25_A = (v21_Out)[3];
                float4 v26_Out = _Vector4_1;
                bool v26_Out_IsConnected = _Vector4_1_IsConnected;
                float4 v27_o = v26_Out;
                float4 v28_o = v27_o;
                float v29_R = (v28_o)[0];
                float v30_G = (v28_o)[1];
                float v31_B = (v28_o)[2];
                float v32_A = (v28_o)[3];
                float v33_Out = _Vector1_2;
                bool v33_Out_IsConnected = _Vector1_2_IsConnected;
                float v34_Out = (v32_A * v33_Out);
                float4 v35_Out = lerp(v21_Out, v28_o, (v34_Out.xxxx));
                float v36_R = (v35_Out)[0];
                float v37_G = (v35_Out)[1];
                float v38_B = (v35_Out)[2];
                float v39_A = (v35_Out)[3];
                float v40_Out = max(v25_A, v39_A);
                float v41_Out = (v40_Out * v1_o);
                float4 v42_o = v35_Out;
                OutVector1_1 = v41_Out;
                OutVector4_2 = v42_o;
            }

            void sg_Fill_Shadow_Multiplier_end_line(float _Vector1, bool _Vector1_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, SGInput IN, out float OutVector1_1)
            {
                float v0_Out = _Vector1_1;
                bool v0_Out_IsConnected = _Vector1_1_IsConnected;
                float v1_o = v0_Out;
                float v2_Out = _Vector1;
                bool v2_Out_IsConnected = _Vector1_IsConnected;
                float v3_Out = (v1_o * v2_Out);
                float v4_Out = (v1_o + v3_Out);
                float v5_Out = saturate(v4_Out);
                OutVector1_1 = v5_Out;
            }

            void sg_Fill_Coloring(float _Fill_Use_Horizontal_Gradient, bool _Fill_Use_Horizontal_Gradient_IsConnected, float4 _Fill_Color_No_Gradient, bool _Fill_Color_No_Gradient_IsConnected, float4 _Fill_Gradient_Color_Right, bool _Fill_Gradient_Color_Right_IsConnected, float4 _Fill_Gradient_Color_Left, bool _Fill_Gradient_Color_Left_IsConnected, float _Vector1, bool _Vector1_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float _Vector1_1, bool _Vector1_1_IsConnected, float _Fill_Second_Bar_Enable, bool _Fill_Second_Bar_Enable_IsConnected, float4 _Fill_Vertical_Darken_Color, bool _Fill_Vertical_Darken_Color_IsConnected, float _Vector1_2, bool _Vector1_2_IsConnected, SGInput IN, out float4 OutVector4_1, out float4 OutVector41_2)
            {
                float v0_Out = _Fill_Second_Bar_Enable;
                bool v0_Out_IsConnected = _Fill_Second_Bar_Enable_IsConnected;
                float4 v1_Out = _Vector4;
                bool v1_Out_IsConnected = _Vector4_IsConnected;
                float v2_Out = _Vector1_1;
                bool v2_Out_IsConnected = _Vector1_1_IsConnected;
                float v3_Out = _Fill_Use_Horizontal_Gradient;
                bool v3_Out_IsConnected = _Fill_Use_Horizontal_Gradient_IsConnected;
                float4 v4_Out = _Fill_Gradient_Color_Right;
                bool v4_Out_IsConnected = _Fill_Gradient_Color_Right_IsConnected;
                float4 v5_Out = _Fill_Gradient_Color_Left;
                bool v5_Out_IsConnected = _Fill_Gradient_Color_Left_IsConnected;
                float v6_Out = _Vector1;
                bool v6_Out_IsConnected = _Vector1_IsConnected;
                float v7_o = v6_Out;
                float4 v8_Out = lerp(v4_Out, v5_Out, (v7_o.xxxx));
                float4 v9_Out = _Fill_Color_No_Gradient;
                bool v9_Out_IsConnected = _Fill_Color_No_Gradient_IsConnected;
                float4 v10_Out = ((bool)v3_Out) ? v8_Out : v9_Out;
                float4 v11_Out = _Fill_Vertical_Darken_Color;
                bool v11_Out_IsConnected = _Fill_Vertical_Darken_Color_IsConnected;
                float v12_Out = _Vector1_2;
                bool v12_Out_IsConnected = _Vector1_2_IsConnected;
                float4 v13_Out = lerp(v10_Out, v11_Out, (v12_Out.xxxx));
                float4 v14_o = v13_Out;
                float4 v15_Out = lerp(v14_o, v1_Out, (v2_Out.xxxx));
                float4 v16_Out = ((bool)v0_Out) ? v15_Out : v14_o;
                float4 v17_o = v16_Out;
                OutVector4_1 = v17_o;
                OutVector41_2 = v14_o;
            }

            void sg_Only_Background_Alpha(float _Fill_Use_Only_Background_Alpha, bool _Fill_Use_Only_Background_Alpha_IsConnected, float4 _Vector4, bool _Vector4_IsConnected, float4 _Vector4_1, bool _Vector4_1_IsConnected, float4 _Fill_Background_color, bool _Fill_Background_color_IsConnected, float _Vector1, bool _Vector1_IsConnected, SGInput IN, out float4 OutVector4_1, out float OutVector1_2)
            {
                float v0_Out = _Fill_Use_Only_Background_Alpha;
                bool v0_Out_IsConnected = _Fill_Use_Only_Background_Alpha_IsConnected;
                float4 v1_Out = _Vector4;
                bool v1_Out_IsConnected = _Vector4_IsConnected;
                float v2_R = (v1_Out)[0];
                float v3_G = (v1_Out)[1];
                float v4_B = (v1_Out)[2];
                float v5_A = (v1_Out)[3];
                float4 v6_Out = _Vector4_1;
                bool v6_Out_IsConnected = _Vector4_1_IsConnected;
                float4 v7_Out = _Fill_Background_color;
                bool v7_Out_IsConnected = _Fill_Background_color_IsConnected;
                float4 v8_Out;
                #if defined(_FILL_OVERLAY_ENABLE)
                    v8_Out = v6_Out;
                #else
                    v8_Out = v7_Out;
                #endif
                float4 v9_o = v8_Out;
                float v10_R = (v9_o)[0];
                float v11_G = (v9_o)[1];
                float v12_B = (v9_o)[2];
                float v13_A = (v9_o)[3];
                float4 v14_Out = float4(v2_R, v3_G, v4_B, v13_A);
                float4 v15_Out = ((bool)v0_Out) ? v14_Out : v9_o;
                float v16_Out = _Vector1;
                bool v16_Out_IsConnected = _Vector1_IsConnected;
                OutVector4_1 = v15_Out;
                OutVector1_2 = v16_Out;
            }

            void sg_Desaturate(float3 _Vector3, bool _Vector3_IsConnected, float _Desaturate, bool _Desaturate_IsConnected, SGInput IN, out float3 Out_Vector3_1)
            {
                float3 v0_Out = _Vector3;
                bool v0_Out_IsConnected = _Vector3_IsConnected;
                float3 v1_Out = _Vector3;
                bool v1_Out_IsConnected = _Vector3_IsConnected;
                float3 v2_Out = float3(0.0, 0.0, 0.0);
                float v3_Out = dot(v1_Out, v2_Out);
                float v4_Out = _Desaturate;
                bool v4_Out_IsConnected = _Desaturate_IsConnected;
                float3 v5_Out = lerp(v0_Out, (v3_Out.xxx), (v4_Out.xxx));
                Out_Vector3_1 = v5_Out;
            }
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.uv = input.uv;
                output.positionWS = posInputs.positionWS;
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                return output;
            }

            void ProgressBar(SGInput IN, out float3 baseColor, out float alpha)
            {
    float v0_Out = _Desaturate;
    bool v0_Out_IsConnected = true;
    float v1_Out = _Custom_Outline_Noise_Guide_1_Strength;
    bool v1_Out_IsConnected = true;
    UnityTexture2D v2_Out = UnityBuildTexture2DStruct(_Custom_Outline_Noise_Guide_1, sampler_Custom_Outline_Noise_Guide_1, _Custom_Outline_Noise_Guide_1_ST);
    bool v2_Out_IsConnected = true;
    float2 v3_Out = _Custom_Outline_Noise_Guide_2_Scroll_Speed.xy;
    bool v3_Out_IsConnected = true;
    UnityTexture2D v4_Out = UnityBuildTexture2DStruct(_Custom_Outline_Noise_Guide_2, sampler_Custom_Outline_Noise_Guide_2, _Custom_Outline_Noise_Guide_2_ST);
    bool v4_Out_IsConnected = true;
    float v5_Out = _Custom_Outline_Noise_Guide_2_Strength;
    bool v5_Out_IsConnected = true;
    float v6_Out = _Custom_Outline_Size;
    bool v6_Out_IsConnected = true;
    float v7_Out = _Custom_Outline_Smoothness;
    bool v7_Out_IsConnected = true;
    float4 v8_Out = IsGammaSpace() ? LinearToSRGB(_Custom_Outline_Color) : _Custom_Outline_Color;
    bool v8_Out_IsConnected = true;
    float v9_Out = _Custom_Outline_Depends_On_Fill_Amount;
    bool v9_Out_IsConnected = true;
    float v10_Out = _Custom_Outline_Use_Noise_Guides;
    bool v10_Out_IsConnected = true;
    float v11_Out = _Custom_Outline_Power;
    bool v11_Out_IsConnected = true;
    float v12_Out = _Custom_Outline_Strength;
    bool v12_Out_IsConnected = true;
    float v13_Out = _Custom_Outline_Offset;
    bool v13_Out_IsConnected = true;
    float2 v14_Out = _Custom_Outline_Noise_Guide_1_Scroll_Speed.xy;
    bool v14_Out_IsConnected = true;
    float v15_Out = _Shadow_Strength;
    bool v15_Out_IsConnected = true;
    float v16_Out = _Shadow_Size;
    bool v16_Out_IsConnected = true;
    float v17_Out = _Shadow_Smoothness;
    bool v17_Out_IsConnected = true;
    float v18_Out = _Shadow_Power;
    bool v18_Out_IsConnected = true;
    float4 v19_Out = IsGammaSpace() ? LinearToSRGB(_Shadow_Color) : _Shadow_Color;
    bool v19_Out_IsConnected = true;
    float4 v20_Out = IsGammaSpace() ? LinearToSRGB(_Simple_Outline_Color) : _Simple_Outline_Color;
    bool v20_Out_IsConnected = true;
    float v21_Out = _Simple_Outline_Thickness;
    bool v21_Out_IsConnected = true;
    float4 v22_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Segments_Color) : _Fill_Segments_Color;
    bool v22_Out_IsConnected = true;
    float v23_Out = _Fill_Shadow_Use_On_Background;
    bool v23_Out_IsConnected = true;
    float v24_Out = _Fill_Use_Only_Background_Alpha;
    bool v24_Out_IsConnected = true;
    float4 v25_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Background_color) : _Fill_Background_color;
    bool v25_Out_IsConnected = true;
    float4 v26_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Shadow_Background_Color) : _Fill_Shadow_Background_Color;
    bool v26_Out_IsConnected = true;
    float v27_Out = _Fill_Shadow_Use_Only_Background_Color_Alpha;
    bool v27_Out_IsConnected = true;
    float4 v28_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Background_color) : _Fill_Background_color;
    bool v28_Out_IsConnected = true;
    float4 v29_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Overlay_Background_Color) : _Fill_Overlay_Background_Color;
    bool v29_Out_IsConnected = true;
    float v30_Out = _Fill_Overlay_Strength;
    bool v30_Out_IsConnected = true;
    float v31_Out = _Fill_Overlay_Background_Color_Impact;
    bool v31_Out_IsConnected = true;
    float v32_Out = _Fill_Overlay_Background_Alpha_Impact;
    bool v32_Out_IsConnected = true;
    float v33_Out = _Fill_Shadow_Use_Only_Color_Alpha;
    bool v33_Out_IsConnected = true;
    float4 v34_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Shadow_Color) : _Fill_Shadow_Color;
    bool v34_Out_IsConnected = true;
    float v35_R = (v34_Out)[0];
    float v36_G = (v34_Out)[1];
    float v37_B = (v34_Out)[2];
    float v38_A = (v34_Out)[3];
    float4 v39_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Shadow_Color) : _Fill_Shadow_Color;
    bool v39_Out_IsConnected = true;
    float v40_Out = _Fill_Use_Horizontal_Gradient;
    bool v40_Out_IsConnected = true;
    float4 v41_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Color_No_Gradient) : _Fill_Color_No_Gradient;
    bool v41_Out_IsConnected = true;
    float4 v42_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Gradient_Color_Right) : _Fill_Gradient_Color_Right;
    float4 v43_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Gradient_Color_Left) : _Fill_Gradient_Color_Left;
    float v44_Out = _Fill_Second_Bar_Enable;
    bool v44_Out_IsConnected = true;
    float4 v45_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Vertical_Darken_Color) : _Fill_Vertical_Darken_Color;
    float v46_Out = _Fill_Vertical_Darken_Power;
    float v47_Out = _Fill_Vertical_Darken_Use_Flat;
    bool v47_Out_IsConnected = true;
    float v48_Out = _Fill_Vertical_Darken_Flip;
    float v49_Out = _Fill_Vertical_Darken_Amount;
    bool v49_Out_IsConnected = true;
    float v50_Out = _Fill_Vertical_Darken_Enable;
    bool v50_Out_IsConnected = true;
    float v51_Out = _Fill_Smooth_Edge_Value;
    bool v51_Out_IsConnected = true;
    float v52_Out = _Fill_Enable_Smooth_Edge;
    float4 v53_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Overlay_Fill_Color) : _Fill_Overlay_Fill_Color;
    bool v53_Out_IsConnected = true;
    float v54_Out = _Fill_Overlay_Strength;
    float v55_Out = _Fill_Overlay_Fill_Color_Impact;
    bool v55_Out_IsConnected = true;
    float v56_Out = _Fill_Overlay_Fill_Alpha_Impact;
    float4 v57_Out = IsGammaSpace() ? LinearToSRGB(_End_Line_Color) : _End_Line_Color;
    bool v57_Out_IsConnected = true;
    float v58_Out = _End_Line_Visibility;
    float v59_Out = _End_Line_Smoothness_Left;
    float v60_Out = _End_Line_Smoothness_Right;
    float v61_Out = _End_Line_Power;
    float v62_Out = _End_Line_Multiply_With_Fill_Shadow;
    float v63_Out = _End_Line_Width_Right;
    float v64_Out = _End_Line_Multiplier;
    float v65_Out = _End_Line_Width_Left;
    float v66_Out = _End_Line_Offset;
    UnityTexture2D v67_Out = UnityBuildTexture2DStruct(_Fill_Overlay_Noise_Guide_1, sampler_Fill_Overlay_Noise_Guide_1, _Fill_Overlay_Noise_Guide_1_ST);
    float2 v68_Out = _Fill_Overlay_Noise_Guide_1_Scroll_Speed.xy;
    UnityTexture2D v69_Out = UnityBuildTexture2DStruct(_Fill_Overlay_Noise_Guide_2, sampler_Fill_Overlay_Noise_Guide_2, _Fill_Overlay_Noise_Guide_2_ST);
    float2 v70_Out = _Fill_Overlay_Noise_Guide_2_Scroll_Speed.xy;
    float v71_Out = _Fill_Overlay_Noise_Guide_1_Strength;
    float v72_Out = _Fill_Overlay_Noise_Guide_2_Strength;
    float v73_Out = _Fill_Overlay_Step_Offset;
    float v74_Out = _Fill_Overlay_Step_Smoothness;
    float v75_Out = _Fill_Overlay_Inverse_Overlay;
    float v76_Out = _Fill_Overlay_Guide_Rotation;
    float v77_OutVector1;
    sg_Fill_Overlay(v67_Out, true, v68_Out, true, v69_Out, true, v70_Out, true, v71_Out, true, v72_Out, true, v73_Out, true, v74_Out, true, v75_Out, true, v76_Out, true, IN, v77_OutVector1);
    float v78_Out = _Fill_Segments_Strength_On_Color;
    float v79_Out = _Fill_Segments_Segments_Number;
    float v80_Out = _Fill_Segments_Width;
    float v81_Out = _Fill_Segments_Edge_Smoothness;
    float2 v82_Out = _General_Shape_Size_Offset.xy;
    float v83_Out = _Fill_Segments_Skew;
    float v84_Out = _Fill_Segments_Custom_Shape_Strength;
    float v85_Out = _Fill_Segments_Add_Alpha_To_1;
    float v86_Out = _Fill_Segments_Multiply_Alpha_To_0;
    float v87_Out = _Fill_Amount;
    float v88_Out = _Fill_Skew;
    float v89_Out = _General_Shape_Height;
    float v90_Out = _Fill_Enable_Smooth_Edge;
    float v91_Out = _Fill_Smooth_Edge_Value;
    float v92_Out = _Fill_Enable_Edge_Guide;
    float v93_Out = _Fill_Guide_Strength;
    float2 v94_Out = _General_Shape_Size_Offset.xy;
    float v95_OutVector1;
    float2 v96_OutVector2;
    sg_Fill_Amount(v87_Out, true, v88_Out, true, v89_Out, true, v90_Out, true, v91_Out, true, v92_Out, true, v93_Out, true, v94_Out, true, IN, v95_OutVector1, v96_OutVector2);
    float v97_o = v95_OutVector1;
    float v98_o = v95_OutVector1;
    float v99_o = v98_o;
    float2 v100_o = v96_OutVector2;
    float v101_Out = _Fill_Smooth_Edge_Value;
    float v102_Out = _Fill_Enable_Smooth_Edge;
    float v103_Out = _Main_Bar_Fill_Amount;
    float v104_Out = _Fill_Shadow_Enable;
    float v105_Out = _Fill_Shadow_Use_Hard_Step;
    float v106_Out = _Second_Bar_Main_Blend;
    float4 v107_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Second_Bar_Color) : _Fill_Second_Bar_Color;
    float v108_Out = _Second_Bar_Invisible_Blend;
    float v109_Out = _Fill_Shadow_Use_Texture_Guide;
    float v110_Out = _Fill_Shadow_Guide_Strength;
    UnityTexture2D v111_Out = UnityBuildTexture2DStruct(_Fill_Shadow_Texture_Guide, sampler_Fill_Shadow_Texture_Guide, _Fill_Shadow_Texture_Guide_ST);
    float v112_Out = _Fill_Shadow_Flip_Guide_Texture;
    float2 v113_Out = _Fill_Shadow_Scroll_Speed_XY.xy;
    float v114_Out = _Fill_Shadow_Enable;
    float v115_Out = _Fill_Skew;
    float v116_Out = _Fill_Guide_Strength;
    float v117_Out = _Fill_Enable_Edge_Guide;
    UnityTexture2D v118_Out = UnityBuildTexture2DStruct(_Fill_Texture_Guide, sampler_Fill_Texture_Guide, _Fill_Texture_Guide_ST);
    float v119_Out = _Fill_Flip_Guide_Texture_90_degrees_rotation;
    float2 v120_Out = _Fill_Scroll_speed_XY.xy;
    float v121_Out = _Fill_Flip_Guide_Texture_Value;
    float v122_Out = _Fill_Segments_Top_Offset;
    float v123_Out = _Fill_Segments_Top_Smoothness;
    float v124_Out = _Fill_Segments_Top_Strength;
    float v125_Out = _Fill_Segments_Use_Edge_Step;
    float4 v126_Out = IsGammaSpace() ? LinearToSRGB(_Fill_Segments_Color) : _Fill_Segments_Color;
    float v127_Out = _Simple_Outline_Use_Color_Alpha_For_Color_Blending;
    float v128_Out = _Shadow_Enable;
    float v129_Out = _General_Shape_Roundness;
    float v130_Out = _Simple_Outline_Thickness;
    float v131_Out = _General_Shape_Height;
    float v132_Out = _General_Shape_Roundness;
    float v133_Out = _General_Shape_Roudness_Top_Right;
    float v134_Out = _General_Shape_Roudness_Bottom_Right;
    float v135_Out = _General_Shape_Roudness_Top_Left;
    float v136_Out = _General_Shape_Roudness_Bottom_Left;
    float v137_Out = _General_Shape_Skew_Left_Top;
    float v138_Out = _General_Shape_Skew_Left_Bottom;
    float v139_Out = _General_Shape_Skew_Right_Top;
    float v140_Out = _General_Shape_Skew_Right_Bottom;
    float v141_Out = _General_Shape_Skew_Right_Invert;
    float v142_Out = _General_Shape_Skew_Left_Invert;
    float v143_Out = _General_Shape_Skew_Right_Edge_Place;
    float v144_Out = _General_Shape_Height;
    float v145_Out = _General_Shape_Skew_Left_Edge_Place;
    float2 v146_OutVector2;
    sg_General_Shape(v137_Out, true, v138_Out, true, v139_Out, true, v140_Out, true, v141_Out, true, v142_Out, true, v143_Out, true, v144_Out, true, v145_Out, true, IN, v146_OutVector2);
    float v147_Out = _General_Shape_Skew_Left_Invert;
    float v148_Out = _General_Shape_Skew_Left_Edge_Place;
    float v149_Out = _General_Shape_Skew_Left_Bottom;
    float v150_Out = _General_Shape_Skew_Left_Top;
    float v151_Out = _General_Shape_Skew_Right_Invert;
    float v152_Out = _General_Shape_Skew_Right_Edge_Place;
    float v153_Out = _General_Shape_Skew_Right_Bottom;
    float v154_Out = _General_Shape_Skew_Right_Top;
    float2 v155_Out = _General_Shape_Size_Offset.xy;
    float v156_Out = _Fill_Shadow_Power_Hardness;
    float v157_Out = _Fill_Shadow_Multiplier_Shadow_Amount;
    UnityTexture2D v158_Out = UnityBuildTexture2DStruct(_Custom_Shape_Noise_Guide_1, sampler_Custom_Shape_Noise_Guide_1, _Custom_Shape_Noise_Guide_1_ST);
    float2 v159_Out = _Custom_Shape_Noise_Guide_1_Scroll_Speed.xy;
    UnityTexture2D v160_Out = UnityBuildTexture2DStruct(_Custom_Shape_Noise_Guide_2, sampler_Custom_Shape_Noise_Guide_2, _Custom_Shape_Noise_Guide_2_ST);
    float v161_Out = _Custom_Shape_Noise_Guide_2_Strength;
    float2 v162_Out = _Custom_Shape_Noise_Guide_2_Scroll_Speed.xy;
    float v163_Out = _Custom_Shape_Noise_Guide_1_Strength;
    float v164_Out = _Custom_Shape_Use_Second_Texture;
    float v165_OutVector1;
    float v166_OutVector11;
    sg_Custom_Shape(v158_Out, true, v159_Out, true, v160_Out, true, v161_Out, true, v162_Out, true, v163_Out, true, v164_Out, true, IN, v165_OutVector1, v166_OutVector11);
    float4 v167_OutVector4;
    float2 v168_OutVector2;
    float2 v169_OutVector21;
    float2 v170_OutVector22;
    float v171_OutVector1;
    float v172_OutVector11;
    float v173_OutVector12;
    sg_2D_Box_Properties(v131_Out, true, v132_Out, true, v133_Out, true, v134_Out, true, v135_Out, true, v136_Out, true, v146_OutVector2, true, v147_Out, true, v148_Out, true, v149_Out, true, v150_Out, true, v151_Out, true, v152_Out, true, v153_Out, true, v154_Out, true, v155_Out, true, v156_Out, true, v157_Out, true, v165_OutVector1, true, IN, v167_OutVector4, v168_OutVector2, v169_OutVector21, v170_OutVector22, v171_OutVector1, v172_OutVector11, v173_OutVector12);
    float v174_Fill;
    float v175_Fill_Hard_Step;
    float v176_Fill_Shadow;
    float v177_Outline;
    float v178_Glow;
    float v179_SDF;
    sg_2D_Box(v129_Out, true, v167_OutVector4, true, v170_OutVector22, true, v130_Out, true, 49.66999816894531, false, v172_OutVector11, true, v171_OutVector1, true, v169_OutVector21, true, v168_OutVector2, true, v173_OutVector12, true, IN, v174_Fill, v175_Fill_Hard_Step, v176_Fill_Shadow, v177_Outline, v178_Glow, v179_SDF);
    float v180_o = v175_Fill_Hard_Step;
    float v181_o = v180_o;
    float v182_o = v174_Fill;
    float v183_o = v182_o;
    float v184_OutVector1;
    sg_Fill_Shadow(v104_Out, true, v176_Fill_Shadow, true, v105_Out, true, IN, v184_OutVector1);
    float v185_o = v184_OutVector1;
    float v186_o = v185_o;
    float v187_OutVector1;
    float v188_OutVector11;
    float v189_OutVector12;
    float v190_OutVector13;
    float v191_OutVector14;
    sg_Fill_Skew_and_Edge_Guide(v115_Out, true, v116_Out, true, v117_Out, true, v118_Out, true, v119_Out, true, v120_Out, true, v121_Out, true, v174_Fill, true, v179_SDF, true, v166_OutVector11, true, IN, v187_OutVector1, v188_OutVector11, v189_OutVector12, v190_OutVector13, v191_OutVector14);
    float v192_o = v188_OutVector11;
    float v193_o = v192_o;
    float v194_o = v187_OutVector1;
    float v195_OutVector1;
    sg_Edge_Value(v51_Out, true, v52_Out, true, v194_o, true, v97_o, true, IN, v195_OutVector1);
    float v196_o = v195_OutVector1;
    float v197_o = v196_o;
    float v198_OutVector1;
    float v199_OutVector11;
    sg_Fill_Vertical_Darken(v46_Out, true, v47_Out, true, v48_Out, true, v49_Out, true, v50_Out, true, v195_OutVector1, true, IN, v198_OutVector1, v199_OutVector11);
    float v200_o = v199_OutVector11;
    float v201_o = v200_o;
    float v202_o = v198_OutVector1;
    float v203_OutVector1;
    float v204_OutVector11;
    float v205_OutVector12;
    float v206_OutVector13;
    float v207_OutVector14;
    sg_Fill_Shadow_Guide(v109_Out, true, v110_Out, true, v111_Out, true, v112_Out, true, v113_Out, true, v114_Out, true, v187_OutVector1, true, v189_OutVector12, true, v190_OutVector13, true, v191_OutVector14, true, IN, v203_OutVector1, v204_OutVector11, v205_OutVector12, v206_OutVector13, v207_OutVector14);
    float v208_OutVector1;
    sg_Fill_Shadow_Multiplier_end_line(v203_OutVector1, true, v186_o, true, IN, v208_OutVector1);
    float v209_o = v208_OutVector1;
    float v210_o = v209_o;
    float v211_o = v210_o;
    float v212_o = v175_Fill_Hard_Step;
    float v213_o = v212_o;
    float v214_o = v213_o;
    float v215_o = v214_o;
    float v216_OutVector1;
    float v217_OutVector11;
    float v218_OutVector12;
    float4 v219_OutVector4;
    float v220_OutVector13;
    float v221_OutVector14;
    float v222_OutVector15;
    float v223_OutVector16;
    float v224_OutVector17;
    sg_Fill_Second_Bar(v100_o, true, v101_Out, true, v102_Out, true, v204_OutVector11, true, v103_Out, true, v208_OutVector1, true, float4(0.0, 0.0, 0.0, 0.0), false, v106_Out, true, v107_Out, true, v108_Out, true, v205_OutVector12, true, v206_OutVector13, true, v207_OutVector14, true, v215_o, true, IN, v216_OutVector1, v217_OutVector11, v218_OutVector12, v219_OutVector4, v220_OutVector13, v221_OutVector14, v222_OutVector15, v223_OutVector16, v224_OutVector17);
    float4 v225_OutVector4;
    float4 v226_OutVector41;
    sg_Fill_Coloring(v40_Out, true, v41_Out, true, v42_Out, true, v43_Out, true, v193_o, true, v219_OutVector4, true, v218_OutVector12, true, v44_Out, true, v45_Out, true, v202_o, true, IN, v225_OutVector4, v226_OutVector41);
    float4 v227_o = v225_OutVector4;
    float v228_o = v217_OutVector11;
    float v229_OutVector1;
    float v230_OutVector11;
    sg_End_Line(v59_Out, true, v99_o, true, v60_Out, true, v61_Out, true, v62_Out, true, v223_OutVector16, true, v63_Out, true, v64_Out, true, v224_OutVector17, true, v65_Out, true, v216_OutVector1, true, v66_Out, true, v77_OutVector1, true, IN, v229_OutVector1, v230_OutVector11);
    float v231_o = v230_OutVector11;
    float4 v232_OutVector4;
    float v233_OutVector1;
    float v234_OutVector11;
    sg_Fill_Overlay_Coloring(v227_o, true, v53_Out, true, v231_o, true, v54_Out, true, v55_Out, true, v56_Out, true, v228_o, true, IN, v232_OutVector4, v233_OutVector1, v234_OutVector11);
    float v235_o = v233_OutVector1;
    float4 v236_OutVector4;
    float v237_OutVector1;
    float v238_OutVector11;
    float v239_OutVector12;
    sg_Fill_Overlay_Coloring_Background(v28_Out, true, v29_Out, true, v235_o, true, v30_Out, true, v31_Out, true, v32_Out, true, v214_o, true, v183_o, true, v179_SDF, true, IN, v236_OutVector4, v237_OutVector1, v238_OutVector11, v239_OutVector12);
    float v240_o = v238_OutVector11;
    float v241_o = v237_OutVector1;
    float v242_o = v241_o;
    float v243_o = v242_o;
    float4 v244_o = v232_OutVector4;
    float v245_R = (v244_o)[0];
    float v246_G = (v244_o)[1];
    float v247_B = (v244_o)[2];
    float v248_A = (v244_o)[3];
    float4 v249_Out = float4(v245_R, v246_G, v247_B, v38_A);
    float4 v250_Out = ((bool)v33_Out) ? v249_Out : v39_Out;
    float v251_o = v234_OutVector11;
    float4 v252_Out = lerp(v250_Out, v244_o, (v251_o.xxxx));
    float4 v253_OutVector4;
    float v254_OutVector1;
    sg_Only_Background_Alpha(v24_Out, true, v252_Out, true, v236_OutVector4, true, v25_Out, true, v240_o, true, IN, v253_OutVector4, v254_OutVector1);
    float4 v255_Out = ((v241_o.xxxx) * v252_Out);
    float4 v256_OutVector4;
    sg_End_Line_Coloring(v255_Out, true, v57_Out, true, v58_Out, true, v229_OutVector1, true, IN, v256_OutVector4);
    float v257_OutVector1;
    float4 v258_OutVector4;
    sg_Background_Coloring(v243_o, true, v254_OutVector1, true, v197_o, true, v23_Out, true, v253_OutVector4, true, v211_o, true, v26_Out, true, v27_Out, true, v256_OutVector4, true, IN, v257_OutVector1, v258_OutVector4);
    float v259_o = v257_OutVector1;
    float4 v260_o = v258_OutVector4;
    float v261_o = v222_OutVector15;
    float v262_o = v220_OutVector13;
    float v263_o = v221_OutVector14;
    float v264_OutVector1;
    float v265_OutVector11;
    float v266_OutVector12;
    sg_Fill_Segments(v79_Out, true, v80_Out, true, v81_Out, true, v82_Out, true, v83_Out, true, v261_o, true, v84_Out, true, v85_Out, true, v262_o, true, v86_Out, true, v263_o, true, v122_Out, true, v123_Out, true, v124_Out, true, v125_Out, true, v126_Out, true, IN, v264_OutVector1, v265_OutVector11, v266_OutVector12);
    float v267_o = v265_OutVector11;
    float v268_Out = (v259_o * v267_o);
    float v269_o = v264_OutVector1;
    float v270_Out = max(v268_Out, v269_o);
    float v271_o = v270_Out;
    float v272_o = v271_o;
    float v273_o = v272_o;
    float v274_o = v266_OutVector12;
    float4 v275_OutVector4;
    sg_Fill_Segments_Coloring(v22_Out, true, v260_o, true, v78_Out, true, v274_o, true, IN, v275_OutVector4);
    float4 v276_OutVector4;
    float v277_OutVector1;
    sg_Simple_Outline(v20_Out, true, v239_OutVector12, true, v21_Out, true, v275_OutVector4, true, v127_Out, true, IN, v276_OutVector4, v277_OutVector1);
    float v278_o = v179_SDF;
    float v279_o = v278_o;
    float v280_o = v279_o;
    float v281_o = v280_o;
    float v282_o = v174_Fill;
    float v283_o = v282_o;
    float v284_o = v283_o;
    float4 v285_OutVector4;
    float v286_OutVector1;
    sg_Shadow(v15_Out, true, v16_Out, true, v17_Out, true, v18_Out, true, v19_Out, true, v276_OutVector4, true, v128_Out, true, v280_o, true, v283_o, true, IN, v285_OutVector4, v286_OutVector1);
    float4 v287_o = v285_OutVector4;
    float4 v288_OutVector4;
    float v289_OutVector1;
    float4 v290_OutVector41;
    float v291_OutVector11;
    sg_Custom_Outline(v1_Out, true, v2_Out, true, v3_Out, true, v4_Out, true, v5_Out, true, v6_Out, true, v7_Out, true, v8_Out, true, v201_o, true, v9_Out, true, v287_o, true, v10_Out, true, v11_Out, true, v12_Out, true, v281_o, true, v13_Out, true, v14_Out, true, v276_OutVector4, true, v284_o, true, IN, v288_OutVector4, v289_OutVector1, v290_OutVector41, v291_OutVector11);
    float v292_o = v286_OutVector1;
    float v293_o = v292_o;
    float v294_Out = _Shadow_Enable;
    float4 v295_Out = IsGammaSpace() ? LinearToSRGB(_Custom_Outline_Color) : _Custom_Outline_Color;
    float4 v296_Out = IsGammaSpace() ? LinearToSRGB(_Shadow_Color) : _Shadow_Color;
    float4 v297_OutVector4;
    float v298_OutVector1;
    sg_Final_Coloring(v273_o, true, v181_o, true, v288_OutVector4, true, v289_OutVector1, true, v277_OutVector1, true, v293_o, true, v294_Out, true, v295_Out, true, v296_Out, true, IN, v297_OutVector4, v298_OutVector1);
    float3 v299_Out_Vector3;
    sg_Desaturate((v297_OutVector4.xyz), true, v0_Out, true, IN, v299_Out_Vector3);
    float v300_Out = _MainAlpha;
    float v301_Out = (v298_OutVector1 * v300_Out);
                baseColor = v299_Out_Vector3;
                alpha = v301_Out;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                SGInput sgIn;
                sgIn.uv0 = float4(input.uv, 0.0, 0.0);
                sgIn.time = float3(_Time.y, _SinTime.w, _CosTime.w);
                sgIn.positionOS = float3(0.0, 0.0, 0.0);
                sgIn.positionWS = input.positionWS;
                sgIn.normalWS = float3(0.0, 0.0, 1.0);
                float3 baseColor;
                float alpha;
                ProgressBar(sgIn, baseColor, alpha);
                half4 color = half4(baseColor, alpha);
                color.rgb = MixFog(color.rgb, input.fogFactor);
                return color;
            }
            ENDHLSL
        }
    }
    CustomEditor "ProceduralProgressBarGUI"
    FallBack Off
}
