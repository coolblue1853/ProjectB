using UnityEngine;

/// <summary>
/// B1 이동: 목표 쪽으로 직진. 목표가 위에 있고 가까우면 점프, 막히면 점프 → 반대로 걷기(BotStuck). 로프·포털 길찾기는 B2(IBotNavigator 교체).
/// </summary>
public sealed class DirectNavigator : IBotNavigator
{
    public const float ReverseSeconds = 1.5f;
    public const float ArriveDx = 0.3f;
    public const float MaxJumpDy = 4f;      // 이보다 높은 발판은 점프로 못 간다(로프·길찾기는 B2)

    private readonly BotStuck _stuck = new BotStuck();
    private float _reverseUntil;
    private int _reverseDir;
    private bool _started;
    private float _lastSteer;

    public int StuckJumps { get; private set; }
    public int Reverses { get; private set; }

    public void Reset()
    {
        _started = false;
        _reverseUntil = 0f;
    }

    public bool Steer(BotContext ctx, float dx, float dy, ref BotIntent intent)
    {
        PlayObservation o = ctx.Obs;
        if (!_started || ctx.Now - _lastSteer > 0.3f)
        {
            _stuck.Reset(ctx.Now, o.X); // 공격하느라 멈췄다 다시 걸을 때 옛 기준점으로 막힘 판정하지 않게
            _started = true;
        }
        _lastSteer = ctx.Now;
        if (ctx.Now < _reverseUntil)
        {
            intent.MoveX = _reverseDir;
            return false;
        }
        intent.MoveX = Mathf.Abs(dx) < ArriveDx ? 0 : dx > 0f ? 1 : -1; // 바로 위·아래면 좌우로 떨지 않는다
        if (dy > 1.5f && dy < MaxJumpDy && Mathf.Abs(dx) < 2.5f && o.Grounded)
        {
            intent.Jump = true; // 위 발판의 목표
        }
        switch (_stuck.Update(ctx.Now, o.X, intent.MoveX != 0))
        {
            case BotStuck.Act.Jump:
                intent.Jump = true;
                StuckJumps++;
                return false;
            case BotStuck.Act.Reverse:
                _reverseDir = -intent.MoveX;
                _reverseUntil = ctx.Now + ReverseSeconds;
                intent.MoveX = _reverseDir;
                Reverses++;
                return false;
        }
        return true;
    }
}

/// <summary>
/// B1 사냥 판단: 가장 가까운(사람 비율대로 고른) 적에게 걸어가 평타로 처치하고, 드롭이 있으면 줍는다. 적이 없으면 발판 끝에서 돌아서며 돌아다닌다.
/// 목표 고르기 비율은 프로필 target.nearest / target.lowestHp / target.dropFirst.
/// </summary>
public sealed class HuntBrain : IBotBrain
{
    public const float AttackRange = 1.3f;   // 근접 평타가 닿는 가로 거리(칼 기준, 실측 전 보수적으로)
    public const float AttackDy = 1.2f;
    public const float DropRange = 8f;
    public const float SameFloorDy = 2f;
    public const float UiCloseAfter = 1.5f;
    public const float IgnoreSeconds = 10f;  // 진척 없는 목표는 이 시간 동안 고르지 않는다

    private readonly IBotNavigator _nav;
    private int _targetUid;
    private int _targetHp;
    private float _targetDist;
    private bool _goDrop;
    private int _wanderDir = 1;
    private float _uiSince = -1f;
    private float _lastBack;
    private readonly BotStuck _progress = new BotStuck();
    private int _killsSeen;
    private readonly System.Collections.Generic.Dictionary<int, float> _ignore = new System.Collections.Generic.Dictionary<int, float>();
    private readonly System.Collections.Generic.HashSet<int> _ignoreNow = new System.Collections.Generic.HashSet<int>();

    public string LastNote { get; private set; } = "";
    public bool NoProgress { get; private set; }

    public HuntBrain(IBotNavigator nav)
    {
        _nav = nav;
    }

