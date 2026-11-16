using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 진척 감시(순수 로직): 남은 거리 지표가 0.5 넘게 줄거나 간선을 해내면 진척. WarnAfter(3초) 진척이 없으면 먼저 다른 경로(Reroute),
/// 다른 경로로도 3초 진척이 없으면 막힘(Stuck)으로 알린다.
/// </summary>
public sealed class BotNavWatch
{
    public const float After = 3f;
    public const int MaxReroutes = 1;

    public enum Verdict { None, Reroute, Stuck }

    /// <summary>제자리가 아니라 이만큼 움직였으면 걷고 있는 것 — 거리 지표(목표가 바뀌거나 지나쳐도 늘어남)가 안 줄어도 막힘으로 보지 않는다</summary>
    public const float MoveProgress = 1.5f;

    private float _last;
    private float _best = float.MaxValue;
    private int _reroutes;
    private float _anchorX = float.NaN;

    public void Reset(float now)
    {
        _last = now;
        _best = float.MaxValue;
        _reroutes = 0;
        _anchorX = float.NaN;
    }

    /// <summary>간선 성공 같은 확실한 진척</summary>
    public void Progress(float now)
    {
        _last = now;
        _best = float.MaxValue;
        _reroutes = 0;
    }

    /// <summary>x = 지금 위치. 마지막 진척 뒤 MoveProgress 넘게 움직였으면 진척(막힘 오탐 방지: 11-16 NormalForest x≈-137 걷는 중 막힘)</summary>
    public Verdict Check(float now, float metric, float x)
    {
        if (float.IsNaN(_anchorX))
        {
            _anchorX = x;
        }
        else if (Mathf.Abs(x - _anchorX) >= MoveProgress)
        {
            _anchorX = x;
            _best = metric;
            _last = now;
            return Verdict.None;
        }
        return Check(now, metric);
    }

    public Verdict Check(float now, float metric)
    {
        if (metric < _best - 0.5f)
        {
            _best = metric;
            _last = now;
            return Verdict.None;
        }
        if (now - _last <= After)
        {
            return Verdict.None;
        }
        _last = now;
        _best = float.MaxValue;
        _anchorX = float.NaN;
        if (_reroutes < MaxReroutes)
        {
            _reroutes++;
            return Verdict.Reroute;
        }
        _reroutes = 0;
        return Verdict.Stuck;
    }
}

/// <summary>
/// B2 이동: 구운 이동 그래프(BotNavGraph)로 길을 찾고 간선 하나씩 진짜 입력으로 해낸다(걷기·떨어지기·점프(길게 눌러 최고 높이)·2단 점프·하향 점프(↓ 먼저 + 점프)·
/// 로프(밑에서 ↑ 톡 → ↑/↓ 누른 채)·포털(↑ 톡)). 같은 구간이거나 그래프가 없으면 DirectNavigator 로 넘긴다.
/// 실패 처리: 간선을 해내지 못하면 그 간선을 20초 빼고 다시 찾는다. BotNavWatch 가 3초 진척 없음 → 다른 경로 → 막힘(위치 로그·스크린샷) 순으로 처리한다.
/// 금지: 위치·속도 대입·게임 함수 호출. 읽기 = PlayObservation(+ 맵 이름).
/// </summary>
public sealed class GraphNavigator : IBotNavigator
{
    public const float BanSeconds = 20f;
    public const float JumpHold = 0.45f;
    public const float ReplanSeconds = 1f;
    public const float StuckShotGap = 20f;
    public const float RopeHang = 2.5f;

    private readonly DirectNavigator _direct = new DirectNavigator();
    private readonly BotNavWatch _watch = new BotNavWatch();
    private readonly Dictionary<int, float> _banUntil = new Dictionary<int, float>();
    private readonly List<int> _banned = new List<int>();
    private BotNavGraph _g;
    private string _gMap;
    private List<int> _path;
    private int _pathFrom = -1, _pathGoal = -1;
    private float _planAt = -10f;
    private int _edge = -1;
    private int _edgeFrom = -1;
    private float _edgeStart;
    private int _tick;
    private bool _airJumped;
    private float _lastShot = -100f;
    private float _lastNoPathStuck = -100f;
    private float _ropeSince = -1f;
    private string _hopFrom, _hopTo, _hopNext;
    private float _lastSteer = -10f;

