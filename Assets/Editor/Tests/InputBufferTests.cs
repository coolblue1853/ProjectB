using NUnit.Framework;

/// <summary>입력 버퍼·코요테 (PlayerStateDesign Phase 5)</summary>
public class InputBufferTests
{
    [Test]
    public void Buffer_WithinWindow_Consumes()
    {
        var b = new InputBuffer(0.1f);
        b.Press(1.00f);
        Assert.IsTrue(b.Consume(1.09f));
    }

    [Test]
    public void Buffer_AfterWindow_Fails()
    {
        var b = new InputBuffer(0.1f);
        b.Press(1.00f);
        Assert.IsFalse(b.Consume(1.11f));
    }

    [Test]
    public void Buffer_ConsumeOnce()
    {
        var b = new InputBuffer(0.1f);
        b.Press(1f);
        Assert.IsTrue(b.Consume(1.01f));
        Assert.IsFalse(b.Consume(1.02f));
    }

    [Test]
    public void Coyote_WalkOff_AllowsGroundJumpBriefly()
    {
        var c = new CoyoteTimer(0.08f);
        c.SetGrounded(true, 0f);
        c.SetGrounded(false, 1f);
        Assert.IsTrue(c.InCoyote(1.07f));
        Assert.IsFalse(c.InCoyote(1.09f));
        Assert.IsTrue(c.FellPastCoyote(1.09f));
    }

    [Test]
    public void Coyote_AfterJump_NotFalling()
    {
        var c = new CoyoteTimer(0.08f);
        c.SetGrounded(true, 0f);
        c.NotifyJumped();
        c.SetGrounded(false, 1f);
        Assert.IsFalse(c.InCoyote(1.01f));
        Assert.IsFalse(c.FellPastCoyote(2f));
    }

    [Test]
    public void Coyote_LandingResets()
    {
        var c = new CoyoteTimer(0.08f);
        c.SetGrounded(true, 0f);
        c.NotifyJumped();
        c.SetGrounded(false, 0.1f);
        c.SetGrounded(true, 0.5f);
        c.SetGrounded(false, 1f);
        Assert.IsTrue(c.InCoyote(1.05f));
    }
}
