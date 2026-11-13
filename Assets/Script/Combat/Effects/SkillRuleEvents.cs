using System.Collections.Generic;
using UnityEngine;

/// <summary>Rule 2차 순수 계산 (EditMode 테스트 대상, P4)</summary>
public static class RuleMath
{
    public const int ChainMaxTargets = 5;

    /// <summary>연쇄 n번째(0부터) 대상의 피해. 감쇠 %가 점프마다 곱으로 쌓이고 최소 1</summary>
    public static int ChainDamage(int baseDmg, float decayPct, int jump)
    {
        float keep = Mathf.Clamp01(1f - Mathf.Clamp(decayPct, 0f, 95f) / 100f);
        return Mathf.Max(1, Mathf.RoundToInt(baseDmg * Mathf.Pow(keep, jump + 1)));
    }

    /// <summary>피해 배열 복제 후 0·1 번(피해 값)만 pct% 로 배율(나머지는 확률 값이라 유지). 항상 새 배열</summary>
    public static int[] ScaleFirstTwo(int[] src, float pct)
    {
        if (src == null)
        {
            return null;
        }
        var r = (int[])src.Clone();
        float mul = pct / 100f;
        for (int i = 0; i < r.Length && i < 2; i++)
        {
            r[i] = Mathf.Max(1, Mathf.RoundToInt(src[i] * mul));
        }
        return r;
    }

