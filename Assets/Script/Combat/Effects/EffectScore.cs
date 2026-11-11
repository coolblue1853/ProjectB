using System.Collections.Generic;

/// <summary>강도 점수·등급 예산 (EquipEffectDesign §3.1·§3.2, 순수). 밸런스 도구용 — 게임 동작에는 쓰지 않는다</summary>
public static class EffectScore
{
    public static float Budget(string grade)
    {
        switch (grade)
        {
            case "R": return 10f;
            case "U": return 24f;
            case "L": return 45f;
            default: return 0f;
        }
    }

    /// <summary>행 하나의 점수. 조건 있으면 ×0.6. Rule 은 소형 12·변형급 24(범위 중간값)</summary>
    public static float Of(EffectEntry e)
    {
        if (e == null)
        {
            return 0f;
        }
        float s;
        if (e.kind == EffectKind.Rule)
        {
            switch (e.rule)
            {
                case EffectRule.Chain:
                case EffectRule.Proc:
                case EffectRule.OnHitSpawn:
                case EffectRule.CooldownRefund:
                case EffectRule.Pierce:
                case EffectRule.Return:
                    s = 12f;
                    break;
                default:
                    s = 24f;
                    break;
            }
        }
        else
        {
            s = e.value * PerUnit(e.op);
        }
        return string.IsNullOrEmpty(e.cond) ? s : s * 0.6f;
    }

    static float PerUnit(EffectOp op)
    {
        switch (op)
        {
            case EffectOp.CooldownPct: return 0.5f;
            case EffectOp.DamagePct: return 0.4f;
            case EffectOp.RangePct:
            case EffectOp.ProjSpeedPct:
            case EffectOp.DurationPct: return 0.2f;
            case EffectOp.StaminaPct: return 0.15f;
            case EffectOp.ProjectileAdd:
            case EffectOp.HitCountAdd:
            case EffectOp.PierceAdd:
            case EffectOp.SummonAdd: return 4f;
            case EffectOp.CastCharge: return 8f;
            default: return 0f;
        }
    }

    /// <summary>효과 id 목록(';') 합계 점수. 모르는 id 는 0</summary>
    public static float OfIds(string effectIds)
    {
        float sum = 0f;
        foreach (string id in SkillEffectService.SplitIds(effectIds))
        {
            List<EffectEntry> l = EffectTable.Get(id);
            for (int i = 0; l != null && i < l.Count; i++)
            {
                sum += Of(l[i]);
            }
        }
        return sum;
    }

    /// <summary>등급 대비 판정: "none"(효과 없음 = 등급 기준 미달), "over"(예산 +30% 초과), "under"(-30% 미만), "ok", "n/a"(C·모르는 등급)</summary>
    public static string Judge(string grade, float score)
    {
        float b = Budget(grade);
        if (b <= 0f)
        {
            return "n/a";
        }
        if (score <= 0f)
        {
            return "none";
        }
        return score > b * 1.3f ? "over" : score < b * 0.7f ? "under" : "ok";
    }
}
