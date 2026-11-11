using System;

/// <summary>
/// 장비 슬롯 규칙 (EquipSlotExpansion.md, v2 11-11) — 순수 C#.
/// 시작 5칸(의상·무기·반지·목걸이·귀걸이)은 항상 열려 있다. 해금 슬롯은 홀수 층계(1 보조장비·3 팔찌·5 허리띠·7 마법석)이고,
/// 층계→슬롯·해금 훅은 Resources/SlotUnlocks.csv(SlotUnlockTable)에서 읽는다. 짝수 층계(펫·룬·성물)는 슬롯이 아니라 별도 콘텐츠(Features).
/// 세이브 키: equip.extra(string[]), equip.extraUnlock(int 비트마스크). 키가 없으면 해금 슬롯은 잠금·빈칸(구 세이브 호환). 키 이름은 바꾸지 않고 배열 끝에만 추가한다.
/// </summary>
public static class SlotRules
{
    public const string SaveKeyExtra = "equip.extra";
    public const string SaveKeyUnlock = "equip.extraUnlock";

    /// <summary>세이브 배열 순서 = 비트 순서. 구 세이브 호환을 위해 순서 고정, 새 슬롯은 끝에만 추가(Belt)</summary>
    public static readonly EquipSlot[] Extras = { EquipSlot.Earring, EquipSlot.Bracelet, EquipSlot.Charm, EquipSlot.Stone, EquipSlot.Belt };

    public const int AllExtraMask = 31;

    /// <summary>시작 5칸(의상·무기·반지·목걸이·귀걸이) — 세트 효과가 세는 칸. 최대 5셋</summary>
    public static readonly EquipSlot[] Starting = { EquipSlot.Outfit, EquipSlot.Weapon, EquipSlot.Ring, EquipSlot.Necklace, EquipSlot.Earring };

    /// <summary>폐지된 슬롯(투구·장갑·하의·신발). 구 세이브·문자열 파싱 전용</summary>
    public static bool IsRetired(EquipSlot slot)
    {
        return slot == EquipSlot.Head || slot == EquipSlot.Hand || slot == EquipSlot.Leg || slot == EquipSlot.Shoes;
    }

    /// <summary>폐지 부위 이름("Head"·"Hand"·"Leg"·"Shoes")인가</summary>
    public static bool IsRetiredArea(string area)
    {
        return area == "Head" || area == "Hand" || area == "Leg" || area == "Shoes";
    }

    /// <summary>시작부터 열려 있는 추가 저장 슬롯(귀걸이)</summary>
    public static bool IsAlwaysOpen(EquipSlot slot)
    {
        return slot == EquipSlot.Earring;
    }

    /// <summary>층계 번호(홀수 1·3·5·7) → 해금되는 장비 슬롯. 짝수 층계(별도 콘텐츠)·범위 밖이면 false</summary>
    public static bool TrySlotForFloor(int floor, out EquipSlot slot)
    {
        return SlotUnlockTable.Current.TrySlotForFloor(floor, out slot);
    }

    /// <summary>해금형 저장 슬롯인가(귀걸이 포함 — 저장 배열에 속하는지)</summary>
    public static bool IsExtra(EquipSlot slot)
    {
        return Array.IndexOf(Extras, slot) >= 0;
    }

    /// <summary>1=Earring, 2=Bracelet, 4=Charm, 8=Stone, 16=Belt. 그 밖의 슬롯은 0</summary>
    public static int Bit(EquipSlot slot)
    {
        int i = Array.IndexOf(Extras, slot);
        return i < 0 ? 0 : 1 << i;
    }

    public static bool IsUnlocked(int mask, EquipSlot slot)
    {
        return !IsExtra(slot) || IsAlwaysOpen(slot) || (mask & Bit(slot)) != 0;
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
