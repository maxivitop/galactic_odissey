using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BlackHoleEffect), PostProcessEvent.BeforeTransparent, "Custom/BlackHole")]
public class BlackHolePostProcessingSettings: PostProcessEffectSettings
{
    public BlackHoleSettingsParameter blackHole = new()
    {
    };

    [Serializable]
    public sealed class BlackHoleSettingsParameter : ParameterOverride<BlackHoleSettings>
    {
        
    }
}
