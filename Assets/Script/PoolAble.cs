using UnityEngine;
using UnityEngine.Pool;

public class PoolAble : MonoBehaviour
{
    public IObjectPool<GameObject> pool { get; set; }

    public void ReleaseObject()
    {
        pool.Release(gameObject);
    }
}