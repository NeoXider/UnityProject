Shader "Hidden/Amazing Assets/Texture Adjustments/Grid"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"

    	

    float2 _Count;
    float2 _Size;
    float  _GridWidth;
    float  _Opacity;

    float4 _BackgroundColor;



	float4 frag_adj (v2f_img i) : SV_Target     
	{     
		float2 uv = frac(i.uv * _Count.xy);
		float2 gSize = min(1, _GridWidth.xx / _Size * _Count.xy);
		float grid = 0;

		if(gSize.x > uv.x || (1 - gSize.x) < (uv.x) || gSize.y > uv.y || (1 - gSize.y) < (uv.y))
			grid = _Opacity;


		return lerp(_BackgroundColor, _Color, grid);
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

            ENDCG
        }
    }
}
