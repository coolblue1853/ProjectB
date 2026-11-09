using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>장비 효과 프레임워크 P0·P1 (EquipEffectDesign §2·§5): 파서·합산·상한·충돌·설명·수치 불변</summary>
public class EquipEffectTests
{
    static EffectEntry Mod(string id, string sel, EffectOp op, float v, string group = "", string cond = "")
    {
        return new EffectEntry { id = id, entry = 1, kind = EffectKind.Mod, selector = sel, op = op, value = v, group = group, cond = cond };
    }

    static EffectEntry RuleE(string id, string sel, EffectRule r, string group = "")
    {
        return new EffectEntry { id = id, entry = 1, kind = EffectKind.Rule, selector = sel, rule = r, group = group, p1 = "Fan", p2 = "60" };
    }

    static ActiveEffect A(EffectEntry e, int grade = 0, int order = 0)
    {
        return new ActiveEffect { entry = e, gradeRank = grade, order = order };
    }

    static SkillMods R(List<ActiveEffect> l, string skill = "Haste", int cd = 0, int hit = 0, int bullet = 0)
    {
        return EffectResolver.Resolve(l, skill, null, cd, hit, bullet, null, null);
    }

    [TearDown]
    public void Cleanup()
    {
        SkillEffectService.ClearAll();
        EffectTable.ResetCache();
        SkillTagTable.ResetCache();
    }

    // ---------- 파서 ----------

    [Test]
    public void Parser_ParsesModAndRule_SkipsHeaderAndBadRows()
    {
        string csv = "id,entry,kind,selector,op,value,cond,group,rule,p1,p2,p3,nameKey,descKey,minGrade\r\n"
            + "EF_A,1,Mod,Scattering_Leaves,ProjectileAdd,2,,,,,,,,,R\r\n"
            + "EF_A,2,Rule,Scattering_Leaves,,,,,FireMode,Fan,60,,,,U\r\n"
            + "EF_B,x,Mod,all,CooldownPct,10,,,,,,,,,\r\n"      // entry 숫자 아님
            + "EF_C,1,Mod,all,Bogus,10,,,,,,,,,\r\n"            // 모르는 op
            + "EF_D,1,Mod,all,CooldownPct,abc,,,,,,,,,\r\n"     // value 오류
            + "EF_E,1,Rule,all,,,,,Nope,,,,,,\r\n"              // 모르는 rule
            + ",1,Mod,all,CooldownPct,5,,,,,,,,,\r\n"           // id 없음
            + "EF_F,1,Mod,,CooldownPct,5,,,,,,,,,\r\n"          // selector 없음
            + "# comment\r\n";
        List<EffectEntry> l = EffectTable.ParseEntries(csv);
        Assert.AreEqual(2, l.Count);
        Assert.AreEqual(EffectOp.ProjectileAdd, l[0].op);
        Assert.AreEqual(2f, l[0].value);
        Assert.AreEqual("R", l[0].minGrade);
        Assert.AreEqual(EffectRule.FireMode, l[1].rule);
        Assert.AreEqual("60", l[1].p2);
    }

    [Test]
    public void Parser_EmptyAndHeaderOnly()
    {
        Assert.AreEqual(0, EffectTable.ParseEntries(null).Count);
        Assert.AreEqual(0, EffectTable.ParseEntries("\uFEFFid,entry,kind\n").Count);
    }

    [Test]
    public void TagParser_ParsesTagsAndNormalizesName()
    {
        var d = SkillTagTable.Parse("skillName,tags\nScattering_Leaves,Projectile;Magic\nbad\n");
        Assert.IsTrue(d["Scattering Leaves"].Contains("Projectile"));
        Assert.IsTrue(d["Scattering Leaves"].Contains("Magic"));
        Assert.AreEqual(1, d.Count);
    }

    // ---------- 선택자 ----------

