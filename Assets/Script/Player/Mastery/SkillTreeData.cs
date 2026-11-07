using System;
using System.Collections.Generic;

/// <summary>
/// ES3 저장 형식(키 skillTree). 노드 ID → 레벨(공용은 "C1", 타입은 "Sword.P1"), 트리별 초기화 횟수.
/// 필드는 이름을 바꾸지 않는다.
/// </summary>
[Serializable]
public class SkillTreeSave
{
    public string[] nodeIds = new string[0];
    public int[] levels = new int[0];
    public string[] respecTrees = new string[0];
    public int[] respecCounts = new int[0];
}

public enum RespecResult
{
    Ok,
    Nothing,
    NoGold,
}

/// <summary>트리 상태(순수). 트리 이름 = SkillTreeRules.CommonTree 또는 WeaponKind 이름. 포인트 획득량(earned)은 호출자가 넘긴다</summary>
public sealed class SkillTreeData
{
    readonly Dictionary<string, Dictionary<string, int>> _trees = new Dictionary<string, Dictionary<string, int>>();
    readonly Dictionary<string, int> _respec = new Dictionary<string, int>();

    public static bool IsValidTree(string tree)
    {
        return tree == SkillTreeRules.CommonTree || TypeMasteryData.IsValidKind(tree);
    }

    public static string FullId(string tree, string nodeId)
    {
        return tree == SkillTreeRules.CommonTree ? nodeId : tree + "." + nodeId;
    }

    public int GetLevel(string tree, string nodeId)
    {
        Dictionary<string, int> t;
        int v;
        return tree != null && nodeId != null && _trees.TryGetValue(tree, out t) && t.TryGetValue(nodeId, out v) ? v : 0;
    }

    public int Spent(string tree)
    {
        Dictionary<string, int> t;
        if (tree == null || !_trees.TryGetValue(tree, out t))
        {
            return 0;
        }
        int sum = 0;
        foreach (KeyValuePair<string, int> kv in t)
        {
            SkillNode n = SkillTreeRules.Find(kv.Key);
            if (n != null)
            {
                sum += n.CostPerLevel * kv.Value;
            }
        }
        return sum;
    }

    public int RespecCount(string tree)
    {
        int v;
        return tree != null && _respec.TryGetValue(tree, out v) ? v : 0;
    }

    /// <summary>1레벨 투자. 성공 시 레벨 +1</summary>
    public InvestResult Invest(string tree, string nodeId, int earned)
    {
        if (!IsValidTree(tree))
        {
            return InvestResult.UnknownNode;
        }
        InvestResult r = SkillTreeRules.CheckInvest(nodeId, tree == SkillTreeRules.CommonTree, id => GetLevel(tree, id), earned - Spent(tree));
        if (r == InvestResult.Ok)
        {
            SetLevel(tree, nodeId, GetLevel(tree, nodeId) + 1);
        }
        return r;
    }

    void SetLevel(string tree, string nodeId, int level)
    {
        Dictionary<string, int> t;
        if (!_trees.TryGetValue(tree, out t))
        {
            t = new Dictionary<string, int>();
            _trees[tree] = t;
        }
        t[nodeId] = level;
    }

    /// <summary>다음 초기화에 드는 골드(첫 1회 0)</summary>
    public int RespecCostNow(string tree)
    {
        return SkillTreeRules.RespecCost(Spent(tree), RespecCount(tree));
    }

    /// <summary>트리 전체 초기화(환불 100%). 횟수 +1. 반환 = 환불 포인트(0 이면 아무 일도 안 함)</summary>
    public int ClearForRespec(string tree)
    {
        int refunded = Spent(tree);
        if (refunded <= 0)
        {
            return 0;
        }
        _trees.Remove(tree);
        _respec[tree] = RespecCount(tree) + 1;
        return refunded;
    }

    /// <summary>획득량보다 많이 투자된 트리를 무료로 비운다(곡선·노드 개정 안전망). 횟수는 올리지 않는다. 비운 트리 수 반환</summary>
    public int FixOverspent(Func<string, int> earnedOf)
    {
        List<string> bad = null;
        foreach (string tree in _trees.Keys)
        {
            if (Spent(tree) > earnedOf(tree))
            {
                if (bad == null)
                {
                    bad = new List<string>();
                }
                bad.Add(tree);
            }
        }
        if (bad == null)
        {
            return 0;
        }
        foreach (string tree in bad)
        {
            _trees.Remove(tree);
        }
        return bad.Count;
    }

