using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>정류장 한 곳 (Resources/FastTravelStops.csv 한 줄)</summary>
public sealed class FastTravelStop
{
    public string Id;           // 맵 이름(MapInfo.GoToMap 인자)
    public int Order;           // 요금 순번
    public int Floor;
    public string DisplayName;
    public float? AnchorX;      // 마차 NPC 월드 X (없으면 NPC 미배치)
    public float? AnchorY;      // 비면 배치 때 지면/기준 NPC 높이
    public string QuestId;      // 해금 퀘스트 (빈칸 = 해금 퀘스트 없음)
    public string NpcId;        // 마차 NPC id (빈칸 = NPC 없음)
    public bool AlwaysOpen;     // true = 처음부터 열림(Town·SideRoad)
}

/// <summary>
/// 정류장 표 로더 (FastTravel.md §8). 컬럼: id,order,floor,displayName,anchorX,anchorY,questId,npcName,alwaysOpen (추가는 맨 뒤).
/// npcName 열은 마차 NPC id(coach_*) 로 쓴다. 쉼표 없는 평문 CSV.
/// </summary>
public static class FastTravelStops
{
    private const string ResourceName = "FastTravelStops";
    private static List<FastTravelStop> s_all;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_all = null;
    }

    public static IReadOnlyList<FastTravelStop> All
    {
        get
        {
            if (s_all == null)
            {
                TextAsset ta = Resources.Load<TextAsset>(ResourceName);
                s_all = Parse(ta != null ? ta.text : "");
            }
            return s_all;
        }
    }

    public static FastTravelStop Find(string id)
    {
        foreach (FastTravelStop s in All)
        {
            if (s.Id == id)
            {
                return s;
            }
        }
        return null;
    }

    /// <summary>테스트용: 표를 주어진 CSV 텍스트로 바꾼다. null 이면 Resources 표로 되돌린다</summary>
    public static void SetTableForTests(string csv)
    {
        s_all = csv == null ? null : Parse(csv);
    }

    public static List<FastTravelStop> Parse(string csv)
    {
        var list = new List<FastTravelStop>();
        string[] lines = (csv ?? "").TrimStart((char)0xFEFF).Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Length == 0)
            {
                continue;
            }
            string[] c = lines[i].Split(',');
            int.TryParse(Col(c, 1), out int order);
            int.TryParse(Col(c, 2), out int floor);
            list.Add(new FastTravelStop
            {
                Id = Col(c, 0),
                Order = order,
                Floor = floor,
                DisplayName = Col(c, 3),
                AnchorX = Num(Col(c, 4)),
                AnchorY = Num(Col(c, 5)),
                QuestId = Col(c, 6),
                NpcId = Col(c, 7),
                AlwaysOpen = Col(c, 8) == "1",
            });
        }
        return list;
    }

    private static string Col(string[] c, int i) => i < c.Length ? c[i].Trim() : "";

    private static float? Num(string s)
    {
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : (float?)null;
    }
}
