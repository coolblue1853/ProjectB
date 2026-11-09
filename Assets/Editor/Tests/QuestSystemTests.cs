using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>REWORK-5 P0~P1: 확장 열 파싱·QuestService·게시판 퀘스트 CSV 이행(U7 수집 실시간)·구 저장 이행·강화석/아티팩트 보상</summary>
public class QuestSystemTests
{
    private sealed class Catalog : IItemCatalog
    {
        public ItemDef Get(string id) => new ItemDef { Id = id, MaxStack = id == "RustedSword" ? 1 : 99 };
        public bool Exists(string id) => true;
    }

    private static readonly IItemCatalog Items = new Catalog();
    private int _level;
    private readonly Dictionary<string, int> _have = new Dictionary<string, int>();
    private readonly List<string> _removed = new List<string>();
    private readonly List<NpcQuestDef> _rewards = new List<NpcQuestDef>();

    [SetUp]
    public void SetUp()
    {
        _level = 1;
        _have.Clear();
        _removed.Clear();
        _rewards.Clear();
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => _level;
        NpcQuestLog.ItemCounter = id => _have.TryGetValue(id, out int n) ? n : 0;
        NpcQuestLog.ItemRemover = (id, n) => { _removed.Add(id + "x" + n); _have[id] = _have.TryGetValue(id, out int h) ? h - n : 0; };
        NpcQuestLog.RewardApplier = d => _rewards.Add(d);
        NpcQuestLog.RewardCapacity = d => true;
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
        NpcQuestLog.ItemRemover = (id, n) => { };
        NpcQuestLog.RewardApplier = NpcQuestRewards.Apply;
        NpcQuestLog.RewardCapacity = NpcQuestRewards.CanReceive;
        NpcQuestLog.SaveHook = () => { };
        QuestService.BountyAvailable = null;
        QuestService.BountyActive = null;
        QuestService.Warn = msg => PickupLog.Warn(msg);
    }

    // ── P0 파서 ──

    [Test]
    public void 구_13열_CSV도_파싱되고_확장_필드는_기본값()
    {
        const string csv = "id,title,giver,minLevel,requires,objective,target,count,exp,gold,rewardItem,rewardCount,description\n" +
                           "T1,제목,ruen,2,,kill,Rabbit_BD,3,10,5,Potion,1,설명";
        List<NpcQuestDef> list = NpcQuestCatalog.Parse(csv);
        Assert.AreEqual(1, list.Count);
        NpcQuestDef d = list[0];
        Assert.AreEqual("ruen", d.Giver);
        Assert.AreEqual("", d.Source);
        Assert.AreEqual("", d.OfferLine);
        Assert.AreEqual(0, d.Stones);
        Assert.AreEqual("ruen", d.ReportNpc, "reportTo 빈칸 = giver");
        Assert.AreEqual("npc", QuestService.SourceOf(d));
    }

    [Test]
    public void 확장_열이_읽히고_처치_대상_목록과_첫_대상()
    {
        const string csv = "h\n" +
                           "T2,제목,questboard,1,,kill,A_BD|B_BD,4,0,100,,,설명,board,side,elder,수락,진행,완료,대장간,2,StarBurst,none,7,collect,X,2";
        NpcQuestDef d = NpcQuestCatalog.Parse(csv)[0];
        Assert.AreEqual("board", d.Source);
        Assert.AreEqual("side", d.Kind);
        Assert.AreEqual("elder", d.ReportNpc);
        Assert.AreEqual("수락", d.OfferLine);
        Assert.AreEqual("진행", d.ProgressLine);
        Assert.AreEqual("완료", d.DoneLine);
        Assert.AreEqual("대장간", d.Zone);
        Assert.AreEqual(2, d.Stones);
        Assert.AreEqual("StarBurst", d.Artifact);
        Assert.AreEqual(7, d.Sort);
        Assert.AreEqual(2, d.Count2);
        Assert.AreEqual("A_BD", d.FirstTarget);
        Assert.IsTrue(d.MatchesTarget("B_BD"));
        Assert.IsFalse(d.MatchesTarget("C_BD"));
        Assert.AreEqual("board", QuestService.SourceOf(d));
    }

    [Test]
    public void 기존_퀘스트_값은_그대로()
    {
        NpcQuestCatalog.SetTableForTests(null);
        NpcQuestDef q = NpcQuestCatalog.Find("Q102");
        Assert.AreEqual("ruen", q.Giver);
        Assert.AreEqual(NpcQuestObjective.Kill, q.Objective);
        Assert.AreEqual("FlowerMole_BD", q.Target);
        Assert.AreEqual(8, q.Count);
        Assert.AreEqual(600, q.Exp);
        Assert.AreEqual("LeafAdae", q.RewardItem);
        Assert.AreEqual("", q.OfferLine, "기존 행은 확장 열이 비어 있다");
        Assert.AreEqual("Q101", q.Requires);
    }

