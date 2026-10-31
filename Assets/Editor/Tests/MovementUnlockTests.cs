using NUnit.Framework;

public class MovementUnlockTests
{
    [TestCase(1, 0, true, false)]
    [TestCase(1, 1, true, true)]
    [TestCase(1, 1, false, false)]
    [TestCase(2, 0, true, true)]
    [TestCase(2, 1, false, false)]
    [TestCase(3, 0, false, true)]
    [TestCase(7, 1, false, true)]
    public void DashProgression(int floor, int brooch, bool ground, bool expected)
    {
        Assert.That(MovementUnlockRules.CanDash(floor, brooch, ground, false), Is.EqualTo(expected));
    }

    [TestCase(1, 1, false)]
    [TestCase(2, 2, true)]
    [TestCase(7, 2, true)]
    public void JumpAndAirControl(int floor, int jumps, bool open)
    {
        Assert.That(MovementUnlockRules.JumpLimit(2, floor, false), Is.EqualTo(jumps));
        Assert.That(MovementUnlockRules.AirTurn(floor), Is.EqualTo(open));
        Assert.That(MovementUnlockRules.Glide(floor), Is.EqualTo(open));
    }

    [TestCase(-1f, 1f, true, false, 0f)]
    [TestCase(-1f, 1f, true, true, -1f)]
    [TestCase(-1f, 1f, false, false, -1f)]
    [TestCase(1f, 1f, true, false, 1f)]
    [TestCase(0f, 1f, true, false, 0f)]
    public void AirReversalOnlyRestrictsOppositeInput(float input, float launch, bool air, bool open, float expected)
    {
        Assert.That(MovementUnlockRules.AirInput(input, launch, air, open), Is.EqualTo(expected));
    }

    [Test]
    public void DebugOpensWithoutChangingProgression()
    {
        Assert.That(MovementUnlockRules.CanDash(1, 0, false, true), Is.True);
        Assert.That(MovementUnlockRules.JumpLimit(2, 1, true), Is.EqualTo(2));
        Assert.That(MovementUnlockRules.CanDash(1, 0, false, false), Is.False);
        Assert.That(MovementUnlockRules.JumpLimit(1, 7, false), Is.EqualTo(1));
        Assert.That(MovementUnlockRules.JumpLimit(5, 2, false), Is.EqualTo(2));
        Assert.That(MovementUnlockRules.JumpLimit(5, 1, true), Is.EqualTo(2));
    }
}

public class MovementUnlockPersistenceTests
{
    private string _previousPath;
    private UnityEngine.GameObject _go;

    [SetUp]
    public void SetUp()
    {
        _previousPath = ES3Settings.defaultSettings.path;
        ES3Settings.defaultSettings.path = System.IO.Path.Combine(UnityEngine.Application.dataPath, "../Logs/q446-tests.es3");
        ES3.DeleteFile();
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) { UnityEngine.Object.DestroyImmediate(_go); }
        ES3.DeleteFile();
        ES3Settings.defaultSettings.path = _previousPath;
    }

    [Test]
    public void FloorProgressIsMonotonicAndPersists()
    {
        _go = new UnityEngine.GameObject("MovementUnlockTest");
        MovementUnlocks state = _go.AddComponent<MovementUnlocks>();
        state.Reload();
        Assert.That(state.HighestFloor, Is.EqualTo(1));
        Assert.That(state.ReachFloor(3, false), Is.True);
        Assert.That(state.ReachFloor(1, false), Is.False);
        Assert.That(state.ReachFloor(3, false), Is.False);
        state.Reload();
        Assert.That(state.HighestFloor, Is.EqualTo(3));
        Assert.That(state.CanDash(false), Is.True);
        Assert.That(ES3.KeyExists(GlideMotor.UnlockKey), Is.False);
    }

    [Test]
    public void LegacyGlideIsPreservedAndOldKeyIsUntouched()
    {
        ES3.Save(GlideMotor.UnlockKey, true);
        ES3.Save("unrelated", 42);
        _go = new UnityEngine.GameObject("MovementUnlockTest");
        MovementUnlocks state = _go.AddComponent<MovementUnlocks>();
        state.Reload();
        Assert.That(state.HighestFloor, Is.EqualTo(2));
        Assert.That(state.Glide, Is.True);
        Assert.That(state.CanDash(false), Is.False);
        Assert.That(ES3.Load<int>("unrelated"), Is.EqualTo(42));
        Assert.That(ES3.Load<bool>(GlideMotor.UnlockKey), Is.True);
    }

    [Test]
    public void BroochAndDebugRoundTrip()
    {
        ES3.Save(MovementUnlocks.FloorKey, 2);
        _go = new UnityEngine.GameObject("MovementUnlockTest");
        MovementUnlocks state = _go.AddComponent<MovementUnlocks>();
        state.Reload();
        Assert.That(state.ReleaseBrooch(1), Is.True);
        Assert.That(state.ReleaseBrooch(1), Is.False);
        state.DebugAll = true;
        Assert.That(state.CanDash(false), Is.True);
        Assert.That(ES3.Load<int>(MovementUnlocks.FloorKey), Is.EqualTo(2));
        state.DebugAll = false;
        state.Reload();
        Assert.That(state.BroochStage, Is.EqualTo(1));
        Assert.That(state.CanDash(false), Is.False);
    }
}
