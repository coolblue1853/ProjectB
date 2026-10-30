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
}
