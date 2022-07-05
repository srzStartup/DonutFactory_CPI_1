using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-3)]
public class ObjectPooler : Singleton<ObjectPooler>
{
    [SerializeField] private Dictionary<string, Queue<GameObject>> poolDictionary;
    [SerializeField] private List<Pool> pools;

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> poolQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject go = Instantiate(pool.prefab);
                if (pool.parent)
                    go.transform.SetParent(pool.parent, pool.worldPositionStays);
                go.SetActive(false);
                poolQueue.Enqueue(go);
            }

            poolDictionary.Add(pool.tag, poolQueue);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, bool isActive = true, bool instantEnqueue = false)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return null;
        }

        if (poolDictionary[tag].Count != 0)
        {
            GameObject spawned = poolDictionary[tag].Dequeue();

            spawned.transform.SetPositionAndRotation(position, rotation);

            if (isActive)
                spawned.SetActive(true);

            if (instantEnqueue)
                poolDictionary[tag].Enqueue(spawned);

            return spawned;
        }
        else
        {
            return null;
        }
    }

    public void PushToQueue(string tag, GameObject go, bool clearParent = true)
    {
        if (clearParent)
            go.transform.parent = null;
        go.SetActive(false);

        poolDictionary[tag].Enqueue(go);
    }
}

[System.Serializable]
public class Pool
{
    public string tag;
    public int size;
    public GameObject prefab;
    public Transform parent;
    public bool worldPositionStays;
    public bool shouldExpand;
}
