using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>획득 경로 (픽업 로그·퀘스트가 구분해서 쓴다)</summary>
public enum AddSource { Pickup, Craft, Shop, Chest, Unequip, Load, Other, Dismantle }

/// <summary>
/// Phase 3a (InventoryRefactorDesign 4절): 인벤토리 쓰기 명령을 모델(SlotContainer)로 처리하고, 바뀐 칸만 슬롯 GameObject 에 반영한다.
/// 드래그·키보드 교환처럼 아직 GameObject 를 직접 옮기는 경로가 남아 있어서, 명령 전에 SyncModelFromViews 로 모델을 맞춘다(양방향 브리지).
/// </summary>
public sealed class InventoryService
{
    readonly InventoryManager inv;
    readonly HashSet<int> dirty = new HashSet<int>();
    readonly Dictionary<int, ItemStack> hidden = new Dictionary<int, ItemStack>(); // 슬롯 박스가 없는 칸의 세이브 아이템(소실 방지용 보관)
    SlotContainer model;

    public static event Action<ItemStack, AddSource> OnItemAdded;

    public InventoryService(InventoryManager inv)
    {
        this.inv = inv;
    }

    int PerPage => inv.maxHor * inv.maxVer;

    /// <summary>슬롯을 다시 훑어 만든 모델(전체 스캔 + 할당이 있으니 매 프레임 부르지 말 것)</summary>
    public SlotContainer ScanModel()
    {
        SyncModelFromViews();
        return model;
    }

    /// <summary>
    /// 3c 준비(그림자 모델): 직전 명령 뒤의 모델과 지금 슬롯이 다른 칸 수 누계. 0 이 아니면 모델을 거치지 않고
    /// GameObject 를 직접 옮긴 경로가 있다는 뜻이다(창고↔인벤 칸 놓기, 장비칸↔인벤, 강화 등). 동작은 바꾸지 않고 기록만 한다.
    /// </summary>
    public int OutsideChanges { get; private set; }

    /// <summary>마지막으로 감지한 모델 밖 변경(첫 칸) 설명</summary>
    public string LastOutsideChange { get; private set; }

    /// <summary>UI 슬롯 → 모델. 잠긴 칸(maxBoxNum 밖)은 명령 대상에서 뺀다</summary>
    public void SyncModelFromViews()
    {
        SlotContainer scan = ViewScanner.BuildFrom(inv.inventoryUI, PerPage, CsvItemCatalog.Shared);
        if (model != null)
        {
            model.OnSlotChanged -= MarkDirty;
            DetectOutsideChanges(model, scan);
        }
        model = scan;
        model.Unlocked = Mathf.Clamp(inv.maxBoxNum, 0, model.Capacity);
        model.OnSlotChanged += MarkDirty;
        dirty.Clear();
    }

    private void DetectOutsideChanges(SlotContainer before, SlotContainer now)
    {
        if (before.Capacity != now.Capacity)
        {
            return;
        }
        int n = 0;
        string first = null;
        for (int i = 0; i < now.Capacity; i++)
        {
            ItemStack a = before[i], b = now[i];
            if (a.IsEmpty && b.IsEmpty)
            {
                continue;
            }
            if (a.Id != b.Id || a.Count != b.Count || a.Tier != b.Tier || a.Upgrade != b.Upgrade)
            {
                n++;
                if (first == null)
                {
                    first = i + ": " + a + " -> " + b;
                }
            }
        }
        if (n == 0)
        {
            return;
        }
        OutsideChanges += n;
        LastOutsideChange = first;
        GameLog.Info(GameLog.Cat.Item, "모델 밖 변경 " + n + "칸 (첫 칸 " + first + ")");
    }

    void MarkDirty(int index)
    {
        dirty.Add(index);
    }

