using System;

/// <summary>
/// 고급 강화 +11~+15 와 정밀 단조 순수 규칙 (FloorCoreContent 4.1·4.2). 표·확률·점수 변환만 담고 난수는 주입받는다.
/// +10 이하는 <see cref="EnhanceRules"/> 가 그대로 맡는다(이 클래스는 현재 단계 10~14 만 다룬다).
/// </summary>
public static class AdvancedEnhanceRules
{
    public const int FromLevel = 10;      // 이 단계에서 시도하면 +11
    public const int MaxLevel = 15;
    public const int Strikes = 3;
    public const int MaxScore = Strikes * 3;
    public const int PityStepPct = 2;     // 연속 실패마다 +2%p
    public const int PityCapPct = 10;     // 최대 +10%p
    public const int RateCapPct = 90;     // 최종 성공률 상한
    public const int FullRefundScore = 9; // 이 점수면 실패 시 결정 50% 반환

    // 인덱스 = 현재 단계 - FromLevel (+10 → +11 시도는 [0])
    static readonly int[] BaseRatePct = { 65, 50, 35, 25, 15 };
    static readonly int[] StoneCost = { 5, 6, 7, 8, 8 };
    static readonly int[] GoldCost = { 1000, 1500, 2500, 4000, 6000 };

    // 정밀 단조 판정 오차(0~100 막대 위 거리 = 막대 길이 %)
    public const float PerfectHalf = 6f;
    public const float GreatHalf = 14f;
    public const float GoodHalf = 25f;

    /// <summary>현재 단계가 고급 강화 대상인가(10~14)</summary>
    public static bool IsAdvanced(int level)
    {
        return level >= FromLevel && level < MaxLevel;
    }

    public static int BasePct(int level)
    {
        return IsAdvanced(level) ? BaseRatePct[level - FromLevel] : 0;
    }

    public static int Stones(int level)
    {
        return IsAdvanced(level) ? StoneCost[level - FromLevel] : 0;
    }

    public static int Gold(int level)
    {
        return IsAdvanced(level) ? GoldCost[level - FromLevel] : 0;
    }

    /// <summary>천장 보조 %p: 연속 실패마다 +2, 최대 +10</summary>
    public static int PityPct(int failStreak)
    {
        return Math.Min(Math.Max(0, failStreak) * PityStepPct, PityCapPct);
    }

    /// <summary>정밀 단조 3타 점수 합(0~9) → 성공률 보너스 %p</summary>
    public static int ForgeBonusPct(int score)
    {
        if (score >= 9)
        {
            return 12;
        }
        if (score >= 7)
        {
            return 10;
        }
        if (score >= 5)
        {
            return 8;
        }
        return score >= 3 ? 4 : 0;
    }

    /// <summary>최종 성공률(0~1): 기본 + 천장 + 단조, 상한 90%</summary>
    public static float FinalRate(int level, int failStreak, int forgeScore)
    {
        int pct = BasePct(level) + PityPct(failStreak) + ForgeBonusPct(forgeScore);
        return Math.Min(pct, RateCapPct) / 100f;
    }

    /// <summary>실패 시 돌려받는 결정 수: 9점이면 50% 올림, 아니면 0</summary>
    public static int RefundStones(int stonesSpent, int forgeScore)
    {
        return forgeScore >= FullRefundScore ? (stonesSpent + 1) / 2 : 0;
    }

    /// <summary>한 번 두드린 판정 점수. cursor·target 은 0~100 막대 위치</summary>
    public static int StrikeScore(float cursor, float target)
    {
        float d = Math.Abs(cursor - target);
        if (d <= PerfectHalf)
        {
            return 3;
        }
        if (d <= GreatHalf)
        {
            return 2;
        }
        return d <= GoodHalf ? 1 : 0;
    }

    /// <param name="roll">0~1 난수</param>
    public static bool Roll(int level, float roll, int failStreak, int forgeScore)
    {
        return roll < FinalRate(level, failStreak, forgeScore);
    }
}
