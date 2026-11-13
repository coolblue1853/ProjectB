using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 마우스 호버 아이템 툴팁(REWORK-2 U26). 런타임 생성 캔버스 하나를 공유한다(씬 수정 없음).
/// 인벤·창고·장비칸(ItemDrag)과 상점 칸(ShopSlotMouse)이 Show/Hide 만 부른다. 상태·커서는 건드리지 않는다.
/// 키보드·패드는 커서+상세창(Z/A)이 이미 같은 정보를 보여 주므로 이 창은 마우스 전용이다.
/// </summary>
public sealed class HoverTip : MonoBehaviour
{
    private const float Width = 520f;
    private const float Pad = 16f;
    private const float Gap = 24f;
    private const float RefreshSec = 0.3f;

    private static HoverTip _instance;

    private Canvas _canvas;
    private RectTransform _canvasRect;
    private RectTransform _panel;
    private TextMeshProUGUI _text;
    private GameObject _root;

    private UnityEngine.Object _owner;
    private Func<string> _provider;
    private float _nextRefresh;
    private string _shown = "";

    public static bool IsShowing => _instance != null && _instance._root != null && _instance._root.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    /// <summary>owner 칸 위에서 provider 문구를 띄운다. owner 가 사라지면 자동으로 닫힌다</summary>
    public static void Show(UnityEngine.Object owner, Func<string> provider)
    {
        if (provider == null || QuantityPopup.IsBlocking || ItemDrag.IsDragging)
        {
            return;
        }
        if (_instance == null)
        {
            var go = new GameObject("HoverTip");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<HoverTip>();
            _instance.Build();
        }
        _instance._owner = owner;
        _instance._provider = provider;
        _instance.Apply(true);
        _instance._root.SetActive(true);
        _instance.Place();
    }

    /// <summary>owner 가 띄운 툴팁만 닫는다(다른 칸이 이미 새로 띄웠으면 건드리지 않는다). owner null 이면 무조건 닫는다</summary>
    public static void Hide(UnityEngine.Object owner = null)
    {
        if (_instance == null || (owner != null && _instance._owner != owner))
        {
            return;
        }
        _instance._owner = null;
        _instance._provider = null;
        _instance._root.SetActive(false);
    }

    private void Update()
    {
        if (!_root.activeSelf)
        {
            return;
        }
        if (_owner == null || QuantityPopup.IsOpen || ItemDrag.IsDragging)
        {
            Hide();
            return;
        }
        if (EffectDetailMode.InputAllowed() && EffectDetailMode.Poll())
        {
            Apply(true); // 상세 토글: 즉시 다시 그린다
        }
        else if (Time.unscaledTime >= _nextRefresh)
        {
            Apply(false);
        }
        Place();
    }

    private void Apply(bool force)
    {
        _nextRefresh = Time.unscaledTime + RefreshSec;
        string next = _provider != null ? _provider() : "";
        if (string.IsNullOrEmpty(next))
        {
            Hide();
            return;
        }
        if (!force && next == _shown)
        {
            return;
        }
        _shown = next;
        _text.text = next;
        float h = _text.GetPreferredValues(next, Width - Pad * 2f, 0f).y;
        _panel.sizeDelta = new Vector2(Width, h + Pad * 2f);
    }

    private void Place()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, mouse.position.ReadValue(), null, out local);
        Rect r = _canvasRect.rect;
        float x, y;
        QuickUseRules.Place(local.x - r.xMin, local.y - r.yMin, _panel.sizeDelta.x, _panel.sizeDelta.y, r.width, r.height, Gap, out x, out y);
        _panel.anchoredPosition = new Vector2(x, y);
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 450; // 수량 창(500) 아래, 인벤 위
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        _canvasRect = (RectTransform)transform;

        _root = new GameObject("Root", typeof(RectTransform));
        _root.transform.SetParent(transform, false);
        var rr = (RectTransform)_root.transform;
        rr.anchorMin = Vector2.zero;
        rr.anchorMax = Vector2.one;
        rr.offsetMin = rr.offsetMax = Vector2.zero;

        Image panel = theme.NewPanel("Panel", _root.transform);
        panel.raycastTarget = false; // 툴팁이 호버를 가로채 깜박이지 않게
        _panel = panel.rectTransform;
        _panel.anchorMin = _panel.anchorMax = _panel.pivot = Vector2.zero;
        _panel.sizeDelta = new Vector2(Width, 100f);

        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(_panel, false);
        _text = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            _text.font = theme.pixelFont;
        }
        _text.fontSize = 22f;
        _text.raycastTarget = false;
        _text.richText = true;
        _text.textWrappingMode = TextWrappingModes.Normal;
        _text.alignment = TextAlignmentOptions.TopLeft;
        RectTransform tr = _text.rectTransform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(Pad, Pad);
        tr.offsetMax = new Vector2(-Pad, -Pad);
        _root.SetActive(false);
    }
}

/// <summary>아이템 한 칸의 툴팁 문구(상세창과 같은 정보를 한 덩어리 텍스트로). 인벤 상세창과 같은 InventoryManager 헬퍼를 쓴다</summary>
public static class ItemTipText
{
    /// <summary>price 가 0 이상이면 그 값(상점 구매가)을, 음수면 아이템 판매가를 표시한다. compare=착용품과 비교</summary>
    public static string Build(ItemCheck item, int price, bool compare)
    {
        if (item == null || item.itemData == null)
        {
            return "";
        }
        ItemData d = item.itemData;
        InventoryManager inv = InventoryManager.instance;
        bool equip = d.type == "Equip";
        var sb = new StringBuilder(256);
        sb.Append(equip ? TooltipBuilder.Name(d.itemNameT, d.rarity, d.upgrade) : d.itemNameT);
        sb.Append("   <color=").Append(TooltipBuilder.RarityColor(d.rarity)).Append('>').Append(TooltipBuilder.RarityName(d.rarity)).Append("  T").Append(d.tear).Append("</color>\n");
        sb.Append(ItemText.TypeLine(d, equip)).Append('\n');
        if (inv != null)
        {
            if (equip)
            {
                sb.Append(inv.EquipStatText(item, compare));
                sb.Append(inv.SetEffectText(d));
            }
            else if (d.type == "Consum" && !string.IsNullOrEmpty(d.effectOb) && !string.IsNullOrEmpty(d.effectPow))
            {
                sb.Append(inv.ConsumEffectText(item));
            }
        }
        string desc = ItemText.Description(d);
        if (!string.IsNullOrEmpty(desc))
        {
            sb.Append(desc).Append('\n');
        }
        sb.Append(Loc.T("Price : ")).Append(price >= 0 ? price : d.price);
        if (item.nowStack > 1 && price < 0)
        {
            sb.Append("  x").Append(item.nowStack);
        }
        return sb.ToString();
    }
}
