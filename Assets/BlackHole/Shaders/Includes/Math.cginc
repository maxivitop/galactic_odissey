static const float maxFloat = 3.402823466e+38;
static const float speedOfLight = 1;
static const float gravitationalConst = 1;
static const float PI = 3.14159265359;

// Returns dstToSphere, dstThroughSphere
// Implementation reference: jeancolasp @ scratchapixel & Sebastian Lague
// https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection.html
// https://www.youtube.com/watch?v=lctXaT9pxA0
float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir)
{
	float3 offset = rayOrigin - sphereCentre;
	float b = dot(offset, rayDir) * 2;
	float c = dot(offset, offset) - pow(sphereRadius, 2);
	float d = pow(b, 2) - 4 * c;

    // Intersected with sphere...
	if (d > 0)
	{
		d = sqrt(d);
		float distToSphere = max(0, (-b - d) / 2);
		float sphereFarDist = (d - b) / 2;
        float dstThroughSphere = sphereFarDist - distToSphere;

		if (sphereFarDist >= 0){
			return float2(distToSphere, dstThroughSphere);
		}
	}

	// No intersections...
	return float2(maxFloat, 0);
}

// Based upon http://www.vias.org/comp_geometry/math_coord_convert_3d.htm
float3 cartesianToRadial(float3 cartesian, float distFromCenter)
{
    float3 radialCoord;
    radialCoord.x = distFromCenter * 0.3f;
    radialCoord.y = atan2(-cartesian.x, -cartesian.y) * 1.5f;
    radialCoord.z = cartesian.z / distFromCenter;
    return radialCoord;
}