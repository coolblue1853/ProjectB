using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>씬을 저장하지 않고 기존 발판 중심에서 화면 기대 밀도와 트리거 내 총 슬롯을 표본 검사한다.</summary>
public static class HuntingDensityAudit
{
    public static string Run()
    {
        var output = new StringBuilder("map,time,anchors,minBefore,maxBefore,minAfter,maxAfter,anchorPeakActiveSlots,failures\n");
        var old = File.ReadAllLines("Docs/Balance/Floor1HuntingBaseline.csv").Skip(1)
            .Select(s => s.Split(',')).ToDictionary(c => c[0] + "/" + c[1]);
        foreach (MapInfo map in UnityEngine.Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!map.name.StartsWith("Lv1_", StringComparison.Ordinal))
            {
                continue;
            }
            foreach (bool day in new[] { true, false })
            {
                EnemySpawner[] spawners = map.GetComponentsInChildren<EnemySpawner>(true)
                    .Where(s => s.isDaytimeSpowner == day && HuntingSpawnTable.Get(map.name, s.name) != null).ToArray();
                var positions = new List<Vector2>();
                foreach (EnemySpawner spawner in spawners)
                {
                    foreach (Transform anchor in spawner.enemyPositon.transform)
                    {
                        positions.Add(anchor.position);
                    }
                }
                double minBefore = double.MaxValue, maxBefore = 0, minAfter = double.MaxValue, maxAfter = 0;
                int peak = 0, failures = 0;
                foreach (Vector2 center in positions)
                {
                    double before = 0, after = 0;
                    int active = 0;
                    foreach (EnemySpawner spawner in spawners)
                    {
                        BoxCollider2D box = spawner.GetComponent<BoxCollider2D>();
                        Vector3 local = spawner.transform.InverseTransformPoint(new Vector3(center.x, center.y, spawner.transform.position.z));
                        if (Mathf.Abs(local.x - box.offset.x) > box.size.x / 2 || Mathf.Abs(local.y - box.offset.y) > box.size.y / 2)
                        {
                            continue;
                        }
                        HuntingSpawnTable.Row setting = HuntingSpawnTable.Get(map.name, spawner.name);
                        int points = spawner.enemyPositon.transform.childCount;
                        int[] indices = HuntingSpawnTable.PositionIndices(setting.Count, points, setting.PointWeights);
                        int previous = int.Parse(old[map.name + "/" + spawner.name][4]);
                        active += setting.Count;
                        if (setting.PointWeights.Length != points)
                        {
                            failures++;
                        }
                        for (int i = 0; i < previous; i++)
                        {
                            before += HuntingRules.VisibleChance(spawner.enemyPositon.transform.GetChild(i).position, center);
                        }
                        foreach (int index in indices)
                        {
                            after += HuntingRules.VisibleChance(spawner.enemyPositon.transform.GetChild(index).position, center);
                        }
                    }
                    minBefore = Math.Min(minBefore, before); maxBefore = Math.Max(maxBefore, before);
                    minAfter = Math.Min(minAfter, after); maxAfter = Math.Max(maxAfter, after);
                    peak = Math.Max(peak, active);
                    if (after < 5.999 || after > 12.001)
                    {
                        failures++;
                    }
                }
                output.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3:F2},{4:F2},{5:F2},{6:F2},{7},{8}",
                    map.name, day ? "day" : "night", positions.Count, minBefore, maxBefore, minAfter, maxAfter, peak, failures));
            }
        }
        Directory.CreateDirectory("Logs/playtest");
        File.WriteAllText("Logs/playtest/hunt_density_verified.csv", output.ToString());
        return output.ToString();
    }
}
