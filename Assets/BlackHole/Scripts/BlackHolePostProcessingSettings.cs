using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BlackHoleEffect), PostProcessEvent.BeforeStack, "Custom/BlackHole")]
public class BlackHolePostProcessingSettings: PostProcessEffectSettings
{
}
