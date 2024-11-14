using UnityEngine;
using UnityEngine.Pool;

public class PoolAble : MonoBehaviour
{
    public IObjectPool<GameObject> pool { get; set; }
    private bool isReleased = false;
    private void OnEnable()
    {
        isReleased = false; // Ȱ��ȭ �� �ʱ�ȭ
    }

    public void ReleaseObject()
    {
        pool.Release(gameObject);
    }
}