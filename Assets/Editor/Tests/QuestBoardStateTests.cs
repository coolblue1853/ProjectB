using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>U6(11-08 제보) 퀘스트 보드 목록: 전체 탭 상태 정렬(완료 가능 > 진행 중 > 수락 가능 > 레벨 부족 > 완료)·상태 판정·아이콘 대체·한글 문구</summary>
public class QuestBoardStateTests
{
    [SetUp]
    public void SetUp()
    {
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => 5;
        NpcQuestLog.ItemCounter = id => 0;
        NpcQuestLog.SaveHook = () => { };
        QuestService.BountyAvailable = () => new List<NpcQuestDef>();
        QuestService.BountyActive = () => new List<NpcQuestDef>();
        QuestService.Warn = msg => { };
    }

    [TearDown]
    public void TearDown()
    {
        NpcQuestCatalog.SetTableForTests(null);
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => LevelSystem.Level;
        NpcQuestLog.ItemCounter = id => InventoryManager.instance != null ? InventoryManager.instance.CountItem(id) : 0;
        NpcQuestLog.SaveHook = () => { };
        QuestService.BountyAvailable = null;
        QuestService.BountyActive = null;
        QuestService.Warn = msg => PickupLog.Warn(msg);
    }

    private static NpcQuestDef Q(string id) => new NpcQuestDef { Id = id, Title = id };

    [Test]
    public void 상태_정렬은_완료_가능_진행_중_수락_가능_레벨_부족_완료_순이고_같은_상태는_원래_순서()
    {
        var st = new Dictionary<string, NpcQuestStatus>
        {
            { "d1", NpcQuestStatus.Done }, { "av1", NpcQuestStatus.Available }, { "low", NpcQuestStatus.LevelLow },
            { "ac1", NpcQuestStatus.Active }, { "r1", NpcQuestStatus.Ready }, { "av2", NpcQuestStatus.Available },
            { "ac2", NpcQuestStatus.Active }, { "r2", NpcQuestStatus.Ready },
        };
        var list = new[] { "d1", "av1", "low", "ac1", "r1", "av2", "ac2", "r2" }.Select(Q).ToList(); // 입력 순서를 명시(Dictionary 열거 순서에 기대지 않는다)
        List<NpcQuestDef> res = QuestListRules.SortByState(list, d => st[d.Id]);
        CollectionAssert.AreEqual(new[] { "r1", "r2", "ac1", "ac2", "av1", "av2", "low", "d1" }, res.Select(d => d.Id).ToArray());
    }

    [Test]
    public void 상태별_실제_탭_판정()
    {
        Assert.AreEqual(QuestListRules.Tab.Active, QuestListRules.TabOf(NpcQuestStatus.Ready));
        Assert.AreEqual(QuestListRules.Tab.Active, QuestListRules.TabOf(NpcQuestStatus.Active));
        Assert.AreEqual(QuestListRules.Tab.Available, QuestListRules.TabOf(NpcQuestStatus.Available));
        Assert.AreEqual(QuestListRules.Tab.Available, QuestListRules.TabOf(NpcQuestStatus.LevelLow));
        Assert.AreEqual(QuestListRules.Tab.Done, QuestListRules.TabOf(NpcQuestStatus.Done));
    }

    [Test]
    public void 전체_탭은_탭_순환의_맨_앞()
    {
        Assert.AreEqual(QuestListRules.Tab.Available, QuestListRules.Step(QuestListRules.Tab.All, 1));
        Assert.AreEqual(QuestListRules.Tab.All, QuestListRules.Step(QuestListRules.Tab.Done, 1));
        Assert.AreEqual(QuestListRules.Tab.Done, QuestListRules.Step(QuestListRules.Tab.All, -1));
        Assert.AreEqual(QuestListRules.Tab.All, QuestListRules.Step(QuestListRules.Tab.Available, -1));
    }

