Shader "Hidden/Amazing Assets/Texture Adjustments/Blend" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	sampler2D _OverlayTexure;
	float _Invert;
	float _Opacity; 
 	 


	float4 frag_adj (v2f_img i) : SV_Target     
	{        
		float4 srcColor = saturate(tex2D(_MainTex, i.uv));   		
		float4 finalColor = srcColor;


		#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				i.uv.y = 1 - i.uv.y;	
		#endif


		float4 overlayColor = saturate(tex2D(_OverlayTexure, i.uv));
		overlayColor = lerp(overlayColor, 1 - overlayColor, _Invert);
		


		#if defined(BLEND_DARKEN)
			finalColor = min(overlayColor, srcColor);
		#elif defined(BLEND_MULTIPLY)
			finalColor = overlayColor * srcColor;
		#elif defined(BLEND_COLORBURN)
			finalColor = 1 - (1 - srcColor) / overlayColor;
		#elif defined(BLEND_LINEARBURN)
			finalColor = overlayColor + srcColor - 1;
		#elif defined(BLEND_LIGHTEN)
			finalColor = max(overlayColor, srcColor);
		#elif defined(BLEND_SCREEN)
			finalColor = 1 - (1 - overlayColor) * (1 - srcColor);
		#elif defined(BLEND_COLORDODGE)
			finalColor = srcColor / (1 - overlayColor);
		#elif defined(BLEND_LINEARDODGE)
			finalColor = overlayColor + srcColor;    
		#elif defined(BLEND_OVERLAY)
			finalColor = lerp((1 - (1 - 2 * (srcColor - 0.5)) * (1 - overlayColor)), (2 * srcColor * overlayColor), step(srcColor, 0.5));
		#elif defined(BLEND_HARDLIGHT)
			finalColor = lerp((1 - (1 - 2 * (overlayColor - 0.5)) * (1 - srcColor)), (2 * overlayColor * srcColor), step(overlayColor, 0.5));
		#elif defined(BLEND_VIVIDLIGHT)
			finalColor = lerp(srcColor / ((1 - overlayColor) * 2), 1 - ((1 - srcColor) * 0.5) / overlayColor, step(overlayColor, 0.5));
		#elif defined(BLEND_LINEARLIGHT)
			finalColor = lerp((srcColor + 2 * overlayColor - 1), (srcColor + 2 * (overlayColor - 0.5)), step(overlayColor, 0.5));
		#elif defined(BLEND_PINLIGHT)
			finalColor = lerp(max(srcColor, 2 * (overlayColor - 0.5)), min(srcColor, 2 * overlayColor), step(overlayColor, 0.5));
		#elif defined(BLEND_HARDMIX)
			finalColor = round(0.5 * (overlayColor + srcColor));
		#elif defined(BLEND_DIFFERENCE)
			finalColor = abs(overlayColor - srcColor);
		#elif defined(BLEND_EXCLUSION)
			finalColor = 0.5 - 2 * (overlayColor - 0.5) * (srcColor - 0.5);
		#elif defined(BLEND_SUBTRACT)
			finalColor = srcColor - overlayColor; 
		#elif defined(BLEND_DIVIDE)  
			finalColor = srcColor / overlayColor; 
    
 
		#elif defined(BLEND_HUE) 
			float3 srcColorHSL = RGBToHSL(srcColor);
		    finalColor.rgb = HSLToRGB(float3(RGBToHSL(overlayColor).r, srcColorHSL.g, srcColorHSL.b));
			
		#elif defined(BLEND_SATURATION) 
			float3 srcColorHSL = RGBToHSL(srcColor);
			finalColor.rgb = HSLToRGB(float3(srcColorHSL.r, RGBToHSL(overlayColor).g, srcColorHSL.b));

		#elif defined(BLEND_COLOR)
			float3 overlayColorHSL = RGBToHSL(overlayColor);
			finalColor.rgb = HSLToRGB(float3(overlayColorHSL.r, overlayColorHSL.g, RGBToHSL(srcColor).b));
			
		#elif defined(BLEND_LUMINOSITY)
			float3 baseHSL = RGBToHSL(srcColor);
			finalColor.rgb = HSLToRGB(float3(baseHSL.r, baseHSL.g, RGBToHSL(overlayColor).b));

		#elif defined(BLEND_WATERMARK)
			finalColor.rgb = lerp(finalColor.rgb, overlayColor.rgb, overlayColor.a);

		#else
			
			//Normal
			finalColor = overlayColor;			
		#endif

							
		return lerp(saturate(srcColor), saturate(finalColor), _Opacity);
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


			#pragma multi_compile_local BLEND_NORMAL BLEND_DARKEN BLEND_MULTIPLY BLEND_COLORBURN BLEND_LINEARBURN BLEND_LIGHTEN BLEND_SCREEN BLEND_COLORDODGE BLEND_LINEARDODGE BLEND_OVERLAY BLEND_HARDLIGHT BLEND_VIVIDLIGHT BLEND_LINEARLIGHT BLEND_PINLIGHT BLEND_HARDMIX BLEND_DIFFERENCE BLEND_EXCLUSION BLEND_SUBTRACT BLEND_DIVIDE BLEND_HUE BLEND_SATURATION BLEND_COLOR BLEND_LUMINOSITY BLEND_WATERMARK


		    ENDCG
		}  
	}
}