Shader "Hidden/Amazing Assets/Texture Adjustments/Gradient" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"


	
	sampler2D _GradientTexture;

	float _Reverse;
	float _Frequency;
	float _Angle;
	int _GradientTypeID;
	 
		

	float4 frag_adj (v2f_img i) : SV_Target     
	{    
		#if defined(RENDER_EMPTY_TEXTURE)

			return tex2D(_MainTex, i.uv);

		#else

			float2 uvRot = Rotate2x2(i.uv, _Angle, float2(0.5, 0.5));
			float2 remap = Remap(uvRot, 0, 1, -1, 1);
			float2 sinUvRot = sin(uvRot * 3.1415926);

			float xValues[4] = {0, 0, 0, 0};

			xValues[0] = uvRot.x;
			xValues[1] = length(remap);
			xValues[2] = 1 - min(sinUvRot.x, sinUvRot.y);
			xValues[3] = atan2(remap.r, remap.g) / 6.2831853 + 0.5;
		

			float uvX = xValues[_GradientTypeID];


			uvX = uvX * _Frequency;
			uvX = lerp(1 - uvX, uvX, _Reverse);


			return tex2D(_GradientTexture, float2(uvX, 0.5));

		#endif
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

			#pragma multi_compile_local _ RENDER_EMPTY_TEXTURE 

		    ENDCG
		}  
	}
}