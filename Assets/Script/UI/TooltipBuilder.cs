using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 디아식 장비 툴팁 문구 (InventoryRefactorDesign 3.1, D3). 기존 툴팁 레이아웃(고정 자식 TMP)에 리치 텍스트로 채운다.
/// - 이름: 등급색 + 강화 접두 "+3 "
/// - 스탯: 강화 배율 반영, 부호는 값에서 한 번만(B-016), 착용품과 비교해 ▲/▼, 착용품에만 있는 스탯은 빨간 "-X" 줄
/// - 세트: "< 이름 > (2/5)", 착용 수에 도달한 단계는 세트색, 아직이면 회색
/// 스탯 목록(Stat)만으로 동작하는 부분은 순수 함수라 EditMode 테스트 대상이다.
/// </summary>
public static class TooltipBuilder
{
    public const string ColorCommon = "#D8D8D8", ColorRare = "#5A8CFF", ColorUnique = "#E0A53C";
    public const string ColorSet = "#3FCF6A", ColorOff = "#7A7A7A", ColorUp = "#4CD964", ColorDown = "#FF5A5A";

    public struct Stat
    {
        public string Key;   // 비교용 키 (번역 전 영어 라벨, 쿨감은 + 스킬 이름)
        public string Label; // 표시 라벨 (번역 후)
        public float Value;
        public string Unit;  // "" 또는 "%"

        public Stat(string key, string label, float value, string unit = "")
        {
            Key = key;
            Label = label;
            Value = value;
            Unit = unit;
        }
    }

    public static string RarityColor(string rarity)
    {
        switch (rarity)
        {
            case "R": return ColorRare;
            case "U": return ColorUnique;
            default: return ColorCommon;
        }
    }

    public static string RarityName(string rarity)
    {
        switch (rarity)
        {
            case "C": return Loc.T("Common");
            case "R": return Loc.T("Rare");
            case "U": return Loc.T("Unique");
            default: return "";
        }
    }

    /// <summary>이름 줄: 등급색 + "+3 " 접두</summary>
    public static string Name(string displayName, string rarity, int upgrade)
    {
        return "<color=" + RarityColor(rarity) + ">" + EnhanceRules.DisplayName(displayName, upgrade) + "</color>";
    }

    /// <summary>부호는 값에서 한 번만: +5 / -5 (B-016 "--5" 방지)</summary>
    public static string Signed(float v)
    {
        string n = Mathf.Approximately(v, Mathf.Round(v)) ? Mathf.RoundToInt(v).ToString() : v.ToString("0.#");
        return v > 0 ? "+" + n : n;
    }

    /// <summary>스탯 줄들. other 가 null 이 아니면 같은 키끼리 비교해 ▲/▼ 를 붙이고, other 에만 있는 스탯은 빨간 줄로 덧붙인다</summary>
    public static string StatLines(List<Stat> mine, List<Stat> other)
    {
        var sb = new StringBuilder();
        foreach (Stat s in mine)
        {
            sb.Append(Signed(s.Value)).Append(s.Unit).Append(' ').Append(s.Label);
            if (other != null)
            {
                AppendDelta(sb, s.Value - Find(other, s.Key), s.Unit);
            }
            sb.Append('\n');
        }
        if (other != null)
        {
            foreach (Stat o in other)
            {
                if (!Has(mine, o.Key) && !Mathf.Approximately(o.Value, 0f))
                {
                    sb.Append("<color=").Append(ColorDown).Append(">").Append(Signed(-o.Value)).Append(o.Unit).Append(' ').Append(o.Label).Append("</color>\n");
                }
            }
        }
        return sb.ToString();
    }

    /// <summary>"  ▲+3" 초록 / "  ▼-2" 빨강 / 같으면 붙이지 않음</summary>
    public static void AppendDelta(StringBuilder sb, float delta, string unit)
    {
        if (Mathf.Abs(delta) < 0.05f)
        {
            return;
        }
        sb.Append("  <color=").Append(delta > 0 ? ColorUp : ColorDown).Append(">")
          .Append(delta > 0 ? "▲" : "▼").Append(Signed(delta)).Append(unit).Append("</color>");
    }

    static float Find(List<Stat> list, string key)
    {
        foreach (Stat s in list)
        {
            if (s.Key == key)
            {
                return s.Value;
            }
        }
        return 0f;
    }

    static bool Has(List<Stat> list, string key)
    {
        foreach (Stat s in list)
        {
            if (s.Key == key)
            {
                return true;
            }
        }
        return false;
    }

    static void Add(List<Stat> list, string key, float value, string unit = "", string labelKey = null)
    {
        if (!Mathf.Approximately(value, 0f))
        {
            list.Add(new Stat(key, Loc.T(labelKey ?? key), value, unit));
        }
    }

    // ── 무기 ──

    /// <summary>무기 주 스탯 줄: "12 ~ 24 피해" + 비교(평균 피해)</summary>
    public static string WeaponDamageLine(int min, int max, int otherMin, int otherMax, bool compare)
    {
        var sb = new StringBuilder();
        sb.Append(min).Append(" ~ ").Append(max).Append(' ').Append(Loc.T("Dmg"));
        if (compare)
        {
            AppendDelta(sb, (min + max) / 2f - (otherMin + otherMax) / 2f, "");
        }
        return sb.Append('\n').ToString();
    }

    public static void WeaponDamage(Weapon w, int upgrade, out int min, out int max)
    {
        float mul = EnhanceRules.WeaponMultiplier(upgrade);
        min = Mathf.RoundToInt(w.minDmg * mul);
        max = Mathf.RoundToInt(w.maxDmg * mul);
    }

