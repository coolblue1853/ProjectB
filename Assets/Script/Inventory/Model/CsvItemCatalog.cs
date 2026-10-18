using System.Collections.Generic;

/// <summary>기존 DatabaseManager(ItemSheet.csv)를 감싸는 카탈로그. 정의는 처음 조회할 때 한 번만 만든다</summary>
public sealed class CsvItemCatalog : IItemCatalog
{
    /// <summary>게임 전역 공용 인스턴스 (ItemSheet 는 실행 중 바뀌지 않는다)</summary>
    public static readonly CsvItemCatalog Shared = new CsvItemCatalog();

    readonly Dictionary<string, ItemDef> cache = new Dictionary<string, ItemDef>();

    public bool Exists(string id)
    {
        return Get(id) != null;
    }

    public ItemDef Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        if (cache.TryGetValue(id, out ItemDef def))
        {
            return def;
        }
        DatabaseManager db = DatabaseManager.instance;
        int index = db != null ? db.FindItemDataIndex(id) : -1;
        if (index < 0)
        {
            cache[id] = null;
            return null;
        }
        ItemData row = db.LoadItemData(index);
        def = new ItemDef
        {
            Id = row.name,
            Type = row.type,
            EquipArea = row.equipArea,
            SetKey = row.tfName,
            Rarity = row.rarity,
            DisplayName = row.itemNameT,
            MaxStack = row.maxStack <= 0 ? 1 : row.maxStack,
            Price = row.price,
        };
        cache[id] = def;
        return def;
    }
}
