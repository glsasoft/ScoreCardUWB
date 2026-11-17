Shader "GLSA/Scene/PBR_EnvStandard"
{
    Properties
    {
        // ---------- 渲染状态 ----------
        [HideInInspector] _Mode ("__mode", Float) = 0
        [HideInInspector] _SrcBlend ("__src", Float) = 1
        [HideInInspector] _DstBlend ("__dst", Float) = 0
        [HideInInspector] _ZWrite ("__zw", Float) = 1
        [HideInInspector] _QueueOffset ("Queue offset", Float) = 0
        _Cull ("剔除模式", Float) = 2

        // ---------- 基础色 ----------
        [MainTexture] _BaseMap ("基础贴图", 2D) = "white" {}
        [MainColor]   _BaseColor ("基础色", Color) = (1,1,1,1)
        _AlphaClip ("透明裁剪", Float) = 0
        _Cutoff ("裁剪阈值", Range(0,1)) = 0.5

        // ---------- 法线 ----------
        // 默认给 gray：DXT5nm 下 A=G=0.5 解出来就是 (0,0,1) 的平法线，不会歪
        _NRMTex ("法线贴图（原始通道）", 2D) = "gray" {}
        _BumpScale ("法线强度", Range(0,2)) = 1
        // 没挂贴图时把这个关掉，直接走平法线，NormalScale 也不会再影响
        _UseNormalMap ("启用法线贴图", Float) = 1
        // 0 = Unity 标准 RGB 法线（B 就是 Z，GLSA 的 BC7 那种）
        // 1 = 原始 DXT5nm 通道（R 恒 1, X 在 G, Y 在 A）—— 那批粉色图选这个
        // 2 = Unity 已按 Normal map 类型导入（内部 DXT5nm / BC5 编码）
        _NormalMode ("法线模式", Float) = 0
        _NormalFlipY ("翻转法线 Y", Float) = 0
        _DXT5nmSwap ("DXT5nm 交换 XY（G↔A）", Float) = 0

        // ---------- SMO ----------
        // R = Smoothness   G = Metallic   B = Occlusion   A = EmissionMask
        _SMO ("SMO 贴图（R 光滑 G 金属 B AO A 自发光遮罩）", 2D) = "white" {}
        _Metallic ("金属度", Range(0,1)) = 1
        _Smoothness ("光滑度", Range(0,1)) = 1
        _OcclusionStrength ("AO 强度", Range(0,1)) = 1

        // ---------- 自发光 ----------
        // 发光强度看 SMO 贴图的 A 通道，发光颜色直接用基础色（基础贴图 × _BaseColor）。
        // _EmissionSize 只是整体倍率，<= 0 就当完全没有自发光。
        _EmissionSize ("自发光强度倍率", Range(0,4)) = 1

        // ---------- 场景附加 ----------
        _LerpColor ("覆盖颜色", Color) = (1,1,1,1)
        _LerpWeight ("覆盖权重", Range(0,1)) = 0
        [HDR] _TutorialColor ("指引颜色", Color) = (1,1,1,1)
        _Tutorial ("指引强度", Range(0,1)) = 0
        [HDR] _AoeSelectColor ("范围指示颜色", Color) = (1,1,1,1)
        _AoeSelect ("范围指示强度", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"     = "UniversalPipeline"
            "RenderType"         = "Opaque"
            "Queue"              = "Geometry"
            "IgnoreProjector"    = "True"
        }
        LOD 300

        // ====================================================================
        //  公共部分：所有 Pass 共用，保证 UnityPerMaterial 布局一致
        // ====================================================================
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // Shadows.hlsl 里会用到 LerpWhiteTo，它住在 core 包的 CommonMaterial.hlsl
        // URP 的 Core.hlsl 不带这个文件，不补的话 ShadowCaster 会报 undeclared identifier
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _NRMTex_ST;
            float4 _SMO_ST;
            float4 _BaseColor;
            float4 _LerpColor;
            float4 _TutorialColor;
            float4 _AoeSelectColor;
            float  _Cutoff;
            float  _BumpScale;
            float  _NormalMode;
            float  _NormalFlipY;
            float  _DXT5nmSwap;
            float  _UseNormalMap;
            float  _Metallic;
            float  _Smoothness;
            float  _OcclusionStrength;
            float  _EmissionSize;
            float  _LerpWeight;
            float  _Tutorial;
            float  _AoeSelect;
            float  _Cull;
            float  _SrcBlend;
            float  _DstBlend;
            float  _ZWrite;
        CBUFFER_END

        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NRMTex);  SAMPLER(sampler_NRMTex);
        TEXTURE2D(_SMO);     SAMPLER(sampler_SMO);

        // Unity 没给材质挂贴图时会补一张 4x4 的内置纹理，texelSize 就是 0.25；
        // 真贴图基本都 >= 32 像素（texelSize <= 0.031），用它自动判断有没有挂图。
        float4 _NRMTex_TexelSize;
        float4 _SMO_TexelSize;

        half GLSA_HasNormalTex()
        {
            return step(_NRMTex_TexelSize.x, 0.1h);
        }

        half GLSA_HasSMOTex()
        {
            return step(_SMO_TexelSize.x, 0.1h);
        }

        // --------------------------------------------------------------------
        //  法线解码（全部自包含，不依赖 Packing.hlsl / UnityCG，URP 各版本通用）
        //    mode 0 : RGB，B 即 Z
        //    mode 1 : 原始通道，R=1，X/Y 在两个通道里，Z 重建   <-- 你要的
        //    mode 2 : Unity 已编码的法线贴图（DXT5nm 或 BC5）
        //
        //  mode 1 的通道顺序：
        //    swap = 0 : X = A, Y = G   （Unity DXT5nm 标准，packed.wy）
        //    swap = 1 : X = G, Y = A   （另一种导出习惯，packed.ga）
        //  两种都能重建出合理法线，但光照方向会差 90°，哪个对要试。
        // --------------------------------------------------------------------
        half3 GLSA_UnpackRGB(half4 p, half scale)
        {
            half3 n = p.rgb * 2.0 - 1.0;
            n.xy *= scale;
            return n;
        }

        half3 GLSA_UnpackTwoChannel(half4 p, half2 xy01, half scale)
        {
            half3 n;
            n.xy  = xy01 * 2.0 - 1.0;
            n.xy *= scale;
            n.z   = sqrt(saturate(1.0 - dot(n.xy, n.xy)));
            return n;
        }

        half3 GLSA_DecodeNormal(half4 packed, half mode, half flipY, half scale, half swap, half useMap)
        {
            // useMap 已经在调用处和「是否挂图」取过交集，0 就直接平法线
            if (useMap < 0.5)
            {
                return half3(0, 0, 1);
            }

            half3 n;
            if (mode > 1.5)
            {
                // Unity 自己压过的法线：不开 DXT5nm 的平台是 RGB/BC5，开了是 X=A,Y=G
                #if defined(UNITY_NO_DXT5nm)
                    n = GLSA_UnpackRGB(packed, scale);
                #else
                    n = GLSA_UnpackTwoChannel(packed, packed.wy, scale);
                #endif
            }
            else if (mode > 0.5)
            {
                // 原始 DXT5nm 通道，Z 用单位球重建
                half2 xy = lerp(packed.wy, packed.ga, swap);   // wy = (A,G) / ga = (G,A)
                n = GLSA_UnpackTwoChannel(packed, xy, scale);
            }
            else
            {
                // 标准切线空间 RGB（GLSA 的 BC7）
                n = GLSA_UnpackRGB(packed, scale);
            }

            n.y = lerp(n.y, -n.y, flipY);

            // 兜底：解出来是零向量（比如灰度默认贴图走 RGB 分支）就回退到平法线
            half len = length(n);
            n = (len > 1e-5h) ? (n / len) : half3(0, 0, 1);
            return n;
        }

        // 由切线空间转到世界空间
        half3 GLSA_TangentToWorld(half3 normalTS, half3 normalWS, half4 tangentWS)
        {
            half3 bitangent = cross(normalWS, tangentWS.xyz) * tangentWS.w;
            half3x3 tbn     = half3x3(tangentWS.xyz, bitangent, normalWS);
            return normalize(mul(normalTS, tbn));
        }
        ENDHLSL

        // ====================================================================
        //  ForwardLit
        // ====================================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   LitPassVertex
            #pragma fragment LitPassFragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            // URP 14 把软阴影拆成四档，只写 _SHADOWS_SOFT 的话
            // URP asset 里的 Soft Shadow Quality（Low/Medium/High）对这个材质不生效。
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            // ---- 烘焙 GI（lightmap）----
            // 少了这四行，物体勾了 Static、场景烘了光照贴图也进不来：
            // bakedGI 只会走 SampleSH 拿探针球谐，看起来跟没烘一样。
            //   LIGHTMAP_ON            : 有静态光照贴图
            //   DIRLIGHTMAP_COMBINED   : 方向性光照贴图（Baked Directional）
            //   LIGHTMAP_SHADOW_MIXING : 烘焙阴影和实时阴影混合（Mixed 光照）
            //   SHADOWS_SHADOWMASK     : Shadowmask 模式的烘焙阴影遮罩
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // 主光 cookie（方向光挂的云影贴图）。
            // 少了这一行，URP 的 RealtimeLights.hlsl 里 #if defined(_LIGHT_COOKIES) 永远不成立，
            // SampleMainLightCookie 整段被编译掉——灯上挂了 cookie 也收不到，而且不会有任何报错。
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                // 烘焙光照贴图用的是第二套 UV（Unity 烘焙时生成的 lightmap UV）
                float4 uv1        : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3  normalWS   : TEXCOORD2;
                half4  tangentWS  : TEXCOORD3;
                half3  viewDirWS  : TEXCOORD4;
                half   fogFactor  : TEXCOORD5;
                // LIGHTMAP_ON 时 xy = lightmap UV；否则 xyz = 顶点球谐（探针 GI）
                float4 lightmapUVOrVertexSH : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS   = nrmInputs.normalWS;
                output.tangentWS  = half4(nrmInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.viewDirWS  = GetWorldSpaceViewDir(posInputs.positionWS);
                output.uv         = input.uv;
                output.fogFactor  = ComputeFogFactor(posInputs.positionCS.z);
                // 两条都写上，宏内部按 LIGHTMAP_ON 各自决定走哪个
                OUTPUT_LIGHTMAP_UV(input.uv1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
                OUTPUT_SH(output.normalWS, output.lightmapUVOrVertexSH.xyz);
                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap));
                // 没挂 SMO 图时 Unity 补白色，R=1 G=1 会变成全金属全光滑的镜子；
                // 所以空槽回退到 S=0 M=0 AO=1 EmisMask=0
                half4 smoRaw    = SAMPLE_TEXTURE2D(_SMO, sampler_SMO, TRANSFORM_TEX(input.uv, _SMO));
                half4 smo       = lerp(half4(0.0h, 0.0h, 1.0h, 0.0h), smoRaw, GLSA_HasSMOTex());
                half4 normalTex = SAMPLE_TEXTURE2D(_NRMTex, sampler_NRMTex, TRANSFORM_TEX(input.uv, _NRMTex));

                // Albedo
                half3 albedo = albedoTex.rgb * _BaseColor.rgb;
                albedo = lerp(albedo, _LerpColor.rgb, _LerpWeight);

                #ifdef _ALPHATEST_ON
                    clip(albedoTex.a * _BaseColor.a - _Cutoff);
                #endif

                // SMO
                half smoothness = saturate(smo.r * _Smoothness);
                half metallic   = saturate(smo.g * _Metallic);
                half occlusion  = lerp(1.0h, smo.b, _OcclusionStrength);
                half emisMask   = smo.a;

                // Normal
                half3 normalTS = GLSA_DecodeNormal(normalTex, _NormalMode, _NormalFlipY, _BumpScale, _DXT5nmSwap, min(_UseNormalMap, GLSA_HasNormalTex()));
                half3 normalWS = GLSA_TangentToWorld(normalTS, input.normalWS, input.tangentWS);

                // Emission：SMO 的 A 通道当强度，发光颜色直接用基础色（Albedo × BaseColor）
                // _EmissionSize <= 0 就是完全没有自发光，不再单独给关闭开关
                half3 emission = albedo * (_EmissionSize * emisMask);
                emission += _TutorialColor.rgb * _Tutorial;
                emission += _AoeSelectColor.rgb * _AoeSelect;

                // ---- InputData ----
                InputData inputData      = (InputData)0;
                inputData.positionWS     = input.positionWS;
                inputData.positionCS     = input.positionCS;
                inputData.normalWS       = normalWS;
                inputData.viewDirectionWS = normalize(input.viewDirWS);
                inputData.shadowCoord    = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord       = input.fogFactor;
                inputData.vertexLighting = half3(0, 0, 0);

                // 烘焙 GI：烘过 lightmap 就直接吃贴图（方向性贴图由 SampleLightmap 内部按
                // DIRLIGHTMAP_COMBINED 处理），没烘过才退回探针球谐。
                #ifdef LIGHTMAP_ON
                    inputData.bakedGI = SampleLightmap(input.lightmapUVOrVertexSH.xy, normalWS);
                #else
                    inputData.bakedGI = SampleSH(normalWS);
                #endif

                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                // Shadowmask 模式的烘焙阴影：只有 Mixed 光照 + Shadowmask 时才真有值，
                // 其余情况保持全白（不遮）。
                #if defined(LIGHTMAP_ON) && defined(SHADOWS_SHADOWMASK)
                    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
                #else
                    inputData.shadowMask = half4(1, 1, 1, 1);
                #endif

                // ---- SurfaceData ----
                SurfaceData surfaceData      = (SurfaceData)0;
                surfaceData.albedo           = albedo;
                surfaceData.alpha            = 1.0h;
                surfaceData.metallic         = metallic;
                surfaceData.smoothness       = smoothness;
                surfaceData.occlusion        = occlusion;
                surfaceData.emission         = emission;
                surfaceData.normalTS         = normalTS;
                surfaceData.specular         = half3(0, 0, 0);
                surfaceData.clearCoatMask    = 0;
                surfaceData.clearCoatSmoothness = 0;

                // SSAO：只有 URP Renderer 里打开了 Screen Space Ambient Occlusion，
                // _SCREEN_SPACE_OCCLUSION 关键字才会生效；没开的话这段整个被编译掉，零开销。
                // 不开这一段的话，上面那句 multi_compile 等于白声明，SSAO 永远进不来。
                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
                    surfaceData.occlusion = min(surfaceData.occlusion, aoFactor.indirectAmbientOcclusion);
                #endif

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a   = 1.0h;
                return color;
            }
            ENDHLSL
        }

        // ====================================================================
        //  ShadowCaster
        // ====================================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex   ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(input.normalOS);
                float3 lightDir   = _LightDirection;

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDir));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                output.uv         = input.uv;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                #ifdef _ALPHATEST_ON
                    half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap)).a * _BaseColor.a;
                    clip(a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        // ====================================================================
        //  DepthOnly
        // ====================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex   DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv         = input.uv;
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                #ifdef _ALPHATEST_ON
                    half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap)).a * _BaseColor.a;
                    clip(a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        // ====================================================================
        //  DepthNormals（SSAO / 深度法线纹理）
        // ====================================================================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex   DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                half3  normalWS   : TEXCOORD1;
                half4  tangentWS  : TEXCOORD2;
            };

            Varyings DepthNormalsVertex(Attributes input)
            {
                Varyings output;
                VertexNormalInputs nrm = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv         = input.uv;
                output.normalWS   = nrm.normalWS;
                output.tangentWS  = half4(nrm.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                return output;
            }

            half4 DepthNormalsFragment(Varyings input) : SV_Target
            {
                #ifdef _ALPHATEST_ON
                    half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap)).a * _BaseColor.a;
                    clip(a - _Cutoff);
                #endif

                half4 normalTex = SAMPLE_TEXTURE2D(_NRMTex, sampler_NRMTex, TRANSFORM_TEX(input.uv, _NRMTex));
                half3 normalTS  = GLSA_DecodeNormal(normalTex, _NormalMode, _NormalFlipY, _BumpScale, _DXT5nmSwap, min(_UseNormalMap, GLSA_HasNormalTex()));
                half3 normalWS  = GLSA_TangentToWorld(normalTS, input.normalWS, input.tangentWS);

                return half4(NormalizeNormalPerPixel(normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"

    // 中文材质面板：ShaderLab 的 drawer 参数不能写中文，所以整套 GUI 放在 C# 里
    CustomEditor "GLSAPBRGUI"
}
