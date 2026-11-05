using NUnit.Framework;

public class EnemyAttackScaleTests
{
    [Test] public void Normal_Unchanged() { Assert.AreEqual(10, EnemyAttackScale.Apply(10, false, 1f)); }
    [Test] public void Elite_x1_5() { Assert.AreEqual(15, EnemyAttackScale.Apply(10, true, 1f)); }
    [Test] public void Phase_Applies_After_Elite() { Assert.AreEqual(45, EnemyAttackScale.Apply(20, true, 1.5f)); }
    [Test] public void BossPhase_NoElite() { Assert.AreEqual(13, EnemyAttackScale.Apply(10, false, 1.3f)); }
}
