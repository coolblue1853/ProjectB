/// <summary>스킬 하나에 대한 효과 질의 결과 (EquipEffectDesign §2.4). 상한 적용 후 값</summary>
public struct SkillMods
{
    public float cooldownPct;
    public float damagePct;
    public float rangePct;
    public float projSpeedPct;
    public float durationPct;
    public float staminaPct;
    public float procChance;
    public int projectileAdd;
    public int hitCountAdd;
    public int pierceAdd;
    public int summonAdd;
    public int castCharge;
    /// <summary>적용되는 Rule 행 (핸들러는 P3~). 없으면 null</summary>
    public EffectEntry[] rules;
    /// <summary>슬롯 초과·Replace 우선으로 탈락한 Rule 행. 없으면 null</summary>
    public EffectEntry[] inactiveRules;
    /// <summary>상한에 걸려 값이 잘렸는지 (툴팁 "상한" 표시용)</summary>
    public bool capped;
}

/// <summary>장비 버킷 합산 상한 (EquipEffectDesign §2.7, 임시 가정)</summary>
public static class EffectCaps
{
    public const float CooldownPct = 60f;
    public const float DamagePct = 100f;
    public const float RangePct = 50f;
    public const float ProjSpeedPct = 60f;
    public const float DurationPct = 100f;
    public const float StaminaPct = 50f;
    public const float ProcChance = 40f;
    public const int ProjectileAdd = 6;
    public const int HitCountAdd = 4;
    public const int PierceAdd = 3;
    public const int SummonAdd = 3;
    public const int CastCharge = 2;
    /// <summary>공용·숙련 트리 쿨감 상한 (WeaponSkillProgression V.3)</summary>
    public const float TreeCooldownPct = 15f;

    /// <summary>탄 +N 상한: min(+6, 기본 탄 수의 100%). baseCount 가 0 이하면 +6 만 본다</summary>
    public static int ClampProjectileAdd(int add, int baseCount)
    {
        int cap = baseCount > 0 && baseCount < ProjectileAdd ? baseCount : ProjectileAdd;
        return add < 0 ? 0 : add > cap ? cap : add;
    }
}

/// <summary>쿨다운 계산 (순수)</summary>
public static class CooldownMath
{
    /// <summary>
    /// 장비 쿨감 % 적용. 감소가 없으면(0 이하) 기본값 그대로.
    /// 하한 0.5초(§2.7)는 수치 불변 검증 전이라 P1 에서는 끈다(minCooldown=0). 켜면 기본이 하한보다 작은 스킬은 기본값 유지.
    /// </summary>
    public static float Apply(float baseCooldown, float equipPct, float minCooldown = 0f)
    {
        if (equipPct <= 0f)
        {
            return baseCooldown;
        }
        float r = baseCooldown * (1f - equipPct / 100f);
        float floor = minCooldown < baseCooldown ? minCooldown : baseCooldown;
        return r < floor ? floor : r;
    }

    /// <summary>장비 %와 트리 %를 곱 결합한 실효 쿨감 %(소비자는 기존처럼 % 하나만 읽는다). 트리가 0 이하면 장비 값 그대로(장비 상한은 이미 적용돼 있음)</summary>
    public static float CombinedPct(float equipPct, float treePct)
    {
        if (treePct <= 0f)
        {
            return equipPct;
        }
        return (1f - Multiplier(equipPct, treePct)) * 100f;
    }

    /// <summary>장비 버킷과 트리 버킷의 곱 결합 배율 (60%·15% → 0.34)</summary>
    public static float Multiplier(float equipPct, float treePct)
    {
        float e = equipPct < 0f ? 0f : equipPct > EffectCaps.CooldownPct ? EffectCaps.CooldownPct : equipPct;
        float t = treePct < 0f ? 0f : treePct > EffectCaps.TreeCooldownPct ? EffectCaps.TreeCooldownPct : treePct;
        return (1f - e / 100f) * (1f - t / 100f);
    }
}
