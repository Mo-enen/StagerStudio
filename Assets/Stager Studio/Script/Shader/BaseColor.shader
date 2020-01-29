
Shader "Moenen/BaseColor" {


	Category{

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off
		 
		//BindChannels{
		//	Bind "Color", color
		//	Bind "Vertex", vertex
		//	Bind "TexCoord", texcoord
		//}

		SubShader{
			Pass{
		//SetTexture[_MainTex]{
		//	combine primary
		//}
	}
}

	}
}