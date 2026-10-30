using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 디아식 비교 패널(BasicFeatures.md 6절, InventoryRefactorDesign 3.1): 인벤에서 장비 상세를 보는 중 Shift(게임패드 LT)를 누르고 있으면
/// 같은 부위 착용품의 툴팁을 상세 창 옆에 하나 더 띄운다. 상세 창의 ▲/▼ 수치 비교는 그대로이고, 이 패널은 착용품 자체를 보여 준다.
/// 착용품 조회·문구는 매 프레임이 아니라 상세 아이템이 바뀌거나 Shift 를 누른 순간에만 다시 만든다.
/// </summary>
public class ComparePanel : MonoBehaviour
{
    private const float Width = 420f;
    private const float Gap = 12f;

    private static ComparePanel _instance;

    private RectTransform _panel;
    private Image _icon;
    private TextMeshProUGUI _head, _body;
    private Canvas _canvas;
    private ItemCheck _shownFor;
    private ItemCheck _shownWorn;
    private Canvas _detailCanvas;
    private bool _wasHeld;
    private readonly Vector3[] _corners = new Vector3[4];

    public static bool IsShowing => _instance != null && _instance._panel != null && _instance._panel.gameObject.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[ComparePanel]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<ComparePanel>();
    }

    private void Awake()
    {
        Build();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        bool held = IsHeld();
        ItemCheck target = held ? DetailTarget() : null;
        if (target == null)
        {
            _shownFor = null;
            _shownWorn = null;
            Hide();
            _wasHeld = held;
            return;
        }
        if (target != _shownFor || !_wasHeld)
        {
            ItemCheck worn = InventoryManager.instance.EquippedFor(target.itemData.equipArea);
            _shownFor = target;
            _shownWorn = worn != null && worn != target ? worn : null;
            if (_shownWorn != null)
            {
                Fill(_shownWorn);
                _detailCanvas = InventoryManager.instance.equipDetail.GetComponentInParent<Canvas>();
            }
        }
        _wasHeld = held;
        if (_shownWorn == null)
        {
            Hide();
            return;
        }
        if (!_panel.gameObject.activeSelf)
        {
            _panel.gameObject.SetActive(true);
        }
        PlaceBeside(InventoryManager.instance.equipDetail.transform as RectTransform);
    }

    private static bool IsHeld()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.shiftKey.isPressed)
        {
            return true;
        }
        Gamepad pad = Gamepad.current;
        return pad != null && pad.leftTrigger.isPressed;
    }

    /// <summary>인벤 칸 장비의 상세 창이 떠 있으면 그 아이템(착용칸 상세는 비교 대상 아님)</summary>
    private static ItemCheck DetailTarget()
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || inv.inventory == null || !inv.inventory.activeSelf || inv.equipDetail == null || !inv.equipDetail.activeSelf
            || inv.state != "detail" || inv.detail == null || inv.detail.itemData == null || inv.detail.itemData.type != "Equip")
        {
            return null;
        }
        return inv.detail;
    }

    /// <summary>패널만 끈다. 조회 결과(_shownFor/_shownWorn)는 대상·누름이 바뀔 때까지 유지(착용품 없는 부위를 매 프레임 다시 조회하지 않게)</summary>
    private void Hide()
    {
        if (_panel.gameObject.activeSelf)
        {
            _panel.gameObject.SetActive(false);
        }
    }

    private void Fill(ItemCheck worn)
    {
        ItemData d = worn.itemData;
        _icon.sprite = worn.image != null ? worn.image.sprite : null;
        _icon.enabled = _icon.sprite != null;
        _head.text = "<size=70%><color=#9ED0FF>" + Loc.T("EQUIPPED") + "</color></size>\n" + TooltipBuilder.Name(d.itemNameT, d.rarity, d.upgrade)
            + "\n<size=80%>" + ItemText.TypeLine(d, true) + "  <color=" + TooltipBuilder.RarityColor(d.rarity) + ">" + TooltipBuilder.RarityName(d.rarity) + "</color></size>";
        GameObject prefab = GearPrefabCache.Get(d.equipArea, worn.name);
        _body.text = d.equipArea == "Weapon"
            ? TooltipBuilder.WeaponBlock(GearPrefabCache.Comp<Weapon>(prefab), d.upgrade, null, 0)
            : TooltipBuilder.ArmorBlock(GearPrefabCache.Comp<Equipment>(prefab), d.upgrade, null, 0);
        Canvas.ForceUpdateCanvases();
        float h = 150f + _body.preferredHeight;
        _panel.sizeDelta = new Vector2(Width, h);
    }

    /// <summary>
    /// 상세 창 왼쪽에 붙인다. 자리가 없으면 오른쪽 아래(상세 창 바닥에 맞춤 — 오른쪽 위는 무기 스킬 상세 창이 차지한다).
    /// 상세 창이 다른 캔버스여도 화면 좌표로 맞춘다
    /// </summary>
    private void PlaceBeside(RectTransform detail)
    {
        if (detail == null)
        {
            return;
        }
        detail.GetWorldCorners(_corners);
        Camera cam = _detailCanvas != null && _detailCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? _detailCanvas.worldCamera : null;
        Vector2 topLeft = RectTransformUtility.WorldToScreenPoint(cam, _corners[1]);
        Vector2 bottomRight = RectTransformUtility.WorldToScreenPoint(cam, _corners[3]);
        float scale = _canvas.scaleFactor > 0f ? _canvas.scaleFactor : 1f;
        float w = (Width + Gap) * scale;
        bool left = topLeft.x - w >= 0f;
        Vector2 screen = left
            ? new Vector2(topLeft.x - w, topLeft.y)
            : new Vector2(bottomRight.x + Gap * scale, bottomRight.y + _panel.sizeDelta.y * scale);
        _panel.anchoredPosition = new Vector2(screen.x / scale, screen.y / scale);
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 60;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image bg = theme.NewPanel("Panel", transform, false, 1f);
        _panel = bg.rectTransform;
        _panel.anchorMin = _panel.anchorMax = Vector2.zero;
        _panel.pivot = new Vector2(0f, 1f);
        _panel.sizeDelta = new Vector2(Width, 300f);

        _icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        _icon.transform.SetParent(_panel, false);
        _icon.preserveAspect = true;
        _icon.raycastTarget = false;
        Place(_icon.rectTransform, new Vector2(18f, -18f), new Vector2(84f, 84f));

        _head = NewText("Head", theme, 28f);
        Place(_head.rectTransform, new Vector2(114f, -14f), new Vector2(Width - 128f, 110f));
        _body = NewText("Body", theme, 24f);
        _body.textWrappingMode = TextWrappingModes.Normal;
        Place(_body.rectTransform, new Vector2(20f, -128f), new Vector2(Width - 40f, 400f));
        _panel.gameObject.SetActive(false);
    }

    private TextMeshProUGUI NewText(string name, UiTheme theme, float size)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(_panel, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(0, 0, 0, 200);
        t.raycastTarget = false;
        return t;
    }

    private static void Place(RectTransform rt, Vector2 topLeft, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = topLeft;
        rt.sizeDelta = size;
    }
}
