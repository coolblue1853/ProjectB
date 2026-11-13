using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 도감 구역 전종 처치(B08~B13) 달성 가능성: 종 목록의 모든 종이 "자기 id 로 처치가 기록되는 실제 스폰"이어야 한다.
/// 기록 id = 처치 메시지 키: 씬 스포너의 프리팹 이름 그대로이거나, 스폰 프로필 행에 killKey(=자기 id)가 있을 때만 자기 id 다.
/// killKey 가 빈 프로필(공유 그림 몹 ForestSpore·DeepSpore·SwampSpore·DeepGoblin)은 원본 프리팹 id 로 기록돼 달성 불가라 구역 목록에서 '-id' 로 뺀다.
/// </summary>
public class BestiarySpawnTests
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    /// <summary>자기 id 로 처치가 기록되는 스폰 id 집합</summary>
    private static HashSet<string> RecordedSpawnIds()
    {
        var ids = new HashSet<string>();
        string csv = Resources.Load<TextAsset>("HuntingSpawnsFloor1").text.Replace("\r", "");
        foreach (string line in csv.Split('\n'))
        {
            string[] c = line.Split(',');
            if (c.Length > 4 && c[4].Trim().Length > 0)
            {
                string p = c[4].Trim();
                int at = p.IndexOf('@');
                string id = at >= 0 ? p.Substring(0, at) : p;
                MonsterTable.Row row = MonsterTable.Get(id);
                if (row != null && row.killKey == id)
                {
                    ids.Add(id);
                }
            }
        }
        string yaml = File.ReadAllText(ScenePath).Replace("\r", "");
        foreach (Match b in Regex.Matches(yaml, @"--- !u!114 &\d+\nMonoBehaviour:(?:(?!\n--- )[\s\S])*"))
        {
            if (b.Value.IndexOf("objectInfos:") < 0)
            {
                continue;
            }
            foreach (Match g in Regex.Matches(b.Value, @"perfab: \{fileID: \d+, guid: ([0-9a-f]{32})"))
            {
                ids.Add(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g.Groups[1].Value)));
            }
        }
        return ids;
    }

    private static List<BestiaryMilestoneDef> ZoneDefs()
    {
        List<BestiaryMilestoneDef> defs = BestiaryMilestoneRules.Parse(Resources.Load<TextAsset>("Floor1BestiaryRewards").text);
        return defs.FindAll(d => d.Metric == "zone");
    }

    [Test]
    public void ZoneRewardSpecies_AllActuallySpawn()
    {
        HashSet<string> recorded = RecordedSpawnIds();
        List<BestiaryMilestoneDef> zones = ZoneDefs();
        Assert.GreaterOrEqual(zones.Count, 6);
        foreach (BestiaryMilestoneDef d in zones)
        {
            foreach (string id in BestiaryZoneRules.SpeciesOf(d.Zone))
            {
                Assert.IsTrue(recorded.Contains(id), d.Id + " 종 " + id + " 는 자기 id 로 처치가 기록되는 스폰이 없어 달성 불가 - Floor1BestiaryRewards zone 칸에 -" + id + " 로 뺀다");
            }
        }
    }

    [Test]
    public void ExcludedTokens_AreReallyUnreachable_SoStaleOnesGetRemoved()
    {
        HashSet<string> recorded = RecordedSpawnIds();
        int tokens = 0;
        foreach (BestiaryMilestoneDef d in ZoneDefs())
        {
            List<string> species = BestiaryZoneRules.SpeciesOf(d.Zone);
            foreach (string token in d.Zone.Split('|'))
            {
                if (!token.StartsWith("-"))
                {
                    continue;
                }
                tokens++;
                string id = token.Substring(1);
                Assert.IsFalse(species.Contains(id), d.Id + " 제외 토큰 " + id + " 가 목록에 남아 있다");
                Assert.IsFalse(recorded.Contains(id), d.Id + " " + id + " 는 이제 자기 id 로 기록된다 - 제외 토큰을 지운다");
            }
        }
        Assert.AreEqual(4, tokens, "ForestSpore, DeepSpore, DeepGoblin, SwampSpore");
    }
}
