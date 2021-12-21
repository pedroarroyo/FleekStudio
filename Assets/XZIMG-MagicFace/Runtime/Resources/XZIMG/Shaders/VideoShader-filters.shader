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

Shader "XZIMG/VideoShaderBF" {
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

#define SIGMA 10.0
#define BSIGMA 0.1
#define MSIZE 15

			const float4x4 kernel = float4x4(
				0.031225216, 0.033322271, 0.035206333, 0.036826804, 0.038138565,
				0.039104044, 0.039695028, 0.039894000, 0.039695028, 0.039104044,
				0.038138565, 0.036826804, 0.035206333, 0.033322271, 0.031225216, 0.0);

			float sigmoid(float a, float f) {
				return 1.0 / (1.0 + exp(-f * a));
			}

			fixed4 frag(v2f input) : SV_Target
			{

				// first gaussian smooth
				//declare stuff
				const int mSize = 11;
				const int kSize = (mSize - 1) / 2;
				float kernel[mSize];
				float3 final_colour = float3(0.0, 0.0, 0.0);

				//create the 1-D kernel
				float sigma = 5.0;
				float Z = 0.0;
				for (int j = 0; j <= kSize; ++j)
				{
					//kernel[kSize + j] = kernel[kSize - j] = normpdf(float(j), sigma);
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
						float uvx = input.uv.x +(float)i / (float)_ScreenParams.xy.x;
						float uvy = input.uv.y +(float)j / (float)_ScreenParams.xy.y;

						final_colour += kernel[kSize + j] * kernel[kSize + i] * tex2D(_MainTex, float2(uvx, uvy)).xyz;

					}
				}
				float4 o = float4(final_colour / (Z*Z), 1.0);


				//
				// now bilateral fiter
				float edgeStrength = length(fwidth(/*tex2D(_MainTex, input.uv)*/o));
				edgeStrength = sigmoid(edgeStrength - 0.05, 100.0)/* - 0.2*/;
				float3 f = float3(1.0 - edgeStrength, 1.0 - edgeStrength, 1.0 -  edgeStrength);
				float4 c = o;// tex2D(_MainTex, input.uv);
				float cx = floor(c.x *3.) / 3.;
				float cy = floor(c.y *3.) / 3.;
				float cz = floor(c.z *3.) / 3.;
				f = (float3(cx, cy, cz)*0.5 + f*0.5);
				return float4(f, 1.0);



			}
				
			ENDCG
		}
	} 
}

