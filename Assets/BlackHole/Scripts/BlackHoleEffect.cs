using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BlackHoleEffect : PostProcessEffectRenderer<BlackHolePostProcessingSettings>
{

    private List<Material> materials = new();
    
    public override void Render(PostProcessRenderContext context)
    {
        UpdateMaterials();
        MaterialRenderer.Render(materials, context);
    }

    private void UpdateMaterials()
    {
        var index = 0;
        var blackHoleSettings = settings.blackHole.value;
        foreach (var instance in Singularity.All)
        {
            if (materials.Count <= index)
            {
                materials.Add(new Material(blackHoleSettings.shader));
            }
            var material = materials[index];
            if (material.IsDestroyed())
            {
                materials[index] = new Material(blackHoleSettings.shader);
                material = materials[index];
            }

            // Update material to match settings
            material.SetColor("_ShadowColor", blackHoleSettings.ShadowColor);

            material.SetInt("_StepCount", blackHoleSettings.StepCount);
            material.SetFloat("_StepSize", blackHoleSettings.StepSize);
            material.SetFloat("_GravitationalConst", blackHoleSettings.GravitationalConst);
            material.SetFloat("_Attenuation", blackHoleSettings.Attenuation);

            material.SetFloat("_MaxEffectRadius", blackHoleSettings.MaxEffectRadius);
            material.SetFloat("_EffectFadeOutDist", blackHoleSettings.EffectFadeOutDist);
            material.SetFloat("_EffectFalloff", blackHoleSettings.EffectFalloff);

            material.SetFloat("_BlueShiftPower", blackHoleSettings.BlueShiftPower);

            material.SetInt("_AccretionQuality", !blackHoleSettings.RenderAccretion ? -1 : 0);
            material.SetColor("_AccretionMainColor", blackHoleSettings.AccretionMainColor);
            material.SetColor("_AccretionInnerColor", blackHoleSettings.AccretionInnerColor);
            material.SetFloat("_AccretionColorShift", blackHoleSettings.AccretionColorShift);
            material.SetFloat("_AccretionFalloff", blackHoleSettings.AccretionFalloff);
            material.SetFloat("_AccretionIntensity", blackHoleSettings.AccretionIntensity);
            material.SetFloat("_AccretionOuterRadius", blackHoleSettings.MaxEffectRadius * blackHoleSettings.AccretionOuterRadius);
            material.SetFloat("_AccretionInnerRadius", blackHoleSettings.MaxEffectRadius * blackHoleSettings.AccretionInnerRadius);
            material.SetFloat("_AccretionWidth", blackHoleSettings.AccretionWidth);
            material.SetVector("_AccretionDir", instance.transform.up);
            material.SetTexture("_AccretionNoiseTex", blackHoleSettings.AccretionNoiseTex);

            int noiseLayerCount = 0;
            float[] sampleScales = new float[4];
            float[] scrollRates = new float[4];
            for (int j = 0; j < blackHoleSettings.NoiseLayers.Length; j++)
            {
                NoiseLayer noiseLayer = blackHoleSettings.NoiseLayers[j];
                if(!noiseLayer.Enabled){
                    continue;
                }

                sampleScales[noiseLayerCount] = noiseLayer.SampleScale;
                scrollRates[noiseLayerCount] = noiseLayer.ScrollRate;
                noiseLayerCount++;
            }

            material.SetFloat("_NoiseLayerCount", noiseLayerCount);
            material.SetFloatArray("_SampleScales", sampleScales);
            material.SetFloatArray("_ScrollRates", scrollRates);

            material.SetFloat("_GasCloudThreshold", blackHoleSettings.GasCloudThreshold);
            material.SetFloat("_TransmittancePower", blackHoleSettings.TransmittancePower);

            material.SetVector("_Position", instance.transform.position);
            material.SetFloat("_SchwarzschildRadius", instance.SchwarzschildRadius);
            index++;
        }
    }
}