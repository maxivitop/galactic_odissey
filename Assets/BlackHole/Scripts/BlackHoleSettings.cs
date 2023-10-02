using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


public class BlackHoleSettings : ScriptableObject
{
    public Shader shader;
    public Cubemap skybox;
    public float schwarzschildRadiusRadius;
    [ColorUsage(default, true)]
    public Color shadowColor = Color.black;
    public int stepCount = 300;
    public float stepSize = 0.1f;
    public float gravitationalConst = 0.4f;
    [Tooltip("Affects how close rays travel from the event horizon.")]
    public float attenuation = 1.8f;
    
    public float maxEffectRadius = 5f;
    public float effectFadeOutDist = 10f;
    public float effectFalloff = 1f;

    public float blueShiftPower;
    
    public bool renderAccretion;
    [ColorUsage(default, true)]
    public Color accretionMainColor;
    [ColorUsage(default, true)]
    public Color accretionInnerColor;
    public float accretionColorShift = 80f;
    public float accretionFalloff;
    public float accretionIntensity;
    [MinMax(0f, 1f)]
    public Vector2 accretionRadius;
    public float accretionWidth;
    public Texture3D accretionNoiseTex;
    public NoiseLayer[] noiseLayers;

    [Range(0f, 1f)][Tooltip("Sample values lower than the threshold will be rejected.")]
    public float gasCloudThreshold;
    [Tooltip("How well light passes through the gas.")]
    public float transmittancePower;
    public float spiralStrength;
    
    private void OnValidate()
    {
        stepCount = Mathf.Max(stepCount, 0);
        stepSize = Mathf.Max(stepSize, 0f);
        gravitationalConst = Mathf.Max(gravitationalConst, 0f);
        attenuation = Mathf.Max(attenuation, 0f);
        blueShiftPower = Mathf.Max(blueShiftPower, 0f);

        maxEffectRadius = Mathf.Max(maxEffectRadius, 0f);
        effectFadeOutDist = Mathf.Max(effectFadeOutDist, 0f);
        accretionWidth = Mathf.Max(accretionWidth, 0f);
    }
}