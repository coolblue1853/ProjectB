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
    public void RefundOnKill_InternalCooldownOncePerKill_EmptyP3Always()
    {
        var next = new Dictionary<string, float>();
        var kill = new Dictionary<string, bool>();
        Assert.IsTrue(RuleMath.RefundOnKill(next, kill, "a", 0f, "1"));
        Assert.IsTrue(RuleMath.RefundOnKill(next, kill, "a", 0f, "1"), "같은 처치 안의 다른 스킬도 환급(모든 스킬)");
        kill.Clear();
        Assert.IsFalse(RuleMath.RefundOnKill(next, kill, "a", 0.5f, "1"), "내부 쿨 1초 안의 다음 처치는 막힘");
        Assert.IsFalse(RuleMath.RefundOnKill(next, kill, "a", 0.5f, "1"));
        kill.Clear();
        Assert.IsTrue(RuleMath.RefundOnKill(next, kill, "a", 1.0f, "1"));
        kill.Clear();
        Assert.IsTrue(RuleMath.RefundOnKill(next, kill, "b", 1.1f, ""), "p3 비면 내부 쿨 없음(기존 동작)");
        Assert.IsTrue(RuleMath.RefundOnKill(next, kill, "b", 1.1f, null));
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

    static EffectEntry Mod(string sel, EffectOp op, float v, string cond = null)
    {
        return new EffectEntry { id = "T", kind = EffectKind.Mod, selector = sel, op = op, value = v, cond = cond };
    }

    [Test]
    public void BasicDamagePct_OnlyBasicSelector_AllExcluded_Capped()
    {
        var act = new List<ActiveEffect>
        {
            new ActiveEffect { entry = Mod("Basic", EffectOp.DamagePct, 10f) },
            new ActiveEffect { entry = Mod("tag:Basic", EffectOp.DamagePct, 15f) },
            new ActiveEffect { entry = Mod("all", EffectOp.DamagePct, 50f) },
            new ActiveEffect { entry = Mod("Basic", EffectOp.RangePct, 50f) },
            new ActiveEffect { entry = Mod("Basic", EffectOp.DamagePct, 99f, "Night") },
        };
        Assert.AreEqual(25f, EffectResolver.BasicDamagePct(act), 0.001f);
        act.Add(new ActiveEffect { entry = Mod("Basic", EffectOp.DamagePct, 500f) });
        Assert.AreEqual(EffectCaps.DamagePct, EffectResolver.BasicDamagePct(act), 0.001f);
        Assert.AreEqual(0f, EffectResolver.BasicDamagePct(null));
    }

    [Test]
    public void BasicDamageMul_SkillHitIgnored_RecomputeApplies()
    {
        SkillEffectService.ClearAll();
        EffectTable.Build(new List<EffectEntry> { Mod("Basic", EffectOp.DamagePct, 20f) });
        EffectTable.Get("T");
        SkillEffectService.AttachIds("Test:Basic", "T");
        Assert.AreEqual(1.2f, SkillEffectService.BasicDamageMul(false), 0.001f);
        Assert.AreEqual(1f, SkillEffectService.BasicDamageMul(true));
        SkillEffectService.ClearAll();
        Assert.AreEqual(1f, SkillEffectService.BasicDamageMul(false));
        EffectTable.ResetCache();
    }

    [Test]
    public void TooltipLines_DescribeAllEntries_SkipUnknown()
    {
        EffectTable.Build(new List<EffectEntry>
        {
            Mod("Gate_Cleave", EffectOp.DamagePct, 15f),
            new EffectEntry { id = "T", entry = 2, kind = EffectKind.Rule, selector = "Gate_Cleave", rule = EffectRule.OnKillCast, p1 = "Stone_Sentinels", p2 = "20" },
        });
        List<string> lines = EffectTooltip.Lines("T;NoSuch", null, e => e.kind == EffectKind.Rule ? " (inactive)" : "");
        Assert.AreEqual(2, lines.Count);
        StringAssert.Contains("damage +15%", lines[0]);
        StringAssert.EndsWith("(inactive)", lines[1]);
        string block = EffectTooltip.Block(lines, "Special Effects");
        StringAssert.Contains("Special Effects", block);
        Assert.AreEqual(string.Empty, EffectTooltip.Block(new List<string>(), "H"));
        EffectTable.ResetCache();
    }

    [Test]
    public void EffectScore_FollowsDesignTable()
    {
        Assert.AreEqual(10f, EffectScore.Of(Mod("x", EffectOp.CooldownPct, 20f)), 0.001f);
        Assert.AreEqual(4f, EffectScore.Of(Mod("x", EffectOp.ProjectileAdd, 1f)), 0.001f);
        Assert.AreEqual(6f, EffectScore.Of(Mod("x", EffectOp.CooldownPct, 20f, "Night")), 0.001f);
        Assert.AreEqual(12f, EffectScore.Of(new EffectEntry { kind = EffectKind.Rule, rule = EffectRule.Chain }), 0.001f);
        Assert.AreEqual("none", EffectScore.Judge("R", 0f));
        Assert.AreEqual("ok", EffectScore.Judge("R", 11f));
        Assert.AreEqual("over", EffectScore.Judge("R", 14f));
        Assert.AreEqual("n/a", EffectScore.Judge("C", 0f));
    }

    [Test]
    public void SetItemCsv_EffectIds_AllExist()
    {
        EffectTable.ResetCache();
        EffectTable.Load();
        string[] lines = UnityEngine.Resources.Load<UnityEngine.TextAsset>("SetItem").text.Replace("\r", "").Split('\n');
        int withIds = 0;
        for (int i = 2; i < lines.Length; i++)
        {
            string[] c = lines[i].Split(',');
            if (c.Length < 3 || string.IsNullOrWhiteSpace(c[c.Length - 1]))
            {
                continue;
            }
            withIds++;
            foreach (string id in SkillEffectService.SplitIds(c[c.Length - 1]))
            {
                Assert.IsNotNull(EffectTable.Get(id), c[0] + " -> " + id);
            }
        }
        Assert.AreEqual(11, withIds);
    }

    [Test]
    public void CooldownCombined_MultipliesEquipAndTree()
    {
        Assert.AreEqual(66f, CooldownMath.CombinedPct(60f, 15f), 0.001f); // 0.4 x 0.85 = 0.34
        Assert.AreEqual(60f, CooldownMath.CombinedPct(60f, 0f), 0.001f);
        Assert.AreEqual(15f, CooldownMath.CombinedPct(0f, 15f), 0.001f);
        Assert.AreEqual(66f, CooldownMath.CombinedPct(90f, 40f), 0.001f, "각 버킷이 먼저 상한(60·15)을 통과한다");
        Assert.AreEqual(5.1f, CooldownMath.Apply(15f, CooldownMath.CombinedPct(60f, 15f)), 0.001f);
    }
}
