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
        { "Wood", "Forest Side Road" },
        { "Forest Mushroom", "Forest Side Road" },
        { "Moss Loach", "Swamp" },
        { "Amber Trout", "Swamp" },
        { "Fairy Dust", "Deep Forest Core" },
        { "Spirit Koi", "Mist Swamp" },
        { "Slime Gel", "Mist Swamp" },
        { "Bog Copper Ore", "Bandit Fortress" },
        { "Ancient Relic Shard", "Tower Outskirts" },
        { FathersRecords.SaveKey, "Tower Outskirts" },
    };

    /// <summary>구역 안내판이 다루는 목표 구역(MonsterSheet zone 이름, 영문 원본). 마차 정류장 맵 id 기준</summary>
    private static readonly Dictionary<string, string[]> SignZones = new Dictionary<string, string[]>
    {
        { "Lv1_DeepForest", new[] { "Deep Forest", "Deep Forest Core" } },
        { "Lv1_Swamp", new[] { "Swamp", "Mist Swamp" } },
        { "Lv1_BanditFortress", new[] { "Bandit Fortress" } },
        { "Lv1_TowerOutskirts", new[] { "Tower Outskirts" } },
    };

    private static readonly Dictionary<string, string> s_cache = new Dictionary<string, string>();

    private static bool MatchesZone(string zone, string name)
    {
        return zone.StartsWith(name, System.StringComparison.Ordinal) && (zone.Length == name.Length || (zone.Length > name.Length + 1 && zone[name.Length] == ' ' && zone[name.Length + 1] == '('));
    }

    /// <summary>안내판 맵 id 의 목표 구역 이름들 (없으면 빈 배열)</summary>
    public static string[] ZonesOfSign(string mapId)
    {
        return mapId != null && SignZones.TryGetValue(mapId, out string[] z) ? z : System.Array.Empty<string>();
    }

    /// <summary>d 의 목표 구역이 keys(영문 원본) 중 하나인가. "구역 (night)" 같은 꼬리표가 붙은 값도 같은 구역으로 본다</summary>
    public static bool InZones(NpcQuestDef d, string[] keys)
    {
        string zone = Of(d);
        if (string.IsNullOrEmpty(zone) || keys == null)
        {
            return false;
        }
        foreach (string k in keys)
        {
            // CSV zone 열은 영문 원본, 파생 값은 현지화된 이름이라 둘 다 비교한다
            if (MatchesZone(zone, k) || MatchesZone(zone, Loc.Region(k)))
            {
                return true;
            }
        }
        return false;
    }

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
        if (!string.IsNullOrEmpty(d.Zone))
        {
            return d.Zone; // CSV zone 열이 있으면 그대로 쓴다 (QuestSystemDesign §2-2)
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
                if (d.Target == NpcQuestObjectiveRules.AnyElite)
                {
                    return Loc.IsKorean ? "정예 둥지" : "Elite Nest";
                }
                MonsterTable.Row row = MonsterTable.Get(d.FirstTarget);
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

/// <summary>
/// 퀘스트 창 구역별 접기 상태 (결정 11-05). 키 = "탭|머리글" 문자열. 새 세이브 키 npcq.folded 만 추가(기존 필드 불변).
/// </summary>
public static class QuestFoldRules
{
    public const string SaveKey = "npcq.folded";
    private static readonly HashSet<string> s_folded = new HashSet<string>();
    private static bool s_loaded;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_folded.Clear();
        s_loaded = false;
    }

    public static string Key(QuestListRules.Tab tab, string header)
    {
        return (int)tab + "|" + header;
    }

    public static bool IsFolded(string key)
    {
        EnsureLoaded();
        return s_folded.Contains(key);
    }

    /// <summary>접기/펼치기 전환 후 저장. 반환 = 접힘 여부</summary>
    public static bool Toggle(string key)
    {
        EnsureLoaded();
        bool folded = s_folded.Add(key);
        if (!folded)
        {
            s_folded.Remove(key);
        }
        Save();
        return folded;
    }

    public static string Serialize(IEnumerable<string> keys)
    {
        return string.Join("\n", keys);
    }

    public static void Deserialize(string text, HashSet<string> into)
    {
        into.Clear();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        foreach (string k in text.Split('\n'))
        {
            if (k.Length > 0)
            {
                into.Add(k);
            }
        }
    }

    private static void EnsureLoaded()
    {
        if (s_loaded)
        {
            return;
        }
        s_loaded = true;
        try
        {
            Deserialize(ES3.KeyExists(SaveKey) ? ES3.Load<string>(SaveKey) : "", s_folded);
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "퀘스트 접기 읽기 실패: " + e.Message);
        }
    }

    private static void Save()
    {
        try
        {
            ES3.Save(SaveKey, Serialize(s_folded));
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "퀘스트 접기 저장 실패: " + e.Message);
        }
    }
}

