using System.Collections.Generic;

/// <summary>
/// 퀘스트 목표 구역 이름 (퀘스트 창·추적 HUD 용). 처치 = MonsterSheet zone, 수집 = 그 아이템을 떨구는 퀘스트 드롭 몹의 zone(없으면 표),
/// 방문 = 지점 이름의 구역, 대화 = 마을. 모르면 빈 문자열.
/// </summary>
public static class NpcQuestZone
{
    // 몹 드롭이 아닌 수집 재료·기록의 출처
    private static readonly Dictionary<string, string> CollectSource = new Dictionary<string, string>
    {
        { "GreenHurb", "Forest Side Road" },
        { "Moss Loach", "Swamp" },
        { FathersRecords.SaveKey, "Tower Outskirts" },
    };

    private static readonly Dictionary<string, string> s_cache = new Dictionary<string, string>();

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_cache.Clear();
    }

    /// <summary>목표 구역 이름 (퀘스트×언어별로 캐시 — 추적 HUD 가 0.5초마다 부른다)</summary>
    public static string Of(NpcQuestDef d)
    {
        if (d == null)
        {
            return "";
        }
        string key = d.Id + (Loc.IsKorean ? "|ko" : "|en");
        if (!s_cache.TryGetValue(key, out string zone))
        {
            zone = Compute(d);
            s_cache[key] = zone;
        }
        return zone;
    }

    private static string Compute(NpcQuestDef d)
    {
        switch (d.Objective)
        {
            case NpcQuestObjective.Kill:
            case NpcQuestObjective.KillElite:
                MonsterTable.Row row = MonsterTable.Get(d.Target);
                return row != null ? Loc.Region(row.zone) : "";
            case NpcQuestObjective.Visit:
                return NpcQuestCatalog.VisitName(d.Target);
            case NpcQuestObjective.Collect:
                return Loc.Region(CollectZone(d.Target));
            case NpcQuestObjective.Craft:
                return Loc.IsKorean ? "화로" : "Campfire";
            default:
                return Loc.IsKorean ? "마을" : "Village";
        }
    }

    private static string CollectZone(string item)
    {
        if (CollectSource.TryGetValue(item, out string zone))
        {
            return zone;
        }
        foreach (MonsterTable.Row r in MonsterTable.All())
        {
            List<ExtraDrops.Entry> drops = ExtraDrops.For(r.id);
            if (drops == null)
            {
                continue;
            }
            foreach (ExtraDrops.Entry e in drops)
            {
                if (e.QuestOnly && e.Item == item)
                {
                    return r.zone;
                }
            }
        }
        return "";
    }
}

/// <summary>퀘스트 창 목록 규칙 (순수): 완료 가능을 맨 위, 나머지 진행 중은 표 순서, 완료는 별도 탭</summary>
public static class QuestListRules
{
    public enum Tab { Active, Done }

    public static List<NpcQuestDef> Active(IList<NpcQuestDef> active, System.Func<NpcQuestDef, bool> isReady)
    {
        var list = new List<NpcQuestDef>(active.Count);
        foreach (NpcQuestDef d in active)
        {
            if (isReady(d))
            {
                list.Add(d);
            }
        }
        foreach (NpcQuestDef d in active)
        {
            if (!isReady(d))
            {
                list.Add(d);
            }
        }
        return list;
    }

    /// <summary>완료한 퀘스트: 표 반대 순서(최근 것이 위)</summary>
    public static List<NpcQuestDef> Done(IEnumerable<NpcQuestDef> all, System.Func<string, bool> isDone)
    {
        var list = new List<NpcQuestDef>();
        foreach (NpcQuestDef d in all)
        {
            if (isDone(d.Id))
            {
                list.Add(d);
            }
        }
        list.Reverse();
        return list;
    }
}
