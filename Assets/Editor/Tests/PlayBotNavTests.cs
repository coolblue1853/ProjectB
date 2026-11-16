using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>자동 플레이 봇 B2 순수 로직: 이동 그래프 굽기·길찾기·맵 경로·진척 감시·사거리·회피·포션·스킬 고르기·맵 순회</summary>
public class PlayBotNavTests
{
    private static BotNavBuild.Solid Box(float x0, float y0, float x1, float y1)
    {
        return BotNavBuild.SolidFromPaths(new[] { new[] { new Vector2(x0, y0), new Vector2(x1, y0), new Vector2(x1, y1), new Vector2(x0, y1) } });
    }

    private static BotNavBuild.Input Ground(float x0 = 0f, float x1 = 20f)
    {
        BotNavBuild.Input input = new BotNavBuild.Input { Map = "T" };
        input.Solids.Add(Box(x0, -2f, x1, 0f));
        return input;
    }

    private static int CountEdges(BotNavGraph g, int from, int to, int kind)
    {
        int n = 0;
        foreach (BotNavGraph.Edge e in g.edges)
        {
            if (e.from == from && e.to == to && e.kind == kind)
            {
                n++;
            }
        }
        return n;
    }

    private static BotNavGraph.Edge Find(BotNavGraph g, int from, int to, int kind)
    {
        foreach (BotNavGraph.Edge e in g.edges)
        {
            if (e.from == from && e.to == to && e.kind == kind)
            {
                return e;
            }
        }
        return null;
    }

    // ───── 굽기 ─────

    [Test]
    public void Build_FlatGround_OneSegment()
    {
        BotNavGraph g = BotNavBuild.Build(Ground());
        Assert.AreEqual(1, g.segs.Length);
        Assert.AreEqual(0f, g.segs[0].X0, 0.01f);
        Assert.AreEqual(20f, g.segs[0].X1, 0.01f);
        Assert.AreEqual(0f, g.segs[0].Y(5f), 0.01f);
        Assert.IsTrue(g.segs[0].openL && g.segs[0].openR);
    }

    [Test]
    public void Build_AdjacentTileRuns_MergeIntoOneSurface()
    {
        BotNavBuild.Input input = new BotNavBuild.Input { Map = "T" };
        input.Solids.Add(Box(0f, -1f, 10f, 0f));
        input.Solids.Add(Box(10f, -1f, 20f, 0f));
        input.Solids.Add(Box(0f, -2f, 20f, -1f)); // 아래 행 윗변은 위 행에 덮여 표면이 아니다
        BotNavGraph g = BotNavBuild.Build(input);
        Assert.AreEqual(1, g.segs.Length);
        Assert.AreEqual(20f, g.segs[0].X1 - g.segs[0].X0, 0.01f);
    }

    [Test]
    public void Build_OneWayPlatform_JumpUpAndDownJump()
    {
        BotNavBuild.Input input = Ground();
        input.Platforms.Add(Rect.MinMaxRect(5f, 1.8f, 12f, 2f));
        BotNavGraph g = BotNavBuild.Build(input);
        Assert.AreEqual(2, g.segs.Length);
        int ground = g.SegAt(1f, 0f), plat = g.SegAt(8f, 2f);
        Assert.IsTrue(g.segs[plat].oneWay);
        BotNavGraph.Edge up = Find(g, ground, plat, BotNavGraph.KindJump);
        Assert.IsNotNull(up);
        Assert.IsTrue(up.sameX);
        Assert.AreEqual(1, CountEdges(g, plat, ground, BotNavGraph.KindDownJump));
        Assert.AreEqual(1, CountEdges(g, plat, ground, BotNavGraph.KindFall) > 0 ? 1 : 0);
    }

