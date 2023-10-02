Shader "Unlit/TrajectoryLine"
{
    Properties 
    {
        
    }

    SubShader 
    {
        Tags {"Queue"="Geometry+101" "RenderType"="Transparent"}
        Cull Off 
        LOD 100

        Pass 
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t 
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
            };

            struct v2f 
            {
                float4 vertex  : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return i.color;
            }

            ENDCG
        }

        // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}