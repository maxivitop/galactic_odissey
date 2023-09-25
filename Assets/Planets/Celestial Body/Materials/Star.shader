Shader "Celestial/Star"
{
	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque"}
		ZWrite On
		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 col : TEXCOORD0;
				float brightnessFalloff : TEXCOORD1;

			};

			sampler2D _MainTex;
			sampler2D _Spectrum;


			v2f vert (appdata v)
			{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
				
					float4 starCol = tex2Dlod(_Spectrum, float4(v.uv.y, 0.5, 0, 0));
					o.col = starCol;
					o.brightnessFalloff = v.uv.x;
		
					return o;
			}

			float4 frag (v2f i) : SV_Target
			{	
				float b = i.brightnessFalloff;
				b = saturate (b+0.1);
				b*=b;
				
				return float4(i.col.rgb * b, 1);
			}

			ENDCG
		}
	}
}
