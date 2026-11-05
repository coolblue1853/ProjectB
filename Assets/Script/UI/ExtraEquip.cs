using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 신규 장비 슬롯(귀걸이·팔찌·부적·마법석) 런타임 상태 (EquipSlotExpansion.md). 순수 규칙은 SlotRules, 여기는 ItemData 보관·ES3 저장·인벤 연결.
/// 기본 8칸 장비 경로(equipBox·equipGear)는 건드리지 않는다. 장착 판정은 ItemSheet equipArea 가 Earring/Bracelet/Charm/Stone 인 아이템만.
/// </summary>
public static class ExtraEquip
{
    static readonly ItemData[] Items = new ItemData[4];
    static int _mask;
    static readonly object[] StatSources = { new object(), new object(), new object(), new object() };
    static bool _loaded;

    public static event Action Changed;

    /// <summary>슬롯별 장착 아이템 보조 스탯을 PlayerStats 에 다시 건다 (R4)</summary>
    static void ApplyStats()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            ExtraEquipStats.Apply(StatSources[i], Items[i] == null ? null : Items[i].name);
        }
    }

    static int Idx(EquipSlot s) => Array.IndexOf(SlotRules.Extras, s);

    public static bool IsUnlocked(EquipSlot s)
    {
        EnsureLoaded();
        return SlotRules.IsUnlocked(_mask, s);
    }

    public static ItemData Get(EquipSlot s)
    {
        EnsureLoaded();
        int i = Idx(s);
        return i < 0 ? null : Items[i];
    }

    /// <summary>해금(영구). 새로 열렸으면 true. SLOT-3 보상 연결이 부른다</summary>
    public static bool Unlock(EquipSlot s)
    {
        EnsureLoaded();
        if (!SlotRules.IsExtra(s) || SlotRules.IsUnlocked(_mask, s))
        {
            return false;
        }
        _mask = SlotRules.Unlock(_mask, s);
        Save();
        Changed?.Invoke();
        return true;
    }

    /// <summary>잠금 툴팁: 해금 조건 한 줄 (설계 §3, 임시 가정)</summary>
    public static string UnlockHint(EquipSlot s)
    {
        switch (s)
        {
            case EquipSlot.Earring: return Loc.T("Locked: complete the Floor 1 bestiary or first-clear a Floor 1 boss");
            case EquipSlot.Bracelet: return Loc.T("Locked: first-clear a Floor 2 boss");
            case EquipSlot.Charm: return Loc.T("Locked: complete the Floor 3 bestiary or finish its quest chain");
            case EquipSlot.Stone: return Loc.T("Locked: first-clear a Floor 4 boss");
            default: return "";
        }
    }

    public static string SlotLabel(EquipSlot s)
    {
        return Loc.T(s == EquipSlot.Earring ? "EARRING" : s == EquipSlot.Bracelet ? "BRACELET" : s == EquipSlot.Charm ? "CHARM" : "STONE");
    }

    /// <summary>11-05 폐기된 부위 이름(Title·Aura·Cape). 순수 판정</summary>
    public static bool IsDiscardedArea(string equipArea)
    {
        return equipArea == "Title" || equipArea == "Aura" || equipArea == "Cape";
    }

    /// <summary>인벤의 신규 슬롯 장비를 장착한다. 대상 부위가 아니면 false(기본 경로로), 잠금이면 true(처리됨·장착 안 함)</summary>
    public static bool TryHandleEquip(ItemCheck item)
    {
        if (item != null && item.itemData != null && IsDiscardedArea(item.itemData.equipArea))
        {
            GameLog.Info(GameLog.Cat.Item, "폐기된 부위 아이템은 장착 불가: " + item.itemData.equipArea);
            return true; // 처리됨: 기본 장착 경로로 넘기지 않는다
        }
        if (item == null || item.itemData == null || !Enum.TryParse(item.itemData.equipArea, out EquipSlot slot) || !SlotRules.IsExtra(slot))
        {
            return false;
        }
        InventoryManager inv = InventoryManager.instance;
        if (!IsUnlocked(slot) || inv == null)
        {
            GameLog.Info(GameLog.Cat.Item, "잠긴 슬롯: " + slot);
            return true;
        }
        ItemData data = item.itemData.Clone();
        data.isNew = false;
        if (!inv.Service.RemoveAt(item, 1))
        {
            return true;
        }
        ItemData old = Items[Idx(slot)];
        Items[Idx(slot)] = data;
        if (old != null)
        {
            ReturnOrRefund(old);
        }
        Save();
        inv.SaveInventory();
        ApplyStats();
        Changed?.Invoke();
        return true;
    }

    /// <summary>해제 → 인벤. 인벤이 가득이면 거절</summary>
    public static bool Unequip(EquipSlot slot)
    {
        EnsureLoaded();
        InventoryManager inv = InventoryManager.instance;
        ItemData d = Get(slot);
        if (d == null || inv == null || !inv.CreatItem(d.name, false, d, AddSource.Unequip))
        {
            return false;
        }
        Items[Idx(slot)] = null;
        Save();
        ApplyStats();
        Changed?.Invoke();
        return true;
    }

    static void EnsureLoaded()
    {
        if (!_loaded)
        {
            Load();
        }
    }

    /// <summary>테스트용 주입점: 가방 반환(true=가방에 들어감). null 이면 InventoryManager 사용</summary>
    public static Func<ItemData, bool> BagReturn;
    /// <summary>테스트용 주입점: 저장 대상 교체(ids, mask). null 이면 ES3</summary>
    public static Action<string[], int> SaveSink;
    static bool _saveBlocked;

    /// <summary>읽기 실패로 Save 를 막고 있는가 (손상 세이브를 빈 값으로 덮지 않으려는 보호)</summary>
    public static bool SaveBlocked => _saveBlocked;

    /// <summary>세이브 로드(키 없음 → 전부 잠금·빈칸, 읽기 실패 → 메모리만 비우고 Save 차단). 인벤 로드 직후 호출</summary>
    public static void Load()
    {
        _loaded = true;
        _saveBlocked = false;
        _mask = 0;
        Array.Clear(Items, 0, Items.Length);
        int rawMask = 0;
        string[] ids = null;
        bool read = false;
        try
        {
            rawMask = SaveUtil.LoadOrThrow(SlotRules.SaveKeyUnlock, 0);
            ids = SaveUtil.LoadOrThrow<string[]>(SlotRules.SaveKeyExtra, null);
            read = true;
        }
        catch (Exception e)
        {
            _saveBlocked = true;
            GameLog.Warn(GameLog.Cat.Item, "신규 장비 슬롯 세이브 읽기 실패, 저장 차단: " + e.Message);
        }
        if (read)
        {
            DatabaseManager db = DatabaseManager.instance;
            LoadCore(rawMask, ids, db == null ? (Func<ItemStack, ItemData>)null : st => ResolveStack(db, st));
        }
        ApplyStats();
        Changed?.Invoke();
    }

    /// <summary>테스트 정리용: 주입점·차단 플래그를 초기화하고 다음 접근 때 세이브를 다시 읽게 한다</summary>
    public static void ResetForTests()
    {
        BagReturn = null;
        SaveSink = null;
        _saveBlocked = false;
        _loaded = false;
        _mask = 0;
        Array.Clear(Items, 0, Items.Length);
        ApplyStats();
    }

    static ItemData ResolveStack(DatabaseManager db, ItemStack st)
    {
        int index = st.IsEmpty ? -1 : db.FindItemDataIndex(st.Id);
        return index < 0 ? null : db.LoadItemData(index);
    }

    /// <summary>로드 본체(테스트 가능). 폐기 장신구를 반환했으면 칸을 비운 채 즉시 저장해 재지급을 막는다</summary>
    public static void LoadCore(int rawMask, string[] ids, Func<ItemStack, ItemData> resolve)
    {
        _loaded = true;
        _mask = rawMask & SlotRules.AllExtraMask;
        Array.Clear(Items, 0, Items.Length);
        bool dirty = false;
        bool bagTouched = false;
        try
        {
            for (int i = 0; ids != null && resolve != null && i < ids.Length && i < Items.Length; i++)
            {
                ItemStack st = SlotRules.DecodeStack(ids[i]);
                ItemData d = st.IsEmpty ? null : resolve(st);
                if (d == null || !SlotRules.IsUnlocked(_mask, SlotRules.Extras[i]))
                {
                    continue;
                }
                if (!Enum.TryParse(d.equipArea, out EquipSlot want) || want != SlotRules.Extras[i])
                {
                    // 폐기된 칭호·오라 등(11-05): 장착 해제하고 인벤으로 반환, 가득이면 판매값 골드 지급
                    bagTouched |= ReturnOrRefund(d);
                    dirty = true;
                    continue;
                }
                d.tear = st.Tier;
                d.upgrade = st.Upgrade;
                Items[i] = d;
            }
        }
        catch (Exception e)
        {
            // 해금 mask 는 이미 읽었으므로 유지한다. 일부 칸만 못 읽었을 수 있어 Save 를 막아 세이브 덮어쓰기를 방지
            _saveBlocked = true;
            GameLog.Warn(GameLog.Cat.Item, "신규 장비 슬롯 세이브 해석 실패, 저장 차단: " + e.Message);
        }
        if (dirty && !_saveBlocked)
        {
            if (bagTouched && BagReturn == null && InventoryManager.instance != null)
            {
                InventoryManager.instance.SaveInventory();
            }
            Save();
        }
    }

    /// <summary>가방에 넣고, 실패하면 판매값 골드로 보상. 가방에 들어갔으면 true</summary>
    static bool ReturnOrRefund(ItemData d)
    {
        InventoryManager inv = InventoryManager.instance;
        bool ok = BagReturn != null ? BagReturn(d) : inv != null && inv.CreatItem(d.name, false, d, AddSource.Unequip);
        if (ok)
        {
            return true;
        }
        DatabaseManager.money += Mathf.Max(0, d.price / 2);
        GameLog.Warn(GameLog.Cat.Item, "장신구 반환 실패(가방 가득/없음), 판매값 골드 지급: " + d.name);
        return false;
    }

    /// <summary>해금 첫 아이템 지급: 가방이 가득이면 판매값 골드로 대체(해금만 남고 아이템이 사라지는 것 방지)</summary>
    public static void GrantFirstItem(string itemName)
    {
        DatabaseManager db = DatabaseManager.instance;
        int index = db == null ? -1 : db.FindItemDataIndex(itemName);
        if (index < 0)
        {
            InventoryManager.instance?.CreatItem(itemName, false, null, AddSource.Other);
            return;
        }
        ItemData d = db.LoadItemData(index);
        InventoryManager inv = InventoryManager.instance;
        bool ok = BagReturn != null ? BagReturn(d) : inv != null && inv.CreatItem(itemName, false, null, AddSource.Other);
        if (!ok)
        {
            DatabaseManager.money += Mathf.Max(0, d.price / 2);
            GameLog.Warn(GameLog.Cat.Item, "해금 첫 아이템 지급 실패, 판매값 골드 지급: " + itemName);
        }
    }

    static void Save()
    {
        if (_saveBlocked)
        {
            return;
        }
        var ids = new string[Items.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = Items[i] == null ? "" : SlotRules.EncodeStack(new ItemStack(Items[i].name, 1, Items[i].tear, Items[i].upgrade));
        }
        try
        {
            if (SaveSink != null)
            {
                SaveSink(ids, _mask);
                return;
            }
            ES3.Save(SlotRules.SaveKeyExtra, ids);
            ES3.Save(SlotRules.SaveKeyUnlock, _mask);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "신규 장비 슬롯 저장 실패: " + e.Message);
        }
    }
}

