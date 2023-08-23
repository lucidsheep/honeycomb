// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/GradientMask"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_GradientTex("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_FillColor ("FillColor", Color) = (1,1,1,1)
		_BlankColor ("BlankColor", Color) = (0,0,0,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.screenPos = ComputeScreenPos(OUT.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _GradientTex;
			fixed4 _FillColor;
			fixed4 _BlankColor;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

//				if(color.a < 0.4) {
//					color.rgb = 0.0;
//					color.a = 0.0;
//				}

				return color;
			}

			float SampleAlphaTexture(float2 uv)
			{
				fixed4 color = tex2D (_GradientTex, uv);
				
				return color.r;
			}
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord);
				float g = SampleAlphaTexture(IN.texcoord);
				fixed4 f;
				f.a = ((_FillColor.a * g) + (_BlankColor.a * (1 - g))) * IN.color.a;
				f.r = ((_FillColor.r * g) + (_BlankColor.r * (1 - g))) * f.a;
				f.g = ((_FillColor.g * g) + (_BlankColor.g * (1 - g))) * f.a;
				f.b = ((_FillColor.b * g) + (_BlankColor.b * (1 - g))) * f.a;

				//f.r = r.r * f.a;
				//f.g = r.g * f.a;
				//f.b = r.b * f.a;
				return f;
			}
		ENDCG
		}
	}
}
