Shader "Hidden/Amazing Assets/Texture Adjustments/Mipmap"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    sampler2D _MipmapTexture;
    float _MipmapLevel;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           
		return tex2Dlod(_MipmapTexture, float4(i.uv.xy, 0.0, _MipmapLevel));
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
