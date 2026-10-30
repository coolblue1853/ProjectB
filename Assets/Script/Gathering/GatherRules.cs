using System;

/// <summary>채집 미니게임 종류: ④ 생태 표본 채집 / ⑤ 절벽 광맥 채굴</summary>
public enum GatherKind { Specimen, Ore }

/// <summary>표본 채집 한 번 누름의 판정</summary>
public enum SpecimenJudge { Miss, Good, Perfect }

/// <summary>채집물·광석 정의. Id 는 ItemSheet.csv name(인벤 키). 희귀도는 공용 ItemRarity(C/R/U)</summary>
public readonly struct GatherDef
{
    public readonly string Id;
    public readonly ItemRarity Rarity;
    public readonly float BaseWeight;
    public readonly int MinCount;
    public readonly int MaxCount;

    public GatherDef(string id, ItemRarity rarity, float baseWeight, int minCount, int maxCount)
    {
        Id = id;
        Rarity = rarity;
        BaseWeight = baseWeight;
        MinCount = minCount;
        MaxCount = maxCount;
    }
}

/// <summary>
/// 표본 채집·광맥 채굴 순수 규칙·수치(MiniGames.md "채집·채굴 구현"). 출처: GPT 기획 대화(10-30) 수치표.
/// 난수는 호출자가 0~1 값으로 넘긴다(테스트 가능).
/// </summary>
public static class GatherRules
{
    // ── ④ 표본 채집: 0~100 왕복 커서, ↑ 로 멈춤 ──
    public const float CursorMax = 100f;
    public const float CursorSpeed = 85f;          // 초당
    public const float PerfectHalf = 5f;           // 목표 중심 ±5
    public const float GoodHalf = 15f;             // 목표 중심 ±15
    public const float TargetMin = 20f;            // 목표 중심 범위(임시 가정 Q-439: 양 끝을 피함)
    public const float TargetMax = 80f;
    public const int SpecimenAttempts = 3;
    public const float SpecimenTimeLimit = 12f;
    public const float MissPause = 0.4f;           // 빗나간 뒤 다음 시도까지(임시 가정 Q-439)
    public const int PerfectBonusCount = 1;
    public const float SpecimenCooldown = 90f;

    // ── ⑤ 광맥 채굴: 약점 발광 주기마다 정타 창 ──
    public const float GlowCycle = 1.5f;
    public const float GlowPeak = 0.75f;           // 주기 안에서 가장 밝은 순간(임시 가정 Q-439: 주기 가운데)
    public const float CritWindow = 0.2f;          // 발광 순간 ±0.2초
    public const int CrackGoal = 5;
    public const int CrackNormal = 1;
    public const int CrackCrit = 2;
    public const float MineTimeLimit = 15f;
    public const float SwingInterval = 0.3f;       // 연타 방지(임시 가정 Q-439)
    public const float OreCooldown = 120f;
    public const float CritRateHigh = 0.8f;
    public const float CritRateMid = 0.5f;

    /// <summary>층계 1 표본(숲). 가중치는 % 기준</summary>
    public static readonly GatherDef[] SpecimenFloor1 =
    {
        new GatherDef("GreenHurb", ItemRarity.C, 45f, 1, 2),
        new GatherDef("Forest Mushroom", ItemRarity.C, 35f, 1, 2),
        new GatherDef("Glow Moth Wing", ItemRarity.R, 17f, 1, 1),
        new GatherDef("Moonpetal", ItemRarity.U, 3f, 1, 1),
    };

    /// <summary>층계 1 광석. C 는 기존 강화 재료 T0_Ore 를 그대로 쓴다(Q-439)</summary>
    public static readonly GatherDef[] OreFloor1 =
    {
        new GatherDef("T0_Ore", ItemRarity.C, 80f, 1, 2),
        new GatherDef("Bog Copper Ore", ItemRarity.R, 17f, 1, 1),
        new GatherDef("Starfall Ore", ItemRarity.U, 3f, 1, 1),
    };

