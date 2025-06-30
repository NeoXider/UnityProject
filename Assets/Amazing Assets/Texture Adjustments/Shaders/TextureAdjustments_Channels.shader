Shader "Hidden/Amazing Assets/Texture Adjustments/Channels" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "UnityCG.cginc" 
	#include "Utilites.cginc"
	 


	sampler2D _AdjustedTexture;
	float4 _Channels;



	float4 frag_adj (v2f_img i) : SV_Target     
	{    
		float4 srcColor = tex2D(_MainTex, i.uv);
		
		        
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			i.uv.y = 1 - i.uv.y;	
		#endif


		return lerp(srcColor, tex2D(_AdjustedTexture, i.uv), _Channels);
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