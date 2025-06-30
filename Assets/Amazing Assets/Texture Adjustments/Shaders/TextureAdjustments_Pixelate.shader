Shader "Hidden/Amazing Assets/Texture Adjustments/Pixelate"
{
    Properties
    {
		_Color("", Color) = (1, 1, 1, 1)
        _MainTex("", 2D) = "white" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"


	
	float2 _Count;
    float _Shape;



	float4 frag_adj (v2f_img i) : SV_Target     
	{     
        float2 uv = 0;

        if(_Shape > 0.5)
        {
            float2 uv1 = floor(i.uv * _Count) / _Count;
            float2 uv2 = (i.uv - uv1) * _Count;

            uv = uv1 + float2(step(1.0 - uv2.y, uv2.x) / (2 * _Count.x), step(uv2.x, uv2.y) / (2 * _Count.y));
        }
        else
        {
            float2 res = 1.0 / _Count.xy;

	        uv = floor(i.uv / res) * res;
        }
		

        return tex2Dlod(_MainTex, float4(uv, 0, 0));
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
