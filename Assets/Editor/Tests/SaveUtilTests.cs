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

    [Test] public void WrongType_LoadOr_ReturnsDefault()
    {
        ES3.Save(Key, "not-an-int", _s); // int 키에 string 저장(손상·타입 변경)
        UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        try
        {
            Assert.AreEqual(7, SaveUtil.LoadOr(Key, 7, _s));
        }
        finally
        {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
        }
    }

    [Test] public void WrongType_LoadOrThrow_Throws()
    {
        ES3.Save(Key, "not-an-int", _s);
        Assert.Catch(() => SaveUtil.LoadOrThrow(Key, 7, _s));
    }

    [Test] public void MissingKey_LoadOrThrow_ReturnsDefault()
    {
        Assert.AreEqual(7, SaveUtil.LoadOrThrow(Key, 7, _s));
    }
}
