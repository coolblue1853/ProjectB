using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

/// <summary>플레이 기록기 R2 순수 로직: 격자 인코딩·사람 신호·회피 창·tk 직렬화·북마크 패드 조합</summary>
public class PlayRecR2Tests
{
    // ───── 격자 ─────

    [Test]
    public void Grid_OriginFloorsNegative()
    {
        Assert.AreEqual(-1 - PlayRecGrid.Left, PlayRecGrid.OriginX(-0.5f));
        Assert.AreEqual(3 - PlayRecGrid.Left, PlayRecGrid.OriginX(3.99f));
        Assert.AreEqual(-2 - PlayRecGrid.Below, PlayRecGrid.OriginY(-1.01f));
    }

    [Test]
    public void Grid_CellCenter_Row0IsTop()
    {
        Assert.AreEqual(10.5f, PlayRecGrid.CellCenterX(10, 0));
        Assert.AreEqual(5f + PlayRecGrid.H - 1 + 0.5f, PlayRecGrid.CellCenterY(5, 0));
        Assert.AreEqual(5.5f, PlayRecGrid.CellCenterY(5, PlayRecGrid.H - 1));
    }

    [Test]
    public void Grid_HashDependsOnCellsAndOrigin_NeverZero()
    {
        byte[] a = new byte[PlayRecGrid.W * PlayRecGrid.H];
        byte[] b = new byte[a.Length];
        Assert.AreEqual(PlayRecGrid.Hash(a, 1, 2), PlayRecGrid.Hash(b, 1, 2));
        b[7] = PlayRecGrid.Ground;
        Assert.AreNotEqual(PlayRecGrid.Hash(a, 1, 2), PlayRecGrid.Hash(b, 1, 2));
        Assert.AreNotEqual(PlayRecGrid.Hash(a, 1, 2), PlayRecGrid.Hash(a, 2, 1));
        Assert.AreNotEqual(0u, PlayRecGrid.Hash(a, 0, 0));
    }

    [Test]
    public void Grid_CellsAndHexEncoding()
    {
        StringBuilder sb = new StringBuilder();
        PlayRecGrid.AppendCells(sb, new byte[] { 0, 1, 2, 3, 4, 5 });
        Assert.AreEqual("012345", sb.ToString());
        sb.Length = 0;
        PlayRecGrid.AppendHex(sb, 0x00ab12cfu);
        Assert.AreEqual("00ab12cf", sb.ToString());
    }

    // ───── 사람 신호 ─────