    public int Jumps { get; private set; }
    public int DownJumps { get; private set; }
    public int Ropes { get; private set; }
    public int Portals { get; private set; }
    public int EdgeFails { get; private set; }
    public int Reroutes { get; private set; }
    public int Stucks { get; private set; }
    public int StuckJumps => _direct.StuckJumps;
    public int Reverses => _direct.Reverses;
    public bool HasGraph => _g != null;
    /// <summary>도약 자리 1.5칸 안·간선 진행 중: 10Hz 판단으로는 한 틱에 1칸씩 지나쳐 턱에 몸이 붙는다 → PlayBot 이 매 프레임 판단한다</summary>
    public bool Precise => _precise && Time.timeScale > 0f && Time.time - _preciseAt < 0.15f;
    private bool _precise;
    private float _preciseAt;
    private bool _ropeCounted;
    /// <summary>마지막 막힘 설명(로그·보고용)</summary>
    public string LastStuck { get; private set; } = "";
    private int _dbgCur = -1, _dbgGoal = -1;

    /// <summary>마지막 TravelTo 실패 이유(경고 로그용)</summary>
    public string TravelFail { get; private set; } = "";

    /// <summary>상태 한 줄(디버그): 지금 구간·목표 구간·진행 간선·남은 경로</summary>
    public string DebugState
    {
        get
        {
            string e = _edge >= 0 && _g != null ? _g.edges[_edge].kind + ":" + _g.edges[_edge].from + ">" + _g.edges[_edge].to : "-";
            return "nav=" + _dbgCur + ">" + _dbgGoal + "/e" + e + "/p" + (_path != null ? _path.Count : -1);
        }
    }

    public void Reset()
    {
        _direct.Reset();
        _path = null;
        _edge = -1;
        _pathFrom = _pathGoal = -1;
    }

    public bool Steer(BotContext ctx, float dx, float dy, ref BotIntent intent)
    {
        PlayObservation o = ctx.Obs;
        EnsureGraph(ctx);
        if (_g == null)
        {
            return _direct.Steer(ctx, dx, dy, ref intent);
        }
        float gx = o.X + dx;
        int goal = _g.SegBelow(gx, o.Y + dy + 0.6f); // 목표 기준점(몹 발~몸 가운데) 바로 아래 표면. 발 기준으로 재면 위 발판 몹이 아래 구간으로 잡힌다
        return SteerTo(ctx, goal, gx, dx, dy, ref intent);
    }

    public bool TravelTo(BotContext ctx, string map, ref BotIntent intent)
    {
        EnsureGraph(ctx);
        if (_g == null)
        {
            TravelFail = "그래프 없음";
            return false;
        }
        if (_hopFrom != _gMap || _hopTo != map)
        {
            _hopFrom = _gMap; // 맵·목적지가 바뀔 때만 계산(매 틱 사전·큐 할당 방지)
            _hopTo = map;
            _hopNext = BotMapRoute.NextHop(BotNavGraph.LoadAll(), _gMap, map);
        }
        string next = _hopNext;
        int pi = next != null ? _g.PortalTo(next, ctx.Obs.X) : -1;
        if (pi < 0)
        {
            TravelFail = next == null ? "맵 인접표에 길 없음 from=" + _gMap : "포털 없음 next=" + next;
            return false;
        }
        BotNavGraph.PortalRec p = _g.portals[pi];
        PlayObservation o = ctx.Obs;
        int cur = Standing(o) ? _g.SegAt(o.X, o.Terrain.FeetY) : -1;
        if (_edge < 0 && cur == p.seg && Mathf.Abs(o.X - p.x) < 0.4f)
        {
            intent.MoveX = 0;
            intent.MoveY = Tap(ctx.Now, 0.6f); // ↑ 톡(포털은 누른 순간만 본다). 맵 이동 횟수는 PlayBot 이 맵 이름 변화로 센다
            if (_watch.Check(ctx.Now, 0f) == BotNavWatch.Verdict.Stuck)
            {
                RecordStuck(ctx); // 포털이 안 탄다(봉인 등): 판단 층이 잠시 맵 이동을 쉰다
                TravelFail = "포털이 안 탄다 → " + p.toMap;
                return false;
            }
            return true;
        }
        bool steered = SteerTo(ctx, p.seg, p.x, p.x - o.X, 0f, ref intent);
        TravelFail = steered ? "" : "포털 쪽 걷기 실패 " + DebugState;
        return steered;
    }

