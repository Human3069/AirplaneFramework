﻿Shader "Scope Pro/Red Dot Effect" {
	Properties {
		_Color ("Glass Color", Color) = (1,1,1,1)
		_RedDotColor ("Emission Color", Color) = (1,1,1,1)
		_RedDotTex ("Red Dot Texture (A)", 2D) = "white" {}
		_RedDotSize ("Size", Range(0,10)) = 0.0
		[Toggle(FIXED_SIZE)] _FixedSize ("Use Fixed Size", Float) = 0
		_RedDotDist ("Distance Offset", Range(0,50)) = 2.0
		_OffsetX ("Horizontal Offset", Float) = 0.0
		_OffsetY ("Vertical Offset", Float) = 0.0

		_Distance ("# Distance", Range(0,50)) = 2.0
		_Scale ("# Scale", Range(0,50)) = 2.0
	}
 
	SubShader {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
       
       
        Pass {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                       
                #include "UnityCG.cginc"
 
                struct appdata_t {
                     float4 vertex : POSITION;
					 float2 uv : TEXCOORD0;
                };
 
                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
					float2 RedDotUV : TEXCOORD1;
                };
 
				fixed4 _Color;
				sampler2D _RedDotTex;
				fixed4 _RedDotColor;
				fixed _RedDotSize;
				fixed _RedDotDist;
				fixed _OffsetX;
				fixed _OffsetY;
                       
                v2f vert (appdata_t v) {
					v2f o = (v2f)0;
                    o.vertex = UnityObjectToClipPos(v.vertex);
					fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					fixed3 viewDir = _WorldSpaceCameraPos - worldPos;
			
					#if defined(FIXED_SIZE)
						fixed3 objectCenter = mul(unity_ObjectToWorld, fixed4(0,0,0,1));
						fixed dist = length(objectCenter - _WorldSpaceCameraPos);
						_RedDotSize *= dist;
					#endif

					o.RedDotUV = v.vertex.xy - fixed2(0, _OffsetY);
					o.RedDotUV -= mul(unity_WorldToObject, viewDir).xy * _RedDotDist;
					o.RedDotUV /= _RedDotSize;
					o.RedDotUV += fixed2(0.5, 0.5);
                    return o;

					/*
					float3 lens_origin = UnityObjectToviewPos(float3(0,0,0));
					float p0 = UnityObjectToviewPos(float3(0,0, _Distance));
					float3 n = UnityObjectToviewPos(float3(0,0,1)) - lens_origin;
					float3 uDir = UnityObjectToviewPos(float3(1,0,0)) - lens_origin;
					float3 vDir = UnityObjectToviewPos(float3(0,1,0)) - lens_origin;
					float vert = UnityObjectToviewPos(v.vertex);

					float a = dot(p0, n) / dot(vert, n);
					float3 vert_prime = a * vert;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = float2(dot(vert_prime - p0, uDir), dot(vert_prime - p0, vDir));
					o.uv = o.uv / (_Scale * _Distance);
					o.uv += float2(0.5, 0.5);
					UNITY_TRANSFER_FOG(o, o.vertex);

					return o;
					*/
                }
                       
                fixed4 frag (v2f i): COLOR {
                    fixed4 col = _Color;
					fixed redDot = tex2D (_RedDotTex, i.RedDotUV).a;
					col.rgb += redDot * _RedDotColor.rgb * _RedDotColor.a;
					col.a += redDot * _RedDotColor.a;
                    return col;
                }
            ENDCG
		}
	}
 }