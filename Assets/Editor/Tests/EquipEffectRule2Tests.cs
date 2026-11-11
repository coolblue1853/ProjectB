using NUnit.Framework;
using System.Collections.Generic;

/// <summary>REWORK-12 P4: Rule 2차 순수 계산 (Chain 감쇠·내부 쿨·트리거 해석)</summary>
public class EquipEffectRule2Tests
{
    [Test]
    public void ChainDamage_DecaysPerJump_MinOne()
    {
        Assert.AreEqual(75, RuleMath.ChainDamage(100, 25f, 0));
        Assert.AreEqual(56, RuleMath.ChainDamage(100, 25f, 1));
        Assert.AreEqual(1, RuleMath.ChainDamage(1, 90f, 3));
    }

    [Test]
    public void ScaleFirstTwo_OnlyDamageValues_NewArray()
    {
        int[] src = { 100, 120, 30, 5 };
        int[] r = RuleMath.ScaleFirstTwo(src, 50f);
        CollectionAssert.AreEqual(new[] { 50, 60, 30, 5 }, r);
        Assert.AreEqual(100, src[0]);
    }

    [Test]
    public void InternalCooldown_BlocksUntilTime_MinPointThree()
    {
        var next = new Dictionary<string, float>();
        Assert.IsTrue(RuleMath.PassInternalCooldown(next, "a", 0f, 1f));
        Assert.IsFalse(RuleMath.PassInternalCooldown(next, "a", 0.9f, 1f));
        Assert.IsTrue(RuleMath.PassInternalCooldown(next, "a", 1.0f, 0f));
        Assert.IsFalse(RuleMath.PassInternalCooldown(next, "a", 1.2f, 0f)); // cd 0 도 0.3초 하한
        Assert.IsTrue(RuleMath.PassInternalCooldown(next, "b", 0f, 1f));
    }

    [Test]
    public void Trigger_ParsesKillHit_UnknownIsNull()
    {
        Assert.AreEqual("Kill", RuleMath.Trigger(""));
        Assert.AreEqual("Kill", RuleMath.Trigger("kill"));
        Assert.AreEqual("Hit", RuleMath.Trigger("Hit"));
        Assert.IsNull(RuleMath.Trigger("Crit"));
    }

    [Test]
    public void SameSkill_SpaceEqualsUnderscore()
    {
        Assert.IsTrue(RuleMath.SameSkill("Scattering Leaves", "Scattering_Leaves"));
        Assert.IsFalse(RuleMath.SameSkill("A", ""));
    }

    [Test]
    public void EventRulesParse_FromCsv()
    {
        string csv = "EF_X,1,Rule,all,,,,,Chain,3,5,30,,,\nEF_X,2,Rule,all,,,,,OnKillCast,Foo,50,1,,,";
        List<EffectEntry> l = EffectTable.ParseEntries(csv);
        Assert.AreEqual(2, l.Count);
        Assert.AreEqual(EffectRule.Chain, l[0].rule);
        Assert.AreEqual(EffectRule.OnKillCast, l[1].rule);
    }

    [Test]
    public void Replace_NoRule_ReturnsBase_WithRule_ReturnsVariant()
    {
        var baseGo = new UnityEngine.GameObject("base");
        var m = new SkillMods();
        Assert.AreSame(baseGo, SkillVariants.Resolve(baseGo, m));
        m.rules = new[] { new EffectEntry { kind = EffectKind.Rule, rule = EffectRule.Replace, p1 = "GateCleave_Wide" } };
        Assert.AreEqual("GateCleave_Wide", SkillVariants.KeyOf(m));
        UnityEngine.GameObject v = SkillVariants.Resolve(baseGo, m);
        Assert.AreNotSame(baseGo, v);
        Assert.IsNotNull(v.GetComponent<DamageObject>());
        UnityEngine.Object.DestroyImmediate(baseGo);
    }

    [Test]
    public void Replace_MissingVariant_FallsBackToBase()
    {
        var baseGo = new UnityEngine.GameObject("base2");
        string key = "NoSuchVariant_" + System.Guid.NewGuid().ToString("N"); // 정적 1회 경고 집합과 무관하게
        var m = new SkillMods { rules = new[] { new EffectEntry { kind = EffectKind.Rule, rule = EffectRule.Replace, p1 = key } } };
        UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("NoSuchVariant_"));
        Assert.AreSame(baseGo, SkillVariants.Resolve(baseGo, m));
        UnityEngine.Object.DestroyImmediate(baseGo);
    }

    [Test]
    public void GateCleaveWide_IsSeparateAsset_BaseUntouched()
    {
        UnityEngine.GameObject basePf = UnityEngine.Resources.Load<UnityEngine.GameObject>("Weapon/DamageObject/GatekeepersOath/GatekeepersOathSlashWave");
        UnityEngine.GameObject v = SkillVariants.Load("GateCleave_Wide");
        Assert.IsNotNull(basePf);
        Assert.IsNotNull(v);
        Assert.AreNotSame(basePf, v);
        Assert.Greater(v.transform.localScale.x, basePf.transform.localScale.x);
        Assert.AreEqual(0.3f, basePf.transform.localScale.x, 0.001f, "공유 원본 스케일 무변경");
    }
}
