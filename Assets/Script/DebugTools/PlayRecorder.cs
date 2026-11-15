using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이 기록기: 한 줄 = JSON 한 객체(jsonl). 사람 플레이를 남겨 통계를 내고, 나중에 "진짜 입력만 쓰는 봇"의 설계 입력으로 쓴다.
/// 종류 k: in(입력) · st(10Hz 상태) · ev(게임 이벤트) · log(GameLog 미러) · warn(경고·예외·막힘). 상세는 PlayBotDesign.md.
/// 게임 로직은 바꾸지 않는다 — 읽기와 이벤트 구독만 한다. 플레이 모드·빌드 모두 기본 켜짐(PlayRecorder.Enabled 로 끈다).
/// </summary>
public sealed class PlayRecorder : MonoBehaviour
{
    /// <summary>정적 스위치. 부트스트랩 전에 false 로 두면 생기지 않고, 실행 중 false 로 바꾸면 기록만 멈춘다.</summary>
    public static bool Enabled = true;
    /// <summary>"human" | "bot". 봇이 같은 형식으로 남길 때 바꾼다.</summary>
    public static string Source = "human";

    private const float SnapshotInterval = 0.1f;
    private const float FlushInterval = 1f;
    private const int LogMax = 300;
    private const int WarnMax = 200;
    private const int WarnKeysMax = 500;
    private const int KeepFiles = 30;

    private static PlayRecorder _inst;
    private static readonly object Gate = new object();
    private static readonly StringBuilder Sb = new StringBuilder(1024);
    private static StreamWriter _writer;
    private static char[] _copyBuf = new char[2048];
    private static int _mainThreadId;
    private static volatile float _lastNow;
    private static string[] _catNames;
    private static readonly string[] DmgKinds = { "basic", "skill", "basic+crit", "skill+crit" };

    public static string FilePath { get; private set; }

    private readonly PlayObservation _obs = new PlayObservation();
    private readonly PlayRecStuck _stuck = new PlayRecStuck();
    private readonly Dictionary<InputAction, ActInfo> _acts = new Dictionary<InputAction, ActInfo>();
    private readonly Dictionary<string, Dedupe> _dedupe = new Dictionary<string, Dedupe>();
    private readonly Dictionary<int, int> _warnCounts = new Dictionary<int, int>();
    private string _map = "";
    private float _moveX;
    private float _nextSnap;
    private float _nextFlush;
    private float _nextMapProbe;
    private int _snapErrors;
    private bool _duplicate;

    private sealed class Dedupe
    {
        public int Frame = -1;
        public int Phase = -1;
        public float X;
        public float Y;
    }

    private sealed class ActInfo
    {
        public string Name;
        public Dedupe Shared;
        public bool SkipChecked;
        public bool Skip;
        public string Dev = "?";
        public float LastX;
        public float LastY;
        public bool IsMove;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        CloseFile(); // 도메인 리로드를 끈 경우 이전 플레이의 핸들을 먼저 닫는다
        _inst = null;
        FilePath = null;
        Enabled = true;
        Source = "human";
        Sb.Length = 0;
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

    // ───────────── 공개 API(정적) ─────────────

    /// <summary>게임 이벤트 한 줄(k:"ev"). a·b·v 는 있을 때만 쓴다(v 는 int.MinValue 면 생략).</summary>
    public static void Event(string name, string a = null, int v = int.MinValue, string b = null)
    {
        if (!Enabled || _writer == null)
        {
            return;
        }
        lock (Gate)
        {
            try
            {
                PlayRecJson.Begin(Sb, Now(), "ev", Source);
                PlayRecJson.KeyStr(Sb, "e", name);
                if (a != null)
                {
                    PlayRecJson.KeyStr(Sb, "a", a);
                }
                if (v != int.MinValue)
                {
                    PlayRecJson.KeyInt(Sb, "v", v);
                }
                if (b != null)
                {
                    PlayRecJson.KeyStr(Sb, "b", b);
                }
                PlayRecJson.End(Sb);
                WriteOut();
            }
            catch (Exception)
            {
                Sb.Length = 0;
            }
        }
    }

    /// <summary>플레이어가 준 피해(EnemyHealth.damage2Enemy 에서 부른다)</summary>
    public static void Dealt(EnemyHealth target, int amount, bool skill, bool crit)
    {
        if (!Enabled || _writer == null)
        {
            return; // 꺼져 있으면 몹 id 문자열도 만들지 않는다
        }
        Event("dmg", target != null ? target.KillMessageId : null, amount, DmgKinds[(skill ? 1 : 0) + (crit ? 2 : 0)]);
    }

