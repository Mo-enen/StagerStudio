Shader "Moenen/Background" {
	Properties{
		[HideInInspector] _MainTex("Base (RGB)", 2D) = "white" {}
		 _Tint("_Tint", Float) = 0
	}

		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma target 2.0
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float4 color : COLOR;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f {
						float4 color : COLOR;
						float4 vertex : SV_POSITION;
						float2 texcoord : TEXCOORD0;
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;
					float _Tint;

					v2f vert(appdata_t v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.color = v.color;
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{
						fixed4 col = lerp(tex2D(_MainTex, i.texcoord) , i.color, _Tint);
						return col;
					}
				ENDCG
			}
		 }

}
