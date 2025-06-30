Shader "Hidden/Amazing Assets/Texture Adjustments/Edge Padding"
{
	Properties
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex ("", 2D) = "" {}	
	}

	CGINCLUDE
	#include "Utilites.cginc"



	sampler2D _AlphaSource;

	

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
		if (BHFD.a > E.a)
			final = BHFD;


		return final;
	}

	float4 frag8(v2f_img i) : SV_Target
	{
		float4 B = tex2D(_MainTex, i.uv + float2( 0, -1) * _MainTex_TexelSize.xy);
		float4 D = tex2D(_MainTex, i.uv + float2(-1,  0) * _MainTex_TexelSize.xy);
		float4 E = tex2D(_MainTex, i.uv + float2( 0,  0) * _MainTex_TexelSize.xy);
		float4 F = tex2D(_MainTex, i.uv + float2( 1,  0) * _MainTex_TexelSize.xy);
		float4 H = tex2D(_MainTex, i.uv + float2( 0,  1) * _MainTex_TexelSize.xy);

		float4 I = tex2D(_MainTex, i.uv + float2(-1,  1) * _MainTex_TexelSize.xy);
		float4 J = tex2D(_MainTex, i.uv + float2( 1,  1) * _MainTex_TexelSize.xy);
		float4 K = tex2D(_MainTex, i.uv + float2(-1, -1) * _MainTex_TexelSize.xy);
		float4 L = tex2D(_MainTex, i.uv + float2( 1, -1) * _MainTex_TexelSize.xy);

		float4 BH = H;
		if (B.a > H.a)
			BH = B;

		float4 FD = D;
		if (F.a > D.a)
			FD = F;

		float4 BHFD = FD;
		if (BH.a > FD.a)
			BHFD = BH;


		float4 IL = L;
		if (I.a > L.a)
			IL = I;

		float4 JK = K;
		if (J.a > K.a)
			JK = J;

		float4 ILJK = JK;
		if (IL.a > JK.a)
			ILJK = IL;


		float4 T = BHFD;
		if (ILJK.a > BHFD.a)
			T = ILJK;


		float4 final = E;
		if (T.a > E.a)
			final = T;

		
		return final;
	}

	float4 frag_CopyAlpha(v2f_img i) : SV_Target
	{
		return float4(tex2D(_MainTex, i.uv).rgb, tex2D(_AlphaSource, i.uv).a);
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
            #pragma fragment frag4

            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag8

            ENDCG
        }
			 
		Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_CopyAlpha

            ENDCG
        }
    }
}
