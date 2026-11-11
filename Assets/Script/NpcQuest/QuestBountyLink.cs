using System.Collections.Generic;

/// <summary>
/// 퀘스트 창(J)에 게시대 현상 의뢰(BountyBoard)를 읽기 전용으로 합쳐 보여 주기 위한 어댑터.
/// 현상 의뢰를 NpcQuestDef 모양으로 바꿔 같은 목록에 넣는다(Id 접두사 "bounty:"). 수락·보고는 입구 모드 창(QuestEntrance.Kiosk)에서 QuestService 로 한다.
/// 진행 저장은 BountyBoard 가 그대로 맡는다(여기서는 쓰지 않는다).
/// </summary>
public static class QuestBountyLink
{
    public const string Prefix = "bounty:";
    public const string Giver = "bountyboard";

    private static readonly Dictionary<string, BountyDef> s_defs = new Dictionary<string, BountyDef>();

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_defs.Clear();
    }

    public static bool IsBounty(NpcQuestDef d)
    {
        return d != null && d.Id != null && d.Id.StartsWith(Prefix);
    }

    /// <summary>오늘 받을 수 있는 현상 의뢰 (진행 중이거나 완료한 것은 제외)</summary>
    public static List<NpcQuestDef> Available()
    {
        var list = new List<NpcQuestDef>();
        BountyBoard b = BountyBoard.Instance;
        if (b == null)
        {
            return list;
        }
        foreach (string id in b.State.Offered)
        {
            BountyDef d = BountyRules.Find(b.Defs, id);
            if (d != null && b.State.Active != id && !b.State.Done.Contains(id))
            {
                list.Add(Adapt(d));
            }
        }
        return list;
    }

    /// <summary>진행 중인 현상 의뢰 (없으면 빈 목록)</summary>
    public static List<NpcQuestDef> Active()
    {
        var list = new List<NpcQuestDef>();
        BountyBoard b = BountyBoard.Instance;
        if (b != null && b.State.Active.Length > 0)
        {
            BountyDef d = BountyRules.Find(b.Defs, b.State.Active);
            if (d != null)
            {
                list.Add(Adapt(d));
            }
        }
        return list;
    }

    public static bool IsReady(NpcQuestDef q)
    {
        BountyBoard b = BountyBoard.Instance;
        return b != null && s_defs.TryGetValue(q.Id, out BountyDef d) && BountyRules.IsReady(b.State, d);
    }

    public static int Progress(NpcQuestDef q)
    {
        BountyBoard b = BountyBoard.Instance;
        return b != null && s_defs.ContainsKey(q.Id) && b.State.Active == q.Id.Substring(Prefix.Length) ? b.State.Progress : 0;
    }

    public static string Zone(NpcQuestDef q)
    {
        return s_defs.TryGetValue(q.Id, out BountyDef d) ? d.Zone : "";
    }

    private static NpcQuestDef Adapt(BountyDef d)
    {
        string id = Prefix + d.Id;
        s_defs[id] = d;
        Floor1Data.Reward r = d.Reward;
        return new NpcQuestDef
        {
            Id = id,
            Title = d.Title, // 유형 태그(현)는 창·HUD 가 붙인다
            Giver = Giver,
            MinLevel = d.MinLevel,
            Objective = NpcQuestObjective.Kill,
            Target = d.Targets != null && d.Targets.Length > 0 ? d.Targets[0] : "",
            Count = d.Count,
            Exp = r != null ? r.Exp : 0,
            Gold = r != null ? r.Gold : 0,
            RewardItem = r != null && r.HasItem ? r.Item : "",
            RewardCount = r != null ? r.Count : 0,
            Description = "같은 몹류를 " + d.Count + "마리 처치하는 하루 현상. 하루가 지나면 사라진다.",
            Source = "bounty",
            ReportTo = Giver,
            Zone = d.Zone,
        };
    }
}
