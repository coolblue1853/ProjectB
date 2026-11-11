using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>REWORK-5 P2~P4: 창 문구·유형 태그·추적 HUD 형식·접기 저장·입구 필터·현상 추적</summary>
public class QuestWindowTests
{
    private int _level;
    private bool _foldSaved;
    private readonly List<NpcQuestDef> _bountyActive = new List<NpcQuestDef>();

    [SetUp]
    public void SetUp()
    {
        _level = 5;
        _foldSaved = false;
        _bountyActive.Clear();
        NpcQuestLog.ClearAll();
        NpcQuestTrack.Reset();
        NpcQuestLog.LevelGetter = () => _level;
        NpcQuestLog.ItemCounter = id => 0;
        NpcQuestLog.SaveHook = () => { };
        QuestService.BountyAvailable = () => new List<NpcQuestDef>();
        QuestService.BountyActive = () => new List<NpcQuestDef>(_bountyActive);
        QuestService.Warn = msg => { };
        NpcQuestTrack.FoldSaver = v => _foldSaved = v;
        NpcQuestTrack.Saver = t => { };
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
        NpcQuestTrack.RestoreDefaults();
        NpcQuestTrack.Reset();
    }

    private static NpcQuestDef Def(string id, string kind, string giver = "ruen", string reportTo = "", string zone = "")
    {
        return new NpcQuestDef { Id = id, Title = "제목" + id, Giver = giver, ReportTo = reportTo, Kind = kind, Zone = zone, MinLevel = 3, Objective = NpcQuestObjective.Kill, Target = "Rabbit_BD", Count = 10, Exp = 50, Gold = 20, Description = "요약 " + id };
    }

    [Test]
    public void 유형_태그는_주_서_보_현()
    {
        Assert.AreEqual("주", QuestService.KindTag(Def("a", "main")));
        Assert.AreEqual("서", QuestService.KindTag(Def("a", "side")));
        Assert.AreEqual("보", QuestService.KindTag(Def("a", "reward")));
        Assert.AreEqual("보", QuestService.KindTag(Def("a", "")), "빈칸 = 보상");
        Assert.AreEqual("현", QuestService.KindTag(Def("bounty:x", "")));
    }

    [Test]
    public void 상세_문구에_요약_요구사항_보상이_있다()
    {
        NpcQuestDef d = Def("a", "main");
        string text = QuestWindowText.Detail(d, QuestListRules.Tab.Active, NpcQuestStatus.Active);
        StringAssert.Contains("요약 a", text);
        StringAssert.Contains("요구 사항", text);
        StringAssert.Contains("0/10", text);
        StringAssert.Contains("[주 메인]", text);
        StringAssert.Contains("EXP 50", QuestWindowText.RewardLine(d));
        StringAssert.Contains("20 G", QuestWindowText.RewardLine(d));
    }

    [Test]
    public void 보상_줄은_강화석_성물을_한글로()
    {
        NpcQuestDef d = Def("a", "side");
        d.Exp = 0;
        d.Gold = 0;
        d.Stones = 3;
        d.Artifact = "StarBurst";
        string line = QuestWindowText.RewardLine(d);
        StringAssert.Contains("강화석 3", line);
        StringAssert.Contains("성물 해금", line);
        d.Stones = 0;
        d.Artifact = "";
        Assert.AreEqual("-", QuestWindowText.RewardLine(d));
    }

    [Test]
    public void 안내_문구_보고_대상과_레벨_부족()
    {
        NpcQuestDef d = Def("a", "reward", giver: "ruen", reportTo: "elder");
        string guide = QuestWindowText.Guide(d, QuestListRules.Tab.Active, NpcQuestStatus.Ready, false, true, 2);
        StringAssert.Contains("촌장에게 가서 보고하세요", guide);
        StringAssert.Contains("2/" + NpcQuestTrack.Max, guide);
        string low = QuestWindowText.Guide(d, QuestListRules.Tab.Available, NpcQuestStatus.LevelLow, false, false, 0);
        StringAssert.Contains("Lv 3 부터", low);
        string entrance = QuestWindowText.Guide(d, QuestListRules.Tab.Available, NpcQuestStatus.Available, true, false, 0);
        StringAssert.Contains("수락할 수 있습니다", entrance);
    }

    [Test]
    public void 목록_줄은_완료_가능에_물음표_수락_가능에_느낌표_추적에_별()
    {
        NpcQuestDef d = Def("a", "main");
        StringAssert.Contains("?", QuestWindowText.RowLine(d, QuestListRules.Tab.Active, NpcQuestStatus.Ready, false));
        StringAssert.Contains("!", QuestWindowText.RowLine(d, QuestListRules.Tab.Available, NpcQuestStatus.Available, false));
        StringAssert.Contains("*", QuestWindowText.RowLine(d, QuestListRules.Tab.Active, NpcQuestStatus.Active, true));
        StringAssert.Contains("(Lv 3)", QuestWindowText.RowLine(d, QuestListRules.Tab.Available, NpcQuestStatus.LevelLow, false));
        StringAssert.Contains("주", QuestWindowText.RowLine(d, QuestListRules.Tab.Active, NpcQuestStatus.Active, false));
    }

    [Test]
    public void 추적_HUD_형식_헤더_유형_목표_구역()
    {
        var all = new List<NpcQuestDef> { Def("a", "main", zone: "샛길") };
        string text = NpcQuestTracker.BuildText(all, all, "", false);
        StringAssert.Contains("▼ 퀘스트 (1)", text);
        StringAssert.Contains("[주] 제목a", text);
        StringAssert.Contains("@샛길", text);
        StringAssert.Contains("0/10", text);
    }

