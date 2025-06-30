Shader "Hidden/Amazing Assets/Texture Adjustments/Stroke"
{
    Properties
    {
		_Color("", Color) = (1, 1, 1, 1)
        _MainTex("", 2D) = "white" {}
    }

    CGINCLUDE
	#include "Utilites.cginc"



	int _Position;
	sampler2D _SrcTextrue;



	float4 frag4(v2f_img i) : SV_Target
	{
		float4 B = tex2D(_MainTex, i.uv + float2( 0, -1) * _MainTex_TexelSize.xy);
		float4 D = tex2D(_MainTex, i.uv + float2(-1,  0) * _MainTex_TexelSize.xy);
		float4 E = tex2D(_MainTex, i.uv + float2( 0,  0) * _MainTex_TexelSize.xy);
		float4 F = tex2D(_MainTex, i.uv + float2( 1,  0) * _MainTex_TexelSize.xy);
		float4 H = tex2D(_MainTex, i.uv + float2( 0,  1) * _MainTex_TexelSize.xy);

		float4 BH = H;
		if (B.a > H.a)
			BH = B;

		float4 FD = D;
		if (F.a > D.a)
			FD = F;

		float4 BHFD = FD;
		if (BH.a > FD.a)
			BHFD = BH;

		float4 final = E;
		if (E.a < BHFD.a)
			final = BHFD;


		return final;
	}

	float4 fragSobel (v2f_img i) : SV_Target     
	{     
		float s = Sobel(_MainTex, i.uv, _MainTex_TexelSize.xy).a;
		s = saturate(s);

		return s * s;
	}
	 
	float4 frag_adj (v2f_img i) : SV_Target     
	{     
		float4 edgeTex = tex2D(_MainTex, i.uv);
		float4 srcTex = tex2D(_SrcTextrue, i.uv);
		
		float p[3] = { edgeTex.a * (1 - srcTex.a), edgeTex.a * srcTex.a, edgeTex.a };

		return lerp(srcTex, _Color, p[_Position]);
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
            #pragma fragment fragSobel
			#pragma target 3.0

            ENDCG
        }

		Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag4
			#pragma target 3.0

            ENDCG
        }

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
