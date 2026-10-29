using NUnit.Framework;

public class UiLockTests
{
    [SetUp]
    public void Reset()
    {
        PlayerStatus.ResetStatics();
        UiLock.Set("A", false);
        UiLock.Set("B", false);
        UiLock.Set("InvGroup", false);
        UiLock.Set("Craft", false);
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
    public void InvGroupAndCraft_Independent()
    {
        UiLock.Set("InvGroup", true);  // 인벤토리·창고·상점 묶음
        UiLock.Set("Craft", true);
        UiLock.Set("InvGroup", false); // 한 창을 닫아도 제작 잠금 유지 (C8)
        Assert.IsTrue(DatabaseManager.isOpenUI);
        UiLock.Set("Craft", false);
        Assert.IsFalse(DatabaseManager.isOpenUI);
    }
}
