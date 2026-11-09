using NUnit.Framework;
using UnityEngine;

/// <summary>장비 효과 P3 (EquipEffectDesign §2.3·§5): FireMode 전략, 기본 전략 = 기존 RoundSpawn 수식, Rule 핸들러 부착·원복</summary>
public class EquipEffectRuleTests
{
    static SkillMods WithRule(EffectRule rule, string p1, string p2 = "", string p3 = "")
    {
        var m = new SkillMods();
        m.rules = new[] { new EffectEntry { kind = EffectKind.Rule, rule = rule, p1 = p1, p2 = p2, p3 = p3 } };
        return m;
    }

    // 기존 Skill.RoundSpawn 의 수식을 그대로 옮긴 기준값
    static void Legacy(int i, int n, float s, float e, float r, bool left, out Vector2 off, out Vector2 dir)
    {
        float step = n > 1 ? (e - s) / (n - 1) : 0f;
        float a = s + i * step;
        if (left)
        {
            a = 180f - a;
        }
        float rad = a * Mathf.Deg2Rad;
        float x = r * Mathf.Cos(rad);
        float y = r * Mathf.Sin(rad);
        off = new Vector2(x, y);
        dir = new Vector2(x, y).normalized;
    }

    [Test]
    public void Default_NoRules_EqualsLegacyFormula()
    {
        float[] radii = { 0.5f, 5f, 0f };
        bool[] faces = { false, true };
        float[][] ranges = { new[] { -30f, 30f }, new[] { 0f, 360f }, new[] { 10f, 10f } };
        foreach (float r in radii)
        {
            foreach (bool left in faces)
            {
                foreach (float[] rg in ranges)
                {
                    for (int n = 1; n <= 8; n++)
                    {
                        IFirePattern p = FirePatterns.Create(new SkillMods(), rg[0], rg[1], r, left);
                        Assert.AreEqual(n, p.Count(n));
                        for (int i = 0; i < n; i++)
                        {
                            p.Get(i, n, out Vector2 o, out Vector2 d);
                            Legacy(i, n, rg[0], rg[1], r, left, out Vector2 lo, out Vector2 ld);
                            Assert.AreEqual(lo, o);
                            Assert.AreEqual(ld, d);
                        }
                    }
                }
            }
        }
    }

    [Test]
    public void Default_UnknownMode_FallsBack()
    {
        IFirePattern p = FirePatterns.Create(WithRule(EffectRule.FireMode, "Spiral"), -30f, 30f, 0.5f, false);
        Assert.IsInstanceOf<DefaultFirePattern>(p);
    }

    [Test]
    public void Fan_UsesGivenArcCenteredOnBase()
    {
        IFirePattern p = FirePatterns.Create(WithRule(EffectRule.FireMode, "Fan", "90"), -30f, 30f, 0.5f, false);
        Assert.AreEqual(4, p.Count(4));
        p.Get(0, 4, out _, out Vector2 first);
        p.Get(3, 4, out _, out Vector2 last);
        Assert.AreEqual(-45f, Mathf.Atan2(first.y, first.x) * Mathf.Rad2Deg, 0.01f);
        Assert.AreEqual(45f, Mathf.Atan2(last.y, last.x) * Mathf.Rad2Deg, 0.01f);
    }

    [Test]
    public void Fan_FaceLeft_Mirrors()
    {
        IFirePattern p = FirePatterns.Create(WithRule(EffectRule.FireMode, "Fan", "90"), -30f, 30f, 0.5f, true);
        p.Get(3, 4, out _, out Vector2 last);
        Assert.Less(last.x, 0f);
    }

    [Test]
    public void Ring_EvenlyAround()
    {
        IFirePattern p = FirePatterns.Create(WithRule(EffectRule.FireMode, "Ring"), 0f, 0f, 1f, false);
        p.Get(0, 4, out _, out Vector2 d0);
        p.Get(1, 4, out _, out Vector2 d1);
        p.Get(2, 4, out _, out Vector2 d2);
        Assert.AreEqual(1f, d0.x, 0.001f);
        Assert.AreEqual(1f, d1.y, 0.001f);
        Assert.AreEqual(-1f, d2.x, 0.001f);
    }

