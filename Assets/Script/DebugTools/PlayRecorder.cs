using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// 플레이 기록기: 한 줄 = JSON 한 객체(jsonl). 사람 플레이를 남겨 통계를 내고 "진짜 입력만 쓰는 봇"의 학습 자료로 쓴다.
/// 이 클래스는 생명주기·스케줄·정적 창구만 맡는다. 파일 = PlayRecWriter, 입력 = PlayRecInput, 게임 이벤트·훅 = PlayRecHooks,
/// 영상 = PlayRecVideo, 북마크 = PlayRecBookmark, 회피·사람 신호 = PlayRecSignals, 경고 = PlayRecWarn, 성능 = PlayRecPerf, 관측 = PlayObservation.
/// 형식표는 PlayBotDesign.md. 게임 로직은 바꾸지 않는다 — 읽기와 이벤트 구독만 한다. 플레이 모드·빌드 모두 기본 켜짐(Enabled 로 끈다).
/// </summary>
public sealed class PlayRecorder : MonoBehaviour
{
    /// <summary>정적 스위치. 부트스트랩 전에 false 로 두면 생기지 않고, 실행 중 false 로 바꾸면 기록만 멈춘다.</summary>
    public static bool Enabled = true;
    /// <summary>영상 녹화 스위치(부트스트랩 전에 바꾼다). false 면 jsonl 만 남긴다.</summary>
    public static bool VideoEnabled = true;
    /// <summary>"human" | "bot". 봇이 같은 형식으로 남길 때 바꾼다.</summary>
    public static string Source = "human";

    public const int TickHz = 20;
    private const float TickInterval = 1f / TickHz;
    private const float FlushInterval = 1f;
    private const int GridSeenMax = 20000;

    private static PlayRecorder _inst;
    private static int _mainThreadId;

    public static string FilePath => PlayRecWriter.FilePath;
    /// <summary>영상 파일 경로(녹화 중이 아니면 null)</summary>
    public static string VideoPath => _inst != null && _inst._video.Active ? _inst._video.Path : null;