    private void EnsureGraph(BotContext ctx)
    {
        string map = BgmDirector.CurrentMap;
        if (map == _gMap)
        {
            return;
        }
        _gMap = map;
        _g = BotNavGraph.Load(map);
        if (_g != null && !MatchesTerrain(_g))
        {
            Debug.LogWarning("[PB][Bot] 이동 그래프가 지형과 다름(다시 굽기: 메뉴 Tools/Bot/Bake Nav Graphs) map=" + map + " — 직진 이동으로 대신한다");
            _g = null;
        }
        _banUntil.Clear();
        _repeatCount.Clear();
        _repeatAt.Clear();
        Reset();
        _watch.Reset(ctx.Now);
    }

    private bool SteerTo(BotContext ctx, int goal, float gx, float dx, float dy, ref BotIntent intent)
    {
        bool ok = SteerToCore(ctx, goal, gx, dx, dy, ref intent);
        BotTrace.MarkEdge(_edge >= 0 && _g != null ? EdgeName(_g.edges[_edge].kind) : "walk", goal);
        return ok;
    }

    private static string EdgeName(int kind)
    {
        switch (kind)
        {
            case BotNavGraph.KindFall: return "drop";
            case BotNavGraph.KindJump: return "jump";
            case BotNavGraph.KindJumpHigh: return "jumphigh";
            case BotNavGraph.KindDownJump: return "down";
            case BotNavGraph.KindRope: return "rope";
            case BotNavGraph.KindPortal: return "portal";
        }
        return "walk";
    }