    [Test]
    public void Parallel_SameDirectionSpreadOffsets()
    {
        IFirePattern p = FirePatterns.Create(WithRule(EffectRule.FireMode, "Parallel", "0.6"), -30f, 30f, 0f, false);
        p.Get(0, 3, out Vector2 o0, out Vector2 d0);
        p.Get(2, 3, out Vector2 o2, out Vector2 d2);
        Assert.AreEqual(d0.x, d2.x, 0.0001f);
        Assert.AreEqual(1.2f, Vector2.Distance(o0, o2), 0.001f);
    }

    [Test]
    public void Backward_DoublesCountMirroredHalf()
    {
        IFirePattern p = FirePatterns.Create(WithRule(EffectRule.FireMode, "Backward"), -30f, 30f, 0.5f, false);
        Assert.AreEqual(8, p.Count(4));
        p.Get(0, 8, out _, out Vector2 front);
        p.Get(4, 8, out _, out Vector2 back);
        Assert.Greater(front.x, 0f);
        Assert.Less(back.x, 0f);
        Assert.AreEqual(front.y, back.y, 0.001f);
    }

    static DamageObject NewProjectile(out GameObject go)
    {
        go = new GameObject("rule_test");
        go.AddComponent<Rigidbody2D>();
        DamageObject d = go.AddComponent<DamageObject>();
        d.isLaunch = true;
        d.maxDamagedEnemies = 1;
        d.holdingTime = 1f;
        d.isDestroyByTime = true;
        return d;
    }

    [Test]
    public void Handlers_NoRules_AttachNothing()
    {
        DamageObject d = NewProjectile(out GameObject go);
        SkillRuleHandlers.ApplyToProjectile(d, new SkillMods());
        Assert.IsNull(go.GetComponent<RuleHoming>());
        Assert.IsNull(go.GetComponent<RuleReturn>());
        Assert.IsNull(go.GetComponent<RuleOnHit>());
        Assert.AreEqual(1, d.maxDamagedEnemies);
        Assert.AreEqual(1f, d.holdingTime);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Handlers_AttachByRule_AndPierceClamped()
    {
        DamageObject d = NewProjectile(out GameObject go);
        SkillRuleHandlers.ApplyToProjectile(d, WithRule(EffectRule.Homing, "540", "9"));
        Assert.IsNotNull(go.GetComponent<RuleHoming>());
        SkillRuleHandlers.ApplyToProjectile(d, WithRule(EffectRule.Pierce, "99"));
        Assert.AreEqual(1 + EffectCaps.PierceAdd, d.maxDamagedEnemies);
        SkillRuleHandlers.ApplyToProjectile(d, WithRule(EffectRule.OnHitSpawn, "Slow", "30", "1"));
        Assert.IsNotNull(go.GetComponent<RuleOnHit>());
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Handlers_Return_DoublesLifeOnce()
    {
        DamageObject d = NewProjectile(out GameObject go);
        SkillRuleHandlers.ApplyToProjectile(d, WithRule(EffectRule.Return, "1"));
        SkillRuleHandlers.ApplyToProjectile(d, WithRule(EffectRule.Return, "1")); // 이중 부착 방지
        Assert.AreEqual(2f, d.holdingTime);
        Assert.AreEqual(1, go.GetComponents<RuleReturn>().Length);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Handlers_NonProjectile_IgnoresMovementRules()
    {
        DamageObject d = NewProjectile(out GameObject go);
        d.isLaunch = false; // 근접 판정
        SkillRuleHandlers.ApplyToProjectile(d, WithRule(EffectRule.Homing, "540"));
        SkillRuleHandlers.ApplyToProjectile(d, WithRule(EffectRule.Return, "1"));
        Assert.IsNull(go.GetComponent<RuleHoming>());
        Assert.IsNull(go.GetComponent<RuleReturn>());
        Object.DestroyImmediate(go);
    }

    [Test]
    public void DevCsv_RuleRowsParse()
    {
        string[] ids = { "EF_TEST_LeafFan", "EF_DEV_Ring", "EF_DEV_Seek", "EF_DEV_Boomerang", "EF_DEV_Blast" };
        EffectTable.Load();
        foreach (string id in ids)
        {
            var rows = EffectTable.Get(id);
            Assert.IsNotNull(rows, id);
            Assert.AreEqual(EffectKind.Rule, rows[0].kind, id);
        }
        Assert.AreEqual(2, EffectTable.Get("EF_DEV_Blast").Count);
    }
}