    /// <summary>GameLog.Info 미러(k:"log"). cat 은 GameLog.Cat 값.</summary>
    public static void Log(int cat, string message)
    {
        if (!Enabled || _writer == null)
        {
            return;
        }
        lock (Gate)
        {
            try
            {
                if (_catNames == null)
                {
                    _catNames = Enum.GetNames(typeof(GameLog.Cat));
                }
                PlayRecJson.Begin(Sb, Now(), "log", Source);
                PlayRecJson.KeyStr(Sb, "cat", cat >= 0 && cat < _catNames.Length ? _catNames[cat] : "?");
                Sb.Append(",\"msg\":");
                PlayRecJson.AppendTrimmed(Sb, message, LogMax);
                PlayRecJson.End(Sb);
                WriteOut();
            }
            catch (Exception)
            {
                Sb.Length = 0;
            }
        }
    }

    private static float Now()
    {
        if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
        {
            _lastNow = Time.time;
        }
        return _lastNow;
    }

    /// <summary>Sb 를 파일 버퍼로 옮기고 비운다(Gate 안에서 부른다).</summary>
    private static void WriteOut()
    {
        if (_writer == null)
        {
            Sb.Length = 0;
            return;
        }
        int len = Sb.Length;
        if (_copyBuf.Length < len)
        {
            _copyBuf = new char[Mathf.NextPowerOfTwo(len)];
        }
        Sb.CopyTo(0, _copyBuf, 0, len);
        Sb.Length = 0;
        try
        {
            _writer.Write(_copyBuf, 0, len);
        }
        catch (IOException)
        {
            Enabled = false; // 디스크가 가득 찼거나 파일이 사라지면 첫 실패에서 기록을 끈다
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
            OpenFile();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PB][PlayRec] 기록 파일을 열지 못해 꺼짐: " + e.Message);
            Enabled = false;
        }
    }