    [Test]
    public void 게시판_전체_탭은_받은_퀘스트와_안_받은_퀘스트를_상태순으로_한_목록에()
    {
        NpcQuestCatalog.SetTableForTests("h\n"
            + "B1,수락가능,questboard,1,,kill,Rabbit_BD,1,0,0,,,x\n"
            + "B2,진행중,questboard,1,,kill,Rabbit_BD,3,0,0,,,x\n"
            + "B3,완료가능,questboard,1,,kill,Rabbit_BD,1,0,0,,,x\n"
            + "B4,완료,questboard,1,,kill,Rabbit_BD,1,0,0,,,x\n"
            + "B5,레벨부족,questboard,9,,kill,Rabbit_BD,1,0,0,,,x\n"
            + "N1,다른NPC,ruen,1,,kill,Rabbit_BD,1,0,0,,,x");
        Assert.IsTrue(NpcQuestLog.SeedRecord("B2", 1, 0));
        Assert.IsTrue(NpcQuestLog.SeedRecord("B3", 1, 1));
        Assert.IsTrue(NpcQuestLog.SeedRecord("B4", 2, 1));
        QuestView v = QuestService.Query(QuestEntrance.Board().WithTab(QuestListRules.Tab.All));
        CollectionAssert.AreEqual(new[] { "B3", "B2", "B1", "B5", "B4" }, v.Items.Select(d => d.Id).ToArray());
    }

    [Test]
    public void 아이콘은_보상_아이템_없으면_유형별_기존_아이콘_항상_이름이_있다()
    {
        var withReward = new NpcQuestDef { Id = "a", Kind = "main", RewardItem = "Recall Scroll" };
        Assert.AreEqual("Recall Scroll", QuestWindowText.IconName(withReward, n => n == "Recall Scroll"));
        var missing = new NpcQuestDef { Id = "b", Kind = "side", RewardItem = "NoSuchIcon" };
        Assert.AreEqual(QuestWindowText.KindIcon(missing), QuestWindowText.IconName(missing, n => n != "NoSuchIcon"));
        var names = new HashSet<string>();
        foreach (string kind in new[] { "main", "side", "reward" })
        {
            string icon = QuestWindowText.KindIcon(new NpcQuestDef { Id = "x", Kind = kind });
            Assert.IsFalse(string.IsNullOrEmpty(icon));
            names.Add(icon);
        }
        names.Add(QuestWindowText.KindIcon(new NpcQuestDef { Id = "bounty:x" }));
        Assert.AreEqual(4, names.Count, "유형마다 다른 아이콘");
    }

    [Test]
    public void 상태_머리글과_빈_목록과_입구_제목은_한글()
    {
        foreach (NpcQuestStatus s in new[] { NpcQuestStatus.Ready, NpcQuestStatus.Active, NpcQuestStatus.Available, NpcQuestStatus.LevelLow, NpcQuestStatus.Done })
        {
            Assert.IsTrue(QuestWindowText.StateName(s).Any(c => c >= '가' && c <= '힣'), s.ToString());
        }
        Assert.AreEqual("완료 가능", QuestWindowText.StateName(NpcQuestStatus.Ready));
        Assert.IsTrue(QuestWindowText.Empty(QuestListRules.Tab.All).Any(c => c >= '가' && c <= '힣'));
        Assert.AreEqual("의뢰 게시판", QuestWindowText.EntranceTitle(QuestEntrance.Board()));
        Assert.AreEqual("퀘스트", QuestWindowText.EntranceTitle(null));
    }

    [Test]
    public void 상세에_의뢰인_NPC_이름과_보상은_한글_라벨()
    {
        var d = new NpcQuestDef { Id = "a", Title = "t", Giver = "questboard", ReportTo = "elder", Kind = "side", MinLevel = 1, Objective = NpcQuestObjective.Kill, Target = "Rabbit_BD", Count = 1, Exp = 50, Gold = 20 };
        string text = QuestWindowText.Detail(d, QuestListRules.Tab.Available, NpcQuestStatus.Available);
        StringAssert.Contains("의뢰인: 의뢰 게시판", text);
        StringAssert.Contains("보고: 촌장", text);
        string reward = QuestWindowText.RewardLine(d);
        StringAssert.Contains("경험치 50", reward);
        StringAssert.Contains("20 골드", reward);
        Assert.IsFalse(reward.Contains("EXP"));
    }
}
