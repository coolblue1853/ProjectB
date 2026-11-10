using System;

/// <summary>
/// [v2 예정 11-10] 해금 규칙이 바뀐다: 시작 5칸(의상·무기·반지·목걸이·귀걸이) 외에 홀수 층계(1 보조장비·3 팔찌·5 허리띠·7 마법석)만 해금 슬롯이고,
/// 해금 조건은 보스 처치가 아니라 스토리 진행이다. 짝수 층계(2 펫·4 룬·6 성물)는 슬롯이 아니라 별도 콘텐츠라 여기에 넣지 않는다.
/// 현재 코드는 옛 4칸(귀걸이·팔찌·부적·마법석)·층계 1~4 순서다. 세이브 키 이름은 바꾸지 않고 추가만 한다.
/// 신규 장비 슬롯(EquipSlotExpansion.md 4~5절) 규칙 — 순수 C#. 기본 8칸은 항상 해금, 신규 4칸은 해금 비트가 필요하다.
/// 세이브 키: equip.extra(string[4]), equip.extraUnlock(int 비트마스크). 키가 없으면 전부 잠금·빈칸(구 세이브 호환).
/// </summary>
public static class SlotRules
{
    public const string SaveKeyExtra = "equip.extra";
    public const string SaveKeyUnlock = "equip.extraUnlock";

    /// <summary>세이브 배열 순서 = 비트 순서</summary>
    public static readonly EquipSlot[] Extras = { EquipSlot.Earring, EquipSlot.Bracelet, EquipSlot.Charm, EquipSlot.Stone };

    public const int AllExtraMask = 15;

    /// <summary>층계 번호(1~4, 탐험가 승급 rank 와 같음) → 해금되는 슬롯. 범위 밖이면 false</summary>
    public static bool TrySlotForFloor(int floor, out EquipSlot slot)
    {
        slot = default;
        if (floor < 1 || floor > Extras.Length)
        {
            return false;
        }
        slot = Extras[floor - 1];
        return true;
    }

    public static bool IsExtra(EquipSlot slot)
    {
        return Array.IndexOf(Extras, slot) >= 0;
    }

    /// <summary>1=Earring, 2=Bracelet, 4=Charm, 8=Stone. 기본 슬롯은 0</summary>
    public static int Bit(EquipSlot slot)
    {
        int i = Array.IndexOf(Extras, slot);
        return i < 0 ? 0 : 1 << i;
    }

    public static bool IsUnlocked(int mask, EquipSlot slot)
    {
        return !IsExtra(slot) || (mask & Bit(slot)) != 0;
    }

    public static int Unlock(int mask, EquipSlot slot)
    {
        return mask | Bit(slot);
    }

    /// <summary>"id" 또는 강화·티어가 있으면 "id|티어|강화". 빈칸은 ""</summary>
    public static string EncodeStack(ItemStack s)
    {
        if (s.IsEmpty)
        {
            return "";
        }
        return s.Tier > 0 || s.Upgrade > 0 ? s.Id + "|" + s.Tier + "|" + s.Upgrade : s.Id;
    }

    public static ItemStack DecodeStack(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return ItemStack.Empty;
        }
        string[] p = text.Split('|');
        if (p[0].Length == 0)
        {
            return ItemStack.Empty;
        }
        int tier = p.Length > 1 && int.TryParse(p[1], out int t) ? t : 0;
        int up = p.Length > 2 && int.TryParse(p[2], out int u) ? u : 0;
        return new ItemStack(p[0], 1, tier, up);
    }
}

/// <summary>신규 슬롯 ES3 입출력 (얇은 래퍼 — 로직은 EquipmentModel/SlotRules). 읽기 실패 시 잠금·빈칸</summary>
public static class EquipExtraSave
{
    public static void Save(EquipmentModel model)
    {
        ES3.Save(SlotRules.SaveKeyExtra, model.ToExtraArray());
        ES3.Save(SlotRules.SaveKeyUnlock, model.ExtraUnlockMask);
    }

    public static void Load(EquipmentModel model, SlotContainer inventory)
    {
        string[] ids = null;
        int mask = 0;
        try
        {
            ids = SaveUtil.LoadOrThrow<string[]>(SlotRules.SaveKeyExtra, null);
            mask = SaveUtil.LoadOrThrow(SlotRules.SaveKeyUnlock, 0);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "신규 장비 슬롯 세이브 읽기 실패, 잠금·빈칸으로 시작: " + e.Message);
            ids = null;
            mask = 0;
        }
        model.LoadExtra(ids, mask, inventory);
    }
}
