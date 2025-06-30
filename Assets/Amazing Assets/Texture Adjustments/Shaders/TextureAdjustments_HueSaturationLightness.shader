Shader "Hidden/Amazing Assets/Texture Adjustments/Hue, Saturation and Lightness" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"		 


	
	float _Hue;
	float _Saturation;
	float _Lightness;
	 
	float4 _KeyColor;
	float _Threshold;
	float _Boost;

	float _Angle;
	float _RangeMin;
	float _RangeMax;



	inline float4 HSL(float4 _srcColor)
	{
		float H = _Hue * -0.0174532;
		float S = _Saturation <= 0.0f ? (_Saturation / 100.0f + 1.0f) : (_Saturation / 50.0f + 1.0f);
		float L = _Lightness * 0.01f;

		float VSU = S * cos(H);
		float VSW = S * sin(H);

		float4 ret = float4(0, 0, 0, 0);
		ret.r = (0.299f + 0.701f * VSU + 0.168f * VSW) * _srcColor.r +
				(0.587f - 0.587f * VSU + 0.330f * VSW) * _srcColor.g +
				(0.114f - 0.114f * VSU - 0.497f * VSW) * _srcColor.b;
		ret.g = (0.299f - 0.299f * VSU - 0.328f * VSW) * _srcColor.r +
				(0.587f + 0.413f * VSU + 0.035f * VSW) * _srcColor.g +
				(0.114f - 0.114f * VSU + 0.292f * VSW) * _srcColor.b;
		ret.b = (0.299f - 0.3f * VSU + 1.25f * VSW) * _srcColor.r +
				(0.587f - 0.588f * VSU - 1.05f * VSW) * _srcColor.g +
				(0.114f + 0.886f * VSU - 0.203f * VSW) * _srcColor.b;
				 

		//Lightness
		ret = lerp(ret * (1 + L), (1.0 - ret) * L + ret, step(0, L));
		

		ret.a = _srcColor.a;

		return ret;
	}

	inline float4 SelectiveByColor(float4 _srcColor, float4 hsl)
	{	
		float diff = saturate (_Threshold * length (_srcColor.rgb - _KeyColor.rgb));
	
		return lerp (hsl, _srcColor, pow(diff, _Boost));
	} 

	inline float4 SelectiveByRange(float4 _srcColor, float4 hsl)
	{	
		//hue range value
		float h = RGBtoHUE(_srcColor.rgb);
	 
		if (_RangeMax > 1.0 && h < _RangeMax - 1.0) h += 1.0;
		if (_RangeMin < 0.0 && h > _RangeMin + 1.0) h -= 1.0;
		

		float2 smoothStep = smoothstep(float2(_Angle, _RangeMin), float2(_RangeMax, _Angle), h);

		_srcColor.rgb = lerp(lerp(hsl.rgb, _srcColor.rgb, smoothStep.x),
							 lerp(_srcColor.rgb, hsl.rgb, smoothStep.y), 
							 step(h, _Angle)); 

		return _srcColor; 
	}
	

	float4 frag_adj (v2f_img i) : SV_Target     
	{          
		float4 mainColor = tex2D(_MainTex, i.uv);
		float4 hsl = HSL(mainColor);


		#if defined(SELECTION_BY_COLOR)

			return SelectiveByColor(mainColor, hsl);

		#elif defined(SELECTION_BY_RANGE)

			return SelectiveByRange(mainColor, hsl);

		#else

			return hsl;

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
			#pragma target 3.0

			#pragma multi_compile_local _ SELECTION_BY_COLOR SELECTION_BY_RANGE

		    ENDCG
		}   
	}
}