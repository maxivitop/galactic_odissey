Shader "Skybox/CustomQuad"
{
    Properties
    {
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Cubemap("Cubemap", Cube) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }

        Pass
        {
            Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            half _Exposure;
            float _Rotation;
            samplerCUBE _Cubemap;
            half4 _Cubemap_HDR;

            float3 RotateAroundYInDegrees(float3 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct appdata_t {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 dir = worldPos - _WorldSpaceCameraPos;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.dir = RotateAroundYInDegrees(dir, _Rotation);// = mul(unity_WorldToCamera, float4(dir, 0));
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                half4 tex = texCUBE(_Cubemap, i.dir);
                half3 c = DecodeHDR(tex, _Cubemap_HDR);
                // c *= unity_ColorSpaceDouble.rgb;
                c *= _Exposure;
                return half4(c, 1);
            }
            ENDCG
        }
    }
}