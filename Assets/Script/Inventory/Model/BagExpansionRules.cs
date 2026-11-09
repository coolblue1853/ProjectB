using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// 가방 칸 확장 규칙(순수). BagExpansion.csv(step,slots,price,minFloor): slots = 구매 뒤 칸 수, minFloor = 살 수 있는 최소 층계.
/// 설계: InventoryRefactorDesign.md §7.
/// </summary>
public sealed class BagExpansionRules
{
    public const int StartSlots = 40;
    public const int MaxSlots = 125;

    public enum Block { None, Max, NeedFloor, NoGold }

    public struct Step
    {
        public int Slots;
        public int Price;
        public int MinFloor;
    }

    private readonly List<Step> _steps = new List<Step>();

    public int StepCount => _steps.Count;

    public static BagExpansionRules Parse(string csv)
    {
        var rules = new BagExpansionRules();
        if (string.IsNullOrEmpty(csv))
        {
            return rules;
        }
        string[] lines = csv.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Trim().Split(',');
            if (c.Length < 4 || !int.TryParse(c[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int slots)
                || !int.TryParse(c[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int price)
                || !int.TryParse(c[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int floor))
            {
                continue;
            }
            rules._steps.Add(new Step { Slots = slots, Price = price, MinFloor = floor });
        }
        rules._steps.Sort((a, b) => a.Slots.CompareTo(b.Slots));
        return rules;
    }

    /// <summary>저장값을 유효 범위로. 0(키 없음)·40 미만 → 40(구 세이브의 25~39 도 40 으로, 더 큰 값은 유지), 125 초과 → 125</summary>
    public static int Clamp(int unlocked)
    {
        return unlocked < StartSlots ? StartSlots : unlocked > MaxSlots ? MaxSlots : unlocked;
    }

    /// <summary>현재 칸 수 다음 단계. 더 없으면 false</summary>
    public bool TryNext(int unlocked, out Step next)
    {
        int cur = Clamp(unlocked);
        foreach (Step s in _steps)
        {
            if (s.Slots > cur && s.Slots <= MaxSlots)
            {
                next = s;
                return true;
            }
        }
        next = default;
        return false;
    }

    /// <summary>구매 가능 판정. None 이면 살 수 있다(next 가 구매할 단계)</summary>
    public Block Check(int unlocked, int floor, int gold, out Step next)
    {
        if (!TryNext(unlocked, out next))
        {
            return Block.Max;
        }
        if (floor < next.MinFloor)
        {
            return Block.NeedFloor;
        }
        return gold < next.Price ? Block.NoGold : Block.None;
    }

    /// <summary>해당 층계에서 닿을 수 있는 최대 칸 수</summary>
    public int FloorCap(int floor)
    {
        int cap = StartSlots;
        foreach (Step s in _steps)
        {
            if (s.MinFloor <= floor && s.Slots > cap)
            {
                cap = s.Slots;
            }
        }
        return cap;
    }

    /// <summary>from 초과 ~ to 이하 단계를 모두 사는 총 비용(가격표 검증·봇 예산용)</summary>
    public int TotalCost(int from, int to)
    {
        int sum = 0;
        foreach (Step s in _steps)
        {
            if (s.Slots > from && s.Slots <= to)
            {
                sum += s.Price;
            }
        }
        return sum;
    }
}
