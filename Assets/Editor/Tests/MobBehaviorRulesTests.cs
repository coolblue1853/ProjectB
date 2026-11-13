using NUnit.Framework;
using UnityEngine;

public class MobBehaviorRulesTests
{
    [Test]
    public void KillPlane_UsesLowerOfMapBottomAndSpawnMinusMargin()
    {
        Assert.AreEqual(-10f - FallRecovery.KillMargin, MobBehaviorRules.KillPlaneY(-10f, 5f), 1e-4f);
        // 맵 하한 값이 잘못돼(기본 -10) 스폰 발판이 그보다 아래면 발판 기준으로 내려 즉사하지 않게
        Assert.AreEqual(-180f - FallRecovery.KillMargin, MobBehaviorRules.KillPlaneY(-10f, -180f), 1e-4f);
    }

    [Test]
    public void KillPlaneAction_BossReturnsNormalRemoved()
    {
        Assert.AreEqual(FallAction.Return, MobBehaviorRules.KillPlaneAction(true), "슬롯 보스는 복귀");
        Assert.AreEqual(FallAction.Remove, MobBehaviorRules.KillPlaneAction(false), "일반 몹은 제거");
    }

    [Test]
    public void ShouldRehome_OnlyFallMobLandedBelowHome()
    {
        Assert.IsTrue(MobBehaviorRules.ShouldRehome(MobEdge.Fall, 0f, -3f, 0f), "떨어져 아래 발판에 선 몹");
        Assert.IsFalse(MobBehaviorRules.ShouldRehome(MobEdge.Fall, 0f, -3f, -5f), "낙하 중");
        Assert.IsFalse(MobBehaviorRules.ShouldRehome(MobEdge.Fall, 0f, -0.5f, 0f), "같은 발판(작은 단차)");
        Assert.IsFalse(MobBehaviorRules.ShouldRehome(MobEdge.Turn, 0f, -3f, 0f), "turn 몹은 기준점 유지");
        Assert.IsFalse(MobBehaviorRules.ShouldRehome(MobEdge.Default, 0f, -3f, 0f));
    }

    [Test]
    public void Parse_AllBlank_IsNull()
    {
        Assert.IsNull(MobBehaviorRules.Parse("", "", "", "", "", "", "", ""));
    }

    [Test]
    public void Parse_ReadsEveryColumn()
    {
        MobBehavior b = MobBehaviorRules.Parse("stay", "fall", "no", "16", "2.5", "24", "reactive", "ranged");
        Assert.AreEqual(MobMove.Stay, b.Move);
        Assert.AreEqual(MobEdge.Fall, b.Edge);
        Assert.IsFalse(b.ChasesPlayer);
        Assert.AreEqual(16f, b.ChaseRange);
        Assert.AreEqual(2.5f, b.VisionY);
        Assert.AreEqual(24f, b.GiveUp);
        Assert.AreEqual(MobAggro.Reactive, b.Aggro);
        Assert.AreEqual(MobAttackKind.Ranged, b.Attack);
    }

    [Test]
    public void MayAcquire_FirstReactivePassive()
    {
        var first = MobBehaviorRules.Parse("", "", "", "", "", "", "first", "");
        var reactive = MobBehaviorRules.Parse("", "", "", "", "", "", "reactive", "");
        var passive = MobBehaviorRules.Parse("", "", "", "", "", "", "passive", "");
        Assert.IsTrue(MobBehaviorRules.MayAcquire(first, false, false));
        Assert.IsFalse(MobBehaviorRules.MayAcquire(reactive, false, false));
        Assert.IsTrue(MobBehaviorRules.MayAcquire(reactive, false, true));
        Assert.IsTrue(MobBehaviorRules.MayAcquire(reactive, true, false), "추격 중이면 유지");
        Assert.IsFalse(MobBehaviorRules.MayAcquire(passive, true, true));
    }

    [Test]
    public void DetectBox_GiveUpWidensOnlyWhenLatchedOrProvoked()
    {
        var b = MobBehaviorRules.Parse("", "", "", "10", "3", "24", "", "");
        Vector2 prefab = new Vector2(20f, 10f);
        Assert.AreEqual(new Vector2(10f, 6f), MobBehaviorRules.DetectBox(b, prefab, false, false));
        Assert.AreEqual(new Vector2(24f, 6f), MobBehaviorRules.DetectBox(b, prefab, true, false));
        Assert.AreEqual(new Vector2(24f, 6f), MobBehaviorRules.DetectBox(b, prefab, false, true));
    }

