using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 추가 장비 슬롯(귀걸이·팔찌·보조장비(Charm)·마법석·허리띠) 런타임 상태 (EquipSlotExpansion.md, v2). 순수 규칙은 SlotRules, 여기는 ItemData 보관·ES3 저장·인벤 연결.
/// 의상·무기·반지·목걸이 경로(equipBox·equipGear)는 건드리지 않는다. 장착 판정은 ItemSheet equipArea 가 Earring/Bracelet/Charm/Stone/Belt 인 아이템만.
/// 귀걸이는 시작부터 열려 있고, 나머지는 SlotUnlocks.csv 의 훅(ExtraEquipRewards.TriggerHook)으로 해금한다.
/// </summary>
public static class ExtraEquip
{
    static readonly ItemData[] Items = new ItemData[SlotRules.Extras.Length];
    static int _mask;
    static readonly object[] StatSources = { new object(), new object(), new object(), new object(), new object() };
    static bool _loaded;

    public static event Action Changed;

    static readonly string[] SetCounted = new string[SlotRules.Extras.Length]; // 세트 수에 더해 둔 tfName(귀걸이만)
    static readonly int[] HpApplied = new int[SlotRules.Extras.Length];
    static readonly string[] HpKey = new string[SlotRules.Extras.Length];

    /// <summary>슬롯별 장착 아이템 보조 스탯을 PlayerStats 에 다시 건다 (R4). 귀걸이는 세트 수에도 센다(시작 5칸)</summary>
    static void ApplyStats()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            ExtraEquipStats.Apply(StatSources[i], Items[i] == null ? null : Items[i].name);
            ExtraEquipPrefabStats.Apply(StatSources[i], Items[i], ref HpKey[i], ref HpApplied[i]); // 옛 투구·신발 프리팹 스탯 보존(표에 없는 아이템)
            // 효과 서비스 출처 연결: 아이템의 effectIds(없으면 Detach)
            SkillEffectService.AttachIds("Extra:" + i, Items[i] == null ? null : Items[i].effectIds, Items[i] == null ? null : Items[i].rarity);
            SyncSetCount(i);
        }
    }

    /// <summary>귀걸이 칸의 세트 키(tfName)를 DatabaseManager 세트 카운터와 맞춘다. 같은 값이면 아무것도 하지 않는다(멱등)</summary>
    static void SyncSetCount(int i)
    {
        string want = SlotRules.Extras[i] == EquipSlot.Earring && Items[i] != null ? Items[i].tfName : null;
        if (string.IsNullOrEmpty(want))
        {
            want = null;
        }
        if (want == SetCounted[i] || DatabaseManager.instance == null)
        {
            return;
        }
        if (SetCounted[i] != null)
        {
            DatabaseManager.MinusSetDict(SetCounted[i], 1);
        }
        if (want != null)
        {
            DatabaseManager.PlusSetDict(want, 1);
        }
        SetCounted[i] = want;
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

    /// <summary>잠금 툴팁: 해금 층계 한 줄 (층계는 SlotUnlocks.csv)</summary>
    public static string UnlockHint(EquipSlot s)
    {
        SlotUnlockRow r = SlotUnlockTable.Current.ForKey(s.ToString());
        return r == null ? "" : Loc.F("Locked: advance the Floor {0} story", r.Floor);
    }

    /// <summary>별도 콘텐츠(펫·룬·성물) 잠금 툴팁</summary>
    public static string ContentUnlockHint(string key)
    {
        SlotUnlockRow r = SlotUnlockTable.Current.ForKey(key);
        return r == null ? "" : Loc.F("Locked: advance the Floor {0} story", r.Floor);
    }

    public static string SlotLabel(EquipSlot s)
    {
        switch (s)
        {
            case EquipSlot.Earring: return Loc.T("EARRING");
            case EquipSlot.Bracelet: return Loc.T("BRACELET");
            case EquipSlot.Charm: return Loc.T("SUBGEAR");
            case EquipSlot.Belt: return Loc.T("BELT");
            default: return Loc.T("STONE");
        }
    }

    /// <summary>11-05 폐기된 부위 이름(Title·Aura·Cape). 순수 판정</summary>
    public static bool IsDiscardedArea(string equipArea)
    {
        return equipArea == "Title" || equipArea == "Aura" || equipArea == "Cape" || SlotRules.IsRetiredArea(equipArea); // 투구·장갑·하의·신발은 v2 에서 슬롯 폐지
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

    /// <summary>
    /// 구 세이브 이행용: 인벤을 거치지 않고 해당 슬롯에 직접 장착. 슬롯이 해금이고 비어 있을 때만 true(아니면 호출자가 인벤으로 돌려보낸다)
    /// </summary>
    public static bool TryPlaceDirect(ItemData data)
    {
        EnsureLoaded();
        if (data == null || !Enum.TryParse(data.equipArea, out EquipSlot slot) || !SlotRules.IsExtra(slot) || !IsUnlocked(slot) || Items[Idx(slot)] != null)
        {
            return false;
        }
        data.isNew = false;
        Items[Idx(slot)] = data;
        if (!Save())
        {
            Items[Idx(slot)] = null; // 저장 확인 전에는 장착으로 치지 않는다(호출자가 구 칸을 지우지 않게)
            return false;
        }
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
        Array.Clear(SetCounted, 0, SetCounted.Length);
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

    /// <summary>저장 성공 여부(읽기 실패 차단·ES3 예외면 false)</summary>
    static bool Save()
    {
        if (_saveBlocked)
        {
            return false;
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
                return true;
            }
            ES3.Save(SlotRules.SaveKeyExtra, ids);
            ES3.Save(SlotRules.SaveKeyUnlock, _mask);
            return true;
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "신규 장비 슬롯 저장 실패: " + e.Message);
            return false;
        }
    }
}

