using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 캠프 화로(② 탐험가 요리, MiniGames.md "탐험가 요리 구현"). 공용 틀(MiniGameSpot)에 요리 세션·쿨타임·지면 붙기를 얹는다.
/// ↑ 로 시작 → 좌우(UI 좌우, 패드 십자)로 재료를 가진 레시피 고르기, 공격 키(X, 패드 West)로 요리 시작(이때 재료 소모)
/// → 굽기는 공격 키로 뒤집기, 끓이기는 공격 키 홀드로 화력 올리기. ESC(패드 East) 중단(시작 뒤 중단은 재료 손실·결과 없음).
/// 방향키를 쓰므로 좌우 이동으로 중단하지 않는다(CancelOnMove false). 잠금 UiLock "Cooking".
/// 결과가 나면 요리(완벽 2·좋음/보통 1) 또는 탄 요리 1 을 GiveReward(AddSource.Cooking) 로 지급하고 이 화로가 90초 쉰다(세션 한정).
/// </summary>
public class CookingStove : MiniGameSpot
{
    public const string Lock = "Cooking";
    private const float SnapUp = 0.5f;
    private const float SnapDown = 10f;
    private const float ResultSeconds = 3f;

    [SerializeField] private SpriteRenderer _mark;

    private bool _snapped;
    private float _readyAt;
    private bool _shownReady = true;
    private bool _inputOn;

    /// <summary>지금 요리 중인 화로(없으면 null). CookingUI 가 읽는다</summary>
    public static CookingStove Current => Active as CookingStove;

    public CookingSession Session { get; private set; }
    /// <summary>결과로 지급한 아이템 이름·개수(결과 단계에서만 유효)</summary>
    public string ResultId { get; private set; }
    public int ResultCount { get; private set; }
    public bool IsReady => Ready;
    /// <summary>시작 때 재료를 소모했는가(중단 시 손실 안내용)</summary>
    public bool Consumed { get; private set; }

    protected override bool Ready => Time.time >= _readyAt;
    protected override bool CancelOnMove => false;
    protected override string PromptKey => "UP  COOK";
    protected override string LockOwner => Lock;
    protected override float ResultHold => ResultSeconds;
    protected override bool HasSession => Session != null;
    protected override bool SessionDone => Session.Phase == CookPhase.Result;
    protected override bool SessionCancelled => Session.Cancelled;
    protected override float ResultElapsed => Session.Elapsed;

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
        bool ready = Ready;
        if (_mark != null && ready != _shownReady && !HasSession)
        {
            _shownReady = ready;
            _mark.color = ready ? Color.white : new Color(0.55f, 0.55f, 0.6f, 0.7f);
        }
        base.Update();
    }

    protected override void StartSession()
    {
        InventoryManager inv = InventoryManager.instance;
        Session = new CookingSession(CookingRules.Available(inv != null ? (System.Func<string, int>)inv.CountItem : (_ => 0)), RandomValue);
        ResultId = null;
        ResultCount = 0;
        Consumed = false;
        SetInput(true);
    }

    protected override void TickSession(float dt, bool held, bool pressed)
    {
        KeyAction input = SharedInput;
        if (Session.Phase == CookPhase.Result)
        {
            Session.Tick(dt, false, false);
            return;
        }
        if (input.UI.Back.WasPressedThisFrame())
        {
            CancelSession();
            return;
        }
        bool attackHeld = input.Player.Attack.IsPressed();
        bool attackPressed = input.Player.Attack.WasPressedThisFrame();
        if (Session.Phase == CookPhase.Select)
        {
            int dx = (input.UI.RightInventory.WasPressedThisFrame() ? 1 : 0) - (input.UI.LeftInventory.WasPressedThisFrame() ? 1 : 0);
            if (dx != 0)
            {
                Session.Select(dx);
            }
            if (attackPressed)
            {
                TryStartCooking();
            }
            return;
        }
        Session.Tick(dt, attackHeld, attackPressed);
    }

    /// <summary>고른 레시피로 시작: 재료가 아직 있으면 소모하고 판을 연다. 모자라면 중단(재료 그대로)</summary>
    public bool TryStartCooking()
    {
        InventoryManager inv = InventoryManager.instance;
        CookRecipe recipe = Session.Recipe ?? (Session.Index < Session.Recipes.Count ? Session.Recipes[Session.Index] : null);
        if (inv == null || recipe == null || !CookingRules.CanCook(recipe, inv.CountItem))
        {
            return false;
        }
        foreach (CookIngredient i in recipe.Ingredients)
        {
            inv.Service.Remove(i.Id, i.Count);
        }
        inv.SaveInventory();
        Session.Confirm();
        Consumed = true;
        GameLog.Info(GameLog.Cat.Item, "Cooking start: " + recipe.Id);
        return true;
    }

    protected override void CancelSession()
    {
        Session.Cancel();
    }

    protected override void ClearSession()
    {
        if (Session != null)
        {
            SetInput(false);
        }
        Session = null;
    }

    protected override void OnResult()
    {
        string id = CookingRules.ResultId(Session.Recipe, Session.Grade);
        int count = CookingRules.ResultCount(Session.Grade);
        ItemRarity rarity = Session.Grade == CookGrade.Fail ? ItemRarity.C : Session.Recipe.Rarity;
        ResultId = id;
        ResultCount = count;
        if (GiveReward(id, count, AddSource.Cooking, rarity) && Session.Grade != CookGrade.Fail)
        {
            PixelCrushers.MessageSystem.SendMessage(this, "Craft", id); // 제작 퀘스트(G11)·NPC 퀘스트 제작 목표
        }
        _readyAt = Time.time + CookingRules.StoveCooldown;
    }

    /// <summary>쿨타임 즉시 해제(프로브용)</summary>
    public void ResetCooldown()
    {
        _readyAt = 0f;
    }

    /// <summary>요리 중에만 좌우·시작·중단 입력을 켠다(공용 KeyAction, 한 번에 한 화로만 진행)</summary>
    private void SetInput(bool on)
    {
        if (_inputOn == on)
        {
            return;
        }
        _inputOn = on;
        KeyAction input = SharedInput;
        InputAction[] actions = { input.UI.LeftInventory, input.UI.RightInventory, input.UI.Back, input.Player.Attack };
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
