Shader "Hidden/Amazing Assets/Texture Adjustments/Color Space" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"


		 
	int _ColorSpace;



	float4 frag_adj (v2f_img i) : SV_Target     
	{       
		float4 c = tex2D(_MainTex, i.uv);

		c.rgb = lerp(GammaToLinearSpace(c.rgb), LinearToGammaSpace(c.rgb), _ColorSpace);

		return c;
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

		    ENDCG
		}  
	}
}