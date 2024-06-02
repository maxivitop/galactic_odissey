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
    private List<T> prefabPool = new(); // unused this frame
    private List<T> spawned = new(); // all ever spawned

    public T Spawn(Vector3 position, Quaternion rotation)
    {
        if (prefabPool.Count == 0)
        {
            var instantiated = Object.Instantiate(prefab, position, rotation);
            spawned.Add(instantiated);
            return instantiated;
        }

        var spawnedTransform = prefabPool[^1];
        prefabPool.RemoveAt(prefabPool.Count - 1);
        spawnedTransform.transform.position = position;
        spawnedTransform.transform.rotation = rotation;
        spawnedTransform.gameObject.SetActive(true);
        return spawnedTransform;
    }

    public void OnRenderingStarted()
    {
        prefabPool.AddRange(spawned);
    }
    
    public void OnRenderingFinished()
    {
        foreach (var t in prefabPool)
        { 
            t.gameObject.SetActive(false); // Disable unused objects for this frame
        }
        prefabPool.Clear();
    }
}