    private bool SteerToCore(BotContext ctx, int goal, float gx, float dx, float dy, ref BotIntent intent)
    {
        PlayObservation o = ctx.Obs;
        if (ctx.Now - _lastSteer > 0.5f)
        {
            _watch.Reset(ctx.Now); // 공격하느라 멈췄다 다시 걸을 때 옛 기준으로 막힘 판정하지 않게
        }
        _lastSteer = ctx.Now;
        _precise = _edge >= 0;
        _preciseAt = ctx.Now;
        if (_edge >= 0 && ContinueEdge(ctx, ref intent))
        {
            _watch.Progress(ctx.Now); // 간선은 제 시간 제한(Finish 실패 → 간선 빼고 다른 길)이 있다. 로프를 오르는 중 거리 지표로 끊지 않는다
            return true;
        }
        int cur = Standing(o) ? _g.SegAt(o.X, o.Terrain.FeetY) : -1;
        _dbgCur = cur;
        _dbgGoal = goal;
        if (cur < 0)
        {
            intent.MoveX = Mathf.Abs(dx) < 0.3f ? 0 : dx > 0f ? 1 : -1; // 공중·로프: 목표 쪽으로만
            if (o.OnRope)
            {
                RopeLoose(ctx, dx, dy, ref intent);
                return true;
            }
            _ropeSince = -1f;
            return Watch(ctx, Mathf.Abs(dx), ref intent);
        }
        if (goal < 0 || cur == goal)
        {
            _path = null;
            bool ok = _direct.Steer(ctx, dx, dy, ref intent);
            if (cur >= 0 && FaceAhead(_g.segs[cur], o.X, intent.MoveX))
            {
                intent.MoveX = 0; // 막힌 끝(턱·벽)에 몸을 붙이지 않는다 — 계단 모서리 고착(TechDebt 11-15)
                intent.Jump = false;
            }
            Watch(ctx, Mathf.Abs(dx), ref intent);
            return ok;
        }
        if (_path == null || cur != _pathFrom || goal != _pathGoal || ctx.Now - _planAt > ReplanSeconds)
        {
            Plan(ctx, cur, goal, gx);
        }
        if (_path == null || _path.Count == 0)
        {
            return NoPath(ctx, ref intent);
        }
        BotNavGraph.Edge e = _g.edges[_path[0]];
        float lo = e.ax0, hi = e.ax1;
        if ((e.kind == BotNavGraph.KindJump || e.kind == BotNavGraph.KindJumpHigh) && e.dir != 0 && hi - lo > 0.4f)
        {
            // 턱(벽)에 몸을 붙이지 않는 자리에서 뛴다
            if (e.dir > 0)
            {
                hi -= 0.25f;
            }
            else
            {
                lo += 0.25f;
            }
        }
        float tx = Mathf.Clamp(o.X, lo, hi);
        float tol = e.kind == BotNavGraph.KindRope ? 0.12f : e.kind == BotNavGraph.KindPortal ? 0.3f : 0.2f;
        _precise = Mathf.Abs(o.X - tx) < 1.5f;
        if (Mathf.Abs(o.X - tx) > tol)
        {
            intent.MoveX = tx > o.X ? 1 : -1;
            if (FaceAhead(_g.segs[cur], o.X, intent.MoveX))
            {
                intent.MoveX = 0;
            }
            return Watch(ctx, Mathf.Abs(tx - o.X), ref intent); // 경로가 바뀌면 Plan 이 다른 tx 를 준다 — 거리만 본다
        }
        if (RepeatBan(ctx.Now, _path[0]))
        {
            _path = null; // 같은 간선만 계속 반복 — 잠시 금지하고 다른 길을 찾는다
            return true;
        }
        _edge = _path[0];
        _edgeFrom = cur;
        _edgeStart = ctx.Now;
        _airJumped = false;
        _ropeCounted = false;
        _tick = 0;
        ContinueEdge(ctx, ref intent);
        return true;
    }

    public const float FaceGap = 0.3f;

    /// <summary>dir 쪽 구간 끝이 막힌 끝(벽·턱)이고 FaceGap 안인가</summary>
    public static bool FaceAhead(BotNavGraph.Seg s, float x, int dir)
    {
        if (dir > 0)
        {
            return !s.openR && s.X1 - x < FaceGap;
        }
        if (dir < 0)
        {
            return !s.openL && x - s.X0 < FaceGap;
        }
        return false;
    }

    public bool BlockedAhead(BotContext ctx, int dir)
    {
        EnsureGraph(ctx);
        PlayObservation o = ctx.Obs;
        int cur = _g != null && Standing(o) ? _g.SegAt(o.X, o.Terrain.FeetY) : -1;
        return cur >= 0 && FaceAhead(_g.segs[cur], o.X, dir);
    }

    /// <summary>서 있다: 접지 판정 또는 세로 속도 0(턱에 몸을 붙이면 게임 isOnGround 가 꺼지는 경우 — B1 막힘 점프 원인)</summary>
    public static bool Standing(PlayObservation o)
    {
        return !o.OnRope && (o.Grounded || Mathf.Abs(o.Vy) < 0.05f);
    }