    [Test]
    public void Selector_NameTagKindAllOr()
    {
        System.Func<string, string, bool> tag = (s, t) => s == "Wisp Bolt" && t == "Projectile";
        Assert.IsTrue(EffectResolver.Matches("Scattering_Leaves", "Scattering Leaves", null, tag));
        Assert.IsFalse(EffectResolver.Matches("Scattering_Leaves", "Haste", null, tag));
        Assert.IsTrue(EffectResolver.Matches("tag:Projectile", "Wisp Bolt", null, tag));
        Assert.IsFalse(EffectResolver.Matches("tag:Projectile", "Haste", null, tag));
        Assert.IsTrue(EffectResolver.Matches("kind:Staff", "Haste", "Staff", tag));
        Assert.IsFalse(EffectResolver.Matches("kind:Staff", "Haste", null, tag));
        Assert.IsTrue(EffectResolver.Matches("all", "Haste", null, null));
        Assert.IsTrue(EffectResolver.Matches("Haste|tag:Projectile", "Haste", null, tag));
    }

    // ---------- 합산·상한 ----------

    [Test]
    public void Caps_CooldownSixty_HitFour()
    {
        var l = new List<ActiveEffect> { A(Mod("a", "Haste", EffectOp.CooldownPct, 50)), A(Mod("b", "Haste", EffectOp.CooldownPct, 40)), A(Mod("c", "Haste", EffectOp.HitCountAdd, 9)) };
        SkillMods m = R(l);
        Assert.AreEqual(60f, m.cooldownPct);
        Assert.AreEqual(4, m.hitCountAdd);
        Assert.IsTrue(m.capped);
    }

    [Test]
    public void Legacy_IsAddedToBucketThenCapped()
    {
        SkillMods m = R(new List<ActiveEffect> { A(Mod("a", "Haste", EffectOp.CooldownPct, 20)) }, "Haste", 50);
        Assert.AreEqual(60f, m.cooldownPct);
        m = R(new List<ActiveEffect> { A(Mod("a", "Haste", EffectOp.CooldownPct, 5)) }, "Haste", 20);
        Assert.AreEqual(25f, m.cooldownPct);
        Assert.IsFalse(m.capped);
    }

    [Test]
    public void Group_TakesMaxNotSum_NoGroupSums()
    {
        var l = new List<ActiveEffect> { A(Mod("a", "Haste", EffectOp.CooldownPct, 10, "g")), A(Mod("b", "Haste", EffectOp.CooldownPct, 25, "g")), A(Mod("c", "Haste", EffectOp.CooldownPct, 5)) };
        Assert.AreEqual(30f, R(l).cooldownPct);
    }

    [Test]
    public void OtherSkillAndCondition_NotApplied()
    {
        var l = new List<ActiveEffect> { A(Mod("a", "Other", EffectOp.CooldownPct, 30)), A(Mod("b", "Haste", EffectOp.CooldownPct, 10, "", "Night")) };
        Assert.AreEqual(0f, R(l).cooldownPct);
        SkillMods m = EffectResolver.Resolve(l, "Haste", null, 0, 0, 0, null, c => c == "Night");
        Assert.AreEqual(10f, m.cooldownPct);
    }

    [Test]
    public void NoActive_FastPath_LegacyOnly()
    {
        SkillMods m = EffectResolver.Resolve(null, "Haste", null, 50, 2, 3, null, null);
        Assert.AreEqual(50f, m.cooldownPct);
        Assert.AreEqual(2, m.hitCountAdd);
        Assert.AreEqual(3, m.projectileAdd);
        Assert.IsNull(m.rules);
    }

    [Test]
    public void ProjectileCap_MinOfSixAndBase()
    {
        Assert.AreEqual(2, EffectCaps.ClampProjectileAdd(2, 4));
        Assert.AreEqual(4, EffectCaps.ClampProjectileAdd(9, 4));
        Assert.AreEqual(6, EffectCaps.ClampProjectileAdd(9, 30));
        Assert.AreEqual(6, EffectCaps.ClampProjectileAdd(9, 0));
        Assert.AreEqual(0, EffectCaps.ClampProjectileAdd(-3, 4));
    }