    [Test]
    public void Build_Step_SplitsAndJumpsUp()
    {
        BotNavBuild.Input input = Ground(0f, 10f);
        input.Solids.Add(Box(10f, -2f, 20f, 1f)); // 1칸 턱
        BotNavGraph g = BotNavBuild.Build(input);
        int low = g.SegAt(3f, 0f), high = g.SegAt(15f, 1f);
        Assert.GreaterOrEqual(low, 0);
        Assert.GreaterOrEqual(high, 0);
        Assert.IsFalse(g.segs[low].openR, "턱에 막힌 끝");
        Assert.IsNotNull(Find(g, low, high, BotNavGraph.KindJump));
        Assert.IsNotNull(Find(g, high, low, BotNavGraph.KindFall));
    }

    [Test]
    public void Build_WallOnGround_SplitsWithoutFall()
    {
        BotNavBuild.Input input = Ground();
        input.Solids.Add(Box(9f, 0f, 11f, 10f)); // 넘을 수 없는 벽
        BotNavGraph g = BotNavBuild.Build(input);
        Assert.AreEqual(3, g.segs.Length, "왼쪽·오른쪽 바닥 + 벽 윗면");
        int left = g.SegAt(2f, 0f), right = g.SegAt(18f, 0f);
        Assert.IsFalse(g.segs[left].openR);
        Assert.IsFalse(g.segs[right].openL);
        Assert.IsNull(g.FindPath(left, 2f, right, 18f, true));
    }

    [Test]
    public void Build_Rope_ConnectsHighPlatformBothWays()
    {
        BotNavBuild.Input input = Ground();
        input.Platforms.Add(Rect.MinMaxRect(4f, 5.8f, 12f, 6f));
        input.Ropes.Add(Rect.MinMaxRect(7.8f, 0.2f, 8.2f, 6.5f));
        BotNavGraph g = BotNavBuild.Build(input);
        int ground = g.SegAt(1f, 0f), plat = g.SegAt(8f, 6f);
        Assert.IsNull(Find(g, ground, plat, BotNavGraph.KindJump), "6칸은 점프로 못 간다");
        BotNavGraph.Edge up = Find(g, ground, plat, BotNavGraph.KindRope);
        Assert.IsNotNull(up);
        Assert.AreEqual(8f, up.ax0, 0.01f);
        Assert.IsNotNull(Find(g, plat, ground, BotNavGraph.KindRope));
        List<int> path = g.FindPath(ground, 1f, plat, 10f, false);
        Assert.AreEqual(1, path.Count);
    }

    [Test]
    public void Build_HighPlatform_NeedsDoubleJump()
    {
        BotNavBuild.Input input = Ground();
        input.Platforms.Add(Rect.MinMaxRect(5f, 3.3f, 12f, 3.5f));
        BotNavGraph g = BotNavBuild.Build(input);
        int ground = g.SegAt(1f, 0f), plat = g.SegAt(8f, 3.5f);
        Assert.IsNotNull(Find(g, ground, plat, BotNavGraph.KindJumpHigh));
        Assert.IsNull(g.FindPath(ground, 1f, plat, 8f, false));
        Assert.IsNotNull(g.FindPath(ground, 1f, plat, 8f, true));
    }

    [Test]
    public void Build_Gap_JumpAcross()
    {
        BotNavBuild.Input input = new BotNavBuild.Input { Map = "T" };
        input.Solids.Add(Box(0f, -2f, 10f, 0f));
        input.Solids.Add(Box(12f, -2f, 20f, 0f));
        input.Solids.Add(Box(-5f, -20f, 30f, -18f)); // 구덩이 바닥
        BotNavGraph g = BotNavBuild.Build(input);
        int a = g.SegAt(5f, 0f), b = g.SegAt(15f, 0f);
        BotNavGraph.Edge e = Find(g, a, b, BotNavGraph.KindJump);
        Assert.IsNotNull(e);
        Assert.AreEqual(1, e.dir);
        Assert.AreEqual(10f, e.ax1, 0.01f);
    }

