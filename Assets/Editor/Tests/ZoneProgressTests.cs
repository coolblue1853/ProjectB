using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class ZoneProgressTests
{
    [Test]
    public void Percent_ClampsAndHandlesZeroTotal()
    {
        Assert.AreEqual(0, ZoneProgress.Percent(3, 0));
        Assert.AreEqual(50, ZoneProgress.Percent(1, 2));
        Assert.AreEqual(100, ZoneProgress.Percent(9, 3));
    }

    [Test]
    public void CompletionPercent_CountsChestMiniAndSet()
    {
        var c = new ZoneProgress.Counts { ChestDone = 1, ChestTotal = 2, MiniDone = 1, MiniTotal = 1, SetParts = 2, Colonies = 3, Elites = 1 };
        Assert.AreEqual(50, ZoneProgress.CompletionPercent(c)); // 4 / 8
        Assert.AreEqual(0, ZoneProgress.CompletionPercent(new ZoneProgress.Counts { MiniTotal = 1 }));
    }

    [Test]
    public void GuardianLine_UsesNameAndGoalOnly()
    {
        ZoneProgress.Zone z = ZoneProgress.Find("Lv1_DeepForest");
        Assert.IsNotNull(z);
        Assert.AreEqual("깊은 숲의 수호자 — 늙은 곰: 깊은 곳에 있다", ZoneProgress.GuardianLine(z, false, true));
        StringAssert.EndsWith("처치 완료", ZoneProgress.GuardianLine(z, true, true));
        Assert.AreEqual("", ZoneProgress.GuardianLine(null, false, true));
        Assert.IsNull(ZoneProgress.Find("Lv1_NearForest_SideRoad"));
    }

    [Test]
    public void NextPartMaterials_SumsHaveCappedByNeed()
    {
        List<SelfCraftPages.Recipe> recipes = SelfCraftPages.Parse("result,materials\nWolfCap,Wolf Fang*8\nWolfHandArmor,Boar Tusk*8\n");
        bool ok = ZoneProgress.NextPartMaterials(recipes, "Wolf", 0, id => id == "Wolf Fang" ? 20 : 0, out int got, out int need);
        Assert.IsTrue(ok);
        Assert.AreEqual(8, got); // 필요량 위로는 세지 않는다
        Assert.AreEqual(8, need);
        Assert.IsTrue(ZoneProgress.NextPartMaterials(recipes, "Wolf", 1, id => 3, out got, out need));
        Assert.AreEqual(3, got);
        Assert.IsFalse(ZoneProgress.NextPartMaterials(recipes, "Wolf", 5, id => 1, out got, out need)); // 세트 완성
    }

    [Test]
    public void OwnedParts_ClampsToFive()
    {
        Assert.AreEqual(5, ZoneProgress.OwnedParts(4, 3));
        Assert.AreEqual(0, ZoneProgress.OwnedParts(0, 0));
    }

    [Test]
    public void Zones_MiniBossIdsExistInMiniBossCsv()
    {
        foreach (ZoneProgress.Zone z in ZoneProgress.Zones)
        {
            Assert.IsTrue(MiniBossTable.All().Any(r => r.Id == z.MiniId && r.Map == z.Map), z.Map);
        }
    }
}
