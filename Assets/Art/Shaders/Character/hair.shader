Shader "yu/hair"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Vector ("vector",vector) = (1,1,1,1)
        _Vector2 ("vector2",vector) = (1,1,1,1)
        _NoiseTex ("扰动贴图",2D) = "black"{}
        _Color ("头发颜色",Color) = (1,1,1,1)
        _Color2 ("头发颜色2",Color) = (0,0,0,0)
        _Color3 ("高光颜色1",Color) = (0,0,0,0)
        _Color4 ("高光颜色2",Color) = (0,0,0,0)
        _AoInt ("Ao强度",range(0,1)) = 0.5
        _AtenInt("高光衰减强度",range(0,1)) = 0.5
        _Clip ("cilp",range(0,1)) = 0
    } 
    SubShader
    {
        Tags { "RenderType"="Transparent""Queue" = "Transparent" }
        LOD 100
        //  "Queue" = "Transparent"
      

        Pass
        {
            // Tags{"Queue" = "Transparent"}
            name "2"
            Blend one OneMinusSrcAlpha
            Cull off
            // ZWrite off
            // ZTest Less
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase

            struct appdata
            {
                float4 vertex    : POSITION;
                float2 uv        : TEXCOORD0;
                float4 tangent   : TANGENT;
                float4 normal    : NORMAL;
                float4 color     : COLOR;
            };

            struct v2f
            {
                float4 uv        : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 pos    : SV_POSITION;
                float3 tangentWs : TEXCOORD2;
                float3 worldPos  : TEXCOORD3;
                float3 tan       : TEXCOORD4;
                float3 normalWs  : TEXCOORD5;
                float3 Color     : TEXCOORD6;
                UNITY_SHADOW_COORDS(7)
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST,_NoiseTex_ST;
            float4 _Vector,_Vector2;
            float4 _Color,_Color2,_Color3,_Color4;
            float _AoInt;
            float _AtenInt;
            float _Clip;

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _NoiseTex);
                
                o.tangentWs = mul(unity_ObjectToWorld,v.tangent);
                o.worldPos = mul(unity_ObjectToWorld,v.vertex);
                o.normalWs = UnityObjectToWorldNormal(v.normal);
                o.tan = normalize(cross(o.tangentWs.xyz,o.normalWs.xyz) * v.tangent.w);
                o.Color = v.color.rgb;
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_SHADOW(o)
                return o;
            }

            float KaijiyaKay (float spePow,float speInt,float noiseInt,float shiftInt,float attenInt,float3 T,float3 H,float3 N,float3 V,float Noise)
            {
                half shift = shiftInt + Noise * noiseInt;
                T = normalize(T + shift * H);
                float dotTH = dot(T,H);
                float atten = lerp(1,max(0,dot(N,V)),attenInt);
                atten *= atten;
                float sinTH = pow(sqrt(1-dotTH*dotTH),spePow);
                float specular = sinTH * atten * speInt;
                return specular;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                col.a = smoothstep(0,_Clip,col.a);
                // return col.a;
                // clip(col.a-_Clip);
                float var_NoiseTex = tex2D(_NoiseTex,i.uv.zw).r;
                // return lerp(1,var_NoiseTex,0.1);
                float noise = var_NoiseTex -0.5;
                float3 N = i.normalWs;
                float3 T = i.tangentWs;
                float3 L = _WorldSpaceLightPos0;
                float3 V = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                float3 H = normalize(L + V);


                //漫反射
                half3 diffuse = 0;
                float NDotL = dot(N,L)*0.4 + 0.6;
                // return dot(N,V);
                float3 difCol = lerp(_Color2.rgb,_Color,NDotL) * lerp(1,var_NoiseTex,0.2);
                diffuse = NDotL * difCol;
                // return float4(diffuse,1);

                //各向异性
                half3 specular = 0;
                float specular01 = KaijiyaKay(_Vector.x,_Vector.w,_Vector.z,_Vector.y,_AtenInt,T,H,N,V,noise);//主高光
                float specular02 = KaijiyaKay(_Vector2.x,_Vector2.w,_Vector2.z,_Vector2.y,_AtenInt,T,H,N,V,noise);// 第二高光
                specular = specular01 * _Color3 + specular02 * _Color4;
                specular *= NDotL;//总高光

                //AO                
                half3 ao = 1;
                ao = lerp(1,i.Color*difCol,_AoInt);
                // return float4(ao,1);

                //阴影
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
                // return atten;
                //最终颜色
                float4 c = 1;
                c.rgb = ao*(specular + diffuse) ;
                // sample the texture
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return float4(c.rgb* col.a,col.a);
            }
            ENDCG
        }
        
    }
    FallBack "Diffuse"
}
