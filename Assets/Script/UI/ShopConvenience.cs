using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 메이플식 상점 편의(BasicFeatures.md 14절): 판매 종류 필터(O / 패드 SELECT, 장비·소비·기타), 판매가 미리보기,
/// 퀵슬롯 물약 충전(H / 패드 LT+X). 되사기·재료 일괄 판매·잠금은 기존 Buyback·InventoryConvenience 가 맡는다.
/// 규칙은 ShopRules(순수), 이 클래스는 입력·표시·인벤토리 연결만 한다.
/// </summary>
public sealed class ShopConvenience : MonoBehaviour
{
    private static SellFilter s_filter = SellFilter.All;
    private static readonly Color Dim = new Color(0.35f, 0.35f, 0.35f, 1f);

    private InventoryManager _inv;
    private TextMeshProUGUI _text;
    private bool _dimmed;
    private int _dimPage = -1;
    private float _nextDim, _nextText;
    private SellFilter _dimFilter;
    private ItemCheck _shownItem;
    private int _shownCount = -1, _shownGold = -1;
    private SellFilter _shownFilter = (SellFilter)(-1);
    private bool _shownVisible;

    public static SellFilter Filter => s_filter;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_filter = SellFilter.All;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("[ShopConvenience]");
        DontDestroyOnLoad(go);
        go.AddComponent<ShopConvenience>();
    }

    /// <summary>판매 필터가 이 종류의 판매를 허용하는가(필터 All 이면 항상)</summary>
    public static bool AllowsSale(string type)
    {
        return ShopRules.FilterMatches(s_filter, type);
    }

    /// <summary>필터를 다음 종류로(전체 → 장비 → 소비 → 기타). O / 패드 SELECT 가 부른다</summary>
    public static void CycleFilter()
    {
        s_filter = ShopRules.NextFilter(s_filter);
        Sfx.Play(SfxId.UiMove, 0.7f);
    }

    private static bool ShopOpen(InventoryManager inv)
    {
        return inv != null && inv.inventory != null && inv.inventory.activeInHierarchy
            && inv.ShopGameObject != null && inv.ShopGameObject.activeSelf;
    }

    private void LateUpdate()
    {
        _inv = InventoryManager.instance;
        if (!ShopOpen(_inv))
        {
            if (s_filter != SellFilter.All)
            {
                s_filter = SellFilter.All; // 상점을 닫으면 필터는 전체로
            }
            RestoreDim();
            SetText(null);
            return;
        }
        bool browse = string.IsNullOrEmpty(_inv.state) || _inv.state == "ShopCusorOn" || _inv.state == "detail";
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        if (browse)
        {
            if ((kb != null && kb.oKey.wasPressedThisFrame) || (pad != null && !pad.leftTrigger.isPressed && pad.selectButton.wasPressedThisFrame))
            {
                CycleFilter();
            }
            if ((kb != null && kb.hKey.wasPressedThisFrame) || (pad != null && pad.leftTrigger.isPressed && pad.buttonWest.wasPressedThisFrame))
            {
                RefillQuickSlots();
            }
        }
        ApplyDim();
        RefreshText(browse);
    }

    private void OnDestroy()
    {
        RestoreDim();
    }

    // ---- 물약 충전 ----------------------------------------------------------------------------------------------

    /// <summary>퀵슬롯의 소비품(이 상점이 파는 것)을 최대 수량까지 채운다. 산 총 개수</summary>
    public static int RefillQuickSlots()
    {
        InventoryManager inv = InventoryManager.instance;
        QuickSlotModel quick = QuickSlotBar.Model;
        DatabaseManager db = DatabaseManager.instance;
        if (inv == null || quick == null || db == null)
        {
            return 0;
        }
        int candidates = 0, full = 0, totalBought = 0;
        ShopRules.BuyBlock lastBlock = ShopRules.BuyBlock.None;
        inv.BeginSaveBatch();
        try
        {
            for (int slot = 0; slot < QuickSlotModel.SlotCount; slot++)
            {
                string id = quick[slot];
                if (id == null || !ShopStockExtender.Sells(id))
                {
                    continue;
                }
                ItemData data = db.LoadItemData(db.FindItemDataIndex(id));
                if (data == null || data.type != "Consum")
                {
                    continue;
                }
                candidates++;
                int price = ShopRules.BuyPrice(data.price);
                int have = inv.Service.ScanModel().Count(id);
                int space = ShopManager.BagSpaceFor(id);
                int amount = ShopRules.RefillAmount(have, ShopRules.RefillTarget(data.maxStack), price, DatabaseManager.money, space);
                if (amount <= 0)
                {
                    if (have >= ShopRules.RefillTarget(data.maxStack))
                    {
                        full++;
                    }
                    else
                    {
                        lastBlock = ShopRules.CheckBuy(price, DatabaseManager.money, space);
                    }
                    continue;
                }
                data.tear = 0;
                data.upgrade = 0;
                if (!inv.Service.Add(id, amount, AddSource.Shop, data))
                {
                    lastBlock = ShopRules.BuyBlock.BagFull;
                    continue;
                }
                DatabaseManager.money -= price * amount;
                totalBought += amount;
                PickupLog.Report(data.itemNameT, amount, data.rarity);
                GameLog.Info(GameLog.Cat.Item, "물약 충전: " + id + " +" + amount + " (-" + price * amount + "G)");
            }
            inv.SaveInventory();
        }
        finally
        {
            inv.EndSaveBatch();
        }
        if (totalBought > 0)
        {
            Sfx.Play(SfxId.Money);
        }
        else if (candidates == 0)
        {
            PickupLog.Warn("퀵슬롯에 이 상점이 파는 소비품이 없다");
        }
        else if (full == candidates)
        {
            PickupLog.Warn("퀵슬롯 물약이 이미 가득하다");
        }
        else
        {
            PickupLog.Warn(lastBlock == ShopRules.BuyBlock.BagFull ? "가방이 가득 차 충전하지 못했다" : "골드가 모자라 충전하지 못했다");
        }
        return totalBought;
    }

    // ---- 필터 표시 ----------------------------------------------------------------------------------------------

    /// <summary>필터 밖 아이콘을 어둡게. 필터·페이지가 바뀌면 바로, 아니면 0.25초마다 다시 훑는다(구매·이동으로 생긴 칸 반영)</summary>
    private void ApplyDim()
    {
        if (s_filter == SellFilter.All)
        {
            RestoreDim();
            return;
        }
        bool changed = !_dimmed || _dimPage != _inv.nowBox || _dimFilter != s_filter;
        if (!changed && Time.unscaledTime < _nextDim)
        {
            return;
        }
        _nextDim = Time.unscaledTime + 0.25f;
        if (_dimmed && changed)
        {
            RestoreDim();
        }
        GameObject page = _inv.inventoryUI != null && _inv.nowBox >= 0 && _inv.nowBox < _inv.inventoryUI.Length ? _inv.inventoryUI[_inv.nowBox] : null;
        if (page == null)
        {
            return;
        }
        foreach (Transform box in page.transform)
        {
            ItemCheck item = box.childCount > 0 ? box.GetChild(0).GetComponent<ItemCheck>() : null;
            if (item == null || item.image == null || item.itemData == null)
            {
                continue;
            }
            Color want = ShopRules.FilterMatches(s_filter, item.itemData.type) ? Color.white : Dim;
            if (item.image.color != want)
            {
                item.image.color = want;
            }
        }
        _dimmed = true;
        _dimPage = _inv.nowBox;
        _dimFilter = s_filter;
    }

    private void RestoreDim()
    {
        if (!_dimmed)
        {
            return;
        }
        _dimmed = false;
        _dimPage = -1;
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || inv.inventoryUI == null)
        {
            return;
        }
        foreach (GameObject page in inv.inventoryUI)
        {
            if (page == null)
            {
                continue;
            }
            foreach (Transform box in page.transform)
            {
                ItemCheck item = box.childCount > 0 ? box.GetChild(0).GetComponent<ItemCheck>() : null;
                if (item != null && item.image != null && item.image.color == Dim)
                {
                    item.image.color = Color.white;
                }
            }
        }
    }

    // ---- 판매가 미리보기 · 안내 ----------------------------------------------------------------------------------

    private void RefreshText(bool browse)
    {
        if (_shownVisible && Time.unscaledTime < _nextText)
        {
            return; // 커서 아이템 조회(GetComponent)는 0.1초마다만
        }
        _nextText = Time.unscaledTime + 0.1f;
        ItemCheck item = null;
        if (browse && (string.IsNullOrEmpty(_inv.state) || _inv.state == "detail"))
        {
            item = _inv.state == "detail" ? _inv.detail : _inv.CursorItem();
        }
        int count = item != null ? item.nowStack : -1;
        int gold = DatabaseManager.money;
        if (_shownVisible && item == _shownItem && count == _shownCount && gold == _shownGold && s_filter == _shownFilter)
        {
            return;
        }
        _shownVisible = true;
        _shownItem = item;
        _shownCount = count;
        _shownGold = gold;
        _shownFilter = s_filter;
        string line1 = "<color=#A0A0A0>판매 필터 </color><color=#FFD480>[" + ShopRules.FilterLabel(s_filter) + "]</color>"
            + "<color=#A0A0A0>  O 바꾸기</color>" + System.Environment.NewLine + "<color=#A0A0A0>H 퀵슬롯 물약 충전  ·  [ ] 되사기 고르기</color>";
        SetText(item != null && item.itemData != null ? line1 + "\n" + SalePreview(item) : line1);
    }

    private static string SalePreview(ItemCheck item)
    {
        ItemData d = item.itemData;
        string name = TooltipBuilder.Name(d.itemNameT, d.rarity, d.upgrade);
        if (d.isLocked)
        {
            return name + " <color=#FF8060>잠금 - 판매할 수 없다 (L 해제)</color>";
        }
        if (!ItemTradeRules.CanSell(item.name))
        {
            return name + " <color=#FF8060>판매할 수 없는 물건</color>";
        }
        if (!ShopRules.FilterMatches(s_filter, d.type))
        {
            return name + " <color=#808080>필터(" + ShopRules.FilterLabel(s_filter) + ") 밖 - 판매 안 됨</color>";
        }
        long total = ShopRules.SaleTotal(d.price, item.nowStack);
        return name + " <color=#FFE08A>개당 " + d.price.ToString("N0") + "G  x" + item.nowStack + " = " + total.ToString("N0") + "G</color>";
    }

    private void SetText(string text)
    {
        if (_text == null)
        {
            if (text == null)
            {
                return;
            }
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 64;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            _text = new GameObject("ShopHint", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            _text.transform.SetParent(transform, false);
            _text.font = UiTheme.Get().pixelFont;
            _text.fontSize = 19f;
            _text.alignment = TextAlignmentOptions.Center;
            _text.raycastTarget = false;
            _text.outlineWidth = 0.3f;
            RectTransform rect = _text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.245f, 0.34f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(800f, 100f);
        }
        if (text == null)
        {
            if (_text.gameObject.activeSelf)
            {
                _text.gameObject.SetActive(false);
            }
            _shownVisible = false;
            return;
        }
        if (!_text.gameObject.activeSelf)
        {
            _text.gameObject.SetActive(true);
        }
        _text.text = text;
    }
}
