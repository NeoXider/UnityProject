Shader "Hidden/Amazing Assets/Texture Adjustments/Channel Swap" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	int _Red; 
	int _Green;
	int _Blue;
	int _Alpha;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           
		float4 srcColor = (tex2D(_MainTex, i.uv));   	

		float data[10] = 
		{
			srcColor.r,
			1 - srcColor.r,

			srcColor.g,
			1 - srcColor.g,

			srcColor.b,
			1 - srcColor.b,

			srcColor.a,
			1 - srcColor.a,

			1, 
			0
		};

		return float4(data[_Red], data[_Green], data[_Blue], data[_Alpha]);
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