/// <summary>해금 보상 연결 (SLOT-3): 해금되면 첫 아이템을 인벤에 지급한다. 같은 슬롯은 한 번만 열리므로 중복 지급 없음</summary>
public static class ExtraEquipRewards
{
    public const string BestiaryEarringMilestone = "B04";
    static readonly string[] FirstItem = { "EarringPlain", "BraceletPlain", "CharmPlain", "StonePlain" };

    public static void OnFloorBossFirstKill(int floor)
    {
        if (SlotRules.TrySlotForFloor(floor, out EquipSlot slot))
        {
            Grant(slot);
        }
    }

    public static void OnBestiaryMilestone(string id)
    {
        if (id == BestiaryEarringMilestone)
        {
            Grant(EquipSlot.Earring);
        }
    }

    public static void Grant(EquipSlot slot)
    {
        if (!ExtraEquip.Unlock(slot))
        {
            return;
        }
        PickupLog.Notice(Loc.T("New equipment slot unlocked: ") + ExtraEquip.SlotLabel(slot));
        int i = Array.IndexOf(SlotRules.Extras, slot);
        string item = i >= 0 ? FirstItem[i] : "";
        if (item.Length > 0 && InventoryManager.instance != null)
        {
            ExtraEquip.GrantFirstItem(item);
        }
    }
}

