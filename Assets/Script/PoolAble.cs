using UnityEngine;
using UnityEngine.Pool;

public class PoolAble : MonoBehaviour
{
    public IObjectPool<GameObject> pool { get; set; }
    private bool isReleased = false;
    private void OnEnable()
    {
        isReleased = false; // 활성화 시 초기화
    }

    public void ReleaseObject()
    {
        pool.Release(gameObject);
    }
}