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

Shader "XZIMG/VideoShader-contours" {
	Properties {
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags{ "Queue" = "Background" }
		LOD 200		
		Pass { 

			Lighting Off
			ZWrite Off
			ZTest Off
			Cull Off

			CGPROGRAM
			// use "vert" function as the vertex shader
			#pragma vertex vert
			// use "frag" function as the pixel (fragment) shader
			#pragma fragment frag

			// vertex shader inputs
			struct appdata
			{
				float4 vertex : POSITION; // vertex position
				float2 uv : TEXCOORD0; // texture coordinate
			};

			// vertex shader outputs ("vertex to fragment")
			struct v2f
			{
				float2 uv : TEXCOORD0; // texture coordinate
				float4 vertex : SV_POSITION; // clip space position
			};
			int _Rotation = 0;
			float _ScaleX = 1.0;
			float _ScaleY = 1.0;
			int _Mirror = 0;
			int _VerticalMirror = 0;

			// vertex shader
			v2f vert(appdata v)
			{
				v2f o;

				// transform position to clip space
				o.vertex = float4(v.vertex.x*_ScaleX, v.vertex.y*_ScaleY, 0.0, 1.0);

				if (_Rotation == 1)
				{
					float tmp = o.vertex.x;
					o.vertex.x = o.vertex.y;
					o.vertex.y = -tmp;
				}
				else if (_Rotation == 2)
				{
					o.vertex.x = -o.vertex.x;
					o.vertex.y = -o.vertex.y;
				}
				else if (_Rotation == 3)
				{
					float tmp = o.vertex.x;
					o.vertex.x = -o.vertex.y;
					o.vertex.y = tmp;
				}

				// pass the texture coordinate
				o.uv = v.uv;
				if (_Mirror == 1)
					o.uv.x = 1.0 - o.uv.x;
				if (_VerticalMirror == 1)
					o.uv.y = 1.0 - o.uv.y;		// image is flipped upside down (depending on pixel formats and devices)

				return o;
			}

			// texture we will sample
			sampler2D _MainTex;

			fixed4 frag(v2f input) : SV_Target
			{
				// first gaussian smooth
				//declare stuff
				const int mSize = 11;
				const int kSize = (mSize - 1) / 2;
				float kernel[mSize];
				float3 final_colour = float3(0.0, 0.0, 0.0);

				//create the 1-D kernel
				float sigma = 3.0;
				float Z = 0.0;
				for (int j = 0; j <= kSize; ++j)
				{
					kernel[kSize + j] = kernel[kSize - j] = 0.39894*exp(-0.5*float(j)*float(j) / (sigma*sigma)) / sigma;
				}

				//get the normalization factor (as the gaussian has been clamped)
				for (int j = 0; j < mSize; ++j)
				{
					Z += kernel[j];
				}

				//read out the texels
				for (int i = -kSize; i <= kSize; ++i)
				{
					for (int j = -kSize; j <= kSize; ++j)
					{
						float uvx = input.uv.x + (float)i / (float)_ScreenParams.xy.x;
						float uvy = input.uv.y + (float)j / (float)_ScreenParams.xy.y;

						final_colour += kernel[kSize + j] * kernel[kSize + i] * tex2D(_MainTex, float2(uvx, uvy)).xyz;

					}
				}
				float4 o = float4(final_colour / (Z*Z), 1.0);

				#define EPS 2.e-3  //3
				float2 uvx = input.uv + float2(EPS,0.);
				float2 uvy = input.uv + float2(0.,EPS);

				float2 ref = float2(.5,.5);
				float3 col0 = tex2D(_MainTex, ref).xyz;
				float lum0 = (col0.x + col0.y + col0.z) / 3.;
				
				float3 tex,texx,texy;
				float2 grad; float g = 1.;

				for (int k = 0; k<10; k++)
				{
					//tex = tex2D(_MainTex, input.uv).xyz;
					tex = o.xyz;

					texx = tex2D(_MainTex, uvx).xyz;
					texy = tex2D(_MainTex, uvy).xyz;
					grad = abs(float2(texx.x - tex.x, texy.x - tex.x));
					//		if (i==0) g = dot(grad,grad);

					input.uv += EPS*grad;
					uvx.x += EPS*grad.x;
					uvy.y += EPS*grad.y;
				}

				float3 col = o;
				float lum = (col.x + col.y + col.z) / 3.;
				g = 4.*dot(grad,grad);
				g = pow(max(0.,1. - g), 30.);
				g = clamp(g, 0., 1.);
				col = g * col / pow(lum, 0.55);

				return float4(col, 1.0);
			}
			


			ENDCG
		}
	} 
}

