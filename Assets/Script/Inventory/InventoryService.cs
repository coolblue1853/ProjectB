using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>획득 경로 (픽업 로그·퀘스트가 구분해서 쓴다)</summary>
public enum AddSource { Pickup, Craft, Shop, Chest, Unequip, Load, Other }

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

    public SlotContainer Model
    {
        get
        {
            SyncModelFromViews();
            return model;
        }
    }

    /// <summary>UI 슬롯 → 모델. 잠긴 칸(maxBoxNum 밖)은 명령 대상에서 뺀다</summary>
    public void SyncModelFromViews()
    {
        if (model != null)
        {
            model.OnSlotChanged -= MarkDirty;
        }
        model = ViewScanner.BuildFrom(inv.inventoryUI, PerPage, CsvItemCatalog.Shared);
        model.Unlocked = Mathf.Clamp(inv.maxBoxNum, 0, model.Capacity);
        model.OnSlotChanged += MarkDirty;
        dirty.Clear();
    }

    void MarkDirty(int index)
    {
        dirty.Add(index);
    }

    /// <summary>1개 이상 넣는다. 전부 들어갈 자리가 없으면 아무것도 넣지 않고 false</summary>
    public bool Add(string id, int count, AddSource source, ItemData data = null)
    {
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
        if (item == null || item.transform.parent == null)
        {
            return -1;
        }
        Transform box = item.transform.parent;
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

    /// <summary>세이브 배열로 모델을 채우고 슬롯 박스가 있는 칸을 전부 그린다(로드)</summary>
    public void LoadFrom(string[,,] saved)
    {
        hidden.Clear();
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
