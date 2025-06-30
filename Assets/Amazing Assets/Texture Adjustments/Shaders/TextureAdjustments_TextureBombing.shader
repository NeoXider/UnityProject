Shader "Hidden/Amazing Assets/Texture Adjustments/Texture Bombing"
{
    Properties
    {
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



	sampler2D _BombTexture;
	float4 _BombTexture_TexelSize;


	#if defined(RENDER_BOMBING)
		
		float _IsFlipBook;
		float2 _FlipBook_ColumnsRows;
		
		float _RandomRotate;
		float _RandomHue;
		float _RandomColor;
		float _RandomAlpha;

		float _SaveAlpha;

		float _ScriptTextureScale;
		float2 _ScriptTextureOffset;

		float _ScriptRotationAngle;
		float _ScriptRandomFlipBook;
		float _ScriptRandomHue;
		float4 _ScriptRandomColor;

	#endif


	float2 FlipBook(float2 uv, float tile, float2 widthHeight)
	{
		float res = widthHeight.x * widthHeight.y;
		tile = tile - res * floor(tile/res);	

	    float2 tileCount = float2(1.0, 1.0) / widthHeight;
	    float tileY = abs(widthHeight.y - (floor(tile * tileCount.x) + 1));
		float tileX = abs(((tile - widthHeight.x * floor(tile * tileCount.x))));
		return (uv + float2(tileX, tileY)) * tileCount;
	}

	float4 frag_adj (v2f_img i) : SV_Target     
	{      
		float4 c = float4(0, 0, 0, 0);
		

		#if defined(RENDER_BOMBING)

			c = tex2D(_MainTex, i.uv);		
		
			float aspectRatio = _MainTex_TexelSize.w / _MainTex_TexelSize.z;
			i.uv *= _MainTex_TexelSize.z > _MainTex_TexelSize.w ? float2(1.0, aspectRatio) : float2(1.0 / aspectRatio, 1.0);


			//ScaleOffset
			float2 uv = i.uv * _ScriptTextureScale - _ScriptTextureOffset;

 
			//Rotate
			float rotateAngle = _ScriptRotationAngle * 360;
			float2 rotUV = Rotate2x2(uv, rotateAngle, float2(0.5, 0.5));					
			uv = lerp(uv, rotUV, _RandomRotate);


			//Correct aspect ratio for bomb texture
			float bombTextureAspectRatio = _BombTexture_TexelSize.z / _BombTexture_TexelSize.w;
			uv.y *= bombTextureAspectRatio;



			//Saturate
			uv = saturate(uv);

					
			//FlipBook
			float randomFlipBookIndex = floor(lerp(0, _FlipBook_ColumnsRows.x * _FlipBook_ColumnsRows.y, _ScriptRandomFlipBook));
			float2 tileUV = FlipBook(uv, randomFlipBookIndex, _FlipBook_ColumnsRows);			
			uv = lerp(uv, tileUV, _IsFlipBook);

 

			//Read Texture
			float4 bomb = tex2D(_BombTexture, uv);


			//Hue
			float3 hue = RGBtoHSV(bomb);
			hue.x = lerp(hue.x - _RandomHue, hue.x + _RandomHue, _ScriptRandomHue);			
			bomb.rgb = HSVToRGB(hue); 


			//Color and Alpha
			float4 color = _ScriptRandomColor;
			color.rgb = lerp(float3(1, 1, 1), color.rgb, _RandomColor);
			color.a = lerp(1, color.a, _RandomAlpha);
			bomb *= color; 


			c.rgb = lerp(c.rgb, bomb.rgb, bomb.a);
			c.a += bomb.a * _SaveAlpha;

		#else

			c = tex2D(_BombTexture, i.uv);

		#endif
 
		return saturate(c);
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

			#pragma multi_compile_local _ RENDER_EMPTY_TEXTURE RENDER_BOMBING

            ENDCG
        }
    }
}
