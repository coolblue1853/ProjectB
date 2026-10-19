using System;
using System.Collections.Generic;

/// <summary>
/// 인벤토리 정렬 순서 (InventoryRefactorDesign 3.2, D4): 타입(Equip→Consum→Misc) → 장비 부위 → 등급(U→R→C) → 티어·강화 내림차순 → 이름.
/// 순수 클래스 — 카탈로그만 있으면 테스트할 수 있다.
/// </summary>
public sealed class ItemSortComparer : IComparer<ItemStack>
{
    static readonly string[] TypeOrder = { "Equip", "Consum", "Misc" };
    static readonly string[] AreaOrder = { "Weapon", "Head", "Chest", "Hand", "Leg", "Shoes", "Ring", "Necklace" };
    static readonly string[] RarityOrder = { "U", "R", "C" };

    readonly IItemCatalog catalog;

    public ItemSortComparer(IItemCatalog catalog)
    {
        this.catalog = catalog;
    }

    public int Compare(ItemStack a, ItemStack b)
    {
        if (a.IsEmpty || b.IsEmpty)
        {
            return a.IsEmpty.CompareTo(b.IsEmpty); // 빈칸은 뒤로
        }
        ItemDef da = catalog.Get(a.Id), db = catalog.Get(b.Id);
        int c = Rank(TypeOrder, da?.Type).CompareTo(Rank(TypeOrder, db?.Type));
        if (c != 0) return c;
        c = Rank(AreaOrder, da?.EquipArea).CompareTo(Rank(AreaOrder, db?.EquipArea));
        if (c != 0) return c;
        c = Rank(RarityOrder, RarityKey(da?.Rarity)).CompareTo(Rank(RarityOrder, RarityKey(db?.Rarity)));
        if (c != 0) return c;
        c = b.Tier.CompareTo(a.Tier);
        if (c != 0) return c;
        c = b.Upgrade.CompareTo(a.Upgrade);
        if (c != 0) return c;
        c = string.CompareOrdinal(da?.DisplayName ?? a.Id, db?.DisplayName ?? b.Id);
        if (c != 0) return c;
        return b.Count.CompareTo(a.Count);
    }

    /// <summary>CSV 등급은 "U"/"Unique" 둘 다 올 수 있어 첫 글자로 맞춘다</summary>
    static string RarityKey(string rarity)
    {
        return string.IsNullOrEmpty(rarity) ? null : rarity.Substring(0, 1).ToUpperInvariant();
    }

    /// <summary>목록에 없으면 맨 뒤</summary>
    static int Rank(string[] order, string value)
    {
        int i = value == null ? -1 : Array.IndexOf(order, value);
        return i < 0 ? order.Length : i;
    }
}
