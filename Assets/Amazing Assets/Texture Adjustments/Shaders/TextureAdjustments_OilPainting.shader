//Kuwahara filter

Shader "Hidden/Amazing Assets/Texture Adjustments/Oil Painting"
{
    Properties
    {
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



	int _Radius;



	float4 frag_Kuwahara (v2f_img i) : SV_Target     
	{           
        float2 uv = i.uv;
        
        float2 start[4] = {{-_Radius, -_Radius}, {-_Radius, 0}, {0, -_Radius}, {0, 0}};

        float4 _MatrixM[4] = 
        {
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0}
        };
 
        float4 _MatrixS[4] = 
        {
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0}
        };   



        #if defined(SHADER_API_GLES)
            _Radius = clamp(_Radius, 0, 4);
        #endif
                
 
        float2 pos;
        float4 col;
        for (int k = 0; k < 4; k++) 
        {
            for(int i = 0; i <= _Radius; i++) 
            {
                for(int j = 0; j <= _Radius; j++) 
                {
                    pos = float2(i, j) + start[k];
                    col = tex2Dlod(_MainTex, float4(uv.x + pos.x * _MainTex_TexelSize.x, uv.y + pos.y * _MainTex_TexelSize.y, 0, 0));
                    
                    _MatrixM[k] += col;
                    _MatrixS[k] += col * col;
                    
                }
            }
        }

 
        float sigma2;
 
        float n = pow(_Radius + 1, 2);
        float4 color = tex2D(_MainTex, uv);
        float min = 1;
  
        for (int l = 0; l < 4; l++) 
        {
            _MatrixM[l] /= n;
            _MatrixS[l] = abs(_MatrixS[l] / n - _MatrixM[l] * _MatrixM[l]);
            sigma2 = _MatrixS[l].r + _MatrixS[l].g + _MatrixS[l].b;
 
            if (sigma2 < min) 
            {
                min = sigma2;
                color = _MatrixM[l];
            }
        }
        
        return color;
	}     


    ENDCG

    Subshader
    {
        Tags { "DisableBatching"="True" }

		ZTest Always Cull Off ZWrite Off
	    Fog { Mode off } 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_Kuwahara
			#pragma target 3.0

            ENDCG
        }
    }
}
