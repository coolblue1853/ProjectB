using System.Collections.Generic;
using System.Text;

/// <summary>실제 장착 상태의 복사본으로 교체 후 누적 세트 효과의 획득·해제를 계산한다.</summary>
public static class SetSwapPreview
{
    public static SetBonusDiff Calculate(IReadOnlyDictionary<string, int> before, string oldSet, string newSet, SetTierTable table)
    {
        var after = new Dictionary<string, int>();
        foreach (KeyValuePair<string, int> pair in before) { after[pair.Key] = pair.Value; }
        if (!string.IsNullOrEmpty(oldSet) && after.TryGetValue(oldSet, out int count))
        {
            after[oldSet] = System.Math.Max(0, count - 1);
        }
        if (!string.IsNullOrEmpty(newSet))
        {
            after.TryGetValue(newSet, out int current);
            after[newSet] = current + 1;
        }
        return SetBonusEvaluator.Diff(before, after, table);
    }

    public static string Build(InventoryManager inv, ItemCheck target, ItemCheck worn)
    {
        var definitions = new List<ItemDef>();
        foreach (UnityEngine.GameObject box in inv.equipBox)
        {
            if (box != null && box.transform.childCount > 0)
            {
                ItemCheck item = box.transform.GetChild(0).GetComponent<ItemCheck>();
                if (item != null && item.itemData != null) { definitions.Add(CsvItemCatalog.Shared.Get(item.name)); }
            }
        }
        Dictionary<string, int> counts = SetBonusEvaluator.CountPieces(definitions);
        string oldSet = worn.itemData.tfName, newSet = target.itemData.tfName;
        var keys = new HashSet<string> { oldSet, newSet };
        var table = new SetTierTable();
        DatabaseManager db = DatabaseManager.instance;
        foreach (string key in keys)
        {
            if (string.IsNullOrEmpty(key)) { continue; }
            for (int i = 1; i <= 10; i++)
            {
                SetData data = db.LoadSetsData(db.FindSetsDataIndex(key + "_" + i));
                if (data != null) { table.Define(key, i, data.effect); }
            }
        }
        SetBonusDiff diff = Calculate(counts, oldSet, newSet, table);
        var text = new StringBuilder("\n<color=#FFD480>교체 시 세트 효과</color>\n");
        if (diff.IsEmpty) { return text.Append("변화 없음").ToString(); }
        foreach (var tier in diff.Remove)
        {
            text.Append("<color=#FF5A5A>- ").Append(Loc.T(worn.itemData.setName ?? tier.set)).Append(" (")
                .Append(tier.tier).Append(") ").Append(TooltipBuilder.FormatSetEffect(table.EffectOf(tier.set, tier.tier))).Append("</color>\n");
        }
        foreach (var tier in diff.Apply)
        {
            text.Append("<color=#4CD964>+ ").Append(Loc.T(target.itemData.setName ?? tier.set)).Append(" (")
                .Append(tier.tier).Append(") ").Append(TooltipBuilder.FormatSetEffect(table.EffectOf(tier.set, tier.tier))).Append("</color>\n");
        }
        return text.ToString().TrimEnd();
    }
}