/// <summary>
/// 장비창 옆 신규 4칸 (코드 생성 임시 UI): 잠금 표시·호버 툴팁·클릭 해제. 게임패드·키보드 커서 이동은 후속(TechDebt).
/// InventoryManager.inventory 아래에 한 번 만든다.
/// </summary>
public sealed class ExtraEquipPanel : MonoBehaviour
{
    readonly TextMeshProUGUI[] _labels = new TextMeshProUGUI[4];
    TextMeshProUGUI _tip;

    public static void Ensure(Transform parent)
    {
        if (parent == null || parent.Find("ExtraEquipPanel") != null)
        {
            return;
        }
        var go = new GameObject("ExtraEquipPanel", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);
        go.AddComponent<ExtraEquipPanel>().Build();
    }

    void Build()
    {
        var rt = (RectTransform)transform;
        // 장비 그리드(BackGround) 아래 빈 띠에 맞춤: 인벤 칸·프리셋 박스를 가리지 않는다
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(-332f, -80f);
        rt.sizeDelta = new Vector2(290f, 66f);
        for (int i = 0; i < 4; i++)
        {
            var box = new GameObject("Box" + i, typeof(RectTransform), typeof(Image));
            box.transform.SetParent(transform, false);
            box.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
            var br = (RectTransform)box.transform;
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0f, 1f);
            br.anchoredPosition = new Vector2(4f + i * 71f, -4f);
            br.sizeDelta = new Vector2(67f, 26f);
            box.AddComponent<ExtraSlotBox>().Init(this, SlotRules.Extras[i]);
            _labels[i] = NewText("Label", box.transform, 10f, new Vector2(67f, 26f), Vector2.zero);
            _labels[i].alignment = TextAlignmentOptions.Center;
            _labels[i].enableAutoSizing = true;
            _labels[i].fontSizeMax = 10f;
            _labels[i].fontSizeMin = 6f;
            _labels[i].overflowMode = TextOverflowModes.Ellipsis;
        }
        _tip = NewText("Tip", transform, 9f, new Vector2(282f, 26f), new Vector2(4f, -34f));
        _tip.enableWordWrapping = true;
        _tip.color = new Color(1f, 1f, 1f, 0.7f);
        ExtraEquip.Changed += Refresh;
        Refresh();
    }

    static TextMeshProUGUI NewText(string name, Transform parent, float size, Vector2 sizeDelta, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        var r = t.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
        r.anchoredPosition = pos;
        r.sizeDelta = sizeDelta;
        return t;
    }

    void OnDestroy()
    {
        ExtraEquip.Changed -= Refresh;
    }

    public void ShowTip(string text)
    {
        if (_tip != null)
        {
            _tip.text = text;
        }
    }

    void Refresh()
    {
        for (int i = 0; i < _labels.Length; i++)
        {
            EquipSlot s = SlotRules.Extras[i];
            ItemData d = ExtraEquip.Get(s);
            _labels[i].text = !ExtraEquip.IsUnlocked(s) ? "[X] " + ExtraEquip.SlotLabel(s)
                : d != null ? (string.IsNullOrEmpty(d.itemNameT) ? d.name : d.itemNameT) : ExtraEquip.SlotLabel(s);
        }
    }
}

/// <summary>신규 슬롯 한 칸: 호버 → 툴팁, 클릭 → 해제</summary>
public sealed class ExtraSlotBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    ExtraEquipPanel _panel;
    EquipSlot _slot;

    public void Init(ExtraEquipPanel panel, EquipSlot slot)
    {
        _panel = panel;
        _slot = slot;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        ItemData d = ExtraEquip.Get(_slot);
        _panel.ShowTip(!ExtraEquip.IsUnlocked(_slot) ? ExtraEquip.UnlockHint(_slot) : d != null ? d.description : ExtraEquip.SlotLabel(_slot));
    }

    public void OnPointerExit(PointerEventData e)
    {
        _panel.ShowTip("");
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (ExtraEquip.IsUnlocked(_slot) && ExtraEquip.Get(_slot) != null)
        {
            ExtraEquip.Unequip(_slot);
        }
    }
}
