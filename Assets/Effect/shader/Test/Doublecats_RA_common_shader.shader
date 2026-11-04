// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DC/Doublecats_RA_common_shader"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[NoScaleOffset]_Base_Tex("Base_Tex", 2D) = "white" {}
		_Base_uv("Base_uv", Vector) = (0,0,0,0)
		_Base_speed("Base_speed", Vector) = (0,0,0,0)
		[Toggle(_USE_CUSTOM1_ZW_MOVE_ON)] _Use_custom1_zw_move("Use_custom1_zw_move", Float) = 0
		[HDR]_R_light_color("R_light_color", Color) = (0,0,0,0)
		[HDR]_R_dark_clolor("R_dark_clolor", Color) = (0,0,0,0)
		_Constart("Constart", Float) = 0
		[HDR]_Color("Color", Color) = (0,0,0,0)
		_Color_power("Color_power", Float) = 0
		_Distance("Distance", Float) = 0
		_Alpha_Constart("Alpha_Constart", Float) = 0
		_Alpha("Alpha", Float) = 0
		[NoScaleOffset]_Dissolve_Tex("Dissolve_Tex", 2D) = "white" {}
		_Dissolve_uv("Dissolve_uv", Vector) = (0,0,0,0)
		_Dissolve_speed("Dissolve_speed", Vector) = (0,0,0,0)
		_Hardness("Hardness", Float) = 0
		_Dissolve("Dissolve", Float) = 0
		[Toggle(_USE_CUSTOM1_X_DISSOLVE_ON)] _use_custom1_x_dissolve("use_custom1_x_dissolve", Float) = 0
		[NoScaleOffset]_Base_mask_Tex("Base_mask_Tex", 2D) = "white" {}
		_Base_mask_uv("Base_mask_uv", Vector) = (0,0,0,0)
		_Base_mask_speed("Base_mask_speed", Vector) = (0,0,0,0)
		[Toggle(_CUSTOM2_ZW_MOVE_MASK_ON)] _Custom2_zw_move_mask("Custom2_zw_move_mask", Float) = 0
		_Distort_Tex("Distort_Tex", 2D) = "white" {}
		_Distort("Distort", Float) = 0
		[Toggle(_USE_CUSTOM1_Y_DISTORT_ON)] _use_custom1_y_distort("use_custom1_y_distort", Float) = 0
		_Mask("Mask", 2D) = "white" {}
		[Toggle(_USE_FRENSEL_ON)] _use_frensel("use_frensel", Float) = 0
		[Toggle(_FRENSEL_FLIP_ON)] _frensel_flip("frensel_flip", Float) = 0
		_frensel("frensel", Float) = 0
		_frensel_edge("frensel_edge", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull Back
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha , One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 999999
			#define REQUIRE_DEPTH_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma shader_feature_local _USE_CUSTOM1_ZW_MOVE_ON
			#pragma shader_feature_local _USE_CUSTOM1_Y_DISTORT_ON
			#pragma shader_feature_local _USE_CUSTOM1_X_DISSOLVE_ON
			#pragma shader_feature_local _CUSTOM2_ZW_MOVE_MASK_ON
			#pragma shader_feature_local _USE_FRENSEL_ON
			#pragma shader_feature_local _FRENSEL_FLIP_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _Base_Tex;
			sampler2D _Distort_Tex;
			sampler2D _Dissolve_Tex;
			sampler2D _Mask;
			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _Base_mask_Tex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color;
			float4 _R_dark_clolor;
			float4 _R_light_color;
			float4 _Base_speed;
			float4 _Base_uv;
			float4 _Distort_Tex_ST;
			float _Distort;
			float _Constart;
			float _Color_power;
			float _Alpha_Constart;
			float _Alpha;
			float4 _Dissolve_speed;
			float4 _Dissolve_uv;
			float _Hardness;
			float _Dissolve;
			float4 _Mask_ST;
			float _Distance;
			float4 _Base_mask_speed;
			float4 _Base_mask_uv;
			float _frensel;
			float _frensel_edge;
			CBUFFER_END


			
			VertexOutput vert ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 vertexPos138 = v.vertex.xyz;
				float4 ase_clipPos138 = TransformObjectToHClip((vertexPos138).xyz);
				float4 screenPos138 = ComputeScreenPos(ase_clipPos138);
				o.ase_texcoord5 = screenPos138;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord7.xyz = ase_worldNormal;
				
				o.ase_texcoord3 = v.ase_texcoord;
				o.ase_texcoord4 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				o.ase_texcoord6 = v.ase_texcoord2;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.w = 0;
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

			half4 frag ( VertexOutput IN  ) : SV_Target
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
				float4 break143 = _Base_speed;
				float mulTime141 = _TimeParameters.x * break143.z;
				float2 appendResult142 = (float2(break143.x , break143.y));
				float2 appendResult97 = (float2(_Base_uv.x , _Base_uv.y));
				float2 appendResult98 = (float2(_Base_uv.z , _Base_uv.w));
				float2 uv095 = IN.ase_texcoord3 * appendResult97 + appendResult98;
				float2 panner140 = ( mulTime141 * appendResult142 + uv095);
				float4 uv192 = IN.ase_texcoord4;
				uv192.xy = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult90 = (float2(uv192.x , uv192.y));
				#ifdef _USE_CUSTOM1_ZW_MOVE_ON
				float2 staticSwitch93 = ( panner140 + appendResult90 );
				#else
				float2 staticSwitch93 = panner140;
				#endif
				float2 uv_Distort_Tex = IN.ase_texcoord3.xy * _Distort_Tex_ST.xy + _Distort_Tex_ST.zw;
				float4 tex2DNode125 = tex2D( _Distort_Tex, uv_Distort_Tex );
				float2 appendResult123 = (float2(tex2DNode125.r , tex2DNode125.g));
				float4 uv0116 = IN.ase_texcoord3;
				uv0116.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _USE_CUSTOM1_Y_DISTORT_ON
				float staticSwitch118 = uv0116.w;
				#else
				float staticSwitch118 = _Distort;
				#endif
				float2 Distort126 = ( appendResult123 * staticSwitch118 );
				float4 tex2DNode20 = tex2D( _Base_Tex, ( staticSwitch93 + Distort126 ) );
				float4 lerpResult130 = lerp( _R_dark_clolor , _R_light_color , pow( tex2DNode20.r , _Constart ));
				float3 temp_output_33_0 = (( _Color * lerpResult130 * _Color_power * IN.ase_color )).rgb;
				
				float4 break156 = _Dissolve_speed;
				float mulTime154 = _TimeParameters.x * break156.z;
				float2 appendResult153 = (float2(break156.x , break156.y));
				float2 appendResult150 = (float2(_Dissolve_uv.x , _Dissolve_uv.y));
				float2 appendResult151 = (float2(_Dissolve_uv.z , _Dissolve_uv.w));
				float2 uv0152 = IN.ase_texcoord3.xy * appendResult150 + appendResult151;
				float2 panner155 = ( mulTime154 * appendResult153 + uv0152);
				float4 uv0113 = IN.ase_texcoord3;
				uv0113.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _USE_CUSTOM1_X_DISSOLVE_ON
				float staticSwitch114 = uv0113.z;
				#else
				float staticSwitch114 = _Dissolve;
				#endif
				float lerpResult107 = lerp( _Hardness , -1.0 , staticSwitch114);
				float2 uv_Mask = IN.ase_texcoord3.xy * _Mask_ST.xy + _Mask_ST.zw;
				float4 screenPos138 = IN.ase_texcoord5;
				float4 ase_screenPosNorm138 = screenPos138 / screenPos138.w;
				ase_screenPosNorm138.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm138.z : ase_screenPosNorm138.z * 0.5 + 0.5;
				float screenDepth138 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm138.xy ),_ZBufferParams);
				float distanceDepth138 = abs( ( screenDepth138 - LinearEyeDepth( ase_screenPosNorm138.z,_ZBufferParams ) ) / ( _Distance ) );
				float4 break163 = _Base_mask_speed;
				float mulTime167 = _TimeParameters.x * break163.z;
				float2 appendResult166 = (float2(break163.x , break163.y));
				float2 appendResult162 = (float2(_Base_mask_uv.x , _Base_mask_uv.y));
				float2 appendResult164 = (float2(_Base_mask_uv.z , _Base_mask_uv.w));
				float2 uv0165 = IN.ase_texcoord3.xy * appendResult162 + appendResult164;
				float2 panner168 = ( mulTime167 * appendResult166 + uv0165);
				float2 temp_output_170_0 = ( panner168 + Distort126 );
				float4 uv2173 = IN.ase_texcoord6;
				uv2173.xy = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult174 = (float2(uv2173.x , uv2173.y));
				#ifdef _CUSTOM2_ZW_MOVE_MASK_ON
				float2 staticSwitch176 = ( temp_output_170_0 + appendResult174 );
				#else
				float2 staticSwitch176 = temp_output_170_0;
				#endif
				float Base_mask177 = tex2D( _Base_mask_Tex, staticSwitch176 ).r;
				float3 ase_worldNormal = IN.ase_texcoord7.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult181 = dot( ase_worldNormal , ase_worldViewDir );
				float temp_output_182_0 = abs( dotResult181 );
				#ifdef _FRENSEL_FLIP_ON
				float staticSwitch189 = ( 1.0 - temp_output_182_0 );
				#else
				float staticSwitch189 = temp_output_182_0;
				#endif
				float smoothstepResult186 = smoothstep( _frensel , ( _frensel + _frensel_edge ) , staticSwitch189);
				#ifdef _USE_FRENSEL_ON
				float staticSwitch187 = smoothstepResult186;
				#else
				float staticSwitch187 = 0.0;
				#endif
				float frensel188 = staticSwitch187;
				float temp_output_35_0 = saturate( ( IN.ase_color.a * pow( tex2DNode20.r , _Alpha_Constart ) * _Alpha * saturate( ( ( tex2D( _Dissolve_Tex, ( panner155 + Distort126 ) ).r * _Hardness ) - lerpResult107 ) ) * tex2D( _Mask, uv_Mask ).r * saturate( distanceDepth138 ) * Base_mask177 * frensel188 ) );
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = temp_output_33_0;
				float Alpha = temp_output_35_0;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 999999
			#define REQUIRE_DEPTH_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma shader_feature_local _USE_CUSTOM1_ZW_MOVE_ON
			#pragma shader_feature_local _USE_CUSTOM1_Y_DISTORT_ON
			#pragma shader_feature_local _USE_CUSTOM1_X_DISSOLVE_ON
			#pragma shader_feature_local _CUSTOM2_ZW_MOVE_MASK_ON
			#pragma shader_feature_local _USE_FRENSEL_ON
			#pragma shader_feature_local _FRENSEL_FLIP_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _Base_Tex;
			sampler2D _Distort_Tex;
			sampler2D _Dissolve_Tex;
			sampler2D _Mask;
			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _Base_mask_Tex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color;
			float4 _R_dark_clolor;
			float4 _R_light_color;
			float4 _Base_speed;
			float4 _Base_uv;
			float4 _Distort_Tex_ST;
			float _Distort;
			float _Constart;
			float _Color_power;
			float _Alpha_Constart;
			float _Alpha;
			float4 _Dissolve_speed;
			float4 _Dissolve_uv;
			float _Hardness;
			float _Dissolve;
			float4 _Mask_ST;
			float _Distance;
			float4 _Base_mask_speed;
			float4 _Base_mask_uv;
			float _frensel;
			float _frensel_edge;
			CBUFFER_END


			
			float3 _LightDirection;

			VertexOutput ShadowPassVertex( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 vertexPos138 = v.vertex.xyz;
				float4 ase_clipPos138 = TransformObjectToHClip((vertexPos138).xyz);
				float4 screenPos138 = ComputeScreenPos(ase_clipPos138);
				o.ase_texcoord4 = screenPos138;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord6.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord;
				o.ase_texcoord3 = v.ase_texcoord1;
				o.ase_texcoord5 = v.ase_texcoord2;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord6.w = 0;
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

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				float3 normalWS = TransformObjectToWorldDir( v.ase_normal );

				float4 clipPos = TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) );

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;

				return o;
			}

			half4 ShadowPassFragment(VertexOutput IN  ) : SV_TARGET
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

				float4 break143 = _Base_speed;
				float mulTime141 = _TimeParameters.x * break143.z;
				float2 appendResult142 = (float2(break143.x , break143.y));
				float2 appendResult97 = (float2(_Base_uv.x , _Base_uv.y));
				float2 appendResult98 = (float2(_Base_uv.z , _Base_uv.w));
				float2 uv095 = IN.ase_texcoord2 * appendResult97 + appendResult98;
				float2 panner140 = ( mulTime141 * appendResult142 + uv095);
				float4 uv192 = IN.ase_texcoord3;
				uv192.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult90 = (float2(uv192.x , uv192.y));
				#ifdef _USE_CUSTOM1_ZW_MOVE_ON
				float2 staticSwitch93 = ( panner140 + appendResult90 );
				#else
				float2 staticSwitch93 = panner140;
				#endif
				float2 uv_Distort_Tex = IN.ase_texcoord2.xy * _Distort_Tex_ST.xy + _Distort_Tex_ST.zw;
				float4 tex2DNode125 = tex2D( _Distort_Tex, uv_Distort_Tex );
				float2 appendResult123 = (float2(tex2DNode125.r , tex2DNode125.g));
				float4 uv0116 = IN.ase_texcoord2;
				uv0116.xy = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _USE_CUSTOM1_Y_DISTORT_ON
				float staticSwitch118 = uv0116.w;
				#else
				float staticSwitch118 = _Distort;
				#endif
				float2 Distort126 = ( appendResult123 * staticSwitch118 );
				float4 tex2DNode20 = tex2D( _Base_Tex, ( staticSwitch93 + Distort126 ) );
				float4 break156 = _Dissolve_speed;
				float mulTime154 = _TimeParameters.x * break156.z;
				float2 appendResult153 = (float2(break156.x , break156.y));
				float2 appendResult150 = (float2(_Dissolve_uv.x , _Dissolve_uv.y));
				float2 appendResult151 = (float2(_Dissolve_uv.z , _Dissolve_uv.w));
				float2 uv0152 = IN.ase_texcoord2.xy * appendResult150 + appendResult151;
				float2 panner155 = ( mulTime154 * appendResult153 + uv0152);
				float4 uv0113 = IN.ase_texcoord2;
				uv0113.xy = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _USE_CUSTOM1_X_DISSOLVE_ON
				float staticSwitch114 = uv0113.z;
				#else
				float staticSwitch114 = _Dissolve;
				#endif
				float lerpResult107 = lerp( _Hardness , -1.0 , staticSwitch114);
				float2 uv_Mask = IN.ase_texcoord2.xy * _Mask_ST.xy + _Mask_ST.zw;
				float4 screenPos138 = IN.ase_texcoord4;
				float4 ase_screenPosNorm138 = screenPos138 / screenPos138.w;
				ase_screenPosNorm138.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm138.z : ase_screenPosNorm138.z * 0.5 + 0.5;
				float screenDepth138 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm138.xy ),_ZBufferParams);
				float distanceDepth138 = abs( ( screenDepth138 - LinearEyeDepth( ase_screenPosNorm138.z,_ZBufferParams ) ) / ( _Distance ) );
				float4 break163 = _Base_mask_speed;
				float mulTime167 = _TimeParameters.x * break163.z;
				float2 appendResult166 = (float2(break163.x , break163.y));
				float2 appendResult162 = (float2(_Base_mask_uv.x , _Base_mask_uv.y));
				float2 appendResult164 = (float2(_Base_mask_uv.z , _Base_mask_uv.w));
				float2 uv0165 = IN.ase_texcoord2.xy * appendResult162 + appendResult164;
				float2 panner168 = ( mulTime167 * appendResult166 + uv0165);
				float2 temp_output_170_0 = ( panner168 + Distort126 );
				float4 uv2173 = IN.ase_texcoord5;
				uv2173.xy = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult174 = (float2(uv2173.x , uv2173.y));
				#ifdef _CUSTOM2_ZW_MOVE_MASK_ON
				float2 staticSwitch176 = ( temp_output_170_0 + appendResult174 );
				#else
				float2 staticSwitch176 = temp_output_170_0;
				#endif
				float Base_mask177 = tex2D( _Base_mask_Tex, staticSwitch176 ).r;
				float3 ase_worldNormal = IN.ase_texcoord6.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult181 = dot( ase_worldNormal , ase_worldViewDir );
				float temp_output_182_0 = abs( dotResult181 );
				#ifdef _FRENSEL_FLIP_ON
				float staticSwitch189 = ( 1.0 - temp_output_182_0 );
				#else
				float staticSwitch189 = temp_output_182_0;
				#endif
				float smoothstepResult186 = smoothstep( _frensel , ( _frensel + _frensel_edge ) , staticSwitch189);
				#ifdef _USE_FRENSEL_ON
				float staticSwitch187 = smoothstepResult186;
				#else
				float staticSwitch187 = 0.0;
				#endif
				float frensel188 = staticSwitch187;
				float temp_output_35_0 = saturate( ( IN.ase_color.a * pow( tex2DNode20.r , _Alpha_Constart ) * _Alpha * saturate( ( ( tex2D( _Dissolve_Tex, ( panner155 + Distort126 ) ).r * _Hardness ) - lerpResult107 ) ) * tex2D( _Mask, uv_Mask ).r * saturate( distanceDepth138 ) * Base_mask177 * frensel188 ) );
				
				float Alpha = temp_output_35_0;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
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
			ColorMask 0

			HLSLPROGRAM
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 999999
			#define REQUIRE_DEPTH_TEXTURE 1

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma shader_feature_local _USE_CUSTOM1_ZW_MOVE_ON
			#pragma shader_feature_local _USE_CUSTOM1_Y_DISTORT_ON
			#pragma shader_feature_local _USE_CUSTOM1_X_DISSOLVE_ON
			#pragma shader_feature_local _CUSTOM2_ZW_MOVE_MASK_ON
			#pragma shader_feature_local _USE_FRENSEL_ON
			#pragma shader_feature_local _FRENSEL_FLIP_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
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
				float4 ase_color : COLOR;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _Base_Tex;
			sampler2D _Distort_Tex;
			sampler2D _Dissolve_Tex;
			sampler2D _Mask;
			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _Base_mask_Tex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Color;
			float4 _R_dark_clolor;
			float4 _R_light_color;
			float4 _Base_speed;
			float4 _Base_uv;
			float4 _Distort_Tex_ST;
			float _Distort;
			float _Constart;
			float _Color_power;
			float _Alpha_Constart;
			float _Alpha;
			float4 _Dissolve_speed;
			float4 _Dissolve_uv;
			float _Hardness;
			float _Dissolve;
			float4 _Mask_ST;
			float _Distance;
			float4 _Base_mask_speed;
			float4 _Base_mask_uv;
			float _frensel;
			float _frensel_edge;
			CBUFFER_END


			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 vertexPos138 = v.vertex.xyz;
				float4 ase_clipPos138 = TransformObjectToHClip((vertexPos138).xyz);
				float4 screenPos138 = ComputeScreenPos(ase_clipPos138);
				o.ase_texcoord4 = screenPos138;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord6.xyz = ase_worldNormal;
				
				o.ase_color = v.ase_color;
				o.ase_texcoord2 = v.ase_texcoord;
				o.ase_texcoord3 = v.ase_texcoord1;
				o.ase_texcoord5 = v.ase_texcoord2;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord6.w = 0;
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

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = TransformWorldToHClip( positionWS );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
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

				float4 break143 = _Base_speed;
				float mulTime141 = _TimeParameters.x * break143.z;
				float2 appendResult142 = (float2(break143.x , break143.y));
				float2 appendResult97 = (float2(_Base_uv.x , _Base_uv.y));
				float2 appendResult98 = (float2(_Base_uv.z , _Base_uv.w));
				float2 uv095 = IN.ase_texcoord2 * appendResult97 + appendResult98;
				float2 panner140 = ( mulTime141 * appendResult142 + uv095);
				float4 uv192 = IN.ase_texcoord3;
				uv192.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult90 = (float2(uv192.x , uv192.y));
				#ifdef _USE_CUSTOM1_ZW_MOVE_ON
				float2 staticSwitch93 = ( panner140 + appendResult90 );
				#else
				float2 staticSwitch93 = panner140;
				#endif
				float2 uv_Distort_Tex = IN.ase_texcoord2.xy * _Distort_Tex_ST.xy + _Distort_Tex_ST.zw;
				float4 tex2DNode125 = tex2D( _Distort_Tex, uv_Distort_Tex );
				float2 appendResult123 = (float2(tex2DNode125.r , tex2DNode125.g));
				float4 uv0116 = IN.ase_texcoord2;
				uv0116.xy = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _USE_CUSTOM1_Y_DISTORT_ON
				float staticSwitch118 = uv0116.w;
				#else
				float staticSwitch118 = _Distort;
				#endif
				float2 Distort126 = ( appendResult123 * staticSwitch118 );
				float4 tex2DNode20 = tex2D( _Base_Tex, ( staticSwitch93 + Distort126 ) );
				float4 break156 = _Dissolve_speed;
				float mulTime154 = _TimeParameters.x * break156.z;
				float2 appendResult153 = (float2(break156.x , break156.y));
				float2 appendResult150 = (float2(_Dissolve_uv.x , _Dissolve_uv.y));
				float2 appendResult151 = (float2(_Dissolve_uv.z , _Dissolve_uv.w));
				float2 uv0152 = IN.ase_texcoord2.xy * appendResult150 + appendResult151;
				float2 panner155 = ( mulTime154 * appendResult153 + uv0152);
				float4 uv0113 = IN.ase_texcoord2;
				uv0113.xy = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _USE_CUSTOM1_X_DISSOLVE_ON
				float staticSwitch114 = uv0113.z;
				#else
				float staticSwitch114 = _Dissolve;
				#endif
				float lerpResult107 = lerp( _Hardness , -1.0 , staticSwitch114);
				float2 uv_Mask = IN.ase_texcoord2.xy * _Mask_ST.xy + _Mask_ST.zw;
				float4 screenPos138 = IN.ase_texcoord4;
				float4 ase_screenPosNorm138 = screenPos138 / screenPos138.w;
				ase_screenPosNorm138.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm138.z : ase_screenPosNorm138.z * 0.5 + 0.5;
				float screenDepth138 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm138.xy ),_ZBufferParams);
				float distanceDepth138 = abs( ( screenDepth138 - LinearEyeDepth( ase_screenPosNorm138.z,_ZBufferParams ) ) / ( _Distance ) );
				float4 break163 = _Base_mask_speed;
				float mulTime167 = _TimeParameters.x * break163.z;
				float2 appendResult166 = (float2(break163.x , break163.y));
				float2 appendResult162 = (float2(_Base_mask_uv.x , _Base_mask_uv.y));
				float2 appendResult164 = (float2(_Base_mask_uv.z , _Base_mask_uv.w));
				float2 uv0165 = IN.ase_texcoord2.xy * appendResult162 + appendResult164;
				float2 panner168 = ( mulTime167 * appendResult166 + uv0165);
				float2 temp_output_170_0 = ( panner168 + Distort126 );
				float4 uv2173 = IN.ase_texcoord5;
				uv2173.xy = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult174 = (float2(uv2173.x , uv2173.y));
				#ifdef _CUSTOM2_ZW_MOVE_MASK_ON
				float2 staticSwitch176 = ( temp_output_170_0 + appendResult174 );
				#else
				float2 staticSwitch176 = temp_output_170_0;
				#endif
				float Base_mask177 = tex2D( _Base_mask_Tex, staticSwitch176 ).r;
				float3 ase_worldNormal = IN.ase_texcoord6.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult181 = dot( ase_worldNormal , ase_worldViewDir );
				float temp_output_182_0 = abs( dotResult181 );
				#ifdef _FRENSEL_FLIP_ON
				float staticSwitch189 = ( 1.0 - temp_output_182_0 );
				#else
				float staticSwitch189 = temp_output_182_0;
				#endif
				float smoothstepResult186 = smoothstep( _frensel , ( _frensel + _frensel_edge ) , staticSwitch189);
				#ifdef _USE_FRENSEL_ON
				float staticSwitch187 = smoothstepResult186;
				#else
				float staticSwitch187 = 0.0;
				#endif
				float frensel188 = staticSwitch187;
				float temp_output_35_0 = saturate( ( IN.ase_color.a * pow( tex2DNode20.r , _Alpha_Constart ) * _Alpha * saturate( ( ( tex2D( _Dissolve_Tex, ( panner155 + Distort126 ) ).r * _Hardness ) - lerpResult107 ) ) * tex2D( _Mask, uv_Mask ).r * saturate( distanceDepth138 ) * Base_mask177 * frensel188 ) );
				
				float Alpha = temp_output_35_0;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

	
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=17800
12.57143;114.2857;2181.714;1005.286;489.137;500.3161;1.662676;True;False
Node;AmplifyShaderEditor.RangedFloatNode;117;-2767.667,451.3081;Inherit;False;Property;_Distort;Distort;23;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;116;-2684.846,609.9512;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;125;-3070.814,205.2634;Inherit;True;Property;_Distort_Tex;Distort_Tex;22;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;144;-2728.793,-350.7201;Inherit;False;Property;_Base_speed;Base_speed;2;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;96;-2764.991,-723.9532;Inherit;False;Property;_Base_uv;Base_uv;1;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;123;-2599.246,248.6011;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;118;-2361.784,486.9801;Inherit;False;Property;_use_custom1_y_distort;use_custom1_y_distort;24;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;160;-1421.711,1868.507;Inherit;False;Property;_Base_mask_uv;Base_mask_uv;19;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;161;-1433.437,2100.984;Inherit;False;Property;_Base_mask_speed;Base_mask_speed;20;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-2157.85,225.0249;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;179;-394.8352,2990.54;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;180;-428.4202,2706.435;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector4Node;146;-2196.28,842.6624;Inherit;False;Property;_Dissolve_uv;Dissolve_uv;13;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;147;-2208.006,1075.14;Inherit;False;Property;_Dissolve_speed;Dissolve_speed;14;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;143;-2408.161,-394.4246;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;98;-2095.88,-557.0792;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;164;-902.1641,2092.565;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;97;-2078.88,-775.0789;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;163;-1214.445,2255.22;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;162;-885.1641,1874.566;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;165;-705.1641,1943.566;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;95;-1898.88,-706.079;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;181;-74.83521,2934.54;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;167;-838.407,2385.342;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;166;-945.407,2281.342;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;141;-2032.123,-264.3029;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;151;-1676.733,1066.721;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;156;-1989.014,1229.376;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.TextureCoordinatesNode;92;-2016.563,-52.4554;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;142;-2139.123,-368.3029;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;126;-1911.598,204.2661;Inherit;False;Distort;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;150;-1659.733,848.7215;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;173;-552.6556,2426.919;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;154;-1612.976,1359.498;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;153;-1719.976,1255.498;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;152;-1479.733,917.7214;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;168;-375.407,2078.342;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;140;-1826.123,-420.3029;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;90;-1588.025,-33.3887;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;169;-101.9688,1958.79;Inherit;False;126;Distort;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.AbsOpNode;182;242.7418,2914.396;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;375.6268,3200.543;Inherit;False;Property;_frensel_edge;frensel_edge;32;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;184;411.5188,3061.536;Inherit;False;Property;_frensel;frensel;31;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;190;546.4998,2909.097;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;174;-124.1176,2445.986;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;170;-127.0135,1734.501;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;155;-1149.976,1052.498;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;91;-1432.025,-102.3887;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-876.5378,932.9454;Inherit;False;126;Distort;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;189;614.8088,2729.004;Inherit;False;Property;_frensel_flip;frensel_flip;30;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;145;-901.5825,708.6565;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;175;94.88242,2462.986;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;113;-350.2611,1085.784;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;185;703.0278,3155.35;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-615.7635,831.8236;Inherit;False;Property;_Dissolve;Dissolve;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;93;-1309.95,-328.4825;Inherit;False;Property;_Use_custom1_zw_move;Use_custom1_zw_move;3;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;127;-1005.449,-92.44879;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;191;1159.786,2709.902;Inherit;False;Constant;_Float15;Float 15;44;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;176;291.9574,2391.892;Inherit;False;Property;_Custom2_zw_move_mask;Custom2_zw_move_mask;21;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;99;-700.1617,508.8627;Inherit;True;Property;_Dissolve_Tex;Dissolve_Tex;12;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;111;-371.5546,595.4609;Inherit;False;Property;_Hardness;Hardness;15;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;186;835.2158,2979.1;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;114;-64.20009,934.1133;Inherit;False;Property;_use_custom1_x_dissolve;use_custom1_x_dissolve;17;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;187;1363.942,2718.494;Inherit;False;Property;_use_frensel;use_frensel;29;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;136;356.4235,735.0688;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;137;615.4236,909.0699;Inherit;False;Property;_Distance;Distance;9;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;171;294.2073,1764.307;Inherit;True;Property;_Base_mask_Tex;Base_mask_Tex;18;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;107;-83.53351,722.2306;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-87.90869,475.0484;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-965.2712,-352.2458;Inherit;True;Property;_Base_Tex;Base_Tex;0;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;158;319.0657,74.29709;Inherit;False;Property;_Alpha_Constart;Alpha_Constart;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;159;169.176,55.80559;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;106;154.4665,484.2305;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;138;693.65,731.8209;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;177;714.3267,1753.741;Inherit;False;Base_mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;188;1609.616,2937.102;Inherit;False;frensel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;192;1089.46,1123.417;Inherit;False;188;frensel;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;129;250.8176,1082.969;Inherit;True;Property;_Mask;Mask;25;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;108;347.3645,456.7877;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;157;382.0789,-114.2966;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;139;973.1312,732.7918;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;30;-426.2529,-39.49192;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;89;-244.3707,181.4384;Inherit;False;Property;_Alpha;Alpha;11;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;178;1218.296,955.031;Inherit;False;177;Base_mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;77;1144.94,-818.4952;Inherit;False;345.9991;319.9341;Comment;3;75;76;135;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;1314.55,591.4058;Inherit;False;8;8;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;134;-392.2093,-373.0392;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;132;-772.1206,-835.1239;Inherit;False;Property;_R_light_color;R_light_color;4;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;133;-364.2225,-194.4455;Inherit;False;Property;_Constart;Constart;6;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;131;-736.7756,-664.9689;Inherit;False;Property;_R_dark_clolor;R_dark_clolor;5;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;42;12.78459,-642.9928;Inherit;False;Property;_Color;Color;7;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;130;-276.4476,-517.0469;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-119.1539,-298.591;Inherit;False;Property;_Color_power;Color_power;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;1313.94,-737.4952;Inherit;False;Property;_Zwrite;Zwrite;26;1;[Enum];Create;True;0;2;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;390.8635,-366.5229;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;76;1325.338,-613.9609;Inherit;False;Property;_Cull;Cull;27;1;[Enum];Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;1197.546,-583.0758;Inherit;False;Property;_Ztest;Ztest;28;2;[IntRange];[Enum];Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;35;1434.153,653.1163;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;33;516.004,-252.5605;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;88;1319.452,106.3459;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;194;1879.28,129.2776;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;DC/Doublecats_RA_common_shader;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;7;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;11;Surface;1;  Blend;0;Two Sided;1;Cast Shadows;1;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;0;Built-in Fog;0;Meta Pass;0;Extra Pre Pass;0;Vertex Position,InvertActionOnDeselection;1;0;5;False;True;True;True;False;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;195;1879.28,129.2776;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;196;1879.28,129.2776;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;193;1879.28,129.2776;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;197;1879.28,129.2776;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;123;0;125;1
WireConnection;123;1;125;2
WireConnection;118;1;117;0
WireConnection;118;0;116;4
WireConnection;120;0;123;0
WireConnection;120;1;118;0
WireConnection;143;0;144;0
WireConnection;98;0;96;3
WireConnection;98;1;96;4
WireConnection;164;0;160;3
WireConnection;164;1;160;4
WireConnection;97;0;96;1
WireConnection;97;1;96;2
WireConnection;163;0;161;0
WireConnection;162;0;160;1
WireConnection;162;1;160;2
WireConnection;165;0;162;0
WireConnection;165;1;164;0
WireConnection;95;0;97;0
WireConnection;95;1;98;0
WireConnection;181;0;180;0
WireConnection;181;1;179;0
WireConnection;167;0;163;2
WireConnection;166;0;163;0
WireConnection;166;1;163;1
WireConnection;141;0;143;2
WireConnection;151;0;146;3
WireConnection;151;1;146;4
WireConnection;156;0;147;0
WireConnection;142;0;143;0
WireConnection;142;1;143;1
WireConnection;126;0;120;0
WireConnection;150;0;146;1
WireConnection;150;1;146;2
WireConnection;154;0;156;2
WireConnection;153;0;156;0
WireConnection;153;1;156;1
WireConnection;152;0;150;0
WireConnection;152;1;151;0
WireConnection;168;0;165;0
WireConnection;168;2;166;0
WireConnection;168;1;167;0
WireConnection;140;0;95;0
WireConnection;140;2;142;0
WireConnection;140;1;141;0
WireConnection;90;0;92;1
WireConnection;90;1;92;2
WireConnection;182;0;181;0
WireConnection;190;0;182;0
WireConnection;174;0;173;1
WireConnection;174;1;173;2
WireConnection;170;0;168;0
WireConnection;170;1;169;0
WireConnection;155;0;152;0
WireConnection;155;2;153;0
WireConnection;155;1;154;0
WireConnection;91;0;140;0
WireConnection;91;1;90;0
WireConnection;189;1;182;0
WireConnection;189;0;190;0
WireConnection;145;0;155;0
WireConnection;145;1;149;0
WireConnection;175;0;170;0
WireConnection;175;1;174;0
WireConnection;185;0;184;0
WireConnection;185;1;183;0
WireConnection;93;1;140;0
WireConnection;93;0;91;0
WireConnection;127;0;93;0
WireConnection;127;1;126;0
WireConnection;176;1;170;0
WireConnection;176;0;175;0
WireConnection;99;1;145;0
WireConnection;186;0;189;0
WireConnection;186;1;184;0
WireConnection;186;2;185;0
WireConnection;114;1;110;0
WireConnection;114;0;113;3
WireConnection;187;1;191;0
WireConnection;187;0;186;0
WireConnection;171;1;176;0
WireConnection;107;0;111;0
WireConnection;107;2;114;0
WireConnection;109;0;99;1
WireConnection;109;1;111;0
WireConnection;20;1;127;0
WireConnection;159;0;20;1
WireConnection;106;0;109;0
WireConnection;106;1;107;0
WireConnection;138;1;136;0
WireConnection;138;0;137;0
WireConnection;177;0;171;1
WireConnection;188;0;187;0
WireConnection;108;0;106;0
WireConnection;157;0;159;0
WireConnection;157;1;158;0
WireConnection;139;0;138;0
WireConnection;31;0;30;4
WireConnection;31;1;157;0
WireConnection;31;2;89;0
WireConnection;31;3;108;0
WireConnection;31;4;129;1
WireConnection;31;5;139;0
WireConnection;31;6;178;0
WireConnection;31;7;192;0
WireConnection;134;0;20;1
WireConnection;134;1;133;0
WireConnection;130;0;131;0
WireConnection;130;1;132;0
WireConnection;130;2;134;0
WireConnection;32;0;42;0
WireConnection;32;1;130;0
WireConnection;32;2;43;0
WireConnection;32;3;30;0
WireConnection;35;0;31;0
WireConnection;33;0;32;0
WireConnection;88;0;33;0
WireConnection;88;3;35;0
WireConnection;194;2;33;0
WireConnection;194;3;35;0
ASEEND*/
//CHKSM=73D5DC67FEE492ADB9E6E41E6C86DF45DBEE7B43