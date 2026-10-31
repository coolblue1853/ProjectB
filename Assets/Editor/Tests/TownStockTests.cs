using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class TownStockTests
{
    private static Dictionary<string, string[]> ReadItems()
    {
        var items = new Dictionary<string, string[]>();
        string[] lines = File.ReadAllLines("Assets/Resources/ItemSheet.csv");
        for (int i = 2; i < lines.Length; i++)
        {
            string[] cells = lines[i].Split(',');
            if (cells.Length >= 18)
            {
                items.Add(cells[1], cells);
            }
        }
        return items;
    }

    [TestCase("Small HP Potion", 25, 120)]
    [TestCase("Medium HP Potion", 45, 220)]
    [TestCase("Large HP Potion", 70, 320)]
    public void Potions_MatchFirstHourBudget(string id, int buy, int heal)
    {
        string[] item = ReadItems()[id];
        Assert.AreEqual(buy, (int)(int.Parse(item[4]) * 1.5f));
        Assert.AreEqual(heal.ToString(), item[9]);
        CollectionAssert.Contains(ShopStockExtender.ExtraItems, id);
    }

    [Test]
    public void Recall_RemainsAffordableWithoutSellingProgressionGear()
    {
        string[] item = ReadItems()["Recall Scroll"];
        Assert.AreEqual(120, (int)(int.Parse(item[4]) * 1.5f));
        CollectionAssert.Contains(ShopStockExtender.ExtraItems, "Recall Scroll");
    }

    [Test]
    public void Stock_HasUniqueValidIconsAndNoTradeProfit()
    {
        var items = ReadItems();
        var seen = new HashSet<string>();
        foreach (string[] row in new[] { ShopStockExtender.ExtraItems, ShopStockExtender.Supplies,
            ShopStockExtender.Weapons, ShopStockExtender.Armor })
        {
            foreach (string id in row)
            {
                Assert.IsTrue(seen.Add(id), "중복 진열: " + id);
                Assert.IsTrue(items.ContainsKey(id), "시트 누락: " + id);
                Assert.IsNotNull(Resources.Load<Sprite>("Item/" + id), "아이콘 누락: " + id);
                int sell = int.Parse(items[id][4]);
                Assert.Greater((int)(sell * 1.5f), sell, "매매 차익: " + id);
            }
        }
        Assert.AreEqual(14, seen.Count);
        Assert.IsFalse(seen.Contains("Potion"));
        Assert.IsFalse(seen.Contains("BroochResonance"));
    }

    [Test]
    public void ShopGear_IsCommonAndCostsAtMostOneEarlyHuntingBudget()
    {
        var items = ReadItems();
        foreach (string[] row in new[] { ShopStockExtender.Weapons, ShopStockExtender.Armor })
        {
            foreach (string id in row)
            {
                string[] item = items[id];
                Assert.AreEqual("Equip", item[2]);
                Assert.AreEqual("C", item[14]);
                Assert.IsNotNull(GearPrefabCache.Get(item[10], id), "장비 프리팹 누락: " + id);
                Assert.LessOrEqual((int)(int.Parse(item[4]) * 1.5f), 920);
            }
        }
        CollectionAssert.DoesNotContain(ShopStockExtender.Weapons, "FangDagger");
        CollectionAssert.DoesNotContain(ShopStockExtender.Armor, "WolfCap");
    }
    [Test]
    public void ServiceLabel_HidesWithItsTargetAndReturnsWhenEnabled()
    {
        var target = new GameObject("TownServiceTestTarget");
        var labelObject = new GameObject("TownServiceTestLabel");
        try
        {
            TownServiceLabel label = labelObject.AddComponent<TownServiceLabel>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(TownServiceLabel).GetField("_target", flags).SetValue(label, target.transform);
            typeof(TownServiceLabel).GetField("_text", flags).SetValue(label, labelObject.AddComponent<TMPro.TextMeshPro>());
            var update = typeof(TownServiceLabel).GetMethod("LateUpdate",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            TMPro.TextMeshPro text = labelObject.GetComponent<TMPro.TextMeshPro>();
            update.Invoke(label, null);
            Assert.IsTrue(text.enabled);
            target.SetActive(false);
            update.Invoke(label, null);
            Assert.IsFalse(text.enabled, "비활성 서비스의 이름표가 남으면 안 된다");
            target.SetActive(true);
            update.Invoke(label, null);
            Assert.IsTrue(text.enabled);
        }
        finally
        {
            Object.DestroyImmediate(labelObject);
            Object.DestroyImmediate(target);
        }
    }
}
