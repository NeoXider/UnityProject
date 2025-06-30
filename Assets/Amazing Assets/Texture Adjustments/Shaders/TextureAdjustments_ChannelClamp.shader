Shader "Hidden/Amazing Assets/Texture Adjustments/Channel Clamp" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	float4 _Min;
	float4 _Max;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           
		float4 c = tex2D(_MainTex, i.uv);   	

		return clamp(c, _Min, _Max);
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