    /// <summary>1개 이상 넣는다. 전부 들어갈 자리가 없으면 아무것도 넣지 않고 false</summary>
    public bool Add(string id, int count, AddSource source, ItemData data = null)
    {
        if (!CsvItemCatalog.Shared.Exists(id))
        {
            return false; // 시트에 없는 이름은 DrawSlot 이 그리지 못해 모델·뷰가 어긋난다(B-020)
        }
        SyncModelFromViews();
        var stack = new ItemStack(id, count, data != null ? data.tear : 0, data != null ? data.upgrade : 0);
        if (!model.TryAddAll(stack))
        {
            return false;
        }
        ApplyDirty(data);
        OnItemAdded?.Invoke(stack, source);
        return true;
    }

    /// <summary>page 페이지만 정렬한다(같은 스택 병합 → ItemSortComparer 순). 바뀐 칸만 다시 그린다</summary>
    public void SortPage(int page)
    {
        SyncModelFromViews();
        model.SortRange(page * PerPage, PerPage, new ItemSortComparer(CsvItemCatalog.Shared));
        ApplyDirty(null);
    }

    /// <summary>앞 칸부터 count 만큼 지운다. 모자라면 아무것도 지우지 않고 false</summary>
    public bool Remove(string id, int count)
    {
        SyncModelFromViews();
        if (!model.TryRemove(id, count))
        {
            return false;
        }
        ApplyDirty(null);
        return true;
    }

    /// <summary>슬롯 GameObject 에 놓인 아이템의 모델 칸 번호. 인벤토리 칸이 아니면 -1</summary>
    public int IndexOf(ItemCheck item)
    {
        if (item == null)
        {
            return -1;
        }
        return IndexOfBox(item.transform.parent);
    }

    /// <summary>인벤토리 슬롯 박스의 모델 칸 번호. 인벤토리 슬롯이 아니면 -1</summary>
    public int IndexOfBox(Transform box)
    {
        if (box == null)
        {
            return -1;
        }
        for (int p = 0; p < inv.inventoryUI.Length; p++)
        {
            if (box.parent == inv.inventoryUI[p].transform)
            {
                int slot = box.GetSiblingIndex();
                return slot < PerPage ? p * PerPage + slot : -1;
            }
        }
        return -1;
    }

    /// <summary>
    /// 인벤토리 칸 → 칸 이동(키보드 교환·드래그 놓기, 3b). 빈칸이면 이동, 같은 스택(id·티어·강화)이면 합치고 넘침은 원래 칸,
    /// 아니면 교환. 두 칸 중 하나라도 인벤토리 칸이 아니거나 같은 칸이면 false.
    /// </summary>
    public bool MoveSlot(Transform fromBox, Transform toBox)
    {
        int from = IndexOfBox(fromBox), to = IndexOfBox(toBox);
        if (from < 0 || to < 0 || from == to)
        {
            return false;
        }
        SyncModelFromViews();
        if (!model.Move(from, to))
        {
            return false;
        }
        ApplyDirty(null);
        return true;
    }

    /// <summary>
    /// 3c: 인벤 칸의 아이템 오브젝트를 통째로 다른 곳(장비칸 등)으로 꺼낸다. 모델에서 칸을 비우고 오브젝트는 파괴하지 않고
    /// newParent 로 옮긴다(장착 아이템은 같은 ItemCheck 를 계속 쓴다). 인벤토리 칸이 아니면 false, 아무것도 안 바뀜.
    /// </summary>
    public bool TakeOut(ItemCheck item, Transform newParent)
    {
        int index = IndexOf(item);
        if (index < 0 || newParent == null)
        {
            return false;
        }
        SyncModelFromViews();
        if (!model.TryRemoveAt(index))
        {
            return false;
        }
        dirty.Clear(); // 뷰는 아래에서 직접 맞춘다(DrawSlot 이 이 오브젝트를 파괴하지 않게)
        item.transform.SetParent(newParent);
        item.transform.position = newParent.position;
        inv.inventoryArray[index % PerPage, index / PerPage] = 0;
        inv.RebuildItemStackCache();
        return true;
    }

