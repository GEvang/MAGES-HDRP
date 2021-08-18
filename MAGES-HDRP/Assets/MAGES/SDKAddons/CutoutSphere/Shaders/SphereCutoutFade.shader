Shader "Custom/SphereCutoutFade" {
	Properties{
		// Basic Cutout Sphere Properties
		_Sphere_position("Position", Vector) = (0,0,0,0)		// The world space position of the sphere (an Object holding the SphereCutout script)
		_Sphere_radius("Radius", Float) = 2				// The radius of the sphere 
		[Toggle]_Invert("Invert", Float) = 0				// Inverts the cutout effect if set to 1!


		// Cutout's border properties
		_Border_radius("Border radius", Range(0 , 2)) = 1									// Radius
		[HDR]_Border_color("Border color", Color) = (0.8602941,0.2087478,0.2087478,0)  // Color
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		[NoScaleOffset]_Emission("Emission", 2D) = "black" {}       // Emmision
		[NoScaleOffset][Normal]_Normal("Normal", 2D) = "bump" {}	    // Normal 
		[NoScaleOffset]_Metallic("Metallic", 2D) = "white" {}		// Mettalic
		_Metallic_multiplier("Metallic_multiplier", Range(0 , 1)) = 0	// Multiply metallic property
		[HDR]_Emission_tint("Emission_tint", Color) = (1,1,1,1)
		_Offset("Offset", Vector) = (0,0,0,0)		// Offset
		_Tiling("Tiling", Vector) = (1,1,0,0)		// Texture tilling



	}
		SubShader{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjectors" = "True"}
			LOD 200

			// ZWrite pre-pass
			Pass {
				ZWrite On
				ColorMask 0

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float4 vert(float4 vertex : POSITION) : SV_POSITION { return UnityObjectToClipPos(vertex); }
				fixed4 frag() : SV_Target { return 0; }
				ENDCG
			}

			CGPROGRAM
					// Physically based Standard lighting model, and enable shadows on all light types
					#pragma surface surf Standard fullforwardshadows alpha:fade

					// Use shader model 3.0 target, to get nicer looking lighting
					#pragma target 3.0

					sampler2D _MainTex;

					struct Input {
						float2 uv_MainTex;
						float3 worldPos;
					};

					half _Glossiness;
					fixed4 _Color;
					uniform float	  _Cutoff = 0.5;
					uniform float4    _Border_color;
					uniform float3    _Sphere_position;
					uniform float     _Sphere_radius;
					uniform float	  _Border_radius;
					uniform float	  _Invert;
					uniform float2    _Tiling;
					uniform float2	  _Offset;
					uniform float4    _Emission_tint;
					uniform sampler2D _Emission;
					uniform sampler2D _Normal;
					uniform sampler2D _Metallic;
					uniform float	  _Metallic_multiplier;



					void surf(Input IN, inout SurfaceOutputStandard o) {
						float2 uvCoordinates = IN.uv_MainTex * _Tiling + _Offset;
						float  distanceFromSphere = distance(_Sphere_position, IN.worldPos);
						float  intersects = step((1.0 - saturate((distanceFromSphere / _Sphere_radius))), 0.5);
						float  distanceToBorder = saturate((distanceFromSphere / (_Border_radius + _Sphere_radius)));
						float  cutoutBorder = (intersects - step((1.0 - distanceToBorder), 0.5));
						o.Normal = UnpackScaleNormal(tex2D(_Normal, uvCoordinates), 0.4);
						fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
						o.Albedo = c.rgb;
						o.Emission = ((_Emission_tint * tex2D(_Emission, uvCoordinates)) + (_Border_color * cutoutBorder)).rgb;
						o.Metallic = (tex2D(_Metallic, uvCoordinates) * _Metallic_multiplier).r;							     // Apply metallic with its multiplier and use the r channel (standard shader uses this for metallic)
						o.Smoothness = _Glossiness;
						o.Alpha = c.a;
						float cutoutMask = lerp(intersects, step(distanceToBorder, 0.5), _Invert);
						clip(cutoutMask - _Cutoff);
					}
					ENDCG
		}
			FallBack "Diffuse"
}