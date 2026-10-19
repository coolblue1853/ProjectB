using NUnit.Framework;

/// <summary>PlayerStateDesign Phase 9 — PlayerStats (Modifier 스탯 코어 + 레거시 facade)</summary>
public class PlayerStatsTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerStats.ResetAll();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerStats.ResetAll();
    }

    static StatModifier Mod(StatId id, ModType t, float v) => new StatModifier(id, t, v);

    [Test]
    public void Order_AddThenPercentThenMultiply_RegardlessOfInsertOrder()
    {
        PlayerStats.Add(Mod(StatId.MaxHp, ModType.Multiply, 50));  // ×1.5
        PlayerStats.Add(Mod(StatId.MaxHp, ModType.Percent, 10));   // ×1.1
        PlayerStats.Add(Mod(StatId.MaxHp, ModType.Add, 100));
        PlayerStats.SetBase(StatId.MaxHp, 100);
        Assert.AreEqual(330f, PlayerStats.Get(StatId.MaxHp), 0.001f); // (100+100)×1.1×1.5
    }

    [Test]
    public void RemoveBySourceGroupId()
    {
        object equip = new object();
        PlayerStats.Add(new StatModifier(StatId.Def, ModType.Add, 5) { Source = equip });
        PlayerStats.Add(new StatModifier(StatId.Def, ModType.Add, 10) { Group = "Set:Rabbit_2" });
        PlayerStats.Add(new StatModifier(StatId.Def, ModType.Add, 3) { Id = "Buff:Def:1" });
        Assert.AreEqual(18, PlayerStats.GetInt(StatId.Def));
        Assert.AreEqual(1, PlayerStats.RemoveBySource(equip));
        Assert.AreEqual(1, PlayerStats.RemoveByGroup("Set:Rabbit_2"));
        Assert.AreEqual(1, PlayerStats.RemoveById("Buff:Def:1"));
        Assert.AreEqual(0, PlayerStats.GetInt(StatId.Def));
    }

    [Test]
    public void Legacy_PlusMinusRoundTripsToZero()
    {
        DatabaseManager.playerDef += 7;
        DatabaseManager.playerDef += 3;
        Assert.AreEqual(10, DatabaseManager.playerDef);
        DatabaseManager.playerDef -= 7;
        DatabaseManager.playerDef -= 3;
        Assert.AreEqual(0, DatabaseManager.playerDef);
    }

    [Test]
    public void Legacy_CoexistsWithModifiers()
    {
        object equip = new object();
        PlayerStats.Add(new StatModifier(StatId.CritRate, ModType.Add, 5) { Source = equip });
        DatabaseManager.playerCritRate += 4; // 레거시 경로(세트 효과 등)
        Assert.AreEqual(9, DatabaseManager.playerCritRate);
        PlayerStats.RemoveBySource(equip);
        Assert.AreEqual(4, DatabaseManager.playerCritRate); // 레거시 값은 남는다
        DatabaseManager.playerCritRate -= 4;
        Assert.AreEqual(0, DatabaseManager.playerCritRate);
    }

    [Test]
    public void GetInt_RoundsInsteadOfTruncating()
    {
        PlayerStats.Add(Mod(StatId.Def, ModType.Add, 0.9999f));
        Assert.AreEqual(1, PlayerStats.GetInt(StatId.Def));
    }

    [Test]
    public void Changed_FiresOnlyWhenValueChanges()
    {
        int fired = 0;
        PlayerStats.Changed += id => { if (id == StatId.DropRate) fired++; };
        DatabaseManager.playerDropRate = 5;
        DatabaseManager.playerDropRate = 5;
        Assert.AreEqual(1, fired);
    }
}
