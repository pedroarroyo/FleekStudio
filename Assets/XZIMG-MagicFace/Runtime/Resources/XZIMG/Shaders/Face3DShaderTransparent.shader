/**
*
* Copyright (c) 2021 XZIMG Limited , All Rights Reserved
* No part of this software and related documentation may be used, copied,
* modified, distributed and transmitted, in any form or by any means,
* without the prior written permission of XZIMG Limited
*
* contact@xzimg.com, www.xzimg.com
*
*/

Shader "XZIMG/Face3DShaderTransparent" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,0.5)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 1.0
		_Metallic("Metallic", Range(0,1)) = 0.1
	}
	
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		CGPROGRAM

#pragma surface surf Standard fullforwardshadows alpha:fade
#pragma target 3.0

		sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o)
	{
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb;
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Standard"
}