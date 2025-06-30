Shader "Hidden/Amazing Assets/Texture Adjustments/Skew" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE 
	#include "Utilites.cginc"



	float2 _Amount;
	float2 _PivotPoint;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           		
		return tex2D(_MainTex, i.uv + (_Amount * _MainTex_TexelSize.xy * 2) * (i.uv.yx - _PivotPoint));
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