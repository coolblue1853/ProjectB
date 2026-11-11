using System;

/// <summary>밤 전용 스폰 몹이 목표인 퀘스트에 "밤까지 약 N분" 안내 한 줄을 만든다(EarlyHourPlan 밤 의존 UX). 판정은 순수 함수.</summary>
public static class QuestNightHint
{
    /// <summary>밤에만 스폰되는 처치 목표 몬스터 id (샛길 허수아비·일반 숲 도깨비불). 낮에도 나오는 몹은 넣지 않는다.</summary>
    private static readonly string[] NightOnlyKillTargets = { "Scarecrow_BD", "Wisp_BD" };

    /// <summary>처치 목표 전부가 밤 전용 몹이면 true. "a|b" 목록은 하나라도 낮에 나오면 false.</summary>
    public static bool IsNightOnly(NpcQuestDef d)
    {
        if (d == null || d.Objective != NpcQuestObjective.Kill || string.IsNullOrEmpty(d.Target))
        {
            return false;
        }
        foreach (string t in d.Target.Split('|'))
        {
            if (Array.IndexOf(NightOnlyKillTargets, t.Trim()) < 0)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>낮일 때 밤 시작까지 남은 초. 이미 밤이거나 값이 이상하면 -1.
    /// value = 하루 진행(0~1), 밤 = (nightStart, nightEnd) 구간(TimeChange: 낮 = value &lt;= start 또는 value &gt;= end).</summary>
    public static float SecondsUntilNight(float value, float totalSeconds, float nightStart, float nightEnd)
    {
        if (totalSeconds <= 0f || nightStart <= 0f || nightStart >= nightEnd || nightEnd > 1f)
        {
            return -1f;
        }
        value = Math.Max(0f, Math.Min(1f, value));
        if (value > nightStart && value < nightEnd)
        {
            return -1f;
        }
        float frac = value <= nightStart ? nightStart - value : 1f - value + nightStart;
        return frac * totalSeconds;
    }

    /// <summary>안내 한 줄. 목표 미완료 + 밤 전용 + 지금 낮일 때만 나온다(아니면 빈 문자열). 1분 미만은 "약 1분".</summary>
    public static string Line(NpcQuestDef d, bool ready, float secondsUntilNight)
    {
        if (ready || secondsUntilNight < 0f || !IsNightOnly(d))
        {
            return "";
        }
        int minutes = Math.Max(1, (int)Math.Ceiling(secondsUntilNight / 60f));
        return "밤까지 약 " + minutes + "분";
    }

    /// <summary>현재 시각(TimeChange)으로 계산한 남은 초. 낮 고정·시계 없음이면 -1(안내 숨김).</summary>
    public static float SecondsUntilNightNow()
    {
        TimeChange tc = TimeChange.instance;
        if (tc == null || tc.LockDaytime || !tc.isDaytime)
        {
            return -1f;
        }
        return SecondsUntilNight(tc.value, tc.totalTime, tc.startNighttime, tc.endNighttime);
    }
}
