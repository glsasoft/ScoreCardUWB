// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Test/极简屏幕扭曲"
{
	Properties
	{
		[Enum(Open,0,Close,4)]_Float1("深度测试", Float) = 0
		_NormalMap("扭曲贴图", 2D) = "bump" {}
		_Distortionpower("扭曲强度", Range( 0 , 0.2)) = 0
		_Vector0("扭曲贴图UV", Vector) = (0,0,0,0)
		[Toggle]_Float0("遮罩开关", Float) = 0
		_Mask("遮罩贴图", 2D) = "white" {}
		[ASEEnd]_Vector1("遮罩贴图UV", Vector) = (0,0,0,0)

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		
		
		GrabPass{ }

		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" "RenderType"="Opaque" "Queue"="Transparent" }
			CGPROGRAM

			#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
			#else
			#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
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


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Float1;
			ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
			uniform sampler2D _NormalMap;
			uniform float4 _NormalMap_ST;
			uniform float2 _Vector0;
			uniform sampler2D _Mask;
			uniform float4 _Mask_ST;
			uniform float2 _Vector1;
			uniform float _Float0;
			uniform float _Distortionpower;
			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord1 = screenPos;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
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
				float4 screenPos = i.ase_texcoord1;
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( screenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				float2 uv_NormalMap = i.ase_texcoord2.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				float2 uv_Mask = i.ase_texcoord2.xy * _Mask_ST.xy + _Mask_ST.zw;
				float lerpResult143 = lerp( 1.0 , tex2D( _Mask, ( uv_Mask + ( _Vector1 * _Time.y ) ) ).r , _Float0);
				float4 screenColor29 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( ase_grabScreenPosNorm + ( ( ( tex2D( _NormalMap, ( uv_NormalMap + ( _Vector0 * _Time.y ) ) ).r * lerpResult143 ) + -0.25 ) * ( _Distortionpower * i.ase_color.a ) ) ).xy);
				float4 appendResult108 = (float4(screenColor29.rgb , 1.0));
				
				
				finalColor = appendResult108;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "LWGUI.LWGUI"
	
	
}
/*ASEBEGIN
Version=18935
-1776;301;1920;771;657.9354;1498.084;1.3;True;True
Node;AmplifyShaderEditor.CommentaryNode;140;-1788.057,-802.7551;Inherit;False;1017.298;447;;7;135;136;138;137;139;126;144;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;141;-1784.155,-1319.482;Inherit;False;1001.008;447.0001;;6;14;133;131;129;134;130;扭曲;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;135;-1738.057,-466.7551;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;136;-1730.057,-606.7551;Inherit;False;Property;_Vector1;遮罩贴图UV;6;0;Create;False;0;0;0;True;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;133;-1734.155,-983.4821;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;131;-1726.155,-1123.482;Inherit;False;Property;_Vector0;扭曲贴图UV;3;0;Create;False;0;0;0;True;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;-1503.057,-540.7551;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;137;-1610.057,-752.7551;Inherit;False;0;126;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-1499.155,-1057.482;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;129;-1606.155,-1269.482;Inherit;False;0;14;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;139;-1258.75,-690.4455;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;142;-1440.626,-272.1286;Inherit;False;591.4407;356.6237;;3;12;49;124;扭曲参数;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;130;-1269.155,-1159.482;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-930.0554,-501.5005;Inherit;False;Property;_Float0;遮罩开关;4;1;[Toggle];Create;False;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;126;-1090.76,-721.171;Inherit;True;Property;_Mask;遮罩贴图;5;0;Create;False;0;0;0;True;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;143;-643.0554,-692.5005;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;49;-1335.995,-122.5051;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-1103.147,-1171.724;Inherit;True;Property;_NormalMap;扭曲贴图;1;0;Create;False;0;0;0;True;0;False;-1;None;None;True;0;True;bump;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-1390.626,-220.2777;Float;False;Property;_Distortionpower;扭曲强度;2;0;Create;False;0;0;0;True;0;False;0;0.1;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-1011.185,-222.1288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;-430.2093,-772.6168;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;122;-154.4024,-582.9045;Inherit;False;ConstantBiasScale;-1;;2;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;-0.25;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;85;95.53326,-840.4856;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;87;430.0044,-836.1335;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenColorNode;29;619.6852,-842.2621;Float;False;Global;_GrabScreen0;Grab Screen 0;4;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;145;1102.691,-1137.863;Inherit;False;Property;_Float1;深度测试;0;1;[Enum];Create;False;0;2;Open;0;Close;4;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;108;864.7222,-834.1837;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;120;1089.942,-835.1738;Float;False;True;-1;2;LWGUI.LWGUI;100;1;Test/极简屏幕扭曲;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;LightMode=ForwardBase;RenderType=Opaque=RenderType;Queue=Transparent=Queue=0;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;138;0;136;0
WireConnection;138;1;135;0
WireConnection;134;0;131;0
WireConnection;134;1;133;0
WireConnection;139;0;137;0
WireConnection;139;1;138;0
WireConnection;130;0;129;0
WireConnection;130;1;134;0
WireConnection;126;1;139;0
WireConnection;143;1;126;1
WireConnection;143;2;144;0
WireConnection;14;1;130;0
WireConnection;124;0;12;0
WireConnection;124;1;49;4
WireConnection;127;0;14;1
WireConnection;127;1;143;0
WireConnection;122;3;127;0
WireConnection;122;2;124;0
WireConnection;87;0;85;0
WireConnection;87;1;122;0
WireConnection;29;0;87;0
WireConnection;108;0;29;0
WireConnection;120;0;108;0
ASEEND*/
//CHKSM=06D630FD90ACD8E4C00BFADEC37251ED991ED8AD