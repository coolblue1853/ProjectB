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

    /// <summary>고급 강화 난수 공급원(0~1). 프로브·테스트가 고정값으로 바꾼다</summary>
    public static System.Func<float> Rng = () => Random.value;

    /// <summary>마지막 시도에서 실제로 인벤에 돌려준 결정 수(정밀 단조 9점 실패)와 적용된 최종 성공률(0~1, 고급 구간만)</summary>
    public static int LastRefund { get; private set; }
    public static float LastRate { get; private set; }

    /// <summary>현재 단계에서 더 올릴 수 있는가: +10 이하는 기존 규칙, +10~+14 는 해금 비트가 있어야 한다</summary>
    public static bool CanEnhance(int level)
    {
        return AdvancedEnhanceRules.IsAdvanced(level) ? ForgeState.IsUnlocked(level) : EnhanceRules.CanEnhance(level);
    }

    public static int StonesFor(int level)
    {
        return AdvancedEnhanceRules.IsAdvanced(level) ? AdvancedEnhanceRules.Stones(level) : EnhanceRules.Stones(level);
    }

    public static int GoldFor(int level)
    {
        return AdvancedEnhanceRules.IsAdvanced(level) ? AdvancedEnhanceRules.Gold(level) : EnhanceRules.Gold(level);
    }

    public static Check CanTry(ItemCheck item)
    {
        if (item == null || item.itemData == null || item.itemData.type != "Equip")
        {
            return Check.NotEquipment;
        }
        int lv = item.itemData.upgrade;
        if (!CanEnhance(lv))
        {
            return Check.MaxLevel;
        }
        if (InventoryManager.instance.CountItem(EnhanceRules.StoneName) < StonesFor(lv))
        {
            return Check.NoStones;
        }
        if (DatabaseManager.money < GoldFor(lv))
        {
            return Check.NoGold;
        }
        return Check.Ok;
    }

    /// <returns>결과. Check가 Ok가 아니면 MaxLevel/Fail 대신 null 의미로 false 반환</returns>
    public static bool TryEnhance(ItemCheck item, out EnhanceRules.Result result, out int newLevel, int forgeScore = 0)
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
        InventoryManager.instance.DeletItemByName(EnhanceRules.StoneName, StonesFor(lv));
        DatabaseManager.money -= GoldFor(lv);

        int streak = FailStreak;
        bool advanced = AdvancedEnhanceRules.IsAdvanced(lv);
        LastRefund = 0;
        LastRate = 0f;
        if (advanced)
        {
            result = RollAdvanced(lv, forgeScore, out newLevel);
        }
        else
        {
            result = EnhanceRules.Roll(lv, Random.value, streak, out newLevel);
        }
        InventoryService service = InventoryManager.instance.Service;
        if (service.IndexOf(item) < 0)
        {
            item.itemData.upgrade = newLevel; // 인벤 밖(지금 강화 창은 인벤 아이템만 보여 주므로 예외 경로)
        }
        else if (!service.SetUpgrade(item, newLevel)) // 인벤 칸이면 모델 명령으로(3c-2)
        {
            GameLog.Warn(GameLog.Cat.Item, "강화 수치 반영 실패(모델 거절): " + item.name);
        }
        if (!advanced) // +11 이상은 별도 천장(ForgeState.Pity)
        {
            FailStreak = result == EnhanceRules.Result.Success ? 0 : streak + 1;
        }

        InventoryManager.instance.SaveInventory();
        Sfx.Play(result == EnhanceRules.Result.Success ? (newLevel >= 5 ? SfxId.EnhanceGreat : SfxId.EnhanceSuccess) : SfxId.EnhanceFail);
        if (result == EnhanceRules.Result.Success)
        {
            FeelFx.EnhanceSuccess(newLevel);
        }
        GameLog.Info(GameLog.Cat.Item, "강화 " + item.itemData.name + " +" + lv + " → +" + newLevel + " (" + result + ", 연속실패 " + FailStreak + ")");
        return true;
    }

    /// <summary>고급 강화 한 번: 단조 점수 반영 성공률로 굴리고, 천장 보조 갱신, 9점 실패 시 결정 반환. 실패는 단계 유지</summary>
    static EnhanceRules.Result RollAdvanced(int lv, int forgeScore, out int newLevel)
    {
        int pity = ForgeState.Pity;
        LastRate = AdvancedEnhanceRules.FinalRate(lv, pity, forgeScore);
        bool ok = Rng() < LastRate;
        if (ok)
        {
            newLevel = lv + 1;
            ForgeState.Pity = 0;
            return EnhanceRules.Result.Success;
        }
        newLevel = lv;
        ForgeState.Pity = pity + 1;
        int refund = AdvancedEnhanceRules.RefundStones(AdvancedEnhanceRules.Stones(lv), forgeScore);
        if (refund > 0)
        {
            // 환불(<= 소모량의 절반)은 방금 차감해 비운 칸에 들어간다. 그래도 못 넣으면 소실을 로그로 남기고 UI 에는 0 으로 알린다.
            if (InventoryManager.instance.Service.Add(EnhanceRules.StoneName, refund, AddSource.Other))
            {
                LastRefund = refund;
            }
            else
            {
                GameLog.Warn(GameLog.Cat.Item, "정밀 단조 환불 실패(인벤 가득): 결정 " + refund);
            }
        }
        return EnhanceRules.Result.Fail;
    }
}
