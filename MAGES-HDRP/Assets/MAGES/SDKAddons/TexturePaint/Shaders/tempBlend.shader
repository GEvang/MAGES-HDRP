/*
MIT License

Copyright (c) 2017 Es_Program

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
Shader "ovidVR/tempBlend" {
	Properties{
	_MainTex("MainTex", 2D) = "white"
	_Brush("Brush", 2D) = "white"
	_BrushScale("BrushScale", FLOAT) = 0.1
	_BrushRotate("Rotate", FLOAT) = 0
	_ControlColor("ControlColor", VECTOR) = (0,0,0,0)
	_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
	[KeywordEnum(USE_CONTROL, USE_BRUSH, NEUTRAL, ALPHA_ONLY)]
	INK_PAINTER_COLOR_BLEND("Color Blend Keyword", FLOAT) = 0
	}

		SubShader{
		CGINCLUDE

#include "InkPainterFoundation.cginc"

		struct app_data {
		float4 vertex:POSITION;
		float4 uv:TEXCOORD0;
	};

	struct v2f {
		float4 screen:SV_POSITION;
		float4 uv:TEXCOORD0;
	};

	sampler2D _MainTex;
	sampler2D _Brush;
	float4 _PaintUV;
	float _BrushScale;
	float _BrushRotate;
	float4 _ControlColor;
	ENDCG

		Pass{
		CGPROGRAM
#pragma multi_compile INK_PAINTER_COLOR_BLEND_USE_CONTROL INK_PAINTER_COLOR_BLEND_USE_BRUSH INK_PAINTER_COLOR_BLEND_NEUTRAL INK_PAINTER_COLOR_BLEND_ALPHA_ONLY
#pragma vertex vert
#pragma fragment frag

		v2f vert(app_data i) {
		v2f o;
		o.screen = UnityObjectToClipPos(i.vertex);
		o.uv = i.uv;
		return o;
	}

	float4 frag(v2f i) : SV_TARGET{
		float h = _BrushScale;
	float4 base = SampleTexture(_MainTex, i.uv.xy);
	float4 brushColor = float4(1, 1, 1, 1);

	if (IsPaintRange(i.uv, _PaintUV, h, _BrushRotate)) {
		float2 uv = CalcBrushUV(i.uv, _PaintUV, h, _BrushRotate);
		brushColor = SampleTexture(_Brush, uv.xy);

		return INK_PAINTER_COLOR_BLEND(base, brushColor, _ControlColor);
	}
	return base;
	}

		ENDCG
	}
	}
}