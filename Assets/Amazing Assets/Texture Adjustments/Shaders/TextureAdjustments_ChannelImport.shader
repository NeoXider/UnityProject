Shader "Hidden/Amazing Assets/Texture Adjustments/Channel Import" 
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "Utilites.cginc"



	sampler2D _ImportedTexture;
	float4 _ImportedTexture_ST;

	int _Red;  
	int _Green;
	int _Blue;
	int _Alpha;

	float4 _ChannelsState;	
	 


	float4 frag_adj (v2f_img i) : SV_Target     
	{   
		float4 sourceColor = tex2D(_MainTex, i.uv);  

		float4 importedTexture = tex2D(_ImportedTexture, i.uv * _ImportedTexture_ST.xy + _ImportedTexture_ST.zw) * _Color;   	 	

		float data[10] = 
		{
			importedTexture.r,
			1 - importedTexture.r,

			importedTexture.g,
			1 - importedTexture.g,

			importedTexture.b,
			1 - importedTexture.b,

			importedTexture.a,
			1 - importedTexture.a,

			1, 
			0
		};

		importedTexture = float4(data[_Red], data[_Green], data[_Blue], data[_Alpha]);


		return lerp(sourceColor, importedTexture, _ChannelsState);
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
