Shader "Hidden/Amazing Assets/Texture Adjustments/Color Overlay" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"

	 	 

	float4 frag_adj (v2f_img i) : SV_Target     
	{        							
		return _Color;
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