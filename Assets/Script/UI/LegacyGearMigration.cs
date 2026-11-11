using System;
using System.Collections.Generic;

/// <summary>
/// 구 세이브 이행 (EQUIP-SLOT-REFORM v2): equipGear 인덱스 2(투구)·6(장갑)·8(하의)·9(신발)에 있던 아이템을 새 슬롯으로 옮긴다.
/// 아이템 시트가 이미 새 부위(Head→Earring, Hand→Ring, Leg→Necklace, Shoes→Charm)로 바뀌어 있으므로 부위는 시트에서 읽는다.
/// 결정 순서: 새 부위가 의상·무기·반지·목걸이이고 그 칸이 비었으면 그 칸 → 귀걸이·보조장비 등이고 해금·빈칸이면 그 슬롯 → 인벤토리 → 공간이 없으면 구 칸에 그대로 둔다(소실 금지, 다음 로드에 다시 시도).
/// 순수 규칙(Decide)은 테스트 가능하게 Unity 의존 없이 둔다.
/// </summary>
public static class LegacyGearMigration
{
    public static readonly int[] LegacyIndexes = { 2, 6, 8, 9 };

    public enum Target { Keep, Box, Extra, Bag }

    public static bool IsLegacyIndex(int i)
    {
        return Array.IndexOf(LegacyIndexes, i) >= 0;
    }

    /// <summary>시작 박스(equipGear 인덱스) 번호: 반지 1·목걸이 3·의상 5·무기 7. 그 밖이면 -1</summary>
    public static int BoxIndexOf(string area)
    {
        switch (area)
        {
            case "Ring": return 1;
            case "Necklace": return 3;
            case "Chest": return 5;
            case "Weapon": return 7;
            default: return -1;
        }
    }

    /// <summary>한 아이템이 어디로 갈지. area=시트의 현재 equipArea(null=시트에 없음). 반환 Keep 은 "지금은 못 옮김"</summary>
    public static Target Decide(string area, bool boxFree, bool extraFree, bool bagHasRoom)
    {
        if (string.IsNullOrEmpty(area) || SlotRules.IsRetiredArea(area))
        {
            return bagHasRoom ? Target.Bag : Target.Keep; // 시트에 없거나 아직 옛 부위인 아이템은 가방으로
        }
        if (BoxIndexOf(area) >= 0 && boxFree)
        {
            return Target.Box;
        }
        if (Enum.TryParse(area, out EquipSlot slot) && SlotRules.IsExtra(slot) && extraFree)
        {
            return Target.Extra;
        }
        return bagHasRoom ? Target.Bag : Target.Keep;
    }

    public sealed class Entry
    {
        public int Index;
        public string Name, Tier, Upgrade;
        public int Flags;
    }

    /// <summary>세이브 equipGear 에서 구 칸(2·6·8·9)의 아이템 목록을 뽑는다(지우지 않음)</summary>
    public static List<Entry> Collect(string[,] gear, int[] flags)
    {
        var list = new List<Entry>();
        if (gear == null)
        {
            return list;
        }
        foreach (int i in LegacyIndexes)
        {
            if (i < gear.GetLength(0) && !string.IsNullOrEmpty(gear[i, 0]))
            {
                list.Add(new Entry { Index = i, Name = gear[i, 0], Tier = gear[i, 1], Upgrade = gear[i, 2], Flags = flags != null && i < flags.Length ? flags[i] : 0 });
            }
        }
        return list;
    }

    /// <summary>구 칸 기록을 지운다(이행이 끝난 항목)</summary>
    public static void Clear(string[,] gear, int[] flags, int index)
    {
        for (int j = 0; j < 3; j++)
        {
            gear[index, j] = null;
        }
        if (flags != null && index < flags.Length)
        {
            flags[index] = 0;
        }
    }
}
