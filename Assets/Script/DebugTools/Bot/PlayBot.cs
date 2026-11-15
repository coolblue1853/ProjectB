using UnityEngine;

/// <summary>
/// 자동 플레이 봇(B1 뼈대). 인지(PlayObservation, 화면 수준) → 판단(IBotBrain, 10Hz) → 입력(BotInput, 진짜 가상 장치) 루프.
/// 금지: transform·rigidbody 조작, 게임 함수 호출(공격·이동·포털·상점), 치트·시간 조작. 게임 이벤트는 읽기 구독만(처치 수 세기).
/// 켜기: <see cref="Run"/> (프로브 bot_start.cs, SRDebugger "Bot" 묶음), 끄기: <see cref="Stop"/>. 실행 중 기록 출처는 "bot"(Logs/playrec_bot/).
/// 설계는 PlayBotDesign.md 10절.
/// </summary>
public sealed class PlayBot : MonoBehaviour
{
    public const float DecideInterval = 0.1f;     // 10Hz
    public const float FallbackReaction = 0.15f;  // 프로필 react.idle 이 비었을 때
    public const float JumpCooldown = 0.6f;
    public const float WarnInterval = 3f;
    public const string GoalHunt = "hunt";

    private static PlayBot _inst;

    private readonly PlayObservation _obs = new PlayObservation();
    private readonly BotContext _ctx = new BotContext();
    private BotInput _input;
    private IBotBrain _brain;
    private DirectNavigator _nav;
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
    private static int _lastKills;
    private static string _lastStatus = "idle";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _inst = null; // 도메인 리로드를 끈 플레이 모드 진입에서 이전 판 값이 남지 않게
        Warns = 0;
        _lastKills = 0;
        _lastStatus = "idle";
    }

    /// <summary>봇을 켠다. minutes = 실제 시간(분), goal = "hunt"(B1 은 이것만). 플레이어가 없으면 false</summary>
    public static bool Run(float minutes, string goal = GoalHunt)
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
            b.Begin(Mathf.Max(0.1f, minutes), string.IsNullOrEmpty(goal) ? GoalHunt : goal);
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

    private void Begin(float minutes, string goal)
    {
        Warns = 0;
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
        PlayRecorder.Restart("bot");
        _input = new BotInput(_ctx.Profile, BotInput.Route.Keyboard, seed + 1);
        _rhythm = new BotAttackRhythm(_ctx.Profile, _ctx.Rng);
        _nav = new DirectNavigator();
        _brain = new HuntBrain(_nav);
        _obs.ForceRescan();
        _obs.Observe();
        _brain.Begin(_ctx);
        _startReal = Time.unscaledTime;
        _endReal = _startReal + minutes * 60f;
        EnemyHealth.Killed += OnKilled;
        Debug.Log("[PB][Bot] 시작 " + minutes + "분 goal=" + goal + " seed=" + seed + " profile n=" + _ctx.Profile.dists.Count);
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
        if (now >= _nextDecide)
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
        BotIntent want = _brain.Decide(_ctx);
        if (want.Back)
        {
            _input.Press("UI/Back");
        }
        if (want.Jump && _obs.Grounded && now - _lastJump > JumpCooldown)
        {
            _lastJump = now;
            _input.Press("Jump");
        }
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
        WarnIfStuck(now);
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
        HuntBrain hb = _brain as HuntBrain;
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
        return "running=1 sec=" + (Time.unscaledTime - _startReal).ToString("F0") + " kills=" + _ctx.Kills + " warns=" + Warns
            + " stuckJumps=" + (_nav != null ? _nav.StuckJumps : 0) + " reverses=" + (_nav != null ? _nav.Reverses : 0)
            + " do=" + _current.Note + " map=" + BgmDirector.CurrentMap;
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