    /// <summary>
    /// 굽은 그래프가 지금 지형과 맞나(맵 들어올 때 한 번): 구간 표면 몇 곳 바로 위에서 아래로 짧게 쏴 땅이 잡히는 비율. 낡은 파일을 조용히 쓰지 않게.
    /// 관측(레이캐스트 읽기)만 한다.
    /// </summary>
    public static bool MatchesTerrain(BotNavGraph g)
    {
        if (g.segs.Length == 0)
        {
            return false;
        }
        int mask = LayerMask.GetMask("Ground", "Platform", "Ground Soft", "Ground Hard", "Wall", "Boundary");
        int step = Mathf.Max(1, g.segs.Length / 12);
        int tried = 0, hit = 0;
        for (int i = 0; i < g.segs.Length; i += step)
        {
            BotNavGraph.Seg sg = g.segs[i];
            float x = (sg.X0 + sg.X1) * 0.5f;
            tried++;
            if (Physics2D.Raycast(new Vector2(x, sg.Y(x) + 0.4f), Vector2.down, 0.8f, mask).collider != null)
            {
                hit++;
            }
        }
        return hit >= tried * 0.7f;
    }

    /// <summary>
    /// 간선 없이 로프에 매달림(간선 실패·목표 바뀜): 목표가 아래 0.5 보다 깊지 않으면 위로(위 끝을 지나 발판에 선다), 아니면 아래로.
    /// RopeHang 초 넘게 매달려 있으면 목표 쪽으로 뛰어내린다(위 끝에 발판이 없는 로프에서 오르내리며 떨던 막힘 — 11-15 NormalForest x=-319.3).
    /// </summary>
    private void RopeLoose(BotContext ctx, float dx, float dy, ref BotIntent intent)
    {
        if (_ropeSince < 0f)
        {
            _ropeSince = ctx.Now;
        }
        if (ctx.Now - _ropeSince > RopeHang)
        {
            intent.MoveX = dx >= 0f ? 1 : -1;
            intent.MoveY = 0;
            intent.Jump = true;
            intent.JumpHold = 0.2f;
            _ropeSince = ctx.Now;
            return;
        }
        intent.MoveX = 0;
        intent.MoveY = dy > -0.5f ? 1 : -1;
    }

    private void Plan(BotContext ctx, int cur, int goal, float gx)
    {
        _banned.Clear();
        foreach (KeyValuePair<int, float> kv in _banUntil)
        {
            if (kv.Value > ctx.Now)
            {
                _banned.Add(kv.Key);
            }
        }
        PlayerController pc = PlayerController.instance;
        bool high = pc != null && pc.MovementJumpLimit >= 2;
        _path = _g.FindPath(cur, ctx.Obs.X, goal, gx, high, _banned);
        _pathFrom = cur;
        _pathGoal = goal;
        _planAt = ctx.Now;
    }

    private bool NoPath(BotContext ctx, ref BotIntent intent)
    {
        intent.MoveX = 0;
        _path = null;
        if (_banned.Count > 0 && ctx.Now - _lastNoPathStuck > BanSeconds)
        {
            _lastNoPathStuck = ctx.Now; // 실패한 간선을 빼고 나니 길이 없다 = 다른 경로도 막힘
            RecordStuck(ctx);
        }
        return false; // 못 가는 목표: 판단 층이 다른 목표를 고른다
    }

    /// <summary>진행 중인 간선을 한 틱 이어 간다. 끝났으면(성공·실패) false</summary>
    private bool ContinueEdge(BotContext ctx, ref BotIntent intent)
    {
        PlayObservation o = ctx.Obs;
        BotNavGraph.Edge e = _g.edges[_edge];
        float t = ctx.Now - _edgeStart;
        int cur = Standing(o) ? _g.SegAt(o.X, o.Terrain.FeetY) : -1;
        int tick = _tick++;
        if (t > 0.3f && cur >= 0 && cur != _edgeFrom)
        {
            return Finish(ctx, true); // 다른 구간에 섰다(목표 구간을 지나쳐도 길은 다시 찾는다). 제자리면 실패
        }
        float limit = e.kind == BotNavGraph.KindRope ? 4f + Mathf.Abs(_g.segs[e.to].Y(e.lx) - _g.segs[e.from].Y(e.lx)) / 4f : 3f;
        if (t > limit || (t > 1.6f && cur == _edgeFrom && e.kind != BotNavGraph.KindRope && e.kind != BotNavGraph.KindPortal))
        {
            return Finish(ctx, false);
        }
        switch (e.kind)
        {
            case BotNavGraph.KindFall:
                intent.MoveX = e.dir;
                break;
            case BotNavGraph.KindJump:
            case BotNavGraph.KindJumpHigh:
                JumpStep(o, e, tick, t, ref intent);
                break;
            case BotNavGraph.KindDownJump:
                intent.MoveY = -1;
                intent.Jump = tick >= 1 && o.Grounded;
                intent.JumpHold = 0.15f;
                if (tick == 1)
                {
                    DownJumps++;
                }
                break;
            case BotNavGraph.KindRope:
                RopeStep(o, e, tick, t, ref intent);
                break;
            case BotNavGraph.KindPortal:
                intent.MoveX = 0;
                intent.MoveY = Tap(t, 0.6f);
                if (tick == 0)
                {
                    Portals++;
                }
                break;
        }
        return true;
    }

