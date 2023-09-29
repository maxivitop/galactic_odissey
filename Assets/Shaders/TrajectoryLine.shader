Shader "Unlit/TrajectoryLine"
{
    SubShader
    {
        ZWrite On
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 vertexColor : TEXCOORD0;
                float2 screenPosition : TEXCOORD1;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = UnityObjectToViewPos(v.vertex);
                o.vertexColor = v.color;
                return o;
            }

            static const float4x4 bayer_matrix =
            {
                -0.5,    0,      -0.375,   0.125,
                0.25,   -0.25,    0.375,  -0.125,
                -0.3125, 0.1875, -0.4375,  0.0625,
                0.4375, -0.0625,  0.3125, -0.1875
            };

            fixed4 frag(v2f i) : SV_Target {
                int2 ditherCoordinate = i.vertex.xy  % 4;
                // return bayer_matrix[0][1] < 0;
                clip(bayer_matrix[ditherCoordinate.x][ditherCoordinate.y] - 0.5 + i.vertexColor.a);
                    
                
                return float4(i.vertexColor.xyz, 1);
            }
            ENDCG
        }
    }

    FallBack "Diffuse" // enables write to z buffer
}