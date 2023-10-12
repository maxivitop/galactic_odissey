using System.Collections.Generic;

public class Shop
{
    public readonly List<ITileData> shopItems = new();
    public readonly Dictionary<Hex, int> shopPositionToIndex = new();
    public readonly ITileData dummy = new TileBgData();

    public void Fill()
    {
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
        shopItems.Add(new GunTileData());
        shopItems.Add(new GeneratorTileData());
    }
    
    
    public void AddToShop(ITileData tileData)
    {
        for (var i = 0; i < shopItems.Count; i++)
        {
            if (shopItems[i] != dummy) continue;
            shopItems[i] = tileData;
            return;
        }
        shopItems.Add(tileData);
    }

    public ITileData ExtractItem(int index)
    {
        var item = shopItems[index];
        shopItems[index] = dummy;
        RemoveUnused(aggressive:false);
        return item;
    }

    public void RemoveUnused(bool aggressive)
    {
        for (var i = shopItems.Count - 1; i >= 0; i--)
        {
            if (shopItems[i] == dummy)
            {
                shopItems.RemoveAt(i);
            }
            else if (!aggressive)
            {
                break;
            }
        }
    }
}
