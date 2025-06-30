Shader "Hidden/Amazing Assets/Texture Adjustments/Brightness and Contrast" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	float _Brightness;
	float _Contrast;
	float4 _RGBACoef;	 



	float4 frag_adj (v2f_img i) : SV_Target     
	{      
		float4 srcColor = tex2D(_MainTex, i.uv);   	

		float4 retColor = (srcColor * ((_Brightness + 100.0f) * 0.01f) - _RGBACoef) * ((_Contrast + 100.0f) * 0.01f) + _RGBACoef;

		return saturate(retColor);
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