using System;
using System.Collections.Generic;

/// <summary>트리가 올려 주는 스탯 종류. 값 단위는 퍼센트(포인트)</summary>
public enum TreeStat
{
    AttackDamage,
    SkillDamage,
    CritChance,
    CritDamage,
    AttackSpeed,
    /// <summary>스킬 쿨다운 감소량(양수 = 감소 %)</summary>
    SkillCooldown,
    /// <summary>스킬 스태미나 소모 감소량(양수 = 감소 %)</summary>
    StaminaCost,
    MoveSpeed,
    MaxHp,
    Defense,
    StaminaRegen,
    /// <summary>타입 극의(레벨 0/1)</summary>
    Ultimate,
}

/// <summary>스킬창 탭(WeaponSkillProgression.md V.3b). 타입 트리 노드는 Type, 공용 노드는 전투·생활·편의성</summary>
public enum SkillTab
{
    Type,
    Combat,
    Life,
    Convenience,
}

public enum InvestResult
{
    Ok,
    UnknownNode,
    MaxLevel,
    NoPoints,
    PrereqUnmet,
}

public sealed class SkillNode
{
    public readonly string Id;
    public readonly string Name;
    public readonly bool Common;
    public readonly int MaxLevel;
    public readonly int CostPerLevel;
    public readonly TreeStat Stat;
    public readonly float PerLevel;
    public readonly string PrereqId;
    public readonly int PrereqLevel;
    public readonly SkillTab Tab;

    public SkillNode(string id, string name, bool common, TreeStat stat, float perLevel, int maxLevel, int cost = 1, string prereqId = null, int prereqLevel = 0, SkillTab tab = SkillTab.Type)
    {
        Tab = tab;
        Id = id;
        Name = name;
        Common = common;
        Stat = stat;
        PerLevel = perLevel;
        MaxLevel = maxLevel;
        CostPerLevel = cost;
        PrereqId = prereqId;
        PrereqLevel = prereqLevel;
    }
}

/// <summary>
/// 스킬트리 순수 규칙 (WeaponSkillProgression.md V.2~V.4). 노드표·선행·상한·리스펙 비용.
/// 노드 ID 는 세이브 키라 불변이다(개정은 추가만).
/// </summary>
public static class SkillTreeRules
{
    /// <summary>공용 트리 이름. 타입 트리 이름은 WeaponKind 이름(Sword...)</summary>
    public const string CommonTree = "Common";

    /// <summary>리스펙 골드 단가(투입 포인트당)</summary>
    public const int RespecGoldPerPoint = 150;
    /// <summary>리스펙 배수 상한(누적 횟수)</summary>
    public const int RespecCountCap = 5;
    /// <summary>평타·스킬 데미지 합 상한(%)</summary>
    public const float DamageSumCap = 30f;

    static readonly SkillNode[] _nodes =
    {
        // 타입 트리 템플릿(모든 타입 공통, 극의 효과만 타입별)
        new SkillNode("P1", "평타 단련", false, TreeStat.AttackDamage, 2f, 5),
        new SkillNode("P2", "기술 단련", false, TreeStat.SkillDamage, 3f, 3, 1, "P1", 3),
        new SkillNode("P3", "극의", false, TreeStat.Ultimate, 1f, 1, 3, "P2", 2),
        new SkillNode("A1", "급소", false, TreeStat.CritChance, 0.5f, 5),
        new SkillNode("A2", "치명", false, TreeStat.CritDamage, 3f, 5, 1, "A1", 3),
        new SkillNode("M1", "연타", false, TreeStat.AttackSpeed, 1.5f, 5),
        new SkillNode("M2", "숙달", false, TreeStat.SkillCooldown, 3f, 3, 1, "M1", 3),
        new SkillNode("M3", "효율", false, TreeStat.StaminaCost, 4f, 3, 1, "M1", 3),
        // 공용 트리(선행 없음)
        new SkillNode("C1", "급소 감각", true, TreeStat.CritChance, 0.5f, 10, 1, null, 0, SkillTab.Combat),
        new SkillNode("C2", "치명 감각", true, TreeStat.CritDamage, 2f, 10, 1, null, 0, SkillTab.Combat),
        new SkillNode("C3", "손놀림", true, TreeStat.AttackSpeed, 1f, 10, 1, null, 0, SkillTab.Combat),
        new SkillNode("C4", "경공", true, TreeStat.MoveSpeed, 1f, 5, 1, null, 0, SkillTab.Convenience),
        new SkillNode("C5", "강인함", true, TreeStat.MaxHp, 2f, 10, 1, null, 0, SkillTab.Combat),
        new SkillNode("C6", "단단함", true, TreeStat.Defense, 1f, 10, 1, null, 0, SkillTab.Combat),
        new SkillNode("C7", "호흡", true, TreeStat.StaminaRegen, 2f, 5, 1, null, 0, SkillTab.Convenience),
    };

    public static IReadOnlyList<SkillNode> Nodes
    {
        get { return _nodes; }
    }