    [Test]
    public void DetectBox_BlankKeepsPrefab()
    {
        var b = MobBehaviorRules.Parse("stay", "", "", "", "", "", "", "");
        Assert.AreEqual(new Vector2(20f, 10f), MobBehaviorRules.DetectBox(b, new Vector2(20f, 10f), true, true));
    }

    [Test]
    public void DetectBox_Threshold()
    {
        Assert.IsTrue(MobBehaviorRules.IsDetectBox(new Vector2(20f, 10f)));
        Assert.IsFalse(MobBehaviorRules.IsDetectBox(new Vector2(15f, 7f)));
        Assert.IsFalse(MobBehaviorRules.IsDetectBox(new Vector2(2f, 2f)));
    }

    [Test]
    public void VerticalGap_DefaultAndOverride()
    {
        Assert.AreEqual(RangedAttackRules.MaxVerticalGap, MobBehaviorRules.VerticalGap(null));
        var b = MobBehaviorRules.Parse("", "", "", "", "3", "", "", "");
        Assert.AreEqual(3f, MobBehaviorRules.VerticalGap(b));
        Assert.IsTrue(MobBehaviorRules.LimitsVertical(null, true));
        Assert.IsFalse(MobBehaviorRules.LimitsVertical(null, false));
        var r = MobBehaviorRules.Parse("", "", "", "", "", "", "", "ranged");
        Assert.IsTrue(MobBehaviorRules.LimitsVertical(r, false));
    }

    [Test]
    public void FitHitbox_GrowsSmallKeepsFeetAndCenter()
    {
        // 월드 0.8x0.6(스케일 2): 로컬 0.4x0.3, 오프셋 y 0.15(발이 y=0)
        bool changed = MobBehaviorRules.FitHitbox(new Vector2(0.4f, 0.3f), new Vector2(0f, 0.15f), new Vector2(2f, 2f), 1.2f, 1.4f, out Vector2 size, out Vector2 off);
        Assert.IsTrue(changed);
        Assert.AreEqual(0.6f, size.x, 1e-4f);
        Assert.AreEqual(0.7f, size.y, 1e-4f);
        Assert.AreEqual(0f, off.x, 1e-4f);
        Assert.AreEqual(0.35f, off.y, 1e-4f, "아래쪽(발) = offset.y - size.y/2 = 0 유지");
    }

    [Test]
    public void FitHitbox_LargeUnchanged()
    {
        bool changed = MobBehaviorRules.FitHitbox(new Vector2(3f, 3f), Vector2.zero, Vector2.one, 1.2f, 1.4f, out Vector2 size, out Vector2 off);
        Assert.IsFalse(changed);
        Assert.AreEqual(new Vector2(3f, 3f), size);
    }

    [Test]
    public void SheetRows_AllParse_AndBossesAreDefault()
    {
        int rows = 0;
        foreach (MonsterTable.Row r in MonsterTable.All())
        {
            rows++;
            if (r.isBoss == true)
            {
                Assert.IsNull(r.behavior, r.id + " 보스는 행동 속성 없음(기존 BT)");
            }
        }
        Assert.Greater(rows, 40);
        Assert.AreEqual(MobMove.Stay, MonsterTable.Get("Scarecrow_BD").behavior.Move);
        Assert.AreEqual(MobAggro.Reactive, MonsterTable.Get("SporeShroom_BD").behavior.Aggro);
    }

    [Test]
    public void SheetRows_EdgeFall_OnlyHoppersNeverEliteOrBoss()
    {
        string[] hoppers = { "Rabbit_BD", "StoneRabbit_BD", "SwampFrog_BD", "SporeBackToad_BD" };
        string[] elites = { "RatChief_BD", "AlphaWolf_BD", "OldBear_BD", "OldCroc_BD", "BanditLieutenant_BD", "ElderGolem_BD" };
        foreach (string id in hoppers)
        {
            Assert.AreEqual(MobEdge.Fall, MonsterTable.Get(id).behavior.Edge, id);
        }
        foreach (string id in elites)
        {
            MobBehavior b = MonsterTable.Get(id).behavior;
            Assert.AreEqual(MobEdge.Turn, b.Edge, id + " 정예는 낭떠러지에서 돌아선다");
            Assert.AreEqual(MobAggro.First, b.Aggro, id);
            Assert.IsTrue(b.ChasesPlayer, id);
            Assert.AreEqual(30f, b.GiveUp, id + " 정예는 끈질기게 추격");
        }
        foreach (MonsterTable.Row r in MonsterTable.All())
        {
            if (r.behavior != null && r.behavior.Edge == MobEdge.Fall)
            {
                Assert.Contains(r.id, hoppers, r.id + " 는 edge=fall 배정 목록 밖");
            }
        }
    }
}
