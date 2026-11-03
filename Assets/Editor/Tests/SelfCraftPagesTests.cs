using NUnit.Framework;

public class SelfCraftPagesTests
{
    [Test]
    public void Parse_ReadsRecipesAndSkipsBroken()
    {
        string csv = "result,materials\nGrilled Forest Carp,Forest Carp*2;Wood*1\n# 주석\nBroken\nNoMats,Wood\nAmber Trout Soup,Amber Trout*1; Moss Loach*1;GreenHurb*2\n";
        var list = SelfCraftPages.Parse(csv);
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual("Grilled Forest Carp", list[0].Result);
        CollectionAssert.AreEqual(new[] { "Forest Carp", "Wood" }, list[0].Ids);
        CollectionAssert.AreEqual(new[] { 2, 1 }, list[0].Counts);
        Assert.AreEqual(3, list[1].Ids.Count);
        Assert.AreEqual("Moss Loach", list[1].Ids[1]);
    }

    [Test]
    public void ShippedCsv_AllNamesExistInItemSheet()
    {
        var csv = UnityEngine.Resources.Load<UnityEngine.TextAsset>(SelfCraftPages.ResourcePath);
        Assert.IsNotNull(csv);
        string sheet = System.IO.File.ReadAllText("Assets/Resources/ItemSheet.csv");
        foreach (var r in SelfCraftPages.Parse(csv.text))
        {
            StringAssert.Contains("," + r.Result + ",", sheet);
            foreach (string id in r.Ids)
            {
                StringAssert.Contains("," + id + ",", sheet);
            }
        }
    }

    [Test]
    public void ArmorCsv_ParsesSigilRingAndNamesExist()
    {
        var csv = UnityEngine.Resources.Load<UnityEngine.TextAsset>(SelfCraftPages.ArmorResourcePath);
        Assert.IsNotNull(csv);
        var list = SelfCraftPages.Parse(csv.text);
        Assert.AreEqual(46, list.Count); // 인장 반지 1 + 첫 1시간 계보 방어구 15(Wolf·Grove·Bog 각 5) + L5 신규 6세트 30
        Assert.AreEqual("Bringer's Sigil Ring", list[0].Result);
        CollectionAssert.AreEqual(new[] { "Death Sigil", "T1_Ore" }, list[0].Ids);
        CollectionAssert.AreEqual(new[] { 8, 10 }, list[0].Counts);
        string sheet = System.IO.File.ReadAllText("Assets/Resources/ItemSheet.csv");
        foreach (var recipe in list)
        {
            StringAssert.Contains("," + recipe.Result + ",", sheet);
            foreach (string id in recipe.Ids)
            {
                StringAssert.Contains("," + id + ",", sheet);
            }
        }
    }

    [Test]
    public void Parse_GoldColumn_MissingOrBlankIsZero()
    {
        var list = SelfCraftPages.Parse("result,materials,gold\nA,Wood*1\nB,Wood*1,\nC,Wood*1,abc\nD,Wood*2,390\n");
        Assert.AreEqual(4, list.Count);
        Assert.AreEqual(0, list[0].Gold);
        Assert.AreEqual(0, list[1].Gold);
        Assert.AreEqual(0, list[2].Gold);
        Assert.AreEqual(390, list[3].Gold);
        Assert.AreEqual(2, list[3].Counts[0]);
    }

    [Test]
    public void GoldRules_ShortBlocksAndSpendDeducts()
    {
        Assert.IsTrue(CraftGoldRules.Short(390, 389));
        Assert.IsFalse(CraftGoldRules.Short(390, 390));
        Assert.IsFalse(CraftGoldRules.Short(0, 0));
        Assert.AreEqual(389, CraftGoldRules.Spend(390, 389)); // 부족: 차감 없음
        Assert.AreEqual(10, CraftGoldRules.Spend(390, 400));
        Assert.AreEqual(400, CraftGoldRules.Spend(0, 400));
        Assert.AreEqual("", CraftGoldRules.CostLabel(0));
        Assert.AreEqual("390 G", CraftGoldRules.CostLabel(390));
    }

    [Test]
    public void ArmorCsv_GoldMatchesSetTable()
    {
        Assert.AreEqual(390, CraftGoldRules.GoldFor("WolfArmor"));
        Assert.AreEqual(290, CraftGoldRules.GoldFor("WispLegArmor"));
        Assert.AreEqual(320, CraftGoldRules.GoldFor("FaeShoes"));
        Assert.AreEqual(0, CraftGoldRules.GoldFor("Bringer's Sigil Ring"));
        int[] sums = { 1300, 2000, 1700, 2500, 2100 };
        string[][] sets = { new[] { "Wolf", "Wisp" }, new[] { "Grove", "Fae" }, new[] { "Bog", "Gloom" }, new[] { "Outlaw", "Raider" }, new[] { "Stone" } };
        string[] parts = { "Cap", "HandArmor", "LegArmor", "Shoes", "Armor" };
        for (int i = 0; i < sets.Length; i++)
        {
            foreach (string set in sets[i])
            {
                int sum = 0;
                foreach (string p in parts) { sum += CraftGoldRules.GoldFor(set + p); }
                Assert.AreEqual(sums[i], sum, 15, set); // 문서 합계는 반올림(부위 표가 기준)
            }
        }
    }
}