    public void Begin(BotContext ctx)
    {
        _targetUid = 0;
        _goDrop = false;
        _nav.Reset();
        _progress.Reset(ctx.Now, ctx.Obs.X);
        _killsSeen = ctx.Kills;
    }

    public BotIntent Decide(BotContext ctx)
    {
        PlayObservation o = ctx.Obs;
        BotIntent it = default(BotIntent);
        if (CloseUi(ctx, ref it))
        {
            return it;
        }
        if (ctx.Kills != _killsSeen)
        {
            _killsSeen = ctx.Kills;
            _progress.Progress(ctx.Now);
            _targetUid = 0;
            _goDrop = o.Drop.Found && ctx.Rng.NextDouble() < ctx.Profile.RatioOr("target.dropFirst", 0f);
        }
        if (_goDrop && o.Drop.Found && Mathf.Abs(o.Drop.Dx) < DropRange)
        {
            return Steer(ctx, o.Drop.Dx, o.Drop.Dy, BotIntent.FocusDrop, "drop", ref it);
        }
        _goDrop = false;
        int idx = FindTarget(ctx);
        if (idx < 0)
        {
            if (o.Drop.Found && Mathf.Abs(o.Drop.Dx) < DropRange)
            {
                return Steer(ctx, o.Drop.Dx, o.Drop.Dy, BotIntent.FocusDrop, "drop", ref it);
            }
            return Wander(ctx, ref it);
        }
        PlayObsEnemies.Foe f = o.Foes.Foes[idx];
        TrackProgress(ctx, f);
        if (NoProgress)
        {
            _ignore[f.Uid] = ctx.Now + IgnoreSeconds; // 못 닿는 목표(다른 층 등)는 잠시 빼고 다시 고른다
            _progress.Progress(ctx.Now);
            _targetUid = 0;
        }
        it.Focus = f.Uid;
        if (Mathf.Abs(f.Dx) <= AttackRange && Mathf.Abs(f.Dy) <= AttackDy)
        {
            int side = f.Dx >= 0f ? 1 : -1;
            it.MoveX = o.Face != side ? side : 0; // 반대를 보고 있으면 한 걸음 돌아선다
            it.Attack = true;
            it.Note = "attack";
            LastNote = it.Note;
            return it;
        }
        return Steer(ctx, f.Dx, f.Dy, f.Uid, "approach", ref it);
    }

    private BotIntent Steer(BotContext ctx, float dx, float dy, int focus, string note, ref BotIntent it)
    {
        it.Focus = focus;
        it.Note = note;
        _nav.Steer(ctx, dx, dy, ref it);
        NoProgress = _progress.NoProgress(ctx.Now);
        LastNote = note;
        return it;
    }

    private bool CloseUi(BotContext ctx, ref BotIntent it)
    {
        if (!ctx.Obs.UiOpen)
        {
            _uiSince = -1f;
            return false;
        }
        if (_uiSince < 0f)
        {
            _uiSince = ctx.Now;
        }
        if (ctx.Now - _uiSince > UiCloseAfter && ctx.Now - _lastBack > UiCloseAfter)
        {
            it.Back = true;
            _lastBack = ctx.Now;
        }
        it.Focus = BotIntent.FocusUi;
        it.Note = "ui";
        LastNote = it.Note;
        return true;
    }

    private void TrackProgress(BotContext ctx, PlayObsEnemies.Foe f)
    {
        if (f.Uid != _targetUid)
        {
            _targetUid = f.Uid;
            _targetHp = f.HpPercent;
            _targetDist = f.Dist;
            _progress.Progress(ctx.Now);
            return;
        }
        if (f.HpPercent < _targetHp || f.Dist < _targetDist - 0.5f)
        {
            _targetHp = f.HpPercent;
            _targetDist = f.Dist;
            _progress.Progress(ctx.Now);
        }
        NoProgress = _progress.NoProgress(ctx.Now);
    }

