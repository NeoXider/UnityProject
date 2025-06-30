Shader "Hidden/Amazing Assets/Texture Adjustments/Blur Dithering"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    int _Samples;
    float2 _Strength;

		

    float4 frag_adj (v2f_img i) : SV_Target     
	{       
        float2 dist = _Strength / _MainTex_TexelSize.zw;
        float2 p0 = i.uv - 0.5 * dist;
        float2 p1 = i.uv + 0.5 * dist;

        float2 s = (p1 - p0) / (_Samples - 1.0);
        float2 ij = floor(fmod(i.uv * _MainTex_TexelSize.zw, 2.0));
        float idx = ij.x + ij.y;
        float4 m = step(abs(idx - float4(0, 1.0, 2.0, 3.0)), 0.5) * float4(0.75, 0.25, 0, 0.5);
        float d = m.x + m.y + m.z + m.w;
        float2 p = p0 + d * s;

        float4 sum = tex2D( _MainTex, p);

        for(int i = 1; i < _Samples; ++i)
        {
            p += s;
            sum += tex2D( _MainTex, p);
        }
        sum = saturate(sum / _Samples);

        return sum;
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
