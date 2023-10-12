using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class PrefabSpawner<T> where T: Component
{
    private static readonly Dictionary<Type, PrefabSpawner<T>> registry = new();

    public static PrefabSpawner<T> Obtain()
    {
        var type = typeof(T);
        if (registry.TryGetValue(type, out var spawner))
        {
            return spawner;
        }
        var prefabSpawner = new PrefabSpawner<T>();
        registry[type] = prefabSpawner;
        return prefabSpawner;
    }

    public T prefab;
    private List<T> prefabPool = new();
    private List<T> spawned = new();

    public T Spawn(Vector3 position)
    {
        if (prefabPool.Count == 0)
        {
            var instantiated = Object.Instantiate(prefab, position, Quaternion.identity);
            spawned.Add(instantiated);
            return instantiated;
        }

        var spawnedTransform = prefabPool[^1];
        prefabPool.RemoveAt(prefabPool.Count - 1);
        spawnedTransform.transform.position = position;
        return spawnedTransform;
    }

    public void Clear()
    {
        prefabPool.AddRange(spawned);
        foreach (var t in spawned)
        {
            t.transform.position = new Vector3(1e5f, 1e5f);
        }
    }
}