/// <summary>
/// 해금 훅 (SLOT-3, v2): 스토리 진행 호출처가 TriggerHook("boss:N" / "bestiary:B04")를 부르면 SlotUnlocks.csv 의 해당 줄을 해금한다.
/// 장비 슬롯은 ExtraEquip.Unlock + 첫 아이템 지급, 별도 콘텐츠(펫·룬·성물)는 ContentFeatures.Unlock. 같은 항목은 한 번만 열리므로 중복 지급 없음.
/// </summary>
public static class ExtraEquipRewards
{
    public const string BestiaryEarringMilestone = "B04"; // 이름은 구 호환: 지금은 1층계 해금(보조장비) 훅 bestiary:B04

    public static void OnFloorBossFirstKill(int floor)
    {
        TriggerHook("boss:" + floor);
    }

    public static void OnBestiaryMilestone(string id)
    {
        TriggerHook("bestiary:" + id);
    }

    /// <summary>훅 이름으로 해금. 새로 열렸으면 true</summary>
    public static bool TriggerHook(string hookId)
    {
        SlotUnlockRow row = SlotUnlockTable.Current.ForHook(hookId);
        if (row == null)
        {
            return false;
        }
        if (!row.IsSlot)
        {
            return ContentFeatures.Unlock(row.Key);
        }
        return Enum.TryParse(row.Key, out EquipSlot slot) && Grant(slot);
    }

    public static bool Grant(EquipSlot slot)
    {
        if (!ExtraEquip.Unlock(slot))
        {
            return false;
        }
        PickupLog.Notice(Loc.T("New equipment slot unlocked: ") + ExtraEquip.SlotLabel(slot));
        string item = slot + "Plain";
        DatabaseManager db = DatabaseManager.instance;
        if (db != null && db.FindItemDataIndex(item) >= 0 && InventoryManager.instance != null)
        {
            ExtraEquip.GrantFirstItem(item);
        }
        return true;
    }
}