    [Test]
    public void Human_Mash_ThreePressesWithinGap_FiresOncePerRun()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        Assert.IsFalse(h.OnPress("Player/Attack", 1.00f, out _));
        Assert.IsFalse(h.OnPress("Player/Attack", 1.10f, out _));
        Assert.IsTrue(h.OnPress("Player/Attack", 1.20f, out PlayRecHuman.Signal s));
        Assert.AreEqual(PlayRecHuman.Kind.Mash, s.Kind);
        Assert.AreEqual("Player/Attack", s.A);
        Assert.IsFalse(h.OnPress("Player/Attack", 1.30f, out _), "같은 연속은 한 번만");
        Assert.IsFalse(h.OnPress("Player/Attack", 2.00f, out _), "간격이 벌어지면 새로 센다");
    }

    [Test]
    public void Human_Mash_SlowPressesDoNotFire()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        h.OnPress("Player/Jump", 0f, out _);
        h.OnPress("Player/Jump", 0.2f, out _);
        Assert.IsFalse(h.OnPress("Player/Jump", 0.4f, out _));
    }

    [Test]
    public void Human_Oscillate_ThreeTurnsInPlace()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        h.OnMoveSign(1, 0.0f, 0f, out _);
        Assert.IsFalse(h.OnMoveSign(-1, 0.3f, 0.3f, out _));
        Assert.IsFalse(h.OnMoveSign(1, 0.6f, 0.1f, out _));
        Assert.IsTrue(h.OnMoveSign(-1, 0.9f, 0.4f, out PlayRecHuman.Signal s));
        Assert.AreEqual(PlayRecHuman.Kind.Oscillate, s.Kind);
    }

    [Test]
    public void Human_Oscillate_WideTravelIsNotInPlace()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        h.OnMoveSign(1, 0.0f, 0f, out _);
        h.OnMoveSign(-1, 0.3f, 0f, out _);
        h.OnMoveSign(1, 0.6f, 3f, out _);
        Assert.IsFalse(h.OnMoveSign(-1, 0.9f, 6f, out _));
    }

    [Test]
    public void Human_QuickClose_UnderOneSecond()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        h.OnWindow("Inventory", true, 5f, out _);
        Assert.IsTrue(h.OnWindow("Inventory", false, 5.6f, out PlayRecHuman.Signal s));
        Assert.AreEqual(PlayRecHuman.Kind.QuickClose, s.Kind);
        h.OnWindow("Shop", true, 10f, out _);
        Assert.IsFalse(h.OnWindow("Shop", false, 12f, out _));
    }

    [Test]
    public void Human_Idle_FiresWhenActivityResumesAfterThreeSeconds()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        Assert.IsFalse(h.Tick(2f, false, out _));
        Assert.IsFalse(h.Tick(3.5f, false, out _));
        Assert.IsTrue(h.IsIdle);
        Assert.IsTrue(h.OnActivity(6f, out PlayRecHuman.Signal s));
        Assert.AreEqual(PlayRecHuman.Kind.Idle, s.Kind);
        Assert.AreEqual(0f, s.T, 1e-4f);
        Assert.AreEqual(6f, s.Dur, 1e-4f);
    }

    [Test]
    public void Human_HeldKeyCountsAsActivity()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        h.Tick(2f, true, out _);
        h.Tick(4f, false, out _);
        Assert.IsFalse(h.IsIdle, "2초에 누르고 있었으니 4초는 아직 2초 무입력");
    }

    [Test]
    public void Human_Pause_ReportsDurationOnResume()
    {
        PlayRecHuman h = new PlayRecHuman();
        h.Reset(0f);
        Assert.IsFalse(h.OnPause(true, 3f, out _));
        Assert.IsTrue(h.OnPause(false, 7f, out PlayRecHuman.Signal s));
        Assert.AreEqual(3f, s.T, 1e-4f);
        Assert.AreEqual(4f, s.Dur, 1e-4f);
    }

    // ───── 회피 창 ─────

    [Test]
    public void Threat_OpensOnTelegraphInRange_RecordsFirstReactionAndHit()
    {
        PlayRecThreat th = new PlayRecThreat();
        th.Enemy(7, "slime", PlayRecThreat.StTele, 3f, 0f, 10f);
        Assert.AreEqual(1, th.OpenCount);
        th.Input(0, -1, 10.2f); // 적이 오른쪽(dx>0) → 왼쪽 이동 = 후퇴
        th.Input(PlayRecThreat.InJump, 0, 10.3f);
        th.Enemy(7, "slime", PlayRecThreat.StAttack, 3f, 0f, 10.5f);
        th.Attempt(7, "hit", 10.5f);
        List<PlayRecThreat.Window> closed = new List<PlayRecThreat.Window>();
        Assert.AreEqual(0, th.Collect(10.6f, closed));
        Assert.AreEqual(1, th.Collect(10.5f + PlayRecThreat.EndGrace + 0.01f, closed));
        PlayRecThreat.Window w = closed[0];
        Assert.AreEqual("hit", w.Result);
        Assert.AreEqual(PlayRecThreat.InBack, w.React);
        Assert.AreEqual(0.2f, w.ReactAt - w.T0, 1e-4f);
        Assert.AreEqual(PlayRecThreat.InBack | PlayRecThreat.InJump, w.Inputs);
        Assert.IsTrue(w.AttackSeen);
        Assert.IsTrue(w.Tele);
    }

    [Test]
    public void Threat_NoAttemptMeansNone_AndOutOfRangeDoesNotOpen()
    {
        PlayRecThreat th = new PlayRecThreat();
        th.Enemy(1, "a", PlayRecThreat.StTele, PlayRecThreat.OpenRangeX + 1f, 0f, 0f);
        th.Enemy(2, "b", PlayRecThreat.StChase, 1f, 0f, 0f);
        Assert.AreEqual(0, th.OpenCount);
        th.Enemy(3, "c", PlayRecThreat.StTele, -2f, 0f, 0f);
        th.Input(0, -1, 0.1f); // 적이 왼쪽 → 왼쪽 이동 = 전진
        List<PlayRecThreat.Window> closed = new List<PlayRecThreat.Window>();
        th.Collect(5f, closed);
        Assert.AreEqual("none", closed[0].Result);
        Assert.AreEqual(PlayRecThreat.InFwd, closed[0].React);
    }

    [Test]
    public void Threat_ResultPriority_HitBeatsIframe()
    {
        PlayRecThreat th = new PlayRecThreat();
        th.Enemy(9, "x", PlayRecThreat.StTele, 1f, 0f, 0f);
        th.Attempt(9, "iframe", 0f);
        th.Attempt(9, "hit", 0f);
        th.Attempt(9, "immune", 0f);
        List<PlayRecThreat.Window> closed = new List<PlayRecThreat.Window>();
        th.Collect(9f, closed);
        Assert.AreEqual("hit", closed[0].Result);
    }

    [Test]
    public void Threat_MaxWindowClosesEvenIfStillActive()
    {
        PlayRecThreat th = new PlayRecThreat();
        th.Enemy(4, "x", PlayRecThreat.StTele, 1f, 0f, 0f);
        th.Enemy(4, "x", PlayRecThreat.StAttack, 1f, 0f, PlayRecThreat.MaxWindow + 0.1f);
        List<PlayRecThreat.Window> closed = new List<PlayRecThreat.Window>();
        Assert.AreEqual(1, th.Collect(PlayRecThreat.MaxWindow + 0.1f, closed));
    }

    [Test]
    public void Threat_AttackWithoutTelegraph_OpensAndTakesEarlierAttempt()
    {
        PlayRecThreat th = new PlayRecThreat();
        th.Attempt(5, "iframe", 1.0f); // 판정 첫 프레임의 시도가 20Hz 관측보다 먼저 온다
        th.Attempt(6, "hit", 0.2f);    // 오래된(RecentGrace 밖) 다른 적 시도는 버려진다
        th.Enemy(5, "rat", PlayRecThreat.StAttack, 1.5f, 0f, 1.05f);
        Assert.AreEqual(1, th.OpenCount);
        List<PlayRecThreat.Window> closed = new List<PlayRecThreat.Window>();
        th.Collect(5f, closed);
        Assert.AreEqual("iframe", closed[0].Result);
        Assert.IsFalse(closed[0].Tele);
        Assert.IsTrue(closed[0].AttackSeen);
        th.Recycle(closed);
        th.Enemy(6, "rat", PlayRecThreat.StAttack, 1.5f, 0f, 6f);
        th.Collect(9f, closed);
        Assert.AreEqual("none", closed[0].Result);
    }

    // ───── tk 직렬화 ─────

    [Test]
    public void Tick_NoPlayer_WritesActionAndMarker()
    {
        PlayObservation o = new PlayObservation();
        StringBuilder sb = new StringBuilder();
        PlayRecJson.Begin(sb, 1f, "tk", "human");
        PlayRecTick.WriteTick(sb, 42, 2.5f, 5, -1f, 0f, o, true);
        PlayRecJson.End(sb);
        Assert.AreEqual("{\"t\":1.000,\"k\":\"tk\",\"src\":\"human\",\"n\":42,\"u\":2.500,\"a\":5,\"mx\":-1.00,\"my\":0.00,\"ui\":1,\"pl\":0}\n", sb.ToString());
    }

    [Test]
    public void Tick_WithPlayer_WritesStateArraysInOrder()
    {
        PlayObservation o = new PlayObservation
        {
            HasPlayer = true, X = 1.234f, Y = -2f, Vx = 0.5f, Vy = 0f, Grounded = true, Face = -1, CanBeHit = true,
            Fsm = "idle", Flags = "I", HasHealth = true, Hp = 50, HpMax = 100, Mp = 10, MpMax = 20, Combo = 2, Hits = 7,
        };
        o.Cooldowns[1] = 1.25f;
        StringBuilder sb = new StringBuilder();
        PlayRecTick.WriteTick(sb, 1, 0f, 0, 0f, 0f, o, false);
        string s = sb.ToString();
        StringAssert.Contains("\"p\":[1.23,-2.00,0.50,0.00]", s);
        StringAssert.Contains("\"s\":25", s); // 접지 1 + 왼쪽 8 + 피해 가능 16
        StringAssert.Contains("\"hp\":[50,100,10,20]", s);
        StringAssert.Contains("\"cd\":[0.0,1.3,0.0,0.0]", s);
        StringAssert.Contains("\"cb\":2,\"hc\":7", s);
        StringAssert.Contains("\"en\":[]", s);
        StringAssert.DoesNotContain("\"gh\"", s); // 격자 없음
        StringAssert.DoesNotContain("\"pj\"", s); // 투사체 없음
    }

    [Test]
    public void Header_ListsBitNames()
    {
        StringBuilder sb = new StringBuilder();
        PlayRecTick.WriteHeader(sb, new List<string> { "Player/Jump", "UI/Back" });
        Assert.AreEqual(",\"bits\":[\"Player/Jump\",\"UI/Back\"]", sb.ToString());
    }

    // ───── 북마크 패드 조합 ─────

    [Test]
    public void SelectR3Chord_NeedsBothHeldAndOnePressedNow()
    {
        Assert.IsTrue(PadFieldInputRules.IsSelectR3Chord(true, true, true));
        Assert.IsFalse(PadFieldInputRules.IsSelectR3Chord(true, true, false), "둘 다 계속 누르고만 있으면 한 번만");
        Assert.IsFalse(PadFieldInputRules.IsSelectR3Chord(true, false, true));
        Assert.IsFalse(PadFieldInputRules.IsSelectR3Chord(false, true, true));
    }
}
