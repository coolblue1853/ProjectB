using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 신규 장비 슬롯(칭호·오라·부적·망토) 런타임 상태 (EquipSlotExpansion.md). 순수 규칙은 SlotRules, 여기는 ItemData 보관·ES3 저장·인벤 연결.
/// 기본 8칸 장비 경로(equipBox·equipGear)는 건드리지 않는다. 장착 판정은 ItemSheet equipArea 가 Title/Aura/Charm/Cape 인 아이템만.
/// </summary>
public static class ExtraEquip
{
    static readonly ItemData[] Items = new ItemData[4];
    static int _mask;
    static bool _loaded;

    public static event Action Changed;

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
            case EquipSlot.Title: return Loc.T("Locked: complete the Floor 1 bestiary or first-clear a Floor 1 boss");
            case EquipSlot.Aura: return Loc.T("Locked: first-clear a Floor 2 boss");
            case EquipSlot.Charm: return Loc.T("Locked: complete the Floor 3 bestiary or finish its quest chain");
            case EquipSlot.Cape: return Loc.T("Locked: first-clear a Floor 4 boss");
            default: return "";
        }
    }

    public static string SlotLabel(EquipSlot s)
    {
        return Loc.T(s == EquipSlot.Title ? "TITLE" : s == EquipSlot.Aura ? "AURA" : s == EquipSlot.Charm ? "CHARM" : "CAPE");
    }

    /// <summary>인벤의 신규 슬롯 장비를 장착한다. 대상 부위가 아니면 false(기본 경로로), 잠금이면 true(처리됨·장착 안 함)</summary>
    public static bool TryHandleEquip(ItemCheck item)
    {
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
            inv.CreatItem(old.name, false, old, AddSource.Unequip);
        }
        Save();
        inv.SaveInventory();
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

    /// <summary>세이브 로드(키 없음·손상 → 전부 잠금·빈칸). 인벤 로드 직후 호출</summary>
    public static void Load()
    {
        _loaded = true;
        _mask = 0;
        Array.Clear(Items, 0, Items.Length);
        try
        {
            if (ES3.KeyExists(SlotRules.SaveKeyUnlock))
            {
                _mask = ES3.Load<int>(SlotRules.SaveKeyUnlock) & SlotRules.AllExtraMask;
            }
            string[] ids = ES3.KeyExists(SlotRules.SaveKeyExtra) ? ES3.Load<string[]>(SlotRules.SaveKeyExtra) : null;
            DatabaseManager db = DatabaseManager.instance;
            for (int i = 0; ids != null && i < ids.Length && i < Items.Length && db != null; i++)
            {
                ItemStack st = SlotRules.DecodeStack(ids[i]);
                int index = st.IsEmpty ? -1 : db.FindItemDataIndex(st.Id);
                if (index < 0 || !SlotRules.IsUnlocked(_mask, SlotRules.Extras[i]))
                {
                    continue;
                }
                ItemData d = db.LoadItemData(index);
                d.tear = st.Tier;
                d.upgrade = st.Upgrade;
                Items[i] = d;
            }
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "신규 장비 슬롯 세이브 읽기 실패, 잠금·빈칸으로 시작: " + e.Message);
            _mask = 0;
            Array.Clear(Items, 0, Items.Length);
        }
        Changed?.Invoke();
    }

    static void Save()
    {
        var ids = new string[Items.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = Items[i] == null ? "" : SlotRules.EncodeStack(new ItemStack(Items[i].name, 1, Items[i].tear, Items[i].upgrade));
        }
        try
        {
            ES3.Save(SlotRules.SaveKeyExtra, ids);
            ES3.Save(SlotRules.SaveKeyUnlock, _mask);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "신규 장비 슬롯 저장 실패: " + e.Message);
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
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -8f);
        rt.sizeDelta = new Vector2(520f, 76f);
        for (int i = 0; i < 4; i++)
        {
            var box = new GameObject("Box" + i, typeof(RectTransform), typeof(Image));
            box.transform.SetParent(transform, false);
            box.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
            var br = (RectTransform)box.transform;
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0f, 1f);
            br.anchoredPosition = new Vector2(8f + i * 128f, -6f);
            br.sizeDelta = new Vector2(120f, 40f);
            box.AddComponent<ExtraSlotBox>().Init(this, SlotRules.Extras[i]);
            _labels[i] = NewText("Label", box.transform, 18f, new Vector2(120f, 40f), Vector2.zero);
            _labels[i].alignment = TextAlignmentOptions.Center;
        }
        _tip = NewText("Tip", transform, 16f, new Vector2(504f, 24f), new Vector2(8f, -48f));
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
