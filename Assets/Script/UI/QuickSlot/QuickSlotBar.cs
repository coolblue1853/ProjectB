using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 퀵슬롯 HUD(BasicFeatures.md 3절): 소비 아이템 4칸을 숫자 1~4(KeyAction Player/Quick1~4)로 사용한다.
/// 인벤이 열려 있으면 같은 키가 '커서 아이템 등록'이 되고, 소비 아이템을 칸으로 드래그해도 등록된다(ItemDrag → TryAssignFromDrag).
/// 사용은 InventoryManager.ConsumeFirst → TryConsume(InventoryService.RemoveAt) 경로만 쓴다. 저장은 ES3 quickslot.items.
/// </summary>
public class QuickSlotBar : MonoBehaviour
{
    public const string SaveKey = "quickslot.items";

    private const float SlotSize = 72f;
    private const float SlotGap = 8f;
    private const float CountRefreshInterval = 0.1f;

    private static QuickSlotBar _instance;

    private readonly QuickSlotModel _model = new QuickSlotModel();
    private readonly Image[] _icons = new Image[QuickSlotModel.SlotCount];
    private readonly Image[] _cdFills = new Image[QuickSlotModel.SlotCount];
    private readonly TextMeshProUGUI[] _counts = new TextMeshProUGUI[QuickSlotModel.SlotCount];
    private readonly RectTransform[] _slotRects = new RectTransform[QuickSlotModel.SlotCount];
    private readonly int[] _shownCounts = new int[QuickSlotModel.SlotCount];
    private readonly string[] _groups = new string[QuickSlotModel.SlotCount];
    private readonly InputAction[] _keys = new InputAction[QuickSlotModel.SlotCount];

    private KeyAction _action; // TODO(키 설정, BasicFeatures 3순위): 리바인딩 오버라이드를 이 인스턴스에도 적용
    private GameObject _root;
    private float _countTimer;

