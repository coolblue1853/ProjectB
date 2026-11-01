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
    [Test]
    public void PendingBanner_WaitsForFadeAndRejectsInactiveMap()
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var go = new UnityEngine.GameObject("BannerTest");
        var map = new UnityEngine.GameObject("InactiveMap");
        var fade = new UnityEngine.GameObject("FadeTest");
        var banner = go.AddComponent<ZoneBanner>();
        var info = map.AddComponent<MapInfo>();
        var fader = fade.AddComponent<ScreenFader>();
        var instance = typeof(ScreenFader).GetField("instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        object previous = instance.GetValue(null);
        try
        {
            instance.SetValue(null, fader);
            typeof(ScreenFader).GetField("_waiting", flags).SetValue(fader, true);
            var pending = (System.Collections.IEnumerator)typeof(ZoneBanner).GetMethod("ShowWhenVisible", flags).Invoke(banner, new object[] { info });
            Assert.IsTrue(pending.MoveNext()); // 맵 변경과 같은 프레임의 암전 시작을 기다린다.
            Assert.IsTrue(pending.MoveNext()); // 암전 중에는 배너를 시작하지 않는다.
            Assert.IsTrue(pending.MoveNext());
            map.SetActive(false);
            typeof(ScreenFader).GetField("_waiting", flags).SetValue(fader, false);
            Assert.IsFalse(pending.MoveNext()); // 대기 중 비활성화된 맵은 표시하지 않는다.
        }
        finally
        {
            instance.SetValue(null, previous);
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(map);
            UnityEngine.Object.DestroyImmediate(fade);
        }
    }
}
