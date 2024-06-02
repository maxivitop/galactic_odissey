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

    public virtual void Render(ITileData data, Vector3 position, Quaternion rotation)
    {
        var spawned = spawner.Spawn(position, rotation);
        Customize(spawned, (TD) data);
    }

    public virtual void OnRenderingStarted()
    {
        spawner.OnRenderingStarted();
    }
    
    public virtual void OnRenderingFinished()
    {
        spawner.OnRenderingFinished();
    }

    protected virtual void Customize(TM obj, TD data) {}

    public Type GetTileDataType()
    {
        return typeof(TD);
    }
}