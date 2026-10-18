/// <summary>
/// 세이브 DTO(Datas.invenItem / chestItem = string[page, slot, {name, count, tier, upgrade}]) ↔ SlotContainer 매핑 (InventoryRefactorDesign 2.7).
/// 필드 형식은 그대로 둔다. 파싱은 TryParse — 수량 ≤ 0 이면 빈칸, tier/upgrade 실패는 0(B-020).
/// CSV 에 없는 id 도 버리지 않는다(원문 보존).
/// </summary>
public static class SaveMapper
{
    public const string EmptyMark = "non";

    /// <summary>세이브 한 칸을 읽는다. 빈칸·잘못된 수량이면 Empty</summary>
    public static ItemStack ReadCell(string[,,] arr, int page, int slot)
    {
        if (arr == null || page >= arr.GetLength(0) || slot >= arr.GetLength(1))
        {
            return ItemStack.Empty;
        }
        string id = arr[page, slot, 0];
        if (string.IsNullOrEmpty(id) || id == EmptyMark)
        {
            return ItemStack.Empty;
        }
        int count = ParseOr(arr[page, slot, 1], 0);
        if (count <= 0)
        {
            return ItemStack.Empty;
        }
        return new ItemStack(id, count, ParseOr(arr[page, slot, 2], 0), ParseOr(arr[page, slot, 3], 0));
    }

    public static void WriteCell(string[,,] arr, int page, int slot, ItemStack s)
    {
        if (s.IsEmpty)
        {
            arr[page, slot, 0] = EmptyMark;
            arr[page, slot, 1] = arr[page, slot, 2] = arr[page, slot, 3] = null;
            return;
        }
        arr[page, slot, 0] = s.Id;
        arr[page, slot, 1] = s.Count.ToString();
        arr[page, slot, 2] = s.Tier.ToString();
        arr[page, slot, 3] = s.Upgrade.ToString();
    }

    /// <summary>모델 → 세이브 배열. index = page * perPage + slot</summary>
    public static string[,,] ToArray(SlotContainer c, int pages, int perPage)
    {
        var arr = new string[pages, perPage, 4];
        for (int p = 0; p < pages; p++)
        {
            for (int s = 0; s < perPage; s++)
            {
                int index = p * perPage + s;
                WriteCell(arr, p, s, index < c.Capacity ? c[index] : ItemStack.Empty);
            }
        }
        return arr;
    }

    /// <summary>세이브 배열 → 모델. 세이브에 있는 칸은 잠금 여부와 상관없이 전부 싣는다(소실 금지)</summary>
    public static SlotContainer FromArray(string[,,] arr, IItemCatalog catalog, int pages, int perPage)
    {
        var c = new SlotContainer(catalog, pages * perPage, pages * perPage);
        for (int p = 0; p < pages; p++)
        {
            for (int s = 0; s < perPage; s++)
            {
                ItemStack st = ReadCell(arr, p, s);
                if (!st.IsEmpty)
                {
                    c.PutAt(p * perPage + s, st);
                }
            }
        }
        return c;
    }

    static int ParseOr(string text, int fallback)
    {
        return int.TryParse(text, out int v) ? v : fallback;
    }
}
