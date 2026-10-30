using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 몬스터 흔적 추적 지점(⑥, MiniGames.md "몬스터 흔적 추적 구현"). 공용 틀(MiniGameSpot)에 추적 세션·대상 고르기·정예 소환을 얹는다.
/// ↑ 로 시작 → 방향키 ← ↑ → 로 흔적 고르기 → 덫 단계에서 공격 키(X, 패드 West)로 설치, ESC(패드 East)로 중단. 잠금 UiLock "Tracking".
/// 대상 = 이 맵·지금 시간대 스포너의 일반 몹(보스·채집물 제외) 중 종류 균등. 성공하면 그 몹을 정예로 확정 소환(EnemySpawner.SpawnTracked)하고,
/// 처치 때 정예 확정 보상 + 계보 재료(덫 품질 1~3) + 계보 무기 1% 추가 판정을 떨어뜨린다. 쿨타임 성공 10분·실패/중단 3분(지점별, 세션 한정).
/// </summary>
public class TrackingSpot : MiniGameSpot
{
    public const string Lock = "Tracking";
    private const float SnapUp = 0.5f;
    private const float SnapDown = 10f;
    private const float ResultHoldTime = 3f;

    [SerializeField] private SpriteRenderer _mark;

    private readonly List<EnemySpawner> _candidates = new List<EnemySpawner>();
    private readonly List<string> _kinds = new List<string>();
    private EnemySpawner[] _spawners;
    private int _cachedDay = -1;
    private bool _snapped;
    private float _readyAt;
    private bool _shownReady = true;
    private bool _inputOn;
    private EnemySpawner _targetSpawner;
    private Vector3 _spawnPoint;

    /// <summary>지금 진행 중인 추적 지점(없으면 null). TrackingUI 가 읽는다</summary>
    public static TrackingSpot Current => Active as TrackingSpot;

    public TrackingSession Session { get; private set; }
    /// <summary>목표 몹 표시 이름</summary>
    public string TargetName { get; private set; }
    /// <summary>목표 몹 프리팹 이름</summary>
    public string TargetPrefab { get; private set; }
    /// <summary>성공 결과에서 정예를 실제로 불렀는가</summary>
    public bool Summoned { get; private set; }
    /// <summary>이번 판 처치 보너스(계보 재료·무기)</summary>
    public IReadOnlyList<string> Bonus => _bonus;
    /// <summary>처치 보너스 계보 재료 이름(없으면 null)과 개수</summary>
    public string BonusMaterial { get; private set; }
    public int BonusMaterialCount { get; private set; }
    /// <summary>계보 무기 추가 판정에 맞았는가</summary>
    public bool BonusWeapon { get; private set; }
    public bool IsReady => Ready;

    private readonly List<string> _bonus = new List<string>();

    protected override bool Ready => Time.time >= _readyAt && !EnemySpawner.HasTrackedTarget && RefreshCandidates() > 0;
    protected override bool CancelOnMove => false;
    protected override string PromptKey => "UP  TRACK";
    protected override string LockOwner => Lock;
    protected override float ResultHold => ResultHoldTime;
    protected override bool HasSession => Session != null;
    protected override bool SessionDone => Session.Done;
    protected override bool SessionCancelled => Session.Result == TrackResult.Cancelled;
    protected override float ResultElapsed => Session.ResultElapsed;

    /// <summary>배치기가 만든 직후 한 번 부른다</summary>
    public void Init(SpriteRenderer mark)
    {
        _mark = mark;
    }

    protected override void Update()
    {
        if (!_snapped)
        {
            _snapped = true;
            SnapToGround(SnapUp, SnapDown);
        }
        bool ready = !HasSession && Ready; // 쿨타임·추적 정예 생존·후보 없음 모두 흐리게
        if (_mark != null && ready != _shownReady && !HasSession)
        {
            _shownReady = ready;
            _mark.color = ready ? Color.white : new Color(0.45f, 0.45f, 0.5f, 0.55f);
        }
        base.Update();
    }

    protected override void StartSession()
    {
        string kind = _kinds[TrackingRules.Pick(_kinds.Count, RandomValue())];
        _targetSpawner = NearestOfKind(kind, out _spawnPoint);
        TargetPrefab = kind;
        TargetName = MonsterTable.DisplayName(kind);
        Summoned = false;
        _bonus.Clear();
        BonusMaterial = null;
        BonusMaterialCount = 0;
        BonusWeapon = false;
        Session = new TrackingSession(TrackingRules.TraceFor(kind), RandomValue);
        SetInput(true);
    }

    protected override void TickSession(float dt, bool held, bool pressed)
    {
        Session.Tick(dt);
        if (Session.Done)
        {
            return;
        }
        KeyAction input = SharedInput;
        if (input.UI.Back.WasPressedThisFrame())
        {
            CancelSession();
            return;
        }
        if (Session.Phase == TrackPhase.Trap)
        {
            if (input.Player.Attack.WasPressedThisFrame())
            {
                Session.SetTrap();
            }
            return;
        }
        if (Session.Elapsed < TrackingRules.ChoiceDelay)
        {
            return;
        }
        if (input.UI.LeftInventory.WasPressedThisFrame())
        {
            Session.Choose(0);
        }
        else if (input.UI.UPInventory.WasPressedThisFrame())
        {
            Session.Choose(1);
        }
        else if (input.UI.RightInventory.WasPressedThisFrame())
        {
            Session.Choose(2);
        }
    }

    protected override void CancelSession()
    {
        Session.Cancel();
        _readyAt = Time.time + TrackingRules.FailCooldown;
    }

