using System;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(PlanetPostProcessingEffectsRenderer), PostProcessEvent.BeforeTransparent, "Custom/Planets")]
public class PlanetPostProcessingEffectsSettings: PostProcessEffectSettings
{
    public PlanetEffectsParameter effects = new();
    
    [Serializable]
    public sealed class PlanetEffectsParameter : ParameterOverride<PlanetEffects>
    {
    }
    
}
