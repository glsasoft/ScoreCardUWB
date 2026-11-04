Shader "Effect/DepthDecal_new"
{
    Properties
    {
        _BaseColor("Base Color",color) = (1,1,1,1)
        _BaseMap("BaseMap", 2D) = "white" {}
        //////////
        _Height("_Height",float) = 0
         _Mask("_Mask", 2D) = "white" {}
    /////////////////
        }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
ZWrite Off
        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                 float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionVS   : TEXCOORD2;
                float4 positionOS   : TEXCOORD3;
                /////////////
                float4 scrPos : TEXCOORD4;
                float2 offset : TEXCOORD5;
            
        ////////////////
    };

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float4 _BaseMap_ST;
            float _Height;
            CBUFFER_END
            TEXTURE2D (_BaseMap);//SAMPLER(sampler_BaseMap);
           // TEXTURE2D (_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            #define smp _linear_clamp
            ///////////
             TEXTURE2D (_Mask);
             SAMPLER(sampler_Mask);

//////////////
             SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                //////////////////////////////
                //将视角向量变换到切线空间
                	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - TransformObjectToWorld(v.positionOS.xyz));
                
                
                float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;   
                float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal );
              	float2 BumpOffset = mul(rotation , viewDir).xy * _Height;  
                
                	//计算UV信息
                	o.offset = BumpOffset;
                //////////////////////////////////////////
                o.positionVS = TransformWorldToView(TransformObjectToWorld(v.positionOS.xyz));
                o.positionOS = v.positionOS;
                o.scrPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

float SampleSceneDepth(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv)).r;
}

            half4 frag(Varyings i) : SV_Target
            {
                // 思路
                // 通过深度图求出像素所在的观察空间所在的z值
                // 通过当前渲染的面片求出像素在观察空间下的坐标
                // 在通过以上两者求出深度图中的像素的xyz坐标
                // 再将此坐标转换片面模型的本地空间，吧XY当做UV来进行进行纹理采样


                //深度图采样
                float2 screenUV = i.scrPos/i.scrPos.w;
              //  half4 depthMap = SAMPLE_TEXTURE2D(_CameraDepthAttachment, sampler_CameraDepthAttachment, screenUV);
                 float depthMap = SampleSceneDepth(screenUV);
                float depth = LinearEyeDepth(depthMap, _ZBufferParams);
               //  return frac(depth);
                float4 depthVS = 1;
                depthVS.z = depth;
                depthVS.xy = i.positionVS.xy*depthVS.z/-i.positionVS.z;
                // return depthVS.x;
                float3 depthWS = mul(unity_CameraToWorld,depthVS);
                // return depthWS.x;
                float3 depthOS = mul(unity_WorldToObject,float4(depthWS,1));
                float2 uv =  depthOS.xy + 0.5; 
                // return uv.y;
                // return depthOS.y;
                half4 c;
                // half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, depthOS.xy);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + i.offset);
     half Mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv ).r;
   
                c = baseMap * _BaseColor;
    c.a = Mask;
                // c.rgb = MixFog(c.rgb, i.fogCoord);
                return  c;
            }
            ENDHLSL
        }
    }



}
