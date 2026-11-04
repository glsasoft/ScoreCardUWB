// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Test/极简00"
{
	Properties
	{
		[Enum(add,1,blend,10)]_Blendmode("混合模式", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_Float22("剔除模式", Float) = 0
		_Float6("整体Alpha强度", Float) = 1
		_Float5("软粒子（相机需开启投影）", Float) = 0
		[Toggle]_Float85("深度写入（模型特效适用）", Float) = 0
		[Enum(open,0,close,4)]_Float18("显示最前面", Float) = 4
		[Main(z4,_KEYWORD,off,off)]_Float9("Custom列表", Float) = 0
		[SubToggle(z4,_)]_Float4("CostomXY控制主贴图uv", Float) = 0
		[SubToggle(z4,_)]_Float10("Custom1W控制颜色强度", Float) = 1
		[SubToggle(z4,_)]_Float11("Custom2Z控制CenterGlow", Float) = 0
		[Main(z1,_KEYWORD,off,off)]_Float24("主贴图列表", Float) = 0
		[HDR][Sub(z1)]_Color0("颜色", Color) = (1,1,1,1)
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
		[Main(z2,_KEYWORD,off,off)]_Float25("遮罩列表", Float) = 0
		[SubToggle(z2,_)]_Float61("遮罩开关", Float) = 0
		[Sub(z2)]_Mask("遮罩贴图", 2D) = "white" {}
		[KWEnum(z2,RGB,_0,A,_1)]_Float14("遮罩通道", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float41("遮罩xclamp", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float42("遮罩yclamp", Float) = 0
		[Sub(z2)]_Float16("遮罩power", Float) = 1
		[Sub(z2)]_Float43("遮罩强度", Float) = 1
		[Sub(z2)]_Float15("遮罩图旋转", Range( 0 , 360)) = 0
		[Sub(z2)]_Vector3("遮罩uv", Vector) = (0,0,0,0)
		[Main(z3,_KEYWORD,off,off)]_Float26("扰动列表", Float) = 0
		[SubToggle(z3,_)]_Float12("扰动开关", Float) = 0
		[Sub(z3)]_Noise("扰动贴图", 2D) = "white" {}
		[Sub(z3)]_Float13("扰动强度", Float) = 0
		[ASEEnd][Sub(z3)]_Vector2("扰动uv", Vector) = (0,0,0,0)

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

			uniform float _Float25;
			uniform float _Float85;
			uniform float _Float18;
			uniform float _Blendmode;
			uniform float _Float22;
			uniform float _Float24;
			uniform float _Float9;
			uniform float _Float26;
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
			uniform float _Float65;
			uniform float _Float66;
			uniform float _Float3;
			uniform sampler2D _Mask;
			uniform float4 _Mask_ST;
			uniform float _Float15;
			uniform float2 _Vector3;
			uniform float _Float41;
			uniform float _Float42;
			uniform float _Float16;
			uniform float _Float43;
			uniform float _Float11;
			uniform float _Float61;
			uniform float4 _Color0;
			uniform float _Float7;
			uniform float _Float10;
			uniform float _Float0;
			uniform float _Float6;
			uniform float _Float14;
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
				float4 tex2DNode619 = tex2D( _Main, ( appendResult642 + ( lerpResult748 * _Float13 ) ) );
				float grayscale1024 = (tex2DNode619.rgb.r + tex2DNode619.rgb.g + tex2DNode619.rgb.b) / 3;
				float4 temp_cast_1 = (grayscale1024).xxxx;
				float4 lerpResult1025 = lerp( tex2DNode619 , temp_cast_1 , _Float65);
				float4 temp_cast_2 = (_Float3).xxxx;
				float4 temp_cast_3 = (1.0).xxxx;
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
				float4 tex2DNode758 = tex2D( _Mask, appendResult873 );
				float temp_output_949_0 = abs( _Float16 );
				float4 temp_cast_4 = (temp_output_949_0).xxxx;
				float4 temp_output_874_0 = ( pow( tex2DNode758 , temp_cast_4 ) * _Float43 );
				float4 temp_cast_5 = (1.0).xxxx;
				float4 texCoord1044 = i.ase_texcoord3;
				texCoord1044.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1060 = lerp( 1.0 , texCoord1044.z , _Float11);
				float4 temp_cast_6 = (lerpResult1060).xxxx;
				float4 clampResult1049 = clamp( ( tex2DNode758 - temp_cast_6 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 clampResult1048 = clamp( ( tex2DNode758 * clampResult1049 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 lerpResult1051 = lerp( temp_cast_5 , clampResult1048 , _Float11);
				float4 lerpResult994 = lerp( temp_cast_3 , ( saturate( temp_output_874_0 ) * lerpResult1051 ) , _Float61);
				float4 texCoord1056 = i.ase_texcoord2;
				texCoord1056.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1058 = lerp( 0.0 , texCoord1056.w , _Float10);
				float4 temp_cast_8 = (tex2DNode619.a).xxxx;
				float4 lerpResult623 = lerp( temp_cast_8 , lerpResult1025 , _Float0);
				float4 temp_cast_9 = (_Float3).xxxx;
				float4 temp_cast_10 = (1.0).xxxx;
				float4 temp_cast_11 = (( pow( tex2DNode758.a , temp_output_949_0 ) * _Float43 )).xxxx;
				float4 lerpResult762 = lerp( temp_cast_11 , temp_output_874_0 , _Float14);
				float4 lerpResult993 = lerp( temp_cast_10 , lerpResult762 , _Float61);
				float4 appendResult621 = (float4(( saturate( pow( ( lerpResult1025 * _Float66 ) , temp_cast_2 ) ) * lerpResult994 * i.ase_color * _Color0.a * ( _Color0 * ( _Float7 + lerpResult1058 ) ) * _Color0 ).rgb , ( saturate( pow( ( _Float66 * lerpResult623 ) , temp_cast_9 ) ) * _Float6 * lerpResult993 * _Color0.a * i.ase_color.a ).r));
				float4 screenPos = i.ase_texcoord4;
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
-35.2;573.6;1890.4;520.6;9554.146;3213.169;11.71951;True;True
Node;AmplifyShaderEditor.CommentaryNode;705;-3719.112,-865.5447;Inherit;False;834.0901;349;;4;687;694;691;692;主贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;687;-3669.112,-796.1475;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;691;-3345.204,-783.5853;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;715;-2650.054,-996.5521;Inherit;False;1015.928;565.5943;;9;643;644;688;646;647;645;649;648;650;主贴图uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;694;-3421.022,-632.5447;Inherit;False;Property;_Float4;CostomXY控制主贴图uv;7;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;757;-1703.928,-143.5765;Inherit;False;1815.883;444;;12;753;755;754;752;756;746;749;751;748;750;745;1005;noise扰动;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1050;-4367.976,706.4527;Inherit;False;4249.49;1486.215;;35;771;767;770;764;766;768;769;765;865;866;870;872;871;869;867;868;873;774;758;949;773;875;874;775;908;792;995;994;1001;763;1002;793;762;993;1053;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;755;-1653.928,189.4235;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;692;-3069.023,-815.5447;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;753;-1640.928,69.42346;Inherit;False;Property;_Vector2;扰动uv;36;0;Create;False;0;0;0;True;1;Sub(z3);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;771;-4317.976,916.4526;Inherit;False;Property;_Float15;遮罩图旋转;30;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;643;-2600.055,-764.1115;Inherit;False;Property;_Float20;主贴图旋转;20;0;Create;False;0;0;0;True;1;Sub(z1);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;644;-2578.991,-946.5522;Inherit;False;0;619;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;645;-2262.821,-541.9579;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;754;-1397.928,113.4235;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;647;-2259.155,-673.2065;Inherit;False;Property;_Vector4;主贴图UV;21;0;Create;False;0;0;0;True;1;Sub(z1);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;752;-1644.928,-93.57654;Inherit;False;0;745;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RadiansOpNode;646;-2301.491,-759.3716;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;766;-3965.975,1028.453;Inherit;False;Property;_Vector3;遮罩uv;31;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;767;-3981.975,1172.453;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;688;-2249.081,-890.0348;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RadiansOpNode;770;-3981.975,916.4526;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;764;-4109.976,756.4527;Inherit;False;0;758;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;648;-2056.248,-860.0392;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;769;-3741.974,852.4526;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;768;-3709.974,1012.452;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;649;-2003.275,-699.8766;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;756;-1271.928,4.423462;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;765;-3517.973,948.4526;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;650;-1787.128,-772.6606;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;718;-1236.165,-933.151;Inherit;False;1031.703;551.2322;;9;699;698;657;659;654;655;658;656;642;主贴图不循环;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;745;-1119.651,-33.1528;Inherit;True;Property;_Noise;扰动贴图;34;0;Create;False;0;0;0;True;1;Sub(z3);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;699;-1183.854,-662.0051;Inherit;False;False;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;866;-3341.973,1044.453;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1005;-779.0649,-33.26379;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;698;-1186.165,-863.2178;Inherit;False;True;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;865;-3341.973,868.4526;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;872;-3133.972,1236.453;Inherit;False;Property;_Float42;遮罩yclamp;27;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;870;-3069.972,884.4526;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;871;-3133.972,1140.453;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;869;-3149.972,964.4526;Inherit;False;Property;_Float41;遮罩xclamp;26;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;654;-887.5344,-824.3127;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;749;-583.2661,177.4821;Inherit;False;Property;_Float12;扰动开关;33;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;659;-896.6205,-497.9189;Inherit;False;Property;_Float2;主贴图yclamp;17;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;655;-889.5344,-585.313;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;657;-902.6205,-747.9181;Inherit;False;Property;_Float1;主贴图xclamp;16;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;746;-583.3665,-11.32115;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;748;-294.6189,-40.55897;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1053;-2846.243,1424.951;Inherit;False;1892.448;747.965;;11;1044;1045;1051;1046;1049;1047;1048;1060;1061;1064;1063;CenterGlow;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;868;-2813.972,1092.453;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;751;-265.0449,128.8889;Inherit;False;Property;_Float13;扰动强度;35;0;Create;False;0;0;0;True;1;Sub(z3);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;658;-613.6202,-665.9182;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;656;-641.8392,-883.151;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;867;-2813.972,804.4526;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;642;-369.4626,-742.9767;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;750;-54.04492,-52.11108;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1063;-2544.565,1779.84;Inherit;False;Constant;_Float17;Float 17;38;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1061;-2732.732,2052.125;Inherit;False;Property;_Float11;Custom2Z控制CenterGlow;9;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1044;-2791.935,1735.724;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;873;-2653.972,916.4526;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;686;498.9437,-910.0938;Inherit;False;1736.342;788.0424;;14;619;624;623;683;680;684;685;681;1024;1025;1026;1028;1029;1027;主贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;758;-2477.972,916.4526;Inherit;True;Property;_Mask;遮罩贴图;24;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1060;-2367.126,1891.509;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;747;334.952,-702.4619;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1046;-2119.823,1981.12;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;774;-2541.972,1156.453;Inherit;False;Property;_Float16;遮罩power;28;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;619;548.9436,-813.4742;Inherit;True;Property;_Main;主贴图;13;0;Create;False;0;0;0;True;1;Sub(z1);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.AbsOpNode;949;-2349.973,1156.453;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1026;896,-768;Inherit;False;Property;_Float65;主贴图去色;15;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;1049;-1933.909,1980.48;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCGrayscale;1024;864,-864;Inherit;False;2;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1047;-1726.718,1962.118;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;624;888.0816,-506.8036;Inherit;False;Property;_Float0;主贴图通道;14;0;Create;False;0;0;0;True;1;KWEnum(z1,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1025;1066.74,-851.812;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;773;-1949.973,964.4526;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;875;-2056.458,1078.938;Inherit;False;Property;_Float43;遮罩强度;29;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1062;597.2994,499.9278;Inherit;False;654.0337;854.0854;;7;672;1057;1056;1058;1059;673;781;颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1029;1252.844,-731.4374;Inherit;False;Property;_Float66;主贴图强度;18;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1059;662.3423,1043.425;Inherit;False;Property;_Float10;Custom1W控制颜色强度;8;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1056;647.2994,868.2593;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;874;-1725.973,980.4525;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;1048;-1565.41,1963.024;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1064;-1556.87,1563.353;Inherit;False;Constant;_Float19;Float 19;38;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;623;1191.565,-584.8971;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;1001;-2029.973,1172.453;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;775;-1485.973,964.4526;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1028;1606.844,-855.4374;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1027;1603.844,-616.4374;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;681;1602.431,-741.8758;Inherit;False;Property;_Float3;主贴图power;19;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1002;-1725.973,1156.453;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;763;-1613.973,1268.453;Inherit;False;Property;_Float14;遮罩通道;25;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;673;898.3174,762.06;Inherit;False;Property;_Float7;颜色强度;12;0;Create;False;0;0;0;True;1;Sub(z1);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1058;915.4586,916.2751;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1051;-1341.833,1604.951;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;793;-744.0037,1192.933;Inherit;False;Constant;_Float23;Float 23;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;908;-1094.974,968.6526;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1057;1099.333,810.941;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;680;1877.765,-849.0938;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;683;1886.111,-681.7486;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;762;-1341.973,1156.453;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;995;-956.689,1218.945;Inherit;False;Property;_Float61;遮罩开关;23;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;672;898.1017,549.9278;Inherit;False;Property;_Color0;颜色;11;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;792;-1197.973,772.4526;Inherit;False;Constant;_Float21;Float 21;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;704;3449.9,1240.301;Inherit;False;804.9153;185.5732;;3;702;701;703;软粒子;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;684;2067.288,-856.2079;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;979;3189.552,643.676;Inherit;False;212;281;;1;669;A;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;670;2888.379,687.4155;Inherit;False;Property;_Float6;整体Alpha强度;2;0;Create;False;0;0;0;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;702;3499.9,1308.893;Inherit;False;Property;_Float5;软粒子（相机需开启投影）;3;0;Create;False;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;993;-541.973,1268.453;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;994;-637.9732,916.4526;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;781;908.2277,1147.013;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;685;2078.847,-680.1552;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;760;3218.835,-415.4697;Inherit;False;216;233;;1;671;RGB;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1065;1326.895,603.9611;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;669;3239.552,693.6761;Inherit;False;5;5;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;671;3268.835,-365.4697;Inherit;False;6;6;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;701;3767.207,1290.874;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;532;4877.106,-1963.568;Inherit;False;227.2825;729.8185;;4;140;141;142;1055;GUI列表;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;703;4086.815,1290.301;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;621;4934.019,-600.772;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;141;4930.62,-1814.86;Inherit;False;Property;_Float25;遮罩列表;22;0;Create;False;0;0;0;True;1;Main(z2,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;550;4604.212,-1519.42;Inherit;False;Property;_Float85;深度写入（模型特效适用）;4;1;[Toggle];Create;False;0;2;open;0;close;4;0;True;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;4613.841,-1635.218;Inherit;False;Property;_Float18;显示最前面;5;1;[Enum];Create;False;0;2;open;0;close;4;0;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;4616.961,-1726.482;Inherit;False;Property;_Blendmode;混合模式;0;1;[Enum];Create;False;0;2;add;1;blend;10;0;True;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;4616.961,-1829.483;Inherit;False;Property;_Float22;剔除模式;1;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;4927.106,-1913.568;Inherit;False;Property;_Float24;主贴图列表;10;0;Create;False;0;0;0;True;1;Main(z1,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;1045;-2409.025,1516.648;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;700;5231.031,-605.5115;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1055;4926.559,-1593.866;Inherit;False;Property;_Float9;Custom列表;6;0;Create;False;0;0;0;True;1;Main(z4,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;4929.316,-1717.562;Inherit;False;Property;_Float26;扰动列表;32;0;Create;False;0;0;0;True;1;Main(z3,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;617;5510.12,-612.5597;Float;False;True;-1;2;LWGUI.LWGUI;100;1;Test/极简00;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;True;120;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;121;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;True;550;True;3;True;130;True;True;0;False;-1;0;False;-1;True;2;RenderType=Opaque=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;691;0;687;1
WireConnection;691;1;687;2
WireConnection;692;1;691;0
WireConnection;692;2;694;0
WireConnection;754;0;753;0
WireConnection;754;1;755;0
WireConnection;646;0;643;0
WireConnection;688;0;644;0
WireConnection;688;1;692;0
WireConnection;770;0;771;0
WireConnection;648;0;688;0
WireConnection;648;2;646;0
WireConnection;769;0;764;0
WireConnection;769;2;770;0
WireConnection;768;0;766;0
WireConnection;768;1;767;0
WireConnection;649;0;647;0
WireConnection;649;1;645;0
WireConnection;756;0;752;0
WireConnection;756;1;754;0
WireConnection;765;0;769;0
WireConnection;765;1;768;0
WireConnection;650;0;648;0
WireConnection;650;1;649;0
WireConnection;745;1;756;0
WireConnection;699;0;650;0
WireConnection;866;0;765;0
WireConnection;1005;0;745;1
WireConnection;1005;1;745;2
WireConnection;698;0;650;0
WireConnection;865;0;765;0
WireConnection;870;0;865;0
WireConnection;871;0;866;0
WireConnection;654;0;698;0
WireConnection;655;0;699;0
WireConnection;746;0;1005;0
WireConnection;746;1;745;4
WireConnection;748;1;746;0
WireConnection;748;2;749;0
WireConnection;868;0;866;0
WireConnection;868;1;871;0
WireConnection;868;2;872;0
WireConnection;658;0;699;0
WireConnection;658;1;655;0
WireConnection;658;2;659;0
WireConnection;656;0;698;0
WireConnection;656;1;654;0
WireConnection;656;2;657;0
WireConnection;867;0;865;0
WireConnection;867;1;870;0
WireConnection;867;2;869;0
WireConnection;642;0;656;0
WireConnection;642;1;658;0
WireConnection;750;0;748;0
WireConnection;750;1;751;0
WireConnection;873;0;867;0
WireConnection;873;1;868;0
WireConnection;758;1;873;0
WireConnection;1060;0;1063;0
WireConnection;1060;1;1044;3
WireConnection;1060;2;1061;0
WireConnection;747;0;642;0
WireConnection;747;1;750;0
WireConnection;1046;0;758;0
WireConnection;1046;1;1060;0
WireConnection;619;1;747;0
WireConnection;949;0;774;0
WireConnection;1049;0;1046;0
WireConnection;1024;0;619;0
WireConnection;1047;0;758;0
WireConnection;1047;1;1049;0
WireConnection;1025;0;619;0
WireConnection;1025;1;1024;0
WireConnection;1025;2;1026;0
WireConnection;773;0;758;0
WireConnection;773;1;949;0
WireConnection;874;0;773;0
WireConnection;874;1;875;0
WireConnection;1048;0;1047;0
WireConnection;623;0;619;4
WireConnection;623;1;1025;0
WireConnection;623;2;624;0
WireConnection;1001;0;758;4
WireConnection;1001;1;949;0
WireConnection;775;0;874;0
WireConnection;1028;0;1025;0
WireConnection;1028;1;1029;0
WireConnection;1027;0;1029;0
WireConnection;1027;1;623;0
WireConnection;1002;0;1001;0
WireConnection;1002;1;875;0
WireConnection;1058;1;1056;4
WireConnection;1058;2;1059;0
WireConnection;1051;0;1064;0
WireConnection;1051;1;1048;0
WireConnection;1051;2;1061;0
WireConnection;908;0;775;0
WireConnection;908;1;1051;0
WireConnection;1057;0;673;0
WireConnection;1057;1;1058;0
WireConnection;680;0;1028;0
WireConnection;680;1;681;0
WireConnection;683;0;1027;0
WireConnection;683;1;681;0
WireConnection;762;0;1002;0
WireConnection;762;1;874;0
WireConnection;762;2;763;0
WireConnection;684;0;680;0
WireConnection;993;0;793;0
WireConnection;993;1;762;0
WireConnection;993;2;995;0
WireConnection;994;0;792;0
WireConnection;994;1;908;0
WireConnection;994;2;995;0
WireConnection;685;0;683;0
WireConnection;1065;0;672;0
WireConnection;1065;1;1057;0
WireConnection;669;0;685;0
WireConnection;669;1;670;0
WireConnection;669;2;993;0
WireConnection;669;3;672;4
WireConnection;669;4;781;4
WireConnection;671;0;684;0
WireConnection;671;1;994;0
WireConnection;671;2;781;0
WireConnection;671;3;672;4
WireConnection;671;4;1065;0
WireConnection;671;5;672;0
WireConnection;701;0;702;0
WireConnection;703;0;701;0
WireConnection;621;0;671;0
WireConnection;621;3;669;0
WireConnection;700;0;621;0
WireConnection;700;1;703;0
WireConnection;617;0;700;0
ASEEND*/
//CHKSM=0DE0F69D24B7431852223B45B37A1132FF45489C