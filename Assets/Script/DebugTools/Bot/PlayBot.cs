using UnityEngine;

/// <summary>
/// 자동 플레이 봇(B1 뼈대 → B2 이동 그래프·전투). 인지(PlayObservation, 화면 수준) → 판단(IBotBrain, 10Hz) → 입력(BotInput, 진짜 가상 장치) 루프.
/// 금지: transform·rigidbody 조작, 게임 함수 호출(공격·이동·포털·상점), 치트·시간 조작. 게임 이벤트는 읽기 구독만(처치 수 세기).
/// 켜기: <see cref="Run"/> (프로브 bot_start.cs, SRDebugger "Bot" 묶음), 끄기: <see cref="Stop"/>. 실행 중 기록 출처는 "bot"(Logs/playrec_bot/).
/// 설계는 PlayBotDesign.md 10·11절.
/// </summary>
public sealed class PlayBot : MonoBehaviour
{
    public const float DecideInterval = 0.1f;     // 10Hz
    public const float FallbackReaction = 0.15f;  // 프로필 react.idle 이 비었을 때
    public const float JumpCooldown = 0.6f;
    public const float WarnInterval = 3f;
    public const string GoalHunt = "hunt";
    /// <summary>기본 사냥 순회(1층계 로프·포털이 있는 맵 3장, 왕복)</summary>
    public const string DefaultMaps = "Lv1_NearForest_SideRoad,Lv1_NormalForest,Lv1_DeepForest";

    private static PlayBot _inst;

    private readonly PlayObservation _obs = new PlayObservation();
    private readonly BotContext _ctx = new BotContext();
    private BotInput _input;
    private IBotBrain _brain;
    private GraphNavigator _nav;
    private HuntBrain _hunt;
    private string _lastMap;
    private bool _wasDead;
    private int _downArm;
    private BotAttackRhythm _rhythm;
    private BotIntent _current;
    private BotIntent _pending;
    private bool _hasPending;
    private float _pendingAt;
    private float _nextDecide;
    private float _lastJump = -10f;
    private float _lastWarn = -10f;
    private float _endReal;
    private float _startReal;
    private bool _resumeHuman = true;

