Shader "Effect/HollowEffect"
{
    Properties
    {
     
       [Enum(UnityEngine.Rendering.CullMode)]_Cull("剔除模式 (默认剔除背面)", Float) = 2
       
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0.0
        [Toggle]_ZWrite("深度写入 (ZWrite) (默认开启)", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("深度测试 (ZTest) (默认小于等于)", Float) = 4 // 4 代表 LEqual (小于或等于)
        [Header(BaseColor)]
        [Space(10)]
        _BaseMap ("Albedo Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMapValue("Base Map Value", Range(0, 10)) = 1
        _BaseMapSaturation("Base Map Saturation", Range(0, 5)) = 1
        _BaseColorIntensity ("Base Color Intensity", Range(0, 5)) = 1
        _EmissionMap ("Emission Map", 2D) = "black" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 0
        
      
        
        [Header(Hollow)]
        _RisAlpha ("R is Alpha", Float) = 0
        _FlowSpeed ("Flow Speed", Range(0, 100)) = 1
        _HollowDirection_X ("Hollow Direction X", Vector) = (1, 0, 0, 0)
        _HollowDirection_Y ("Hollow Direction Y", Vector) = (0, 1, 0, 0)
        
        
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float _Unlit;
        float _Queue;
        float _Static;
        
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float _BaseMapValue, _BaseMapSaturation, _BaseColorIntensity;
        float _EmissionIntensity;
        float4 _EmissionColor;
        
      

        float _Hollow;
        float _RisAlpha;
        float _FlowSpeed;
        float4 _HollowDirection_X;
        float4 _HollowDirection_Y;

       
        CBUFFER_END
        
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(_DissolveMap);
        SAMPLER(sampler_DissolveMap);
        
        ENDHLSL
        
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
           
            
            Cull [_Cull]
            ZWrite[_ZWrite]
            Blend [_SrcBlend] [_DstBlend]
            ZTest [_ZTest]
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

          
           
            
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float4 lightMapUV : TEXCOORD1;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
              
                float4 directionFactor : TEXCOORD6;
            };
            
     
           
            v2f vert(a2v IN)
            {
                v2f OUT;
                VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.vertex.xyz);
                VertexNormalInputs vertex_normal_inputs = GetVertexNormalInputs(IN.normal.xyz, IN.tangent);
               
             
                OUT.directionFactor.x = dot(_HollowDirection_X, IN.vertex);
                OUT.directionFactor.y = dot(_HollowDirection_Y, IN.vertex);
               OUT.position = vertex_position_inputs.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                
                return OUT;
            }

            float4 frag(v2f IN): SV_Target
            {

               // return 0.5;
                
                float2 uv =float2(IN.directionFactor.x - _Time.y * _FlowSpeed, IN.directionFactor.y);
                
                
                float4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                float3 valueBaseTexColor = baseTexColor.rgb * _BaseMapValue;
                float3 minSaturationColor = 0.2125 * baseTexColor.r + 0.7154 * baseTexColor.g + 0.0721 * baseTexColor.b;
                float3 baseColor = lerp(minSaturationColor, valueBaseTexColor, _BaseMapSaturation) * _BaseColorIntensity * _BaseColor;

               
                
                //PBR Params
                float3 emColor = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor.rgb * _EmissionIntensity;
              
                
             
                float finalAlpha = baseTexColor.a * _BaseColor.a;
                finalAlpha = lerp(baseTexColor.r * _BaseColor.a, finalAlpha, step(_RisAlpha, 0));

             
                //Hollow Color
                float3 hollowFinalColor = saturate(baseColor.rgb) + emColor;
                
                //Final Color
                float3 finalColor = hollowFinalColor;

               
                
                return float4(finalColor, finalAlpha);
            }
            
            ENDHLSL
        }
        
        
    }
    //CustomEditor "PBRSceneGUI_V4"
}
