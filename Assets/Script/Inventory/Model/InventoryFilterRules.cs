/// <summary>가방 보기 필터 (InventoryRefactorDesign §8). 표시만 거른다 — 슬롯 인덱스·세이브는 불변. 순수 클래스.</summary>
public enum InventoryFilter { All = 0, Equip = 1, Consum = 2, Misc = 3 }

public static class InventoryFilterRules
{
    public static InventoryFilter Next(InventoryFilter f)
    {
        return (InventoryFilter)(((int)f + 1) % 4);
    }

    /// <summary>필터에 맞는 아이템인가. 빈칸(type null)은 전체 필터에서만 true 로 두고, 호출 측이 빈칸을 따로 처리한다.</summary>
    public static bool Matches(InventoryFilter f, string itemType)
    {
        switch (f)
        {
            case InventoryFilter.All: return true;
            case InventoryFilter.Equip: return itemType == "Equip";
            case InventoryFilter.Consum: return itemType == "Consum";
            default: return itemType == "Misc";
        }
    }

    public static string Label(InventoryFilter f)
    {
        switch (f)
        {
            case InventoryFilter.Equip: return "장비";
            case InventoryFilter.Consum: return "소비";
            case InventoryFilter.Misc: return "재료";
            default: return "전체";
        }
    }
}
