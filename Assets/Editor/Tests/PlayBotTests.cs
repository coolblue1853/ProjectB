using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>자동 플레이 봇 B0·B1 순수 로직: 키 매핑·프로필 샘플링·평타 리듬·막힘 판정·목표 고르기·재생 변환·기록 폴더</summary>
public class PlayBotTests
{
    private static BotProfile.Dist D(float a, float b, float c, float d, float e)
    {
        return new BotProfile.Dist { key = "x", n = 50, p10 = a, p25 = b, p50 = c, p75 = d, p90 = e };
    }

    // ───── 키 매핑 ─────

    [Test]
    public void KeyFromPath_Names()
    {
        Assert.AreEqual(Key.X, BotKeyMap.KeyFromPath("<Keyboard>/x"));
        Assert.AreEqual(Key.LeftArrow, BotKeyMap.KeyFromPath("<Keyboard>/leftArrow"));
        Assert.AreEqual(Key.LeftShift, BotKeyMap.KeyFromPath("<Keyboard>/leftShift"));
        Assert.AreEqual(Key.Digit1, BotKeyMap.KeyFromPath("<Keyboard>/1"));
        Assert.AreEqual(Key.Enter, BotKeyMap.KeyFromPath("<Keyboard>/enter"));
    }

    [Test]
    public void KeyFromPath_RejectsOthers()
    {
        Assert.AreEqual(Key.None, BotKeyMap.KeyFromPath("<Gamepad>/buttonSouth"));
        Assert.AreEqual(Key.None, BotKeyMap.KeyFromPath(""));
        Assert.AreEqual(Key.None, BotKeyMap.KeyFromPath(null));
        Assert.AreEqual(Key.None, BotKeyMap.KeyFromPath("<Keyboard>/12"));
    }

    [Test]
    public void PadFromPath_Kinds()
    {
        GamepadButton b;
        Assert.AreEqual(BotKeyMap.PadKind.Button, BotKeyMap.PadFromPath("<Gamepad>/buttonWest", out b));
        Assert.AreEqual(GamepadButton.West, b);
        Assert.AreEqual(BotKeyMap.PadKind.Button, BotKeyMap.PadFromPath("<Gamepad>/dpad/up", out b));
        Assert.AreEqual(GamepadButton.DpadUp, b);
        Assert.AreEqual(BotKeyMap.PadKind.LeftTrigger, BotKeyMap.PadFromPath("<Gamepad>/leftTrigger", out b));
        Assert.AreEqual(BotKeyMap.PadKind.StickLeft, BotKeyMap.PadFromPath("<Gamepad>/leftStick/left", out b));
        Assert.AreEqual(BotKeyMap.PadKind.None, BotKeyMap.PadFromPath("<Keyboard>/x", out b));
    }

    [Test]
    public void FullName_AddsPlayerMap()
    {
        Assert.AreEqual("Player/Attack", BotKeyMap.FullName("Attack"));
        Assert.AreEqual("UI/Back", BotKeyMap.FullName("UI/Back"));
    }

    [Test]
    public void Resolve_ButtonAndAxis_FromAsset()
    {
        var asset = new KeyAction().asset;
        string pos, neg;
        Assert.IsTrue(BotKeyMap.Resolve(asset.FindAction("Player/Attack"), false, out pos, out neg));
        Assert.AreEqual("<Keyboard>/x", pos);
        Assert.IsNull(neg);
        Assert.IsTrue(BotKeyMap.Resolve(asset.FindAction("Player/Attack"), true, out pos, out neg));
        Assert.AreEqual("<Gamepad>/buttonWest", pos);
        Assert.IsTrue(BotKeyMap.Resolve(asset.FindAction("Player/Move"), false, out pos, out neg));
        Assert.AreEqual("<Keyboard>/rightArrow", pos);
        Assert.AreEqual("<Keyboard>/leftArrow", neg);
        Assert.IsTrue(BotKeyMap.Resolve(asset.FindAction("UI/verticalCheck"), false, out pos, out neg));
        Assert.AreEqual("<Keyboard>/upArrow", pos);
        Assert.IsFalse(BotKeyMap.Resolve(asset.FindAction("Player/Bestiary"), true, out pos, out neg));
    }

