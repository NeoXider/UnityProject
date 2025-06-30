Shader "Hidden/Amazing Assets/Texture Adjustments/Noise Grainy"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float _Monochromatic;
    sampler2D _NoiseTexture;


    
	float4 frag_adj (v2f_img i) : SV_Target     
	{       
        float4 c = tex2D(_NoiseTexture, i.uv * NORMALIZE_NOISE_TEXTURE_UV);        
        
        return _Monochromatic > 0.5 ? c.rrrr : c;
	}     

    ENDCG

    Subshader
    {
        ZTest Always Cull Off ZWrite Off
	    Fog { Mode off } 

        Pass
        {
            CGPROGRAM
			#pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_adj

            ENDCG
        }
    }
}
