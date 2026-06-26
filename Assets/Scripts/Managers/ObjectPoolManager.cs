using System.Collections.Generic;
using UnityEngine;
using Singleton;

public class ObjectPoolManager : SingletonBase<ObjectPoolManager>
{
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
    }

    public GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        int key = prefab.GetInstanceID();
        if (!poolDictionary.TryGetValue(key, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            poolDictionary[key] = queue;
        }

        GameObject obj = null;
        while (queue.Count > 0)
        {
            GameObject dequeued = queue.Dequeue();
            if (dequeued != null)
            {
                obj = dequeued;
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                break;
            }
        }

        if (obj == null)
        {
            obj = Instantiate(prefab, position, rotation);
            PooledObject pooledScript = obj.GetComponent<PooledObject>();
            if (pooledScript == null) pooledScript = obj.AddComponent<PooledObject>();
            pooledScript.prefabKey = key;
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        PooledObject pooledScript = obj.GetComponent<PooledObject>();
        if (pooledScript != null)
        {
            int key = pooledScript.prefabKey;
            if (!poolDictionary.TryGetValue(key, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                poolDictionary[key] = queue;
            }

            obj.SetActive(false);
            if (!queue.Contains(obj))
            {
                queue.Enqueue(obj);
            }
        }
        else
        {
            Destroy(obj); // Fallback if not instantiated via object pooling
        }
    }
}