    [Test]
    public void Resolve_FollowsOverride()
    {
        var asset = new KeyAction().asset;
        InputAction atk = asset.FindAction("Player/Attack");
        atk.ApplyBindingOverride(InputBindings.BindingIndex(atk, false), "<Keyboard>/k");
        string pos, neg;
        BotKeyMap.Resolve(atk, false, out pos, out neg);
        Assert.AreEqual("<Keyboard>/k", pos);
    }

    [Test]
    public void EveryPlayerCombatAction_HasPadBinding()
    {
        var asset = new KeyAction().asset;
        string[] combat = { "Move", "Jump", "Dash", "Run", "Attack", "SkillA", "SkillS", "SkillD", "SkillF" };
        foreach (string a in combat)
        {
            string pos, neg;
            Assert.IsTrue(BotKeyMap.Resolve(asset.FindAction("Player/" + a), true, out pos, out neg), a);
        }
    }

    // ───── 프로필 ─────

    [Test]
    public void Profile_ParsesNullAsInvalid()
    {
        string json = "{\"version\":1,\"minN\":5,\"dists\":[{\"key\":\"a\",\"n\":3,\"unit\":\"s\",\"p10\":null,\"p25\":null,\"p50\":null,\"p75\":null,\"p90\":null},"
            + "{\"key\":\"b\",\"n\":9,\"unit\":\"s\",\"p10\":0.1,\"p25\":0.2,\"p50\":0.3,\"p75\":0.4,\"p90\":0.5}],"
            + "\"ratios\":[{\"key\":\"r\",\"n\":2,\"k\":1,\"value\":null},{\"key\":\"q\",\"n\":9,\"k\":3,\"value\":0.33}],\"rates\":[]}";
        BotProfile p = BotProfile.Parse(json);
        Assert.IsFalse(p.FindDist("a").Valid);
        Assert.IsTrue(p.FindDist("b").Valid);
        Assert.AreEqual(7f, p.Sample("a", new System.Random(1), 7f));
        Assert.AreEqual(9f, p.Sample("missing", new System.Random(1), 9f));
        Assert.AreEqual(0.5f, p.RatioOr("r", 0.5f));
        Assert.AreEqual(0.33f, p.RatioOr("q", 0f), 1e-5f);
    }

    [Test]
    public void Quantile_HitsKnotsAndStaysNonNegative()
    {
        BotProfile.Dist d = D(0.1f, 0.2f, 0.3f, 0.5f, 0.9f);
        Assert.AreEqual(0.1f, BotProfile.Quantile(d, 0.1f), 1e-5f);
        Assert.AreEqual(0.3f, BotProfile.Quantile(d, 0.5f), 1e-5f);
        Assert.AreEqual(0.9f, BotProfile.Quantile(d, 0.9f), 1e-5f);
        Assert.GreaterOrEqual(BotProfile.Quantile(d, 0f), 0f);
        Assert.Greater(BotProfile.Quantile(d, 1f), 0.9f);
    }

    [Test]
    public void Sample_MedianNearP50()
    {
        var p = new BotProfile();
        BotProfile.Dist d = D(0.1f, 0.2f, 0.3f, 0.5f, 0.9f);
        d.key = "k";
        p.dists.Add(d);
        var rng = new System.Random(7);
        var v = new List<float>();
        for (int i = 0; i < 2001; i++)
        {
            v.Add(p.Sample("k", rng, -1f));
        }
        v.Sort();
        Assert.AreEqual(0.3f, v[1000], 0.03f);
    }

    [Test]
    public void ShippedProfile_LoadsCoreKeys()
    {
        BotProfile p = BotProfile.Load();
        Assert.IsTrue(p.FindDist("press.Attack").Valid);
        Assert.IsTrue(p.FindDist("attack.burstWithin").Valid);
        Assert.IsTrue(p.FindDist("react.idle").Valid);
    }

    // ───── 리듬·막힘·목표 ─────