    public static QuickSlotModel Model => _instance != null ? _instance._model : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[QuickSlotBar]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<QuickSlotBar>();
    }

    private void Awake()
    {
        _action = new KeyAction();
        _keys[0] = _action.Player.Quick1;
        _keys[1] = _action.Player.Quick2;
        _keys[2] = _action.Player.Quick3;
        _keys[3] = _action.Player.Quick4;
        BuildHud();
        _model.LoadFrom(LoadSaved());
        _model.Changed += OnModelChanged;
        ApplyModel();
    }

    private static string[] LoadSaved()
    {
        try
        {
            return ES3.KeyExists(SaveKey) ? ES3.Load<string[]>(SaveKey) : null;
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "퀵슬롯 세이브 읽기 실패, 빈 칸으로 시작: " + e.Message);
            return null;
        }
    }

    private void OnEnable()
    {
        _action?.Enable();
    }

    private void OnDisable()
    {
        _action?.Disable();
    }

    private void OnDestroy()
    {
        _model.Changed -= OnModelChanged;
        _action?.Dispose();
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        for (int i = 0; i < QuickSlotModel.SlotCount; i++)
        {
            if (_keys[i].WasPressedThisFrame())
            {
                OnKey(i);
            }
        }
        UpdateVisibility();
        UpdateCooldowns();
        _countTimer -= Time.unscaledDeltaTime;
        if (_countTimer <= 0f)
        {
            _countTimer = CountRefreshInterval;
            RefreshCounts();
        }
    }

    private void OnKey(int slot)
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv != null && inv.inventory != null && inv.inventory.activeSelf)
        {
            // 나누기·이동 같은 진행 중 상태에서는 등록하지 않는다(커서 아이템이 바뀌는 중)
            if (string.IsNullOrEmpty(inv.state) || inv.state == "detail")
            {
                AssignFrom(slot, inv.CursorItem());
            }
            return;
        }
        if (!DatabaseManager.isOpenUI)
        {
            Use(slot);
        }
    }

    /// <summary>slot 칸의 아이템을 1개 사용한다(사용했으면 true). 프로브·게임패드 확장용 공개 창구</summary>
    public static bool Use(int slot)
    {
        if (_instance == null || InventoryManager.instance == null)
        {
            return false;
        }
        string id = _instance._model[slot];
        if (id == null)
        {
            return false;
        }
        bool used = InventoryManager.instance.ConsumeFirst(id);
        if (used)
        {
            _instance.Punch(slot);
            GameLog.Info(GameLog.Cat.Item, "퀵슬롯 " + (slot + 1) + " 사용: " + id);
        }
        InventoryManager.instance.RebuildItemStackCache(); // 표시 캐시는 0.25초 주기라 사용 직후 수량을 바로 맞춘다
        _instance.RefreshCounts();
        return used;
    }

    /// <summary>item(소비 아이템)을 slot 칸에 등록한다. 소비가 아니면 경고 후 false</summary>
    public static bool AssignFrom(int slot, ItemCheck item)
    {
        if (_instance == null || item == null || item.itemData == null)
        {
            return false;
        }
        if (item.itemData.type != "Consum")
        {
            PickupLog.Warn(Loc.T("Only consumables go in quick slots"));
            return false;
        }
        _instance._model.Assign(slot, item.name);
        _instance.Punch(slot);
        Sfx.Play(SfxId.UiMove, 0.7f);
        return true;
    }

    /// <summary>드래그를 놓은 화면 좌표가 퀵슬롯 칸 위면 등록을 시도한다(ItemDrag 가 부른다). 칸 위였으면 true(등록 거절 포함)</summary>
    public static bool TryAssignFromDrag(Vector2 screenPos, ItemCheck item)
    {
        if (_instance == null || _instance._root == null || !_instance._root.activeInHierarchy)
        {
            return false;
        }
        for (int i = 0; i < QuickSlotModel.SlotCount; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(_instance._slotRects[i], screenPos, null))
            {
                AssignFrom(i, item);
                return true;
            }
        }
        return false;
    }

    private void OnModelChanged()
    {
        ApplyModel();
        ES3.Save(SaveKey, _model.ToSaveArray());
    }

    private void ApplyModel()
    {
        for (int i = 0; i < QuickSlotModel.SlotCount; i++)
        {
            string id = _model[i];
            Sprite sprite = id != null ? Resources.Load<Sprite>("Item/" + id) : null;
            _icons[i].sprite = sprite;
            _icons[i].enabled = sprite != null;
            _groups[i] = id != null ? GroupOfItem(id) : null;
            _shownCounts[i] = int.MinValue;
        }
        RefreshCounts();
    }

    private static string GroupOfItem(string id)
    {
        DatabaseManager db = DatabaseManager.instance;
        if (db == null)
        {
            return ConsumeCooldown.MiscGroup;
        }
        ItemData data = db.LoadItemData(db.FindItemDataIndex(id));
        return ConsumeCooldown.GroupOf(data != null ? data.effectOb : null);
    }

    /// <summary>수량은 InventoryManager 표시 캐시(inventoryItemStack, 슬롯 기준 재계산)를 읽는다 — 매번 슬롯을 훑지 않게</summary>
    private void RefreshCounts()
    {
        for (int i = 0; i < QuickSlotModel.SlotCount; i++)
        {
            string id = _model[i];
            int n = 0;
            if (id != null)
            {
                DatabaseManager.inventoryItemStack.TryGetValue(id, out n);
            }
            if (n == _shownCounts[i])
            {
                continue;
            }
            _shownCounts[i] = n;
            _counts[i].text = id != null ? n.ToString() : "";
            _icons[i].color = n > 0 ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.8f);
        }
    }

    private void UpdateCooldowns()
    {
        float now = Time.time;
        for (int i = 0; i < QuickSlotModel.SlotCount; i++)
        {
            _cdFills[i].fillAmount = _groups[i] != null ? ConsumeCooldown.Shared.Fraction(_groups[i], now) : 0f;
        }
    }

    /// <summary>인벤토리가 열려 있으면 보인다(드래그 대상). 그 밖의 UI(상점·대화·일시정지)가 열리면 숨긴다</summary>
    private void UpdateVisibility()
    {
        InventoryManager inv = InventoryManager.instance;
        bool invOpen = inv != null && inv.inventory != null && inv.inventory.activeSelf;
        bool show = invOpen || !DatabaseManager.isOpenUI;
        if (_root.activeSelf != show)
        {
            _root.SetActive(show);
        }
    }

    private void Punch(int slot)
    {
        Transform t = _slotRects[slot];
        t.DOKill(true);
        t.localScale = Vector3.one;
        t.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f).SetLink(t.gameObject);
    }

    // ── HUD ──
    private void BuildHud()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _root = new GameObject("Bar", typeof(RectTransform));
        _root.transform.SetParent(transform, false);
        var rr = (RectTransform)_root.transform;
        rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 0f);
        float width = QuickSlotModel.SlotCount * SlotSize + (QuickSlotModel.SlotCount - 1) * SlotGap;
        rr.sizeDelta = new Vector2(width, SlotSize);
        rr.anchoredPosition = new Vector2(0f, 20f);

        for (int i = 0; i < QuickSlotModel.SlotCount; i++)
        {
            BuildSlot(theme, rr, i);
        }
    }

    private void BuildSlot(UiTheme theme, RectTransform parent, int i)
    {
        Image chip = theme.NewPanel("Slot" + (i + 1), parent, false, 0.85f);
        RectTransform cr = chip.rectTransform;
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0f, 0.5f);
        cr.sizeDelta = new Vector2(SlotSize, SlotSize);
        cr.anchoredPosition = new Vector2(i * (SlotSize + SlotGap), 0f);
        _slotRects[i] = cr;

        _icons[i] = NewImage("Icon", cr, 12f);
        _icons[i].preserveAspect = true;
        _icons[i].enabled = false;

        Image cd = NewImage("Cooldown", cr, 6f);
        cd.color = new Color(0f, 0f, 0f, 0.55f);
        cd.type = Image.Type.Filled;
        cd.fillMethod = Image.FillMethod.Radial360;
        cd.fillOrigin = (int)Image.Origin360.Top;
        cd.fillClockwise = false;
        cd.fillAmount = 0f;
        _cdFills[i] = cd;

        TextMeshProUGUI key = NewText("Key", cr, theme, 24f, TextAlignmentOptions.TopLeft, new Color(1f, 0.95f, 0.8f));
        key.text = (i + 1).ToString();
        key.rectTransform.offsetMin = new Vector2(7f, 0f);
        key.rectTransform.offsetMax = new Vector2(0f, -4f);

        _counts[i] = NewText("Count", cr, theme, 26f, TextAlignmentOptions.BottomRight, Color.white);
        _counts[i].rectTransform.offsetMin = new Vector2(0f, 5f);
        _counts[i].rectTransform.offsetMax = new Vector2(-7f, 0f);
    }

    private static Image NewImage(string name, RectTransform parent, float inset)
    {
        var img = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.transform.SetParent(parent, false);
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.offsetMin = new Vector2(inset, inset);
        img.rectTransform.offsetMax = new Vector2(-inset, -inset);
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI NewText(string name, RectTransform parent, UiTheme theme, float size, TextAlignmentOptions align, Color color)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 220);
        t.raycastTarget = false;
        t.rectTransform.anchorMin = Vector2.zero;
        t.rectTransform.anchorMax = Vector2.one;
        return t;
    }
}
