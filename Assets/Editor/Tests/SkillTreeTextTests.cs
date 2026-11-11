using NUnit.Framework;

public class SkillTreeTextTests
{
    [Test]
    public void NodesOf_SplitsByTab()
    {
        Assert.AreEqual(8, SkillTreeRules.NodesOf(SkillTab.Type).Count);
        Assert.AreEqual(5, SkillTreeRules.NodesOf(SkillTab.Combat).Count);
        Assert.AreEqual(0, SkillTreeRules.NodesOf(SkillTab.Life).Count);
        Assert.AreEqual(2, SkillTreeRules.NodesOf(SkillTab.Convenience).Count);
    }

    [Test]
    public void EffectLine_ShowsNowAndNext()
    {
        SkillNode p1 = SkillTreeRules.Find("P1");
        Assert.AreEqual("평타 데미지 +2% (레벨당)", SkillTreeText.EffectLine(p1, 0, WeaponKind.Sword));
        Assert.AreEqual("평타 데미지 +4% (다음 +2%)", SkillTreeText.EffectLine(p1, 2, WeaponKind.Sword));
        Assert.AreEqual("평타 데미지 +10%", SkillTreeText.EffectLine(p1, 5, WeaponKind.Sword));
        Assert.AreEqual("스킬 쿨다운 -3% (레벨당)", SkillTreeText.EffectLine(SkillTreeRules.Find("M2"), 0, WeaponKind.Sword));
    }

    [Test]
    public void IsCapped_UsesDamageSumAndStatCap()
    {
        Assert.IsTrue(SkillTreeText.IsCapped(TreeStat.AttackDamage, 20f, 10f, 20f));
        Assert.IsFalse(SkillTreeText.IsCapped(TreeStat.AttackDamage, 10f, 10f, 10f));
        Assert.IsTrue(SkillTreeText.IsCapped(TreeStat.CritChance, 0f, 0f, 10f));
        Assert.IsFalse(SkillTreeText.IsCapped(TreeStat.MaxHp, 0f, 0f, 999f));
    }

    [Test]
    public void Titles_FollowLevelBands()
    {
        Assert.AreEqual("견습", SkillTreeText.TitleOf(1));
        Assert.AreEqual("숙련자", SkillTreeText.TitleOf(3));
        Assert.AreEqual("명인", SkillTreeText.TitleOf(10));
    }
}
