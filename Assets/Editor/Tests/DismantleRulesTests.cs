using System.Collections.Generic;
using NUnit.Framework;

public class DismantleRulesTests
{
    private static int Count(List<KeyValuePair<string, int>> list, string id)
    {
        int n = 0;
        foreach (var kv in list)
        {
            if (kv.Key == id)
            {
                n += kv.Value;
            }
        }
        return n;
    }

    [Test]
    public void Common_GivesOreByTier()
    {
        var t0 = DismantleRules.Outputs("C", 0, 0);
        Assert.AreEqual(1, Count(t0, DismantleRules.Ore0));
        Assert.AreEqual(0, Count(t0, EnhanceRules.StoneName));
        Assert.AreEqual(1, Count(DismantleRules.Outputs("C", 1, 0), DismantleRules.Ore1));
    }

    [Test]
    public void Rare_OrePlusStone_Unique_ThreeStones()
    {
        var r = DismantleRules.Outputs("R", 0, 0);
        Assert.AreEqual(1, Count(r, DismantleRules.Ore0));
        Assert.AreEqual(1, Count(r, EnhanceRules.StoneName));
        var u = DismantleRules.Outputs("U", 1, 0);
        Assert.AreEqual(3, Count(u, EnhanceRules.StoneName));
        Assert.AreEqual(0, Count(u, DismantleRules.Ore1));
    }

    [Test]
    public void Upgrade_RefundsStonePerTwoLevels()
    {
        Assert.AreEqual(2, Count(DismantleRules.Outputs("C", 0, 5), EnhanceRules.StoneName));
        Assert.AreEqual(1 + 5, Count(DismantleRules.Outputs("R", 0, 10), EnhanceRules.StoneName));
    }
}
