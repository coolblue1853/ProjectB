using UnityEngine;

/// <summary>
/// 봇 스킬 투자(B5-1). 레벨업·숙련으로 쓸 수 있는 포인트가 생기면 스킬창을 진짜 키(SkillWindow 단축키)로 열고,
/// 탭(→)·줄(↑↓)·확인(Enter)으로 투자한 뒤 Back 으로 닫는다. SkillTreeService.Invest 를 직접 부르지 않는다(읽기만: Available·LevelOf).
/// 전투 중·다른 창이 열려 있으면 하지 않는다. 규칙은 BotSkillRules(순수). 설계 PlayBotDesign.md 16절
/// </summary>
public sealed class BotSkillInvest
{
    public const float EvalInterval = 2f;
    public const float OpenWait = 2.5f;
    public const float RunTimeout = 40f;
    public const float FailCooldown = 60f;
    public const int MaxOpenTries = 3;

    private float _nextEval;
    private float _nextKey;
    private float _blockedUntil;
    private float _startedAt;
    private bool _active;
    private bool _wasOpen;
    private int _openTries;
    private int _startTotal;

    /// <summary>투자 성공 수(실행 전후 노드 레벨 합 차이)</summary>
    public int Invested { get; private set; }
    public int Aborts { get; private set; }
    public bool Busy => _active;

    public void Begin(BotContext ctx)
    {
        _active = false;
        Invested = 0;
        Aborts = 0;
        _openTries = 0;
        _blockedUntil = ctx.Now + 5f;
        _nextEval = ctx.Now;
    }

    private static string TypeTree()
    {
        return SkillTreeService.TreeOf(SkillTreeEffects.CurrentKind());
    }

    private static BotSkillPick Plan()
    {
        string tt = TypeTree();
        return BotSkillRules.Pick(tt, tt != null ? SkillTreeService.Available(tt) : 0,
            SkillTreeService.Available(SkillTreeRules.CommonTree), (tree, id) => SkillTreeService.LevelOf(tree, id));
    }

    private static int TotalLevels()
    {
        int n = 0;
        string tt = TypeTree();
        foreach (SkillNode node in SkillTreeRules.Nodes)
        {
            string tree = node.Common ? SkillTreeRules.CommonTree : tt;
            n += tree == null ? 0 : SkillTreeService.LevelOf(tree, node.Id);
        }
        return n;
    }

    /// <summary>이 틱의 조작을 맡았으면 true. foeNear = 가까이 적(전투 양보)</summary>
    public bool Run(BotContext ctx, bool foeNear, ref BotIntent it)
    {
        if (_active)
        {
            return Drive(ctx, ref it);
        }
        if (ctx.Now < _nextEval || ctx.Now < _blockedUntil)
        {
            return false;
        }
        _nextEval = ctx.Now + EvalInterval;
        if (foeNear || ctx.Obs.UiOpen || CombatState.IsInCombat())
        {
            return false;
        }
        BotSkillPick p = Plan();
        if (!p.Found)
        {
            return false;
        }
        _active = true;
        _wasOpen = false;
        _startedAt = ctx.Now;
        _openTries = 0;
        _startTotal = TotalLevels();
        Debug.Log("[PB][Bot] 스킬 투자 시작 Lv=" + LevelSystem.Level + " 노드=" + p.Tree + "/" + p.NodeId);
        return Drive(ctx, ref it);
    }

    private bool Drive(BotContext ctx, ref BotIntent it)
    {
        if (ctx.Now - _startedAt > RunTimeout)
        {
            return Fail(ctx, "시간 초과", ref it);
        }
        if (!SkillTreeWindow.IsOpen)
        {
            if (_wasOpen)
            {
                Finish(ctx, "창 닫힘"); // 우리가 Back 으로 닫았다
                return false;
            }
            if (ctx.Obs.UiOpen)
            {
                return Fail(ctx, "다른 창이 열려 있음", ref it);
            }
            if (ctx.Now < _nextKey)
            {
                return true;
            }
            if (_openTries++ >= MaxOpenTries)
            {
                return Fail(ctx, "스킬창이 안 열림", ref it);
            }
            Press(ctx, BotKey.SkillWindow, ref it, OpenWait);
            it.Note = "skill:open";
            return true;
        }
        _wasOpen = true;
        BotSkillPick p = Plan();
        int rows = p.Found ? SkillTreeRules.NodesOf(SkillTreeWindow.CurrentTab).Count : 0;
        BotKey k = BotSkillRules.Step(p.Found, SkillTreeWindow.CurrentTab, SkillTreeWindow.CursorIndex, p.Tab, p.Row, rows);
        Press(ctx, k, ref it, k == BotKey.Confirm ? 0.35f : 0f);
        it.Note = "skill:" + k;
        return true;
    }

    private void Press(BotContext ctx, BotKey key, ref BotIntent it, float extraWait)
    {
        if (key == BotKey.None || ctx.Now < _nextKey)
        {
            return;
        }
        it.Menu = key;
        float gap = ctx.Profile.Sample("menu.focusInterval", ctx.Rng, 0.25f);
        _nextKey = ctx.Now + Mathf.Clamp(gap, 0.22f, 0.9f) + extraWait;
    }

    private void Finish(BotContext ctx, string why)
    {
        int gained = Mathf.Max(0, TotalLevels() - _startTotal);
        Invested += gained;
        Debug.Log("[PB][Bot] 스킬 투자 끝 " + why + " 투자 +" + gained + " 누계=" + Invested + " Lv=" + LevelSystem.Level);
        _active = false;
        _blockedUntil = ctx.Now + 3f;
    }

    private bool Fail(BotContext ctx, string why, ref BotIntent it)
    {
        Aborts++;
        Invested += Mathf.Max(0, TotalLevels() - _startTotal); // 중단돼도 이미 한 투자는 센다
        Debug.LogWarning("[PB][Bot] 스킬 투자 중단 " + why);
        _active = false;
        _blockedUntil = ctx.Now + FailCooldown;
        if (SkillTreeWindow.IsOpen)
        {
            it.Menu = BotKey.Back; // 열려 있으면 이 틱에 닫는다
            return true;
        }
        return false;
    }
}
