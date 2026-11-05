using System;
using System.Collections.Generic;

/// <summary>장비 부위 = 세이브 인덱스 그대로 (equipGear[슬롯]). 10~13 은 해금형 비외형 장신구 슬롯(귀걸이·팔찌·부적·마법석, EquipSlotExpansion.md, 별도 ES3 키). 11-05 이전 이름 Title·Aura·Cape 는 폐기, 값 유지</summary>
public enum EquipSlot { Ring = 1, Head = 2, Necklace = 3, Chest = 5, Hand = 6, Weapon = 7, Leg = 8, Shoes = 9, Earring = 10, Bracelet = 11, Charm = 12, Stone = 13 }

/// <summary>
/// 장비 모델 (InventoryRefactorDesign 2.6) — 순수 C#. 부위 문자열 매핑은 TryGetSlot 한 곳(B-011 "Neckles" 류 재발 방지).
/// 장착은 인벤 칸과 교환, 해제는 인벤에 공간이 있을 때만(티어·강화 보존).
/// </summary>
public sealed class EquipmentModel
{
    readonly Dictionary<EquipSlot, ItemStack> slots = new Dictionary<EquipSlot, ItemStack>();
    readonly IItemCatalog catalog;

    public EquipmentModel(IItemCatalog catalog)
    {
        this.catalog = catalog;
    }

    public event Action<EquipSlot, ItemStack, ItemStack> OnChanged; // 슬롯, 이전, 이후

    /// <summary>신규 슬롯 해금 비트마스크 (SlotRules.Bit). 기본 8칸은 항상 해금</summary>
    public int ExtraUnlockMask { get; private set; }

    public bool IsUnlocked(EquipSlot slot)
    {
        return SlotRules.IsUnlocked(ExtraUnlockMask, slot);
    }

    /// <summary>신규 슬롯 해금(영구, 이미 해금이면 false). 기본 8칸은 false</summary>
    public bool Unlock(EquipSlot slot)
    {
        if (!SlotRules.IsExtra(slot) || IsUnlocked(slot))
        {
            return false;
        }
        ExtraUnlockMask = SlotRules.Unlock(ExtraUnlockMask, slot);
        return true;
    }

    /// <summary>신규 슬롯 아이템을 세이브 문자열 4개로 (Earring·Bracelet·Charm·Stone, 빈칸 "")</summary>
    public string[] ToExtraArray()
    {
        var arr = new string[SlotRules.Extras.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = SlotRules.EncodeStack(this[SlotRules.Extras[i]]);
        }
        return arr;
    }

    /// <summary>
    /// 세이브에서 신규 슬롯 복원. null·짧은 배열(구 세이브)은 빈칸. 해금 안 됐거나 부위가 맞지 않는(폐기된) 아이템이 있으면 인벤토리로 돌려보낸다
    /// (공간이 없으면 소실 방지로 그 자리에 둔다). 돌려보낸 개수를 반환
    /// </summary>
    public int LoadExtra(string[] ids, int unlockMask, SlotContainer inventory)
    {
        ExtraUnlockMask = unlockMask & SlotRules.AllExtraMask;
        int returned = 0;
        for (int i = 0; i < SlotRules.Extras.Length; i++)
        {
            EquipSlot slot = SlotRules.Extras[i];
            slots.Remove(slot);
            ItemStack st = ids != null && i < ids.Length ? SlotRules.DecodeStack(ids[i]) : ItemStack.Empty;
            if (st.IsEmpty)
            {
                continue;
            }
            ItemDef def = catalog.Get(st.Id);
            bool mismatch = def == null || !TryGetSlot(def, out EquipSlot want) || want != slot; // 폐기 아이템(구 Title/Aura 등)이 낀 구 세이브
            if ((mismatch || !IsUnlocked(slot)) && inventory != null && inventory.TryAddAll(st))
            {
                returned++;
                continue;
            }
            slots[slot] = st;
        }
        return returned;
    }

    public ItemStack this[EquipSlot s] => slots.TryGetValue(s, out ItemStack v) ? v : ItemStack.Empty;

    public IEnumerable<ItemDef> EquippedDefs()
    {
        foreach (var kv in slots)
        {
            if (!kv.Value.IsEmpty)
            {
                yield return catalog.Get(kv.Value.Id);
            }
        }
    }

    public static bool TryGetSlot(ItemDef def, out EquipSlot slot)
    {
        slot = default;
        if (def == null || def.Type != "Equip" || string.IsNullOrEmpty(def.EquipArea))
        {
            return false;
        }
        return Enum.TryParse(def.EquipArea, out slot) && Enum.IsDefined(typeof(EquipSlot), slot);
    }

    /// <summary>인벤 index 칸의 장비를 장착. 기존 장비는 그 칸으로 교환. 부위가 안 맞으면 false</summary>
    public bool Equip(SlotContainer inventory, int index)
    {
        ItemStack item = inventory[index];
        if (item.IsEmpty || !TryGetSlot(catalog.Get(item.Id), out EquipSlot slot))
        {
            return false;
        }
        if (!IsUnlocked(slot))
        {
            return false;
        }
        ItemStack old = this[slot];
        inventory.TryRemoveAt(index);
        if (!old.IsEmpty)
        {
            inventory.PutAt(index, old);
        }
        slots[slot] = item.WithCount(1);
        OnChanged?.Invoke(slot, old, slots[slot]);
        return true;
    }

    /// <summary>해제 → 인벤 빈칸에. 공간이 없으면 거절</summary>
    public bool Unequip(EquipSlot slot, SlotContainer inventory)
    {
        ItemStack old = this[slot];
        if (old.IsEmpty || !inventory.TryAddAll(old))
        {
            return false;
        }
        slots.Remove(slot);
        OnChanged?.Invoke(slot, old, ItemStack.Empty);
        return true;
    }
}
