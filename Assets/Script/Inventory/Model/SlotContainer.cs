using System;
using System.Collections.Generic;

/// <summary>
/// 인벤토리·창고 공용 칸 모델 (InventoryRefactorDesign 2.2). 순수 C# — 씬·UI 의존 없음, EditMode 테스트 대상.
/// 원칙: 모든 실패는 상태 불변 + false/0. 이벤트(OnSlotChanged)는 실제로 바뀐 칸마다 1회.
/// 페이지는 뷰 개념이다(index = page * perPage + slot). Unlocked 밖 칸은 쓰지 않는다.
/// </summary>
public sealed class SlotContainer
{
    readonly IItemCatalog catalog;
    readonly ItemStack[] slots;

    public SlotContainer(IItemCatalog catalog, int capacity, int unlocked)
    {
        this.catalog = catalog;
        slots = new ItemStack[capacity];
        Unlocked = Math.Min(unlocked, capacity);
    }

    public int Capacity => slots.Length;
    public int Unlocked { get; set; }
    public ItemStack this[int index] => slots[index];
    public event Action<int> OnSlotChanged;

    int MaxStack(string id)
    {
        ItemDef d = catalog.Get(id);
        return d == null ? 1 : Math.Max(1, d.MaxStack);
    }

    void Set(int index, ItemStack s)
    {
        slots[index] = s.IsEmpty ? ItemStack.Empty : s;
        OnSlotChanged?.Invoke(index);
    }

    public int Count(string id)
    {
        int total = 0;
        for (int i = 0; i < Unlocked; i++)
        {
            if (slots[i].Id == id)
            {
                total += slots[i].Count;
            }
        }
        return total;
    }

    /// <summary>넣을 수 있는 최대 수량 (덜 찬 같은 스택 여유 + 빈칸 수 × maxStack)</summary>
    public int SpaceFor(ItemStack s)
    {
        if (s.IsEmpty)
        {
            return 0;
        }
        int max = MaxStack(s.Id), space = 0;
        for (int i = 0; i < Unlocked; i++)
        {
            if (slots[i].IsEmpty)
            {
                space += max;
            }
            else if (slots[i].CanStackWith(s))
            {
                space += Math.Max(0, max - slots[i].Count);
            }
        }
        return space;
    }

    /// <summary>덜 찬 스택 → 첫 빈칸 순으로 넣는다. 넣은 수량 반환(0이면 상태 불변)</summary>
    public int TryAdd(ItemStack s)
    {
        if (s.IsEmpty)
        {
            return 0;
        }
        int max = MaxStack(s.Id), left = s.Count;
        for (int i = 0; i < Unlocked && left > 0; i++)
        {
            if (slots[i].CanStackWith(s) && slots[i].Count < max)
            {
                int put = Math.Min(left, max - slots[i].Count);
                Set(i, slots[i].WithCount(slots[i].Count + put));
                left -= put;
            }
        }
        for (int i = 0; i < Unlocked && left > 0; i++)
        {
            if (slots[i].IsEmpty)
            {
                int put = Math.Min(left, max);
                Set(i, s.WithCount(put));
                left -= put;
            }
        }
        return s.Count - left;
    }

    /// <summary>전부 들어갈 때만 넣는다(원자적)</summary>
    public bool TryAddAll(ItemStack s)
    {
        if (s.IsEmpty || SpaceFor(s) < s.Count)
        {
            return false;
        }
        return TryAdd(s) == s.Count;
    }

    /// <summary>앞 칸부터 n개 제거. 부족하면 false, 상태 불변</summary>
    public bool TryRemove(string id, int n)
    {
        if (n <= 0 || Count(id) < n)
        {
            return false;
        }
        for (int i = 0; i < Unlocked && n > 0; i++)
        {
            if (slots[i].Id != id)
            {
                continue;
            }
            int take = Math.Min(n, slots[i].Count);
            Set(i, slots[i].WithCount(slots[i].Count - take));
            n -= take;
        }
        return true;
    }

