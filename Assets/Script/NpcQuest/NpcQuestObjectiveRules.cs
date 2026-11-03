using System.Collections.Generic;

/// <summary>신규 퀘스트 목표(killElite·equip·enhance·bestiary)의 target 문법 해석과 기본 상태 조회 (문법은 NpcQuestCatalog 주석)</summary>
public static class NpcQuestObjectiveRules
{
    public const string ArmorTarget = "armor";
    public const string WeaponTarget = "weapon";
    public const string SetsPrefix = "sets:";
    public const string AnyTarget = "any";
    public const string AnyElite = "*";

    private static readonly string[] ArmorAreas = { "Head", "Chest", "Hand", "Leg", "Shoes" };

    /// <summary>"sets:N" 이면 N(최소 부위 수)을 돌려준다</summary>
    public static bool TryParseSets(string target, out int minParts)
    {
        minParts = 0;
        return target != null && target.StartsWith(SetsPrefix, System.StringComparison.Ordinal)
            && int.TryParse(target.Substring(SetsPrefix.Length), System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out minParts) && minParts > 0;
    }

    /// <summary>N부위 이상 낀 세트 종류 수</summary>
    public static int CountSets(IReadOnlyDictionary<string, int> setCounts, int minParts)
    {
        int n = 0;
        foreach (KeyValuePair<string, int> kv in setCounts)
        {
            if (kv.Value >= minParts)
            {
                n++;
            }
        }
        return n;
    }

    /// <summary>강화 퀘스트 target 이 이 부위의 강화를 인정하는가</summary>
    public static bool EnhanceMatches(string target, string equipArea)
    {
        if (target == WeaponTarget)
        {
            return equipArea == "Weapon";
        }
        if (target == ArmorTarget)
        {
            return System.Array.IndexOf(ArmorAreas, equipArea) >= 0;
        }
        return true; // any
    }

    /// <summary>정예 처치 퀘스트 target 이 이 몬스터를 인정하는가</summary>
    public static bool EliteMatches(string target, string monsterId)
    {
        return target == AnyElite || target == monsterId;
    }

    // ── 기본 상태 조회 (실제 게임 상태) ──

    public static int DefaultEquipCount(string target)
    {
        if (target == ArmorTarget)
        {
            int n = 0;
            foreach (string area in ArmorAreas)
            {
                if (Worn(area) != null)
                {
                    n++;
                }
            }
            return n;
        }
        if (TryParseSets(target, out int minParts))
        {
            return CountSets(DatabaseManager.setsEffectStack, minParts);
        }
        return DatabaseManager.setsEffectStack.TryGetValue(target, out int parts) ? parts : 0;
    }

    /// <summary>착용 중인 장비 중 target 에 맞는 최고 강화 단계</summary>
    public static int DefaultEnhanceSeed(string target)
    {
        int best = 0;
        foreach (string area in new[] { "Weapon", "Head", "Chest", "Hand", "Leg", "Shoes", "Ring", "Necklace" })
        {
            ItemCheck worn = EnhanceMatches(target, area) ? Worn(area) : null;
            if (worn != null && worn.itemData != null && worn.itemData.upgrade > best)
            {
                best = worn.itemData.upgrade;
            }
        }
        return best;
    }

    public static int DefaultBestiaryKinds()
    {
        return BestiaryRecord.KindsKilled;
    }

    private static ItemCheck Worn(string area)
    {
        return InventoryManager.instance != null ? InventoryManager.instance.EquippedFor(area) : null;
    }
}