    public static List<Stat> WeaponStats(Weapon w)
    {
        var list = new List<Stat>();
        Add(list, "Critical Chance", w.critPer);
        Add(list, "Critical Dmg", w.critDmg);
        Add(list, "Dmg Increase", w.incDmg, "", w.incDmg < 0 ? "Dmg Decrease" : null);
        Add(list, "Armor Penetration", w.ignDef);
        Add(list, "Skill Dmg", w.skillDmg);
        Add(list, "Additional Dmg", w.addDmg);
        return list;
    }

    public static string WeaponBlock(Weapon w, int upgrade, Weapon other, int otherUpgrade)
    {
        WeaponDamage(w, upgrade, out int min, out int max);
        int omin = 0, omax = 0;
        if (other != null)
        {
            WeaponDamage(other, otherUpgrade, out omin, out omax);
        }
        return WeaponDamageLine(min, max, omin, omax, other != null) + StatLines(WeaponStats(w), other != null ? WeaponStats(other) : null);
    }

    // ── 방어구·장신구 ──

    public static List<Stat> ArmorStats(Equipment e, int upgrade)
    {
        float mul = EnhanceRules.GearMultiplier(upgrade);
        var list = new List<Stat>();
        Add(list, "Basic Dmg", Mathf.RoundToInt(e.basicDmg * mul));
        Add(list, "Def", Mathf.RoundToInt(e.armor * mul));
        Add(list, "Hp", Mathf.RoundToInt(e.hp * mul));
        Add(list, "Critical Rate", e.critical);
        Add(list, "Drop Rate", e.dropRate);
        Add(list, "Movement Speed", e.moveSpeed);
        Add(list, "Attack Speed", e.attSpeed);
        Add(list, "Critical Dmg", e.criticalDmg);
        if (e.coolDownSkill != null)
        {
            foreach (Equipment.SkillCoolTime c in e.coolDownSkill)
            {
                if (c.coolDownCount != 0)
                {
                    list.Add(new Stat("Cooldown " + c.skillName, Loc.T("Cooldown") + " " + c.skillName, -c.coolDownCount, "%"));
                }
            }
        }
        Add(list, "Dmg", e.incDmg, "%");
        Add(list, "Incoming Dmg", e.addIncomingDmg, "%");
        if (e.isBleeding)
        {
            Add(list, "Chance to Cause Bleeding", e.bleedingPerCent, "%");
        }
        Add(list, "Dmg to Bleeding Enemies", e.bleedingDmgPer, "%");
        Add(list, "Poison Dmg", e.poisonDmg);
        return list;
    }

    public static string ArmorBlock(Equipment e, int upgrade, Equipment other, int otherUpgrade)
    {
        return StatLines(ArmorStats(e, upgrade), other != null ? ArmorStats(other, otherUpgrade) : null);
    }

    // ── 세트 ──

    /// <summary>세트 블록: 머리줄 "< 이름 > (착용/전체)" + 단계별 효과(도달 단계는 세트색, 아니면 회색)</summary>
    public static string SetBlock(string setDisplay, int worn, int total, IList<KeyValuePair<int, string>> tiers)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(setDisplay))
        {
            sb.Append("<color=").Append(ColorSet).Append(">< ").Append(setDisplay).Append(" >");
            if (total > 0)
            {
                sb.Append(" (").Append(worn).Append('/').Append(total).Append(')');
            }
            sb.Append("</color>\n");
        }
        foreach (KeyValuePair<int, string> t in tiers)
        {
            sb.Append("<color=").Append(worn >= t.Key ? ColorSet : ColorOff).Append(">(").Append(t.Key).Append(") ")
              .Append(t.Value).Append("</color>\n");
        }
        return sb.ToString();
    }

    /// <summary>SetItem CSV 스탯 키 → 표시용 영어(번역 키)</summary>
    static string SetStatName(string key)
    {
        switch (key)
        {
            case "AtkSpeed": return "Attack Speed";
            case "BleedDmg": return "Dmg to Bleeding Enemies";
            case "Crit": return "Critical Rate";
            case "CritDmg": return "Critical Dmg";
            case "DropRate": return "Drop Rate";
            case "IncDmg": return "Dmg Increase";
            case "MoveSpeed": return "Movement Speed";
            case "Poison": return "Poison Dmg";
            default: return key;
        }
    }

    /// <summary>SetItem CSV 효과 문자열 "(+ 5 armor)/(+ 1 skillHitCount Rising_Roots)" → 읽을 수 있는 줄들</summary>
    public static string FormatSetEffect(string effect)
    {
        var sb = new StringBuilder();
        string[] parts = effect.Replace("(", "").Replace(")", " ").Split('/');
        for (int k = 0; k < parts.Length; k++)
        {
            string[] e = parts[k].Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (e.Length < 3)
            {
                sb.Append(parts[k].Trim());
            }
            else if ((e[2] == "skillHitCount" || e[2] == "bulletCount") && e.Length > 3)
            {
                sb.Append(e[0]).Append(e[1]).Append(" [").Append(e[3].Replace("_", " ")).Append("] ").Append(Loc.T("count"));
            }
            else if (e[2] == "coolDown" && e.Length > 3)
            {
                sb.Append('-').Append(e[1]).Append("% ").Append(Loc.T("CoolDown")).Append(" [").Append(e[3].Replace("_", " ")).Append(']');
            }
            else
            {
                sb.Append(e[0]).Append(e[1]).Append(' ').Append(Loc.T(SetStatName(e[2])));
            }
            if (k != parts.Length - 1)
            {
                sb.Append(",\n    ");
            }
        }
        return sb.ToString();
    }
}
