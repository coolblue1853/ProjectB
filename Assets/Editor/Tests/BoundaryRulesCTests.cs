using NUnit.Framework;

/// <summary>AUTO-79: 십자 이동·LT 조합·방패 LT 가드 경계값 고정(현재 동작 기록).</summary>
public class BoundaryRulesCTests
{
    // DpadMoveRules: 데드존 없음, stick != 0f 정확 비교
    [Test]
    public void Dpad_StickExactlyZero_UsesDpad()
    {
        Assert.AreEqual(-1f, DpadMoveRules.Resolve(true, 0f, true, false));
        Assert.AreEqual(1f, DpadMoveRules.Resolve(true, 0f, false, true));
    }

    [Test]
    public void Dpad_TinyStick_StickWins()
    {
        Assert.AreEqual(0.0001f, DpadMoveRules.Resolve(true, 0.0001f, true, false));
        Assert.AreEqual(-0.0001f, DpadMoveRules.Resolve(true, -0.0001f, false, true));
    }

    [Test]
    public void Dpad_StickSymmetric()
    {
        Assert.AreEqual(0.5f, DpadMoveRules.Resolve(true, 0.5f, true, false));
        Assert.AreEqual(-0.5f, DpadMoveRules.Resolve(true, -0.5f, false, true));
        Assert.AreEqual(1f, DpadMoveRules.Resolve(true, 1f, true, true));
        Assert.AreEqual(-1f, DpadMoveRules.Resolve(true, -1f, true, true));
    }

    [Test]
    public void Dpad_NegativeZeroStick_TreatedAsZero()
    {
        Assert.AreEqual(1f, DpadMoveRules.Resolve(true, -0f, false, true));
    }

    [Test]
    public void Dpad_NoInput_ReturnsZero()
    {
        Assert.AreEqual(0f, DpadMoveRules.Resolve(true, 0f, false, false));
        Assert.AreEqual(0f, DpadMoveRules.Resolve(true, 0f, true, true));
    }

    [Test]
    public void Dpad_Disabled_ReturnsStickUnchanged()
    {
        Assert.AreEqual(0f, DpadMoveRules.Resolve(false, 0f, true, false));
        Assert.AreEqual(-0.3f, DpadMoveRules.Resolve(false, -0.3f, false, true));
    }

    [Test]
    public void Dpad_NaNStick_PassesThroughNaN()
    {
        // NaN != 0 이라 스틱 우선 분기로 그대로 반환된다(현재 동작 기록; 입력 시스템은 NaN 을 내지 않는다)
        Assert.IsTrue(float.IsNaN(DpadMoveRules.Resolve(true, float.NaN, true, false)));
    }

    [Test]
    public void Dpad_QuickSlotBlock_Boundaries()
    {
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(true, 0));
        Assert.IsTrue(DpadMoveRules.BlocksQuickSlot(true, 1));
        Assert.IsTrue(DpadMoveRules.BlocksQuickSlot(true, 2));
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(true, 3));
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(true, -1));
        Assert.IsFalse(DpadMoveRules.BlocksQuickSlot(false, 1));
    }

    // PadChordRules: 4가지 (lt, 눌림) 조합 전수
    [Test]
    public void RtAlone_AllCombos()
    {
        Assert.IsTrue(PadChordRules.RtAlone(false, true));
        Assert.IsFalse(PadChordRules.RtAlone(true, true));
        Assert.IsFalse(PadChordRules.RtAlone(false, false));
        Assert.IsFalse(PadChordRules.RtAlone(true, false));
    }

    [Test]
    public void FilterChord_AllCombos()
    {
        Assert.IsTrue(PadChordRules.FilterChord(true, true));
        Assert.IsFalse(PadChordRules.FilterChord(false, true));
        Assert.IsFalse(PadChordRules.FilterChord(true, false));
        Assert.IsFalse(PadChordRules.FilterChord(false, false));
    }

    [Test]
    public void LockChord_AllCombos()
    {
        Assert.IsTrue(PadChordRules.LockChord(true, true));
        Assert.IsFalse(PadChordRules.LockChord(false, true));
        Assert.IsFalse(PadChordRules.LockChord(true, false));
        Assert.IsFalse(PadChordRules.LockChord(false, false));
    }

    [Test]
    public void RtAlone_And_FilterChord_AreExclusiveOnSamePress()
    {
        foreach (bool lt in new[] { false, true })
        {
            foreach (bool rt in new[] { false, true })
            {
                Assert.IsFalse(PadChordRules.RtAlone(lt, rt) && PadChordRules.FilterChord(lt, rt));
            }
        }
    }

    // GuardInputRules
    [Test]
    public void Guard_OnlySkillSWithShieldAndLt()
    {
        Assert.IsTrue(GuardInputRules.SuppressSkillTrigger(GuardInputRules.SkillSSlot, true, true));
        Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(GuardInputRules.SkillSSlot, false, true));
        Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(GuardInputRules.SkillSSlot, true, false));
        Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(GuardInputRules.SkillSSlot, false, false));
    }

    [Test]
    public void Guard_OtherSlots_NeverSuppressed()
    {
        foreach (int slot in new[] { 0, 2, 3, 4, -1, int.MaxValue })
        {
            Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(slot, true, true), "slot " + slot);
        }
    }

    [Test]
    public void Guard_SkillSSlotIsOne()
    {
        Assert.AreEqual(1, GuardInputRules.SkillSSlot);
    }
}