    /// <summary>탭에 속한 노드(표 순서). 생활 탭은 아직 노드가 없다(채집·낚시·요리·제작 훅 확인 뒤 추가)</summary>
    public static List<SkillNode> NodesOf(SkillTab tab)
    {
        var list = new List<SkillNode>();
        for (int i = 0; i < _nodes.Length; i++)
        {
            if (_nodes[i].Tab == tab)
            {
                list.Add(_nodes[i]);
            }
        }
        return list;
    }

    public static SkillNode Find(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        for (int i = 0; i < _nodes.Length; i++)
        {
            if (_nodes[i].Id == id)
            {
                return _nodes[i];
            }
        }
        return null;
    }

    /// <summary>트리 전체 포인트 용량(타입 32, 공용 60)</summary>
    public static int Capacity(bool common)
    {
        int sum = 0;
        for (int i = 0; i < _nodes.Length; i++)
        {
            if (_nodes[i].Common == common)
            {
                sum += _nodes[i].MaxLevel * _nodes[i].CostPerLevel;
            }
        }
        return sum;
    }

    /// <summary>공용 트리 획득 포인트 = 플레이어 Lv-1 (Lv2 부터 1씩). 저장하지 않는다</summary>
    public static int CommonEarnedPoints(int playerLevel)
    {
        return Math.Max(0, playerLevel - 1);
    }

    /// <summary>투자 가능 판정. levelOf = 같은 트리 노드 ID → 현재 레벨</summary>
    public static InvestResult CheckInvest(string nodeId, bool commonTree, Func<string, int> levelOf, int available)
    {
        SkillNode n = Find(nodeId);
        if (n == null || n.Common != commonTree)
        {
            return InvestResult.UnknownNode;
        }
        if (levelOf(nodeId) >= n.MaxLevel)
        {
            return InvestResult.MaxLevel;
        }
        if (n.PrereqId != null && levelOf(n.PrereqId) < n.PrereqLevel)
        {
            return InvestResult.PrereqUnmet;
        }
        return available >= n.CostPerLevel ? InvestResult.Ok : InvestResult.NoPoints;
    }

    /// <summary>합산 상한(트리 출처만, %). 상한 없는 스탯은 float.MaxValue</summary>
    public static float Cap(TreeStat stat)
    {
        switch (stat)
        {
            case TreeStat.CritChance: return 10f;
            case TreeStat.CritDamage: return 40f;
            case TreeStat.AttackSpeed: return 25f;
            case TreeStat.AttackDamage:
            case TreeStat.SkillDamage: return DamageSumCap;
            case TreeStat.MoveSpeed: return 10f;
            case TreeStat.SkillCooldown: return 15f;
            case TreeStat.StaminaCost: return 20f;
            default: return float.MaxValue;
        }
    }

    public static float Clamp(TreeStat stat, float value)
    {
        return Math.Max(0f, Math.Min(value, Cap(stat)));
    }

    /// <summary>
    /// 정수 스탯(PlayerStats.GetInt = 반올림)에 얹을 트리 값. 0.5·1.5 단위 합이 은행가 반올림(0.5→0, 2.5→2)으로 뭉치지 않게
    /// 0 에서 먼 쪽으로 올린다(레벨마다 단조 증가: 0.5·n → 1,1,2,2,3).
    /// </summary>
    public static float ToWholePoints(float value)
    {
        return (float)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    /// <summary>극의 배율. 마지막 타 계열(느린 무기는 더 크게)과 전 타격 계열(연타·원거리·캐스터는 작게)</summary>
    public static float UltimateMul(WeaponKind kind, bool isSkill, bool isFinalHit)
    {
        switch (kind)
        {
            case WeaponKind.Staff:
            case WeaponKind.Tome:
                return 1.10f;
            case WeaponKind.Dagger:
            case WeaponKind.Dual:
            case WeaponKind.Claw:
            case WeaponKind.Knuckle:
                return 1.06f;
            case WeaponKind.Bow:
            case WeaponKind.Crossbow:
                return 1.08f;
            case WeaponKind.Greatsword:
            case WeaponKind.Axe:
            case WeaponKind.Mace:
            case WeaponKind.Scythe:
                return !isSkill && isFinalHit ? 1.20f : 1f;
            default:
                return !isSkill && isFinalHit ? 1.15f : 1f;
        }
    }

    /// <summary>평타+스킬 데미지 합이 상한을 넘으면 비율을 유지한 채 줄인다</summary>
    public static void ClampDamagePair(ref float attack, ref float skill)
    {
        float sum = attack + skill;
        if (sum > DamageSumCap && sum > 0f)
        {
            float k = DamageSumCap / sum;
            attack *= k;
            skill *= k;
        }
    }

    /// <summary>리스펙 골드. priorRespecs = 그 트리의 지난 초기화 횟수. 첫 1회(0) 무료</summary>
    public static int RespecCost(int spentPoints, int priorRespecs)
    {
        if (priorRespecs <= 0 || spentPoints <= 0)
        {
            return 0;
        }
        return RespecGoldPerPoint * spentPoints * Math.Min(priorRespecs, RespecCountCap);
    }
}
