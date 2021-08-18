// This shader actually makes the object always facing the camera no matter how it rotates to the world
// We do not add another tranformation to the object to achieve such a behavior. Instead we efficiently
// remove a transformation process
Shader "ORamaVRCustomShaders/Billboard" {
	Properties{
		_MainTex("Texture Image", 2D) = "white" {}
	}
		SubShader{
			
		Pass{
			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag
         
			uniform sampler2D _MainTex;

			struct vertexInput {
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};

			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				// We keep the Projection matrix intact for obvious reasons
				// But we multuply ModelView with 0
				// and then at the end with add the gameobjects position in Object space
				output.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
								+ float4(input.vertex.x, input.vertex.y, 0.0, 0.0));

				output.tex = input.tex;

				return output;
			}

			float4 frag(vertexOutput i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.tex);

				return col;
			}

		ENDCG
		}
	}
}