using NUnit.Framework;

/// <summary>REWORK-11 후속: 요리 레시피 CSV 로더 — 기존 코드 값과 같고, 비면 폴백</summary>
public class CookingRecipeCsvTests
{
    [TearDown]
    public void TearDown()
    {
        CookingRules.SetRecipesForTests(null);
    }

    [Test]
    public void CSV_파일은_코드_기본값과_같은_6종이다()
    {
        CookRecipe[] csv = CookingRecipeCsv.Load();
        Assert.IsNotNull(csv, "Resources/CookingRecipes.csv 로드");
        CookRecipe[] def = CookingRules.DefaultRecipes;
        Assert.AreEqual(def.Length, csv.Length);
        for (int i = 0; i < def.Length; i++)
        {
            Assert.AreEqual(def[i].Id, csv[i].Id);
            Assert.AreEqual(def[i].Method, csv[i].Method);
            Assert.AreEqual(def[i].Rarity, csv[i].Rarity);
            Assert.AreEqual(def[i].Ingredients.Length, csv[i].Ingredients.Length);
            for (int k = 0; k < def[i].Ingredients.Length; k++)
            {
                Assert.AreEqual(def[i].Ingredients[k].Id, csv[i].Ingredients[k].Id);
                Assert.AreEqual(def[i].Ingredients[k].Count, csv[i].Ingredients[k].Count);
            }
        }
    }

    [Test]
    public void Recipes_는_CSV_를_읽는다()
    {
        CookingRules.SetRecipesForTests(null);
        Assert.AreEqual(6, CookingRules.Recipes.Length);
        Assert.AreEqual("Herb Roasted Rabbit", CookingRules.Recipes[0].Id);
    }

    [Test]
    public void 빈_본문이나_유효_줄_0이면_null_이라_기본값_폴백()
    {
        Assert.IsNull(CookingRecipeCsv.Parse(""));
        Assert.IsNull(CookingRecipeCsv.Parse(null));
        Assert.IsNull(CookingRecipeCsv.Parse("id,method,rarity,ingredients\n"));
        Assert.IsNull(CookingRecipeCsv.Parse("id,method,rarity,ingredients\nX,Fry,C,A*1"));
    }

    [Test]
    public void 잘못된_줄은_건너뛰고_나머지를_읽는다_CRLF_허용()
    {
        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        string text = "id,method,rarity,ingredients\r\nA,Grill,C,Meat*2;Wood*1\r\nB,Boil,Z,Fish*1\r\nC,Boil,U,Fish*x\r\nD,boil,r,Fish*3\r\n";
        CookRecipe[] r = CookingRecipeCsv.Parse(text);
        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
        Assert.AreEqual(2, r.Length);
        Assert.AreEqual("A", r[0].Id);
        Assert.AreEqual(2, r[0].Ingredients.Length);
        Assert.AreEqual("D", r[1].Id);
        Assert.AreEqual(CookMethod.Boil, r[1].Method);
        Assert.AreEqual(ItemRarity.R, r[1].Rarity);
        Assert.AreEqual(3, r[1].Ingredients[0].Count);
    }
}
