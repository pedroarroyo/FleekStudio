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

Shader "XZIMG/VideoShaderHairDying" {
	Properties {
		_MainTex1("Image", 2D) = "white" {}
		_MainTex2("hairs", 2D) = "white"	{}
		_Color("Color", Color) = (0.2, 0.2, 1.0, 0.35)
	}
	SubShader {
		Tags{ "Queue" = "Background" }		
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
			fixed4 _Color;
			int _ActivateSegmentation = 0;
            int _InvertTextureChannels = 0;
            
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
			sampler2D _MainTex1;
			sampler2D _MainTex2;

			fixed4 frag(v2f i) : SV_Target
			{
				// sample texture and return it
				fixed4 col1 = tex2D(_MainTex1, i.uv);
                if (_InvertTextureChannels == 1)	// This is for ios video capture format
                    col1 = fixed4(col1.b, col1.g, col1.r, col1.a);
				if (_ActivateSegmentation)
				{
					fixed4 col2 = tex2D(_MainTex2, i.uv);
					float mask = col2.a * _Color.a;
					fixed4 col = col1*(1.0f - mask) + _Color*mask;
					return col;
				}
				else return col1;
			}
			ENDCG
		}
	} 
}

