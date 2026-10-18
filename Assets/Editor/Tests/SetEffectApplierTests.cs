using NUnit.Framework;

/// <summary>세트 효과 파서·적용/해제 대칭 (ItemSpec 3절)</summary>
public class SetEffectApplierTests
{
    int def, crit, atk, poison;

    [SetUp]
    public void Save()
    {
        def = DatabaseManager.playerDef; crit = DatabaseManager.playerCritRate;
        atk = DatabaseManager.attackSpeedBuff; poison = DatabaseManager.addPoisonDmg;
    }

    [TearDown]
    public void Restore()
    {
        DatabaseManager.playerDef = def; DatabaseManager.playerCritRate = crit;
        DatabaseManager.attackSpeedBuff = atk; DatabaseManager.addPoisonDmg = poison;
        DatabaseManager.skillBulletCount.Remove("Scattering Leaves");
    }

    [Test]
    public void Parse_SkillNameUnderscoreToSpace()
    {
        var list = SetEffectApplier.Parse("(+)1 skillHitCount Rising_Roots/(+)2 bulletCount Scattering_Leaves");
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual("Rising Roots", list[0].skill);
        Assert.AreEqual(2, list[1].value);
    }

    [Test]
    public void Parse_MinusSignAndGarbage()
    {
        var list = SetEffectApplier.Parse("(-)5 Def/garbage/(+)x Crit");
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(-5, list[0].value);
    }

    [Test]
    public void ApplyThenRemove_ReturnsToBaseline()
    {
        const string fx = "(+)10 Def/(+)5 Crit/(+)15 AtkSpeed/(+)3 Poison/(+)2 bulletCount Scattering_Leaves";
        SetEffectApplier.Apply(fx, true);
        Assert.AreEqual(def + 10, DatabaseManager.playerDef);
        Assert.AreEqual(crit + 5, DatabaseManager.playerCritRate);
        Assert.AreEqual(2, DatabaseManager.skillBulletCount["Scattering Leaves"]);
        SetEffectApplier.Apply(fx, false);
        Assert.AreEqual(def, DatabaseManager.playerDef);
        Assert.AreEqual(crit, DatabaseManager.playerCritRate);
        Assert.AreEqual(atk, DatabaseManager.attackSpeedBuff);
        Assert.AreEqual(poison, DatabaseManager.addPoisonDmg);
        Assert.IsFalse(DatabaseManager.skillBulletCount.ContainsKey("Scattering Leaves"));
    }

    [Test]
    public void AllSetRowsInCsv_Parse()
    {
        var csv = UnityEngine.Resources.Load<UnityEngine.TextAsset>("SetItem");
        var lines = csv.text.Split('\n');
        for (int i = 2; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;
            var cols = line.Split(',');
            Assert.Greater(SetEffectApplier.Parse(cols[1]).Count, 0, "파싱 실패: " + line);
        }
    }
}
