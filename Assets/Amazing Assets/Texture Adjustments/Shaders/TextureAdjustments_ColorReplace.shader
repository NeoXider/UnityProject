Shader "Hidden/Amazing Assets/Texture Adjustments/Color Replace" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"
		 


	float4 _KeyColor; 
	float _Threshold;
	float _Boost;
		
	float _Angle;
	float _RangeMin;
	float _RangeMax; 

	float4 _TargetColor; 

 
	 
	inline float4 ReplaceColorByColor(float4 _srcColor)
	{
		float diff = saturate (_Threshold * length (_srcColor.rgb - _KeyColor.rgb));
	
		float4 c = lerp (_TargetColor, _srcColor, pow(diff, _Boost));
		c.a = _srcColor.a;

		return c;
	} 

	inline float4 ReplaceColorByRange(float4 _srcColor)
	{
		//hue range value
		float h = RGBtoHUE(_srcColor.rgb);
	
		if (_RangeMax > 1.0 && h < _RangeMax - 1.0) h += 1.0;
		if (_RangeMin < 0.0 && h > _RangeMin + 1.0) h -= 1.0;

		float2 smoothStep = smoothstep(float2(_Angle, _RangeMin), float2(_RangeMax, _Angle), h);
						
		float4 c = 0;
		c.rgb = lerp(lerp(_TargetColor.rgb, _srcColor.rgb, smoothStep.x),
					 lerp(_srcColor.rgb, _TargetColor.rgb, smoothStep.y),
					 step(h, _Angle)); 
		c.a = _srcColor.a;

		return c;
	}

	float4 frag_adj (v2f_img i) : SV_Target     
	{   
		float4 mainTex = tex2D(_MainTex, i.uv);

		#if defined(REPLACE_BY_RANGE)
			return ReplaceColorByRange(mainTex);
		#else
			return ReplaceColorByColor(mainTex);
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

			#pragma multi_compile_local _ REPLACE_BY_RANGE

		    ENDCG
		}  
	}	
}