    /// <summary>
    /// 3c: 인벤 밖(장비칸 등)의 아이템 오브젝트를 그대로 index 칸에 넣는다(새로 만들지 않음). 칸이 차 있거나 잠겼으면 false, 아무것도 안 바뀜.
    /// </summary>
    public bool PutIn(int index, ItemCheck item)
    {
        if (item == null || item.itemData == null || index < 0 || IndexOf(item) >= 0) // 이미 인벤 칸에 있는 오브젝트는 거절(한 오브젝트가 두 칸 차지 방지)
        {
            return false;
        }
        SyncModelFromViews();
        var s = new ItemStack(item.itemData.name, Mathf.Max(1, item.nowStack), item.itemData.tear, item.itemData.upgrade);
        if (!model.PutAt(index, s))
        {
            return false;
        }
        dirty.Clear();
        Transform box = inv.inventoryUI[index / PerPage].transform.GetChild(index % PerPage);
        item.transform.SetParent(box);
        item.transform.position = box.position;
        inv.inventoryArray[index % PerPage, index / PerPage] = 1;
        inv.RebuildItemStackCache();
        return true;
    }

    /// <summary>
    /// 3c-2: 인벤 칸 아이템의 강화 수치를 바꾼다(강화). 모델은 ReplaceAt, 뷰는 같은 오브젝트의 itemData 만 바꾼다
    /// (DrawSlot 은 강화가 다르면 오브젝트를 새로 만들어 강화 창이 쥔 참조가 끊기므로 쓰지 않는다). 인벤 칸이 아니면 false.
    /// </summary>
    public bool SetUpgrade(ItemCheck item, int upgrade)
    {
        int index = IndexOf(item);
        if (index < 0 || item.itemData == null)
        {
            return false;
        }
        SyncModelFromViews();
        ItemStack cur = model[index];
        if (!model.ReplaceAt(index, new ItemStack(cur.Id, cur.Count, cur.Tier, upgrade)))
        {
            return false;
        }
        dirty.Clear();
        item.itemData.upgrade = upgrade;
        inv.RebuildItemStackCache();
        return true;
    }

    /// <summary>
    /// 3c-3: 인벤 칸 아이템의 수량을 count(1 ~ 최대 스택)로 바꾼다(창고에서 합치기). 같은 오브젝트의 nowStack 만 바꾼다.
    /// 인벤 칸이 아니거나 범위 밖이면 false, 아무것도 안 바뀜. 0 으로 만들려면 RemoveAt.
    /// </summary>
    public bool SetCount(ItemCheck item, int count)
    {
        int index = IndexOf(item);
        if (index < 0 || item.itemData == null || count < 1 || count > Mathf.Max(1, item.itemData.maxStack))
        {
            return false;
        }
        SyncModelFromViews();
        ItemStack cur = model[index];
        if (cur.IsEmpty || !model.TryRemoveAt(index) || !model.PutAt(index, cur.WithCount(count)))
        {
            return false;
        }
        dirty.Clear();
        item.nowStack = count;
        inv.RebuildItemStackCache();
        return true;
    }

    /// <summary>특정 아이템(칸)에서 n 개를 뺀다(판매·소비). 인벤토리 칸이 아니거나 수량이 모자라면 false</summary>
    public bool RemoveAt(ItemCheck item, int n)
    {
        int index = IndexOf(item);
        if (index < 0 || n <= 0)
        {
            return false;
        }
        SyncModelFromViews();
        ItemStack cur = model[index];
        if (cur.IsEmpty || cur.Count < n)
        {
            return false;
        }
        model.TryRemoveAt(index);
        if (cur.Count > n)
        {
            model.PutAt(index, cur.WithCount(cur.Count - n));
        }
        ApplyDirty(null);
        return true;
    }

