using NUnit.Framework;

public class UiLockTests
{
    [SetUp]
    public void Reset()
    {
        PlayerStatus.ResetStatics();
        UiLock.Set("A", false);
        UiLock.Set("B", false);
    }

    [Test]
    public void TwoWindows_ClosingOne_KeepsLock()
    {
        UiLock.Set("A", true);
        UiLock.Set("B", true);
        UiLock.Set("A", false);
        Assert.IsTrue(PlayerStatus.Has(StatusFlag.InputLock));
        UiLock.Set("B", false);
        Assert.IsFalse(PlayerStatus.Has(StatusFlag.InputLock));
    }

    [Test]
    public void DoubleSet_IsIdempotent()
    {
        UiLock.Set("A", true);
        UiLock.Set("A", true);
        Assert.AreEqual(1, PlayerStatus.Count(StatusFlag.InputLock));
        UiLock.Set("A", false);
        UiLock.Set("A", false);
        Assert.AreEqual(0, PlayerStatus.Count(StatusFlag.InputLock));
    }

    [Test]
    public void LegacyAndNamed_Independent()
    {
        DatabaseManager.isOpenUI = true;   // 레거시 UI
        UiLock.Set("A", true);
        DatabaseManager.isOpenUI = false;  // 레거시가 닫혀도 A 잠금 유지
        Assert.IsTrue(DatabaseManager.isOpenUI);
        UiLock.Set("A", false);
        Assert.IsFalse(DatabaseManager.isOpenUI);
    }
}
