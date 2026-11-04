using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ArrivalSafeZoneTests
{
    private static readonly Vector2 Arrival = new Vector2(-236.4f, -0.7f);

    [Test]
    public void NearArrival_IsInside()
    {
        var zones = new List<Vector2> { Arrival };
        Assert.IsTrue(ArrivalSafeZone.IsInside(Arrival + new Vector2(4f, 0f), zones));
        Assert.IsTrue(ArrivalSafeZone.IsInside(Arrival + new Vector2(-10f, 3f), zones));
    }

    [Test]
    public void FarFromArrival_IsOutside()
    {
        var zones = new List<Vector2> { Arrival };
        Assert.IsFalse(ArrivalSafeZone.IsInside(Arrival + new Vector2(ArrivalSafeZone.RadiusX + 1f, 0f), zones));
        Assert.IsFalse(ArrivalSafeZone.IsInside(Arrival + new Vector2(0f, ArrivalSafeZone.RadiusY + 1f), zones));
    }

    [Test]
    public void PickIndex_SkipsPointsInsideZone()
    {
        var zones = new List<Vector2> { Arrival };
        var candidates = new List<Vector2> { Arrival + new Vector2(2f, 0f), Arrival + new Vector2(40f, 0f) };
        Assert.AreEqual(1, ArrivalSafeZone.PickIndex(candidates, 0, zones));
    }

    [Test]
    public void PickIndex_AllInside_ReturnsMinusOne()
    {
        var zones = new List<Vector2> { Arrival };
        var candidates = new List<Vector2> { Arrival, Arrival + new Vector2(3f, 1f) };
        Assert.AreEqual(-1, ArrivalSafeZone.PickIndex(candidates, 1, zones));
    }

    [Test]
    public void PickIndex_NoZones_KeepsPreferred()
    {
        var candidates = new List<Vector2> { Vector2.zero, Vector2.one };
        Assert.AreEqual(1, ArrivalSafeZone.PickIndex(candidates, 1, new List<Vector2>()));
    }

    [Test]
    public void BossBanner_NotShownInTransitOrFar()
    {
        Assert.IsFalse(BossIntroRules.ShouldPlay(true, 3f));
        Assert.IsFalse(BossIntroRules.ShouldPlay(false, BossIntroRules.MaxDistance + 1f));
        Assert.IsTrue(BossIntroRules.ShouldPlay(false, 10f));
    }
}