    /// <summary>빈칸이면 이동, 합칠 수 있으면 합치고 넘침은 원위치에 남김, 아니면 교환(가득 찬 같은 스택끼리도 교환)</summary>
    public bool Move(int from, int to)
    {
        if (from == to || !InRange(from) || !InRange(to) || slots[from].IsEmpty)
        {
            return false;
        }
        ItemStack a = slots[from], b = slots[to];
        if (b.IsEmpty)
        {
            Set(to, a);
            Set(from, ItemStack.Empty);
            return true;
        }
        int max = MaxStack(a.Id);
        if (a.CanStackWith(b) && b.Count < max)
        {
            int put = Math.Min(a.Count, max - b.Count);
            Set(to, b.WithCount(b.Count + put));
            Set(from, a.WithCount(a.Count - put));
            return true;
        }
        Set(to, a);
        Set(from, b);
        return true;
    }

    /// <summary>index 칸에서 amount 만큼 떼어 빈칸에 둔다. 1 ≤ amount &lt; count, 빈칸 없으면 false(총량 보존, B-009)</summary>
    public bool Split(int index, int amount, int preferPage = -1, int perPage = 25)
    {
        if (!InRange(index) || slots[index].IsEmpty || amount < 1 || amount >= slots[index].Count)
        {
            return false;
        }
        int target = -1;
        if (preferPage >= 0)
        {
            for (int i = preferPage * perPage; i < Math.Min(Unlocked, (preferPage + 1) * perPage); i++)
            {
                if (slots[i].IsEmpty)
                {
                    target = i;
                    break;
                }
            }
        }
        for (int i = 0; target < 0 && i < Unlocked; i++)
        {
            if (slots[i].IsEmpty)
            {
                target = i;
            }
        }
        if (target < 0)
        {
            return false;
        }
        ItemStack s = slots[index];
        Set(index, s.WithCount(s.Count - amount));
        Set(target, s.WithCount(amount));
        return true;
    }

    /// <summary>같은 스택을 합친 뒤 비교자 순으로 앞에서부터 채운다</summary>
    public void Sort(IComparer<ItemStack> cmp)
    {
        var temp = new SlotContainer(catalog, Unlocked, Unlocked);
        for (int i = 0; i < Unlocked; i++)
        {
            if (!slots[i].IsEmpty)
            {
                temp.TryAdd(slots[i]);
            }
        }
        var list = new List<ItemStack>();
        for (int i = 0; i < Unlocked; i++)
        {
            if (!temp.slots[i].IsEmpty)
            {
                list.Add(temp.slots[i]);
            }
        }
        list.Sort(cmp);
        for (int i = 0; i < Unlocked; i++)
        {
            ItemStack next = i < list.Count ? list[i] : ItemStack.Empty;
            if (!Same(slots[i], next))
            {
                Set(i, next);
            }
        }
    }

    /// <summary>src 칸에서 dst 로 최대 amount 옮기고, 실제로 옮긴 수량만 원본에서 차감한다(B-006/008). 티어·강화 보존(B-014)</summary>
    public static int Transfer(SlotContainer src, int index, SlotContainer dst, int amount)
    {
        if (!src.InRange(index) || src.slots[index].IsEmpty || amount <= 0)
        {
            return 0;
        }
        ItemStack s = src.slots[index];
        int moved = dst.TryAdd(s.WithCount(Math.Min(amount, s.Count)));
        if (moved > 0)
        {
            src.Set(index, s.WithCount(s.Count - moved));
        }
        return moved;
    }

    bool InRange(int i)
    {
        return i >= 0 && i < Unlocked;
    }

    static bool Same(ItemStack a, ItemStack b)
    {
        return a.Id == b.Id && a.Count == b.Count && a.Tier == b.Tier && a.Upgrade == b.Upgrade;
    }
}
