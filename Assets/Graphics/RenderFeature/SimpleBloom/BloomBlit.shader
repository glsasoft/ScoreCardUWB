Shader "Unlit/BloomBlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        int downSampleRadius;
        float downSampleSigma;
        int upSampleRadius;
        float upSampleSigma;
        float bloomIntensityEffect;
        float bloomIntensityScene;
        float4 bloomColorEffect;
        float4 bloomColorScene;
        float4 _BloomThreshold;
        CBUFFER_END

        sampler2D _BloomTex;
        
        TEXTURE2D(_CameraOpaqueTexture);
        SAMPLER(sampler_CameraOpaqueTexture);
        
        float GaussWeight2D(float x,float y,float sigma)
        {
            float E = 2.71828182846;
            float sigma_2 = pow(sigma,2);

            float a = -(x*x + y*y) / (2.0 * sigma_2);
            return pow(E,a) / (2.0 * PI * sigma_2);
        }

        float4 GaussNxN(sampler2D tex,float2 uv,int n,float2 stride,float sigma)
        {
            float4 color = float4(0,0,0, 0);
            int r = n / 2;
            float weight = 0.0;

            for (int i = -r; i <= r; i++) {
                for (int j = -r; j <= r; j++) {
                    float2 coord = uv + float2(i,j) * stride;
                    float4 c = tex2D(tex,coord);
                    color.a = c.a;
                    float luma = dot(float3(0.2126,0.7152,0.0722), c.rgb);

                    float w1 = GaussWeight2D(i,j,sigma);
                    float w2 = 1.0 / (1.0 + luma) * w1;
                    color.rgb += c.rgb * w2;
                    weight += w2;
                }
            }

            color.rgb /= weight;
            return color;
        }

        ENDHLSL

        // Donwsize And Blur
        // DownSample
        // 0
        Pass
        {
            ColorMask RGBA
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 color = GaussNxN(_MainTex, i.uv, downSampleRadius, _MainTex_TexelSize.xy, downSampleSigma);
                return color;
            }
            ENDHLSL
        }

        // UpSize And Blur
        // UpSample
        // 1
        Pass
        {
            ColorMask RGBA
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _PrewMip;
            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 prev_stride = 0.5 * _MainTex_TexelSize.xy;
                float2 curr_stride = _MainTex_TexelSize.xy;

                half4 color = GaussNxN(_PrewMip, i.uv, upSampleRadius, prev_stride, upSampleSigma)
                                + GaussNxN(_MainTex, i.uv, upSampleRadius, curr_stride, upSampleSigma);
                return color;
            }
            ENDHLSL
        }

        // Output
        // 2
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _EffectBloomTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 effectColor = tex2D(_EffectBloomTex, i.uv);
                float4 color = tex2D(_MainTex, i.uv);
                return float4(saturate((color.rgb + effectColor.rgb) * (1 - effectColor.a) + effectColor.rgb * effectColor.a), 1);
            }
            ENDHLSL
        }


        // Filter And Donwsize And Blur
        // First DownSample
        // 3
        Pass
        {
            ColorMask RGBA
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            int firstDownSampleRadius;
            float firstDownSampleSigma;
            
            half4 frag (v2f i) : SV_Target
            {
                // sample the texture

                half4 color = GaussNxN(_MainTex, i.uv, firstDownSampleRadius, _MainTex_TexelSize.xy, firstDownSampleSigma);
                return color;
            }
            ENDHLSL
        }

        // Mask
        // 4
        Pass
        {
            ColorMask RGBA
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float3 ApplyBloomThreshold(float3 color) {
                float brightness = Max3(color.r, color.g, color.b);  // 取最大亮度分量
                float soft = brightness + _BloomThreshold.y;         // 软过渡起点
                soft = clamp(soft, 0.0, _BloomThreshold.z);          // 限制软过渡范围
                soft = soft * soft * _BloomThreshold.w;              // 平方计算曲率
                float contribution = max(soft, brightness - _BloomThreshold.x); // 综合软硬阈值
                contribution /= max(brightness, 0.00001);            // 归一化
                return color * contribution;                         // 输出加权颜色
            }
            
            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 color = tex2D(_MainTex,i.uv);
                float3 bloomColor = ApplyBloomThreshold(color.rgb);
                return float4(bloomColor, color.a);
            }
            ENDHLSL
        }

        // Merge Effect
        // 5
        Pass
        {
            ColorMask RGBA
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 bloomColor = tex2D(_BloomTex, i.uv) * bloomIntensityEffect * bloomColorEffect;
                float4 color = tex2D(_MainTex, i.uv);
                return float4(saturate(color.rgb + bloomColor.rgb), color.a);
            }
            ENDHLSL
        }

        // Merge Scene
        // 6
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 bloomColor = tex2D(_BloomTex, i.uv) * bloomIntensityScene * bloomColorScene;
                float4 color = tex2D(_MainTex, i.uv);
                return float4(saturate(color.rgb + bloomColor.rgb), 1);
            }
            ENDHLSL
        }
    }
}