    private static void OpenFile()
    {
        string dir;
#if UNITY_EDITOR
        dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "playrec");
#else
        dir = Path.Combine(Application.persistentDataPath, "playrec");
#endif
        Directory.CreateDirectory(dir);
        PruneOldFiles(dir, KeepFiles);
        FilePath = Path.Combine(dir, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jsonl");
        _writer = new StreamWriter(new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false), 16384);
        _writer.NewLine = "\n";
    }

    /// <summary>오래된 기록은 최근 keep 개만 남긴다(빌드에서 파일이 무한히 쌓이지 않게). 실패해도 무시.</summary>
    private static void PruneOldFiles(string dir, int keep)
    {
        try
        {
            string[] files = Directory.GetFiles(dir, "*.jsonl");
            if (files.Length < keep)
            {
                return;
            }
            Array.Sort(files, StringComparer.Ordinal); // 파일명이 시각이라 사전순 = 시간순
            for (int i = 0; i <= files.Length - keep; i++)
            {
                File.Delete(files[i]);
            }
        }
        catch (Exception)
        {
            // 정리 실패는 기록에 영향 없다
        }
    }

    private void OnEnable()
    {
        if (_duplicate)
        {
            return;
        }
        InputSystem.onActionChange += OnActionChange;
        Application.logMessageReceivedThreaded += OnLogMessage;
        EnemyHealth.Killed += OnKilled;
        LevelSystem.LeveledUp += OnLevelUp;
        Portal.MapChanged += OnMapChanged;
        NpcQuestLog.Accepted += OnQuestAccepted;
        NpcQuestLog.Completed += OnQuestCompleted;
        Skill.Cast += OnSkillCast;
        PickupLog.Picked += OnPicked;
    }

    private void OnDisable()
    {
        if (_duplicate)
        {
            return;
        }
        InputSystem.onActionChange -= OnActionChange;
        Application.logMessageReceivedThreaded -= OnLogMessage;
        EnemyHealth.Killed -= OnKilled;
        LevelSystem.LeveledUp -= OnLevelUp;
        Portal.MapChanged -= OnMapChanged;
        NpcQuestLog.Accepted -= OnQuestAccepted;
        NpcQuestLog.Completed -= OnQuestCompleted;
        Skill.Cast -= OnSkillCast;
        PickupLog.Picked -= OnPicked;
    }

    private void OnApplicationQuit()
    {
        CloseFile();
    }

    private void OnDestroy()
    {
        if (_inst == this)
        {
            CloseFile();
            _inst = null;
        }
    }

    private static void CloseFile()
    {
        lock (Gate)
        {
            if (_writer == null)
            {
                return;
            }
            try
            {
                _writer.Flush();
                _writer.Dispose();
            }
            catch (Exception)
            {
                // 닫는 중 오류는 무시
            }
            _writer = null;
        }
    }

    private void Update()
    {
        if (!Enabled || _writer == null)
        {
            return;
        }
        float now = Time.time;
        _lastNow = now;
        if (now >= _nextSnap)
        {
            _nextSnap = now + SnapshotInterval;
            TakeSnapshot(now);
        }
        float real = Time.unscaledTime;
        if (real >= _nextFlush)
        {
            _nextFlush = real + FlushInterval;
            lock (Gate)
            {
                try
                {
                    _writer.Flush();
                }
                catch (Exception)
                {
                    // 디스크 오류로 게임이 멈추면 안 된다
                }
            }
        }
        if (string.IsNullOrEmpty(_map) && real >= _nextMapProbe)
        {
            _nextMapProbe = real + 5f;
            _map = BgmDirector.CurrentMap ?? "";
        }
    }

    private void TakeSnapshot(float now)
    {
        lock (Gate)
        {
            int start = Sb.Length;
            try
            {
                _obs.Observe();
                PlayRecSnapshot.Write(Sb, now, Source, _map, _obs);
                WriteOut();
            }
            catch (Exception e)
            {
                Sb.Length = start;
                _snapErrors++;
                if (_snapErrors <= 3)
                {
                    Warn("snapshot", e.GetType().Name + ": " + e.Message, null);
                }
            }
        }
        bool held = _obs.HasPlayer && Mathf.Abs(_moveX) > 0.3f && !_obs.UiOpen && !_obs.OnRope && _obs.Fsm != "dash";
        if (_stuck.Feed(now, held, _obs.X, _obs.Y))
        {
            lock (Gate)
            {
                try
                {
                    PlayRecJson.Begin(Sb, now, "warn", Source);
                    PlayRecJson.KeyStr(Sb, "ty", "stuck");
                    PlayRecJson.KeyStr(Sb, "map", _map);
                    PlayRecJson.KeyNum(Sb, "x", _obs.X, 2);
                    PlayRecJson.KeyNum(Sb, "y", _obs.Y, 2);
                    PlayRecJson.KeyStr(Sb, "fsm", _obs.Fsm);
                    PlayRecJson.KeyNum(Sb, "mv", _moveX, 2);
                    PlayRecJson.End(Sb);
                    WriteOut();
                }
                catch (Exception)
                {
                    Sb.Length = 0;
                }
            }
        }
    }

    /// <summary>k:"warn" 한 줄(Gate 안/밖 어디서든). 같은 메시지는 처음 5번 + 100번째마다만 남긴다.</summary>
    private void Warn(string type, string message, string stack)
    {
        int key = (message ?? "").GetHashCode();
        int count;
        lock (Gate)
        {
            if (_warnCounts.Count >= WarnKeysMax && !_warnCounts.ContainsKey(key))
            {
                _warnCounts.Clear(); // 종류가 무한히 늘어나는 경우의 상한
            }
            _warnCounts.TryGetValue(key, out count);
            count++;
            _warnCounts[key] = count;
            if (count > 5 && count % 100 != 0)
            {
                return;
            }
            string stackLine = FirstLine(stack);
            try
            {
                PlayRecJson.Begin(Sb, Now(), "warn", Source);
                PlayRecJson.KeyStr(Sb, "ty", type);
                Sb.Append(",\"msg\":");
                PlayRecJson.AppendTrimmed(Sb, message, WarnMax);
                if (!string.IsNullOrEmpty(stackLine))
                {
                    Sb.Append(",\"stk\":");
                    PlayRecJson.AppendTrimmed(Sb, stackLine, WarnMax);
                }
                if (count > 1)
                {
                    PlayRecJson.KeyInt(Sb, "n", count);
                }
                PlayRecJson.End(Sb);
                if (_writer != null)
                {
                    WriteOut();
                }
                else
                {
                    Sb.Length = 0;
                }
            }
            catch (Exception)
            {
                Sb.Length = 0;
            }
        }
    }

    private void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (!Enabled || _writer == null || type == LogType.Log)
        {
            return;
        }
        string ty = type == LogType.Warning ? "Warning" : type == LogType.Exception ? "Exception" : "Error";
        Warn(ty, condition, stackTrace);
    }

    private static string FirstLine(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        int nl = text.IndexOf('\n');
        return nl >= 0 ? text.Substring(0, nl) : text;
    }

    // ───────────── 입력(k:"in") ─────────────

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (!Enabled || _writer == null)
        {
            return;
        }
        int phase;
        string ph;
        switch (change)
        {
            case InputActionChange.ActionStarted: phase = 0; ph = "S"; break;
            case InputActionChange.ActionPerformed: phase = 1; ph = "P"; break;
            case InputActionChange.ActionCanceled: phase = 2; ph = "C"; break;
            default: return;
        }
        InputAction action = obj as InputAction;
        if (action == null)
        {
            return;
        }
        ActInfo info;
        if (!_acts.TryGetValue(action, out info))
        {
            info = NewInfo(action);
            _acts[action] = info;
        }
        InputControl control = action.activeControl;
        Type valueType = action.activeValueType;
        if (control != null)
        {
            info.Dev = DeviceName(control.device);
            if (!info.SkipChecked)
            {
                info.SkipChecked = true;
                info.Skip = control.device is Pointer && valueType == typeof(Vector2); // 마우스 이동 같은 고빈도 값
            }
        }
        if (info.Skip)
        {
            return;
        }
        float x = 0f;
        float y = 0f;
        bool vec = false;
        if (phase != 2 && valueType == typeof(Vector2))
        {
            Vector2 v2 = action.ReadValue<Vector2>();
            x = v2.x;
            y = v2.y;
            vec = true;
        }
        else if (phase != 2 && valueType == typeof(float))
        {
            x = action.ReadValue<float>();
        }
        if (info.IsMove)
        {
            _moveX = phase == 2 ? 0f : x; // 막힘 판정용(throttle·중복 제거 전에 갱신)
        }
        // 같은 이름의 액션 인스턴스가 여럿이라 같은 프레임·같은 값은 한 번만 남긴다
        Dedupe dd = info.Shared;
        int frame = Time.frameCount;
        if (dd.Frame == frame && dd.Phase == phase && Mathf.Approximately(dd.X, x) && Mathf.Approximately(dd.Y, y))
        {
            return;
        }
        // 축·벡터의 Performed 는 변화량이 클 때만(스틱 미세 떨림 제외)
        if (phase == 1 && (valueType == typeof(Vector2) || valueType == typeof(float)))
        {
            float dx = x - info.LastX;
            float dy = y - info.LastY;
            bool zeroCross = (x == 0f) != (info.LastX == 0f) || (y == 0f) != (info.LastY == 0f);
            if (!zeroCross && dx * dx + dy * dy < 0.0625f)
            {
                return;
            }
        }
        dd.Frame = frame;
        dd.Phase = phase;
        dd.X = x;
        dd.Y = y;
        info.LastX = x;
        info.LastY = y;
        lock (Gate)
        {
            try
            {
                PlayRecJson.Begin(Sb, Now(), "in", Source);
                PlayRecJson.KeyStr(Sb, "a", info.Name);
                PlayRecJson.KeyStr(Sb, "ph", ph);
                PlayRecJson.KeyStr(Sb, "dev", info.Dev);
                if (vec)
                {
                    PlayRecJson.KeyNum(Sb, "x", x, 3);
                    PlayRecJson.KeyNum(Sb, "y", y, 3);
                }
                else
                {
                    PlayRecJson.KeyNum(Sb, "v", x, 3);
                }
                PlayRecJson.End(Sb);
                WriteOut();
            }
            catch (Exception)
            {
                Sb.Length = 0;
            }
        }
    }

    private ActInfo NewInfo(InputAction action)
    {
        string name = (action.actionMap != null ? action.actionMap.name + "/" : "") + action.name;
        Dedupe shared;
        if (!_dedupe.TryGetValue(name, out shared))
        {
            shared = new Dedupe();
            _dedupe[name] = shared;
        }
        return new ActInfo { Name = name, Shared = shared, IsMove = name == "Player/Move" };
    }

    private static string DeviceName(InputDevice d)
    {
        if (d is Keyboard)
        {
            return "kb";
        }
        if (d is Gamepad)
        {
            return "pad";
        }
        if (d is Pointer)
        {
            return "mouse";
        }
        return "other";
    }

    // ───────────── 게임 이벤트 구독 ─────────────

    private static void OnKilled(EnemyHealth e)
    {
        Event("kill", e != null ? e.KillMessageId : null);
    }

    private static void OnLevelUp(int level)
    {
        Event("lvup", null, level);
    }

    private void OnMapChanged(GameObject openMap)
    {
        _map = openMap != null ? openMap.name : "";
        _obs.ForceRescan();
        Event("map", _map);
    }

    private static void OnQuestAccepted(NpcQuestDef q)
    {
        Event("qacc", q != null ? q.Id : null);
    }

    private static void OnQuestCompleted(NpcQuestDef q)
    {
        Event("qdone", q != null ? q.Id : null);
    }

    private static void OnSkillCast(Skill s)
    {
        Event("skill", s != null ? s.skillName : null, s != null ? s.ActiveSlot : -1);
    }

    private static void OnPicked(string item)
    {
        Event("pick", item);
    }
}
