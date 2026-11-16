// =====================================================================================
//  GF2（少女前线2：追放）gf_shader/scene/env_unlit  ——  参数表复刻
//
//  来源：H:\LOL\GF2Exilium\_unpack\GF2_ShaderPack\02_Scene_场景\gf_shader_scene_env_unlit.shader
//
//  解包产物没有 HLSL 函数体（AssetStudio 只能导到 "shader disassembly not supported on DXBC"），
//  所以这里的做法是：Properties 段 1:1 照抄；Pass 按参数名 + 渲染状态推断，写成能在 URP 14 跑的实现。
//
//  与原版的差异（只有下面 4 处，其余参数名 / 类型 / 默认值 / 排列顺序全部未动）：
//    1. 去掉 CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.GF2.GFUnlitShaderGUI"
//       那个 GUI 类在 GF2 客户端里，本地没有，留着会报 "Could not find a custom editor"
//    2. _Mode / _SrcBlend / _DstBlend / _ZWrite / _QueueOffset 加 [HideInInspector]
//       原版靠上面那个自定义 GUI 把它们藏起来，本地没 GUI 会全裸显示在 Inspector
//    3. Pass 状态从 "Blend 0 Zero Zero / Blend 1 Zero Zero" 改成 "Blend [_SrcBlend] [_DstBlend]"
//       原版走 MRT 双目标，那两个 Zero Zero 直接照抄在普通 URP 单目标管线里会渲成纯黑
//    4. 补一个 DepthOnly Pass（原版没有，但 URP 开 _CameraDepthTexture 时需要）
//
//  原版有、这里保留但代码不采样：_MainTex（和 _BaseMap 是同一个槽的两个名字，原版遗留别名）
//  原版有、这里保留但代码不读：_ColorMask0 / _ColorMask1（MRT 写入掩码，单目标用不上）
//  原版有、这里没实现：_ALPHABLEND_ON / _ALPHAPREMULTIPLY_ON 两个变体关键字
//        —— 原版是给自定义 GUI 切 Blend 用的，这里由 _SrcBlend / _DstBlend 直接控制，不需要
//
//  定位：远景面片 / 背景板。不参与光照，只吃贴图 + 雾。
//  _FogDensityOffset 是整个 GF2 场景 shader 包里唯一一个"单独修正雾浓度"的参数，
//  远景面片靠它比近景更快融进雾里，这是这支 shader 最值得抄的东西。
// =====================================================================================

