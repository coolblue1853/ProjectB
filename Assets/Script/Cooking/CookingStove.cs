using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 캠프 화로(② 탐험가 요리, MiniGames.md "탐험가 요리 구현"). 공용 틀(MiniGameSpot)에 요리 세션·쿨타임·지면 붙기를 얹는다.
/// ↑ 로 시작 → 좌우(UI 좌우, 패드 십자)로 재료를 가진 레시피 고르기, 공격 키(X, 패드 West)로 요리 시작(이때 재료 소모)
/// → 굽기는 공격 키로 뒤집기, 끓이기는 공격 키 홀드로 화력 올리기. ESC(패드 East) 중단(시작 뒤 중단은 재료 손실·결과 없음).
/// 방향키를 쓰므로 좌우 이동으로 중단하지 않는다(CancelOnMove false). 잠금 UiLock "Cooking".
/// 결과가 나면 요리(완벽 2·좋음/보통 1) 또는 탄 요리 1 을 GiveReward(AddSource.Cooking) 로 지급하고 이 화로가 90초 쉰다(세션 한정).
/// 일괄 요리(REWORK-11, Cooking.md): 시작 때 재료가 2회분 이상이면 QuantityPopup 으로 개수(최대 10)를 고른다.
/// 한 판(미니게임 1회)의 등급이 전부에 적용된다. 마을 요리대(_kitchen)는 쿨타임이 없다.
/// </summary>
public class CookingStove : MiniGameSpot
{
    public const string Lock = "Cooking";
    private const float SnapUp = 0.5f;
    private const float SnapDown = 10f;
    private const float ResultSeconds = 3f;

    [SerializeField] private SpriteRenderer _mark;
    [SerializeField] private bool _kitchen;

    private bool _snapped;
    private float _readyAt;
    private bool _shownReady = true;
    private bool _inputOn;
    private bool _popupOpen;

    /// <summary>지금 요리 중인 화로(없으면 null). CookingUI 가 읽는다</summary>
    public static CookingStove Current => Active as CookingStove;

