using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>ItemSheet category 열: 잡템 분류가 레시피·퀘스트·보상·현상금·보물·도감 보상 참조 아이템과 겹치지 않는다</summary>
public class JunkCategoryDataTests
{
    private static readonly string[] Candidates =
    {
        "Small Bone", "Round Bone", "Rabbit Hide", "Fine Rabbit Hide", "Disgusting Rat Meat", "Rat Thief's Stash",
        "Filthy Rat Hide", "Rat Cheif 's Loot", "Ominous Black Liquid", "Blackened Branch", "Malice-Infused Branch",
        "Eerie Cloth Scrap", "Gloomy Cloth Scrap", "Tough Rat Hide",
    };

    private static string[] Lines(string name)
    {
        string text = Resources.Load<TextAsset>(name).text;
        text = text.Replace(((char)13).ToString(), "").Replace(((char)0xFEFF).ToString(), "");
        return text.Split((char)10);
    }

    private static HashSet<string> Cells(string name, params string[] skipColumns)
    {
        var set = new HashSet<string>();
        string[] lines = Lines(name);
        string[] head = lines[0].Split(',');
        var skip = new HashSet<int>();
        for (int i = 0; i < head.Length; i++)
        {
            if (System.Array.IndexOf(skipColumns, head[i].Trim()) >= 0) { skip.Add(i); }
        }
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            for (int j = 0; j < c.Length; j++)
            {
                if (skip.Contains(j)) { continue; }
                foreach (string part in c[j].Split(';'))
                {
                    set.Add(part.Split('*')[0].Trim());
                }
            }
        }
        return set;
    }

    [Test]
    public void JunkItems_AreNotReferencedByRecipesQuestsOrRewards()
    {
        var junk = new Dictionary<string, string>();
        string[] items = Lines("ItemSheet");
        int col = System.Array.IndexOf(items[1].Split(','), "category");
        Assert.GreaterOrEqual(col, 0, "category 열 없음");
        for (int i = 2; i < items.Length; i++)
        {
            string[] c = items[i].Split(',');
            if (c.Length > col && c[col].Trim() == "Junk") { junk[c[1].Trim()] = c[2].Trim(); }
        }
        Assert.AreEqual(Candidates.Length, junk.Count, "1층계 잡템 후보 수");
        var refs = new HashSet<string>();
        refs.UnionWith(Cells("SelfCraftRecipes"));
        refs.UnionWith(Cells("ArmorCraftRecipes"));
        refs.UnionWith(Cells("QuestsFloor1", "description", "title"));
        refs.UnionWith(Cells("Floor1Bounties", "title"));
        refs.UnionWith(Cells("Floor1Treasures", "hint"));
        refs.UnionWith(Cells("Floor1BestiaryRewards", "title"));
        refs.UnionWith(Cells("ShopStock"));
        foreach (KeyValuePair<string, string> kv in junk)
        {
            Assert.AreEqual("Misc", kv.Value, "잡템은 Misc 만: " + kv.Key);
            Assert.IsFalse(refs.Contains(kv.Key), "참조되는 아이템은 Junk 금지: " + kv.Key);
        }
    }

    // 11-11 정정: 이전 핀은 "퀘스트 보상이라 비분류"였으나 CSV·씬 어디에도 레시피·퀘스트·보상 참조가 없고 쥐 몹 드롭(씬 DropManager)뿐이다.
    // JunkItems_AreNotReferencedByRecipesQuestsOrRewards 가 참조 금지를 계속 지키므로 Junk 로 분류한다(B5).
    [Test]
    public void ToughRatHide_IsJunk_NotReferencedByDataTables()
    {
        foreach (string line in Lines("ItemSheet"))
        {
            string[] c = line.Split(',');
            if (c.Length > 18 && c[1] == "Tough Rat Hide") { Assert.AreEqual("Junk", c[18].Trim()); }
        }
    }
}
