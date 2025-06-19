// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Light Volume Samples/Light Volumes Detector"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("Texture", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Glossiness("Smoothness", Range( 0 , 1)) = 1
		[Enum(UnityEngine.Rendering.CullMode)]_Culling("Culling", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull [_Culling]
		CGINCLUDE
		#include "../../Shaders/LightVolumes.cginc"
		#include "UnityStandardBRDF.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#define ASE_VERSION 19801
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			float3 worldPos;
		};

		uniform float _Culling;
		uniform sampler2D _MainTex;
		uniform float _Metallic;
		uniform float _Glossiness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult471 = (float2(1.0 , 0.5));
			float lerpResult469 = lerp( 0.0 , 0.5 , saturate( _UdonLightVolumeEnabled ));
			float2 appendResult472 = (float2(0.0 , lerpResult469));
			float4 tex2DNode3 = tex2D( _MainTex, (i.uv_texcoord*appendResult471 + appendResult472) );
			float3 Albedo337 = tex2DNode3.rgb;
			float3 temp_output_454_0 = Albedo337;
			o.Albedo = temp_output_454_0;
			float3 ase_normalWS = i.worldNormal;
			float3 normalizeResult17_g225 = normalize( ase_normalWS );
			float3 worldNormal2_g225 = normalizeResult17_g225;
			float localLightVolumeSH1_g3 = ( 0.0 );
			float3 ase_positionWS = i.worldPos;
			float3 temp_output_6_0_g3 = ase_positionWS;
			float3 worldPos1_g3 = temp_output_6_0_g3;
			float3 L01_g3 = float3( 0,0,0 );
			float3 L1r1_g3 = float3( 0,0,0 );
			float3 L1g1_g3 = float3( 0,0,0 );
			float3 L1b1_g3 = float3( 0,0,0 );
			LightVolumeSH( worldPos1_g3 , L01_g3 , L1r1_g3 , L1g1_g3 , L1b1_g3 );
			float3 L098 = L01_g3;
			float3 L02_g225 = L098;
			float3 L1r99 = L1r1_g3;
			float3 L1r2_g225 = L1r99;
			float3 L1g100 = L1g1_g3;
			float3 L1g2_g225 = L1g100;
			float3 L1b101 = L1b1_g3;
			float3 L1b2_g225 = L1b101;
			float3 localLightVolumeEvaluate2_g225 = LightVolumeEvaluate( worldNormal2_g225 , L02_g225 , L1r2_g225 , L1g2_g225 , L1b2_g225 );
			float Metallic334 = ( (Albedo337).x * _Metallic );
			float3 temp_output_138_0_g224 = Albedo337;
			float3 albedo157_g224 = temp_output_138_0_g224;
			float Smoothness109 = ( (Albedo337).y * _Glossiness );
			float temp_output_3_0_g224 = Smoothness109;
			float smoothness157_g224 = temp_output_3_0_g224;
			float temp_output_137_0_g224 = Metallic334;
			float metallic157_g224 = temp_output_137_0_g224;
			float3 normalizeResult161_g224 = normalize( ase_normalWS );
			float3 temp_output_2_0_g224 = normalizeResult161_g224;
			float3 worldNormal157_g224 = temp_output_2_0_g224;
			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_positionWS );
			float3 ase_viewDirSafeWS = Unity_SafeNormalize( ase_viewVectorWS );
			float3 temp_output_9_0_g224 = ase_viewDirSafeWS;
			float3 viewDir157_g224 = temp_output_9_0_g224;
			float3 temp_output_65_0_g224 = L098;
			float3 L0157_g224 = temp_output_65_0_g224;
			float3 temp_output_1_0_g224 = L1r99;
			float3 L1r157_g224 = temp_output_1_0_g224;
			float3 temp_output_36_0_g224 = L1g100;
			float3 L1g157_g224 = temp_output_36_0_g224;
			float3 temp_output_37_0_g224 = L1b101;
			float3 L1b157_g224 = temp_output_37_0_g224;
			float3 localLightVolumeSpecular157_g224 = LightVolumeSpecular( albedo157_g224 , smoothness157_g224 , metallic157_g224 , worldNormal157_g224 , viewDir157_g224 , L0157_g224 , L1r157_g224 , L1g157_g224 , L1b157_g224 );
			float3 Speculars412 = localLightVolumeSpecular157_g224;
			float3 Emission452 = ( ( localLightVolumeEvaluate2_g225 * Albedo337 * ( 1.0 - Metallic334 ) ) + Speculars412 );
			o.Emission = ( ( Albedo337 + Emission452 ) * 0.5 );
			o.Metallic = Metallic334;
			o.Smoothness = Smoothness109;
			o.Alpha = 1;
			float Opacity455 = tex2DNode3.a;
			clip( Opacity455 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred noambient 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
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
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
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
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.CommentaryNode;457;-3301.363,-1392;Inherit;False;2041.363;775.4034;Albedo;10;473;472;471;470;469;468;467;455;337;3;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;467;-3200,-752;Inherit;False;Global;_UdonLightVolumeEnabled;_UdonLightVolumeEnabled;1;0;Fetch;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;468;-2896,-752;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;469;-2720,-800;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;470;-2544,-1136;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;471;-2496,-880;Inherit;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;472;-2496,-768;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;473;-2272,-1120;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;3;-1968,-1152;Inherit;True;Property;_MainTex;Texture;1;0;Create;False;0;0;0;False;0;False;-1;23152d215e966f74192da73daab1fe68;23152d215e966f74192da73daab1fe68;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;337;-1680,-1136;Inherit;False;Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;481;-2608,-528;Inherit;False;337;Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-2096,-288;Inherit;False;Property;_Glossiness;Smoothness;3;0;Create;False;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;483;-2384,-528;Inherit;True;False;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-2096,-192;Inherit;False;Property;_Metallic;Metallic;2;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;486;-2368,-224;Inherit;True;True;False;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;78;-1936,-16;Inherit;False;LightVolume;-1;;3;78706f2b7f33b1c44b4f381a7904a7e1;4,8,0,10,0,11,0,12,0;1;6;FLOAT3;0,0,0;False;4;FLOAT3;13;FLOAT3;14;FLOAT3;15;FLOAT3;16
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;485;-1808,-320;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;488;-1824,-224;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;459;-656,-880;Inherit;False;893.6;568.6;Speculars;9;412;475;122;115;108;123;340;339;111;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;334;-1664,-192;Inherit;False;Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;109;-1664,-272;Inherit;False;Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;-1664,16;Inherit;False;L1r;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;-1664,112;Inherit;False;L1g;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-1664,208;Inherit;False;L1b;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;98;-1664,-80;Inherit;False;L0;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-576,-752;Inherit;False;109;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;339;-576,-816;Inherit;False;337;Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;340;-576,-688;Inherit;False;334;Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-576,-432;Inherit;False;101;L1b;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;-576,-560;Inherit;False;99;L1r;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-576,-624;Inherit;False;98;L0;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-576,-496;Inherit;False;100;L1g;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;458;-656,-176;Inherit;False;1403.16;406.6;Indirect and Speculars;12;413;452;124;451;450;449;448;474;406;80;338;336;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;336;-368,128;Inherit;False;334;Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;475;-304,-832;Inherit;False;LightVolumeSpecular;-1;;224;a5ec4a1f240e00f47a5deb132f113431;1,159,0;9;138;FLOAT3;1,1,1;False;3;FLOAT;0;False;137;FLOAT;0;False;65;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;36;FLOAT3;0,0,0;False;37;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;9;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;448;-576,-128;Inherit;False;98;L0;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;449;-576,-64;Inherit;False;99;L1r;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;450;-576,0;Inherit;False;100;L1g;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;451;-576,64;Inherit;False;101;L1b;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;338;-224,48;Inherit;False;337;Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;80;-192,128;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;474;-288,-128;Inherit;False;LightVolumeEvaluate;-1;;225;4919cc1d83093f24f802ce655e9f3303;0;5;5;FLOAT3;0,0,0;False;13;FLOAT3;1,1,1;False;14;FLOAT3;0,0,0;False;15;FLOAT3;0,0,0;False;16;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;412;-16,-832;Inherit;False;Speculars;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;406;0,-128;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;413;0,32;Inherit;False;412;Speculars;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;124;192,-128;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;452;336,-128;Inherit;False;Emission;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;454;1088,-256;Inherit;False;337;Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;453;1088,-96;Inherit;False;452;Emission;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;455;-1664,-1040;Inherit;False;Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;479;1312,-144;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;478;1280,-32;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;440;1088,-16;Inherit;False;334;Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;418;1504,240;Inherit;False;Property;_Culling;Culling;4;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;456;1072,144;Inherit;False;455;Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;441;1072,64;Inherit;False;109;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;477;1440,-144;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;72;1600,-224;Float;False;True;-1;7;AmplifyShaderEditor.MaterialInspector;0;0;Standard;Light Volume Samples/Light Volumes Detector;False;False;False;False;True;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;_ZWrite;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;AlphaTest;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;True;_Culling;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;468;0;467;0
WireConnection;469;2;468;0
WireConnection;472;1;469;0
WireConnection;473;0;470;0
WireConnection;473;1;471;0
WireConnection;473;2;472;0
WireConnection;3;1;473;0
WireConnection;337;0;3;5
WireConnection;483;0;481;0
WireConnection;486;0;481;0
WireConnection;485;0;483;0
WireConnection;485;1;52;0
WireConnection;488;0;486;0
WireConnection;488;1;53;0
WireConnection;334;0;488;0
WireConnection;109;0;485;0
WireConnection;99;0;78;14
WireConnection;100;0;78;15
WireConnection;101;0;78;16
WireConnection;98;0;78;13
WireConnection;475;138;339;0
WireConnection;475;3;111;0
WireConnection;475;137;340;0
WireConnection;475;65;115;0
WireConnection;475;1;108;0
WireConnection;475;36;122;0
WireConnection;475;37;123;0
WireConnection;80;0;336;0
WireConnection;474;13;448;0
WireConnection;474;14;449;0
WireConnection;474;15;450;0
WireConnection;474;16;451;0
WireConnection;412;0;475;0
WireConnection;406;0;474;0
WireConnection;406;1;338;0
WireConnection;406;2;80;0
WireConnection;124;0;406;0
WireConnection;124;1;413;0
WireConnection;452;0;124;0
WireConnection;455;0;3;4
WireConnection;479;0;454;0
WireConnection;479;1;453;0
WireConnection;477;0;479;0
WireConnection;477;1;478;0
WireConnection;72;0;454;0
WireConnection;72;2;477;0
WireConnection;72;3;440;0
WireConnection;72;4;441;0
WireConnection;72;10;456;0
ASEEND*/
//CHKSM=5B331C43129D8556D78D87E1D5EF0279770C03B8