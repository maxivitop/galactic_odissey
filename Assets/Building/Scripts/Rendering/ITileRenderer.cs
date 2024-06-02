using System;
using UnityEngine;

public interface ITileRenderer
{
    Type GetTileDataType();
    
    void Render(ITileData data, Vector3 position, Quaternion rotation);
    
    /**
     * Called once before each rendering frame
     */
    void OnRenderingStarted();
    
    void OnRenderingFinished();
}