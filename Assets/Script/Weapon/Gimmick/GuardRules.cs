using UnityEngine;

/// <summary>
/// 홀드 방어 순수 규칙(F2-GUARD 공용, Floor2Plan.md §5.5 #1·#8). 방어 중이면 피해를 ReduceRatio 만큼 줄인다.
/// 값은 임시 가정(OpenQuestions Q-AUTO15). 반사·패링은 AUTO-17 에서 이 클래스에 얹는다.
/// </summary>
public static class GuardRules
{
    /// <summary>방어 중 피해 감소율(0.7 = 70% 막음)</summary>
    public const float ReduceRatio = 0.7f;

    /// <summary>방어 결과 피해. 방어 아님/피해 0 이하면 그대로, 방어 중 양수 피해는 최소 1.</summary>
    public static int Resolve(int damage, bool holding, float reduceRatio = ReduceRatio)
    {
        if (!holding || damage <= 0)
        {
            return damage;
        }
        int taken = Mathf.RoundToInt(damage * (1f - Mathf.Clamp01(reduceRatio)));
        return Mathf.Max(1, taken);
    }
}
