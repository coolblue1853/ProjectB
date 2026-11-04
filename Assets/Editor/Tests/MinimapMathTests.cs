using NUnit.Framework;
using UnityEngine;

public class MinimapMathTests
{
    [Test]
    public void WorldToUv_MapsAndClamps()
    {
        var area = new Rect(-10f, 0f, 20f, 10f);
        Assert.AreEqual(new Vector2(0.5f, 0.5f), MinimapMath.WorldToUv(new Vector2(0f, 5f), area));
        Assert.AreEqual(new Vector2(0f, 1f), MinimapMath.WorldToUv(new Vector2(-50f, 50f), area));
        Assert.AreEqual(new Vector2(0.5f, 0.5f), MinimapMath.WorldToUv(Vector2.zero, new Rect(0f, 0f, 0f, 0f)));
    }

    [Test]
    public void Fit_KeepsAspectWithinMax()
    {
        Vector2 wide = MinimapMath.Fit(new Vector2(400f, 50f), new Vector2(340f, 150f), 40f);
        Assert.AreEqual(340f, wide.x, 1e-3f);
        Assert.AreEqual(42.5f, wide.y, 1e-3f);
        Vector2 tall = MinimapMath.Fit(new Vector2(10f, 100f), new Vector2(340f, 150f), 40f);
        Assert.AreEqual(150f, tall.y, 1e-3f);
        Assert.AreEqual(40f, tall.x, 1e-3f); // 최소 변
    }

    [Test]
    public void IsVisitWanted_OnlyUnfinishedMatchingVisit()
    {
        var q = new NpcQuestDef { Objective = NpcQuestObjective.Visit, Target = "m1", Count = 1 };
        var other = new NpcQuestDef { Objective = NpcQuestObjective.Visit, Target = "m2", Count = 1 };
        var list = new[] { q, other };
        Assert.IsTrue(MinimapMath.IsVisitWanted(list, "m1", d => 0));
        Assert.IsFalse(MinimapMath.IsVisitWanted(list, "m1", d => 1));
        Assert.IsFalse(MinimapMath.IsVisitWanted(list, "m3", d => 0));
        Assert.IsFalse(MinimapMath.IsVisitWanted(null, "m1", d => 0));
    }
}
