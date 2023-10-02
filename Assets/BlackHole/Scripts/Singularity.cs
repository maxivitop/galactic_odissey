using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Singularity : MonoBehaviour
{
    public static readonly List<Singularity> All = new();
    private BlackHoleSettings settings;
    public BlackHoleSettings Settings {
        get {
            if (t == 0)
            {
                return start;
            }
            if (Math.Abs(t - 1) < 0.0001f)
            {
                return end;
            }
            InterpolateSettings();
            return settings;
        }
    }

    public BlackHoleSettings start;
    public BlackHoleSettings end;
    [Range(0, 1)]
    public float t;
    
    private void OnEnable()
    {
        settings = ScriptableObject.CreateInstance(typeof(BlackHoleSettings)) as BlackHoleSettings;
        All.Add(this);
    }
    private void OnDisable()
    {
        All.Remove(this);
    }

    private void InterpolateSettings()
    {
        settings.shader = start.shader;
        settings.skybox = start.skybox;
        settings.schwarzschildRadiusRadius = Mathf.Lerp(start.schwarzschildRadiusRadius, end.schwarzschildRadiusRadius, t);
        settings.shadowColor = Color.Lerp(start.shadowColor, end.shadowColor, t);
        settings.stepCount = Mathf.RoundToInt(Mathf.Lerp(start.stepCount, end.stepCount, t));
        settings.stepSize = Mathf.Lerp(start.stepSize, end.stepSize, t);
        settings.gravitationalConst = Mathf.Lerp(start.gravitationalConst, end.gravitationalConst, t);
        settings.attenuation = Mathf.Lerp(start.attenuation, end.attenuation, t);

        settings.maxEffectRadius = Mathf.Lerp(start.maxEffectRadius, end.maxEffectRadius, t);
        settings.effectFadeOutDist = Mathf.Lerp(start.effectFadeOutDist, end.effectFadeOutDist, t);
        settings.effectFalloff = Mathf.Lerp(start.effectFalloff, end.effectFalloff, t);

        settings.blueShiftPower = Mathf.Lerp(start.blueShiftPower, end.blueShiftPower, t);

        settings.renderAccretion = start.renderAccretion;
        settings.accretionMainColor = Color.Lerp(start.accretionMainColor, end.accretionMainColor, t);
        settings.accretionInnerColor = Color.Lerp(start.accretionInnerColor, end.accretionInnerColor, t);
        settings.accretionColorShift = Mathf.Lerp(start.accretionColorShift, end.accretionColorShift, t);
        settings.accretionFalloff = Mathf.Lerp(start.accretionFalloff, end.accretionFalloff, t);
        settings.accretionIntensity = Mathf.Lerp(start.accretionIntensity, end.accretionIntensity, t);
        settings.accretionRadius = Vector2.Lerp(start.accretionRadius, end.accretionRadius, t);
        settings.accretionWidth = Mathf.Lerp(start.accretionWidth, end.accretionWidth, t);
        settings.accretionNoiseTex = start.accretionNoiseTex;
        NoiseLayer[] noiseLayers = new NoiseLayer[start.noiseLayers.Length];
        for (int i = 0; i < start.noiseLayers.Length; i++)
        {
            noiseLayers[i] = new NoiseLayer();
            noiseLayers[i].sampleScale =
                Vector3.Lerp(start.noiseLayers[i].sampleScale, end.noiseLayers[i].sampleScale, t);
            noiseLayers[i].scrollRate =
                Vector3.Lerp(start.noiseLayers[i].scrollRate, end.noiseLayers[i].scrollRate, t);
            noiseLayers[i].enabled = start.noiseLayers[i].enabled;
        }
        settings.noiseLayers = noiseLayers;
        settings.gasCloudThreshold = Mathf.Lerp(start.gasCloudThreshold, end.gasCloudThreshold, t);
        settings.transmittancePower = Mathf.Lerp(start.transmittancePower, end.transmittancePower, t);
        settings.spiralStrength = Mathf.Lerp(start.spiralStrength, end.spiralStrength, t);
    }
}