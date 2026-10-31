using System.Collections.Generic;
using NUnit.Framework;
using ProjectB.EditorTools;

/// <summary>NPC 퀘스트 v1: CSV 파싱·대상 id 존재·상태 전이·선행/레벨 조건·보상·세이브 직렬화 (ClassicMapleDesign.md 10절)</summary>
public class NpcQuestTests
{
    private sealed class RewardCatalog : IItemCatalog
    {
        public ItemDef Get(string id) => new ItemDef { Id = id, MaxStack = 99 };
        public bool Exists(string id) => true;
    }

    private static readonly IItemCatalog RewardItems = new RewardCatalog();
    private int _level;
    private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
    private readonly List<NpcQuestDef> _rewards = new List<NpcQuestDef>();
    private readonly List<string> _removed = new List<string>();
    private int _records;

    [SetUp]
    public void SetUp()
    {
        _level = 1;
        _items.Clear();
        _rewards.Clear();
        _removed.Clear();
        _records = 0;
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => _level;
        NpcQuestLog.ItemCounter = id => _items.TryGetValue(id, out int n) ? n : 0;
        NpcQuestLog.ItemRemover = (id, n) =>
        {
            _removed.Add(id + "x" + n);
            _items[id] = _items.TryGetValue(id, out int have) ? have - n : 0;
        };
        NpcQuestLog.RecordCounter = () => _records;
        NpcQuestLog.RewardApplier = d => _rewards.Add(d);
        NpcQuestLog.RewardCapacity = d => true;
        NpcQuestLog.SaveHook = () => { };
    }

    [TearDown]
    public void TearDown()
    {
        NpcQuestLog.ClearAll();
        NpcQuestLog.LevelGetter = () => LevelSystem.Level;
        NpcQuestLog.ItemCounter = id => InventoryManager.instance != null ? InventoryManager.instance.CountItem(id) : 0;
        NpcQuestLog.RecordCounter = () => 0;
        NpcQuestLog.RewardApplier = NpcQuestRewards.Apply;
        NpcQuestLog.RewardCapacity = NpcQuestRewards.CanReceive;
        NpcQuestLog.SaveHook = () => { };
    }

