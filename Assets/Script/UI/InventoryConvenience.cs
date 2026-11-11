using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>잠금·잡템 판매·열람 상태 UI. 기존 인벤토리/창고 명령과 저장 배치를 재사용한다.</summary>
public sealed class InventoryConvenience : MonoBehaviour
{
    private BulkSaleRules.Plan _pending;
    private float _until;
    private TextMeshProUGUI _hint;
    private GameObject _seenPage;
    private InventoryManager _inv;
    private BulkSaleRules.Plan _shownPlan;
    private bool _shownShop;
    private bool _hintTextReady;
    private GameObject _sellButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("[InventoryConvenience]");
        DontDestroyOnLoad(go);
        go.AddComponent<InventoryConvenience>();
    }

    private void LateUpdate()
    {
        _inv = InventoryManager.instance;
        bool open = _inv != null && _inv.inventory != null && _inv.inventory.activeInHierarchy;
        GameObject page = open ? _inv.inventoryUI[_inv.nowBox] : null;
        if (page != _seenPage)
        {
            MarkSeen(_seenPage);
            _seenPage = page;
        }
        if (!open)
        {
            _pending = null;
            if (_hint != null) { _hint.gameObject.SetActive(false); }
            if (_sellButton != null) { _sellButton.SetActive(false); }
            return;
        }
        bool shop = _inv.ShopGameObject != null && _inv.ShopGameObject.activeSelf;
        bool browse = string.IsNullOrEmpty(_inv.state) || _inv.state == "detail" || _inv.state == "ShopCusorOn" || _inv.state == "chestOpen";
        if (!shop || !browse || Time.unscaledTime > _until) { _pending = null; }
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        if (browse && ((kb != null && kb.lKey.wasPressedThisFrame) || (pad != null && PadChordRules.LockChord(pad.leftTrigger.isPressed, pad.rightStickButton.wasPressedThisFrame))))
        {
            ToggleLock(_inv, _inv.state == "detail" ? _inv.detail : _inv.CursorItem());
            _pending = null;
        }
        bool plain = string.IsNullOrEmpty(_inv.state) || _inv.state == "detail";
        if (plain && !shop && _inv.chest == null && !QuantityPopup.IsBlocking
            && ((kb != null && kb.deleteKey.wasPressedThisFrame)
                || (pad != null && _inv.state == "detail" && PadChordRules.RtAlone(pad.leftTrigger.isPressed, pad.rightTrigger.wasPressedThisFrame))))
        {
            // U24 버리기: Delete / 패드는 상세창에서 RT(칸 위 RT 는 프리셋 적용이라 상세창 한정) → 수량·확인 창
            InventoryDrop.Request(_inv, _inv.state == "detail" ? _inv.detail : _inv.CursorItem());
            _pending = null;
        }
        if (shop && browse && ((kb != null && kb.jKey.wasPressedThisFrame) || (pad != null && pad.leftTrigger.isPressed && pad.leftStickButton.wasPressedThisFrame)))
        {
            RequestBulkSale(_inv);
        }
        ShowHint(shop);
        ShowSellButton(shop && browse && !ConfirmPopup.IsBlocking);
    }

    /// <summary>상점 창 일괄 판매 버튼(마우스 클릭). 키보드 J·패드 LT+L3 는 위 LateUpdate 가 받는다</summary>
    private void ShowSellButton(bool visible)
    {
        if (_sellButton == null)
        {
            if (!visible || _hint == null) { return; }
            UiTheme theme = UiTheme.Get();
            Image img = theme.NewPanel("SellJunkButton", transform, true);
            img.raycastTarget = true;
            RectTransform rect = img.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -160f);
            rect.sizeDelta = new Vector2(520f, 64f);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => RequestBulkSaleConfirm(InventoryManager.instance));
            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(rect, false);
            label.font = theme.pixelFont;
            label.fontSize = 26f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = "잡템 일괄 판매  (J / LT+L3)";
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            _sellButton = img.gameObject;
        }
        if (_sellButton.activeSelf != visible) { _sellButton.SetActive(visible); }
    }

    public static bool ToggleLock(InventoryManager inv, ItemCheck item)
    {
        if (inv == null || item == null || item.itemData == null || inv.Service.IndexOf(item) < 0)
        {
            return false;
        }
        item.itemData.isLocked = !item.itemData.isLocked;
        inv.SaveInventory();
        PickupLog.Report(item.itemData.isLocked ? "아이템 잠금 (L 해제)" : "아이템 잠금 해제", 1, "C");
        return true;
    }

    public bool RequestBulkSale(InventoryManager inv)
    {
        if (inv == null || inv.ShopGameObject == null || !inv.ShopGameObject.activeSelf
            || !(string.IsNullOrEmpty(inv.state) || inv.state == "detail" || inv.state == "ShopCusorOn"))
        {
            _pending = null;
            return false;
        }
        BulkSaleRules.Plan current = BulkSaleRules.Build(inv.Service.ScanModel(), CsvItemCatalog.Shared);
        if (current.Count == 0 || current.Price > int.MaxValue - (long)DatabaseManager.money)
        {
            _pending = null;
            PickupLog.Warn(current.Count == 0 ? "판매할 잠금 해제 잡템이 없습니다" : "골드 한도를 초과합니다");
            return false;
        }
        if (Time.unscaledTime > _until || !BulkSaleRules.Matches(_pending, current))
        {
            _pending = current;
            _until = Time.unscaledTime + 8f;
            return false;
        }
        _pending = null;
        ExecuteSale(inv, current);
        return true;
    }

    /// <summary>버튼 경로: 계획을 만들어 확인창(ConfirmPopup)을 거친 뒤 같은 판매 로직(ExecuteSale)을 실행한다</summary>
    public bool RequestBulkSaleConfirm(InventoryManager inv)
    {
        if (inv == null || inv.ShopGameObject == null || !inv.ShopGameObject.activeSelf
            || !(string.IsNullOrEmpty(inv.state) || inv.state == "detail" || inv.state == "ShopCusorOn")
            || ConfirmPopup.IsBlocking || QuantityPopup.IsBlocking)
        {
            return false;
        }
        BulkSaleRules.Plan plan = BulkSaleRules.Build(inv.Service.ScanModel(), CsvItemCatalog.Shared);
        if (plan.Count == 0 || plan.Price > int.MaxValue - (long)DatabaseManager.money)
        {
            _pending = null;
            PickupLog.Warn(plan.Count == 0 ? "판매할 잠금 해제 잡템이 없습니다" : "골드 한도를 초과합니다");
            return false;
        }
        _pending = null;
        ConfirmPopup.Open(new ConfirmRequest
        {
            Title = "잡템 일괄 판매",
            Message = "잠금 해제 잡템 " + plan.Count + "개를 " + plan.Price + "G 에 판매합니다.\n(잠금·창고 아이템 제외)",
            YesLabel = "판매",
            NoLabel = "취소",
            OnYes = () =>
            {
                // 확인창이 열린 사이 인벤이 바뀌었을 수 있으니 다시 계산해 맞을 때만 판매
                InventoryManager now = InventoryManager.instance;
                if (now == null || now.ShopGameObject == null || !now.ShopGameObject.activeSelf)
                {
                    return;
                }
                BulkSaleRules.Plan fresh = BulkSaleRules.Build(now.Service.ScanModel(), CsvItemCatalog.Shared);
                if (fresh.Count == 0 || !BulkSaleRules.Matches(plan, fresh))
                {
                    PickupLog.Warn("인벤토리가 바뀌어 판매를 취소했습니다");
                    return;
                }
                ExecuteSale(now, fresh);
            },
        });
        return true;
    }

    private static void ExecuteSale(InventoryManager inv, BulkSaleRules.Plan current)
    {
        if (inv.state == "detail") { inv.DetailOff(); }
        inv.BeginSaveBatch();
        try
        {
            foreach (int index in current.Indices)
            {
                Transform box = inv.inventoryUI[index / (inv.maxHor * inv.maxVer)].transform.GetChild(index % (inv.maxHor * inv.maxVer));
                ItemCheck item = box.GetChild(0).GetComponent<ItemCheck>();
                ItemData data = item.itemData;
                int count = item.nowStack;
                int price = data.price * count;
                if (!data.isLocked && inv.Service.RemoveAt(item, count))
                {
                    DatabaseManager.money += price;
                    Buyback.Record(data.name, count, data.tear, data.upgrade, price);
                }
            }
            inv.SaveInventory();
        }
        finally
        {
            inv.EndSaveBatch();
        }
        Sfx.Play(SfxId.Money);
        PickupLog.Report("잡템 " + current.Count + "개 판매 +" + current.Price + "G", 1, "C");
    }

    private void MarkSeen(GameObject page)
    {
        if (page == null || _inv == null) { return; }
        bool changed = false;
        foreach (Transform box in page.transform)
        {
            if (box.childCount == 0) { continue; }
            ItemCheck item = box.GetChild(0).GetComponent<ItemCheck>();
            if (item != null && item.itemData != null && item.itemData.isNew)
            {
                item.itemData.isNew = false;
                changed = true;
            }
        }
        if (changed) { _inv.SaveInventory(); }
    }

    private void ShowHint(bool shop)
    {
        if (_hint == null)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 65;
            if (GetComponent<GraphicRaycaster>() == null) { gameObject.AddComponent<GraphicRaycaster>(); }
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            _hint = new GameObject("ConvenienceHint", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            _hint.transform.SetParent(transform, false);
            _hint.font = UiTheme.Get().pixelFont;
            _hint.fontSize = 22f;
            _hint.alignment = TextAlignmentOptions.Center;
            _hint.raycastTarget = false;
            _hint.outlineWidth = 0.3f;
            RectTransform rect = _hint.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -72f);
            rect.sizeDelta = new Vector2(1320f, 80f);
        }
        _hint.gameObject.SetActive(_inv.state != "detail" && _inv.state != "Equipment");
        if (_hintTextReady && _shownPlan == _pending && _shownShop == shop)
        {
            return;
        }
        _hintTextReady = true;
        _shownPlan = _pending;
        _shownShop = shop;
        string text = _pending != null
            ? "<color=#FFD480>잡템 " + _pending.Count + "개 / " + _pending.Price + "G  —  J 또는 LT+L3 다시 눌러 판매</color>\n잡템만 판매 · 잠금·창고 제외 · 8초 후 취소"
            : "L / LT+R3 잠금·해제 (L 표시) · 초록 점: 새 아이템" + (shop ? "   J / LT+L3 잡템 일괄 판매" : "");
        if (_hint.text != text) { _hint.text = text; }
    }
}
