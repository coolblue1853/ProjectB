using NUnit.Framework;

/// <summary>WSKILL-V2-2 P7: 정수 반올림 뭉침 해소·극의 타입별 배율·전투 중 판정(수치 고정)</summary>
public class SkillTreeP7Tests
{
    [Test]
    public void ToWholePoints_IsMonotonicAndNotBankers()
    {
        // 0.5 단위 5레벨 = 0.5..2.5 → 1,1,2,2,3 (은행가 반올림이면 0,1,2,2,2)
        int[] expect = { 1, 1, 2, 2, 3 };
        for (int lv = 1; lv <= 5; lv++)
        {
            Assert.AreEqual(expect[lv - 1], (int)SkillTreeRules.ToWholePoints(0.5f * lv), "A1 Lv" + lv);
        }
        // 1.5 단위 = 1.5,3,4.5,6,7.5 → 2,3,5,6,8
        int[] speed = { 2, 3, 5, 6, 8 };
        for (int lv = 1; lv <= 5; lv++)
        {
            Assert.AreEqual(speed[lv - 1], (int)SkillTreeRules.ToWholePoints(1.5f * lv), "M1 Lv" + lv);
        }
        Assert.AreEqual(0f, SkillTreeRules.ToWholePoints(0f));
    }

    [TestCase(WeaponKind.Sword, false, true, 1.15f)]
    [TestCase(WeaponKind.Sword, false, false, 1f)]
    [TestCase(WeaponKind.Sword, true, true, 1f)]
    [TestCase(WeaponKind.Greatsword, false, true, 1.20f)]
    [TestCase(WeaponKind.Axe, false, false, 1f)]
    [TestCase(WeaponKind.Dagger, false, false, 1.06f)]
    [TestCase(WeaponKind.Dagger, true, false, 1.06f)]
    [TestCase(WeaponKind.Bow, false, false, 1.08f)]
    [TestCase(WeaponKind.Crossbow, true, false, 1.08f)]
    [TestCase(WeaponKind.Staff, true, false, 1.10f)]
    [TestCase(WeaponKind.Tome, false, true, 1.10f)]
    [TestCase(WeaponKind.Spear, false, true, 1.15f)]
    public void UltimateMul_ByKind(WeaponKind kind, bool skill, bool final, float expect)
    {
        Assert.AreEqual(expect, SkillTreeRules.UltimateMul(kind, skill, final), 1e-4f);
    }

    [Test]
    public void Combat_WindowExpires()
    {
        CombatState.Mark(100f);
        Assert.IsTrue(CombatState.IsInCombat(100f));
        Assert.IsTrue(CombatState.IsInCombat(100f + CombatState.Window - 0.01f));
        Assert.IsFalse(CombatState.IsInCombat(100f + CombatState.Window));
    }
}
