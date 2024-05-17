using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PooledObject : MonoBehaviour
{
    public IObjectPool<PooledObject> pool;

    public void SetPool(IObjectPool<PooledObject> _pool) => pool = _pool;

    public virtual void ResetObject() { }

    public virtual void Release() { pool.Release(this); }

    //called when the pool is disposed or the object is destroyed
    //override for any objects that need to clean up additional stuff
    //e.g. area select instantiates a UI element for its highlight effect
    //e.g. armoured charge gives player hearts armour in the UI
    public virtual void DestroyFromPool() { }
}
