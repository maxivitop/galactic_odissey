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
        foreach (var instance in Singularity.All)
        {
            var blackHoleSettings = instance.Settings;
            if (materials.Count <= index)
            {
                materials.Add(new Material(blackHoleSettings.shader));
            }
            var material = materials[index];
            if (material == null)
            {
                materials[index] = new Material(blackHoleSettings.shader);
                material = materials[index];
            }

            // Update material to match settings
            material.SetTexture("_Skybox", blackHoleSettings.skybox);
            material.SetColor("_ShadowColor", blackHoleSettings.shadowColor);

            material.SetInt("_StepCount", blackHoleSettings.stepCount);
            material.SetFloat("_StepSize", blackHoleSettings.stepSize);
            material.SetFloat("_GravitationalConst", blackHoleSettings.gravitationalConst);
            material.SetFloat("_Attenuation", blackHoleSettings.attenuation);

            material.SetFloat("_MaxEffectRadius", blackHoleSettings.maxEffectRadius);
            material.SetFloat("_EffectFadeOutDist", blackHoleSettings.effectFadeOutDist);
            material.SetFloat("_EffectFalloff", blackHoleSettings.effectFalloff);

            material.SetFloat("_BlueShiftPower", blackHoleSettings.blueShiftPower);

            material.SetInt("_AccretionQuality", !blackHoleSettings.renderAccretion ? -1 : 0);
            material.SetColor("_AccretionMainColor", blackHoleSettings.accretionMainColor);
            material.SetColor("_AccretionInnerColor", blackHoleSettings.accretionInnerColor);
            material.SetFloat("_AccretionColorShift", blackHoleSettings.accretionColorShift);
            material.SetFloat("_AccretionFalloff", blackHoleSettings.accretionFalloff);
            material.SetFloat("_AccretionIntensity", blackHoleSettings.accretionIntensity);
            material.SetFloat("_AccretionOuterRadius", blackHoleSettings.maxEffectRadius * blackHoleSettings.accretionRadius.y);
            material.SetFloat("_AccretionInnerRadius", blackHoleSettings.maxEffectRadius * blackHoleSettings.accretionRadius.x);
            material.SetFloat("_AccretionWidth", blackHoleSettings.accretionWidth);
            material.SetVector("_AccretionDir", instance.transform.up);
            material.SetTexture("_AccretionNoiseTex", blackHoleSettings.accretionNoiseTex);
            material.SetFloat("_SpiralStrength", blackHoleSettings.spiralStrength);

            int noiseLayerCount = 0;
            Vector4[] sampleScales = new Vector4[4];
            Vector4[] scrollRates = new Vector4[4];
            for (int j = 0; j < blackHoleSettings.noiseLayers.Length; j++)
            {
                NoiseLayer noiseLayer = blackHoleSettings.noiseLayers[j];
                if(!noiseLayer.enabled){
                    continue;
                }

                sampleScales[noiseLayerCount] = noiseLayer.sampleScale;
                scrollRates[noiseLayerCount] = noiseLayer.scrollRate;
                noiseLayerCount++;
            }

            material.SetFloat("_NoiseLayerCount", noiseLayerCount);
            material.SetVectorArray("_SampleScales", sampleScales);
            material.SetVectorArray("_ScrollRates", scrollRates);

            material.SetFloat("_GasCloudThreshold", blackHoleSettings.gasCloudThreshold);
            material.SetFloat("_TransmittancePower", blackHoleSettings.transmittancePower);

            material.SetVector("_Position", instance.transform.position);
            material.SetFloat("_SchwarzschildRadius", blackHoleSettings.schwarzschildRadiusRadius);
            index++;
        }
        for (int i = materials.Count - 1; i >= index; i--)
        {
            materials.RemoveAt(i);
        }
    }
}