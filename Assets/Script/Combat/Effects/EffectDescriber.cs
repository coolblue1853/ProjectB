using System;
using System.Globalization;
using System.Text;

/// <summary>
/// 효과 행 → 자동 설명 문장 (EquipEffectDesign §2.9). 템플릿은 영어 원문, 번역은 호출자가 tr(Loc.T)로 넘긴다. 순수 함수.
/// </summary>
public static class EffectDescriber
{
    /// <summary>{target} = 스킬 이름/태그/무기 종류. descKey 수동 문구가 있으면 manual(descKey) 우선</summary>
    public static string Describe(EffectEntry e, Func<string, string> tr = null, Func<string, string> manual = null)
    {
        if (e == null)
        {
            return "";
        }
        if (!string.IsNullOrEmpty(e.descKey) && manual != null)
        {
            string m = manual(e.descKey);
            if (!string.IsNullOrEmpty(m))
            {
                return m;
            }
        }
        string target = Target(e.selector, tr);
        string line = e.kind == EffectKind.Mod ? ModLine(e, target, tr) : RuleLine(e, target, tr);
        if (!string.IsNullOrEmpty(e.cond))
        {
            line += " (" + Tr(tr, "when") + " " + e.cond + ")";
        }
        return line;
    }

    static string Tr(Func<string, string> tr, string s)
    {
        return tr == null ? s : tr(s);
    }

    public static string Num(float v)
    {
        return v.ToString("0.##", CultureInfo.InvariantCulture);
    }

    public static string Target(string selector, Func<string, string> tr)
    {
        if (string.IsNullOrEmpty(selector))
        {
            return "";
        }
        var sb = new StringBuilder();
        string[] parts = selector.Split('|');
        for (int i = 0; i < parts.Length; i++)
        {
            string s = parts[i].Trim();
            if (i > 0)
            {
                sb.Append(" / ");
            }
            if (s == "all")
            {
                sb.Append(Tr(tr, "All skills"));
            }
            else if (s.StartsWith("tag:", StringComparison.Ordinal))
            {
                sb.Append(Tr(tr, s.Substring(4) + " skills"));
            }
            else if (s.StartsWith("kind:", StringComparison.Ordinal))
            {
                sb.Append(Tr(tr, s.Substring(5) + " skills"));
            }
            else
            {
                sb.Append(Tr(tr, SkillTagTable.Norm(s)));
            }
        }
        return sb.ToString();
    }

    static string ModLine(EffectEntry e, string target, Func<string, string> tr)
    {
        string v = Num(e.value);
        switch (e.op)
        {
            case EffectOp.CooldownPct: return Fmt(tr, "{0} cooldown -{1}%", target, v);
            case EffectOp.DamagePct: return Fmt(tr, "{0} damage +{1}%", target, v);
            case EffectOp.RangePct: return Fmt(tr, "{0} range +{1}%", target, v);
            case EffectOp.ProjSpeedPct: return Fmt(tr, "{0} projectile speed +{1}%", target, v);
            case EffectOp.DurationPct: return Fmt(tr, "{0} duration +{1}%", target, v);
            case EffectOp.ProjectileAdd: return Fmt(tr, "{0} projectiles +{1}", target, v);
            case EffectOp.HitCountAdd: return Fmt(tr, "{0} hits +{1}", target, v);
            case EffectOp.PierceAdd: return Fmt(tr, "{0} pierce +{1}", target, v);
            case EffectOp.SummonAdd: return Fmt(tr, "{0} summons +{1}", target, v);
            case EffectOp.StaminaPct: return Fmt(tr, "{0} stamina cost -{1}%", target, v);
            case EffectOp.ProcChance: return Fmt(tr, "{0} proc chance +{1}%", target, v);
            case EffectOp.CastCharge: return Fmt(tr, "{0} charges +{1}", target, v);
            default: return "";
        }
    }

    static string RuleLine(EffectEntry e, string target, Func<string, string> tr)
    {
        switch (e.rule)
        {
            case EffectRule.FireMode: return Fmt(tr, "{0} fire as {1} ({2} deg)", target, e.p1, e.p2);
            case EffectRule.Homing: return Fmt(tr, "{0} projectiles home in on enemies", target);
            case EffectRule.Retarget: return Fmt(tr, "{0} target {1}", target, e.p1);
            case EffectRule.OnHitSpawn: return Fmt(tr, "{0} on hit: {1} ({2}% damage)", target, e.p1, e.p2);
            case EffectRule.Pierce: return Fmt(tr, "{0} pierce {1} enemies", target, e.p1);
            case EffectRule.Return: return Fmt(tr, "{0} return at max range", target);
            case EffectRule.Chain: return Fmt(tr, "{0} chain to up to {1} nearby enemies", target, e.p1);
            case EffectRule.OnKillCast: return Fmt(tr, "{0} on kill: cast {1}", target, Tr(tr, SkillTagTable.Norm(e.p1)));
            case EffectRule.Proc: return Fmt(tr, "{0} may also cast {1} ({2}%)", target, Tr(tr, SkillTagTable.Norm(e.p1)), e.p2);
            case EffectRule.CooldownRefund: return Fmt(tr, "{0} cooldown -{1}s on {2}", target, e.p1, e.p2);
            case EffectRule.Replace: return Fmt(tr, "{0} replaced by variant {1}", target, e.p1);
            default: return "";
        }
    }

    static string Fmt(Func<string, string> tr, string template, params object[] args)
    {
        return string.Format(CultureInfo.InvariantCulture, Tr(tr, template), args);
    }
}