    public CookingSession Session { get; private set; }
    /// <summary>결과로 지급한 아이템 이름·개수(결과 단계에서만 유효)</summary>
    public string ResultId { get; private set; }
    public int ResultCount { get; private set; }
    public bool IsReady => Ready;
    /// <summary>시작 때 재료를 소모했는가(중단 시 손실 안내용)</summary>
    public bool Consumed { get; private set; }
    /// <summary>마을 요리대인가(쿨타임 없음)</summary>
    public bool IsKitchen => _kitchen;
    /// <summary>수량 선택 창이 열려 있는가</summary>
    public bool PopupOpen => _popupOpen;
    /// <summary>결과 단계에서 가방이 모자라 못 받은 개수</summary>
    public int Lost { get; private set; }

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
    public void Init(SpriteRenderer mark, bool kitchen = false)
    {
        _mark = mark;
        _kitchen = kitchen;
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
        if (_popupOpen || QuantityPopup.IsBlocking)
        {
            return; // 수량 창이 입력을 쓴다(Esc/B 는 창만 닫고, 같은 프레임에 요리를 중단하지 않는다)
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
                RequestStart();
            }
            return;
        }
        Session.Tick(dt, attackHeld, attackPressed);
    }

    /// <summary>선택 단계 좌우 이동(마우스 ◀▶ 버튼용). 수량 창이 열려 있으면 무시</summary>
    public void SelectDelta(int dx)
    {
        if (Session != null && Session.Phase == CookPhase.Select && !_popupOpen)
        {
            Session.Select(dx);
        }
    }

    /// <summary>고른 레시피의 시작 요청: 2회분 이상이면 수량 창, 1회분이면 바로 시작(키·패드 X/West, 마우스 버튼 공용)</summary>
    public bool RequestStart()
    {
        InventoryManager inv = InventoryManager.instance;
        CookRecipe recipe = SelectedRecipe();
        if (inv == null || recipe == null || _popupOpen || QuantityPopup.IsOpen || Session.Phase != CookPhase.Select)
        {
            return false;
        }
        int max = CookingRules.MaxCount(recipe, inv.CountItem);
        if (max < 1)
        {
            return false;
        }
        if (max == 1)
        {
            return TryStartCooking(1);
        }
        _popupOpen = true;
        QuantityPopup.Open(new QuantityRequest
        {
            Title = Loc.T("Cook how many?"),
            ItemName = ItemText.DisplayName(recipe.Id),
            Min = 1,
            Max = max,
            Initial = 1,
            ConfirmLabel = Loc.T("Cook"),
            Detail = n => DetailText(recipe, n),
            OnConfirm = n =>
            {
                _popupOpen = false;
                if (Active == this && Session != null && Session.Phase == CookPhase.Select)
                {
                    TryStartCooking(n);
                }
            },
            OnCancel = () => _popupOpen = false,
        });
        return true;
    }

    private CookRecipe SelectedRecipe()
    {
        if (Session == null)
        {
            return null;
        }
        return Session.Recipe ?? (Session.Index < Session.Recipes.Count ? Session.Recipes[Session.Index] : null);
    }

    /// <summary>수량 창 아래 줄: 필요 재료 총량 + 진행 시간(일괄이어도 한 판)</summary>
    private static string DetailText(CookRecipe recipe, int n)
    {
        var sb = new System.Text.StringBuilder();
        foreach (CookIngredient i in recipe.Ingredients)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }
            sb.Append(ItemText.DisplayName(i.Id)).Append(" x").Append(i.Count * n);
        }
        sb.Append("  /  ").Append(Mathf.RoundToInt(CookingRules.PlaySeconds(recipe.Method))).Append('s');
        return sb.ToString();
    }

    /// <summary>고른 레시피를 count 개분으로 시작: 재료가 아직 있으면 소모하고 판을 연다. 모자라면 중단(재료 그대로)</summary>
    public bool TryStartCooking(int count = 1)
    {
        InventoryManager inv = InventoryManager.instance;
        CookRecipe recipe = SelectedRecipe();
        count = Mathf.Max(1, count);
        if (inv == null || recipe == null || CookingRules.MaxCount(recipe, inv.CountItem) < count)
        {
            return false;
        }
        foreach (CookIngredient i in recipe.Ingredients)
        {
            inv.Service.Remove(i.Id, i.Count * count);
        }
        inv.SaveInventory();
        Session.Confirm(count);
        Consumed = true;
        GameLog.Info(GameLog.Cat.Item, "Cooking start: " + recipe.Id + " x" + count);
        return true;
    }

    protected override void CancelSession()
    {
        CloseOwnPopup();
        Session.Cancel();
    }

    private void CloseOwnPopup()
    {
        if (_popupOpen)
        {
            _popupOpen = false;
            QuantityPopup.ForceClose();
        }
    }

    protected override void ClearSession()
    {
        CloseOwnPopup();
        if (Session != null)
        {
            SetInput(false);
        }
        Session = null;
    }

    protected override void OnResult()
    {
        string id = CookingRules.ResultId(Session.Recipe, Session.Grade);
        int total = CookingRules.ResultCount(Session.Grade, Session.Count);
        ItemRarity rarity = Session.Grade == CookGrade.Fail ? ItemRarity.C : Session.Recipe.Rarity;
        int given = GiveBatch(id, total, rarity);
        ResultId = id;
        ResultCount = given;
        Lost = total - given;
        if (given > 0 && Session.Grade != CookGrade.Fail)
        {
            for (int i = 0, n = Mathf.Min(Session.Count, given); i < n; i++)
            {
                PixelCrushers.MessageSystem.SendMessage(this, "Craft", id); // 제작 퀘스트(G11)·NPC 퀘스트 제작 목표(판 수만큼)
            }
        }
        _readyAt = Time.time + (_kitchen ? CookingRules.KitchenCooldown : CookingRules.StoveCooldown);
    }

    /// <summary>가방이 모자라면 들어가는 만큼만 지급한다(알림은 한 번). 지급한 개수를 돌려준다</summary>
    private int GiveBatch(string id, int total, ItemRarity rarity)
    {
        InventoryManager inv = InventoryManager.instance;
        for (int n = total; n >= 1 && inv != null; n--)
        {
            if (!inv.Service.Add(id, n, AddSource.Cooking))
            {
                continue;
            }
            inv.SaveInventory();
            PickupLog.Report(ItemText.DisplayName(id), n, rarity.ToString());
            GameLog.Info(GameLog.Cat.Item, AddSource.Cooking + ": " + id + " x" + n);
            if (n < total)
            {
                PickupLog.ReportFull();
            }
            return n;
        }
        GiveReward(id, 1, AddSource.Cooking, rarity); // 한 개도 못 넣음 → BagFull 표시·알림
        return 0;
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
