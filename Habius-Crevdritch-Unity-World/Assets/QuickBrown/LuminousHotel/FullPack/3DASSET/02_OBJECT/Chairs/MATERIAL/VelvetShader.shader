// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VelvetShader"
{
	Properties
	{
		[NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		_BaseColorHue("BaseColorHue", Range( 0 , 1)) = 0
		_BaseColorSaturation("BaseColorSaturation", Range( -1 , 1)) = 0
		_BaseColorValue("BaseColorValue", Range( -1 , 1)) = 0
		_ColorTint("ColorTint", Color) = (0,0,0,0)
		[NoScaleOffset]_MetalicSmoothness("MetalicSmoothness", 2D) = "white" {}
		_Smoothness("Smoothness", Float) = 1
		[NoScaleOffset][Normal]_Normal("Normal", 2D) = "bump" {}
		_NormalStrength("NormalStrength", Float) = 1
		[NoScaleOffset]_AO_Detail("AO_Detail", 2D) = "white" {}
		_AO_DetailStrength("AO_DetailStrength", Float) = 0
		[NoScaleOffset][Normal]_Normal_Detail("Normal_Detail", 2D) = "bump" {}
		_Normal_DetailStrength("Normal_DetailStrength", Float) = 1
		_ScaleDetail("ScaleDetail", Float) = 7
		_RimPower("RimPower", Float) = 1
		[NoScaleOffset]_RimNoise("RimNoise", 2D) = "white" {}
		_RimColor("RimColor", Color) = (1,1,1,0)
		_RimNoiseScale("RimNoiseScale", Float) = 1
		[NoScaleOffset]_RimMask("RimMask", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			half ASEIsFrontFacing : VFACE;
		};

		uniform sampler2D _Normal_Detail;
		uniform float _ScaleDetail;
		uniform float _Normal_DetailStrength;
		uniform sampler2D _RimMask;
		uniform sampler2D _Normal;
		uniform float _NormalStrength;
		uniform sampler2D _Albedo;
		uniform float _BaseColorHue;
		uniform float _BaseColorSaturation;
		uniform float _BaseColorValue;
		uniform float4 _ColorTint;
		uniform float _RimPower;
		uniform float4 _RimColor;
		uniform sampler2D _RimNoise;
		uniform float _RimNoiseScale;
		uniform sampler2D _MetalicSmoothness;
		uniform float _Smoothness;
		uniform sampler2D _AO_Detail;
		uniform float _AO_DetailStrength;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_ScaleDetail).xx;
			float2 uv_TexCoord13 = i.uv_texcoord * temp_cast_0;
			float2 uv_RimMask45 = i.uv_texcoord;
			float4 tex2DNode45 = tex2D( _RimMask, uv_RimMask45 );
			float3 lerpResult78 = lerp( UnpackScaleNormal( tex2D( _Normal_Detail, uv_TexCoord13 ), _Normal_DetailStrength ) , float3( 0,0,1 ) , tex2DNode45.rgb);
			float3 temp_output_17_0 = BlendNormals( lerpResult78 , UnpackScaleNormal( tex2D( _Normal, i.uv_texcoord ), _NormalStrength ) );
			o.Normal = temp_output_17_0;
			float3 hsvTorgb51 = RGBToHSV( tex2D( _Albedo, i.uv_texcoord ).rgb );
			float2 uv_RimMask65 = i.uv_texcoord;
			float4 temp_output_76_0 = ( 1.0 - tex2D( _RimMask, uv_RimMask65 ) );
			float3 hsvTorgb52 = HSVToRGB( float3(( hsvTorgb51.x + ( _BaseColorHue * temp_output_76_0 ) ).r,( hsvTorgb51.y + ( _BaseColorSaturation * temp_output_76_0 ) ).r,( hsvTorgb51.z + ( _BaseColorValue * temp_output_76_0 ) ).r) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 ase_tanViewDir = mul( ase_worldToTangent, ase_worldViewDir );
			float3 normalizeResult20 = normalize( ase_tanViewDir );
			float dotResult21 = dot( temp_output_17_0 , normalizeResult20 );
			float2 temp_cast_7 = (_RimNoiseScale).xx;
			float2 uv_TexCoord29 = i.uv_texcoord * temp_cast_7;
			float4 lerpResult44 = lerp( ( ( pow( ( 1.0 - saturate( dotResult21 ) ) , _RimPower ) * _RimColor ) * tex2D( _RimNoise, uv_TexCoord29 ) ) , float4( 0,0,0,0 ) , tex2DNode45);
			o.Albedo = ( ( float4( ( hsvTorgb52 + float3( 0,0,0 ) ) , 0.0 ) * _ColorTint ) + lerpResult44 ).rgb;
			float4 tex2DNode2 = tex2D( _MetalicSmoothness, i.uv_texcoord );
			float4 switchResult49 = (((i.ASEIsFrontFacing>0)?(tex2DNode2):(float4( 0,0,0,0 ))));
			o.Metallic = switchResult49.r;
			float switchResult50 = (((i.ASEIsFrontFacing>0)?(( tex2DNode2.a * _Smoothness )):(0.0)));
			o.Smoothness = switchResult50;
			float4 color11 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 lerpResult6 = lerp( color11 , tex2D( _AO_Detail, uv_TexCoord13 ) , _AO_DetailStrength);
			float4 lerpResult79 = lerp( lerpResult6 , float4( 1,1,1,0 ) , tex2DNode45);
			o.Occlusion = lerpResult79.r;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.RangedFloatNode;14;-3997.774,87.43391;Inherit;False;Property;_ScaleDetail;ScaleDetail;13;0;Create;True;0;0;0;False;0;False;7;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-3808.268,75.01088;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;42;-3520.962,-117.2397;Inherit;False;Property;_Normal_DetailStrength;Normal_DetailStrength;12;0;Create;True;0;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;15;-3129.936,-165.8288;Inherit;True;Property;_Normal_Detail;Normal_Detail;11;2;[NoScaleOffset];[Normal];Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;45;-1384.844,1456.098;Inherit;True;Property;_RimMask;RimMask;18;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;36;-3439.076,541.4271;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;43;-3082.5,1137.552;Inherit;False;1631.86;702.9469;Rim;13;27;23;21;19;24;26;25;38;40;30;29;28;20;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-3343.036,100.6125;Inherit;False;Property;_NormalStrength;NormalStrength;8;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-3124.743,37.08271;Inherit;True;Property;_Normal;Normal;7;2;[NoScaleOffset];[Normal];Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;78;-2643.2,-339.5983;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;19;-3032.5,1203.758;Inherit;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;20;-2817.571,1209.962;Inherit;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;17;-2490.848,-164.3529;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;65;-1702.912,275.9962;Inherit;True;Property;_TextureSample0;Texture Sample 0;18;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;45;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;21;-2547.865,1187.552;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-1392.916,-33.44436;Inherit;False;Property;_BaseColorHue;BaseColorHue;1;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-2290.447,-41.02626;Inherit;True;Property;_Albedo;Albedo;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;23;-2414.38,1188.647;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-1388.833,90.80811;Inherit;False;Property;_BaseColorSaturation;BaseColorSaturation;2;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;76;-1286.59,292.575;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-1378.921,212.1959;Inherit;False;Property;_BaseColorValue;BaseColorValue;3;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-989.4673,7.28418;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-993.1673,122.4842;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-992.322,273.1559;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;24;-2271.401,1189.273;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2672.703,1642.438;Inherit;False;Property;_RimNoiseScale;RimNoiseScale;17;0;Create;True;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;51;-1736.625,-42.09424;Inherit;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;26;-2287.457,1268.577;Inherit;False;Property;_RimPower;RimPower;14;0;Create;True;0;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-2425.596,1636.961;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;61;-831.2854,169.0146;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0.1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;25;-2104.438,1189.162;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;40;-2127.279,1349.062;Inherit;False;Property;_RimColor;RimColor;16;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.9528301,0.2382075,0.2809993,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;53;-847.9366,-54.49848;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0.1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-830.6539,63.11919;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0.1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-1880.097,1278.923;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;28;-2127.328,1610.498;Inherit;True;Property;_RimNoise;RimNoise;15;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;8317142117f23b44dbb00777b92721e7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.HSVToRGBNode;52;-600.7849,-5.39816;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-1619.64,1338.189;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;70;-293.9521,-64.89445;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;33;-152.8428,201.9251;Inherit;False;Property;_ColorTint;ColorTint;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-631.2188,1271.795;Inherit;True;Property;_AO_Detail;AO_Detail;9;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-581.4913,1082.989;Inherit;False;Constant;_Color1;Color 1;6;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-554.7695,1474.834;Inherit;False;Property;_AO_DetailStrength;AO_DetailStrength;10;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-950.9883,548.7219;Inherit;True;Property;_MetalicSmoothness;MetalicSmoothness;5;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;223.9058,-80.26123;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-817.3378,837.6534;Inherit;False;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-565.4873,710.4889;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;338.9462,163.3112;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;6;-266.9966,1083.715;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwitchByFaceNode;49;-537.5102,549.6409;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;79;270.3397,734.2817;Inherit;False;3;0;COLOR;1,1,1,0;False;1;COLOR;1,1,1,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwitchByFaceNode;50;-393.8551,668.7469;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1349.081,70.62782;Float;False;True;-1;2;;0;0;Standard;VelvetShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.LerpOp;44;-1069.132,1280.461;Inherit;False;3;0;COLOR;1,1,1,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
WireConnection;13;0;14;0
WireConnection;15;1;13;0
WireConnection;15;5;42;0
WireConnection;3;1;36;0
WireConnection;3;5;41;0
WireConnection;78;0;15;0
WireConnection;78;2;45;0
WireConnection;20;0;19;0
WireConnection;17;0;78;0
WireConnection;17;1;3;0
WireConnection;21;0;17;0
WireConnection;21;1;20;0
WireConnection;1;1;36;0
WireConnection;23;0;21;0
WireConnection;76;0;65;0
WireConnection;75;0;54;0
WireConnection;75;1;76;0
WireConnection;74;0;58;0
WireConnection;74;1;76;0
WireConnection;73;0;60;0
WireConnection;73;1;76;0
WireConnection;24;0;23;0
WireConnection;51;0;1;0
WireConnection;29;0;30;0
WireConnection;61;0;51;3
WireConnection;61;1;73;0
WireConnection;25;0;24;0
WireConnection;25;1;26;0
WireConnection;53;0;51;1
WireConnection;53;1;75;0
WireConnection;59;0;51;2
WireConnection;59;1;74;0
WireConnection;38;0;25;0
WireConnection;38;1;40;0
WireConnection;28;1;29;0
WireConnection;52;0;53;0
WireConnection;52;1;59;0
WireConnection;52;2;61;0
WireConnection;27;0;38;0
WireConnection;27;1;28;0
WireConnection;70;0;52;0
WireConnection;4;1;13;0
WireConnection;2;1;36;0
WireConnection;32;0;70;0
WireConnection;32;1;33;0
WireConnection;34;0;2;4
WireConnection;34;1;35;0
WireConnection;31;0;32;0
WireConnection;31;1;44;0
WireConnection;6;0;11;0
WireConnection;6;1;4;0
WireConnection;6;2;5;0
WireConnection;49;0;2;0
WireConnection;79;0;6;0
WireConnection;79;2;45;0
WireConnection;50;0;34;0
WireConnection;0;0;31;0
WireConnection;0;1;17;0
WireConnection;0;3;49;0
WireConnection;0;4;50;0
WireConnection;0;5;79;0
WireConnection;44;0;27;0
WireConnection;44;2;45;0
ASEEND*/
//CHKSM=5A465B13A63F1063711470778CD48D73E09BF20C