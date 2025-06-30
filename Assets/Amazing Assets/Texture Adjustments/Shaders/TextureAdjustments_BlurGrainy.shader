Shader "Hidden/Amazing Assets/Texture Adjustments/Blur Grainy"
{
    Properties
    {
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"


	
	float _Radius;
	int _Samples; 

	sampler2D _NoiseTexture;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           
		float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);
		float radius = tex2D(_NoiseTexture, i.uv * NORMALIZE_NOISE_TEXTURE_UV).r;
		float2 offset = float2(0.0, 0.0);
	
		for(int k = 0; k < _Samples; k++)
		{
			#if defined(SHADER_API_GLES)
				radius = frac(37.65 * radius + 0.614);
				offset = (radius - 0.5) * 2.0;
				radius = frac(37.65 * radius + 0.614);
			#else
				radius = frac(3712.65 * radius + 0.61432);
				offset = (radius - 0.5) * 2.0;
				radius = frac(3712.65 * radius + 0.61432);
			#endif

			offset.y = (radius - 0.5) * 2.0;

			finalColor += tex2Dlod(_MainTex, float4(i.uv + offset * _Radius, 0.0, 0.0));
		}


		return finalColor / _Samples;
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