    private readonly PlayObservation _obs = new PlayObservation();
    private readonly PlayRecStuck _stuck = new PlayRecStuck();
    private readonly PlayRecInput _input = new PlayRecInput();
    private readonly PlayRecHooks _hooks = new PlayRecHooks();
    private readonly PlayRecSignals _signals = new PlayRecSignals();
    private readonly PlayRecWarn _warn = new PlayRecWarn();
    private readonly PlayRecPerf _perf = new PlayRecPerf();
    private readonly PlayRecVideo _video = new PlayRecVideo();
    private readonly PlayRecBookmark _bookmark = new PlayRecBookmark();
    private readonly HashSet<uint> _gridsWritten = new HashSet<uint>();
    private readonly int[] _statScratch = new int[PlayRecHooks.StatCount];
    private readonly WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();
    private long _tick;
    private float _nextTick;
    private float _nextFlush;
    private float _nextContext;
    private float _nextVf;
    private float _nextMapProbe;
    private float _startReal;
    private int _ctxSig;
    private int _snapErrors;
    private bool _duplicate;
    private bool _metaWritten;
    private bool _shutDown;
    private bool _anyHeld;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        if (_inst != null)
        {
            _inst.Shutdown(); // 도메인 리로드를 끈 경우 이전 플레이의 파일·ffmpeg 를 먼저 닫는다
        }
        PlayRecWriter.ResetStatics();
        _inst = null;
        Enabled = true;
        VideoEnabled = true;
        Source = "human";
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (!Enabled || _inst != null)
        {
            return;
        }
        GameObject go = new GameObject("PlayRecorder");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<PlayRecorder>();
    }

    // ───────────── 공개 API(정적, 게임 코드 훅) ─────────────

    private static bool On => Enabled && PlayRecWriter.IsOpen;
    /// <summary>기록 중이고 인스턴스가 있을 때만 훅 대상(아니면 null — 호출이 무시된다)</summary>
    private static PlayRecHooks LiveHooks => On && _inst != null ? _inst._hooks : null;

    /// <summary>게임 이벤트 한 줄(k:"ev"). a·b·v 는 있을 때만 쓴다(v 는 int.MinValue 면 생략). 결정 종류는 k:"dec" 도 남는다.</summary>
    public static void Event(string name, string a = null, int v = int.MinValue, string b = null)
    {
        if (!On)
        {
            return;
        }
        PlayRecHooks.WriteEvent(name, a, v, b);
        if (_inst != null && Thread.CurrentThread.ManagedThreadId == _mainThreadId)
        {
            _inst._hooks.OnEvent(name, a, v, b);
        }
    }

    /// <summary>플레이어가 준 피해(EnemyHealth.damage2Enemy). kb = 넉백 힘(슈퍼아머면 0), cb = 콤보 단계·hc = 히트 카운터는 여기서 읽는다.</summary>
    public static void Dealt(EnemyHealth target, int amount, bool skill, bool crit, float knock = 0f)
    {
        if (On)
        {
            PlayRecHooks.WriteDealt(target, amount, skill, crit, knock); // 꺼져 있으면 몹 id 문자열도 만들지 않는다
        }
    }

    /// <summary>공격 쪽이 damage2Player 직전에 부른다(EnemyDamageObject·ContactDamage)</summary>
    public static void HitBy(GameObject attacker) => LiveHooks?.HitBy(attacker);

    /// <summary>레벨 차로 몹 공격이 빗나갔다</summary>
    public static void HitMiss(GameObject attacker) => LiveHooks?.HitMiss(attacker);

    /// <summary>PlayerHealthManager.damage2Player 첫 줄. hurtInvuln = 피격 후 무적 중, invincible = 대시·스킬 무적</summary>
    public static void HitAttempt(int damage, bool hurtInvuln, bool invincible) => LiveHooks?.HitAttempt(damage, hurtInvuln, invincible);

    /// <summary>방어 판정 직후(PlayerGuard.TryIntercept)</summary>
    public static void Guarded(bool guarded, bool parried) => LiveHooks?.Guarded(guarded, parried);

    /// <summary>GameLog.Info 미러(k:"log"). cat 은 GameLog.Cat 값.</summary>
    public static void Log(int cat, string message)
    {
        if (On)
        {
            PlayRecHooks.WriteLog(cat, message);
        }
    }

    // ───────────── 생명주기 ─────────────

    private void Awake()
    {
        if (_inst != null && _inst != this)
        {
            _duplicate = true; // OnEnable 에서 정적 이벤트를 이중 구독하지 않는다
            Destroy(gameObject);
            return;
        }
        _inst = this;
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        try
        {
            PlayRecWriter.Open();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PB][PlayRec] 기록 파일을 열지 못해 꺼짐: " + e.Message);
            Enabled = false;
            return;
        }
        _startReal = Time.unscaledTime;
        PadFieldInput.SelectR3ChordEnabled = true; // 북마크 패드 조합은 기록 중에만
        _signals.Reset(_startReal);
        _perf.Reset(_startReal);
        _hooks.Threat = _signals.Threat;
        _hooks.MapChanged = OnMapChanged;
        if (VideoEnabled)
        {
            _video.Start(System.IO.Path.Combine(PlayRecWriter.Dir, PlayRecWriter.Session + ".mp4"));
        }
#if UNITY_EDITOR
        UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ShutdownBeforeReload;
