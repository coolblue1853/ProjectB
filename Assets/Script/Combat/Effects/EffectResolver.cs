using System;
using System.Collections.Generic;

/// <summary>활성 효과 행 + 출처 정보 (충돌 우선순위용)</summary>
public struct ActiveEffect
{
    public EffectEntry entry;
    /// <summary>등급 순위: C=0 R=1 U=2 L=3</summary>
    public int gradeRank;
    /// <summary>장착 순서(클수록 나중)</summary>
    public int order;
}

/// <summary>
/// 합산·상한·충돌 해소 (순수, EquipEffectDesign §2.6·2.7). 유니티 의존 없음 → EditMode 테스트.
/// </summary>
public static class EffectResolver
{
    public static int GradeRank(string grade)
    {
        switch (grade)
        {
            case "R": return 1;
            case "U": return 2;
            case "L": return 3;
            default: return 0;
        }
    }

    /// <summary>평타 피해 +% 합계: 선택자가 "Basic" 또는 "tag:Basic" 인 조건 없는 DamagePct 행만(all 은 평타 제외). 상한 적용</summary>
    public static float BasicDamagePct(IList<ActiveEffect> active)
    {
        float sum = 0f;
        for (int i = 0; active != null && i < active.Count; i++)
        {
            EffectEntry e = active[i].entry;
            if (e.kind != EffectKind.Mod || e.op != EffectOp.DamagePct || !string.IsNullOrEmpty(e.cond) || string.IsNullOrEmpty(e.selector))
            {
                continue;
            }
            foreach (string part in e.selector.Split('|'))
            {
                string p = part.Trim();
                if (p == "Basic" || p == "tag:Basic")
                {
                    sum += e.value;
                    break;
                }
            }
        }
        return sum < 0f ? 0f : sum > EffectCaps.DamagePct ? EffectCaps.DamagePct : sum;
    }

