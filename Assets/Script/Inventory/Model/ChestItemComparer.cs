using System;
using System.Collections.Generic;

/// <summary>창고: 종류 → 등급 → 표시 이름 → 티어·강화 순. 인벤토리의 부위 우선 정렬과 구분한다.</summary>
public sealed class ChestItemComparer : IComparer<ItemStack>
{
    private readonly IItemCatalog _catalog;

    public ChestItemComparer(IItemCatalog catalog)
    {
        _catalog = catalog;
    }

    public int Compare(ItemStack a, ItemStack b)
    {
        if (a.IsEmpty || b.IsEmpty)
        {
            return a.IsEmpty.CompareTo(b.IsEmpty);
        }
        ItemDef da = _catalog.Get(a.Id), db = _catalog.Get(b.Id);
        int result = TypeRank(da?.Type).CompareTo(TypeRank(db?.Type));
        if (result == 0)
        {
            result = RarityRank(da?.Rarity).CompareTo(RarityRank(db?.Rarity));
        }
        if (result == 0)
        {
            result = string.CompareOrdinal(da?.DisplayName ?? a.Id, db?.DisplayName ?? b.Id);
        }
        if (result == 0)
        {
            result = string.CompareOrdinal(a.Id, b.Id);
        }
        if (result == 0)
        {
            result = b.Tier.CompareTo(a.Tier);
        }
        if (result == 0)
        {
            result = b.Upgrade.CompareTo(a.Upgrade);
        }
        return result == 0 ? b.Count.CompareTo(a.Count) : result;
    }

    private static int TypeRank(string type)
    {
        return type == "Equip" ? 0 : type == "Consum" ? 1 : type == "Misc" ? 2 : 3;
    }

    private static int RarityRank(string rarity)
    {
        string key = string.IsNullOrEmpty(rarity) ? "" : rarity.Substring(0, 1).ToUpperInvariant();
        return key == "L" ? 0 : key == "U" ? 1 : key == "R" ? 2 : key == "C" ? 3 : 4;
    }
}
