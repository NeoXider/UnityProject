Shader "Hidden/Amazing Assets/Texture Adjustments/Blur Gaussian"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"


		
    float4 GaussianBlur(float2 uv, float2 coeff)
    {
        float4 s = tex2D(_MainTex, uv) * 0.2271;

        float2 offset = coeff * 1.3846;
        s += (tex2D(_MainTex, uv + offset) + tex2D(_MainTex, uv - offset)) * 0.3162;

        offset = coeff * 3.2308;
        s += (tex2D(_MainTex, uv + offset) + tex2D(_MainTex, uv - offset)) * 0.0703;

        return s;
    }
	    

    float4 frag_h(v2f_img i) : SV_Target
    { 
        return GaussianBlur(i.uv, float2(_MainTex_TexelSize.x, 0));
    }

    float4 frag_v(v2f_img i) : SV_Target
    {
        return GaussianBlur(i.uv, float2(0, _MainTex_TexelSize.y));
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
            #pragma fragment frag_h

            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_v			

            ENDCG
        }
    }
}
