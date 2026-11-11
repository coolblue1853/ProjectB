using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 툴팁·세트 창의 "특수 효과" 구획 (EquipEffectDesign §2.9·§7, P6). 효과 id → 자동 설명 줄.
/// 색 전용 구분 금지: 비활성은 "(비활성)", 상한은 "(상한)", 미달성 세트 단계는 "(잠김)" 문구를 붙인다. 순수 함수는 EditMode 테스트 대상.
/// </summary>
public static class EffectTooltip
{
    const string ColorEffect = "#9FE8FF";
    const string ColorOff = "#808080";
    const string ColorOn = "#FFD480";

    /// <summary>효과 id 목록(';')의 모든 행 설명. 모르는 id·빈 설명은 건너뛴다. status 가 있으면 줄 끝에 덧붙인다</summary>
    public static List<string> Lines(string effectIds, Func<string, string> tr, Func<EffectEntry, string> status = null)
    {
        var lines = new List<string>();
        foreach (string id in SkillEffectService.SplitIds(effectIds))
        {
            List<EffectEntry> entries = EffectTable.Get(id);
            if (entries == null)
            {
                continue;
            }
            for (int i = 0; i < entries.Count; i++)
            {
                string line = EffectDescriber.Describe(entries[i], tr);
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                lines.Add(status == null ? line : line + status(entries[i]));
            }
        }
        return lines;
    }

    /// <summary>줄 목록 → 색 입힌 블록(앞에 머리줄). 줄이 없으면 빈 문자열</summary>
    public static string Block(IList<string> lines, string header)
    {
        if (lines == null || lines.Count == 0)
        {
            return string.Empty;
        }
        var sb = new StringBuilder("\n<color=").Append(ColorEffect).Append('>').Append(header).Append("</color>\n");
        for (int i = 0; i < lines.Count; i++)
        {
            sb.Append("<color=").Append(ColorEffect).Append(">- ").Append(lines[i]).Append("</color>\n");
        }
        return sb.ToString();
    }

    /// <summary>아이템 툴팁용(상세창·호버 공용). 효과가 없으면 빈 문자열</summary>
    public static string ForItem(string effectIds, WeaponKind kind)
    {
        if (string.IsNullOrEmpty(effectIds))
        {
            return string.Empty;
        }
        return Block(Lines(effectIds, Loc.T, e => Status(e, kind)), Loc.T("Special Effects"));
    }

    /// <summary>세트 단계 효과 줄(세트 블록 단계 줄 아래에 붙인다, 끝에 개행 없음). 달성 = 세트색, 미달성 = 회색 + "(잠김)". 없으면 빈 문자열</summary>
    public static string ForSetTier(string effectIds, bool reached, WeaponKind kind)
    {
        if (string.IsNullOrEmpty(effectIds))
        {
            return string.Empty;
        }
        Func<EffectEntry, string> status = reached ? (Func<EffectEntry, string>)(e => Status(e, kind)) : null;
        List<string> lines = Lines(effectIds, Loc.T, status);
        var sb = new StringBuilder();
        for (int i = 0; i < lines.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('\n');
            }
            sb.Append("<color=").Append(reached ? ColorOn : ColorOff).Append(">    + ").Append(lines[i]);
            if (!reached)
            {
                sb.Append(' ').Append(Loc.T("(locked)"));
            }
            sb.Append("</color>");
        }
        return sb.ToString();
    }

    /// <summary>
    /// 현재 장착 상태에서 이 행의 상태 문구. 이름 선택자 스킬에 대해 Query: 슬롯 초과로 탈락한 Rule 이면 "(비활성)",
    /// 합산이 상한에 걸렸으면 "(상한)". 활성 효과가 없으면 빈 문자열(미장착 아이템 툴팁)
    /// </summary>
    public static string Status(EffectEntry e, WeaponKind kind)
    {
        if (e == null || SkillEffectService.ActiveCount == 0 || string.IsNullOrEmpty(e.selector) || e.selector == "all"
            || e.selector.IndexOf(':') >= 0 || e.selector.IndexOf('|') >= 0)
        {
            return string.Empty;
        }
        SkillMods m = SkillEffectService.Query(e.selector, kind);
        if (e.kind == EffectKind.Rule && m.inactiveRules != null && Array.IndexOf(m.inactiveRules, e) >= 0)
        {
            return " " + Loc.T("(inactive)");
        }
        return e.kind == EffectKind.Mod && m.capped ? " " + Loc.T("(capped)") : string.Empty;
    }
}
