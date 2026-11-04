// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Test/极简02_1"
{
	Properties
	{
		[Enum(add,1,blend,10)]_Blendmode("混合模式", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_Float22("剔除模式", Float) = 0
		_Float6("整体Alpha强度", Float) = 1
		_Float5("软粒子（相机需开启投影）", Float) = 0
		[Toggle]_Float85("深度写入（模型特效适用）", Float) = 0
		[Enum(open,0,close,4)]_Float18("显示最前面", Float) = 4
		[Main(z9,_KEYWORD,off,off)]_Float82("Custom列表", Float) = 0
		[SubToggle(z9,_)]_Float4("Custom1XY控制主贴图uv", Float) = 0
		[SubToggle(z9,_)]_Float38("Custom1Z控制溶解程度", Float) = 0
		[SubToggle(z9,_)]_Float83("Custom1W控制颜色强度", Float) = 1
		[SubToggle(z9,_)]_Float84("Custom2XY控制附加溶解贴图uv", Float) = 0
		[SubToggle(z9,_)]_Float92("XY控制溶解贴图uv加溶解贴图uv", Float) = 0
		[SubToggle(z9,_)]_Float86("Custom2Z控制centerGlow(遮罩开关全部打开)", Float) = 0
		[Main(z1,_KEYWORD,off,off)]_Float24("主贴图列表", Float) = 0
		[HDR][Sub(z1)]_Color0("颜色", Color) = (1,1,1,1)
		[HDR][Sub(z1)]_Color5("次颜色", Color) = (1,0,0,1)
		[Sub(z1)]_Float72("渐变颜色power", Float) = 0
		[KWEnum(z1,U,_0,V,_1)]_Float71("渐变方向", Float) = 0
		[Sub(z1)]_Float7("颜色强度", Float) = 1
		[Sub(z1)]_Main("主贴图", 2D) = "white" {}
		[KWEnum(z1,RGB,_0,A,_1)]_Float0("主贴图通道", Float) = 0
		[SubToggle(z1,_)]_Float73("主贴图去色", Float) = 0
		[KWEnum(z1,off,_0,on,_1)]_Float1("主贴图xclamp", Float) = 0
		[KWEnum(z1,off,_0,on,_1)]_Float2("主贴图yclamp", Float) = 0
		[Sub(z1)]_Float74("贴图强度", Float) = 1
		[Sub(z1)]_Float3("主贴图power", Float) = 1
		[Sub(z1)]_Float20("主贴图旋转", Range( 0 , 360)) = 0
		[Sub(z1)]_Vector4("主贴图UV", Vector) = (0,0,0,0)
		[SubToggle(z1,_)]_Float9("极坐标开关", Float) = 0
		[SubToggle(z1,_)]_Float8("主贴图极坐标uv翻转", Float) = 0
		[Sub(z1)]_Vector1("主贴图极坐标中心点与重复度", Vector) = (0.5,0.5,1,1)
		[Sub(z1)]_Vector0("主贴图极坐标uv偏移", Vector) = (0,0,0,0)
		[Sub(z1)]_Float11("旋涡强度", Float) = 0
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
		[Sub(z2)]_Mask1("m贴图", 2D) = "white" {}
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
		[SubToggle(z3,_)]_Float81("扰动主贴图", Float) = 0
		[SubToggle(z3,_)]_Float80("扰动遮罩", Float) = 0
		[SubToggle(z3,_)]_Float79("扰动附加遮罩", Float) = 0
		[SubToggle(z3,_)]_Float96("扰动两张溶解", Float) = 0
		[SubToggle(z3,_)]_Float99("扰动叠加贴图", Float) = 0
		[Sub(z3)]_Float13("扰动强度", Float) = 0
		[Sub(z3)]_Vector2("扰动uv", Vector) = (0,0,0,0)
		[Main(z4,_KEYWORD,off,off)]_Float27("溶解列表", Float) = 0
		[SubToggle(z4,_)]_Float106("溶解开关", Float) = 0
		[Sub(z4)]_Dissolve("溶解贴图", 2D) = "white" {}
		[Sub(z4)]_Float78("溶解贴图旋转", Range( 0 , 360)) = 0
		[Sub(z4)]_qd("溶解贴图强度", Float) = 1
		[Sub(z4)]_Float28("溶解程度", Range( -0.2 , 1.2)) = -0.2
		[Sub(z4)]_Float35("软（左）硬（右）程度", Range( -0.5 , 0.5)) = -0.5
		[Sub(z4)]_Color2("外层亮边颜色", Color) = (1,1,1,1)
		[Sub(z4)]_Float37("外层亮边颜色强度", Float) = 1
		[Sub(z4)]_Float108("外层亮边宽度", Float) = 0
		[Sub(z4)]_Color3("内层亮边2颜色", Color) = (1,1,1,1)
		[Sub(z4)]_Float39("内层亮边2颜色强度", Float) = 1
		[Sub(z4)]_Float36("内层亮边宽度", Float) = 0
		[Sub(z4)]_Vector5("溶解贴图uv", Vector) = (0,0,0,0)
		[SubToggle(z4,_)]_Float55("附加溶解贴图开关", Float) = 0
		[Sub(z4)]_Dissolve1("附加溶解贴图", 2D) = "white" {}
		[Sub(z4)]_Float77("附加溶解贴图旋转", Float) = 0
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
		[KWEnum(z5,Add,_0,Mul,_1)]_Float59("通道算法（Add时为叠加）", Float) = 0
		[Sub(z5)]_Float68("叠加贴图强度", Float) = 1
		[Sub(z5)]_Float54("叠加贴图power", Float) = 1
		[Sub(z5)]_Float56("叠加旋转", Range( 0 , 360)) = 0
		[Sub(z5)]_Vector7("叠加uv", Vector) = (0,0,0,0)
		[Main(z7,_KEYWORD,off,off)]_Float69("非尼尔列表", Float) = 0
		[SubToggle(z7,_)]_Float67("菲尼尔开关", Float) = 0
		[Sub(z7)]_Float64("菲尼尔power", Float) = 1
		[Sub(z7)]_Float63("菲尼尔范围", Float) = 1
		[SubToggle(z7,_)]_Float65("反向菲尼尔（虚化边缘）", Float) = 0
		[Main(z8,_KEYWORD,off,off)]_Float70("顶点偏移列表", Float) = 0
		[SubToggle(z8,_)]_Float101("顶点偏移开关", Float) = 0
		[Sub(z8)]_Vertex3("顶点偏移贴图", 2D) = "white" {}
		[SubToggle(z8,_)]_Float87("模型顶点法向", Float) = 0
		[Sub(z8)]_Float90("顶点贴图旋转", Range( 0 , 360)) = 0
		[Sub(z8)]_Float89("顶点偏移强度", Float) = 0
		[SubToggle(z8,_)]_Float98("顶点偏移强度自定义", Float) = 0
		[Sub(z8)]_Vector13("顶点偏移轴向", Vector) = (0,0,0,0)
		[Sub(z8)]_Vector12("顶点贴图UV", Vector) = (0,0,0,0)
		[Sub(z8)]_TextureSample8("顶点遮罩", 2D) = "white" {}
		[KWEnum(z8,Off,_0,On,_1)]_Float103("顶点MxClamp", Float) = 0
		[KWEnum(z8,Off,_0,On,_1)]_Float102("顶点MyClamp", Float) = 0
		[Sub(z8)]_Float100("顶点偏移M旋转", Range( 0 , 360)) = 0
		[ASEEnd][Sub(z8)]_Vector14("顶点偏移MUV", Vector) = (0,0,0,0)

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

			#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_D3D9)
			#define FRONT_FACE_SEMANTIC VFACE
			#define FRONT_FACE_TYPE float
			#else
			#define FRONT_FACE_SEMANTIC SV_IsFrontFace
			#define FRONT_FACE_TYPE bool
			#endif


			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityStandardBRDF.cginc"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float3 ase_normal : NORMAL;
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
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Float26;
			uniform float _Float22;
			uniform float _Float82;
			uniform float _Float24;
			uniform float _Float40;
			uniform float _Float70;
			uniform float _Float27;
			uniform float _Float18;
			uniform float _Float25;
			uniform float _Float31;
			uniform float _Blendmode;
			uniform float _Float69;
			uniform float _Float85;
			uniform sampler2D _Vertex3;
			uniform float2 _Vector12;
			uniform float _Float90;
			uniform float _Float89;
			uniform float _Float98;
			uniform float3 _Vector13;
			uniform float _Float87;
			uniform sampler2D _TextureSample8;
			uniform float _Float100;
			uniform float2 _Vector14;
			uniform float _Float103;
			uniform float _Float102;
			uniform float _Float101;
			uniform sampler2D _Main;
			uniform float4 _Main_ST;
			uniform float _Float4;
			uniform float _Float20;
			uniform float2 _Vector4;
			uniform float _Float11;
			uniform float4 _Vector1;
			uniform float2 _Vector0;
			uniform float _Float9;
			uniform float _Float1;
			uniform float _Float2;
			uniform float _Float8;
			uniform sampler2D _Noise;
			uniform float4 _Noise_ST;
			uniform float2 _Vector2;
			uniform float _Float12;
			uniform float _Float13;
			uniform float _Float81;
			uniform float _Float73;
			uniform float _Float74;
			uniform float _Float3;
			uniform sampler2D _Mask;
			uniform float _Float80;
			uniform float4 _Mask_ST;
			uniform float _Float15;
			uniform float2 _Vector3;
			uniform float _Float41;
			uniform float _Float42;
			uniform float _Float16;
			uniform float _Float43;
			uniform sampler2D _Mask1;
			uniform float _Float79;
			uniform float4 _Mask1_ST;
			uniform float _Float44;
			uniform float2 _Vector6;
			uniform float _Float45;
			uniform float _Float46;
			uniform float _Float47;
			uniform float _Float48;
			uniform float _Float62;
			uniform float _Float86;
			uniform float _Float61;
			uniform float _Float35;
			uniform sampler2D _Dissolve;
			uniform float4 _Dissolve_ST;
			uniform float _Float92;
			uniform float _Float78;
			uniform float2 _Vector5;
			uniform float _Float96;
			uniform float _qd;
			uniform sampler2D _Dissolve1;
			uniform float4 _Dissolve1_ST;
			uniform float _Float84;
			uniform float _Float77;
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
			uniform float _Float108;
			uniform float _Float106;
			uniform float4 _Color5;
			uniform float4 _Color0;
			uniform float _Float71;
			uniform float _Float72;
			uniform float _Float7;
			uniform float _Float83;
			uniform float _Float63;
			uniform float _Float64;
			uniform float _Float65;
			uniform float _Float67;
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
			uniform float _Float68;
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

				float2 texCoord1024 = v.ase_texcoord * float2( 1,1 ) + float2( 0,0 );
				float cos1032 = cos( radians( _Float90 ) );
				float sin1032 = sin( radians( _Float90 ) );
				float2 rotator1032 = mul( texCoord1024 - float2( 0,0 ) , float2x2( cos1032 , -sin1032 , sin1032 , cos1032 )) + float2( 0,0 );
				float2 panner1037 = ( 1.0 * _Time.y * _Vector12 + rotator1032);
				float4 tex2DNode1049 = tex2Dlod( _Vertex3, float4( panner1037, 0, 0.0) );
				float4 texCoord1035 = v.ase_texcoord1;
				texCoord1035.xy = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1045 = lerp( _Float89 , texCoord1035.w , _Float98);
				float3 lerpResult1034 = lerp( float3( 0,0,0 ) , v.ase_normal , _Float87);
				float2 texCoord1006 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float cos1008 = cos( radians( _Float100 ) );
				float sin1008 = sin( radians( _Float100 ) );
				float2 rotator1008 = mul( texCoord1006 - float2( 0.5,0.5 ) , float2x2( cos1008 , -sin1008 , sin1008 , cos1008 )) + float2( 0.5,0.5 );
				float2 temp_output_1011_0 = ( rotator1008 + ( _Vector14 * _Time.y ) );
				float temp_output_1015_0 = (temp_output_1011_0).x;
				float lerpResult1028 = lerp( temp_output_1015_0 , saturate( temp_output_1015_0 ) , _Float103);
				float temp_output_1013_0 = (temp_output_1011_0).y;
				float lerpResult1033 = lerp( temp_output_1013_0 , saturate( temp_output_1013_0 ) , _Float102);
				float2 appendResult1041 = (float2(lerpResult1028 , lerpResult1033));
				float4 tex2DNode1047 = tex2Dlod( _TextureSample8, float4( appendResult1041, 0, 0.0) );
				float3 lerpResult1053 = lerp( float3( 0,0,0 ) , ( tex2DNode1049.r * lerpResult1045 * ( _Vector13 + lerpResult1034 ) * tex2DNode1047.r * tex2DNode1049.a * tex2DNode1047.a ) , _Float101);
				float3 Vertex1054 = lerpResult1053;
				
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord4.xyz = ase_worldNormal;
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord5 = screenPos;
				
				o.ase_texcoord1 = v.ase_texcoord;
				o.ase_texcoord2 = v.ase_texcoord1;
				o.ase_texcoord3 = v.ase_texcoord2;
				o.ase_color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = Vertex1054;
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
			
			fixed4 frag (v2f i , FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC) : SV_Target
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
				float2 temp_output_649_0 = ( _Vector4 * _Time.y );
				float cos735 = cos( ( ( ( 1.0 - length( (uv_Main*2.0 + -1.0) ) ) * 2.0 * _Float11 ) * UNITY_PI ) );
				float sin735 = sin( ( ( ( 1.0 - length( (uv_Main*2.0 + -1.0) ) ) * 2.0 * _Float11 ) * UNITY_PI ) );
				float2 rotator735 = mul( rotator648 - float2( 0.5,0.5 ) , float2x2( cos735 , -sin735 , sin735 , cos735 )) + float2( 0.5,0.5 );
				float2 appendResult728 = (float2(_Vector1.x , _Vector1.y));
				float2 CenteredUV15_g1 = ( rotator735 - appendResult728 );
				float2 break17_g1 = CenteredUV15_g1;
				float2 appendResult23_g1 = (float2(( length( CenteredUV15_g1 ) * _Vector1.z * 2.0 ) , ( atan2( break17_g1.x , break17_g1.y ) * ( 1.0 / 6.28318548202515 ) * _Vector1.w )));
				float2 lerpResult716 = lerp( ( rotator648 + temp_output_649_0 ) , ( appendResult23_g1 + _Vector0 + temp_output_649_0 ) , _Float9);
				float temp_output_698_0 = (lerpResult716).x;
				float lerpResult656 = lerp( temp_output_698_0 , saturate( temp_output_698_0 ) , _Float1);
				float temp_output_699_0 = (lerpResult716).y;
				float lerpResult658 = lerp( temp_output_699_0 , saturate( temp_output_699_0 ) , _Float2);
				float2 appendResult642 = (float2(lerpResult656 , lerpResult658));
				float2 appendResult710 = (float2((appendResult642).y , (appendResult642).x));
				float2 lerpResult711 = lerp( appendResult642 , appendResult710 , _Float8);
				float2 uv_Noise = i.ase_texcoord1.xy * _Noise_ST.xy + _Noise_ST.zw;
				float4 tex2DNode745 = tex2D( _Noise, ( uv_Noise + ( _Vector2 * _Time.y ) ) );
				float2 appendResult1089 = (float2(tex2DNode745.r , tex2DNode745.g));
				float2 lerpResult748 = lerp( float2( 0,0 ) , ( appendResult1089 * tex2DNode745.a ) , _Float12);
				float2 temp_output_750_0 = ( lerpResult748 * _Float13 );
				float2 lerpResult1132 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float81);
				float4 tex2DNode619 = tex2D( _Main, ( lerpResult711 + lerpResult1132 ) );
				float grayscale1106 = (tex2DNode619.rgb.r + tex2DNode619.rgb.g + tex2DNode619.rgb.b) / 3;
				float4 temp_cast_1 = (grayscale1106).xxxx;
				float4 lerpResult1107 = lerp( tex2DNode619 , temp_cast_1 , _Float73);
				float4 temp_cast_2 = (_Float3).xxxx;
				float4 temp_cast_3 = (1.0).xxxx;
				float2 lerpResult1127 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float80);
				float2 NoisMask11129 = lerpResult1127;
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
				float4 tex2DNode758 = tex2D( _Mask, ( NoisMask11129 + appendResult873 ) );
				float temp_output_949_0 = abs( _Float16 );
				float4 temp_cast_4 = (temp_output_949_0).xxxx;
				float4 temp_output_775_0 = saturate( ( pow( tex2DNode758 , temp_cast_4 ) * _Float43 ) );
				float4 temp_cast_5 = (1.0).xxxx;
				float2 lerpResult1125 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float79);
				float2 NoisMask21131 = lerpResult1125;
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
				float4 tex2DNode895 = tex2D( _Mask1, ( NoisMask21131 + appendResult894 ) );
				float temp_output_950_0 = abs( _Float47 );
				float4 temp_cast_6 = (temp_output_950_0).xxxx;
				float4 temp_output_901_0 = saturate( ( pow( tex2DNode895 , temp_cast_6 ) * _Float48 ) );
				float4 lerpResult996 = lerp( temp_cast_5 , temp_output_901_0 , _Float62);
				float4 temp_cast_7 = (1.0).xxxx;
				float4 texCoord1153 = i.ase_texcoord3;
				texCoord1153.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1155 = lerp( 1.0 , texCoord1153.z , _Float86);
				float4 temp_cast_8 = (lerpResult1155).xxxx;
				float4 clampResult1157 = clamp( ( tex2DNode895 - temp_cast_8 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 clampResult1160 = clamp( ( tex2DNode895 * clampResult1157 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 lerpResult1161 = lerp( temp_cast_7 , clampResult1160 , _Float86);
				float4 lerpResult994 = lerp( temp_cast_3 , ( temp_output_775_0 * lerpResult996 * lerpResult1161 ) , _Float61);
				float clampResult1186 = clamp( _Float35 , -0.5 , 0.5 );
				float temp_output_1187_0 = ( 1.0 - clampResult1186 );
				float2 uv_Dissolve = i.ase_texcoord1.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float4 texCoord1146 = i.ase_texcoord3;
				texCoord1146.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult1147 = (float2(texCoord1146.x , texCoord1146.y));
				float2 lerpResult1164 = lerp( float2( 0,0 ) , appendResult1147 , _Float92);
				float cos1120 = cos( radians( _Float78 ) );
				float sin1120 = sin( radians( _Float78 ) );
				float2 rotator1120 = mul( ( uv_Dissolve + lerpResult1164 ) - float2( 0,0 ) , float2x2( cos1120 , -sin1120 , sin1120 , cos1120 )) + float2( 0,0 );
				float2 lerpResult1170 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float96);
				float2 NoisRJ1171 = lerpResult1170;
				float4 tex2DNode795 = tex2D( _Dissolve, ( ( rotator1120 + ( _Vector5 * _Time.y ) ) + NoisRJ1171 ) );
				float2 uv_Dissolve1 = i.ase_texcoord1.xy * _Dissolve1_ST.xy + _Dissolve1_ST.zw;
				float2 lerpResult1149 = lerp( float2( 0,0 ) , appendResult1147 , _Float84);
				float cos1116 = cos( radians( _Float77 ) );
				float sin1116 = sin( radians( _Float77 ) );
				float2 rotator1116 = mul( ( uv_Dissolve1 + lerpResult1149 ) - float2( 0,0 ) , float2x2( cos1116 , -sin1116 , sin1116 , cos1116 )) + float2( 0,0 );
				float lerpResult932 = lerp( 1.0 , ( tex2D( _Dissolve1, ( NoisRJ1171 + ( rotator1116 + ( _aegrdfgaergare * _Time.y ) ) ) ).r * _Float541111111111111111111 ) , _Float55);
				float lerpResult944 = lerp( ( tex2DNode795.r * tex2DNode795.a * _qd ) , lerpResult932 , _Float30);
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
				float smoothstepResult798 = smoothstep( clampResult1186 , temp_output_1187_0 , ( temp_output_806_0 + _Float108 ));
				float lerpResult1194 = lerp( 1.0 , smoothstepResult798 , _Float106);
				float2 texCoord1098 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1100 = lerp( texCoord1098.x , texCoord1098.y , _Float71);
				float4 lerpResult1095 = lerp( _Color5 , _Color0 , saturate( pow( ( 1.0 - lerpResult1100 ) , _Float72 ) ));
				float4 texCoord1142 = i.ase_texcoord1;
				texCoord1142.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1143 = lerp( 0.0 , texCoord1142.w , _Float83);
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = Unity_SafeNormalize( ase_worldViewDir );
				float3 ase_worldNormal = i.ase_texcoord4.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float fresnelNdotV1026 = dot( normalize( ( normalizedWorldNormal * ase_vface ) ), ase_worldViewDir );
				float fresnelNode1026 = ( 0.0 + _Float63 * pow( max( 1.0 - fresnelNdotV1026 , 0.0001 ), _Float64 ) );
				float temp_output_1027_0 = saturate( fresnelNode1026 );
				float lerpResult1044 = lerp( temp_output_1027_0 , ( 1.0 - temp_output_1027_0 ) , _Float65);
				float lerpResult1051 = lerp( 1.0 , lerpResult1044 , _Float67);
				float4 temp_output_671_0 = ( saturate( pow( ( lerpResult1107 * _Float74 ) , temp_cast_2 ) ) * lerpResult994 * lerpResult1194 * lerpResult1095 * ( _Color0 * ( _Float7 + lerpResult1143 ) ) * i.ase_color * _Color0.a * lerpResult1051 );
				float smoothstepResult1197 = smoothstep( clampResult1186 , temp_output_1187_0 , ( temp_output_806_0 + _Float36 ));
				float lerpResult1191 = lerp( 0.0 , ( smoothstepResult798 - smoothstepResult1197 ) , _Float106);
				float4 lerpResult843 = lerp( temp_output_671_0 , ( _Float37 * _Color2 ) , lerpResult1191);
				float smoothstepResult1189 = smoothstep( clampResult1186 , temp_output_1187_0 , temp_output_806_0);
				float lerpResult1193 = lerp( 0.0 , ( smoothstepResult1197 - smoothstepResult1189 ) , _Float106);
				float4 lerpResult852 = lerp( lerpResult843 , ( _Float39 * _Color3 ) , lerpResult1193);
				float4 temp_cast_9 = (_Float17).xxxx;
				float2 uv_TextureSample0 = i.ase_texcoord1.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
				float cos986 = cos( radians( _Float56 ) );
				float sin986 = sin( radians( _Float56 ) );
				float2 rotator986 = mul( uv_TextureSample0 - float2( 0.5,0.5 ) , float2x2( cos986 , -sin986 , sin986 , cos986 )) + float2( 0.5,0.5 );
				float2 lerpResult1178 = lerp( float2( 0,0 ) , temp_output_750_0 , _Float99);
				float2 Noisdiejia1176 = lerpResult1178;
				float4 tex2DNode951 = tex2D( _TextureSample0, ( ( rotator986 + ( _Vector7 * _Time.y ) ) + Noisdiejia1176 ) );
				float4 temp_cast_10 = (_Float54).xxxx;
				float4 lerpResult992 = lerp( float4( 0,0,0,0 ) , saturate( ( lerpResult1095 * pow( ( _Color4 * tex2DNode951 ) , temp_cast_10 ) * _Float68 ) ) , _Float60);
				float4 temp_cast_12 = (tex2DNode951.a).xxxx;
				float4 lerpResult956 = lerp( temp_cast_12 , tex2DNode951 , _Float57);
				float4 temp_cast_13 = (_Float54).xxxx;
				float4 temp_output_959_0 = saturate( pow( lerpResult956 , temp_cast_13 ) );
				float4 lerpResult990 = lerp( float4( 0,0,0,0 ) , temp_output_959_0 , _Float60);
				float4 temp_cast_14 = (tex2DNode619.a).xxxx;
				float4 lerpResult623 = lerp( temp_cast_14 , lerpResult1107 , _Float0);
				float4 temp_cast_15 = (_Float3).xxxx;
				float4 temp_output_685_0 = saturate( pow( ( lerpResult623 * _Float74 ) , temp_cast_15 ) );
				float4 temp_cast_16 = (1.0).xxxx;
				float4 lerpResult989 = lerp( temp_cast_16 , temp_output_959_0 , _Float60);
				float4 lerpResult980 = lerp( ( lerpResult990 + temp_output_685_0 ) , ( lerpResult989 * temp_output_685_0 ) , _Float59);
				float4 temp_cast_17 = (1.0).xxxx;
				float4 temp_cast_18 = (( pow( tex2DNode758.a , temp_output_949_0 ) * _Float43 )).xxxx;
				float4 lerpResult762 = lerp( temp_cast_18 , temp_output_775_0 , _Float14);
				float4 temp_cast_19 = (1.0).xxxx;
				float4 temp_cast_20 = (( pow( tex2DNode895.a , temp_output_950_0 ) * _Float48 )).xxxx;
				float4 lerpResult906 = lerp( temp_cast_20 , temp_output_901_0 , _Float50);
				float4 lerpResult997 = lerp( temp_cast_19 , lerpResult906 , _Float62);
				float4 lerpResult993 = lerp( temp_cast_17 , ( lerpResult762 * lerpResult997 ) , _Float61);
				float4 appendResult621 = (float4(( lerpResult852 + ( pow( temp_output_671_0 , temp_cast_9 ) * _Float19 * _Color1 * _Color1.a ) + lerpResult992 ).rgb , ( lerpResult980 * ( temp_output_685_0 * _Float6 * lerpResult993 * lerpResult1194 * _Color0.a * i.ase_color.a * lerpResult1051 ) ).r));
				float4 screenPos = i.ase_texcoord5;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth701 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float distanceDepth701 = abs( ( screenDepth701 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Float5 ) );
				
				
				finalColor = ( appendResult621 * saturate( distanceDepth701 ) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "LWGUI.LWGUI"
	
	
}
/*ASEBEGIN
Version=18935
130.4;251.2;1890.4;689.4;877.9584;1732.787;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;715;-5364.063,-835.6169;Inherit;False;1015.928;565.5943;;9;643;644;688;646;647;645;649;648;650;主贴图uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;705;-6497.776,-717.094;Inherit;False;834.0901;349;;4;687;694;691;692;主贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;644;-5293,-785.617;Inherit;False;0;619;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;744;-5733.526,-158.7803;Inherit;False;1389.171;378.1163;;8;742;741;739;740;738;737;743;735;旋涡旋转;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;687;-6447.776,-647.6968;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;742;-5683.526,-68.88614;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1180;-4211.143,530.9204;Inherit;False;1952.047;1143.714;;26;755;753;754;752;756;745;1089;1090;749;748;751;750;1172;1124;1170;1125;1171;1131;1126;1127;1129;1133;1177;1178;1132;1176;noise扰动;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;691;-6123.868,-635.1345;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;694;-6199.687,-484.094;Inherit;False;Property;_Float4;Custom1XY控制主贴图uv;7;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;741;-5449.497,-72.66394;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;753;-4148.143,743.9204;Inherit;False;Property;_Vector2;扰动uv;65;0;Create;False;0;0;0;True;1;Sub(z3);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;692;-5847.688,-667.094;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;739;-5216.004,3.083937;Inherit;False;Constant;_Float10;Float 10;25;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;738;-5232.82,-75.80936;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;755;-4161.143,863.9204;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;740;-5213.497,103.3361;Inherit;False;Property;_Float11;旋涡强度;32;0;Create;False;0;0;0;True;1;Sub(z1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;643;-5314.063,-603.1763;Inherit;False;Property;_Float20;主贴图旋转;26;0;Create;False;0;0;0;True;1;Sub(z1);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;646;-5015.499,-598.4365;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;734;-4237.034,-163.8523;Inherit;False;1242.484;580.2072;;7;716;727;714;728;730;729;717;主贴图极坐标1;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;737;-4975.819,-68.80936;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;688;-4963.089,-729.0996;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;754;-3942.143,789.9204;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;752;-4152.143,580.9204;Inherit;False;0;745;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;648;-4770.256,-699.1042;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;727;-4187.034,-18.61925;Inherit;False;Property;_Vector1;主贴图极坐标中心点与重复度;30;0;Create;False;0;0;0;True;1;Sub(z1);False;0.5,0.5,1,1;0.5,0.5,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PiNode;743;-4766.841,-51.08536;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;756;-3778.143,681.9204;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;647;-4973.163,-512.2715;Inherit;False;Property;_Vector4;主贴图UV;27;0;Create;False;0;0;0;True;1;Sub(z1);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;645;-4976.829,-381.0228;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;735;-4558.355,-108.7803;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1138;-7296.737,2892.699;Inherit;False;4153.864;645.6753;;35;896;895;950;899;898;900;1000;916;901;1087;996;1088;904;917;906;997;878;879;882;880;881;883;884;885;886;887;891;888;890;889;893;892;894;1136;1137;附加遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;728;-3879.784,-2.731768;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;745;-3630.866,628.3441;Inherit;True;Property;_Noise;扰动贴图;58;0;Create;False;0;0;0;True;1;Sub(z3);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;714;-3689.707,-3.517656;Inherit;False;Polar Coordinates;-1;;1;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;730;-3879.287,148.0216;Inherit;False;Property;_Vector0;主贴图极坐标uv偏移;31;0;Create;False;0;0;0;True;1;Sub(z1);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;878;-7246.737,3107.699;Inherit;False;Property;_Float44;m旋转;54;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1167;-9419.111,6063.063;Inherit;False;841.3867;513.4023;;6;1146;1147;1149;1164;1148;1165;溶解贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;1089;-3254.931,634.6448;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;649;-4717.283,-538.9414;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;650;-4501.136,-611.7255;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;879;-7035.738,2942.699;Inherit;False;0;895;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RadiansOpNode;882;-6896.742,3110.699;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1146;-9369.111,6293.863;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;881;-6885.742,3217.699;Inherit;False;Property;_Vector6;muv;55;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;1134;-6434.946,1908.808;Inherit;False;4396.683;807.3033;;37;774;949;758;875;773;874;1085;775;763;995;792;1086;908;762;994;793;914;993;771;767;766;764;770;769;768;765;866;865;869;870;872;871;867;868;873;1128;1130;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;717;-3674.438,-91.66425;Inherit;False;Property;_Float9;极坐标开关;28;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;749;-3090.481,851.979;Inherit;False;Property;_Float12;扰动开关;57;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;729;-3405.737,75.23145;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;880;-6897.742,3355.699;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1090;-3068.931,649.6448;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;883;-6666.741,3039.699;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;771;-6384.946,2132.705;Inherit;False;Property;_Float15;遮罩图旋转;45;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;884;-6629.741,3207.698;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1119;-8046.003,6257.698;Inherit;False;1901.868;533.6255;Comment;16;938;936;931;934;937;935;945;1117;939;1118;941;940;942;1116;1150;1175;附加溶解贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;1147;-9037.2,6310.025;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;751;-2838.585,789.5684;Inherit;False;Property;_Float13;扰动强度;64;0;Create;False;0;0;0;True;1;Sub(z3);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;718;-2939.585,-723.7425;Inherit;False;1031.703;551.2322;;9;699;698;657;659;654;655;658;656;642;主贴图不循环;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1148;-9113.021,6461.065;Inherit;False;Property;_Float84;Custom2XY控制附加溶解贴图uv;10;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;716;-3178.551,-113.8523;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;748;-2801.834,633.938;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;766;-6023.946,2242.704;Inherit;False;Property;_Vector3;遮罩uv;46;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ComponentMaskNode;699;-2887.275,-452.5966;Inherit;False;False;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;939;-8009.989,6298.6;Inherit;False;0;931;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1165;-9146.657,6188.258;Inherit;False;Property;_Float92;XY控制溶解贴图uv加溶解贴图uv;11;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1118;-7996.083,6520.193;Inherit;False;Property;_Float77;附加溶解贴图旋转;82;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1172;-3264.709,1409.364;Inherit;False;Property;_Float96;扰动两张溶解;62;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;764;-6173.946,1967.705;Inherit;False;0;758;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;698;-2889.585,-653.8093;Inherit;False;True;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;750;-2613.13,639.1024;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RadiansOpNode;770;-6034.946,2135.705;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;885;-6432.74,3137.699;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1149;-8759.725,6289.763;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1168;-7275.292,4886.913;Inherit;False;1060.153;666.6304;;9;1122;1121;840;839;1120;838;837;1166;836;溶解贴图UV;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;767;-6035.946,2380.704;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1150;-7740.691,6361.757;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;941;-7761.961,6697.882;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;940;-7739.193,6558.896;Inherit;False;Property;_aegrdfgaergare;附加溶解贴图uv;85;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;836;-7225.292,4936.913;Inherit;False;0;795;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1164;-8773.862,6113.063;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;886;-6260.186,3240.374;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;655;-2592.954,-375.9045;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1122;-7159.35,5206.455;Inherit;False;Property;_Float78;溶解贴图旋转;69;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;654;-2590.954,-614.9042;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;769;-5804.947,2064.705;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;768;-5767.947,2232.704;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;659;-2600.04,-288.5104;Inherit;False;Property;_Float2;主贴图yclamp;23;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;657;-2606.04,-538.5096;Inherit;False;Property;_Float1;主贴图xclamp;22;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1117;-7681.283,6484.991;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;887;-6256.186,3055.373;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1170;-3012.929,1367.642;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1124;-3253.787,1230.103;Inherit;False;Property;_Float79;扰动附加遮罩;61;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;765;-5570.947,2162.705;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;918;-7818.144,6853.026;Inherit;False;1591.615;427.6962;;11;925;924;923;922;921;920;919;927;928;929;926;方向溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.RadiansOpNode;1121;-6815.361,5197.653;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;658;-2317.04,-456.5097;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;891;-6049.184,3325.374;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;889;-5998.184,3069.373;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;890;-6059.184,3422.374;Inherit;False;Property;_Float46;myclamp;51;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;942;-7501.313,6573.94;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;840;-6880.537,5291.642;Inherit;False;Property;_Vector5;溶解贴图uv;79;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;1166;-6817.688,5013.65;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;888;-6066.184,3157.373;Inherit;False;Property;_Float45;mxclamp;50;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1171;-2803.158,1365.748;Inherit;False;NoisRJ;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1125;-3029.481,1189.601;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;656;-2345.259,-673.7425;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;1116;-7521.961,6377.882;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;839;-6896.734,5443.143;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;713;-1658.393,-581.4369;Inherit;False;744.8823;393.0596;;5;706;707;712;710;711;主贴图极坐标2;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;938;-7325.968,6466.695;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;1120;-6621.642,5146.145;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;920;-7773.145,7019.286;Inherit;False;Property;_Float94;方向V偏移;89;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;866;-5398.394,2265.379;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;893;-5742.184,3290.374;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;892;-5742.184,2990.373;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;642;-2072.882,-533.5682;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;865;-5394.394,2080.379;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;838;-6613.538,5322.642;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1126;-3251.325,1056.436;Inherit;False;Property;_Float80;扰动遮罩;60;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1131;-2806.238,1189.219;Inherit;False;NoisMask2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;919;-7770.145,6940.474;Inherit;False;Property;_Float93;方向U偏移;88;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1173;-8622.048,5744.384;Inherit;False;1171;NoisRJ;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;828;-5969.413,5049.518;Inherit;False;3867.203;1427.534;;16;810;809;800;829;834;835;930;944;932;1111;1181;1194;1195;1196;1191;1193;溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;1137;-5397.246,3024.436;Inherit;False;1131;NoisMask2;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;988;-1777.479,-1598.546;Inherit;False;4020.037;699.9189;;31;968;961;967;965;964;986;966;962;969;951;956;960;955;977;959;954;982;958;981;983;980;990;989;991;992;1092;1094;1169;1179;1201;1202;叠加;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1175;-7166.498,6419.71;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;837;-6367.538,5274.641;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;870;-5136.394,2094.379;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;707;-1608.393,-376.0345;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;872;-5197.394,2447.379;Inherit;False;Property;_Float42;遮罩yclamp;42;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;894;-5573.186,3106.373;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1151;-4743.379,3728.057;Inherit;False;1892.448;560.886;;10;1161;1160;1159;1158;1157;1156;1155;1154;1153;1152;CenterGlow;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;921;-7727.268,7154.724;Inherit;False;Property;_Float95;方向溶解旋转（默认左黑右白）;87;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;869;-5204.394,2182.379;Inherit;False;Property;_Float41;遮罩xclamp;41;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;871;-5187.394,2350.379;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;922;-7593.27,6962.725;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1127;-3027.019,1018.934;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;706;-1607.393,-478.0345;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;923;-7332.271,7153.724;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1129;-2810.842,1012.37;Inherit;False;NoisMask1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1136;-5144.855,3089.809;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1174;-6151.546,5277.637;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;867;-4880.394,2015.38;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;710;-1350.393,-453.0345;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;712;-1374.398,-304.3775;Inherit;False;Property;_Float8;主贴图极坐标uv翻转;29;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;924;-7416.268,6913.724;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;810;-5919.413,5187.714;Inherit;False;785.1409;280;;2;795;796;溶解贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1152;-4714.48,4171.073;Inherit;False;Property;_Float86;Custom2Z控制centerGlow(遮罩开关全部打开);12;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;968;-1788.879,-1380.346;Inherit;False;Property;_Float56;叠加旋转;98;0;Create;False;0;0;0;True;1;Sub(z5);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1154;-4474.646,3949.961;Inherit;False;Constant;_Float29;Float 29;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1153;-4718.456,3934.453;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;868;-4880.394,2315.379;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1133;-2658.993,830.5891;Inherit;False;Property;_Float81;扰动主贴图;59;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;931;-7022.354,6423.424;Inherit;True;Property;_Dissolve1;附加溶解贴图;81;0;Create;False;0;0;0;False;1;Sub(z4);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;936;-7008.395,6646.695;Inherit;False;Property;_Float541111111111111111111;附加溶解贴图强度;84;0;Create;False;0;0;0;True;1;Sub(z4);False;1;11;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1177;-3279.573,1537.434;Inherit;False;Property;_Float99;扰动叠加贴图;63;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;925;-7145.274,7026.725;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;934;-6555.614,6327.577;Inherit;False;Constant;_Float53;Float 53;69;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;873;-4711.394,2131.38;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;711;-1097.51,-531.4369;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1111;-5818.928,5476.107;Inherit;False;Property;_qd;溶解贴图强度;70;0;Create;False;0;0;0;True;1;Sub(z4);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1132;-2441.096,626.166;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1155;-4236.115,4092.471;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1130;-4555.031,1994.517;Inherit;False;1129;NoisMask1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1009;-1023.886,3117.784;Inherit;False;1475.065;723.4756;;14;1051;1048;1046;1044;1042;1038;1027;1026;1019;1018;1016;1014;1012;1010;菲尼尔;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;896;-4848.786,3316.94;Inherit;False;Property;_Float47;mpower;53;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;935;-6651.395,6487.695;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;964;-1445.879,-1270.346;Inherit;False;Property;_Vector7;叠加uv;99;0;Create;False;0;0;0;True;1;Sub(z5);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;895;-4846.552,3048.783;Inherit;True;Property;_Mask1;m贴图;48;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;795;-5904.612,5225.814;Inherit;True;Property;_Dissolve;溶解贴图;68;0;Create;False;0;0;0;True;1;Sub(z4);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1178;-3017.173,1517.434;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RadiansOpNode;967;-1462.879,-1384.346;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;961;-1604.88,-1549.346;Inherit;False;0;951;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1001;-1156.938,4857.515;Inherit;False;3297.479;1396.09;;38;1054;1053;1052;1050;1049;1047;1045;1043;1041;1040;1039;1037;1036;1035;1034;1033;1032;1031;1030;1029;1028;1025;1024;1023;1022;1021;1020;1017;1015;1013;1011;1008;1007;1006;1005;1004;1003;1002;顶点偏移;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;965;-1463.879,-1139.346;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;937;-6737.395,6641.695;Inherit;False;Property;_Float55;附加溶解贴图开关;80;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;800;-5556.483,5525.252;Inherit;False;Property;_Float28;溶解程度;71;0;Create;False;0;0;0;True;1;Sub(z4);False;-0.2;0;-0.2;1.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;829;-5688.991,5682.592;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;945;-6444.135,6650.674;Inherit;False;Property;_Float30;溶解纹理偏向;83;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;747;-793.5852,-458.4539;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1156;-3983.365,4067.649;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1128;-4355.458,2045.707;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1104;-630.7073,850.3853;Inherit;False;1328.171;1352.669;;16;978;1096;1095;1097;1098;1099;1100;1101;1102;1103;781;673;1142;1143;1144;1163;颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;774;-4107.62,2476.564;Inherit;False;Property;_Float16;遮罩power;44;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;809;-4695.581,5106.57;Inherit;False;1733.592;529.1241;;7;805;807;798;806;804;1198;1200;软边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;796;-5570.071,5268.635;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;986;-1218.367,-1480.282;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;686;-629.5935,-666.0858;Inherit;False;1736.342;788.0424;;14;619;624;623;683;680;684;685;681;1105;1106;1107;1108;1109;1110;主贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;835;-5368.463,5807.325;Inherit;False;Property;_Float38;Custom1Z控制溶解程度;8;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;926;-6873.207,7191.499;Inherit;False;Property;_Float97;方向溶解开关;86;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1002;-1106.938,5908.312;Inherit;False;Property;_Float100;顶点偏移M旋转;117;0;Create;False;0;0;0;False;1;Sub(z8);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.FaceVariableNode;1012;-894.595,3301.25;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;927;-6933.719,6988.86;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1176;-2802.773,1535.034;Inherit;False;Noisdiejia;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldNormalVector;1010;-1000.331,3161.146;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;966;-1199.879,-1222.346;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;928;-6860.256,6903.027;Inherit;False;Constant;_Float52;Float 52;32;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;932;-5908.796,6363.703;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;950;-4653.971,3321.078;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;929;-6581.902,7013.614;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;619;-595.5936,-573.4662;Inherit;True;Property;_Main;主贴图;19;0;Create;False;0;0;0;True;1;Sub(z1);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1181;-4595.495,5695.214;Inherit;False;2056.17;779.0845;;11;1197;1192;1190;1189;1188;1187;1186;1185;1184;1183;1182;亮边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;1157;-3781.526,4065.875;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1014;-983.3411,3691.077;Inherit;False;Property;_Float64;菲尼尔power;102;0;Create;False;0;0;0;True;1;Sub(z7);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;1016;-934.884,3374.303;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;1005;-772.186,5994.644;Inherit;False;Property;_Vector14;顶点偏移MUV;118;0;Create;False;0;0;0;False;1;Sub(z8);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;1098;-564.7074,1900.731;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RadiansOpNode;1004;-813.5209,5916.479;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;1003;-775.851,6127.894;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1179;-1147.224,-1083.399;Inherit;False;1176;Noisdiejia;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1019;-971.3411,3550.078;Inherit;False;Property;_Float63;菲尼尔范围;103;0;Create;False;0;0;0;True;1;Sub(z7);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;899;-4339.353,3048.914;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1006;-815.5159,5762.033;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;962;-954.8789,-1394.346;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1018;-677.595,3248.251;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;898;-4415.497,3188.696;Inherit;False;Property;_Float48;m强度;52;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;944;-5553.392,6313.087;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;834;-5033.32,5540.778;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;805;-4640.072,5149.813;Inherit;False;Constant;_Float33;Float 33;44;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;949;-3900.843,2474.78;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1099;-516.7073,2055.592;Inherit;False;Property;_Float71;渐变方向;17;0;Create;False;0;0;0;True;1;KWEnum(z1,U,_0,V,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;758;-4155.386,2108.407;Inherit;True;Property;_Mask;遮罩贴图;39;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;1008;-569.2792,5809.81;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1105;-301.2988,-497.0728;Inherit;False;Property;_Float73;主贴图去色;21;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1100;-260.7071,1927.592;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1182;-4478.645,5831.922;Inherit;False;Constant;_Float75;Float 75;86;0;Create;True;0;0;0;False;0;False;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;930;-5276.746,6297.028;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;900;-4094.494,3105.695;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;1026;-630.076,3428.106;Inherit;False;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1183;-4477.645,5908.922;Inherit;False;Constant;_Float32;Float 32;86;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;773;-3703.193,2081.539;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1007;-516.3073,5969.973;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCGrayscale;1106;-298.2988,-623.0728;Inherit;False;2;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1169;-808.9568,-1399.865;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1184;-4545.495,5745.214;Inherit;False;Property;_Float35;软（左）硬（右）程度;72;0;Create;False;0;0;0;True;1;Sub(z4);False;-0.5;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;804;-4353.162,5129.337;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;875;-3753.336,2284.32;Inherit;False;Property;_Float43;遮罩强度;43;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1158;-3522.81,4049.16;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;807;-4485.364,5273.422;Inherit;False;Constant;_Float34;Float 34;44;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;1087;-4353.224,3293.463;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1142;-595.8759,1290.516;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1102;-137.707,2051.593;Inherit;False;Property;_Float72;渐变颜色power;16;0;Create;False;0;0;0;True;1;Sub(z1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;806;-4146.418,5142.224;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1185;-3716.346,5982.819;Inherit;False;Property;_Float36;内层亮边宽度;78;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1144;-580.4985,1481.62;Inherit;False;Property;_Float83;Custom1W控制颜色强度;9;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;969;-478.9008,-1231.104;Inherit;False;Property;_Float57;叠加贴图通道;94;0;Create;False;0;0;0;True;1;KWEnum(z5,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1109;-9.370972,-423.2335;Inherit;False;Property;_Float74;贴图强度;24;0;Create;False;0;0;0;True;1;Sub(z1);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;1186;-4240.645,5790.922;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1027;-387.183,3430.11;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;874;-3403.337,2164.319;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1011;-300.1612,5897.188;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;951;-649.2002,-1432.299;Inherit;True;Property;_TextureSample0;叠加贴图;92;0;Create;False;0;0;0;True;1;Sub(z5);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1200;-3857.335,5298.433;Inherit;False;Property;_Float108;外层亮边宽度;75;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;901;-3933.739,3100.238;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;916;-3950.404,2957.963;Inherit;False;Constant;_Float49;Float 49;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;1160;-3356.63,4045.111;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1107;-123.5588,-555.8848;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;624;-239.4556,-248.7956;Inherit;False;Property;_Float0;主贴图通道;20;0;Create;False;0;0;0;True;1;KWEnum(z1,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1000;-3734.385,3401.973;Inherit;False;Property;_Float62;附加遮罩开关;47;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1101;-70.70697,1940.592;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1159;-3333.978,3790.275;Inherit;False;Constant;_Float76;Float 76;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;904;-4013.681,3386.029;Inherit;False;Property;_Float50;m通道;49;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;775;-3164.168,2149.408;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1188;-3505.758,5965.013;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1017;-37.36424,5140.197;Inherit;False;Property;_Float90;顶点贴图旋转;109;0;Create;False;0;0;0;False;1;Sub(z8);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1187;-3884.262,5790.062;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1198;-3639.771,5157.359;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;960;-91.69485,-1341.916;Inherit;False;Property;_Float54;叠加贴图power;97;0;Create;False;0;0;0;True;1;Sub(z5);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;1015;-110.4162,5907.604;Inherit;False;FLOAT;0;1;2;3;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1042;-293.1742,3532.294;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1108;300.8517,-560.8456;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1088;-4109.365,3282.244;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1201;-328.2344,-1563.6;Inherit;False;Property;_Color4;叠加贴图颜色;93;1;[HDR];Create;False;0;0;0;True;1;Sub(z5);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;1013;-128.4162,6024.604;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;1103;100.7738,1947.169;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;681;339.8939,-424.8678;Inherit;False;Property;_Float3;主贴图power;25;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;673;-311.3839,1203.029;Inherit;False;Property;_Float7;颜色强度;18;0;Create;False;0;0;0;True;1;Sub(z1);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;1085;-3639.524,2442.801;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;978;-135.7327,900.3853;Inherit;False;283;578.226;;2;672;1141;颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;1143;-314.8759,1357.516;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1038;-422.3387,3684.077;Inherit;False;Property;_Float65;反向菲尼尔（虚化边缘）;104;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;996;-3408.948,2962.307;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;956;-310.0049,-1355.748;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;623;79.0278,-291.8891;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1161;-3163.262,3794.114;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1044;-58.79682,3440.011;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;680;634.2279,-603.0858;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;792;-2882.807,1958.808;Inherit;False;Constant;_Float21;Float 21;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1021;229.6347,5069.198;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1022;59.58365,5821.604;Inherit;False;Property;_Float103;顶点MxClamp;115;0;Create;False;0;0;0;True;1;KWEnum(z8,Off,_0,On,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1195;-2631.838,5519.229;Inherit;False;Property;_Float106;溶解开关;67;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;672;-266.4306,957.6713;Inherit;False;Property;_Color0;颜色;14;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;1197;-3205.9,5862.395;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1196;-2706.346,5151.952;Inherit;False;Constant;_Float88;Float 88;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1097;300.483,1868.138;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1202;-17.1582,-1492.788;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1110;407.8691,-286.0239;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;995;-2705.949,2471.988;Inherit;False;Property;_Float61;遮罩开关;38;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1046;-93.99066,3320.031;Inherit;False;Constant;_Float66;Float 66;94;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1048;-64.33881,3663.077;Inherit;False;Property;_Float67;菲尼尔开关;101;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1141;-88.87585,1212.516;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1024;116.8146,4907.515;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;798;-3215.987,5156.569;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;763;-3239.427,2462.923;Inherit;False;Property;_Float14;遮罩通道;40;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;955;187.963,-1217.985;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;906;-3749.847,3259.297;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;1023;161.5836,5914.604;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;948;2300.726,-113.3147;Inherit;False;911.7981;680.3812;;8;822;823;844;843;855;854;856;852;溶解亮边;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1086;-3391.667,2374.583;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;908;-2679.296,2158.219;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1025;144.5836,6138.604;Inherit;False;Property;_Float102;顶点MyClamp;116;0;Create;False;0;0;0;True;1;KWEnum(z8,Off,_0,On,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1096;-117.2589,1698.16;Inherit;False;Property;_Color5;次颜色;15;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,0,0,1;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;1020;65.58365,6042.604;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;917;-3567.581,3169.631;Inherit;False;Constant;_Float51;Float 51;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1163;237.02,999.2878;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;991;362.6609,-1096.178;Inherit;False;Property;_Float60;叠加开关;91;0;Create;False;0;0;0;True;1;SubToggle(z5,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;781;-84.61813,1501.302;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1051;181.3604,3434.075;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;684;938.7513,-612.1999;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1092;171.803,-1331.568;Inherit;False;Property;_Float68;叠加贴图强度;96;0;Create;False;0;0;0;True;1;Sub(z5);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;683;683.5739,-291.7406;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1030;715.9748,5682.152;Inherit;False;Property;_Float87;模型顶点法向;108;0;Create;False;0;0;0;True;1;SubToggle(z8,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1190;-2786.319,5856.092;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;1031;720.8808,5519.486;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;997;-3324.871,3198.864;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1095;540.7405,1117.445;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1189;-3207.458,6219.238;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;760;1926.657,-595.9029;Inherit;False;216;233;;1;671;RGB;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;1029;446.3236,5175.635;Inherit;False;Property;_Vector12;顶点贴图UV;113;0;Create;False;0;0;0;True;1;Sub(z8);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;1033;385.5834,5994.604;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;823;2394.726,-63.31485;Inherit;False;Property;_Float37;外层亮边颜色强度;74;0;Create;False;0;0;0;True;1;Sub(z4);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;822;2350.726,15.68513;Inherit;False;Property;_Color2;外层亮边颜色;73;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;994;-2317.717,2107.066;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1028;385.5834,5770.604;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1194;-2384.541,5210.577;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;959;515.7194,-1237.646;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;977;779.4763,-1322.238;Inherit;False;Constant;_Float58;Float 58;78;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;954;175.6169,-1439.33;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;791;2273.678,1880.113;Inherit;False;628.9875;471.9476;;5;786;789;785;788;787;高光;1,1,1,1;0;0
Node;AmplifyShaderEditor.RotatorNode;1032;445.3236,4975.636;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;762;-2984.857,2235.901;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;1037;713.3226,5072.634;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;685;907.092,-290.8468;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1191;-2330.244,5840.167;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1035;258.6076,5447.921;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;854;2499.158,219.6239;Inherit;False;Property;_Float39;内层亮边2颜色强度;77;0;Create;False;0;0;0;True;1;Sub(z4);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;671;1976.657,-545.9029;Inherit;False;8;8;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;1041;625.5835,5898.604;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1039;398.5334,5625.153;Inherit;False;Property;_Float98;顶点偏移强度自定义;111;0;Create;False;0;0;0;True;1;SubToggle(z8,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1040;403.3726,5307.8;Inherit;False;Property;_Float89;顶点偏移强度;110;0;Create;False;0;0;0;True;1;Sub(z8);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;1036;948.0456,5479.665;Inherit;False;Property;_Vector13;顶点偏移轴向;112;0;Create;False;0;0;0;False;1;Sub(z8);False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;786;2323.678,1984.013;Inherit;False;Property;_Float17;高光细节;35;0;Create;False;0;0;0;True;1;Sub(z6);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;844;2633.899,-45.58105;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;989;1051.662,-1280.178;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;855;2461.158,323.624;Inherit;False;Property;_Color3;内层亮边2颜色;76;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1094;397.803,-1464.568;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;914;-2589.873,2581.11;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;990;1047.662,-1084.178;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1034;956.8803,5623.188;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;1162;1671.559,941.0541;Inherit;False;578.4781;305;;2;669;670;Alpha;1,0,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;793;-2440.139,2448.697;Inherit;False;Constant;_Float23;Float 23;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1192;-2774.723,6220.899;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;670;1721.559,1000.561;Inherit;False;Property;_Float6;整体Alpha强度;2;0;Create;False;0;0;0;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;856;2779.958,309.1241;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;982;1334.418,-1259.119;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;843;2880.533,-41.96875;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;789;2440.665,2140.062;Inherit;False;Property;_Color1;高光颜色;34;0;Create;False;0;0;0;True;1;Sub(z6);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;983;1336.818,-1093.319;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;788;2513.665,2054.062;Inherit;False;Property;_Float19;高光强度;36;0;Create;False;0;0;0;True;1;Sub(z6);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;993;-2220.269,2455.056;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;704;2321.363,1484.309;Inherit;False;804.9153;185.5732;;3;702;701;703;软粒子;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;958;576.7194,-1425.746;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;981;1523.509,-1014.628;Inherit;False;Property;_Float59;通道算法（Add时为叠加）;95;0;Create;False;0;0;0;True;1;KWEnum(z5,Add,_0,Mul,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1193;-2326.095,6148.516;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1045;804.1547,5250.772;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1047;823.5204,5871.646;Inherit;True;Property;_TextureSample8;顶点遮罩;114;0;Create;False;0;0;0;True;1;Sub(z8);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;1043;1166.981,5538.087;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;1049;934.1826,5052.2;Inherit;True;Property;_Vertex3;顶点偏移贴图;107;0;Create;False;0;0;0;True;1;Sub(z8);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;785;2513.779,1930.113;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;992;1029.662,-1468.178;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;852;3030.524,268.7175;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1050;1482.407,5282.919;Inherit;False;Property;_Float101;顶点偏移开关;106;0;Create;False;0;0;0;True;1;SubToggle(z8,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;787;2736.665,1964.061;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;702;2371.363,1552.901;Inherit;False;Property;_Float5;软粒子（相机需开启投影）;3;0;Create;False;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1052;1385.243,5081.139;Inherit;False;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;669;2088.037,991.0541;Inherit;False;7;7;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;980;2060.557,-1153.674;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;784;4755.613,-686.793;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;985;4752.986,-405.3799;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;701;2638.67,1534.882;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1053;1673.567,5072.936;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;703;2958.278,1534.309;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;621;4973.019,-627.772;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;1140;4911.674,-1969.568;Inherit;False;242.1333;931.3217;Comment;9;140;142;141;952;143;1078;794;1079;1139;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1054;1907.539,5053.818;Inherit;False;Vertex;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;550;4604.212,-1519.42;Inherit;False;Property;_Float85;深度写入（模型特效适用）;4;1;[Toggle];Create;False;0;2;open;0;close;4;0;True;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1078;4968.222,-1343.162;Inherit;False;Property;_Float69;非尼尔列表;100;0;Create;False;0;0;0;True;1;Main(z7,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;700;5231.031,-605.5115;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;794;4974.246,-1436.953;Inherit;False;Property;_Float31;高光列表;33;0;Create;False;0;0;0;True;1;Main(z6,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1055;5440.649,-448.846;Inherit;False;1054;Vertex;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;130;4613.841,-1635.218;Inherit;False;Property;_Float18;显示最前面;5;1;[Enum];Create;False;0;2;open;0;close;4;0;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;4616.961,-1726.482;Inherit;False;Property;_Blendmode;混合模式;0;1;[Enum];Create;False;0;2;add;1;blend;10;0;True;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;4975.62,-1820.86;Inherit;False;Property;_Float25;遮罩列表;37;0;Create;False;0;0;0;True;1;Main(z2,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;952;4973.714,-1526.668;Inherit;False;Property;_Float40;叠加列表;90;0;Create;False;0;0;0;True;1;Main(z5,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;4972.106,-1919.568;Inherit;False;Property;_Float24;主贴图列表;13;0;Create;False;0;0;0;True;1;Main(z1,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1139;4961.674,-1154.246;Inherit;False;Property;_Float82;Custom列表;6;0;Create;False;0;0;0;True;1;Main(z9,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;4616.961,-1829.483;Inherit;False;Property;_Float22;剔除模式;1;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;4974.316,-1723.562;Inherit;False;Property;_Float26;扰动列表;56;0;Create;False;0;0;0;True;1;Main(z3,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;4975.976,-1628.63;Inherit;False;Property;_Float27;溶解列表;66;0;Create;False;0;0;0;True;1;Main(z4,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1079;4963.807,-1256.151;Inherit;False;Property;_Float70;顶点偏移列表;105;0;Create;False;0;0;0;True;1;Main(z8,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;617;5696.02,-594.3596;Float;False;True;-1;2;LWGUI.LWGUI;100;1;Test/极简02_1;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;True;120;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;121;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;True;550;True;3;True;130;True;True;0;False;-1;0;False;-1;True;2;RenderType=Opaque=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;742;0;644;0
WireConnection;691;0;687;1
WireConnection;691;1;687;2
WireConnection;741;0;742;0
WireConnection;692;1;691;0
WireConnection;692;2;694;0
WireConnection;738;0;741;0
WireConnection;646;0;643;0
WireConnection;737;0;738;0
WireConnection;737;1;739;0
WireConnection;737;2;740;0
WireConnection;688;0;644;0
WireConnection;688;1;692;0
WireConnection;754;0;753;0
WireConnection;754;1;755;0
WireConnection;648;0;688;0
WireConnection;648;2;646;0
WireConnection;743;0;737;0
WireConnection;756;0;752;0
WireConnection;756;1;754;0
WireConnection;735;0;648;0
WireConnection;735;2;743;0
WireConnection;728;0;727;1
WireConnection;728;1;727;2
WireConnection;745;1;756;0
WireConnection;714;1;735;0
WireConnection;714;2;728;0
WireConnection;714;3;727;3
WireConnection;714;4;727;4
WireConnection;1089;0;745;1
WireConnection;1089;1;745;2
WireConnection;649;0;647;0
WireConnection;649;1;645;0
WireConnection;650;0;648;0
WireConnection;650;1;649;0
WireConnection;882;0;878;0
WireConnection;729;0;714;0
WireConnection;729;1;730;0
WireConnection;729;2;649;0
WireConnection;1090;0;1089;0
WireConnection;1090;1;745;4
WireConnection;883;0;879;0
WireConnection;883;2;882;0
WireConnection;884;0;881;0
WireConnection;884;1;880;0
WireConnection;1147;0;1146;1
WireConnection;1147;1;1146;2
WireConnection;716;0;650;0
WireConnection;716;1;729;0
WireConnection;716;2;717;0
WireConnection;748;1;1090;0
WireConnection;748;2;749;0
WireConnection;699;0;716;0
WireConnection;698;0;716;0
WireConnection;750;0;748;0
WireConnection;750;1;751;0
WireConnection;770;0;771;0
WireConnection;885;0;883;0
WireConnection;885;1;884;0
WireConnection;1149;1;1147;0
WireConnection;1149;2;1148;0
WireConnection;1150;0;939;0
WireConnection;1150;1;1149;0
WireConnection;1164;1;1147;0
WireConnection;1164;2;1165;0
WireConnection;886;0;885;0
WireConnection;655;0;699;0
WireConnection;654;0;698;0
WireConnection;769;0;764;0
WireConnection;769;2;770;0
WireConnection;768;0;766;0
WireConnection;768;1;767;0
WireConnection;1117;0;1118;0
WireConnection;887;0;885;0
WireConnection;1170;1;750;0
WireConnection;1170;2;1172;0
WireConnection;765;0;769;0
WireConnection;765;1;768;0
WireConnection;1121;0;1122;0
WireConnection;658;0;699;0
WireConnection;658;1;655;0
WireConnection;658;2;659;0
WireConnection;891;0;886;0
WireConnection;889;0;887;0
WireConnection;942;0;940;0
WireConnection;942;1;941;0
WireConnection;1166;0;836;0
WireConnection;1166;1;1164;0
WireConnection;1171;0;1170;0
WireConnection;1125;1;750;0
WireConnection;1125;2;1124;0
WireConnection;656;0;698;0
WireConnection;656;1;654;0
WireConnection;656;2;657;0
WireConnection;1116;0;1150;0
WireConnection;1116;2;1117;0
WireConnection;938;0;1116;0
WireConnection;938;1;942;0
WireConnection;1120;0;1166;0
WireConnection;1120;2;1121;0
WireConnection;866;0;765;0
WireConnection;893;0;886;0
WireConnection;893;1;891;0
WireConnection;893;2;890;0
WireConnection;892;0;887;0
WireConnection;892;1;889;0
WireConnection;892;2;888;0
WireConnection;642;0;656;0
WireConnection;642;1;658;0
WireConnection;865;0;765;0
WireConnection;838;0;840;0
WireConnection;838;1;839;0
WireConnection;1131;0;1125;0
WireConnection;1175;0;1173;0
WireConnection;1175;1;938;0
WireConnection;837;0;1120;0
WireConnection;837;1;838;0
WireConnection;870;0;865;0
WireConnection;707;0;642;0
WireConnection;894;0;892;0
WireConnection;894;1;893;0
WireConnection;871;0;866;0
WireConnection;922;0;919;0
WireConnection;922;1;920;0
WireConnection;1127;1;750;0
WireConnection;1127;2;1126;0
WireConnection;706;0;642;0
WireConnection;923;0;921;0
WireConnection;1129;0;1127;0
WireConnection;1136;0;1137;0
WireConnection;1136;1;894;0
WireConnection;1174;0;837;0
WireConnection;1174;1;1173;0
WireConnection;867;0;865;0
WireConnection;867;1;870;0
WireConnection;867;2;869;0
WireConnection;710;0;706;0
WireConnection;710;1;707;0
WireConnection;924;1;922;0
WireConnection;868;0;866;0
WireConnection;868;1;871;0
WireConnection;868;2;872;0
WireConnection;931;1;1175;0
WireConnection;925;0;924;0
WireConnection;925;2;923;0
WireConnection;873;0;867;0
WireConnection;873;1;868;0
WireConnection;711;0;642;0
WireConnection;711;1;710;0
WireConnection;711;2;712;0
WireConnection;1132;1;750;0
WireConnection;1132;2;1133;0
WireConnection;1155;0;1154;0
WireConnection;1155;1;1153;3
WireConnection;1155;2;1152;0
WireConnection;935;0;931;1
WireConnection;935;1;936;0
WireConnection;895;1;1136;0
WireConnection;795;1;1174;0
WireConnection;1178;1;750;0
WireConnection;1178;2;1177;0
WireConnection;967;0;968;0
WireConnection;747;0;711;0
WireConnection;747;1;1132;0
WireConnection;1156;0;895;0
WireConnection;1156;1;1155;0
WireConnection;1128;0;1130;0
WireConnection;1128;1;873;0
WireConnection;796;0;795;1
WireConnection;796;1;795;4
WireConnection;796;2;1111;0
WireConnection;986;0;961;0
WireConnection;986;2;967;0
WireConnection;927;0;925;0
WireConnection;1176;0;1178;0
WireConnection;966;0;964;0
WireConnection;966;1;965;0
WireConnection;932;0;934;0
WireConnection;932;1;935;0
WireConnection;932;2;937;0
WireConnection;950;0;896;0
WireConnection;929;0;928;0
WireConnection;929;1;927;0
WireConnection;929;2;926;0
WireConnection;619;1;747;0
WireConnection;1157;0;1156;0
WireConnection;1004;0;1002;0
WireConnection;899;0;895;0
WireConnection;899;1;950;0
WireConnection;962;0;986;0
WireConnection;962;1;966;0
WireConnection;1018;0;1010;0
WireConnection;1018;1;1012;0
WireConnection;944;0;796;0
WireConnection;944;1;932;0
WireConnection;944;2;945;0
WireConnection;834;0;800;0
WireConnection;834;1;829;3
WireConnection;834;2;835;0
WireConnection;949;0;774;0
WireConnection;758;1;1128;0
WireConnection;1008;0;1006;0
WireConnection;1008;2;1004;0
WireConnection;1100;0;1098;1
WireConnection;1100;1;1098;2
WireConnection;1100;2;1099;0
WireConnection;930;0;944;0
WireConnection;930;1;929;0
WireConnection;900;0;899;0
WireConnection;900;1;898;0
WireConnection;1026;0;1018;0
WireConnection;1026;4;1016;0
WireConnection;1026;2;1019;0
WireConnection;1026;3;1014;0
WireConnection;773;0;758;0
WireConnection;773;1;949;0
WireConnection;1007;0;1005;0
WireConnection;1007;1;1003;0
WireConnection;1106;0;619;0
WireConnection;1169;0;962;0
WireConnection;1169;1;1179;0
WireConnection;804;0;834;0
WireConnection;804;1;805;0
WireConnection;1158;0;895;0
WireConnection;1158;1;1157;0
WireConnection;1087;0;895;4
WireConnection;1087;1;950;0
WireConnection;806;0;930;0
WireConnection;806;1;804;0
WireConnection;806;2;807;0
WireConnection;1186;0;1184;0
WireConnection;1186;1;1182;0
WireConnection;1186;2;1183;0
WireConnection;1027;0;1026;0
WireConnection;874;0;773;0
WireConnection;874;1;875;0
WireConnection;1011;0;1008;0
WireConnection;1011;1;1007;0
WireConnection;951;1;1169;0
WireConnection;901;0;900;0
WireConnection;1160;0;1158;0
WireConnection;1107;0;619;0
WireConnection;1107;1;1106;0
WireConnection;1107;2;1105;0
WireConnection;1101;0;1100;0
WireConnection;775;0;874;0
WireConnection;1188;0;806;0
WireConnection;1188;1;1185;0
WireConnection;1187;0;1186;0
WireConnection;1198;0;806;0
WireConnection;1198;1;1200;0
WireConnection;1015;0;1011;0
WireConnection;1042;0;1027;0
WireConnection;1108;0;1107;0
WireConnection;1108;1;1109;0
WireConnection;1088;0;1087;0
WireConnection;1088;1;898;0
WireConnection;1013;0;1011;0
WireConnection;1103;0;1101;0
WireConnection;1103;1;1102;0
WireConnection;1085;0;758;4
WireConnection;1085;1;949;0
WireConnection;1143;1;1142;4
WireConnection;1143;2;1144;0
WireConnection;996;0;916;0
WireConnection;996;1;901;0
WireConnection;996;2;1000;0
WireConnection;956;0;951;4
WireConnection;956;1;951;0
WireConnection;956;2;969;0
WireConnection;623;0;619;4
WireConnection;623;1;1107;0
WireConnection;623;2;624;0
WireConnection;1161;0;1159;0
WireConnection;1161;1;1160;0
WireConnection;1161;2;1152;0
WireConnection;1044;0;1027;0
WireConnection;1044;1;1042;0
WireConnection;1044;2;1038;0
WireConnection;680;0;1108;0
WireConnection;680;1;681;0
WireConnection;1021;0;1017;0
WireConnection;1197;0;1188;0
WireConnection;1197;1;1186;0
WireConnection;1197;2;1187;0
WireConnection;1097;0;1103;0
WireConnection;1202;0;1201;0
WireConnection;1202;1;951;0
WireConnection;1110;0;623;0
WireConnection;1110;1;1109;0
WireConnection;1141;0;673;0
WireConnection;1141;1;1143;0
WireConnection;798;0;1198;0
WireConnection;798;1;1186;0
WireConnection;798;2;1187;0
WireConnection;955;0;956;0
WireConnection;955;1;960;0
WireConnection;906;0;1088;0
WireConnection;906;1;901;0
WireConnection;906;2;904;0
WireConnection;1023;0;1015;0
WireConnection;1086;0;1085;0
WireConnection;1086;1;875;0
WireConnection;908;0;775;0
WireConnection;908;1;996;0
WireConnection;908;2;1161;0
WireConnection;1020;0;1013;0
WireConnection;1163;0;672;0
WireConnection;1163;1;1141;0
WireConnection;1051;0;1046;0
WireConnection;1051;1;1044;0
WireConnection;1051;2;1048;0
WireConnection;684;0;680;0
WireConnection;683;0;1110;0
WireConnection;683;1;681;0
WireConnection;1190;0;798;0
WireConnection;1190;1;1197;0
WireConnection;997;0;917;0
WireConnection;997;1;906;0
WireConnection;997;2;1000;0
WireConnection;1095;0;1096;0
WireConnection;1095;1;672;0
WireConnection;1095;2;1097;0
WireConnection;1189;0;806;0
WireConnection;1189;1;1186;0
WireConnection;1189;2;1187;0
WireConnection;1033;0;1013;0
WireConnection;1033;1;1020;0
WireConnection;1033;2;1025;0
WireConnection;994;0;792;0
WireConnection;994;1;908;0
WireConnection;994;2;995;0
WireConnection;1028;0;1015;0
WireConnection;1028;1;1023;0
WireConnection;1028;2;1022;0
WireConnection;1194;0;1196;0
WireConnection;1194;1;798;0
WireConnection;1194;2;1195;0
WireConnection;959;0;955;0
WireConnection;954;0;1202;0
WireConnection;954;1;960;0
WireConnection;1032;0;1024;0
WireConnection;1032;2;1021;0
WireConnection;762;0;1086;0
WireConnection;762;1;775;0
WireConnection;762;2;763;0
WireConnection;1037;0;1032;0
WireConnection;1037;2;1029;0
WireConnection;685;0;683;0
WireConnection;1191;1;1190;0
WireConnection;1191;2;1195;0
WireConnection;671;0;684;0
WireConnection;671;1;994;0
WireConnection;671;2;1194;0
WireConnection;671;3;1095;0
WireConnection;671;4;1163;0
WireConnection;671;5;781;0
WireConnection;671;6;672;4
WireConnection;671;7;1051;0
WireConnection;1041;0;1028;0
WireConnection;1041;1;1033;0
WireConnection;844;0;823;0
WireConnection;844;1;822;0
WireConnection;989;0;977;0
WireConnection;989;1;959;0
WireConnection;989;2;991;0
WireConnection;1094;0;1095;0
WireConnection;1094;1;954;0
WireConnection;1094;2;1092;0
WireConnection;914;0;762;0
WireConnection;914;1;997;0
WireConnection;990;1;959;0
WireConnection;990;2;991;0
WireConnection;1034;1;1031;0
WireConnection;1034;2;1030;0
WireConnection;1192;0;1197;0
WireConnection;1192;1;1189;0
WireConnection;856;0;854;0
WireConnection;856;1;855;0
WireConnection;982;0;989;0
WireConnection;982;1;685;0
WireConnection;843;0;671;0
WireConnection;843;1;844;0
WireConnection;843;2;1191;0
WireConnection;983;0;990;0
WireConnection;983;1;685;0
WireConnection;993;0;793;0
WireConnection;993;1;914;0
WireConnection;993;2;995;0
WireConnection;958;0;1094;0
WireConnection;1193;1;1192;0
WireConnection;1193;2;1195;0
WireConnection;1045;0;1040;0
WireConnection;1045;1;1035;4
WireConnection;1045;2;1039;0
WireConnection;1047;1;1041;0
WireConnection;1043;0;1036;0
WireConnection;1043;1;1034;0
WireConnection;1049;1;1037;0
WireConnection;785;0;671;0
WireConnection;785;1;786;0
WireConnection;992;1;958;0
WireConnection;992;2;991;0
WireConnection;852;0;843;0
WireConnection;852;1;856;0
WireConnection;852;2;1193;0
WireConnection;787;0;785;0
WireConnection;787;1;788;0
WireConnection;787;2;789;0
WireConnection;787;3;789;4
WireConnection;1052;0;1049;1
WireConnection;1052;1;1045;0
WireConnection;1052;2;1043;0
WireConnection;1052;3;1047;1
WireConnection;1052;4;1049;4
WireConnection;1052;5;1047;4
WireConnection;669;0;685;0
WireConnection;669;1;670;0
WireConnection;669;2;993;0
WireConnection;669;3;1194;0
WireConnection;669;4;672;4
WireConnection;669;5;781;4
WireConnection;669;6;1051;0
WireConnection;980;0;983;0
WireConnection;980;1;982;0
WireConnection;980;2;981;0
WireConnection;784;0;852;0
WireConnection;784;1;787;0
WireConnection;784;2;992;0
WireConnection;985;0;980;0
WireConnection;985;1;669;0
WireConnection;701;0;702;0
WireConnection;1053;1;1052;0
WireConnection;1053;2;1050;0
WireConnection;703;0;701;0
WireConnection;621;0;784;0
WireConnection;621;3;985;0
WireConnection;1054;0;1053;0
WireConnection;700;0;621;0
WireConnection;700;1;703;0
WireConnection;617;0;700;0
WireConnection;617;1;1055;0
ASEEND*/
//CHKSM=7BCA228DB58E7F6CEF4AE29CA9C8D470494183BD