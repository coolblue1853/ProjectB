using System;
using System.Collections.Generic;

/// <summary>스킬 투자 한 건의 계획: 어느 탭의 몇 번째 줄(노드)에 투자할지</summary>
public struct BotSkillPick
{
    public bool Found;
    public string Tree;
    public string NodeId;
    public SkillTab Tab;
    public int Row;
}

/// <summary>
/// 봇 스킬 투자 규칙(순수, EditMode 시험). 포인트가 있으면 우선순위표에서 첫 투자 가능 노드를 고른다.
/// 타입 트리(무기 숙련 포인트) 먼저, 다음 공용 트리(플레이어 Lv-1 포인트). 설계 PlayBotDesign.md 16절
/// </summary>
public static class BotSkillRules
{
    /// <summary>타입 트리 우선순위(노드 id, 목표 레벨 상한). 평타 → 급소 → 연타를 먼저, 그다음 기술·치명·숙달</summary>
    public static readonly KeyValuePair<string, int>[] TypeOrder =
    {
        new KeyValuePair<string, int>("P1", 5),
        new KeyValuePair<string, int>("A1", 3),
        new KeyValuePair<string, int>("M1", 3),
        new KeyValuePair<string, int>("P2", 3),
        new KeyValuePair<string, int>("A2", 3),
        new KeyValuePair<string, int>("M2", 3),
        new KeyValuePair<string, int>("A1", 5),
        new KeyValuePair<string, int>("M1", 5),
        new KeyValuePair<string, int>("A2", 5),
        new KeyValuePair<string, int>("M3", 3),
        new KeyValuePair<string, int>("P3", 1),
    };

    /// <summary>공용 트리 우선순위: 강인함·단단함·손놀림·급소 감각·치명 감각을 라운드로 올린 뒤 경공·호흡</summary>
    public static readonly KeyValuePair<string, int>[] CommonOrder = BuildCommon();

    private static KeyValuePair<string, int>[] BuildCommon()
    {
        var list = new List<KeyValuePair<string, int>>();
        string[] ids = { "C5", "C6", "C3", "C1", "C2" };
        int[] caps = { 3, 6, 10 };
        for (int c = 0; c < caps.Length; c++)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                list.Add(new KeyValuePair<string, int>(ids[i], caps[c]));
            }
        }
        list.Add(new KeyValuePair<string, int>("C4", 5));
        list.Add(new KeyValuePair<string, int>("C7", 5));
        return list.ToArray();
    }

    /// <summary>
    /// 투자할 노드를 고른다. typeTree 가 null 이면 타입 트리는 건너뛴다(무기 없음). levelOf(tree, nodeId) = 현재 노드 레벨.
    /// 포인트 0 이면 못 고른다.
    /// </summary>
    public static BotSkillPick Pick(string typeTree, int availType, int availCommon, Func<string, string, int> levelOf)
    {
        if (typeTree != null && availType > 0)
        {
            BotSkillPick p = PickFrom(typeTree, availType, TypeOrder, levelOf);
            if (p.Found)
            {
                return p;
            }
        }
        if (availCommon > 0)
        {
            return PickFrom(SkillTreeRules.CommonTree, availCommon, CommonOrder, levelOf);
        }
        return default(BotSkillPick);
    }

    private static BotSkillPick PickFrom(string tree, int avail, KeyValuePair<string, int>[] order, Func<string, string, int> levelOf)
    {
        bool common = tree == SkillTreeRules.CommonTree;
        for (int i = 0; i < order.Length; i++)
        {
            SkillNode n = SkillTreeRules.Find(order[i].Key);
            if (n == null || levelOf(tree, n.Id) >= order[i].Value)
            {
                continue;
            }
            if (SkillTreeRules.CheckInvest(n.Id, common, id => levelOf(tree, id), avail) != InvestResult.Ok)
            {
                continue;
            }
            List<SkillNode> rows = SkillTreeRules.NodesOf(n.Tab);
            int row = rows.FindIndex(r => r.Id == n.Id);
            if (row < 0)
            {
                continue;
            }
            return new BotSkillPick { Found = true, Tree = tree, NodeId = n.Id, Tab = n.Tab, Row = row };
        }
        return default(BotSkillPick);
    }

    /// <summary>스킬창 안에서 목표 줄까지 누를 키 한 번. 탭 → 줄(위아래 순환 중 가까운 쪽) → 확인. 목표가 없으면 Back</summary>
    public static BotKey Step(bool found, SkillTab curTab, int cursor, SkillTab wantTab, int wantRow, int rowCount)
    {
        if (!found)
        {
            return BotKey.Back;
        }
        if (curTab != wantTab)
        {
            return BotKey.Right; // 탭은 오른쪽으로 한 칸씩 순환(Type → Combat → Life → Convenience)
        }
        if (rowCount <= 0)
        {
            return BotKey.Back;
        }
        if (cursor == wantRow)
        {
            return BotKey.Confirm;
        }
        int down = ((wantRow - cursor) % rowCount + rowCount) % rowCount;
        return down <= rowCount - down ? BotKey.Down : BotKey.Up;
    }
}
