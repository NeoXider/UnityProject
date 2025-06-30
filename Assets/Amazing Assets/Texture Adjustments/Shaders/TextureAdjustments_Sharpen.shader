Shader "Hidden/Amazing Assets/Texture Adjustments/Sharpen"
{
    Properties
    {   
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float _Strength;



	float4 frag_adj (v2f_img i) : SV_Target     
	{   
		float4 blur =  tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * 1.5);
			   blur += tex2D(_MainTex, i.uv + float2( _MainTex_TexelSize.x, -_MainTex_TexelSize.y) * 1.5);
			   blur += tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x,  _MainTex_TexelSize.y) * 1.5);
			   blur += tex2D(_MainTex, i.uv + float2( _MainTex_TexelSize.x,  _MainTex_TexelSize.y) * 1.5);
			   blur *= 0.25;

		float4 srcColor = tex2D(_MainTex, i.uv);


		return saturate(srcColor + (srcColor - blur) * _Strength);
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
