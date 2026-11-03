using System.Collections.Generic;
using NUnit.Framework;

/// <summary>NPC 퀘스트 신규 목표(killElite·equip·enhance·bestiary): 문법 파싱과 타입별 진행·완료 (Floor1Completion §4-1)</summary>
public class NpcQuestObjectiveTests
{
    private const string Csv =
        "id,title,giver,minLevel,requires,objective,target,count,exp,gold,rewardItem,rewardCount,description\n" +
        "T1,정예,ruen,1,,killElite,WolfBoss,2,10,0,,0,d\n" +
        "T2,아무정예,ruen,1,,killElite,*,1,10,0,,0,d\n" +
        "T3,세트,smith,1,,equip,Grove,3,10,0,,0,d\n" +
        "T4,방어구,merchant,1,,Equip,armor,2,10,0,,0,d\n" +
        "T5,세트종류,smith,1,,equip,sets:3,2,10,0,,0,d\n" +
        "T6,무기강화,smith,1,,enhance,weapon,3,10,0,,0,d\n" +
        "T7,방어구강화,smith,1,,enhance,armor,5,10,0,,0,d\n" +
        "T8,도감,elder,1,,bestiary,any,12,10,0,,0,d\n" +
        "T9,기존킬,ruen,1,,kill,Rabbit_BD,1,10,0,,0,d\n";

    private readonly Dictionary<string, int> _equip = new Dictionary<string, int>();
    private int _seed;
    private int _kinds;

    [SetUp]
    public void SetUp()
    {
        _equip.Clear();
        _seed = 0;
        _kinds = 0;
        NpcQuestCatalog.SetTableForTests(Csv);
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => 99;
        NpcQuestLog.RewardApplier = d => { };
        NpcQuestLog.RewardCapacity = d => true;
        NpcQuestLog.SaveHook = () => { };
        NpcQuestLog.EquipCounter = t => NpcQuestObjectiveRules.TryParseSets(t, out int min)
            ? NpcQuestObjectiveRules.CountSets(_equip, min)
            : _equip.TryGetValue(t, out int n) ? n : 0;
        NpcQuestLog.EnhanceSeed = t => _seed;
        NpcQuestLog.BestiaryCounter = () => _kinds;
    }

    [TearDown]
    public void TearDown()
    {
        NpcQuestCatalog.SetTableForTests(null);
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => LevelSystem.Level;
        NpcQuestLog.RewardApplier = NpcQuestRewards.Apply;
        NpcQuestLog.RewardCapacity = NpcQuestRewards.CanReceive;
        NpcQuestLog.SaveHook = () => { };
        NpcQuestLog.EquipCounter = NpcQuestObjectiveRules.DefaultEquipCount;
        NpcQuestLog.EnhanceSeed = NpcQuestObjectiveRules.DefaultEnhanceSeed;
        NpcQuestLog.BestiaryCounter = NpcQuestObjectiveRules.DefaultBestiaryKinds;
    }

    [Test] public void 신규_목표_타입이_파싱된다_대소문자_무시_기존_타입_유지()
    {
        Assert.AreEqual(9, NpcQuestCatalog.All.Count);
        Assert.AreEqual(NpcQuestObjective.KillElite, NpcQuestCatalog.Find("T1").Objective);
        Assert.AreEqual(NpcQuestObjective.Equip, NpcQuestCatalog.Find("T4").Objective);
        Assert.AreEqual(NpcQuestObjective.Enhance, NpcQuestCatalog.Find("T6").Objective);
        Assert.AreEqual(NpcQuestObjective.Bestiary, NpcQuestCatalog.Find("T8").Objective);
        Assert.AreEqual(NpcQuestObjective.Kill, NpcQuestCatalog.Find("T9").Objective);
    }

    [Test] public void 실제_표는_기존_행을_그대로_파싱한다()
    {
        NpcQuestCatalog.SetTableForTests(null);
        Assert.IsNotNull(NpcQuestCatalog.Find("Q101"));
        Assert.IsNotNull(NpcQuestCatalog.Find("Q602"));
    }

