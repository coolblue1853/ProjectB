using System.Collections.Generic;
using NUnit.Framework;
using ProjectB.EditorTools;
using UnityEngine;

/// <summary>1층계 신규 맵 2장(Lv1_DeepForestCore·Lv1_MistSwamp, Floor1Completion 14-3) 데이터: 퀘스트 4건·주둔·수집물·지하 석실 앵커</summary>
public class Floor1NewMapsTests
{
    private static readonly string[] NewMaps = { "Lv1_DeepForestCore", "Lv1_MistSwamp" };

    [Test]
    public void NewQuests_ExpSumIs7200_AndRewardsTargetsExist()
    {
        var items = new HashSet<string>();
        foreach (ItemRow r in ItemTable.Load().Rows)
        {
            items.Add(r.Name);
        }
        int exp = 0;
        foreach (string id in new[] { "Q310", "Q311", "Q410", "Q411" })
        {
            NpcQuestDef d = NpcQuestCatalog.Find(id);
            Assert.IsNotNull(d, id);
            exp += d.Exp;
            if (id == "Q310")
            {
                // Q-A1: Q303 과 목표 중복을 피해 처치 퀘스트로 교체.
                Assert.AreEqual(NpcQuestObjective.Kill, d.Objective, id);
                Assert.AreEqual("DeepSpore_BD", d.Target, id);
            }
            else
            {
                Assert.AreEqual(NpcQuestObjective.Collect, d.Objective, id);
                Assert.IsTrue(items.Contains(d.Target), id + " 수집 " + d.Target);
            }
            Assert.IsTrue(items.Contains(d.RewardItem), id + " 보상 " + d.RewardItem);
            Assert.IsNotNull(NpcQuestCatalog.Find(d.Requires), id + " 선행 " + d.Requires);
        }
        Assert.AreEqual(7200, exp);
    }

    [Test]
    public void HuntingRows_ForNewMaps_UseExistingMonstersAndSumPlausibly()
    {
        foreach (string map in NewMaps)
        {
            int rows = 0;
            foreach (KeyValuePair<string, HuntingSpawnTable.Row> kv in HuntingSpawnTable.Parse(Resources.Load<TextAsset>("HuntingSpawnsFloor1").text))
            {
                if (!kv.Key.StartsWith(map + "/"))
                {
                    continue;
                }
                rows++;
                if (!string.IsNullOrEmpty(kv.Value.Profile))
                {
                    Assert.IsNotNull(MonsterTable.Get(kv.Value.Profile), kv.Key + " 프로필 " + kv.Value.Profile);
                }
            }
            Assert.AreEqual(8, rows, map);
        }
    }

    [Test]
    public void RareNodesAndTreasures_HaveEntriesForNewMaps()
    {
        string rare = Resources.Load<TextAsset>("Floor1RareNodes").text;
        string treasure = Resources.Load<TextAsset>("Floor1Treasures").text;
        foreach (string map in NewMaps)
        {
            StringAssert.Contains("," + map + ",", rare, map);
            StringAssert.Contains("," + map + ",", treasure, map);
        }
    }

    [Test]
    public void UndergroundAnchors_MatchExistingExcavationAndM5()
    {
        string[] lines = Resources.Load<TextAsset>("Floor1UndergroundAnchors").text.Replace("\r", "").Split('\n');
        Assert.GreaterOrEqual(lines.Length, 3);
        string mini = Resources.Load<TextAsset>("MiniBosses").text;
        StringAssert.Contains("ElderGolem_BD,Lv1_TowerOutskirts,-250,-2.9,", mini);
        bool rune = false, garrison = false;
        foreach (string line in lines)
        {
            string[] c = line.Split(',');
            if (c.Length < 5 || c[1] != "Lv1_TowerOutskirts")
            {
                continue;
            }
            rune |= c[4] == "RuneExcavation" && c[2] == "-285" && c[3] == "1.4";
            garrison |= c[4] == "M5Garrison" && c[2] == "-250" && c[3] == "-2.9";
        }
        Assert.IsTrue(rune && garrison);
    }

    [Test]
    public void GroundMoveMul_DefaultsToOneAndScalesMoveSpeed()
    {
        MapInfo.Apply(null);
        Assert.AreEqual(1f, MapInfo.CurrentGroundMoveMul);
        var go = new GameObject("t");
        MapInfo info = go.AddComponent<MapInfo>();
        info.groundMoveMul = 0.8f;
        MapInfo.Apply(info);
        // ProCamera2D 경계가 없으면 Apply 가 일찍 끝나지만 배율은 그 전에 정해진다
        Assert.AreEqual(0.8f, MapInfo.CurrentGroundMoveMul, 0.0001f);
        MapInfo.Apply(null);
        Object.DestroyImmediate(go);
    }
}
