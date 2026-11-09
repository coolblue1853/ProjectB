using UnityEngine;

/// <summary>
/// 스킬이 만든 판정 오브젝트에 장비 효과 수치(P2: 피해·범위·지속·탄 속도)를 입히고 스태미나 소모를 계산한다 (EquipEffectDesign §2.2).
/// 효과가 없으면(모든 값 0) 기존 SetDamge 와 완전히 같다 — 같은 배열 참조를 그대로 넘긴다.
/// </summary>
public static class SkillModApplier
{
    /// <summary>피해 배열 각 값에 (1+pct/100) 곱. pct 가 0 이하면 원본 참조 그대로</summary>
    public static int[] ScaleDamage(int[] src, float pct)
    {
        if (src == null || pct <= 0f)
        {
            return src;
        }
        var r = new int[src.Length];
        float mul = 1f + pct / 100f;
        for (int i = 0; i < src.Length; i++)
        {
            r[i] = Mathf.RoundToInt(src[i] * mul);
        }
        return r;
    }

    /// <summary>스태미나 소모 -% 적용(0 이하 감소면 원본). 결과는 0 이상</summary>
    public static int Stamina(int baseCost, float pct)
    {
        if (pct <= 0f || baseCost <= 0)
        {
            return baseCost;
        }
        return Mathf.Max(0, Mathf.RoundToInt(baseCost * (1f - pct / 100f)));
    }

    /// <summary>생성 직후(Start 전) 호출. 피해 배열 설정 + 범위·지속·탄 속도 배율</summary>
    public static void Apply(DamageObject d, int[] baseDamage, in SkillMods m)
    {
        d.SetDamge(ScaleDamage(baseDamage, m.damagePct));
        if (m.rangePct > 0f)
        {
            d.transform.localScale *= 1f + m.rangePct / 100f;
        }
        if (m.durationPct > 0f && d.isDestroyByTime)
        {
            d.holdingTime *= 1f + m.durationPct / 100f;
        }
        if (m.projSpeedPct > 0f)
        {
            float mul = 1f + m.projSpeedPct / 100f;
            d.launchForce *= mul;
            d.desiredSpeed *= mul;
        }
    }
}
