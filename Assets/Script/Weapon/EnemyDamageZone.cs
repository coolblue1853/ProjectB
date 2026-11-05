using UnityEngine;

/// <summary>
/// 플레이어 무기용 '적 대상 지속 피해 장판'(F2-AREA). Spawn 하면 반경 안의 적에게 GroundZoneRules 틱마다 피해.
/// 모양은 그리지 않는다(그래픽 보류 — 필요하면 기존 이펙트를 자식으로 붙인다). 틱 피해는 onlyDamage2Enemy(방어·치명 무시).
/// </summary>
public sealed class EnemyDamageZone : MonoBehaviour
{
    private static readonly Collider2D[] s_buffer = new Collider2D[32];

    private GroundZoneRules _rules;
    private float _radius;
    private int _tickDamage;
    private float _slowMul = 1f;
    private float _slowSeconds;
    private readonly System.Collections.Generic.List<EnemyHealth> _hit = new System.Collections.Generic.List<EnemyHealth>(8);

    public static EnemyDamageZone Spawn(Vector3 pos, float radius, int tickDamage, float duration = GroundZoneRules.DefaultDuration,
        float interval = GroundZoneRules.DefaultInterval, float slowMul = 1f, float slowSeconds = 0f)
    {
        var go = new GameObject("EnemyDamageZone");
        go.transform.position = pos;
        var z = go.AddComponent<EnemyDamageZone>();
        z._rules = new GroundZoneRules(duration, interval);
        z._radius = radius;
        z._tickDamage = tickDamage;
        z._slowMul = slowMul;
        z._slowSeconds = slowSeconds;
        return z;
    }

    private void Update()
    {
        if (_rules == null)
        {
            return;
        }
        int ticks = _rules.Step(Time.deltaTime);
        for (int i = 0; i < ticks; i++)
        {
            Tick();
        }
        if (_rules.Expired)
        {
            Destroy(gameObject);
        }
    }

    private void Tick()
    {
        int n = Physics2D.OverlapCircle(transform.position, _radius, new ContactFilter2D().NoFilter(), s_buffer);
        _hit.Clear();
        for (int i = 0; i < n; i++)
        {
            EnemyHealth e = s_buffer[i].GetComponentInParent<EnemyHealth>();
            if (e == null || !e.isActiveAndEnabled || e.nowHP <= 0 || _hit.Contains(e))
            {
                continue;
            }
            _hit.Add(e);
            e.onlyDamage2Enemy(_tickDamage);
            if (_slowSeconds > 0f)
            {
                e.ApplySlow(_slowMul, _slowSeconds);
            }
        }
    }
}