    /// <summary>살아 있는 현재 목표를 이어 가고, 없으면 사람 비율대로 새로 고른다</summary>
    private int FindTarget(BotContext ctx)
    {
        PlayObsEnemies foes = ctx.Obs.Foes;
        _ignoreNow.Clear();
        if (_ignore.Count > 64)
        {
            _ignore.Clear(); // 풀 재사용 uid 가 쌓이지 않게(만료 판정보다 단순하게 비운다)
        }
        foreach (var kv in _ignore)
        {
            if (kv.Value > ctx.Now)
            {
                _ignoreNow.Add(kv.Key);
            }
        }
        for (int i = 0; i < foes.FoeCount; i++)
        {
            if (foes.Foes[i].Uid == _targetUid && Alive(foes.Foes[i]) && !_ignoreNow.Contains(_targetUid))
            {
                return i;
            }
        }
        return Choose(foes, ctx.Rng.NextDouble(), ctx.Profile, _ignoreNow);
    }

    private static bool Alive(PlayObsEnemies.Foe f)
    {
        return f.State != 5 && f.HpPercent > 0;
    }

    private static bool Usable(PlayObsEnemies.Foe f, System.Collections.Generic.HashSet<int> ignore)
    {
        return Alive(f) && (ignore == null || !ignore.Contains(f.Uid));
    }

    /// <summary>
    /// 순수 선택: 같은 층(|dy|≤2) 후보가 있으면 그 안에서. r &lt; lowestHp 비율 → HP 최저, 그다음 nearest 비율 → 최근접, 나머지 → 두 번째로 가까운 것.
    /// Foes 는 거리순이다. 후보 없으면 -1.
    /// </summary>
    public static int Choose(PlayObsEnemies foes, double r, BotProfile profile, System.Collections.Generic.HashSet<int> ignore = null)
    {
        bool anySame = false;
        for (int i = 0; i < foes.FoeCount; i++)
        {
            if (Usable(foes.Foes[i], ignore) && Mathf.Abs(foes.Foes[i].Dy) <= SameFloorDy)
            {
                anySame = true;
                break;
            }
        }
        int first = -1, second = -1, lowest = -1;
        for (int i = 0; i < foes.FoeCount; i++)
        {
            PlayObsEnemies.Foe f = foes.Foes[i];
            if (!Usable(f, ignore) || (anySame && Mathf.Abs(f.Dy) > SameFloorDy))
            {
                continue;
            }
            if (first < 0)
            {
                first = i;
            }
            else if (second < 0)
            {
                second = i;
            }
            if (lowest < 0 || f.HpPercent < foes.Foes[lowest].HpPercent)
            {
                lowest = i;
            }
        }
        if (first < 0)
        {
            return -1;
        }
        float pLow = profile.RatioOr("target.lowestHp", 0f);
        float pNear = profile.RatioOr("target.nearest", 1f);
        if (r < pLow)
        {
            return lowest;
        }
        if (r < pLow + pNear || second < 0)
        {
            return first;
        }
        return second;
    }

    private BotIntent Wander(BotContext ctx, ref BotIntent it)
    {
        PlayObservation o = ctx.Obs;
        float edge = _wanderDir > 0 ? o.Terrain.EdgeR : o.Terrain.EdgeL;
        if (o.Grounded && edge >= 0f && edge < 0.6f && o.Terrain.DropF > 2f)
        {
            _wanderDir = -_wanderDir; // 낭떠러지 앞에서 돌아선다
        }
        it.Focus = BotIntent.FocusWander;
        it.Note = "wander";
        it.MoveX = _wanderDir;
        BotIntent probe = it;
        _nav.Steer(ctx, _wanderDir * 10f, 0f, ref probe);
        if (probe.MoveX != _wanderDir)
        {
            _wanderDir = probe.MoveX; // 막혀서 반대로
        }
        it.MoveX = probe.MoveX;
        it.Jump = probe.Jump;
        LastNote = it.Note;
        return it;
    }
}
