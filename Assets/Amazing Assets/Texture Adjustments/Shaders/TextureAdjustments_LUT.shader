Shader "Hidden/Amazing Assets/Texture Adjustments/LUT"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    sampler3D _LUTTexure;
    int _FlipVertical;
    int _ColorSpace;



	float4 frag_adj (v2f_img i) : SV_Target     
	{     
        #if defined(READ_LUT_3D_TEXTURE)

            float4 c = tex2D(_MainTex, i.uv);


		    #ifdef UNITY_COLORSPACE_GAMMA			
			    c.rgb = tex3D(_LUTTexure, c.rgb).rgb;
		    #else
			    c.rgb = tex3D(_LUTTexure, sqrt(c.rgb)).rgb;
			    c.rgb = c.rgb * c.rgb;			
		    #endif

            return c; 

        #elif defined(RENDER_EMPTY_TEXTURE)

            return tex2D(_MainTex, i.uv);

        #else   //BUILD_LUT_3D_TEXTURE

            i.uv.y *= lerp(1, -1, _FlipVertical);

            float4 c = tex2D(_MainTex, i.uv);


            float4 result[3] = 
            {
                c,
                float4(LinearToGammaSpace(c.rgb), c.a),
                float4(GammaToLinearSpace(c.rgb), c.a)
            };
            

            return result[_ColorSpace];

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
            #pragma vertex vert_img
            #pragma fragment frag_adj
			#pragma target 3.0

            #pragma multi_compile_local BUILD_LUT_3D_TEXTURE RENDER_EMPTY_TEXTURE READ_LUT_3D_TEXTURE

            ENDCG
        }
    }
}
