Shader "Hidden/Amazing Assets/Texture Adjustments/Blur Directional"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"


    	
    float _Samples;
    float2 _Direction; 
 
  

	float4 frag_adj (v2f_img i) : SV_Target     
	{         
		float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);

        
		for (int j = -_Samples; j < _Samples; j++)
			finalColor += tex2Dlod(_MainTex, float4(i.uv - _Direction * j, 0.0, 0.0));

		
        return finalColor / (_Samples * 2.0);
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