    /// <summary>선택자 일치. "all", 스킬 이름, "tag:X", "kind:X", '|' 로 OR. 스킬 이름은 _ ↔ 공백 무시</summary>
    public static bool Matches(string selector, string skillName, string kind, Func<string, string, bool> hasTag)
    {
        if (string.IsNullOrEmpty(selector))
        {
            return false;
        }
        string[] parts = selector.Split('|');
        for (int i = 0; i < parts.Length; i++)
        {
            string s = parts[i].Trim();
            if (s == "all")
            {
                return true;
            }
            if (s.StartsWith("tag:", StringComparison.Ordinal))
            {
                if (hasTag != null && hasTag(skillName, s.Substring(4)))
                {
                    return true;
                }
            }
            else if (s.StartsWith("kind:", StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(kind) && string.Equals(s.Substring(5), kind, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            else if (string.Equals(SkillTagTable.Norm(s), SkillTagTable.Norm(skillName), StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 스킬 하나의 최종 효과. legacy* 는 레거시 Dictionary 값(장비 버킷에 더해 같은 상한을 통과).
    /// condEval 이 null 이면 조건 있는 행은 비활성.
    /// </summary>
    public static SkillMods Resolve(IList<ActiveEffect> active, string skillName, string kind, int legacyCooldown, int legacyHit, int legacyBullet,
        Func<string, string, bool> hasTag, Func<string, bool> condEval)
    {
        if (active == null || active.Count == 0)
        {
            // 활성 행이 없으면(P1 현재 전부) 할당 없이 레거시 값만 상한 통과
            var f = new SkillMods();
            bool c = false;
            f.cooldownPct = Clamp(legacyCooldown, EffectCaps.CooldownPct, ref c);
            f.hitCountAdd = (int)Clamp(legacyHit, EffectCaps.HitCountAdd, ref c);
            f.projectileAdd = (int)Clamp(legacyBullet, EffectCaps.ProjectileAdd, ref c);
            f.capped = c;
            return f;
        }
        float[] pct = new float[16];
        var best = new Dictionary<string, float>(); // group|op → 최댓값
        var rules = new List<ActiveEffect>();
        if (active != null)
        {
            for (int i = 0; i < active.Count; i++)
            {
                EffectEntry e = active[i].entry;
                if (!Matches(e.selector, skillName, kind, hasTag) || (!string.IsNullOrEmpty(e.cond) && (condEval == null || !condEval(e.cond))))
                {
                    continue;
                }
                if (e.kind == EffectKind.Rule)
                {
                    rules.Add(active[i]);
                }
                else if (string.IsNullOrEmpty(e.group))
                {
                    pct[(int)e.op] += e.value;
                }
                else
                {
                    string key = e.group + "|" + (int)e.op;
                    if (!best.TryGetValue(key, out float old) || e.value > old)
                    {
                        best[key] = e.value;
                    }
                }
            }
            foreach (KeyValuePair<string, float> kv in best)
            {
                pct[int.Parse(kv.Key.Substring(kv.Key.IndexOf('|') + 1))] += kv.Value;
            }
        }
        pct[(int)EffectOp.CooldownPct] += legacyCooldown;
        pct[(int)EffectOp.HitCountAdd] += legacyHit;
        pct[(int)EffectOp.ProjectileAdd] += legacyBullet;

        var m = new SkillMods();
        bool cap = false;
        m.cooldownPct = Clamp(pct[(int)EffectOp.CooldownPct], EffectCaps.CooldownPct, ref cap);
        m.damagePct = Clamp(pct[(int)EffectOp.DamagePct], EffectCaps.DamagePct, ref cap);
        m.rangePct = Clamp(pct[(int)EffectOp.RangePct], EffectCaps.RangePct, ref cap);
        m.projSpeedPct = Clamp(pct[(int)EffectOp.ProjSpeedPct], EffectCaps.ProjSpeedPct, ref cap);
        m.durationPct = Clamp(pct[(int)EffectOp.DurationPct], EffectCaps.DurationPct, ref cap);
        m.staminaPct = Clamp(pct[(int)EffectOp.StaminaPct], EffectCaps.StaminaPct, ref cap);
        m.procChance = Clamp(pct[(int)EffectOp.ProcChance], EffectCaps.ProcChance, ref cap);
        m.projectileAdd = (int)Clamp(pct[(int)EffectOp.ProjectileAdd], EffectCaps.ProjectileAdd, ref cap);
        m.hitCountAdd = (int)Clamp(pct[(int)EffectOp.HitCountAdd], EffectCaps.HitCountAdd, ref cap);
        m.pierceAdd = (int)Clamp(pct[(int)EffectOp.PierceAdd], EffectCaps.PierceAdd, ref cap);
        m.summonAdd = (int)Clamp(pct[(int)EffectOp.SummonAdd], EffectCaps.SummonAdd, ref cap);
        m.castCharge = (int)Clamp(pct[(int)EffectOp.CastCharge], EffectCaps.CastCharge, ref cap);
        m.capped = cap;
        if (rules.Count > 0)
        {
            ResolveRules(rules, ref m);
        }
        return m;
    }

    static float Clamp(float v, float max, ref bool capped)
    {
        if (v < 0f)
        {
            return 0f;
        }
        if (v > max)
        {
            capped = true;
            return max;
        }
        return v;
    }

    // 슬롯 분류: 0=FireMode 1개, 1=Retarget/Homing 합쳐 1개, 2=Replace 1개, 3=Chain·Proc·OnHitSpawn·OnKillCast 합쳐 2개, -1=제한 없음
    static int SlotOf(EffectRule r)
    {
        switch (r)
        {
            case EffectRule.FireMode: return 0;
            case EffectRule.Retarget:
            case EffectRule.Homing: return 1;
            case EffectRule.Replace: return 2;
            case EffectRule.Chain:
            case EffectRule.Proc:
            case EffectRule.OnHitSpawn:
            case EffectRule.OnKillCast: return 3;
            default: return -1;
        }
    }

    static readonly int[] SlotLimit = { 1, 1, 1, 2 };

    static void ResolveRules(List<ActiveEffect> rules, ref SkillMods m)
    {
        // 우선순위: 등급 높은 쪽 > 같은 등급이면 장착 순서가 나중인 쪽
        rules.Sort((a, b) => a.gradeRank != b.gradeRank ? b.gradeRank.CompareTo(a.gradeRank) : b.order.CompareTo(a.order));
        bool hasReplace = false;
        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i].entry.rule == EffectRule.Replace)
            {
                hasReplace = true;
                break;
            }
        }
        var on = new List<EffectEntry>();
        var off = new List<EffectEntry>();
        var groupSeen = new HashSet<string>();
        int[] used = new int[4];
        for (int i = 0; i < rules.Count; i++)
        {
            EffectEntry e = rules[i].entry;
            int slot = SlotOf(e.rule);
            bool drop = false;
            if (hasReplace && e.rule == EffectRule.FireMode)
            {
                drop = true; // 변형 프리팹이 FireMode 를 포함할 수 있어 변형 우선
            }
            else if (!string.IsNullOrEmpty(e.group) && !groupSeen.Add(e.group + "|R"))
            {
                drop = true; // 같은 그룹은 우선순위 1개만
            }
            else if (slot >= 0)
            {
                if (used[slot] >= SlotLimit[slot])
                {
                    drop = true;
                }
                else
                {
                    used[slot]++;
                }
            }
            (drop ? off : on).Add(e);
        }
        m.rules = on.Count > 0 ? on.ToArray() : null;
        m.inactiveRules = off.Count > 0 ? off.ToArray() : null;
    }
}
