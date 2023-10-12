using System;
using UnityEngine;

public abstract class BaseTileRenderer<TM, TD> : MonoBehaviour, ITileRenderer
    where TM: Component
    where TD: ITileData
{
    public TM prefab;
    private readonly PrefabSpawner<TM> spawner = new();

    protected virtual void Awake()
    {
        spawner.prefab = prefab;
    }

    public virtual void Render(ITileData data, Vector3 position)
    {
        var spawned = spawner.Spawn(position);
        Customize(spawned, (TD) data);
    }

    public virtual void Clear()
    {
        spawner.Clear();
    }

    protected virtual void Customize(TM obj, TD data) {}

    public Type GetTileDataType()
    {
        return typeof(TD);
    }
}