    [Test] public void CSV_24개_파싱_EXP_합계_28425()
    {
        Assert.AreEqual(24, NpcQuestCatalog.All.Count);
        int exp = 0;
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            exp += d.Exp;
        }
        Assert.AreEqual(28425, exp);
    }

    [Test] public void 모든_대상_id가_실제로_존재한다()
    {
        var items = new HashSet<string>();
        foreach (ItemRow r in ItemTable.Load().Rows)
        {
            items.Add(r.Name);
        }
        var ids = new HashSet<string>();
        string[] npcs = { "elder", "smith", "merchant", "ruen", "sena", "eve" };
        string[] visits = { "forest_night_post", "deep_nest", "tower_altar" };
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            Assert.IsTrue(ids.Add(d.Id), "중복 id " + d.Id);
            Assert.Contains(d.Giver, npcs, d.Id + " 의뢰 NPC");
            if (!string.IsNullOrEmpty(d.Requires))
            {
                Assert.IsNotNull(NpcQuestCatalog.Find(d.Requires), d.Id + " 선행 " + d.Requires);
            }
            if (!string.IsNullOrEmpty(d.RewardItem))
            {
                Assert.IsTrue(items.Contains(d.RewardItem), d.Id + " 보상 아이템 " + d.RewardItem);
                Assert.Greater(d.RewardCount, 0, d.Id);
            }
            switch (d.Objective)
            {
                case NpcQuestObjective.Kill:
                    Assert.IsNotNull(MonsterTable.Get(d.Target), d.Id + " 몬스터 " + d.Target);
                    break;
                case NpcQuestObjective.Collect:
                    Assert.IsTrue(d.IsRecordsTarget || items.Contains(d.Target), d.Id + " 수집 " + d.Target);
                    break;
                case NpcQuestObjective.Craft:
                    Assert.IsTrue(items.Contains(d.Target), d.Id + " 제작 " + d.Target);
                    break;
                case NpcQuestObjective.Visit:
                    Assert.Contains(d.Target, visits, d.Id);
                    break;
                case NpcQuestObjective.Talk:
                    Assert.Contains(d.Target, npcs, d.Id);
                    break;
            }
        }
    }

    [Test] public void 전용_드롭_4종은_시트와_드롭표에_있다()
    {
        string[] drops = { "StarBarkFragment", "EntHeartwood", "SwampStarShard", "StolenForgeCrate" };
        var items = new HashSet<string>();
        foreach (ItemRow r in ItemTable.Load().Rows)
        {
            items.Add(r.Name);
        }
        foreach (string item in drops)
        {
            Assert.IsTrue(items.Contains(item), item);
        }
        var table = ExtraDrops.Parse(UnityEngine.Resources.Load<UnityEngine.TextAsset>("MonsterExtraDrops").text);
        int questOnly = 0;
        foreach (var kv in table)
        {
            foreach (ExtraDrops.Entry e in kv.Value)
            {
                if (e.QuestOnly)
                {
                    questOnly++;
                    Assert.Contains(e.Item, drops);
                }
            }
        }
        Assert.AreEqual(4, questOnly);
    }

    [Test] public void 선행_미완료는_Locked_레벨_부족은_LevelLow()
    {
        Assert.AreEqual(NpcQuestStatus.Available, NpcQuestLog.StatusOf("Q101"));
        Assert.AreEqual(NpcQuestStatus.Locked, NpcQuestLog.StatusOf("Q102"));
        NpcQuestLog.Accept("Q101");
        NpcQuestLog.Complete("Q101");
        Assert.AreEqual(NpcQuestStatus.Available, NpcQuestLog.StatusOf("Q102"));
        Assert.AreEqual(NpcQuestStatus.Locked, NpcQuestLog.StatusOf("Q103")); // 선행 Q102
        NpcQuestLog.Accept("Q102");
        for (int i = 0; i < 8; i++)
        {
            NpcQuestLog.OnKill("FlowerMole_BD");
        }
        NpcQuestLog.Complete("Q102");
        Assert.AreEqual(NpcQuestStatus.LevelLow, NpcQuestLog.StatusOf("Q103")); // 최소 Lv2, 지금 Lv1
        Assert.IsFalse(NpcQuestLog.Accept("Q103"));
        _level = 2;
        Assert.AreEqual(NpcQuestStatus.Available, NpcQuestLog.StatusOf("Q103"));
    }

    [Test] public void 장로_대화_퀘스트는_수락_즉시_완료_가능_보상_지급()
    {
        Assert.IsTrue(NpcQuestLog.Accept("Q101"));
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("Q101"));
        Assert.IsTrue(NpcQuestLog.Complete("Q101"));
        Assert.AreEqual(1, _rewards.Count);
        Assert.AreEqual(700, _rewards[0].Exp);
        Assert.AreEqual(NpcQuestStatus.Done, NpcQuestLog.StatusOf("Q101"));
        Assert.IsFalse(NpcQuestLog.Complete("Q101"), "중복 보상 금지");
        Assert.IsFalse(NpcQuestLog.Accept("Q101"));
    }

    [Test] public void 처치_진행은_수락_후부터_다른_몹은_무시()
    {
        NpcQuestLog.Accept("Q101");
        NpcQuestLog.Complete("Q101");
        NpcQuestLog.OnKill("FlowerMole_BD"); // 수락 전 처치는 세지 않는다
        Assert.IsTrue(NpcQuestLog.Accept("Q102"));
        Assert.AreEqual(0, NpcQuestLog.Progress(NpcQuestCatalog.Find("Q102")));
        NpcQuestLog.OnKill("Rabbit_BD");
        NpcQuestLog.OnKill("FlowerMole_BD");
        Assert.AreEqual(1, NpcQuestLog.Progress(NpcQuestCatalog.Find("Q102")));
        Assert.IsFalse(NpcQuestLog.Complete("Q102"), "목표 전 보고 불가");
        for (int i = 0; i < 20; i++)
        {
            NpcQuestLog.OnKill("FlowerMole_BD");
        }
        Assert.AreEqual(8, NpcQuestLog.Progress(NpcQuestCatalog.Find("Q102")), "목표를 넘기지 않는다");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("Q102"));
    }

    [Test] public void 수집_진행은_인벤토리_개수_완료하면_재료_회수()
    {
        Pass("Q101");
        Pass("Q102");
        _level = 2;
        Assert.IsTrue(NpcQuestLog.Accept("Q103"));
        _items["GreenHurb"] = 2;
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("Q103"));
        _items["GreenHurb"] = 5;
        Assert.AreEqual(3, NpcQuestLog.Progress(NpcQuestCatalog.Find("Q103")), "목표까지만 센다");
        Assert.IsTrue(NpcQuestLog.Complete("Q103"));
        CollectionAssert.Contains(_removed, "GreenHurbx3");
    }

    [Test] public void 전용_드롭은_진행_중이고_부족할_때만_원한다()
    {
        Assert.IsFalse(NpcQuestLog.WantsDrop("StarBarkFragment"));
        Pass("Q101");
        Pass("Q102");
        _level = 3;
        Assert.IsTrue(NpcQuestLog.Accept("Q201"));
        Assert.IsTrue(NpcQuestLog.WantsDrop("StarBarkFragment"));
        _items["StarBarkFragment"] = 3;
        Assert.IsFalse(NpcQuestLog.WantsDrop("StarBarkFragment"), "이미 충분");
        _items["StarBarkFragment"] = 0;
        Assert.IsTrue(NpcQuestLog.WantsDrop("StarBarkFragment"));
        Assert.IsFalse(NpcQuestLog.WantsDrop("EntHeartwood"));
    }

    [Test] public void 발굴_기록_개수로_Q602_판정()
    {
        NpcQuestDef d = NpcQuestCatalog.Find("Q602");
        Assert.IsTrue(d.IsRecordsTarget);
        ForceDone("Q601");
        _level = 13;
        Assert.IsTrue(NpcQuestLog.Accept("Q602"));
        _records = 4;
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("Q602"));
        _records = 5;
        Assert.IsTrue(NpcQuestLog.Complete("Q602"));
        Assert.AreEqual(0, _removed.Count, "기록은 회수하지 않는다");
    }

    [Test] public void 방문_제작_대화_목표()
    {
        ForceDone("Q101"); ForceDone("Q102"); ForceDone("Q202");
        _level = 5;
        Assert.IsTrue(NpcQuestLog.Accept("Q204"));
        NpcQuestLog.OnVisit("deep_nest");
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("Q204"));
        NpcQuestLog.OnVisit("forest_night_post");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("Q204"));

        ForceDone("Q103");
        _level = 4;
        Assert.IsTrue(NpcQuestLog.Accept("Q203"));
        NpcQuestLog.OnCraft("Rabbit Meat");
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("Q203"));
        NpcQuestLog.OnCraft("Rabbit Meat Stew");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("Q203"));
    }

    [Test] public void NPC별_대표_상태는_완료가능_수락가능_레벨부족_순()
    {
        NpcQuestStatus s = NpcQuestLog.BestFor("elder", out NpcQuestDef q);
        Assert.AreEqual(NpcQuestStatus.Available, s);
        Assert.AreEqual("Q101", q.Id);
        NpcQuestLog.Accept("Q101");
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.BestFor("elder", out q));
        NpcQuestLog.Complete("Q101");
        // Q201(Lv3) 은 선행 Q101 완료 + 레벨 1 → 회색 !
        Assert.AreEqual(NpcQuestStatus.LevelLow, NpcQuestLog.BestFor("elder", out q));
        Assert.AreEqual("Q201", q.Id);
        Assert.AreEqual(NpcQuestStatus.Locked, NpcQuestLog.BestFor("eve", out q), "이브는 선행 때문에 아직 표시 없음");
    }

    [Test] public void 세이브_직렬화_왕복()
    {
        Pass("Q101");
        NpcQuestLog.Accept("Q102");
        NpcQuestLog.OnKill("FlowerMole_BD");
        NpcQuestLog.OnKill("FlowerMole_BD");
        string text = NpcQuestLog.Serialize();
        Assert.AreEqual("Q101:2:1;Q102:1:2", text);
        NpcQuestLog.ClearAll();
        Assert.AreEqual(NpcQuestStatus.Available, NpcQuestLog.StatusOf("Q101"));
        NpcQuestLog.Deserialize(text);
        Assert.IsTrue(NpcQuestLog.IsDone("Q101"));
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("Q102"));
        Assert.AreEqual(2, NpcQuestLog.Progress(NpcQuestCatalog.Find("Q102")));
    }

    [Test] public void 세이브_깨진_항목은_건너뛴다()
    {
        NpcQuestLog.Deserialize("Q101:2:1;ZZZ:1:1;Q102:9:0;Q102:x:1;::;Q103:1:-5");
        Assert.IsTrue(NpcQuestLog.IsDone("Q101"));
        Assert.AreEqual(NpcQuestStatus.Available, NpcQuestLog.StatusOf("Q102")); // 잘못된 항목은 무시, Q101 완료라 수락 가능
        Assert.AreEqual(NpcQuestStatus.Active, NpcQuestLog.StatusOf("Q103")); // 음수 진행은 0 으로
    }

    [Test]
    public void 보상_공간_부족이면_재료_보상_완료_저장_모두_그대로()
    {
        NpcQuestLog.Deserialize("Q103:1:0");
        _items["GreenHurb"] = 3;
        NpcQuestLog.RewardCapacity = d => false;
        int saves = 0;
        NpcQuestLog.SaveHook = () => saves++;
        Assert.IsFalse(NpcQuestLog.Complete("Q103"));
        Assert.AreEqual(NpcQuestStatus.Ready, NpcQuestLog.StatusOf("Q103"));
        Assert.AreEqual(3, _items["GreenHurb"]);
        Assert.AreEqual(0, _removed.Count);
        Assert.AreEqual(0, _rewards.Count);
        Assert.AreEqual(0, saves);
        NpcQuestLog.RewardCapacity = d => true;
        Assert.IsTrue(NpcQuestLog.Complete("Q103"));
        Assert.IsFalse(NpcQuestLog.Complete("Q103"));
        Assert.AreEqual(1, _rewards.Count);
    }

    [Test]
    public void 수집_재료_회수로_생길_공간도_보상_검사에_포함()
    {
        var inventory = new SlotContainer(RewardItems, 1, 1);
        inventory.PutAt(0, new ItemStack("GreenHurb", 3));
        Assert.IsTrue(NpcQuestRewards.CanFitReward(NpcQuestCatalog.Find("Q103"), inventory, RewardItems));
        Assert.AreEqual(3, inventory.Count("GreenHurb"), "사전 검사는 원본 불변");
    }

    [Test]
    public void 만원_인벤에서_키_아이템_보상은_거절()
    {
        var inventory = new SlotContainer(RewardItems, 1, 1);
        inventory.PutAt(0, new ItemStack("Wood", 1));
        Assert.IsFalse(NpcQuestRewards.CanFitReward(NpcQuestCatalog.Find("Q604"), inventory, RewardItems));
        Assert.AreEqual(1, inventory.Count("Wood"));
    }

    [Test]
    public void 보상_두개는_스택_한칸_여유만으로_보고할_수_없다()
    {
        var inventory = new SlotContainer(RewardItems, 1, 1);
        int max = RewardItems.Get("Enhance Stone").MaxStack;
        inventory.PutAt(0, new ItemStack("Enhance Stone", max - 1));
        Assert.IsFalse(NpcQuestRewards.CanFitReward(NpcQuestCatalog.Find("Q603"), inventory, RewardItems));
        Assert.AreEqual(max - 1, inventory.Count("Enhance Stone"));
    }

    [Test]
    public void 브로치_공명은_판매_불가_일반_퀘스트_재료는_판매_가능()
    {
        Assert.IsFalse(ItemTradeRules.CanSell("BroochResonance"));
        Assert.IsTrue(ItemTradeRules.CanSell("StarBarkFragment"));
        Assert.IsTrue(ItemTradeRules.CanSell("Wood"));
        Assert.IsFalse(ItemTradeRules.CanSell(null));
    }

    private void Pass(string id)
    {
        Assert.IsTrue(NpcQuestLog.Accept(id), "수락 " + id);
        NpcQuestDef d = NpcQuestCatalog.Find(id);
        if (d.Objective == NpcQuestObjective.Kill)
        {
            for (int i = 0; i < d.Count; i++)
            {
                NpcQuestLog.OnKill(d.Target);
            }
        }
        Assert.IsTrue(NpcQuestLog.Complete(id), "완료 " + id);
    }

    private static void ForceDone(string id)
    {
        NpcQuestLog.Deserialize(NpcQuestLog.Serialize() + (NpcQuestLog.Serialize().Length > 0 ? ";" : "") + id + ":2:1");
    }
}