Shader "GLSA/Scene/EnvUnlit"
{
    Properties
    {
        // ---------- 渲染状态（原版顺序：_Mode 排最前，在 _Cutoff 之前）----------
        [HideInInspector] _Mode ("__mode", Float) = 0
        [HideInInspector] _SrcBlend ("__src", Float) = 1
        [HideInInspector] _DstBlend ("__dst", Float) = 0
        [HideInInspector] _ZWrite ("__zw", Float) = 1
        [HideInInspector] _QueueOffset ("Queue offset", Float) = 0

        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2

        // ---------- Albedo ----------
        [Header(Albedo)] _BaseColor ("Albedo Color", Color) = (1,1,1,1)
        _BaseMap ("Albedo Map", 2D) = "white" { }

        // ---------- 远景三件套（原版就是这个顺序，没有 Header）----------
        _GlowIntensity ("_GlowIntensity", Range(0, 1)) = 1
        _FresnelPower ("Fresnel Power", Range(0, 1)) = 0.85
        _FresnelClamp ("Fresnel Clamp", Range(0, 1)) = 0.75
        _FogDensityOffset ("Fog Density Offset", Range(-1, 1)) = 0

        // 原版遗留的别名槽，代码不采样。老材质可能只填了这一个。
        _MainTex ("BaseMap", 2D) = "white" { }

        // ---------- AerialFog 兼容（新增，不属于原版参数表）----------
        // 勾上：不吃 URP 内置雾，雾全部交给 AerialFog 全屏后处理（项目默认，场景 m_Fog = 0）
        // 取消：恢复原版行为，用 URP 内置雾 + _FogDensityOffset
        [Toggle(_AERIALFOG_ON)] _UseAerialFog ("吃 AerialFog（跳过内置雾）", Float) = 1

        // MRT 写入掩码，原版走双目标渲染，单目标 URP 用不到
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorMask0 ("ColorMask0", Float) = 15
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorMask1 ("ColorMask1", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"  = "UniversalPipeline"
            "RenderType"      = "Opaque"
            "Queue"           = "Geometry"
            "IgnoreProjector" = "True"
        }
        LOD 300

        // ============================================================================
        //  公共段：两个 Pass 共用，保证 UnityPerMaterial 布局一致
        // ============================================================================
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Properties 里声明的每个标量 / 向量都必须进 UnityPerMaterial，
        // 少一个 Unity 就会警告 "Property (_X) exists in Properties block but not in the CBUFFER"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _MainTex_ST;
            float4 _BaseColor;
            float  _Cutoff;
            float  _Cull;
            float  _GlowIntensity;
            float  _FresnelPower;
            float  _FresnelClamp;
            float  _FogDensityOffset;
            float  _UseAerialFog;
            float  _Mode;
            float  _SrcBlend;
            float  _DstBlend;
            float  _ZWrite;
            float  _QueueOffset;
            float  _ColorMask0;
            float  _ColorMask1;
        CBUFFER_END

        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);   // 别名槽，声明只为消 Unity 的未使用提示

        // ----------------------------------------------------------------------------
        //  边缘项
        //  原版 _FresnelPower 是 Range(0,1)，直接当 pow 指数的话指数只能在 0..1，
        //  1 次方和 0 次方几乎没区别，做不出边缘。这里映射成 1..10 的指数区间。
        //  _FresnelClamp 就是边缘上限，原版默认 0.75，挺高的，说明它本来是往亮里加。
        //  但拿不到函数体，没法确认是加亮还是压暗，所以默认走"压暗"（伪造体积感），
        //  要改成边缘亮边的，把 UnlitPassFragment 里那一行换成注释掉的版本就行。
        // ----------------------------------------------------------------------------
        half GLSA_FresnelFactor(half3 normalWS, half3 viewDirWS)
        {
            half ndv        = saturate(dot(normalize(normalWS), normalize(viewDirWS)));
            half fresnelExp = lerp(1.0h, 10.0h, _FresnelPower);
            return min(pow(1.0h - ndv, fresnelExp), _FresnelClamp);
        }
        ENDHLSL

        // ============================================================================
        //  ForwardLit
        // ============================================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            // 强制写深度：AerialFog 全屏后处理靠 _CameraDepthTexture 反推每个像素的世界坐标，
            // 谁没写进深度图，雾就拿它背后的深度去算（见 AerialFog-踩坑记录.md 第 1 条）。
            // 远景面片没有关深度的理由，_ZWrite 参数保留但这里不再听它的。
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   UnlitPassVertex
            #pragma fragment UnlitPassFragment

            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _AERIALFOG_ON

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3  normalWS   : TEXCOORD2;
                half3  viewDirWS  : TEXCOORD3;
                half   fogFactor  : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS   = nrmInputs.normalWS;
                // 远景面片可能压得很扁，用世界坐标算视线方向比归一化 positionWS 稳
                output.viewDirWS  = GetWorldSpaceViewDir(posInputs.positionWS);
                output.uv         = input.uv;
                output.fogFactor  = ComputeFogFactor(posInputs.positionCS.z);
                return output;
            }

            half4 UnlitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap));
                half4 col       = albedoTex * _BaseColor;
                half  alpha     = col.a;

                #if defined(_ALPHATEST_ON)
                    clip(alpha - _Cutoff);
                #endif

                // 整体亮度倍率。原版默认 1，也就是不改变颜色，纯当提亮 / 压暗用。
                col.rgb *= _GlowIntensity;

                // 边缘
                half fresnel = GLSA_FresnelFactor(input.normalWS, input.viewDirWS);
                col.rgb *= (1.0h - fresnel * 0.5h);            // 默认：边缘压暗，伪造体积
                // col.rgb += fresnel * _BaseColor.rgb;        // 想要边缘亮边就换这一行

                // ---- 雾 ----
                // 项目的所有场景 m_Fog 都是 0，内置雾本来就不跑。这里留两条互斥的路，
                // 是为了防止 AerialFog 和内置雾同时作用、叠成双份。
                //
                //   _AERIALFOG_ON（默认勾选）：什么都不做。不透明物在 250 写深度，
                //       AerialFog 在 450 按深度把它全屏雾化——这就是正确结果，不需要 shader 插手。
                //   取消勾选：走 URP 内置雾，_FogDensityOffset 只有在这条路上才有意义。
                //
                // 注意：AerialFog 是全屏后处理，它的雾量由深度决定，per-object 调不了。
                // 想在 AerialFog 模式下单独让某块远景更浓，得改 Feature（加 stencil 或额外 RT），
                // 不是 shader 能解决的事。
                #if !defined(_AERIALFOG_ON)
                    half fogFactor = input.fogFactor;
                    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                        fogFactor = saturate(fogFactor + _FogDensityOffset);
                    #endif
                    col.rgb = MixFog(col.rgb, fogFactor);
                #endif

                return half4(col.rgb, alpha);
            }
            ENDHLSL
        }

        // ============================================================================
        //  DepthOnly
        //  原版没有这个 Pass（GF2 的 MRT 管线自带深度输出），URP 里要单独补，
        //  否则开了 _CameraDepthTexture 的场景里远景面片在深度图上是个洞。
        // ============================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            DepthVaryings DepthOnlyVertex(DepthAttributes input)
            {
                DepthVaryings output = (DepthVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv         = input.uv;
                return output;
            }

            half4 DepthOnlyFragment(DepthVaryings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);

                #if defined(_ALPHATEST_ON)
                    half4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap));
                    clip(albedoTex.a * _BaseColor.a - _Cutoff);
                #endif

                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
