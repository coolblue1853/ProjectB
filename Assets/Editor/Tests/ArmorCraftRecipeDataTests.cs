using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>ArmorCraftRecipes.csv 데이터 무결성: 결과·재료 아이템 실존, 중복 결과 없음, 계보 6세트 5부위, 신규 무기 세트 편입</summary>
public class ArmorCraftRecipeDataTests
{
    private static readonly string[] NewSets = { "Wisp", "Fae", "Gloom", "Outlaw", "Raider", "Stone" };
    private static readonly string[] Parts = { "Cap", "HandArmor", "LegArmor", "Shoes", "Armor" };

    private static Dictionary<string, string[]> LoadItems()
    {
        var dict = new Dictionary<string, string[]>();
        string[] lines = Resources.Load<TextAsset>("ItemSheet").text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length > 11) { dict[c[1].Trim()] = c; }
        }
        return dict;
    }

    private static List<string[]> LoadRecipes()
    {
        var list = new List<string[]>();
        string[] lines = Resources.Load<TextAsset>("ArmorCraftRecipes").text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length == 0) { continue; }
            list.Add(lines[i].Split(','));
        }
        return list;
    }

    [Test]
    public void Recipes_ResultAndMaterialsExist_NoDuplicateResult()
    {
        var items = LoadItems();
        var seen = new HashSet<string>();
        foreach (string[] r in LoadRecipes())
        {
            Assert.IsTrue(r.Length == 2 || r.Length == 3, "열 수(gold 열 선택): " + r[0]);
            Assert.IsTrue(items.ContainsKey(r[0]), "결과 아이템 없음: " + r[0]);
            Assert.IsTrue(seen.Add(r[0]), "결과 중복: " + r[0]);
            foreach (string m in r[1].Split(';'))
            {
                string[] p = m.Split('*');
                Assert.AreEqual(2, p.Length, "재료 형식: " + r[0] + " " + m);
                Assert.IsTrue(items.ContainsKey(p[0]), "재료 아이템 없음: " + p[0]);
                Assert.IsTrue(int.TryParse(p[1], out int n) && n > 0, "재료 수량: " + r[0] + " " + m);
            }
        }
    }

    [Test]
    public void Recipes_SixLineageSetsHaveFiveParts()
    {
        var have = new HashSet<string>();
        foreach (string[] r in LoadRecipes()) { have.Add(r[0]); }
        foreach (string s in NewSets)
        {
            foreach (string p in Parts) { Assert.IsTrue(have.Contains(s + p), "레시피 없음: " + s + p); }
        }
    }

    [Test]
    public void NewWeapons_AssignedToSetKey()
    {
        var items = LoadItems();
        foreach (string w in new[] { "TwinFang", "HuntersLongbow", "GnarledClub", "WoodcuttersAxe", "VineLash" })
        {
            Assert.IsFalse(string.IsNullOrEmpty(items[w][11]), "setName 비어 있음: " + w);
        }
    }
}
