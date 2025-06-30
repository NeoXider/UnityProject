Shader "Hidden/Amazing Assets/Texture Adjustments/Levels" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	float4 _InputMin; 
	float4 _InputMax;
	float4 _InputGamma;
	float4 _OutputMin;
	float4 _OutputMax;

	 

	float4 frag_adj (v2f_img i) : SV_Target     
	{           
		float4 srcColor = (tex2D(_MainTex, i.uv));   	

		float4 a = max(srcColor - _InputMin, 0) / max(0.001, _InputMax - _InputMin); 
	
		float4 p = pow(min(a, 1), 1.0 / _InputGamma); 
	

		return lerp(_OutputMin, _OutputMax, p);
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