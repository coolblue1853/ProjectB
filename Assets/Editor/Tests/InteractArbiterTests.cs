using NUnit.Framework;
using UnityEngine;

/// <summary>U32: ↑ 상호작용 중재 — 가장 가까운 후보만 통과, 같은 자리 후보는 함께 통과</summary>
public sealed class InteractArbiterTests
{
    private GameObject _a;
    private GameObject _b;
    private GameObject _c;

    [SetUp]
    public void SetUp()
    {
        InteractArbiter.Clear();
        _a = new GameObject("A");
        _b = new GameObject("B");
        _c = new GameObject("C");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_a);
        Object.DestroyImmediate(_b);
        Object.DestroyImmediate(_c);
        InteractArbiter.Clear();
    }

    [Test]
    public void OutOfRange_NeverWins()
    {
        Assert.IsFalse(InteractArbiter.Contend(_a, Vector3.zero, false));
    }

    [Test]
    public void SingleCandidate_Wins()
    {
        Assert.IsTrue(InteractArbiter.Contend(_a, Vector3.zero, true));
    }

    [Test]
    public void NoPlayer_AllInRangeCandidatesPass()
    {
        // 플레이어가 없으면 거리는 0 으로 같다 → 같은 자리로 취급되어 둘 다 통과(기존 동작 유지)
        Assert.IsTrue(InteractArbiter.Contend(_a, new Vector3(0f, 0f, 0f), true));
        Assert.IsTrue(InteractArbiter.Contend(_b, new Vector3(5f, 0f, 0f), true));
    }
}
