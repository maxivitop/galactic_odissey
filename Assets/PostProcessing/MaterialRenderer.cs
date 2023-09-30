using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public static class MaterialRenderer
{
    public static void Render(List<Material> materials, PostProcessRenderContext context)
    {
        if (materials.Count == 0)
        {
            context.command.Blit(context.source, context.destination);
            return;
        } 
        if (materials.Count == 1)
        {
            context.command.Blit(context.source, context.destination, materials[0]);
            return;
        }
        int tmpId1 = 69420123;
        int tmpId2 = 69420124;
        context.command.GetTemporaryRT(tmpId1, context.width, context.height);
        context.command.GetTemporaryRT(tmpId2, context.width, context.height);
        var tmpRenderTextureIdentifier1 = new RenderTargetIdentifier(tmpId1);
        var tmpRenderTextureIdentifier2 = new RenderTargetIdentifier(tmpId2);
        for (var index = 0; index < materials.Count; index++)
        {
            var material = materials[index];
            var use1 = index % 2 == 0;
            var src = use1 ? tmpRenderTextureIdentifier2 : tmpRenderTextureIdentifier1;
            var dest = use1 ? tmpRenderTextureIdentifier1 : tmpRenderTextureIdentifier2;
            if (index == 0)
            {
                src = context.source;
            }
            if (index == materials.Count - 1)
            {
                dest = context.destination;
            }
            context.command.Blit(src, dest, material);
        }
        context.command.ReleaseTemporaryRT(tmpId1);
        context.command.ReleaseTemporaryRT(tmpId2);
    }

}