    [Test]
    public void CooldownMultiplier_EquipTimesTree()
    {
        Assert.AreEqual(0.34f, CooldownMath.Multiplier(60f, 15f), 0.0001f);
        Assert.AreEqual(0.34f, CooldownMath.Multiplier(99f, 40f), 0.0001f); // 각 버킷 상한 선적용
    }

    // ---------- 충돌 규칙 ----------

    [Test]
    public void Rules_FireModeSlot_HigherGradeWins_LoserInactive()
    {
        var l = new List<ActiveEffect> { A(RuleE("lo", "Haste", EffectRule.FireMode), 1, 5), A(RuleE("hi", "Haste", EffectRule.FireMode), 3, 1) };
        SkillMods m = R(l);
        Assert.AreEqual(1, m.rules.Length);
        Assert.AreEqual("hi", m.rules[0].id);
        Assert.AreEqual("lo", m.inactiveRules[0].id);
    }

    [Test]
    public void Rules_SameGrade_LaterOrderWins()
    {
        var l = new List<ActiveEffect> { A(RuleE("early", "Haste", EffectRule.Homing), 2, 1), A(RuleE("late", "Haste", EffectRule.Retarget), 2, 9) };
        Assert.AreEqual("late", R(l).rules[0].id); // Retarget/Homing 합쳐 1개
    }

    [Test]
    public void Rules_ProcClassMaxTwo()
    {
        var l = new List<ActiveEffect>
        {
            A(RuleE("1", "Haste", EffectRule.Chain), 2, 1), A(RuleE("2", "Haste", EffectRule.Proc), 2, 2),
            A(RuleE("3", "Haste", EffectRule.OnHitSpawn), 2, 3), A(RuleE("4", "Haste", EffectRule.CooldownRefund), 2, 4),
        };
        SkillMods m = R(l);
        Assert.AreEqual(3, m.rules.Length);        // 2개 + 제한 없는 CooldownRefund
        Assert.AreEqual(1, m.inactiveRules.Length);
        Assert.AreEqual("1", m.inactiveRules[0].id); // 가장 먼저 장착된 것이 탈락
    }

    [Test]
    public void Rules_ReplaceDropsFireMode()
    {
        var l = new List<ActiveEffect> { A(RuleE("fm", "Haste", EffectRule.FireMode), 3, 9), A(RuleE("rp", "Haste", EffectRule.Replace), 1, 1) };
        SkillMods m = R(l);
        Assert.AreEqual("rp", m.rules[0].id);
        Assert.AreEqual("fm", m.inactiveRules[0].id);
    }

    [Test]
    public void Rules_SameGroupKeepsBestOnly()
    {
        var l = new List<ActiveEffect> { A(RuleE("a", "Haste", EffectRule.Pierce, "g"), 1, 1), A(RuleE("b", "Haste", EffectRule.Return, "g"), 2, 2) };
        SkillMods m = R(l);
        Assert.AreEqual(1, m.rules.Length);
        Assert.AreEqual("b", m.rules[0].id);
    }

    // ---------- 서비스 ----------

    [Test]
    public void Service_AttachDetachIdempotent()
    {
        EffectTable.Build(new List<EffectEntry> { Mod("EF_T", "Svc Skill", EffectOp.HitCountAdd, 2) });
        SkillEffectService.Attach("Item:x", new[] { "EF_T" });
        SkillEffectService.Attach("Item:x", new[] { "EF_T" }); // 같은 키 재호출 = 교체
        Assert.AreEqual(1, SkillEffectService.ActiveCount);
        Assert.AreEqual(2, SkillEffectService.Query("Svc_Skill", WeaponKind.None).hitCountAdd);
        SkillEffectService.Detach("Item:x");
        SkillEffectService.Detach("Item:x");
        Assert.AreEqual(0, SkillEffectService.ActiveCount);
        Assert.AreEqual(0, SkillEffectService.Query("Svc Skill", WeaponKind.None).hitCountAdd);
        SkillEffectService.Attach("Item:y", new[] { "EF_missing" }); // 모르는 id 는 경고 후 무시
        Assert.AreEqual(0, SkillEffectService.ActiveCount);
    }