    [Test]
    public void AttackRhythm_BurstThenGap()
    {
        var p = new BotProfile();
        p.dists.Add(new BotProfile.Dist { key = "attack.burstLen", n = 9, p10 = 3, p25 = 3, p50 = 3, p75 = 3, p90 = 3 });
        p.dists.Add(new BotProfile.Dist { key = "attack.burstWithin", n = 9, p10 = 0.2f, p25 = 0.2f, p50 = 0.2f, p75 = 0.2f, p90 = 0.2f });
        p.dists.Add(new BotProfile.Dist { key = "attack.burstGap", n = 9, p10 = 1f, p25 = 1f, p50 = 1f, p75 = 1f, p90 = 1f });
        var r = new BotAttackRhythm(p, new System.Random(1));
        var presses = new List<float>();
        for (float t = 0f; t < 3f; t += 0.01f)
        {
            if (r.Due(t))
            {
                presses.Add(t);
            }
        }
        Assert.GreaterOrEqual(presses.Count, 5);
        Assert.AreEqual(0.2f, presses[1] - presses[0], 0.02f);
        Assert.AreEqual(1.0f, presses[3] - presses[2], 0.02f); // 3타 묶음 뒤 쉼
    }

    [Test]
    public void Stuck_JumpThenReverse()
    {
        var s = new BotStuck();
        s.Reset(0f, 0f);
        Assert.AreEqual(BotStuck.Act.None, s.Update(0.5f, 0.1f, true));
        Assert.AreEqual(BotStuck.Act.Jump, s.Update(0.9f, 0.1f, true));
        Assert.AreEqual(BotStuck.Act.None, s.Update(1.5f, 0.1f, true));
        Assert.AreEqual(BotStuck.Act.Reverse, s.Update(2.1f, 0.1f, true));
        Assert.AreEqual(BotStuck.Act.None, s.Update(2.2f, 2f, true)); // 움직이면 초기화
    }

    [Test]
    public void Stuck_NotWhenStanding()
    {
        var s = new BotStuck();
        s.Reset(0f, 0f);
        Assert.AreEqual(BotStuck.Act.None, s.Update(5f, 0f, false));
        Assert.IsTrue(s.NoProgress(3.5f));
        s.Progress(3.5f);
        Assert.IsFalse(s.NoProgress(4f));
    }

    private static PlayObsEnemies Foes(params PlayObsEnemies.Foe[] f)
    {
        var e = new PlayObsEnemies();
        for (int i = 0; i < f.Length; i++)
        {
            e.Foes[i] = f[i];
        }
        e.FoeCount = f.Length;
        return e;
    }

    private static PlayObsEnemies.Foe F(int uid, float dx, float dy, int hp, int state = 0)
    {
        return new PlayObsEnemies.Foe { Uid = uid, Dx = dx, Dy = dy, Dist = UnityEngine.Mathf.Abs(dx), HpPercent = hp, State = state };
    }

    [Test]
    public void Choose_NearestLowestSecond()
    {
        var p = new BotProfile();
        p.ratios.Add(new BotProfile.Ratio { key = "target.lowestHp", n = 9, value = 0.1f });
        p.ratios.Add(new BotProfile.Ratio { key = "target.nearest", n = 9, value = 0.5f });
        PlayObsEnemies e = Foes(F(1, 1f, 0f, 100), F(2, 3f, 0f, 40), F(3, 4f, 0f, 90));
        Assert.AreEqual(1, HuntBrain.Choose(e, 0.05, p)); // HP 최저
        Assert.AreEqual(0, HuntBrain.Choose(e, 0.3, p));  // 최근접
        Assert.AreEqual(1, HuntBrain.Choose(e, 0.9, p));  // 두 번째로 가까움
    }

    [Test]
    public void Choose_SkipsDeadAndPrefersSameFloor()
    {
        var p = new BotProfile();
        PlayObsEnemies e = Foes(F(1, 0.5f, 0f, 0, 5), F(2, 1f, 5f, 100), F(3, 6f, 0.3f, 100));
        Assert.AreEqual(2, HuntBrain.Choose(e, 0.3, p));
        Assert.AreEqual(-1, HuntBrain.Choose(Foes(F(1, 1f, 0f, 0, 5)), 0.3, p));
        Assert.AreEqual(1, HuntBrain.Choose(e, 0.3, p, new HashSet<int> { 3 })); // 무시 목록이면 다른 층이라도 남은 것

    }

