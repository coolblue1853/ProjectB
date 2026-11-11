using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>AUTO-35 (LA-2): 세트 개수 집계 키 = ItemDef.SetKey(=ItemSheet tfName). 비계보 무기 5종의 현재 세트 소속을 고정한다(의도는 바꾸지 않음).</summary>
public class SetCountKeyTests
{
    private static readonly string[] NonLineageWeapons = { "TwinFang", "GnarledClub", "VineLash", "HuntersLongbow", "WoodcuttersAxe" };

    private static List<string[]> SheetRows(out string[] header)
    {
        var lines = Resources.Load<TextAsset>("ItemSheet").text.Replace("\r", "").Split('\n').Where(l => l.Length > 0).ToList();
        string[] h = lines[1].Split(',');
        header = h;
        return lines.Skip(2).Select(l => l.Split(',')).Where(c => c.Length == h.Length).ToList();
    }

    private static ItemDef DefOf(string[] c, string[] h)
    {
        return new ItemDef { Id = c[1], SetKey = c[System.Array.IndexOf(h, "tfName")] };
    }

    [Test]
    public void NonLineageWeapons_BelongToExistingSetByTfName()
    {
        var rows = SheetRows(out string[] h);
        int nameCol = System.Array.IndexOf(h, "name");
        int tf = System.Array.IndexOf(h, "tfName");
        var setKeys = Resources.Load<TextAsset>("SetItem").text.Replace("\r", "").Split('\n').Skip(2)
            .Where(l => l.Length > 0).Select(l => l.Split(',')[0]).ToList();
        foreach (string w in NonLineageWeapons)
        {
            string[] c = rows.FirstOrDefault(r => r[nameCol] == w);
            Assert.IsNotNull(c, w + " 행 없음");
            Assert.IsNotEmpty(c[tf], w + " tfName 비어 있음");
            foreach (string t in new[] { "_2", "_4", "_5" })
            {
                Assert.Contains(c[tf] + t, setKeys, w + " 세트 키 없음");
            }
        }
    }

    [Test]
    public void CountPieces_KeysByTfName_NotBySetNameColumn()
    {
        var rows = SheetRows(out string[] h);
        int nameCol = System.Array.IndexOf(h, "name");
        var defs = NonLineageWeapons.Select(w => DefOf(rows.First(r => r[nameCol] == w), h)).ToList();
        Dictionary<string, int> counts = SetBonusEvaluator.CountPieces(defs);
        Assert.AreEqual(NonLineageWeapons.Length, counts.Values.Sum(), "무기 1개당 세트 1점");
        foreach (ItemDef d in defs)
        {
            Assert.IsTrue(counts.ContainsKey(d.SetKey), d.Id);
        }
        // 같은 세트 방어구 1점과 합산: 키가 tfName 이므로 무기도 방어구와 같은 카운트를 쓴다
        var armor = new ItemDef { Id = "WolfArmor", SetKey = "WolfArmor" };
        var twin = defs.First(d => d.Id == "TwinFang");
        Assert.AreEqual(2, SetBonusEvaluator.CountPieces(new[] { armor, twin })["WolfArmor"]);
    }
}
