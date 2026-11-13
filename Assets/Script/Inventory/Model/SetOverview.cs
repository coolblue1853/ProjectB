using System.Collections.Generic;

/// <summary>세트 창 한 줄: 착용 중인 세트 하나의 착용 수·정의 단계·켜진 단계·다음 단계</summary>
public sealed class SetOverviewEntry
{
    public string SetKey;
    public int Worn;
    public readonly List<int> AllTiers = new List<int>();
    public readonly List<int> ActiveTiers = new List<int>();
    /// <summary>아직 안 켜진 가장 낮은 단계. 전부 켜졌으면 0</summary>
    public int NextTier;
    /// <summary>다음 단계까지 더 입어야 할 수. 전부 켜졌으면 0</summary>
    public int NeedForNext => NextTier > 0 ? NextTier - Worn : 0;

    public bool IsOn(int tier) => ActiveTiers.Contains(tier);
}

/// <summary>
/// 세트 창(REWORK-12 P6) 순수 규칙 — 착용 수 사전과 단계 표에서 화면에 보일 세트 목록을 만든다.
/// 켜짐 판정은 SetBonusEvaluator.ActiveTiers 와 같다(정의 단계 중 착용 수 이하 전부, 누적).
/// 순서: 켜진 단계 많은 세트 → 착용 많은 세트 → 세트 키.
/// </summary>
public static class SetOverview
{
    public static List<SetOverviewEntry> Build(IReadOnlyDictionary<string, int> counts, SetTierTable table)
    {
        var list = new List<SetOverviewEntry>();
        foreach (KeyValuePair<string, int> pair in counts)
        {
            if (string.IsNullOrEmpty(pair.Key) || pair.Value <= 0 || !table.HasSet(pair.Key))
            {
                continue;
            }
            var e = new SetOverviewEntry { SetKey = pair.Key, Worn = pair.Value };
            foreach (int t in table.TiersOf(pair.Key))
            {
                e.AllTiers.Add(t);
                if (t <= pair.Value)
                {
                    e.ActiveTiers.Add(t);
                }
                else if (e.NextTier == 0)
                {
                    e.NextTier = t;
                }
            }
            list.Add(e);
        }
        list.Sort(Compare);
        return list;
    }

    public static int TotalActiveTiers(List<SetOverviewEntry> list)
    {
        int n = 0;
        foreach (SetOverviewEntry e in list)
        {
            n += e.ActiveTiers.Count;
        }
        return n;
    }

    private static int Compare(SetOverviewEntry a, SetOverviewEntry b)
    {
        int c = b.ActiveTiers.Count.CompareTo(a.ActiveTiers.Count);
        if (c != 0)
        {
            return c;
        }
        c = b.Worn.CompareTo(a.Worn);
        return c != 0 ? c : string.CompareOrdinal(a.SetKey, b.SetKey);
    }
}