    [Test]
    public void 추적_HUD_상한_초과는_외_n건_접으면_헤더만()
    {
        var all = new List<NpcQuestDef>();
        for (int i = 0; i < NpcQuestTrack.Max + 1; i++)
        {
            all.Add(Def("q" + i, "reward"));
        }
        List<NpcQuestDef> shown = NpcQuestTrack.Tracked(all);
        Assert.AreEqual(NpcQuestTrack.Max, shown.Count);
        string text = NpcQuestTracker.BuildText(all, shown, "세트줄", false);
        StringAssert.Contains("…외 1건", text);
        StringAssert.Contains("세트줄", text);
        string folded = NpcQuestTracker.BuildText(all, shown, "세트줄", true);
        StringAssert.Contains("▶ 퀘스트 (" + all.Count + ")", folded);
        StringAssert.DoesNotContain("제목q0", folded);
        StringAssert.DoesNotContain("세트줄", folded);
    }

    [Test]
    public void 접기_상태는_저장_훅으로_기록된다()
    {
        Assert.IsFalse(NpcQuestTrack.HudFolded);
        NpcQuestTrack.SetHudFolded(true);
        Assert.IsTrue(NpcQuestTrack.HudFolded);
        Assert.IsTrue(_foldSaved);
        NpcQuestTrack.FoldLoader = () => false;
        NpcQuestTrack.LoadFold();
        Assert.IsFalse(NpcQuestTrack.HudFolded);
        Assert.AreEqual("quest.hudFold", NpcQuestTrack.FoldKey);
    }

    [Test]
    public void 추적_저장_문자열은_현상_id도_읽는다()
    {
        NpcQuestTrack.Deserialize("bounty:b1;nope");
        var active = new List<NpcQuestDef> { Def("bounty:b1", "") };
        Assert.IsTrue(NpcQuestTrack.IsTracked("bounty:b1", active));
        Assert.AreEqual(1, NpcQuestTrack.Tracked(active).Count);
    }

    [Test]
    public void 진행_중_전부는_CSV_퀘스트와_현상을_합친다()
    {
        NpcQuestCatalog.SetTableForTests("h\nT1,제목,ruen,1,,kill,Rabbit_BD,3,0,0,,,x");
        Assert.IsTrue(QuestService.Accept("T1"));
        _bountyActive.Add(Def("bounty:b1", ""));
        List<string> ids = QuestService.ActiveAll().Select(d => d.Id).ToList();
        CollectionAssert.AreEqual(new[] { "T1", "bounty:b1" }, ids);
        Assert.AreEqual(2, NpcQuestTrack.Tracked(QuestService.ActiveAll()).Count, "현상도 추적 상한에 포함");
    }

    [Test]
    public void 입구_필터_보고_대상이_다른_퀘스트도_ReportToo_로_보인다()
    {
        NpcQuestCatalog.SetTableForTests("h\nB1,게시판,questboard,1,,kill,Rabbit_BD,1,0,0,,,x\nB2,촌장,elder,1,,kill,Rabbit_BD,1,0,0,,,x,board,,questboard");
        var plain = new QuestFilter { Giver = "questboard" };
        CollectionAssert.AreEqual(new[] { "B1" }, QuestService.Query(plain).Items.Select(d => d.Id).ToArray());
        var wide = new QuestFilter { Giver = "questboard", ReportToo = true };
        CollectionAssert.AreEquivalent(new[] { "B1", "B2" }, QuestService.Query(wide).Items.Select(d => d.Id).ToArray());
    }

    [Test]
    public void 입구에서_보고_가능_여부는_보고_대상으로_판단()
    {
        NpcQuestDef d = Def("a", "", giver: "ruen", reportTo: "elder");
        Assert.IsTrue(QuestEntrance.CanReportHere(d, new QuestFilter { Giver = "elder" }));
        Assert.IsFalse(QuestEntrance.CanReportHere(d, new QuestFilter { Giver = "ruen" }));
        Assert.IsTrue(QuestEntrance.CanReportHere(d, null));
        Assert.AreEqual("questboard", QuestEntrance.Board().Giver);
        Assert.AreEqual("bounty", QuestEntrance.Kiosk().Source);
    }

    [Test]
    public void 입구_HasAnything_수락_가능_또는_진행_중()
    {
        NpcQuestCatalog.SetTableForTests("h\nB1,게시판,questboard,1,,kill,Rabbit_BD,1,0,0,,,x");
        Assert.IsTrue(QuestEntrance.HasAnything(QuestEntrance.Board()));
        QuestService.Accept("B1");
        Assert.IsTrue(QuestEntrance.HasAnything(QuestEntrance.Board()), "진행 중도 입구에 보인다");
        NpcQuestCatalog.SetTableForTests("h\nZ1,다른,ruen,1,,kill,Rabbit_BD,1,0,0,,,x");
        Assert.IsFalse(QuestEntrance.HasAnything(QuestEntrance.Board()));
    }

    [Test]
    public void 빈_목록_안내는_한글()
    {
        foreach (QuestListRules.Tab t in new[] { QuestListRules.Tab.Available, QuestListRules.Tab.Active, QuestListRules.Tab.Done })
        {
            Assert.IsTrue(QuestWindowText.Empty(t).Any(c => c >= '가' && c <= '힣'));
        }
    }
}
