Shader "Custom/GaussBlur" {

    Properties {
		 [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TextureSize ("_TextureSize",Float) = 512
        _BlurRadius ("_BlurRadius",Range(1,48) ) = 16
    }

    SubShader {

		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		


		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			sampler2D _MainTex;
			int _BlurRadius;
			float _TextureSize;




			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};



			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			
			v2f vert( appdata_t v ) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.color = v.color;

				return o;
			} 
       

		   float GetGaussianDistribution( float x, float y, float rho ) {
				float g = 1.0f / sqrt( 2.0f * 3.141592654f * rho * rho );
				return g * exp( -(x * x + y * y) / (2 * rho * rho) );
			}




			float4 GetGaussBlurColor( float2 uv ) {

				
				float space = 1.0/_TextureSize; 
				float rho = (float)_BlurRadius * space / 3.0;

				float weightTotal = 0;
				for( int x1 = -_BlurRadius ; x1 <= _BlurRadius ; x1++ ){
					for( int y1 = -_BlurRadius ; y1 <= _BlurRadius ; y1++ ){
						weightTotal += GetGaussianDistribution(x1 * space, y1 * space, rho );
					}
				}

				float4 colorTmp = float4(0,0,0,0);
				for( int x = -_BlurRadius ; x <= _BlurRadius ; x++ ){
					for( int y = -_BlurRadius ; y <= _BlurRadius ; y++ ) {
						float weight = GetGaussianDistribution( x * space, y * space, rho )/weightTotal;
						float4 color = tex2D(_MainTex,uv + float2(x * space,y * space));
						color = color * weight;
						colorTmp += color;
					}
				}

				return colorTmp;

			}




			half4 frag(v2f i) : SV_Target {
				return GetGaussBlurColor(i.uv) * i.color;
			}




        ENDCG


        }


    }
    FallBack "Diffuse"
}