Shader "Hidden/Amazing Assets/Texture Adjustments/Mask" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "UnityCG.cginc" 
	#include "Utilites.cginc"
	 

		
	sampler2D _MaskTexure;
	float4 _MaskTexureTilingOffset;
	float _MaskInvert;
	float _MaskStrength; 
	sampler2D _AdjustedTexture;
	int _Channels;



	float ExtractMask(float2 uv)
	{	
		float4 t = tex2D(_MaskTexure, uv * _MaskTexureTilingOffset.xy + _MaskTexureTilingOffset.zw);
		
		float value[5] = {t.r, t.g, t.b, t.a, Luminance(t)};
		float mask = value[_Channels];
		
		mask = lerp(mask, 1 - mask, _MaskInvert);
		mask = saturate(mask + _MaskStrength);
		
		return mask;
	}
	 
	float4 frag_adj (v2f_img i) : SV_Target     
	{      		
		float4 srcColor = tex2D(_MainTex, i.uv);

				
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			i.uv.y = 1 - i.uv.y;	
		#endif

		
		return lerp(srcColor, tex2D(_AdjustedTexture, i.uv), ExtractMask(i.uv));
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