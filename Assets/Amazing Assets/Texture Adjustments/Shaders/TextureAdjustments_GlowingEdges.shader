Shader "Hidden/Amazing Assets/Texture Adjustments/Glowing Edges"
{
    Properties
    {
		_Color("", Color) = (1, 1, 1, 1)
        _MainTex("", 2D) = "white" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



	float _Strength;
    float _Saturation;
    float _Lightness;

    

    inline float3 SL(float3 _srcColor)
	{
		float S = _Saturation <= 0.0f ? (_Saturation / 100.0f + 1.0f) : (_Saturation / 50.0f + 1.0f);
		float L = _Lightness * 0.01f;


		float4 ret = float4(0, 0, 0, 0);
		ret.r = (0.299f + 0.701f * S) * _srcColor.r +
				(0.587f - 0.587f * S) * _srcColor.g +
				(0.114f - 0.114f * S) * _srcColor.b;
		ret.g = (0.299f - 0.299f * S) * _srcColor.r +
				(0.587f + 0.413f * S) * _srcColor.g +
				(0.114f - 0.114f * S) * _srcColor.b;
		ret.b = (0.299f - 0.3f * S) * _srcColor.r +
				(0.587f - 0.588f * S) * _srcColor.g +
				(0.114f + 0.886f * S) * _srcColor.b;
				 

		//Lightness
		ret = lerp(ret * (1 + L), (1.0 - ret) * L + ret, step(0, L));
				
		return ret;
	}

	float4 frag_adj (v2f_img i) : SV_Target     
	{     
		float4 s = Sobel(_MainTex, i.uv, _MainTex_TexelSize.xy);

		s.rgb = SL(s);
		s.rgb = pow(s.rgb, _Strength);

		return s;
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
