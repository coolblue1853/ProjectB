using NUnit.Framework;

/// <summary>SLOT-R4: 신규 비외형 슬롯 보조 스탯 파싱·합산·해제, 세트 판정 무영향</summary>
public class ExtraEquipStatsTests
{
    [Test]
    public void Parse_SkipsZerosAndHeader()
    {
        var t = ExtraEquipStats.Parse("name,critical,criticalDmg,attSpeed,moveSpeed,dropRate,def,incDmg\r\nA,2,0,0,0,0,0,0\r\nB,3,5,0,0,0,0,0\r\n");
        Assert.AreEqual(1, t["A"].Length);
        Assert.AreEqual(StatId.CritRate, t["A"][0].stat);
        Assert.AreEqual(2f, t["A"][0].value);
        Assert.AreEqual(2, t["B"].Length);
        Assert.AreEqual(StatId.CritDmg, t["B"][1].stat);
    }

    [Test]
    public void ShippedCsv_AllExtraItemsHaveStats()
    {
        foreach (string n in new[] { "EarringPlain", "EarringHunter", "BraceletPlain", "BraceletSwift", "CharmPlain", "StonePlain" })
        {
            Assert.Greater(ExtraEquipStats.For(n).Length, 0, n);
        }
        Assert.AreEqual(0, ExtraEquipStats.For("RustedSword").Length);
    }

    [Test]
    public void Apply_AddsThenRemoves_BySource()
    {
        var src = new object();
        float before = PlayerStats.Get(StatId.CritRate);
        ExtraEquipStats.Apply(src, "EarringPlain");
        Assert.AreEqual(before + 2f, PlayerStats.Get(StatId.CritRate), 0.001f);
        ExtraEquipStats.Apply(src, "EarringPlain"); // 중복 적용 방지
        Assert.AreEqual(before + 2f, PlayerStats.Get(StatId.CritRate), 0.001f);
        ExtraEquipStats.Apply(src, null);
        Assert.AreEqual(before, PlayerStats.Get(StatId.CritRate), 0.001f);
    }

    [Test]
    public void ExtraSlotItems_NotInBaseSetSlots()
    {
        // 세트 판정은 기본 8칸만: 신규 슬롯은 SlotRules.Extras 에만 있다
        foreach (EquipSlot s in new[] { EquipSlot.Ring, EquipSlot.Head, EquipSlot.Necklace, EquipSlot.Chest, EquipSlot.Hand, EquipSlot.Weapon, EquipSlot.Leg, EquipSlot.Shoes })
        {
            Assert.IsFalse(SlotRules.IsExtra(s), s.ToString());
        }
    }
}