    protected override void ClearSession()
    {
        if (Session != null)
        {
            SetInput(false);
            if (!Session.Done)
            {
                _readyAt = Time.time + TrackingRules.FailCooldown; // 진행 중에 맵이 꺼짐 = 중단
            }
        }
        Session = null;
    }

    protected override void OnResult()
    {
        if (Session.Result != TrackResult.Success)
        {
            _readyAt = Time.time + TrackingRules.FailCooldown;
            return;
        }
        _readyAt = Time.time + TrackingRules.SuccessCooldown;
        Summon();
    }

    /// <summary>쿨타임 즉시 해제(프로브용)</summary>
    public void ResetCooldown()
    {
        _readyAt = 0f;
    }

    /// <summary>목표 몹 정예를 목표 스포너의 가장 가까운 스폰 자리에 부른다. 처치 보너스는 지금 정해 스포너에 맡긴다</summary>
    private void Summon()
    {
        bool day = TimeChange.instance == null || TimeChange.instance.isDaytime;
        if (_targetSpawner == null || _targetSpawner.isDaytimeSpowner != day)
        {
            return; // 추적 중에 시간대가 바뀌었으면 흔적이 끊긴다
        }
        List<DropRow> rows = DropRows(_targetSpawner);
        _bonus.AddRange(TrackingRules.BonusLoot(rows, Session.Quality, RandomValue()));
        BonusMaterial = TrackingRules.LineageMaterial(rows);
        BonusMaterialCount = BonusMaterial != null ? TrackingRules.BonusCount(Session.Quality) : 0;
        BonusWeapon = _bonus.Count > BonusMaterialCount;
        EnemyHealth e = _targetSpawner.SpawnTracked(_spawnPoint, _bonus, TrackingRules.StunSeconds(Session.Quality), TrackingRules.TargetLifetime);
        Summoned = e != null;
        GameLog.Info(GameLog.Cat.Spawn, "Tracking: " + TargetPrefab + " trap=" + Session.Quality + " summoned=" + Summoned);
    }

    private static List<DropRow> DropRows(EnemySpawner spawner)
    {
        var rows = new List<DropRow>();
        DatabaseManager db = DatabaseManager.instance;
        if (spawner.dropManager == null || db == null)
        {
            return rows;
        }
        foreach ((string name, float chance) d in spawner.dropManager.AllDrops())
        {
            int idx = db.FindItemDataIndex(d.name);
            if (idx >= 0)
            {
                rows.Add(new DropRow(d.name, d.chance, db.LoadItemData(idx).type));
            }
        }
        return rows;
    }

    /// <summary>
    /// 이 맵·지금 시간대 후보 스포너와 몹 종류를 고른다(시간대가 바뀔 때만 다시 계산). 후보 = 보스가 아니고 드롭표에 장비가 있는 일반 몹(채집물 제외)
    /// </summary>
    private int RefreshCandidates()
    {
        bool day = TimeChange.instance == null || TimeChange.instance.isDaytime;
        int key = day ? 1 : 0;
        if (key == _cachedDay)
        {
            return _kinds.Count;
        }
        if (DatabaseManager.instance == null)
        {
            return 0; // 아이템 표가 아직 없으면 다음 프레임에 다시
        }
        _cachedDay = key;
        _candidates.Clear();
        _kinds.Clear();
        if (_spawners == null)
        {
            _spawners = transform.parent != null ? transform.parent.GetComponentsInChildren<EnemySpawner>(true) : new EnemySpawner[0];
        }
        foreach (EnemySpawner s in _spawners)
        {
            if (s.isDaytimeSpowner != day || !IsHuntable(s))
            {
                continue;
            }
            _candidates.Add(s);
            string kind = s.EnemyPrefab.name;
            if (!_kinds.Contains(kind))
            {
                _kinds.Add(kind);
            }
        }
        return _kinds.Count;
    }

    public static bool IsHuntable(EnemySpawner s)
    {
        GameObject prefab = s.EnemyPrefab;
        if (prefab == null || !prefab.TryGetComponent(out EnemyHealth eh) || eh.isBossMob)
        {
            return false;
        }
        return s.dropManager != null && DatabaseManager.instance != null && s.dropManager.EquipmentNames().Count > 0;
    }

    /// <summary>kind 종류 후보 중 스폰 자리가 이 지점에 가장 가까운 스포너와 그 자리</summary>
    private EnemySpawner NearestOfKind(string kind, out Vector3 point)
    {
        EnemySpawner best = null;
        point = transform.position;
        float bestDist = float.MaxValue;
        foreach (EnemySpawner s in _candidates)
        {
            if (s == null || s.EnemyPrefab.name != kind || s.enemyPositon == null)
            {
                continue;
            }
            foreach (Transform p in s.enemyPositon.transform)
            {
                float d = Mathf.Abs(p.position.x - transform.position.x) + Mathf.Abs(p.position.y - transform.position.y);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = s;
                    point = p.position;
                }
            }
        }
        return best;
    }

    /// <summary>추적 중에만 방향·덫·중단 입력을 켠다(공용 KeyAction, 한 번에 한 지점만 진행)</summary>
    private void SetInput(bool on)
    {
        if (_inputOn == on)
        {
            return;
        }
        _inputOn = on;
        KeyAction input = SharedInput;
        InputAction[] actions =
        {
            input.UI.LeftInventory, input.UI.RightInventory, input.UI.UPInventory, input.UI.Back, input.Player.Attack,
        };
        foreach (InputAction a in actions)
        {
            if (on)
            {
                a.Enable();
            }
            else
            {
                a.Disable();
            }
        }
    }
}
