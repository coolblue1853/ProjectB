using System.Collections.Generic;

/// <summary>잡템(category=Junk)만 대상인 일괄 판매 계획. 확인 뒤 구성이 달라지면 다시 확인한다.</summary>
public static class BulkSaleRules
{
    public sealed class Plan
    {
        public readonly List<int> Indices = new List<int>();
        public readonly List<ItemStack> Stacks = new List<ItemStack>();
        public int Count;
        public long Price;
    }

    public const string JunkCategory = "Junk";

    public static bool IsJunk(ItemDef definition)
    {
        return definition != null && definition.Category == JunkCategory;
    }

    public static bool Eligible(ItemStack stack, ItemDef definition)
    {
        return !stack.IsEmpty && !stack.IsLocked && IsJunk(definition) && definition.Type == "Misc"
            && definition.Price > 0 && ItemTradeRules.CanSell(stack.Id);
    }

    public static Plan Build(SlotContainer model, IItemCatalog catalog)
    {
        var plan = new Plan();
        for (int i = 0; i < model.Unlocked; i++)
        {
            ItemStack stack = model[i];
            ItemDef definition = catalog.Get(stack.Id);
            if (!Eligible(stack, definition))
            {
                continue;
            }
            plan.Indices.Add(i);
            plan.Stacks.Add(stack);
            plan.Count += stack.Count;
            plan.Price += (long)definition.Price * stack.Count;
        }
        return plan;
    }

    public static bool Matches(Plan before, Plan after)
    {
        if (before == null || after == null || before.Price != after.Price || before.Indices.Count != after.Indices.Count)
        {
            return false;
        }
        for (int i = 0; i < before.Indices.Count; i++)
        {
            ItemStack a = before.Stacks[i], b = after.Stacks[i];
            if (before.Indices[i] != after.Indices[i] || a.Id != b.Id || a.Count != b.Count
                || a.Tier != b.Tier || a.Upgrade != b.Upgrade || a.IsLocked != b.IsLocked)
            {
                return false;
            }
        }
        return true;
    }
}
