using NUnit.Framework;

/// <summary>두 번째 목표(objective2=killElite, EarlyHourPlan R-6): 둘 다 채워야 완료 가능, 세이브 4번째 필드 왕복, 옛 3필드 세이브 호환</summary>
public class NpcQuestExtraObjectiveTests
{
    private const string Header = "id,title,giver,minLevel,requires,objective,target,count,exp,gold,rewardItem,rewardCount,description,source,kind,reportTo,offerLine,progressLine,doneLine,zone,stones,artifact,repeat,sort,objective2,target2,count2\n";
    private const string Csv = Header +
        "X1,쥐,ruen,1,,kill,RatThief_BD,2,10,0,,0,d,,reward,,,,,,,,,,killElite,RatChief_BD,1\n" +
        "X2,일반,ruen,1,,kill,RatThief_BD,1,10,0,,0,d,,reward,,,,,,,,,,,,\n";

    [SetUp]
    public void SetUp()
    {
        NpcQuestCatalog.SetTableForTests(Csv);
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => 99;
        NpcQuestLog.RewardApplier = d => { };
        NpcQuestLog.RewardCapacity = d => true;
        NpcQuestLog.SaveHook = () => { };
    }

    [TearDown]
    public void TearDown()
    {
        NpcQuestCatalog.SetTableForTests(null);
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => LevelSystem.Level;
        NpcQuestLog.RewardApplier = NpcQuestRewards.Apply;
        NpcQuestLog.RewardCapacity = NpcQuestRewards.CanReceive;
    }

    [Test]
    public void 두_번째_목표가_남으면_Ready_가_되지_않는다()
    {
        Assert.IsTrue(NpcQuestLog.Accept("X1"));
        NpcQuestLog.OnKill("RatThief_BD");
        NpcQuestLog.OnKill("RatThief_BD");
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("X1"));
        NpcQuestLog.OnKillElite("Rabbit_BD"); // 다른 정예는 세지 않는다
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("X1"));
        NpcQuestLog.OnKillElite("RatChief_BD");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("X1"));
        Assert.IsTrue(NpcQuestLog.Complete("X1"));
        Assert.AreEqual(1, NpcQuestLog.Progress2(NpcQuestCatalog.Find("X1")));
    }

    [Test]
    public void 정예를_먼저_잡아도_첫_목표가_차야_Ready()
    {
        NpcQuestLog.Accept("X1");
        NpcQuestLog.OnKillElite("RatChief_BD");
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("X1"));
        NpcQuestLog.OnKill("RatThief_BD");
        NpcQuestLog.OnKill("RatThief_BD");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("X1"));
    }

    [Test]
    public void 세이브_왕복은_4필드_옛_세이브는_3필드로_읽는다()
    {
        NpcQuestLog.Accept("X1");
        NpcQuestLog.OnKill("RatThief_BD");
        NpcQuestLog.OnKillElite("RatChief_BD");
        string text = NpcQuestLog.Serialize();
        Assert.AreEqual("X1:1:1:1", text);
        NpcQuestLog.ClearAll();
        NpcQuestLog.Deserialize(text);
        Assert.AreEqual(1, NpcQuestLog.Progress2(NpcQuestCatalog.Find("X1")));
        NpcQuestLog.Deserialize("X1:1:2"); // 4필드 도입 전 세이브: 두 번째 목표 0
        Assert.AreEqual(0, NpcQuestLog.Progress2(NpcQuestCatalog.Find("X1")));
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("X1"));
    }

    [Test]
    public void 두_번째_목표_없는_퀘스트는_현행_3필드_그대로()
    {
        NpcQuestLog.Accept("X2");
        NpcQuestLog.OnKillElite("RatChief_BD");
        Assert.AreEqual("X2:1:0", NpcQuestLog.Serialize());
        Assert.IsFalse(NpcQuestLog.HasExtra(NpcQuestCatalog.Find("X2")));
        NpcQuestLog.OnKill("RatThief_BD");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("X2"));
    }

    [Test]
    public void 실제_Q106_은_쥐_두목_정예를_요구하고_MonsterSheet_와_맞는다()
    {
        NpcQuestCatalog.SetTableForTests(null);
        NpcQuestDef d = NpcQuestCatalog.Find("Q106");
        Assert.IsTrue(NpcQuestLog.HasExtra(d));
        Assert.AreEqual("RatChief_BD", d.Target2);
        MonsterTable.Row row = MonsterTable.Get("RatChief_BD");
        Assert.IsNotNull(row);
        Assert.AreEqual("RatChief_BD", row.killKey, "killElite 메시지 id");
        Assert.IsTrue(TempMobTraits.Has(row.trait, "elite"));
    }

    [Test]
    public void 창_요구_사항은_두_번째_목표를_따로_한_줄로_줄마다_달성_표시()
    {
        NpcQuestDef d = NpcQuestCatalog.Find("X1");
        NpcQuestLog.Accept("X1");
        NpcQuestLog.OnKill("RatThief_BD");
        NpcQuestLog.OnKill("RatThief_BD");
        string text = QuestWindowText.Detail(d, QuestListRules.Tab.Active, NpcQuestLog.StatusOf("X1"));
        string[] lines = text.Split('\n');
        string main = System.Array.Find(lines, l => l.Contains(" 처치 2/2"));
        string extra = System.Array.Find(lines, l => l.Contains(" 처치 0/1"));
        Assert.IsNotNull(main, text);
        Assert.IsNotNull(extra, text);
        Assert.AreNotEqual(main, extra);
        StringAssert.Contains("(달성)", main);
        StringAssert.DoesNotContain("(달성)", extra);
        Assert.AreEqual(QuestService.ObjectiveMain(d) + " · " + QuestService.ObjectiveExtra(d), QuestService.Objective(d), "추적 HUD 한 줄 형식 유지");
    }

    [Test]
    public void 두_번째_목표가_없으면_Objective_는_첫_줄과_같다()
    {
        NpcQuestDef d = NpcQuestCatalog.Find("X2");
        Assert.AreEqual("", QuestService.ObjectiveExtra(d));
        Assert.AreEqual(QuestService.ObjectiveMain(d), QuestService.Objective(d));
    }

    [Test]
    public void 실제_CSV_의_objective2_는_모두_판정_가능한_값이다()
    {
        NpcQuestCatalog.SetTableForTests(null);
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (!string.IsNullOrEmpty(d.Objective2) || !string.IsNullOrEmpty(d.Target2) || d.Count2 > 0)
            {
                Assert.IsTrue(NpcQuestLog.HasExtra(d), d.Id + ": objective2 판정 불가(" + d.Objective2 + "/" + d.Target2 + "/" + d.Count2 + ") — 지원 값은 killElite 뿐");
            }
        }
    }
}
