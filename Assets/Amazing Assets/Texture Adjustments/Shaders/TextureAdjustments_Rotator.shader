Shader "Hidden/Amazing Assets/Texture Adjustments/Rotator" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"


		
	float _Angle;
	float2 _PivotPoint;

	 

	float4 frag_adj (v2f_img i) : SV_Target     
	{      
		float2 uv = Rotate2x2(i.uv, _Angle, _PivotPoint);

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