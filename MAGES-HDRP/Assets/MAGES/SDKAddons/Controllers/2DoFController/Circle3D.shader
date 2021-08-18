// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Circle3D" {
    Properties{
        _MainTex("Font Texture", 2D) = "white" {}
        _Color("Text Color", Color) = (1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15
        _HighlightSectionOffset("Highlight Section Offset", Float) = 0
        _HighlightSectionRadius("Highlight Section Radius", Range(0.001, 1.0)) = 20.0
    }

        SubShader{

            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
            }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Lighting Off
            Cull Off
            ZTest Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform fixed4 _Color;
                uniform float _HighlightSectionOffset;
                uniform float _HighlightSectionRadius;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color * _Color;

                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
    #ifdef UNITY_HALF_TEXEL_OFFSET
                    o.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1,1);
    #endif
                    return o;
                }

                float maprange(float value, float from1, float to1, float from2, float to2) {
                    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = i.color;
                    col.a *= tex2D(_MainTex, i.texcoord).a;

                    clip(col.a - 0.01);

                    float sinValue = tan(_HighlightSectionOffset);

                    float mappedX = maprange(i.texcoord.x, 0, 1, -0.5, 0.5);
                    mappedX += 5*maprange(_HighlightSectionOffset, 0, 360, -0.5, 0.5);

                    mappedX = frac(mappedX);

                    if (mappedX * 2 < _HighlightSectionRadius)
                    {
                        col.r *= 2.0;
                        col.g *= 2.0;
                        col.b *= 2.0;
                    }
                    // if ( sinValue > 0.0 &&
                    //     (sinValue - (i.texcoord.x ) >= 0.0f)
                    //         &&
                    //     (sinValue - (i.texcoord.x ) <= 0.05))

                    //     {
                    //         col.r *= 2.0;
                    //         col.g *= 2.0;
                    //         col.b *= 2.0;
                    //     }

                    return col;
                }
                ENDCG
            }
        }
}