// Made with Amplify Shader Editor v1.9.9.12
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "URP/多功能材质PRO(Test)"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode( "双面显示", Float ) = 0
		[Enum(Additive,1,AlphaBlend,10)] _dst( "混合模式", Float ) = 0
		_MainAlpha( "总透明度", Range( 0, 1 ) ) = 1
		[HDR] _MT_Color( "主贴图颜色", Color ) = ( 0.8962264, 0.8962264, 0.8962264, 1 )
		[Toggle( _MT_RADIAL_ON )] _MT_Radial( "主贴图极坐标", Float ) = 0
		_MT_Rotato( "主贴图旋转", Range( 0, 1 ) ) = 0
		[Toggle( _MT_USER_ON )] _MT_UseR( "主帖图R通道", Float ) = 0
		_MT( "主贴图", 2D ) = "white" {}
		_MT_Alpha( "主帖图强度", Range( 0, 1 ) ) = 1
		_MT_UV( "主贴图流动", Vector ) = ( 0, 0, 0, 0 )
		[Enum(R,0,G,1,B,2,A,3)] _MT_Channel( "主贴图通道", Int ) = 3
		[Toggle( _USEMT02_ON )] _UseMT02( "开启贴图2", Float ) = 0
		[Toggle( _MT_RADIAL02_ON )] _MT_Radial02( "主帖图02极坐标", Float ) = 0
		_MT_Rotato02( "主帖图02旋转", Range( 0, 1 ) ) = 0
		[HDR] _MT_Color02( "主帖图02颜色", Color ) = ( 0.8962264, 0.8962264, 0.8962264, 1 )
		_MT02( "主帖图02", 2D ) = "white" {}
		_MT_Alpha02( "主帖图02强度", Range( 0, 1 ) ) = 1
		_MT_UV02( "主帖图02UV", Vector ) = ( 0, 0, 0, 0 )
		[Enum(ColorA,0,R,1,G,2,B,3,A,4)] _MT02_Channel( "主帖图02通道", Int ) = 0
		[Toggle( _MASK_RADIAL1_ON )] _Mask_Radial1( "第一层遮罩极坐标", Float ) = 0
		_Mask_Rotato( "第一层遮罩旋转", Range( 0, 1 ) ) = 0
		[Header(Mask)] _Mask( "第一层遮罩", 2D ) = "white" {}
		_MaskUV( "第一层遮罩流动", Vector ) = ( 0, 0, 0, 0 )
		_Mask_Power( "遮罩对比度", Range( 0, 5 ) ) = 1
		_Mask_Value( "遮罩强度", Range( 0, 5 ) ) = 1
		[Enum(R,0,G,1,B,2,A,3)] _Mask_Channel( "遮罩通道", Int ) = 0
		[Header(Mask02)][Toggle( _USE_MASK02_ON )] _Use_Mask02( "开启第二层遮罩", Float ) = 0
		[Toggle( _MASKMUTUPYORADD_ON )] _MaskMutupyoradd( "遮罩相乘模式(更淡)", Float ) = 0
		[Toggle( _MASK02_RADIAL2_ON )] _Mask02_Radial2( "第二层遮罩极坐标", Float ) = 0
		_Mask02_Rotato( "第二层遮罩旋转", Range( 0, 1 ) ) = 0
		_Mask02( "第二层遮罩", 2D ) = "white" {}
		_Mask02UV( "第二层遮罩流动", Vector ) = ( 0, 0, 0, 0 )
		_Mask02_Power( "第二层遮罩对比度", Range( 0, 5 ) ) = 1
		_Mask02_Value( "第二层遮罩强度", Range( 0, 5 ) ) = 1
		[Enum(R,0,G,1,B,2,A,3)] _Mask02_Channel( "遮罩2通道", Int ) = 0
		[Header(Turbulence)][Toggle( _USETURBULENCE_ON )] _UseTurbulence( "使用扰动", Float ) = 0
		[Toggle( _TURBULENCE_RADIAL_ON )] _Turbulence_Radial( "扰动极坐标", Float ) = 0
		_Turbulence_Rotato1( "扰动旋转", Range( 0, 1 ) ) = 0
		_Turbulence( "扰动", 2D ) = "white" {}
		_Turbulence_UV( "扰动流动", Vector ) = ( 0, 0, 0, 0 )
		_Turbulence_Value( "扰动强度", Range( 0, 0.5 ) ) = 0
		[Enum(R,0,G,1,B,2,A,3)] _Turbulence_Channel( "扰动通道", Int ) = 0
		[Header(Turbulence_Mask)][Toggle( _USE_TURBULENCEMASK_ON )] _Use_TurbulenceMask( "开启扰动遮罩", Float ) = 0
		_TurbulenceMask_Rotato( "扰动遮罩旋转", Range( 0, 1 ) ) = 0
		_Turbulence_Mask( "扰动遮罩", 2D ) = "white" {}
		_Turbulence_Mask_Power1( "扰动遮罩对比度", Range( 0, 2 ) ) = 0
		_Turbulence_Mask_Value( "扰动遮罩强度", Range( 0, 2 ) ) = 1
		_TurbulenceMask_UV( "扰动遮罩流动", Vector ) = ( 0, 0, 0, 0 )
		[Enum(R,0,G,1,B,2,A,3)] _TurbulenceMask_Channel( "扰动遮罩通道", Int ) = 0
		[Header(Dissolve)][Toggle( _USEDISSOLVE_ON )] _UseDissolve( "开启溶解", Float ) = 0
		[Toggle( _DISSOLVE_RADIAL3_ON )] _Dissolve_Radial3( "溶解极坐标", Float ) = 0
		_Dissolve_Rotato( "溶解旋转", Range( 0, 1 ) ) = 0
		_Dissolve( "溶解贴图", 2D ) = "white" {}
		_Dissolve_UV( "溶解流动", Vector ) = ( 0, 0, 0, 0 )
		_Dissolve_Value( "溶解度", Range( 0, 1 ) ) = 0
		_Dissolve_Soft( "软硬溶解", Range( 0, 1 ) ) = 0
		_DissolveWidth( "溶解亮边宽度", Range( 0, 0.1 ) ) = 0
		[HDR] _DissolveWidth_Color( "溶解亮边颜色", Color ) = ( 0.9339623, 0.9339623, 0.9339623, 1 )
		_Dissolve_Raodong( "溶解贴图收扰动强度", Float ) = 0
		[Enum(R,0,G,1,B,2,A,3)] _Dissolve_Channel( "溶解通道", Int ) = 0
		[Header(DissolveMask)][Toggle( _USE_DISSOLVEMASK_ON )] _Use_DissolveMask( "开启溶解遮罩", Float ) = 0
		_DissolveMask( "溶解 遮罩", 2D ) = "white" {}
		_DissolveMask_Power( "溶解遮罩对比度", Range( 0, 2 ) ) = 0
		_DissolveMask_Value( "溶解遮罩强度", Range( 0, 2 ) ) = 1
		_DissolveMask_Rotato1( "溶解遮罩旋转", Range( 0, 1 ) ) = 0
		_DissolveMask_UV( "溶解遮罩流动", Vector ) = ( 0, 0, 0, 0 )
		[Enum(R,0,G,1,B,2,A,3)] _DissolveMask_Channel( "溶解遮罩通道", Int ) = 0
		[Header(Ramp)][Toggle( _USERAMP_ON )] _UseRamp( "使用渐变", Float ) = 0
		_Ramp( "渐变图", 2D ) = "white" {}
		_Ramp_Rotato( "渐变旋转", Range( 0, 1 ) ) = 0
		_RampUV( "渐变流动", Vector ) = ( 0, 0, 0, 0 )
		[HideInInspector] _Mask_ST( "Mask_ST", Vector ) = ( 1, 1, 0, 0 )
		[HideInInspector] _Mask02_ST( "Mask02_ST", Vector ) = ( 1, 1, 0, 0 )
		[HideInInspector] _MT02_ST( "MT02_ST", Vector ) = ( 1, 1, 0, 0 )
		[HideInInspector] _MT_ST( "MT_ST", Vector ) = ( 1, 1, 0, 0 )
		[HideInInspector] _Turbulence_ST( "Turbulence_ST", Vector ) = ( 1, 1, 0, 0 )
		[HideInInspector] _Dissolve_ST( "Dissolve_ST", Vector ) = ( 1, 1, 0, 0 )
		_Desaturate( "去饱和度", Range( 0, 1 ) ) = 0

		// ===== 颜色调整（参考 NBShader）=====
		[Toggle( _HUE_SHIFT_ON )] _HueShift_Toggle( "色相", Float ) = 0
		_HueShift( "色相偏移", Range( 0, 1 ) ) = 0
		[Toggle( _SATURATION_ON )] _ChangeSaturability_Toggle( "饱和度", Float ) = 0
		_Saturability( "饱和度", Range( 0, 1 ) ) = 0
		[Toggle( _CONTRAST_ON )] _Contrast_Toggle( "对比度", Float ) = 0
		_Contrast( "对比度", Float ) = 1
		_ContrastMidColor( "对比度中值颜色", Color ) = ( 0.5, 0.5, 0.5, 1 )
		[Toggle( _COLOR_ADJUST_ONLY_MAINTEX )] _ColorAdjustmentOnlyAffectMainTex( "颜色调整仅影响主贴图", Float ) = 0


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		//[HideInInspector][ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

		//[HideInInspector] _AlphaClip("__clip", Float) = 0.0
	}

	SubShader
	{
		PackageRequirements
		{
			"com.unity.render-pipelines.universal": "[14.0,15.0]"
		}

		

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Unlit" }

	LOD 0

		ZWrite On
		Cull [_CullMode]
		AlphaToMask Off
		ColorMask RGBA
		Blend One Zero, One Zero
		BlendOp Add, Add

		

		HLSLINCLUDE
		#pragma target 3.0
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#define ASE_ADJUST_CLIP_POSITION( x ) x

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

			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			Blend SrcAlpha [_dst], Zero Zero
			BlendOp Add, Add

			

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011


			

			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _USEDISSOLVE_ON
			#pragma shader_feature_local _USERAMP_ON
			#pragma shader_feature_local _USEMT02_ON
			#pragma shader_feature_local _MT_USER_ON
			#pragma shader_feature_local _USETURBULENCE_ON
			#pragma shader_feature_local _MT_RADIAL_ON
			#pragma shader_feature_local _TURBULENCE_RADIAL_ON
			#pragma shader_feature_local _USE_TURBULENCEMASK_ON
			#pragma shader_feature_local _MT_RADIAL02_ON
			#pragma shader_feature_local _DISSOLVE_RADIAL3_ON
			#pragma shader_feature_local _USE_DISSOLVEMASK_ON
			#pragma shader_feature_local _USE_MASK02_ON
			#pragma shader_feature_local _MASK_RADIAL1_ON
			#pragma shader_feature_local _MASKMUTUPYORADD_ON
			#pragma shader_feature_local _MASK02_RADIAL2_ON
			#pragma shader_feature_local _HUE_SHIFT_ON _SATURATION_ON _CONTRAST_ON _COLOR_ADJUST_ONLY_MAINTEX


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 positionWSAndFogFactor : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				half4 tangentWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MT_Color02;
			float4 _MT_Color;
			float4 _Mask_ST;
			float4 _DissolveMask_ST;
			float4 _Dissolve_ST;
			float4 _Turbulence_Mask_ST;
			float4 _DissolveWidth_Color;
			float4 _Turbulence_ST;
			float4 _MT_ST;
			float4 _Ramp_ST;
			float4 _Mask02_ST;
			float4 _MT02_ST;
			float2 _Mask02UV;
			float2 _Turbulence_UV;
			float2 _MT_UV02;
			float2 _TurbulenceMask_UV;
			float2 _MT_UV;
			float2 _Dissolve_UV;
			float2 _RampUV;
			float2 _MaskUV;
			float2 _DissolveMask_UV;
			float _Dissolve_Value;
			float _DissolveMask_Value;
			float _CullMode;
			float _Mask_Rotato;
			float _Desaturate;
			float _Mask_Power;
			float _Mask_Value;
			float _Mask02_Rotato;
			float _Mask02_Power;
			float _DissolveWidth;
			float _DissolveMask_Power;
			float _Dissolve_Soft;
			float _Dissolve_Rotato;
			float _Dissolve_Raodong;
			float _Mask02_Value;
			float _MT_Alpha02;
			float _MT_Rotato02;
			float _MT_Alpha;
			float _Turbulence_Mask_Value;
			float _Turbulence_Mask_Power1;
			float _TurbulenceMask_Rotato;
			float _Turbulence_Rotato1;
			float _Turbulence_Value;
			float _MT_Rotato;
			float _Ramp_Rotato;
			float _dst;
			float _DissolveMask_Rotato1;
			float _MainAlpha;
			float _MT_Channel;
			float _MT02_Channel;
			float _Mask_Channel;
			float _Mask02_Channel;
			float _Turbulence_Channel;
			float _TurbulenceMask_Channel;
			float _Dissolve_Channel;
			float _DissolveMask_Channel;
			float _HueShift_Toggle;
			float _HueShift;
			float _ChangeSaturability_Toggle;
			float _Saturability;
			float _Contrast_Toggle;
			float _Contrast;
			float4 _ContrastMidColor;
			float _ColorAdjustmentOnlyAffectMainTex;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifndef NB_CHANNEL_HELPERS
			#define NB_CHANNEL_HELPERS
			float ChannelSelect( float4 c, float idx )
			{
				return idx < 0.5 ? c.r : ( idx < 1.5 ? c.g : ( idx < 2.5 ? c.b : c.a ) );
			}
			float3 ApplyColorAdjustment( float3 col, float hueShift, float saturability, float contrast, float3 contrastMid )
			{
				#if defined( _HUE_SHIFT_ON )
					half3 hsv = RgbToHsv( col );
					hsv.r += hueShift;
					col = HsvToRgb( hsv );
				#endif
				#if defined( _CONTRAST_ON )
					col = lerp( contrastMid, col, contrast );
				#endif
				#if defined( _SATURATION_ON )
					half3 wb = luminance( col );
					col = lerp( wb, col, saturability );
				#endif
				return col;
			}
			#endif

			sampler2D _Ramp;
			sampler2D _MT;
			sampler2D _Sampler60169;
			sampler2D _Turbulence;
			sampler2D _Sampler60233;
			sampler2D _Turbulence_Mask;
			sampler2D _MT02;
			sampler2D _Sampler60258;
			sampler2D _Dissolve;
			sampler2D _Sampler60199;
			sampler2D _DissolveMask;
			sampler2D _Mask;
			sampler2D _Sampler60180;
			sampler2D _Mask02;
			sampler2D _Sampler60188;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_texcoord3.xy = input.ase_texcoord.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				float fogFactor = 0;
				#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
					fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWSAndFogFactor = float4( vertexInput.positionWS, fogFactor );
				output.normalWS = normalInput.normalWS;
				output.tangentWS = half4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
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
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined( _SURFACE_TYPE_TRANSPARENT )
					const bool isTransparent = true;
				#else
					const bool isTransparent = false;
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWSAndFogFactor.xyz );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWSAndFogFactor.xyz;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				half3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				float4 temp_cast_0 = (1.0).xxxx;
				float2 uv_Ramp = input.ase_texcoord3.xy * _Ramp_ST.xy + _Ramp_ST.zw;
				float cos69 = cos( ( ( _Ramp_Rotato * PI ) * 2.0 ) );
				float sin69 = sin( ( ( _Ramp_Rotato * PI ) * 2.0 ) );
				float2 rotator69 = mul( uv_Ramp - float2( 0.5,0.5 ) , float2x2( cos69 , -sin69 , sin69 , cos69 )) + float2( 0.5,0.5 );
				float2 panner71 = ( 1.0 * _Time.y * _RampUV + rotator69);
				#ifdef _USERAMP_ON
				float4 staticSwitch164 = tex2D( _Ramp, panner71 );
				#else
				float4 staticSwitch164 = temp_cast_0;
				#endif
				float4 Ramp73 = staticSwitch164;
				float2 uv_MT = input.ase_texcoord3.xy * _MT_ST.xy + _MT_ST.zw;
				float2 temp_output_1_0_g12 = float2( 1,1 );
				float2 texCoord80_g12 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g12 = (float2(( (temp_output_1_0_g12).x * texCoord80_g12.x ) , ( texCoord80_g12.y * (temp_output_1_0_g12).y )));
				float2 temp_output_11_0_g12 = float2( 0,0 );
				float2 texCoord81_g12 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g12 = ( ( (temp_output_11_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g12);
				float2 panner19_g12 = ( ( _TimeParameters.x * (temp_output_11_0_g12).y ) * float2( 0,1 ) + texCoord81_g12);
				float2 appendResult24_g12 = (float2((panner18_g12).x , (panner19_g12).y));
				float2 MT_UV173 = _MT_UV;
				float2 temp_output_47_0_g12 = MT_UV173;
				float2 texCoord78_g12 = input.ase_texcoord3.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g12 = ( texCoord78_g12 - float2( 1,1 ) );
				float2 appendResult39_g12 = (float2(frac( ( atan2( (temp_output_31_0_g12).x , (temp_output_31_0_g12).y ) / TWO_PI ) ) , length( temp_output_31_0_g12 )));
				float2 panner54_g12 = ( ( (temp_output_47_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g12);
				float2 panner55_g12 = ( ( _TimeParameters.x * (temp_output_47_0_g12).y ) * float2( 0,1 ) + appendResult39_g12);
				float2 appendResult58_g12 = (float2((panner54_g12).x , (panner55_g12).y));
				float2 appendResult171 = (float2(_MT_ST.z , _MT_ST.w));
				float2 MT_ST_ZW172 = appendResult171;
				#ifdef _MT_RADIAL_ON
				float2 staticSwitch177 = ( ( ( (tex2D( _Sampler60169, ( appendResult10_g12 + appendResult24_g12 ) )).rg * 1.0 ) + ( _MT_ST.xy * appendResult58_g12 ) ) + MT_ST_ZW172 );
				#else
				float2 staticSwitch177 = uv_MT;
				#endif
				float cos5 = cos( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float sin5 = sin( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float2 rotator5 = mul( staticSwitch177 - float2( 0.5,0.5 ) , float2x2( cos5 , -sin5 , sin5 , cos5 )) + float2( 0.5,0.5 );
				float2 panner2 = ( 1.0 * _Time.y * _MT_UV + rotator5);
				float2 uv_Turbulence = input.ase_texcoord3.xy * _Turbulence_ST.xy + _Turbulence_ST.zw;
				float2 temp_output_1_0_g9 = float2( 1,1 );
				float2 texCoord80_g9 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g9 = (float2(( (temp_output_1_0_g9).x * texCoord80_g9.x ) , ( texCoord80_g9.y * (temp_output_1_0_g9).y )));
				float2 temp_output_11_0_g9 = float2( 0,0 );
				float2 texCoord81_g9 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g9 = ( ( (temp_output_11_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g9);
				float2 panner19_g9 = ( ( _TimeParameters.x * (temp_output_11_0_g9).y ) * float2( 0,1 ) + texCoord81_g9);
				float2 appendResult24_g9 = (float2((panner18_g9).x , (panner19_g9).y));
				float2 TurbulenceUV237 = _Turbulence_UV;
				float2 temp_output_47_0_g9 = TurbulenceUV237;
				float2 texCoord78_g9 = input.ase_texcoord3.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g9 = ( texCoord78_g9 - float2( 1,1 ) );
				float2 appendResult39_g9 = (float2(frac( ( atan2( (temp_output_31_0_g9).x , (temp_output_31_0_g9).y ) / TWO_PI ) ) , length( temp_output_31_0_g9 )));
				float2 panner54_g9 = ( ( (temp_output_47_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g9);
				float2 panner55_g9 = ( ( _TimeParameters.x * (temp_output_47_0_g9).y ) * float2( 0,1 ) + appendResult39_g9);
				float2 appendResult58_g9 = (float2((panner54_g9).x , (panner55_g9).y));
				float2 appendResult229 = (float2(_Turbulence_ST.z , _Turbulence_ST.w));
				float2 Turbulence_ST_ZW230 = appendResult229;
				#ifdef _TURBULENCE_RADIAL_ON
				float2 staticSwitch236 = ( ( ( (tex2D( _Sampler60233, ( appendResult10_g9 + appendResult24_g9 ) )).rg * 1.0 ) + ( _Turbulence_ST.xy * appendResult58_g9 ) ) + Turbulence_ST_ZW230 );
				#else
				float2 staticSwitch236 = uv_Turbulence;
				#endif
				float cos224 = cos( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float sin224 = sin( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float2 rotator224 = mul( staticSwitch236 - float2( 0.5,0.5 ) , float2x2( cos224 , -sin224 , sin224 , cos224 )) + float2( 0.5,0.5 );
				float2 panner46 = ( 1.0 * _Time.y * _Turbulence_UV + rotator224);
				float4 tex2DNode43 = tex2D( _Turbulence, panner46 );
				float2 uv_Turbulence_Mask = input.ase_texcoord3.xy * _Turbulence_Mask_ST.xy + _Turbulence_Mask_ST.zw;
				float cos57 = cos( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float sin57 = sin( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float2 rotator57 = mul( uv_Turbulence_Mask - float2( 0.5,0.5 ) , float2x2( cos57 , -sin57 , sin57 , cos57 )) + float2( 0.5,0.5 );
				float2 panner62 = ( 1.0 * _Time.y * _TurbulenceMask_UV + rotator57);
				#ifdef _USE_TURBULENCEMASK_ON
				float staticSwitch225 = ( pow( ChannelSelect( tex2D( _Turbulence_Mask, panner62 ), _TurbulenceMask_Channel ) , _Turbulence_Mask_Power1 ) * _Turbulence_Mask_Value );
				#else
				float staticSwitch225 = 1.0;
				#endif
				float Turbulence50 = ( _Turbulence_Value * ChannelSelect( tex2DNode43, _Turbulence_Channel ) * staticSwitch225 );
				#ifdef _USETURBULENCE_ON
				float2 staticSwitch64 = ( panner2 + Turbulence50 );
				#else
				float2 staticSwitch64 = panner2;
				#endif
				float4 tex2DNode1 = tex2D( _MT, staticSwitch64 );
				float4 temp_cast_3 = (ChannelSelect( tex2DNode1, _MT_Channel )).xxxx;
				#ifdef _MT_USER_ON
				float4 staticSwitch220 = temp_cast_3;
				#else
				float4 staticSwitch220 = tex2DNode1;
				#endif
				float4 temp_output_40_0 = ( ( staticSwitch220 * _MT_Alpha ) * _MT_Color );
				#if defined( _COLOR_ADJUST_ONLY_MAINTEX )
				temp_output_40_0.rgb = ApplyColorAdjustment( temp_output_40_0.rgb, _HueShift, _Saturability, _Contrast, _ContrastMidColor );
				#endif
				float2 uv_MT02 = input.ase_texcoord3.xy * _MT02_ST.xy + _MT02_ST.zw;
				float2 temp_output_1_0_g14 = float2( 1,1 );
				float2 texCoord80_g14 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g14 = (float2(( (temp_output_1_0_g14).x * texCoord80_g14.x ) , ( texCoord80_g14.y * (temp_output_1_0_g14).y )));
				float2 temp_output_11_0_g14 = float2( 0,0 );
				float2 texCoord81_g14 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g14 = ( ( (temp_output_11_0_g14).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g14);
				float2 panner19_g14 = ( ( _TimeParameters.x * (temp_output_11_0_g14).y ) * float2( 0,1 ) + texCoord81_g14);
				float2 appendResult24_g14 = (float2((panner18_g14).x , (panner19_g14).y));
				float2 MT_UV02252 = _MT_UV02;
				float2 temp_output_47_0_g14 = MT_UV02252;
				float2 texCoord78_g14 = input.ase_texcoord3.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g14 = ( texCoord78_g14 - float2( 1,1 ) );
				float2 appendResult39_g14 = (float2(frac( ( atan2( (temp_output_31_0_g14).x , (temp_output_31_0_g14).y ) / TWO_PI ) ) , length( temp_output_31_0_g14 )));
				float2 panner54_g14 = ( ( (temp_output_47_0_g14).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g14);
				float2 panner55_g14 = ( ( _TimeParameters.x * (temp_output_47_0_g14).y ) * float2( 0,1 ) + appendResult39_g14);
				float2 appendResult58_g14 = (float2((panner54_g14).x , (panner55_g14).y));
				float2 appendResult253 = (float2(_MT02_ST.z , _MT02_ST.w));
				float2 MT02_ST_ZW254 = appendResult253;
				#ifdef _MT_RADIAL02_ON
				float2 staticSwitch264 = ( ( ( (tex2D( _Sampler60258, ( appendResult10_g14 + appendResult24_g14 ) )).rg * 1.0 ) + ( _MT02_ST.xy * appendResult58_g14 ) ) + MT02_ST_ZW254 );
				#else
				float2 staticSwitch264 = uv_MT02;
				#endif
				float cos266 = cos( ( ( _MT_Rotato02 * PI ) * 2.0 ) );
				float sin266 = sin( ( ( _MT_Rotato02 * PI ) * 2.0 ) );
				float2 rotator266 = mul( staticSwitch264 - float2( 0.5,0.5 ) , float2x2( cos266 , -sin266 , sin266 , cos266 )) + float2( 0.5,0.5 );
				float2 panner268 = ( 1.0 * _Time.y * _MT_UV02 + rotator266);
				#ifdef _USETURBULENCE_ON
				float2 staticSwitch274 = ( panner268 + Turbulence50 );
				#else
				float2 staticSwitch274 = panner268;
				#endif
				float4 tex2DNode270 = tex2D( _MT02, staticSwitch274 );
				float3 MT02280 = ( ( tex2DNode270.rgb * _MT_Alpha02 ) * _MT_Color02.rgb ) * ( ( _MT02_Channel < 0.5 ) ? _MT_Color02.a : ChannelSelect( tex2DNode270, _MT02_Channel - 1.0 ) );
				#ifdef _USEMT02_ON
				float4 staticSwitch285 = ( temp_output_40_0 + float4( MT02280 , 0.0 ) );
				#else
				float4 staticSwitch285 = temp_output_40_0;
				#endif
				float4 temp_output_74_0 = ( Ramp73 * staticSwitch285 );
				float temp_output_130_0 = ( 1.0 - _Dissolve_Soft );
				float2 uv_Dissolve = input.ase_texcoord3.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float2 temp_output_1_0_g4 = float2( 1,1 );
				float2 texCoord80_g4 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g4 = (float2(( (temp_output_1_0_g4).x * texCoord80_g4.x ) , ( texCoord80_g4.y * (temp_output_1_0_g4).y )));
				float2 temp_output_11_0_g4 = float2( 0,0 );
				float2 texCoord81_g4 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g4 = ( ( (temp_output_11_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g4);
				float2 panner19_g4 = ( ( _TimeParameters.x * (temp_output_11_0_g4).y ) * float2( 0,1 ) + texCoord81_g4);
				float2 appendResult24_g4 = (float2((panner18_g4).x , (panner19_g4).y));
				float2 Dissolve_UV204 = _Dissolve_UV;
				float2 temp_output_47_0_g4 = Dissolve_UV204;
				float2 texCoord78_g4 = input.ase_texcoord3.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g4 = ( texCoord78_g4 - float2( 1,1 ) );
				float2 appendResult39_g4 = (float2(frac( ( atan2( (temp_output_31_0_g4).x , (temp_output_31_0_g4).y ) / TWO_PI ) ) , length( temp_output_31_0_g4 )));
				float2 panner54_g4 = ( ( (temp_output_47_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g4);
				float2 panner55_g4 = ( ( _TimeParameters.x * (temp_output_47_0_g4).y ) * float2( 0,1 ) + appendResult39_g4);
				float2 appendResult58_g4 = (float2((panner54_g4).x , (panner55_g4).y));
				float2 appendResult201 = (float2(_Dissolve_ST.z , _Dissolve_ST.w));
				float2 Dissolve_ST_ZW202 = appendResult201;
				#ifdef _DISSOLVE_RADIAL3_ON
				float2 staticSwitch196 = ( ( ( (tex2D( _Sampler60199, ( appendResult10_g4 + appendResult24_g4 ) )).rg * 1.0 ) + ( _Dissolve_ST.xy * appendResult58_g4 ) ) + Dissolve_ST_ZW202 );
				#else
				float2 staticSwitch196 = uv_Dissolve;
				#endif
				float DissolveRaodong145 = ChannelSelect( tex2DNode43, _Turbulence_Channel );
				float cos139 = cos( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float sin139 = sin( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float2 rotator139 = mul( ( staticSwitch196 + ( DissolveRaodong145 * _Dissolve_Raodong ) ) - float2( 0.5,0.5 ) , float2x2( cos139 , -sin139 , sin139 , cos139 )) + float2( 0.5,0.5 );
				float2 panner127 = ( 1.0 * _Time.y * _Dissolve_UV + rotator139);
				float2 uv_DissolveMask = input.ase_texcoord3.xy * _DissolveMask_ST.xy + _DissolveMask_ST.zw;
				float cos153 = cos( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float sin153 = sin( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float2 rotator153 = mul( uv_DissolveMask - float2( 0.5,0.5 ) , float2x2( cos153 , -sin153 , sin153 , cos153 )) + float2( 0.5,0.5 );
				float2 panner150 = ( 1.0 * _Time.y * _DissolveMask_UV + rotator153);
				#ifdef _USE_DISSOLVEMASK_ON
				float staticSwitch227 = ( pow( ChannelSelect( tex2D( _DissolveMask, panner150 ), _DissolveMask_Channel ) , _DissolveMask_Power ) * _DissolveMask_Value );
				#else
				float staticSwitch227 = 1.0;
				#endif
				float temp_output_115_0 = ( ( 1.0 + ( ChannelSelect( tex2D( _Dissolve, panner127 ), _Dissolve_Channel ) * staticSwitch227 ) ) - ( 2.0 * _Dissolve_Value ) );
				float smoothstepResult116 = smoothstep( 0.0 , temp_output_130_0 , temp_output_115_0);
				float smoothstepResult121 = smoothstep( 0.0 , ( temp_output_130_0 + _DissolveWidth ) , temp_output_115_0);
				float4 DissolveWidth136 = ( ( smoothstepResult116 - smoothstepResult121 ) * _DissolveWidth_Color );
				#ifdef _USEDISSOLVE_ON
				float4 staticSwitch166 = ( temp_output_74_0 + DissolveWidth136 );
				#else
				float4 staticSwitch166 = temp_output_74_0;
				#endif
				float4 MT23 = staticSwitch166;
				float3 temp_output_2_0_g13 = ( MT23 * input.ase_color ).rgb;
				float dotResult3_g13 = dot( temp_output_2_0_g13 , float3( 0.299, 0.587, 0.114 ) );
				float3 temp_cast_8 = (dotResult3_g13).xxx;
				float3 lerpResult8_g13 = lerp( temp_output_2_0_g13 , temp_cast_8 , _Desaturate);
				
				float MT_Alpha53 = ChannelSelect( tex2DNode1, _MT_Channel );
				float2 uv_Mask = input.ase_texcoord3.xy * _Mask_ST.xy + _Mask_ST.zw;
				float2 temp_output_1_0_g11 = float2( 1,1 );
				float2 texCoord80_g11 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g11 = (float2(( (temp_output_1_0_g11).x * texCoord80_g11.x ) , ( texCoord80_g11.y * (temp_output_1_0_g11).y )));
				float2 temp_output_11_0_g11 = float2( 0,0 );
				float2 texCoord81_g11 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g11 = ( ( (temp_output_11_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g11);
				float2 panner19_g11 = ( ( _TimeParameters.x * (temp_output_11_0_g11).y ) * float2( 0,1 ) + texCoord81_g11);
				float2 appendResult24_g11 = (float2((panner18_g11).x , (panner19_g11).y));
				float2 Mask_UV194 = _MaskUV;
				float2 temp_output_47_0_g11 = Mask_UV194;
				float2 texCoord78_g11 = input.ase_texcoord3.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g11 = ( texCoord78_g11 - float2( 1,1 ) );
				float2 appendResult39_g11 = (float2(frac( ( atan2( (temp_output_31_0_g11).x , (temp_output_31_0_g11).y ) / TWO_PI ) ) , length( temp_output_31_0_g11 )));
				float2 panner54_g11 = ( ( (temp_output_47_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g11);
				float2 panner55_g11 = ( ( _TimeParameters.x * (temp_output_47_0_g11).y ) * float2( 0,1 ) + appendResult39_g11);
				float2 appendResult58_g11 = (float2((panner54_g11).x , (panner55_g11).y));
				float2 appendResult178 = (float2(_Mask_ST.z , _Mask_ST.w));
				float2 Mask_ST_ZW179 = appendResult178;
				#ifdef _MASK_RADIAL1_ON
				float2 staticSwitch183 = ( ( ( (tex2D( _Sampler60180, ( appendResult10_g11 + appendResult24_g11 ) )).rg * 1.0 ) + ( _Mask_ST.xy * appendResult58_g11 ) ) + Mask_ST_ZW179 );
				#else
				float2 staticSwitch183 = uv_Mask;
				#endif
				float cos11 = cos( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float sin11 = sin( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float2 rotator11 = mul( staticSwitch183 - float2( 0.5,0.5 ) , float2x2( cos11 , -sin11 , sin11 , cos11 )) + float2( 0.5,0.5 );
				float2 panner9 = ( 1.0 * _Time.y * _MaskUV + rotator11);
				float temp_output_77_0 = ( pow( ChannelSelect( tex2D( _Mask, panner9 ), _Mask_Channel ) , _Mask_Power ) * _Mask_Value );
				float2 uv_Mask02 = input.ase_texcoord3.xy * _Mask02_ST.xy + _Mask02_ST.zw;
				float2 temp_output_1_0_g10 = float2( 1,1 );
				float2 texCoord80_g10 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g10 = (float2(( (temp_output_1_0_g10).x * texCoord80_g10.x ) , ( texCoord80_g10.y * (temp_output_1_0_g10).y )));
				float2 temp_output_11_0_g10 = float2( 0,0 );
				float2 texCoord81_g10 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g10 = ( ( (temp_output_11_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g10);
				float2 panner19_g10 = ( ( _TimeParameters.x * (temp_output_11_0_g10).y ) * float2( 0,1 ) + texCoord81_g10);
				float2 appendResult24_g10 = (float2((panner18_g10).x , (panner19_g10).y));
				float2 Mask02_UV195 = _Mask02UV;
				float2 temp_output_47_0_g10 = Mask02_UV195;
				float2 texCoord78_g10 = input.ase_texcoord3.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g10 = ( texCoord78_g10 - float2( 1,1 ) );
				float2 appendResult39_g10 = (float2(frac( ( atan2( (temp_output_31_0_g10).x , (temp_output_31_0_g10).y ) / TWO_PI ) ) , length( temp_output_31_0_g10 )));
				float2 panner54_g10 = ( ( (temp_output_47_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g10);
				float2 panner55_g10 = ( ( _TimeParameters.x * (temp_output_47_0_g10).y ) * float2( 0,1 ) + appendResult39_g10);
				float2 appendResult58_g10 = (float2((panner54_g10).x , (panner55_g10).y));
				float2 appendResult186 = (float2(_Mask02_ST.z , _Mask02_ST.w));
				float2 Mask02_ST_ZW187 = appendResult186;
				#ifdef _MASK02_RADIAL2_ON
				float2 staticSwitch191 = ( ( ( (tex2D( _Sampler60188, ( appendResult10_g10 + appendResult24_g10 ) )).rg * 1.0 ) + ( _Mask02_ST.xy * appendResult58_g10 ) ) + Mask02_ST_ZW187 );
				#else
				float2 staticSwitch191 = uv_Mask02;
				#endif
				float cos34 = cos( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float sin34 = sin( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float2 rotator34 = mul( staticSwitch191 - float2( 0.5,0.5 ) , float2x2( cos34 , -sin34 , sin34 , cos34 )) + float2( 0.5,0.5 );
				float2 panner35 = ( 1.0 * _Time.y * _Mask02UV + rotator34);
				float temp_output_80_0 = ( pow( ChannelSelect( tex2D( _Mask02, panner35 ), _Mask02_Channel ) , _Mask02_Power ) * _Mask02_Value );
				#ifdef _MASKMUTUPYORADD_ON
				float staticSwitch249 = ( temp_output_77_0 * temp_output_80_0 );
				#else
				float staticSwitch249 = ( temp_output_77_0 + temp_output_80_0 );
				#endif
				#ifdef _USE_MASK02_ON
				float staticSwitch38 = staticSwitch249;
				#else
				float staticSwitch38 = temp_output_77_0;
				#endif
				float Mask55 = staticSwitch38;
				float DissolveMask137 = saturate( smoothstepResult116 );
				#ifdef _USEDISSOLVE_ON
				float staticSwitch167 = DissolveMask137;
				#else
				float staticSwitch167 = 1.0;
				#endif
				

				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = lerpResult8_g13;
				#if !defined( _COLOR_ADJUST_ONLY_MAINTEX )
				Color = ApplyColorAdjustment( Color, _HueShift, _Saturability, _Contrast, _ContrastMidColor );
				#endif
				float3 Normal = float3(0, 0, 1);
				#ifdef _USEMT02_ON
				float MT02_Vis53 = ( _MT02_Channel < 0.5 ) ? tex2DNode270.a : ChannelSelect( tex2DNode270, _MT02_Channel - 1.0 );
				float Alpha = ( input.ase_color.a * max( MT_Alpha53, MT02_Vis53 ) * Mask55 * staticSwitch167 * _MainAlpha );
				#else
				float Alpha = ( input.ase_color.a * MT_Alpha53 * Mask55 * staticSwitch167 * _MainAlpha );
				#endif
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif


				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.normalWS = NormalWS;
				inputData.viewDirectionWS = ViewDirWS;

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.positionWSAndFogFactor.w);
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(input.positionCS, Color);
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						Color.rgb = MixFogColor(Color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						Color.rgb = MixFog(Color.rgb, inputData.fogCoord);
					#endif
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				#if defined( ASE_OPAQUE_KEEP_ALPHA )
					return half4( Color, Alpha );
				#else
					return half4( Color, OutputAlpha( Alpha, isTransparent ) );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011


			

			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW // @diogo: removed _vertex for POM node

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _USETURBULENCE_ON
			#pragma shader_feature_local _MT_RADIAL_ON
			#pragma shader_feature_local _TURBULENCE_RADIAL_ON
			#pragma shader_feature_local _USE_TURBULENCEMASK_ON
			#pragma shader_feature_local _USE_MASK02_ON
			#pragma shader_feature_local _MASK_RADIAL1_ON
			#pragma shader_feature_local _MASKMUTUPYORADD_ON
			#pragma shader_feature_local _MASK02_RADIAL2_ON
			#pragma shader_feature_local _HUE_SHIFT_ON _SATURATION_ON _CONTRAST_ON _COLOR_ADJUST_ONLY_MAINTEX
			#pragma shader_feature_local _USEDISSOLVE_ON
			#pragma shader_feature_local _DISSOLVE_RADIAL3_ON
			#pragma shader_feature_local _USE_DISSOLVEMASK_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MT_Color02;
			float4 _MT_Color;
			float4 _Mask_ST;
			float4 _DissolveMask_ST;
			float4 _Dissolve_ST;
			float4 _Turbulence_Mask_ST;
			float4 _DissolveWidth_Color;
			float4 _Turbulence_ST;
			float4 _MT_ST;
			float4 _Ramp_ST;
			float4 _Mask02_ST;
			float4 _MT02_ST;
			float2 _Mask02UV;
			float2 _Turbulence_UV;
			float2 _MT_UV02;
			float2 _TurbulenceMask_UV;
			float2 _MT_UV;
			float2 _Dissolve_UV;
			float2 _RampUV;
			float2 _MaskUV;
			float2 _DissolveMask_UV;
			float _Dissolve_Value;
			float _DissolveMask_Value;
			float _CullMode;
			float _Mask_Rotato;
			float _Desaturate;
			float _Mask_Power;
			float _Mask_Value;
			float _Mask02_Rotato;
			float _Mask02_Power;
			float _DissolveWidth;
			float _DissolveMask_Power;
			float _Dissolve_Soft;
			float _Dissolve_Rotato;
			float _Dissolve_Raodong;
			float _Mask02_Value;
			float _MT_Alpha02;
			float _MT_Rotato02;
			float _MT_Alpha;
			float _Turbulence_Mask_Value;
			float _Turbulence_Mask_Power1;
			float _TurbulenceMask_Rotato;
			float _Turbulence_Rotato1;
			float _Turbulence_Value;
			float _MT_Rotato;
			float _Ramp_Rotato;
			float _dst;
			float _DissolveMask_Rotato1;
			float _MainAlpha;
			float _MT_Channel;
			float _MT02_Channel;
			float _Mask_Channel;
			float _Mask02_Channel;
			float _Turbulence_Channel;
			float _TurbulenceMask_Channel;
			float _Dissolve_Channel;
			float _DissolveMask_Channel;
			float _HueShift_Toggle;
			float _HueShift;
			float _ChangeSaturability_Toggle;
			float _Saturability;
			float _Contrast_Toggle;
			float _Contrast;
			float4 _ContrastMidColor;
			float _ColorAdjustmentOnlyAffectMainTex;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifndef NB_CHANNEL_HELPERS
			#define NB_CHANNEL_HELPERS
			float ChannelSelect( float4 c, float idx )
			{
				return idx < 0.5 ? c.r : ( idx < 1.5 ? c.g : ( idx < 2.5 ? c.b : c.a ) );
			}
			float3 ApplyColorAdjustment( float3 col, float hueShift, float saturability, float contrast, float3 contrastMid )
			{
				#if defined( _HUE_SHIFT_ON )
					half3 hsv = RgbToHsv( col );
					hsv.r += hueShift;
					col = HsvToRgb( hsv );
				#endif
				#if defined( _CONTRAST_ON )
					col = lerp( contrastMid, col, contrast );
				#endif
				#if defined( _SATURATION_ON )
					half3 wb = luminance( col );
					col = lerp( wb, col, saturability );
				#endif
				return col;
			}
			#endif

			sampler2D _MT;
			sampler2D _Sampler60169;
			sampler2D _Turbulence;
			sampler2D _Sampler60233;
			sampler2D _Turbulence_Mask;
			sampler2D _Mask;
			sampler2D _Sampler60180;
			sampler2D _Mask02;
			sampler2D _Sampler60188;
			sampler2D _Dissolve;
			sampler2D _Sampler60199;
			sampler2D _DissolveMask;


			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input )
			{
				PackedVaryings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				output.ase_color = input.ase_color;
				output.ase_texcoord.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				float3 positionWS = TransformObjectToWorld( input.positionOS.xyz );
				half3 normalWS = TransformObjectToWorldDir(input.normalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
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
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				float2 uv_MT = input.ase_texcoord.xy * _MT_ST.xy + _MT_ST.zw;
				float2 temp_output_1_0_g12 = float2( 1,1 );
				float2 texCoord80_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g12 = (float2(( (temp_output_1_0_g12).x * texCoord80_g12.x ) , ( texCoord80_g12.y * (temp_output_1_0_g12).y )));
				float2 temp_output_11_0_g12 = float2( 0,0 );
				float2 texCoord81_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g12 = ( ( (temp_output_11_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g12);
				float2 panner19_g12 = ( ( _TimeParameters.x * (temp_output_11_0_g12).y ) * float2( 0,1 ) + texCoord81_g12);
				float2 appendResult24_g12 = (float2((panner18_g12).x , (panner19_g12).y));
				float2 MT_UV173 = _MT_UV;
				float2 temp_output_47_0_g12 = MT_UV173;
				float2 texCoord78_g12 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g12 = ( texCoord78_g12 - float2( 1,1 ) );
				float2 appendResult39_g12 = (float2(frac( ( atan2( (temp_output_31_0_g12).x , (temp_output_31_0_g12).y ) / TWO_PI ) ) , length( temp_output_31_0_g12 )));
				float2 panner54_g12 = ( ( (temp_output_47_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g12);
				float2 panner55_g12 = ( ( _TimeParameters.x * (temp_output_47_0_g12).y ) * float2( 0,1 ) + appendResult39_g12);
				float2 appendResult58_g12 = (float2((panner54_g12).x , (panner55_g12).y));
				float2 appendResult171 = (float2(_MT_ST.z , _MT_ST.w));
				float2 MT_ST_ZW172 = appendResult171;
				#ifdef _MT_RADIAL_ON
				float2 staticSwitch177 = ( ( ( (tex2D( _Sampler60169, ( appendResult10_g12 + appendResult24_g12 ) )).rg * 1.0 ) + ( _MT_ST.xy * appendResult58_g12 ) ) + MT_ST_ZW172 );
				#else
				float2 staticSwitch177 = uv_MT;
				#endif
				float cos5 = cos( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float sin5 = sin( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float2 rotator5 = mul( staticSwitch177 - float2( 0.5,0.5 ) , float2x2( cos5 , -sin5 , sin5 , cos5 )) + float2( 0.5,0.5 );
				float2 panner2 = ( 1.0 * _Time.y * _MT_UV + rotator5);
				float2 uv_Turbulence = input.ase_texcoord.xy * _Turbulence_ST.xy + _Turbulence_ST.zw;
				float2 temp_output_1_0_g9 = float2( 1,1 );
				float2 texCoord80_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g9 = (float2(( (temp_output_1_0_g9).x * texCoord80_g9.x ) , ( texCoord80_g9.y * (temp_output_1_0_g9).y )));
				float2 temp_output_11_0_g9 = float2( 0,0 );
				float2 texCoord81_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g9 = ( ( (temp_output_11_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g9);
				float2 panner19_g9 = ( ( _TimeParameters.x * (temp_output_11_0_g9).y ) * float2( 0,1 ) + texCoord81_g9);
				float2 appendResult24_g9 = (float2((panner18_g9).x , (panner19_g9).y));
				float2 TurbulenceUV237 = _Turbulence_UV;
				float2 temp_output_47_0_g9 = TurbulenceUV237;
				float2 texCoord78_g9 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g9 = ( texCoord78_g9 - float2( 1,1 ) );
				float2 appendResult39_g9 = (float2(frac( ( atan2( (temp_output_31_0_g9).x , (temp_output_31_0_g9).y ) / TWO_PI ) ) , length( temp_output_31_0_g9 )));
				float2 panner54_g9 = ( ( (temp_output_47_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g9);
				float2 panner55_g9 = ( ( _TimeParameters.x * (temp_output_47_0_g9).y ) * float2( 0,1 ) + appendResult39_g9);
				float2 appendResult58_g9 = (float2((panner54_g9).x , (panner55_g9).y));
				float2 appendResult229 = (float2(_Turbulence_ST.z , _Turbulence_ST.w));
				float2 Turbulence_ST_ZW230 = appendResult229;
				#ifdef _TURBULENCE_RADIAL_ON
				float2 staticSwitch236 = ( ( ( (tex2D( _Sampler60233, ( appendResult10_g9 + appendResult24_g9 ) )).rg * 1.0 ) + ( _Turbulence_ST.xy * appendResult58_g9 ) ) + Turbulence_ST_ZW230 );
				#else
				float2 staticSwitch236 = uv_Turbulence;
				#endif
				float cos224 = cos( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float sin224 = sin( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float2 rotator224 = mul( staticSwitch236 - float2( 0.5,0.5 ) , float2x2( cos224 , -sin224 , sin224 , cos224 )) + float2( 0.5,0.5 );
				float2 panner46 = ( 1.0 * _Time.y * _Turbulence_UV + rotator224);
				float4 tex2DNode43 = tex2D( _Turbulence, panner46 );
				float2 uv_Turbulence_Mask = input.ase_texcoord.xy * _Turbulence_Mask_ST.xy + _Turbulence_Mask_ST.zw;
				float cos57 = cos( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float sin57 = sin( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float2 rotator57 = mul( uv_Turbulence_Mask - float2( 0.5,0.5 ) , float2x2( cos57 , -sin57 , sin57 , cos57 )) + float2( 0.5,0.5 );
				float2 panner62 = ( 1.0 * _Time.y * _TurbulenceMask_UV + rotator57);
				#ifdef _USE_TURBULENCEMASK_ON
				float staticSwitch225 = ( pow( ChannelSelect( tex2D( _Turbulence_Mask, panner62 ), _TurbulenceMask_Channel ) , _Turbulence_Mask_Power1 ) * _Turbulence_Mask_Value );
				#else
				float staticSwitch225 = 1.0;
				#endif
				float Turbulence50 = ( _Turbulence_Value * ChannelSelect( tex2DNode43, _Turbulence_Channel ) * staticSwitch225 );
				#ifdef _USETURBULENCE_ON
				float2 staticSwitch64 = ( panner2 + Turbulence50 );
				#else
				float2 staticSwitch64 = panner2;
				#endif
				float4 tex2DNode1 = tex2D( _MT, staticSwitch64 );
				float MT_Alpha53 = ChannelSelect( tex2DNode1, _MT_Channel );
				float2 uv_Mask = input.ase_texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
				float2 temp_output_1_0_g11 = float2( 1,1 );
				float2 texCoord80_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g11 = (float2(( (temp_output_1_0_g11).x * texCoord80_g11.x ) , ( texCoord80_g11.y * (temp_output_1_0_g11).y )));
				float2 temp_output_11_0_g11 = float2( 0,0 );
				float2 texCoord81_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g11 = ( ( (temp_output_11_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g11);
				float2 panner19_g11 = ( ( _TimeParameters.x * (temp_output_11_0_g11).y ) * float2( 0,1 ) + texCoord81_g11);
				float2 appendResult24_g11 = (float2((panner18_g11).x , (panner19_g11).y));
				float2 Mask_UV194 = _MaskUV;
				float2 temp_output_47_0_g11 = Mask_UV194;
				float2 texCoord78_g11 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g11 = ( texCoord78_g11 - float2( 1,1 ) );
				float2 appendResult39_g11 = (float2(frac( ( atan2( (temp_output_31_0_g11).x , (temp_output_31_0_g11).y ) / TWO_PI ) ) , length( temp_output_31_0_g11 )));
				float2 panner54_g11 = ( ( (temp_output_47_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g11);
				float2 panner55_g11 = ( ( _TimeParameters.x * (temp_output_47_0_g11).y ) * float2( 0,1 ) + appendResult39_g11);
				float2 appendResult58_g11 = (float2((panner54_g11).x , (panner55_g11).y));
				float2 appendResult178 = (float2(_Mask_ST.z , _Mask_ST.w));
				float2 Mask_ST_ZW179 = appendResult178;
				#ifdef _MASK_RADIAL1_ON
				float2 staticSwitch183 = ( ( ( (tex2D( _Sampler60180, ( appendResult10_g11 + appendResult24_g11 ) )).rg * 1.0 ) + ( _Mask_ST.xy * appendResult58_g11 ) ) + Mask_ST_ZW179 );
				#else
				float2 staticSwitch183 = uv_Mask;
				#endif
				float cos11 = cos( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float sin11 = sin( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float2 rotator11 = mul( staticSwitch183 - float2( 0.5,0.5 ) , float2x2( cos11 , -sin11 , sin11 , cos11 )) + float2( 0.5,0.5 );
				float2 panner9 = ( 1.0 * _Time.y * _MaskUV + rotator11);
				float temp_output_77_0 = ( pow( ChannelSelect( tex2D( _Mask, panner9 ), _Mask_Channel ) , _Mask_Power ) * _Mask_Value );
				float2 uv_Mask02 = input.ase_texcoord.xy * _Mask02_ST.xy + _Mask02_ST.zw;
				float2 temp_output_1_0_g10 = float2( 1,1 );
				float2 texCoord80_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g10 = (float2(( (temp_output_1_0_g10).x * texCoord80_g10.x ) , ( texCoord80_g10.y * (temp_output_1_0_g10).y )));
				float2 temp_output_11_0_g10 = float2( 0,0 );
				float2 texCoord81_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g10 = ( ( (temp_output_11_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g10);
				float2 panner19_g10 = ( ( _TimeParameters.x * (temp_output_11_0_g10).y ) * float2( 0,1 ) + texCoord81_g10);
				float2 appendResult24_g10 = (float2((panner18_g10).x , (panner19_g10).y));
				float2 Mask02_UV195 = _Mask02UV;
				float2 temp_output_47_0_g10 = Mask02_UV195;
				float2 texCoord78_g10 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g10 = ( texCoord78_g10 - float2( 1,1 ) );
				float2 appendResult39_g10 = (float2(frac( ( atan2( (temp_output_31_0_g10).x , (temp_output_31_0_g10).y ) / TWO_PI ) ) , length( temp_output_31_0_g10 )));
				float2 panner54_g10 = ( ( (temp_output_47_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g10);
				float2 panner55_g10 = ( ( _TimeParameters.x * (temp_output_47_0_g10).y ) * float2( 0,1 ) + appendResult39_g10);
				float2 appendResult58_g10 = (float2((panner54_g10).x , (panner55_g10).y));
				float2 appendResult186 = (float2(_Mask02_ST.z , _Mask02_ST.w));
				float2 Mask02_ST_ZW187 = appendResult186;
				#ifdef _MASK02_RADIAL2_ON
				float2 staticSwitch191 = ( ( ( (tex2D( _Sampler60188, ( appendResult10_g10 + appendResult24_g10 ) )).rg * 1.0 ) + ( _Mask02_ST.xy * appendResult58_g10 ) ) + Mask02_ST_ZW187 );
				#else
				float2 staticSwitch191 = uv_Mask02;
				#endif
				float cos34 = cos( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float sin34 = sin( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float2 rotator34 = mul( staticSwitch191 - float2( 0.5,0.5 ) , float2x2( cos34 , -sin34 , sin34 , cos34 )) + float2( 0.5,0.5 );
				float2 panner35 = ( 1.0 * _Time.y * _Mask02UV + rotator34);
				float temp_output_80_0 = ( pow( ChannelSelect( tex2D( _Mask02, panner35 ), _Mask02_Channel ) , _Mask02_Power ) * _Mask02_Value );
				#ifdef _MASKMUTUPYORADD_ON
				float staticSwitch249 = ( temp_output_77_0 * temp_output_80_0 );
				#else
				float staticSwitch249 = ( temp_output_77_0 + temp_output_80_0 );
				#endif
				#ifdef _USE_MASK02_ON
				float staticSwitch38 = staticSwitch249;
				#else
				float staticSwitch38 = temp_output_77_0;
				#endif
				float Mask55 = staticSwitch38;
				float temp_output_130_0 = ( 1.0 - _Dissolve_Soft );
				float2 uv_Dissolve = input.ase_texcoord.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float2 temp_output_1_0_g4 = float2( 1,1 );
				float2 texCoord80_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g4 = (float2(( (temp_output_1_0_g4).x * texCoord80_g4.x ) , ( texCoord80_g4.y * (temp_output_1_0_g4).y )));
				float2 temp_output_11_0_g4 = float2( 0,0 );
				float2 texCoord81_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g4 = ( ( (temp_output_11_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g4);
				float2 panner19_g4 = ( ( _TimeParameters.x * (temp_output_11_0_g4).y ) * float2( 0,1 ) + texCoord81_g4);
				float2 appendResult24_g4 = (float2((panner18_g4).x , (panner19_g4).y));
				float2 Dissolve_UV204 = _Dissolve_UV;
				float2 temp_output_47_0_g4 = Dissolve_UV204;
				float2 texCoord78_g4 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g4 = ( texCoord78_g4 - float2( 1,1 ) );
				float2 appendResult39_g4 = (float2(frac( ( atan2( (temp_output_31_0_g4).x , (temp_output_31_0_g4).y ) / TWO_PI ) ) , length( temp_output_31_0_g4 )));
				float2 panner54_g4 = ( ( (temp_output_47_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g4);
				float2 panner55_g4 = ( ( _TimeParameters.x * (temp_output_47_0_g4).y ) * float2( 0,1 ) + appendResult39_g4);
				float2 appendResult58_g4 = (float2((panner54_g4).x , (panner55_g4).y));
				float2 appendResult201 = (float2(_Dissolve_ST.z , _Dissolve_ST.w));
				float2 Dissolve_ST_ZW202 = appendResult201;
				#ifdef _DISSOLVE_RADIAL3_ON
				float2 staticSwitch196 = ( ( ( (tex2D( _Sampler60199, ( appendResult10_g4 + appendResult24_g4 ) )).rg * 1.0 ) + ( _Dissolve_ST.xy * appendResult58_g4 ) ) + Dissolve_ST_ZW202 );
				#else
				float2 staticSwitch196 = uv_Dissolve;
				#endif
				float DissolveRaodong145 = ChannelSelect( tex2DNode43, _Turbulence_Channel );
				float cos139 = cos( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float sin139 = sin( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float2 rotator139 = mul( ( staticSwitch196 + ( DissolveRaodong145 * _Dissolve_Raodong ) ) - float2( 0.5,0.5 ) , float2x2( cos139 , -sin139 , sin139 , cos139 )) + float2( 0.5,0.5 );
				float2 panner127 = ( 1.0 * _Time.y * _Dissolve_UV + rotator139);
				float2 uv_DissolveMask = input.ase_texcoord.xy * _DissolveMask_ST.xy + _DissolveMask_ST.zw;
				float cos153 = cos( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float sin153 = sin( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float2 rotator153 = mul( uv_DissolveMask - float2( 0.5,0.5 ) , float2x2( cos153 , -sin153 , sin153 , cos153 )) + float2( 0.5,0.5 );
				float2 panner150 = ( 1.0 * _Time.y * _DissolveMask_UV + rotator153);
				#ifdef _USE_DISSOLVEMASK_ON
				float staticSwitch227 = ( pow( ChannelSelect( tex2D( _DissolveMask, panner150 ), _DissolveMask_Channel ) , _DissolveMask_Power ) * _DissolveMask_Value );
				#else
				float staticSwitch227 = 1.0;
				#endif
				float temp_output_115_0 = ( ( 1.0 + ( ChannelSelect( tex2D( _Dissolve, panner127 ), _Dissolve_Channel ) * staticSwitch227 ) ) - ( 2.0 * _Dissolve_Value ) );
				float smoothstepResult116 = smoothstep( 0.0 , temp_output_130_0 , temp_output_115_0);
				float DissolveMask137 = saturate( smoothstepResult116 );
				#ifdef _USEDISSOLVE_ON
				float staticSwitch167 = DissolveMask137;
				#else
				float staticSwitch167 = 1.0;
				#endif
				


				float Alpha = ( input.ase_color.a * MT_Alpha53 * Mask55 * staticSwitch167 * _MainAlpha );
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					#if defined( _ALPHATEST_SHADOW_ON )
						AlphaDiscard( Alpha, AlphaClipThresholdShadow );
					#else
						AlphaDiscard( Alpha, AlphaClipThreshold );
					#endif
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _USETURBULENCE_ON
			#pragma shader_feature_local _MT_RADIAL_ON
			#pragma shader_feature_local _TURBULENCE_RADIAL_ON
			#pragma shader_feature_local _USE_TURBULENCEMASK_ON
			#pragma shader_feature_local _USE_MASK02_ON
			#pragma shader_feature_local _MASK_RADIAL1_ON
			#pragma shader_feature_local _MASKMUTUPYORADD_ON
			#pragma shader_feature_local _MASK02_RADIAL2_ON
			#pragma shader_feature_local _HUE_SHIFT_ON _SATURATION_ON _CONTRAST_ON _COLOR_ADJUST_ONLY_MAINTEX
			#pragma shader_feature_local _USEDISSOLVE_ON
			#pragma shader_feature_local _DISSOLVE_RADIAL3_ON
			#pragma shader_feature_local _USE_DISSOLVEMASK_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MT_Color02;
			float4 _MT_Color;
			float4 _Mask_ST;
			float4 _DissolveMask_ST;
			float4 _Dissolve_ST;
			float4 _Turbulence_Mask_ST;
			float4 _DissolveWidth_Color;
			float4 _Turbulence_ST;
			float4 _MT_ST;
			float4 _Ramp_ST;
			float4 _Mask02_ST;
			float4 _MT02_ST;
			float2 _Mask02UV;
			float2 _Turbulence_UV;
			float2 _MT_UV02;
			float2 _TurbulenceMask_UV;
			float2 _MT_UV;
			float2 _Dissolve_UV;
			float2 _RampUV;
			float2 _MaskUV;
			float2 _DissolveMask_UV;
			float _Dissolve_Value;
			float _DissolveMask_Value;
			float _CullMode;
			float _Mask_Rotato;
			float _Desaturate;
			float _Mask_Power;
			float _Mask_Value;
			float _Mask02_Rotato;
			float _Mask02_Power;
			float _DissolveWidth;
			float _DissolveMask_Power;
			float _Dissolve_Soft;
			float _Dissolve_Rotato;
			float _Dissolve_Raodong;
			float _Mask02_Value;
			float _MT_Alpha02;
			float _MT_Rotato02;
			float _MT_Alpha;
			float _Turbulence_Mask_Value;
			float _Turbulence_Mask_Power1;
			float _TurbulenceMask_Rotato;
			float _Turbulence_Rotato1;
			float _Turbulence_Value;
			float _MT_Rotato;
			float _Ramp_Rotato;
			float _dst;
			float _DissolveMask_Rotato1;
			float _MainAlpha;
			float _MT_Channel;
			float _MT02_Channel;
			float _Mask_Channel;
			float _Mask02_Channel;
			float _Turbulence_Channel;
			float _TurbulenceMask_Channel;
			float _Dissolve_Channel;
			float _DissolveMask_Channel;
			float _HueShift_Toggle;
			float _HueShift;
			float _ChangeSaturability_Toggle;
			float _Saturability;
			float _Contrast_Toggle;
			float _Contrast;
			float4 _ContrastMidColor;
			float _ColorAdjustmentOnlyAffectMainTex;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifndef NB_CHANNEL_HELPERS
			#define NB_CHANNEL_HELPERS
			float ChannelSelect( float4 c, float idx )
			{
				return idx < 0.5 ? c.r : ( idx < 1.5 ? c.g : ( idx < 2.5 ? c.b : c.a ) );
			}
			float3 ApplyColorAdjustment( float3 col, float hueShift, float saturability, float contrast, float3 contrastMid )
			{
				#if defined( _HUE_SHIFT_ON )
					half3 hsv = RgbToHsv( col );
					hsv.r += hueShift;
					col = HsvToRgb( hsv );
				#endif
				#if defined( _CONTRAST_ON )
					col = lerp( contrastMid, col, contrast );
				#endif
				#if defined( _SATURATION_ON )
					half3 wb = luminance( col );
					col = lerp( wb, col, saturability );
				#endif
				return col;
			}
			#endif

			sampler2D _MT;
			sampler2D _Sampler60169;
			sampler2D _Turbulence;
			sampler2D _Sampler60233;
			sampler2D _Turbulence_Mask;
			sampler2D _Mask;
			sampler2D _Sampler60180;
			sampler2D _Mask02;
			sampler2D _Sampler60188;
			sampler2D _Dissolve;
			sampler2D _Sampler60199;
			sampler2D _DissolveMask;


			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_color = input.ase_color;
				output.ase_texcoord.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
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
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				float2 uv_MT = input.ase_texcoord.xy * _MT_ST.xy + _MT_ST.zw;
				float2 temp_output_1_0_g12 = float2( 1,1 );
				float2 texCoord80_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g12 = (float2(( (temp_output_1_0_g12).x * texCoord80_g12.x ) , ( texCoord80_g12.y * (temp_output_1_0_g12).y )));
				float2 temp_output_11_0_g12 = float2( 0,0 );
				float2 texCoord81_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g12 = ( ( (temp_output_11_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g12);
				float2 panner19_g12 = ( ( _TimeParameters.x * (temp_output_11_0_g12).y ) * float2( 0,1 ) + texCoord81_g12);
				float2 appendResult24_g12 = (float2((panner18_g12).x , (panner19_g12).y));
				float2 MT_UV173 = _MT_UV;
				float2 temp_output_47_0_g12 = MT_UV173;
				float2 texCoord78_g12 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g12 = ( texCoord78_g12 - float2( 1,1 ) );
				float2 appendResult39_g12 = (float2(frac( ( atan2( (temp_output_31_0_g12).x , (temp_output_31_0_g12).y ) / TWO_PI ) ) , length( temp_output_31_0_g12 )));
				float2 panner54_g12 = ( ( (temp_output_47_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g12);
				float2 panner55_g12 = ( ( _TimeParameters.x * (temp_output_47_0_g12).y ) * float2( 0,1 ) + appendResult39_g12);
				float2 appendResult58_g12 = (float2((panner54_g12).x , (panner55_g12).y));
				float2 appendResult171 = (float2(_MT_ST.z , _MT_ST.w));
				float2 MT_ST_ZW172 = appendResult171;
				#ifdef _MT_RADIAL_ON
				float2 staticSwitch177 = ( ( ( (tex2D( _Sampler60169, ( appendResult10_g12 + appendResult24_g12 ) )).rg * 1.0 ) + ( _MT_ST.xy * appendResult58_g12 ) ) + MT_ST_ZW172 );
				#else
				float2 staticSwitch177 = uv_MT;
				#endif
				float cos5 = cos( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float sin5 = sin( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float2 rotator5 = mul( staticSwitch177 - float2( 0.5,0.5 ) , float2x2( cos5 , -sin5 , sin5 , cos5 )) + float2( 0.5,0.5 );
				float2 panner2 = ( 1.0 * _Time.y * _MT_UV + rotator5);
				float2 uv_Turbulence = input.ase_texcoord.xy * _Turbulence_ST.xy + _Turbulence_ST.zw;
				float2 temp_output_1_0_g9 = float2( 1,1 );
				float2 texCoord80_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g9 = (float2(( (temp_output_1_0_g9).x * texCoord80_g9.x ) , ( texCoord80_g9.y * (temp_output_1_0_g9).y )));
				float2 temp_output_11_0_g9 = float2( 0,0 );
				float2 texCoord81_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g9 = ( ( (temp_output_11_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g9);
				float2 panner19_g9 = ( ( _TimeParameters.x * (temp_output_11_0_g9).y ) * float2( 0,1 ) + texCoord81_g9);
				float2 appendResult24_g9 = (float2((panner18_g9).x , (panner19_g9).y));
				float2 TurbulenceUV237 = _Turbulence_UV;
				float2 temp_output_47_0_g9 = TurbulenceUV237;
				float2 texCoord78_g9 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g9 = ( texCoord78_g9 - float2( 1,1 ) );
				float2 appendResult39_g9 = (float2(frac( ( atan2( (temp_output_31_0_g9).x , (temp_output_31_0_g9).y ) / TWO_PI ) ) , length( temp_output_31_0_g9 )));
				float2 panner54_g9 = ( ( (temp_output_47_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g9);
				float2 panner55_g9 = ( ( _TimeParameters.x * (temp_output_47_0_g9).y ) * float2( 0,1 ) + appendResult39_g9);
				float2 appendResult58_g9 = (float2((panner54_g9).x , (panner55_g9).y));
				float2 appendResult229 = (float2(_Turbulence_ST.z , _Turbulence_ST.w));
				float2 Turbulence_ST_ZW230 = appendResult229;
				#ifdef _TURBULENCE_RADIAL_ON
				float2 staticSwitch236 = ( ( ( (tex2D( _Sampler60233, ( appendResult10_g9 + appendResult24_g9 ) )).rg * 1.0 ) + ( _Turbulence_ST.xy * appendResult58_g9 ) ) + Turbulence_ST_ZW230 );
				#else
				float2 staticSwitch236 = uv_Turbulence;
				#endif
				float cos224 = cos( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float sin224 = sin( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float2 rotator224 = mul( staticSwitch236 - float2( 0.5,0.5 ) , float2x2( cos224 , -sin224 , sin224 , cos224 )) + float2( 0.5,0.5 );
				float2 panner46 = ( 1.0 * _Time.y * _Turbulence_UV + rotator224);
				float4 tex2DNode43 = tex2D( _Turbulence, panner46 );
				float2 uv_Turbulence_Mask = input.ase_texcoord.xy * _Turbulence_Mask_ST.xy + _Turbulence_Mask_ST.zw;
				float cos57 = cos( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float sin57 = sin( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float2 rotator57 = mul( uv_Turbulence_Mask - float2( 0.5,0.5 ) , float2x2( cos57 , -sin57 , sin57 , cos57 )) + float2( 0.5,0.5 );
				float2 panner62 = ( 1.0 * _Time.y * _TurbulenceMask_UV + rotator57);
				#ifdef _USE_TURBULENCEMASK_ON
				float staticSwitch225 = ( pow( ChannelSelect( tex2D( _Turbulence_Mask, panner62 ), _TurbulenceMask_Channel ) , _Turbulence_Mask_Power1 ) * _Turbulence_Mask_Value );
				#else
				float staticSwitch225 = 1.0;
				#endif
				float Turbulence50 = ( _Turbulence_Value * ChannelSelect( tex2DNode43, _Turbulence_Channel ) * staticSwitch225 );
				#ifdef _USETURBULENCE_ON
				float2 staticSwitch64 = ( panner2 + Turbulence50 );
				#else
				float2 staticSwitch64 = panner2;
				#endif
				float4 tex2DNode1 = tex2D( _MT, staticSwitch64 );
				float MT_Alpha53 = ChannelSelect( tex2DNode1, _MT_Channel );
				float2 uv_Mask = input.ase_texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
				float2 temp_output_1_0_g11 = float2( 1,1 );
				float2 texCoord80_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g11 = (float2(( (temp_output_1_0_g11).x * texCoord80_g11.x ) , ( texCoord80_g11.y * (temp_output_1_0_g11).y )));
				float2 temp_output_11_0_g11 = float2( 0,0 );
				float2 texCoord81_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g11 = ( ( (temp_output_11_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g11);
				float2 panner19_g11 = ( ( _TimeParameters.x * (temp_output_11_0_g11).y ) * float2( 0,1 ) + texCoord81_g11);
				float2 appendResult24_g11 = (float2((panner18_g11).x , (panner19_g11).y));
				float2 Mask_UV194 = _MaskUV;
				float2 temp_output_47_0_g11 = Mask_UV194;
				float2 texCoord78_g11 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g11 = ( texCoord78_g11 - float2( 1,1 ) );
				float2 appendResult39_g11 = (float2(frac( ( atan2( (temp_output_31_0_g11).x , (temp_output_31_0_g11).y ) / TWO_PI ) ) , length( temp_output_31_0_g11 )));
				float2 panner54_g11 = ( ( (temp_output_47_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g11);
				float2 panner55_g11 = ( ( _TimeParameters.x * (temp_output_47_0_g11).y ) * float2( 0,1 ) + appendResult39_g11);
				float2 appendResult58_g11 = (float2((panner54_g11).x , (panner55_g11).y));
				float2 appendResult178 = (float2(_Mask_ST.z , _Mask_ST.w));
				float2 Mask_ST_ZW179 = appendResult178;
				#ifdef _MASK_RADIAL1_ON
				float2 staticSwitch183 = ( ( ( (tex2D( _Sampler60180, ( appendResult10_g11 + appendResult24_g11 ) )).rg * 1.0 ) + ( _Mask_ST.xy * appendResult58_g11 ) ) + Mask_ST_ZW179 );
				#else
				float2 staticSwitch183 = uv_Mask;
				#endif
				float cos11 = cos( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float sin11 = sin( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float2 rotator11 = mul( staticSwitch183 - float2( 0.5,0.5 ) , float2x2( cos11 , -sin11 , sin11 , cos11 )) + float2( 0.5,0.5 );
				float2 panner9 = ( 1.0 * _Time.y * _MaskUV + rotator11);
				float temp_output_77_0 = ( pow( ChannelSelect( tex2D( _Mask, panner9 ), _Mask_Channel ) , _Mask_Power ) * _Mask_Value );
				float2 uv_Mask02 = input.ase_texcoord.xy * _Mask02_ST.xy + _Mask02_ST.zw;
				float2 temp_output_1_0_g10 = float2( 1,1 );
				float2 texCoord80_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g10 = (float2(( (temp_output_1_0_g10).x * texCoord80_g10.x ) , ( texCoord80_g10.y * (temp_output_1_0_g10).y )));
				float2 temp_output_11_0_g10 = float2( 0,0 );
				float2 texCoord81_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g10 = ( ( (temp_output_11_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g10);
				float2 panner19_g10 = ( ( _TimeParameters.x * (temp_output_11_0_g10).y ) * float2( 0,1 ) + texCoord81_g10);
				float2 appendResult24_g10 = (float2((panner18_g10).x , (panner19_g10).y));
				float2 Mask02_UV195 = _Mask02UV;
				float2 temp_output_47_0_g10 = Mask02_UV195;
				float2 texCoord78_g10 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g10 = ( texCoord78_g10 - float2( 1,1 ) );
				float2 appendResult39_g10 = (float2(frac( ( atan2( (temp_output_31_0_g10).x , (temp_output_31_0_g10).y ) / TWO_PI ) ) , length( temp_output_31_0_g10 )));
				float2 panner54_g10 = ( ( (temp_output_47_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g10);
				float2 panner55_g10 = ( ( _TimeParameters.x * (temp_output_47_0_g10).y ) * float2( 0,1 ) + appendResult39_g10);
				float2 appendResult58_g10 = (float2((panner54_g10).x , (panner55_g10).y));
				float2 appendResult186 = (float2(_Mask02_ST.z , _Mask02_ST.w));
				float2 Mask02_ST_ZW187 = appendResult186;
				#ifdef _MASK02_RADIAL2_ON
				float2 staticSwitch191 = ( ( ( (tex2D( _Sampler60188, ( appendResult10_g10 + appendResult24_g10 ) )).rg * 1.0 ) + ( _Mask02_ST.xy * appendResult58_g10 ) ) + Mask02_ST_ZW187 );
				#else
				float2 staticSwitch191 = uv_Mask02;
				#endif
				float cos34 = cos( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float sin34 = sin( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float2 rotator34 = mul( staticSwitch191 - float2( 0.5,0.5 ) , float2x2( cos34 , -sin34 , sin34 , cos34 )) + float2( 0.5,0.5 );
				float2 panner35 = ( 1.0 * _Time.y * _Mask02UV + rotator34);
				float temp_output_80_0 = ( pow( ChannelSelect( tex2D( _Mask02, panner35 ), _Mask02_Channel ) , _Mask02_Power ) * _Mask02_Value );
				#ifdef _MASKMUTUPYORADD_ON
				float staticSwitch249 = ( temp_output_77_0 * temp_output_80_0 );
				#else
				float staticSwitch249 = ( temp_output_77_0 + temp_output_80_0 );
				#endif
				#ifdef _USE_MASK02_ON
				float staticSwitch38 = staticSwitch249;
				#else
				float staticSwitch38 = temp_output_77_0;
				#endif
				float Mask55 = staticSwitch38;
				float temp_output_130_0 = ( 1.0 - _Dissolve_Soft );
				float2 uv_Dissolve = input.ase_texcoord.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float2 temp_output_1_0_g4 = float2( 1,1 );
				float2 texCoord80_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g4 = (float2(( (temp_output_1_0_g4).x * texCoord80_g4.x ) , ( texCoord80_g4.y * (temp_output_1_0_g4).y )));
				float2 temp_output_11_0_g4 = float2( 0,0 );
				float2 texCoord81_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g4 = ( ( (temp_output_11_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g4);
				float2 panner19_g4 = ( ( _TimeParameters.x * (temp_output_11_0_g4).y ) * float2( 0,1 ) + texCoord81_g4);
				float2 appendResult24_g4 = (float2((panner18_g4).x , (panner19_g4).y));
				float2 Dissolve_UV204 = _Dissolve_UV;
				float2 temp_output_47_0_g4 = Dissolve_UV204;
				float2 texCoord78_g4 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g4 = ( texCoord78_g4 - float2( 1,1 ) );
				float2 appendResult39_g4 = (float2(frac( ( atan2( (temp_output_31_0_g4).x , (temp_output_31_0_g4).y ) / TWO_PI ) ) , length( temp_output_31_0_g4 )));
				float2 panner54_g4 = ( ( (temp_output_47_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g4);
				float2 panner55_g4 = ( ( _TimeParameters.x * (temp_output_47_0_g4).y ) * float2( 0,1 ) + appendResult39_g4);
				float2 appendResult58_g4 = (float2((panner54_g4).x , (panner55_g4).y));
				float2 appendResult201 = (float2(_Dissolve_ST.z , _Dissolve_ST.w));
				float2 Dissolve_ST_ZW202 = appendResult201;
				#ifdef _DISSOLVE_RADIAL3_ON
				float2 staticSwitch196 = ( ( ( (tex2D( _Sampler60199, ( appendResult10_g4 + appendResult24_g4 ) )).rg * 1.0 ) + ( _Dissolve_ST.xy * appendResult58_g4 ) ) + Dissolve_ST_ZW202 );
				#else
				float2 staticSwitch196 = uv_Dissolve;
				#endif
				float DissolveRaodong145 = ChannelSelect( tex2DNode43, _Turbulence_Channel );
				float cos139 = cos( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float sin139 = sin( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float2 rotator139 = mul( ( staticSwitch196 + ( DissolveRaodong145 * _Dissolve_Raodong ) ) - float2( 0.5,0.5 ) , float2x2( cos139 , -sin139 , sin139 , cos139 )) + float2( 0.5,0.5 );
				float2 panner127 = ( 1.0 * _Time.y * _Dissolve_UV + rotator139);
				float2 uv_DissolveMask = input.ase_texcoord.xy * _DissolveMask_ST.xy + _DissolveMask_ST.zw;
				float cos153 = cos( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float sin153 = sin( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float2 rotator153 = mul( uv_DissolveMask - float2( 0.5,0.5 ) , float2x2( cos153 , -sin153 , sin153 , cos153 )) + float2( 0.5,0.5 );
				float2 panner150 = ( 1.0 * _Time.y * _DissolveMask_UV + rotator153);
				#ifdef _USE_DISSOLVEMASK_ON
				float staticSwitch227 = ( pow( ChannelSelect( tex2D( _DissolveMask, panner150 ), _DissolveMask_Channel ) , _DissolveMask_Power ) * _DissolveMask_Value );
				#else
				float staticSwitch227 = 1.0;
				#endif
				float temp_output_115_0 = ( ( 1.0 + ( ChannelSelect( tex2D( _Dissolve, panner127 ), _Dissolve_Channel ) * staticSwitch227 ) ) - ( 2.0 * _Dissolve_Value ) );
				float smoothstepResult116 = smoothstep( 0.0 , temp_output_130_0 , temp_output_115_0);
				float DissolveMask137 = saturate( smoothstepResult116 );
				#ifdef _USEDISSOLVE_ON
				float staticSwitch167 = DissolveMask137;
				#else
				float staticSwitch167 = 1.0;
				#endif
				


				float Alpha = ( input.ase_color.a * MT_Alpha53 * Mask55 * staticSwitch167 * _MainAlpha );
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _USETURBULENCE_ON
			#pragma shader_feature_local _MT_RADIAL_ON
			#pragma shader_feature_local _TURBULENCE_RADIAL_ON
			#pragma shader_feature_local _USE_TURBULENCEMASK_ON
			#pragma shader_feature_local _USE_MASK02_ON
			#pragma shader_feature_local _MASK_RADIAL1_ON
			#pragma shader_feature_local _MASKMUTUPYORADD_ON
			#pragma shader_feature_local _MASK02_RADIAL2_ON
			#pragma shader_feature_local _HUE_SHIFT_ON _SATURATION_ON _CONTRAST_ON _COLOR_ADJUST_ONLY_MAINTEX
			#pragma shader_feature_local _USEDISSOLVE_ON
			#pragma shader_feature_local _DISSOLVE_RADIAL3_ON
			#pragma shader_feature_local _USE_DISSOLVEMASK_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MT_Color02;
			float4 _MT_Color;
			float4 _Mask_ST;
			float4 _DissolveMask_ST;
			float4 _Dissolve_ST;
			float4 _Turbulence_Mask_ST;
			float4 _DissolveWidth_Color;
			float4 _Turbulence_ST;
			float4 _MT_ST;
			float4 _Ramp_ST;
			float4 _Mask02_ST;
			float4 _MT02_ST;
			float2 _Mask02UV;
			float2 _Turbulence_UV;
			float2 _MT_UV02;
			float2 _TurbulenceMask_UV;
			float2 _MT_UV;
			float2 _Dissolve_UV;
			float2 _RampUV;
			float2 _MaskUV;
			float2 _DissolveMask_UV;
			float _Dissolve_Value;
			float _DissolveMask_Value;
			float _CullMode;
			float _Mask_Rotato;
			float _Desaturate;
			float _Mask_Power;
			float _Mask_Value;
			float _Mask02_Rotato;
			float _Mask02_Power;
			float _DissolveWidth;
			float _DissolveMask_Power;
			float _Dissolve_Soft;
			float _Dissolve_Rotato;
			float _Dissolve_Raodong;
			float _Mask02_Value;
			float _MT_Alpha02;
			float _MT_Rotato02;
			float _MT_Alpha;
			float _Turbulence_Mask_Value;
			float _Turbulence_Mask_Power1;
			float _TurbulenceMask_Rotato;
			float _Turbulence_Rotato1;
			float _Turbulence_Value;
			float _MT_Rotato;
			float _Ramp_Rotato;
			float _dst;
			float _DissolveMask_Rotato1;
			float _MainAlpha;
			float _MT_Channel;
			float _MT02_Channel;
			float _Mask_Channel;
			float _Mask02_Channel;
			float _Turbulence_Channel;
			float _TurbulenceMask_Channel;
			float _Dissolve_Channel;
			float _DissolveMask_Channel;
			float _HueShift_Toggle;
			float _HueShift;
			float _ChangeSaturability_Toggle;
			float _Saturability;
			float _Contrast_Toggle;
			float _Contrast;
			float4 _ContrastMidColor;
			float _ColorAdjustmentOnlyAffectMainTex;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifndef NB_CHANNEL_HELPERS
			#define NB_CHANNEL_HELPERS
			float ChannelSelect( float4 c, float idx )
			{
				return idx < 0.5 ? c.r : ( idx < 1.5 ? c.g : ( idx < 2.5 ? c.b : c.a ) );
			}
			float3 ApplyColorAdjustment( float3 col, float hueShift, float saturability, float contrast, float3 contrastMid )
			{
				#if defined( _HUE_SHIFT_ON )
					half3 hsv = RgbToHsv( col );
					hsv.r += hueShift;
					col = HsvToRgb( hsv );
				#endif
				#if defined( _CONTRAST_ON )
					col = lerp( contrastMid, col, contrast );
				#endif
				#if defined( _SATURATION_ON )
					half3 wb = luminance( col );
					col = lerp( wb, col, saturability );
				#endif
				return col;
			}
			#endif

			sampler2D _MT;
			sampler2D _Sampler60169;
			sampler2D _Turbulence;
			sampler2D _Sampler60233;
			sampler2D _Turbulence_Mask;
			sampler2D _Mask;
			sampler2D _Sampler60180;
			sampler2D _Mask02;
			sampler2D _Sampler60188;
			sampler2D _Dissolve;
			sampler2D _Sampler60199;
			sampler2D _DissolveMask;


			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_color = input.ase_color;
				output.ase_texcoord.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
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
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_MT = input.ase_texcoord.xy * _MT_ST.xy + _MT_ST.zw;
				float2 temp_output_1_0_g12 = float2( 1,1 );
				float2 texCoord80_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g12 = (float2(( (temp_output_1_0_g12).x * texCoord80_g12.x ) , ( texCoord80_g12.y * (temp_output_1_0_g12).y )));
				float2 temp_output_11_0_g12 = float2( 0,0 );
				float2 texCoord81_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g12 = ( ( (temp_output_11_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g12);
				float2 panner19_g12 = ( ( _TimeParameters.x * (temp_output_11_0_g12).y ) * float2( 0,1 ) + texCoord81_g12);
				float2 appendResult24_g12 = (float2((panner18_g12).x , (panner19_g12).y));
				float2 MT_UV173 = _MT_UV;
				float2 temp_output_47_0_g12 = MT_UV173;
				float2 texCoord78_g12 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g12 = ( texCoord78_g12 - float2( 1,1 ) );
				float2 appendResult39_g12 = (float2(frac( ( atan2( (temp_output_31_0_g12).x , (temp_output_31_0_g12).y ) / TWO_PI ) ) , length( temp_output_31_0_g12 )));
				float2 panner54_g12 = ( ( (temp_output_47_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g12);
				float2 panner55_g12 = ( ( _TimeParameters.x * (temp_output_47_0_g12).y ) * float2( 0,1 ) + appendResult39_g12);
				float2 appendResult58_g12 = (float2((panner54_g12).x , (panner55_g12).y));
				float2 appendResult171 = (float2(_MT_ST.z , _MT_ST.w));
				float2 MT_ST_ZW172 = appendResult171;
				#ifdef _MT_RADIAL_ON
				float2 staticSwitch177 = ( ( ( (tex2D( _Sampler60169, ( appendResult10_g12 + appendResult24_g12 ) )).rg * 1.0 ) + ( _MT_ST.xy * appendResult58_g12 ) ) + MT_ST_ZW172 );
				#else
				float2 staticSwitch177 = uv_MT;
				#endif
				float cos5 = cos( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float sin5 = sin( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float2 rotator5 = mul( staticSwitch177 - float2( 0.5,0.5 ) , float2x2( cos5 , -sin5 , sin5 , cos5 )) + float2( 0.5,0.5 );
				float2 panner2 = ( 1.0 * _Time.y * _MT_UV + rotator5);
				float2 uv_Turbulence = input.ase_texcoord.xy * _Turbulence_ST.xy + _Turbulence_ST.zw;
				float2 temp_output_1_0_g9 = float2( 1,1 );
				float2 texCoord80_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g9 = (float2(( (temp_output_1_0_g9).x * texCoord80_g9.x ) , ( texCoord80_g9.y * (temp_output_1_0_g9).y )));
				float2 temp_output_11_0_g9 = float2( 0,0 );
				float2 texCoord81_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g9 = ( ( (temp_output_11_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g9);
				float2 panner19_g9 = ( ( _TimeParameters.x * (temp_output_11_0_g9).y ) * float2( 0,1 ) + texCoord81_g9);
				float2 appendResult24_g9 = (float2((panner18_g9).x , (panner19_g9).y));
				float2 TurbulenceUV237 = _Turbulence_UV;
				float2 temp_output_47_0_g9 = TurbulenceUV237;
				float2 texCoord78_g9 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g9 = ( texCoord78_g9 - float2( 1,1 ) );
				float2 appendResult39_g9 = (float2(frac( ( atan2( (temp_output_31_0_g9).x , (temp_output_31_0_g9).y ) / TWO_PI ) ) , length( temp_output_31_0_g9 )));
				float2 panner54_g9 = ( ( (temp_output_47_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g9);
				float2 panner55_g9 = ( ( _TimeParameters.x * (temp_output_47_0_g9).y ) * float2( 0,1 ) + appendResult39_g9);
				float2 appendResult58_g9 = (float2((panner54_g9).x , (panner55_g9).y));
				float2 appendResult229 = (float2(_Turbulence_ST.z , _Turbulence_ST.w));
				float2 Turbulence_ST_ZW230 = appendResult229;
				#ifdef _TURBULENCE_RADIAL_ON
				float2 staticSwitch236 = ( ( ( (tex2D( _Sampler60233, ( appendResult10_g9 + appendResult24_g9 ) )).rg * 1.0 ) + ( _Turbulence_ST.xy * appendResult58_g9 ) ) + Turbulence_ST_ZW230 );
				#else
				float2 staticSwitch236 = uv_Turbulence;
				#endif
				float cos224 = cos( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float sin224 = sin( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float2 rotator224 = mul( staticSwitch236 - float2( 0.5,0.5 ) , float2x2( cos224 , -sin224 , sin224 , cos224 )) + float2( 0.5,0.5 );
				float2 panner46 = ( 1.0 * _Time.y * _Turbulence_UV + rotator224);
				float4 tex2DNode43 = tex2D( _Turbulence, panner46 );
				float2 uv_Turbulence_Mask = input.ase_texcoord.xy * _Turbulence_Mask_ST.xy + _Turbulence_Mask_ST.zw;
				float cos57 = cos( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float sin57 = sin( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float2 rotator57 = mul( uv_Turbulence_Mask - float2( 0.5,0.5 ) , float2x2( cos57 , -sin57 , sin57 , cos57 )) + float2( 0.5,0.5 );
				float2 panner62 = ( 1.0 * _Time.y * _TurbulenceMask_UV + rotator57);
				#ifdef _USE_TURBULENCEMASK_ON
				float staticSwitch225 = ( pow( ChannelSelect( tex2D( _Turbulence_Mask, panner62 ), _TurbulenceMask_Channel ) , _Turbulence_Mask_Power1 ) * _Turbulence_Mask_Value );
				#else
				float staticSwitch225 = 1.0;
				#endif
				float Turbulence50 = ( _Turbulence_Value * ChannelSelect( tex2DNode43, _Turbulence_Channel ) * staticSwitch225 );
				#ifdef _USETURBULENCE_ON
				float2 staticSwitch64 = ( panner2 + Turbulence50 );
				#else
				float2 staticSwitch64 = panner2;
				#endif
				float4 tex2DNode1 = tex2D( _MT, staticSwitch64 );
				float MT_Alpha53 = ChannelSelect( tex2DNode1, _MT_Channel );
				float2 uv_Mask = input.ase_texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
				float2 temp_output_1_0_g11 = float2( 1,1 );
				float2 texCoord80_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g11 = (float2(( (temp_output_1_0_g11).x * texCoord80_g11.x ) , ( texCoord80_g11.y * (temp_output_1_0_g11).y )));
				float2 temp_output_11_0_g11 = float2( 0,0 );
				float2 texCoord81_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g11 = ( ( (temp_output_11_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g11);
				float2 panner19_g11 = ( ( _TimeParameters.x * (temp_output_11_0_g11).y ) * float2( 0,1 ) + texCoord81_g11);
				float2 appendResult24_g11 = (float2((panner18_g11).x , (panner19_g11).y));
				float2 Mask_UV194 = _MaskUV;
				float2 temp_output_47_0_g11 = Mask_UV194;
				float2 texCoord78_g11 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g11 = ( texCoord78_g11 - float2( 1,1 ) );
				float2 appendResult39_g11 = (float2(frac( ( atan2( (temp_output_31_0_g11).x , (temp_output_31_0_g11).y ) / TWO_PI ) ) , length( temp_output_31_0_g11 )));
				float2 panner54_g11 = ( ( (temp_output_47_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g11);
				float2 panner55_g11 = ( ( _TimeParameters.x * (temp_output_47_0_g11).y ) * float2( 0,1 ) + appendResult39_g11);
				float2 appendResult58_g11 = (float2((panner54_g11).x , (panner55_g11).y));
				float2 appendResult178 = (float2(_Mask_ST.z , _Mask_ST.w));
				float2 Mask_ST_ZW179 = appendResult178;
				#ifdef _MASK_RADIAL1_ON
				float2 staticSwitch183 = ( ( ( (tex2D( _Sampler60180, ( appendResult10_g11 + appendResult24_g11 ) )).rg * 1.0 ) + ( _Mask_ST.xy * appendResult58_g11 ) ) + Mask_ST_ZW179 );
				#else
				float2 staticSwitch183 = uv_Mask;
				#endif
				float cos11 = cos( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float sin11 = sin( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float2 rotator11 = mul( staticSwitch183 - float2( 0.5,0.5 ) , float2x2( cos11 , -sin11 , sin11 , cos11 )) + float2( 0.5,0.5 );
				float2 panner9 = ( 1.0 * _Time.y * _MaskUV + rotator11);
				float temp_output_77_0 = ( pow( ChannelSelect( tex2D( _Mask, panner9 ), _Mask_Channel ) , _Mask_Power ) * _Mask_Value );
				float2 uv_Mask02 = input.ase_texcoord.xy * _Mask02_ST.xy + _Mask02_ST.zw;
				float2 temp_output_1_0_g10 = float2( 1,1 );
				float2 texCoord80_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g10 = (float2(( (temp_output_1_0_g10).x * texCoord80_g10.x ) , ( texCoord80_g10.y * (temp_output_1_0_g10).y )));
				float2 temp_output_11_0_g10 = float2( 0,0 );
				float2 texCoord81_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g10 = ( ( (temp_output_11_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g10);
				float2 panner19_g10 = ( ( _TimeParameters.x * (temp_output_11_0_g10).y ) * float2( 0,1 ) + texCoord81_g10);
				float2 appendResult24_g10 = (float2((panner18_g10).x , (panner19_g10).y));
				float2 Mask02_UV195 = _Mask02UV;
				float2 temp_output_47_0_g10 = Mask02_UV195;
				float2 texCoord78_g10 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g10 = ( texCoord78_g10 - float2( 1,1 ) );
				float2 appendResult39_g10 = (float2(frac( ( atan2( (temp_output_31_0_g10).x , (temp_output_31_0_g10).y ) / TWO_PI ) ) , length( temp_output_31_0_g10 )));
				float2 panner54_g10 = ( ( (temp_output_47_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g10);
				float2 panner55_g10 = ( ( _TimeParameters.x * (temp_output_47_0_g10).y ) * float2( 0,1 ) + appendResult39_g10);
				float2 appendResult58_g10 = (float2((panner54_g10).x , (panner55_g10).y));
				float2 appendResult186 = (float2(_Mask02_ST.z , _Mask02_ST.w));
				float2 Mask02_ST_ZW187 = appendResult186;
				#ifdef _MASK02_RADIAL2_ON
				float2 staticSwitch191 = ( ( ( (tex2D( _Sampler60188, ( appendResult10_g10 + appendResult24_g10 ) )).rg * 1.0 ) + ( _Mask02_ST.xy * appendResult58_g10 ) ) + Mask02_ST_ZW187 );
				#else
				float2 staticSwitch191 = uv_Mask02;
				#endif
				float cos34 = cos( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float sin34 = sin( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float2 rotator34 = mul( staticSwitch191 - float2( 0.5,0.5 ) , float2x2( cos34 , -sin34 , sin34 , cos34 )) + float2( 0.5,0.5 );
				float2 panner35 = ( 1.0 * _Time.y * _Mask02UV + rotator34);
				float temp_output_80_0 = ( pow( ChannelSelect( tex2D( _Mask02, panner35 ), _Mask02_Channel ) , _Mask02_Power ) * _Mask02_Value );
				#ifdef _MASKMUTUPYORADD_ON
				float staticSwitch249 = ( temp_output_77_0 * temp_output_80_0 );
				#else
				float staticSwitch249 = ( temp_output_77_0 + temp_output_80_0 );
				#endif
				#ifdef _USE_MASK02_ON
				float staticSwitch38 = staticSwitch249;
				#else
				float staticSwitch38 = temp_output_77_0;
				#endif
				float Mask55 = staticSwitch38;
				float temp_output_130_0 = ( 1.0 - _Dissolve_Soft );
				float2 uv_Dissolve = input.ase_texcoord.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float2 temp_output_1_0_g4 = float2( 1,1 );
				float2 texCoord80_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g4 = (float2(( (temp_output_1_0_g4).x * texCoord80_g4.x ) , ( texCoord80_g4.y * (temp_output_1_0_g4).y )));
				float2 temp_output_11_0_g4 = float2( 0,0 );
				float2 texCoord81_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g4 = ( ( (temp_output_11_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g4);
				float2 panner19_g4 = ( ( _TimeParameters.x * (temp_output_11_0_g4).y ) * float2( 0,1 ) + texCoord81_g4);
				float2 appendResult24_g4 = (float2((panner18_g4).x , (panner19_g4).y));
				float2 Dissolve_UV204 = _Dissolve_UV;
				float2 temp_output_47_0_g4 = Dissolve_UV204;
				float2 texCoord78_g4 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g4 = ( texCoord78_g4 - float2( 1,1 ) );
				float2 appendResult39_g4 = (float2(frac( ( atan2( (temp_output_31_0_g4).x , (temp_output_31_0_g4).y ) / TWO_PI ) ) , length( temp_output_31_0_g4 )));
				float2 panner54_g4 = ( ( (temp_output_47_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g4);
				float2 panner55_g4 = ( ( _TimeParameters.x * (temp_output_47_0_g4).y ) * float2( 0,1 ) + appendResult39_g4);
				float2 appendResult58_g4 = (float2((panner54_g4).x , (panner55_g4).y));
				float2 appendResult201 = (float2(_Dissolve_ST.z , _Dissolve_ST.w));
				float2 Dissolve_ST_ZW202 = appendResult201;
				#ifdef _DISSOLVE_RADIAL3_ON
				float2 staticSwitch196 = ( ( ( (tex2D( _Sampler60199, ( appendResult10_g4 + appendResult24_g4 ) )).rg * 1.0 ) + ( _Dissolve_ST.xy * appendResult58_g4 ) ) + Dissolve_ST_ZW202 );
				#else
				float2 staticSwitch196 = uv_Dissolve;
				#endif
				float DissolveRaodong145 = ChannelSelect( tex2DNode43, _Turbulence_Channel );
				float cos139 = cos( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float sin139 = sin( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float2 rotator139 = mul( ( staticSwitch196 + ( DissolveRaodong145 * _Dissolve_Raodong ) ) - float2( 0.5,0.5 ) , float2x2( cos139 , -sin139 , sin139 , cos139 )) + float2( 0.5,0.5 );
				float2 panner127 = ( 1.0 * _Time.y * _Dissolve_UV + rotator139);
				float2 uv_DissolveMask = input.ase_texcoord.xy * _DissolveMask_ST.xy + _DissolveMask_ST.zw;
				float cos153 = cos( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float sin153 = sin( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float2 rotator153 = mul( uv_DissolveMask - float2( 0.5,0.5 ) , float2x2( cos153 , -sin153 , sin153 , cos153 )) + float2( 0.5,0.5 );
				float2 panner150 = ( 1.0 * _Time.y * _DissolveMask_UV + rotator153);
				#ifdef _USE_DISSOLVEMASK_ON
				float staticSwitch227 = ( pow( ChannelSelect( tex2D( _DissolveMask, panner150 ), _DissolveMask_Channel ) , _DissolveMask_Power ) * _DissolveMask_Value );
				#else
				float staticSwitch227 = 1.0;
				#endif
				float temp_output_115_0 = ( ( 1.0 + ( ChannelSelect( tex2D( _Dissolve, panner127 ), _Dissolve_Channel ) * staticSwitch227 ) ) - ( 2.0 * _Dissolve_Value ) );
				float smoothstepResult116 = smoothstep( 0.0 , temp_output_130_0 , temp_output_115_0);
				float DissolveMask137 = saturate( smoothstepResult116 );
				#ifdef _USEDISSOLVE_ON
				float staticSwitch167 = DissolveMask137;
				#else
				float staticSwitch167 = 1.0;
				#endif
				


				surfaceDescription.Alpha = ( input.ase_color.a * MT_Alpha53 * Mask55 * staticSwitch167 * _MainAlpha );
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _USETURBULENCE_ON
			#pragma shader_feature_local _MT_RADIAL_ON
			#pragma shader_feature_local _TURBULENCE_RADIAL_ON
			#pragma shader_feature_local _USE_TURBULENCEMASK_ON
			#pragma shader_feature_local _USE_MASK02_ON
			#pragma shader_feature_local _MASK_RADIAL1_ON
			#pragma shader_feature_local _MASKMUTUPYORADD_ON
			#pragma shader_feature_local _MASK02_RADIAL2_ON
			#pragma shader_feature_local _HUE_SHIFT_ON _SATURATION_ON _CONTRAST_ON _COLOR_ADJUST_ONLY_MAINTEX
			#pragma shader_feature_local _USEDISSOLVE_ON
			#pragma shader_feature_local _DISSOLVE_RADIAL3_ON
			#pragma shader_feature_local _USE_DISSOLVEMASK_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MT_Color02;
			float4 _MT_Color;
			float4 _Mask_ST;
			float4 _DissolveMask_ST;
			float4 _Dissolve_ST;
			float4 _Turbulence_Mask_ST;
			float4 _DissolveWidth_Color;
			float4 _Turbulence_ST;
			float4 _MT_ST;
			float4 _Ramp_ST;
			float4 _Mask02_ST;
			float4 _MT02_ST;
			float2 _Mask02UV;
			float2 _Turbulence_UV;
			float2 _MT_UV02;
			float2 _TurbulenceMask_UV;
			float2 _MT_UV;
			float2 _Dissolve_UV;
			float2 _RampUV;
			float2 _MaskUV;
			float2 _DissolveMask_UV;
			float _Dissolve_Value;
			float _DissolveMask_Value;
			float _CullMode;
			float _Mask_Rotato;
			float _Desaturate;
			float _Mask_Power;
			float _Mask_Value;
			float _Mask02_Rotato;
			float _Mask02_Power;
			float _DissolveWidth;
			float _DissolveMask_Power;
			float _Dissolve_Soft;
			float _Dissolve_Rotato;
			float _Dissolve_Raodong;
			float _Mask02_Value;
			float _MT_Alpha02;
			float _MT_Rotato02;
			float _MT_Alpha;
			float _Turbulence_Mask_Value;
			float _Turbulence_Mask_Power1;
			float _TurbulenceMask_Rotato;
			float _Turbulence_Rotato1;
			float _Turbulence_Value;
			float _MT_Rotato;
			float _Ramp_Rotato;
			float _dst;
			float _DissolveMask_Rotato1;
			float _MainAlpha;
			float _MT_Channel;
			float _MT02_Channel;
			float _Mask_Channel;
			float _Mask02_Channel;
			float _Turbulence_Channel;
			float _TurbulenceMask_Channel;
			float _Dissolve_Channel;
			float _DissolveMask_Channel;
			float _HueShift_Toggle;
			float _HueShift;
			float _ChangeSaturability_Toggle;
			float _Saturability;
			float _Contrast_Toggle;
			float _Contrast;
			float4 _ContrastMidColor;
			float _ColorAdjustmentOnlyAffectMainTex;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifndef NB_CHANNEL_HELPERS
			#define NB_CHANNEL_HELPERS
			float ChannelSelect( float4 c, float idx )
			{
				return idx < 0.5 ? c.r : ( idx < 1.5 ? c.g : ( idx < 2.5 ? c.b : c.a ) );
			}
			float3 ApplyColorAdjustment( float3 col, float hueShift, float saturability, float contrast, float3 contrastMid )
			{
				#if defined( _HUE_SHIFT_ON )
					half3 hsv = RgbToHsv( col );
					hsv.r += hueShift;
					col = HsvToRgb( hsv );
				#endif
				#if defined( _CONTRAST_ON )
					col = lerp( contrastMid, col, contrast );
				#endif
				#if defined( _SATURATION_ON )
					half3 wb = luminance( col );
					col = lerp( wb, col, saturability );
				#endif
				return col;
			}
			#endif

			sampler2D _MT;
			sampler2D _Sampler60169;
			sampler2D _Turbulence;
			sampler2D _Sampler60233;
			sampler2D _Turbulence_Mask;
			sampler2D _Mask;
			sampler2D _Sampler60180;
			sampler2D _Mask02;
			sampler2D _Sampler60188;
			sampler2D _Dissolve;
			sampler2D _Sampler60199;
			sampler2D _DissolveMask;


			
			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_color = input.ase_color;
				output.ase_texcoord.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
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
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_MT = input.ase_texcoord.xy * _MT_ST.xy + _MT_ST.zw;
				float2 temp_output_1_0_g12 = float2( 1,1 );
				float2 texCoord80_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g12 = (float2(( (temp_output_1_0_g12).x * texCoord80_g12.x ) , ( texCoord80_g12.y * (temp_output_1_0_g12).y )));
				float2 temp_output_11_0_g12 = float2( 0,0 );
				float2 texCoord81_g12 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g12 = ( ( (temp_output_11_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g12);
				float2 panner19_g12 = ( ( _TimeParameters.x * (temp_output_11_0_g12).y ) * float2( 0,1 ) + texCoord81_g12);
				float2 appendResult24_g12 = (float2((panner18_g12).x , (panner19_g12).y));
				float2 MT_UV173 = _MT_UV;
				float2 temp_output_47_0_g12 = MT_UV173;
				float2 texCoord78_g12 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g12 = ( texCoord78_g12 - float2( 1,1 ) );
				float2 appendResult39_g12 = (float2(frac( ( atan2( (temp_output_31_0_g12).x , (temp_output_31_0_g12).y ) / TWO_PI ) ) , length( temp_output_31_0_g12 )));
				float2 panner54_g12 = ( ( (temp_output_47_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g12);
				float2 panner55_g12 = ( ( _TimeParameters.x * (temp_output_47_0_g12).y ) * float2( 0,1 ) + appendResult39_g12);
				float2 appendResult58_g12 = (float2((panner54_g12).x , (panner55_g12).y));
				float2 appendResult171 = (float2(_MT_ST.z , _MT_ST.w));
				float2 MT_ST_ZW172 = appendResult171;
				#ifdef _MT_RADIAL_ON
				float2 staticSwitch177 = ( ( ( (tex2D( _Sampler60169, ( appendResult10_g12 + appendResult24_g12 ) )).rg * 1.0 ) + ( _MT_ST.xy * appendResult58_g12 ) ) + MT_ST_ZW172 );
				#else
				float2 staticSwitch177 = uv_MT;
				#endif
				float cos5 = cos( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float sin5 = sin( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float2 rotator5 = mul( staticSwitch177 - float2( 0.5,0.5 ) , float2x2( cos5 , -sin5 , sin5 , cos5 )) + float2( 0.5,0.5 );
				float2 panner2 = ( 1.0 * _Time.y * _MT_UV + rotator5);
				float2 uv_Turbulence = input.ase_texcoord.xy * _Turbulence_ST.xy + _Turbulence_ST.zw;
				float2 temp_output_1_0_g9 = float2( 1,1 );
				float2 texCoord80_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g9 = (float2(( (temp_output_1_0_g9).x * texCoord80_g9.x ) , ( texCoord80_g9.y * (temp_output_1_0_g9).y )));
				float2 temp_output_11_0_g9 = float2( 0,0 );
				float2 texCoord81_g9 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g9 = ( ( (temp_output_11_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g9);
				float2 panner19_g9 = ( ( _TimeParameters.x * (temp_output_11_0_g9).y ) * float2( 0,1 ) + texCoord81_g9);
				float2 appendResult24_g9 = (float2((panner18_g9).x , (panner19_g9).y));
				float2 TurbulenceUV237 = _Turbulence_UV;
				float2 temp_output_47_0_g9 = TurbulenceUV237;
				float2 texCoord78_g9 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g9 = ( texCoord78_g9 - float2( 1,1 ) );
				float2 appendResult39_g9 = (float2(frac( ( atan2( (temp_output_31_0_g9).x , (temp_output_31_0_g9).y ) / TWO_PI ) ) , length( temp_output_31_0_g9 )));
				float2 panner54_g9 = ( ( (temp_output_47_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g9);
				float2 panner55_g9 = ( ( _TimeParameters.x * (temp_output_47_0_g9).y ) * float2( 0,1 ) + appendResult39_g9);
				float2 appendResult58_g9 = (float2((panner54_g9).x , (panner55_g9).y));
				float2 appendResult229 = (float2(_Turbulence_ST.z , _Turbulence_ST.w));
				float2 Turbulence_ST_ZW230 = appendResult229;
				#ifdef _TURBULENCE_RADIAL_ON
				float2 staticSwitch236 = ( ( ( (tex2D( _Sampler60233, ( appendResult10_g9 + appendResult24_g9 ) )).rg * 1.0 ) + ( _Turbulence_ST.xy * appendResult58_g9 ) ) + Turbulence_ST_ZW230 );
				#else
				float2 staticSwitch236 = uv_Turbulence;
				#endif
				float cos224 = cos( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float sin224 = sin( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float2 rotator224 = mul( staticSwitch236 - float2( 0.5,0.5 ) , float2x2( cos224 , -sin224 , sin224 , cos224 )) + float2( 0.5,0.5 );
				float2 panner46 = ( 1.0 * _Time.y * _Turbulence_UV + rotator224);
				float4 tex2DNode43 = tex2D( _Turbulence, panner46 );
				float2 uv_Turbulence_Mask = input.ase_texcoord.xy * _Turbulence_Mask_ST.xy + _Turbulence_Mask_ST.zw;
				float cos57 = cos( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float sin57 = sin( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float2 rotator57 = mul( uv_Turbulence_Mask - float2( 0.5,0.5 ) , float2x2( cos57 , -sin57 , sin57 , cos57 )) + float2( 0.5,0.5 );
				float2 panner62 = ( 1.0 * _Time.y * _TurbulenceMask_UV + rotator57);
				#ifdef _USE_TURBULENCEMASK_ON
				float staticSwitch225 = ( pow( ChannelSelect( tex2D( _Turbulence_Mask, panner62 ), _TurbulenceMask_Channel ) , _Turbulence_Mask_Power1 ) * _Turbulence_Mask_Value );
				#else
				float staticSwitch225 = 1.0;
				#endif
				float Turbulence50 = ( _Turbulence_Value * ChannelSelect( tex2DNode43, _Turbulence_Channel ) * staticSwitch225 );
				#ifdef _USETURBULENCE_ON
				float2 staticSwitch64 = ( panner2 + Turbulence50 );
				#else
				float2 staticSwitch64 = panner2;
				#endif
				float4 tex2DNode1 = tex2D( _MT, staticSwitch64 );
				float MT_Alpha53 = ChannelSelect( tex2DNode1, _MT_Channel );
				float2 uv_Mask = input.ase_texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
				float2 temp_output_1_0_g11 = float2( 1,1 );
				float2 texCoord80_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g11 = (float2(( (temp_output_1_0_g11).x * texCoord80_g11.x ) , ( texCoord80_g11.y * (temp_output_1_0_g11).y )));
				float2 temp_output_11_0_g11 = float2( 0,0 );
				float2 texCoord81_g11 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g11 = ( ( (temp_output_11_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g11);
				float2 panner19_g11 = ( ( _TimeParameters.x * (temp_output_11_0_g11).y ) * float2( 0,1 ) + texCoord81_g11);
				float2 appendResult24_g11 = (float2((panner18_g11).x , (panner19_g11).y));
				float2 Mask_UV194 = _MaskUV;
				float2 temp_output_47_0_g11 = Mask_UV194;
				float2 texCoord78_g11 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g11 = ( texCoord78_g11 - float2( 1,1 ) );
				float2 appendResult39_g11 = (float2(frac( ( atan2( (temp_output_31_0_g11).x , (temp_output_31_0_g11).y ) / TWO_PI ) ) , length( temp_output_31_0_g11 )));
				float2 panner54_g11 = ( ( (temp_output_47_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g11);
				float2 panner55_g11 = ( ( _TimeParameters.x * (temp_output_47_0_g11).y ) * float2( 0,1 ) + appendResult39_g11);
				float2 appendResult58_g11 = (float2((panner54_g11).x , (panner55_g11).y));
				float2 appendResult178 = (float2(_Mask_ST.z , _Mask_ST.w));
				float2 Mask_ST_ZW179 = appendResult178;
				#ifdef _MASK_RADIAL1_ON
				float2 staticSwitch183 = ( ( ( (tex2D( _Sampler60180, ( appendResult10_g11 + appendResult24_g11 ) )).rg * 1.0 ) + ( _Mask_ST.xy * appendResult58_g11 ) ) + Mask_ST_ZW179 );
				#else
				float2 staticSwitch183 = uv_Mask;
				#endif
				float cos11 = cos( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float sin11 = sin( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float2 rotator11 = mul( staticSwitch183 - float2( 0.5,0.5 ) , float2x2( cos11 , -sin11 , sin11 , cos11 )) + float2( 0.5,0.5 );
				float2 panner9 = ( 1.0 * _Time.y * _MaskUV + rotator11);
				float temp_output_77_0 = ( pow( ChannelSelect( tex2D( _Mask, panner9 ), _Mask_Channel ) , _Mask_Power ) * _Mask_Value );
				float2 uv_Mask02 = input.ase_texcoord.xy * _Mask02_ST.xy + _Mask02_ST.zw;
				float2 temp_output_1_0_g10 = float2( 1,1 );
				float2 texCoord80_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g10 = (float2(( (temp_output_1_0_g10).x * texCoord80_g10.x ) , ( texCoord80_g10.y * (temp_output_1_0_g10).y )));
				float2 temp_output_11_0_g10 = float2( 0,0 );
				float2 texCoord81_g10 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g10 = ( ( (temp_output_11_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g10);
				float2 panner19_g10 = ( ( _TimeParameters.x * (temp_output_11_0_g10).y ) * float2( 0,1 ) + texCoord81_g10);
				float2 appendResult24_g10 = (float2((panner18_g10).x , (panner19_g10).y));
				float2 Mask02_UV195 = _Mask02UV;
				float2 temp_output_47_0_g10 = Mask02_UV195;
				float2 texCoord78_g10 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g10 = ( texCoord78_g10 - float2( 1,1 ) );
				float2 appendResult39_g10 = (float2(frac( ( atan2( (temp_output_31_0_g10).x , (temp_output_31_0_g10).y ) / TWO_PI ) ) , length( temp_output_31_0_g10 )));
				float2 panner54_g10 = ( ( (temp_output_47_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g10);
				float2 panner55_g10 = ( ( _TimeParameters.x * (temp_output_47_0_g10).y ) * float2( 0,1 ) + appendResult39_g10);
				float2 appendResult58_g10 = (float2((panner54_g10).x , (panner55_g10).y));
				float2 appendResult186 = (float2(_Mask02_ST.z , _Mask02_ST.w));
				float2 Mask02_ST_ZW187 = appendResult186;
				#ifdef _MASK02_RADIAL2_ON
				float2 staticSwitch191 = ( ( ( (tex2D( _Sampler60188, ( appendResult10_g10 + appendResult24_g10 ) )).rg * 1.0 ) + ( _Mask02_ST.xy * appendResult58_g10 ) ) + Mask02_ST_ZW187 );
				#else
				float2 staticSwitch191 = uv_Mask02;
				#endif
				float cos34 = cos( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float sin34 = sin( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float2 rotator34 = mul( staticSwitch191 - float2( 0.5,0.5 ) , float2x2( cos34 , -sin34 , sin34 , cos34 )) + float2( 0.5,0.5 );
				float2 panner35 = ( 1.0 * _Time.y * _Mask02UV + rotator34);
				float temp_output_80_0 = ( pow( ChannelSelect( tex2D( _Mask02, panner35 ), _Mask02_Channel ) , _Mask02_Power ) * _Mask02_Value );
				#ifdef _MASKMUTUPYORADD_ON
				float staticSwitch249 = ( temp_output_77_0 * temp_output_80_0 );
				#else
				float staticSwitch249 = ( temp_output_77_0 + temp_output_80_0 );
				#endif
				#ifdef _USE_MASK02_ON
				float staticSwitch38 = staticSwitch249;
				#else
				float staticSwitch38 = temp_output_77_0;
				#endif
				float Mask55 = staticSwitch38;
				float temp_output_130_0 = ( 1.0 - _Dissolve_Soft );
				float2 uv_Dissolve = input.ase_texcoord.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float2 temp_output_1_0_g4 = float2( 1,1 );
				float2 texCoord80_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g4 = (float2(( (temp_output_1_0_g4).x * texCoord80_g4.x ) , ( texCoord80_g4.y * (temp_output_1_0_g4).y )));
				float2 temp_output_11_0_g4 = float2( 0,0 );
				float2 texCoord81_g4 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g4 = ( ( (temp_output_11_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g4);
				float2 panner19_g4 = ( ( _TimeParameters.x * (temp_output_11_0_g4).y ) * float2( 0,1 ) + texCoord81_g4);
				float2 appendResult24_g4 = (float2((panner18_g4).x , (panner19_g4).y));
				float2 Dissolve_UV204 = _Dissolve_UV;
				float2 temp_output_47_0_g4 = Dissolve_UV204;
				float2 texCoord78_g4 = input.ase_texcoord.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g4 = ( texCoord78_g4 - float2( 1,1 ) );
				float2 appendResult39_g4 = (float2(frac( ( atan2( (temp_output_31_0_g4).x , (temp_output_31_0_g4).y ) / TWO_PI ) ) , length( temp_output_31_0_g4 )));
				float2 panner54_g4 = ( ( (temp_output_47_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g4);
				float2 panner55_g4 = ( ( _TimeParameters.x * (temp_output_47_0_g4).y ) * float2( 0,1 ) + appendResult39_g4);
				float2 appendResult58_g4 = (float2((panner54_g4).x , (panner55_g4).y));
				float2 appendResult201 = (float2(_Dissolve_ST.z , _Dissolve_ST.w));
				float2 Dissolve_ST_ZW202 = appendResult201;
				#ifdef _DISSOLVE_RADIAL3_ON
				float2 staticSwitch196 = ( ( ( (tex2D( _Sampler60199, ( appendResult10_g4 + appendResult24_g4 ) )).rg * 1.0 ) + ( _Dissolve_ST.xy * appendResult58_g4 ) ) + Dissolve_ST_ZW202 );
				#else
				float2 staticSwitch196 = uv_Dissolve;
				#endif
				float DissolveRaodong145 = ChannelSelect( tex2DNode43, _Turbulence_Channel );
				float cos139 = cos( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float sin139 = sin( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float2 rotator139 = mul( ( staticSwitch196 + ( DissolveRaodong145 * _Dissolve_Raodong ) ) - float2( 0.5,0.5 ) , float2x2( cos139 , -sin139 , sin139 , cos139 )) + float2( 0.5,0.5 );
				float2 panner127 = ( 1.0 * _Time.y * _Dissolve_UV + rotator139);
				float2 uv_DissolveMask = input.ase_texcoord.xy * _DissolveMask_ST.xy + _DissolveMask_ST.zw;
				float cos153 = cos( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float sin153 = sin( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float2 rotator153 = mul( uv_DissolveMask - float2( 0.5,0.5 ) , float2x2( cos153 , -sin153 , sin153 , cos153 )) + float2( 0.5,0.5 );
				float2 panner150 = ( 1.0 * _Time.y * _DissolveMask_UV + rotator153);
				#ifdef _USE_DISSOLVEMASK_ON
				float staticSwitch227 = ( pow( ChannelSelect( tex2D( _DissolveMask, panner150 ), _DissolveMask_Channel ) , _DissolveMask_Power ) * _DissolveMask_Value );
				#else
				float staticSwitch227 = 1.0;
				#endif
				float temp_output_115_0 = ( ( 1.0 + ( ChannelSelect( tex2D( _Dissolve, panner127 ), _Dissolve_Channel ) * staticSwitch227 ) ) - ( 2.0 * _Dissolve_Value ) );
				float smoothstepResult116 = smoothstep( 0.0 , temp_output_130_0 , temp_output_115_0);
				float DissolveMask137 = saturate( smoothstepResult116 );
				#ifdef _USEDISSOLVE_ON
				float staticSwitch167 = DissolveMask137;
				#else
				float staticSwitch167 = 1.0;
				#endif
				


				surfaceDescription.Alpha = ( input.ase_color.a * MT_Alpha53 * Mask55 * staticSwitch167 * _MainAlpha );
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = unity_SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			

        	#define _NORMAL_DROPOFF_TS 1
        	#pragma multi_compile_instancing
        	#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
        	#define ASE_FOG 1
        	#define _SURFACE_TYPE_TRANSPARENT 1
        	#define ASE_VERSION 19912
        	#define ASE_SRP_VERSION 140011


			

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS
			#define VARYINGS_NEED_TANGENT_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140009
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _USETURBULENCE_ON
			#pragma shader_feature_local _MT_RADIAL_ON
			#pragma shader_feature_local _TURBULENCE_RADIAL_ON
			#pragma shader_feature_local _USE_TURBULENCEMASK_ON
			#pragma shader_feature_local _USE_MASK02_ON
			#pragma shader_feature_local _MASK_RADIAL1_ON
			#pragma shader_feature_local _MASKMUTUPYORADD_ON
			#pragma shader_feature_local _MASK02_RADIAL2_ON
			#pragma shader_feature_local _HUE_SHIFT_ON _SATURATION_ON _CONTRAST_ON _COLOR_ADJUST_ONLY_MAINTEX
			#pragma shader_feature_local _USEDISSOLVE_ON
			#pragma shader_feature_local _DISSOLVE_RADIAL3_ON
			#pragma shader_feature_local _USE_DISSOLVEMASK_ON


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				half3 normalWS : TEXCOORD0;
				float4 tangentWS : TEXCOORD1;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MT_Color02;
			float4 _MT_Color;
			float4 _Mask_ST;
			float4 _DissolveMask_ST;
			float4 _Dissolve_ST;
			float4 _Turbulence_Mask_ST;
			float4 _DissolveWidth_Color;
			float4 _Turbulence_ST;
			float4 _MT_ST;
			float4 _Ramp_ST;
			float4 _Mask02_ST;
			float4 _MT02_ST;
			float2 _Mask02UV;
			float2 _Turbulence_UV;
			float2 _MT_UV02;
			float2 _TurbulenceMask_UV;
			float2 _MT_UV;
			float2 _Dissolve_UV;
			float2 _RampUV;
			float2 _MaskUV;
			float2 _DissolveMask_UV;
			float _Dissolve_Value;
			float _DissolveMask_Value;
			float _CullMode;
			float _Mask_Rotato;
			float _Desaturate;
			float _Mask_Power;
			float _Mask_Value;
			float _Mask02_Rotato;
			float _Mask02_Power;
			float _DissolveWidth;
			float _DissolveMask_Power;
			float _Dissolve_Soft;
			float _Dissolve_Rotato;
			float _Dissolve_Raodong;
			float _Mask02_Value;
			float _MT_Alpha02;
			float _MT_Rotato02;
			float _MT_Alpha;
			float _Turbulence_Mask_Value;
			float _Turbulence_Mask_Power1;
			float _TurbulenceMask_Rotato;
			float _Turbulence_Rotato1;
			float _Turbulence_Value;
			float _MT_Rotato;
			float _Ramp_Rotato;
			float _dst;
			float _DissolveMask_Rotato1;
			float _MainAlpha;
			float _MT_Channel;
			float _MT02_Channel;
			float _Mask_Channel;
			float _Mask02_Channel;
			float _Turbulence_Channel;
			float _TurbulenceMask_Channel;
			float _Dissolve_Channel;
			float _DissolveMask_Channel;
			float _HueShift_Toggle;
			float _HueShift;
			float _ChangeSaturability_Toggle;
			float _Saturability;
			float _Contrast_Toggle;
			float _Contrast;
			float4 _ContrastMidColor;
			float _ColorAdjustmentOnlyAffectMainTex;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifndef NB_CHANNEL_HELPERS
			#define NB_CHANNEL_HELPERS
			float ChannelSelect( float4 c, float idx )
			{
				return idx < 0.5 ? c.r : ( idx < 1.5 ? c.g : ( idx < 2.5 ? c.b : c.a ) );
			}
			float3 ApplyColorAdjustment( float3 col, float hueShift, float saturability, float contrast, float3 contrastMid )
			{
				#if defined( _HUE_SHIFT_ON )
					half3 hsv = RgbToHsv( col );
					hsv.r += hueShift;
					col = HsvToRgb( hsv );
				#endif
				#if defined( _CONTRAST_ON )
					col = lerp( contrastMid, col, contrast );
				#endif
				#if defined( _SATURATION_ON )
					half3 wb = luminance( col );
					col = lerp( wb, col, saturability );
				#endif
				return col;
			}
			#endif

			sampler2D _MT;
			sampler2D _Sampler60169;
			sampler2D _Turbulence;
			sampler2D _Sampler60233;
			sampler2D _Turbulence_Mask;
			sampler2D _Mask;
			sampler2D _Sampler60180;
			sampler2D _Mask02;
			sampler2D _Sampler60188;
			sampler2D _Dissolve;
			sampler2D _Sampler60199;
			sampler2D _DissolveMask;


			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_color = input.ase_color;
				output.ase_texcoord2.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_color = input.ase_color;
				output.ase_texcoord = input.ase_texcoord;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
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
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			void frag(PackedVaryings input
						, out half4 outNormalWS : SV_Target0
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 NormalWS = input.normalWS * renormFactor;
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				float2 uv_MT = input.ase_texcoord2.xy * _MT_ST.xy + _MT_ST.zw;
				float2 temp_output_1_0_g12 = float2( 1,1 );
				float2 texCoord80_g12 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g12 = (float2(( (temp_output_1_0_g12).x * texCoord80_g12.x ) , ( texCoord80_g12.y * (temp_output_1_0_g12).y )));
				float2 temp_output_11_0_g12 = float2( 0,0 );
				float2 texCoord81_g12 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g12 = ( ( (temp_output_11_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g12);
				float2 panner19_g12 = ( ( _TimeParameters.x * (temp_output_11_0_g12).y ) * float2( 0,1 ) + texCoord81_g12);
				float2 appendResult24_g12 = (float2((panner18_g12).x , (panner19_g12).y));
				float2 MT_UV173 = _MT_UV;
				float2 temp_output_47_0_g12 = MT_UV173;
				float2 texCoord78_g12 = input.ase_texcoord2.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g12 = ( texCoord78_g12 - float2( 1,1 ) );
				float2 appendResult39_g12 = (float2(frac( ( atan2( (temp_output_31_0_g12).x , (temp_output_31_0_g12).y ) / TWO_PI ) ) , length( temp_output_31_0_g12 )));
				float2 panner54_g12 = ( ( (temp_output_47_0_g12).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g12);
				float2 panner55_g12 = ( ( _TimeParameters.x * (temp_output_47_0_g12).y ) * float2( 0,1 ) + appendResult39_g12);
				float2 appendResult58_g12 = (float2((panner54_g12).x , (panner55_g12).y));
				float2 appendResult171 = (float2(_MT_ST.z , _MT_ST.w));
				float2 MT_ST_ZW172 = appendResult171;
				#ifdef _MT_RADIAL_ON
				float2 staticSwitch177 = ( ( ( (tex2D( _Sampler60169, ( appendResult10_g12 + appendResult24_g12 ) )).rg * 1.0 ) + ( _MT_ST.xy * appendResult58_g12 ) ) + MT_ST_ZW172 );
				#else
				float2 staticSwitch177 = uv_MT;
				#endif
				float cos5 = cos( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float sin5 = sin( ( ( _MT_Rotato * PI ) * 2.0 ) );
				float2 rotator5 = mul( staticSwitch177 - float2( 0.5,0.5 ) , float2x2( cos5 , -sin5 , sin5 , cos5 )) + float2( 0.5,0.5 );
				float2 panner2 = ( 1.0 * _Time.y * _MT_UV + rotator5);
				float2 uv_Turbulence = input.ase_texcoord2.xy * _Turbulence_ST.xy + _Turbulence_ST.zw;
				float2 temp_output_1_0_g9 = float2( 1,1 );
				float2 texCoord80_g9 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g9 = (float2(( (temp_output_1_0_g9).x * texCoord80_g9.x ) , ( texCoord80_g9.y * (temp_output_1_0_g9).y )));
				float2 temp_output_11_0_g9 = float2( 0,0 );
				float2 texCoord81_g9 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g9 = ( ( (temp_output_11_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g9);
				float2 panner19_g9 = ( ( _TimeParameters.x * (temp_output_11_0_g9).y ) * float2( 0,1 ) + texCoord81_g9);
				float2 appendResult24_g9 = (float2((panner18_g9).x , (panner19_g9).y));
				float2 TurbulenceUV237 = _Turbulence_UV;
				float2 temp_output_47_0_g9 = TurbulenceUV237;
				float2 texCoord78_g9 = input.ase_texcoord2.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g9 = ( texCoord78_g9 - float2( 1,1 ) );
				float2 appendResult39_g9 = (float2(frac( ( atan2( (temp_output_31_0_g9).x , (temp_output_31_0_g9).y ) / TWO_PI ) ) , length( temp_output_31_0_g9 )));
				float2 panner54_g9 = ( ( (temp_output_47_0_g9).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g9);
				float2 panner55_g9 = ( ( _TimeParameters.x * (temp_output_47_0_g9).y ) * float2( 0,1 ) + appendResult39_g9);
				float2 appendResult58_g9 = (float2((panner54_g9).x , (panner55_g9).y));
				float2 appendResult229 = (float2(_Turbulence_ST.z , _Turbulence_ST.w));
				float2 Turbulence_ST_ZW230 = appendResult229;
				#ifdef _TURBULENCE_RADIAL_ON
				float2 staticSwitch236 = ( ( ( (tex2D( _Sampler60233, ( appendResult10_g9 + appendResult24_g9 ) )).rg * 1.0 ) + ( _Turbulence_ST.xy * appendResult58_g9 ) ) + Turbulence_ST_ZW230 );
				#else
				float2 staticSwitch236 = uv_Turbulence;
				#endif
				float cos224 = cos( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float sin224 = sin( ( ( _Turbulence_Rotato1 * PI ) * 2.0 ) );
				float2 rotator224 = mul( staticSwitch236 - float2( 0.5,0.5 ) , float2x2( cos224 , -sin224 , sin224 , cos224 )) + float2( 0.5,0.5 );
				float2 panner46 = ( 1.0 * _Time.y * _Turbulence_UV + rotator224);
				float4 tex2DNode43 = tex2D( _Turbulence, panner46 );
				float2 uv_Turbulence_Mask = input.ase_texcoord2.xy * _Turbulence_Mask_ST.xy + _Turbulence_Mask_ST.zw;
				float cos57 = cos( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float sin57 = sin( ( ( _TurbulenceMask_Rotato * PI ) * 2.0 ) );
				float2 rotator57 = mul( uv_Turbulence_Mask - float2( 0.5,0.5 ) , float2x2( cos57 , -sin57 , sin57 , cos57 )) + float2( 0.5,0.5 );
				float2 panner62 = ( 1.0 * _Time.y * _TurbulenceMask_UV + rotator57);
				#ifdef _USE_TURBULENCEMASK_ON
				float staticSwitch225 = ( pow( ChannelSelect( tex2D( _Turbulence_Mask, panner62 ), _TurbulenceMask_Channel ) , _Turbulence_Mask_Power1 ) * _Turbulence_Mask_Value );
				#else
				float staticSwitch225 = 1.0;
				#endif
				float Turbulence50 = ( _Turbulence_Value * ChannelSelect( tex2DNode43, _Turbulence_Channel ) * staticSwitch225 );
				#ifdef _USETURBULENCE_ON
				float2 staticSwitch64 = ( panner2 + Turbulence50 );
				#else
				float2 staticSwitch64 = panner2;
				#endif
				float4 tex2DNode1 = tex2D( _MT, staticSwitch64 );
				float MT_Alpha53 = ChannelSelect( tex2DNode1, _MT_Channel );
				float2 uv_Mask = input.ase_texcoord2.xy * _Mask_ST.xy + _Mask_ST.zw;
				float2 temp_output_1_0_g11 = float2( 1,1 );
				float2 texCoord80_g11 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g11 = (float2(( (temp_output_1_0_g11).x * texCoord80_g11.x ) , ( texCoord80_g11.y * (temp_output_1_0_g11).y )));
				float2 temp_output_11_0_g11 = float2( 0,0 );
				float2 texCoord81_g11 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g11 = ( ( (temp_output_11_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g11);
				float2 panner19_g11 = ( ( _TimeParameters.x * (temp_output_11_0_g11).y ) * float2( 0,1 ) + texCoord81_g11);
				float2 appendResult24_g11 = (float2((panner18_g11).x , (panner19_g11).y));
				float2 Mask_UV194 = _MaskUV;
				float2 temp_output_47_0_g11 = Mask_UV194;
				float2 texCoord78_g11 = input.ase_texcoord2.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g11 = ( texCoord78_g11 - float2( 1,1 ) );
				float2 appendResult39_g11 = (float2(frac( ( atan2( (temp_output_31_0_g11).x , (temp_output_31_0_g11).y ) / TWO_PI ) ) , length( temp_output_31_0_g11 )));
				float2 panner54_g11 = ( ( (temp_output_47_0_g11).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g11);
				float2 panner55_g11 = ( ( _TimeParameters.x * (temp_output_47_0_g11).y ) * float2( 0,1 ) + appendResult39_g11);
				float2 appendResult58_g11 = (float2((panner54_g11).x , (panner55_g11).y));
				float2 appendResult178 = (float2(_Mask_ST.z , _Mask_ST.w));
				float2 Mask_ST_ZW179 = appendResult178;
				#ifdef _MASK_RADIAL1_ON
				float2 staticSwitch183 = ( ( ( (tex2D( _Sampler60180, ( appendResult10_g11 + appendResult24_g11 ) )).rg * 1.0 ) + ( _Mask_ST.xy * appendResult58_g11 ) ) + Mask_ST_ZW179 );
				#else
				float2 staticSwitch183 = uv_Mask;
				#endif
				float cos11 = cos( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float sin11 = sin( ( ( _Mask_Rotato * PI ) * 2.0 ) );
				float2 rotator11 = mul( staticSwitch183 - float2( 0.5,0.5 ) , float2x2( cos11 , -sin11 , sin11 , cos11 )) + float2( 0.5,0.5 );
				float2 panner9 = ( 1.0 * _Time.y * _MaskUV + rotator11);
				float temp_output_77_0 = ( pow( ChannelSelect( tex2D( _Mask, panner9 ), _Mask_Channel ) , _Mask_Power ) * _Mask_Value );
				float2 uv_Mask02 = input.ase_texcoord2.xy * _Mask02_ST.xy + _Mask02_ST.zw;
				float2 temp_output_1_0_g10 = float2( 1,1 );
				float2 texCoord80_g10 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g10 = (float2(( (temp_output_1_0_g10).x * texCoord80_g10.x ) , ( texCoord80_g10.y * (temp_output_1_0_g10).y )));
				float2 temp_output_11_0_g10 = float2( 0,0 );
				float2 texCoord81_g10 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g10 = ( ( (temp_output_11_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g10);
				float2 panner19_g10 = ( ( _TimeParameters.x * (temp_output_11_0_g10).y ) * float2( 0,1 ) + texCoord81_g10);
				float2 appendResult24_g10 = (float2((panner18_g10).x , (panner19_g10).y));
				float2 Mask02_UV195 = _Mask02UV;
				float2 temp_output_47_0_g10 = Mask02_UV195;
				float2 texCoord78_g10 = input.ase_texcoord2.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g10 = ( texCoord78_g10 - float2( 1,1 ) );
				float2 appendResult39_g10 = (float2(frac( ( atan2( (temp_output_31_0_g10).x , (temp_output_31_0_g10).y ) / TWO_PI ) ) , length( temp_output_31_0_g10 )));
				float2 panner54_g10 = ( ( (temp_output_47_0_g10).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g10);
				float2 panner55_g10 = ( ( _TimeParameters.x * (temp_output_47_0_g10).y ) * float2( 0,1 ) + appendResult39_g10);
				float2 appendResult58_g10 = (float2((panner54_g10).x , (panner55_g10).y));
				float2 appendResult186 = (float2(_Mask02_ST.z , _Mask02_ST.w));
				float2 Mask02_ST_ZW187 = appendResult186;
				#ifdef _MASK02_RADIAL2_ON
				float2 staticSwitch191 = ( ( ( (tex2D( _Sampler60188, ( appendResult10_g10 + appendResult24_g10 ) )).rg * 1.0 ) + ( _Mask02_ST.xy * appendResult58_g10 ) ) + Mask02_ST_ZW187 );
				#else
				float2 staticSwitch191 = uv_Mask02;
				#endif
				float cos34 = cos( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float sin34 = sin( ( ( _Mask02_Rotato * PI ) * 2.0 ) );
				float2 rotator34 = mul( staticSwitch191 - float2( 0.5,0.5 ) , float2x2( cos34 , -sin34 , sin34 , cos34 )) + float2( 0.5,0.5 );
				float2 panner35 = ( 1.0 * _Time.y * _Mask02UV + rotator34);
				float temp_output_80_0 = ( pow( ChannelSelect( tex2D( _Mask02, panner35 ), _Mask02_Channel ) , _Mask02_Power ) * _Mask02_Value );
				#ifdef _MASKMUTUPYORADD_ON
				float staticSwitch249 = ( temp_output_77_0 * temp_output_80_0 );
				#else
				float staticSwitch249 = ( temp_output_77_0 + temp_output_80_0 );
				#endif
				#ifdef _USE_MASK02_ON
				float staticSwitch38 = staticSwitch249;
				#else
				float staticSwitch38 = temp_output_77_0;
				#endif
				float Mask55 = staticSwitch38;
				float temp_output_130_0 = ( 1.0 - _Dissolve_Soft );
				float2 uv_Dissolve = input.ase_texcoord2.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
				float2 temp_output_1_0_g4 = float2( 1,1 );
				float2 texCoord80_g4 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult10_g4 = (float2(( (temp_output_1_0_g4).x * texCoord80_g4.x ) , ( texCoord80_g4.y * (temp_output_1_0_g4).y )));
				float2 temp_output_11_0_g4 = float2( 0,0 );
				float2 texCoord81_g4 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner18_g4 = ( ( (temp_output_11_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + texCoord81_g4);
				float2 panner19_g4 = ( ( _TimeParameters.x * (temp_output_11_0_g4).y ) * float2( 0,1 ) + texCoord81_g4);
				float2 appendResult24_g4 = (float2((panner18_g4).x , (panner19_g4).y));
				float2 Dissolve_UV204 = _Dissolve_UV;
				float2 temp_output_47_0_g4 = Dissolve_UV204;
				float2 texCoord78_g4 = input.ase_texcoord2.xy * float2( 2,2 ) + float2( 0,0 );
				float2 temp_output_31_0_g4 = ( texCoord78_g4 - float2( 1,1 ) );
				float2 appendResult39_g4 = (float2(frac( ( atan2( (temp_output_31_0_g4).x , (temp_output_31_0_g4).y ) / TWO_PI ) ) , length( temp_output_31_0_g4 )));
				float2 panner54_g4 = ( ( (temp_output_47_0_g4).x * _TimeParameters.x ) * float2( 1,0 ) + appendResult39_g4);
				float2 panner55_g4 = ( ( _TimeParameters.x * (temp_output_47_0_g4).y ) * float2( 0,1 ) + appendResult39_g4);
				float2 appendResult58_g4 = (float2((panner54_g4).x , (panner55_g4).y));
				float2 appendResult201 = (float2(_Dissolve_ST.z , _Dissolve_ST.w));
				float2 Dissolve_ST_ZW202 = appendResult201;
				#ifdef _DISSOLVE_RADIAL3_ON
				float2 staticSwitch196 = ( ( ( (tex2D( _Sampler60199, ( appendResult10_g4 + appendResult24_g4 ) )).rg * 1.0 ) + ( _Dissolve_ST.xy * appendResult58_g4 ) ) + Dissolve_ST_ZW202 );
				#else
				float2 staticSwitch196 = uv_Dissolve;
				#endif
				float DissolveRaodong145 = ChannelSelect( tex2DNode43, _Turbulence_Channel );
				float cos139 = cos( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float sin139 = sin( ( ( _Dissolve_Rotato * PI ) * 2.0 ) );
				float2 rotator139 = mul( ( staticSwitch196 + ( DissolveRaodong145 * _Dissolve_Raodong ) ) - float2( 0.5,0.5 ) , float2x2( cos139 , -sin139 , sin139 , cos139 )) + float2( 0.5,0.5 );
				float2 panner127 = ( 1.0 * _Time.y * _Dissolve_UV + rotator139);
				float2 uv_DissolveMask = input.ase_texcoord2.xy * _DissolveMask_ST.xy + _DissolveMask_ST.zw;
				float cos153 = cos( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float sin153 = sin( ( ( _DissolveMask_Rotato1 * PI ) * 2.0 ) );
				float2 rotator153 = mul( uv_DissolveMask - float2( 0.5,0.5 ) , float2x2( cos153 , -sin153 , sin153 , cos153 )) + float2( 0.5,0.5 );
				float2 panner150 = ( 1.0 * _Time.y * _DissolveMask_UV + rotator153);
				#ifdef _USE_DISSOLVEMASK_ON
				float staticSwitch227 = ( pow( ChannelSelect( tex2D( _DissolveMask, panner150 ), _DissolveMask_Channel ) , _DissolveMask_Power ) * _DissolveMask_Value );
				#else
				float staticSwitch227 = 1.0;
				#endif
				float temp_output_115_0 = ( ( 1.0 + ( ChannelSelect( tex2D( _Dissolve, panner127 ), _Dissolve_Channel ) * staticSwitch227 ) ) - ( 2.0 * _Dissolve_Value ) );
				float smoothstepResult116 = smoothstep( 0.0 , temp_output_130_0 , temp_output_115_0);
				float DissolveMask137 = saturate( smoothstepResult116 );
				#ifdef _USEDISSOLVE_ON
				float staticSwitch167 = DissolveMask137;
				#else
				float staticSwitch167 = 1.0;
				#endif
				


				float3 Normal = float3(0, 0, 1);
				float Alpha = ( input.ase_color.a * MT_Alpha53 * Mask55 * staticSwitch167 * _MainAlpha );
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(NormalWS);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = NormalWS;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}
			ENDHLSL
		}

	
	}
	

	

	CustomEditor "MultiFuncShaderGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19912
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":114,"pos":[-5521.143,4094.084],"params":["Inherit","False","6325.219","2303.131","Comment","52","198","146","147","128","196","203","202","200","201","199","197","126","204","162","161","151","160","159","152","157","156","155","154","153","150","158","149","148","142","141","140","139","143","127","137","136","125","122","118","129","132","130","124","123","121","120","119","117","116","115","227","228","溶解","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":49,"pos":[-5024,2352],"params":["Inherit","False","3706.035","1310.899","Comment","35","50","44","225","43","45","226","84","46","87","85","224","48","56","86","223","47","62","222","57","60","221","58","63","61","59","145","229","230","231","232","233","234","235","236","237","扰动","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":143,"pos":[-3184,4432],"params":["Inherit","False","Property","_Dissolve_UV","溶解流动","47","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":19,"pos":[-5224,-576],"params":["Inherit","False","5541.149","2793.206","Comment","47","77","76","78","79","80","83","81","82","55","38","10","247","9","36","11","35","183","12","34","15","181","13","191","31","182","180","14","32","189","30","179","185","184","29","188","190","178","194","192","193","187","16","186","195","33","249","250","遮罩","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":204,"pos":[-3192.977,4571.965],"params":["Inherit","False","Dissolve_UV","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":201,"pos":[-4834.519,4358.978],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":156,"pos":[-3739.979,5729.686],"params":["Inherit","False","Property","_DissolveMask_Rotato1","溶解遮罩旋转","57","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":59,"pos":[-3824,3584],"params":["Inherit","False","Property","_TurbulenceMask_Rotato","扰动遮罩旋转","38","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":48,"pos":[-3152,2608],"params":["Inherit","False","Property","_Turbulence_UV","扰动流动","35","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":200,"pos":[-4858.833,4173.205],"params":["Inherit","False","Property","_Dissolve_ST","Dissolve_ST","68","1","[HideInInspector]","Create","True","0","0","0","False","0","False","Object","-1","","1,1,0,0","1,1,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":203,"pos":[-4541.154,4517.593],"params":["Inherit","False","204","Dissolve_UV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":154,"pos":[-3737.129,5642.649],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":202,"pos":[-4866.023,4497.46],"params":["Inherit","False","Dissolve_ST_ZW","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":61,"pos":[-3808,3488],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":229,"pos":[-4928,2752],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":237,"pos":[-3168,2768],"params":["Inherit","False","TurbulenceUV","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":33,"pos":[-3144,688],"params":["Inherit","False","Property","_Mask02UV","第二层遮罩流动","28","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":199,"pos":[-4509.819,4278.143],"params":["Inherit","False","RadialUVDistortion","-1","","4","051d65e7699b41a4c800363fd0e822b2","0","7","60","SAMPLER2D","_Sampler60199","False","1","FLOAT2","1,1","False","11","FLOAT2","0,0","False","65","FLOAT","1","False","68","FLOAT2","1,1","False","47","FLOAT2","1,1","False","29","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":198,"pos":[-4130.048,4398.638],"params":["Inherit","False","202","Dissolve_ST_ZW","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":155,"pos":[-3707.875,5539.917],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":145,"pos":[-2384,2640],"params":["Inherit","False","DissolveRaodong","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":63,"pos":[-3792,3392],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":58,"pos":[-4768,3216],"params":["Inherit","False","0","56","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":232,"pos":[-4960,2592],"params":["Inherit","False","Property","_Turbulence_ST","Turbulence_ST","67","1","[HideInInspector]","Create","True","0","0","0","False","0","False","Object","-1","","1,1,0,0","1,1,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":230,"pos":[-4960,2864],"params":["Inherit","False","Turbulence_ST_ZW","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":231,"pos":[-4592,2768],"params":["Inherit","False","237","TurbulenceUV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":152,"pos":[-4144.257,5399.406],"params":["Inherit","False","0","151","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":195,"pos":[-3144,816],"params":["Inherit","False","Mask02_UV","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":186,"pos":[-4576,920],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":197,"pos":[-4079.014,4295.553],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":147,"pos":[-3967.399,4679.583],"params":["Inherit","False","Property","_Dissolve_Raodong","溶解贴图收扰动强度","52","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":142,"pos":[-3552,4576],"params":["Inherit","False","Property","_Dissolve_Rotato","溶解旋转","45","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":153,"pos":[-3713.974,5415.658],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":157,"pos":[-3367.071,5578.677],"params":["Inherit","False","Property","_DissolveMask_UV","溶解遮罩流动","58","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":146,"pos":[-3968,4592],"params":["Inherit","False","145","DissolveRaodong","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":128,"pos":[-4541.811,4140.22],"params":["Inherit","False","0","126","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":60,"pos":[-3440,3424],"params":["Inherit","False","Property","_TurbulenceMask_UV","扰动遮罩流动","42","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":57,"pos":[-3792,3264],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":221,"pos":[-3664,2784],"params":["Inherit","False","Property","_Turbulence_Rotato1","扰动旋转","33","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":233,"pos":[-4608,2528],"params":["Inherit","False","RadialUVDistortion","-1","","9","051d65e7699b41a4c800363fd0e822b2","0","7","60","SAMPLER2D","_Sampler60233","False","1","FLOAT2","1,1","False","11","FLOAT2","0,0","False","65","FLOAT","1","False","68","FLOAT2","1,1","False","47","FLOAT2","1,1","False","29","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":234,"pos":[-3920,2720],"params":["Inherit","False","230","Turbulence_ST_ZW","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":16,"pos":[-3184,-272],"params":["Inherit","False","Property","_MaskUV","第一层遮罩流动","20","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":187,"pos":[-4608,1056],"params":["Inherit","False","Mask02_ST_ZW","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":193,"pos":[-4296,1072],"params":["Inherit","False","195","Mask02_UV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":192,"pos":[-4600,736],"params":["Inherit","False","Property","_Mask02_ST","Mask02_ST","64","1","[HideInInspector]","Create","True","0","0","0","False","0","False","Object","-1","","1,1,0,0","1,1,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":20,"pos":[48,-3096],"params":["Inherit","False","7038.266","2141.699","Comment","62","135","23","75","166","134","74","285","283","282","281","41","40","280","276","278","218","277","275","216","220","264","261","259","255","254","256","270","170","1","177","257","252","251","265","274","64","271","268","267","266","263","262","260","258","253","53","2","51","5","52","8","175","3","6","176","169","7","174","172","171","173","4","主贴图","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":149,"pos":[-3898.19,4487.006],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":150,"pos":[-3372.162,5411.682],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":196,"pos":[-4047.613,4140.916],"params":["Inherit","False","Property","_Dissolve_Radial3","溶解极坐标","44","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":47,"pos":[-4960,2416],"params":["Inherit","False","0","43","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":62,"pos":[-3456,3264],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":222,"pos":[-3600,2672],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":235,"pos":[-3888,2592],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":140,"pos":[-3545.027,4491.329],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":194,"pos":[-3192,-120],"params":["Inherit","False","Mask_UV","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":178,"pos":[-4616,-56],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":190,"pos":[-3856,984],"params":["Inherit","False","187","Mask02_ST_ZW","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":188,"pos":[-4248,840],"params":["Inherit","False","RadialUVDistortion","-1","","10","051d65e7699b41a4c800363fd0e822b2","0","7","60","SAMPLER2D","_Sampler60188","False","1","FLOAT2","1,1","False","11","FLOAT2","0,0","False","65","FLOAT","1","False","68","FLOAT2","1,1","False","47","FLOAT2","1,1","False","29","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":29,"pos":[-3528,840],"params":["Inherit","False","Property","_Mask02_Rotato","第二层遮罩旋转","26","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":141,"pos":[-3515.774,4388.596],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":148,"pos":[-3813.104,4233.08],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":162,"pos":[-2817.44,5562.246],"params":["Inherit","False","Property","_DissolveMask_Power","溶解遮罩对比度","55","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":151,"pos":[-3087.677,5387.6],"params":["Inherit","True","Property","_DissolveMask","溶解 遮罩","54","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":86,"pos":[-2928,3440],"params":["Inherit","False","Property","_Turbulence_Mask_Power1","扰动遮罩对比度","40","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":56,"pos":[-3168,3232],"params":["Inherit","True","Property","_Turbulence_Mask","扰动遮罩","39","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":223,"pos":[-3584,2560],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":236,"pos":[-3920,2448],"params":["Inherit","False","Property","_Turbulence_Radial","扰动极坐标","32","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":184,"pos":[-4648,-224],"params":["Inherit","False","Property","_Mask_ST","Mask_ST","63","1","[HideInInspector]","Create","True","0","0","0","False","0","False","Object","-1","","1,1,0,0","1,1,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":185,"pos":[-4376,-32],"params":["Inherit","False","194","Mask_UV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":179,"pos":[-4648,48],"params":["Inherit","False","Mask_ST_ZW","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":30,"pos":[-3520,752],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":189,"pos":[-3824,856],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":32,"pos":[-3952,504],"params":["Inherit","False","0","36","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":4,"pos":[2144,-2728],"params":["Inherit","False","Property","_MT_UV","主贴图流动","9","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":160,"pos":[-2720.364,5425.164],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":161,"pos":[-2534.507,5551.939],"params":["Inherit","False","Property","_DissolveMask_Value","溶解遮罩强度","56","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":139,"pos":[-3520,4272],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":85,"pos":[-2832,3296],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":87,"pos":[-2640,3424],"params":["Inherit","False","Property","_Turbulence_Mask_Value","扰动遮罩强度","41","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":224,"pos":[-3584,2432],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":14,"pos":[-3528,-112],"params":["Inherit","False","Property","_Mask_Rotato","第一层遮罩旋转","18","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":180,"pos":[-4392,-320],"params":["Inherit","False","RadialUVDistortion","-1","","11","051d65e7699b41a4c800363fd0e822b2","0","7","60","SAMPLER2D","_Sampler60180","False","1","FLOAT2","1,1","False","11","FLOAT2","0,0","False","65","FLOAT","1","False","68","FLOAT2","1,1","False","47","FLOAT2","1,1","False","29","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":182,"pos":[-3984,-64],"params":["Inherit","False","179","Mask_ST_ZW","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":31,"pos":[-3488,648],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":191,"pos":[-3760,704],"params":["Inherit","False","Property","_Mask02_Radial2","第二层遮罩极坐标","25","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":173,"pos":[2128,-2584],"params":["Inherit","False","MT_UV","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":171,"pos":[320,-2584],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":127,"pos":[-3184,4288],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":159,"pos":[-2518.507,5449.781],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":228,"pos":[-2859.194,4915.32],"params":["Inherit","False","Constant","_Float3","Float 2","56","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":84,"pos":[-2624,3328],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":226,"pos":[-2704,3040],"params":["Inherit","False","Constant","_Float2","Float 2","56","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":46,"pos":[-3168,2464],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":13,"pos":[-3520,-208],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":181,"pos":[-3952,-168],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":15,"pos":[-4280,-488],"params":["Inherit","False","0","10","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":34,"pos":[-3496,528],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":172,"pos":[288,-2472],"params":["Inherit","False","MT_ST_ZW","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":174,"pos":[576,-2536],"params":["Inherit","False","173","MT_UV","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":170,"pos":[288,-2760],"params":["Inherit","False","Property","_MT_ST","MT_ST","66","1","[HideInInspector]","Create","True","0","0","0","False","0","False","Object","-1","","1,1,0,0","1,1,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":227,"pos":[-2667.194,4979.32],"params":["Inherit","False","Property","_Use_DissolveMask","开启溶解遮罩","53","0","Create","False","0","0","0","False","1","Header(DissolveMask)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":126,"pos":[-2928,4240],"params":["Inherit","True","Property","_Dissolve","溶解贴图","46","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":225,"pos":[-2512,3104],"params":["Inherit","False","Property","_Use_TurbulenceMask","开启扰动遮罩","37","0","Create","False","0","0","0","False","1","Header(Turbulence_Mask)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":43,"pos":[-2768,2448],"params":["Inherit","True","Property","_Turbulence","扰动","34","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":45,"pos":[-2336,2432],"params":["Inherit","False","Property","_Turbulence_Value","扰动强度","36","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","0.5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":12,"pos":[-3520,-312],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":183,"pos":[-3824,-264],"params":["Inherit","False","Property","_Mask_Radial1","第一层遮罩极坐标","17","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":35,"pos":[-3160,520],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":7,"pos":[1744,-2568],"params":["Inherit","False","Property","_MT_Rotato","主贴图旋转","5","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":169,"pos":[640,-2808],"params":["Inherit","False","RadialUVDistortion","-1","","12","051d65e7699b41a4c800363fd0e822b2","0","7","60","SAMPLER2D","_Sampler60169","False","1","FLOAT2","1,1","False","11","FLOAT2","0,0","False","65","FLOAT","1","False","68","FLOAT2","1,1","False","47","FLOAT2","1,1","False","29","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":176,"pos":[1328,-2616],"params":["Inherit","False","172","MT_ST_ZW","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":158,"pos":[-2546.63,4495.04],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":118,"pos":[-2208,4496],"params":["Inherit","False","Property","_Dissolve_Value","溶解度","48","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":44,"pos":[-1920,2512],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":11,"pos":[-3528,-432],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":36,"pos":[-2872,496],"params":["Inherit","True","Property","_Mask02","第二层遮罩","27","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":82,"pos":[-2536,632],"params":["Inherit","False","Property","_Mask02_Power","第二层遮罩对比度","29","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":6,"pos":[1760,-2664],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":3,"pos":[1296,-3048],"params":["Inherit","False","0","1","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":175,"pos":[1360,-2744],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":117,"pos":[-1985.922,4272.254],"params":["Inherit","False","2","2","0","FLOAT","1","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":129,"pos":[-1728,4624],"params":["Inherit","False","Property","_Dissolve_Soft","软硬溶解","49","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":119,"pos":[-1856,4464],"params":["Inherit","False","2","2","0","FLOAT","2","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":50,"pos":[-1664,2480],"params":["Inherit","False","Turbulence","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":9,"pos":[-3192,-400],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":81,"pos":[-2504,520],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":83,"pos":[-2320,648],"params":["Inherit","False","Property","_Mask02_Value","第二层遮罩强度","30","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":8,"pos":[1808,-2776],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":177,"pos":[1328,-2888],"params":["Inherit","False","Property","_MT_Radial","主贴图极坐标","4","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":130,"pos":[-1520,4512],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":115,"pos":[-1664,4192],"params":["Inherit","True","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":10,"pos":[-2904,-464],"params":["Inherit","True","Property","_Mask","第一层遮罩","19","1","[Header]","Create","False","1","Mask","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":80,"pos":[-2304,544],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":78,"pos":[-2712,-224],"params":["Inherit","False","Property","_Mask_Power","遮罩对比度","21","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":52,"pos":[2496,-2600],"params":["Inherit","False","50","Turbulence","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":5,"pos":[1808,-2888],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":116,"pos":[-1177.452,4373.058],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":79,"pos":[-2392,-288],"params":["Inherit","False","Property","_Mask_Value","遮罩强度","22","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","5","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":76,"pos":[-2408,-496],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":247,"pos":[-1856,-96],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":250,"pos":[-1328,224],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":51,"pos":[2528,-2712],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":2,"pos":[2144,-2856],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":132,"pos":[-960,4208],"params":["Inherit","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":77,"pos":[-2216,-496],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":64,"pos":[2496,-2840],"params":["Inherit","False","Property","_UseTurbulence","使用扰动","31","0","Create","False","0","0","0","False","1","Header(Turbulence)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":249,"pos":[-1096,200],"params":["Inherit","False","Property","_MaskMutupyoradd","遮罩相乘模式(更淡)","24","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":137,"pos":[-720,4208],"params":["Inherit","False","DissolveMask","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":38,"pos":[-1248,-176],"params":["Inherit","False","Property","_Use_Mask02","开启第二层遮罩","23","0","Create","False","0","0","0","False","1","Header(Mask02)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":1,"pos":[2752,-2856],"params":["Inherit","True","Property","_MT","主贴图","7","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":276,"pos":[3592,-1832],"params":["Inherit","False","Property","_MT_Color02","主帖图02颜色","13","1","[HDR]","Create","False","0","0","0","False","0","False","Object","-1","","0.8962264,0.8962264,0.8962264,1","0.9150943,0.9150943,0.9150943,1","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":168,"pos":[704,416],"params":["Inherit","False","Constant","_Float1","Float 1","47","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":138,"pos":[672,512],"params":["Inherit","False","137","DissolveMask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":55,"pos":[-976,-176],"params":["Inherit","False","Mask","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":53,"pos":[2800,-2648],"params":["Inherit","False","MT_Alpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":283,"pos":[3896,-1616],"params":["Inherit","False","MT02_ALP","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":88,"pos":[-4408,-1864],"params":["Inherit","False","2002.054","682.7473","Comment","11","67","65","70","73","72","71","69","68","66","164","165","Ramp","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":26,"pos":[799.75,-38.91448],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":54,"pos":[944,240],"params":["Inherit","False","53","MT_Alpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":22,"pos":[944,320],"params":["Inherit","False","55","Mask","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":167,"pos":[896,416],"params":["Inherit","False","Property","_Keyword0","Keyword 0","43","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","166","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":238,"pos":[784,640],"params":["Inherit","False","Property","_MainAlpha","总透明度","2","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":284,"pos":[936,776],"params":["Inherit","False","283","MT02_ALP","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":27,"pos":[1207.888,243.4648],"params":["Inherit","False","6","6","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":120,"pos":[-1333.717,4692.501],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":121,"pos":[-1160.582,4608.449],"params":["Inherit","False","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":124,"pos":[-683.4602,4647.31],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":136,"pos":[-518.5153,4630.609],"params":["Inherit","False","DissolveWidth","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":66,"pos":[-3976,-1560],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":68,"pos":[-3912,-1672],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":69,"pos":[-3920,-1792],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":71,"pos":[-3576,-1760],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":70,"pos":[-3576,-1632],"params":["Inherit","False","Property","_RampUV","渐变流动","62","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":67,"pos":[-4328,-1792],"params":["Inherit","False","0","72","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":73,"pos":[-2640,-1752],"params":["Inherit","False","Ramp","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":72,"pos":[-3296,-1736],"params":["Inherit","True","Property","_Ramp","渐变图","60","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":123,"pos":[-928,4464],"params":["Inherit","True","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":65,"pos":[-3952,-1480],"params":["Inherit","False","Property","_Ramp_Rotato","渐变旋转","61","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":165,"pos":[-3160,-1816],"params":["Inherit","False","Constant","_Float0","Float 0","45","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":164,"pos":[-2896,-1752],"params":["Inherit","False","Property","_UseRamp","使用渐变","59","0","Create","False","0","0","0","False","1","Header(Ramp)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":17,"pos":[1728,16],"params":["Inherit","False","Property","_CullMode","双面显示","0","1","[Enum]","Create","False","0","1","Option1","0","1","UnityEngine.Rendering.CullMode","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":215,"pos":[1745.829,191.2145],"params":["Inherit","False","Property","_dst","混合模式","1","1","[Enum]","Create","False","0","2","Additive","1","AlphaBlend","10","0","True","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":122,"pos":[-1390.145,4801.76],"params":["Inherit","False","Property","_DissolveWidth","溶解亮边宽度","50","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","0.1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":125,"pos":[-898.4603,4750.31],"params":["Inherit","False","Property","_DissolveWidth_Color","溶解亮边颜色","51","1","[HDR]","Create","False","0","0","0","False","0","False","Object","-1","","0.9339623,0.9339623,0.9339623,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":24,"pos":[762.7852,-285.345],"params":["Inherit","True","23","MT","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":25,"pos":[1040,-96],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":246,"pos":[1216,-96],"params":["Inherit","False","Desaturate","69","","13","859ae4ee44453e949a771bc575cfa81a","0","1","2","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":253,"pos":[256,-1680],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":258,"pos":[576,-1904],"params":["Inherit","False","RadialUVDistortion","-1","","14","051d65e7699b41a4c800363fd0e822b2","0","7","60","SAMPLER2D","_Sampler60258","False","1","FLOAT2","1,1","False","11","FLOAT2","0,0","False","65","FLOAT","1","False","68","FLOAT2","1,1","False","47","FLOAT2","1,1","False","29","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":260,"pos":[1696,-1760],"params":["Inherit","False","1","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":262,"pos":[1296,-1840],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":263,"pos":[1744,-1872],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RotatorNode, AmplifyShaderEditor","id":266,"pos":[1744,-1984],"params":["Inherit","False","3","0","FLOAT2","0,0","False","1","FLOAT2","0.5,0.5","False","2","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":267,"pos":[2464,-1808],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PannerNode, AmplifyShaderEditor","id":268,"pos":[2080,-1952],"params":["Inherit","False","3","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":271,"pos":[2752,-1744],"params":["Inherit","False","MT_Alpha02","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":274,"pos":[2400,-2072],"params":["Inherit","False","Property","_Keyword1","Keyword 1","31","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","64","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":265,"pos":[2432,-1696],"params":["Inherit","False","50","Turbulence","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":251,"pos":[2080,-1824],"params":["Inherit","False","Property","_MT_UV02","主帖图02UV","16","0","Create","False","0","0","0","False","0","False","Object","-1","","0,0","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":252,"pos":[2064,-1680],"params":["Inherit","False","MT_UV02","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":257,"pos":[1680,-1664],"params":["Inherit","False","Property","_MT_Rotato02","主帖图02旋转","12","0","Create","False","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":270,"pos":[2736,-1960],"params":["Inherit","True","Property","_MT02","主帖图02","14","0","Create","False","0","0","0","False","0","False","","-1","None","None","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":256,"pos":[224,-1856],"params":["Inherit","False","Property","_MT02_ST","MT02_ST","65","1","[HideInInspector]","Create","True","0","0","0","False","0","False","Object","-1","","1,1,0,0","1,1,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":254,"pos":[224,-1568],"params":["Inherit","False","MT02_ST_ZW","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":255,"pos":[544,-1640],"params":["Inherit","False","252","MT_UV02","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":259,"pos":[1264,-1712],"params":["Inherit","False","254","MT02_ST_ZW","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":261,"pos":[1240,-2144],"params":["Inherit","False","0","270","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":264,"pos":[1264,-1984],"params":["Inherit","False","Property","_MT_Radial02","主帖图02极坐标","11","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT2","0,0","False","0","FLOAT2","0,0","False","2","FLOAT2","0,0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT2","0,0","False","6","FLOAT2","0,0","False","7","FLOAT2","0,0","False","8","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":220,"pos":[3112,-2848],"params":["Inherit","False","Property","_MT_UseR","主帖图R通道","6","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":216,"pos":[3608,-2760],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT","0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":275,"pos":[3624,-1944],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":277,"pos":[3240,-1952],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":218,"pos":[3496,-2648],"params":["Inherit","False","Property","_MT_Alpha","主帖图强度","8","0","Create","False","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":278,"pos":[3128,-1840],"params":["Inherit","False","Property","_MT_Alpha02","主帖图02强度","15","1","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":40,"pos":[3888,-2768],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":41,"pos":[3856,-2656],"params":["Inherit","False","Property","_MT_Color","主贴图颜色","3","1","[HDR]","Create","False","0","0","0","False","0","False","Object","-1","","0.8962264,0.8962264,0.8962264,1","0.9150943,0.9150943,0.9150943,1","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":281,"pos":[4144,-2512],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT3","0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":282,"pos":[3840,-2416],"params":["Inherit","False","280","MT02","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":285,"pos":[4352,-2560],"params":["Inherit","False","Property","_UseMT02","开启贴图2","10","0","Create","False","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":74,"pos":[4816,-2584],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":134,"pos":[5168,-2504],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":166,"pos":[5168,-2600],"params":["Inherit","False","Property","_UseDissolve","开启溶解","43","0","Create","False","0","0","0","False","1","Header(Dissolve)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","COLOR","0,0,0,0","False","0","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","3","COLOR","0,0,0,0","False","4","COLOR","0,0,0,0","False","5","COLOR","0,0,0,0","False","6","COLOR","0,0,0,0","False","7","COLOR","0,0,0,0","False","8","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":75,"pos":[4768,-2680],"params":["Inherit","False","73","Ramp","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":23,"pos":[5440,-2600],"params":["Inherit","False","MT","-1","True","1","0","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":135,"pos":[5168,-2392],"params":["Inherit","False","136","DissolveWidth","1","0","OBJECT","","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":280,"pos":[3912,-1936],"params":["Inherit","False","MT02","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":205,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ExtraPrePass","0","0","ExtraPrePass","6","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","0","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":207,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ShadowCaster","0","2","ShadowCaster","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","False","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=ShadowCaster","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":208,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthOnly","0","3","DepthOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","True","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","False","False","False","True","1","LightMode=DepthOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":209,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Meta","0","4","Meta","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Meta","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":210,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Universal2D","0","5","Universal2D","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=Universal2D","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":211,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","SceneSelectionPass","0","6","SceneSelectionPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=SceneSelectionPass","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":212,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ScenePickingPass","0","7","ScenePickingPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Picking","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":213,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormals","0","8","DepthNormals","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":214,"pos":[1426.314,-28.71771],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","13","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormalsOnly","0","9","DepthNormalsOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","True","9","d3d11","metal","vulkan","xboxone","xboxseries","playstation","ps4","ps5","switch","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":206,"pos":[1426.314,-28.71771],"params":["Float","False","True","-1","3","UnityEditor.ShaderGraphUnlitGUI","0","19","URP/多功能材质PRO(Test)","2992e84f91cbeb14eab234972e07ea9d","True","Forward","0","1","Forward","11","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","True","True","2","True","_CullMode","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Transparent=RenderType","Queue=Transparent=Queue=0","UniversalMaterialType=Unlit","True","2","True","12","all","0","True","True","2","5","False","","10","True","_dst","1","0","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","2","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalForwardOnly","False","False","0","","0","0","Standard","28","Surface","1","639111253027504619","  Keep Alpha","0","0","  Blend","0","0","Two Sided","0","639111253103618063","Alpha Clipping","0","639181395098108227","  Use Shadow Threshold","0","0","Fragment Normal Space","0","0","Forward Only","0","0","Cast Shadows","1","0","Receive Shadows","1","0","Receive SSAO","1","0","GPU Instancing","1","0","LOD CrossFade","1","0","Built-in Fog","1","0","Meta Pass","0","0","Extra Pre Pass","0","0","Tessellation","0","0","  Phong","0","0","  Strength","0.5,False,","0","  Type","0","0","  Tess","16,False,","0","  Min","10,False,","0","  Max","25,False,","0","  Edge Length","16,False,","0","  Max Displacement","25,False,","0","Write Depth","0","0","  Conservative","0","0","Vertex Position","1","0","0","10","False","True","True","True","False","False","True","True","True","False","False","","False","0"]}
{"wire":[204,0,143,0]}
{"wire":[201,0,200,3]}
{"wire":[201,1,200,4]}
{"wire":[154,0,156,0]}
{"wire":[202,0,201,0]}
{"wire":[61,0,59,0]}
{"wire":[229,0,232,3]}
{"wire":[229,1,232,4]}
{"wire":[237,0,48,0]}
{"wire":[199,68,200,0]}
{"wire":[199,47,203,0]}
{"wire":[155,0,154,0]}
{"wire":[145,0,43,1]}
{"wire":[63,0,61,0]}
{"wire":[230,0,229,0]}
{"wire":[195,0,33,0]}
{"wire":[186,0,192,3]}
{"wire":[186,1,192,4]}
{"wire":[197,0,199,0]}
{"wire":[197,1,198,0]}
{"wire":[153,0,152,0]}
{"wire":[153,2,155,0]}
{"wire":[57,0,58,0]}
{"wire":[57,2,63,0]}
{"wire":[233,68,232,0]}
{"wire":[233,47,231,0]}
{"wire":[187,0,186,0]}
{"wire":[149,0,146,0]}
{"wire":[149,1,147,0]}
{"wire":[150,0,153,0]}
{"wire":[150,2,157,0]}
{"wire":[196,1,128,0]}
{"wire":[196,0,197,0]}
{"wire":[62,0,57,0]}
{"wire":[62,2,60,0]}
{"wire":[222,0,221,0]}
{"wire":[235,0,233,0]}
{"wire":[235,1,234,0]}
{"wire":[140,0,142,0]}
{"wire":[194,0,16,0]}
{"wire":[178,0,184,3]}
{"wire":[178,1,184,4]}
{"wire":[188,68,192,0]}
{"wire":[188,47,193,0]}
{"wire":[141,0,140,0]}
{"wire":[148,0,196,0]}
{"wire":[148,1,149,0]}
{"wire":[151,1,150,0]}
{"wire":[56,1,62,0]}
{"wire":[223,0,222,0]}
{"wire":[236,1,47,0]}
{"wire":[236,0,235,0]}
{"wire":[179,0,178,0]}
{"wire":[30,0,29,0]}
{"wire":[189,0,188,0]}
{"wire":[189,1,190,0]}
{"wire":[160,0,151,1]}
{"wire":[160,1,162,0]}
{"wire":[139,0,148,0]}
{"wire":[139,2,141,0]}
{"wire":[85,0,56,1]}
{"wire":[85,1,86,0]}
{"wire":[224,0,236,0]}
{"wire":[224,2,223,0]}
{"wire":[180,68,184,0]}
{"wire":[180,47,185,0]}
{"wire":[31,0,30,0]}
{"wire":[191,1,32,0]}
{"wire":[191,0,189,0]}
{"wire":[173,0,4,0]}
{"wire":[171,0,170,3]}
{"wire":[171,1,170,4]}
{"wire":[127,0,139,0]}
{"wire":[127,2,143,0]}
{"wire":[159,0,160,0]}
{"wire":[159,1,161,0]}
{"wire":[84,0,85,0]}
{"wire":[84,1,87,0]}
{"wire":[46,0,224,0]}
{"wire":[46,2,48,0]}
{"wire":[13,0,14,0]}
{"wire":[181,0,180,0]}
{"wire":[181,1,182,0]}
{"wire":[34,0,191,0]}
{"wire":[34,2,31,0]}
{"wire":[172,0,171,0]}
{"wire":[227,1,228,0]}
{"wire":[227,0,159,0]}
{"wire":[126,1,127,0]}
{"wire":[225,1,226,0]}
{"wire":[225,0,84,0]}
{"wire":[43,1,46,0]}
{"wire":[12,0,13,0]}
{"wire":[183,1,15,0]}
{"wire":[183,0,181,0]}
{"wire":[35,0,34,0]}
{"wire":[35,2,33,0]}
{"wire":[169,68,170,0]}
{"wire":[169,47,174,0]}
{"wire":[158,0,126,1]}
{"wire":[158,1,227,0]}
{"wire":[44,0,45,0]}
{"wire":[44,1,43,1]}
{"wire":[44,2,225,0]}
{"wire":[11,0,183,0]}
{"wire":[11,2,12,0]}
{"wire":[36,1,35,0]}
{"wire":[6,0,7,0]}
{"wire":[175,0,169,0]}
{"wire":[175,1,176,0]}
{"wire":[117,1,158,0]}
{"wire":[119,1,118,0]}
{"wire":[50,0,44,0]}
{"wire":[9,0,11,0]}
{"wire":[9,2,16,0]}
{"wire":[81,0,36,1]}
{"wire":[81,1,82,0]}
{"wire":[8,0,6,0]}
{"wire":[177,1,3,0]}
{"wire":[177,0,175,0]}
{"wire":[130,0,129,0]}
{"wire":[115,0,117,0]}
{"wire":[115,1,119,0]}
{"wire":[10,1,9,0]}
{"wire":[80,0,81,0]}
{"wire":[80,1,83,0]}
{"wire":[5,0,177,0]}
{"wire":[5,2,8,0]}
{"wire":[116,0,115,0]}
{"wire":[116,2,130,0]}
{"wire":[76,0,10,1]}
{"wire":[76,1,78,0]}
{"wire":[247,0,77,0]}
{"wire":[247,1,80,0]}
{"wire":[250,0,77,0]}
{"wire":[250,1,80,0]}
{"wire":[51,0,2,0]}
{"wire":[51,1,52,0]}
{"wire":[2,0,5,0]}
{"wire":[2,2,4,0]}
{"wire":[132,0,116,0]}
{"wire":[77,0,76,0]}
{"wire":[77,1,79,0]}
{"wire":[64,1,2,0]}
{"wire":[64,0,51,0]}
{"wire":[249,1,247,0]}
{"wire":[249,0,250,0]}
{"wire":[137,0,132,0]}
{"wire":[38,1,77,0]}
{"wire":[38,0,249,0]}
{"wire":[1,1,64,0]}
{"wire":[55,0,38,0]}
{"wire":[53,0,1,4]}
{"wire":[283,0,276,4]}
{"wire":[167,1,168,0]}
{"wire":[167,0,138,0]}
{"wire":[27,0,26,4]}
{"wire":[27,1,54,0]}
{"wire":[27,2,22,0]}
{"wire":[27,3,167,0]}
{"wire":[27,4,238,0]}
{"wire":[27,5,284,0]}
{"wire":[120,0,130,0]}
{"wire":[120,1,122,0]}
{"wire":[121,0,115,0]}
{"wire":[121,2,120,0]}
{"wire":[124,0,123,0]}
{"wire":[124,1,125,0]}
{"wire":[136,0,124,0]}
{"wire":[66,0,65,0]}
{"wire":[68,0,66,0]}
{"wire":[69,0,67,0]}
{"wire":[69,2,68,0]}
{"wire":[71,0,69,0]}
{"wire":[71,2,70,0]}
{"wire":[73,0,164,0]}
{"wire":[72,1,71,0]}
{"wire":[123,0,116,0]}
{"wire":[123,1,121,0]}
{"wire":[164,1,165,0]}
{"wire":[164,0,72,0]}
{"wire":[25,0,24,0]}
{"wire":[25,1,26,0]}
{"wire":[246,2,25,0]}
{"wire":[253,0,256,3]}
{"wire":[253,1,256,4]}
{"wire":[258,68,256,0]}
{"wire":[258,47,255,0]}
{"wire":[260,0,257,0]}
{"wire":[262,0,258,0]}
{"wire":[262,1,259,0]}
{"wire":[263,0,260,0]}
{"wire":[266,0,264,0]}
{"wire":[266,2,263,0]}
{"wire":[267,0,268,0]}
{"wire":[267,1,265,0]}
{"wire":[268,0,266,0]}
{"wire":[268,2,251,0]}
{"wire":[271,0,270,4]}
{"wire":[274,1,268,0]}
{"wire":[274,0,267,0]}
{"wire":[252,0,251,0]}
{"wire":[270,1,274,0]}
{"wire":[254,0,253,0]}
{"wire":[264,1,261,0]}
{"wire":[264,0,262,0]}
{"wire":[220,1,1,0]}
{"wire":[220,0,1,1]}
{"wire":[216,0,220,0]}
{"wire":[216,1,218,0]}
{"wire":[275,0,277,0]}
{"wire":[275,1,276,5]}
{"wire":[277,0,270,5]}
{"wire":[277,1,278,0]}
{"wire":[40,0,216,0]}
{"wire":[40,1,41,0]}
{"wire":[281,0,40,0]}
{"wire":[281,1,282,0]}
{"wire":[285,1,40,0]}
{"wire":[285,0,281,0]}
{"wire":[74,0,75,0]}
{"wire":[74,1,285,0]}
{"wire":[134,0,74,0]}
{"wire":[134,1,135,0]}
{"wire":[166,1,74,0]}
{"wire":[166,0,134,0]}
{"wire":[23,0,166,0]}
{"wire":[280,0,275,0]}
{"wire":[206,2,246,0]}
{"wire":[206,3,27,0]}
ASEEND*/
//CHKSM=4B0CBC7E3F8C7BD68F3AFFE7C76564EFE8900295