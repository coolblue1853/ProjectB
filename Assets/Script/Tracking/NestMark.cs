using UnityEngine;

/// <summary>
/// 둥지 추적 표식 하나(흔적 0~2 또는 둥지 3). 공용 틀(MiniGameSpot)의 짧은 "살피기" 세션만 담당하고, 순서·시간·소환은 NestHuntSite 가 한다.
/// ↑ 로 시작 → 0.8초 살피는 동안 조작 잠금(UiLock "Tracking", 이동하면 중단) → 끝나면 사이트에 보고.
/// </summary>
public class NestMark : MiniGameSpot
{
    public const string Lock = "Tracking";
    private const float SnapUp = 0.5f;
    private const float SnapDown = 10f;
    private const float ResultHoldTime = 0.1f;

    private NestHuntSite _site;
    private int _index;
    private SpriteRenderer _renderer;
    private bool _snapped;
    private bool _has;
    private bool _cancelled;
    private float _elapsed;
    private float _resultTime;
    private bool _shownReady = true;
    private bool _shownVisible = true;

    public int Index => _index;
    /// <summary>살피는 중이면 0~1 진행</summary>
    public float Progress => Mathf.Clamp01(_elapsed / NestHuntRules.InvestigateTime);
    /// <summary>지금 이 표식을 살피는 중인가</summary>
    public bool Investigating => _has;
    /// <summary>지면에 붙은 뒤의 월드 위치(소환 자리·힌트 거리용)</summary>
    public Vector3 Position => transform.position;

    protected override bool Ready => _site != null && _site.CanInvestigate(_index);
    protected override string PromptKey => "UP  INVESTIGATE";
    protected override string LockOwner => Lock;
    protected override float ResultHold => ResultHoldTime;
    protected override bool HasSession => _has;
    protected override bool SessionDone => _cancelled || _elapsed >= NestHuntRules.InvestigateTime;
    protected override bool SessionCancelled => _cancelled;
    protected override float ResultElapsed => _resultTime;

    /// <summary>배치기가 만든 직후 한 번 부른다</summary>
    public void Init(NestHuntSite site, int index, SpriteRenderer renderer)
    {
        _site = site;
        _index = index;
        _renderer = renderer;
        site.Register(this);
    }

    protected override void Update()
    {
        if (!_snapped)
        {
            _snapped = true;
            SnapToGround(SnapUp, SnapDown);
        }
        RefreshLook();
        base.Update();
    }

    /// <summary>보이는 표식(다음 차례거나 시작 표식)만 그리고, 시작 표식은 쿨타임 동안 흐리게</summary>
    private void RefreshLook()
    {
        if (_renderer == null || _has)
        {
            return;
        }
        bool visible = _site != null && _site.IsVisible(_index);
        if (visible != _shownVisible)
        {
            _shownVisible = visible;
            _renderer.enabled = visible;
        }
        bool ready = Ready || _site.State.Running;
        if (ready != _shownReady)
        {
            _shownReady = ready;
            _renderer.color = ready || _index > 0 ? Color.white : new Color(0.45f, 0.45f, 0.5f, 0.55f);
        }
    }

    protected override void StartSession()
    {
        _has = true;
        _cancelled = false;
        _elapsed = 0f;
        _resultTime = 0f;
    }

    protected override void TickSession(float dt, bool held, bool pressed)
    {
        if (SessionDone)
        {
            _resultTime += dt;
            return;
        }
        _elapsed += dt;
    }

    protected override void CancelSession()
    {
        _cancelled = true;
    }

    protected override void ClearSession()
    {
        _has = false;
        _elapsed = 0f;
    }

    protected override void OnResult()
    {
        if (_site != null)
        {
            _site.OnInvestigated(_index);
        }
    }
}
