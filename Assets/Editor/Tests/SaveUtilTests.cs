using NUnit.Framework;

/// <summary>SaveUtil.LoadOr: 키 없음→기본값, 있음→값. Cache 위치만 써서 디스크에 쓰지 않는다.</summary>
public class SaveUtilTests
{
    const string Key = "saveutil.test.key";
    ES3Settings _s;

    [SetUp] public void SetUp()
    {
        _s = new ES3Settings(ES3.Location.Cache);
        ES3.DeleteKey(Key, _s);
    }

    [TearDown] public void TearDown()
    {
        ES3.DeleteKey(Key, _s);
    }

    [Test] public void MissingKey_ReturnsDefault()
    {
        Assert.AreEqual(7, SaveUtil.LoadOr(Key, 7, _s));
    }

    [Test] public void ExistingKey_ReturnsValue()
    {
        ES3.Save(Key, 42, _s);
        Assert.AreEqual(42, SaveUtil.LoadOr(Key, 7, _s));
    }
}