/// <summary>
/// 장비창 아래 추가 5칸 (코드 생성 임시 UI): 잠금 표시·호버 툴팁·클릭 해제. 키보드·패드는 InventoryManager 장비 커서 격자의 마지막 줄(equipCusor 10~14)이 같은 칸을 가리킨다.
/// InventoryManager.inventory 아래에 한 번 만든다. 폐지된 4칸(투구·장갑·하의·신발)은 만들지 않는다.
/// </summary>
public sealed class ExtraEquipPanel : MonoBehaviour
{
    static ExtraEquipPanel _instance;
    static readonly EquipSlot[] DisplayOrder = { EquipSlot.Earring, EquipSlot.Charm, EquipSlot.Bracelet, EquipSlot.Belt, EquipSlot.Stone };
    readonly TextMeshProUGUI[] _labels = new TextMeshProUGUI[SlotRules.Extras.Length];
    readonly GameObject[] _boxes = new GameObject[SlotRules.Extras.Length];
    readonly GameObject[] _petBoxes = new GameObject[3];
    readonly TextMeshProUGUI[] _petLabels = new TextMeshProUGUI[3];
    TextMeshProUGUI _tip;
    GameObject _setBox;
    SetOverviewPopup _setPopup;

    /// <summary>장비 커서 격자에서 펫 줄(켜기·HP%·MP%)의 노드 번호 시작값. EquipSlot 값(10~14)과 겹치지 않는다</summary>
    public const int PetNodeBase = 20;

    /// <summary>펫 줄 오른쪽 끝 "SET" 칸(세트 창, REWORK-12 P6)의 커서 노드 번호</summary>
    public const int SetNode = PetNodeBase + 3;

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

    /// <summary>슬롯 칸 오브젝트(커서 위치용). 패널이 없으면 null</summary>
    public static GameObject BoxOf(EquipSlot slot)
    {
        int i = Array.IndexOf(SlotRules.Extras, slot);
        return _instance != null && i >= 0 ? _instance._boxes[i] : null;
    }

    /// <summary>장비 커서 노드(10~14 슬롯, 20~22 펫 줄)의 칸 오브젝트. 패널이 없으면 null</summary>
    public static GameObject BoxOfNode(int node)
    {
        if (node == SetNode)
        {
            return _instance != null ? _instance._setBox : null;
        }
        if (node >= PetNodeBase)
        {
            int k = node - PetNodeBase;
            return _instance != null && k < _instance._petBoxes.Length ? _instance._petBoxes[k] : null;
        }
        return BoxOf((EquipSlot)node);
    }

    /// <summary>키보드·패드 커서가 칸에 올라왔을 때 툴팁(호버와 같은 문구)</summary>
    public static void TipForNode(int node)
    {
        if (_instance != null)
        {
            _instance.ShowSetOverview(node == SetNode);
            if (node == SetNode)
            {
                _instance.ShowTip(SetTipText());
                return;
            }
            _instance.ShowTip(node >= PetNodeBase ? PetTipText(node - PetNodeBase) : node >= 10 ? SlotTipText((EquipSlot)node) : "");
        }
    }

    /// <summary>키보드·패드 선택·클릭: 슬롯 칸이면 해제, 펫 줄이면 자동 먹기 켜기/기준 % 바꾸기</summary>
    public static void ActivateNode(int node)
    {
        if (node == SetNode)
        {
            if (_instance != null)
            {
                _instance.ShowSetOverview(true); // 다시 계산해서 띄운다(장착 변경 직후 갱신용)
            }
            return;
        }
        if (node < PetNodeBase)
        {
            Activate((EquipSlot)node);
            return;
        }
        switch (node - PetNodeBase)
        {
            case 0: PetFeature.Toggle(); break;
            case 1: PetFeature.CycleHp(); break;
            default: PetFeature.CycleMp(); break;
        }
        if (_instance != null)
        {
            _instance.Refresh();
            _instance.ShowTip(PetTipText(node - PetNodeBase));
        }
    }

    public static string PetTipText(int k)
    {
        if (!PetFeature.IsUnlocked)
        {
            return ExtraEquip.ContentUnlockHint(ContentFeatures.Pet);
        }
        return k == 0 ? Loc.T("Pet: auto-drink potions (select to toggle)") : k == 1 ? Loc.T("Drink an HP potion at or below this % (select to change)") : Loc.T("Drink a stamina potion at or below this % (select to change)");
    }

    /// <summary>키보드·패드 선택·클릭: 잠금이 아니고 장착품이 있으면 해제(인벤이 가득이면 거절)</summary>
    public static void Activate(EquipSlot slot)
    {
        if (ExtraEquip.IsUnlocked(slot) && ExtraEquip.Get(slot) != null)
        {
            ExtraEquip.Unequip(slot);
        }
    }

