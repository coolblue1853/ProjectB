/// <summary>
/// 인벤토리 칸 하나의 값 (InventoryRefactorDesign 2.1). 빈칸은 Empty(Id == null).
/// 합치기 조건은 id + tier + upgrade 동일 — 강화 수치가 다른 장비는 섞이지 않는다(B-014 근본 해결).
/// </summary>
public readonly struct ItemStack
{
    public readonly string Id;
    public readonly int Count, Tier, Upgrade;

    public static readonly ItemStack Empty = default;

    public ItemStack(string id, int count, int tier = 0, int upgrade = 0)
    {
        Id = id;
        Count = count;
        Tier = tier;
        Upgrade = upgrade;
    }

    public bool IsEmpty => Id == null || Count <= 0;

    public bool CanStackWith(ItemStack o)
    {
        return !IsEmpty && !o.IsEmpty && Id == o.Id && Tier == o.Tier && Upgrade == o.Upgrade;
    }

    public ItemStack WithCount(int c)
    {
        return c <= 0 ? Empty : new ItemStack(Id, c, Tier, Upgrade);
    }

    public override string ToString()
    {
        return IsEmpty ? "(empty)" : Id + " x" + Count + (Upgrade > 0 ? " +" + Upgrade : "") + (Tier > 0 ? " T" + Tier : "");
    }
}

/// <summary>아이템 정의(CSV 행)의 읽기 전용 뷰. 인스턴스 값은 ItemStack 에만 둔다</summary>
public sealed class ItemDef
{
    public string Id, Type, EquipArea, SetKey, Rarity, DisplayName;
    public int MaxStack = 1, Price;
}

public interface IItemCatalog
{
    ItemDef Get(string id);
    bool Exists(string id);
}
