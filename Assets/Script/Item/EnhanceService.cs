using UnityEngine;

/// <summary>
/// 강화 실행: 재료·골드 확인 → 차감 → EnhanceRules.Roll → 아이템 upgrade 갱신 → 저장·연출.
/// 연속 실패 수는 ES3("enhanceFailStreak")로 저장해 재접속해도 찬스타임이 유지된다.
/// </summary>
public static class EnhanceService
{
    const string StreakKey = "enhanceFailStreak";

    public static int FailStreak
    {
        get { return ES3.KeyExists(StreakKey) ? ES3.Load<int>(StreakKey) : 0; }
        private set { ES3.Save(StreakKey, value); }
    }

    public enum Check { Ok, MaxLevel, NoStones, NoGold, NotEquipment }

    public static Check CanTry(ItemCheck item)
    {
        if (item == null || item.itemData == null || item.itemData.type != "Equip")
        {
            return Check.NotEquipment;
        }
        int lv = item.itemData.upgrade;
        if (!EnhanceRules.CanEnhance(lv))
        {
            return Check.MaxLevel;
        }
        if (InventoryManager.instance.CountItem(EnhanceRules.StoneName) < EnhanceRules.Stones(lv))
        {
            return Check.NoStones;
        }
        if (DatabaseManager.money < EnhanceRules.Gold(lv))
        {
            return Check.NoGold;
        }
        return Check.Ok;
    }

    /// <returns>결과. Check가 Ok가 아니면 MaxLevel/Fail 대신 null 의미로 false 반환</returns>
    public static bool TryEnhance(ItemCheck item, out EnhanceRules.Result result, out int newLevel)
    {
        result = EnhanceRules.Result.Fail;
        newLevel = item != null && item.itemData != null ? item.itemData.upgrade : 0;
        Check check = CanTry(item);
        if (check != Check.Ok)
        {
            GameLog.Info(GameLog.Cat.Item, "강화 불가: " + check);
            Sfx.Play(SfxId.CraftFail);
            return false;
        }
        int lv = item.itemData.upgrade;
        InventoryManager.instance.DeletItemByName(EnhanceRules.StoneName, EnhanceRules.Stones(lv));
        DatabaseManager.money -= EnhanceRules.Gold(lv);

        int streak = FailStreak;
        result = EnhanceRules.Roll(lv, Random.value, streak, out newLevel);
        item.itemData.upgrade = newLevel;
        FailStreak = result == EnhanceRules.Result.Success ? 0 : streak + 1;

        InventoryManager.instance.RebuildItemStackCache();
        InventoryManager.instance.SaveInventory();
        Sfx.Play(result == EnhanceRules.Result.Success ? (newLevel >= 5 ? SfxId.RareDrop : SfxId.Craft) : SfxId.CraftFail);
        if (result == EnhanceRules.Result.Success)
        {
            FeelFx.EnhanceSuccess(newLevel);
        }
        GameLog.Info(GameLog.Cat.Item, "강화 " + item.itemData.name + " +" + lv + " → +" + newLevel + " (" + result + ", 연속실패 " + FailStreak + ")");
        return true;
    }
}
