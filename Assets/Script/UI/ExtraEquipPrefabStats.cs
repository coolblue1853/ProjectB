using UnityEngine;

/// <summary>
/// 추가 장비 슬롯(귀걸이·보조장비 등)에 낀 아이템의 프리팹 스탯(Equipment 컴포넌트) 적용 (EQUIP-SLOT-REFORM v2).
/// 옛 투구·신발은 슬롯이 폐지되고 귀걸이·보조장비로 바뀌었지만 스탯은 Resources/{옛부위}/{이름} 프리팹에 있다 — ExtraEquipStats.csv 에 없는 아이템만 여기서 읽어 그대로 건다.
/// 출혈·쿨감 같은 특수 필드는 방어구 프리팹에 없어 다루지 않는다.
/// </summary>
public static class ExtraEquipPrefabStats
{
    /// <summary>
    /// 스탯을 source 로 다시 건다(호출 전에 PlayerStats.RemoveBySource 가 이전분을 지웠다고 본다). 최대 HP 는 아이템·강화가 바뀔 때만 다시 더한다
    /// (hpKey = 마지막으로 반영한 "이름|강화", hpApplied = 그때 더한 값; 다시 불러도 회복이 반복되지 않게).
    /// </summary>
    public static void Apply(object source, ItemData d, ref string hpKey, ref int hpApplied)
    {
        PlayerHealthManager phm = PlayerHealthManager.Instance;
        Equipment eq = d == null || ExtraEquipStats.For(d.name).Length > 0 ? null : GearPrefabCache.Comp<Equipment>(GearPrefabCache.Get(d.equipArea, d.name));
        string key = eq == null ? null : d.name + "|" + d.upgrade;
        if (key != hpKey && phm != null)
        {
            if (hpApplied != 0)
            {
                phm.EquipmentActiveFalse(hpApplied);
            }
            hpApplied = 0;
            hpKey = null;
        }
        if (eq == null)
        {
            return; // 표(ExtraEquipStats.csv)가 담당하는 아이템·빈칸
        }
        float mult = EnhanceRules.GearMultiplier(d.upgrade);
        Add(source, d, StatId.DropRate, eq.dropRate);
        Add(source, d, StatId.CritRate, eq.critical);
        Add(source, d, StatId.MoveSpeed, eq.moveSpeed);
        Add(source, d, StatId.AttackSpeed, eq.attSpeed);
        Add(source, d, StatId.CritDmg, eq.criticalDmg);
        Add(source, d, StatId.BasicDmg, Mathf.RoundToInt(eq.basicDmg * mult));
        Add(source, d, StatId.Def, Mathf.RoundToInt(Mathf.RoundToInt(eq.armor * mult) * Equipment.ArmorToDef));
        Add(source, d, StatId.BleedIncDmg, eq.bleedingDmgPer);
        Add(source, d, StatId.IncomingDmg, eq.addIncomingDmg);
        Add(source, d, StatId.IncDmg, eq.incDmg);
        Add(source, d, StatId.PoisonDmg, eq.poisonDmg);
        int hp = Mathf.RoundToInt(eq.hp * mult);
        if (key != hpKey && phm != null)
        {
            hpKey = key;
            hpApplied = hp;
            if (hp != 0)
            {
                phm.EquipmentActiveTrue(hp);
            }
        }
    }

    static void Add(object source, ItemData d, StatId id, int value)
    {
        if (value != 0)
        {
            PlayerStats.Add(new StatModifier(id, ModType.Add, value) { Source = source, Owner = "Extra:" + d.name });
        }
    }
}