    // ---------- 설명 ----------

    [Test]
    public void Describer_Snapshots()
    {
        Assert.AreEqual("Scattering Leaves projectiles +2", EffectDescriber.Describe(Mod("a", "Scattering_Leaves", EffectOp.ProjectileAdd, 2)));
        Assert.AreEqual("Haste cooldown -20%", EffectDescriber.Describe(Mod("a", "Haste", EffectOp.CooldownPct, 20)));
        Assert.AreEqual("Projectile skills damage +12.5%", EffectDescriber.Describe(Mod("a", "tag:Projectile", EffectOp.DamagePct, 12.5f)));
        Assert.AreEqual("All skills cooldown -5% (when Night)", EffectDescriber.Describe(Mod("a", "all", EffectOp.CooldownPct, 5, "", "Night")));
        Assert.AreEqual("Scattering Leaves fire as Fan (60 deg)", EffectDescriber.Describe(RuleE("r", "Scattering_Leaves", EffectRule.FireMode)));
        Assert.AreEqual("MANUAL", EffectDescriber.Describe(new EffectEntry { kind = EffectKind.Mod, selector = "all", op = EffectOp.CooldownPct, value = 1, descKey = "k" }, null, k => "MANUAL"));
        Assert.AreEqual("[Haste cooldown -20%]", "[" + EffectDescriber.Describe(Mod("a", "Haste", EffectOp.CooldownPct, 20), s => s) + "]");
    }

    // ---------- P1 수치 불변 ----------

    static float OldCooldown(float baseTime, string skill)
    {
        float p = 0;
        if (DatabaseManager.skillCoolDown.ContainsKey(skill))
        {
            p = DatabaseManager.skillCoolDown[skill];
        }
        return p == 0 ? baseTime : baseTime * (1 - p / 100f);
    }

    static int OldHit(int baseCount, string skill)
    {
        return DatabaseManager.skillHitCount.ContainsKey(skill) ? baseCount + DatabaseManager.skillHitCount[skill] : baseCount;
    }

    static int OldBullet(int baseCount, string skill)
    {
        return DatabaseManager.skillBulletCount.ContainsKey(skill) ? baseCount + DatabaseManager.skillBulletCount[skill] : baseCount;
    }

    static float NewCooldown(float baseTime, string skill)
    {
        return CooldownMath.Apply(baseTime, SkillEffectService.Query(skill, WeaponKind.None).cooldownPct);
    }

    static int NewHit(int baseCount, string skill)
    {
        return baseCount + SkillEffectService.Query(skill, WeaponKind.None).hitCountAdd;
    }

    static int NewBullet(int baseCount, string skill)
    {
        return baseCount + EffectCaps.ClampProjectileAdd(SkillEffectService.Query(skill, WeaponKind.None).projectileAdd, baseCount);
    }

