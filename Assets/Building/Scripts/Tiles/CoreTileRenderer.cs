using System;
using UnityEngine;

[Serializable]
public class CoreTileData : BaseTileData { }

public class CoreTileRenderer : BaseTileRenderer<Transform, CoreTileData> {}