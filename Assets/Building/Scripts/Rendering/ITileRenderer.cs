using System;
using UnityEngine;

public interface ITileRenderer
{
    Type GetTileDataType();
    
    void Render(ITileData data, Vector3 position);
    
    /**
     * Called once before each rendering frame
     */
    void Clear();
}