    /// <summary>CooldownRefund 트리거 해석: "Kill"(기본)·"Hit". 모르는 값(Crit 등)은 null = 미지원</summary>
    public static string Trigger(string p2)
    {
        if (string.IsNullOrEmpty(p2) || string.Equals(p2, "Kill", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Kill";
        }
        return string.Equals(p2, "Hit", System.StringComparison.OrdinalIgnoreCase) ? "Hit" : null;
    }

    /// <summary>내부 쿨 확인: 지금이 다음 허용 시각 이상이면 true 이고 다음 시각을 갱신. cd 는 최소 0.3초(무한 연쇄 방지, 설계 §8-5)</summary>
    public static bool PassInternalCooldown(Dictionary<string, float> next, string key, float now, float cd)
    {
        if (next.TryGetValue(key, out float t) && now < t)
        {
            return false;
        }
        next[key] = now + Mathf.Max(0.3f, cd);
        return true;
    }

    /// <summary>
    /// CooldownRefund(Kill) 내부 쿨: p3(초)가 비면 항상 허용(기존 동작). 있으면 처치 한 번에 항목별로 한 번만 판정하고
    /// 그 결과를 같은 처치의 다른 스킬에도 쓴다(선택자 all = 모든 스킬 환급, 사냥꾼 부적). thisKill 은 처치마다 비운다
    /// </summary>
    public static bool RefundOnKill(Dictionary<string, float> next, Dictionary<string, bool> thisKill, string key, float now, string p3)
    {
        if (string.IsNullOrEmpty(p3))
        {
            return true;
        }
        if (thisKill.TryGetValue(key, out bool ok))
        {
            return ok;
        }
        if (!float.TryParse(p3, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float cd))
        {
            return true;
        }
        return RefundOnKill(next, thisKill, key, now, cd);
    }

    /// <summary>파싱·키 조립을 미리 해 둔 호출용(EffectEntry 캐시). cd 가 NaN 이면 내부 쿨 없음</summary>
    public static bool RefundOnKill(Dictionary<string, float> next, Dictionary<string, bool> thisKill, string key, float now, float cd)
    {
        if (float.IsNaN(cd))
        {
            return true;
        }
        if (thisKill.TryGetValue(key, out bool ok))
        {
            return ok;
        }
        ok = PassInternalCooldown(next, key, now, cd);
        thisKill[key] = ok;
        return ok;
    }

    /// <summary>스킬 이름 비교(공백 = 밑줄 동일 취급, 대소문자 무시)</summary>
    public static bool SameSkill(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
        {
            return false;
        }
        return string.Equals(a.Replace(' ', '_'), b.Replace(' ', '_'), System.StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Rule 2차 (EquipEffectDesign §2.3, P4): OnKillCast·Proc·CooldownRefund(Kill) 의 이벤트 처리. Chain·CooldownRefund(Hit)는 판정 리스너(RuleChain·RuleHitRefund).
/// 모든 연쇄 시전은 무료 시전(Skill.SpawnFree: 첫 판정 프리팹만, 규칙 재적용 없음)이라 다시 연쇄되지 않는다 + 항목별 내부 쿨 필수.
/// 효과가 하나도 없으면 아무 일도 하지 않는다.
/// </summary>
public static class SkillRuleEvents
{
    private static readonly Dictionary<string, float> Next = new Dictionary<string, float>();
    private static readonly List<EffectEntry> Seen = new List<EffectEntry>(8);
    private static readonly HashSet<string> Missing = new HashSet<string>();
    private static readonly Dictionary<string, bool> RefundThisKill = new Dictionary<string, bool>(4);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Hook()
    {
        EnemyHealth.Killed -= OnKilled;
        EnemyHealth.Killed += OnKilled;
        Skill.Cast -= OnCast;
        Skill.Cast += OnCast;
        Next.Clear();
        RefundThisKill.Clear();
        RuleHitRefund.ClearCooldowns();
    }

    public static void ClearCooldowns()
    {
        Next.Clear();
        RefundThisKill.Clear();
    }

    private static void OnKilled(EnemyHealth enemy)
    {
        Weapon w = Weapon.Equipped;
        if (enemy == null || w == null || w.skill == null || SkillEffectService.ActiveCount == 0)
        {
            return;
        }
        Seen.Clear();
        RefundThisKill.Clear();
        Vector3 pos = enemy.transform.position + Vector3.up * 0.2f;
        for (int i = 0; i < w.skill.Length; i++)
        {
            Skill s = w.skill[i];
            if (s == null || string.IsNullOrEmpty(s.skillName))
            {
                continue;
            }
            SkillMods m = SkillEffectService.Query(s.skillName, w.Kind);
            if (m.rules == null)
            {
                continue;
            }
            for (int k = 0; k < m.rules.Length; k++)
            {
                EffectEntry r = m.rules[k];
                if (r.rule == EffectRule.OnKillCast && !Seen.Contains(r))
                {
                    Seen.Add(r);
                    CastFree(w, r, pos, "OnKill:");
                }
                else if (r.rule == EffectRule.CooldownRefund && RuleMath.Trigger(r.p2) == "Kill"
                    && (string.IsNullOrEmpty(r.p3) || RuleMath.RefundOnKill(Next, RefundThisKill, r.RefundKey, Time.time, r.RefundCooldown)))
                {
                    Refund(s, SkillRuleHandlers.F(r.p1, 1f));
                }
            }
        }
    }

    private static void OnCast(Skill s)
    {
        Weapon w = Weapon.Equipped;
        if (s == null || w == null || SkillEffectService.ActiveCount == 0)
        {
            return;
        }
        SkillMods m = SkillEffectService.Query(s.skillName, w.Kind);
        if (m.rules == null)
        {
            return;
        }
        for (int k = 0; k < m.rules.Length; k++)
        {
            EffectEntry r = m.rules[k];
            if (r.rule != EffectRule.Proc)
            {
                continue;
            }
            float chance = SkillRuleHandlers.F(r.p2, 10f) + m.procChance;
            if (Random.value * 100f >= chance)
            {
                continue;
            }
            Vector3 pos = s.player != null ? s.player.transform.position : s.transform.position;
            float face = s.player != null ? Mathf.Sign(s.player.transform.localScale.x) : 1f;
            CastFree(w, r, pos, "Proc:", face);
        }
    }

    private static void CastFree(Weapon w, EffectEntry r, Vector3 pos, string tag, float face = 1f)
    {
        Skill target = FindSkill(w, r.p1);
        if (target == null)
        {
            if (Missing.Add(r.id + "#" + r.entry + "#" + r.p1)) // 같은 항목은 한 번만 경고(스팸 방지)
            {
                GameLog.Warn(GameLog.Cat.Combat, "장비 효과 " + r.id + ": 장착 무기에 스킬 '" + r.p1 + "' 없음(" + tag + ")");
            }
            return;
        }
        if (!RuleMath.PassInternalCooldown(Next, tag + r.id + "#" + r.entry, Time.time, SkillRuleHandlers.F(r.p3, 1f)))
        {
            return;
        }
        float dmgPct = tag == "OnKill:" ? SkillRuleHandlers.F(r.p2, 100f) : 100f; // OnKillCast p2=피해%, Proc p2=확률이라 피해는 100%
        target.SpawnFree(pos, dmgPct, face);
    }

    /// <summary>장착 무기의 스킬 중 이름이 같은 것. 없으면 null</summary>
    public static Skill FindSkill(Weapon w, string name)
    {
        if (w == null || w.skill == null)
        {
            return null;
        }
        for (int i = 0; i < w.skill.Length; i++)
        {
            if (w.skill[i] != null && RuleMath.SameSkill(w.skill[i].skillName, name))
            {
                return w.skill[i];
            }
        }
        return null;
    }

    public static void Refund(Skill s, float seconds)
    {
        if (s != null && SkillCooldown.instance != null && s.ActiveSlot >= 0)
        {
            SkillCooldown.instance.Refund(s.ActiveSlot, seconds);
        }
    }
}

/// <summary>Chain: 적중한 적 주변 N명에게 점프마다 감쇠된 피해를 전이 (p1=N≤5, p2=거리, p3=점프당 감쇠%). 연쇄 피해는 직접 피해라 재연쇄되지 않는다</summary>
public sealed class RuleChain : MonoBehaviour, IHitListener
{
    private static readonly EnemyHealth[] Pick = new EnemyHealth[RuleMath.ChainMaxTargets];
    private static readonly float[] PickDist = new float[RuleMath.ChainMaxTargets];
    private int _n = 2;
    private float _range = 4f;
    private float _decay = 25f;

    public void Init(DamageObject d, int n, float range, float decayPct)
    {
        _n = Mathf.Clamp(n, 1, RuleMath.ChainMaxTargets);
        _range = Mathf.Max(0.5f, range);
        _decay = decayPct;
        d.AddHitListener(this);
    }

    public void OnSkillHit(DamageObject hitbox, EnemyHealth enemy)
    {
        int[] src = hitbox.damageArr;
        if (enemy == null || src == null || src.Length == 0)
        {
            return;
        }
        int found = Nearest(enemy);
        for (int j = 0; j < found; j++)
        {
            int[] dmg = (int[])src.Clone();
            for (int i = 0; i < dmg.Length && i < 2; i++)
            {
                dmg[i] = RuleMath.ChainDamage(src[i], _decay, j);
            }
            EnemyHealth t = Pick[j];
            if (t != null && t.nowHP > 0)
            {
                t.damage2Enemy(dmg, 0f, 0f, Vector2.zero, enemy.transform.position.x, false, true, 0f, 1f);
            }
        }
    }

    /// <summary>가까운 순 최대 _n 명을 Pick 에 채우고 수를 돌려준다 (할당 없음)</summary>
    private int Nearest(EnemyHealth from)
    {
        int count = 0;
        Vector2 o = from.transform.position;
        foreach (EnemyHealth e in EnemyHealth.Active)
        {
            if (e == null || e == from || e.nowHP <= 0 || !e.IsMonster)
            {
                continue;
            }
            float dist = Vector2.Distance(o, e.transform.position);
            if (dist > _range || (count == _n && dist >= PickDist[count - 1]))
            {
                continue;
            }
            int p = count < _n ? count++ : count - 1;
            while (p > 0 && PickDist[p - 1] > dist)
            {
                Pick[p] = Pick[p - 1];
                PickDist[p] = PickDist[p - 1];
                p--;
            }
            Pick[p] = e;
            PickDist[p] = dist;
        }
        return count;
    }
}

/// <summary>CooldownRefund(Hit): 이 스킬 판정이 적을 맞힐 때마다 그 스킬의 남은 쿨을 줄인다 (내부 쿨 0.3초 이상)</summary>
public sealed class RuleHitRefund : MonoBehaviour, IHitListener
{
    private static readonly Dictionary<string, float> Next = new Dictionary<string, float>();
    private float _seconds = 1f;
    private string _key;

    public static void ClearCooldowns()
    {
        Next.Clear();
    }

    public void Init(DamageObject d, float seconds, string key)
    {
        _seconds = seconds;
        _key = key;
        d.AddHitListener(this);
    }

    public void OnSkillHit(DamageObject hitbox, EnemyHealth enemy)
    {
        if (!RuleMath.PassInternalCooldown(Next, _key, Time.time, 0.3f))
        {
            return;
        }
        SkillRuleEvents.Refund(SkillRuleEvents.FindSkill(Weapon.Equipped, hitbox.skillName), _seconds);
    }
}
