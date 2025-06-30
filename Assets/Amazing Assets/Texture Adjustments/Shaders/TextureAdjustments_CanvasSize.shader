Shader "Hidden/Amazing Assets/Texture Adjustments/Canvas Size"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float4 _TilingOffset;
	 
    float4 _BackgroundColor;
    int _FillType;  

    sampler2D _CustomTexture;
    float4 _CustomTexture_TilingOffset;
    float _Angle;



	float4 frag_adj (v2f_img i) : SV_Target     
	{           
        float2 uv = i.uv * _TilingOffset.xy + _TilingOffset.zw;
		float4 tex = tex2D(_MainTex, uv);

        float2 customUV = Rotate2x2(i.uv, _Angle, float2(0.5, 0.5)) * _CustomTexture_TilingOffset.xy + _CustomTexture_TilingOffset.zw;
        float4 customTex = tex2Dlod(_CustomTexture, float4(customUV, 0, 0));

        float4 res[4] = { tex, _BackgroundColor, customTex, customTex };

        float mask = 1;
        if(uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
		    mask = 0;
                
             
        return lerp(res[_FillType], tex, mask);
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
