using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>AUTO-23: 2층계 ON 상태 가정 — 몹→드롭→세트 키 연결, 계보 무기 연결, T8~T10 스탯 범위(Floor2Plan §3.2).</summary>
public class Floor2LinkTests
{
    private static string[] Rows(string baseName, string preview, int skip)
    {
        Floor2Data.Enabled = true;
        try
        {
            string text = Floor2Data.Merge(Resources.Load<TextAsset>(baseName).text, preview);
            return text.Replace("\r", "").Split('\n').Skip(skip).Where(l => l.Length > 0).ToArray();
        }
        finally
        {
            Floor2Data.Enabled = false;
        }
    }

    private static string[] Mobs() { return Rows("MonsterSheet", "Floor2Monsters", 1); }
    private static string[] Items() { return Rows("ItemSheet", "Floor2ItemSheet", 2); }
    private static string[] Sets() { return Rows("SetItem", "Floor2SetItem", 2); }
    private static string[] Drops() { return Rows("MonsterExtraDrops", "Floor2ExtraDrops", 1); }

    [Test]
    public void Drops_MobAndItemExist()
    {
        var mobs = new HashSet<string>(Mobs().Select(l => l.Split(',')[0]));
        var items = new HashSet<string>(Items().Select(l => l.Split(',')[1]));
        foreach (string d in Drops())
        {
            string[] c = d.Split(',');
            Assert.IsTrue(mobs.Contains(c[0]), "몹 없음: " + c[0]);
            Assert.IsTrue(items.Contains(c[1]), "아이템 없음: " + c[1]);
        }
    }

    [Test]
    public void SetKeys_AllTiersExist_ForEveryTfName()
    {
        var keys = new HashSet<string>(Sets().Select(l => l.Split(',')[0]));
        var tf = Items().Select(l => l.Split(',')).Where(c => int.Parse(c[0]) >= 268).Select(c => c[11]).Where(s => s.Length > 0).Distinct().ToList();
        Assert.IsNotEmpty(tf);
        foreach (string t in tf)
        {
            foreach (int n in new[] { 2, 4, 5 })
            {
                Assert.IsTrue(keys.Contains(t + "_" + n), "세트 키 없음: " + t + "_" + n);
            }
        }
    }

    [Test]
    public void LineageWeapons_DropFromMob_AndSetFourHasCooldownSkill()
    {
        var dropped = new HashSet<string>(Drops().Select(l => l.Split(',')[1]));
        var four = Sets().Select(l => l.Split(',')).Where(c => c[0].EndsWith("_4")).ToDictionary(c => c[0], c => c[1]);
        foreach (string[] c in Items().Select(l => l.Split(',')).Where(c => c[10] == "Weapon" && int.Parse(c[0]) >= 268))
        {
            Assert.IsTrue(dropped.Contains(c[1]), "드롭 몹 없는 무기: " + c[1]);
            if (c[11].Length > 0)
            {
                Assert.IsTrue(four[c[11] + "_4"].Contains("coolDown "), "4세트에 쿨감 스킬 키 없음: " + c[11]);
            }
        }
    }

    [Test]
    public void Floor2Mobs_StatsWithinPlanRange()
    {
        var tier = new Dictionary<string, int> { { "Statue Garden", 8 }, { "Knight Citadel", 9 }, { "Dragon Nest", 10 } };
        foreach (string l in Mobs().Where(x => tier.Keys.Any(k => x.Split(',')[3].StartsWith(k))))
        {
            string[] c = l.Split(',');
            string zone = tier.Keys.First(k => c[3].StartsWith(k));
            int t = tier[zone];
            float avgHp = 160f * Mathf.Pow(1.25f, t - 1);
            float avgDef = 3 * t + 2;
            float hp = float.Parse(c[4]);
            float def = float.Parse(c[5]);
            if (c[6] == "1")
            {
                Assert.GreaterOrEqual(hp, avgHp * 11.8f, l);
                Assert.LessOrEqual(hp, avgHp * 40.5f, l);
                Assert.AreEqual(avgDef * 1.5f, def, 1.5f, l);
            }
            else
            {
                Assert.GreaterOrEqual(hp, avgHp * 0.45f, l);
                Assert.LessOrEqual(hp, avgHp * 1.5f, l);
                Assert.GreaterOrEqual(def, avgDef * 0.35f, l);
                Assert.LessOrEqual(def, avgDef * 1.5f, l);
            }
        }
    }
}
