using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 오브젝트 풀에 속한 오브젝트의 공통 베이스.
/// 반환은 반드시 <see cref="ReleaseObject"/>로 한다. 이미 반환된(비활성) 오브젝트를 다시 반환하면 무시한다.
/// </summary>
public class PoolAble : MonoBehaviour
{
    public IObjectPool<GameObject> pool { get; set; }

    /// <summary>풀에 반환되어 비활성 상태인지. 풀은 반환 시 SetActive(false)를 한다.</summary>
    public bool IsInPool => !gameObject.activeSelf;

    public void ReleaseObject()
    {
        if (IsInPool)
        {
            GameLog.Info(GameLog.Cat.Pool, "이중 반환 무시: " + name, this);
            return; // 이중 반환 방지 (지연 콜백·여러 경로에서 동시에 반환 요청)
        }

        OnBeforeRelease();

        if (pool == null)
        {
            Destroy(gameObject); // 풀 없이 배치된 오브젝트
            return;
        }

        pool.Release(gameObject);
    }

    /// <summary>
    /// 반환 직전 정리(트윈 Kill 등). 반환은 <see cref="ReleaseObject"/> 한 곳이므로 여기서 하면 모든 반환 경로에 적용된다(B-141).
    /// OnDisable 은 부모와 함께 꺼질 때도 불리므로(풀 반환이 아님) 반환 전용 정리는 여기에 둔다.
    /// </summary>
    protected virtual void OnBeforeRelease()
    {
    }
}
