using System.Collections.Generic;
using UnityEngine;

public class GameObjectsPool
{
    private List<GameObject> pooledObjects;
    private GameObject objectToPool;
    private System.Func<GameObject, GameObject> resetFunc;
    private int amount;

    public GameObjectsPool(
        string name,
        GameObject objectToPool,
        System.Func<GameObject, GameObject> resetFunc,
        int amount = 15)
    {
        this.objectToPool = objectToPool;
        this.resetFunc = resetFunc;
        this.amount = amount;

        Fill(name);
    }

    private void Fill(string name)
    {
        var parentInst = new GameObject(name);

        pooledObjects = new List<GameObject>(amount);
        for (int i = 0; i < amount; i++)
        {
            var inst = Object.Instantiate(objectToPool);
            inst.transform.parent = parentInst.transform;
            inst.SetActive(false);
            pooledObjects.Add(inst);
        }
    }

    public GameObject GetObject()
    {
        foreach (var obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return resetFunc(obj);
            }
        }
        Logger.Log("Pool empty, instantiated a new obj");
        return Object.Instantiate(objectToPool);
    }

    public void ReturnToPool(GameObject obj)
    {
        if (pooledObjects.Contains(obj))
        {
            obj.SetActive(false);
        }
        else
        {
            Logger.Log("Object not returned to pool");
            Object.Destroy(obj);
        }
    }
}
