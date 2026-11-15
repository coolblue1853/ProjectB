using System.Collections.Generic;

/// <summary>퀘스트 따라가기 판단에 쓰는 퀘스트 한 건의 읽기값(게임 상태를 복사한 것)</summary>
public struct BotQuestInfo
{
    public string Id;
    public NpcQuestStatus Status;
    public NpcQuestObjective Objective;
    public string Giver;
    public string ReportNpc;
    public string Kind;
    public string Source;
    public string Target;
    public string[] Targets;
    /// <summary>처치형: 목표 몹이 나오는 맵(모르면 null)</summary>
    public string HuntMap;
    public bool Tracked;
}

public enum BotQuestJobKind { None, Report, Accept }

public struct BotQuestJob
{
    public BotQuestJobKind Kind;
    public string QuestId;
    public string Npc;
    public string Map;
}

/// <summary>퀘스트 따라가기 규칙(순수, EditMode 시험). 설계 PlayBotDesign.md 15절</summary>
public static class BotQuestRules
{
    public const string Town = "Town";
    public const int MaxActiveKill = 2;

    /// <summary>봇이 걸어가서 말 걸 수 있는 NPC 의 맵(NpcQuestPlacer 배치와 같다). 게시판·입구·모르는 id 는 null</summary>
    public static string NpcMap(string npcId)
    {
        switch (npcId)
        {
            case "elder":
            case "ruen":
            case "sena":
            case "merchant":
            case "smith":
                return Town;
            case "eve":
                return "Lv1_NormalForest";
            default:
                return null;
        }
    }

    /// <summary>"a|b" 목록 대상을 배열로</summary>
    public static string[] Split(string target)
    {
        return string.IsNullOrEmpty(target) ? new string[0] : target.Split('|');
    }

    /// <summary>목표 몹들이 나오는 맵: 지금 맵이 맞으면 그대로, 아니면 이름순 첫 맵. 없으면 null</summary>
    public static string PickHuntMap(string[] targets, IDictionary<string, HashSet<string>> mapMobs, string current)
    {
        if (targets == null || mapMobs == null)
        {
            return null;
        }
        if (current != null && mapMobs.TryGetValue(current, out HashSet<string> here) && Has(here, targets))
        {
            return current;
        }
        string best = null;
        foreach (KeyValuePair<string, HashSet<string>> kv in mapMobs)
        {
            if (Has(kv.Value, targets) && (best == null || string.CompareOrdinal(kv.Key, best) < 0))
            {
                best = kv.Key;
            }
        }
        return best;
    }

    private static bool Has(HashSet<string> mobs, string[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (mobs.Contains(targets[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 이번에 할 마을·NPC 일. 보고 가능(완료) 퀘스트가 먼저(지금 맵 → 추적 중 → 표 순서),
    /// 없으면 진행 중 처치 퀘스트가 MaxActiveKill 미만일 때 받을 수 있는 처치·"수락한 NPC 와 대화" 퀘스트(메인 → 지금 맵 → 표 순서).
    /// </summary>
    public static BotQuestJob PickJob(IList<BotQuestInfo> q, string curMap)
    {
        int best = -1;
        int bestScore = -1;
        int activeKill = 0;
        for (int i = 0; i < q.Count; i++)
        {
            BotQuestInfo d = q[i];
            if (d.Status == NpcQuestStatus.Active && d.Objective == NpcQuestObjective.Kill)
            {
                activeKill++;
            }
            string map = NpcMap(d.ReportNpc);
            if (d.Status != NpcQuestStatus.Ready || map == null)
            {
                continue;
            }
            int score = 1 + (map == curMap ? 2 : 0) + (d.Tracked ? 1 : 0);
            if (score > bestScore)
            {
                bestScore = score;
                best = i;
            }
        }
        if (best >= 0)
        {
            return Make(BotQuestJobKind.Report, q[best], q[best].ReportNpc);
        }
        if (activeKill >= MaxActiveKill)
        {
            return default(BotQuestJob);
        }
        for (int i = 0; i < q.Count; i++)
        {
            BotQuestInfo d = q[i];
            if (d.Status != NpcQuestStatus.Available || NpcMap(d.Giver) == null || !(string.IsNullOrEmpty(d.Source) || d.Source == "npc"))
            {
                continue;
            }
            bool kill = d.Objective == NpcQuestObjective.Kill && d.HuntMap != null;
            bool instant = d.Objective == NpcQuestObjective.Talk && d.Target == d.Giver;
            if (!kill && !instant)
            {
                continue;
            }
            int score = 1 + (d.Kind == "main" ? 4 : 0) + (NpcMap(d.Giver) == curMap ? 2 : 0);
            if (score > bestScore)
            {
                bestScore = score;
                best = i;
            }
        }
        return best >= 0 ? Make(BotQuestJobKind.Accept, q[best], q[best].Giver) : default(BotQuestJob);
    }

    private static BotQuestJob Make(BotQuestJobKind kind, BotQuestInfo d, string npc)
    {
        return new BotQuestJob { Kind = kind, QuestId = d.Id, Npc = npc, Map = NpcMap(npc) };
    }

    /// <summary>사냥 방향: 진행 중(완료 전) 처치 퀘스트 중 추적 중인 것 → 표 순서. 없으면 -1</summary>
    public static int PickHunt(IList<BotQuestInfo> q)
    {
        int best = -1;
        for (int i = 0; i < q.Count; i++)
        {
            BotQuestInfo d = q[i];
            if (d.Status != NpcQuestStatus.Active || d.Objective != NpcQuestObjective.Kill || d.HuntMap == null)
            {
                continue;
            }
            if (best < 0 || (d.Tracked && !q[best].Tracked))
            {
                best = i;
            }
        }
        return best;
    }

    /// <summary>NPC 선택지 커서를 "퀘스트 이야기" 줄로: 위/아래 한 칸, 같으면 Select, 줄이 없으면 Back</summary>
    public static BotKey ToNpcQuest(bool menuOpen, int cursor, int questRow)
    {
        if (!menuOpen)
        {
            return BotKey.None;
        }
        if (questRow < 0)
        {
            return BotKey.Back;
        }
        if (cursor < questRow)
        {
            return BotKey.Down;
        }
        return cursor > questRow ? BotKey.Up : BotKey.Select;
    }
}