    // ── P1 게시판 퀘스트 CSV ──

    [Test]
    public void 게시판_퀘스트는_구_정의와_보상_목표가_같다()
    {
        foreach (QuestCatalog.Def old in QuestCatalog.Chain.Concat(QuestCatalog.Side).Concat(QuestCatalog.Story))
        {
            NpcQuestDef d = NpcQuestCatalog.Find(old.id);
            Assert.IsNotNull(d, old.id);
            Assert.AreEqual(old.count, d.Count, old.id + " count");
            Assert.AreEqual(old.gold, d.Gold, old.id + " gold");
            Assert.AreEqual(old.stones, d.Stones, old.id + " stones");
            Assert.AreEqual(old.exp, d.Exp, old.id + " exp");
            Assert.AreEqual(old.requires ?? "", d.Requires, old.id + " requires");
            Assert.AreEqual(old.artifact.ToString() == "None" ? ArtifactId.None : old.artifact, NpcQuestRewards.ParseArtifact(d.Artifact), old.id + " artifact");
            Assert.AreEqual(old.id.StartsWith("st_") ? "elder" : "questboard", d.Giver, old.id + " giver");
            if (old.message == "Kill")
            {
                Assert.AreEqual(NpcQuestObjective.Kill, d.Objective, old.id);
                CollectionAssert.AreEquivalent(old.targets, d.Target.Split('|'), old.id + " targets");
            }
        }
        Assert.AreEqual(QuestMigration.LegacyIds.Length, QuestCatalog.Chain.Count + QuestCatalog.Side.Count + QuestCatalog.Story.Count);
    }

    [Test]
    public void 게시판_퀘스트_한글_대사_열이_채워져_있다()
    {
        foreach (string id in QuestMigration.LegacyIds)
        {
            NpcQuestDef d = NpcQuestCatalog.Find(id);
            Assert.IsNotEmpty(d.OfferLine, id + " offerLine");
            Assert.IsNotEmpty(d.DoneLine, id + " doneLine");
            Assert.IsFalse(d.Title.Any(c => c < 128 && char.IsLetter(c) && c != 'I' && c != 'V'), id + " 제목에 영문 없음(로마 숫자 제외): " + d.Title);
            foreach (string page in NpcQuestVoice.Offer(d))
            {
                Assert.LessOrEqual(page.Count(c => c == '\n'), 1, id + " 한 페이지 2줄 이내");
            }
        }
    }

