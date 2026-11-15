using System.Collections.Generic;
using NUnit.Framework;

/// <summary>자동 플레이 봇 퀘스트 따라가기 순수 로직: 일 고르기·사냥 맵·NPC 선택지 키</summary>
public class PlayBotQuestTests
{
    private static BotQuestInfo Q(string id, NpcQuestStatus st, NpcQuestObjective o, string giver, string report = null, string kind = "", string hunt = null)
    {
        return new BotQuestInfo { Id = id, Status = st, Objective = o, Giver = giver, ReportNpc = report ?? giver, Kind = kind, Target = giver, Targets = new string[0], HuntMap = hunt };
    }

    [Test]
    public void NpcMap_KnownAndUnknown()
    {
        Assert.AreEqual("Town", BotQuestRules.NpcMap("ruen"));
        Assert.AreEqual("Lv1_NormalForest", BotQuestRules.NpcMap("eve"));
        Assert.IsNull(BotQuestRules.NpcMap("questboard"));
    }

    [Test]
    public void PickJob_ReportBeforeAccept()
    {
        var q = new List<BotQuestInfo>
        {
            Q("A", NpcQuestStatus.Available, NpcQuestObjective.Kill, "ruen", hunt: "M1"),
            Q("B", NpcQuestStatus.Ready, NpcQuestObjective.Kill, "ruen"),
        };
        BotQuestJob j = BotQuestRules.PickJob(q, "Town");
        Assert.AreEqual(BotQuestJobKind.Report, j.Kind);
        Assert.AreEqual("B", j.QuestId);
        Assert.AreEqual("Town", j.Map);
    }

    [Test]
    public void PickJob_AcceptSkipsUnsupported_AndCapsActiveKill()
    {
        var q = new List<BotQuestInfo>
        {
            Q("C", NpcQuestStatus.Available, NpcQuestObjective.Collect, "ruen"),
            Q("K", NpcQuestStatus.Available, NpcQuestObjective.Kill, "ruen", hunt: null),
            Q("T", NpcQuestStatus.Available, NpcQuestObjective.Kill, "ruen", hunt: "M1"),
        };
        Assert.AreEqual("T", BotQuestRules.PickJob(q, "Town").QuestId);
        q.Add(Q("X1", NpcQuestStatus.Active, NpcQuestObjective.Kill, "ruen", hunt: "M1"));
        q.Add(Q("X2", NpcQuestStatus.Active, NpcQuestObjective.Kill, "ruen", hunt: "M1"));
        Assert.AreEqual(BotQuestJobKind.None, BotQuestRules.PickJob(q, "Town").Kind);
    }

    [Test]
    public void PickJob_InstantTalkAndMainFirst()
    {
        var q = new List<BotQuestInfo>
        {
            Q("S", NpcQuestStatus.Available, NpcQuestObjective.Kill, "ruen", kind: "side", hunt: "M1"),
            Q("M", NpcQuestStatus.Available, NpcQuestObjective.Talk, "elder", kind: "main"),
        };
        Assert.AreEqual("M", BotQuestRules.PickJob(q, "Town").QuestId);
        Assert.AreEqual(BotQuestJobKind.None, BotQuestRules.PickJob(new List<BotQuestInfo>(), "Town").Kind);
    }

    [Test]
    public void PickHuntMap_PrefersCurrentThenName()
    {
        var m = new Dictionary<string, HashSet<string>>
        {
            { "B", new HashSet<string> { "mole" } },
            { "A", new HashSet<string> { "mole", "boar" } },
        };
        Assert.AreEqual("B", BotQuestRules.PickHuntMap(new[] { "mole" }, m, "B"));
        Assert.AreEqual("A", BotQuestRules.PickHuntMap(new[] { "mole" }, m, "Town"));
        Assert.IsNull(BotQuestRules.PickHuntMap(new[] { "bear" }, m, "A"));
    }

    [Test]
    public void PickHunt_TrackedFirst()
    {
        var q = new List<BotQuestInfo> { Q("a", NpcQuestStatus.Active, NpcQuestObjective.Kill, "r", hunt: "M1"), Q("b", NpcQuestStatus.Active, NpcQuestObjective.Kill, "r", hunt: "M2"), Q("c", NpcQuestStatus.Ready, NpcQuestObjective.Kill, "r", hunt: "M3") };
        Assert.AreEqual(0, BotQuestRules.PickHunt(q));
        BotQuestInfo b = q[1];
        b.Tracked = true;
        q[1] = b;
        Assert.AreEqual(1, BotQuestRules.PickHunt(q));
    }

    [Test]
    public void ToNpcQuest_Keys()
    {
        Assert.AreEqual(BotKey.None, BotQuestRules.ToNpcQuest(false, 0, 0));
        Assert.AreEqual(BotKey.Back, BotQuestRules.ToNpcQuest(true, 0, -1));
        Assert.AreEqual(BotKey.Down, BotQuestRules.ToNpcQuest(true, 0, 1));
        Assert.AreEqual(BotKey.Up, BotQuestRules.ToNpcQuest(true, 2, 1));
        Assert.AreEqual(BotKey.Select, BotQuestRules.ToNpcQuest(true, 1, 1));
    }
}
