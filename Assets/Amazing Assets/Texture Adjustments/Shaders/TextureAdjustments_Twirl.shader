Shader "Hidden/Amazing Assets/Texture Adjustments/Twirl" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	float _Strength;
	float2 _PivotPoint;
	 


	float4 frag_adj (v2f_img i) : SV_Target     
	{          		
		float2 delta = i.uv - _PivotPoint;
		float angle = _Strength * length(delta) * 0.0174533;
		float x = cos(angle) * delta.x - sin(angle) * delta.y;
		float y = sin(angle) * delta.x + cos(angle) * delta.y;
		float2 uv = float2(x + _PivotPoint.x, y + _PivotPoint.y);
				

		return tex2D(_MainTex, uv);
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