    /// <summary>
    /// 점프: 서 있는 동안은 좌우를 떼고 점프만 요청한다(턱에 몸을 붙인 채 밀면 계단 모서리에서 게임 접지 판정이 꺼져 남은 점프가 0 이 된다 — B1 막힘 점프 원인).
    /// 0.5초 안에 못 뜨면 0.3초 턱 반대로 물러섰다가 다시 뛴다. 뜬 뒤에는 도착 x 쪽으로 누른다. 높은 점프는 꼭대기 근처에서 한 번 더.
    /// </summary>
    private void JumpStep(PlayObservation o, BotNavGraph.Edge e, int tick, float t, ref BotIntent intent)
    {
        if (Standing(o))
        {
            bool backOff = t >= 0.5f && t < 0.8f;
            intent.MoveX = backOff ? -e.dir : 0;
            if (!backOff)
            {
                intent.Jump = true; // 뜰 때까지 계속 요청(점프 쿨에 한 번 걸려도 놓치지 않게)
                intent.JumpHold = JumpHold;
            }
            if (tick == 0)
            {
                Jumps++;
            }
            return;
        }
        intent.MoveX = e.sameX ? 0 : (Mathf.Abs(e.lx - o.X) < 0.3f ? 0 : e.lx > o.X ? 1 : -1);
        if (e.kind == BotNavGraph.KindJumpHigh && !_airJumped && o.Vy < 1.5f)
        {
            _airJumped = true;
            intent.Jump = true;
            intent.AirJump = true;
            intent.JumpHold = JumpHold;
        }
    }

    /// <summary>주기 period 마다 0.1초 누름(1) — 판단 빈도(10Hz·매 프레임)와 무관한 톡</summary>
    public static int Tap(float t, float period)
    {
        return t % period < 0.1f ? 1 : 0;
    }

    private void RopeStep(PlayObservation o, BotNavGraph.Edge e, int tick, float t, ref BotIntent intent)
    {
        float targetY = _g.segs[e.to].Y(e.lx);
        int dirY = targetY > _g.segs[e.from].Y(e.lx) ? 1 : -1;
        intent.MoveX = 0;
        if (o.OnRope)
        {
            intent.MoveY = dirY; // 누른 채로 오르내린다 — 위 끝을 지나면 트리거를 벗어나 발판에 선다
            if (!_ropeCounted)
            {
                _ropeCounted = true;
                Ropes++;
            }
            return;
        }
        if (_ropeCounted && RopeArrived(o.Terrain.FeetY, targetY, dirY))
        {
            return; // 로프 위 끝을 벗어난 뒤: 목표 발판 높이 근처면 점프·↑ 없이 그냥 내려앉는다(Grounded 가 잠깐 참이라 다시 뛰어 로프 끝~최고점을 오가던 막힘, 11-16 SideRoad 로프0)
        }
        if (Mathf.Abs(o.X - e.lx) > 0.25f && o.Grounded)
        {
            intent.MoveX = e.lx > o.X ? 1 : -1;
            return;
        }
        if (o.NearLadder)
        {
            intent.MoveY = Tap(t, 0.2f) * dirY; // ↑/↓ 톡(잡기는 누른 순간만 본다)
            return;
        }
        if (dirY > 0 && o.Grounded && t > 0.3f)
        {
            intent.Jump = true; // 로프 밑이 머리보다 높다 → 뛰어서 잡는다
            intent.JumpHold = 0.3f;
        }
        intent.MoveY = !o.Grounded ? Tap(t, 0.2f) * dirY : 0;
    }