    public static string SetTipText()
    {
        return "착용 세트 효과 (세트색 단계 = 켜짐)"; // 세트 문구는 SetSwapPreview 와 같이 한글 직접(Loc 용어 미등록)
    }

    /// <summary>세트 창 띄우기/숨기기: SET 칸 호버·커서일 때만 보인다</summary>
    public void ShowSetOverview(bool on)
    {
        if (_setPopup == null)
        {
            return;
        }
        if (on)
        {
            _setPopup.Show();
        }
        else
        {
            _setPopup.Hide();
        }
    }

    public static string SlotTipText(EquipSlot slot)
    {
        ItemData d = ExtraEquip.Get(slot);
        return !ExtraEquip.IsUnlocked(slot) ? ExtraEquip.UnlockHint(slot) : d != null ? d.description : ExtraEquip.SlotLabel(slot);
    }

    void Build()
    {
        _instance = this;
        var rt = (RectTransform)transform;
        // 장비 그리드(BackGround) 아래 빈 띠에 맞춤: 인벤 칸·프리셋 박스를 가리지 않는다
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(-332f, -80f);
        rt.sizeDelta = new Vector2(290f, 60f); // 아래 툴팁 줄은 패널 밖으로 나가 장비 프리셋 박스 위에 잠깐 겹친다
        int n = DisplayOrder.Length;
        float w = (282f - (n - 1) * 3f) / n; // 칸 너비: 패널 폭 안에 n 칸
        for (int i = 0; i < n; i++)
        {
            // 화면 순서 = 층계 순서(귀걸이 → 보조장비 1 → 팔찌 3 → 허리띠 5 → 마법석 7), 저장 순서(Extras)와는 별개
            int slotIndex = Array.IndexOf(SlotRules.Extras, DisplayOrder[i]);
            var box = new GameObject("Box" + slotIndex, typeof(RectTransform), typeof(Image));
            box.transform.SetParent(transform, false);
            box.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
            var br = (RectTransform)box.transform;
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0f, 1f);
            br.anchoredPosition = new Vector2(4f + i * (w + 3f), -4f);
            br.sizeDelta = new Vector2(w, 26f);
            box.AddComponent<ExtraSlotBox>().Init(this, SlotRules.Extras[slotIndex]);
            _boxes[slotIndex] = box;
            _labels[slotIndex] = NewText("Label", box.transform, 10f, new Vector2(w, 26f), Vector2.zero);
            _labels[slotIndex].alignment = TextAlignmentOptions.Center;
            _labels[slotIndex].enableAutoSizing = true;
            _labels[slotIndex].fontSizeMax = 10f;
            _labels[slotIndex].fontSizeMin = 6f;
            _labels[slotIndex].overflowMode = TextOverflowModes.Ellipsis;
        }
        string[] petNames = { "PetToggle", "PetHp", "PetMp" };
        float pw = (282f - 9f) / 4f; // 펫 3칸 + SET 1칸
        for (int k = 0; k < 3; k++)
        {
            var pbox = new GameObject(petNames[k], typeof(RectTransform), typeof(Image));
            pbox.transform.SetParent(transform, false);
            pbox.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
            var pr = (RectTransform)pbox.transform;
            pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0f, 1f);
            pr.anchoredPosition = new Vector2(4f + k * (pw + 3f), -33f);
            pr.sizeDelta = new Vector2(pw, 22f);
            pbox.AddComponent<ExtraSlotBox>().InitNode(this, PetNodeBase + k);
            _petBoxes[k] = pbox;
            _petLabels[k] = NewText("Label", pbox.transform, 10f, new Vector2(pw, 22f), Vector2.zero);
            _petLabels[k].alignment = TextAlignmentOptions.Center;
            _petLabels[k].enableAutoSizing = true;
            _petLabels[k].fontSizeMax = 10f;
            _petLabels[k].fontSizeMin = 6f;
            _petLabels[k].overflowMode = TextOverflowModes.Ellipsis;
        }
        BuildSetBox(pw);
        PetFeature.Changed += Refresh;
        ContentFeatures.Unlocked += OnContentUnlocked;
        _tip = NewText("Tip", transform, 9f, new Vector2(282f, 26f), new Vector2(4f, -58f));
        _tip.enableWordWrapping = true;
        _tip.color = new Color(1f, 1f, 1f, 0.7f);
        ExtraEquip.Changed += Refresh;
        Refresh();
    }

    /// <summary>펫 줄 오른쪽 끝 SET 칸 + 세트 창 팝업(기존 칸과 같은 반투명 스킨)</summary>
    private void BuildSetBox(float pw)
    {
        _setBox = new GameObject("SetOverview", typeof(RectTransform), typeof(Image));
        _setBox.transform.SetParent(transform, false);
        _setBox.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
        var sr = (RectTransform)_setBox.transform;
        sr.anchorMin = sr.anchorMax = sr.pivot = new Vector2(0f, 1f);
        sr.anchoredPosition = new Vector2(4f + 3f * (pw + 3f), -33f);
        sr.sizeDelta = new Vector2(pw, 22f);
        _setBox.AddComponent<ExtraSlotBox>().InitNode(this, SetNode);
        TextMeshProUGUI label = NewText("Label", _setBox.transform, 10f, new Vector2(pw, 22f), Vector2.zero);
        label.alignment = TextAlignmentOptions.Center;
        label.text = "세트";
        _setPopup = SetOverviewPopup.Create(transform);
    }

    private void OnDisable()
    {
        ShowSetOverview(false); // 장비창을 닫으면 세트 창도 닫는다
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
        PetFeature.Changed -= Refresh;
        ContentFeatures.Unlocked -= OnContentUnlocked;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public void ShowTip(string text)
    {
        if (_tip != null)
        {
            _tip.text = text;
        }
    }

    void OnContentUnlocked(string key)
    {
        Refresh();
    }

    void Refresh()
    {
        bool pet = PetFeature.IsUnlocked;
        _petLabels[0].text = !pet ? "[X] " + Loc.T("PET") : Loc.T("PET") + (PetFeature.Enabled ? " ON" : " OFF");
        _petLabels[1].text = "HP " + PetFeature.HpPercent + "%";
        _petLabels[2].text = "SP " + PetFeature.MpPercent + "%";
        if (_setPopup != null && _setPopup.IsShown)
        {
            _setPopup.Show(); // 열린 채 추가 칸 장착이 바뀌면 다시 계산
        }
        for (int i = 0; i < _labels.Length; i++)
        {
            EquipSlot s = SlotRules.Extras[i];
            ItemData d = ExtraEquip.Get(s);
            _labels[i].text = !ExtraEquip.IsUnlocked(s) ? "[X] " + ExtraEquip.SlotLabel(s)
                : d != null ? (string.IsNullOrEmpty(d.itemNameT) ? d.name : d.itemNameT) : ExtraEquip.SlotLabel(s);
        }
    }
}

