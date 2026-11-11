using System;
using System.Globalization;
using UnityEngine;

/// <summary>DamageObject 가 적을 때린 직후 불리는 리스너 (장비 효과 OnHitSpawn)</summary>
public interface IHitListener
{
    void OnSkillHit(DamageObject hitbox, EnemyHealth enemy);
}

/// <summary>
/// Rule 핸들러 1차 (EquipEffectDesign §2.3, P3): Homing·Pierce·Return·OnHitSpawn(Explode/Slow). FireMode 는 FirePatterns.
/// 코드 화이트리스트: 아래 switch 에 없는 rule 은 무시한다. 효과(rules)가 없으면 아무것도 붙지 않는다.
/// 스킬이 만든 판정 오브젝트 생성 직후(SkillModApplier.Apply, Start 전)에 호출된다.
/// </summary>
public static class SkillRuleHandlers
{
    public static float F(string s, float def)
    {
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : def;
    }

    public static void ApplyToProjectile(DamageObject d, in SkillMods m)
    {
        if (m.pierceAdd > 0 && d.isLaunch)
        {
            d.maxDamagedEnemies += m.pierceAdd; // PierceAdd: 탄이 추가로 뚫는 적 수
        }
        EffectEntry[] rules = m.rules;
        if (rules == null)
        {
            return;
        }
        for (int i = 0; i < rules.Length; i++)
        {
            EffectEntry e = rules[i];
            switch (e.rule)
            {
                case EffectRule.Homing:
                    if (d.isLaunch && !d.TryGetComponent(out RuleHoming _))
                    {
                        d.gameObject.AddComponent<RuleHoming>().Init(F(e.p1, 360f), F(e.p2, 9f));
                    }
                    break;
                case EffectRule.Pierce:
                    if (d.isLaunch)
                    {
                        d.maxDamagedEnemies += Mathf.Clamp((int)F(e.p1, 1f), 0, EffectCaps.PierceAdd);
                    }
                    break;
                case EffectRule.Return:
                    if (d.isLaunch && !d.TryGetComponent(out RuleReturn _))
                    {
                        d.gameObject.AddComponent<RuleReturn>().Init(d);
                    }
                    break;
                case EffectRule.OnHitSpawn:
                    AddHitSpawn(d, e);
                    break;
                case EffectRule.Chain:
                    if (!d.TryGetComponent(out RuleChain _))
                    {
                        d.gameObject.AddComponent<RuleChain>().Init(d, (int)F(e.p1, 2f), F(e.p2, 4f), F(e.p3, 25f));
                    }
                    break;
                case EffectRule.CooldownRefund:
                    if (RuleMath.Trigger(e.p2) == "Hit" && !d.TryGetComponent(out RuleHitRefund _))
                    {
                        d.gameObject.AddComponent<RuleHitRefund>().Init(d, F(e.p1, 1f), e.id + "#" + e.entry);
                    }
                    break;
                default:
                    break; // FireMode 는 발사부, OnKillCast·Proc·CooldownRefund(Kill)는 SkillRuleEvents
            }
        }
    }

    static void AddHitSpawn(DamageObject d, EffectEntry e)
    {
        if (!Enum.TryParse(e.p1, true, out RuleOnHit.Kind kind) || (kind != RuleOnHit.Kind.Explode && kind != RuleOnHit.Kind.Slow))
        {
            return; // Bleed·Poison 등은 후속
        }
        RuleOnHit comp = d.gameObject.AddComponent<RuleOnHit>();
        comp.Init(d, kind, F(e.p2, 50f), F(e.p3, kind == RuleOnHit.Kind.Explode ? 1.5f : 1f));
    }
}

/// <summary>Homing: 직선으로 날아가는 판정 탄이 가장 가까운 적 쪽으로 방향을 튼다 (속도 크기 유지)</summary>
public sealed class RuleHoming : MonoBehaviour
{
    float _turnRate = 360f;
    float _seekRadius = 9f;
    float _retarget;
    Transform _target;
    Collider2D _targetCol;
    Rigidbody2D _rb;

