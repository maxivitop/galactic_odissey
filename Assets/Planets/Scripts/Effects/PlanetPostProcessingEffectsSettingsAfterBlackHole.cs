using System;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(PlanetPostProcessingEffectsRendererAfterBlackHole), PostProcessEvent.BeforeStack, "Custom/PlanetsAfterBlackHole")]
public class PlanetPostProcessingEffectsSettingsAfterBlackHole: PostProcessEffectSettings
{
    public PlanetEffectsParameter effects = new();
}