/// <summary>퀘스트 창 목록 규칙 (순수): 받을 수 있는 / 진행 중(완료 가능이 맨 위) / 완료 세 탭, 같은 구역끼리 묶는다</summary>
public static class QuestListRules
{
    /// <summary>탭. All(전체, U6)은 접기 키 정수((int)tab)가 바뀌지 않게 맨 뒤에 붙였고, 화면·순환 순서는 Order 를 따른다</summary>
    public enum Tab { Available, Active, Done, All }

    /// <summary>화면 왼쪽부터·순환 순서: 전체 → 받을 수 있는 → 진행 중 → 완료</summary>
    public static readonly Tab[] Order = { Tab.All, Tab.Available, Tab.Active, Tab.Done };

    /// <summary>좌(-1)/우(+1) 탭 순환 (Order 순서)</summary>
    public static Tab Step(Tab tab, int dir)
    {
        int n = Order.Length;
        int i = System.Array.IndexOf(Order, tab);
        return Order[((i < 0 ? 0 : i) + dir % n + n) % n];
    }

    /// <summary>상태 정렬 순위 (메이플 관례): 완료 가능 0 · 진행 중 1 · 수락 가능 2 · 레벨 부족 3 · 완료 4 · 잠김 5</summary>
    public static int Rank(NpcQuestStatus s)
    {
        switch (s)
        {
            case NpcQuestStatus.Ready: return 0;
            case NpcQuestStatus.Active: return 1;
            case NpcQuestStatus.Available: return 2;
            case NpcQuestStatus.LevelLow: return 3;
            case NpcQuestStatus.Done: return 4;
            default: return 5;
        }
    }

    /// <summary>상태 순위로 안정 정렬 (같은 상태는 원래 순서)</summary>
    public static List<NpcQuestDef> SortByState(IList<NpcQuestDef> list, System.Func<NpcQuestDef, NpcQuestStatus> statusOf)
    {
        var buckets = new List<NpcQuestDef>[6];
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new List<NpcQuestDef>();
        }
        foreach (NpcQuestDef d in list)
        {
            buckets[Rank(statusOf(d))].Add(d);
        }
        var res = new List<NpcQuestDef>(list.Count);
        foreach (List<NpcQuestDef> b in buckets)
        {
            res.AddRange(b);
        }
        return res;
    }

    /// <summary>상태가 속하는 실제 탭 (전체 탭에서 수락·보고·추적·포기 판정에 쓴다)</summary>
    public static Tab TabOf(NpcQuestStatus s)
    {
        switch (s)
        {
            case NpcQuestStatus.Ready:
            case NpcQuestStatus.Active:
                return Tab.Active;
            case NpcQuestStatus.Done:
                return Tab.Done;
            default:
                return Tab.Available;
        }
    }

    /// <summary>받을 수 있는 퀘스트: 수락 가능을 먼저, 레벨 부족(회색)을 뒤에. 각각 표 순서</summary>
    public static List<NpcQuestDef> Available(IEnumerable<NpcQuestDef> all, System.Func<NpcQuestDef, NpcQuestStatus> statusOf)
    {
        var open = new List<NpcQuestDef>();
        var low = new List<NpcQuestDef>();
        foreach (NpcQuestDef d in all)
        {
            NpcQuestStatus st = statusOf(d);
            if (st == NpcQuestStatus.Available)
            {
                open.Add(d);
            }
            else if (st == NpcQuestStatus.LevelLow)
            {
                low.Add(d);
            }
        }
        open.AddRange(low);
        return open;
    }

    /// <summary>같은 구역끼리 모은다 — 구역은 처음 나온 순서, 구역 안은 원래 순서(안정)</summary>
    public static List<NpcQuestDef> GroupByZone(IList<NpcQuestDef> list, System.Func<NpcQuestDef, string> zoneOf)
    {
        var order = new List<string>();
        var buckets = new Dictionary<string, List<NpcQuestDef>>();
        foreach (NpcQuestDef d in list)
        {
            string z = zoneOf(d) ?? "";
            if (!buckets.TryGetValue(z, out List<NpcQuestDef> bucket))
            {
                bucket = new List<NpcQuestDef>();
                buckets[z] = bucket;
                order.Add(z);
            }
            bucket.Add(d);
        }
        var res = new List<NpcQuestDef>(list.Count);
        foreach (string z in order)
        {
            res.AddRange(buckets[z]);
        }
        return res;
    }

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
