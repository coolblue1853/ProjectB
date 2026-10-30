using UnityEngine;

/// <summary>
/// 채집 지점(④ 표본 채집 / ⑤ 광맥 채굴, MiniGames.md "채집·채굴 구현"). 공용 틀(MiniGameSpot)에 채집 세션·쿨타임·지면 붙기를 얹는다.
/// 잠금 UiLock "Gather". 성공 때만 GiveReward(AddSource.Gathering/Mining), 성공한 지점은 쿨타임 동안 흐리게 쉰다(세이브 안 함).
/// 처음 켜질 때 한 번 아래 지면(Ground)에 붙는다.
/// </summary>
public class GatherNode : MiniGameSpot
{
    public const string Lock = "Gather";
    private const float SnapUp = 0.5f; // 머리 위 발판에 올라가지 않게 낮게(배치 y 는 잰 지면 높이)
    private const float SnapDown = 10f;

    [SerializeField] private GatherKind _kind;
    [SerializeField] private int _floor = 1;
    [SerializeField] private SpriteRenderer _mark;

    private bool _snapped;
    private float _readyAt;
    private bool _shownReady = true;

    /// <summary>지금 진행 중인 지점(없으면 null). GatherUI 가 읽는다</summary>
    public static GatherNode Current => Active as GatherNode;

    public GatherSession Session { get; private set; }
    public GatherKind Kind => _kind;
    public bool IsReady => Ready;

    protected override bool Ready => Time.time >= _readyAt;
    protected override string PromptKey => _kind == GatherKind.Ore ? "UP  MINE" : "UP  GATHER";
    protected override string LockOwner => Lock;
    protected override float ResultHold => 1.4f;
    protected override bool HasSession => Session != null;
    protected override bool SessionDone => Session.Done;
    protected override bool SessionCancelled => Session.Result == GatherResult.Cancelled;
    protected override float ResultElapsed => Session.Elapsed;

    /// <summary>배치기가 만든 직후 한 번 부른다(부모 맵이 꺼져 있으면 Awake 보다 먼저)</summary>
    public void Init(GatherKind kind, int floor, SpriteRenderer mark)
    {
        _kind = kind;
        _floor = floor;
        _mark = mark;
        RefreshPrompt(); // 켜진 맵이면 Awake 가 Init 보다 먼저 돌아 기본 종류로 번역됐다
    }

    protected override void Update()
    {
        if (!_snapped)
        {
            _snapped = true;
            SnapToGround();
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
        GatherDef[] table = GatherRules.TableFor(_kind, _floor);
        Session = _kind == GatherKind.Ore
            ? (GatherSession)new MiningSession(table, RandomValue)
            : new SpecimenSession(table, RandomValue);
    }

    protected override void TickSession(float dt, bool held, bool pressed)
    {
        Session.Tick(dt, pressed);
    }

    protected override void CancelSession()
    {
        Session.Cancel();
    }

    protected override void ClearSession()
    {
        Session = null;
    }

    protected override void OnResult()
    {
        if (Session.Result != GatherResult.Success)
        {
            return;
        }
        AddSource source = _kind == GatherKind.Ore ? AddSource.Mining : AddSource.Gathering;
        if (GiveReward(Session.Item.Id, Session.Count, source, Session.Item.Rarity))
        {
            _readyAt = Time.time + GatherRules.Cooldown(_kind); // 가방이 가득 차 못 받았으면 쉬지 않는다
        }
    }

    /// <summary>쿨타임 즉시 해제(프로브용)</summary>
    public void ResetCooldown()
    {
        _readyAt = 0f;
    }

    private void SnapToGround()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Ground"));
        filter.useTriggers = false;
        var hits = new RaycastHit2D[4];
        Vector2 from = (Vector2)transform.position + Vector2.up * SnapUp;
        int n = Physics2D.Raycast(from, Vector2.down, filter, hits, SnapUp + SnapDown);
        if (n > 0)
        {
            Vector3 p = transform.position;
            transform.position = new Vector3(p.x, hits[0].point.y, p.z);
        }
        else
        {
            Debug.LogWarning("[PB][Gather] 지면 못 찾음: " + name + " @" + transform.position);
        }
    }
}
