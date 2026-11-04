// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Test/URP极简01"
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
		[Main(z7,_KEYWORD,off,off)]_Float75("Custom列表", Float) = 0
		[SubToggle(z1,_)]_Float4("Custom1XY控制主贴图uv", Float) = 0
		[SubToggle(z7,_)]_Float38("Custom1Z控制溶解程度", Float) = 0
		[SubToggle(z7,_)]_Float69("Custom1W增加贴图亮度", Float) = 1
		[SubToggle(z7,_)]_Float76("Custom2XY控制附加溶解贴图uv", Float) = 0
		[SubToggle(z7,_)]_Float74("Custom2Z控制centerGlow", Float) = 0
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
		[Sub(z4)]_Float71("溶解贴图纹理加强", Float) = 0
		[KWEnum(z4,R,_0,G,_1)]_Float62("溶解通道", Float) = 0
		[Sub(z4)]_Float66("溶解贴图旋转", Range( 0 , 360)) = 0
		[Sub(z4)]_Float28("溶解程度", Range( -0.2 , 1.2)) = -0.2
		[Sub(z4)]_Float29("软溶解", Range( -0.5 , 0.5)) = -0.25
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
		[ASEEnd][Sub(z5)]_Vector7("叠加uv", Vector) = (0,0,0,0)

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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color4;
			float4 _Dissolve_ST;
			float4 _Mask1_ST;
			float4 _Mask_ST;
			float4 _Color0;
			float4 _Color2;
			float4 _Dissolve1_ST;
			float4 _Color3;
			float4 _Main_ST;
			float4 _Noise_ST;
			float4 _TextureSample0_ST;
			float4 _Color1;
			float2 _Vector3;
			float2 _Vector6;
			float2 _Vector2;
			float2 _Vector4;
			float2 _aegrdfgaergare;
			float2 _Vector7;
			float2 _Vector5;
			float _Float541111111111111111111;
			float _Float55;
			float _Float67;
			float _Float76;
			float _Float30;
			float _Float71;
			float _Float62;
			float _Float66;
			float _Float93;
			float _Float94;
			float _Float60;
			float _Float97;
			float _Float50;
			float _Float14;
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
			float _Float95;
			float _Float38;
			float _Float40;
			float _Float37;
			float _Float8;
			float _Float2;
			float _Float1;
			float _Float20;
			float _Float4;
			float _Float31;
			float _Float85;
			float _Float27;
			float _Float75;
			float _Float22;
			float _Float24;
			float _Blendmode;
			float _Float26;
			float _Float25;
			float _Float18;
			float _Float12;
			float _Float13;
			float _Float7;
			float _Float3;
			float _Float65;
			float _Float29;
			float _Float69;
			float _Float10;
			float _Float74;
			float _Float11;
			float _Float47;
			float _Float28;
			float _Float48;
			float _Float45;
			float _Float44;
			float _Float16;
			float _Float43;
			float _Float42;
			float _Float41;
			float _Float15;
			float _Float46;
			float _Float61;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
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

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord6 = screenPos;
				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				o.ase_texcoord4 = v.ase_texcoord1;
				o.ase_texcoord5 = v.ase_texcoord2;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
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
				float4 texCoord1080 = IN.ase_texcoord5;
				texCoord1080.xy = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1081 = lerp( 1.0 , texCoord1080.z , _Float74);
				float4 temp_cast_11 = (lerpResult1081).xxxx;
				float4 clampResult1083 = clamp( ( tex2DNode895 - temp_cast_11 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
				float4 clampResult1085 = clamp( ( tex2DNode895 * clampResult1083 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
				float4 lerpResult1087 = lerp( temp_cast_10 , clampResult1085 , _Float74);
				float4 lerpResult1011 = lerp( temp_cast_6 , ( pow( saferPower773 , temp_cast_7 ) * lerpResult1012 * lerpResult1087 ) , _Float10);
				float4 texCoord999 = IN.ase_texcoord4;
				texCoord999.xy = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult1062 = lerp( 0.0 , texCoord999.w , _Float69);
				float4 temp_output_1066_0 = ( _Color0 + lerpResult1062 );
				float4 switchResult1029 = (((ase_vface>0)?(temp_output_1066_0):(_Color4)));
				float4 lerpResult1032 = lerp( temp_output_1066_0 , switchResult1029 , _Float65);
				float switchResult1031 = (((ase_vface>0)?(_Color0.a):(_Color4.a)));
				float4 temp_output_671_0 = ( pow( saferPower680 , temp_cast_5 ) * lerpResult1011 * lerpResult1032 * IN.ase_color * switchResult1031 * temp_output_1066_0 );
				float4 texCoord829 = IN.ase_texcoord4;
				texCoord829.xy = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult834 = lerp( _Float28 , texCoord829.z , _Float38);
				float2 uv_Dissolve = IN.ase_texcoord3.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float cos1036 = cos( radians( _Float66 ) );
				float sin1036 = sin( radians( _Float66 ) );
				float2 rotator1036 = mul( uv_Dissolve - float2( 0.5,0.5 ) , float2x2( cos1036 , -sin1036 , sin1036 , cos1036 )) + float2( 0.5,0.5 );
				float4 tex2DNode795 = tex2D( _Dissolve, ( rotator1036 + ( _Vector5 * _TimeParameters.x ) ) );
				float lerpResult1025 = lerp( tex2DNode795.r , tex2DNode795.g , _Float62);
				float2 uv_Dissolve1 = IN.ase_texcoord3.xy * _Dissolve1_ST.xy + _Dissolve1_ST.zw;
				float2 texCoord1112 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult1113 = (float2(texCoord1112.x , texCoord1112.y));
				float2 lerpResult1115 = lerp( float2( 0,0 ) , appendResult1113 , _Float76);
				float cos1041 = cos( radians( _Float67 ) );
				float sin1041 = sin( radians( _Float67 ) );
				float2 rotator1041 = mul( ( uv_Dissolve1 + lerpResult1115 ) - float2( 0.5,0.5 ) , float2x2( cos1041 , -sin1041 , sin1041 , cos1041 )) + float2( 0.5,0.5 );
				float lerpResult932 = lerp( 1.0 , ( tex2D( _Dissolve1, ( rotator1041 + ( _aegrdfgaergare * _TimeParameters.x ) ) ).r * _Float541111111111111111111 ) , _Float55);
				float lerpResult944 = lerp( ( lerpResult1025 + _Float71 ) , lerpResult932 , _Float30);
				float2 appendResult922 = (float2(_Float93 , _Float94));
				float2 texCoord924 = IN.ase_texcoord3.xy * float2( 1,1 ) + appendResult922;
				float cos925 = cos( radians( _Float95 ) );
				float sin925 = sin( radians( _Float95 ) );
				float2 rotator925 = mul( texCoord924 - float2( 0.5,0.5 ) , float2x2( cos925 , -sin925 , sin925 , cos925 )) + float2( 0.5,0.5 );
				float lerpResult929 = lerp( 1.0 , (rotator925).x , _Float97);
				float temp_output_930_0 = ( lerpResult944 * lerpResult929 );
				float temp_output_812_0 = step( lerpResult834 , temp_output_930_0 );
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
				float4 screenPos = IN.ase_texcoord6;
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
				float saferPower1076 = abs( _Float47 );
				float4 temp_cast_23 = (tex2DNode895.a).xxxx;
				float4 lerpResult906 = lerp( temp_cast_23 , tex2DNode895 , _Float50);
				float lerpResult1014 = lerp( 1.0 , pow( saferPower1076 , ( _Float48 * lerpResult906 ).r ) , _Float11);
				float4 lerpResult1009 = lerp( temp_cast_20 , ( pow( saferPower1046 , temp_cast_22 ) * lerpResult1014 ) , _Float10);
				float clampResult1073 = clamp( _Float29 , -0.5 , 0.5 );
				float smoothstepResult798 = smoothstep( clampResult1073 , ( 1.0 - clampResult1073 ) , ( temp_output_930_0 + ( lerpResult834 * -2.0 ) + 1.0 ));
				float lerpResult1016 = lerp( smoothstepResult798 , temp_output_812_0 , _Float60);
				float lerpResult1015 = lerp( 1.0 , lerpResult1016 , _Float61);
				float lerpResult1034 = lerp( _Color0.a , switchResult1031 , _Float65);
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = ( ( lerpResult852 + ( pow( saferPower785 , temp_cast_12 ) * _Float19 * _Color1 * _Color1.a ) + lerpResult1005 ) * temp_output_703_0 ).rgb;
				float Alpha = ( lerpResult980 * ( temp_output_683_0 * _Float6 * lerpResult1009 * lerpResult1015 * lerpResult1034 * IN.ase_color.a ) * temp_output_703_0 ).r;
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
-1805;89;1920;891;6527.303;-4168.731;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;705;-4290.684,-852.1572;Inherit;False;834.0901;349;;4;687;694;691;692;主贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;687;-4240.684,-782.7599;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;715;-3156.972,-970.6801;Inherit;False;1015.928;565.5943;;9;643;644;688;646;647;645;649;648;650;主贴图uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;915;-4064.021,1598.443;Inherit;False;3444.734;645.6752;;31;900;894;893;899;892;878;882;879;881;890;888;896;898;891;883;887;889;880;884;885;886;904;895;906;916;917;1012;1014;1013;1070;1076;附加遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;691;-3916.776,-770.1977;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;694;-3992.594,-619.1572;Inherit;False;Property;_Float4;Custom1XY控制主贴图uv;7;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;692;-3640.595,-802.1572;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;644;-3085.908,-920.6802;Inherit;False;0;619;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;1116;-5973.939,4428.97;Inherit;False;779.3872;338.4731;;4;1113;1115;1112;1114;附加溶解贴图custom;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;643;-3106.972,-738.2395;Inherit;False;Property;_Float20;主贴图旋转;23;0;Create;False;0;0;0;True;1;Sub(z1);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;878;-4014.021,1813.443;Inherit;False;Property;_Float44;m旋转;47;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1112;-5923.939,4478.97;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;879;-3803.021,1648.443;Inherit;False;0;895;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;645;-2769.739,-516.0858;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;688;-2755.999,-864.1628;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;881;-3653.027,1923.442;Inherit;False;Property;_Vector6;muv;48;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RadiansOpNode;646;-2808.408,-733.4997;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;880;-3665.027,2061.442;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;882;-3664.027,1816.443;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;647;-2766.073,-647.3347;Inherit;False;Property;_Vector4;主贴图UV;24;0;Create;False;0;0;0;True;1;Sub(z1);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;1045;-5075.341,4358.782;Inherit;False;1984.322;547.9778;;14;940;941;942;938;936;931;935;934;937;945;1041;939;1042;1044;附加溶解贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1117;-3456.315,640.9621;Inherit;False;4141.369;777.4522;;33;771;764;766;767;770;768;769;765;865;866;869;871;870;872;868;867;873;875;758;874;774;763;773;1010;908;792;762;1075;1011;1046;793;914;1009;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;884;-3397.026,1913.442;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;883;-3434.026,1745.443;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;649;-2510.193,-674.0046;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1040;-4589.611,3235.572;Inherit;False;990.9915;552.217;;8;837;838;840;839;1036;1037;836;1039;溶解uv;1,1,1,1;0;0
Node;AmplifyShaderEditor.RotatorNode;648;-2563.166,-834.1674;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;1113;-5598.663,4496.682;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1114;-5734.425,4651.443;Inherit;False;Property;_Float76;Custom2XY控制附加溶解贴图uv;10;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;718;-1811.048,-967.7505;Inherit;False;1031.703;551.2322;;9;699;698;657;659;654;655;658;656;642;主贴图不循环;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;885;-3200.023,1843.443;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1115;-5376.552,4502.351;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;939;-4991.28,4396.782;Inherit;False;0;931;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;771;-3406.315,855.9623;Inherit;False;Property;_Float15;遮罩图旋转;38;0;Create;False;0;0;0;True;1;Sub(z2);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;650;-2294.046,-746.7887;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;757;-1703.928,-143.5765;Inherit;False;1815.883;444;;11;753;755;754;752;756;746;749;751;748;750;745;noise扰动;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1044;-5065.341,4660.525;Inherit;False;Property;_Float67;附加溶解贴图旋转;73;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1039;-4629.611,3431.062;Inherit;False;Property;_Float66;溶解贴图旋转;60;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;836;-4420.212,3285.572;Inherit;False;0;795;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;940;-4653.28,4623.781;Inherit;False;Property;_aegrdfgaergare;附加溶解贴图uv;75;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ComponentMaskNode;886;-3027.472,1946.117;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;764;-3195.316,690.9621;Inherit;False;0;758;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;753;-1640.928,69.42346;Inherit;False;Property;_Vector2;扰动uv;53;0;Create;False;0;0;0;True;1;Sub(z3);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;1100;-4678.485,4473.873;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;941;-4671.28,4763.409;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;840;-4347.619,3525.288;Inherit;False;Property;_Vector5;溶解贴图uv;69;0;Create;False;0;0;0;True;1;Sub(z4);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ComponentMaskNode;699;-1758.737,-696.6046;Inherit;False;False;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;839;-4363.816,3676.789;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1037;-4308.837,3440.197;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;767;-3057.316,1103.962;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;698;-1761.048,-897.8173;Inherit;False;True;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;766;-3045.316,965.9623;Inherit;False;Property;_Vector3;遮罩uv;39;0;Create;False;0;0;0;True;1;Sub(z2);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ComponentMaskNode;887;-3023.472,1761.118;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;770;-3056.316,858.9623;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1042;-4786.17,4630.058;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;918;-4735.542,4995.511;Inherit;False;1591.615;427.6962;;11;925;924;923;922;921;920;919;927;928;929;926;方向溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;755;-1653.928,189.4235;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;919;-4687.542,5082.959;Inherit;False;Property;_Float93;方向U偏移;78;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;891;-2816.47,2031.117;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;889;-2765.47,1775.118;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;888;-2833.47,1863.118;Inherit;False;Property;_Float45;mxclamp;43;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;920;-4690.542,5161.771;Inherit;False;Property;_Float94;方向V偏移;79;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;942;-4417.28,4710.781;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;654;-1462.417,-858.9122;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;828;-3335.642,3286.492;Inherit;False;3867.203;1427.534;;16;810;809;800;803;829;834;835;930;944;1015;1016;1018;1026;932;1067;1068;溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.RotatorNode;1036;-4063.597,3339.53;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;657;-1477.503,-782.5176;Inherit;False;Property;_Float1;主贴图xclamp;19;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;768;-2789.319,955.9623;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;890;-2826.47,2128.118;Inherit;False;Property;_Float46;myclamp;44;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;752;-1644.928,-93.57654;Inherit;False;0;745;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;659;-1471.503,-532.5184;Inherit;False;Property;_Float2;主贴图yclamp;20;0;Create;False;0;0;0;True;1;KWEnum(z1,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;754;-1397.928,113.4235;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;838;-4080.621,3556.288;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;655;-1464.417,-619.9125;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;769;-2826.318,787.9623;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;1041;-4523.17,4498.058;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;756;-1202.928,1.423462;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;810;-3285.642,3424.688;Inherit;False;785.1409;280;;2;795;1025;溶解贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;837;-3750.621,3466.287;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;1077;-2450.129,2459.592;Inherit;False;1892.448;560.886;;10;1087;1086;1085;1084;1083;1082;1081;1080;1079;1078;CenterGlow;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;893;-2509.47,1996.117;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;938;-4241.282,4606.781;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;658;-1188.503,-700.5177;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;921;-4644.666,5297.209;Inherit;False;Property;_Float95;方向溶解旋转（默认左黑右白）;77;0;Create;False;0;0;0;False;1;Sub(z4);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;656;-1216.722,-917.7505;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;892;-2509.47,1696.118;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;765;-2592.319,885.9623;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;922;-4510.667,5105.21;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1078;-2419.23,2907.607;Inherit;False;Property;_Float74;Custom2Z控制centerGlow;11;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1080;-2425.206,2665.989;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1079;-2181.396,2681.496;Inherit;False;Constant;_Float77;Float 77;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;713;-529.8556,-825.4449;Inherit;False;744.8823;393.0596;;5;706;707;712;710;711;主贴图极坐标2;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;1026;-3195.457,3700.082;Inherit;False;Property;_Float62;溶解通道;59;0;Create;False;0;0;0;True;1;KWEnum(z4,R,_0,G,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;931;-4044.219,4561.428;Inherit;True;Property;_Dissolve1;附加溶解贴图;71;0;Create;False;0;0;0;False;1;Sub(z4);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;795;-3316.842,3476.788;Inherit;True;Property;_Dissolve;溶解贴图;57;0;Create;False;0;0;0;True;1;Sub(z4);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;745;-990.6514,-36.1528;Inherit;True;Property;_Noise;扰动贴图;51;0;Create;False;0;0;0;True;1;Sub(z3);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;894;-2340.471,1812.118;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;642;-944.3453,-777.5762;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;866;-2419.765,988.6371;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;865;-2415.765,803.6371;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;923;-4249.669,5296.209;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;936;-3955.28,4786.781;Inherit;False;Property;_Float541111111111111111111;附加溶解贴图强度;74;0;Create;False;0;0;0;True;1;Sub(z4);False;1;11;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;924;-4333.666,5056.209;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1081;-1942.865,2824.005;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1068;-3229.73,4245.943;Inherit;False;Property;_Float71;溶解贴图纹理加强;58;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;871;-2208.765,1073.638;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;937;-3684.28,4781.781;Inherit;False;Property;_Float55;附加溶解贴图开关;70;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;749;-583.2661,176.1942;Inherit;False;Property;_Float12;扰动开关;50;0;Create;False;0;0;0;True;1;SubToggle(z3,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;934;-3516.28,4469.782;Inherit;False;Constant;_Float53;Float 53;69;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;707;-479.8557,-620.0425;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;869;-2225.765,905.6371;Inherit;False;Property;_Float41;遮罩xclamp;34;0;Create;False;0;0;0;False;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;925;-4062.673,5169.21;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;746;-583.3665,-11.32115;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;870;-2157.765,817.6371;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;895;-2176.273,1750.854;Inherit;True;Property;_Mask1;m贴图;41;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;988;-534.1824,-1879.859;Inherit;False;4020.037;699.9189;;25;968;961;967;965;964;986;966;962;969;951;956;960;955;977;959;954;982;958;981;983;980;1005;1007;1008;1006;叠加;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;1025;-2896.457,3517.082;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;872;-2218.765,1170.637;Inherit;False;Property;_Float42;遮罩yclamp;35;0;Create;False;0;0;0;True;1;KWEnum(z2,off,_0,on,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;706;-478.8557,-722.0425;Inherit;False;False;True;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;935;-3598.28,4627.781;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;867;-1901.767,738.6371;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;829;-2940.26,3861.507;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;945;-3391.02,4790.76;Inherit;False;Property;_Float30;溶解纹理偏向;72;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;868;-1901.767,1038.638;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1067;-3026.579,4388.096;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1082;-1690.117,2799.184;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;863;-2417.909,3974.823;Inherit;False;2089.603;882.1636;;16;847;848;850;815;816;845;817;851;842;814;812;1020;1021;1023;1024;1017;亮边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;748;-294.6189,-40.55897;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;928;-3777.657,5045.512;Inherit;False;Constant;_Float52;Float 52;32;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;968;-483.1824,-1660.859;Inherit;False;Property;_Float56;叠加旋转;86;0;Create;False;0;0;0;True;1;Sub(z5);False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;712;-245.8609,-548.3855;Inherit;False;Property;_Float8;主贴图极坐标uv翻转;25;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;927;-3851.12,5131.345;Inherit;True;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;751;-210.9782,136.7216;Inherit;False;Property;_Float13;扰动强度;52;0;Create;False;0;0;0;True;1;Sub(z3);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;800;-2922.713,3762.226;Inherit;False;Property;_Float28;溶解程度;61;0;Create;False;0;0;0;True;1;Sub(z4);False;-0.2;0;-0.2;1.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;932;-3275.025,4600.675;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;835;-2734.693,4044.299;Inherit;False;Property;_Float38;Custom1Z控制溶解程度;8;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;926;-3790.606,5333.982;Inherit;False;Property;_Float97;方向溶解开关;76;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;710;-221.8557,-697.0425;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;1083;-1488.276,2797.41;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;965;-158.1829,-1419.859;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;967;-157.1829,-1664.859;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;964;-140.1829,-1550.859;Inherit;False;Property;_Vector7;叠加uv;87;0;Create;False;0;0;0;True;1;Sub(z5);False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;929;-3499.303,5156.099;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1118;1066.392,45.48611;Inherit;False;1580.576;694.2251;;12;1064;999;1066;1030;1029;1033;1032;781;1031;1034;672;1062;颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;817;-2372.908,4270.424;Inherit;False;Property;_Float36;外层亮边宽度;65;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;873;-1732.767,854.6371;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;898;-1834.618,1880.966;Inherit;False;Property;_Float48;m强度;46;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;834;-2399.549,3777.752;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;750;43.53482,-113.7403;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;944;-2919.621,4550.06;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;961;-299.1829,-1829.859;Inherit;False;0;951;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;711;31.02668,-775.4449;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;686;457.023,-987.8872;Inherit;False;1736.342;788.0424;;9;619;624;623;683;680;681;1028;1053;1057;主贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;930;-2642.975,4534;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;758;-1558.572,860.373;Inherit;True;Property;_Mask;遮罩贴图;32;0;Create;False;0;0;0;True;1;Sub(z2);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;900;-1612.418,1790.366;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;875;-1198.324,944.211;Inherit;False;Property;_Float43;遮罩强度;37;0;Create;False;0;0;0;True;1;Sub(z2);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1084;-1229.56,2780.696;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RotatorNode;986;87.32974,-1760.795;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;747;289.1848,-698.8005;Inherit;False;2;2;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;999;1119.827,307.3084;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;816;-2065.997,4280.07;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;850;-2393.774,4644.025;Inherit;False;Property;_Float35;内层亮边2宽度;68;0;Create;False;0;0;0;True;1;Sub(z4);False;0;0.06;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;896;-1474.507,1957.009;Inherit;False;Property;_Float47;mpower;45;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;966;105.8174,-1502.859;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1064;1115.392,513.0705;Inherit;False;Property;_Float69;Custom1W增加贴图亮度;9;0;Create;False;0;0;0;True;1;SubToggle(z7,_);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1086;-1040.728,2521.809;Inherit;False;Constant;_Float78;Float 78;93;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1062;1442.271,368.5869;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;1085;-1063.38,2776.646;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;672;1376.711,101.7052;Inherit;False;Property;_Color0;颜色Front;14;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;809;-2061.809,3343.544;Inherit;False;1733.592;529.1241;;10;805;807;798;801;806;804;1071;1072;1073;1074;软边溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;619;482.8695,-925.7279;Inherit;True;Property;_Main;主贴图;16;0;Create;False;0;0;0;True;1;Sub(z1);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;962;350.8173,-1674.859;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;774;-815.6848,997.5303;Inherit;False;Property;_Float16;遮罩power;36;0;Create;False;0;0;0;True;1;Sub(z2);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;874;-901.6605,885.6776;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;812;-1820.376,4024.823;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;916;-995.1266,1666.034;Inherit;False;Constant;_Float49;Float 49;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;815;-1826.998,4304.216;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1013;-1194.284,1891.037;Inherit;False;Property;_Float11;附加遮罩开关;40;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;845;-2054.845,4605.78;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;904;-2105.004,2118.698;Inherit;False;Property;_Float50;mA通道;42;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;899;-1272.676,1789.384;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1057;1009.169,-815.1925;Inherit;False;Property;_Float7;主贴图强度;22;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;951;594.0963,-1713.612;Inherit;True;Property;_TextureSample0;叠加贴图;82;0;Create;False;0;0;0;True;1;Sub(z5);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1087;-870.0131,2525.648;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1012;-825.3159,1715.396;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1030;1744.969,319.4858;Inherit;False;Property;_Color4;颜色Back;15;1;[HDR];Create;False;0;0;0;True;1;Sub(z1);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;1066;1736.692,106.114;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;805;-2001.764,3447.529;Inherit;False;Constant;_Float33;Float 33;44;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1020;-1095.473,4066.881;Inherit;False;Constant;_Float63;Float 63;44;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1071;-1340.615,3691.132;Inherit;False;Constant;_Float72;Float 72;85;0;Create;True;0;0;0;False;0;False;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1072;-1341.289,3791.014;Inherit;False;Constant;_Float73;Float 73;85;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;773;-436.9232,894.1078;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;763;-1454.773,1193.759;Inherit;False;Property;_Float14;遮罩A通道;33;0;Create;False;0;0;0;True;1;KWEnum(z2,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;906;-1791.17,2044.767;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1028;819.2833,-920.4319;Inherit;False;Property;_Keyword0;主贴图通道;17;0;Create;False;0;0;0;False;0;False;1;0;0;True;;KeywordEnum;4;RGBA;R;G;B;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;969;764.396,-1512.417;Inherit;False;Property;_Float57;叠加贴图通道;83;0;Create;False;0;0;0;True;1;KWEnum(z5,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;847;-1850.171,4602.987;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;814;-1354.998,4125.916;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1017;-1452.244,4395.299;Inherit;False;Property;_Float60;溶解模式;56;0;Create;False;0;0;0;True;1;KWEnum(z4,Soft,_0,Rim,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;801;-1665.961,3657.527;Inherit;False;Property;_Float29;软溶解;62;0;Create;False;0;0;0;True;1;Sub(z4);False;-0.25;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1033;2064.969,271.4858;Inherit;False;Property;_Float65;双面颜色;13;0;Create;False;0;0;0;True;1;SubToggle(z1,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwitchByFaceNode;1029;2064.969,139.899;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;960;1251.602,-1617.229;Inherit;False;Property;_Float54;叠加贴图power;85;0;Create;False;0;0;0;True;1;Sub(z5);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;848;-1351.491,4559.586;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1021;-855.3522,4159.485;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;948;3676.666,-343.5782;Inherit;False;911.7981;680.3812;;10;822;823;820;844;843;855;854;853;856;852;溶解亮边;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1053;1271.682,-919.4614;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;807;-1591.057,3527.138;Inherit;False;Constant;_Float34;Float 34;44;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;792;-246.2426,737.2682;Inherit;False;Constant;_Float21;Float 21;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1024;-1061.893,4517.709;Inherit;False;Constant;_Float64;Float 64;44;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;681;1258.635,-801.2971;Inherit;False;Property;_Float3;主贴图power;21;0;Create;False;0;0;0;True;1;Sub(z1);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;1073;-1082.328,3661.524;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;908;-82.47479,910.185;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1070;-1493.129,2051.764;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;762;-1133.538,1075.473;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;956;1033.292,-1631.061;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;804;-1705.856,3426.053;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1010;-333.2654,1094.915;Inherit;False;Property;_Float10;遮罩开关;31;0;Create;False;0;0;0;True;1;SubToggle(z2,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;781;1762.677,532.7114;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;680;1582.71,-913.4387;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SwitchByFaceNode;1031;2064.969,415.4857;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;791;3580.895,1849.147;Inherit;False;628.9875;471.9476;;5;786;789;785;788;787;高光;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;917;-976.8917,1882.918;Inherit;False;Constant;_Float51;Float 51;64;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1011;364.2919,781.3272;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;822;3726.666,-214.5783;Inherit;False;Property;_Color2;外层亮边颜色;63;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;955;1501.26,-1527.298;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;806;-1283.44,3423.643;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1075;-849.3668,1092.971;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;1076;-1195.961,2026.737;Inherit;False;True;2;0;FLOAT;0;False;1;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;842;-580.5126,4151.015;Inherit;False;rJ;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1023;-845.074,4598.771;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;624;697.4335,-513.0425;Inherit;False;Property;_Float0;主贴图A通道;18;0;Create;False;0;0;0;True;1;KWEnum(z1,RGB,_0,A,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;760;3055.194,-839.9109;Inherit;False;216;233;;1;671;RGB;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;1074;-837.764,3720.391;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1032;2464.969,95.48618;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;823;3770.666,-293.5783;Inherit;False;Property;_Float37;外层亮边颜色强度;64;0;Create;False;0;0;0;True;1;Sub(z4);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;855;3837.098,93.36063;Inherit;False;Property;_Color3;内层亮边2颜色;66;0;Create;False;0;0;0;True;1;Sub(z4);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;671;3105.194,-789.9109;Inherit;False;6;6;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;FLOAT;0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;977;1761.773,-1611.551;Inherit;False;Constant;_Float58;Float 58;78;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;1046;-521.0646,1098.906;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;854;3875.098,-10.63948;Inherit;False;Property;_Float39;内层亮边2颜色强度;67;0;Create;False;0;0;0;True;1;Sub(z4);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;704;3628.58,1453.343;Inherit;False;804.9153;185.5732;;3;702;701;703;软粒子;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;623;1031.917,-545.136;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;959;1759.016,-1518.959;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;820;3964.304,-116.9101;Inherit;False;842;rJ;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1006;1753.321,-1355.996;Inherit;False;Property;_Float9;叠加开关;81;0;Create;False;0;0;0;True;1;SubToggle(z5,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;798;-582.2145,3393.543;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;786;3630.895,1953.047;Inherit;False;Property;_Float17;高光细节;28;0;Create;False;0;0;0;True;1;Sub(z6);False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;851;-560.7872,4620.938;Inherit;False;RJ2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;844;4009.839,-275.8445;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;954;1497.914,-1710.643;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1014;-798.0834,1974.837;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;683;1640.879,-542.5417;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;1119;3179.753,688.5531;Inherit;False;755.478;289.493;;2;670;669;Alpha;1,0,0,1;0;0
Node;AmplifyShaderEditor.LerpOp;1016;-51.89231,3530.509;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;958;1758.016,-1707.059;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1018;-48.89231,3682.509;Inherit;False;Property;_Float61;溶解开关;55;0;Create;False;0;0;0;True;1;SubToggle(z4,_);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;853;4122.047,220.8028;Inherit;False;851;RJ2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;785;3820.996,1899.147;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;793;207.9454,1067.388;Inherit;False;Constant;_Float23;Float 23;39;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;788;3820.882,2023.096;Inherit;False;Property;_Float19;高光强度;29;0;Create;False;0;0;0;True;1;Sub(z6);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;843;4256.473,-272.2322;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;789;3747.882,2109.096;Inherit;False;Property;_Color1;高光颜色;27;0;Create;False;0;0;0;True;1;Sub(z6);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1007;2166.321,-1552.996;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;803;-102.0132,3373.905;Inherit;False;Constant;_Float32;Float 32;44;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;856;4155.898,78.86073;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;702;3678.58,1521.935;Inherit;False;Property;_Float5;软粒子（相机需开启投影）;3;0;Create;False;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1008;2172.321,-1402.996;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;914;148.1378,1283.414;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1009;503.053,1069.336;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;983;2580.114,-1374.632;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;982;2577.714,-1540.432;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;787;4043.882,1933.095;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;701;3945.887,1503.916;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;981;2766.805,-1295.941;Inherit;False;Property;_Float59;A通道算法（Add时通道不同）;84;0;Create;False;0;0;0;True;1;KWEnum(z5,Mul,_0,Add,_1);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1034;2464.969,367.4857;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1005;2161.486,-1718.826;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;852;4406.464,38.45406;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;670;3229.753,738.5531;Inherit;False;Property;_Float6;整体Alpha强度;2;0;Create;False;0;0;0;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1015;216.3055,3454.937;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;532;4877.106,-1963.568;Inherit;False;227.2825;729.8185;;7;140;141;142;143;794;952;1088;GUI列表;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;669;3773.231,747.0461;Inherit;False;6;6;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;703;4265.495,1503.343;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;784;5222.927,-693.6653;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;980;3303.854,-1434.987;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;140;4927.106,-1913.568;Inherit;False;Property;_Float24;主贴图列表;12;0;Create;False;0;0;0;True;1;Main(z1,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;4930.62,-1814.86;Inherit;False;Property;_Float25;遮罩列表;30;0;Create;False;0;0;0;True;1;Main(z2,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1088;4921.767,-1340.441;Inherit;False;Property;_Float75;Custom列表;6;0;Create;False;0;0;0;True;1;Main(z7,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;4929.316,-1717.562;Inherit;False;Property;_Float26;扰动列表;49;0;Create;False;0;0;0;True;1;Main(z3,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;700;5634.345,-681.3838;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;120;4616.961,-1726.482;Inherit;False;Property;_Blendmode;混合模式;0;1;[Enum];Create;False;0;2;add;1;blend;10;0;True;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;985;5220.3,-412.2522;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;794;4929.246,-1430.953;Inherit;False;Property;_Float31;高光列表;26;0;Create;False;0;0;0;True;1;Main(z6,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;4613.841,-1635.218;Inherit;False;Property;_Float18;显示最前面;5;1;[Enum];Create;False;0;2;open;0;close;4;0;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;4616.961,-1829.483;Inherit;False;Property;_Float22;剔除模式;1;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;952;4928.714,-1520.668;Inherit;False;Property;_Float40;叠加列表;80;0;Create;False;0;0;0;True;1;Main(z5,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;4930.976,-1622.63;Inherit;False;Property;_Float27;溶解列表;54;0;Create;False;0;0;0;True;1;Main(z4,_KEYWORD,off,off);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;550;4604.212,-1519.42;Inherit;False;Property;_Float85;深度写入（模型特效适用）;4;1;[Toggle];Create;False;0;2;open;0;close;4;0;True;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;993;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;990;5977.434,-619.432;Float;False;True;-1;2;LWGUI.LWGUI;0;3;Test/URP极简01;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;121;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;True;True;2;5;False;-1;10;True;120;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;True;550;True;3;True;130;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForwardOnly;False;False;0;;0;0;Standard;22;Surface;1;638618962662775602;  Blend;0;0;Two Sided;1;0;Cast Shadows;0;638618962654221104;  Use Shadow Threshold;0;0;Receive Shadows;0;638618962643455879;GPU Instancing;0;638618962648618279;LOD CrossFade;0;0;Built-in Fog;0;0;DOTS Instancing;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;False;False;False;False;False;False;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;998;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormalsOnly;False;True;15;d3d9;d3d11_9x;d3d11;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;989;5977.434,-619.432;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;997;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormalsOnly;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;994;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;10;True;120;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;992;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;995;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;991;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;996;5510.12,-612.5597;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;691;0;687;1
WireConnection;691;1;687;2
WireConnection;692;1;691;0
WireConnection;692;2;694;0
WireConnection;688;0;644;0
WireConnection;688;1;692;0
WireConnection;646;0;643;0
WireConnection;882;0;878;0
WireConnection;884;0;881;0
WireConnection;884;1;880;0
WireConnection;883;0;879;0
WireConnection;883;2;882;0
WireConnection;649;0;647;0
WireConnection;649;1;645;0
WireConnection;648;0;688;0
WireConnection;648;2;646;0
WireConnection;1113;0;1112;1
WireConnection;1113;1;1112;2
WireConnection;885;0;883;0
WireConnection;885;1;884;0
WireConnection;1115;1;1113;0
WireConnection;1115;2;1114;0
WireConnection;650;0;648;0
WireConnection;650;1;649;0
WireConnection;886;0;885;0
WireConnection;1100;0;939;0
WireConnection;1100;1;1115;0
WireConnection;699;0;650;0
WireConnection;1037;0;1039;0
WireConnection;698;0;650;0
WireConnection;887;0;885;0
WireConnection;770;0;771;0
WireConnection;1042;0;1044;0
WireConnection;891;0;886;0
WireConnection;889;0;887;0
WireConnection;942;0;940;0
WireConnection;942;1;941;0
WireConnection;654;0;698;0
WireConnection;1036;0;836;0
WireConnection;1036;2;1037;0
WireConnection;768;0;766;0
WireConnection;768;1;767;0
WireConnection;754;0;753;0
WireConnection;754;1;755;0
WireConnection;838;0;840;0
WireConnection;838;1;839;0
WireConnection;655;0;699;0
WireConnection;769;0;764;0
WireConnection;769;2;770;0
WireConnection;1041;0;1100;0
WireConnection;1041;2;1042;0
WireConnection;756;0;752;0
WireConnection;756;1;754;0
WireConnection;837;0;1036;0
WireConnection;837;1;838;0
WireConnection;893;0;886;0
WireConnection;893;1;891;0
WireConnection;893;2;890;0
WireConnection;938;0;1041;0
WireConnection;938;1;942;0
WireConnection;658;0;699;0
WireConnection;658;1;655;0
WireConnection;658;2;659;0
WireConnection;656;0;698;0
WireConnection;656;1;654;0
WireConnection;656;2;657;0
WireConnection;892;0;887;0
WireConnection;892;1;889;0
WireConnection;892;2;888;0
WireConnection;765;0;769;0
WireConnection;765;1;768;0
WireConnection;922;0;919;0
WireConnection;922;1;920;0
WireConnection;931;1;938;0
WireConnection;795;1;837;0
WireConnection;745;1;756;0
WireConnection;894;0;892;0
WireConnection;894;1;893;0
WireConnection;642;0;656;0
WireConnection;642;1;658;0
WireConnection;866;0;765;0
WireConnection;865;0;765;0
WireConnection;923;0;921;0
WireConnection;924;1;922;0
WireConnection;1081;0;1079;0
WireConnection;1081;1;1080;3
WireConnection;1081;2;1078;0
WireConnection;871;0;866;0
WireConnection;707;0;642;0
WireConnection;925;0;924;0
WireConnection;925;2;923;0
WireConnection;746;0;745;0
WireConnection;746;1;745;4
WireConnection;870;0;865;0
WireConnection;895;1;894;0
WireConnection;1025;0;795;1
WireConnection;1025;1;795;2
WireConnection;1025;2;1026;0
WireConnection;706;0;642;0
WireConnection;935;0;931;1
WireConnection;935;1;936;0
WireConnection;867;0;865;0
WireConnection;867;1;870;0
WireConnection;867;2;869;0
WireConnection;868;0;866;0
WireConnection;868;1;871;0
WireConnection;868;2;872;0
WireConnection;1067;0;1025;0
WireConnection;1067;1;1068;0
WireConnection;1082;0;895;0
WireConnection;1082;1;1081;0
WireConnection;748;1;746;0
WireConnection;748;2;749;0
WireConnection;927;0;925;0
WireConnection;932;0;934;0
WireConnection;932;1;935;0
WireConnection;932;2;937;0
WireConnection;710;0;706;0
WireConnection;710;1;707;0
WireConnection;1083;0;1082;0
WireConnection;967;0;968;0
WireConnection;929;0;928;0
WireConnection;929;1;927;0
WireConnection;929;2;926;0
WireConnection;873;0;867;0
WireConnection;873;1;868;0
WireConnection;834;0;800;0
WireConnection;834;1;829;3
WireConnection;834;2;835;0
WireConnection;750;0;748;0
WireConnection;750;1;751;0
WireConnection;944;0;1067;0
WireConnection;944;1;932;0
WireConnection;944;2;945;0
WireConnection;711;0;642;0
WireConnection;711;1;710;0
WireConnection;711;2;712;0
WireConnection;930;0;944;0
WireConnection;930;1;929;0
WireConnection;758;1;873;0
WireConnection;900;0;895;0
WireConnection;900;1;898;0
WireConnection;1084;0;895;0
WireConnection;1084;1;1083;0
WireConnection;986;0;961;0
WireConnection;986;2;967;0
WireConnection;747;0;711;0
WireConnection;747;1;750;0
WireConnection;816;0;817;0
WireConnection;816;1;834;0
WireConnection;966;0;964;0
WireConnection;966;1;965;0
WireConnection;1062;1;999;4
WireConnection;1062;2;1064;0
WireConnection;1085;0;1084;0
WireConnection;619;1;747;0
WireConnection;962;0;986;0
WireConnection;962;1;966;0
WireConnection;874;0;758;0
WireConnection;874;1;875;0
WireConnection;812;0;834;0
WireConnection;812;1;930;0
WireConnection;815;0;816;0
WireConnection;815;1;930;0
WireConnection;845;0;834;0
WireConnection;845;1;817;0
WireConnection;845;2;850;0
WireConnection;899;0;900;0
WireConnection;899;1;896;0
WireConnection;951;1;962;0
WireConnection;1087;0;1086;0
WireConnection;1087;1;1085;0
WireConnection;1087;2;1078;0
WireConnection;1012;0;916;0
WireConnection;1012;1;899;0
WireConnection;1012;2;1013;0
WireConnection;1066;0;672;0
WireConnection;1066;1;1062;0
WireConnection;773;0;874;0
WireConnection;773;1;774;0
WireConnection;906;0;895;4
WireConnection;906;1;895;0
WireConnection;906;2;904;0
WireConnection;1028;1;619;0
WireConnection;1028;0;619;1
WireConnection;1028;2;619;2
WireConnection;1028;3;619;3
WireConnection;847;0;845;0
WireConnection;847;1;930;0
WireConnection;814;0;812;0
WireConnection;814;1;815;0
WireConnection;1029;0;1066;0
WireConnection;1029;1;1030;0
WireConnection;848;0;815;0
WireConnection;848;1;847;0
WireConnection;1021;0;1020;0
WireConnection;1021;1;814;0
WireConnection;1021;2;1017;0
WireConnection;1053;0;1028;0
WireConnection;1053;1;1057;0
WireConnection;1073;0;801;0
WireConnection;1073;1;1071;0
WireConnection;1073;2;1072;0
WireConnection;908;0;773;0
WireConnection;908;1;1012;0
WireConnection;908;2;1087;0
WireConnection;1070;0;898;0
WireConnection;1070;1;906;0
WireConnection;762;0;758;4
WireConnection;762;1;758;0
WireConnection;762;2;763;0
WireConnection;956;0;951;4
WireConnection;956;1;951;0
WireConnection;956;2;969;0
WireConnection;804;0;834;0
WireConnection;804;1;805;0
WireConnection;680;0;1053;0
WireConnection;680;1;681;0
WireConnection;1031;0;672;4
WireConnection;1031;1;1030;4
WireConnection;1011;0;792;0
WireConnection;1011;1;908;0
WireConnection;1011;2;1010;0
WireConnection;955;0;956;0
WireConnection;955;1;960;0
WireConnection;806;0;930;0
WireConnection;806;1;804;0
WireConnection;806;2;807;0
WireConnection;1075;0;875;0
WireConnection;1075;1;762;0
WireConnection;1076;0;896;0
WireConnection;1076;1;1070;0
WireConnection;842;0;1021;0
WireConnection;1023;0;1024;0
WireConnection;1023;1;848;0
WireConnection;1023;2;1017;0
WireConnection;1074;0;1073;0
WireConnection;1032;0;1066;0
WireConnection;1032;1;1029;0
WireConnection;1032;2;1033;0
WireConnection;671;0;680;0
WireConnection;671;1;1011;0
WireConnection;671;2;1032;0
WireConnection;671;3;781;0
WireConnection;671;4;1031;0
WireConnection;671;5;1066;0
WireConnection;1046;0;1075;0
WireConnection;1046;1;774;0
WireConnection;623;0;619;4
WireConnection;623;1;619;0
WireConnection;623;2;624;0
WireConnection;959;0;955;0
WireConnection;798;0;806;0
WireConnection;798;1;1073;0
WireConnection;798;2;1074;0
WireConnection;851;0;1023;0
WireConnection;844;0;823;0
WireConnection;844;1;822;0
WireConnection;954;0;951;0
WireConnection;954;1;960;0
WireConnection;1014;0;917;0
WireConnection;1014;1;1076;0
WireConnection;1014;2;1013;0
WireConnection;683;0;623;0
WireConnection;683;1;681;0
WireConnection;1016;0;798;0
WireConnection;1016;1;812;0
WireConnection;1016;2;1017;0
WireConnection;958;0;954;0
WireConnection;785;0;671;0
WireConnection;785;1;786;0
WireConnection;843;0;671;0
WireConnection;843;1;844;0
WireConnection;843;2;820;0
WireConnection;1007;0;977;0
WireConnection;1007;1;959;0
WireConnection;1007;2;1006;0
WireConnection;856;0;854;0
WireConnection;856;1;855;0
WireConnection;1008;1;959;0
WireConnection;1008;2;1006;0
WireConnection;914;0;1046;0
WireConnection;914;1;1014;0
WireConnection;1009;0;793;0
WireConnection;1009;1;914;0
WireConnection;1009;2;1010;0
WireConnection;983;0;1008;0
WireConnection;983;1;683;0
WireConnection;982;0;1007;0
WireConnection;982;1;683;0
WireConnection;787;0;785;0
WireConnection;787;1;788;0
WireConnection;787;2;789;0
WireConnection;787;3;789;4
WireConnection;701;0;702;0
WireConnection;1034;0;672;4
WireConnection;1034;1;1031;0
WireConnection;1034;2;1033;0
WireConnection;1005;1;958;0
WireConnection;1005;2;1006;0
WireConnection;852;0;843;0
WireConnection;852;1;856;0
WireConnection;852;2;853;0
WireConnection;1015;0;803;0
WireConnection;1015;1;1016;0
WireConnection;1015;2;1018;0
WireConnection;669;0;683;0
WireConnection;669;1;670;0
WireConnection;669;2;1009;0
WireConnection;669;3;1015;0
WireConnection;669;4;1034;0
WireConnection;669;5;781;4
WireConnection;703;0;701;0
WireConnection;784;0;852;0
WireConnection;784;1;787;0
WireConnection;784;2;1005;0
WireConnection;980;0;982;0
WireConnection;980;1;983;0
WireConnection;980;2;981;0
WireConnection;700;0;784;0
WireConnection;700;1;703;0
WireConnection;985;0;980;0
WireConnection;985;1;669;0
WireConnection;985;2;703;0
WireConnection;990;2;700;0
WireConnection;990;3;985;0
ASEEND*/
//CHKSM=8DC98AD93995CDCA28F63D8CCDDEAD87302F81F0