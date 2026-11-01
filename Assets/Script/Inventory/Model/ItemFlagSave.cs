/// <summary>기존 아이템 배열 차원은 유지하고 추가 ES3 필드에 인스턴스 상태를 보존한다.</summary>
public static class ItemFlagSave
{
    public static int Encode(bool locked, bool fresh)
    {
        return (locked ? 1 : 0) | (fresh ? 2 : 0);
    }

    public static int At(int[,] flags, int page, int slot)
    {
        return flags != null && page >= 0 && slot >= 0 && page < flags.GetLength(0) && slot < flags.GetLength(1)
            ? flags[page, slot] : 0;
    }

    public static ItemStack Read(ItemStack stack, int[,] flags, int page, int slot)
    {
        int value = At(flags, page, slot);
        return stack.WithFlags((value & 1) != 0, (value & 2) != 0);
    }

    public static void Apply(ItemData data, int value)
    {
        data.isLocked = (value & 1) != 0;
        data.isNew = (value & 2) != 0;
    }

    public static int[,] ToArray(SlotContainer model, int pages, int perPage)
    {
        var flags = new int[pages, perPage];
        for (int i = 0; i < model.Capacity && i < pages * perPage; i++)
        {
            flags[i / perPage, i % perPage] = Encode(model[i].IsLocked, model[i].IsNew);
        }
        return flags;
    }
}