    [Test]
    public void Build_SameMapPortal_Edge_CrossMapPortal_Seated()
    {
        BotNavBuild.Input input = Ground();
        input.Solids.Add(Box(9f, 0f, 11f, 10f));
        input.Portals.Add(new BotNavGraph.PortalRec { x = 2f, y = 0.5f, toMap = "", toX = 18f, toY = 0.5f });
        input.Portals.Add(new BotNavGraph.PortalRec { x = 15f, y = 0.5f, toMap = "Other", toX = 0f, toY = 0f });
        BotNavGraph g = BotNavBuild.Build(input);
        int left = g.SegAt(2f, 0f), right = g.SegAt(18f, 0f);
        Assert.IsNotNull(Find(g, left, right, BotNavGraph.KindPortal));
        Assert.AreEqual(right, g.portals[1].seg);
        Assert.AreEqual(1, g.PortalTo("Other", 0f));
        Assert.AreEqual(-1, g.PortalTo("None", 0f));
    }

    [Test]
    public void Inside_EvenOdd_Hole()
    {
        BotNavBuild.Solid s = BotNavBuild.SolidFromPaths(new[]
        {
            new[] { new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10), new Vector2(0, 10) },
            new[] { new Vector2(3, 3), new Vector2(7, 3), new Vector2(7, 7), new Vector2(3, 7) },
        });
        Assert.IsTrue(BotNavBuild.Inside(s, new Vector2(1, 1)));
        Assert.IsFalse(BotNavBuild.Inside(s, new Vector2(5, 5)));
        Assert.IsFalse(BotNavBuild.Inside(s, new Vector2(11, 5)));
    }

    // ───── 그래프 조회·길찾기 ─────

    [Test]
    public void Seg_Y_InterpolatesSlope()
    {
        BotNavGraph.Seg s = new BotNavGraph.Seg { px = new[] { 0f, 4f, 8f }, py = new[] { 0f, 2f, 2f } };
        Assert.AreEqual(1f, s.Y(2f), 1e-4f);
        Assert.AreEqual(2f, s.Y(6f), 1e-4f);
        Assert.AreEqual(0f, s.Y(-3f), 1e-4f);
    }

    [Test]
    public void FindPath_BannedEdge_TakesOtherRoute()
    {
        BotNavBuild.Input input = Ground();
        input.Platforms.Add(Rect.MinMaxRect(4f, 5.8f, 12f, 6f));
        input.Ropes.Add(Rect.MinMaxRect(5.8f, 0.2f, 6.2f, 6.5f));
        input.Ropes.Add(Rect.MinMaxRect(9.8f, 0.2f, 10.2f, 6.5f));
        BotNavGraph g = BotNavBuild.Build(input);
        int ground = g.SegAt(1f, 0f), plat = g.SegAt(8f, 6f);
        List<int> p1 = g.FindPath(ground, 1f, plat, 8f, false);
        Assert.AreEqual(6f, g.edges[p1[0]].ax0, 0.01f, "가까운 로프");
        List<int> p2 = g.FindPath(ground, 1f, plat, 8f, false, new HashSet<int> { p1[0] });
        Assert.AreEqual(10f, g.edges[p2[0]].ax0, 0.01f, "막힌 간선을 빼면 다른 로프");
        Assert.AreEqual(0, g.FindPath(ground, 1f, ground, 3f, false).Count);
    }

    [Test]
    public void SegBelow_PicksHighestUnder()
    {
        BotNavBuild.Input input = Ground();
        input.Platforms.Add(Rect.MinMaxRect(5f, 1.8f, 12f, 2f));
        BotNavGraph g = BotNavBuild.Build(input);
        Assert.AreEqual(g.SegAt(8f, 2f), g.SegBelow(8f, 3f));
        Assert.AreEqual(g.SegAt(8f, 0f), g.SegBelow(8f, 1f));
        Assert.AreEqual(-1, g.SegBelow(30f, 1f));
        Assert.AreEqual(-1, g.SegAt(8f, 1f), "두 표면 사이 공중");
    }

    [Test]
    public void Graph_JsonRoundTrip()
    {
        BotNavBuild.Input input = Ground();
        input.Platforms.Add(Rect.MinMaxRect(5f, 1.8f, 12f, 2f));
        BotNavGraph g = BotNavBuild.Build(input);
        BotNavGraph r = BotNavGraph.FromJson(JsonUtility.ToJson(g));
        Assert.AreEqual(g.segs.Length, r.segs.Length);
        Assert.AreEqual(g.edges.Length, r.edges.Length);
        Assert.AreEqual("T", r.map);
        Assert.IsNull(BotNavGraph.FromJson("{"));
    }

    [Test]
    public void MapRoute_NextHop_AvoidsTown()
    {
        BotNavGraph a = new BotNavGraph { map = "A", portals = new[] { P("Town"), P("B") } };
        BotNavGraph b = new BotNavGraph { map = "B", portals = new[] { P("A"), P("C") } };
        BotNavGraph c = new BotNavGraph { map = "C", portals = new[] { P("B") } };
        BotNavGraph t = new BotNavGraph { map = "Town", portals = new[] { P("C") } };
        BotNavGraph[] all = { a, b, c, t };
        Assert.AreEqual("B", BotMapRoute.NextHop(all, "A", "C"), "마을 지름길을 쓰지 않는다");
        Assert.AreEqual("Town", BotMapRoute.NextHop(all, "A", "Town"));
        Assert.AreEqual("C", BotMapRoute.NextHop(all, "Town", "C"));
        Assert.AreEqual("A", BotMapRoute.NextHop(all, "A", "A"));
        Assert.IsNull(BotMapRoute.NextHop(all, "A", "Z"));
    }

    private static BotNavGraph.PortalRec P(string to)
    {
        return new BotNavGraph.PortalRec { seg = 0, toMap = to };
    }

    // ───── 실패 처리 ─────

    [Test]
    public void NavWatch_RerouteThenStuck()
    {
        BotNavWatch w = new BotNavWatch();
        w.Reset(0f);
        Assert.AreEqual(BotNavWatch.Verdict.None, w.Check(0.1f, 10f));
        Assert.AreEqual(BotNavWatch.Verdict.None, w.Check(2f, 9f), "1 줄어든 건 진척");
        Assert.AreEqual(BotNavWatch.Verdict.None, w.Check(4.9f, 9f));
        Assert.AreEqual(BotNavWatch.Verdict.Reroute, w.Check(5.1f, 9f));
        Assert.AreEqual(BotNavWatch.Verdict.None, w.Check(5.2f, 9f), "다른 길 첫 지표");
        Assert.AreEqual(BotNavWatch.Verdict.Stuck, w.Check(8.4f, 9f));
        w.Progress(9f);
        Assert.AreEqual(BotNavWatch.Verdict.None, w.Check(11f, 9f));
    }

    [Test]
    public void Navigator_Tap_And_Standing()
    {
        Assert.AreEqual(1, GraphNavigator.Tap(0.05f, 0.6f));
        Assert.AreEqual(0, GraphNavigator.Tap(0.3f, 0.6f));
        Assert.AreEqual(1, GraphNavigator.Tap(0.65f, 0.6f));
        PlayObservation o = new PlayObservation { Grounded = false, Vy = 0f };
        Assert.IsTrue(GraphNavigator.Standing(o), "턱에 붙어 접지가 꺼져도 세로 속도 0 이면 서 있다");
        o.Vy = -3f;
        Assert.IsFalse(GraphNavigator.Standing(o));
        o.Grounded = true;
        o.OnRope = true;
        Assert.IsFalse(GraphNavigator.Standing(o), "로프 위는 서 있지 않다");
    }

    // ───── 전투 ─────

    [Test]
    public void Range_BaseByKind_ShrinksOnMisses()
    {
        BotRange r = new BotRange();
        r.SetKind(WeaponKind.Spear);
        Assert.AreEqual(2.3f, r.Range, 1e-4f);
        Assert.Greater(BotRange.Base(WeaponKind.Bow), BotRange.Base(WeaponKind.Sword));
        r.Swing(false);
        r.Swing(true);
        r.Swing(false);
        r.Swing(false);
        Assert.AreEqual(2.3f, r.Range, 1e-4f, "맞으면 빗나감 수를 지운다");
        r.Swing(false);
        Assert.AreEqual(2.1f, r.Range, 1e-4f);
        r.SetKind(WeaponKind.Spear);
        Assert.AreEqual(2.1f, r.Range, 1e-4f, "같은 계열이면 유지");
        r.SetKind(WeaponKind.Dagger);
        Assert.AreEqual(1.2f, r.Range, 1e-4f);
    }

    private static BotProfile AvoidProfile(string only)
    {
        BotProfile p = new BotProfile();
        p.ratios.Add(new BotProfile.Ratio { key = only, n = 10, k = 10, value = 1f });
        p.dists.Add(new BotProfile.Dist { key = "avoid.rt", n = 50, p10 = 0.2f, p25 = 0.2f, p50 = 0.2f, p75 = 0.2f, p90 = 0.2f });
        return p;
    }

    private static void SetFoe(PlayObsEnemies e, int uid, float dx, int state)
    {
        e.FoeCount = 1;
        e.Foes[0] = new PlayObsEnemies.Foe { Uid = uid, Dx = dx, Dy = 0f, State = state, HpPercent = 100 };
    }

    [Test]
    public void Avoid_ReactsAfterDelay_OncePerWindow()
    {
        BotProfile p = AvoidProfile("avoid.r.back");
        BotAvoid a = new BotAvoid();
        PlayObsEnemies e = new PlayObsEnemies();
        System.Random rng = new System.Random(1);
        SetFoe(e, 7, 2f, PlayRecThreat.StTele);
        Assert.AreEqual(BotAvoid.React.None, a.Update(e, 0f, p, rng));
        Assert.AreEqual(BotAvoid.React.Back, a.Update(e, 0.25f, p, rng));
        Assert.AreEqual(1, a.FoeDir);
        Assert.AreEqual(BotAvoid.React.None, a.Update(e, 0.7f, p, rng), "반응은 Active 초만");
        Assert.AreEqual(BotAvoid.React.None, a.Update(e, 1.5f, p, rng), "같은 창은 한 번");
        SetFoe(e, 7, 2f, PlayRecThreat.StChase);
        a.Update(e, 1.6f, p, rng);
        SetFoe(e, 7, -2f, PlayRecThreat.StAttack);
        a.Update(e, 1.7f, p, rng);
        Assert.AreEqual(BotAvoid.React.Back, a.Update(e, 2.0f, p, rng));
        Assert.AreEqual(-1, a.FoeDir);
        Assert.AreEqual(2, a.Windows);
        Assert.AreEqual(2, a.Moves, "반응 하나당 1(틱 수 아님)");
        SetFoe(e, 8, 6f, PlayRecThreat.StAttack);
        Assert.AreEqual(2, a.Windows, "먼 적은 보지 않는다");
    }

    [Test]
    public void Avoid_SampleKind_FollowsRatios()
    {
        BotProfile p = new BotProfile();
        p.ratios.Add(new BotProfile.Ratio { key = "avoid.r.atk", n = 10, k = 7, value = 0.7f });
        p.ratios.Add(new BotProfile.Ratio { key = "avoid.r.jump", n = 10, k = 3, value = 0.3f });
        Assert.AreEqual(BotAvoid.React.Attack, BotAvoid.SampleKind(p, 0.5));
        Assert.AreEqual(BotAvoid.React.Jump, BotAvoid.SampleKind(p, 0.8));
        Assert.AreEqual(BotAvoid.React.Attack, BotAvoid.SampleKind(new BotProfile(), 0.9), "빈 프로필 = 그냥 친다");
    }

    [Test]
    public void Potion_UsesHpSlotBelowThreshold_SkipsDeadSlot()
    {
        BotProfile p = new BotProfile();
        p.dists.Add(new BotProfile.Dist { key = "potion.hpPct", n = 50, p10 = 60f, p25 = 60f, p50 = 60f, p75 = 60f, p90 = 60f });
        BotPotion pot = new BotPotion();
        System.Random rng = new System.Random(3);
        string[] slots = { "Glow Stamina Potion", "Small HP Potion", "Potion", null };
        Assert.AreEqual(0, pot.Decide(70, 100, 0f, slots, p, rng));
        Assert.AreEqual(2, pot.Decide(50, 100, 1f, slots, p, rng));
        Assert.AreEqual(0, pot.Decide(50, 100, 2f, slots, p, rng), "쿨");
        Assert.AreEqual(3, pot.Decide(50, 100, 3f, slots, p, rng), "안 오른 칸은 빼고 다음 칸");
        Assert.AreEqual(2, pot.Uses);
        Assert.IsFalse(BotPotion.IsHpPotion("Glow Stamina Potion"));
        Assert.IsTrue(BotPotion.IsHpPotion("Minor Herbal Potion"));
    }

    [Test]
    public void Skill_PicksReadyByHumanWeights()
    {
        BotProfile p = new BotProfile();
        p.dists.Add(new BotProfile.Dist { key = "press.SkillA", n = 300 });
        p.dists.Add(new BotProfile.Dist { key = "press.SkillS", n = 100 });
        float[] cd = { 0f, 0f, 5f, 0f };
        bool[] on = { true, true, true, false };
        float[] fail = new float[4];
        Assert.AreEqual(1, BotSkill.Pick(cd, on, fail, 0f, 0.5, p));
        Assert.AreEqual(2, BotSkill.Pick(cd, on, fail, 0f, 0.9, p));
        fail[0] = 10f;
        Assert.AreEqual(2, BotSkill.Pick(cd, on, fail, 0f, 0.1, p), "실패한 칸은 잠시 뺀다");
        cd[1] = 1f;
        Assert.AreEqual(0, BotSkill.Pick(cd, on, fail, 0f, 0.1, p));
    }

    [Test]
    public void Circuit_NextIndex_PingPong()
    {
        int dir = 1;
        Assert.AreEqual(1, HuntBrain.NextIndex(3, 0, true, ref dir));
        Assert.AreEqual(2, HuntBrain.NextIndex(3, 1, true, ref dir));
        Assert.AreEqual(1, HuntBrain.NextIndex(3, 2, true, ref dir));
        Assert.AreEqual(-1, dir);
        Assert.AreEqual(0, HuntBrain.NextIndex(3, 1, true, ref dir));
        Assert.AreEqual(1, HuntBrain.NextIndex(3, 0, true, ref dir));
        Assert.AreEqual(0, HuntBrain.NextIndex(3, 2, false, ref dir), "목록 밖이면 첫 맵");
    }

    [Test]
    public void RopeArrived_발판높이_근처면_내려앉기()
    {
        Assert.IsTrue(GraphNavigator.RopeArrived(7.6f, 6.4f, 1));
        Assert.IsTrue(GraphNavigator.RopeArrived(5.9f, 6.4f, 1));
        Assert.IsFalse(GraphNavigator.RopeArrived(5.0f, 6.4f, 1));
        Assert.IsTrue(GraphNavigator.RopeArrived(0.0f, 0.4f, -1));
        Assert.IsFalse(GraphNavigator.RopeArrived(3.0f, 0.4f, -1));
    }
}
