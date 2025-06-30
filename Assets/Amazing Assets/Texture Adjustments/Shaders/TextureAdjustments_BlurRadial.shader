Shader "Hidden/Amazing Assets/Texture Adjustments/Blur Radial"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float _Intensity;
    int _Samples;
    float2 _PivotPoint;

		

    float4 frag_adj (v2f_img i) : SV_Target     
	{   
        float2 uv = i.uv.xy - _PivotPoint;
        float4 color = float4(0, 0, 0, 0);

        float scale;           
        for(int i = 0; i < _Samples; i++)
        {
            scale = 1 + _Intensity * i;
            color += tex2D(_MainTex, uv * scale + _PivotPoint);
        }                
                
        color = saturate(color / _Samples);
        
        return  color;
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