    /// <summary>로프를 탄 뒤 로프에서 벗어났고, 발이 목표 발판 높이(위로 가면 0.6 아래까지, 아래로 가면 0.6 위까지)에 닿았다</summary>
    public static bool RopeArrived(float feetY, float targetY, int dirY)
    {
        return dirY > 0 ? feetY >= targetY - 0.6f : feetY <= targetY + 0.6f;
    }

    public const int RepeatLimit = 5;
    public const float RepeatWindow = 30f;
    public const float RepeatBanSeconds = 15f;
    private readonly Dictionary<int, int> _repeatCount = new Dictionary<int, int>();
    private readonly Dictionary<int, float> _repeatAt = new Dictionary<int, float>();

    /// <summary>같은 간선을 30초 안에 5번째 시작하면 15초 금지(SideRoad 로프0 위·아래 점프·낙하 반복, 11-16)</summary>
    private bool RepeatBan(float now, int edge)
    {
        _repeatCount.TryGetValue(edge, out int n);
        _repeatAt.TryGetValue(edge, out float at);
        n = now - at > RepeatWindow ? 1 : n + 1;
        _repeatAt[edge] = now;
        if (n < RepeatLimit)
        {
            _repeatCount[edge] = n;
            return false;
        }
        _repeatCount[edge] = 0;
        _banUntil[edge] = now + RepeatBanSeconds;
        EdgeRepeatBans++;
        return true;
    }

    public int EdgeRepeatBans { get; private set; }

    private bool Finish(BotContext ctx, bool ok)
    {
        if (ok)
        {
            _watch.Progress(ctx.Now);
        }
        else
        {
            EdgeFails++;
            _banUntil[_edge] = ctx.Now + BanSeconds; // 다른 길로
        }
        _edge = -1;
        _path = null;
        return false;
    }

    private bool Watch(BotContext ctx, float metric, ref BotIntent intent)
    {
        if (ctx.Obs.Flags.IndexOf('M') >= 0)
        {
            _watch.Progress(ctx.Now); // 공격·스킬 이동 잠금 중에는 못 움직이는 게 정상
            return true;
        }
        BotNavWatch.Verdict v = _watch.Check(ctx.Now, metric, ctx.Obs.X);
        if (v == BotNavWatch.Verdict.None)
        {
            return true;
        }
        if (_path != null && _path.Count > 0)
        {
            _banUntil[_path[0]] = ctx.Now + BanSeconds;
        }
        _edge = -1;
        _path = null;
        if (v == BotNavWatch.Verdict.Reroute)
        {
            Reroutes++;
            return true;
        }
        RecordStuck(ctx);
        return false;
    }

    private void RecordStuck(BotContext ctx)
    {
        Stucks++;
        PlayObservation o = ctx.Obs;
        LastStuck = _gMap + " x=" + o.X.ToString("F1") + " y=" + o.Y.ToString("F1");
        bool shot = false;
        if (ctx.Now - _lastShot > StuckShotGap)
        {
            _lastShot = ctx.Now;
            shot = true;
            PlayRecorder.RequestBookmark(); // 기록기 북마크(bm 행 + mark_NN.png) — 화면 캡처는 읽기
        }
        Debug.LogWarning("[PB][Bot] 막힘 " + LastStuck + " grounded=" + o.Grounded + " rope=" + o.OnRope + (shot ? " bookmark" : ""));
    }
}
