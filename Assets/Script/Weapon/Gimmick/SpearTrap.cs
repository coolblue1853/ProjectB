using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 투창 함정(판정 전용, 스프라이트 없음). 착지 지점에 원형 트리거 콜라이더를 두고 Duration 동안 Interval 마다 적에게 틱 피해.
/// 동시 상한 MaxLive 개(초과 시 가장 오래된 것을 회수), 비활성 오브젝트는 풀에 보관해 재사용.
/// </summary>
public sealed class SpearTrap : MonoBehaviour
{
    public const int MaxLive = 3;

    private static readonly Stack<SpearTrap> s_pool = new Stack<SpearTrap>(MaxLive);
    private static readonly List<SpearTrap> s_live = new List<SpearTrap>(MaxLive);
    private static readonly Collider2D[] s_buffer = new Collider2D[32];
    private static readonly List<EnemyHealth> s_hit = new List<EnemyHealth>(8);

    private CircleCollider2D _col;
    private GroundZoneRules _rules;
    private int _tickDamage;
    private float _until;

    public static int LiveCount => s_live.Count;
    public float ExpireTime => _until;

    public static SpearTrap Spawn(Vector3 pos, float radius, int tickDamage, float duration, float interval)
    {
        if (s_live.Count >= MaxLive)
        {
            s_live[0].Release();
        }
        SpearTrap t = null;
        while (s_pool.Count > 0 && t == null)
        {
            t = s_pool.Pop();
        }
        if (t == null)
        {
            var go = new GameObject("SpearTrap");
            t = go.AddComponent<SpearTrap>();
            t._col = go.AddComponent<CircleCollider2D>();
            t._col.isTrigger = true;
        }
        t.transform.position = pos;
        t._col.radius = radius;
        t._tickDamage = tickDamage;
        t._rules = new GroundZoneRules(duration, interval);
        t._until = Time.time + duration;
        t.gameObject.SetActive(true);
        s_live.Add(t);
        return t;
    }

    private void Update()
    {
        int ticks = _rules.Step(Time.deltaTime);
        for (int i = 0; i < ticks; i++)
        {
            Tick();
        }
        if (_rules.Expired)
        {
            Release();
        }
    }

    private void Tick()
    {
        int n = _col.Overlap(new ContactFilter2D().NoFilter(), s_buffer);
        s_hit.Clear();
        for (int i = 0; i < n; i++)
        {
            EnemyHealth e = s_buffer[i].GetComponentInParent<EnemyHealth>();
            if (e == null || !e.isActiveAndEnabled || e.nowHP <= 0 || s_hit.Contains(e))
            {
                continue;
            }
            s_hit.Add(e);
            e.onlyDamage2Enemy(_tickDamage);
        }
    }

    private void Release()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        s_live.Remove(this);
        gameObject.SetActive(false);
        s_pool.Push(this);
    }

    private void OnDestroy()
    {
        s_live.Remove(this);
    }
}
