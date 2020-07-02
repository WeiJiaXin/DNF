using System.Collections.Generic;
using UnityEngine;

public static class PoolManager
{
    private static Dictionary<string, Pool> pools;

    static PoolManager()
    {
        pools = new Dictionary<string, Pool>();
    }

    public static Pool<T> GetPool<T>(string path) where T : MonoBehaviour
    {
        if (!pools.ContainsKey(path))
        {
            pools.Add(path, new Pool(Resources.Load<GameObject>(path)));
        }

        return new Pool<T>(pools[path]);
    }
}