    [Test] public void killElite_는_정예_메시지만_세고_지정_몹_또는_별표만_인정()
    {
        Assert.IsTrue(NpcQuestLog.Accept("T1"));
        NpcQuestLog.OnKill("WolfBoss"); // 일반 처치 메시지는 세지 않는다
        NpcQuestLog.OnKillElite("Rabbit_BD");
        Assert.AreEqual(0, NpcQuestLog.Progress(NpcQuestCatalog.Find("T1")));
        NpcQuestLog.OnKillElite("WolfBoss");
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("T1"));
        NpcQuestLog.OnKillElite("WolfBoss");
        NpcQuestLog.OnKillElite("WolfBoss");
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("T1")), "목표를 넘기지 않는다");
        Assert.IsTrue(NpcQuestLog.Complete("T1"));

        Assert.IsTrue(NpcQuestLog.Accept("T2"));
        NpcQuestLog.OnKillElite("Anything");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T2"));
    }

    [Test] public void killElite_는_수락_전_처치를_세지_않고_세이브_왕복한다()
    {
        NpcQuestLog.OnKillElite("WolfBoss");
        NpcQuestLog.Accept("T1");
        NpcQuestLog.OnKillElite("WolfBoss");
        string text = NpcQuestLog.Serialize();
        Assert.AreEqual("T1:1:1", text);
        NpcQuestLog.ClearAll();
        NpcQuestLog.Deserialize(text);
        Assert.AreEqual(1, NpcQuestLog.Progress(NpcQuestCatalog.Find("T1")));
    }

    [Test] public void equip_세트_부위는_착용_상태로_계산하고_해제하면_줄어든다()
    {
        Assert.IsTrue(NpcQuestLog.Accept("T3"));
        _equip["Grove"] = 2;
        _equip["Bog"] = 3; // 다른 세트는 무관
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("T3")));
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("T3"));
        _equip["Grove"] = 3;
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T3"));
        _equip["Grove"] = 2;
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("T3"));
        _equip["Grove"] = 5;
        Assert.AreEqual(3, NpcQuestLog.Progress(NpcQuestCatalog.Find("T3")), "목표까지만 센다");
        Assert.IsTrue(NpcQuestLog.Complete("T3"));
        _equip["Grove"] = 0;
        Assert.AreEqual(NpcQuestStatus.Done, NpcQuestLog.StatusOf("T3"), "완료 뒤에는 유지");
    }

    [Test] public void equip_방어구와_세트종류_문법()
    {
        Assert.IsTrue(NpcQuestObjectiveRules.TryParseSets("sets:3", out int min));
        Assert.AreEqual(3, min);
        Assert.IsFalse(NpcQuestObjectiveRules.TryParseSets("Grove", out _));
        Assert.IsFalse(NpcQuestObjectiveRules.TryParseSets("sets:0", out _));
        Assert.IsFalse(NpcQuestObjectiveRules.TryParseSets("sets:x", out _));

        NpcQuestLog.Accept("T5");
        _equip["A"] = 3;
        _equip["B"] = 2;
        _equip["C"] = 4;
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("T5")));
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T5"));
        _equip["B"] = 1;
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("T5")), "A·C 두 세트면 충분");
        _equip["C"] = 2;
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("T5"));

        NpcQuestLog.Accept("T4");
        _equip["armor"] = 1;
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("T4"));
        _equip["armor"] = 2;
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T4"));
    }

    [Test] public void enhance_는_수락_시_착용품_단계부터_시작하고_성공_시_최고값을_저장()
    {
        _seed = 1;
        Assert.IsTrue(NpcQuestLog.Accept("T6"));
        Assert.AreEqual(1, NpcQuestLog.Progress(NpcQuestCatalog.Find("T6")));
        NpcQuestLog.OnEnhance("Chest", 3); // 방어구는 무기 목표에 무관
        Assert.AreEqual(1, NpcQuestLog.Progress(NpcQuestCatalog.Find("T6")));
        NpcQuestLog.OnEnhance("Weapon", 2);
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("T6")));
        NpcQuestLog.OnEnhance("Weapon", 1); // 낮은 값은 무시
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("T6")));
        Assert.IsFalse(NpcQuestLog.Complete("T6"));
        NpcQuestLog.OnEnhance("Weapon", 4);
        Assert.AreEqual(3, NpcQuestLog.Progress(NpcQuestCatalog.Find("T6")), "목표 단계까지만");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T6"));
        Assert.IsTrue(NpcQuestLog.Complete("T6"));
    }

    [Test] public void enhance_방어구_목표는_무기를_무시하고_이미_충분하면_바로_완료_가능()
    {
        _seed = 5;
        NpcQuestLog.Accept("T7");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T7"));
        Assert.IsTrue(NpcQuestObjectiveRules.EnhanceMatches("armor", "Head"));
        Assert.IsFalse(NpcQuestObjectiveRules.EnhanceMatches("armor", "Weapon"));
        Assert.IsFalse(NpcQuestObjectiveRules.EnhanceMatches("armor", "Ring"));
        Assert.IsTrue(NpcQuestObjectiveRules.EnhanceMatches("any", "Ring"));
        Assert.IsTrue(NpcQuestObjectiveRules.EnhanceMatches("weapon", "Weapon"));
    }

    [Test] public void bestiary_는_도감_종류_수로_계산한다()
    {
        NpcQuestLog.Accept("T8");
        _kinds = 11;
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("T8"));
        _kinds = 12;
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T8"));
        _kinds = 30;
        Assert.AreEqual(12, NpcQuestLog.Progress(NpcQuestCatalog.Find("T8")));
        Assert.IsTrue(NpcQuestLog.Complete("T8"));
    }

    [Test] public void 디버그_채우기는_계산형_목표를_건드리지_않고_저장형은_채운다()
    {
        NpcQuestLog.Accept("T1");
        NpcQuestLog.Accept("T3");
        NpcQuestLog.DebugFillActive();
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T1"));
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("T3"));
    }

    [Test] public void 기존_처치_목표는_정예_메시지에_반응하지_않는다()
    {
        NpcQuestLog.Accept("T9");
        NpcQuestLog.OnKillElite("Rabbit_BD");
        Assert.AreEqual(0, NpcQuestLog.Progress(NpcQuestCatalog.Find("T9")));
        NpcQuestLog.OnKill("Rabbit_BD");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("T9"));
    }
}