    /// <summary>page 페이지의 열린 칸 중 첫 빈칸. 없으면 -1 (model 은 호출 전에 동기화돼 있어야 한다)</summary>
    int FirstEmptyInPage(int page)
    {
        if (page < 0 || page >= inv.inventoryUI.Length)
        {
            return -1;
        }
        int end = Mathf.Min(model.Unlocked, (page + 1) * PerPage);
        for (int i = page * PerPage; i < end; i++)
        {
            if (model[i].IsEmpty)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 인벤토리 밖(드래그 중·창고)에서 온 스택을 page 페이지의 첫 빈칸에 그대로 놓는다(합치지 않음).
    /// 빈칸이 없거나 시트에 없는 이름이면 아무것도 하지 않고 false — 호출자는 원본을 지우지 말 것.
    /// source 를 주면 OnItemAdded 를 발행한다(창고에서 온 경우 Chest, 인벤 안 페이지 이동이면 null).
    /// </summary>
    public bool PutInPage(int page, ItemStack s, ItemData data, AddSource? source = null)
    {
        // DrawSlot 이 칸을 비우는 이름(시트에 없음)을 미리 거른다 — 성공을 보고한 뒤 호출자가 원본을 지우면 소실
        if (s.IsEmpty || !CsvItemCatalog.Shared.Exists(s.Id))
        {
            return false;
        }
        SyncModelFromViews();
        int target = FirstEmptyInPage(page);
        if (target < 0 || !model.PutAt(target, s))
        {
            return false;
        }
        ApplyDirty(data != null ? data.Clone() : null); // 티어·강화 보존(B-014), 원본과 참조 공유 금지
        if (source.HasValue)
        {
            OnItemAdded?.Invoke(s, source.Value);
        }
        return true;
    }

    /// <summary>인벤토리 칸의 아이템을 page 페이지 첫 빈칸으로 통째로 옮긴다(키보드 페이지 이동). 같은 페이지·빈칸 없음이면 false</summary>
    public bool MoveToPage(ItemCheck item, int page)
    {
        int from = IndexOf(item);
        if (from < 0 || from / PerPage == page)
        {
            return false;
        }
        SyncModelFromViews();
        int target = FirstEmptyInPage(page);
        if (target < 0 || !model.Move(from, target))
        {
            return false;
        }
        ApplyDirty(null);
        return true;
    }

    /// <summary>인벤토리 칸에서 amount 개를 떼어 page 페이지 빈칸(없으면 다른 페이지 빈칸)에 둔다. 빈칸이 없으면 false, 총량 보존(B-009)</summary>
    public bool SplitAt(ItemCheck item, int amount, int page)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }
        SyncModelFromViews();
        if (!model.Split(index, amount, page, PerPage))
        {
            return false;
        }
        ApplyDirty(null);
        return true;
    }

    /// <summary>세이브 배열로 모델을 채우고 슬롯 박스가 있는 칸을 전부 그린다(로드)</summary>
    public void LoadFrom(string[,,] saved)
    {
        hidden.Clear();
        if (model != null)
        {
            model.OnSlotChanged -= MarkDirty;
            model = null; // 로드는 뷰를 직접 다시 그리므로 그림자 비교 기준을 버린다(모델 밖 변경 오탐 방지)
        }
        SlotContainer loaded = SaveMapper.FromArray(saved, CsvItemCatalog.Shared, inv.inventoryUI.Length, PerPage);
        int drawn = 0;
        for (int index = 0; index < loaded.Capacity; index++)
        {
            int page = index / PerPage, slot = index % PerPage;
            if (slot >= inv.inventoryUI[page].transform.childCount)
            {
                if (!loaded[index].IsEmpty)
                {
                    hidden[index] = loaded[index];
                    GameLog.Warn(GameLog.Cat.Item, "세이브 아이템이 열린 칸 밖에 있음(보존): " + loaded[index]);
                }
                continue;
            }
            DrawSlot(index, loaded[index], null);
            if (!loaded[index].IsEmpty)
            {
                drawn++;
                OnItemAdded?.Invoke(loaded[index], AddSource.Load);
            }
        }
        SyncModelFromViews();
        inv.RebuildItemStackCache();
        GameLog.Info(GameLog.Cat.Item, "인벤토리 로드: " + drawn + "칸");
    }

