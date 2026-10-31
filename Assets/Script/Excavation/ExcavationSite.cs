using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 고대 유물 발굴 유적(③, MiniGames.md "고대 유물 발굴 구현"). 공용 틀(MiniGameSpot)에 발굴판 세션·쿨타임·지면 붙기를 얹는다.
/// ↑ 로 시작 → 방향키(UI 좌우상하, 패드 십자·스틱)로 커서, 공격 키(X, 패드 West)로 파기, ESC(패드 East)로 중단.
/// 방향키를 커서로 쓰므로 좌우 이동으로 중단하지 않는다(CancelOnMove false). 잠금 UiLock "Excavation".
/// 유물을 캐는 즉시 GiveReward(AddSource.Excavation) — 실패해도 캔 것은 유지. '아버지의 기록'은 인벤이 아니라 수집 로그(FathersRecords).
/// 유물을 하나라도 캐면 쿨타임(기본 5분·희귀 10분, 세션 한정).
/// </summary>
public class ExcavationSite : MiniGameSpot
{
    public const string Lock = "Excavation";
    private const float SnapUp = 0.5f;
    private const float SnapDown = 10f;
    private const float ResultShort = 2.5f;
    private const float ResultWithRecord = 9f;

    [SerializeField] private int _floor = 1;
    [SerializeField] private SpriteRenderer _mark;

    private bool _snapped;
    private float _readyAt;
    private bool _shownReady = true;
    private int _grantedFindCount;
    private bool _inputOn;

    /// <summary>지금 진행 중인 유적(없으면 null). ExcavationUI 가 읽는다</summary>
    public static ExcavationSite Current => Active as ExcavationSite;

    public ExcavationSession Session { get; private set; }
    /// <summary>이번 판에 얻은 기록 편 번호(-1 = 없음)</summary>
    public int RecordFound { get; private set; } = -1;
    /// <summary>기록이 나왔지만 5편을 다 모아 대체 보상을 받았는가</summary>
    public bool RecordReplaced { get; private set; }
    public bool IsReady => Ready;

    protected override bool Ready => Time.time >= _readyAt;
    protected override bool CancelOnMove => false;
    protected override string PromptKey => "UP  EXCAVATE";
    protected override string LockOwner => Lock;
    protected override float ResultHold => RecordFound >= 0 ? ResultWithRecord : ResultShort;
    protected override bool HasSession => Session != null;
    protected override bool SessionDone => Session.Done;
    protected override bool SessionCancelled => Session.Result == DigResult.Cancelled;
    protected override float ResultElapsed => Session.Elapsed;

    /// <summary>배치기가 만든 직후 한 번 부른다</summary>
    public void Init(int floor, SpriteRenderer mark)
    {
        _floor = floor;
        _mark = mark;
    }

    protected override void Update()
    {
        if (!_snapped)
        {
            _snapped = true;
            SnapToGround(SnapUp, SnapDown);
        }
        bool ready = Ready;
        if (_mark != null && ready != _shownReady && !HasSession)
        {
            _shownReady = ready;
            _mark.color = ready ? Color.white : new Color(0.45f, 0.45f, 0.5f, 0.55f);
        }
        base.Update();
    }

    protected override void StartSession()
    {
        DigSpec spec = ExcavationRules.SpecFor(RandomValue());
        Session = new ExcavationSession(spec, ExcavationRules.TableFor(_floor), RandomValue);
        _grantedFindCount = 0;
        RecordFound = -1;
        RecordReplaced = false;
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
        int dx = (input.UI.RightInventory.WasPressedThisFrame() ? 1 : 0) - (input.UI.LeftInventory.WasPressedThisFrame() ? 1 : 0);
        int dy = (input.UI.DownInventory.WasPressedThisFrame() ? 1 : 0) - (input.UI.UPInventory.WasPressedThisFrame() ? 1 : 0); // 판 위쪽 = y 0
        if (dx != 0 || dy != 0)
        {
            Session.Move(dx, dy);
        }
        if (input.Player.Attack.WasPressedThisFrame() && Session.Dig())
        {
            GrantNewFinds();
        }
    }

    protected override void CancelSession()
    {
        Session.Cancel();
        StartCooldownIfUsed();
    }

    protected override void ClearSession()
    {
        if (Session != null)
        {
            SetInput(false);
            if (!Session.Done)
            {
                StartCooldownIfUsed(); // 진행 중에 맵이 꺼져도 이미 캔 유물이 있으면 쉰다
            }
        }
        Session = null;
    }

    protected override void OnResult()
    {
        StartCooldownIfUsed();
    }

    /// <summary>쿨타임 즉시 해제(프로브용)</summary>
    public void ResetCooldown()
    {
        _readyAt = 0f;
    }

    /// <summary>캔 유물 보상을 지급(유물마다 한 번). 기록은 수집 로그로, 다 모았으면 대체 아이템</summary>
    private void GrantNewFinds()
    {
        for (; _grantedFindCount < Session.Finds.Count; _grantedFindCount++)
        {
            DigFind f = Session.Finds[_grantedFindCount];
            if (f.Def.Id != ExcavationRules.RecordId)
            {
                GiveReward(f.Def.Id, f.Count, AddSource.Excavation, f.Def.Rarity);
                continue;
            }
            int index = ExcavationRules.PickRecord(FathersRecords.LoadMask(), FathersRecords.All.Length, RandomValue());
            if (index < 0)
            {
                RecordReplaced = true;
                GiveReward(ExcavationRules.RecordFallbackId, 1, AddSource.Excavation, ItemRarity.R);
                continue;
            }
            FathersRecords.Unlock(index);
            RecordFound = index;
            PickupLog.Report(Loc.T("Father's Record") + " " + (index + 1), 1, ItemRarity.U.ToString());
            GameLog.Info(GameLog.Cat.Item, "Excavation: father's record " + (index + 1));
        }
    }

    private void StartCooldownIfUsed()
    {
        if (Session != null && Session.RelicsFound > 0)
        {
            _readyAt = Time.time + ExcavationRules.CooldownFor(Session.Spec);
        }
    }

    /// <summary>발굴 중에만 커서·파기·중단 입력을 켠다(공용 KeyAction, 한 번에 한 유적만 진행)</summary>
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
            input.UI.LeftInventory, input.UI.RightInventory, input.UI.UPInventory, input.UI.DownInventory,
            input.UI.Back, input.Player.Attack,
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
