using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>AUTO-2: 퀘스트·드롭·상점·현상금 데이터 교차 참조 무결성(읽기만). 위반 목록은 Docs/Playtest/DataIntegrity.md 로.</summary>
public class DataIntegrityTests
{
    private static string[][] Rows(string file)
    {
        string text = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/" + file)).TrimStart((char)0xFEFF);
        return text.Replace("\r", "").Split('\n').Skip(1).Where(l => l.Length > 0).Select(l => l.Split(',')).ToArray();
    }

    private static HashSet<string> Col(string file, int col)
    {
        return new HashSet<string>(Rows(file).Where(r => r.Length > col).Select(r => r[col].Trim()));
    }

    [TestCase("Q604")]
    [TestCase("Q704")]
    public void GraduationGate_ReachableFromRoot(string id)
    {
        NpcQuestDef d = NpcQuestCatalog.Find(id);
        Assert.IsNotNull(d, id);
        var seen = new HashSet<string>();
        while (d != null)
        {
            Assert.IsTrue(seen.Add(d.Id), "순환: " + d.Id);
            if (string.IsNullOrEmpty(d.Requires))
            {
                Assert.AreEqual("Q101", d.Id, "루트가 Q101 이 아님");
                return;
            }
            d = NpcQuestCatalog.Find(d.Requires);
            Assert.IsNotNull(d, "선행 끊김");
        }
    }

    [Test]
    public void Q604_Target_IsGatekeeperInMonsterSheet()
    {
        NpcQuestDef d = NpcQuestCatalog.Find("Q604");
        Assert.IsTrue(Col("MonsterSheet.csv", 0).Contains(d.Target), d.Target);
    }

    [Test]
    public void MonsterExtraDrops_MonstersAndItemsExist()
    {
        var monsters = Col("MonsterSheet.csv", 0);
        var items = Col("ItemSheet.csv", 1);
        var bad = Rows("MonsterExtraDrops.csv").Where(r => !monsters.Contains(r[0].Trim()) || !items.Contains(r[1].Trim()))
            .Select(r => r[0] + "→" + r[1]).ToArray();
        Assert.IsEmpty(bad, string.Join(", ", bad));
    }

    [Test]
    public void ShopStock_ItemsExist()
    {
        var items = Col("ItemSheet.csv", 1);
        var bad = Rows("ShopStock.csv").Where(r => !items.Contains(r[2].Trim())).Select(r => r[2]).Distinct().ToArray();
        Assert.IsEmpty(bad, string.Join(", ", bad));
    }

    [Test]
    public void Bounties_TargetsAndItemsExist()
    {
        var monsters = Col("MonsterSheet.csv", 0);
        var items = Col("ItemSheet.csv", 1);
        var bad = new List<string>();
        foreach (string[] r in Rows("Floor1Bounties.csv"))
        {
            foreach (string t in r[3].Split('|'))
            {
                if (!monsters.Contains(t.Trim())) { bad.Add(r[0] + " 몹 " + t); }
            }
            if (r.Length > 8 && r[8].Trim().Length > 0 && !items.Contains(r[8].Trim())) { bad.Add(r[0] + " 아이템 " + r[8]); }
        }
        Assert.IsEmpty(bad, string.Join(", ", bad));
    }

    [Test]
    public void Treasures_ItemsExist()
    {
        var items = Col("ItemSheet.csv", 1);
        var bad = Rows("Floor1Treasures.csv").Where(r => r[6].Trim().Length > 0 && !items.Contains(r[6].Trim())).Select(r => r[0] + " " + r[6]).ToArray();
        Assert.IsEmpty(bad, string.Join(", ", bad));
    }

    [Test]
    public void EquippedSetQuests_TargetSetsHaveItems()
    {
        var tf = Col("ItemSheet.csv", 11);
        var bad = NpcQuestCatalog.All.Where(d => d.Objective == NpcQuestObjective.Equip && d.Target != "armor" && !d.Target.StartsWith("sets:") && !tf.Contains(d.Target))
            .Select(d => d.Id + " " + d.Target).ToArray();
        Assert.IsEmpty(bad, string.Join(", ", bad));
    }
}
