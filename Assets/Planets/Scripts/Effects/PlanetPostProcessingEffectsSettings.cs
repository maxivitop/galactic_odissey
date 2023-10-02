using System;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(PlanetPostProcessingEffectsRenderer), PostProcessEvent.BeforeStack, "Custom/Planets")]
public class PlanetPostProcessingEffectsSettings: PostProcessEffectSettings
{
    public PlanetEffectsParameter effects = new();
}
