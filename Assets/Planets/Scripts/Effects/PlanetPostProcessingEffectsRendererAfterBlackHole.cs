using UnityEngine.Rendering.PostProcessing;

public class PlanetPostProcessingEffectsRendererAfterBlackHole : PostProcessEffectRenderer<PlanetPostProcessingEffectsSettingsAfterBlackHole>
{

    public override void Render(PostProcessRenderContext context)
    {
        MaterialRenderer.Render(settings.effects.value.GetMaterials(true), context);
    }

}
