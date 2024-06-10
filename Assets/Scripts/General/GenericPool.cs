using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GenericPool : MonoBehaviour
{
    private GameObject objectPrefab;
    public ObjectPool<PooledObject> objectPool;
    public int inactiveCount;
    public int activeCount;

    public GenericPool Init(GameObject prefab, int startCapacity = 10, bool preInstantiate = true)
    {
        objectPrefab = prefab;
        objectPool = new ObjectPool<PooledObject>(CreateObject, OnTakeObjectFromPool, OnReturnObjectToPool, OnDestroyObjectInPool, true, startCapacity);
        if(preInstantiate)
        {
            List<PooledObject> list = new List<PooledObject>();
            for(int i = 0; i < startCapacity; i++)
            {
                PooledObject pooledObject = objectPool.Get();
                list.Add(pooledObject);
            }
            for(int i = 0; i < startCapacity; i++)
            {
                objectPool.Release(list[i]);
            }
        }
        return this;
    }

    public ObjectPool<PooledObject> GetPool() { return objectPool; }

    public PooledObject CreateObject()
    {
        var _object = Instantiate(objectPrefab).GetComponent<PooledObject>();
        _object.transform.SetParent(transform);
        _object.SetPool(objectPool);
        _object.gameObject.SetActive(false);
        return _object;
    }

    public void OnTakeObjectFromPool(PooledObject _object)
    {
        _object.ResetObject();
    }

    public void OnReturnObjectToPool(PooledObject _object)
    {
        _object.gameObject.SetActive(false);
    }

    public void OnDestroyObjectInPool(PooledObject _object)
    {
        _object.DestroyFromPool();
        Destroy(_object.gameObject);
    }

    public void ReleaseChildren()
    {
        int childCount = transform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                transform.GetChild(i).GetComponent<PooledObject>().Release();
            }
        }
    }
}
