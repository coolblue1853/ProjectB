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
    public void RopeAllowsJumpButRequiresAirDashUnlockWithoutGroundContact()
    {
        _go = new UnityEngine.GameObject("MovementUnlockTest");
        MovementUnlocks state = _go.AddComponent<MovementUnlocks>();
        state.Reload();
        state.ReleaseBrooch(1);
        PlayerController player = _go.AddComponent<PlayerController>();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(PlayerController).GetField("_movementUnlocks", flags).SetValue(player, state);
        player.isUpLadder = true;
        player.jumpsRemaining = 1;

        Assert.That(player.CanMovementJump, Is.True);
        Assert.That(player.CanMovementDash, Is.False);
        player.isUpLadder = false;
        var coyote = new CoyoteTimer(0.08f);
        coyote.SetGrounded(true, UnityEngine.Time.time);
        coyote.SetGrounded(false, UnityEngine.Time.time);
        typeof(PlayerController).GetField("_coyote", flags).SetValue(player, coyote);
        Assert.That(player.CanMovementJump, Is.True);
        Assert.That(player.CanMovementDash, Is.False);
        state.ReachFloor(3, false);
        Assert.That(player.CanMovementDash, Is.True);
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

    [Test]
    public void StandingOnGroundKeepsSingleJumpAcrossPhysicsTicks()
    {
        PlayerController player = CreateGroundedPlayer();
        InvokeGroundCheck(player);
        InvokeGroundCheck(player);
        Assert.That(player.isGround, Is.True);
        Assert.That(player.isOnGround, Is.True);
        Assert.That(player.jumpsRemaining, Is.EqualTo(1));
        Assert.That(player.CanMovementJump, Is.True);
    }

    [Test]
    public void SaveDatasPreservesMapAlongsideProgress()
    {
        var data = new Datas { mapName = "Lv1_NormalForest", level = 5, exp = 123, money = 456 };
        ES3.Save("firsthour.datas", data);
        Datas loaded = ES3.Load<Datas>("firsthour.datas");
        Assert.That(loaded.mapName, Is.EqualTo("Lv1_NormalForest"));
        Assert.That(loaded.level, Is.EqualTo(5));
        Assert.That(loaded.exp, Is.EqualTo(123));
        Assert.That(loaded.money, Is.EqualTo(456));
    }

    [Test]
    public void RisingFromGroundDoesNotRefillSingleJump()
    {
        PlayerController player = CreateGroundedPlayer();
        player.jumpsRemaining = 0;
        player.GetComponent<UnityEngine.Rigidbody2D>().linearVelocity = UnityEngine.Vector2.up * 10f;
        InvokeGroundCheck(player);
        Assert.That(player.isGround, Is.False);
        Assert.That(player.jumpsRemaining, Is.Zero);
        Assert.That(player.CanMovementJump, Is.False);
    }

    private PlayerController CreateGroundedPlayer()
    {
        _go = new UnityEngine.GameObject("FirstHourGroundTest");
        _go.transform.position = new UnityEngine.Vector3(4000f, 4000f, 0f);
        MovementUnlocks state = _go.AddComponent<MovementUnlocks>();
        state.Reload();
        var rb = _go.AddComponent<UnityEngine.Rigidbody2D>();
        rb.gravityScale = 0f;
        PlayerController player = _go.AddComponent<PlayerController>();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(PlayerController).GetField("_movementUnlocks", flags).SetValue(player, state);
        typeof(PlayerController).GetField("rb", flags).SetValue(player, rb);
        var ground = new UnityEngine.GameObject("Ground");
        ground.transform.SetParent(_go.transform, false);
        ground.transform.localPosition = UnityEngine.Vector3.down;
        ground.layer = UnityEngine.LayerMask.NameToLayer("Ground");
        ground.AddComponent<UnityEngine.BoxCollider2D>().size = new UnityEngine.Vector2(2f, 1f);
        player.groundCheck = NewGroundCheck(-0.1f);
        player.groundCheck2 = NewGroundCheck(0.1f);
        UnityEngine.Physics2D.SyncTransforms();
        return player;
    }

    private UnityEngine.GameObject NewGroundCheck(float x)
    {
        var check = new UnityEngine.GameObject("GroundCheck");
        check.transform.SetParent(_go.transform, false);
        check.transform.localPosition = new UnityEngine.Vector3(x, -0.25f, 0f);
        return check;
    }

    private static void InvokeGroundCheck(PlayerController player)
    {
        typeof(PlayerController).GetMethod("FixedUpdate", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.NonPublic).Invoke(player, null);
    }
}