    [Test]
    public void U7_땔감은_수락_전_가방_보유량부터_센다()
    {
        _have["Wood"] = 7;
        Assert.IsTrue(QuestService.Accept("s_wood"));
        NpcQuestDef d = NpcQuestCatalog.Find("s_wood");
        Assert.AreEqual(NpcQuestObjective.Collect, d.Objective);
        Assert.AreEqual(7, NpcQuestLog.Progress(d), "수락 직후 0 이 아니라 가방 7개");
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf(d));
        _have["Wood"] = 12;
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf(d));
        Assert.AreEqual(10, NpcQuestLog.Progress(d), "상한은 필요 수량");
        Assert.IsTrue(QuestService.Report("s_wood"));
        CollectionAssert.AreEqual(new[] { "Woodx10" }, _removed);
        Assert.AreEqual(1, _rewards.Count);
    }

    [Test]
    public void U7_치유사는_가방에_이미_6개면_수락하자마자_완료_가능()
    {
        _have["GreenHurb"] = 6;
        Assert.IsTrue(QuestService.Accept("s_herb"));
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("s_herb"));
        StringAssert.Contains("6/6", QuestService.Objective(NpcQuestCatalog.Find("s_herb")));
    }

    [Test]
    public void 토끼_소동은_두_토끼_종류를_세고_다른_몹은_세지_않는다()
    {
        Assert.IsTrue(QuestService.Accept("q_rabbits"));
        NpcQuestLog.OnKill("Rabbit_BD");
        NpcQuestLog.OnKill("StoneRabbit_BD");
        NpcQuestLog.OnKill("FlowerMole_BD");
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("q_rabbits")));
    }

    [Test]
    public void 대장장이의_제자는_제작_Any_메시지만_센다()
    {
        Assert.IsTrue(QuestService.Accept("s_craft"));
        NpcQuestLog.OnCraft("WolfCap");   // 결과 이름 메시지는 세지 않는다(Any 가 따로 온다)
        NpcQuestLog.OnCraft("Any");
        NpcQuestLog.OnCraft("Any");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("s_craft"));
    }

    [Test]
    public void 체인은_선행_완료_후에만_열린다()
    {
        Assert.AreEqual(NpcQuestStatus.Locked, NpcQuestLog.StatusOf("q_moles"));
        NpcQuestLog.Deserialize("q_rabbits:2:10");
        Assert.AreEqual(NpcQuestStatus.Available, NpcQuestLog.StatusOf("q_moles"));
    }

    // ── 보상 ──

    [Test]
    public void 강화석_보상도_가방_공간_검사에_들어간다()
    {
        NpcQuestDef d = NpcQuestCatalog.Find("q_rabbits"); // 강화석 1, 아이템 보상 없음
        var full = new SlotContainer(Items, 2, 2);
        full.PutAt(0, new ItemStack("RustedSword", 1));
        full.PutAt(1, new ItemStack("RustedSword", 1));
        Assert.IsFalse(NpcQuestRewards.CanFitReward(d, full, Items), "빈 칸 없음");
        var roomy = new SlotContainer(Items, 2, 2);
        Assert.IsTrue(NpcQuestRewards.CanFitReward(d, roomy, Items));
        Assert.AreEqual(ArtifactId.StarBurst, NpcQuestRewards.ParseArtifact("StarBurst"));
        Assert.AreEqual(ArtifactId.None, NpcQuestRewards.ParseArtifact(""));
        Assert.AreEqual(ArtifactId.None, NpcQuestRewards.ParseArtifact("없는이름"));
    }

    [Test]
    public void 가방이_가득이면_Report_가_보류하고_진행을_보존한다()
    {
        _have["Wood"] = 10;
        QuestService.Accept("s_wood");
        NpcQuestLog.RewardCapacity = d => false;
        Assert.IsFalse(QuestService.Report("s_wood"));
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("s_wood"));
        Assert.AreEqual(0, _removed.Count);
        NpcQuestLog.RewardCapacity = d => true;
        Assert.IsTrue(QuestService.Report("s_wood"));
    }

    // ── QuestService ──

    [Test]
    public void Query_게시판_필터와_탭()
    {
        var f = new QuestFilter { Tab = QuestListRules.Tab.Available, Giver = "questboard" };
        List<string> ids = QuestService.Query(f).Items.Select(d => d.Id).ToList();
        CollectionAssert.AreEquivalent(new[] { "q_rabbits", "s_wood", "s_herb", "s_craft" }, ids);
        Assert.IsFalse(ids.Contains("q_moles"), "선행 미완 = Locked 는 목록에 없다");
        QuestService.Accept("s_wood");
        f.Tab = QuestListRules.Tab.Active;
        CollectionAssert.AreEqual(new[] { "s_wood" }, QuestService.Query(f).Items.Select(d => d.Id).ToArray());
        f.Tab = QuestListRules.Tab.Done;
        Assert.AreEqual(0, QuestService.Query(f).Items.Count);
        var src = new QuestFilter { Source = "board", Tab = QuestListRules.Tab.Active };
        Assert.AreEqual(1, QuestService.Query(src).Items.Count);
    }

    [Test]
    public void Query_진행_중_탭은_완료_가능이_맨_위()
    {
        _have["Wood"] = 10;
        QuestService.Accept("s_herb");
        QuestService.Accept("s_wood");
        List<string> ids = QuestService.Query(new QuestFilter { Tab = QuestListRules.Tab.Active }).Items.Select(d => d.Id).ToList();
        Assert.AreEqual("s_wood", ids[0], "땔감은 이미 완료 가능");
    }

    [Test]
    public void CanAccept_사유와_포기()
    {
        string reason;
        Assert.IsFalse(QuestService.CanAccept(NpcQuestCatalog.Find("q_moles"), out reason));
        StringAssert.Contains("먼저", reason);
        Assert.IsTrue(QuestService.CanAccept(NpcQuestCatalog.Find("s_wood"), out reason));
        _level = 1;
        NpcQuestCatalog.SetTableForTests("h\nL1,레벨,ruen,5,,kill,Rabbit_BD,1,0,0,,,x");
        Assert.IsFalse(QuestService.CanAccept(NpcQuestCatalog.Find("L1"), out reason));
        StringAssert.Contains("Lv 5", reason);
        NpcQuestCatalog.SetTableForTests(null);

        QuestService.Accept("s_wood");
        Assert.IsFalse(QuestService.CanAccept(NpcQuestCatalog.Find("s_wood"), out reason));
        Assert.IsTrue(QuestService.Abandon("s_wood"));
        Assert.AreEqual(NpcQuestStatus.Available, NpcQuestLog.StatusOf("s_wood"), "포기 후 다시 받을 수 있다");
        Assert.IsFalse(QuestService.Abandon("s_wood"), "진행 기록이 없으면 false");
        NpcQuestLog.Deserialize("s_herb:2:6");
        Assert.IsFalse(QuestService.Abandon("s_herb"), "완료분은 포기 불가");
        Assert.AreEqual(NpcQuestStatus.Done, NpcQuestLog.StatusOf("s_herb"));
    }

    [Test]
    public void Objective_와_ReportTarget_문장()
    {
        NpcQuestDef wood = NpcQuestCatalog.Find("s_wood");
        _have["Wood"] = 4;
        QuestService.Accept("s_wood");
        StringAssert.EndsWith(" 수집 4/10", QuestService.Objective(wood));
        Assert.AreEqual("의뢰 게시판", QuestService.ReportTarget(wood));
        Assert.AreEqual("촌장", QuestService.ReportTarget(NpcQuestCatalog.Find("st_father1")));
        StringAssert.EndsWith(" 처치 0/10", QuestService.Objective(NpcQuestCatalog.Find("q_rabbits")));
        StringAssert.EndsWith(" 제작 0/2", QuestService.Objective(NpcQuestCatalog.Find("s_craft")));
    }

    // ── 구 저장 이행 ──

    [Test]
    public void 이행은_완료와_진행_중을_옮기고_수집은_진행_0()
    {
        var legacy = new List<QuestMigration.Legacy>
        {
            new QuestMigration.Legacy { Id = "q_rabbits", State = QuestMigration.LegacyState.Done },
            new QuestMigration.Legacy { Id = "q_moles", State = QuestMigration.LegacyState.Active, Progress = 5 },
            new QuestMigration.Legacy { Id = "s_wood", State = QuestMigration.LegacyState.Active, Progress = 3 },
            new QuestMigration.Legacy { Id = "q_terror", State = QuestMigration.LegacyState.None },
            new QuestMigration.Legacy { Id = "없는id", State = QuestMigration.LegacyState.Done },
        };
        Assert.AreEqual(3, QuestMigration.Apply(legacy));
        Assert.AreEqual("q_rabbits:2:10;q_moles:1:5;s_wood:1:0", NpcQuestLog.Serialize());
        Assert.IsTrue(NpcQuestLog.IsDone("q_rabbits"));
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("q_moles"));
    }

    [Test]
    public void 이행은_기존_npcq_기록을_덮지_않고_두_번_해도_같다()
    {
        NpcQuestLog.Deserialize("s_herb:1:2");
        var legacy = new List<QuestMigration.Legacy>
        {
            new QuestMigration.Legacy { Id = "s_herb", State = QuestMigration.LegacyState.Done },
            new QuestMigration.Legacy { Id = "q_rabbits", State = QuestMigration.LegacyState.Active, Progress = 99 },
        };
        Assert.AreEqual(1, QuestMigration.Apply(legacy));
        string once = NpcQuestLog.Serialize();
        Assert.AreEqual("q_rabbits:1:10;s_herb:1:2", once, "진행값은 필요 수량으로 자르고 기존 기록 우선");
        Assert.AreEqual(0, QuestMigration.Apply(legacy));
        Assert.AreEqual(once, NpcQuestLog.Serialize());
    }

    [Test]
    public void 이행_버전_훅과_Needed()
    {
        int saved = 0;
        QuestMigration.VersionGetter = () => saved;
        QuestMigration.VersionSetter = v => saved = v;
        try
        {
            Assert.IsTrue(QuestMigration.Needed());
            QuestMigration.VersionSetter(QuestMigration.Version);
            Assert.IsFalse(QuestMigration.Needed());
            QuestMigration.RunOnce(null); // 저널 없으면 이행했다고 표시하지 않는다(이미 표시된 상태라 변화 없음)
            Assert.AreEqual(QuestMigration.Version, saved);
            saved = 0;
            QuestMigration.RunOnce(null);
            Assert.AreEqual(0, saved, "저널이 없으면 버전을 올리지 않는다");
        }
        finally
        {
            QuestMigration.VersionGetter = () => SaveUtil.LoadOr(QuestMigration.SaveKey, 0);
            QuestMigration.VersionSetter = v => ES3.Save(QuestMigration.SaveKey, v);
        }
    }

    [Test]
    public void 세이브_직렬화는_새_행도_왕복한다()
    {
        NpcQuestLog.Deserialize("st_father1:2:1;st_father2:1:4;s_craft:1:1;q_rabbits:1:3");
        string s = NpcQuestLog.Serialize();
        NpcQuestLog.Deserialize(s);
        Assert.AreEqual(s, NpcQuestLog.Serialize());
        Assert.IsTrue(NpcQuestLog.IsDone("st_father1"));
    }
}
