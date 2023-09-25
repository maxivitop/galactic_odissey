// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Star"
{
	Properties
	{
		_Angle("Angle", Range(0.0, 360.0)) = 0.0
	}
	
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque"}
        Cull Back
        
        GrabPass
	    {
	        "_BackgroundTexture"
	    }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define TAU 6.28318530718
            
			struct appdata
			{
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

            struct v2f 
			{
            	float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 vertex : TEXCOORD6;
                float3 worldPos : TEXCOORD1;
                float3 viewVector : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
            	float4 grabPos : TEXCOORD4;
            	float4 centeredPos : TEXCOORD5;
            	float3 centerViewDir : TEXCOORD7;
            };

            sampler2D _BackgroundTexture;
            float _Angle;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

            	o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                float2 uv = v.uv * 2 - 1; // Re-maps the uv so that it is centered on the screen
                float3 viewVector = mul(unity_CameraInvProjection, float4(uv.x, uv.y, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));
                o.grabPos = ComputeGrabScreenPos(o.pos);
            	o.centeredPos = o.pos - UnityObjectToClipPos(float4(0, 0, 0, 0));
            	o.centerViewDir = -normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz));
				o.vertex = v.vertex; 
                o.uv = v.uv;
                return o;
            }
        
            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldViewDir = -normalize(UnityWorldSpaceViewDir(i.worldPos)); //Direction of ray from the camera towards the object surface
               
            	float2 uvCentered = (i.uv - 0.5)*2;
            	float r = length(uvCentered);
                // if (r < 0.25) {
	               //  return 0;
                // }
            	float distortionAngle = (1-r) * sin(r*TAU*2);
                if (r > 1) {
	                distortionAngle = 0;
                }
            	float3 towardsCenter = float3(-normalize(uvCentered), 0);
            	float3 perpToViewDirAndInPlaneWithTowardsCenter =
            		normalize(cross(cross(worldViewDir, i.centerViewDir), worldViewDir));

            	float3 testRot = normalize(cross(worldViewDir, float3(0, 1, 0)));
            	// return float4(testRot, 0);
            	float angleRad = _Angle / 360.0 * TAU;
            	angleRad *= distortionAngle;
				
            	return float4(UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, perpToViewDirAndInPlaneWithTowardsCenter));
            	return i.centeredPos;
            	return float4(worldViewDir, 0);

            	half3 reflection = -worldViewDir; //reflect(-worldViewDir, i.worldNormal); // Direction of ray after hitting the surface of object
                half4 reflectionProbeColor = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflection);
            	float3 grab = i.grabPos / i.grabPos.w;
            	half4 bgColor = tex2D(_BackgroundTexture, grab);
                return reflectionProbeColor;
            }
            ENDCG
        }
    }
}
