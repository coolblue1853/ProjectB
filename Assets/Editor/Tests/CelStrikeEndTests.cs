using NUnit.Framework;

/// <summary>R2 11-15: 스킬 동작 시간이 셀 타격 프레임 끝보다 짧으면 타격 그림이 잘린다 — 끝 시각 계산(StartTimeOf(strike + 1))</summary>
public class CelStrikeEndTests
{
    [Test]
    public void StrikeEnd_UsesPerFrameTimes()
    {
        var clip = new CelClip { frame = 0.1f, loop = false, sprites = new UnityEngine.Sprite[4], times = new[] { 0.08f, 0.09f, 0.1f, 0.15f }, strike = 2 };
        Assert.AreEqual(0.27f, clip.StartTimeOf(clip.strike + 1), 1e-4f);
        Assert.AreEqual(0.17f, clip.StartTimeOf(clip.strike), 1e-4f);
    }

    [Test]
    public void StrikeEnd_FallsBackToFrameWhenNoTimes()
    {
        var clip = new CelClip { frame = 0.1f, loop = false, sprites = new UnityEngine.Sprite[4], strike = 1 };
        Assert.AreEqual(0.2f, clip.StartTimeOf(clip.strike + 1), 1e-4f);
    }
}
