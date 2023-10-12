using System;
using UnityEngine;

[Serializable]
public class TileBgData : ITileData
{
    public TileState State { get; set; }
}

public class TileBgRenderer : BaseTileRenderer<HexTile, TileBgData>
{
    public Color normalColor;
    public Color emptyColor;
    public Color selectedColor;
    public Color hoveredColor;
    public Color outlineColor;
    protected override void Customize(HexTile obj, TileBgData bgData)
    {
        var color = bgData.State switch
        {
            TileState.Normal => normalColor,
            TileState.Outline => outlineColor,
            TileState.Hovered => hoveredColor,
            TileState.Selected => selectedColor,
            _ => emptyColor
        };

        obj.spriteRenderer.color = color;
    }
}