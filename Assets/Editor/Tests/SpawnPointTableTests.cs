using NUnit.Framework;
using UnityEngine;

public class SpawnPointTableTests
{
    [Test]
    public void Parse_ReadsPointsAndSkipsBadRows()
    {
        var rows = SpawnPointTable.Parse("map,spawner,points\nA,Tree,-1:2|3.5:-4\nA,Bad,x");
        Assert.AreEqual(2, rows["A/Tree"].Length);
        Assert.AreEqual(new Vector2(3.5f, -4f), rows["A/Tree"][1]);
        Assert.IsFalse(rows.ContainsKey("A/Bad"));
    }

    [Test]
    public void Floor1Resources_AreNotAtTownExit()
    {
        // 11-01 사용자: 마을 바로 앞(샛길 도착 x −425 근처)에 몰려 있던 나무·광석을 맵 곳곳으로
        var rows = SpawnPointTable.Parse(Resources.Load<TextAsset>("SpawnPointsFloor1").text);
        Assert.AreEqual(10, rows.Count);
        foreach (var pair in rows)
        {
            Assert.GreaterOrEqual(pair.Value.Length, 4, pair.Key);
            foreach (Vector2 p in pair.Value)
            {
                Assert.That(Mathf.Abs(p.x - (-425f)), Is.GreaterThan(20f), pair.Key);
            }
        }
    }
}
