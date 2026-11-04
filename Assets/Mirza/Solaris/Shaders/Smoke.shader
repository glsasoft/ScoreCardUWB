// Made with Amplify Shader Editor v1.9.9.12
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mirza/Smoke"
{
	Properties
	{
		[HDR][Header(Colour)][Space(5)] _Colour( "Colour", Color ) = ( 1, 1, 1, 1 )
		_AlphaErosion( "Alpha Erosion", Range( 0, 1 ) ) = 0
		_ParticleAlphaErosionoverLifetime( "Particle Alpha Erosion over Lifetime", Range( 0, 1 ) ) = 0
		[Header(Texture)][Space(5)] _BaseMap( "Base Map", 2D ) = "white" {}
		_BaseAlpha( "Base Alpha", Range( 0, 1 ) ) = 1
		_BaseAlphaPower( "Base Alpha Power", Range( 0.5, 2 ) ) = 1
		_NormalMap( "Normal Map", 2D ) = "white" {}
		_NormalMapScale( "Normal Map Scale", Float ) = 1
		[Header(Depth Fade)][Space(5)] _DepthFade( "Depth Fade", Float ) = 0
		_DepthFadePower( "Depth Fade Power", Float ) = 1
		[Header(Camera Depth Fade)][Space(5)] _CameraDepthFadeLength( "Camera Depth Fade Length", Float ) = 0
		_CameraDepthFadeOffset( "Camera Depth Fade Offset", Float ) = 0
		_CameraDepthFadePower( "Camera Depth Fade Power", Float ) = 1
		[Header(Particle Settings)][Space(5)] _ParticleRandomization( "Particle Randomization", Range( 0, 1 ) ) = 1
		[Header(Lighting)][Space(5)][Toggle( _LIGHTINGENABLED_ON )] _LightingEnabled( "Lighting Enabled", Float ) = 0
		[Space(5)] _MainLight( "Main Light", Range( 0, 1 ) ) = 1
		[Space(5)] _AdditionalLights( "Additional Lights", Range( 0, 1 ) ) = 1
		_AdditionalLightsTranslucency( "Additional Lights Translucency", Range( 0, 8 ) ) = 1
		[Space(5)][Toggle( _ADDITIONALLIGHTSTRANSLUCENCYENABLED_ON )] _AdditionalLightsTranslucencyEnabled( "Additional Lights Translucency Enabled", Float ) = 0
		[Header(Additive Noise)][Space(5)] _AdditiveNoise( "Additive Noise", Range( 0, 2 ) ) = 1
		[Toggle( _ADDITIVENOISEENABLED_ON )] _AdditiveNoiseEnabled( "Additive Noise Enabled", Float ) = 1
		_AdditiveNoiseScale( "Additive Noise Scale", Float ) = 40
		_AdditiveNoiseAnimation( "Additive Noise Animation", Vector ) = ( 0, 0, 0, 0 )
		_AdditiveNoiseOffset( "Additive Noise Offset", Vector ) = ( 0, 0, 0, 0 )
		_AdditiveNoisePower( "Additive Noise Power", Float ) = 1
		_AdditiveNoisetoRGB( "Additive Noise to RGB", Range( 0, 1 ) ) = 0
		[Space(5)] _AdditiveNoiseNormalfromHeight( "Additive Noise Normal from Height", Float ) = 0
		[Header(Subtractive Noise)][Space(5)] _SubtractiveNoise( "Subtractive Noise", Range( 0, 1 ) ) = 0
		[Toggle( _SUBTRACTIVENOISEENABLED_ON )] _SubtractiveNoiseEnabled( "Subtractive Noise Enabled", Float ) = 0
		_SubtractiveNoiseScale( "Subtractive Noise Scale", Float ) = 40
		_SubtractiveNoiseAnimation( "Subtractive Noise Animation", Vector ) = ( 0, 0, 0, 0 )
		_SubtractiveNoiseOffset( "Subtractive Noise Offset", Vector ) = ( 0, 0, 0, 0 )
		_SubtractiveNoisePower( "Subtractive Noise Power", Float ) = 1
		[Header(Distortion Noise)][Space(5)] _DistortionNoise( "Distortion Noise", Range( 0, 0.1 ) ) = 0
		[Toggle( _DISTORTIONNOISEENABLED_ON )] _DistortionNoiseEnabled( "Distortion Noise Enabled", Float ) = 1
		_DistortionNoiseScale( "Distortion Noise Scale", Float ) = 2
		_DistortionNoiseAnimation( "Distortion Noise Animation", Vector ) = ( 0, 0, 0, 0 )
		_DistortionNoiseOffset( "Distortion Noise Offset", Vector ) = ( 0, 0, 0, 0 )
		[IntRange] _DistortionNoiseOctaves( "Distortion Noise Octaves", Range( 1, 8 ) ) = 1


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
		Cull Off
		AlphaToMask Off
		ColorMask RGBA
		Blend One Zero, One Zero
		BlendOp Add, Add

		

		HLSLINCLUDE
		#pragma target 4.5
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
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
			#define REQUIRE_DEPTH_TEXTURE 1


			

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

			#include "../../VFXToolkit/Shaders/_Includes/Noise.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _LIGHTINGENABLED_ON
			#pragma shader_feature_local _DISTORTIONNOISEENABLED_ON
			#pragma shader_feature_local _ADDITIVENOISEENABLED_ON
			#pragma shader_feature_local _ADDITIONALLIGHTSTRANSLUCENCYENABLED_ON
			#pragma shader_feature_local _SUBTRACTIVENOISEENABLED_ON
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#if (UNITY_VERSION >= 202220)
			#pragma multi_compile _ _LIGHT_LAYERS
			#endif
			#if (UNITY_VERSION >= 202220) && (UNITY_VERSION < 60010000)
			#pragma multi_compile _ _FORWARD_PLUS
			#endif
			#if (UNITY_VERSION >= 60010000)
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#endif


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
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DistortionNoiseOffset;
			float4 _DistortionNoiseAnimation;
			float4 _NormalMap_ST;
			float4 _AdditiveNoiseOffset;
			float4 _AdditiveNoiseAnimation;
			float4 _Colour;
			float4 _SubtractiveNoiseAnimation;
			float4 _SubtractiveNoiseOffset;
			float _DistortionNoiseScale;
			float _AlphaErosion;
			float _ParticleAlphaErosionoverLifetime;
			float _SubtractiveNoiseScale;
			float _SubtractiveNoisePower;
			float _SubtractiveNoise;
			float _DepthFade;
			float _DepthFadePower;
			float _CameraDepthFadeLength;
			float _BaseAlpha;
			float _BaseAlphaPower;
			float _AdditiveNoiseNormalfromHeight;
			float _AdditionalLightsTranslucency;
			float _CameraDepthFadeOffset;
			float _NormalMapScale;
			float _MainLight;
			float _AdditiveNoisetoRGB;
			float _AdditiveNoise;
			float _AdditiveNoisePower;
			float _AdditiveNoiseScale;
			float _DistortionNoise;
			float _DistortionNoiseOctaves;
			float _ParticleRandomization;
			float _AdditionalLights;
			float _CameraDepthFadePower;
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

			sampler2D _BaseMap;
			sampler2D _NormalMap;


			inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }
			inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }
			inline float valueNoise (float2 uv)
			{
				float2 i = floor(uv);
				float2 f = frac( uv );
				f = f* f * (3.0 - 2.0 * f);
				uv = abs( frac(uv) - 0.5);
				float2 c0 = i + float2( 0.0, 0.0 );
				float2 c1 = i + float2( 1.0, 0.0 );
				float2 c2 = i + float2( 0.0, 1.0 );
				float2 c3 = i + float2( 1.0, 1.0 );
				float r0 = noise_randomValue( c0 );
				float r1 = noise_randomValue( c1 );
				float r2 = noise_randomValue( c2 );
				float r3 = noise_randomValue( c3 );
				float bottomOfGrid = noise_interpolate( r0, r1, f.x );
				float topOfGrid = noise_interpolate( r2, r3, f.x );
				float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
				return t;
			}
			
			float SimpleNoise(float2 UV)
			{
				float t = 0.0;
				float freq = pow( 2.0, float( 0 ) );
				float amp = pow( 0.5, float( 3 - 0 ) );
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(1));
				amp = pow(0.5, float(3-1));
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(2));
				amp = pow(0.5, float(3-2));
				t += valueNoise( UV/freq )*amp;
				return t;
			}
			
			half4 CalculateShadowMask343_g305(  )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				half4 shadowMask = inputData.shadowMask;
				#elif !defined (LIGHTMAP_ON)
				half4 shadowMask = unity_ProbesOcclusion;
				#else
				half4 shadowMask = half4(1, 1, 1, 1);
				#endif
				return shadowMask;
			}
			
			float3 AdditionalLightsLambertMask14x( float3 WorldPosition, float2 ScreenUV, float3 WorldNormal, float4 ShadowMask )
			{
				float3 Color = 0;
				#if defined(_ADDITIONAL_LIGHTS)
					#define SUM_LIGHTLAMBERT(Light)\
						half3 AttLightColor = Light.color * ( Light.distanceAttenuation * Light.shadowAttenuation );\
						Color += LightingLambert( AttLightColor, Light.direction, WorldNormal );
					InputData inputData = (InputData)0;
					inputData.normalizedScreenSpaceUV = ScreenUV;
					inputData.positionWS = WorldPosition;
					uint meshRenderingLayers = GetMeshRenderingLayer();
					uint pixelLightCount = GetAdditionalLightsCount();	
					#if USE_FORWARD_PLUS
					for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					{
						FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					}
					#endif
					
					LIGHT_LOOP_BEGIN( pixelLightCount )
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					LIGHT_LOOP_END
				#endif
				return Color;
			}
			
			half4 CalculateShadowMask343_g302(  )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				half4 shadowMask = inputData.shadowMask;
				#elif !defined (LIGHTMAP_ON)
				half4 shadowMask = unity_ProbesOcclusion;
				#else
				half4 shadowMask = half4(1, 1, 1, 1);
				#endif
				return shadowMask;
			}
			
			float3 ASESafeNormalize(float3 inVec)
			{
				float dp3 = max(1.175494351e-38, dot(inVec, inVec));
				return inVec* rsqrt(dp3);
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord5.x = eyeDepth;
				
				output.ase_texcoord3 = input.ase_texcoord;
				output.ase_texcoord4 = input.positionOS;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord5.yzw = 0;

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

				float2 texCoord244 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float localSimplexNoise_float2_g297 = ( 0.0 );
				float Particle_Stable_Random157 = ( ( input.ase_texcoord3.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float4 temp_output_10_0_g295 = ( float4( ( input.ase_texcoord4.xyz * _DistortionNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _DistortionNoiseOffset + Particle_Stable_Random157 ) + ( _DistortionNoiseAnimation * _TimeParameters.x ) ) );
				float3 position2_g297 = (temp_output_10_0_g295).xyz;
				float angle2_g297 = (temp_output_10_0_g295).w;
				float octaves2_g297 = _DistortionNoiseOctaves;
				float noise2_g297 = 0.0;
				float3 gradient2_g297 = float3( 0,0,0 );
				SimplexNoise_float( position2_g297 , angle2_g297 , octaves2_g297 , noise2_g297 , gradient2_g297 );
				#ifdef _DISTORTIONNOISEENABLED_ON
				float3 staticSwitch233 = ( gradient2_g297 * _DistortionNoise );
				#else
				float3 staticSwitch233 = float3( 0,0,0 );
				#endif
				float3 Distortion_Noise234 = staticSwitch233;
				float4 tex2DNode10 = tex2D( _BaseMap, ( float3( texCoord244 ,  0.0 ) + Distortion_Noise234 ).xy );
				float4 temp_output_10_0_g288 = ( float4( ( input.ase_texcoord4.xyz * _AdditiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _AdditiveNoiseOffset + Particle_Stable_Random157 ) + ( _AdditiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_154_0 = (temp_output_10_0_g288).xyz;
				float simpleNoise175 = SimpleNoise( temp_output_154_0.xy );
				#ifdef _ADDITIVENOISEENABLED_ON
				float staticSwitch176 = ( pow( simpleNoise175 , _AdditiveNoisePower ) * _AdditiveNoise );
				#else
				float staticSwitch176 = 0.0;
				#endif
				float Additive_Noise155 = staticSwitch176;
				float4 temp_output_40_0 = ( ( tex2DNode10 + ( Additive_Noise155 * _AdditiveNoisetoRGB * float4( 1, 1, 1, 0 ) ) ) * input.ase_color * _Colour );
				float3 Coloured_Base_RGB48 = (temp_output_40_0).rgb;
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float3 WorldPosition288_g305 = PositionWS;
				float3 WorldPosition299_g305 = WorldPosition288_g305;
				float2 ScreenUV286_g305 = (ScreenPosNorm).xy;
				float2 ScreenUV299_g305 = ScreenUV286_g305;
				float2 uv_NormalMap = input.ase_texcoord3.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				float3 unpack123 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), _NormalMapScale );
				unpack123.z = lerp( 1, unpack123.z, saturate(_NormalMapScale) );
				float3 tex2DNode123 = unpack123;
				float3 Normal_Map125 = tex2DNode123;
				float3 temp_output_111_0_g308 = ddx( PositionWS );
				float3 temp_output_113_0_g308 = cross( ddy( PositionWS ) , NormalWS );
				float dotResult115_g308 = dot( temp_output_111_0_g308 , temp_output_113_0_g308 );
				float temp_output_20_0_g308 = Additive_Noise155;
				float3 normalizeResult130_g308 = normalize( ( ( abs( dotResult115_g308 ) * NormalWS ) - ( _AdditiveNoiseNormalfromHeight * float3( 0.05,0.05,0.05 ) * sign( dotResult115_g308 ) * ( ( ddx( temp_output_20_0_g308 ) * temp_output_113_0_g308 ) + ( ddy( temp_output_20_0_g308 ) * cross( NormalWS , temp_output_111_0_g308 ) ) ) ) ) );
				float3x3 ase_worldToTangent = float3x3( TangentWS, BitangentWS, NormalWS );
				float3 worldToTangentDir42_g308 = mul( ase_worldToTangent, normalizeResult130_g308 );
				#ifdef _ADDITIVENOISEENABLED_ON
				float3 staticSwitch203 = BlendNormal( worldToTangentDir42_g308 , Normal_Map125 );
				#else
				float3 staticSwitch203 = Normal_Map125;
				#endif
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 tanNormal280_g305 = staticSwitch203;
				float3 worldNormal280_g305 = normalize( float3( dot( tanToWorld0, tanNormal280_g305 ), dot( tanToWorld1, tanNormal280_g305 ), dot( tanToWorld2, tanNormal280_g305 ) ) );
				float3 WorldNormal281_g305 = worldNormal280_g305;
				float3 WorldNormal299_g305 = WorldNormal281_g305;
				half4 localCalculateShadowMask343_g305 = CalculateShadowMask343_g305();
				float4 ShadowMask360_g305 = localCalculateShadowMask343_g305;
				float4 ShadowMask299_g305 = ShadowMask360_g305;
				float3 localAdditionalLightsLambertMask14x299_g305 = AdditionalLightsLambertMask14x( WorldPosition299_g305 , ScreenUV299_g305 , WorldNormal299_g305 , ShadowMask299_g305 );
				float3 temp_output_87_0 = localAdditionalLightsLambertMask14x299_g305;
				float3 WorldPosition288_g302 = PositionWS;
				float3 WorldPosition299_g302 = WorldPosition288_g302;
				float2 ScreenUV286_g302 = (ScreenPosNorm).xy;
				float2 ScreenUV299_g302 = ScreenUV286_g302;
				float3 Negative_Normal_Map208 = -tex2DNode123;
				float3 temp_output_111_0_g307 = ddx( PositionWS );
				float3 temp_output_113_0_g307 = cross( ddy( PositionWS ) , NormalWS );
				float dotResult115_g307 = dot( temp_output_111_0_g307 , temp_output_113_0_g307 );
				float temp_output_20_0_g307 = ( ( 1.0 - Additive_Noise155 ) * 0.5 );
				float3 normalizeResult130_g307 = normalize( ( ( abs( dotResult115_g307 ) * NormalWS ) - ( _AdditiveNoiseNormalfromHeight * float3( 0.05,0.05,0.05 ) * sign( dotResult115_g307 ) * ( ( ddx( temp_output_20_0_g307 ) * temp_output_113_0_g307 ) + ( ddy( temp_output_20_0_g307 ) * cross( NormalWS , temp_output_111_0_g307 ) ) ) ) ) );
				float3 worldToTangentDir42_g307 = mul( ase_worldToTangent, normalizeResult130_g307 );
				#ifdef _ADDITIVENOISEENABLED_ON
				float3 staticSwitch205 = BlendNormal( worldToTangentDir42_g307 , Negative_Normal_Map208 );
				#else
				float3 staticSwitch205 = Negative_Normal_Map208;
				#endif
				float3 tanNormal280_g302 = staticSwitch205;
				float3 worldNormal280_g302 = normalize( float3( dot( tanToWorld0, tanNormal280_g302 ), dot( tanToWorld1, tanNormal280_g302 ), dot( tanToWorld2, tanNormal280_g302 ) ) );
				float3 WorldNormal281_g302 = worldNormal280_g302;
				float3 WorldNormal299_g302 = WorldNormal281_g302;
				half4 localCalculateShadowMask343_g302 = CalculateShadowMask343_g302();
				float4 ShadowMask360_g302 = localCalculateShadowMask343_g302;
				float4 ShadowMask299_g302 = ShadowMask360_g302;
				float3 localAdditionalLightsLambertMask14x299_g302 = AdditionalLightsLambertMask14x( WorldPosition299_g302 , ScreenUV299_g302 , WorldNormal299_g302 , ShadowMask299_g302 );
				float3 temp_output_88_0 = localAdditionalLightsLambertMask14x299_g302;
				float3 normalizeResult98 = ASESafeNormalize( temp_output_88_0 );
				float Base_R115 = ( tex2DNode10.r + Additive_Noise155 );
				#ifdef _ADDITIONALLIGHTSTRANSLUCENCYENABLED_ON
				float3 staticSwitch109 = ( temp_output_87_0 + ( normalizeResult98 * length( temp_output_88_0 ) * ( Base_R115 * _AdditionalLightsTranslucency ) ) );
				#else
				float3 staticSwitch109 = temp_output_87_0;
				#endif
				float3 Additional_Lights113 = ( staticSwitch109 * _AdditionalLights );
				float3 Lighting96 = ( ( ase_lightColor.rgb * ase_lightColor.a * _MainLight ) + Additional_Lights113 );
				#ifdef _LIGHTINGENABLED_ON
				float3 staticSwitch119 = ( Coloured_Base_RGB48 + Lighting96 );
				#else
				float3 staticSwitch119 = Coloured_Base_RGB48;
				#endif
				float3 Colour86 = staticSwitch119;
				
				float Coloured_Base_Alpha47 = (temp_output_40_0).a;
				float Particle_Alpha_Erosion_over_Lifetime135 = ( input.ase_texcoord3.z * _ParticleAlphaErosionoverLifetime );
				float4 temp_output_10_0_g298 = ( float4( ( input.ase_texcoord4.xyz * _SubtractiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _SubtractiveNoiseOffset + Particle_Stable_Random157 ) + ( _SubtractiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_187_0 = (temp_output_10_0_g298).xyz;
				float simpleNoise196 = SimpleNoise( temp_output_187_0.xy );
				#ifdef _SUBTRACTIVENOISEENABLED_ON
				float staticSwitch192 = ( pow( simpleNoise196 , _SubtractiveNoisePower ) * _SubtractiveNoise );
				#else
				float staticSwitch192 = 0.0;
				#endif
				float Subtractive_Noise193 = staticSwitch192;
				float temp_output_7_0_g300 = 0.0;
				float temp_output_23_0_g300 = 1.0;
				float screenDepth14 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth14 = saturate( abs( ( screenDepth14 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				float temp_output_20_0_g300 = distanceDepth14;
				float temp_output_4_0_g300 = _DepthFadePower;
				float smoothstepResult22_g300 = smoothstep( temp_output_7_0_g300 , temp_output_23_0_g300 , pow( temp_output_20_0_g300 , temp_output_4_0_g300 ));
				float Depth_Fade143 = smoothstepResult22_g300;
				float temp_output_7_0_g301 = 0.0;
				float temp_output_23_0_g301 = 1.0;
				float eyeDepth = input.ase_texcoord5.x;
				float cameraDepthFade148 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g301 = cameraDepthFade148;
				float temp_output_4_0_g301 = _CameraDepthFadePower;
				float smoothstepResult22_g301 = smoothstep( temp_output_7_0_g301 , temp_output_23_0_g301 , pow( temp_output_20_0_g301 , temp_output_4_0_g301 ));
				float Camera_Depth_Fade151 = smoothstepResult22_g301;
				float Alpha51 = ( saturate( ( ( ( ( pow( Coloured_Base_Alpha47 , _BaseAlphaPower ) * _BaseAlpha ) + ( Additive_Noise155 * Coloured_Base_Alpha47 ) ) - ( _AlphaErosion + Particle_Alpha_Erosion_over_Lifetime135 ) ) - Subtractive_Noise193 ) ) * Depth_Fade143 * Camera_Depth_Fade151 );
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = Colour86;
				float3 Normal = float3(0, 0, 1);
				float Alpha = Alpha51;
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
			#define REQUIRE_DEPTH_TEXTURE 1


			

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

			#include "../../VFXToolkit/Shaders/_Includes/Noise.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DISTORTIONNOISEENABLED_ON
			#pragma shader_feature_local _ADDITIVENOISEENABLED_ON
			#pragma shader_feature_local _SUBTRACTIVENOISEENABLED_ON


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
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DistortionNoiseOffset;
			float4 _DistortionNoiseAnimation;
			float4 _NormalMap_ST;
			float4 _AdditiveNoiseOffset;
			float4 _AdditiveNoiseAnimation;
			float4 _Colour;
			float4 _SubtractiveNoiseAnimation;
			float4 _SubtractiveNoiseOffset;
			float _DistortionNoiseScale;
			float _AlphaErosion;
			float _ParticleAlphaErosionoverLifetime;
			float _SubtractiveNoiseScale;
			float _SubtractiveNoisePower;
			float _SubtractiveNoise;
			float _DepthFade;
			float _DepthFadePower;
			float _CameraDepthFadeLength;
			float _BaseAlpha;
			float _BaseAlphaPower;
			float _AdditiveNoiseNormalfromHeight;
			float _AdditionalLightsTranslucency;
			float _CameraDepthFadeOffset;
			float _NormalMapScale;
			float _MainLight;
			float _AdditiveNoisetoRGB;
			float _AdditiveNoise;
			float _AdditiveNoisePower;
			float _AdditiveNoiseScale;
			float _DistortionNoise;
			float _DistortionNoiseOctaves;
			float _ParticleRandomization;
			float _AdditionalLights;
			float _CameraDepthFadePower;
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

			sampler2D _BaseMap;


			float3 _LightDirection;
			float3 _LightPosition;

			inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }
			inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }
			inline float valueNoise (float2 uv)
			{
				float2 i = floor(uv);
				float2 f = frac( uv );
				f = f* f * (3.0 - 2.0 * f);
				uv = abs( frac(uv) - 0.5);
				float2 c0 = i + float2( 0.0, 0.0 );
				float2 c1 = i + float2( 1.0, 0.0 );
				float2 c2 = i + float2( 0.0, 1.0 );
				float2 c3 = i + float2( 1.0, 1.0 );
				float r0 = noise_randomValue( c0 );
				float r1 = noise_randomValue( c1 );
				float r2 = noise_randomValue( c2 );
				float r3 = noise_randomValue( c3 );
				float bottomOfGrid = noise_interpolate( r0, r1, f.x );
				float topOfGrid = noise_interpolate( r2, r3, f.x );
				float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
				return t;
			}
			
			float SimpleNoise(float2 UV)
			{
				float t = 0.0;
				float freq = pow( 2.0, float( 0 ) );
				float amp = pow( 0.5, float( 3 - 0 ) );
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(1));
				amp = pow(0.5, float(3-1));
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(2));
				amp = pow(0.5, float(3-2));
				t += valueNoise( UV/freq )*amp;
				return t;
			}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord2.x = eyeDepth;
				
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.positionOS;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord2.yzw = 0;

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

				float2 texCoord244 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float localSimplexNoise_float2_g297 = ( 0.0 );
				float Particle_Stable_Random157 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float4 temp_output_10_0_g295 = ( float4( ( input.ase_texcoord1.xyz * _DistortionNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _DistortionNoiseOffset + Particle_Stable_Random157 ) + ( _DistortionNoiseAnimation * _TimeParameters.x ) ) );
				float3 position2_g297 = (temp_output_10_0_g295).xyz;
				float angle2_g297 = (temp_output_10_0_g295).w;
				float octaves2_g297 = _DistortionNoiseOctaves;
				float noise2_g297 = 0.0;
				float3 gradient2_g297 = float3( 0,0,0 );
				SimplexNoise_float( position2_g297 , angle2_g297 , octaves2_g297 , noise2_g297 , gradient2_g297 );
				#ifdef _DISTORTIONNOISEENABLED_ON
				float3 staticSwitch233 = ( gradient2_g297 * _DistortionNoise );
				#else
				float3 staticSwitch233 = float3( 0,0,0 );
				#endif
				float3 Distortion_Noise234 = staticSwitch233;
				float4 tex2DNode10 = tex2D( _BaseMap, ( float3( texCoord244 ,  0.0 ) + Distortion_Noise234 ).xy );
				float4 temp_output_10_0_g288 = ( float4( ( input.ase_texcoord1.xyz * _AdditiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _AdditiveNoiseOffset + Particle_Stable_Random157 ) + ( _AdditiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_154_0 = (temp_output_10_0_g288).xyz;
				float simpleNoise175 = SimpleNoise( temp_output_154_0.xy );
				#ifdef _ADDITIVENOISEENABLED_ON
				float staticSwitch176 = ( pow( simpleNoise175 , _AdditiveNoisePower ) * _AdditiveNoise );
				#else
				float staticSwitch176 = 0.0;
				#endif
				float Additive_Noise155 = staticSwitch176;
				float4 temp_output_40_0 = ( ( tex2DNode10 + ( Additive_Noise155 * _AdditiveNoisetoRGB * float4( 1, 1, 1, 0 ) ) ) * input.ase_color * _Colour );
				float Coloured_Base_Alpha47 = (temp_output_40_0).a;
				float Particle_Alpha_Erosion_over_Lifetime135 = ( input.ase_texcoord.z * _ParticleAlphaErosionoverLifetime );
				float4 temp_output_10_0_g298 = ( float4( ( input.ase_texcoord1.xyz * _SubtractiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _SubtractiveNoiseOffset + Particle_Stable_Random157 ) + ( _SubtractiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_187_0 = (temp_output_10_0_g298).xyz;
				float simpleNoise196 = SimpleNoise( temp_output_187_0.xy );
				#ifdef _SUBTRACTIVENOISEENABLED_ON
				float staticSwitch192 = ( pow( simpleNoise196 , _SubtractiveNoisePower ) * _SubtractiveNoise );
				#else
				float staticSwitch192 = 0.0;
				#endif
				float Subtractive_Noise193 = staticSwitch192;
				float temp_output_7_0_g300 = 0.0;
				float temp_output_23_0_g300 = 1.0;
				float screenDepth14 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth14 = saturate( abs( ( screenDepth14 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				float temp_output_20_0_g300 = distanceDepth14;
				float temp_output_4_0_g300 = _DepthFadePower;
				float smoothstepResult22_g300 = smoothstep( temp_output_7_0_g300 , temp_output_23_0_g300 , pow( temp_output_20_0_g300 , temp_output_4_0_g300 ));
				float Depth_Fade143 = smoothstepResult22_g300;
				float temp_output_7_0_g301 = 0.0;
				float temp_output_23_0_g301 = 1.0;
				float eyeDepth = input.ase_texcoord2.x;
				float cameraDepthFade148 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g301 = cameraDepthFade148;
				float temp_output_4_0_g301 = _CameraDepthFadePower;
				float smoothstepResult22_g301 = smoothstep( temp_output_7_0_g301 , temp_output_23_0_g301 , pow( temp_output_20_0_g301 , temp_output_4_0_g301 ));
				float Camera_Depth_Fade151 = smoothstepResult22_g301;
				float Alpha51 = ( saturate( ( ( ( ( pow( Coloured_Base_Alpha47 , _BaseAlphaPower ) * _BaseAlpha ) + ( Additive_Noise155 * Coloured_Base_Alpha47 ) ) - ( _AlphaErosion + Particle_Alpha_Erosion_over_Lifetime135 ) ) - Subtractive_Noise193 ) ) * Depth_Fade143 * Camera_Depth_Fade151 );
				

				float Alpha = Alpha51;
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
			#define REQUIRE_DEPTH_TEXTURE 1


			

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

			#include "../../VFXToolkit/Shaders/_Includes/Noise.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DISTORTIONNOISEENABLED_ON
			#pragma shader_feature_local _ADDITIVENOISEENABLED_ON
			#pragma shader_feature_local _SUBTRACTIVENOISEENABLED_ON


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
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DistortionNoiseOffset;
			float4 _DistortionNoiseAnimation;
			float4 _NormalMap_ST;
			float4 _AdditiveNoiseOffset;
			float4 _AdditiveNoiseAnimation;
			float4 _Colour;
			float4 _SubtractiveNoiseAnimation;
			float4 _SubtractiveNoiseOffset;
			float _DistortionNoiseScale;
			float _AlphaErosion;
			float _ParticleAlphaErosionoverLifetime;
			float _SubtractiveNoiseScale;
			float _SubtractiveNoisePower;
			float _SubtractiveNoise;
			float _DepthFade;
			float _DepthFadePower;
			float _CameraDepthFadeLength;
			float _BaseAlpha;
			float _BaseAlphaPower;
			float _AdditiveNoiseNormalfromHeight;
			float _AdditionalLightsTranslucency;
			float _CameraDepthFadeOffset;
			float _NormalMapScale;
			float _MainLight;
			float _AdditiveNoisetoRGB;
			float _AdditiveNoise;
			float _AdditiveNoisePower;
			float _AdditiveNoiseScale;
			float _DistortionNoise;
			float _DistortionNoiseOctaves;
			float _ParticleRandomization;
			float _AdditionalLights;
			float _CameraDepthFadePower;
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

			sampler2D _BaseMap;


			inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }
			inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }
			inline float valueNoise (float2 uv)
			{
				float2 i = floor(uv);
				float2 f = frac( uv );
				f = f* f * (3.0 - 2.0 * f);
				uv = abs( frac(uv) - 0.5);
				float2 c0 = i + float2( 0.0, 0.0 );
				float2 c1 = i + float2( 1.0, 0.0 );
				float2 c2 = i + float2( 0.0, 1.0 );
				float2 c3 = i + float2( 1.0, 1.0 );
				float r0 = noise_randomValue( c0 );
				float r1 = noise_randomValue( c1 );
				float r2 = noise_randomValue( c2 );
				float r3 = noise_randomValue( c3 );
				float bottomOfGrid = noise_interpolate( r0, r1, f.x );
				float topOfGrid = noise_interpolate( r2, r3, f.x );
				float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
				return t;
			}
			
			float SimpleNoise(float2 UV)
			{
				float t = 0.0;
				float freq = pow( 2.0, float( 0 ) );
				float amp = pow( 0.5, float( 3 - 0 ) );
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(1));
				amp = pow(0.5, float(3-1));
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(2));
				amp = pow(0.5, float(3-2));
				t += valueNoise( UV/freq )*amp;
				return t;
			}
			

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

				float4 ase_positionCS = TransformObjectToHClip( ( input.positionOS ).xyz );
				float4 screenPos = ComputeScreenPos( ase_positionCS );
				output.ase_texcoord2 = screenPos;
				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord3.x = eyeDepth;
				
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.positionOS;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.yzw = 0;

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

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 texCoord244 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float localSimplexNoise_float2_g297 = ( 0.0 );
				float Particle_Stable_Random157 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float4 temp_output_10_0_g295 = ( float4( ( input.ase_texcoord1.xyz * _DistortionNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _DistortionNoiseOffset + Particle_Stable_Random157 ) + ( _DistortionNoiseAnimation * _TimeParameters.x ) ) );
				float3 position2_g297 = (temp_output_10_0_g295).xyz;
				float angle2_g297 = (temp_output_10_0_g295).w;
				float octaves2_g297 = _DistortionNoiseOctaves;
				float noise2_g297 = 0.0;
				float3 gradient2_g297 = float3( 0,0,0 );
				SimplexNoise_float( position2_g297 , angle2_g297 , octaves2_g297 , noise2_g297 , gradient2_g297 );
				#ifdef _DISTORTIONNOISEENABLED_ON
				float3 staticSwitch233 = ( gradient2_g297 * _DistortionNoise );
				#else
				float3 staticSwitch233 = float3( 0,0,0 );
				#endif
				float3 Distortion_Noise234 = staticSwitch233;
				float4 tex2DNode10 = tex2D( _BaseMap, ( float3( texCoord244 ,  0.0 ) + Distortion_Noise234 ).xy );
				float4 temp_output_10_0_g288 = ( float4( ( input.ase_texcoord1.xyz * _AdditiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _AdditiveNoiseOffset + Particle_Stable_Random157 ) + ( _AdditiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_154_0 = (temp_output_10_0_g288).xyz;
				float simpleNoise175 = SimpleNoise( temp_output_154_0.xy );
				#ifdef _ADDITIVENOISEENABLED_ON
				float staticSwitch176 = ( pow( simpleNoise175 , _AdditiveNoisePower ) * _AdditiveNoise );
				#else
				float staticSwitch176 = 0.0;
				#endif
				float Additive_Noise155 = staticSwitch176;
				float4 temp_output_40_0 = ( ( tex2DNode10 + ( Additive_Noise155 * _AdditiveNoisetoRGB * float4( 1, 1, 1, 0 ) ) ) * input.ase_color * _Colour );
				float Coloured_Base_Alpha47 = (temp_output_40_0).a;
				float Particle_Alpha_Erosion_over_Lifetime135 = ( input.ase_texcoord.z * _ParticleAlphaErosionoverLifetime );
				float4 temp_output_10_0_g298 = ( float4( ( input.ase_texcoord1.xyz * _SubtractiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _SubtractiveNoiseOffset + Particle_Stable_Random157 ) + ( _SubtractiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_187_0 = (temp_output_10_0_g298).xyz;
				float simpleNoise196 = SimpleNoise( temp_output_187_0.xy );
				#ifdef _SUBTRACTIVENOISEENABLED_ON
				float staticSwitch192 = ( pow( simpleNoise196 , _SubtractiveNoisePower ) * _SubtractiveNoise );
				#else
				float staticSwitch192 = 0.0;
				#endif
				float Subtractive_Noise193 = staticSwitch192;
				float temp_output_7_0_g300 = 0.0;
				float temp_output_23_0_g300 = 1.0;
				float4 screenPos = input.ase_texcoord2;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float screenDepth14 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth14 = saturate( abs( ( screenDepth14 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				float temp_output_20_0_g300 = distanceDepth14;
				float temp_output_4_0_g300 = _DepthFadePower;
				float smoothstepResult22_g300 = smoothstep( temp_output_7_0_g300 , temp_output_23_0_g300 , pow( temp_output_20_0_g300 , temp_output_4_0_g300 ));
				float Depth_Fade143 = smoothstepResult22_g300;
				float temp_output_7_0_g301 = 0.0;
				float temp_output_23_0_g301 = 1.0;
				float eyeDepth = input.ase_texcoord3.x;
				float cameraDepthFade148 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g301 = cameraDepthFade148;
				float temp_output_4_0_g301 = _CameraDepthFadePower;
				float smoothstepResult22_g301 = smoothstep( temp_output_7_0_g301 , temp_output_23_0_g301 , pow( temp_output_20_0_g301 , temp_output_4_0_g301 ));
				float Camera_Depth_Fade151 = smoothstepResult22_g301;
				float Alpha51 = ( saturate( ( ( ( ( pow( Coloured_Base_Alpha47 , _BaseAlphaPower ) * _BaseAlpha ) + ( Additive_Noise155 * Coloured_Base_Alpha47 ) ) - ( _AlphaErosion + Particle_Alpha_Erosion_over_Lifetime135 ) ) - Subtractive_Noise193 ) ) * Depth_Fade143 * Camera_Depth_Fade151 );
				

				surfaceDescription.Alpha = Alpha51;
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
			#define REQUIRE_DEPTH_TEXTURE 1


			

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

			#include "../../VFXToolkit/Shaders/_Includes/Noise.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DISTORTIONNOISEENABLED_ON
			#pragma shader_feature_local _ADDITIVENOISEENABLED_ON
			#pragma shader_feature_local _SUBTRACTIVENOISEENABLED_ON


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
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DistortionNoiseOffset;
			float4 _DistortionNoiseAnimation;
			float4 _NormalMap_ST;
			float4 _AdditiveNoiseOffset;
			float4 _AdditiveNoiseAnimation;
			float4 _Colour;
			float4 _SubtractiveNoiseAnimation;
			float4 _SubtractiveNoiseOffset;
			float _DistortionNoiseScale;
			float _AlphaErosion;
			float _ParticleAlphaErosionoverLifetime;
			float _SubtractiveNoiseScale;
			float _SubtractiveNoisePower;
			float _SubtractiveNoise;
			float _DepthFade;
			float _DepthFadePower;
			float _CameraDepthFadeLength;
			float _BaseAlpha;
			float _BaseAlphaPower;
			float _AdditiveNoiseNormalfromHeight;
			float _AdditionalLightsTranslucency;
			float _CameraDepthFadeOffset;
			float _NormalMapScale;
			float _MainLight;
			float _AdditiveNoisetoRGB;
			float _AdditiveNoise;
			float _AdditiveNoisePower;
			float _AdditiveNoiseScale;
			float _DistortionNoise;
			float _DistortionNoiseOctaves;
			float _ParticleRandomization;
			float _AdditionalLights;
			float _CameraDepthFadePower;
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

			sampler2D _BaseMap;


			inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }
			inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }
			inline float valueNoise (float2 uv)
			{
				float2 i = floor(uv);
				float2 f = frac( uv );
				f = f* f * (3.0 - 2.0 * f);
				uv = abs( frac(uv) - 0.5);
				float2 c0 = i + float2( 0.0, 0.0 );
				float2 c1 = i + float2( 1.0, 0.0 );
				float2 c2 = i + float2( 0.0, 1.0 );
				float2 c3 = i + float2( 1.0, 1.0 );
				float r0 = noise_randomValue( c0 );
				float r1 = noise_randomValue( c1 );
				float r2 = noise_randomValue( c2 );
				float r3 = noise_randomValue( c3 );
				float bottomOfGrid = noise_interpolate( r0, r1, f.x );
				float topOfGrid = noise_interpolate( r2, r3, f.x );
				float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
				return t;
			}
			
			float SimpleNoise(float2 UV)
			{
				float t = 0.0;
				float freq = pow( 2.0, float( 0 ) );
				float amp = pow( 0.5, float( 3 - 0 ) );
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(1));
				amp = pow(0.5, float(3-1));
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(2));
				amp = pow(0.5, float(3-2));
				t += valueNoise( UV/freq )*amp;
				return t;
			}
			

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

				float4 ase_positionCS = TransformObjectToHClip( ( input.positionOS ).xyz );
				float4 screenPos = ComputeScreenPos( ase_positionCS );
				output.ase_texcoord2 = screenPos;
				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord3.x = eyeDepth;
				
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord1 = input.positionOS;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.yzw = 0;

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

			half4 frag(PackedVaryings input ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 texCoord244 = input.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float localSimplexNoise_float2_g297 = ( 0.0 );
				float Particle_Stable_Random157 = ( ( input.ase_texcoord.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float4 temp_output_10_0_g295 = ( float4( ( input.ase_texcoord1.xyz * _DistortionNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _DistortionNoiseOffset + Particle_Stable_Random157 ) + ( _DistortionNoiseAnimation * _TimeParameters.x ) ) );
				float3 position2_g297 = (temp_output_10_0_g295).xyz;
				float angle2_g297 = (temp_output_10_0_g295).w;
				float octaves2_g297 = _DistortionNoiseOctaves;
				float noise2_g297 = 0.0;
				float3 gradient2_g297 = float3( 0,0,0 );
				SimplexNoise_float( position2_g297 , angle2_g297 , octaves2_g297 , noise2_g297 , gradient2_g297 );
				#ifdef _DISTORTIONNOISEENABLED_ON
				float3 staticSwitch233 = ( gradient2_g297 * _DistortionNoise );
				#else
				float3 staticSwitch233 = float3( 0,0,0 );
				#endif
				float3 Distortion_Noise234 = staticSwitch233;
				float4 tex2DNode10 = tex2D( _BaseMap, ( float3( texCoord244 ,  0.0 ) + Distortion_Noise234 ).xy );
				float4 temp_output_10_0_g288 = ( float4( ( input.ase_texcoord1.xyz * _AdditiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _AdditiveNoiseOffset + Particle_Stable_Random157 ) + ( _AdditiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_154_0 = (temp_output_10_0_g288).xyz;
				float simpleNoise175 = SimpleNoise( temp_output_154_0.xy );
				#ifdef _ADDITIVENOISEENABLED_ON
				float staticSwitch176 = ( pow( simpleNoise175 , _AdditiveNoisePower ) * _AdditiveNoise );
				#else
				float staticSwitch176 = 0.0;
				#endif
				float Additive_Noise155 = staticSwitch176;
				float4 temp_output_40_0 = ( ( tex2DNode10 + ( Additive_Noise155 * _AdditiveNoisetoRGB * float4( 1, 1, 1, 0 ) ) ) * input.ase_color * _Colour );
				float Coloured_Base_Alpha47 = (temp_output_40_0).a;
				float Particle_Alpha_Erosion_over_Lifetime135 = ( input.ase_texcoord.z * _ParticleAlphaErosionoverLifetime );
				float4 temp_output_10_0_g298 = ( float4( ( input.ase_texcoord1.xyz * _SubtractiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _SubtractiveNoiseOffset + Particle_Stable_Random157 ) + ( _SubtractiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_187_0 = (temp_output_10_0_g298).xyz;
				float simpleNoise196 = SimpleNoise( temp_output_187_0.xy );
				#ifdef _SUBTRACTIVENOISEENABLED_ON
				float staticSwitch192 = ( pow( simpleNoise196 , _SubtractiveNoisePower ) * _SubtractiveNoise );
				#else
				float staticSwitch192 = 0.0;
				#endif
				float Subtractive_Noise193 = staticSwitch192;
				float temp_output_7_0_g300 = 0.0;
				float temp_output_23_0_g300 = 1.0;
				float4 screenPos = input.ase_texcoord2;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float screenDepth14 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth14 = saturate( abs( ( screenDepth14 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				float temp_output_20_0_g300 = distanceDepth14;
				float temp_output_4_0_g300 = _DepthFadePower;
				float smoothstepResult22_g300 = smoothstep( temp_output_7_0_g300 , temp_output_23_0_g300 , pow( temp_output_20_0_g300 , temp_output_4_0_g300 ));
				float Depth_Fade143 = smoothstepResult22_g300;
				float temp_output_7_0_g301 = 0.0;
				float temp_output_23_0_g301 = 1.0;
				float eyeDepth = input.ase_texcoord3.x;
				float cameraDepthFade148 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g301 = cameraDepthFade148;
				float temp_output_4_0_g301 = _CameraDepthFadePower;
				float smoothstepResult22_g301 = smoothstep( temp_output_7_0_g301 , temp_output_23_0_g301 , pow( temp_output_20_0_g301 , temp_output_4_0_g301 ));
				float Camera_Depth_Fade151 = smoothstepResult22_g301;
				float Alpha51 = ( saturate( ( ( ( ( pow( Coloured_Base_Alpha47 , _BaseAlphaPower ) * _BaseAlpha ) + ( Additive_Noise155 * Coloured_Base_Alpha47 ) ) - ( _AlphaErosion + Particle_Alpha_Erosion_over_Lifetime135 ) ) - Subtractive_Noise193 ) ) * Depth_Fade143 * Camera_Depth_Fade151 );
				

				surfaceDescription.Alpha = Alpha51;
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
        	#define REQUIRE_DEPTH_TEXTURE 1


			

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

			#include "../../VFXToolkit/Shaders/_Includes/Noise.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _DISTORTIONNOISEENABLED_ON
			#pragma shader_feature_local _ADDITIVENOISEENABLED_ON
			#pragma shader_feature_local _SUBTRACTIVENOISEENABLED_ON


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
				half3 normalWS : TEXCOORD0;
				float4 tangentWS : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DistortionNoiseOffset;
			float4 _DistortionNoiseAnimation;
			float4 _NormalMap_ST;
			float4 _AdditiveNoiseOffset;
			float4 _AdditiveNoiseAnimation;
			float4 _Colour;
			float4 _SubtractiveNoiseAnimation;
			float4 _SubtractiveNoiseOffset;
			float _DistortionNoiseScale;
			float _AlphaErosion;
			float _ParticleAlphaErosionoverLifetime;
			float _SubtractiveNoiseScale;
			float _SubtractiveNoisePower;
			float _SubtractiveNoise;
			float _DepthFade;
			float _DepthFadePower;
			float _CameraDepthFadeLength;
			float _BaseAlpha;
			float _BaseAlphaPower;
			float _AdditiveNoiseNormalfromHeight;
			float _AdditionalLightsTranslucency;
			float _CameraDepthFadeOffset;
			float _NormalMapScale;
			float _MainLight;
			float _AdditiveNoisetoRGB;
			float _AdditiveNoise;
			float _AdditiveNoisePower;
			float _AdditiveNoiseScale;
			float _DistortionNoise;
			float _DistortionNoiseOctaves;
			float _ParticleRandomization;
			float _AdditionalLights;
			float _CameraDepthFadePower;
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

			sampler2D _BaseMap;


			inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }
			inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }
			inline float valueNoise (float2 uv)
			{
				float2 i = floor(uv);
				float2 f = frac( uv );
				f = f* f * (3.0 - 2.0 * f);
				uv = abs( frac(uv) - 0.5);
				float2 c0 = i + float2( 0.0, 0.0 );
				float2 c1 = i + float2( 1.0, 0.0 );
				float2 c2 = i + float2( 0.0, 1.0 );
				float2 c3 = i + float2( 1.0, 1.0 );
				float r0 = noise_randomValue( c0 );
				float r1 = noise_randomValue( c1 );
				float r2 = noise_randomValue( c2 );
				float r3 = noise_randomValue( c3 );
				float bottomOfGrid = noise_interpolate( r0, r1, f.x );
				float topOfGrid = noise_interpolate( r2, r3, f.x );
				float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
				return t;
			}
			
			float SimpleNoise(float2 UV)
			{
				float t = 0.0;
				float freq = pow( 2.0, float( 0 ) );
				float amp = pow( 0.5, float( 3 - 0 ) );
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(1));
				amp = pow(0.5, float(3-1));
				t += valueNoise( UV/freq )*amp;
				freq = pow(2.0, float(2));
				amp = pow(0.5, float(3-2));
				t += valueNoise( UV/freq )*amp;
				return t;
			}
			

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

				float3 objectToViewPos = TransformWorldToView( TransformObjectToWorld( input.positionOS.xyz ) );
				float eyeDepth = -objectToViewPos.z;
				output.ase_texcoord4.x = eyeDepth;
				
				output.ase_texcoord2 = input.ase_texcoord;
				output.ase_texcoord3 = input.positionOS;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord4.yzw = 0;
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

				float2 texCoord244 = input.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float localSimplexNoise_float2_g297 = ( 0.0 );
				float Particle_Stable_Random157 = ( ( input.ase_texcoord2.w - 0.5 ) * 100.0 * _ParticleRandomization );
				float4 temp_output_10_0_g295 = ( float4( ( input.ase_texcoord3.xyz * _DistortionNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _DistortionNoiseOffset + Particle_Stable_Random157 ) + ( _DistortionNoiseAnimation * _TimeParameters.x ) ) );
				float3 position2_g297 = (temp_output_10_0_g295).xyz;
				float angle2_g297 = (temp_output_10_0_g295).w;
				float octaves2_g297 = _DistortionNoiseOctaves;
				float noise2_g297 = 0.0;
				float3 gradient2_g297 = float3( 0,0,0 );
				SimplexNoise_float( position2_g297 , angle2_g297 , octaves2_g297 , noise2_g297 , gradient2_g297 );
				#ifdef _DISTORTIONNOISEENABLED_ON
				float3 staticSwitch233 = ( gradient2_g297 * _DistortionNoise );
				#else
				float3 staticSwitch233 = float3( 0,0,0 );
				#endif
				float3 Distortion_Noise234 = staticSwitch233;
				float4 tex2DNode10 = tex2D( _BaseMap, ( float3( texCoord244 ,  0.0 ) + Distortion_Noise234 ).xy );
				float4 temp_output_10_0_g288 = ( float4( ( input.ase_texcoord3.xyz * _AdditiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _AdditiveNoiseOffset + Particle_Stable_Random157 ) + ( _AdditiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_154_0 = (temp_output_10_0_g288).xyz;
				float simpleNoise175 = SimpleNoise( temp_output_154_0.xy );
				#ifdef _ADDITIVENOISEENABLED_ON
				float staticSwitch176 = ( pow( simpleNoise175 , _AdditiveNoisePower ) * _AdditiveNoise );
				#else
				float staticSwitch176 = 0.0;
				#endif
				float Additive_Noise155 = staticSwitch176;
				float4 temp_output_40_0 = ( ( tex2DNode10 + ( Additive_Noise155 * _AdditiveNoisetoRGB * float4( 1, 1, 1, 0 ) ) ) * input.ase_color * _Colour );
				float Coloured_Base_Alpha47 = (temp_output_40_0).a;
				float Particle_Alpha_Erosion_over_Lifetime135 = ( input.ase_texcoord2.z * _ParticleAlphaErosionoverLifetime );
				float4 temp_output_10_0_g298 = ( float4( ( input.ase_texcoord3.xyz * _SubtractiveNoiseScale * float3( 1,1,1 ) ) , 0.0 ) - ( ( _SubtractiveNoiseOffset + Particle_Stable_Random157 ) + ( _SubtractiveNoiseAnimation * _TimeParameters.x ) ) );
				float3 temp_output_187_0 = (temp_output_10_0_g298).xyz;
				float simpleNoise196 = SimpleNoise( temp_output_187_0.xy );
				#ifdef _SUBTRACTIVENOISEENABLED_ON
				float staticSwitch192 = ( pow( simpleNoise196 , _SubtractiveNoisePower ) * _SubtractiveNoise );
				#else
				float staticSwitch192 = 0.0;
				#endif
				float Subtractive_Noise193 = staticSwitch192;
				float temp_output_7_0_g300 = 0.0;
				float temp_output_23_0_g300 = 1.0;
				float screenDepth14 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ScreenPosNorm.xy ),_ZBufferParams);
				float distanceDepth14 = saturate( abs( ( screenDepth14 - LinearEyeDepth( ScreenPosNorm.z,_ZBufferParams ) ) / ( _DepthFade ) ) );
				float temp_output_20_0_g300 = distanceDepth14;
				float temp_output_4_0_g300 = _DepthFadePower;
				float smoothstepResult22_g300 = smoothstep( temp_output_7_0_g300 , temp_output_23_0_g300 , pow( temp_output_20_0_g300 , temp_output_4_0_g300 ));
				float Depth_Fade143 = smoothstepResult22_g300;
				float temp_output_7_0_g301 = 0.0;
				float temp_output_23_0_g301 = 1.0;
				float eyeDepth = input.ase_texcoord4.x;
				float cameraDepthFade148 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeLength);
				float temp_output_20_0_g301 = cameraDepthFade148;
				float temp_output_4_0_g301 = _CameraDepthFadePower;
				float smoothstepResult22_g301 = smoothstep( temp_output_7_0_g301 , temp_output_23_0_g301 , pow( temp_output_20_0_g301 , temp_output_4_0_g301 ));
				float Camera_Depth_Fade151 = smoothstepResult22_g301;
				float Alpha51 = ( saturate( ( ( ( ( pow( Coloured_Base_Alpha47 , _BaseAlphaPower ) * _BaseAlpha ) + ( Additive_Noise155 * Coloured_Base_Alpha47 ) ) - ( _AlphaErosion + Particle_Alpha_Erosion_over_Lifetime135 ) ) - Subtractive_Noise193 ) ) * Depth_Fade143 * Camera_Depth_Fade151 );
				

				float3 Normal = float3(0, 0, 1);
				float Alpha = Alpha51;
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
	

	

	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19912
{"type":"AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor","id":132,"pos":[-3328,-640],"params":["Inherit","False","0","4","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":163,"pos":[-2944,-384],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":164,"pos":[-2944,-256],"params":["Inherit","False","Property","_ParticleRandomization","Particle Randomization","14","0","Create","True","0","0","0","False","2","Header(Particle Settings)","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":165,"pos":[-2560,-384],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT","100","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":157,"pos":[-2304,-384],"params":["Inherit","False","Particle Stable Random","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":167,"pos":[-4224,3408],"params":["Inherit","False","157","Particle Stable Random","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":162,"pos":[-4224,3216],"params":["Inherit","False","Property","_AdditiveNoiseOffset","Additive Noise Offset","24","0","Create","True","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":236,"pos":[-8192,-176],"params":["Inherit","False","157","Particle Stable Random","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":237,"pos":[-8192,-368],"params":["Inherit","False","Property","_DistortionNoiseOffset","Distortion Noise Offset","40","0","Create","True","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor","id":159,"pos":[-4224,2816],"params":["Inherit","False","0","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":166,"pos":[-3840,3216],"params":["Inherit","False","2","2","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":160,"pos":[-4224,2960],"params":["Inherit","False","Property","_AdditiveNoiseScale","Additive Noise Scale","22","0","Create","True","0","0","0","False","0","False","Object","-1","","40","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":161,"pos":[-4224,3040],"params":["Inherit","False","Property","_AdditiveNoiseAnimation","Additive Noise Animation","23","0","Create","True","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor","id":238,"pos":[-8192,-768],"params":["Inherit","False","0","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":239,"pos":[-7808,-368],"params":["Inherit","False","2","2","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":240,"pos":[-8192,-624],"params":["Inherit","False","Property","_DistortionNoiseScale","Distortion Noise Scale","38","0","Create","True","0","0","0","False","0","False","Object","-1","","2","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":241,"pos":[-8192,-544],"params":["Inherit","False","Property","_DistortionNoiseAnimation","Distortion Noise Animation","39","0","Create","True","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":154,"pos":[-3584,2816],"params":["Inherit","False","Scale Tiling Offset Animation","-1","","288","650501f4d90f3194eb72a847e06cc2e3","1,21,0","6","4","FLOAT3","0,0,0","False","7","FLOAT","1","False","8","FLOAT3","1,1,1","False","9","FLOAT4","0,0,0,0","False","19","INT","0","False","12","FLOAT4","0,0,0,0","False","2","FLOAT3","0","FLOAT","15"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":242,"pos":[-7552,-768],"params":["Inherit","False","Scale Tiling Offset Animation","-1","","295","650501f4d90f3194eb72a847e06cc2e3","1,21,0","6","4","FLOAT3","0,0,0","False","7","FLOAT","1","False","8","FLOAT3","1,1,1","False","9","FLOAT4","0,0,0,0","False","19","INT","0","False","12","FLOAT4","0,0,0,0","False","2","FLOAT3","0","FLOAT","15"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":243,"pos":[-7552,-592],"params":["Inherit","False","Property","_DistortionNoiseOctaves","Distortion Noise Octaves","41","1","[IntRange]","Create","True","0","0","0","False","0","False","Object","-1","","1","0","1","8","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.NoiseGeneratorNode, AmplifyShaderEditor","id":175,"pos":[-3200,2560],"params":["Inherit","True","Simple","True","False","2","0","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":171,"pos":[-3200,2944],"params":["Inherit","False","Property","_AdditiveNoisePower","Additive Noise Power","26","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":235,"pos":[-7168,-768],"params":["Inherit","False","Simplex Noise","-1","","297","c68ae2e20c00ec54aaecd9d04797372e","0","3","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":231,"pos":[-6784,-640],"params":["Inherit","False","Property","_DistortionNoise","Distortion Noise","36","0","Create","True","0","0","0","False","2","Header(Distortion Noise)","Space(5)","False","Object","-1","","0","0","0","0.1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":170,"pos":[-2816,2816],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":169,"pos":[-2816,2944],"params":["Inherit","False","Property","_AdditiveNoise","Additive Noise","20","0","Create","True","0","0","0","False","2","Header(Additive Noise)","Space(5)","False","Object","-1","","1","0","0","2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":232,"pos":[-6400,-768],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":168,"pos":[-2432,2816],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":233,"pos":[-6144,-768],"params":["Inherit","False","Property","_DistortionNoiseEnabled","Distortion Noise Enabled","37","0","Create","True","0","0","0","False","0","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":176,"pos":[-2176,2816],"params":["Inherit","False","Property","_AdditiveNoiseEnabled","Additive Noise Enabled","21","0","Create","True","0","0","0","False","0","False","","0","1","1","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":234,"pos":[-5760,-768],"params":["Inherit","False","Distortion Noise","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":155,"pos":[-1792,2816],"params":["Inherit","False","Additive Noise","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":244,"pos":[-5504,-1024],"params":["Inherit","False","0","-1","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":246,"pos":[-5504,-896],"params":["Inherit","False","234","Distortion Noise","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":218,"pos":[-4992,-816],"params":["Inherit","False","155","Additive Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":220,"pos":[-4992,-656],"params":["Inherit","False","Constant","_Vector0","Vector 0","35","0","Create","True","0","0","0","False","0","False","Object","-1","","1,1,1,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":223,"pos":[-4992,-736],"params":["Inherit","False","Property","_AdditiveNoisetoRGB","Additive Noise to RGB","27","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":245,"pos":[-5248,-1024],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":10,"pos":[-4992,-1024],"params":["Inherit","True","Property","_BaseMap","Base Map","3","0","Create","True","0","0","0","False","2","Header(Texture)","Space(5)","False","","-1","e727c3978c253c54ea6bc105a8da932e","e727c3978c253c54ea6bc105a8da932e","True","0","False","white","Auto","False","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":219,"pos":[-4480,-816],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT4","0,0,0,0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":182,"pos":[-4224,4176],"params":["Inherit","False","157","Particle Stable Random","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":181,"pos":[-4224,3984],"params":["Inherit","False","Property","_SubtractiveNoiseOffset","Subtractive Noise Offset","33","0","Create","True","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor","id":43,"pos":[-4224,-768],"params":["Inherit","False","0","5","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":122,"pos":[-4224,-592],"params":["Inherit","False","Property","_Colour","Colour","0","1","[HDR]","Create","True","0","0","0","False","2","Header(Colour)","Space(5)","False","Object","-1","","1,1,1,1","0,0,0,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":217,"pos":[-4224,-1024],"params":["Inherit","False","2","2","0","COLOR","0,0,0,0","False","1","FLOAT4","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor","id":183,"pos":[-4224,3584],"params":["Inherit","False","0","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":185,"pos":[-3840,3984],"params":["Inherit","False","2","2","0","FLOAT4","0,0,0,0","False","1","FLOAT","0","False","1","FLOAT4","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":186,"pos":[-4224,3728],"params":["Inherit","False","Property","_SubtractiveNoiseScale","Subtractive Noise Scale","31","0","Create","True","0","0","0","False","0","False","Object","-1","","40","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor","id":184,"pos":[-4224,3808],"params":["Inherit","False","Property","_SubtractiveNoiseAnimation","Subtractive Noise Animation","32","0","Create","True","0","0","0","False","0","False","Object","-1","","0,0,0,0","0,0,0,0","0","5","FLOAT4","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":40,"pos":[-3584,-1024],"params":["Inherit","False","3","3","0","COLOR","0,0,0,0","False","1","COLOR","0,0,0,0","False","2","COLOR","0,0,0,0","False","1","COLOR","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":187,"pos":[-3584,3584],"params":["Inherit","False","Scale Tiling Offset Animation","-1","","298","650501f4d90f3194eb72a847e06cc2e3","1,21,0","6","4","FLOAT3","0,0,0","False","7","FLOAT","1","False","8","FLOAT3","1,1,1","False","9","FLOAT4","0,0,0,0","False","19","INT","0","False","12","FLOAT4","0,0,0,0","False","2","FLOAT3","0","FLOAT","15"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":46,"pos":[-3328,-896],"params":["Inherit","False","False","False","False","True","1","0","COLOR","0,0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":133,"pos":[-2944,-512],"params":["Inherit","False","Property","_ParticleAlphaErosionoverLifetime","Particle Alpha Erosion over Lifetime","2","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":188,"pos":[-3200,3712],"params":["Inherit","False","Property","_SubtractiveNoisePower","Subtractive Noise Power","35","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":47,"pos":[-3072,-896],"params":["Inherit","False","Coloured Base Alpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.NoiseGeneratorNode, AmplifyShaderEditor","id":196,"pos":[-3200,3328],"params":["Inherit","True","Simple","True","False","2","0","FLOAT2","0,0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":134,"pos":[-2560,-640],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":189,"pos":[-2816,3584],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":190,"pos":[-2816,3712],"params":["Inherit","False","Property","_SubtractiveNoise","Subtractive Noise","29","0","Create","True","0","0","0","False","2","Header(Subtractive Noise)","Space(5)","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":50,"pos":[-4352,384],"params":["Inherit","False","47","Coloured Base Alpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":141,"pos":[-4352,512],"params":["Inherit","False","Property","_BaseAlphaPower","Base Alpha Power","5","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0.5","2","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":135,"pos":[-2304,-640],"params":["Inherit","False","Particle Alpha Erosion over Lifetime","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":191,"pos":[-2432,3584],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":214,"pos":[-4352,720],"params":["Inherit","False","47","Coloured Base Alpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":180,"pos":[-4352,640],"params":["Inherit","False","155","Additive Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":142,"pos":[-3968,384],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":227,"pos":[-3968,512],"params":["Inherit","False","Property","_BaseAlpha","Base Alpha","4","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":146,"pos":[-2944,80],"params":["Inherit","False","Property","_CameraDepthFadeOffset","Camera Depth Fade Offset","12","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":147,"pos":[-2944,0],"params":["Inherit","False","Property","_CameraDepthFadeLength","Camera Depth Fade Length","11","0","Create","True","0","0","0","False","2","Header(Camera Depth Fade)","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":16,"pos":[-4224,0],"params":["Inherit","False","Property","_DepthFade","Depth Fade","9","0","Create","True","0","0","0","False","2","Header(Depth Fade)","Space(5)","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":192,"pos":[-2176,3584],"params":["Inherit","False","Property","_SubtractiveNoiseEnabled","Subtractive Noise Enabled","30","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT","0","False","0","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","4","FLOAT","0","False","5","FLOAT","0","False","6","FLOAT","0","False","7","FLOAT","0","False","8","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":213,"pos":[-3584,640],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":138,"pos":[-3328,592],"params":["Inherit","False","135","Particle Alpha Erosion over Lifetime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":130,"pos":[-3328,512],"params":["Inherit","False","Property","_AlphaErosion","Alpha Erosion","1","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":226,"pos":[-3584,384],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":121,"pos":[-3968,128],"params":["Inherit","False","Property","_DepthFadePower","Depth Fade Power","10","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DepthFade, AmplifyShaderEditor","id":14,"pos":[-3968,0],"params":["Inherit","False","True","True","True","2","1","FLOAT3","0,0,0","False","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CameraDepthFade, AmplifyShaderEditor","id":148,"pos":[-2560,0],"params":["Inherit","False","3","2","FLOAT3","0,0,0","False","0","FLOAT","1","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":149,"pos":[-2560,128],"params":["Inherit","False","Property","_CameraDepthFadePower","Camera Depth Fade Power","13","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":193,"pos":[-1792,3584],"params":["Inherit","False","Subtractive Noise","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":212,"pos":[-3328,384],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":137,"pos":[-2944,512],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":120,"pos":[-3584,0],"params":["Inherit","False","Power Smoothstep","-1","","300","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":150,"pos":[-2176,0],"params":["Inherit","False","Power Smoothstep","-1","","301","eaa8bfb6a4986cb418a1675cea297eed","1,24,0","4","20","FLOAT","1","False","4","FLOAT","1","False","7","FLOAT","0","False","23","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":129,"pos":[-2688,384],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":173,"pos":[-2688,512],"params":["Inherit","False","193","Subtractive Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":143,"pos":[-3200,0],"params":["Inherit","False","Depth Fade","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":151,"pos":[-1792,0],"params":["Inherit","False","Camera Depth Fade","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":172,"pos":[-2432,384],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":145,"pos":[-2304,512],"params":["Inherit","False","143","Depth Fade","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":152,"pos":[-2304,592],"params":["Inherit","False","151","Camera Depth Fade","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor","id":131,"pos":[-2176,384],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":144,"pos":[-1920,384],"params":["Inherit","False","3","3","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":51,"pos":[-1664,384],"params":["Inherit","False","Alpha","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":12,"pos":[-384,128],"params":["Inherit","False","Property","_Smoothness","Smoothness","8","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":49,"pos":[-384,0],"params":["Inherit","False","86","Colour","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":124,"pos":[-4224,-384],"params":["Inherit","False","Property","_NormalMapScale","Normal Map Scale","7","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor","id":123,"pos":[-3968,-384],"params":["Inherit","True","Property","_NormalMap","Normal Map","6","0","Create","True","0","0","0","False","0","False","","-1","e727c3978c253c54ea6bc105a8da932e","e727c3978c253c54ea6bc105a8da932e","True","0","False","white","Auto","True","Object","-1","Auto","Texture2D","False","8","0","SAMPLER2D","","False","1","FLOAT2","0,0","False","2","FLOAT","0","False","3","FLOAT2","0,0","False","4","FLOAT2","0,0","False","5","FLOAT","1","False","6","FLOAT","0","False","7","SAMPLERSTATE","","False","6","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor","id":45,"pos":[-3328,-1024],"params":["Inherit","False","True","True","True","False","1","0","COLOR","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":52,"pos":[-384,256],"params":["Inherit","False","51","Alpha","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WireNode, AmplifyShaderEditor","id":199,"pos":[-3856,-1296],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":197,"pos":[-3328,-1280],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":115,"pos":[-3072,-1280],"params":["Inherit","False","Base R","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.NegateNode, AmplifyShaderEditor","id":207,"pos":[-3584,-256],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":125,"pos":[-3328,-384],"params":["Inherit","False","Normal Map","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":208,"pos":[-3328,-256],"params":["Inherit","False","Negative Normal Map","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":48,"pos":[-3072,-1024],"params":["Inherit","False","Coloured Base RGB","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":198,"pos":[-3840,-1152],"params":["Inherit","False","155","Additive Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":97,"pos":[-2048,1280],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor","id":98,"pos":[-2560,1280],"params":["Inherit","False","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LengthOpNode, AmplifyShaderEditor","id":99,"pos":[-2560,1408],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":100,"pos":[-2304,1408],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":101,"pos":[-2560,1536],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":107,"pos":[-2944,1536],"params":["Inherit","False","115","Base R","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":109,"pos":[-1792,1152],"params":["Inherit","False","Property","_AdditionalLightsTranslucencyEnabled","Additional Lights Translucency Enabled","19","0","Create","True","0","0","0","False","1","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":86,"pos":[-3200,2176],"params":["Inherit","False","Colour","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":119,"pos":[-3584,2176],"params":["Inherit","False","Property","_LightingEnabled","Lighting Enabled","15","0","Create","True","0","0","0","False","2","Header(Lighting)","Space(5)","False","","0","0","0","True","","Toggle","2","Key0","Key1","Create","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":88,"pos":[-2944,1280],"params":["Inherit","False","SRP Additional Light","-1","","302","6c86746ad131a0a408ca599df5f40861","3,6,1,351,0,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":108,"pos":[-2944,1616],"params":["Inherit","False","Property","_AdditionalLightsTranslucency","Additional Lights Translucency","18","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","8","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":92,"pos":[-4224,1920],"params":["Inherit","False","Property","_MainLight","Main Light","16","0","Create","True","0","0","0","False","1","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":93,"pos":[-3840,1792],"params":["Inherit","False","3","3","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":94,"pos":[-3840,1920],"params":["Inherit","False","113","Additional Lights","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":95,"pos":[-3456,1792],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":96,"pos":[-3328,1792],"params":["Inherit","False","Lighting","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.LightColorNode, AmplifyShaderEditor","id":102,"pos":[-4224,1792],"params":["Inherit","False","0","3","COLOR","0","FLOAT3","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":111,"pos":[-1792,1280],"params":["Inherit","False","Property","_AdditionalLights","Additional Lights","17","0","Create","True","0","0","0","False","1","Space(5)","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":195,"pos":[-3200,3584],"params":["Inherit","False","Simplex Noise","-1","","304","c68ae2e20c00ec54aaecd9d04797372e","0","3","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":194,"pos":[-3584,3760],"params":["Inherit","False","Property","_SubtractiveNoiseOctaves","Subtractive Noise Octaves","34","1","[IntRange]","Create","True","0","0","0","False","0","False","Object","-1","","1","0","1","8","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":87,"pos":[-2944,1152],"params":["Inherit","False","SRP Additional Light","-1","","305","6c86746ad131a0a408ca599df5f40861","3,6,1,351,0,23,0","6","2","FLOAT3","0,0,0","False","11","FLOAT3","0,0,0","False","345","FLOAT3","0,0,0","False","346","FLOAT3","0,0,0","False","347","FLOAT","0.5","False","32","FLOAT4","0,0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":106,"pos":[-4480,1408],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0.5","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":90,"pos":[-4736,1408],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":103,"pos":[-4224,1280],"params":["Inherit","False","Normal From Height","-1","","307","1942fe2c5f1a1f94881a33d532e4afeb","0","2","20","FLOAT","0","False","110","FLOAT","1","False","2","FLOAT3","40","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.BlendNormalsNode, AmplifyShaderEditor","id":200,"pos":[-3712,1152],"params":["Inherit","False","0","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.BlendNormalsNode, AmplifyShaderEditor","id":201,"pos":[-3712,1280],"params":["Inherit","False","0","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":210,"pos":[-3712,1408],"params":["Inherit","False","208","Negative Normal Map","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":105,"pos":[-4992,1232],"params":["Inherit","False","Property","_AdditiveNoiseNormalfromHeight","Additive Noise Normal from Height","28","0","Create","True","0","0","0","False","1","Space(5)","False","Object","-1","","0","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":89,"pos":[-4992,1408],"params":["Inherit","False","155","Additive Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":116,"pos":[-4224,2176],"params":["Inherit","False","48","Coloured Base RGB","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":118,"pos":[-4224,2304],"params":["Inherit","False","96","Lighting","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":117,"pos":[-3840,2304],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":112,"pos":[-1280,1152],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":113,"pos":[-1024,1152],"params":["Inherit","False","Additional Lights","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":110,"pos":[-4992,1152],"params":["Inherit","False","155","Additive Noise","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":203,"pos":[-3328,1024],"params":["Inherit","False","Property","_Keyword0","Keyword 0","21","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","176","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":104,"pos":[-4224,1152],"params":["Inherit","False","Normal From Height","-1","","308","1942fe2c5f1a1f94881a33d532e4afeb","0","2","20","FLOAT","0","False","110","FLOAT","1","False","2","FLOAT3","40","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":209,"pos":[-4224,1408],"params":["Inherit","False","208","Negative Normal Map","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":206,"pos":[-4224,1024],"params":["Inherit","False","125","Normal Map","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":153,"pos":[-3200,2816],"params":["Inherit","False","Simplex Noise","-1","","309","c68ae2e20c00ec54aaecd9d04797372e","0","3","4","FLOAT3","0,0,0","False","6","FLOAT","0","False","7","FLOAT","1","False","2","FLOAT","0","FLOAT3","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":156,"pos":[-3584,2992],"params":["Inherit","False","Property","_AdditiveNoiseOctaves","Additive Noise Octaves","25","1","[IntRange]","Create","True","0","0","0","False","0","False","Object","-1","","1","0","1","8","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":229,"pos":[-7168,-640],"params":["Inherit","False","Property","_DistortionNoisePower","Distortion Noise Power","42","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor","id":205,"pos":[-3328,1408],"params":["Inherit","False","Property","_Keyword1","Keyword 0","21","0","Create","True","0","0","0","False","0","False","","0","0","0","True","","Toggle","2","Key0","Key1","Reference","176","True","True","All","9","1","FLOAT3","0,0,0","False","0","FLOAT3","0,0,0","False","2","FLOAT3","0,0,0","False","3","FLOAT3","0,0,0","False","4","FLOAT3","0,0,0","False","5","FLOAT3","0,0,0","False","6","FLOAT3","0,0,0","False","7","FLOAT3","0,0,0","False","8","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":76,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ExtraPrePass","0","0","ExtraPrePass","6","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","0","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":77,"pos":[0,0],"params":["Float","False","True","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","19","Mirza/Smoke","2992e84f91cbeb14eab234972e07ea9d","True","Forward","0","1","Forward","11","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Transparent=RenderType","Queue=Transparent=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","True","1","5","False","","10","False","","1","1","False","","10","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","2","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalForwardOnly","False","False","0","","0","0","Standard","28","Surface","1","638687650521782574","  Keep Alpha","0","0","  Blend","0","0","Two Sided","0","638687676030191110","Alpha Clipping","0","638687650528450418","  Use Shadow Threshold","0","0","Fragment Normal Space","0","0","Forward Only","0","0","Cast Shadows","0","638687686610047826","Receive Shadows","1","0","Receive SSAO","1","0","GPU Instancing","1","0","LOD CrossFade","1","0","Built-in Fog","1","0","Meta Pass","0","0","Extra Pre Pass","0","0","Tessellation","0","0","  Phong","0","0","  Strength","0.5,False,","0","  Type","0","0","  Tess","16,False,","0","  Min","10,False,","0","  Max","25,False,","0","  Edge Length","16,False,","0","  Max Displacement","25,False,","0","Write Depth","0","0","  Conservative","0","0","Vertex Position","1","0","0","10","False","True","False","True","False","False","True","True","True","False","False","","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":78,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ShadowCaster","0","2","ShadowCaster","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","False","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=ShadowCaster","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":79,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthOnly","0","3","DepthOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","True","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","False","False","False","True","1","LightMode=DepthOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":80,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Meta","0","4","Meta","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Meta","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":81,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","Universal2D","0","5","Universal2D","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=Universal2D","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":82,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","SceneSelectionPass","0","6","SceneSelectionPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=SceneSelectionPass","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":83,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","ScenePickingPass","0","7","ScenePickingPass","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Picking","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":84,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormals","0","8","DepthNormals","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":85,"pos":[0,0],"params":["Float","False","False","-1","2","UnityEditor.ShaderGraphUnlitGUI","0","1","New Amplify Shader","2992e84f91cbeb14eab234972e07ea9d","True","DepthNormalsOnly","0","9","DepthNormalsOnly","0","False","True","1","1","False","","0","False","","1","1","False","","0","False","","True","1","False","","1","False","","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","False","False","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Unlit","True","5","True","12","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormalsOnly","False","True","9","d3d11","metal","vulkan","xboxone","xboxseries","playstation","ps4","ps5","switch","0","","0","0","Standard","0","False","0"]}
{"wire":[163,0,132,4]}
{"wire":[165,0,163,0]}
{"wire":[165,2,164,0]}
{"wire":[157,0,165,0]}
{"wire":[166,0,162,0]}
{"wire":[166,1,167,0]}
{"wire":[239,0,237,0]}
{"wire":[239,1,236,0]}
{"wire":[154,4,159,0]}
{"wire":[154,7,160,0]}
{"wire":[154,9,161,0]}
{"wire":[154,12,166,0]}
{"wire":[242,4,238,0]}
{"wire":[242,7,240,0]}
{"wire":[242,9,241,0]}
{"wire":[242,12,239,0]}
{"wire":[175,0,154,0]}
{"wire":[235,4,242,0]}
{"wire":[235,6,242,15]}
{"wire":[235,7,243,0]}
{"wire":[170,0,175,0]}
{"wire":[170,1,171,0]}
{"wire":[232,0,235,3]}
{"wire":[232,1,231,0]}
{"wire":[168,0,170,0]}
{"wire":[168,1,169,0]}
{"wire":[233,0,232,0]}
{"wire":[176,0,168,0]}
{"wire":[234,0,233,0]}
{"wire":[155,0,176,0]}
{"wire":[245,0,244,0]}
{"wire":[245,1,246,0]}
{"wire":[10,1,245,0]}
{"wire":[219,0,218,0]}
{"wire":[219,1,223,0]}
{"wire":[219,2,220,0]}
{"wire":[217,0,10,0]}
{"wire":[217,1,219,0]}
{"wire":[185,0,181,0]}
{"wire":[185,1,182,0]}
{"wire":[40,0,217,0]}
{"wire":[40,1,43,0]}
{"wire":[40,2,122,0]}
{"wire":[187,4,183,0]}
{"wire":[187,7,186,0]}
{"wire":[187,9,184,0]}
{"wire":[187,12,185,0]}
{"wire":[46,0,40,0]}
{"wire":[47,0,46,0]}
{"wire":[196,0,187,0]}
{"wire":[134,0,132,3]}
{"wire":[134,1,133,0]}
{"wire":[189,0,196,0]}
{"wire":[189,1,188,0]}
{"wire":[135,0,134,0]}
{"wire":[191,0,189,0]}
{"wire":[191,1,190,0]}
{"wire":[142,0,50,0]}
{"wire":[142,1,141,0]}
{"wire":[192,0,191,0]}
{"wire":[213,0,180,0]}
{"wire":[213,1,214,0]}
{"wire":[226,0,142,0]}
{"wire":[226,1,227,0]}
{"wire":[14,0,16,0]}
{"wire":[148,0,147,0]}
{"wire":[148,1,146,0]}
{"wire":[193,0,192,0]}
{"wire":[212,0,226,0]}
{"wire":[212,1,213,0]}
{"wire":[137,0,130,0]}
{"wire":[137,1,138,0]}
{"wire":[120,20,14,0]}
{"wire":[120,4,121,0]}
{"wire":[150,20,148,0]}
{"wire":[150,4,149,0]}
{"wire":[129,0,212,0]}
{"wire":[129,1,137,0]}
{"wire":[143,0,120,0]}
{"wire":[151,0,150,0]}
{"wire":[172,0,129,0]}
{"wire":[172,1,173,0]}
{"wire":[131,0,172,0]}
{"wire":[144,0,131,0]}
{"wire":[144,1,145,0]}
{"wire":[144,2,152,0]}
{"wire":[51,0,144,0]}
{"wire":[123,5,124,0]}
{"wire":[45,0,40,0]}
{"wire":[199,0,10,1]}
{"wire":[197,0,199,0]}
{"wire":[197,1,198,0]}
{"wire":[115,0,197,0]}
{"wire":[207,0,123,0]}
{"wire":[125,0,123,0]}
{"wire":[208,0,207,0]}
{"wire":[48,0,45,0]}
{"wire":[97,0,87,0]}
{"wire":[97,1,100,0]}
{"wire":[98,0,88,0]}
{"wire":[99,0,88,0]}
{"wire":[100,0,98,0]}
{"wire":[100,1,99,0]}
{"wire":[100,2,101,0]}
{"wire":[101,0,107,0]}
{"wire":[101,1,108,0]}
{"wire":[109,1,87,0]}
{"wire":[109,0,97,0]}
{"wire":[86,0,119,0]}
{"wire":[119,1,116,0]}
{"wire":[119,0,117,0]}
{"wire":[88,2,205,0]}
{"wire":[93,0,102,1]}
{"wire":[93,1,102,2]}
{"wire":[93,2,92,0]}
{"wire":[95,0,93,0]}
{"wire":[95,1,94,0]}
{"wire":[96,0,95,0]}
{"wire":[195,4,187,0]}
{"wire":[195,6,187,15]}
{"wire":[195,7,194,0]}
{"wire":[87,2,203,0]}
{"wire":[106,0,90,0]}
{"wire":[90,0,89,0]}
{"wire":[103,20,106,0]}
{"wire":[103,110,105,0]}
{"wire":[200,0,104,40]}
{"wire":[200,1,206,0]}
{"wire":[201,0,103,40]}
{"wire":[201,1,209,0]}
{"wire":[117,0,116,0]}
{"wire":[117,1,118,0]}
{"wire":[112,0,109,0]}
{"wire":[112,1,111,0]}
{"wire":[113,0,112,0]}
{"wire":[203,1,206,0]}
{"wire":[203,0,200,0]}
{"wire":[104,20,110,0]}
{"wire":[104,110,105,0]}
{"wire":[153,4,154,0]}
{"wire":[153,6,154,15]}
{"wire":[153,7,156,0]}
{"wire":[205,1,210,0]}
{"wire":[205,0,201,0]}
{"wire":[77,2,49,0]}
{"wire":[77,3,52,0]}
ASEEND*/
//CHKSM=43F1BB38D26780D36CBA2AE2709E3C30BD34D5A0