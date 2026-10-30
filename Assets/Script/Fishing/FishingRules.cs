using System;

/// <summary>캐스팅 품질: 짧음(0.3초 미만) / 보통 / 최적(0.65~0.95초)</summary>
public enum CastQuality { Short, Normal, Optimal }

/// <summary>어종 정의. Id 는 ItemSheet.csv name(인벤 키)</summary>
public readonly struct FishDef
{
    public readonly string Id;
    public readonly ItemRarity Rarity;
    public readonly float BaseWeight;
    public readonly int MinCount;
    public readonly int MaxCount;

    public FishDef(string id, ItemRarity rarity, float baseWeight, int minCount, int maxCount)
    {
        Id = id;
        Rarity = rarity;
        BaseWeight = baseWeight;
        MinCount = minCount;
        MaxCount = maxCount;
    }
}

/// <summary>낚싯대 등급 효과(안전 구간 양쪽 확장·대기 시간 배율·R/U 가중치 배율)</summary>
public readonly struct RodDef
{
    public readonly string Name;
    public readonly float SafeBonus;
    public readonly float WaitMul;
    public readonly float RareWeightMul;

    public RodDef(string name, float safeBonus, float waitMul, float rareWeightMul)
    {
        Name = name;
        SafeBonus = safeBonus;
        WaitMul = waitMul;
        RareWeightMul = rareWeightMul;
    }
}

/// <summary>
/// 층계 낚시 순수 규칙·수치(MiniGames.md "층계 낚시 구현"). 출처: GPT 기획 대화(10-30) 수치표.
/// 난수는 호출자가 0~1 값으로 넘긴다(테스트 가능).
/// </summary>
public static class FishingRules
{
    // ── 캐스팅 ──
    public const float CastMin = 0.3f;
    public const float CastMax = 1.2f;
    public const float CastOptimalMin = 0.65f;
    public const float CastOptimalMax = 0.95f;
    public const float OptimalWaitMul = 0.9f;
    public const float ShortWaitMul = 1.3f;          // 임시 가정(Q-436): 짧은 캐스팅은 대기 +30%
    public const float OptimalRareMul = 1.2f;        // 임시 가정(Q-436): "희귀 가중치 소폭" = ×1.2

    // ── 대기·입질 ──
    public const float WaitMin = 3f;
    public const float WaitMax = 8f;
    public const float BiteWindow = 0.6f;

    // ── 장력 ──
    public const float TensionStart = 40f;
    public const float TensionMax = 100f;
    public const float HoldRate = 18f;
    public const float ReleaseRate = -24f;
    public const float ResistCycle = 3f;
    public const float ResistDuration = 0.8f;
    public const float ProgressInSafe = 10f;
    public const float ProgressOutSafe = -2f;
    public const float SlackTension = 8f;
    public const float SlackLimit = 2f;
    public const float SnapTension = 90f;
    public const float SnapLimit = 1f;
    public const float ProgressGoal = 100f;
    public const float ReelTimeLimit = 35f;

    /// <summary>층계 1 어종(숲 수계). 가중치는 % 기준, 롤 때 다시 정규화</summary>
    public static readonly FishDef[] Floor1 =
    {
        new FishDef("Forest Carp", ItemRarity.C, 50f, 1, 2),
        new FishDef("Moss Loach", ItemRarity.C, 30f, 1, 2),
        new FishDef("Amber Trout", ItemRarity.R, 17f, 1, 1),
        new FishDef("Spirit Koi", ItemRarity.U, 3f, 1, 1),
    };

    /// <summary>낚싯대 3등급. 0 = 낡은 낚싯대(기본)</summary>
    public static readonly RodDef[] Rods =
    {
        new RodDef("Old Rod", 0f, 1f, 1f),
        new RodDef("Explorer Rod", 5f, 0.9f, 1.25f),
        new RodDef("Skyreach Rod", 10f, 0.8f, 1.5f),
    };

    public static RodDef Rod(int grade)
    {
        return Rods[Math.Max(0, Math.Min(Rods.Length - 1, grade))];
    }

