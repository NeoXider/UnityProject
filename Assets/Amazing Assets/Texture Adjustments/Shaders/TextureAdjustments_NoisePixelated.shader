Shader "Hidden/Amazing Assets/Texture Adjustments/Noise Pixelated"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float2 _Scale;
    float2 _Offset;
    float _SquareAspectRation;

    int _ColorMode;
    float4 _Color1;
    float4 _Color2;
    sampler2D _GradientTexture;

    sampler2D _NoiseTexture;



	float4 frag_adj (v2f_img i) : SV_Target     
	{         
        #if defined(RENDER_EMPTY_TEXTURE)

            return tex2D(_MainTex, i.uv);

        #else

            float aspectRatio = _MainTex_TexelSize.w / _MainTex_TexelSize.z;
            float2 newUV = _MainTex_TexelSize.z > _MainTex_TexelSize.w ? float2(1.0, aspectRatio) : float2(1.0 / aspectRatio, 1.0);
	        i.uv *= lerp(float2(1, 1), newUV, _SquareAspectRation);

     
            float2 uv = floor(i.uv * _Scale + _Offset) / _Scale;
            float r = tex2Dlod(_NoiseTexture, float4(uv * NORMALIZE_NOISE_TEXTURE_UV, 0, 0)).r;

            float4 colors[3] = {r.xxxx, lerp(_Color1, _Color2, r), tex2D(_GradientTexture, float2(r, 0.5))};


            return colors[_ColorMode];

        #endif
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

            #pragma multi_compile_local _ RENDER_EMPTY_TEXTURE

            ENDCG
        }
    }
}
