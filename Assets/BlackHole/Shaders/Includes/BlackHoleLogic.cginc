int _AccretionQuality;
float3 _AccretionMainColor;
float3 _AccretionInnerColor;
float _AccretionColorShift;
float _AccretionFalloff;
float _AccretionOuterRadius;
float _AccretionInnerRadius;
float _AccretionWidth;
float3 _AccretionDir;

sampler3D _AccretionNoiseTex;

int _NoiseLayerCount;
float _SampleScales[4];
float _ScrollRates[4];

float _GasCloudThreshold;
float _TransmittancePower;

float _BlueShiftPower;

// Samples from a 3D noise texture with a layered approach of varying scale and scroll rates
float sampleNoiseTexture(float3 position)
{
    float value = 0;
    for (int n = 0; n < _NoiseLayerCount; n++)
    {
        float3 offsetCoord = position;
        offsetCoord.y += _Time.x * _ScrollRates[n];
        float noiseValue = tex3Dlod(_AccretionNoiseTex, float4(offsetCoord / _SampleScales[n] / PI * 2, 0.0f));
        value += max(0, noiseValue - _GasCloudThreshold) * _TransmittancePower;
    }
    return value;
}

// Accumulates the approximate density of gas at a given point
// Result is color graded for an adjustable outcome
void sampleGasVolume(inout float3 color, float3 position, float3 rayDir, float3 volCenter, float volRadius, float stepSize)
{
    // If accretion disc disabled...
    if (_AccretionQuality == -1){
        return;
    }

    float distFromCenter = distance(position, volCenter);
    if (distFromCenter < _AccretionInnerRadius) {
        return;
    }
    // Full quality volumetrics
    float scale = _AccretionWidth;
    float3 scaledPos = position - volCenter;
    scaledPos.xy *= scale;
    scaledPos += volCenter;
    float3 scaledDir = rayDir * stepSize;
    scaledDir.xy *= scale;
    float3 nextScaledPos = scaledPos + scaledDir;
    scaledDir = normalize(scaledDir);
    float outerRad = _AccretionOuterRadius*scale;

    float2 outerHitInfo = raySphere(volCenter, outerRad, scaledPos, scaledDir);
    float2 nextOuterHitInfo = raySphere(volCenter, outerRad, nextScaledPos, scaledDir);
    
    
    
    float dstThroughBounds = outerHitInfo.y - nextOuterHitInfo.y;

   
    
    float radialGradient = 1 - saturate((distFromCenter - _AccretionInnerRadius) / _AccretionOuterRadius);
    float density = saturate(6*pow(radialGradient, 12)*(1-radialGradient));
    float3 radialPos = cartesianToRadial(position, distFromCenter);
    density *= sampleNoiseTexture(radialPos);
    
    float colorFalloff = pow(density, _AccretionColorShift);
    float3 baseColor = lerp(_AccretionMainColor, _AccretionInnerColor, colorFalloff);
    color += baseColor * dstThroughBounds * pow(density, _AccretionFalloff);
}


void blueShift(inout float3 color, float shift) {
    for (float i = 0; shift > 0 && i < 3; i++) {
        float3 shiftAmount = min(shift, color);
        color -= shiftAmount;
        color.g += shiftAmount.r;
        color.b += shiftAmount.g;
        color.b += shiftAmount.b / 2;
        color.r += shiftAmount.b / 2;
        color = saturate(color);
        shift -= (shiftAmount.r + shiftAmount.g + shiftAmount.b) / 3; 
    }
}

// Gravitational shift source: https://astronomy.swin.edu.au/cosmos/g/Gravitational+Redshift
// As a photon travels toward/away from a mass, it may gain or lose energy causing it to change frequency
// The closer we are to a large mass, photons get blue shifted
void computeGravitationalShift(inout float3 color, float3 position, float3 center, float gravConst, float mass)
{
    float dist = distance(position, center);
    if (_BlueShiftPower <= 0 || dist < 0.001f){
        return;
    }
    float shift = exp(-dist * _BlueShiftPower);
    blueShift(color, shift);
}