    /// <summary>층계별 어종 표. 표가 없는 층계는 층계 1 표(Q-436)</summary>
    public static FishDef[] TableFor(int floor)
    {
        return Floor1;
    }

    public static CastQuality EvaluateCast(float holdSeconds)
    {
        if (holdSeconds < CastMin)
        {
            return CastQuality.Short;
        }
        if (holdSeconds >= CastOptimalMin && holdSeconds <= CastOptimalMax)
        {
            return CastQuality.Optimal;
        }
        return CastQuality.Normal;
    }

    /// <summary>입질까지 대기 시간(초). roll01 = 0~1 난수</summary>
    public static float WaitSeconds(float roll01, CastQuality quality, RodDef rod)
    {
        float t = WaitMin + (WaitMax - WaitMin) * Clamp01(roll01);
        if (quality == CastQuality.Optimal)
        {
            t *= OptimalWaitMul;
        }
        else if (quality == CastQuality.Short)
        {
            t *= ShortWaitMul;
        }
        return t * rod.WaitMul;
    }

    /// <summary>가중치 적용 뒤의 실제 가중치(R/U 만 낚싯대·최적 캐스팅 배율)</summary>
    public static float Weight(FishDef fish, RodDef rod, CastQuality quality)
    {
        if (fish.Rarity == ItemRarity.C)
        {
            return fish.BaseWeight;
        }
        float mul = rod.RareWeightMul * (quality == CastQuality.Optimal ? OptimalRareMul : 1f);
        return fish.BaseWeight * mul;
    }

    /// <summary>어종 표에서 하나를 고른다(정규화된 누적 확률). 표 인덱스를 돌려준다</summary>
    public static int RollFish(FishDef[] table, float roll01, RodDef rod, CastQuality quality, float rarePoints = 0f)
    {
        var weights = new float[table.Length];
        float total = 0f;
        float commons = 0f;
        float rares = 0f;
        for (int i = 0; i < table.Length; i++)
        {
            weights[i] = Weight(table[i], rod, quality);
            total += weights[i];
            if (table[i].Rarity == ItemRarity.C)
            {
                commons += weights[i];
            }
            else
            {
                rares += weights[i];
            }
        }
        // 요리 버프(희귀 확률 +n%p): 전체의 n% 만큼을 일반 어종에서 R/U 로 옮긴다(R/U 끼리는 기존 비율 유지)
        float shift = Math.Min(commons, total * Math.Max(0f, rarePoints) / 100f);
        if (shift > 0f && rares > 0f)
        {
            for (int i = 0; i < table.Length; i++)
            {
                weights[i] *= table[i].Rarity == ItemRarity.C ? (commons - shift) / commons : (rares + shift) / rares;
            }
        }
        float pick = Clamp01(roll01) * total;
        float acc = 0f;
        for (int i = 0; i < table.Length; i++)
        {
            acc += weights[i];
            if (pick < acc)
            {
                return i;
            }
        }
        return table.Length - 1;
    }

    public static int RollCount(FishDef fish, float roll01)
    {
        int span = fish.MaxCount - fish.MinCount + 1;
        int n = fish.MinCount + (int)(Clamp01(roll01) * span);
        return Math.Min(fish.MaxCount, n);
    }

    /// <summary>희귀도별 안전 장력 구간 + 낚싯대 확장(양쪽)</summary>
    public static void SafeZone(ItemRarity rarity, RodDef rod, out float min, out float max)
    {
        switch (rarity)
        {
            case ItemRarity.R:
                min = 35f;
                max = 65f;
                break;
            case ItemRarity.U:
                min = 40f;
                max = 60f;
                break;
            default:
                min = 25f;
                max = 75f;
                break;
        }
        min = Math.Max(0f, min - rod.SafeBonus);
        max = Math.Min(TensionMax, max + rod.SafeBonus);
    }

    /// <summary>저항 중 추가 장력(초당)</summary>
    public static float ResistRate(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.R:
                return 11f;
            case ItemRarity.U:
                return 14f;
            default:
                return 8f;
        }
    }

    private static float Clamp01(float v)
    {
        return v < 0f ? 0f : (v > 0.999999f ? 0.999999f : v);
    }
}