    [Test]
    public void Choose_SkipsProps()
    {
        var p = new BotProfile();
        PlayObsEnemies.Foe tree = F(1, 0.2f, 0f, 100);
        tree.Prop = true; // 나무·바위 같은 파괴 오브젝트는 목표가 아니다(11-17 봇이 나무를 몹처럼 침)
        PlayObsEnemies e = Foes(tree, F(2, 3f, 0f, 100));
        Assert.AreEqual(1, HuntBrain.Choose(e, 0.3, p));
        Assert.AreEqual(-1, HuntBrain.Choose(Foes(tree), 0.3, p));
    }

    // ───── 재생·기록 폴더 ─────

    [Test]
    public void Replay_BuildsKeySteps()
    {
        var asset = new KeyAction().asset;
        string[] lines =
        {
            "{\"t\":10.0,\"u\":10.0,\"k\":\"in\",\"a\":\"Player/Attack\",\"ph\":\"S\",\"dev\":\"kb\",\"v\":1}",
            "{\"t\":10.1,\"u\":10.1,\"k\":\"in\",\"a\":\"Player/Attack\",\"ph\":\"C\",\"dev\":\"kb\",\"v\":0}",
            "{\"t\":10.2,\"u\":10.2,\"k\":\"in\",\"a\":\"Player/Move\",\"ph\":\"S\",\"dev\":\"kb\",\"v\":-1}",
            "{\"t\":10.3,\"u\":10.3,\"k\":\"in\",\"a\":\"Debug Menu/Debug Horizontal\",\"ph\":\"S\",\"dev\":\"kb\",\"v\":1}",
            "{\"t\":10.4,\"u\":10.4,\"k\":\"in\",\"a\":\"Player/Move\",\"ph\":\"C\",\"dev\":\"kb\",\"v\":0}",
        };
        List<BotReplay.Step> s = BotReplay.BuildFromLines(lines, asset);
        Assert.AreEqual(6, s.Count);
        Assert.AreEqual("<Keyboard>/x", s[0].Path);
        Assert.IsTrue(s[0].Down);
        Assert.IsFalse(s[1].Down);
        Assert.AreEqual(0.2f, s[2].T, 1e-4f);
        Assert.AreEqual("<Keyboard>/rightArrow", s[2].Path);
        Assert.IsFalse(s[2].Down);
        Assert.AreEqual("<Keyboard>/leftArrow", s[3].Path);
        Assert.IsTrue(s[3].Down);
        Assert.IsFalse(s[5].Down);
    }

    [Test]
    public void RecorderDir_BySource()
    {
        Assert.AreEqual("playrec", PlayRecWriter.DirNameFor("human"));
        Assert.AreEqual("playrec", PlayRecWriter.DirNameFor(null));
        Assert.AreEqual("playrec_bot", PlayRecWriter.DirNameFor("bot"));
        Assert.AreEqual("playrec_bot", PlayRecWriter.DirNameFor("replay"));
        Assert.AreEqual("playrec_test", PlayRecWriter.DirNameFor("test"));
    }

    [Test]
    public void AgentFlag_ActiveOnlyInWindow()
    {
        System.DateTime now = new System.DateTime(2026, 11, 16, 5, 0, 0, System.DateTimeKind.Utc);
        Assert.IsTrue(PlayRecAgentFlag.IsActive(now.AddMinutes(-10), now));
        Assert.IsFalse(PlayRecAgentFlag.IsActive(now.AddMinutes(-31), now));
        Assert.IsFalse(PlayRecAgentFlag.IsActive(now.AddHours(-5), now));
    }

    [Test]
    public void NavWatch_WalkingIsNotStuck()
    {
        BotNavWatch w = new BotNavWatch();
        w.Reset(0f);
        // 목표가 바뀌어 거리 지표가 줄지 않아도(30 → 40 → 40) 계속 걸으면 막힘이 아니다
        float x = 0f;
        for (int i = 1; i <= 100; i++)
        {
            x += 0.3f;
            Assert.AreEqual(BotNavWatch.Verdict.None, w.Check(i * 0.1f, 40f, x), "t=" + i * 0.1f);
        }
    }