    public static bool IsRunning => _inst != null;
    public static int Kills => _inst != null ? _inst._ctx.Kills : _lastKills;
    public static int Warns { get; private set; }
    public static int MapMoves { get; private set; }
    public static int Deaths { get; private set; }
    private static int _lastKills;
    private static string _lastStatus = "idle";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _inst = null; // 도메인 리로드를 끈 플레이 모드 진입에서 이전 판 값이 남지 않게
        Warns = 0;
        MapMoves = 0;
        Deaths = 0;
        _lastKills = 0;
        _lastStatus = "idle";
    }

    /// <summary>봇을 켠다. minutes = 실제 시간(분), goal = "hunt", maps = 사냥 순회 맵(쉼표, 비면 DefaultMaps · "-" 면 맵 이동 없음). 플레이어가 없으면 false</summary>
    public static bool Run(float minutes, string goal = GoalHunt, string maps = null)
    {
        if (_inst != null || BotReplay.IsRunning)
        {
            return false; // 가상 키보드는 하나만
        }
        if (PlayerController.instance == null)
        {
            Debug.LogWarning("[PB][Bot] 플레이어가 없어 시작하지 않음");
            return false;
        }
        GameObject go = new GameObject("PlayBot");
        DontDestroyOnLoad(go);
        PlayBot b = go.AddComponent<PlayBot>();
        try
        {
            b.Begin(Mathf.Max(0.1f, minutes), string.IsNullOrEmpty(goal) ? GoalHunt : goal, maps);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PB][Bot] 시작 실패: " + e.Message);
            b.Teardown();
            Destroy(go);
            return false;
        }
        _inst = b; // 다 만든 뒤에만 실행 중으로 본다
        return true;
    }

    /// <summary>봇을 끈다. resumeHuman = 사람 기록 세션으로 되돌린다(bot_run.sh 는 곧 플레이를 끄므로 false — 사람 폴더에 빈 세션을 만들지 않게)</summary>
    public static void Stop(bool resumeHuman = true)
    {
        if (_inst == null)
        {
            return;
        }
        PlayBot b = _inst;
        b._resumeHuman = resumeHuman;
        b.Cleanup();
        Destroy(b.gameObject);
    }

    public static string Status()
    {
        if (_inst == null)
        {
            return _lastStatus;
        }
        return _inst.Describe();
    }

    private void Begin(float minutes, string goal, string maps)
    {
        Warns = 0;
        MapMoves = 0;
        Deaths = 0;
        if (goal != GoalHunt)
        {
            Debug.LogWarning("[PB][Bot] B1 은 hunt 만 지원 — " + goal + " 대신 hunt");
            goal = GoalHunt;
        }
        int seed = System.Environment.TickCount;
        _ctx.Profile = BotProfile.Load();
        _ctx.Rng = new System.Random(seed);
        _ctx.Obs = _obs;
        _ctx.Goal = goal;
        _ctx.Kills = 0;
        _ctx.Now = Time.time;
        _ctx.Maps = maps == "-" ? null : (string.IsNullOrEmpty(maps) ? DefaultMaps : maps).Split(',');
        PlayRecorder.Restart("bot");
        _input = new BotInput(_ctx.Profile, BotInput.Route.Keyboard, seed + 1);
        _rhythm = new BotAttackRhythm(_ctx.Profile, _ctx.Rng);
        _nav = new GraphNavigator();
        _hunt = new HuntBrain(_nav);
        _brain = _hunt;
        _obs.ForceRescan();
        _obs.Observe();
        _brain.Begin(_ctx);
        _lastMap = BgmDirector.CurrentMap;
        _startReal = Time.unscaledTime;
        _endReal = _startReal + minutes * 60f;
        EnemyHealth.Killed += OnKilled;
        Debug.Log("[PB][Bot] 시작 " + minutes + "분 goal=" + goal + " seed=" + seed + " profile n=" + _ctx.Profile.dists.Count + " maps=" + maps + " graph=" + (BotNavGraph.Load(_lastMap) != null));
    }

    private void OnKilled(EnemyHealth e)
    {
        _ctx.Kills++;
    }

    private void Update()
    {
        if (_input == null)
        {
            return;
        }
        if (Time.unscaledTime >= _endReal)
        {
            Stop(_resumeHuman);
            return;
        }
        float now = Time.time;
        _ctx.Now = now;
        if (now >= _nextDecide || (_nav != null && _nav.Precise))
        {
            _nextDecide = now + DecideInterval;
            Decide(now);
        }
        if (_hasPending && now >= _pendingAt)
        {
            _hasPending = false;
            Commit(_pending, now);
        }
        if (_current.Attack && _rhythm.Due(now))
        {
            _input.Press("Attack");
        }
        _input.Tick(now);
    }

    private void Decide(float now)
    {
        _obs.Observe();
        if (!_obs.HasPlayer)
        {
            return;
        }
        TrackMapAndDeath();
        BotIntent want = _brain.Decide(_ctx);
        if (want.Back)
        {
            _input.Press("UI/Back");
        }
        PressJump(want, now);
        if (want.Focus != _current.Focus && !(_hasPending && want.Focus == _pending.Focus))
        {
            // 새 목표: 사람 반응 시간만큼 늦게 바꾼다
            _pending = want;
            _hasPending = true;
            _pendingAt = now + _ctx.Profile.Sample("react.idle", _ctx.Rng, FallbackReaction);
        }
        else if (_hasPending)
        {
            _pending = want;
        }
        else if (!want.SameAction(_current))
        {
            Commit(want, now);
        }
        PressActions(want);
        WarnIfStuck(now);
    }

    /// <summary>점프: 땅·로프에서(AirJump 면 공중도). 하향 점프는 ↓ 가 먼저 눌린 다음 틱에 누른다(게임이 점프 순간의 ↓ 를 본다)</summary>
    private void PressJump(BotIntent want, float now)
    {
        if (!want.Jump)
        {
            _downArm = 0;
            return;
        }
        if (want.MoveY < 0 && (_input.StickValue.y > -0.5f || _downArm++ < 1))
        {
            _input.Stick(want.MoveX, want.MoveY); // ↓ 를 먼저(반응 지연 없이) 누른다
            return;
        }
        bool canJump = _obs.Grounded || _obs.OnRope || want.AirJump || Mathf.Abs(_obs.Vy) < 0.05f; // 턱에 붙어 isOnGround 가 꺼져도 게임은 TouchingFloor 로 점프를 받는다
        if (canJump && (want.AirJump || now - _lastJump > JumpCooldown))
        {
            _lastJump = now;
            _downArm = 0;
            _input.Press("Jump", want.JumpHold > 0f ? want.JumpHold : -1f);
        }
    }

    /// <summary>대시·스킬·퀵슬롯: 이동 축을 반영한 뒤 한 번씩 누른다(대시 방향 = 바라보는 쪽)</summary>
    private void PressActions(BotIntent want)
    {
        if (want.Dash)
        {
            _input.Stick(want.MoveX, want.MoveY);
            _input.Press("Dash");
        }
        if (want.Skill > 0 && want.Skill <= BotSkill.Actions.Length)
        {
            _input.Press(BotSkill.Actions[want.Skill - 1]);
        }
        if (want.Quick > 0 && want.Quick <= QuickActions.Length)
        {
            _input.Press(QuickActions[want.Quick - 1]);
        }
    }

    private static readonly string[] QuickActions = { "Quick1", "Quick2", "Quick3", "Quick4" };

    /// <summary>맵 이동·사망을 관측으로 센다(맵 이름 변화, HP 0 진입)</summary>
    private void TrackMapAndDeath()
    {
        string map = BgmDirector.CurrentMap;
        if (map != _lastMap)
        {
            MapMoves++;
            Debug.Log("[PB][Bot] 맵 이동 " + _lastMap + " -> " + map + " graph=" + (BotNavGraph.Load(map) != null));
            _lastMap = map;
        }
        bool dead = _obs.HasHealth && _obs.Hp <= 0;
        if (dead && !_wasDead)
        {
            Deaths++;
            Debug.LogWarning("[PB][Bot] 사망 map=" + map + " x=" + _obs.X.ToString("F1"));
        }
        _wasDead = dead;
    }

    private void Commit(BotIntent it, float now)
    {
        if (!it.Attack)
        {
            _rhythm.Reset();
        }
        _current = it;
        _input.Stick(it.MoveX, it.MoveY);
    }

    private void WarnIfStuck(float now)
    {
        HuntBrain hb = _hunt;
        if (hb == null || !hb.NoProgress || now - _lastWarn < WarnInterval)
        {
            return;
        }
        _lastWarn = now;
        Warns++;
        Debug.LogWarning("[PB][Bot] 진척 없음 3초+ map=" + BgmDirector.CurrentMap + " x=" + _obs.X.ToString("F1") + " y=" + _obs.Y.ToString("F1") + " do=" + hb.LastNote);
    }

    private string Describe()
    {
        string s = "running=1 sec=" + (Time.unscaledTime - _startReal).ToString("F0") + " kills=" + _ctx.Kills + " deaths=" + Deaths
            + " mapMoves=" + MapMoves + " warns=" + Warns + " do=" + _current.Note + " map=" + BgmDirector.CurrentMap;
        if (_nav != null)
        {
            s += " stucks=" + _nav.Stucks + " reroutes=" + _nav.Reroutes + " edgeFails=" + _nav.EdgeFails + " stuckJumps=" + _nav.StuckJumps
                + " reverses=" + _nav.Reverses + " navJumps=" + _nav.Jumps + " downJumps=" + _nav.DownJumps + " ropes=" + _nav.Ropes + " portals=" + _nav.Portals
                + " graph=" + (_nav.HasGraph ? 1 : 0) + " " + _nav.DebugState + (_nav.Stucks > 0 ? " lastStuck=" + _nav.LastStuck.Replace(' ', '_') : "");
        }
        if (_hunt != null)
        {
            s += " skills=" + _hunt.Skills + " dashes=" + _hunt.Dashes + " avoidMoves=" + _hunt.Avoids + " avoidWindows=" + _hunt.AvoidWindows + " potions=" + _hunt.Potions
                + " range=" + _hunt.Range.ToString("F1");
        }
        return s;
    }

    /// <summary>정상 정지: 키를 다 떼고 장치 제거, 기록 출처 되돌리기, 로그</summary>
    private void Cleanup()
    {
        if (_input == null)
        {
            Teardown();
            return;
        }
        _lastKills = _ctx.Kills;
        _lastStatus = Describe().Replace("running=1", "running=0");
        _input.ReleaseAll();
        Teardown();
        if (_resumeHuman)
        {
            PlayRecorder.Restart("human");
        }
        Debug.Log("[PB][Bot] 정지 " + _lastStatus);
    }

    /// <summary>최소 정리(구독 해제·장치 제거). 플레이 종료 중에도 안전한 것만 한다</summary>
    private void Teardown()
    {
        EnemyHealth.Killed -= OnKilled;
        if (_input != null)
        {
            _input.Dispose();
            _input = null;
        }
        if (_inst == this)
        {
            _inst = null;
        }
    }

    private void OnDestroy()
    {
        Teardown(); // 플레이 종료·씬 정리: 새 기록 세션·로그·다른 싱글턴 조회 없이 장치만 치운다
    }
}
