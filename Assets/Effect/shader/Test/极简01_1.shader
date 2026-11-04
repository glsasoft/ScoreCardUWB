// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Test/极简01_1"
{
	Properties
	{
		[Enum(add,1,blend,10)]_Blendmode("混合模式", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_Float22("剔除模式", Float) = 0
		_Float6("整体Alpha强度", Float) = 1
		_Float5("软粒子（相机需开启投影）", Float) = 0
		[Toggle]_Float85("深度写入（模型特效适用）", Float) = 0
		[Enum(open,0,close,4)]_Float18("显示最前面", Float) = 4
		[Main(z7,_KEYWORD,off,off)]_Float10("Custom列表", Float) = 0
		[SubToggle(z7,_)]_Float4("custom1XY控制主贴图uv", Float) = 0
		[SubToggle(z7,_)]_Float38("custom1Z控制溶解程度", Float) = 0
		[SubToggle(z7,_)]_Float9("Costom1W控制颜色强度", Float) = 1
		[SubToggle(z7,_)]_Float75("Custom2XY控制溶解贴图uv", Float) = 0
		[SubToggle(z7,_)]_Float8("Custom2XY控制附加溶解贴图uv", Float) = 0
		[SubToggle(z7,_)]_Float11("Custom2Z控制centerGlow（遮罩开关全部打开）", Float) = 0
		[Main(z1,_KEYWORD,off,off)]_Float24("主贴图列表", Float) = 0
		[HDR][Sub(z1)]_Color0("颜色", Color) = (1,1,1,1)
		[HDR][Sub(z1)]_Color5("次颜色", Color) = (1,0,0,1)
		[Sub(z1)]_power("渐变颜色power", Float) = 0
		[KWEnum(z1,U,_0,V,_1)]_Float64("渐变方向", Float) = 0
		[Sub(z1)]_Float7("颜色强度", Float) = 1
		[Sub(z1)]_Main("主贴图", 2D) = "white" {}
		[KWEnum(z1,RGB,_0,A,_1)]_Float0("主贴图通道", Float) = 0
		[SubToggle(z1,_)]_Float65("主贴图去色", Float) = 0
		[KWEnum(z1,off,_0,on,_1)]_Float1("主贴图xclamp", Float) = 0
		[KWEnum(z1,off,_0,on,_1)]_Float2("主贴图yclamp", Float) = 0
		[Sub(z1)]_Float66("主贴图强度", Float) = 1
		[Sub(z1)]_Float3("主贴图power", Float) = 1
		[Sub(z1)]_Float20("主贴图旋转", Range( 0 , 360)) = 0
		[Sub(z1)]_Vector4("主贴图UV", Vector) = (0,0,0,0)
		[Main(z6,_KEYWORD,off,off)]_Float31("高光列表", Float) = 0
		[Sub(z6)]_Color1("高光颜色", Color) = (1,1,1,1)
		[Sub(z6)]_Float17("高光细节", Float) = 1
		[Sub(z6)]_Float19("高光强度", Float) = 0
		[Main(z2,_KEYWORD,off,off)]_Float25("遮罩列表", Float) = 0
		[SubToggle(z2,_)]_Float61("遮罩开关", Float) = 0
		[Sub(z2)]_Mask("遮罩贴图", 2D) = "white" {}
		[KWEnum(z2,RGB,_0,A,_1)]_Float14("遮罩通道", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float41("遮罩xclamp", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float42("遮罩yclamp", Float) = 0
		[Sub(z2)]_Float43("遮罩强度", Float) = 1
		[Sub(z2)]_Float16("遮罩power", Float) = 1
		[Sub(z2)]_Float15("遮罩图旋转", Range( 0 , 360)) = 0
		[Sub(z2)]_Vector3("遮罩uv", Vector) = (0,0,0,0)
		[SubToggle(z2,_)]_Float62("附加遮罩开关", Float) = 0
		[Sub(z2)]_Mask1("m遮罩贴图（CenterGlow）", 2D) = "white" {}
		[KWEnum(z2,RGB,_0,A,_1)]_Float50("m通道", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float45("mxclamp", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float46("myclamp", Float) = 0
		[Sub(z2)]_Float48("m强度", Float) = 1
		[Sub(z2)]_Float47("mpower", Float) = 1
		[Sub(z2)]_Float44("m旋转", Range( 0 , 360)) = 0
		[Sub(z2)]_Vector6("muv", Vector) = (0,0,0,0)
		[Main(z3,_KEYWORD,off,off)]_Float26("扰动列表", Float) = 0
		[SubToggle(z3,_)]_Float12("扰动开关", Float) = 0
		[Sub(z3)]_Noise("扰动贴图", 2D) = "white" {}
		[SubToggle(z3,_)]_Float72("扰动主贴图", Float) = 0
		[SubToggle(z3,_)]_Float73("扰动遮罩", Float) = 0
		[SubToggle(z3,_)]_Float74("扰动附加遮罩", Float) = 0
		[SubToggle(z3,_)]_Float96("扰动两张溶解", Float) = 0
		[SubToggle(z3,_)]_Float99("扰动叠加贴图", Float) = 0
		[Sub(z3)]_Float13("扰动强度", Float) = 0
		[Sub(z3)]_Vector2("扰动uv", Vector) = (0,0,0,0)
		[Main(z4,_KEYWORD,off,off)]_Float27("溶解列表", Float) = 0
		[SubToggle(z4,_)]_Float32("溶解开关", Float) = 0
		[Sub(z4)]_Dissolve("溶解贴图", 2D) = "white" {}
		[Sub(z4)]_Float70("溶解贴图旋转", Range( 0 , 360)) = 0
		[Sub(z4)]_Float69("溶解贴图强度", Float) = 1
		[Sub(z4)]_Float28("溶解程度", Range( -0.2 , 1.2)) = -0.2
		[Sub(z4)]_Float29("软（左）硬（右）程度", Range( 0 , 0.5)) = 0.5
		[Sub(z4)]_Color2("外层亮边颜色", Color) = (1,1,1,1)
		[Sub(z4)]_Float37("外层亮边颜色强度", Float) = 1
		[Sub(z4)]_Float79("外层亮边宽度", Float) = 0
		[Sub(z4)]_Color3("内层亮边2颜色", Color) = (1,1,1,1)
		[Sub(z4)]_Float39("内层亮边2颜色强度", Float) = 1
		[Sub(z4)]_Float36("内层亮边宽度", Float) = 0
		[Sub(z4)]_Vector5("溶解贴图uv", Vector) = (0,0,0,0)
		[SubToggle(z4,_)]_Float55("附加溶解贴图开关", Float) = 0
		[Sub(z4)]_Dissolve1("附加溶解贴图", 2D) = "white" {}
		[Sub(z4)]_Float71("附加溶解贴图旋转", Range( 0 , 360)) = 0
		[Sub(z4)]_Float30("溶解纹理偏向", Range( 0 , 1)) = 0
		[Sub(z4)]_Float541111111111111111111("附加溶解贴图强度", Float) = 1
		[Sub(z4)]_aegrdfgaergare("附加溶解贴图uv", Vector) = (0,0,0,0)
		[SubToggle(z4,_)]_Float97("方向溶解开关", Float) = 0
		[Sub(z4)]_Float95("方向溶解旋转（默认左黑右白）", Range( 0 , 360)) = 0
		[Sub(z4)]_Float93("方向U偏移", Float) = 0
		[Sub(z4)]_Float94("方向V偏移", Float) = 0
		[Main(z5,_KEYWORD,off,off)]_Float40("叠加列表", Float) = 0
		[SubToggle(z5,_)]_Float60("叠加开关", Float) = 0
		[Sub(z5)]_TextureSample0("叠加贴图", 2D) = "white" {}
		[HDR][Sub(z5)]_Color4("叠加贴图颜色", Color) = (1,1,1,1)
		[KWEnum(z5,RGB,_0,A,_1)]_Float57("叠加贴图通道", Float) = 0
		[KWEnum(z5,Add,_0,Mul,_1)]_Float59("A通道算法（Add时为叠加）", Float) = 0
		[Sub(z5)]_Float63("叠加贴图强度", Float) = 1
		[Sub(z5)]_Float54("叠加贴图power", Float) = 1
		[Sub(z5)]_Float56("叠加旋转", Range( 0 , 360)) = 0
		[ASEEnd][Sub(z5)]_Vector7("叠加uv", Vector) = (0,0,0,0)

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha [_Blendmode]
		AlphaToMask Off
		Cull [_Float22]
		ColorMask RGBA
		ZWrite [_Float85]
		ZTest [_Float18]
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_COLOR


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Float85;
			uniform float _Float40;
			uniform float _Blendmode;
			uniform float _Float31;
			uniform float _Float22;
			uniform float _Float10;
			uniform float _Float24;
			uniform float _Float18;
			uniform float _Float27;
			uniform float _Float26;
			uniform float _Float25;
			uniform sampler2D _Main;
			uniform float4 _Main_ST;
			uniform float _Float4;
			uniform float _Float20;
			uniform float2 _Vector4;
			uniform float _Float1;
			uniform float _Float2;
			uniform sampler2D _Noise;
			uniform float4 _Noise_ST;
			uniform float2 _Vector2;
			uniform float _Float12;
			uniform float _Float13;
			uniform float _Float72;
			uniform float _Float65;
			uniform float _Float66;
			uniform float _Float3;
			uniform sampler2D _Mask;
			uniform float _Float73;
			uniform float4 _Mask_ST;
			uniform float _Float15;
			uniform float2 _Vector3;
			uniform float _Float41;
			uniform float _Float42;
			uniform float _Float16;
			uniform float _Float43;
			uniform sampler2D _Mask1;
			uniform float _Float74;
			uniform float4 _Mask1_ST;
			uniform float _Float44;
			uniform float2 _Vector6;
			uniform float _Float45;
			uniform float _Float46;
			uniform float _Float47;
			uniform float _Float48;
			uniform float _Float62;
			uniform float _Float11;
			uniform float _Float61;
			uniform float _Float29;
			uniform sampler2D _Dissolve;
			uniform float4 _Dissolve_ST;
			uniform float _Float75;
			uniform float _Float70;
			uniform float2 _Vector5;
			uniform float _Float96;
			uniform float _Float69;
			uniform sampler2D _Dissolve1;
			uniform float4 _Dissolve1_ST;
			uniform float _Float8;
			uniform float _Float71;
			uniform float2 _aegrdfgaergare;
			uniform float _Float541111111111111111111;
			uniform float _Float55;
			uniform float _Float30;
			uniform float _Float93;
			uniform float _Float94;
			uniform float _Float95;
			uniform float _Float97;
			uniform float _Float28;
			uniform float _Float38;
			uniform float _Float79;
			uniform float _Float32;
			uniform float4 _Color5;
			uniform float4 _Color0;
			uniform float _Float64;
			uniform float _power;
			uniform float _Float7;
			uniform float _Float9;
			uniform float _Float37;
			uniform float4 _Color2;
			uniform float _Float36;
			uniform float _Float39;
			uniform float4 _Color3;
			uniform float _Float17;
			uniform float _Float19;
			uniform float4 _Color1;
			uniform float4 _Color4;
			uniform sampler2D _TextureSample0;
			uniform float4 _TextureSample0_ST;
			uniform float _Float56;
			uniform float2 _Vector7;
			uniform float _Float99;
			uniform float _Float54;
			uniform float _Float63;
			uniform float _Float60;
			uniform float _Float57;
			uniform float _Float0;
			uniform float _Float59;
			uniform float _Float6;
			uniform float _Float14;
			uniform float _Float50;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _Float5;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord4 = screenPos;
				
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				o.ase_texcoord2 = v.ase_texcoord1;
				o.ase_texcoord3 = v.ase_texcoord2;
				o.ase_color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_Main = i.ase_texcoord1.xy * _Main_ST.xy + _Main_ST.zw;
				float4 texCoord687 = i.ase_texcoord2;
				texCoord687.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult691 = (float2(texCoord687.x , texCoord687.y));
				float2 lerpResult692 = lerp( float2( 0,0 ) , appendResult691 , _Float4);
				float cos648 = cos( radians( _Float20 ) );
				float sin648 = sin( radians( _Float20 ) );
				float2 rotator648 = mul( ( uv_Main + lerpResult692 ) - float2( 0.5,0.5 ) , float2x2( cos648 , -sin648 , sin648 , cos648 )) + float2( 0.5,0.5 );
				float2 temp_output_650_0 = ( rotator648 + ( _Vector4 * _Time.y ) );
				float temp_output_698_0 = (temp_output_650_0).x;
				float lerpResult656 = lerp( temp_output_698_0 , saturate( temp_output_698_0 ) , _Float1);
				float temp_output_699_0 = (temp_output_650_0).y;
				float lerpResult658 = lerp( temp_output_699_0 , saturate( temp_output_699_0 ) , _Float2);
				float2 appendResult642 = (float2(lerpResult656 , lerpResult658));
				float2 uv_Noise = i.ase_texcoord1.xy * _Noise_ST.xy + _Noise_ST.zw;
				float4 tex2DNode745 = tex2D( _Noise, ( uv_Noise + ( _Vector2 * _Time.y ) ) );
				float2 appendResult1005 = (float2(tex2DNode745.r , tex2DNode745.g));
				float2 lerpResult748 = lerp( float2( 0,0 ) , ( appendResult1005 * tex2DNode745.a ) , _Float12);
				float2 temp_output_750_0 = ( lerpResult748 * _Float13 );
				float2 lerpResult1044 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float72);
				float4 tex2DNode619 = tex2D( _Main, ( appendResult642 + lerpResult1044 ) );
				float grayscale1024 = (tex2DNode619.rgb.r + tex2DNode619.rgb.g + tex2DNode619.rgb.b) / 3;
				float4 temp_cast_1 = (grayscale1024).xxxx;
				float4 lerpResult1025 = lerp( tex2DNode619 , temp_cast_1 , _Float65);
				float4 temp_cast_2 = (_Float3).xxxx;
				float4 temp_cast_3 = (1.0).xxxx;
				float2 lerpResult1048 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float73);
				float2 NoisMask11054 = lerpResult1048;
				float2 uv_Mask = i.ase_texcoord1.xy * _Mask_ST.xy + _Mask_ST.zw;
				float cos769 = cos( radians( _Float15 ) );
				float sin769 = sin( radians( _Float15 ) );
				float2 rotator769 = mul( uv_Mask - float2( 0,0 ) , float2x2( cos769 , -sin769 , sin769 , cos769 )) + float2( 0,0 );
				float2 temp_output_765_0 = ( rotator769 + ( _Vector3 * _Time.y ) );
				float temp_output_865_0 = (temp_output_765_0).x;
				float lerpResult867 = lerp( temp_output_865_0 , saturate( temp_output_865_0 ) , _Float41);
				float temp_output_866_0 = (temp_output_765_0).y;
				float lerpResult868 = lerp( temp_output_866_0 , saturate( temp_output_866_0 ) , _Float42);
				float2 appendResult873 = (float2(lerpResult867 , lerpResult868));
				float4 tex2DNode758 = tex2D( _Mask, ( NoisMask11054 + appendResult873 ) );
				float temp_output_949_0 = abs( _Float16 );
				float4 temp_cast_4 = (temp_output_949_0).xxxx;
				float4 temp_output_874_0 = ( pow( tex2DNode758 , temp_cast_4 ) * _Float43 );
				float4 temp_cast_5 = (1.0).xxxx;
				float2 lerpResult1050 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float74);
				float2 NoisMask21055 = lerpResult1050;
				float2 uv_Mask1 = i.ase_texcoord1.xy * _Mask1_ST.xy + _Mask1_ST.zw;
				float cos883 = cos( radians( _Float44 ) );
				float sin883 = sin( radians( _Float44 ) );
				float2 rotator883 = mul( uv_Mask1 - float2( 0,0 ) , float2x2( cos883 , -sin883 , sin883 , cos883 )) + float2( 0,0 );
				float2 temp_output_885_0 = ( rotator883 + ( _Vector6 * _Time.y ) );
				float temp_output_887_0 = (temp_output_885_0).x;
				float lerpResult892 = lerp( temp_output_887_0 , saturate( temp_output_887_0 ) , _Float45);
				float temp_output_886_0 = (temp_output_885_0).y;
				float lerpResult893 = lerp( temp_output_886_0 , saturate( temp_output_886_0 ) , _Float46);
				float2 appendResult894 = (float2(lerpResult892 , lerpResult893));
				float4 tex2DNode895 = tex2D( _Mask1, ( NoisMask21055 + appendResult894 ) );
				float temp_output_950_0 = abs( _Float47 );
				float4 temp_cast_6 = (temp_output_950_0).xxxx;
				float4 temp_output_901_0 = saturate( ( pow( tex2DNode895 , temp_cast_6 ) * _Float48 ) );
				float4 lerpResult996 = lerp( temp_cast_5 , temp_output_901_0 , _Float62);
				float4 temp_cast_7 = (1.0).xxxx;
				float4 texCoord1059 = i.ase_texcoord3;
				texCoord1059.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1092 = lerp( 1.0 , texCoord1059.z , _Float11);
				float4 temp_cast_8 = (lerpResult1092).xxxx;
				float4 clampResult1062 = clamp( ( tex2DNode895 - temp_cast_8 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 clampResult1064 = clamp( ( tex2DNode895 * clampResult1062 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 lerpResult1058 = lerp( temp_cast_7 , clampResult1064 , _Float11);
				float4 lerpResult994 = lerp( temp_cast_3 , ( saturate( temp_output_874_0 ) * lerpResult996 * lerpResult1058 ) , _Float61);
				float clampResult1030 = clamp( _Float29 , 0.0 , 0.5 );
				float temp_output_1033_0 = ( 1.0 - clampResult1030 );
				float2 uv_Dissolve = i.ase_texcoord1.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float4 texCoord1081 = i.ase_texcoord3;
				texCoord1081.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult1082 = (float2(texCoord1081.x , texCoord1081.y));
				float2 lerpResult1099 = lerp( float2( 0,0 ) , appendResult1082 , _Float75);
				float cos1036 = cos( radians( _Float70 ) );
				float sin1036 = sin( radians( _Float70 ) );
				float2 rotator1036 = mul( ( uv_Dissolve + lerpResult1099 ) - float2( 0,0 ) , float2x2( cos1036 , -sin1036 , sin1036 , cos1036 )) + float2( 0,0 );
				float2 lerpResult1124 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float96);
				float2 NoisRJ1125 = lerpResult1124;
				float4 tex2DNode795 = tex2D( _Dissolve, ( rotator1036 + ( _Vector5 * _Time.y ) + NoisRJ1125 ) );
				float2 uv_Dissolve1 = i.ase_texcoord1.xy * _Dissolve1_ST.xy + _Dissolve1_ST.zw;
				float2 lerpResult1084 = lerp( float2( 0,0 ) , appendResult1082 , _Float8);
				float cos1040 = cos( radians( _Float71 ) );
				float sin1040 = sin( radians( _Float71 ) );
				float2 rotator1040 = mul( ( uv_Dissolve1 + lerpResult1084 ) - float2( 0,0 ) , float2x2( cos1040 , -sin1040 , sin1040 , cos1040 )) + float2( 0,0 );
				float lerpResult932 = lerp( 1.0 , ( tex2D( _Dissolve1, ( rotator1040 + ( _aegrdfgaergare * _Time.y ) + NoisRJ1125 ) ).r * _Float541111111111111111111 ) , _Float55);
				float lerpResult944 = lerp( ( tex2DNode795.r * tex2DNode795.a * _Float69 ) , lerpResult932 , _Float30);
				float2 appendResult922 = (float2(_Float93 , _Float94));
				float2 texCoord924 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult922;
				float cos925 = cos( radians( _Float95 ) );
				float sin925 = sin( radians( _Float95 ) );
				float2 rotator925 = mul( texCoord924 - float2( 0.5,0.5 ) , float2x2( cos925 , -sin925 , sin925 , cos925 )) + float2( 0.5,0.5 );
				float lerpResult929 = lerp( 1.0 , (rotator925).x , _Float97);
				float4 texCoord829 = i.ase_texcoord2;
				texCoord829.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult834 = lerp( _Float28 , texCoord829.z , _Float38);
				float temp_output_806_0 = ( ( lerpResult944 * lerpResult929 ) + ( lerpResult834 * -2.0 ) + 1.0 );
				float smoothstepResult798 = smoothstep( clampResult1030 , temp_output_1033_0 , ( temp_output_806_0 + _Float79 ));
				float lerpResult1114 = lerp( 1.0 , smoothstepResult798 , _Float32);
				float2 texCoord1011 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1020 = lerp( texCoord1011.x , texCoord1011.y , _Float64);
				float4 lerpResult1015 = lerp( _Color5 , _Color0 , saturate( pow( ( 1.0 - lerpResult1020 ) , _power ) ));
				float4 texCoord1087 = i.ase_texcoord2;
				texCoord1087.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1088 = lerp( 0.0 , texCoord1087.w , _Float9);
				float4 temp_output_671_0 = ( saturate( pow( ( lerpResult1025 * _Float66 ) , temp_cast_2 ) ) * lerpResult994 * lerpResult1114 * lerpResult1015 * ( _Color0 * ( _Float7 + lerpResult1088 ) ) * i.ase_color * _Color0.a );
				float smoothstepResult1103 = smoothstep( clampResult1030 , temp_output_1033_0 , ( temp_output_806_0 + _Float36 ));
				float lerpResult1117 = lerp( 0.0 , ( smoothstepResult798 - smoothstepResult1103 ) , _Float32);
				float4 lerpResult843 = lerp( temp_output_671_0 , ( _Float37 * _Color2 ) , lerpResult1117);
				float smoothstepResult1109 = smoothstep( clampResult1030 , temp_output_1033_0 , temp_output_806_0);
				float lerpResult1118 = lerp( 0.0 , ( smoothstepResult1103 - smoothstepResult1109 ) , _Float32);
				float4 lerpResult852 = lerp( lerpResult843 , ( _Float39 * _Color3 ) , lerpResult1118);
				float4 temp_cast_9 = (_Float17).xxxx;
				float2 uv_TextureSample0 = i.ase_texcoord1.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
				float cos986 = cos( radians( _Float56 ) );
				float sin986 = sin( radians( _Float56 ) );
				float2 rotator986 = mul( uv_TextureSample0 - float2( 0.5,0.5 ) , float2x2( cos986 , -sin986 , sin986 , cos986 )) + float2( 0.5,0.5 );
				float2 lerpResult1126 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float99);
				float2 Noisdiejia1127 = lerpResult1126;
				float4 tex2DNode951 = tex2D( _TextureSample0, ( rotator986 + ( _Vector7 * _Time.y ) + Noisdiejia1127 ) );
				float4 temp_cast_10 = (_Float54).xxxx;
				float4 lerpResult992 = lerp( float4( 0,0,0,0 ) , ( lerpResult1015 * saturate( pow( ( _Color4 * tex2DNode951 ) , temp_cast_10 ) ) * _Float63 ) , _Float60);
				float4 temp_cast_12 = (tex2DNode951.a).xxxx;
				float4 lerpResult956 = lerp( temp_cast_12 , tex2DNode951 , _Float57);
				float4 temp_cast_13 = (_Float54).xxxx;
				float4 temp_output_959_0 = saturate( pow( lerpResult956 , temp_cast_13 ) );
				float4 lerpResult990 = lerp( float4( 0,0,0,0 ) , temp_output_959_0 , _Float60);
				float4 temp_cast_14 = (tex2DNode619.a).xxxx;
				float4 lerpResult623 = lerp( temp_cast_14 , lerpResult1025 , _Float0);
				float4 temp_cast_15 = (_Float3).xxxx;
				float4 temp_output_685_0 = saturate( pow( ( _Float66 * lerpResult623 ) , temp_cast_15 ) );
				float4 temp_cast_16 = (1.0).xxxx;
				float4 lerpResult989 = lerp( temp_cast_16 , temp_output_959_0 , _Float60);
				float4 lerpResult980 = lerp( ( lerpResult990 + temp_output_685_0 ) , ( lerpResult989 * temp_output_685_0 ) , _Float59);
				float4 temp_cast_17 = (1.0).xxxx;
				float4 temp_cast_18 = (( pow( tex2DNode758.a , temp_output_949_0 ) * _Float43 )).xxxx;
				float4 lerpResult762 = lerp( temp_cast_18 , temp_output_874_0 , _Float14);
				float4 temp_cast_19 = (1.0).xxxx;
				float4 temp_cast_20 = (( pow( tex2DNode895.a , temp_output_950_0 ) * _Float48 )).xxxx;
				float4 lerpResult906 = lerp( temp_cast_20 , temp_output_901_0 , _Float50);
				float4 lerpResult997 = lerp( temp_cast_19 , lerpResult906 , _Float62);
				float4 lerpResult993 = lerp( temp_cast_17 , ( lerpResult762 * lerpResult997 ) , _Float61);
				float4 appendResult621 = (float4(( lerpResult852 + ( pow( temp_output_671_0 , temp_cast_9 ) * _Float19 * _Color1 * _Color1.a ) + lerpResult992 ).rgb , ( lerpResult980 * ( temp_output_685_0 * _Float6 * lerpResult993 * lerpResult1114 * _Color0.a * i.ase_color.a ) ).r));
				float4 screenPos = i.ase_texcoord4;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth701 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float distanceDepth701 = saturate( abs( ( screenDepth701 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Float5 ) ) );
				
				
				finalColor = ( appendResult621 * distanceDepth701 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "LWGUI.LWGUI"
	
	
}
/*ASEBEGIN
Version=18935
130.4;251.2;1890.4;689.4;-134.5986;1942.769;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;1130;-3502.046,-160.5759;Inherit;False;2083.492;1183.731;;26;753;755;752;754;756;745;1005;746;749;748;751;750;1123;1124;1051;1050;1125;1049;1055;1048;1128;1046;1054;1126;1044;1127;noise扰动;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;753;-3437.046,52.42427;Inherit;False;Property;_Vector2;扰动uv;60;0;Create;False;0;0;0;True;1;Sub(z3);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;755;-3452.046,181.4245;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;752;-3441.046,-110.5759;Inherit;False;0;745;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;754;-3194.046,96.42411;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;756;-3068.046,-12.57585;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;745;-2915.769,-50.15195;Inherit;True;Property;_Noise;扰动贴图;53;0;Create;False;0;0;0;True;1;Sub(z3);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;999;-4541.774,2262.744;Inherit;False;3603.428;644.675;;35;878;881;879;882;880;883;884;885;887;886;890;891;889;888;892;893;894;896;895;950;898;899;900;916;901;904;906;917;996;997;1000;1003;1004;1052;1057;附加遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;705;-4012.594,-865.9828;Inherit;False;834.0901;349;;4;687;694;691;692;主贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;1005;-2575.184,-50.26306;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;878;-4493.774,2470.743;Inherit;False;Property;_Float44;m旋转;49;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;687;-3962.594,-796.5856;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;691;-3638.684,-784.0233;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;749;-2379.384,160.483;Inherit;False;Property;_Float12;扰动开关;52;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;881;-4125.773,2582.743;Inherit;False;Property;_Vector6;muv;50;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;746;-2379.485,-28.32045;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;694;-3715.504,-632.9828;Inherit;False;Property;_Float4;custom1XY控制主贴图uv;7;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;882;-4143.704,2462.968;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;715;-2908.29,-960.4611;Inherit;False;1015.928;565.5943;;9;643;644;688;646;647;645;649;648;650;主贴图uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;880;-4141.773,2726.744;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1102;-8343.886,4556.721;Inherit;False;1069.375;567.2646;;6;1081;1082;1084;1083;1099;1100;溶解贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;879;-4285.774,2310.744;Inherit;False;0;895;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;998;-4302.07,1239.775;Inherit;False;4016.871;825.1143;;37;771;766;770;764;767;768;769;765;865;866;872;869;870;871;867;868;873;774;949;758;773;875;874;775;792;763;908;995;994;762;793;914;993;1001;1002;1047;1056;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;644;-2837.226,-910.4612;Inherit;False;0;619;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;748;-2090.738,-57.55825;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1081;-8293.886,4843.983;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;643;-2858.29,-728.0205;Inherit;False;Property;_Float20;主贴图旋转;26;0;Create;False;0;0;0;True;1;Sub(z1);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;751;-2061.163,111.8896;Inherit;False;Property;_Float13;扰动强度;59;0;Create;False;0;0;0;True;1;Sub(z3);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;884;-3869.774,2566.744;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;771;-4287.537,1468.983;Inherit;False;Property;_Float15;遮罩图旋转;40;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;883;-3919.704,2398.968;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;692;-3362.505,-815.9828;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;885;-3677.774,2502.744;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1123;-2516.678,742.6239;Inherit;False;Property;_Float96;扰动两张溶解;57;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;750;-1850.161,-69.11035;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;766;-3935.541,1580.983;Inherit;False;Property;_Vector3;遮罩uv;41;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;688;-2507.316,-853.9438;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;647;-2517.39,-637.1157;Inherit;False;Property;_Vector4;主贴图UV;27;0;Create;False;0;0;0;True;1;Sub(z1);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;767;-3951.541,1724.983;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1043;-7118.01,5136.471;Inherit;False;1983.884;583.2234;;15;938;936;931;937;935;934;945;1040;941;942;939;1041;940;1042;1085;附加溶解贴图uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.RadiansOpNode;646;-2559.726,-723.2807;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1083;-8037.801,5008.585;Inherit;False;Property;_Float8;Custom2XY控制附加溶解贴图uv;11;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1082;-7961.98,4857.545;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RadiansOpNode;770;-3951.541,1468.983;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;764;-4079.541,1308.983;Inherit;False;0;758;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;645;-2521.056,-505.8671;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1051;-2516.908,538.11;Inherit;False;Property;_Float74;扰动附加遮罩;56;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1124;-2290.497,701.7018;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1042;-7120.521,5419.639;Inherit;False;Property;_Float71;附加溶解贴图旋转;77;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;769;-3711.541,1404.983;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;768;-3679.541,1564.983;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;886;-3501.774,2598.743;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;649;-2261.51,-663.7856;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;648;-2314.483,-823.9484;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1100;-8043.71,4673.762;Inherit;False;Property;_Float75;Custom2XY控制溶解贴图uv;10;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1084;-7618.982,4827.923;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;939;-7057.224,5175.69;Inherit;False;0;931;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1113;-6392.423,3928.894;Inherit;False;1052.989;653.8198;;9;836;1038;840;839;1037;1101;1036;838;837;溶解贴图UV;1,1,1,1;0;0
Node;AmplifyShaderEditor.ComponentMaskNode;887;-3501.774,2422.743;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;941;-6896.607,5657.847;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;890;-3309.774,2790.743;Inherit;False;Property;_Float46;myclamp;46;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1125;-2050.326,713.9114;Inherit;False;NoisRJ;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1050;-2292.603,497.6081;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;918;-6482.359,5833.036;Inherit;False;1591.615;427.6962;;11;925;924;923;922;921;920;919;927;928;929;926;方向溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;889;-3267.259,2443.347;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1038;-6223.914,4231.406;Inherit;False;Property;_Float70;溶解贴图旋转;64;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;891;-3293.774,2694.744;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1085;-6765.743,5275.721;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;718;-1209.259,-914.7432;Inherit;False;1031.703;551.2322;;9;699;698;657;659;654;655;658;656;642;主贴图不循环;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;1099;-7623.952,4626.481;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;765;-3487.541,1500.983;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;888;-3317.447,2532.556;Inherit;False;Property;_Float45;mxclamp;45;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;836;-6342.423,3978.894;Inherit;False;0;795;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;940;-6906.238,5526.613;Inherit;False;Property;_aegrdfgaergare;附加溶解贴图uv;80;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;650;-1992.232,-735.1715;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RadiansOpNode;1041;-6770.783,5424.264;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;893;-3076.986,2658.482;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1069;-7016.246,4782.08;Inherit;False;1125;NoisRJ;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;892;-3101.541,2414.989;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;840;-6005.633,4320.813;Inherit;False;Property;_Vector5;溶解贴图uv;74;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;920;-6437.36,5999.296;Inherit;False;Property;_Float94;方向V偏移;84;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;1040;-6563.104,5314.713;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1055;-2050.272,493.344;Inherit;False;NoisMask2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;866;-3311.541,1596.983;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;919;-6434.36,5920.484;Inherit;False;Property;_Float93;方向U偏移;83;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;839;-6021.831,4472.314;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1049;-2514.447,364.4426;Inherit;False;Property;_Float73;扰动遮罩;55;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;942;-6613.914,5577.256;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RadiansOpNode;1037;-5935.917,4237.806;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;865;-3311.541,1420.983;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;699;-1156.948,-643.5973;Inherit;False;False;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1101;-6024.396,4069.779;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;698;-1159.259,-844.81;Inherit;False;True;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;659;-869.7146,-479.5111;Inherit;False;Property;_Float2;主贴图yclamp;23;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;869;-3119.54,1516.983;Inherit;False;Property;_Float41;遮罩xclamp;36;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;657;-875.7146,-729.5103;Inherit;False;Property;_Float1;主贴图xclamp;22;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;870;-3045.322,1433.128;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;655;-862.6285,-566.9052;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;921;-6391.483,6134.733;Inherit;False;Property;_Float95;方向溶解旋转（默认左黑右白）;82;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;922;-6257.485,5942.735;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;871;-3103.54,1692.983;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1057;-2932.476,2341.923;Inherit;False;1055;NoisMask2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1066;-2824.888,3041.361;Inherit;False;1892.448;560.886;;10;1059;1058;1061;1062;1063;1064;1092;1093;1095;1094;CenterGlow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;828;-5053.703,4095.261;Inherit;False;3867.203;1427.534;;14;810;809;800;829;834;835;930;944;932;1035;1112;1114;1115;1116;溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;654;-860.6285,-805.9048;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;938;-6263.588,5410.77;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;894;-2895.954,2484.161;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1048;-2300.141,319.9406;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;872;-3103.54,1788.983;Inherit;False;Property;_Float42;遮罩yclamp;37;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;1036;-5775.917,4189.806;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;838;-5738.634,4351.813;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;924;-6080.483,5893.734;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;658;-586.7147,-647.5104;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1046;-1826.45,97.22233;Inherit;False;Property;_Float72;扰动主贴图;54;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1054;-2048.509,337.8208;Inherit;False;NoisMask1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;867;-2855.54,1373.983;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1094;-2556.155,3263.266;Inherit;False;Constant;_Float77;Float 77;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;923;-5996.487,6133.733;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;868;-2864.54,1629.983;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1128;-2525.255,907.7556;Inherit;False;Property;_Float99;扰动叠加贴图;58;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1059;-2799.965,3247.758;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;988;-579.777,-1853.554;Inherit;False;4020.037;699.9189;;30;968;961;967;965;964;986;966;962;969;951;956;960;955;977;959;954;982;958;981;983;980;990;989;991;992;1008;1009;1120;1121;1129;叠加;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;810;-5003.703,4233.457;Inherit;False;785.1409;280;;2;795;796;溶解贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1093;-2793.989,3489.377;Inherit;False;Property;_Float11;Custom2Z控制centerGlow（遮罩开关全部打开）;12;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;936;-5998.387,5599.87;Inherit;False;Property;_Float541111111111111111111;附加溶解贴图强度;79;0;Create;False;0;0;0;True;1;Sub(z4);False;1;11;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;656;-614.9337,-864.7432;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1052;-2752.096,2390.82;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;837;-5492.634,4303.812;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;931;-6087.325,5374.515;Inherit;True;Property;_Dissolve1;附加溶解贴图;76;0;Create;False;0;0;0;False;1;Sub(z4);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1126;-2285.141,863.4938;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1056;-2639.342,1375.325;Inherit;False;1054;NoisMask1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;873;-2664.54,1500.983;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1035;-4879.426,4511.796;Inherit;False;Property;_Float69;溶解贴图强度;65;0;Create;False;0;0;0;False;1;Sub(z4);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1092;-2317.624,3405.775;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;925;-5809.49,6006.735;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1044;-1600.553,-103.2009;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;935;-5641.386,5440.869;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;934;-5559.386,5282.87;Inherit;False;Constant;_Float53;Float 53;69;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;968;-528.7772,-1634.554;Inherit;False;Property;_Float56;叠加旋转;93;0;Create;False;0;0;0;True;1;Sub(z5);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;795;-4981.104,4271.159;Inherit;True;Property;_Dissolve;溶解贴图;63;0;Create;False;0;0;0;True;1;Sub(z4);False;-1;None;12d718b2c0e194143bf6c237b4a528d2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;896;-2605.773,2710.744;Inherit;False;Property;_Float47;mpower;48;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;937;-5727.386,5594.87;Inherit;False;Property;_Float55;附加溶解贴图开关;75;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;642;-342.5569,-724.5688;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;895;-2592.49,2467.388;Inherit;True;Property;_Mask1;m遮罩贴图（CenterGlow）;43;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;835;-4452.753,4853.069;Inherit;False;Property;_Float38;custom1Z控制溶解程度;8;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1047;-2446.505,1490.688;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1022;328.4902,514.7003;Inherit;False;1302.474;1017.203;;17;1020;1019;1012;1016;1013;781;1023;1011;1021;672;673;1015;1086;1087;1088;1089;1098;颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;926;-5537.423,6171.507;Inherit;False;Property;_Float97;方向溶解开关;81;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;774;-2462.069,1703.775;Inherit;False;Property;_Float16;遮罩power;39;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;829;-4775.283,4728.336;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;800;-4604.534,4590.324;Inherit;False;Property;_Float28;溶解程度;66;0;Create;False;0;0;0;True;1;Sub(z4);False;-0.2;0;-0.2;1.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;945;-5434.125,5603.849;Inherit;False;Property;_Float30;溶解纹理偏向;78;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;967;-202.7779,-1638.554;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;950;-2397.773,2710.744;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1127;-2048.243,861.4512;Inherit;False;Noisdiejia;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1061;-2064.876,3380.954;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;965;-203.7779,-1393.554;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;686;498.9437,-910.0938;Inherit;False;1736.342;788.0424;;14;619;624;623;683;680;684;685;681;1024;1025;1026;1028;1029;1027;主贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;928;-5524.474,5883.037;Inherit;False;Constant;_Float52;Float 52;32;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;927;-5597.937,5968.87;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;964;-199.3779,-1550.954;Inherit;False;Property;_Vector7;叠加uv;94;0;Create;False;0;0;0;True;1;Sub(z5);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;796;-4654.362,4314.377;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;961;-344.7779,-1803.554;Inherit;False;0;951;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;809;-3779.87,4152.313;Inherit;False;1733.592;529.1241;;7;805;807;798;806;804;1104;1111;软边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;932;-4993.085,5409.444;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;747;334.952,-702.4619;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1021;363.0628,1342.07;Inherit;False;Property;_Float64;渐变方向;17;0;Create;False;0;0;0;True;1;KWEnum(z1,U,_0,V,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;899;-2189.774,2438.743;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;758;-2329.069,1468.775;Inherit;True;Property;_Mask;遮罩贴图;34;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;966;44.22243,-1541.354;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;1062;-1863.035,3379.18;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.RotatorNode;986;41.73484,-1734.49;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1011;338.4902,1158.61;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;929;-5246.119,5993.624;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;619;548.9436,-813.4742;Inherit;True;Property;_Main;主贴图;19;0;Create;False;0;0;0;True;1;Sub(z1);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1112;-3436.487,4694.458;Inherit;False;2056.17;779.0845;;11;1033;801;1031;1032;1108;1110;1109;1106;1103;1107;1030;亮边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;834;-4117.608,4586.521;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;898;-2169.774,2623.743;Inherit;False;Property;_Float48;m强度;47;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;949;-2270.07,1703.775;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;944;-4637.683,5358.828;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1129;-10.57971,-1291.928;Inherit;False;1127;Noisdiejia;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;805;-3723.36,4242.557;Inherit;False;Constant;_Float33;Float 33;44;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1032;-3318.637,4908.166;Inherit;False;Constant;_Float68;Float 68;86;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;773;-1870.07,1511.775;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;801;-3386.487,4744.458;Inherit;False;Property;_Float29;软（左）硬（右）程度;67;0;Create;False;0;0;0;True;1;Sub(z4);False;0.5;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1026;896,-768;Inherit;False;Property;_Float65;主贴图去色;21;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;962;305.2223,-1648.554;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1063;-1604.319,3362.465;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;807;-3251.652,4302.165;Inherit;False;Constant;_Float34;Float 34;44;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;875;-1976.555,1626.26;Inherit;False;Property;_Float43;遮罩强度;38;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;930;-4361.037,5342.769;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;900;-1933.774,2438.743;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCGrayscale;1024;864,-864;Inherit;False;2;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1031;-3319.637,4831.166;Inherit;False;Constant;_Float67;Float 67;86;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1020;583.0629,1209.07;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;804;-3448.452,4214.08;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;624;888.0816,-506.8036;Inherit;False;Property;_Float0;主贴图通道;20;0;Create;False;0;0;0;True;1;KWEnum(z1,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1089;381.4733,1031.295;Inherit;False;Property;_Float9;Costom1W控制颜色强度;9;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;1004;-2157.774,2758.744;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1111;-2703.192,4309.225;Inherit;False;Property;_Float79;外层亮边宽度;70;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;916;-1661.774,2310.744;Inherit;False;Constant;_Float49;Float 49;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;874;-1646.07,1527.775;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1019;672.9424,1415.903;Inherit;False;Property;_power;渐变颜色power;16;0;Create;False;0;0;0;True;1;Sub(z1);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;969;653.3629,-1383.441;Inherit;False;Property;_Float57;叠加贴图通道;89;0;Create;False;0;0;0;True;1;KWEnum(z5,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1087;386.6158,837.4382;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;1064;-1438.139,3358.416;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;1023;750.4244,1247.5;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1108;-2557.34,4982.063;Inherit;False;Property;_Float36;内层亮边宽度;73;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1029;1252.844,-731.4374;Inherit;False;Property;_Float66;主贴图强度;24;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;806;-2955.705,4204.968;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;901;-1725.774,2438.743;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1025;1066.74,-851.812;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;1030;-3081.637,4790.166;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1095;-1415.487,3103.579;Inherit;False;Constant;_Float78;Float 78;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;951;481.9276,-1678.985;Inherit;True;Property;_TextureSample0;叠加贴图;87;0;Create;False;0;0;0;True;1;Sub(z5);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1000;-1533.774,2822.743;Inherit;False;Property;_Float62;附加遮罩开关;42;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;904;-1805.774,2806.743;Inherit;False;Property;_Float50;m通道;44;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1003;-1869.774,2630.743;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1107;-2346.752,4964.257;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;1001;-1950.07,1719.775;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1058;-1244.772,3107.418;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;1033;-2663.972,4750.304;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;1012;877.2357,1363.296;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1028;1606.844,-855.4374;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1120;822.0812,-1799.001;Inherit;False;Property;_Color4;叠加贴图颜色;88;1;[HDR];Create;False;0;0;0;True;1;Sub(z5);False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;775;-1406.07,1511.775;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;960;1129.007,-1600.924;Inherit;False;Property;_Float54;叠加贴图power;92;0;Create;False;0;0;0;True;1;Sub(z5);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;956;962.6859,-1460.182;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1104;-2508.738,4199.188;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;996;-1197.774,2326.744;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1088;664.5502,879.2839;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;623;1191.565,-584.8971;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;673;525.0034,735.7686;Inherit;False;Property;_Float7;颜色强度;18;0;Create;False;0;0;0;True;1;Sub(z1);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;681;1602.431,-741.8758;Inherit;False;Property;_Float3;主贴图power;25;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1027;1603.844,-616.4374;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1013;917.1681,1072.464;Inherit;False;Property;_Color5;次颜色;15;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,0,0,1;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;680;1877.765,-849.0938;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;798;-2300.275,4202.312;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1002;-1646.07,1703.775;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1016;1078.42,1296.029;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;995;-953.07,1814.775;Inherit;False;Property;_Float61;遮罩开关;33;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;908;-926.07,1511.775;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;792;-1118.07,1319.775;Inherit;False;Constant;_Float21;Float 21;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;906;-1549.774,2678.744;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;763;-1534.07,1815.775;Inherit;False;Property;_Float14;遮罩通道;35;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1103;-2046.894,4861.639;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1115;-1793.729,4529.477;Inherit;False;Property;_Float32;溶解开关;62;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;955;1352.665,-1497.993;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1086;861.1762,775.4272;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;917;-1357.774,2598.743;Inherit;False;Constant;_Float51;Float 51;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1116;-1780.211,4169.535;Inherit;False;Constant;_Float80;Float 80;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1121;1137.692,-1738.899;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;672;593.6955,529.9717;Inherit;False;Property;_Color0;颜色;14;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;948;3364.806,-577.7418;Inherit;False;911.7981;680.3812;;8;822;823;844;843;855;854;856;852;溶解亮边;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;781;933.52,885.9263;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;997;-1117.774,2630.743;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1106;-1627.313,4855.336;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;823;3458.806,-527.7419;Inherit;False;Property;_Float37;外层亮边颜色强度;69;0;Create;False;0;0;0;True;1;Sub(z4);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1098;1093.613,674.8748;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;991;1560.363,-1351.186;Inherit;False;Property;_Float60;叠加开关;86;0;Create;False;0;0;0;True;1;SubToggle(z5,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;822;3414.806,-448.7421;Inherit;False;Property;_Color2;外层亮边颜色;68;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;684;2067.288,-856.2079;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;1097;2920.694,-780.7308;Inherit;False;212;305;;1;671;RGB;1,0,0,1;0;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1109;-2048.453,5218.482;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;683;1886.111,-681.7486;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;791;3402.215,1636.105;Inherit;False;628.9875;471.9476;;5;786;789;785;788;787;高光;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;959;1642.421,-1485.654;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;954;1348.319,-1683.338;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1015;1342.327,596.0429;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1114;-1484.588,4221.821;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;977;1977.178,-1577.246;Inherit;False;Constant;_Float58;Float 58;78;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;994;-558.0698,1463.775;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;762;-1262.07,1703.775;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1110;-1615.717,5220.143;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;989;2249.363,-1535.186;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;914;-830.0699,1943.775;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;685;2078.847,-680.1552;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;793;-686.0699,1815.775;Inherit;False;Constant;_Float23;Float 23;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;855;3525.238,-140.8038;Inherit;False;Property;_Color3;内层亮边2颜色;71;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;786;3452.215,1740.005;Inherit;False;Property;_Float17;高光细节;30;0;Create;False;0;0;0;True;1;Sub(z6);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;958;1636.421,-1679.754;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1117;-1223.292,4830.225;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;990;2245.363,-1339.186;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;854;3563.238,-244.8036;Inherit;False;Property;_Float39;内层亮边2颜色强度;72;0;Create;False;0;0;0;True;1;Sub(z4);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1096;2623.096,688.5531;Inherit;False;755.478;289.493;;2;670;669;Alpha;1,0,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1009;1617.327,-1589.685;Inherit;False;Property;_Float63;叠加贴图强度;91;0;Create;False;0;0;0;True;1;Sub(z5);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;671;2970.694,-730.7308;Inherit;False;7;7;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;844;3697.979,-510.0084;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;982;2532.119,-1514.127;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1008;1939.329,-1720.591;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;788;3642.202,1810.054;Inherit;False;Property;_Float19;高光强度;31;0;Create;False;0;0;0;True;1;Sub(z6);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;993;-462.0698,1815.775;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;785;3642.316,1686.105;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;981;2610.785,-1246.718;Inherit;False;Property;_Float59;A通道算法（Add时为叠加）;90;0;Create;False;0;0;0;True;1;KWEnum(z5,Add,_0,Mul,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;789;3569.202,1896.054;Inherit;False;Property;_Color1;高光颜色;29;0;Create;False;0;0;0;True;1;Sub(z6);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;843;3944.614,-506.3961;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;670;2673.096,738.5531;Inherit;False;Property;_Float6;整体Alpha强度;2;0;Create;False;0;0;0;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;856;3844.038,-155.3037;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1118;-1209.958,5092.644;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;983;2534.519,-1348.327;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;704;3449.9,1240.301;Inherit;False;804.9153;185.5732;;2;702;701;软粒子;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;787;3865.202,1720.053;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;852;4094.603,-195.7102;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;980;2951.476,-1325.736;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;669;3216.574,747.0461;Inherit;False;6;6;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;992;2227.363,-1723.186;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;784;4755.613,-686.793;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1119;4747.155,-434.2447;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;702;3499.9,1308.893;Inherit;False;Property;_Float5;软粒子（相机需开启投影）;3;0;Create;False;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;701;3767.207,1290.874;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;621;4973.019,-627.772;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;532;4877.106,-1963.568;Inherit;False;227.2825;729.8185;;7;794;143;141;140;952;142;1091;GUI列表;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;141;4930.62,-1814.86;Inherit;False;Property;_Float25;遮罩列表;32;0;Create;False;0;0;0;True;1;Main(z2,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;4929.316,-1717.562;Inherit;False;Property;_Float26;扰动列表;51;0;Create;False;0;0;0;True;1;Main(z3,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;4930.976,-1622.63;Inherit;False;Property;_Float27;溶解列表;61;0;Create;False;0;0;0;True;1;Main(z4,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;4613.841,-1635.218;Inherit;False;Property;_Float18;显示最前面;5;1;[Enum];Create;False;0;2;open;0;close;4;0;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;4927.106,-1913.568;Inherit;False;Property;_Float24;主贴图列表;13;0;Create;False;0;0;0;True;1;Main(z1,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;700;5231.031,-605.5115;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;121;4616.961,-1829.483;Inherit;False;Property;_Float22;剔除模式;1;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;4616.961,-1726.482;Inherit;False;Property;_Blendmode;混合模式;0;1;[Enum];Create;False;0;2;add;1;blend;10;0;True;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;952;4928.714,-1520.668;Inherit;False;Property;_Float40;叠加列表;85;0;Create;False;0;0;0;True;1;Main(z5,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;550;4604.212,-1519.42;Inherit;False;Property;_Float85;深度写入（模型特效适用）;4;1;[Toggle];Create;False;0;2;open;0;close;4;0;True;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1091;4926.083,-1326.958;Inherit;False;Property;_Float10;Custom列表;6;0;Create;False;0;0;0;True;1;Main(z7,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;794;4929.246,-1430.953;Inherit;False;Property;_Float31;高光列表;28;0;Create;False;0;0;0;True;1;Main(z6,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;617;5510.12,-612.5597;Float;False;True;-1;2;LWGUI.LWGUI;100;1;Test/极简01_1;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;True;120;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;121;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;1;True;550;True;3;True;130;True;True;0;False;-1;0;False;-1;True;2;RenderType=Opaque=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;754;0;753;0
WireConnection;754;1;755;0
WireConnection;756;0;752;0
WireConnection;756;1;754;0
WireConnection;745;1;756;0
WireConnection;1005;0;745;1
WireConnection;1005;1;745;2
WireConnection;691;0;687;1
WireConnection;691;1;687;2
WireConnection;746;0;1005;0
WireConnection;746;1;745;4
WireConnection;882;0;878;0
WireConnection;748;1;746;0
WireConnection;748;2;749;0
WireConnection;884;0;881;0
WireConnection;884;1;880;0
WireConnection;883;0;879;0
WireConnection;883;2;882;0
WireConnection;692;1;691;0
WireConnection;692;2;694;0
WireConnection;885;0;883;0
WireConnection;885;1;884;0
WireConnection;750;0;748;0
WireConnection;750;1;751;0
WireConnection;688;0;644;0
WireConnection;688;1;692;0
WireConnection;646;0;643;0
WireConnection;1082;0;1081;1
WireConnection;1082;1;1081;2
WireConnection;770;0;771;0
WireConnection;1124;1;750;0
WireConnection;1124;2;1123;0
WireConnection;769;0;764;0
WireConnection;769;2;770;0
WireConnection;768;0;766;0
WireConnection;768;1;767;0
WireConnection;886;0;885;0
WireConnection;649;0;647;0
WireConnection;649;1;645;0
WireConnection;648;0;688;0
WireConnection;648;2;646;0
WireConnection;1084;1;1082;0
WireConnection;1084;2;1083;0
WireConnection;887;0;885;0
WireConnection;1125;0;1124;0
WireConnection;1050;1;750;0
WireConnection;1050;2;1051;0
WireConnection;889;0;887;0
WireConnection;891;0;886;0
WireConnection;1085;0;939;0
WireConnection;1085;1;1084;0
WireConnection;1099;1;1082;0
WireConnection;1099;2;1100;0
WireConnection;765;0;769;0
WireConnection;765;1;768;0
WireConnection;650;0;648;0
WireConnection;650;1;649;0
WireConnection;1041;0;1042;0
WireConnection;893;0;886;0
WireConnection;893;1;891;0
WireConnection;893;2;890;0
WireConnection;892;0;887;0
WireConnection;892;1;889;0
WireConnection;892;2;888;0
WireConnection;1040;0;1085;0
WireConnection;1040;2;1041;0
WireConnection;1055;0;1050;0
WireConnection;866;0;765;0
WireConnection;942;0;940;0
WireConnection;942;1;941;0
WireConnection;1037;0;1038;0
WireConnection;865;0;765;0
WireConnection;699;0;650;0
WireConnection;1101;0;836;0
WireConnection;1101;1;1099;0
WireConnection;698;0;650;0
WireConnection;870;0;865;0
WireConnection;655;0;699;0
WireConnection;922;0;919;0
WireConnection;922;1;920;0
WireConnection;871;0;866;0
WireConnection;654;0;698;0
WireConnection;938;0;1040;0
WireConnection;938;1;942;0
WireConnection;938;2;1069;0
WireConnection;894;0;892;0
WireConnection;894;1;893;0
WireConnection;1048;1;750;0
WireConnection;1048;2;1049;0
WireConnection;1036;0;1101;0
WireConnection;1036;2;1037;0
WireConnection;838;0;840;0
WireConnection;838;1;839;0
WireConnection;924;1;922;0
WireConnection;658;0;699;0
WireConnection;658;1;655;0
WireConnection;658;2;659;0
WireConnection;1054;0;1048;0
WireConnection;867;0;865;0
WireConnection;867;1;870;0
WireConnection;867;2;869;0
WireConnection;923;0;921;0
WireConnection;868;0;866;0
WireConnection;868;1;871;0
WireConnection;868;2;872;0
WireConnection;656;0;698;0
WireConnection;656;1;654;0
WireConnection;656;2;657;0
WireConnection;1052;0;1057;0
WireConnection;1052;1;894;0
WireConnection;837;0;1036;0
WireConnection;837;1;838;0
WireConnection;837;2;1069;0
WireConnection;931;1;938;0
WireConnection;1126;1;750;0
WireConnection;1126;2;1128;0
WireConnection;873;0;867;0
WireConnection;873;1;868;0
WireConnection;1092;0;1094;0
WireConnection;1092;1;1059;3
WireConnection;1092;2;1093;0
WireConnection;925;0;924;0
WireConnection;925;2;923;0
WireConnection;1044;1;750;0
WireConnection;1044;2;1046;0
WireConnection;935;0;931;1
WireConnection;935;1;936;0
WireConnection;795;1;837;0
WireConnection;642;0;656;0
WireConnection;642;1;658;0
WireConnection;895;1;1052;0
WireConnection;1047;0;1056;0
WireConnection;1047;1;873;0
WireConnection;967;0;968;0
WireConnection;950;0;896;0
WireConnection;1127;0;1126;0
WireConnection;1061;0;895;0
WireConnection;1061;1;1092;0
WireConnection;927;0;925;0
WireConnection;796;0;795;1
WireConnection;796;1;795;4
WireConnection;796;2;1035;0
WireConnection;932;0;934;0
WireConnection;932;1;935;0
WireConnection;932;2;937;0
WireConnection;747;0;642;0
WireConnection;747;1;1044;0
WireConnection;899;0;895;0
WireConnection;899;1;950;0
WireConnection;758;1;1047;0
WireConnection;966;0;964;0
WireConnection;966;1;965;0
WireConnection;1062;0;1061;0
WireConnection;986;0;961;0
WireConnection;986;2;967;0
WireConnection;929;0;928;0
WireConnection;929;1;927;0
WireConnection;929;2;926;0
WireConnection;619;1;747;0
WireConnection;834;0;800;0
WireConnection;834;1;829;3
WireConnection;834;2;835;0
WireConnection;949;0;774;0
WireConnection;944;0;796;0
WireConnection;944;1;932;0
WireConnection;944;2;945;0
WireConnection;773;0;758;0
WireConnection;773;1;949;0
WireConnection;962;0;986;0
WireConnection;962;1;966;0
WireConnection;962;2;1129;0
WireConnection;1063;0;895;0
WireConnection;1063;1;1062;0
WireConnection;930;0;944;0
WireConnection;930;1;929;0
WireConnection;900;0;899;0
WireConnection;900;1;898;0
WireConnection;1024;0;619;0
WireConnection;1020;0;1011;1
WireConnection;1020;1;1011;2
WireConnection;1020;2;1021;0
WireConnection;804;0;834;0
WireConnection;804;1;805;0
WireConnection;1004;0;895;4
WireConnection;1004;1;950;0
WireConnection;874;0;773;0
WireConnection;874;1;875;0
WireConnection;1064;0;1063;0
WireConnection;1023;0;1020;0
WireConnection;806;0;930;0
WireConnection;806;1;804;0
WireConnection;806;2;807;0
WireConnection;901;0;900;0
WireConnection;1025;0;619;0
WireConnection;1025;1;1024;0
WireConnection;1025;2;1026;0
WireConnection;1030;0;801;0
WireConnection;1030;1;1031;0
WireConnection;1030;2;1032;0
WireConnection;951;1;962;0
WireConnection;1003;0;1004;0
WireConnection;1003;1;898;0
WireConnection;1107;0;806;0
WireConnection;1107;1;1108;0
WireConnection;1001;0;758;4
WireConnection;1001;1;949;0
WireConnection;1058;0;1095;0
WireConnection;1058;1;1064;0
WireConnection;1058;2;1093;0
WireConnection;1033;0;1030;0
WireConnection;1012;0;1023;0
WireConnection;1012;1;1019;0
WireConnection;1028;0;1025;0
WireConnection;1028;1;1029;0
WireConnection;775;0;874;0
WireConnection;956;0;951;4
WireConnection;956;1;951;0
WireConnection;956;2;969;0
WireConnection;1104;0;806;0
WireConnection;1104;1;1111;0
WireConnection;996;0;916;0
WireConnection;996;1;901;0
WireConnection;996;2;1000;0
WireConnection;1088;1;1087;4
WireConnection;1088;2;1089;0
WireConnection;623;0;619;4
WireConnection;623;1;1025;0
WireConnection;623;2;624;0
WireConnection;1027;0;1029;0
WireConnection;1027;1;623;0
WireConnection;680;0;1028;0
WireConnection;680;1;681;0
WireConnection;798;0;1104;0
WireConnection;798;1;1030;0
WireConnection;798;2;1033;0
WireConnection;1002;0;1001;0
WireConnection;1002;1;875;0
WireConnection;1016;0;1012;0
WireConnection;908;0;775;0
WireConnection;908;1;996;0
WireConnection;908;2;1058;0
WireConnection;906;0;1003;0
WireConnection;906;1;901;0
WireConnection;906;2;904;0
WireConnection;1103;0;1107;0
WireConnection;1103;1;1030;0
WireConnection;1103;2;1033;0
WireConnection;955;0;956;0
WireConnection;955;1;960;0
WireConnection;1086;0;673;0
WireConnection;1086;1;1088;0
WireConnection;1121;0;1120;0
WireConnection;1121;1;951;0
WireConnection;997;0;917;0
WireConnection;997;1;906;0
WireConnection;997;2;1000;0
WireConnection;1106;0;798;0
WireConnection;1106;1;1103;0
WireConnection;1098;0;672;0
WireConnection;1098;1;1086;0
WireConnection;684;0;680;0
WireConnection;1109;0;806;0
WireConnection;1109;1;1030;0
WireConnection;1109;2;1033;0
WireConnection;683;0;1027;0
WireConnection;683;1;681;0
WireConnection;959;0;955;0
WireConnection;954;0;1121;0
WireConnection;954;1;960;0
WireConnection;1015;0;1013;0
WireConnection;1015;1;672;0
WireConnection;1015;2;1016;0
WireConnection;1114;0;1116;0
WireConnection;1114;1;798;0
WireConnection;1114;2;1115;0
WireConnection;994;0;792;0
WireConnection;994;1;908;0
WireConnection;994;2;995;0
WireConnection;762;0;1002;0
WireConnection;762;1;874;0
WireConnection;762;2;763;0
WireConnection;1110;0;1103;0
WireConnection;1110;1;1109;0
WireConnection;989;0;977;0
WireConnection;989;1;959;0
WireConnection;989;2;991;0
WireConnection;914;0;762;0
WireConnection;914;1;997;0
WireConnection;685;0;683;0
WireConnection;958;0;954;0
WireConnection;1117;1;1106;0
WireConnection;1117;2;1115;0
WireConnection;990;1;959;0
WireConnection;990;2;991;0
WireConnection;671;0;684;0
WireConnection;671;1;994;0
WireConnection;671;2;1114;0
WireConnection;671;3;1015;0
WireConnection;671;4;1098;0
WireConnection;671;5;781;0
WireConnection;671;6;672;4
WireConnection;844;0;823;0
WireConnection;844;1;822;0
WireConnection;982;0;989;0
WireConnection;982;1;685;0
WireConnection;1008;0;1015;0
WireConnection;1008;1;958;0
WireConnection;1008;2;1009;0
WireConnection;993;0;793;0
WireConnection;993;1;914;0
WireConnection;993;2;995;0
WireConnection;785;0;671;0
WireConnection;785;1;786;0
WireConnection;843;0;671;0
WireConnection;843;1;844;0
WireConnection;843;2;1117;0
WireConnection;856;0;854;0
WireConnection;856;1;855;0
WireConnection;1118;1;1110;0
WireConnection;1118;2;1115;0
WireConnection;983;0;990;0
WireConnection;983;1;685;0
WireConnection;787;0;785;0
WireConnection;787;1;788;0
WireConnection;787;2;789;0
WireConnection;787;3;789;4
WireConnection;852;0;843;0
WireConnection;852;1;856;0
WireConnection;852;2;1118;0
WireConnection;980;0;983;0
WireConnection;980;1;982;0
WireConnection;980;2;981;0
WireConnection;669;0;685;0
WireConnection;669;1;670;0
WireConnection;669;2;993;0
WireConnection;669;3;1114;0
WireConnection;669;4;672;4
WireConnection;669;5;781;4
WireConnection;992;1;1008;0
WireConnection;992;2;991;0
WireConnection;784;0;852;0
WireConnection;784;1;787;0
WireConnection;784;2;992;0
WireConnection;1119;0;980;0
WireConnection;1119;1;669;0
WireConnection;701;0;702;0
WireConnection;621;0;784;0
WireConnection;621;3;1119;0
WireConnection;700;0;621;0
WireConnection;700;1;701;0
WireConnection;617;0;700;0
ASEEND*/
//CHKSM=A72D4E984B367B97CED4D748F1F5C74D6B0D0355