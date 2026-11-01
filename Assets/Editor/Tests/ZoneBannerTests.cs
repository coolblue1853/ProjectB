using NUnit.Framework;

public class ZoneBannerTests
{
    [Test]
    public void ShouldShow_FirstVisitThenCooldown()
    {
        Assert.IsTrue(ZoneBanner.ShouldShow(0f, false, 10f));              // 처음
        Assert.IsFalse(ZoneBanner.ShouldShow(10f, true, 70f));              // 마을 왕복(1분 뒤)
        Assert.IsTrue(ZoneBanner.ShouldShow(10f, true, 10f + ZoneBanner.RepeatCooldown));
    }
}
