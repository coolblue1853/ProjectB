using UnityEngine;

/// <summary>
/// 전투 중 판정(스킬창 등 "전투 중 차단" 용). 플레이어가 피격하거나 적을 타격한 시각을 기록하고
/// 마지막 교전 이후 <see cref="Window"/> 초 안이면 전투 중으로 본다(WeaponSkillProgression.md V.4 불가 조건).
/// </summary>
public static class CombatState
{
    /// <summary>마지막 교전 뒤 전투 상태가 유지되는 시간(초)</summary>
    public const float Window = 5f;

    static float _last = float.NegativeInfinity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _last = float.NegativeInfinity;
    }

    /// <summary>교전 발생(피격·타격). now = Time.unscaledTime</summary>
    public static void Mark(float now)
    {
        _last = now;
    }

    public static bool IsInCombat(float now)
    {
        return now - _last < Window;
    }

    public static bool IsInCombat()
    {
        return IsInCombat(Time.unscaledTime);
    }
}
