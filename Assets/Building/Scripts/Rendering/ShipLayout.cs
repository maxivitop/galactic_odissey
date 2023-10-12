using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ShipLayout : MonoBehaviour
{
    public int mapSize = 3;
    public Transform shopPivot;
    private readonly Ship ship = new();
    private readonly Shop shop = new();
    private readonly ISet<Hex> map = new HashSet<Hex>();
    private ISet<Hex> outline = new HashSet<Hex>();
    private Dictionary<Type, ITileRenderer> renderers = new();
    private Vector3 mousePosition;
    private Vector3 shopPosition;
    private Hex mouseShipPositionHex;
    private Hex mouseShopPositionHex;
    private TileBgData tileBgData = new();
    [CanBeNull] private ITileData draggingTile;

    private void Start()
    {
        shop.Fill();
        foreach (var tileRenderer in GetComponents<ITileRenderer>())
        {
            renderers[tileRenderer.GetTileDataType()] = tileRenderer;
        }
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
    }

    private void Update()
    {
        foreach (var (_, tileRenderer) in renderers)
        {
            tileRenderer.Clear();
        }
        
        UpdateOutline();
        UpdateShopPosition();
        mousePosition = Camera.main!.ScreenToWorldPoint(AddCameraDepth(Input.mousePosition));
        mouseShipPositionHex = Hex.FromCartesian(mousePosition - transform.position);
        mouseShopPositionHex = Hex.FromCartesian(mousePosition - shopPosition);
        DrawMapAndShip();
        DrawShop();
        DrawDraggingTile();
    }

    private void DrawMapAndShip()
    {
        foreach (var hex in map)
        {
            var state = TileState.Empty;
            if (outline.Contains(hex))
            {
                state = TileState.Outline;
            }
            if (ship.tiles.ContainsKey(hex))
            {
                state = TileState.Normal;
            }
            if (hex == mouseShipPositionHex)
            {
                state = TileState.Hovered;
            }
            if (hex == mouseShipPositionHex && Input.GetMouseButton(0))
            {
                state = TileState.Selected;
            }
            Vector3 position = hex.ToCartesian();
            position += transform.position;
            DrawTile(tileBgData, state, position);
            if (ship.tiles.TryGetValue(hex, out var shipData))
            {
                DrawTile(shipData, state, position);
            }
        }
    }
    
    private void UpdateShopPosition()
    {
        var topLeftCorner = Vector3.zero;
        topLeftCorner.y = Screen.height;
        shopPosition = Camera.main!.ScreenToWorldPoint(AddCameraDepth(topLeftCorner));
        shopPosition += shopPivot.position;
    }

    private void DrawShop()
    {
        shop.shopPositionToIndex.Clear();
        var position = Hex.zero;
        Vector2 shopPosV2 = shopPosition;
        var i = 0;
        foreach (var shopItem in shop.shopItems)
        {
            if (i % 6 == 0)
            {
                position = Hex.right * i / 6;
            }

            if (shopItem == shop.dummy)
            {
                DrawTile(tileBgData, TileState.Empty, position.ToCartesian() + shopPosV2);
            }
            else
            {
                DrawTile(tileBgData, TileState.Normal, position.ToCartesian() + shopPosV2);
                DrawTile(shopItem, TileState.Normal, position.ToCartesian() + shopPosV2);
                shop.shopPositionToIndex[position] = i;
            }
            position += i % 2 == 0 ? Hex.downRight : Hex.downLeft;
            i++;
        }
    }

    private void DrawDraggingTile()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (ship.tiles.TryGetValue(mouseShipPositionHex, out var draggingData))
            {
                draggingTile = draggingData;
                ship.tiles.Remove(mouseShipPositionHex);
            }

            if (shop.shopPositionToIndex.TryGetValue(mouseShopPositionHex, out var shopIndex))
            {
                draggingTile = shop.shopItems[shopIndex];
                shop.shopItems[shopIndex] = shop.dummy;
            }
        }

        if (Input.GetMouseButtonUp(0) && draggingTile != null)
        {
            if (ship.tiles.TryGetValue(mouseShipPositionHex, out var oldShipData))
            {
                shop.AddToShop(oldShipData);
            }

            if (map.Contains(mouseShipPositionHex))
            {
                ship.tiles[mouseShipPositionHex] = draggingTile;
            }
            else
            {
                shop.AddToShop(draggingTile);
            }
            draggingTile = null;
            shop.RemoveUnused(aggressive:true);
        }

        if (draggingTile != null)
        {
            DrawTile(draggingTile, TileState.Hovered, mousePosition);
        }
    }

    private void DrawTile(ITileData tileData, TileState state, Vector3 position)
    {
        tileData.State = state;
        renderers[tileData.GetType()].Render(tileData, position);
    }


    private void UpdateOutline()
    {
        outline.Clear();
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

    private Vector3 AddCameraDepth(Vector3 screenPosition)
    {
        screenPosition.z = -Camera.main!.transform.position.z;
        return screenPosition;
    }
}
