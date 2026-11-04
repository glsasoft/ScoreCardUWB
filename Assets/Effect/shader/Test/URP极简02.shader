// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Test/URP极简02"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[Enum(add,1,blend,10)]_Blendmode("混合模式", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_Float22("剔除模式", Float) = 0
		_Float6("整体Alpha强度", Float) = 1
		_Float5("软粒子（相机需开启投影）", Float) = 0
		[Toggle]_Float85("深度写入（模型特效适用）", Float) = 0
		[Enum(open,0,close,4)]_Float18("显示最前面", Float) = 4
		[Main(z9,_KEYWORD,off,off)]_Float84("Custom列表", Float) = 0
		[SubToggle(z9,_)]_Float4("Custom1XY控制主贴图uv", Float) = 0
		[SubToggle(z9,_)]_Float38("Custom1Z控制溶解程度", Float) = 0
		[SubToggle(z9,_)]_Float87("Custom1W增加贴图亮度", Float) = 1
		[SubToggle(z9,_)]_Float90("Custom2XY控制附加溶解贴图uv", Float) = 0
		[SubToggle(z9,_)]_Float81("Custom2Z控制centerGlow", Float) = 0
		[Main(z1,_KEYWORD,off,off)]_Float24("主贴图列表", Float) = 0
		[SubToggle(z1,_)]_Float65("双面颜色", Float) = 0
		[HDR][Sub(z1)]_Color0("颜色Front", Color) = (1,1,1,1)
		[HDR][Sub(z1)]_Color4("颜色Back", Color) = (1,1,1,1)
		[Sub(z1)]_Main("主贴图", 2D) = "white" {}
		[KeywordEnum(RGBA,R,G,B)] _Keyword0("主贴图通道", Float) = 0
		[KWEnum(z1,RGB,_0,A,_1)]_Float0("主贴图A通道", Float) = 0
		[KWEnum(z1,off,_0,on,_1)]_Float1("主贴图xclamp", Float) = 0
		[KWEnum(z1,off,_0,on,_1)]_Float2("主贴图yclamp", Float) = 0
		[Sub(z1)]_Float3("主贴图power", Float) = 1
		[Sub(z1)]_Float7("主贴图强度", Float) = 1
		[Sub(z1)]_Float20("主贴图旋转", Range( 0 , 360)) = 0
		[Sub(z1)]_Vector4("主贴图UV", Vector) = (0,0,0,0)
		[SubToggle(z1,_)]_Float8("主贴图极坐标uv翻转", Float) = 0
		[Main(z6,_KEYWORD,off,off)]_Float31("高光列表", Float) = 0
		[Sub(z6)]_Color1("高光颜色", Color) = (1,1,1,1)
		[Sub(z6)]_Float17("高光细节", Float) = 1
		[Sub(z6)]_Float19("高光强度", Float) = 0
		[Main(z2,_KEYWORD,off,off)]_Float25("遮罩列表", Float) = 0
		[SubToggle(z2,_)]_Float10("遮罩开关", Float) = 0
		[Sub(z2)]_Mask("遮罩贴图", 2D) = "white" {}
		[KWEnum(z2,RGB,_0,A,_1)]_Float14("遮罩A通道", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float41("遮罩xclamp", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float42("遮罩yclamp", Float) = 0
		[Sub(z2)]_Float16("遮罩power", Float) = 1
		[Sub(z2)]_Float43("遮罩强度", Float) = 1
		[Sub(z2)]_Float15("遮罩图旋转", Range( 0 , 360)) = 0
		[Sub(z2)]_Vector3("遮罩uv", Vector) = (0,0,0,0)
		[SubToggle(z2,_)]_Float11("附加遮罩开关", Float) = 0
		[Sub(z2)]_Mask1("m贴图", 2D) = "white" {}
		[KWEnum(z2,RGB,_0,A,_1)]_Float50("mA通道", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float45("mxclamp", Float) = 0
		[KWEnum(z2,off,_0,on,_1)]_Float46("myclamp", Float) = 0
		[Sub(z2)]_Float47("mpower", Float) = 1
		[Sub(z2)]_Float48("m强度", Float) = 1
		[Sub(z2)]_Float44("m旋转", Range( 0 , 360)) = 0
		[Sub(z2)]_Vector6("muv", Vector) = (0,0,0,0)
		[Main(z3,_KEYWORD,off,off)]_Float26("扰动列表", Float) = 0
		[SubToggle(z3,_)]_Float12("扰动开关", Float) = 0
		[Sub(z3)]_Noise("扰动贴图", 2D) = "white" {}
		[Sub(z3)]_Float13("扰动强度", Float) = 0
		[Sub(z3)]_Vector2("扰动uv", Vector) = (0,0,0,0)
		[Main(z4,_KEYWORD,off,off)]_Float27("溶解列表", Float) = 0
		[SubToggle(z4,_)]_Float61("溶解开关", Float) = 0
		[KWEnum(z4,Soft,_0,Rim,_1)]_Float60("溶解模式", Float) = 0
		[Sub(z4)]_Dissolve("溶解贴图", 2D) = "white" {}
		[Sub(z4)]_Float78("溶解贴图纹理加强", Float) = 0
		[KWEnum(z4,R,_0,G,_1)]_Float62("溶解通道", Float) = 0
		[Sub(z4)]_Float66("溶解贴图旋转", Range( 0 , 360)) = 0
		[Sub(z4)]_Float28("溶解程度", Range( -0.2 , 1.2)) = 1
		[Sub(z4)]_Float29("软溶解", Range( -0.5 , 0.5)) = 0
		[Sub(z4)]_Color2("外层亮边颜色", Color) = (1,1,1,1)
		[Sub(z4)]_Float37("外层亮边颜色强度", Float) = 1
		[Sub(z4)]_Float36("外层亮边宽度", Float) = 0
		[Sub(z4)]_Color3("内层亮边2颜色", Color) = (1,1,1,1)
		[Sub(z4)]_Float39("内层亮边2颜色强度", Float) = 1
		[Sub(z4)]_Float35("内层亮边2宽度", Float) = 0
		[Sub(z4)]_Vector5("溶解贴图uv", Vector) = (0,0,0,0)
		[SubToggle(z4,_)]_Float55("附加溶解贴图开关", Float) = 0
		[Sub(z4)]_Dissolve1("附加溶解贴图", 2D) = "white" {}
		[Sub(z4)]_Float30("溶解纹理偏向", Range( 0 , 1)) = 0
		[Sub(z4)]_Float67("附加溶解贴图旋转", Range( 0 , 360)) = 0
		[Sub(z4)]_Float541111111111111111111("附加溶解贴图强度", Float) = 1
		[Sub(z4)]_aegrdfgaergare("附加溶解贴图uv", Vector) = (0,0,0,0)
		[SubToggle(z4,_)]_Float97("方向溶解开关", Float) = 0
		[Sub(z4)]_Float95("方向溶解旋转（默认左黑右白）", Range( 0 , 360)) = 0
		[Sub(z4)]_Float93("方向U偏移", Float) = 0
		[Sub(z4)]_Float94("方向V偏移", Float) = 0
		[Main(z5,_KEYWORD,off,off)]_Float40("叠加列表", Float) = 0
		[SubToggle(z5,_)]_Float9("叠加开关", Float) = 0
		[Sub(z5)]_TextureSample0("叠加贴图", 2D) = "white" {}
		[KWEnum(z5,RGB,_0,A,_1)]_Float57("叠加贴图通道", Float) = 0
		[KWEnum(z5,Mul,_0,Add,_1)]_Float59("A通道算法（Add时通道不同）", Float) = 0
		[Sub(z5)]_Float54("叠加贴图power", Float) = 1
		[Sub(z5)]_Float56("叠加旋转", Range( 0 , 360)) = 0
		[Main(z7,_KEYWORD,off,off)]_Float76("菲尼尔列表", Float) = 0
		[Sub(z5)]_Vector7("叠加uv", Vector) = (0,0,0,0)
		[SubToggle(z7,_)]_Float74("菲尼尔开关", Float) = 0
		[Sub(z7)]_Float70("菲尼尔power", Float) = 1
		[Sub(z7)]_Float71("菲尼尔范围", Float) = 1
		[SubToggle(z7,_)]_Float72("反向菲尼尔（虚化边缘）", Float) = 0
		[Main(z8,_KEYWORD,off,off)]_Float75("顶点偏移列表", Float) = 0
		[SubToggle(z8,_)]_Float100("顶点偏移开关", Float) = 0
		[Sub(z8)]_Vertex2("顶点偏移贴图", 2D) = "white" {}
		[SubToggle(z8,_)]_Float86("模型顶点法向", Float) = 0
		[Sub(z8)]_Float89("顶点贴图旋转", Range( 0 , 360)) = 0
		[Sub(z8)]_Float88("顶点偏移强度", Float) = 0
		[SubToggle(z8,_)]_Float96("顶点偏移强度自定义", Float) = 0
		[Sub(z8)]_Vector12("顶点偏移轴向", Vector) = (0,0,0,0)
		[Sub(z8)]_Vector11("顶点贴图UV", Vector) = (0,0,0,0)
		[Sub(z8)]_Vertex("顶点遮罩", 2D) = "white" {}
		[KWEnum(z8,Off,_0,On,_1)]_Float102("顶点MxClamp", Float) = 0
		[KWEnum(z8,Off,_0,On,_1)]_Float101("顶点MyClamp", Float) = 0
		[Sub(z8)]_Float99("顶点偏移M旋转", Range( 0 , 360)) = 0
		[ASEEnd][Sub(z8)]_Vector13("顶点偏移MUV", Vector) = (0,0,0,0)

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull [_Float22]
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 3.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }
			
			Blend SrcAlpha [_Blendmode], One OneMinusSrcAlpha
			ZWrite [_Float85]
			ZTest [_Float18]
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 999999
			#define REQUIRE_DEPTH_TEXTURE 1

			
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma shader_feature _ _SAMPLE_GI
			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ DEBUG_DISPLAY
			#define SHADERPASS SHADERPASS_UNLIT


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"


			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR
			#pragma multi_compile_local _KEYWORD0_RGBA _KEYWORD0_R _KEYWORD0_G _KEYWORD0_B


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
				float fogFactor : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color0;
			float4 _Dissolve_ST;
			float4 _Noise_ST;
			float4 _Dissolve1_ST;
			float4 _Color4;
			float4 _Mask_ST;
			float4 _Main_ST;
			float4 _Color2;
			float4 _Mask1_ST;
			float4 _Color3;
			float4 _TextureSample0_ST;
			float4 _Color1;
			float3 _Vector12;
			float2 _Vector11;
			float2 _Vector2;
			float2 _Vector4;
			float2 _Vector3;
			float2 _aegrdfgaergare;
			float2 _Vector13;
			float2 _Vector5;
			float2 _Vector7;
			float2 _Vector6;
			float _Float541111111111111111111;
			float _Float95;
			float _Float30;
			float _Float60;
			float _Float28;
			float _Float55;
			float _Float93;
			float _Float38;
			float _Float67;
			float _Float90;
			float _Float78;
			float _Float94;
			float _Float97;
			float _Float71;
			float _Float87;
			float _Float6;
			float _Float59;
			float _Float0;
			float _Float57;
			float _Float5;
			float _Float9;
			float _Float54;
			float _Float56;
			float _Float19;
			float _Float17;
			float _Float35;
			float _Float39;
			float _Float36;
			float _Float37;
			float _Float74;
			float _Float72;
			float _Float70;
			float _Float62;
			float _Float65;
			float _Float61;
			float _Float76;
			float _Float11;
			float _Float29;
			float _Float101;
			float _Float102;
			float _Float99;
			float _Float86;
			float _Float96;
			float _Float88;
			float _Float89;
			float _Float84;
			float _Float31;
			float _Float40;
			float _Float22;
			float _Float85;
			float _Blendmode;
			float _Float26;
			float _Float18;
			float _Float75;
			float _Float25;
			float _Float24;
			float _Float27;
			float _Float100;
			float _Float66;
			float _Float4;
			float _Float1;
			float _Float10;
			float _Float81;
			float _Float14;
			float _Float47;
			float _Float48;
			float _Float46;
			float _Float45;
			float _Float44;
			float _Float16;
			float _Float43;
			float _Float42;
			float _Float41;
			float _Float15;
			float _Float3;
			float _Float7;
			float _Float13;
			float _Float12;
			float _Float8;
			float _Float2;
			float _Float20;
			float _Float50;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _Vertex2;
			sampler2D _Vertex;
			sampler2D _Main;
			sampler2D _Noise;
			sampler2D _Mask;
			sampler2D _Mask1;
			sampler2D _Dissolve;
			sampler2D _Dissolve1;
			sampler2D _TextureSample0;
			uniform float4 _CameraDepthTexture_TexelSize;


						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord1087 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float cos1094 = cos( radians( _Float89 ) );
				float sin1094 = sin( radians( _Float89 ) );
				float2 rotator1094 = mul( texCoord1087 - float2( 0,0 ) , float2x2( cos1094 , -sin1094 , sin1094 , cos1094 )) + float2( 0,0 );
				float2 panner1103 = ( 1.0 * _Time.y * _Vector11 + rotator1094);
				float4 tex2DNode1111 = tex2Dlod( _Vertex2, float4( panner1103, 0, 0.0) );
				float4 texCoord1106 = v.ase_texcoord1;
				texCoord1106.xy = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1112 = lerp( _Float88 , texCoord1106.w , _Float96);
				float3 lerpResult1108 = lerp( float3( 0,0,0 ) , v.ase_normal , _Float86);
				float2 texCoord1068 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float cos1078 = cos( radians( _Float99 ) );
				float sin1078 = sin( radians( _Float99 ) );
				float2 rotator1078 = mul( texCoord1068 - float2( 0.5,0.5 ) , float2x2( cos1078 , -sin1078 , sin1078 , cos1078 )) + float2( 0.5,0.5 );
				float2 temp_output_1080_0 = ( rotator1078 + ( _Vector13 * _TimeParameters.x ) );
				float temp_output_1083_0 = (temp_output_1080_0).x;
				float lerpResult1100 = lerp( temp_output_1083_0 , saturate( temp_output_1083_0 ) , _Float102);
				float temp_output_1085_0 = (temp_output_1080_0).y;
				float lerpResult1102 = lerp( temp_output_1085_0 , saturate( temp_output_1085_0 ) , _Float101);
				float2 appendResult1110 = (float2(lerpResult1100 , lerpResult1102));
				float4 tex2DNode1114 = tex2Dlod( _Vertex, float4( appendResult1110, 0, 0.0) );
				float3 lerpResult1117 = lerp( float3( 0,0,0 ) , ( tex2DNode1111.r * lerpResult1112 * ( _Vector12 + lerpResult1108 ) * tex2DNode1114.r * tex2DNode1111.a * tex2DNode1114.a ) , _Float100);
				float3 Vertex1118 = lerpResult1117;
				
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord6.xyz = ase_worldNormal;
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord7 = screenPos;
				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				o.ase_texcoord4 = v.ase_texcoord1;
				o.ase_texcoord5 = v.ase_texcoord2;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				o.ase_texcoord6.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = Vertex1118;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				#ifdef ASE_FOG
				o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN , FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif
				float2 uv_Main = IN.ase_texcoord3.xy * _Main_ST.xy + _Main_ST.zw;
				float4 texCoord687 = IN.ase_texcoord4;
				texCoord687.xy = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult691 = (float2(texCoord687.x , texCoord687.y));
				float2 lerpResult692 = lerp( float2( 0,0 ) , appendResult691 , _Float4);
				float cos648 = cos( radians( _Float20 ) );
				float sin648 = sin( radians( _Float20 ) );
				float2 rotator648 = mul( ( uv_Main + lerpResult692 ) - float2( 0.5,0.5 ) , float2x2( cos648 , -sin648 , sin648 , cos648 )) + float2( 0.5,0.5 );
				float2 temp_output_650_0 = ( rotator648 + ( _Vector4 * _TimeParameters.x ) );
				float temp_output_698_0 = (temp_output_650_0).x;
				float lerpResult656 = lerp( temp_output_698_0 , saturate( temp_output_698_0 ) , _Float1);
				float temp_output_699_0 = (temp_output_650_0).y;
				float lerpResult658 = lerp( temp_output_699_0 , saturate( temp_output_699_0 ) , _Float2);
				float2 appendResult642 = (float2(lerpResult656 , lerpResult658));
				float2 appendResult710 = (float2((appendResult642).y , (appendResult642).x));
				float2 lerpResult711 = lerp( appendResult642 , appendResult710 , _Float8);
				float2 uv_Noise = IN.ase_texcoord3.xy * _Noise_ST.xy + _Noise_ST.zw;
				float4 tex2DNode745 = tex2D( _Noise, ( uv_Noise + ( _Vector2 * _TimeParameters.x ) ) );
				float4 lerpResult748 = lerp( float4( 0,0,0,0 ) , ( tex2DNode745 * tex2DNode745.a ) , _Float12);
				float4 tex2DNode619 = tex2D( _Main, ( float4( lerpResult711, 0.0 , 0.0 ) + ( lerpResult748 * _Float13 ) ).rg );
				float4 temp_cast_2 = (tex2DNode619.r).xxxx;
				float4 temp_cast_3 = (tex2DNode619.g).xxxx;
				float4 temp_cast_4 = (tex2DNode619.b).xxxx;
				#if defined(_KEYWORD0_RGBA)
				float4 staticSwitch1028 = tex2DNode619;
				#elif defined(_KEYWORD0_R)
				float4 staticSwitch1028 = temp_cast_2;
				#elif defined(_KEYWORD0_G)
				float4 staticSwitch1028 = temp_cast_3;
				#elif defined(_KEYWORD0_B)
				float4 staticSwitch1028 = temp_cast_4;
				#else
				float4 staticSwitch1028 = tex2DNode619;
				#endif
				float4 saferPower680 = abs( ( staticSwitch1028 * _Float7 ) );
				float4 temp_cast_5 = (_Float3).xxxx;
				float4 temp_cast_6 = (1.0).xxxx;
				float2 uv_Mask = IN.ase_texcoord3.xy * _Mask_ST.xy + _Mask_ST.zw;
				float cos769 = cos( radians( _Float15 ) );
				float sin769 = sin( radians( _Float15 ) );
				float2 rotator769 = mul( uv_Mask - float2( 0,0 ) , float2x2( cos769 , -sin769 , sin769 , cos769 )) + float2( 0,0 );
				float2 temp_output_765_0 = ( rotator769 + ( _Vector3 * _TimeParameters.x ) );
				float temp_output_865_0 = (temp_output_765_0).x;
				float lerpResult867 = lerp( temp_output_865_0 , saturate( temp_output_865_0 ) , _Float41);
				float temp_output_866_0 = (temp_output_765_0).y;
				float lerpResult868 = lerp( temp_output_866_0 , saturate( temp_output_866_0 ) , _Float42);
				float2 appendResult873 = (float2(lerpResult867 , lerpResult868));
				float4 tex2DNode758 = tex2D( _Mask, appendResult873 );
				float4 saferPower773 = abs( ( tex2DNode758 * _Float43 ) );
				float4 temp_cast_7 = (_Float16).xxxx;
				float4 temp_cast_8 = (1.0).xxxx;
				float2 uv_Mask1 = IN.ase_texcoord3.xy * _Mask1_ST.xy + _Mask1_ST.zw;
				float cos883 = cos( radians( _Float44 ) );
				float sin883 = sin( radians( _Float44 ) );
				float2 rotator883 = mul( uv_Mask1 - float2( 0,0 ) , float2x2( cos883 , -sin883 , sin883 , cos883 )) + float2( 0,0 );
				float2 temp_output_885_0 = ( rotator883 + ( _Vector6 * _TimeParameters.x ) );
				float temp_output_887_0 = (temp_output_885_0).x;
				float lerpResult892 = lerp( temp_output_887_0 , saturate( temp_output_887_0 ) , _Float45);
				float temp_output_886_0 = (temp_output_885_0).y;
				float lerpResult893 = lerp( temp_output_886_0 , saturate( temp_output_886_0 ) , _Float46);
				float2 appendResult894 = (float2(lerpResult892 , lerpResult893));
				float4 tex2DNode895 = tex2D( _Mask1, appendResult894 );
				float4 saferPower899 = abs( ( tex2DNode895 * _Float48 ) );
				float4 temp_cast_9 = (_Float47).xxxx;
				float4 lerpResult1012 = lerp( temp_cast_8 , pow( saferPower899 , temp_cast_9 ) , _Float11);
				float4 temp_cast_10 = (1.0).xxxx;
				float4 texCoord1137 = IN.ase_texcoord5;
				texCoord1137.xy = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1138 = lerp( 1.0 , texCoord1137.z , _Float81);
				float4 temp_cast_11 = (lerpResult1138).xxxx;
				float4 clampResult1140 = clamp( ( tex2DNode895 - temp_cast_11 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
				float4 clampResult1143 = clamp( ( tex2DNode895 * clampResult1140 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
				float4 lerpResult1144 = lerp( temp_cast_10 , clampResult1143 , _Float81);
				float4 lerpResult1011 = lerp( temp_cast_6 , ( pow( saferPower773 , temp_cast_7 ) * lerpResult1012 * lerpResult1144 ) , _Float10);
				float clampResult1128 = clamp( _Float29 , -0.5 , 0.5 );
				float2 uv_Dissolve = IN.ase_texcoord3.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float cos1036 = cos( radians( _Float66 ) );
				float sin1036 = sin( radians( _Float66 ) );
				float2 rotator1036 = mul( uv_Dissolve - float2( 0.5,0.5 ) , float2x2( cos1036 , -sin1036 , sin1036 , cos1036 )) + float2( 0.5,0.5 );
				float4 tex2DNode795 = tex2D( _Dissolve, ( rotator1036 + ( _Vector5 * _TimeParameters.x ) ) );
				float lerpResult1025 = lerp( tex2DNode795.r , tex2DNode795.g , _Float62);
				float2 uv_Dissolve1 = IN.ase_texcoord3.xy * _Dissolve1_ST.xy + _Dissolve1_ST.zw;
				float2 texCoord1154 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult1155 = (float2(texCoord1154.x , texCoord1154.y));
				float2 lerpResult1157 = lerp( float2( 0,0 ) , appendResult1155 , _Float90);
				float cos1041 = cos( radians( _Float67 ) );
				float sin1041 = sin( radians( _Float67 ) );
				float2 rotator1041 = mul( ( uv_Dissolve1 + lerpResult1157 ) - float2( 0.5,0.5 ) , float2x2( cos1041 , -sin1041 , sin1041 , cos1041 )) + float2( 0.5,0.5 );
				float lerpResult932 = lerp( 1.0 , ( tex2D( _Dissolve1, ( rotator1041 + ( _aegrdfgaergare * _TimeParameters.x ) ) ).r * _Float541111111111111111111 ) , _Float55);
				float lerpResult944 = lerp( ( lerpResult1025 + _Float78 ) , lerpResult932 , _Float30);
				float2 appendResult922 = (float2(_Float93 , _Float94));
				float2 texCoord924 = IN.ase_texcoord3.xy * float2( 1,1 ) + appendResult922;
				float cos925 = cos( radians( _Float95 ) );
				float sin925 = sin( radians( _Float95 ) );
				float2 rotator925 = mul( texCoord924 - float2( 0.5,0.5 ) , float2x2( cos925 , -sin925 , sin925 , cos925 )) + float2( 0.5,0.5 );
				float lerpResult929 = lerp( 1.0 , (rotator925).x , _Float97);
				float temp_output_930_0 = ( lerpResult944 * lerpResult929 );
				float4 texCoord829 = IN.ase_texcoord4;
				texCoord829.xy = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult834 = lerp( _Float28 , texCoord829.z , _Float38);
				float smoothstepResult798 = smoothstep( clampResult1128 , ( 1.0 - clampResult1128 ) , ( temp_output_930_0 + ( lerpResult834 * -2.0 ) + 1.0 ));
				float temp_output_812_0 = step( lerpResult834 , temp_output_930_0 );
				float lerpResult1016 = lerp( smoothstepResult798 , temp_output_812_0 , _Float60);
				float lerpResult1015 = lerp( 1.0 , lerpResult1016 , _Float61);
				float4 texCoord1148 = IN.ase_texcoord4;
				texCoord1148.xy = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1149 = lerp( 0.0 , texCoord1148.w , _Float87);
				float4 temp_output_1150_0 = ( _Color0 + lerpResult1149 );
				float4 switchResult1029 = (((ase_vface>0)?(temp_output_1150_0):(_Color4)));
				float4 lerpResult1032 = lerp( temp_output_1150_0 , switchResult1029 , _Float65);
				float switchResult1031 = (((ase_vface>0)?(_Color0.a):(_Color4.a)));
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float3 ase_worldNormal = IN.ase_texcoord6.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float fresnelNdotV1081 = dot( normalize( ( normalizedWorldNormal * ase_vface ) ), ase_worldViewDir );
				float fresnelNode1081 = ( 0.0 + _Float71 * pow( max( 1.0 - fresnelNdotV1081 , 0.0001 ), _Float70 ) );
				float temp_output_1082_0 = saturate( fresnelNode1081 );
				float lerpResult1101 = lerp( temp_output_1082_0 , ( 1.0 - temp_output_1082_0 ) , _Float72);
				float lerpResult1104 = lerp( 1.0 , lerpResult1101 , _Float74);
				float4 temp_output_671_0 = ( pow( saferPower680 , temp_cast_5 ) * lerpResult1011 * lerpResult1015 * lerpResult1032 * IN.ase_color * switchResult1031 * lerpResult1104 );
				float temp_output_815_0 = step( ( _Float36 + lerpResult834 ) , temp_output_930_0 );
				float lerpResult1021 = lerp( 0.0 , ( temp_output_812_0 - temp_output_815_0 ) , _Float60);
				float rJ842 = lerpResult1021;
				float4 lerpResult843 = lerp( temp_output_671_0 , ( _Float37 * _Color2 ) , rJ842);
				float lerpResult1023 = lerp( 0.0 , ( temp_output_815_0 - step( ( lerpResult834 + _Float36 + _Float35 ) , temp_output_930_0 ) ) , _Float60);
				float RJ2851 = lerpResult1023;
				float4 lerpResult852 = lerp( lerpResult843 , ( _Float39 * _Color3 ) , RJ2851);
				float4 saferPower785 = abs( temp_output_671_0 );
				float4 temp_cast_12 = (_Float17).xxxx;
				float2 uv_TextureSample0 = IN.ase_texcoord3.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
				float cos986 = cos( radians( _Float56 ) );
				float sin986 = sin( radians( _Float56 ) );
				float2 rotator986 = mul( uv_TextureSample0 - float2( 0.5,0.5 ) , float2x2( cos986 , -sin986 , sin986 , cos986 )) + float2( 0.5,0.5 );
				float4 tex2DNode951 = tex2D( _TextureSample0, ( rotator986 + ( _Vector7 * _TimeParameters.x ) ) );
				float4 saferPower954 = abs( tex2DNode951 );
				float4 temp_cast_13 = (_Float54).xxxx;
				float4 lerpResult1005 = lerp( float4( 0,0,0,0 ) , saturate( pow( saferPower954 , temp_cast_13 ) ) , _Float9);
				float4 screenPos = IN.ase_texcoord7;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth701 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float distanceDepth701 = abs( ( screenDepth701 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _Float5 ) );
				float temp_output_703_0 = saturate( distanceDepth701 );
				
				float4 temp_cast_15 = (1.0).xxxx;
				float4 temp_cast_16 = (tex2DNode951.a).xxxx;
				float4 lerpResult956 = lerp( temp_cast_16 , tex2DNode951 , _Float57);
				float4 saferPower955 = abs( lerpResult956 );
				float4 temp_cast_17 = (_Float54).xxxx;
				float4 temp_output_959_0 = saturate( pow( saferPower955 , temp_cast_17 ) );
				float4 lerpResult1007 = lerp( temp_cast_15 , temp_output_959_0 , _Float9);
				float4 temp_cast_18 = (tex2DNode619.a).xxxx;
				float4 lerpResult623 = lerp( temp_cast_18 , tex2DNode619 , _Float0);
				float4 saferPower683 = abs( lerpResult623 );
				float4 temp_cast_19 = (_Float3).xxxx;
				float4 temp_output_683_0 = pow( saferPower683 , temp_cast_19 );
				float4 lerpResult1008 = lerp( float4( 0,0,0,0 ) , temp_output_959_0 , _Float9);
				float4 lerpResult980 = lerp( ( lerpResult1007 * temp_output_683_0 ) , ( lerpResult1008 + temp_output_683_0 ) , _Float59);
				float4 temp_cast_20 = (1.0).xxxx;
				float4 temp_cast_21 = (tex2DNode758.a).xxxx;
				float4 lerpResult762 = lerp( temp_cast_21 , tex2DNode758 , _Float14);
				float4 saferPower1046 = abs( ( _Float43 * lerpResult762 ) );
				float4 temp_cast_22 = (_Float16).xxxx;
				float4 temp_cast_23 = (1.0).xxxx;
				float4 temp_cast_24 = (tex2DNode895.a).xxxx;
				float4 lerpResult906 = lerp( temp_cast_24 , tex2DNode895 , _Float50);
				float4 saferPower1048 = abs( ( _Float48 * lerpResult906 ) );
				float4 temp_cast_25 = (_Float47).xxxx;
				float4 lerpResult1014 = lerp( temp_cast_23 , pow( saferPower1048 , temp_cast_25 ) , _Float11);
				float4 lerpResult1009 = lerp( temp_cast_20 , ( pow( saferPower1046 , temp_cast_22 ) * lerpResult1014 ) , _Float10);
				float lerpResult1034 = lerp( _Color0.a , switchResult1031 , _Float65);
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = ( ( lerpResult852 + ( pow( saferPower785 , temp_cast_12 ) * _Float19 * _Color1 * _Color1.a ) + lerpResult1005 ) * temp_output_703_0 ).rgb;
				float Alpha = ( lerpResult980 * ( temp_output_683_0 * _Float6 * lerpResult1009 * lerpResult1015 * lerpResult1034 * IN.ase_color.a * lerpResult1104 ) * temp_output_703_0 ).r;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.clipPos, Color);
				#endif

				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif


				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

	
	}
	
	CustomEditor "LWGUI.LWGUI"
	
	
}
/*ASEBEGIN
Version=18935
-1805;89;1920;891;4061.177;-3642.894;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;705;-4290.684,-852.1572;Inherit;False;834.0901;349;;4;687;694;691;692;主贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;687;-4240.684,-782.7599;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;915;-3844.935,1818.554;Inherit;False;3444.734;645.6752;;32;900;894;893;899;892;878;882;879;881;890;888;896;898;891;883;887;889;880;884;885;886;904;895;901;906;916;917;1012;1014;1013;1048;1132;附加遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;694;-3992.594,-619.1572;Inherit;False;Property;_Float4;Custom1XY控制主贴图uv;7;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;691;-3916.776,-770.1977;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;715;-3156.972,-970.6801;Inherit;False;1015.928;565.5943;;9;643;644;688;646;647;645;649;648;650;主贴图uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;692;-3640.595,-802.1572;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;878;-3794.935,2033.554;Inherit;False;Property;_Float44;m旋转;47;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;644;-3085.908,-920.6802;Inherit;False;0;619;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;643;-3106.972,-738.2395;Inherit;False;Property;_Float20;主贴图旋转;23;0;Create;False;0;0;0;True;1;Sub(z1);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1153;-6143.418,4454.662;Inherit;False;779.3872;338.4731;;4;1157;1156;1155;1154;附加溶解贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.RadiansOpNode;882;-3444.941,2036.555;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;646;-2808.408,-733.4997;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;881;-3433.941,2143.554;Inherit;False;Property;_Vector6;muv;48;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;647;-2766.073,-647.3347;Inherit;False;Property;_Vector4;主贴图UV;24;0;Create;False;0;0;0;True;1;Sub(z1);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;1154;-6093.418,4504.662;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;688;-2755.999,-864.1628;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;880;-3445.941,2281.554;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;879;-3583.936,1868.554;Inherit;False;0;895;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;645;-2769.739,-516.0858;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1155;-5768.143,4522.374;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1045;-5273.286,4369.91;Inherit;False;1984.322;547.9778;;14;940;941;942;938;936;931;935;934;937;945;1041;939;1042;1044;附加溶解贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;649;-2510.193,-674.0046;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1156;-5903.904,4677.134;Inherit;False;Property;_Float90;Custom2XY控制附加溶解贴图uv;10;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1040;-4787.556,3246.701;Inherit;False;990.9915;552.217;;8;837;838;840;839;1036;1037;836;1039;溶解uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;884;-3177.94,2133.554;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1133;-3407.747,901.9066;Inherit;False;4141.369;771.5731;;33;771;770;764;766;767;769;768;765;865;866;871;869;870;872;868;867;873;875;758;774;874;773;763;762;908;1010;792;1011;1131;1046;793;914;1009;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.RotatorNode;883;-3214.94,1965.554;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;648;-2563.166,-834.1674;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;650;-2294.046,-746.7887;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1157;-5546.031,4528.042;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1044;-5249.286,4716.653;Inherit;False;Property;_Float67;附加溶解贴图旋转;73;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;939;-5248.225,4415.91;Inherit;False;0;931;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;718;-1811.048,-967.7505;Inherit;False;1031.703;551.2322;;9;699;698;657;659;654;655;658;656;642;主贴图不循环;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;757;-1703.928,-143.5765;Inherit;False;1815.883;444;;11;753;755;754;752;756;746;749;751;748;750;745;noise扰动;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1039;-4827.556,3442.191;Inherit;False;Property;_Float66;溶解贴图旋转;60;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;771;-3357.747,1116.907;Inherit;False;Property;_Float15;遮罩图旋转;38;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;885;-2980.938,2063.555;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;918;-4933.486,5006.64;Inherit;False;1591.615;427.6962;;11;925;924;923;922;921;920;919;927;928;929;926;方向溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;766;-2996.748,1226.906;Inherit;False;Property;_Vector3;遮罩uv;39;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;839;-4561.761,3687.918;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;764;-3146.748,951.9066;Inherit;False;0;758;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;698;-1761.048,-897.8173;Inherit;False;True;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;767;-3008.748,1364.906;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;887;-2804.385,1981.229;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;840;-4545.563,3536.417;Inherit;False;Property;_Vector5;溶解贴图uv;69;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ComponentMaskNode;699;-1758.737,-696.6046;Inherit;False;False;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;886;-2808.385,2166.229;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1158;-4940.492,4468.968;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;836;-4618.157,3296.701;Inherit;False;0;795;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;755;-1653.928,189.4235;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;770;-3007.748,1119.907;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;753;-1640.928,69.42346;Inherit;False;Property;_Vector2;扰动uv;53;0;Create;False;0;0;0;True;1;Sub(z3);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;941;-4843.225,4799.537;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;940;-4836.225,4662.909;Inherit;False;Property;_aegrdfgaergare;附加溶解贴图uv;75;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RadiansOpNode;1037;-4506.782,3451.326;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1042;-4974.115,4626.186;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;888;-2614.384,2083.23;Inherit;False;Property;_Float45;mxclamp;43;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;889;-2546.384,1995.229;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;891;-2597.384,2251.23;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;942;-4615.225,4721.909;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;659;-1471.503,-532.5184;Inherit;False;Property;_Float2;主贴图yclamp;20;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;1036;-4261.542,3350.659;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;920;-4888.487,5172.898;Inherit;False;Property;_Float94;方向V偏移;79;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;657;-1477.503,-782.5176;Inherit;False;Property;_Float1;主贴图xclamp;19;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;919;-4885.487,5094.088;Inherit;False;Property;_Float93;方向U偏移;78;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;768;-2740.751,1216.906;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;1041;-4721.115,4509.186;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;838;-4278.565,3567.417;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;655;-1464.417,-619.9125;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;752;-1644.928,-93.57654;Inherit;False;0;745;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;890;-2607.384,2348.229;Inherit;False;Property;_Float46;myclamp;44;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;828;-3533.585,3297.621;Inherit;False;3867.203;1427.534;;16;810;809;800;803;829;834;835;930;944;1015;1016;1018;1026;932;1124;1125;溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.RotatorNode;769;-2777.75,1048.907;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;754;-1397.928,113.4235;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;654;-1462.417,-858.9122;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;922;-4708.612,5116.338;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;658;-1188.503,-700.5177;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;938;-4439.227,4617.909;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;921;-4842.61,5308.337;Inherit;False;Property;_Float95;方向溶解旋转（默认左黑右白）;77;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;810;-3483.585,3435.817;Inherit;False;785.1409;280;;2;795;1025;溶解贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;892;-2290.384,1916.229;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1134;-2252.543,2558.635;Inherit;False;1892.448;560.886;;10;1144;1143;1142;1141;1140;1139;1138;1137;1136;1135;CenterGlow;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;837;-3948.564,3477.416;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;765;-2543.75,1146.907;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;756;-1202.928,1.423462;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;656;-1216.722,-917.7505;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;893;-2290.384,2216.23;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;923;-4447.614,5307.337;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;865;-2367.197,1064.582;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;894;-2121.385,2032.229;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;795;-3514.785,3487.917;Inherit;True;Property;_Dissolve;溶解贴图;57;0;Create;False;0;0;0;True;1;Sub(z4);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;931;-4242.164,4572.556;Inherit;True;Property;_Dissolve1;附加溶解贴图;71;0;Create;False;0;0;0;False;1;Sub(z4);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1065;699.5504,4648.385;Inherit;False;3297.479;1396.09;;38;1118;1117;1116;1115;1114;1113;1112;1111;1110;1109;1108;1107;1106;1105;1103;1102;1100;1098;1097;1096;1094;1092;1091;1090;1089;1088;1087;1085;1084;1083;1080;1078;1074;1072;1071;1070;1068;1067;顶点偏移;1,1,1,1;0;0
Node;AmplifyShaderEditor.ComponentMaskNode;866;-2371.197,1249.581;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;642;-944.3453,-777.5762;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1136;-1983.81,2780.54;Inherit;False;Constant;_Float82;Float 77;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;924;-4531.61,5067.338;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1135;-2221.644,3006.65;Inherit;False;Property;_Float81;Custom2Z控制centerGlow;11;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1026;-3393.4,3711.211;Inherit;False;Property;_Float62;溶解通道;59;0;Create;False;0;0;0;True;1;KWEnum(z4,R,_0,G,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1137;-2227.62,2765.032;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;713;-529.8556,-825.4449;Inherit;False;744.8823;393.0596;;5;706;707;712;710;711;主贴图极坐标2;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;936;-4153.224,4797.909;Inherit;False;Property;_Float541111111111111111111;附加溶解贴图强度;74;0;Create;False;0;0;0;True;1;Sub(z4);False;1;11;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;745;-990.6514,-36.1528;Inherit;True;Property;_Noise;扰动贴图;51;0;Create;False;0;0;0;True;1;Sub(z3);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1066;1078.585,3531.37;Inherit;False;1475.065;723.4756;;14;1104;1101;1099;1095;1093;1086;1082;1081;1079;1077;1076;1075;1073;1069;菲尼尔;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1067;749.55,5699.183;Inherit;False;Property;_Float99;顶点偏移M旋转;105;0;Create;False;0;0;0;False;1;Sub(z8);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1125;-3461.136,4278.357;Inherit;False;Property;_Float78;溶解贴图纹理加强;58;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;925;-4260.617,5180.338;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;937;-3882.223,4792.909;Inherit;False;Property;_Float55;附加溶解贴图开关;70;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;870;-2109.196,1078.582;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;935;-3796.223,4638.909;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;706;-478.8557,-722.0425;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;988;-534.1824,-1879.859;Inherit;False;4020.037;699.9189;;25;968;961;967;965;964;986;966;962;969;951;956;960;955;977;959;954;982;958;981;983;980;1005;1007;1008;1006;叠加;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;869;-2177.197,1166.581;Inherit;False;Property;_Float41;遮罩xclamp;34;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1138;-1745.278,2923.048;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;895;-1957.187,1970.965;Inherit;True;Property;_Mask1;m贴图;41;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;934;-3714.223,4480.91;Inherit;False;Constant;_Float53;Float 53;69;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;707;-479.8557,-620.0425;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1025;-3094.4,3528.211;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;871;-2160.196,1334.581;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;749;-583.2661,176.1942;Inherit;False;Property;_Float12;扰动开关;50;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;872;-2170.196,1431.581;Inherit;False;Property;_Float42;遮罩yclamp;35;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;746;-583.3665,-11.32115;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;868;-1853.196,1299.581;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;863;-2615.852,3985.952;Inherit;False;2089.603;882.1636;;16;847;848;850;815;816;845;817;851;842;814;812;1020;1021;1023;1024;1017;亮边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;926;-3988.549,5345.11;Inherit;False;Property;_Float97;方向溶解开关;76;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;968;-483.1824,-1660.859;Inherit;False;Property;_Float56;叠加旋转;86;0;Create;False;0;0;0;True;1;Sub(z5);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.FaceVariableNode;1073;1207.877,3714.835;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;751;-210.9782,136.7216;Inherit;False;Property;_Float13;扰动强度;52;0;Create;False;0;0;0;True;1;Sub(z3);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;829;-3253.164,3930.696;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;1071;1084.302,5785.513;Inherit;False;Property;_Vector13;顶点偏移MUV;106;0;Create;False;0;0;0;False;1;Sub(z8);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;867;-1853.196,999.5817;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1068;1040.972,5550.214;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;1072;1080.637,5918.762;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;1069;1102.14,3574.73;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;809;-2259.752,3354.673;Inherit;False;1733.592;529.1241;;10;805;807;798;801;806;804;1126;1127;1128;1129;软边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;835;-2932.636,4055.428;Inherit;False;Property;_Float38;Custom1Z控制溶解程度;8;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;712;-245.8609,-548.3855;Inherit;False;Property;_Float8;主贴图极坐标uv翻转;25;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;945;-3588.963,4801.889;Inherit;False;Property;_Float30;溶解纹理偏向;72;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1124;-3257.985,4420.511;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;748;-294.6189,-40.55897;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;932;-3472.968,4611.804;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;928;-3975.6,5056.641;Inherit;False;Constant;_Float52;Float 52;32;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;927;-4049.063,5142.473;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;710;-221.8557,-697.0425;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;800;-3120.656,3773.355;Inherit;False;Property;_Float28;溶解程度;61;0;Create;False;0;0;0;True;1;Sub(z4);False;1;0;-0.2;1.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1070;1042.967,5707.349;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1139;-1492.53,2898.228;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;801;-1774.37,3630.204;Inherit;False;Property;_Float29;软溶解;62;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;967;-157.1829,-1664.859;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1075;1119.13,4104.665;Inherit;False;Property;_Float70;菲尼尔power;90;0;Create;False;0;0;0;True;1;Sub(z7);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;834;-2597.492,3788.881;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;1078;1287.209,5600.681;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;750;43.53482,-113.7403;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;805;-2233.242,3424.116;Inherit;False;Constant;_Float33;Float 33;44;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;929;-3697.246,5167.227;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;964;-140.1829,-1550.859;Inherit;False;Property;_Vector7;叠加uv;88;0;Create;False;0;0;0;True;1;Sub(z5);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1074;1340.181,5760.843;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;898;-1585.132,2089.878;Inherit;False;Property;_Float48;m强度;46;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;961;-299.1829,-1829.859;Inherit;False;0;951;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;711;31.02668,-775.4449;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;965;-158.1829,-1419.859;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;1077;1167.588,3787.887;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;944;-3117.564,4561.188;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1076;1131.13,3963.665;Inherit;False;Property;_Float71;菲尼尔范围;91;0;Create;False;0;0;0;True;1;Sub(z7);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1079;1424.877,3661.835;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;1140;-1290.689,2896.453;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1126;-1454.143,3750.613;Inherit;False;Constant;_Float79;Float 73;85;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;873;-1684.197,1115.582;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;817;-2570.851,4281.553;Inherit;False;Property;_Float36;外层亮边宽度;65;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1151;968.4254,18.07187;Inherit;False;1599.971;769.5508;;12;1030;1029;1033;1031;1032;781;1034;1149;1150;672;1147;1148;颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1127;-1456.669,3673.131;Inherit;False;Constant;_Float80;Float 72;85;0;Create;True;0;0;0;False;0;False;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;686;457.023,-987.8872;Inherit;False;1736.342;788.0424;;9;619;624;623;683;680;681;1028;1053;1057;主贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1141;-1031.973,2879.74;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;758;-1510.002,1121.317;Inherit;True;Property;_Mask;遮罩贴图;32;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;896;-1352.622,2127.922;Inherit;False;Property;_Float47;mpower;45;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;850;-2591.717,4655.153;Inherit;False;Property;_Float35;内层亮边2宽度;68;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.06;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;986;87.32974,-1760.795;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;930;-2840.918,4545.129;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1080;1556.327,5688.058;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FresnelNode;1081;1472.395,3841.692;Inherit;False;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;804;-1937.334,3402.64;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;966;105.8174,-1502.859;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;875;-1086.154,1251.795;Inherit;False;Property;_Float43;遮罩强度;37;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;816;-2263.94,4291.198;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1147;1018.425,511.5022;Inherit;False;Property;_Float87;Custom1W增加贴图亮度;9;0;Create;False;0;0;0;True;1;SubToggle(z9,_);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;807;-1822.535,3503.725;Inherit;False;Constant;_Float34;Float 34;44;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;747;289.1848,-698.8005;Inherit;False;2;2;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;900;-1380.532,2004.078;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1148;1022.86,305.7401;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;1128;-1247.982,3629.123;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;672;1252.396,68.07187;Inherit;False;Property;_Color0;颜色Front;14;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;774;-798.2357,1249.275;Inherit;False;Property;_Float16;遮罩power;36;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;812;-2018.319,4035.952;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1129;-1003.418,3687.99;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;806;-1524.789,3386.126;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;845;-2252.788,4616.908;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1082;1715.287,3843.696;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;962;350.8173,-1674.859;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;899;-1179.99,1993.495;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1142;-843.1406,2620.853;Inherit;False;Constant;_Float83;Float 78;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;1083;1746.071,5698.474;Inherit;False;FLOAT;0;1;2;3;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;904;-1825.918,2296.409;Inherit;False;Property;_Float50;mA通道;42;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;1143;-865.7928,2875.689;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;815;-2024.941,4315.345;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1149;1345.305,367.0186;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;916;-776.0408,1886.145;Inherit;False;Constant;_Float49;Float 49;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;1085;1728.071,5815.474;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;874;-830.6115,1138.142;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1013;-1143.998,1927.949;Inherit;False;Property;_Float11;附加遮罩开关;40;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1084;1819.122,4931.07;Inherit;False;Property;_Float89;顶点贴图旋转;97;0;Create;False;0;0;0;False;1;Sub(z8);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;619;482.8695,-925.7279;Inherit;True;Property;_Main;主贴图;16;0;Create;False;0;0;0;True;1;Sub(z1);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;969;764.396,-1512.417;Inherit;False;Property;_Float57;叠加贴图通道;83;0;Create;False;0;0;0;True;1;KWEnum(z5,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1088;1922.071,5833.474;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;763;-1387.804,1421.103;Inherit;False;Property;_Float14;遮罩A通道;33;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1092;2018.072,5705.474;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;1028;819.2833,-920.4319;Inherit;False;Property;_Keyword0;主贴图通道;17;0;Create;False;0;0;0;False;0;False;1;0;0;True;;KeywordEnum;4;RGBA;R;G;B;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1057;946.9291,-756.6324;Inherit;False;Property;_Float7;主贴图强度;22;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1144;-672.425,2624.691;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1020;-1293.416,4078.01;Inherit;False;Constant;_Float63;Float 63;44;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1093;1680.131,4097.665;Inherit;False;Property;_Float72;反向菲尼尔（虚化边缘）;92;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;906;-1592.884,2240.879;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1150;1667.813,95.75772;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;773;-493.9539,1159.852;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1030;1666.396,318.659;Inherit;False;Property;_Color4;颜色Back;15;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1089;2001.072,5929.474;Inherit;False;Property;_Float101;顶点MyClamp;104;0;Create;False;0;0;0;True;1;KWEnum(z8,Off,_0,On,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1086;1809.296,3945.88;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;798;-780.1574,3404.672;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;814;-1552.941,4137.045;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;847;-2048.114,4614.115;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;951;594.0963,-1713.612;Inherit;True;Property;_TextureSample0;叠加贴图;82;0;Create;False;0;0;0;True;1;Sub(z5);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RadiansOpNode;1091;2086.124,4860.07;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1087;1973.303,4698.385;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1017;-1650.187,4406.428;Inherit;False;Property;_Float60;溶解模式;56;0;Create;False;0;0;0;True;1;KWEnum(z4,Soft,_0,Rim,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1090;1916.071,5612.474;Inherit;False;Property;_Float102;顶点MxClamp;103;0;Create;False;0;0;0;True;1;KWEnum(z8,Off,_0,On,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1012;-606.2303,1935.507;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;792;-197.6735,998.2128;Inherit;False;Constant;_Float21;Float 21;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1095;2008.479,3733.617;Inherit;False;Constant;_Float73;Float 15;94;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1132;-1363.602,2255.415;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;960;1251.602,-1617.229;Inherit;False;Property;_Float54;叠加贴图power;85;0;Create;False;0;0;0;True;1;Sub(z5);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1010;-284.6963,1355.859;Inherit;False;Property;_Float10;遮罩开关;31;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;681;1300.315,-803.7775;Inherit;False;Property;_Float3;主贴图power;21;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;762;-1077.769,1362.817;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;908;-33.9058,1171.129;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RotatorNode;1094;2301.813,4766.507;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;956;1033.292,-1631.061;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;1097;2302.813,4966.507;Inherit;False;Property;_Vector11;顶点贴图UV;101;0;Create;False;0;0;0;True;1;Sub(z8);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.NormalVertexDataNode;1096;2577.369,5310.358;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1053;1106.802,-925.0612;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;948;3676.666,-343.5782;Inherit;False;911.7981;680.3812;;10;822;823;820;844;843;855;854;853;856;852;溶解亮边;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1099;2038.131,4076.665;Inherit;False;Property;_Float74;菲尼尔开关;89;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1033;2023.196,316.2589;Inherit;False;Property;_Float65;双面颜色;13;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwitchByFaceNode;1029;1986.396,139.0719;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;803;-299.9562,3385.034;Inherit;False;Constant;_Float32;Float 32;44;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1024;-1259.836,4528.838;Inherit;False;Constant;_Float64;Float 64;44;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1018;-246.8353,3693.638;Inherit;False;Property;_Float61;溶解开关;55;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1101;2043.674,3853.596;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1016;-249.8353,3541.638;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1102;2242.073,5785.474;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;848;-1549.434,4570.715;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1021;-1053.295,4170.613;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1100;2242.073,5561.474;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1098;2572.464,5473.025;Inherit;False;Property;_Float86;模型顶点法向;96;0;Create;False;0;0;0;True;1;SubToggle(z8,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;823;3770.666,-293.5783;Inherit;False;Property;_Float37;外层亮边颜色强度;64;0;Create;False;0;0;0;True;1;Sub(z4);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1108;2813.37,5414.058;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;842;-778.4555,4162.144;Inherit;False;rJ;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1023;-1043.017,4609.899;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;1107;2804.535,5270.536;Inherit;False;Property;_Vector12;顶点偏移轴向;100;0;Create;False;0;0;0;False;1;Sub(z8);False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;624;697.4335,-513.0425;Inherit;False;Property;_Float0;主贴图A通道;18;0;Create;False;0;0;0;True;1;KWEnum(z1,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;680;1569.67,-922.4786;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;1110;2482.073,5689.474;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;1103;2569.812,4863.506;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;791;3580.895,1849.147;Inherit;False;628.9875;471.9476;;5;786;789;785;788;787;高光;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1131;-832.9,1382.721;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1105;2259.862,5098.672;Inherit;False;Property;_Float88;顶点偏移强度;98;0;Create;False;0;0;0;True;1;Sub(z8);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1104;2283.83,3847.661;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;781;1668.597,580.6227;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;917;-757.8058,2103.03;Inherit;False;Constant;_Float51;Float 51;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1106;2115.096,5238.793;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1015;18.36256,3466.066;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;760;3055.194,-839.9109;Inherit;False;216;233;;1;671;RGB;1,1,1,1;0;0
Node;AmplifyShaderEditor.PowerNode;1048;-1048.435,2226.801;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1032;2386.397,94.65909;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1011;412.8612,1036.393;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1109;2255.024,5416.025;Inherit;False;Property;_Float96;顶点偏移强度自定义;99;0;Create;False;0;0;0;True;1;SubToggle(z8,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwitchByFaceNode;1031;1986.396,414.6589;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;955;1501.26,-1527.298;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;822;3726.666,-214.5783;Inherit;False;Property;_Color2;外层亮边颜色;63;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;977;1761.773,-1611.551;Inherit;False;Constant;_Float58;Float 58;78;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;959;1759.016,-1518.959;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;704;3628.58,1453.343;Inherit;False;804.9153;185.5732;;3;702;701;703;软粒子;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;1114;2676.11,5661.518;Inherit;True;Property;_Vertex;顶点遮罩;102;0;Create;False;0;0;0;True;1;Sub(z8);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;1046;-483.3754,1375.529;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1111;2790.672,4843.071;Inherit;True;Property;_Vertex2;顶点偏移贴图;95;0;Create;False;0;0;0;True;1;Sub(z8);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1014;-578.9977,2194.949;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;623;997.5806,-550.8588;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;786;3630.895,1953.047;Inherit;False;Property;_Float17;高光细节;28;0;Create;False;0;0;0;True;1;Sub(z6);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1112;2660.644,5041.646;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1006;1753.321,-1355.996;Inherit;False;Property;_Float9;叠加开关;81;0;Create;False;0;0;0;True;1;SubToggle(z5,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;671;3105.194,-789.9109;Inherit;False;7;7;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;FLOAT;0;False;6;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;954;1497.914,-1710.643;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;851;-758.7301,4632.066;Inherit;False;RJ2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;854;3875.098,-10.63948;Inherit;False;Property;_Float39;内层亮边2颜色强度;67;0;Create;False;0;0;0;True;1;Sub(z4);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;855;3837.098,93.36063;Inherit;False;Property;_Color3;内层亮边2颜色;66;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;844;4009.839,-275.8445;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;820;3964.304,-116.9101;Inherit;False;842;rJ;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1113;3023.47,5328.957;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;785;3820.996,1899.147;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;789;3747.882,2109.096;Inherit;False;Property;_Color1;高光颜色;27;0;Create;False;0;0;0;True;1;Sub(z6);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;843;4256.473,-272.2322;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;856;4155.898,78.86073;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;914;196.7068,1538.479;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;793;82.73172,1333.762;Inherit;False;Constant;_Float23;Float 23;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;683;1607.661,-512.9229;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1115;3241.733,4872.01;Inherit;False;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1116;3338.897,5073.792;Inherit;False;Property;_Float100;顶点偏移开关;94;0;Create;False;0;0;0;True;1;SubToggle(z8,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;788;3820.882,2023.096;Inherit;False;Property;_Float19;高光强度;29;0;Create;False;0;0;0;True;1;Sub(z6);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;853;4122.047,220.8028;Inherit;False;851;RJ2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;958;1758.016,-1707.059;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1008;2172.321,-1402.996;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1007;2166.321,-1552.996;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;1152;3202.553,701.8537;Inherit;False;755.478;313.493;;2;670;669;Alpha;1,0,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;702;3678.58,1521.935;Inherit;False;Property;_Float5;软粒子（相机需开启投影）;3;0;Create;False;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;787;4043.882,1933.095;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1009;551.6228,1324.4;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;670;3252.553,751.8537;Inherit;False;Property;_Float6;整体Alpha强度;2;0;Create;False;0;0;0;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1034;2386.397,366.6589;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;981;2766.805,-1295.941;Inherit;False;Property;_Float59;A通道算法（Add时通道不同）;84;0;Create;False;0;0;0;True;1;KWEnum(z5,Mul,_0,Add,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;852;4406.464,38.45406;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;982;2577.714,-1540.432;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;983;2580.114,-1374.632;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;701;3945.887,1503.916;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1005;2161.486,-1718.826;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1117;3530.056,4863.808;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;784;5222.927,-693.6653;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;703;4265.495,1503.343;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;980;3303.854,-1434.987;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;669;3796.031,760.3467;Inherit;False;7;7;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;1146;4552.379,-1965.775;Inherit;False;558.9463;959.2749;Comment;13;1121;143;140;141;130;142;120;550;121;952;794;1120;1145;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1118;3764.03,4843.688;Inherit;False;Vertex;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;120;4615.128,-1726.482;Inherit;False;Property;_Blendmode;混合模式;0;1;[Enum];Create;False;0;2;add;1;blend;10;0;True;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;901;-994.3758,2049.421;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;142;4927.483,-1719.769;Inherit;False;Property;_Float26;扰动列表;49;0;Create;False;0;0;0;True;1;Main(z3,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;4925.273,-1915.775;Inherit;False;Property;_Float24;主贴图列表;12;0;Create;False;0;0;0;True;1;Main(z1,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;4615.128,-1829.483;Inherit;False;Property;_Float22;剔除模式;1;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;4612.008,-1635.218;Inherit;False;Property;_Float18;显示最前面;5;1;[Enum];Create;False;0;2;open;0;close;4;0;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;952;4926.881,-1522.875;Inherit;False;Property;_Float40;叠加列表;80;0;Create;False;0;0;0;True;1;Main(z5,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;700;5634.345,-681.3838;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;1119;5693.909,-442.955;Inherit;False;1118;Vertex;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1121;4921.767,-1328.809;Inherit;False;Property;_Float76;菲尼尔列表;87;0;Create;False;0;0;0;True;1;Main(z7,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1120;4921.325,-1218.763;Inherit;False;Property;_Float75;顶点偏移列表;93;0;Create;False;0;0;0;True;1;Main(z8,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;985;5220.3,-412.2522;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;550;4602.379,-1519.42;Inherit;False;Property;_Float85;深度写入（模型特效适用）;4;1;[Toggle];Create;False;0;2;open;0;close;4;0;True;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;4928.787,-1817.067;Inherit;False;Property;_Float25;遮罩列表;30;0;Create;False;0;0;0;True;1;Main(z2,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;794;4927.413,-1433.16;Inherit;False;Property;_Float31;高光列表;26;0;Create;False;0;0;0;True;1;Main(z6,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1145;4927.213,-1122.5;Inherit;False;Property;_Float84;Custom列表;6;0;Create;False;0;0;0;True;1;Main(z9,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;4929.143,-1624.837;Inherit;False;Property;_Float27;溶解列表;54;0;Create;False;0;0;0;True;1;Main(z4,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;993;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;992;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;996;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;998;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormalsOnly;False;True;15;d3d9;d3d11_9x;d3d11;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;995;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;997;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormalsOnly;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;994;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;10;True;120;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;990;5977.434,-619.432;Float;False;True;-1;2;LWGUI.LWGUI;0;3;Test/URP极简02;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;121;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;True;True;2;5;False;-1;10;True;120;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;True;550;True;3;True;130;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForwardOnly;False;False;0;;0;0;Standard;22;Surface;1;638618962662775602;  Blend;0;0;Two Sided;1;0;Cast Shadows;0;638618962654221104;  Use Shadow Threshold;0;0;Receive Shadows;0;638618962643455879;GPU Instancing;0;638618962648618279;LOD CrossFade;0;0;Built-in Fog;0;0;DOTS Instancing;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;False;False;False;False;False;False;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;989;5977.434,-619.432;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;991;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;691;0;687;1
WireConnection;691;1;687;2
WireConnection;692;1;691;0
WireConnection;692;2;694;0
WireConnection;882;0;878;0
WireConnection;646;0;643;0
WireConnection;688;0;644;0
WireConnection;688;1;692;0
WireConnection;1155;0;1154;1
WireConnection;1155;1;1154;2
WireConnection;649;0;647;0
WireConnection;649;1;645;0
WireConnection;884;0;881;0
WireConnection;884;1;880;0
WireConnection;883;0;879;0
WireConnection;883;2;882;0
WireConnection;648;0;688;0
WireConnection;648;2;646;0
WireConnection;650;0;648;0
WireConnection;650;1;649;0
WireConnection;1157;1;1155;0
WireConnection;1157;2;1156;0
WireConnection;885;0;883;0
WireConnection;885;1;884;0
WireConnection;698;0;650;0
WireConnection;887;0;885;0
WireConnection;699;0;650;0
WireConnection;886;0;885;0
WireConnection;1158;0;939;0
WireConnection;1158;1;1157;0
WireConnection;770;0;771;0
WireConnection;1037;0;1039;0
WireConnection;1042;0;1044;0
WireConnection;889;0;887;0
WireConnection;891;0;886;0
WireConnection;942;0;940;0
WireConnection;942;1;941;0
WireConnection;1036;0;836;0
WireConnection;1036;2;1037;0
WireConnection;768;0;766;0
WireConnection;768;1;767;0
WireConnection;1041;0;1158;0
WireConnection;1041;2;1042;0
WireConnection;838;0;840;0
WireConnection;838;1;839;0
WireConnection;655;0;699;0
WireConnection;769;0;764;0
WireConnection;769;2;770;0
WireConnection;754;0;753;0
WireConnection;754;1;755;0
WireConnection;654;0;698;0
WireConnection;922;0;919;0
WireConnection;922;1;920;0
WireConnection;658;0;699;0
WireConnection;658;1;655;0
WireConnection;658;2;659;0
WireConnection;938;0;1041;0
WireConnection;938;1;942;0
WireConnection;892;0;887;0
WireConnection;892;1;889;0
WireConnection;892;2;888;0
WireConnection;837;0;1036;0
WireConnection;837;1;838;0
WireConnection;765;0;769;0
WireConnection;765;1;768;0
WireConnection;756;0;752;0
WireConnection;756;1;754;0
WireConnection;656;0;698;0
WireConnection;656;1;654;0
WireConnection;656;2;657;0
WireConnection;893;0;886;0
WireConnection;893;1;891;0
WireConnection;893;2;890;0
WireConnection;923;0;921;0
WireConnection;865;0;765;0
WireConnection;894;0;892;0
WireConnection;894;1;893;0
WireConnection;795;1;837;0
WireConnection;931;1;938;0
WireConnection;866;0;765;0
WireConnection;642;0;656;0
WireConnection;642;1;658;0
WireConnection;924;1;922;0
WireConnection;745;1;756;0
WireConnection;925;0;924;0
WireConnection;925;2;923;0
WireConnection;870;0;865;0
WireConnection;935;0;931;1
WireConnection;935;1;936;0
WireConnection;706;0;642;0
WireConnection;1138;0;1136;0
WireConnection;1138;1;1137;3
WireConnection;1138;2;1135;0
WireConnection;895;1;894;0
WireConnection;707;0;642;0
WireConnection;1025;0;795;1
WireConnection;1025;1;795;2
WireConnection;1025;2;1026;0
WireConnection;871;0;866;0
WireConnection;746;0;745;0
WireConnection;746;1;745;4
WireConnection;868;0;866;0
WireConnection;868;1;871;0
WireConnection;868;2;872;0
WireConnection;867;0;865;0
WireConnection;867;1;870;0
WireConnection;867;2;869;0
WireConnection;1124;0;1025;0
WireConnection;1124;1;1125;0
WireConnection;748;1;746;0
WireConnection;748;2;749;0
WireConnection;932;0;934;0
WireConnection;932;1;935;0
WireConnection;932;2;937;0
WireConnection;927;0;925;0
WireConnection;710;0;706;0
WireConnection;710;1;707;0
WireConnection;1070;0;1067;0
WireConnection;1139;0;895;0
WireConnection;1139;1;1138;0
WireConnection;967;0;968;0
WireConnection;834;0;800;0
WireConnection;834;1;829;3
WireConnection;834;2;835;0
WireConnection;1078;0;1068;0
WireConnection;1078;2;1070;0
WireConnection;750;0;748;0
WireConnection;750;1;751;0
WireConnection;929;0;928;0
WireConnection;929;1;927;0
WireConnection;929;2;926;0
WireConnection;1074;0;1071;0
WireConnection;1074;1;1072;0
WireConnection;711;0;642;0
WireConnection;711;1;710;0
WireConnection;711;2;712;0
WireConnection;944;0;1124;0
WireConnection;944;1;932;0
WireConnection;944;2;945;0
WireConnection;1079;0;1069;0
WireConnection;1079;1;1073;0
WireConnection;1140;0;1139;0
WireConnection;873;0;867;0
WireConnection;873;1;868;0
WireConnection;1141;0;895;0
WireConnection;1141;1;1140;0
WireConnection;758;1;873;0
WireConnection;986;0;961;0
WireConnection;986;2;967;0
WireConnection;930;0;944;0
WireConnection;930;1;929;0
WireConnection;1080;0;1078;0
WireConnection;1080;1;1074;0
WireConnection;1081;0;1079;0
WireConnection;1081;4;1077;0
WireConnection;1081;2;1076;0
WireConnection;1081;3;1075;0
WireConnection;804;0;834;0
WireConnection;804;1;805;0
WireConnection;966;0;964;0
WireConnection;966;1;965;0
WireConnection;816;0;817;0
WireConnection;816;1;834;0
WireConnection;747;0;711;0
WireConnection;747;1;750;0
WireConnection;900;0;895;0
WireConnection;900;1;898;0
WireConnection;1128;0;801;0
WireConnection;1128;1;1127;0
WireConnection;1128;2;1126;0
WireConnection;812;0;834;0
WireConnection;812;1;930;0
WireConnection;1129;0;1128;0
WireConnection;806;0;930;0
WireConnection;806;1;804;0
WireConnection;806;2;807;0
WireConnection;845;0;834;0
WireConnection;845;1;817;0
WireConnection;845;2;850;0
WireConnection;1082;0;1081;0
WireConnection;962;0;986;0
WireConnection;962;1;966;0
WireConnection;899;0;900;0
WireConnection;899;1;896;0
WireConnection;1083;0;1080;0
WireConnection;1143;0;1141;0
WireConnection;815;0;816;0
WireConnection;815;1;930;0
WireConnection;1149;1;1148;4
WireConnection;1149;2;1147;0
WireConnection;1085;0;1080;0
WireConnection;874;0;758;0
WireConnection;874;1;875;0
WireConnection;619;1;747;0
WireConnection;1088;0;1085;0
WireConnection;1092;0;1083;0
WireConnection;1028;1;619;0
WireConnection;1028;0;619;1
WireConnection;1028;2;619;2
WireConnection;1028;3;619;3
WireConnection;1144;0;1142;0
WireConnection;1144;1;1143;0
WireConnection;1144;2;1135;0
WireConnection;906;0;895;4
WireConnection;906;1;895;0
WireConnection;906;2;904;0
WireConnection;1150;0;672;0
WireConnection;1150;1;1149;0
WireConnection;773;0;874;0
WireConnection;773;1;774;0
WireConnection;1086;0;1082;0
WireConnection;798;0;806;0
WireConnection;798;1;1128;0
WireConnection;798;2;1129;0
WireConnection;814;0;812;0
WireConnection;814;1;815;0
WireConnection;847;0;845;0
WireConnection;847;1;930;0
WireConnection;951;1;962;0
WireConnection;1091;0;1084;0
WireConnection;1012;0;916;0
WireConnection;1012;1;899;0
WireConnection;1012;2;1013;0
WireConnection;1132;0;898;0
WireConnection;1132;1;906;0
WireConnection;762;0;758;4
WireConnection;762;1;758;0
WireConnection;762;2;763;0
WireConnection;908;0;773;0
WireConnection;908;1;1012;0
WireConnection;908;2;1144;0
WireConnection;1094;0;1087;0
WireConnection;1094;2;1091;0
WireConnection;956;0;951;4
WireConnection;956;1;951;0
WireConnection;956;2;969;0
WireConnection;1053;0;1028;0
WireConnection;1053;1;1057;0
WireConnection;1029;0;1150;0
WireConnection;1029;1;1030;0
WireConnection;1101;0;1082;0
WireConnection;1101;1;1086;0
WireConnection;1101;2;1093;0
WireConnection;1016;0;798;0
WireConnection;1016;1;812;0
WireConnection;1016;2;1017;0
WireConnection;1102;0;1085;0
WireConnection;1102;1;1088;0
WireConnection;1102;2;1089;0
WireConnection;848;0;815;0
WireConnection;848;1;847;0
WireConnection;1021;0;1020;0
WireConnection;1021;1;814;0
WireConnection;1021;2;1017;0
WireConnection;1100;0;1083;0
WireConnection;1100;1;1092;0
WireConnection;1100;2;1090;0
WireConnection;1108;1;1096;0
WireConnection;1108;2;1098;0
WireConnection;842;0;1021;0
WireConnection;1023;0;1024;0
WireConnection;1023;1;848;0
WireConnection;1023;2;1017;0
WireConnection;680;0;1053;0
WireConnection;680;1;681;0
WireConnection;1110;0;1100;0
WireConnection;1110;1;1102;0
WireConnection;1103;0;1094;0
WireConnection;1103;2;1097;0
WireConnection;1131;0;875;0
WireConnection;1131;1;762;0
WireConnection;1104;0;1095;0
WireConnection;1104;1;1101;0
WireConnection;1104;2;1099;0
WireConnection;1015;0;803;0
WireConnection;1015;1;1016;0
WireConnection;1015;2;1018;0
WireConnection;1048;0;1132;0
WireConnection;1048;1;896;0
WireConnection;1032;0;1150;0
WireConnection;1032;1;1029;0
WireConnection;1032;2;1033;0
WireConnection;1011;0;792;0
WireConnection;1011;1;908;0
WireConnection;1011;2;1010;0
WireConnection;1031;0;672;4
WireConnection;1031;1;1030;4
WireConnection;955;0;956;0
WireConnection;955;1;960;0
WireConnection;959;0;955;0
WireConnection;1114;1;1110;0
WireConnection;1046;0;1131;0
WireConnection;1046;1;774;0
WireConnection;1111;1;1103;0
WireConnection;1014;0;917;0
WireConnection;1014;1;1048;0
WireConnection;1014;2;1013;0
WireConnection;623;0;619;4
WireConnection;623;1;619;0
WireConnection;623;2;624;0
WireConnection;1112;0;1105;0
WireConnection;1112;1;1106;4
WireConnection;1112;2;1109;0
WireConnection;671;0;680;0
WireConnection;671;1;1011;0
WireConnection;671;2;1015;0
WireConnection;671;3;1032;0
WireConnection;671;4;781;0
WireConnection;671;5;1031;0
WireConnection;671;6;1104;0
WireConnection;954;0;951;0
WireConnection;954;1;960;0
WireConnection;851;0;1023;0
WireConnection;844;0;823;0
WireConnection;844;1;822;0
WireConnection;1113;0;1107;0
WireConnection;1113;1;1108;0
WireConnection;785;0;671;0
WireConnection;785;1;786;0
WireConnection;843;0;671;0
WireConnection;843;1;844;0
WireConnection;843;2;820;0
WireConnection;856;0;854;0
WireConnection;856;1;855;0
WireConnection;914;0;1046;0
WireConnection;914;1;1014;0
WireConnection;683;0;623;0
WireConnection;683;1;681;0
WireConnection;1115;0;1111;1
WireConnection;1115;1;1112;0
WireConnection;1115;2;1113;0
WireConnection;1115;3;1114;1
WireConnection;1115;4;1111;4
WireConnection;1115;5;1114;4
WireConnection;958;0;954;0
WireConnection;1008;1;959;0
WireConnection;1008;2;1006;0
WireConnection;1007;0;977;0
WireConnection;1007;1;959;0
WireConnection;1007;2;1006;0
WireConnection;787;0;785;0
WireConnection;787;1;788;0
WireConnection;787;2;789;0
WireConnection;787;3;789;4
WireConnection;1009;0;793;0
WireConnection;1009;1;914;0
WireConnection;1009;2;1010;0
WireConnection;1034;0;672;4
WireConnection;1034;1;1031;0
WireConnection;1034;2;1033;0
WireConnection;852;0;843;0
WireConnection;852;1;856;0
WireConnection;852;2;853;0
WireConnection;982;0;1007;0
WireConnection;982;1;683;0
WireConnection;983;0;1008;0
WireConnection;983;1;683;0
WireConnection;701;0;702;0
WireConnection;1005;1;958;0
WireConnection;1005;2;1006;0
WireConnection;1117;1;1115;0
WireConnection;1117;2;1116;0
WireConnection;784;0;852;0
WireConnection;784;1;787;0
WireConnection;784;2;1005;0
WireConnection;703;0;701;0
WireConnection;980;0;982;0
WireConnection;980;1;983;0
WireConnection;980;2;981;0
WireConnection;669;0;683;0
WireConnection;669;1;670;0
WireConnection;669;2;1009;0
WireConnection;669;3;1015;0
WireConnection;669;4;1034;0
WireConnection;669;5;781;4
WireConnection;669;6;1104;0
WireConnection;1118;0;1117;0
WireConnection;700;0;784;0
WireConnection;700;1;703;0
WireConnection;985;0;980;0
WireConnection;985;1;669;0
WireConnection;985;2;703;0
WireConnection;990;2;700;0
WireConnection;990;3;985;0
WireConnection;990;5;1119;0
ASEEND*/
//CHKSM=17F9FC75DA21FE1CD3BD462270CA4B79DD2CEBAB