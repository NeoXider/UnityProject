Shader "Hidden/Amazing Assets/Texture Adjustments/Noise Gradient"
{
    Properties
    {
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



	int _Octaves;
	float2 _Scale;
	float _Power;
	float _Offset;
	float2 _Scroll;

	float _AspectRatio;
	float _IsFixed;

	float _UseFixedSteps;
	float _FixedSteps;
	float4 _Color2;
	sampler2D _GradientTexture;

	int _ColorMode;



	float2 GradientNoiseDir(float2 p)
	{
		#if defined(SHADER_API_GLES)
			p = p % 28;
			float x = (34 * p.x + 1) * p.x % 28 + p.y;
			x = (34 * x + 1) * x % 28;
		#else
			p = p % 289;
			float x = (34 * p.x + 1) * p.x % 289 + p.y;
			x = (34 * x + 1) * x % 289;
		#endif

		x = frac(x / 41) * 2 - 1;
		return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
	}

	float Noise(float2 p)
	{
		float2 ip = floor(p);
		float2 fp = frac(p);
		float d00 = dot(GradientNoiseDir(ip), fp);
		float d01 = dot(GradientNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
		float d10 = dot(GradientNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
		float d11 = dot(GradientNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));
		fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
		return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
	}
	
	float4 frag_adj (v2f_img i) : SV_Target     
	{           		
		#if defined(RENDER_EMPTY_TEXTURE)

			return tex2D(_MainTex, i.uv);
			
		#else

			float o = 0.5;
			float s = 1.0;
			float w = 0.5;

			//AspectRatio
			float aspectRatio = _MainTex_TexelSize.w / _MainTex_TexelSize.z;
			i.uv *= _MainTex_TexelSize.z > _MainTex_TexelSize.w ? float2(1.0, aspectRatio) : float2(1.0 / aspectRatio, 1.0);

			for (int k = 0; k < _Octaves; k++)
			{           
				o += Noise(i.uv * _Scale * s + _Scroll) * w * 2;
           
				s *= 2.0;
				w *= 0.5;
			}


			o = saturate(o);
			o = saturate(o + _Offset);
			o = pow(o, _Power);			
				

			if(_IsFixed > 0.5)	
			{
				o = ((o - 0.5) * 2) < (_Offset * 0.999) ? 0 : 1;			
			}
				        
			float4 colors[3] = {o.xxxx, lerp(_Color2, _Color, o), tex2Dlod(_GradientTexture, float4(lerp(o, floor(o * _FixedSteps) / _FixedSteps, _UseFixedSteps), 0.5, 0, 0))};
		
		
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
