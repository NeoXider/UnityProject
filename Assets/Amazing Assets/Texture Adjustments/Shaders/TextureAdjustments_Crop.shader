Shader "Hidden/Amazing Assets/Texture Adjustments/Crop" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	float4 _TilingOffset;
	float _Threshold;
	int _ChannelID;
	

	
	float AreColorsClose(float3 _c1, float3 _c2, float _threshold)
    {
		float r = _c1.r - _c2.r,
              g = _c1.g - _c2.g,
              b = _c1.b - _c2.b;

        return (r * r + g * g + b * b) <= (_threshold * _threshold) ? 1 : 0;
    }

	float4 frag (v2f_img i) : SV_Target     
	{           		
		#if defined(CROP_BY_COLOR)

			return AreColorsClose(tex2D(_MainTex, i.uv).rgb, _Color.rgb, _Threshold);

		#elif defined(CROP_BY_CHANNEL)

			return tex2D(_MainTex, i.uv)[_ChannelID] >= _Threshold;

		#else

			return tex2D(_MainTex, i.uv * _TilingOffset.xy + _TilingOffset.zw);

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
		    #pragma fragment frag

			#pragma multi_compile_local _ CROP_BY_COLOR CROP_BY_CHANNEL

		    ENDCG
		}  
	}
}