/// <summary>일괄 보관 규칙. 종류는 선택한 재료·소비품의 동일 ID이며 인스턴스 속성은 각각 보존한다.</summary>
public static class ChestTransferRules
{
    public static int Deposit(SlotContainer inventory, SlotContainer chest, IItemCatalog catalog, string id = null)
    {
        int moved = 0;
        for (int i = 0; i < inventory.Unlocked; i++)
        {
            ItemStack stack = inventory[i];
            ItemDef def = stack.IsEmpty ? null : catalog.Get(stack.Id);
            bool matches = id == null ? def?.Type == "Misc" :
                stack.Id == id && (def?.Type == "Misc" || def?.Type == "Consum");
            if (matches)
            {
                moved += SlotContainer.Transfer(inventory, i, chest, stack.Count);
            }
        }
        return moved;
    }
}
