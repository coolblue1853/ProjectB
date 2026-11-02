using NUnit.Framework;

public class HudPolicyTests
{
    [Test]
    public void QuestHudTop_NoTracker_StartsAtColumnTop()
    {
        Assert.AreEqual(HudPolicy.ColumnTopAboveMiddle, HudPolicy.QuestHudTop(0f), 1e-4f);
    }

    [Test]
    public void QuestHudTop_StacksBelowTracker()
    {
        float top = HudPolicy.QuestHudTop(100f);
        Assert.AreEqual(HudPolicy.ColumnTopAboveMiddle - 100f - HudPolicy.ColumnGap, top, 1e-4f);
        Assert.Less(top, HudPolicy.QuestHudTop(0f));
    }

    [Test]
    public void QuestHudFontSize_ScalesTemplateSizes_AndIsIdempotent()
    {
        float k = 600f / 1080f;
        int title = HudPolicy.QuestHudFontSize(30, k);
        int body = HudPolicy.QuestHudFontSize(24, k);
        Assert.AreEqual(14, title);
        Assert.AreEqual(13, body);
        Assert.AreEqual(title, HudPolicy.QuestHudFontSize(title, k));
        Assert.AreEqual(body, HudPolicy.QuestHudFontSize(body, k));
    }

    [Test]
    public void QuestHudFontSize_FullHeightCanvas_MapsTemplateSizes()
    {
        Assert.AreEqual(26, HudPolicy.QuestHudFontSize(30, 1f));
        Assert.AreEqual(24, HudPolicy.QuestHudFontSize(24, 1f));
    }
}
