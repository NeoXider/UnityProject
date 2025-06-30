Shader "Hidden/Amazing Assets/Texture Adjustments/Gradient Ramp" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"

		

	sampler2D _GradientTexture;
	float _Offset;
		
	 

	float4 frag_adj (v2f_img i) : SV_Target     
	{   
		float4 srcColor = tex2D(_MainTex, i.uv);  

		#if defined(RENDER_EMPTY_TEXTURE)

			return srcColor;

		#else
 
			float2 uv = float2 (Luminance(srcColor) + _Offset, 0.5);

			return tex2D(_GradientTexture, uv);

		#endif
	}     

	ENDCG 
	
	Subshader 
	{
	    ZTest Always Cull Off ZWrite Off
	    Fog { Mode off } 
		
		Pass 
		{    
		    CGPROGRAM
		    #pragma vertex vert_img
		    #pragma fragment frag_adj

			#pragma multi_compile_local _ RENDER_EMPTY_TEXTURE 

		    ENDCG
		}  
	}
}