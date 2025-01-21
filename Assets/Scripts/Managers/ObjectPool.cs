using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPool : SingletonPersistent<ObjectPool>
{
    private List<GameObject>[] pooledObjects;
    [SerializeField] int[] poolAmount;

    [SerializeField] GameObject[] pooledObjectPrefabs;

    // Generates X number of objects based on poolAmount, corresponding with pooledObjectPrefabs
    void Start()
    {
        pooledObjects = new List<GameObject>[pooledObjectPrefabs.Length];

        for (int i = 0; i < pooledObjectPrefabs.Length; i++)
        {
            pooledObjects[i] = new List<GameObject>();
            for (int j = 0; j < poolAmount[i]; j++)
            {
                GameObject obj = Instantiate(pooledObjectPrefabs[i]);
                obj.transform.SetParent(this.transform);
                obj.SetActive(false);
                pooledObjects[i].Add(obj);
            }
        }
    }

    //Called via ObjectPool.Instance.GetPooledObject(obj). Sets an object active, and creates a new one if there are none avaliable
    public GameObject GetPooledObject(GameObject targetObject)
    {
        for (int i = 0; i < pooledObjectPrefabs.Length; i++)
        {
            if (pooledObjectPrefabs[i] == targetObject)
            {
                for (int j = 0; j < pooledObjects[i].Count; j++)
                {
                    if (!pooledObjects[i][j].activeInHierarchy) { return pooledObjects[i][j]; }
                }

                GameObject obj = Instantiate(pooledObjectPrefabs[i]);
                obj.transform.SetParent(this.transform);
                obj.SetActive(false);
                pooledObjects[i].Add(obj);
                return obj;
            }
        }
        return null;

    }
}
