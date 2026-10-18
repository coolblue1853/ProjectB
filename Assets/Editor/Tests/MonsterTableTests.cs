using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>MonsterSheet.csv 무결성 (C5): 도감 몬스터마다 행이 있고, Enemy_BD 프리팹마다 행이 있으며, 값이 파싱된다</summary>
public class MonsterTableTests
{
    static HashSet<string> Ids()
    {
        var text = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/MonsterSheet.csv");
        Assert.IsNotNull(text, "MonsterSheet.csv 없음");
        var ids = new HashSet<string>();
        string[] lines = text.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] c = lines[i].TrimEnd('\r').Split(',');
            if (c.Length >= 9 && !string.IsNullOrEmpty(c[0]))
            {
                Assert.IsTrue(int.TryParse(c[4], out int hp) && hp > 0, c[0] + " maxHP 비정상: " + c[4]);
                Assert.IsTrue(int.TryParse(c[5], out _), c[0] + " enemyDef 비정상: " + c[5]);
                ids.Add(c[0]);
            }
        }
        return ids;
    }

    [Test]
    public void EveryEnemyPrefab_HasRow()
    {
        HashSet<string> ids = Ids();
        foreach (string g in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab/Enemy_BD" }))
        {
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
            if (pf.GetComponent<EnemyHealth>() != null)
            {
                Assert.IsTrue(ids.Contains(pf.name), "MonsterSheet 행 없음: " + pf.name);
            }
        }
    }

    [Test]
    public void EveryBestiaryEntry_HasRow()
    {
        HashSet<string> ids = Ids();
        var data = AssetDatabase.LoadAssetAtPath<BestiaryData>("Assets/Resources/UI/BestiaryData.asset");
        Assert.IsNotNull(data);
        foreach (BestiaryData.Entry e in data.entries)
        {
            Assert.IsTrue(ids.Contains(e.id), "도감 몬스터의 MonsterSheet 행 없음: " + e.id);
        }
    }
}
