using NUnit.Framework;

public class GuardInputRulesTests
{
    [Test]
    public void ShieldEquipped_LtSkillS_Suppressed()
    {
        Assert.IsTrue(GuardInputRules.SuppressSkillTrigger(GuardInputRules.SkillSSlot, true, true));
    }

    [Test]
    public void KeyboardS_NeverSuppressed()
    {
        Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(GuardInputRules.SkillSSlot, true, false));
    }

    [Test]
    public void NoShield_LtSkillS_Works()
    {
        Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(GuardInputRules.SkillSSlot, false, true));
    }

    [Test]
    public void OtherSlots_Untouched()
    {
        Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(0, true, true));
        Assert.IsFalse(GuardInputRules.SuppressSkillTrigger(2, true, true));
    }
}