    /// <summary>트리 노드 효과 합(%, 상한 전). 해당 트리의 stat 노드 레벨 x PerLevel</summary>
    public float Sum(string tree, TreeStat stat)
    {
        Dictionary<string, int> t;
        if (tree == null || !_trees.TryGetValue(tree, out t))
        {
            return 0f;
        }
        float sum = 0f;
        foreach (KeyValuePair<string, int> kv in t)
        {
            SkillNode n = SkillTreeRules.Find(kv.Key);
            if (n != null && n.Stat == stat)
            {
                sum += n.PerLevel * kv.Value;
            }
        }
        return sum;
    }

    /// <summary>집계: 공용 + 타입 트리(typeTree 가 null 이면 공용만), 합산 상한 적용. Ultimate 는 타입 트리만(0/1)</summary>
    public float Mod(TreeStat stat, string typeTree)
    {
        string tt = IsValidKind(typeTree) ? typeTree : null;
        if (stat == TreeStat.Ultimate)
        {
            return Sum(tt, stat);
        }
        if (stat == TreeStat.AttackDamage || stat == TreeStat.SkillDamage)
        {
            float atk = Sum(tt, TreeStat.AttackDamage) + Sum(SkillTreeRules.CommonTree, TreeStat.AttackDamage);
            float skl = Sum(tt, TreeStat.SkillDamage) + Sum(SkillTreeRules.CommonTree, TreeStat.SkillDamage);
            SkillTreeRules.ClampDamagePair(ref atk, ref skl);
            return stat == TreeStat.AttackDamage ? atk : skl;
        }
        return SkillTreeRules.Clamp(stat, Sum(tt, stat) + Sum(SkillTreeRules.CommonTree, stat));
    }

    static bool IsValidKind(string name)
    {
        return TypeMasteryData.IsValidKind(name);
    }

    public SkillTreeSave ToSave()
    {
        var ids = new List<string>();
        var lvs = new List<int>();
        foreach (KeyValuePair<string, Dictionary<string, int>> tree in _trees)
        {
            foreach (KeyValuePair<string, int> kv in tree.Value)
            {
                if (kv.Value > 0)
                {
                    ids.Add(FullId(tree.Key, kv.Key));
                    lvs.Add(kv.Value);
                }
            }
        }
        var s = new SkillTreeSave { nodeIds = ids.ToArray(), levels = lvs.ToArray() };
        s.respecTrees = new string[_respec.Count];
        s.respecCounts = new int[_respec.Count];
        int i = 0;
        foreach (KeyValuePair<string, int> kv in _respec)
        {
            s.respecTrees[i] = kv.Key;
            s.respecCounts[i] = kv.Value;
            i++;
        }
        return s;
    }

    /// <summary>알 수 없는 노드·트리는 무시한다(삭제된 노드 = 자동 환불). 레벨은 최대치로 자른다</summary>
    public void LoadFrom(SkillTreeSave s)
    {
        _trees.Clear();
        _respec.Clear();
        if (s == null)
        {
            return;
        }
        if (s.nodeIds != null && s.levels != null)
        {
            int n = Math.Min(s.nodeIds.Length, s.levels.Length);
            for (int i = 0; i < n; i++)
            {
                LoadNode(s.nodeIds[i], s.levels[i]);
            }
        }
        if (s.respecTrees != null && s.respecCounts != null)
        {
            int n = Math.Min(s.respecTrees.Length, s.respecCounts.Length);
            for (int i = 0; i < n; i++)
            {
                if (IsValidTree(s.respecTrees[i]) && s.respecCounts[i] > 0)
                {
                    _respec[s.respecTrees[i]] = s.respecCounts[i];
                }
            }
        }
    }

    void LoadNode(string fullId, int level)
    {
        if (string.IsNullOrEmpty(fullId) || level <= 0)
        {
            return;
        }
        string tree = SkillTreeRules.CommonTree;
        string nodeId = fullId;
        int dot = fullId.IndexOf('.');
        if (dot > 0)
        {
            tree = fullId.Substring(0, dot);
            nodeId = fullId.Substring(dot + 1);
        }
        SkillNode node = SkillTreeRules.Find(nodeId);
        if (node == null || !IsValidTree(tree) || node.Common != (tree == SkillTreeRules.CommonTree))
        {
            return;
        }
        SetLevel(tree, nodeId, Math.Min(level, node.MaxLevel));
    }
}
