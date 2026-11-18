// =====================================================================================
//  GLSA / Scene / VHSScreen  ——  显示器屏幕：序列帧播放 + 常驻 VHS 录像带底噪
//
//  用途：贴在场景里显示器 / 电视 / 监控模型的屏幕面上，一块自发光 Unlit 面片。
//        序列帧图集按 _FPS 循环播放，上面压一层一直存在的 VHS 信号毛病：
//        行抖动、色度溢出、扫描线、白噪、磁头切换带、屏幕暗角、亮度呼吸。
//
//  设计文档：docs/superpowers/specs/2026-11-18-vhs-flipbook-screen-shader-design.md
//
//  ---------------------------------------------------------------------------------
//  几条实现上的取舍，改之前先看这里
//
//  1. 单 Pass，Opaque 队列，不写 ShadowCaster（屏幕不投影，省一次 draw）。
//     补了 DepthOnly，因为场景开了 _CameraDepthTexture / 深度预通道，
//     缺这个 Pass 屏幕在深度图里是空的，后面的软粒子、SSAO 会穿帮。
//
//  2. 不开任何 shader keyword，全部参数直接驱动。变体数保持为 1。
//     想加开关就加参数，别加 keyword——屏幕材质会在场景里出现很多份。
//
//  3. 所有材质属性必须在 CBUFFER_START(UnityPerMaterial) 里，顺序与 Properties 一致，
//     否则 SRP Batcher 失效。
//     外部驱动帧索引请用 material.SetFloat("_FrameIndex", n)，
//     **不要用 MaterialPropertyBlock** —— MPB 会把这个 renderer 踢出 SRP Batcher。
//
//  4. 色度溢出走简化 YIQ（分离亮度 / 色度，色度单独水平模糊再合回去），
//     不是简单的 R/B 错位采样。后者会让整块画面色彩歪掉，而且暗部会有假的紫边。
//
//  5. 图集采样走手动 LOD（SAMPLE_TEXTURE2D_LOD），导数用**未抖动**的 UV 算。
//     直接让硬件对抖动后的 UV 求导会导致行抖动把 mip 拉爆，远景一片糊。
//     帧格之间留 1 texel 内缩（padding），压掉相邻帧渗色。
//
//  6. 不做按相机距离的衰减。抖动是 UV 空间的固定位移，屏幕在画面里变小位移也跟着等比例变小，
//     再加上 mipmap 的低通，亚像素级的抖动自己就被平均掉了，不需要额外一个距离参数。
//     扫描线和噪点是例外——它们按"每条线 / 每个噪点格分到几个像素"自动淡出
//     （Frag 里的 scanFade / noiseFade）。这不是距离衰减，是采样频率的抗混叠硬需求，
//     去掉的话远景直接摩尔纹成粗横条，mipmap 救不了它。
// =====================================================================================

