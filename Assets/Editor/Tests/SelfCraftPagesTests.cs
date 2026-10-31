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
        Assert.AreEqual(16, list.Count); // 인장 반지 1 + 첫 1시간 계보 방어구 15(Wolf·Grove·Bog 각 5)
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
}
