using System;
using System.Collections.Generic;

/// <summary>장비 부위 = 세이브 인덱스 그대로 (equipGear[슬롯])</summary>
public enum EquipSlot { Ring = 1, Head = 2, Necklace = 3, Chest = 5, Hand = 6, Weapon = 7, Leg = 8, Shoes = 9 }

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