#endif
        StartCoroutine(EndOfFrameLoop());
    }

    private void OnEnable()
    {
        if (_duplicate)
        {
            return;
        }
        _input.Subscribe();
        _input.Pressed += OnPressed;
        _warn.Subscribe();
        _hooks.Subscribe();
    }

    private void OnPressed(string action, int moveSign)
    {
        _signals.OnPressed(action, moveSign, Time.time, Time.unscaledTime, _obs.X);
    }

    private void OnDisable()
    {
        if (_duplicate)
        {
            return;
        }
        _input.Unsubscribe();
        _input.Pressed -= OnPressed;
        _warn.Unsubscribe();
        _hooks.Unsubscribe();
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    private void OnDestroy()
    {
        if (_inst == this)
        {
            Shutdown();
            _inst = null;
        }
    }

    /// <summary>도메인 리로드 직전: 파이프 스레드가 중단되기 전에 영상 마무리를 기다린다(최대 3초)</summary>
    private void ShutdownBeforeReload()
    {
        Shutdown(true);
    }

    /// <summary>영상(ffmpeg 정상 종료)·파일을 닫는다. 플레이 정지·앱 종료·도메인 리로드 어디서 불려도 한 번만 일한다.
    /// block=false 면 영상 마무리는 파이프 스레드에 맡기고 메인 스레드는 기다리지 않는다.</summary>
    private void Shutdown(bool block = false)
    {
        if (_shutDown)
        {
            return;
        }
        _shutDown = true;
        PadFieldInput.SelectR3ChordEnabled = false;
#if UNITY_EDITOR
        UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= ShutdownBeforeReload;
#endif
        try
        {
            WriteVideoMark(true);
            _video.Stop(block);
            _bookmark.Release();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PB][PlayRec] 종료 중 오류: " + e.Message);
        }
        PlayRecWriter.Close();
    }

    private IEnumerator EndOfFrameLoop()
    {
        while (true)
        {
            yield return _endOfFrame;
            if (On && !_shutDown)
            {
                _video.CaptureIfDue(Time.unscaledTime);
                _bookmark.CaptureIfQueued();
            }
        }
    }

    private void Update()
    {
        if (!On || _shutDown)
        {
            return;
        }
        float now = Time.time;
        float real = Time.unscaledTime;
        PlayRecWriter.Tick(now, real);
        _perf.Feed(real, Time.unscaledDeltaTime);
        _hooks.Flush(_statScratch);
        if (real >= _nextTick)
        {
            _nextTick = real - _nextTick > 0.25f ? real + TickInterval : _nextTick + TickInterval;
            Tick(now, real);
        }
        if (PlayRecBookmark.Pressed())
        {
            try
            {
                Bookmark(real);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[PB][PlayRec] 북마크 실패: " + e.Message);
            }
        }
        if (real >= _nextVf)
        {
            _nextVf = real + 1f;
            WriteVideoMark(false);
        }
        if (real >= _nextFlush)
        {
            _nextFlush = real + FlushInterval;
            PlayRecWriter.Flush();
        }
        if (string.IsNullOrEmpty(_hooks.Map) && real >= _nextMapProbe)
        {
            _nextMapProbe = real + 5f;
            _hooks.Map = BgmDirector.CurrentMap ?? "";
        }
    }

    /// <summary>20Hz 학습 틱: 관측 → tk(+ 처음 보는 격자면 gd) → 10Hz 마다 st → 1초마다 cx. 회피·사람 신호도 여기서.</summary>
    private void Tick(float now, float real)
    {
        _tick++;
        try
        {
            _obs.Observe();
            bool uiOpen = _obs.UiOpen;
            if (_obs.Ui.Observe(uiOpen))
            {
                WriteFocus();
            }
            _hooks.CacheStats(_obs.Ui.Focus, uiOpen ? _obs.UiWindows[_obs.UiWindows.Count - 1] : "");
            if (real >= _nextContext)
            {
                _nextContext = real + 1f;
                _obs.ObserveContext();
                WriteContextIfChanged(false);
            }
            if (!_metaWritten && ((_obs.HasPlayer && !string.IsNullOrEmpty(_hooks.Map)) || real - _startReal > 10f))
            {
                _metaWritten = true;
                PlayRecMeta.Write(_hooks.Map, _obs, _video);
            }
            WriteGridIfNew(false);
            WriteTick(real, uiOpen);
            if ((_tick & 1) == 0)
            {
                WriteSnapshot(now);
                CheckStuck(now);
            }
            _signals.FeedThreat(_obs.Foes, now);
            _signals.FeedHuman(real, _anyHeld, PauseMenu.IsOpen, _obs.UiWindows);
        }
        catch (Exception e)
        {
            lock (PlayRecWriter.Gate)
            {
                PlayRecWriter.Discard();
            }
            _snapErrors++;
            if (_snapErrors <= 3)
            {
                _warn.Write("snapshot", e.GetType().Name + ": " + e.Message, e.StackTrace);
            }
        }
    }

    private void WriteTick(float real, bool uiOpen)
    {
        long mask = _input.HeldMask(out float mx, out float my);
        lock (PlayRecWriter.Gate)
        {
            if (_input.HeaderDirty)
            {
                _input.HeaderDirty = false;
                PlayRecWriter.Begin("tkh");
                PlayRecTick.WriteHeader(PlayRecWriter.Sb, _input.BitNames);
                PlayRecWriter.Commit();
            }
            StringBuilder sb = PlayRecWriter.Begin("tk");
            PlayRecTick.WriteTick(sb, _tick, real, mask, mx, my, _obs, uiOpen);
            PlayRecWriter.Commit();
        }
        _anyHeld = mask != 0 || Mathf.Abs(mx) > 0.3f || Mathf.Abs(my) > 0.3f;
    }

    private void WriteSnapshot(float now)
    {
        lock (PlayRecWriter.Gate)
        {
            PlayRecSnapshot.Write(PlayRecWriter.Sb, now, Source, _hooks.Map, _obs);
            PlayRecWriter.CommitRaw();
        }
    }

    private void WriteGridIfNew(bool force)
    {
        PlayObsTerrain t = _obs.Terrain;
        if (!_obs.HasPlayer || t.Hash == 0 || (!force && _gridsWritten.Contains(t.Hash)))
        {
            return;
        }
        if (_gridsWritten.Count >= GridSeenMax)
        {
            _gridsWritten.Clear();
        }
        _gridsWritten.Add(t.Hash);
        lock (PlayRecWriter.Gate)
        {
            StringBuilder sb = PlayRecWriter.Begin("gd");
            PlayRecTick.WriteGrid(sb, t, _hooks.Map);
            PlayRecWriter.Commit();
        }
    }

    private void WriteContextIfChanged(bool force)
    {
        PlayObsContext c = _obs.Context;
        if (!force && c.Signature == _ctxSig)
        {
            return;
        }
        _ctxSig = c.Signature;
        lock (PlayRecWriter.Gate)
        {
            StringBuilder sb = PlayRecWriter.Begin("cx");
            PlayRecTick.WriteContext(sb, c);
            PlayRecWriter.Commit();
        }
    }

    private void WriteFocus()
    {
        lock (PlayRecWriter.Gate)
        {
            StringBuilder sb = PlayRecWriter.Begin("fo");
            PlayRecJson.KeyStr(sb, "p", _obs.Ui.Focus);
            PlayRecWriter.Commit();
        }
    }

    /// <summary>k:"vf": 영상 프레임 번호 ↔ 시각. f = 마지막으로 넣은 프레임 번호(0부터), fu = 그 프레임의 캡처 시각(unscaledTime)</summary>
    private void WriteVideoMark(bool final)
    {
        if (!PlayRecWriter.IsOpen || (_video.Path == null))
        {
            return;
        }
        lock (PlayRecWriter.Gate)
        {
            StringBuilder sb = PlayRecWriter.Begin("vf");
            PlayRecJson.KeyInt(sb, "f", _video.Frames - 1);
            PlayRecJson.KeyNum(sb, "fu", _video.LastFrameReal, 3);
            PlayRecJson.KeyNum(sb, "u", Time.unscaledTime, 3);
            PlayRecJson.KeyInt(sb, "drop", _video.Dropped);
            PlayRecJson.KeyInt(sb, "on", _video.Active ? 1 : 0);
            if (final)
            {
                PlayRecJson.KeyInt(sb, "end", 1);
            }
            if (_video.Error != null)
            {
                PlayRecJson.KeyStr(sb, "err", _video.Error);
            }
            PlayRecWriter.Commit();
        }
    }

    private void Bookmark(float real)
    {
        if (_bookmark.Begin(real, _hooks.Map, _obs) < 0)
        {
            return;
        }
        // 그 순간의 상태 전체: st·cx·gd 를 강제로 한 번 더 쓴다(tk 는 다음 틱)
        WriteSnapshot(Time.time);
        _obs.ObserveContext();
        WriteContextIfChanged(true);
        WriteGridIfNew(true);
        WriteVideoMark(false);
        _nextTick = 0f;
    }

    private void OnMapChanged(string map)
    {
        _obs.ForceRescan();
        _signals.Threat.Clear();
    }

    private void CheckStuck(float now)
    {
        bool held = _obs.HasPlayer && Mathf.Abs(_input.MoveX) > 0.3f && !_obs.UiOpen && !_obs.OnRope && _obs.Fsm != "dash";
        if (!_stuck.Feed(now, held, _obs.X, _obs.Y))
        {
            return;
        }
        lock (PlayRecWriter.Gate)
        {
            StringBuilder sb = PlayRecWriter.Begin("warn");
            PlayRecJson.KeyStr(sb, "ty", "stuck");
            PlayRecJson.KeyStr(sb, "map", _hooks.Map);
            PlayRecJson.KeyNum(sb, "x", _obs.X, 2);
            PlayRecJson.KeyNum(sb, "y", _obs.Y, 2);
            PlayRecJson.KeyStr(sb, "fsm", _obs.Fsm);
            PlayRecJson.KeyNum(sb, "mv", _input.MoveX, 2);
            PlayRecWriter.Commit();
        }
    }
}
