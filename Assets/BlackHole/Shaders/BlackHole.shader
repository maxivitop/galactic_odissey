Shader "Hidden/BlackHole"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Skybox("SkyBox", Cube) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./Includes/Math.cginc"
            #include "./Includes/BlackHoleLogic.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 viewVector : TEXCOORD2;
                float centerDepth : TEXCOORD3;
            };

            sampler2D _MainTex;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            samplerCUBE _Skybox;
            half4 _Skybox_HDR;
            float3 _Position;
            float _SchwarzschildRadius;

            float3 _ShadowColor;

            float _StepSize;
            int _StepCount;
            float _GravitationalConst;
            float _Attenuation;

            float _MaxEffectRadius;
            float _EffectFadeOutDist;
            float _EffectFalloff;

            float LinearDepthToRawDepth(float linearDepth)
            {
                return (1.0f - (linearDepth * _ZBufferParams.y)) / (linearDepth * _ZBufferParams.x);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                float2 uv = v.uv * 2 - 1; // Re-maps the uv so that it is centered on the screen
                float3 viewVector = mul(unity_CameraInvProjection, float4(uv.x, uv.y, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));
                const float nearPlane = _ProjectionParams.y;
                const float farPlane = _ProjectionParams.z;
                
                o.centerDepth = (distance(_Position, _WorldSpaceCameraPos) - nearPlane) / (farPlane - nearPlane);
                o.centerDepth = LinearDepthToRawDepth(o.centerDepth);
                o.uv = v.uv;
                return o;
            }

            // Re-directs a view ray to move under the influence of gravity
            // Mimics the effect of gravitational lensing seen in warped spacetime
            void warpRay(inout float3 rayDir, float3 rayPos, float stepSize)
            {
                float3 centerDir = _Position - rayPos;
                float distance = length(centerDir);
                float sqrLength = pow(distance, _Attenuation);
                centerDir = normalize(centerDir);

                // Application of Newton's law of universal gravitation (modified variant)
                // Because our view ray has no mass, we assume m1 * m2 =~ 1
                float force = _GravitationalConst * (1 / sqrLength);

                float3 acceleration = rayDir + (centerDir * force * stepSize);
                rayDir = normalize(acceleration); // We only want velocity so normalize the acceleration
            }

             float3 RotateAroundYInDegrees(float3 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            float4 sampleSkybox(float3 dir) {
                half4 tex = texCUBE(_Skybox, normalize(dir));
                half3 c = DecodeHDR(tex, _Skybox_HDR);
                // c *= unity_ColorSpaceDouble.rgb;
                c *= 0.215; // _Exposure
                return half4(c, 1);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Determine the origin & direction of our ray to march through the scene
                float4 originalCol = tex2D(_MainTex, i.uv);
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);
                
                // Figure out where the effect bounds are
                float2 boundsHitInfo = raySphere(_Position, _MaxEffectRadius, rayOrigin, rayDir);
                float dstToBounds = boundsHitInfo.x;
                float dstThroughBounds = boundsHitInfo.y;
                float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float depth = LinearEyeDepth(nonLinearDepth) * length(i.viewVector);

                // If we are looking through the bounds render the black hole
                if (dstThroughBounds > 0 && distance(rayOrigin, _Position) < depth - 0.1) // 0.1 to not distort object at center
                {            
                    // Move the rayOrigin to the first point within the distortion bounds
                    rayOrigin += rayDir * dstToBounds;
                    float3 rayPos = rayOrigin;
                    float3 gasVolume = 0;
                    int shadowMask = 0;

                    // March our ray through the scene
                    for (float s = 0; s < _StepCount; s++)
                    {
                        sampleGasVolume(gasVolume, rayPos, rayDir, _Position, _MaxEffectRadius, _StepSize);
                        warpRay(rayDir, rayPos, _StepSize);
                        rayPos += rayDir * _StepSize;
                        // Ray should be terminated if it falls within the event horizon
                        float distFromCenter = distance(_Position, rayPos);
                        if (distFromCenter <= _SchwarzschildRadius) {
                            shadowMask = 1;
                            break;
                        }
                        // ...Likewise if the ray leaves the simulation bounds
                        if (distFromCenter > _MaxEffectRadius) {
                            break;
                        }
                    }
                    float3 finalCol;

                    // If the ray is absorbed by the event horizon, render the shadow
                    if (shadowMask){
                        finalCol = _ShadowColor;
                    }
                    else
                    {
                        // ...Otherwise, gravitationally lens the scene

                        // Convert the rayPos to a screen space position which can be used to read the screen UVs
                        float3 distortedRayDir = rayDir * length(i.viewVector);
                        if (dstThroughBounds <= _EffectFadeOutDist)
                        {
                            distortedRayDir =
                                i.viewVector + (distortedRayDir-i.viewVector) * pow(dstThroughBounds / _EffectFadeOutDist, _EffectFalloff);
                        }
                        float4 rayCameraSpace = mul(unity_WorldToCamera, float4(distortedRayDir, 0));
                        float4 rayUVProjection = mul(unity_CameraProjection, float4(rayCameraSpace));
                        float2 distortedScreenUV = float2(rayUVProjection.x / 2 + 0.5, rayUVProjection.y / 2 + 0.5);
                         if (distortedScreenUV.x > 1 ||
                            distortedScreenUV.y > 1 ||
                            distortedScreenUV.x < 0 ||
                            distortedScreenUV.y < 0) {
                            finalCol = sampleSkybox(RotateAroundYInDegrees(rayDir, 90));
                        } else {
                            float distortedNonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, distortedScreenUV);
                            if (distortedNonLinearDepth > i.centerDepth && distortedNonLinearDepth > 0) {
                                finalCol = sampleSkybox(RotateAroundYInDegrees(rayDir, 90));
                            } else {
                                finalCol = tex2D(_MainTex, distortedScreenUV);
                            }
                        }
                    }

                    // Incorperate the gas disc effect
                    finalCol += gasVolume;
                    // // Gravitational blue shifting
                    computeGravitationalShift(finalCol, _WorldSpaceCameraPos, _Position);
                    return float4(finalCol, 1);
                }
            
                // If we are not looking through the render bounds, just return the un-modified scene color
                computeGravitationalShift(originalCol.xyz, _WorldSpaceCameraPos, _Position);
                return originalCol;
            }
            ENDCG
        }
    }
}