Shader "Hidden/Amazing Assets/Texture Adjustments/Checkerboard"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float2 _Count;
    float4 _Color2;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           		
        float checker = floor(i.uv.x * _Count.x * 2) + floor(i.uv.y * _Count.y * 2);
        checker = saturate(frac(checker * 0.5) * 2);

        return lerp(_Color, _Color2, checker);
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
