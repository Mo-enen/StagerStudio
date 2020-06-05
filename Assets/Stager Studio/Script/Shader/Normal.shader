Shader "Object/Normal" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_ZoneMinMax("MinMax", Vector) = (0,0,3000,2000)
	}

		SubShader{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

			ZWrite Off
			Cull Back 
			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float4 color: Color;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 texcoord : TEXCOORD0;
					float4 color: Color;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _ZoneMinMax;

				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.color = v.color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					

				// Color
				fixed4 col = tex2D(_MainTex, i.texcoord);
				col *= i.color;

				// Mask
				if (
					i.vertex.x < _ZoneMinMax.x ||
					i.vertex.y < _ZoneMinMax.y ||
					i.vertex.x > _ZoneMinMax.z ||
					i.vertex.y > _ZoneMinMax.w
				) {
					col.a *= 0.15f;
				}

				return col;
			}
		ENDCG
	}
		}

}
