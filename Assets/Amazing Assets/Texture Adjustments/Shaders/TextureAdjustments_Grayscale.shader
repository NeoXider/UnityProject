Shader "Hidden/Amazing Assets/Texture Adjustments/Grayscale"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"


	
    float3 _Luminance;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           
		float4 srcColor = tex2D(_MainTex, i.uv);
		float lum = dot(srcColor.rgb, _Luminance);
		
		return float4(lum, lum, lum, srcColor.a);
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
