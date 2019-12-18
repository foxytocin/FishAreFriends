// Code von Florian Eder

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]

public class ObjectPoolItem
{
    public GameObject objectToPool;
    public int amountToPool;
    public string identifier;
}

public class ObjectPooler : MonoBehaviour
{


    public List<ObjectPoolItem> itemsToPool;
    public Dictionary<string, Queue<GameObject>> pooledObjectsDictionary;


    #region singelton
    public static ObjectPooler Instance;

    void Awake()
    {
        Instance = this;
    }

    #endregion singelton

    // Use this for initialization
    void Start()
    {
        pooledObjectsDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (ObjectPoolItem item in itemsToPool)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = (GameObject)Instantiate(item.objectToPool);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            pooledObjectsDictionary.Add(item.identifier, queue);
        }
    }


    public GameObject SpawnObjectFromPool(string identifier, Vector3 position, Quaternion rotation)
    {

        if (!pooledObjectsDictionary.ContainsKey(identifier))
        {
            Debug.Log("Object Pool doesnt contains object identified by key: " + identifier);
            return null;
        }

        GameObject objectToSpawn = pooledObjectsDictionary[identifier].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        pooledObjectsDictionary[identifier].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public GameObject GetPooledObject(string identifier)
    {
        if (!pooledObjectsDictionary.ContainsKey(identifier))
        {
            Debug.Log("Object Pool doesnt contains object identified by key: " + identifier);
            return null;
        }

        GameObject gameObject = pooledObjectsDictionary[identifier].Dequeue();
        pooledObjectsDictionary[identifier].Enqueue(gameObject);

        return gameObject;

    }


    public GameObject SpawnRandomObjectFromPool(Vector3 position, Quaternion rotation)
    {
        int randomNumber = Random.Range(0, itemsToPool.Count - 1);
        string identifier = itemsToPool[randomNumber].identifier;

        GameObject objectToSpawn = pooledObjectsDictionary[identifier].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        pooledObjectsDictionary[identifier].Enqueue(objectToSpawn);

        return objectToSpawn;
    }


    public GameObject GetRandomPooledObject()
    {
        int randomNumber = Random.Range(0, itemsToPool.Count - 1);
        string identifier = itemsToPool[randomNumber].identifier;

        if (pooledObjectsDictionary == null)
        {
            return null;
        }

        Queue<GameObject> queue = pooledObjectsDictionary[identifier];
        if (queue == null)
        {
            Debug.Log("Queue is empty");
        }

        GameObject gameObject = queue.Dequeue();
        pooledObjectsDictionary[identifier].Enqueue(gameObject);

        return gameObject;
    }

}
