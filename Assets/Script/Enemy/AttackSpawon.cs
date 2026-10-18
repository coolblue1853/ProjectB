using UnityEngine;

/// <summary>
/// 지연 소환 공격(예: 크리핑테러의 발밑 화염). 풀에서 꺼낼 때마다 summonTime 뒤 공격을 켜고 deletTime 뒤 풀로 반환한다.
/// (기존: Start에서 한 번만 예약 + Destroy → 재사용 시 소환 안 됨·풀 개수 감소, B-204)
/// </summary>
public class AttackSpawon : PoolAble
{
    public float summonTime;
    public GameObject attackPrefab;
    public float deletTime;

    private void OnEnable()
    {
        if (attackPrefab != null)
        {
            attackPrefab.SetActive(false);
        }
        Invoke(nameof(Summon), summonTime);
        Invoke(nameof(Delete), deletTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void Summon()
    {
        if (attackPrefab != null)
        {
            attackPrefab.SetActive(true);
        }
    }

    void Delete()
    {
        ReleaseObject();
    }
}
