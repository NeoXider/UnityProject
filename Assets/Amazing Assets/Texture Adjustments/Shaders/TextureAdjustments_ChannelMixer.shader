Shader "Hidden/Amazing Assets/Texture Adjustments/Channel Mixer" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"

		 

	float3 _Red;
	float3 _Green;
	float3 _Blue;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           
		float4 c = tex2D(_MainTex, i.uv);   	
		
		float3 rgb = float3(dot(c.rgb, _Red), dot(c.rgb, _Green), dot(c.rgb, _Blue));

		return float4(rgb, c.a);
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