    /// <summary>현재 슬롯 + 열린 칸 밖에 보관 중인 아이템까지 담은 세이브 배열</summary>
    public string[,,] BuildSaveArray()
    {
        int pages = inv.inventoryUI.Length;
        SlotContainer scan = ViewScanner.BuildFrom(inv.inventoryUI, PerPage, CsvItemCatalog.Shared);
        string[,,] arr = SaveMapper.ToArray(scan, pages, PerPage);
        foreach (KeyValuePair<int, ItemStack> kv in hidden)
        {
            int page = kv.Key / PerPage, slot = kv.Key % PerPage;
            if (page < pages && SaveMapper.ReadCell(arr, page, slot).IsEmpty)
            {
                SaveMapper.WriteCell(arr, page, slot, kv.Value);
            }
        }
        return arr;
    }

    /// <summary>모델에서 바뀐 칸만 슬롯 GameObject 에 그린다 (아이템 오브젝트 생성·삭제·수량)</summary>
    void ApplyDirty(ItemData newData)
    {
        var order = new List<int>(dirty);
        order.Sort();
        foreach (int index in order)
        {
            if (DrawSlot(index, model[index], newData))
            {
                newData = null; // 넘겨받은 ItemData 인스턴스는 한 칸에만 쓴다(공유 참조 방지)
            }
        }
        dirty.Clear();
        inv.RebuildItemStackCache();
    }

    /// <summary>칸 하나를 그린다. newData 를 새 아이템에 썼으면 true</summary>
    bool DrawSlot(int index, ItemStack s, ItemData newData)
    {
        int page = index / PerPage, slot = index % PerPage;
        Transform box = inv.inventoryUI[page].transform.GetChild(slot);
        ItemCheck cur = box.childCount > 0 ? box.GetChild(0).GetComponent<ItemCheck>() : null;
        if (s.IsEmpty)
        {
            if (cur != null)
            {
                DetachAndDestroy(cur.gameObject);
            }
            inv.inventoryArray[slot, page] = 0;
            return false;
        }
        bool used = false;
        bool same = cur != null && cur.itemData != null && cur.itemData.name == s.Id && cur.itemData.tear == s.Tier && cur.itemData.upgrade == s.Upgrade;
        if (!same)
        {
            if (cur != null)
            {
                DetachAndDestroy(cur.gameObject);
            }
            GameObject go = UnityEngine.Object.Instantiate(inv.itemPrefab, box.position, Quaternion.identity, box);
            cur = go.GetComponent<ItemCheck>();
            ItemData data = newData != null && newData.name == s.Id ? newData : null;
            used = data != null;
            cur.SetItem(s.Id, data);
            if (cur.itemData == null) // 시트에 없는 이름(손상·구버전 세이브)은 칸을 비운다(B-020)
            {
                DetachAndDestroy(go);
                inv.inventoryArray[slot, page] = 0;
                return false;
            }
            cur.itemData.tear = s.Tier;
            cur.itemData.upgrade = s.Upgrade;
        }
        cur.nowStack = s.Count;
        inv.inventoryArray[slot, page] = 1;
        return used;
    }

    /// <summary>Destroy 는 프레임 끝에 일어나므로 먼저 부모에서 떼어 같은 프레임의 스캔에 잡히지 않게 한다</summary>
    static void DetachAndDestroy(GameObject go)
    {
        go.transform.SetParent(null, false);
        UnityEngine.Object.Destroy(go);
    }
}
