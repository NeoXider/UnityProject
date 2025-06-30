Shader "Hidden/Amazing Assets/Texture Adjustments/Rotate" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"


	
	float _Angle;



	float4 frag_adj (v2f_img i) : SV_Target     
	{      
		return tex2D(_MainTex, Rotate2x2(i.uv, _Angle, float2(0.5, 0.5)));
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