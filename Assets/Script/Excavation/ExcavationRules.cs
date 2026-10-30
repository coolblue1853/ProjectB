using System;

/// <summary>발굴판 칸 내용</summary>
public enum DigContent { Empty, Relic, Hazard }

/// <summary>발굴판 규격(기본 유적 / 희귀 유적)</summary>
public readonly struct DigSpec
{
    public readonly int Size;
    public readonly int Relics;
    public readonly int Picks;
    public readonly int Hazards;
    public readonly float TimeLimit;
    public readonly bool Rare;

    public DigSpec(int size, int relics, int picks, int hazards, float timeLimit, bool rare)
    {
        Size = size;
        Relics = relics;
        Picks = picks;
        Hazards = hazards;
        TimeLimit = timeLimit;
        Rare = rare;
    }
}

/// <summary>
/// ③ 고대 유물 발굴 순수 규칙·수치(MiniGames.md "고대 유물 발굴 구현"). 출처: GPT 기획 대화(10-30) 수치표.
/// 숨겨진 5×5 발굴판에서 칸을 골라 판다. 판 칸에는 상하좌우 4칸의 유물 수가 드러난다(지뢰찾기 변형).
/// 난수는 호출자가 0~1 값으로 넘긴다(테스트 가능).
/// </summary>
public static class ExcavationRules
{
    public const int CollapseLimit = 100;
    public const int CollapsePerHazard = 50;
    public const float RareSiteChance = 0.15f;
    public const float Cooldown = 300f;            // 유물을 하나라도 캐면 5분(희귀 유적이면 10분)
    public const float RareCooldown = 600f;
    public const string RecordId = "#FathersRecord"; // 보상표의 '아버지의 기록' 자리(인벤 아이템 아님)
    public const string RecordFallbackId = "Ancient Gear Core"; // 5편 다 모았으면 대체

    public static readonly DigSpec Basic = new DigSpec(5, 2, 12, 3, 45f, false);
    public static readonly DigSpec RareSite = new DigSpec(5, 3, 13, 4, 60f, true);

    /// <summary>층계 1 보상표(유물 하나마다 독립 추첨). 기록은 GPT 2% → 6%(Q-440, 5편 수집이 너무 느림), 그만큼 파편에서 뺐다</summary>
    public static readonly GatherDef[] Floor1 =
    {
        new GatherDef("Ancient Relic Shard", ItemRarity.C, 41f, 1, 2),
        new GatherDef("Ancient Blueprint Fragment", ItemRarity.C, 30f, 1, 1),
        new GatherDef("Ancient Gear Core", ItemRarity.R, 15f, 1, 1),
        new GatherDef("Forgotten Runestone", ItemRarity.R, 8f, 1, 1),
        new GatherDef(RecordId, ItemRarity.U, 6f, 1, 1),
    };

    /// <summary>층계별 표. 표가 없는 층계는 층계 1 표</summary>
    public static GatherDef[] TableFor(int floor)
    {
        return Floor1;
    }

    public static DigSpec SpecFor(float roll01)
    {
        return roll01 < RareSiteChance ? RareSite : Basic;
    }

    public static float CooldownFor(DigSpec spec)
    {
        return spec.Rare ? RareCooldown : Cooldown;
    }

    /// <summary>
    /// 발굴판 생성. 가운데 칸은 빈 칸(시작 때 공짜로 드러남). 유물끼리는 상하좌우로 붙지 않고, 위험 칸은 유물·가운데와 겹치지 않는다.
    /// 칸 순서는 y * size + x.
    /// </summary>
    public static DigContent[] Generate(DigSpec spec, Func<float> random)
    {
        int n = spec.Size * spec.Size;
        var cells = new DigContent[n];
        int center = Center(spec.Size);
        int placed = 0;
        for (int guard = 0; placed < spec.Relics && guard < 500; guard++)
        {
            int i = Pick(n, random());
            if (i == center || cells[i] != DigContent.Empty || TouchesRelic(cells, spec.Size, i))
            {
                continue;
            }
            cells[i] = DigContent.Relic;
            placed++;
        }
        placed = 0;
        for (int guard = 0; placed < spec.Hazards && guard < 500; guard++)
        {
            int i = Pick(n, random());
            if (i == center || cells[i] != DigContent.Empty)
            {
                continue;
            }
            cells[i] = DigContent.Hazard;
            placed++;
        }
        return cells;
    }

    public static int Center(int size)
    {
        return (size / 2) * size + size / 2;
    }

    /// <summary>상하좌우 4칸의 유물 수</summary>
    public static int Hint(DigContent[] cells, int size, int index)
    {
        int x = index % size;
        int y = index / size;
        return RelicAt(cells, size, x - 1, y) + RelicAt(cells, size, x + 1, y)
            + RelicAt(cells, size, x, y - 1) + RelicAt(cells, size, x, y + 1);
    }

    /// <summary>
    /// 기록 추첨: 아직 못 모은 편 중 균등(mask 비트 i = i번 편 수집). 다 모았으면 -1(대체 보상).
    /// </summary>
    public static int PickRecord(int mask, int count, float roll01)
    {
        int missing = 0;
        for (int i = 0; i < count; i++)
        {
            if ((mask & (1 << i)) == 0)
            {
                missing++;
            }
        }
        if (missing == 0)
        {
            return -1;
        }
        int k = Math.Min(missing - 1, (int)(Clamp01(roll01) * missing));
        for (int i = 0; i < count; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                continue;
            }
            if (k-- == 0)
            {
                return i;
            }
        }
        return -1;
    }

    private static bool TouchesRelic(DigContent[] cells, int size, int index)
    {
        return Hint(cells, size, index) > 0;
    }

    private static int RelicAt(DigContent[] cells, int size, int x, int y)
    {
        if (x < 0 || y < 0 || x >= size || y >= size)
        {
            return 0;
        }
        return cells[y * size + x] == DigContent.Relic ? 1 : 0;
    }

    private static int Pick(int n, float roll01)
    {
        return Math.Min(n - 1, (int)(Clamp01(roll01) * n));
    }

    private static float Clamp01(float v)
    {
        return v < 0f ? 0f : (v > 0.999999f ? 0.999999f : v);
    }
}
