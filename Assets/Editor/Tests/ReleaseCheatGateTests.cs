using NUnit.Framework;
using SRDebugger;

public class ReleaseCheatGateTests
{
    [Test]
    public void RestoreIfPending_RestoresOriginalValueAndClearsKey()
    {
        Settings settings = Settings.Instance;
        Assert.IsNotNull(settings);
        if (ReleaseCheatGate.HasPending)
        {
            Assert.Ignore("복원 대기 중인 실제 값이 있어 건너뜀");
        }

        bool original = settings.IsEnabled;
        try
        {
            ReleaseCheatGate.SetPendingForTest(original);
            settings.IsEnabled = !original; // 빌드 실패로 값이 바뀐 채 남은 상태를 흉내
            ReleaseCheatGate.RestoreIfPending();
            Assert.AreEqual(original, settings.IsEnabled);
            Assert.IsFalse(ReleaseCheatGate.HasPending);
        }
        finally
        {
            settings.IsEnabled = original;
        }
    }
}
