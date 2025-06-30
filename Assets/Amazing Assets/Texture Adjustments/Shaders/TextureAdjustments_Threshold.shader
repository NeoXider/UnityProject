Shader "Hidden/Amazing Assets/Texture Adjustments/Threshold"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float _Level;
    float _Noise; 
    float2 _ClampMinMax;
    sampler2D _NoiseTexture;


    
	float4 frag_adj (v2f_img i) : SV_Target     
	{       		
		float4 srcColor = tex2D(_MainTex, i.uv);		 

		float n = _Level + _Noise * (tex2D(_NoiseTexture, i.uv * NORMALIZE_NOISE_TEXTURE_UV).r - 0.5);

        float s = dot(srcColor.rgb, float3(0.222, 0.707, 0.071)) > n ? 1 : 0;
           

		return clamp(s, _ClampMinMax.x, _ClampMinMax.y);
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
