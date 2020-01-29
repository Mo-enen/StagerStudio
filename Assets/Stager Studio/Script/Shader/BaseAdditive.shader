
Shader "Moenen/BaseAdditive" {

	
	Category{

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off //Fog{ Color(0,0,0,0) }

		//BindChannels{
			//Bind "Color", color
			//Bind "Vertex", vertex
			//Bind "TexCoord", texcoord
		//}

		SubShader{
			Pass{
				//SetTexture[_MaainTex]{
				//	combine primary
				//}
			}
		}

	}
}