    /// <summary>층계별 표. 표가 없는 층계는 층계 1 표</summary>
    public static GatherDef[] TableFor(GatherKind kind, int floor)
    {
        return kind == GatherKind.Ore ? OreFloor1 : SpecimenFloor1;
    }

    public static float Cooldown(GatherKind kind)
    {
        return kind == GatherKind.Ore ? OreCooldown : SpecimenCooldown;
    }

    /// <summary>시간 t 의 커서 위치(0~100 왕복, 0 에서 출발)</summary>
    public static float CursorAt(float t)
    {
        float period = 2f * CursorMax / CursorSpeed;
        float p = t % period;
        if (p < 0f)
        {
            p += period;
        }
        float d = p * CursorSpeed;
        return d <= CursorMax ? d : 2f * CursorMax - d;
    }

    public static float TargetCenter(float roll01)
    {
        return TargetMin + (TargetMax - TargetMin) * Clamp01(roll01);
    }

    public static SpecimenJudge Judge(float cursor, float target)
    {
        float d = Math.Abs(cursor - target);
        if (d <= PerfectHalf)
        {
            return SpecimenJudge.Perfect;
        }
        return d <= GoodHalf ? SpecimenJudge.Good : SpecimenJudge.Miss;
    }

    /// <summary>약점이 지금 빛나는가(정타 창 안). t = 채굴 시작 뒤 경과 초</summary>
    public static bool IsCritTime(float t)
    {
        float p = t % GlowCycle;
        return Math.Abs(p - GlowPeak) <= CritWindow;
    }

    /// <summary>정타율에 따른 R/U 확률 보정(%p, C 에서 차감)</summary>
    public static void RareBonus(float critRate, out float rBonus, out float uBonus)
    {
        if (critRate >= CritRateHigh)
        {
            rBonus = 10f;
            uBonus = 3f;
        }
        else if (critRate >= CritRateMid)
        {
            rBonus = 5f;
            uBonus = 1f;
        }
        else
        {
            rBonus = 0f;
            uBonus = 0f;
        }
    }

    /// <summary>보정 뒤 실제 가중치. C 는 (R+U 보정 합 / C 종 수)만큼 줄어든다(0 미만 금지)</summary>
    public static float Weight(GatherDef[] table, int index, float rBonus, float uBonus)
    {
        GatherDef d = table[index];
        switch (d.Rarity)
        {
            case ItemRarity.R:
                return d.BaseWeight + rBonus;
            case ItemRarity.U:
                return d.BaseWeight + uBonus;
            default:
                int commons = 0;
                foreach (GatherDef g in table)
                {
                    if (g.Rarity == ItemRarity.C)
                    {
                        commons++;
                    }
                }
                return Math.Max(0f, d.BaseWeight - (rBonus + uBonus) / Math.Max(1, commons));
        }
    }

    /// <summary>표에서 하나를 고른다(정규화된 누적 확률). 표 인덱스를 돌려준다</summary>
    public static int Roll(GatherDef[] table, float roll01, float rBonus, float uBonus)
    {
        float total = 0f;
        for (int i = 0; i < table.Length; i++)
        {
            total += Weight(table, i, rBonus, uBonus);
        }
        float pick = Clamp01(roll01) * total;
        float acc = 0f;
        for (int i = 0; i < table.Length; i++)
        {
            acc += Weight(table, i, rBonus, uBonus);
            if (pick < acc)
            {
                return i;
            }
        }
        return table.Length - 1;
    }

    public static int RollCount(GatherDef def, float roll01)
    {
        int span = def.MaxCount - def.MinCount + 1;
        int n = def.MinCount + (int)(Clamp01(roll01) * span);
        return Math.Min(def.MaxCount, n);
    }

    private static float Clamp01(float v)
    {
        return v < 0f ? 0f : (v > 0.999999f ? 0.999999f : v);
    }
}
