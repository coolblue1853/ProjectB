using System.Text;
using NUnit.Framework;

/// <summary>밤 몹 배율·정예 장비 확률·기록기 R2.1 순수 규칙 (PlayBalance_1115 총괄 결정)</summary>
public class NightMobRulesTests
{
    [Test]
    public void Parse_ReadsFirstDataRow()
    {
        NightMobRules.Mul m = NightMobRules.Parse("hpMul,atkMul,expMul\r\n1.5,1.2,1.1\r\n");
        Assert.AreEqual(1.5f, m.Hp, 1e-4f);
        Assert.AreEqual(1.2f, m.Atk, 1e-4f);
        Assert.AreEqual(1.1f, m.Exp, 1e-4f);
    }

    [TestCase("")]
    [TestCase("hpMul,atkMul,expMul\n")]
    [TestCase("hpMul,atkMul,expMul\n1,2\n")]
    public void Parse_MissingOrShortRow_UsesDefault(string text)
    {
        NightMobRules.Mul m = NightMobRules.Parse(text);
        Assert.AreEqual(NightMobRules.Default.Hp, m.Hp, 1e-4f);
        Assert.AreEqual(NightMobRules.Default.Atk, m.Atk, 1e-4f);
        Assert.AreEqual(NightMobRules.Default.Exp, m.Exp, 1e-4f);
    }

    [Test]
    public void Parse_BadCell_FallsBackPerCell()
    {
        NightMobRules.Mul m = NightMobRules.Parse("h,a,e\nx,0,1.25\n");
        Assert.AreEqual(NightMobRules.Default.Hp, m.Hp, 1e-4f);
        Assert.AreEqual(NightMobRules.Default.Atk, m.Atk, 1e-4f);
        Assert.AreEqual(1.25f, m.Exp, 1e-4f);
    }

    [Test]
    public void Scale_RoundsAndKeepsZero()
    {
        Assert.AreEqual(36, NightMobRules.Scale(26, 1.4f));   // 36.4
        Assert.AreEqual(7, NightMobRules.Scale(6, 1.15f));    // 6.9
        Assert.AreEqual(0, NightMobRules.Scale(0, 1.4f));
        Assert.AreEqual(1, NightMobRules.Scale(1, 0.1f), "양수는 최소 1");
    }

    [Test]
    public void ShippedCsv_IsDecisionValues()
    {
        var csv = UnityEngine.Resources.Load<UnityEngine.TextAsset>("NightMobMultipliers");
        Assert.NotNull(csv);
        NightMobRules.Mul m = NightMobRules.Parse(csv.text);
        Assert.AreEqual(1.4f, m.Hp, 1e-4f);
        Assert.AreEqual(1.3f, m.Atk, 1e-4f);
        Assert.AreEqual(1.15f, m.Exp, 1e-4f);
    }

    [Test]
    public void For_DayIsIdentity()
    {
        Assert.AreEqual(1f, NightMobRules.For(false).Hp);
        Assert.AreEqual(1f, NightMobRules.For(false).Atk);
        Assert.AreEqual(1f, NightMobRules.For(false).Exp);
    }

    [Test]
    public void NightStacksWithElite_Multiplicatively()
    {
        int night = NightMobRules.Scale(100, 1.4f);
        int elite = UnityEngine.Mathf.RoundToInt(night * EnemyHealth.EliteHpMul);
        Assert.AreEqual(560, elite); // 1.4 x 4
    }

    [TestCase(0f, true)]
    [TestCase(0.39f, true)]
    [TestCase(0.4f, false)]
    [TestCase(0.99f, false)]
    public void EliteEquipDrops_FortyPercent(float roll, bool expected)
    {
        Assert.AreEqual(expected, EnemySpawner.EliteEquipDrops(roll));
    }

    [TestCase("", false, false)]
    [TestCase("ML", false, true)]
    [TestCase("M", false, true)]
    [TestCase("IS", false, false)]
    [TestCase("", true, true)]
    [TestCase(null, false, false)]
    public void StuckExcused_MoveLockOrAttack(string flags, bool attacking, bool expected)
    {
        Assert.AreEqual(expected, PlayRecStuck.IsExcused(flags, attacking));
    }

    [Test]
    public void Json_KoreanStaysUtf8_NoReplacementChar()
    {
        StringBuilder sb = new StringBuilder();
        PlayRecJson.Begin(sb, 1f, "log", "human");
        PlayRecJson.KeyStr(sb, "msg", "정예 승격: 쥐 두목 HP 160");
        PlayRecJson.End(sb);
        byte[] bytes = new UTF8Encoding(false).GetBytes(sb.ToString()); // PlayRecWriter 와 같은 인코딩
        string back = Encoding.UTF8.GetString(bytes);
        StringAssert.Contains("정예 승격: 쥐 두목", back);
        Assert.IsFalse(back.Contains("�"));
    }
}
