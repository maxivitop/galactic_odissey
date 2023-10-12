using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TileBgRenderer))]
public class ShipLayout : MonoBehaviour
{
    public int mapSize = 3;
    private readonly Ship ship = new();
    private readonly ISet<Hex> map = new HashSet<Hex>();
    private ISet<Hex> outline = new HashSet<Hex>();
    private TileBgRenderer tileBgRenderer;
    private Dictionary<Type, ITileRenderer> renderers = new();

    private void Start()
    {
        tileBgRenderer = GetComponent<TileBgRenderer>();
        ship.tiles[Hex.zero] = new CoreTileData();
        ship.tiles[Hex.upRight] = new GeneratorTileData()
        {
            radius = 2
        };
        ship.tiles[Hex.downRight] = new GunTileData();
        foreach (var hex in Hex.Circle(Hex.zero, mapSize))
        {
            map.Add(hex);
        }
        foreach (var tileRenderer in GetComponents<ITileRenderer>())
        {
            renderers[tileRenderer.GetTileDataType()] = tileRenderer;
        }
    }

    private void Update()
    {
        foreach (var (_, tileRenderer) in renderers)
        {
            tileRenderer.Clear();
        }
        
        UpdateOutline();
        var mousePos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        var mousePosHex = Hex.FromCartesian(mousePos);
        foreach (var hex in map)
        {
            var state = TileState.Empty;
            var tileData = new TileBgData();
            if (outline.Contains(hex))
            {
                state = TileState.Outline;
            }
            if (ship.tiles.ContainsKey(hex))
            {
                state = TileState.Normal;
            }
            if (hex == mousePosHex)
            {
                state = TileState.Hovered;
            }
            if (hex == mousePosHex && Input.GetMouseButton(0))
            {
                state = TileState.Selected;
            }

            tileData.State = state;
            var position = hex.ToCartesian();
            
            tileBgRenderer.Render(tileData, position);

            if (ship.tiles.TryGetValue(hex, out var shipTile))
            {
                shipTile.State = state;
                renderers[shipTile.GetType()].Render(shipTile, position);
            }
        }
    }

    private void UpdateOutline()
    {
        foreach (var hex in ship.tiles.Keys)
        {
            foreach (var neighbour in Hex.neighbours)
            {
                var currHex = hex + neighbour;
                if (map.Contains(currHex))
                {
                    outline.Add(currHex);
                }
            }
        }
    }
}