    [Test]
    public void NavWatch_StandingStillIsStuck()
    {
        BotNavWatch w = new BotNavWatch();
        w.Reset(0f);
        BotNavWatch.Verdict v = BotNavWatch.Verdict.None;
        BotNavWatch.Verdict first = BotNavWatch.Verdict.None;
        for (int i = 1; i <= 80 && v != BotNavWatch.Verdict.Stuck; i++)
        {
            v = w.Check(i * 0.1f, 40f, 5f);
            if (first == BotNavWatch.Verdict.None && v != BotNavWatch.Verdict.None)
            {
                first = v;
            }
        }
        Assert.AreEqual(BotNavWatch.Verdict.Reroute, first);
        Assert.AreEqual(BotNavWatch.Verdict.Stuck, v);
    }

    [Test]
    public void NavWatch_ClimbingStairsIsNotStuck()
    {
        // 계단 점프: 바닥이 0.26 → 1.26 → 2.26 으로 올라가는 중(점프 꼭대기 오르내림 포함), x 는 제자리 근처
        BotNavWatch w = new BotNavWatch();
        w.Reset(0f);
        for (int i = 1; i <= 120; i++)
        {
            float t = i * 0.1f;
            float floor = 0.26f + (int)((t + 2f) / 4f);
            float y = floor + (i % 10 == 0 ? 2.5f : 0f);
            Assert.AreEqual(BotNavWatch.Verdict.None, w.Check(t, 40f, -386f, y, true), "t=" + t);
        }
    }

    [Test]
    public void NavWatch_JumpingInPlaceIsStillStuck()
    {
        // 같은 높이 제자리 점프(바닥 안 올라감)는 점프 간선 수행 중이어도 막힘
        BotNavWatch w = new BotNavWatch();
        w.Reset(0f);
        BotNavWatch.Verdict v = BotNavWatch.Verdict.None;
        for (int i = 1; i <= 100 && v != BotNavWatch.Verdict.Stuck; i++)
        {
            float y = 1.26f + (i % 10 < 3 ? 2.5f : 0f);
            v = w.Check(i * 0.1f, 40f, -386f, y, true);
        }
        Assert.AreEqual(BotNavWatch.Verdict.Stuck, v);
    }

    [Test]
    public void NavWatch_FlatStandingIsStuckEvenIfClimbing()
    {
        BotNavWatch w = new BotNavWatch();
        w.Reset(0f);
        BotNavWatch.Verdict v = BotNavWatch.Verdict.None;
        for (int i = 1; i <= 100 && v != BotNavWatch.Verdict.Stuck; i++)
        {
            v = w.Check(i * 0.1f, 40f, 5f, 2.26f, true);
        }
        Assert.AreEqual(BotNavWatch.Verdict.Stuck, v);
    }

    [Test]
    public void NavWatch_ClimbRise_PureFunction()
    {
        System.Collections.Generic.List<float> ts = new System.Collections.Generic.List<float>();
        System.Collections.Generic.List<float> ys = new System.Collections.Generic.List<float>();
        for (int i = 0; i <= 40; i++)
        {
            ts.Add(i * 0.1f);
            ys.Add(i < 20 ? 0.26f : 1.26f);
        }
        Assert.AreEqual(1f, BotNavWatch.ClimbRise(ts, ys, 4f), 0.001f);
        Assert.AreEqual(0f, BotNavWatch.ClimbRise(ts, ys, 1f), 0.001f, "기록이 짧으면 0");
        Assert.AreEqual(0f, BotNavWatch.ClimbRise(new System.Collections.Generic.List<float>(), new System.Collections.Generic.List<float>(), 9f));
    }

    [Test]
    public void Stall_FiresAfterSecondsAndClears()
    {
        BotStall s = new BotStall();
        Assert.IsFalse(s.Update(0f, true, 1.5f));
        Assert.IsFalse(s.Update(1.4f, true, 1.5f));
        Assert.IsTrue(s.Update(1.6f, true, 1.5f));
        Assert.IsFalse(s.Update(1.7f, true, 1.5f)); // 한 번 주고 다시 센다
        Assert.IsFalse(s.Update(2.0f, false, 1.5f));
        Assert.IsFalse(s.Update(2.1f, true, 1.5f));
    }
}
