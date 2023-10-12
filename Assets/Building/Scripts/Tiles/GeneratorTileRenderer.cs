using System;
using UnityEngine;

[Serializable]
public class GeneratorTileData : BaseTileData
{
    public int radius = 1;
}

public class GeneratorTileRenderer : BaseTileRenderer<Transform, GeneratorTileData>
{
    public FilledHexTile indicatorPrefab;
    public Color indicatorColor;
    private readonly PrefabSpawner<FilledHexTile> indicatorSpawner = new();

    protected override void Awake()
    {
        base.Awake();
        indicatorSpawner.prefab = indicatorPrefab;
    }

    public override void Render(ITileData data, Vector3 position)
    {
        base.Render(data, position);
        if (data.State != TileState.Hovered) return;
        var generatorData = (GeneratorTileData) data;
        foreach (var neighbour in Hex.Circle(Hex.zero, generatorData.radius))
        {
            var indicator = 
                indicatorSpawner.Spawn(position + (Vector3) neighbour.ToCartesian());
            indicator.spriteRenderer.color = indicatorColor;
        }
    }

    public override void Clear()
    {
        base.Clear();
        indicatorSpawner.Clear();
    }
}