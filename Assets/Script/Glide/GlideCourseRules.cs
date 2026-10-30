using UnityEngine;

public enum GlideGrade { C, B, A, S }

/// <summary>
/// 활공 코스 평가·보상 순수 규칙(FloorCoreContent.md 2절). C 완주 ×1 / B 기준 시간 안 ×1.2 / A 85% 안·추락 0 ×1.5 /
/// S 70% 안·추락 0·바람 문장 전부 ×2. 보상 = 기본 × 평가 배율(내림). 하루 첫 완주 100%, 이후 50%.
/// </summary>
public static class GlideCourseRules
{
    public const float RankAFraction = 0.85f;
    public const float RankSFraction = 0.70f;
    public const float RepeatMul = 0.5f;
    public const float DefaultDayLength = 300f;

    public static GlideGrade Grade(float seconds, float baseSeconds, int falls, int sentencesFound, int sentencesTotal)
    {
        if (seconds <= baseSeconds * RankSFraction + 1e-3f && falls == 0 && sentencesFound >= sentencesTotal)
        {
            return GlideGrade.S;
        }
        if (seconds <= baseSeconds * RankAFraction + 1e-3f && falls == 0)
        {
            return GlideGrade.A;
        }
        if (seconds <= baseSeconds)
        {
            return GlideGrade.B;
        }
        return GlideGrade.C;
    }

    public static float GradeMul(GlideGrade grade)
    {
        switch (grade)
        {
            case GlideGrade.S: return 2f;
            case GlideGrade.A: return 1.5f;
            case GlideGrade.B: return 1.2f;
            default: return 1f;
        }
    }

    /// <summary>보상 개수: floor(기본 × 평가 배율 × 반복 배율), 최소 1</summary>
    public static int Reward(int baseCount, GlideGrade grade, bool firstOfDay)
    {
        float v = baseCount * GradeMul(grade) * (firstOfDay ? 1f : RepeatMul);
        return Mathf.Max(1, Mathf.FloorToInt(v + 1e-4f));
    }

    /// <summary>게임 내 하루 번호(낮/밤 1주기 = dayLength 초)</summary>
    public static int DayIndex(float time, float dayLength)
    {
        return Mathf.FloorToInt(time / Mathf.Max(1f, dayLength));
    }
}
