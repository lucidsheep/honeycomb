// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/LinearFill"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_GradientTex ("Gradient", 2D) = "white" {}
		_Fill("Fill", Float) = 0
		_Color ("Tint", Color) = (1,1,1,1)
		_Slanted("Slanted", Float) = 0
		[MaterialToggle] _Inverted("Inverted", Float) = 0
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
			sampler2D _AlphaTex;
			sampler2D _GradientTex;
			float _Fill;
			float _Inverted;
			float _Slanted;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				return color;
			}

			float SampleAlphaTexture(float2 uv)
			{
				fixed4 color = tex2D (_GradientTex, uv);
				if(_Inverted > 0)
				{
					if(uv.x < (1 - _Fill - (uv.y * _Slanted))) return 0.0;
					return 1.0;
				}

				if(uv.x - (uv.y * _Slanted)> _Fill) return 0.0;
				return 1.0;
			}
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord);
				float g = SampleAlphaTexture(IN.texcoord);
				fixed4 r = IN.color;
				fixed4 f;
				f.a = c.a * g;
				f.r = c.r * r.r * c.a * f.a;
				f.g = c.g * r.g * c.a * f.a;
				f.b = c.b * r.b * c.a * f.a;
				return f;
			}
		ENDCG
		}
	}
}
