using UnityEngine;

/// <summary>
/// 임시 몹 원거리 사격 프로필(MonsterSheet trait 열). shoot = 숲 사냥꾼의 직선 화살, rune = 유적 파수꾼의 느리고 약한 룬 사격.
/// 순수 규칙: 사거리·수직 한계·쿨·준비 시간·피해 환산. 초반 몹은 소박하게(피해 = 몸박 x 배율).
/// </summary>
public static class MobShootRules
{
    public readonly struct Profile
    {
        public readonly float MinRange, MaxRange, Cooldown, Windup, DamageMul;

        public Profile(float minRange, float maxRange, float cooldown, float windup, float damageMul)
        {
            MinRange = minRange;
            MaxRange = maxRange;
            Cooldown = cooldown;
            Windup = windup;
            DamageMul = damageMul;
        }
    }

    // 최소 거리 안쪽은 기존 근접 BT 가 맡는다. 수직 한계는 RangedAttackRules.MaxVerticalGap(U16) 을 그대로 쓴다.
    public static readonly Profile Shoot = new Profile(2f, 8f, 3.2f, 0.45f, 0.5f);
    public static readonly Profile Rune = new Profile(2.5f, 6f, 4f, 0.7f, 0.6f);

    /// <summary>trait 문자열에서 사격 프로필을 찾는다. shoot 이 rune 보다 우선, 둘 다 없으면 false.</summary>
    public static bool TryGetProfile(string traits, out Profile profile)
    {
        if (TempMobTraits.Has(traits, "shoot"))
        {
            profile = Shoot;
            return true;
        }
        if (TempMobTraits.Has(traits, "rune"))
        {
            profile = Rune;
            return true;
        }
        profile = default;
        return false;
    }

    /// <summary>사격을 시작해도 되는가(dx/dy = 플레이어 - 몹). 거리는 최소·최대 포함, 수직은 한계 이내, 쿨이 끝나야 한다.</summary>
    public static bool CanStart(Profile p, float dx, float dy, bool cooldownReady)
    {
        float ax = Mathf.Abs(dx);
        return cooldownReady && ax >= p.MinRange && ax <= p.MaxRange && RangedAttackRules.InReach(0f, dy);
    }

    /// <summary>사격선이 지형에 막혔는가. hitDistance = 지형 레이캐스트 적중 거리(없으면 음수), shotDistance = 몹→플레이어 거리.</summary>
    public static bool IsBlocked(float hitDistance, float shotDistance)
    {
        return hitDistance >= 0f && hitDistance < shotDistance;
    }

    /// <summary>화살 피해 = 몸박 기준값 x 배율(반올림, 최소 1). 몸박 값이 없으면 0(사격 안 함).</summary>
    public static int ShotDamage(int contactBase, float mul)
    {
        return contactBase <= 0 ? 0 : Mathf.Max(1, Mathf.RoundToInt(contactBase * mul));
    }
}