/// <summary>추가 슬롯 한 칸: 호버 → 툴팁, 클릭 → 해제</summary>
public sealed class ExtraSlotBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    ExtraEquipPanel _panel;
    EquipSlot _slot;

    int _node = -1; // 펫 줄·SET 칸이면 장비 커서 노드 번호(20~23), 슬롯 칸이면 -1

    public void Init(ExtraEquipPanel panel, EquipSlot slot)
    {
        _panel = panel;
        _slot = slot;
    }

    public void InitNode(ExtraEquipPanel panel, int node)
    {
        _panel = panel;
        _node = node;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (_node == ExtraEquipPanel.SetNode)
        {
            _panel.ShowSetOverview(true);
            _panel.ShowTip(ExtraEquipPanel.SetTipText());
            return;
        }
        _panel.ShowTip(_node >= 0 ? ExtraEquipPanel.PetTipText(_node - ExtraEquipPanel.PetNodeBase) : ExtraEquipPanel.SlotTipText(_slot));
    }

    public void OnPointerExit(PointerEventData e)
    {
        _panel.ShowSetOverview(false);
        _panel.ShowTip("");
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (_node >= 0)
        {
            ExtraEquipPanel.ActivateNode(_node);
            return;
        }
        ExtraEquipPanel.Activate(_slot);
    }
}
