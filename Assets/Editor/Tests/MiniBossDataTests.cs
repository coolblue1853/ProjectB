using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>미니보스 M1~M5(Floor1Completion 4-4) 데이터 무결성: MiniBosses.csv, MonsterSheet, ItemSheet, MonsterExtraDrops 참조 일치</summary>
public class MiniBossDataTests
{
    private static string[] Lines(string resource)
    {
        TextAsset t = Resources.Load<TextAsset>(resource);
        Assert.IsNotNull(t, resource + ".csv 없음");
        return t.text.Replace("\r", "").Split('\n');
    }

    private static HashSet<string> ItemNames()
    {
        var names = new HashSet<string>();
        string[] lines = Lines("ItemSheet");
        for (int i = 2; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length > 1 && c[1].Length > 0)
            {
                names.Add(c[1]);
            }
        }
        return names;
    }

    private static List<MiniBossTable.Row> Rows()
    {
        return MiniBossTable.Parse(Resources.Load<TextAsset>("MiniBosses").text);
    }

    [Test]
    public void MiniBosses_FiveRows_AllReferencesExist()
    {
        List<MiniBossTable.Row> rows = Rows();
        Assert.AreEqual(5, rows.Count);
        HashSet<string> items = ItemNames();
        var seen = new HashSet<string>();
        foreach (MiniBossTable.Row r in rows)
        {
            Assert.IsTrue(seen.Add(r.Id), "중복 id " + r.Id);
            MonsterTable.Row mini = MonsterTable.Get(r.Id);
            MonsterTable.Row baseRow = MonsterTable.Get(r.BaseMonster);
            Assert.IsNotNull(mini, "MonsterSheet 행 없음: " + r.Id);
            Assert.IsNotNull(baseRow, "baseMonster MonsterSheet 행 없음: " + r.BaseMonster);
            Assert.IsFalse(mini.isBoss ?? false, r.Id + " 는 보스가 아니다(정예 표식 경로)");
            Assert.AreEqual(r.Id, mini.killKey, r.Id + " killKey 는 id 와 같아야 killElite 목표가 맞는다");
            Assert.IsTrue(mini.maxHP.Value >= baseRow.maxHP.Value * 4 && mini.maxHP.Value <= baseRow.maxHP.Value * 10, r.Id + " HP 는 일반 몹 x4~x10 (REWORK-15: 후반 일반 몹 HP 상향으로 x10 에서 완화)");
            Assert.AreEqual(baseRow.contactDamage.Value, mini.contactDamage.Value, r.Id + " 몸박 기준값(정예 x1.5 는 코드)");
            Assert.AreEqual(baseRow.exp.Value * 20, mini.exp.Value, r.Id + " EXP 는 일반 x20");
            Assert.IsTrue(r.RespawnSec >= 60f, r.Id + " 재리젠");
            Assert.IsNotEmpty(r.Guaranteed, r.Id + " 확정 드롭");
            foreach (MiniBossTable.Stack s in r.Guaranteed)
            {
                Assert.IsTrue(items.Contains(s.Item), r.Id + " 확정 드롭 아이템 없음: " + s.Item);
            }
        }
    }

    [Test]
    public void MiniBosses_ExtraDropItemsExist()
    {
        HashSet<string> items = ItemNames();
        foreach (MiniBossTable.Row r in Rows())
        {
            List<ExtraDrops.Entry> list = ExtraDrops.For(r.Id);
            Assert.IsNotNull(list, r.Id + " 확률 드롭 행 없음");
            foreach (ExtraDrops.Entry e in list)
            {
                Assert.IsTrue(items.Contains(e.Item), r.Id + " 추가 드롭 아이템 없음: " + e.Item);
                Assert.IsTrue(e.Probability > 0f && e.Probability <= 0.5f, r.Id + " " + e.Item);
            }
        }
    }

    [Test]
    public void ZoneSealItems_ExistAndAreNotSellable()
    {
        HashSet<string> items = ItemNames();
        foreach (string seal in new[] { "ZoneSeal_Swamp", "ZoneSeal_Fortress", "ZoneSeal_Tower" })
        {
            Assert.IsTrue(items.Contains(seal), seal);
            Assert.IsFalse(ItemTradeRules.CanSell(seal), seal + " 는 판매 불가");
        }
    }

    [Test]
    public void BuildLoot_AppliesRetryMultiplierWithMinimumOne()
    {
        var row = new MiniBossTable.Row();
        row.Guaranteed.Add(new MiniBossTable.Stack { Item = "Wolf Fang", Count = 6 });
        row.Guaranteed.Add(new MiniBossTable.Stack { Item = "Enhance Stone", Count = 1 });
        Assert.AreEqual(7, MiniBossTable.BuildLoot(row, 1f).Count);
        List<string> half = MiniBossTable.BuildLoot(row, 0.5f);
        Assert.AreEqual(4, half.Count); // 3 + 1
        Assert.AreEqual(1, half.FindAll(x => x == "Enhance Stone").Count);
        Assert.AreEqual(2, MiniBossTable.BuildLoot(row, 0.25f).FindAll(x => x == "Wolf Fang").Count); // ceil(1.5)
    }

    [Test]
    public void Parse_SkipsBadRows()
    {
        List<MiniBossTable.Row> rows = MiniBossTable.Parse("id,map,x,y,baseMonster,respawnSec,guaranteed\nA,M,1,2,B,300,X:2|Y:3\nbad,row\nC,M,x,2,B,300,X:1\n");
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual(2, rows[0].Guaranteed.Count);
        Assert.AreEqual(3, rows[0].Guaranteed[1].Count);
    }
}
