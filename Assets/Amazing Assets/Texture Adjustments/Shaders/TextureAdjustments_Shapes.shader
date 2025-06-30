Shader "Hidden/Amazing Assets/Texture Adjustments/Shapes"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



    float2 _Count;
    float2 _WidthHeight;
    float  _Sides;
    float  _Angle;
    float  _Thickness;

    float4 _ShapeColor;
    float4 _BackgroundColor;



	float Shape(float2 uv, float sides, float2 res)
    {		
        uv = (uv * 2 - 1) / (res * cos(PI / sides));
        uv.y *= -1;

        float pCoord = atan2(uv.x, uv.y);
        float r = PI_2 / sides;
        float distance = cos(floor(0.5 + pCoord / r) * r - pCoord) * length(uv);
        
		return saturate((1 - distance) / fwidth(distance));
    }

		

	float4 frag_adj (v2f_img i) : SV_Target     
	{     
		float2 uv = frac(i.uv * _Count);


		uv = Rotate2x2(uv, _Angle, float2(0.5, 0.5));

		float outRect  = Shape(uv, _Sides, _WidthHeight);
		float innerRect = Shape(uv, _Sides, _WidthHeight * (1 - _Thickness));


		return lerp(_BackgroundColor, _ShapeColor, outRect * (1 - innerRect));
	}      

 //   float4 frag_adj (v2f_img i) : SV_Target     
	//{     
	//	float2 uv = frac(i.uv * _Count);


	//	uv = Rotate2x2(Rotate2x2(uv, _Angle, float2(0.5, 0.5)), _Angle, float2(0.5, 0.5));

	//	float outRect  = Shape(uv, _Sides, _WidthHeight);
	//	float innerRect = Shape(uv, _Sides, _WidthHeight * (1 - _Thickness));

 //       float a1  = Shape(uv, _Sides - 1, _WidthHeight * 0.1);
	//	float a2 = Shape(uv, _Sides + 2, _WidthHeight * (1 - _Thickness * 0.1));

 //       float a3  = Shape(uv, _Sides - 2, _WidthHeight * 0.2);
	//	float a4 = Shape(uv, _Sides + 3, _WidthHeight * (1 - _Thickness * 0.2));

 //       float a5  = Shape(uv, _Sides - 5, _WidthHeight * 0.5);
	//	float a6 = Shape(uv, _Sides + 6, _WidthHeight * (1 - _Thickness * 0.6));

 //       float a7  = Shape(uv, _Sides - 7, _WidthHeight * 0.7);
	//	float a8 = Shape(uv, _Sides + 8, _WidthHeight * (1 - _Thickness * 0.8));

 //       float coef1 = a1 + a2 + a3 + a4 * a5 - a6 + a7 / a8;
 //       float coef2 = a1 - a2 / a3 + a4 - a5 * a6 - lerp(a7, a8, a7 * a1);

	//	return lerp(_BackgroundColor, _ShapeColor, outRect *  coef1 * (1 - innerRect - coef2));
	//}      


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