    /// <summary>SetItem.csv 전 행(모든 단계)과 Equipment 프리팹 쿨감을 한 스킬에 전부 합쳐도 상한 이내인지 + 구·신 결과 동일</summary>
    [Test]
    public void P1_AllSetsAndEquipment_NewEqualsOld()
    {
        var cd = new Dictionary<string, int>(DatabaseManager.skillCoolDown);
        var hit = new Dictionary<string, int>(DatabaseManager.skillHitCount);
        var bl = new Dictionary<string, int>(DatabaseManager.skillBulletCount);
        try
        {
            DatabaseManager.skillCoolDown.Clear();
            DatabaseManager.skillHitCount.Clear();
            DatabaseManager.skillBulletCount.Clear();
            TextAsset sets = Resources.Load<TextAsset>("SetItem");
            Assert.IsNotNull(sets);
            foreach (string line in sets.text.Replace("\r", "").Split('\n'))
            {
                int c = line.IndexOf(',');
                if (c > 0 && line.IndexOf('(') > c)
                {
                    SetEffectApplier.Apply(line.Substring(c + 1), true);
                }
            }
            var prefabMax = new Dictionary<string, int>();
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" }))
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                var eq = go == null ? null : go.GetComponent<Equipment>();
                if (eq == null || eq.coolDownSkill == null)
                {
                    continue;
                }
                foreach (Equipment.SkillCoolTime s in eq.coolDownSkill)
                {
                    prefabMax.TryGetValue(s.skillName, out int pm);
                    prefabMax[s.skillName] = Mathf.Max(pm, s.coolDownCount); // 같은 부위 프리팹은 동시 장착 불가 -> 최댓값 1개
                }
            }
            foreach (KeyValuePair<string, int> kv in prefabMax)
            {
                DatabaseManager.skillCoolDown.TryGetValue(kv.Key, out int pv);
                DatabaseManager.skillCoolDown[kv.Key] = pv + kv.Value;
            }
            int maxCd = 0;
            var names = new HashSet<string>(DatabaseManager.skillCoolDown.Keys);
            names.UnionWith(DatabaseManager.skillHitCount.Keys);
            names.UnionWith(DatabaseManager.skillBulletCount.Keys);
            names.Add("Unlisted Skill");
            foreach (string n in names)
            {
                if (DatabaseManager.skillCoolDown.TryGetValue(n, out int v) && v > maxCd)
                {
                    maxCd = v;
                }
                foreach (float bt in new[] { 0.3f, 1f, 3f, 8f, 20f })
                {
                    Assert.AreEqual(OldCooldown(bt, n), NewCooldown(bt, n), 0f, "쿨 " + n);
                }
                // 탄 기본 수는 실제 프리팹 값(Scattering Leaves 4, 기본 30)만. 기본 < 레거시 가산인 조합은 현행 데이터에 없다(상한 min(+6, 기본))
                foreach (int b in new[] { 4, 30 })
                {
                    Assert.AreEqual(OldHit(b, n), NewHit(b, n), "타격 " + n);
                    Assert.AreEqual(OldBullet(b, n), NewBullet(b, n), "탄 " + n);
                }
            }
            Debug.Log("[EquipEffectTests] 스킬 " + names.Count + "종 신구 동일, 최대 쿨감 합 " + maxCd + "% (상한 60)");
            Assert.LessOrEqual(maxCd, 60, "현행 쿨감 합이 상한 60 초과");
        }
        finally
        {
            Restore(DatabaseManager.skillCoolDown, cd);
            Restore(DatabaseManager.skillHitCount, hit);
            Restore(DatabaseManager.skillBulletCount, bl);
        }
    }

    [Test]
    public void P1_SweepLegacyValues_NewEqualsOld()
    {
        var cd = new Dictionary<string, int>(DatabaseManager.skillCoolDown);
        try
        {
            for (int p = 0; p <= 60; p++)
            {
                DatabaseManager.skillCoolDown["Sweep"] = p;
                if (p == 0)
                {
                    DatabaseManager.skillCoolDown.Remove("Sweep");
                }
                foreach (float bt in new[] { 0.3f, 1f, 2.5f, 8f, 30f })
                {
                    Assert.AreEqual(OldCooldown(bt, "Sweep"), NewCooldown(bt, "Sweep"), 0f, "p=" + p + " base=" + bt);
                }
            }
        }
        finally
        {
            Restore(DatabaseManager.skillCoolDown, cd);
        }
    }

    static void Restore(Dictionary<string, int> target, Dictionary<string, int> saved)
    {
        target.Clear();
        foreach (KeyValuePair<string, int> kv in saved)
        {
            target[kv.Key] = kv.Value;
        }
    }
}
