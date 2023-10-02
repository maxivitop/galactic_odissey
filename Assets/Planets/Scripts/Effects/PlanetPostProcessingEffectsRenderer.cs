using UnityEngine.Rendering.PostProcessing;

public class PlanetPostProcessingEffectsRenderer : PostProcessEffectRenderer<PlanetPostProcessingEffectsSettings>
{

    public override void Render(PostProcessRenderContext context)
    {
        MaterialRenderer.Render(settings.effects.value.GetMaterials(false), context);
    }

}
