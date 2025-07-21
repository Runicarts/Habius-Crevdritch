// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "QuickBrown/BackPanelShader"
{
	Properties
	{
		_Main("Main", 2D) = "white" {}
		_EmissionMap("EmissionMap", 2D) = "white" {}
		_EmissionMultiplier("EmissionMultiplier", Float) = 0
		_dot("dot", 2D) = "white" {}
		_DotOpacity("DotOpacity", Range( 0 , 1)) = 0
		_MaskMap("MaskMap", 2D) = "white" {}
		_AO("AO", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Main;
		uniform float4 _Main_ST;
		uniform sampler2D _dot;
		uniform float4 _dot_ST;
		uniform float _DotOpacity;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform sampler2D _MaskMap;
		uniform float4 _MaskMap_ST;
		uniform float _AO;
		uniform float _EmissionMultiplier;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Main = i.uv_texcoord * _Main_ST.xy + _Main_ST.zw;
			float2 uv_dot = i.uv_texcoord * _dot_ST.xy + _dot_ST.zw;
			float4 lerpResult10 = lerp( tex2D( _dot, uv_dot ) , float4( 1,1,1,0 ) , _DotOpacity);
			float4 temp_output_8_0 = ( tex2D( _Main, uv_Main ) * lerpResult10 );
			o.Albedo = temp_output_8_0.rgb;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			float2 uv_MaskMap = i.uv_texcoord * _MaskMap_ST.xy + _MaskMap_ST.zw;
			float4 tex2DNode12 = tex2D( _MaskMap, uv_MaskMap );
			float lerpResult15 = lerp( tex2DNode12.g , 1.0 , _AO);
			o.Emission = ( temp_output_8_0 * ( ( tex2D( _EmissionMap, uv_EmissionMap ) * lerpResult15 ) * _EmissionMultiplier ) ).rgb;
			o.Smoothness = tex2DNode12.a;
			o.Occlusion = tex2DNode12.g;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-105,261.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-209,441.5;Inherit;False;Property;_EmissionMultiplier;EmissionMultiplier;2;0;Create;True;0;0;0;False;0;False;0;7.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-35,127.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-268.7,-103.3134;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;7;-766.7148,-414.0681;Inherit;True;Property;_dot;dot;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1006.451,-425.1543;Inherit;False;0;7;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;10;-416.5809,-408.9774;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;508,-366;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;QuickBrown/BackPanelShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;1;-754.3414,-104.3859;Inherit;True;Property;_Main;Main;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-345.3468,202.4109;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-973.4175,112.0618;Inherit;True;Property;_EmissionMap;EmissionMap;1;0;Create;True;0;0;0;False;0;False;-1;None;7d96ce1aafeea7a409e89591cd5a5382;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;15;-521.3468,268.4109;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-854.7463,697.811;Inherit;False;Property;_AO;AO;6;0;Create;True;0;0;0;False;0;False;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-910.0223,408.5585;Inherit;True;Property;_MaskMap;MaskMap;5;0;Create;True;0;0;0;False;0;False;-1;None;eed2ce421c3788241987947e8c4bc533;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-741.5076,-214.3105;Inherit;False;Property;_DotOpacity;DotOpacity;4;0;Create;True;0;0;0;False;0;False;0;0.35;0;1;0;1;FLOAT;0
WireConnection;5;0;13;0
WireConnection;5;1;4;0
WireConnection;3;0;8;0
WireConnection;3;1;5;0
WireConnection;8;0;1;0
WireConnection;8;1;10;0
WireConnection;7;1;9;0
WireConnection;10;0;7;0
WireConnection;10;2;11;0
WireConnection;0;0;8;0
WireConnection;0;2;3;0
WireConnection;0;4;12;4
WireConnection;0;5;12;2
WireConnection;13;0;2;0
WireConnection;13;1;15;0
WireConnection;15;0;12;2
WireConnection;15;2;16;0
ASEEND*/
//CHKSM=1BD33CA4F1B878EE8231A0AC716DADE479B43D7C