using System.Globalization;

/// <summary>스킬창·툴팁 문자열(순수). 노드 효과 한 줄·타입 이름·칭호·극의 설명</summary>
public static class SkillTreeText
{
    static readonly string[] Titles = { "견습", "숙련자", "전문가", "달인", "명인" };

    public static string TitleOf(int level)
    {
        return Titles[TypeMasteryRules.TitleIndex(level)];
    }

    public static string KindName(WeaponKind kind)
    {
        switch (kind)
        {
            case WeaponKind.Sword: return "검";
            case WeaponKind.Greatsword: return "대검";
            case WeaponKind.Scythe: return "낫";
            case WeaponKind.Dagger: return "단검";
            case WeaponKind.Knuckle: return "너클";
            case WeaponKind.Claw: return "클로";
            case WeaponKind.Spear: return "창";
            case WeaponKind.Adae: return "아대";
            case WeaponKind.Staff: return "지팡이";
            case WeaponKind.Axe: return "도끼";
            case WeaponKind.Mace: return "철퇴";
            case WeaponKind.Whip: return "채찍";
            case WeaponKind.Flail: return "플레일";
            case WeaponKind.Dual: return "쌍검";
            case WeaponKind.Tachi: return "태도";
            case WeaponKind.Bow: return "활";
            case WeaponKind.Crossbow: return "석궁";
            case WeaponKind.Tome: return "마도서";
            case WeaponKind.Javelin: return "투창";
            case WeaponKind.Bulwark: return "방벽";
            default: return "무기";
        }
    }

    public static string TabName(SkillTab tab, WeaponKind kind)
    {
        switch (tab)
        {
            case SkillTab.Type: return kind == WeaponKind.None ? "무기 숙련" : KindName(kind) + " 숙련";
            case SkillTab.Combat: return "전투";
            case SkillTab.Life: return "생활";
            default: return "편의성";
        }
    }

    public static string StatLabel(TreeStat stat)
    {
        switch (stat)
        {
            case TreeStat.AttackDamage: return "평타 데미지";
            case TreeStat.SkillDamage: return "스킬 데미지";
            case TreeStat.CritChance: return "치명타 확률";
            case TreeStat.CritDamage: return "치명타 피해";
            case TreeStat.AttackSpeed: return "공격 속도";
            case TreeStat.SkillCooldown: return "스킬 쿨다운";
            case TreeStat.StaminaCost: return "스킬 스태미나 소모";
            case TreeStat.MoveSpeed: return "이동 속도";
            case TreeStat.MaxHp: return "최대 HP";
            case TreeStat.Defense: return "방어";
            case TreeStat.StaminaRegen: return "스태미나 회복";
            default: return "극의";
        }
    }

    /// <summary>값 문자열. 치명타 확률은 %p, 쿨·소모는 부호 -, 나머지는 +%</summary>
    public static string ValueText(TreeStat stat, float value)
    {
        string v = value.ToString("0.##", CultureInfo.InvariantCulture);
        switch (stat)
        {
            case TreeStat.CritChance: return "+" + v + "%p";
            case TreeStat.CritDamage: return "+" + v + "%p";
            case TreeStat.SkillCooldown:
            case TreeStat.StaminaCost: return "-" + v + "%";
            default: return "+" + v + "%";
        }
    }

    public static string UltimateText(WeaponKind kind)
    {
        if (kind == WeaponKind.Staff || kind == WeaponKind.Tome)
        {
            return "발사체 데미지 +10%";
        }
        return "평타 마지막 타 데미지 +15%";
    }

    /// <summary>노드 효과 한 줄: "평타 데미지 +4% (다음 +2%)" / 최대면 다음 생략 / 극의는 설명 문구</summary>
    public static string EffectLine(SkillNode n, int level, WeaponKind kind)
    {
        if (n.Stat == TreeStat.Ultimate)
        {
            return UltimateText(kind) + (level > 0 ? " (적용 중)" : "");
        }
        string now = ValueText(n.Stat, n.PerLevel * level);
        string label = StatLabel(n.Stat);
        if (level >= n.MaxLevel)
        {
            return label + " " + now;
        }
        string next = ValueText(n.Stat, n.PerLevel);
        return level <= 0 ? label + " " + next + " (레벨당)" : label + " " + now + " (다음 " + next + ")";
    }

    /// <summary>선행 문구. 선행 없음이면 빈 문자열</summary>
    public static string PrereqText(SkillNode n)
    {
        if (n.PrereqId == null)
        {
            return string.Empty;
        }
        SkillNode p = SkillTreeRules.Find(n.PrereqId);
        return (p != null ? p.Name : n.PrereqId) + " Lv" + n.PrereqLevel + " 필요";
    }

    public static string InvestMessage(InvestResult r, SkillNode n)
    {
        switch (r)
        {
            case InvestResult.NoPoints: return "포인트가 부족합니다 (필요 " + (n != null ? n.CostPerLevel : 1) + ")";
            case InvestResult.PrereqUnmet: return n != null ? PrereqText(n) : "선행 노드 필요";
            case InvestResult.MaxLevel: return "이미 최대 레벨입니다";
            default: return "";
        }
    }

    /// <summary>합산 상한에 닿았는가(트리 출처). 평타·스킬 데미지는 합으로 판정</summary>
    public static bool IsCapped(TreeStat stat, float modAttack, float modSkill, float modSelf)
    {
        if (stat == TreeStat.AttackDamage || stat == TreeStat.SkillDamage)
        {
            return modAttack + modSkill >= SkillTreeRules.DamageSumCap - 0.01f;
        }
        float cap = SkillTreeRules.Cap(stat);
        return cap < float.MaxValue && modSelf >= cap - 0.01f;
    }
}
