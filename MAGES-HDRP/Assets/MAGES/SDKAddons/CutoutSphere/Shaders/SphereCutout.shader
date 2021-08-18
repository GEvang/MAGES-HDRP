Shader "Custom/SphereCutout"
{
	// This is a custom surface shader made to immitate a spheric cutout effect
	// Made by Andreas Michelakis ~1043
	// Based on unity documentation

	Properties
	{
		// Basic Cutout Sphere Properties
		_Sphere_position("Position", Vector)    = (0,0,0,0)		// The world space position of the sphere (an Object holding the SphereCutout script)
		_Sphere_radius("Radius", Float)	        = 2				// The radius of the sphere 
		[Toggle]_Invert("Invert", Float)        = 0				// Inverts the cutout effect if set to 1!


		// Cutout's border properties
		_Border_radius("Border radius", Range(0 , 2))  = 1									// Radius
		[HDR]_Border_color("Border color", Color)      = (0.8602941,0.2087478,0.2087478,0)  // Color

		// Material Texture Properties
		[NoScaleOffset]_Albedo("Albedo", 2D)		 = "white" {}		// Albedo 
		_Albedo_tint("Albedo_tint", Color)			 = (1,1,1,1)		// Albedo tint
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset][Normal]_Normal("Normal", 2D) = "bump" {}	    // Normal 
		[NoScaleOffset]_Metallic("Metallic", 2D)	 = "white" {}		// Mettalic
		_Metallic_multiplier("Metallic_multiplier", Range(0 , 1)) = 0	// Multiply metallic property
		_Smoothness("Smoothness", Range(0 , 1))		 = 0				// Smoothness
		[NoScaleOffset]_Occlusion("Occlusion", 2D)   = "white" {}		// Occlusion
		_Occlusion_strength("Occlusion strength", Range(0 , 1))   = 0	// Occlusion strength
		[NoScaleOffset]_Emission("Emission", 2D)     = "black" {}       // Emmision
		[HDR]_Emission_tint("Emission_tint", Color)  = (1,1,1,1)		// Emission tint
		_Tiling("Tiling", Vector)					 = (1,1,0,0)		// Texture tilling
		_Offset("Offset", Vector)					 = (0,0,0,0)		// Offset
		_Cutoff("Mask Clip Value", Float)			 = 0.5			    // Mask clip/cutoff
		[HideInInspector] _texcoord("", 2D)		     = "white" {}			  
		[HideInInspector] __dirty("", Int)			 = 1
	}

	SubShader
	{  // Rendered in sync with geometry shader as opaque
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true" }
		Cull off  //Both faces are rendered
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface mySurf Standard keepalpha addshadow fullforwardshadows
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		fixed4 _Color;
		uniform sampler2D _Normal;
		uniform float2    _Tiling;
		uniform float2	  _Offset;
		uniform float4    _Albedo_tint;
		uniform sampler2D _Albedo;
		uniform float4    _Emission_tint;
		uniform sampler2D _Emission;
		uniform float4    _Border_color;
		uniform float3    _Sphere_position;
		uniform float     _Sphere_radius;
		uniform float	  _Border_radius;
		uniform sampler2D _Metallic;
		uniform float	  _Metallic_multiplier;
		uniform float     _Smoothness;
		uniform sampler2D _Occlusion;
		uniform float     _Occlusion_strength;
		uniform float	  _Invert;
		uniform float	  _Cutoff = 0.5;

		void mySurf(Input i , inout SurfaceOutputStandard o)
		{
			float2 uvCoordinates      = i.uv_texcoord * _Tiling + _Offset;											 // Calculate uv coordinates with tiling and offset;  
			float  distanceFromSphere = distance(_Sphere_position , i.worldPos);									 // Calculate distance between the object and the sphere
			float  intersects         = step((1.0 - saturate((distanceFromSphere / _Sphere_radius))) , 0.5);		 // Check if sphere is "intersecting" with object ~~ (1-(1 to 0) >= 0.5) ~~ (returns 0 or 1)
			float  distanceToBorder   = saturate((distanceFromSphere / (_Border_radius + _Sphere_radius)));			 // Calculate a value between 1-0 representing the coloured border pixels changed based on distanceFromSphere
			float  cutoutBorder       = (intersects - step((1.0 - distanceToBorder) , 0.5));						 // Create a border according to the distance from sphere (returns 0 or 1) 
			o.Normal	 =  UnpackScaleNormal(tex2D(_Normal, uvCoordinates), 0.4);											 // Unpack compressed normal map
			fixed4 c =		(tex2D(_Albedo, uvCoordinates)) * _Color;
			o.Albedo = c.rgb;																					 // Apply albedo with its tint
			o.Emission   = ((_Emission_tint * tex2D(_Emission, uvCoordinates)) + (_Border_color * cutoutBorder)).rgb;// Apply emission considering cutout border properties (border effect created here)
			o.Metallic   = (tex2D(_Metallic, uvCoordinates) * _Metallic_multiplier).r;							     // Apply metallic with its multiplier and use the r channel (standard shader uses this for metallic)
			o.Smoothness = _Smoothness;																				 // Apply smoothness
			o.Occlusion = lerp(1.0, tex2D(_Occlusion, uvCoordinates).r, _Occlusion_strength);						 // Apply Occlusion
			o.Alpha = c.a;
			float cutoutMask = lerp(intersects,step(distanceToBorder , 0.5),_Invert);								 // Interpolate between intersects value ( 1 or 0 ) and border ( 1 or 0 ) by invertion ( 0 or 1 )
			clip(cutoutMask - _Cutoff);																				 // Discard pixel if cutoutMask-Cutoff < 0
		}
		ENDCG
		}
			Fallback "Diffuse"
}