    public void Init(float turnRate, float seekRadius)
    {
        _turnRate = turnRate;
        _seekRadius = seekRadius;
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (_rb == null)
        {
            return;
        }
        Vector2 v = _rb.linearVelocity;
        float speed = v.magnitude;
        if (speed < 0.01f)
        {
            return;
        }
        _retarget -= Time.deltaTime;
        if (_retarget <= 0f || _target == null)
        {
            _retarget = 0.2f;
            _target = Nearest();
            _targetCol = null;
            if (_target != null)
            {
                _target.TryGetComponent(out _targetCol);
            }
        }
        if (_target == null)
        {
            return;
        }
        Vector2 aim = _targetCol != null ? (Vector2)_targetCol.bounds.center : (Vector2)_target.position;
        Vector2 want = (aim - (Vector2)transform.position).normalized;
        float a = Vector2.SignedAngle(v, want);
        float step = Mathf.Clamp(a, -_turnRate * Time.deltaTime, _turnRate * Time.deltaTime);
        Vector2 nv = (Vector2)(Quaternion.Euler(0f, 0f, step) * v.normalized) * speed;
        _rb.linearVelocity = nv;
        transform.up = nv.normalized;
    }

    Transform Nearest()
    {
        Transform best = null;
        float bestD = _seekRadius;
        foreach (EnemyHealth e in EnemyHealth.Active)
        {
            if (e == null || e.nowHP <= 0 || !e.IsMonster)
            {
                continue;
            }
            float dist = Vector2.Distance(e.transform.position, transform.position);
            if (dist < bestD)
            {
                bestD = dist;
                best = e.transform;
            }
        }
        return best;
    }
}

/// <summary>Return: 수명의 절반 지점에서 속도를 뒤집어 되돌아오고, 이미 맞힌 적도 다시 맞힌다. 수명은 2배</summary>
public sealed class RuleReturn : MonoBehaviour
{
    DamageObject _d;
    Rigidbody2D _rb;
    float _delay = 0.5f;
    float _age;
    bool _done;

    public void Init(DamageObject d)
    {
        _d = d;
        float life = d.isDestroyByTime && d.holdingTime > 0f ? d.holdingTime / StatMath.AttackSpeedMul : 1f;
        _delay = life;
        if (d.isDestroyByTime && d.holdingTime > 0f)
        {
            d.holdingTime *= 2f; // 왕복이 들어가도록
        }
        d.maxDamagedEnemies += 99; // 첫 적중에 사라지지 않게(수명으로 끝)
        _rb = d.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (_done || _rb == null)
        {
            return;
        }
        _age += Time.deltaTime;
        if (_age >= _delay)
        {
            _done = true;
            _rb.linearVelocity = -_rb.linearVelocity;
            if (_rb.linearVelocity.sqrMagnitude > 0.0001f)
            {
                transform.up = _rb.linearVelocity.normalized;
            }
            _d.ResetStruck();
        }
    }
}

/// <summary>OnHitSpawn: 적중 시 작은 폭발(Explode: p2=피해%, p3=반경) 또는 둔화(Slow: p2=감속%, p3=초)</summary>
public sealed class RuleOnHit : MonoBehaviour, IHitListener
{
    public enum Kind
    {
        Explode,
        Slow,
        Bleed,
        Poison,
    }

    static readonly Collider2D[] Buf = new Collider2D[24];
    Kind _kind;
    float _p2;
    float _p3;

    public void Init(DamageObject d, Kind kind, float p2, float p3)
    {
        _kind = kind;
        _p2 = p2;
        _p3 = p3;
        d.AddHitListener(this);
    }

    public void OnSkillHit(DamageObject hitbox, EnemyHealth enemy)
    {
        if (enemy == null)
        {
            return;
        }
        if (_kind == Kind.Slow)
        {
            enemy.ApplySlow(Mathf.Clamp01(1f - _p2 / 100f), _p3);
            return;
        }
        int[] src = hitbox.damageArr;
        if (src == null || src.Length == 0)
        {
            return;
        }
        int[] dmg = (int[])src.Clone();
        float mul = _p2 / 100f;
        for (int i = 0; i < dmg.Length && i < 2; i++)
        {
            dmg[i] = Mathf.Max(1, Mathf.RoundToInt(src[i] * mul)); // 0·1 번만 피해(나머지는 확률 값)
        }
        int n = Physics2D.OverlapCircleNonAlloc(enemy.transform.position, _p3, Buf);
        for (int i = 0; i < n; i++)
        {
            Collider2D c = Buf[i];
            if (!c.CompareTag("Enemy") || !c.TryGetComponent(out EnemyHealth other) || other == enemy || other.nowHP <= 0 || !other.IsMonster)
            {
                continue;
            }
            other.damage2Enemy(dmg, 0f, 0f, Vector2.zero, enemy.transform.position.x, false, true, 0f, 1f);
        }
    }
}