Shader "GLSA/Scene/VHSScreen"
{
    Properties
    {
        // ---------- 序列帧 ----------
        [Header(Frames)]
        _MainTex ("序列帧图集", 2D) = "white" {}
        [IntRange] _Cols ("横向帧数", Range(1, 32)) = 4
        [IntRange] _Rows ("纵向帧数", Range(1, 32)) = 4
        _FPS ("播放帧率", Float) = 12
        _StartFrame ("起始帧", Float) = 0
        _UseExternalFrame ("外部帧索引（1=由脚本推帧）", Range(0, 1)) = 0
        _FrameIndex ("外部帧索引值", Float) = 0
        _MipBias ("图集 Mip 偏移", Range(-3, 0)) = -0.5

        // ---------- 屏幕本身 ----------
        [Header(Screen)]
        [HDR] _Tint ("整体色调", Color) = (1, 1, 1, 1)
        _Brightness ("HDR 亮度（>1 交给 Bloom 做外溢光）", Float) = 1.6
        _Desaturate ("整体降低饱和", Range(0, 1)) = 0.12
        _Vignette ("屏幕暗角", Range(0, 1)) = 0.35
        _VignettePower ("暗角收边", Range(0.5, 8)) = 2.2

        // ---------- 行抖动 ----------
        [Header(Jitter)]
        _JitterStrength ("行抖动强度", Range(0, 0.05)) = 0.0035
        _JitterSpeed ("行抖动速度", Float) = 6
        _JitterRowFreq ("行随机密度", Range(1, 512)) = 128
        _JumpChance ("瞬跳概率", Range(0, 1)) = 0.12
        _JumpStrength ("瞬跳强度", Range(0, 0.2)) = 0.018
        _JumpSpeed ("瞬跳速度", Float) = 5

        // ---------- 色度溢出 ----------
        [Header(Chroma)]
        _ChromaShift ("色度偏移", Range(0, 0.05)) = 0.0035
        _ChromaBlur ("色度模糊半径", Range(0, 0.05)) = 0.0045
        _ChromaBleed ("色度糊散程度", Range(0, 1)) = 0.7

        // ---------- 扫描线与噪点 ----------
        [Header(ScanlineNoise)]
        _ScanlineCount ("扫描线条数", Float) = 200
        _ScanlineStrength ("扫描线强度", Range(0, 1)) = 0.16
        _NoiseStrength ("噪点强度", Range(0, 0.5)) = 0.02
        _NoiseSpeed ("噪点刷新速度", Float) = 24
        _NoiseDensity ("噪点密度", Vector) = (1024, 576, 0, 0)

        // ---------- 磁头切换带 ----------
        [Header(HeadSwitching)]
        _HeadSpeed ("切换带速度", Range(0, 1)) = 0.06
        _HeadWidth ("切换带宽度", Range(0, 0.3)) = 0.015
        _HeadShift ("切换带横向错位", Range(0, 0.1)) = 0.008
        _HeadBright ("切换带提亮", Range(0, 1)) = 0.12

        // ---------- 亮度 ----------
        [Header(Brightness)]
        _Flicker ("亮度呼吸", Range(0, 0.5)) = 0.05
        _FlickerSpeed ("呼吸速度", Float) = 1.4

        // ---------- 渲染状态 ----------
        [Header(RenderState)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("背面剔除", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "UniversalMaterialType" = "Unlit"
        }

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _Tint;
            float4 _NoiseDensity;
            float  _Cols;
            float  _Rows;
            float  _FPS;
            float  _StartFrame;
            float  _UseExternalFrame;
            float  _FrameIndex;
            float  _MipBias;
            float  _Brightness;
            float  _Desaturate;
            float  _Vignette;
            float  _VignettePower;
            float  _JitterStrength;
            float  _JitterSpeed;
            float  _JitterRowFreq;
            float  _JumpChance;
            float  _JumpStrength;
            float  _JumpSpeed;
            float  _ChromaShift;
            float  _ChromaBlur;
            float  _ChromaBleed;
            float  _ScanlineCount;
            float  _ScanlineStrength;
            float  _NoiseStrength;
            float  _NoiseSpeed;
            float  _HeadSpeed;
            float  _HeadWidth;
            float  _HeadShift;
            float  _HeadBright;
            float  _Flicker;
            float  _FlickerSpeed;
            float  _Cull;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv         : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv         : TEXCOORD0;
            float  fogCoord   : TEXCOORD1;
        };

        // -------------------------------------------------------------------------
        //  便宜的 hash，不用噪声图也不用三角函数。
        //  Dave Hoskins 那套，移动端够快。
        // -------------------------------------------------------------------------
        float Hash11(float p)
        {
            p = frac(p * 0.1031);
            p *= p + 33.33;
            p *= p + p;
            return frac(p);
        }

        float Hash21(float2 p)
        {
            float3 p3 = frac(p.xyx * 0.1031);
            p3 += dot(p3, p3.yzx + 33.33);
            return frac((p3.x + p3.y) * p3.z);
        }

        // -------------------------------------------------------------------------
        //  简化 YIQ。色度溢出是要把亮度(Y)和色度(I/Q)分开处理：
        //  VHS 的 Cb/Cr 带宽比 Y 低得多，所以色度会横向糊开、还会比亮度慢半拍。
        //  直接对 RGB 分通道错位得到的是假色边，不是 VHS 的味道。
        // -------------------------------------------------------------------------
        float3 RGBToYIQ(float3 c)
        {
            return float3(
                dot(c, float3( 0.299,  0.587,  0.114)),
                dot(c, float3( 0.596, -0.274, -0.322)),
                dot(c, float3( 0.211, -0.523,  0.312)));
        }

        float3 YIQToRGB(float3 yiq)
        {
            return float3(
                yiq.x + 0.956 * yiq.y + 0.621 * yiq.z,
                yiq.x - 0.272 * yiq.y - 0.647 * yiq.z,
                yiq.x - 1.106 * yiq.y + 1.703 * yiq.z);
        }

        // 把跑偏的 UV 映射回当前帧在图集里的格子里，并留 1 texel 内缩防渗色
        float2 ToFrameUV(float2 uvIn, float2 baseOffset, float2 cellSize, float2 padPx)
        {
            float2 c    = saturate(uvIn);
            float2 span = max(cellSize - padPx * 2.0, 1e-5);
            return baseOffset + padPx + c * span;
        }

        // -------------------------------------------------------------------------
        //  顶点
        // -------------------------------------------------------------------------
        Varyings Vert(Attributes input)
        {
            Varyings o = (Varyings)0;
            VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);

            o.positionCS = posInputs.positionCS;
            o.uv         = input.uv;
            o.fogCoord   = ComputeFogFactor(posInputs.positionCS.z);
            return o;
        }

        // -------------------------------------------------------------------------
        //  片元
        // -------------------------------------------------------------------------
        half4 Frag(Varyings input) : SV_Target
        {
            float  time   = _Time.y;
            float2 uv0    = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;

            // ---- 序列帧当前帧号与帧格 ----
            float totalFrames = max(1.0, floor(_Cols) * floor(_Rows));
            float frameRaw    = (_UseExternalFrame > 0.5)
                              ? _FrameIndex
                              : (_StartFrame + floor(time * max(_FPS, 0.0)));
            float frame       = fmod(frameRaw, totalFrames);
            frame             = frame < 0.0 ? frame + totalFrames : frame;

            float2 grid     = float2(max(1.0, floor(_Cols)), max(1.0, floor(_Rows)));
            float2 cellSize = 1.0 / grid;
            float2 padPx    = _MainTex_TexelSize.xy;

            float col = fmod(frame, grid.x);
            float row = floor(frame / grid.x);
            // 图集按左上角起算，Unity 的 UV V 轴朝上，这里翻一次
            float2 baseOffset = float2(col, grid.y - 1.0 - row) * cellSize;

            // ---- 磁头切换带：一条从下往上跑的横条，带内横向错位 + 提亮 ----
            float headPos   = frac(time * _HeadSpeed);
            float headDist  = abs(frac(uv0.y - headPos + 0.5) - 0.5);
            float headBand  = 1.0 - smoothstep(0.0, max(_HeadWidth, 1e-4), headDist);
            headBand        = headBand * headBand; // 软一点，别是硬边
            float headDir   = Hash11(floor(time * 3.0) * 2.17 + 5.0) > 0.5 ? 1.0 : -1.0;
            float headShift = headBand * _HeadShift * headDir;

            // ---- 行抖动：按行量化后 hash，再叠一条沿 y 走的低频游走 ----
            float rowId    = floor(uv0.y * _JitterRowFreq);
            float stepTime = floor(time * _JitterSpeed);
            float rowRand  = Hash11(rowId * 1.37 + stepTime * 7.13);
            float wobble   = sin(uv0.y * 21.0 + time * 2.7) * 0.5 + 0.5;
            float jitter   = ((rowRand - 0.5) * 2.0 + (wobble - 0.5) * 0.6) * _JitterStrength;

            // ---- 瞬跳：整块画面偶尔横向跳一格 ----
            float jumpStep = floor(time * _JumpSpeed);
            float jumpRand = Hash11(jumpStep * 3.71 + 11.0);
            float jump     = step(1.0 - _JumpChance, jumpRand)
                           * (Hash11(jumpStep * 1.93) - 0.5) * 2.0 * _JumpStrength;

            float2 shifty = uv0 + float2(jitter + jump + headShift, 0.0);

            // ---- 采样 LOD：导数用未抖动的 uv0 算，抖动不能影响 mip ----
            float2 dx  = ddx(uv0) * _MainTex_TexelSize.zw;
            float2 dy  = ddy(uv0) * _MainTex_TexelSize.zw;
            float  lod = 0.5 * log2(max(max(dot(dx, dx), dot(dy, dy)), 1e-8));
            lod = clamp(lod + _MipBias, 0.0, 12.0);

            // ---- 三次采样：中心（取亮度）+ 左右（取色度）----
            float2 chromaStep = float2(_ChromaShift + _ChromaBlur, 0.0);

            float2 uvC = ToFrameUV(shifty, baseOffset, cellSize, padPx);
            float2 uvR = ToFrameUV(shifty + chromaStep, baseOffset, cellSize, padPx);
            float2 uvL = ToFrameUV(shifty - chromaStep, baseOffset, cellSize, padPx);

            float3 cCenter = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uvC, lod).rgb;
            float3 cRight  = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uvR, lod).rgb;
            float3 cLeft   = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uvL, lod).rgb;

            // 亮度取中心（轮廓保持锐利），色度取左右平均（带宽被限制住、糊开）
            float3 yiqC = RGBToYIQ(cCenter);
            float3 yiqR = RGBToYIQ(cRight);
            float3 yiqL = RGBToYIQ(cLeft);

            float  luma      = yiqC.x;
            float2 chromaRaw = yiqC.yz;
            float2 chromaBlur = (yiqR.yz + yiqL.yz) * 0.5;
            float2 chroma    = lerp(chromaRaw, chromaBlur, saturate(_ChromaBleed));

            float3 color = YIQToRGB(float3(luma, chroma));

            // ---- 扫描线 ----
            // 屏幕在画面里只占几十像素高时，固定条数会直接摩尔纹成粗横条，
            // 所以按"每条扫描线分到多少像素"淡出，少于 1 像素就没必要画了。
            float scanPixPerLine = _ScanlineCount * abs(ddy(uv0.y));
            float scanFade       = saturate(scanPixPerLine - 1.0);
            float scanPhase = uv0.y * _ScanlineCount * 6.28318530718;
            float scan      = 0.5 + 0.5 * sin(scanPhase);
            scan            = pow(scan, 0.6); // 让暗线窄一点，别一半一半
            float scanMul   = 1.0 - _ScanlineStrength * scan * scanFade;

            // ---- 白噪：绝对幅度恒定，暗部自然显得更脏，这就是录像带雪花的样子 ----
            // 每个噪点格不足 1 像素时同样淡出，否则远景会闪。
            float noiseCellPx = 1.0 / max(_NoiseDensity.y * abs(ddy(uv0.y)), 1e-6);
            float noiseFade   = saturate(noiseCellPx - 1.0);
            float noiseT   = floor(time * _NoiseSpeed);
            float noiseCell = Hash21(floor(uv0 * max(_NoiseDensity.xy, float2(1.0, 1.0))) + noiseT * 0.61);
            color += (noiseCell - 0.5) * _NoiseStrength * 2.0 * noiseFade;

            // ---- 磁头带提亮 ----
            color += headBand * _HeadBright;

            // ---- 屏幕暗角：边角压暗，顺带把玻璃边缘的脏感做出来 ----
            float2 vigVec = uv0 - 0.5;
            float  vigR   = saturate(length(vigVec) * 1.4142136);
            float  vig    = 1.0 - _Vignette * pow(vigR, _VignettePower);

            // ---- 亮度呼吸：一条低频 sin 加一点随机的亮度跳 ----
            float flickSmooth = sin(time * _FlickerSpeed * 6.28318530718) * 0.5 + 0.5;
            float flickRand   = Hash11(floor(time * 12.0) * 4.13);
            float brightMul   = 1.0 + (flickSmooth - 0.5) * _Flicker * 2.0 - flickRand * _Flicker * 0.35;
            brightMul = max(brightMul, 0.0);

            color *= scanMul * vig * brightMul;

            // ---- 降饱和 + 色调 + HDR 亮度（>1 的部分交给 Bloom）----
            float finalLuma = dot(color, float3(0.299, 0.587, 0.114));
            color = lerp(color, finalLuma.xxx, saturate(_Desaturate));
            color *= _Tint.rgb * _Brightness;

            color = MixFog(color, input.fogCoord);
            return half4(color, 1.0);
        }

        ENDHLSL

        // =====================================================================
        //  主 Pass
        // =====================================================================
        Pass
        {
            Name "VHSScreen"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            ZTest LEqual
            Cull [_Cull]
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            ENDHLSL
        }

        // =====================================================================
        //  深度写入：场景开了深度图 / 预通道，没这个 Pass 屏幕在深度里是空的
        // =====================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment FragDepthOnly

            half4 FragDepthOnly(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
