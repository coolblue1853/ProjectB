using System.Text;
using UnityEngine;

/// <summary>
/// 퀘스트 창(Q) 표시 문구 (REWORK-5 P2). 창 코드와 분리해 순수 함수로 둔다 — EditMode 테스트가 문자열을 확인한다.
/// 모든 문구는 한글. 그래픽 없이 색·글자 태그만 쓴다.
/// </summary>
public static class QuestWindowText
{
    public const string Gold = "#FFD633";
    public const string Grey = "#888888";

    /// <summary>유형 이름: 주 메인 / 서 사이드 / 보 보상 / 현 현상</summary>
    public static string KindName(NpcQuestDef d)
    {
        switch (QuestService.KindTag(d))
        {
            case "주": return "메인";
            case "서": return "사이드";
            case "현": return "현상";
            default: return "보상";
        }
    }

    public static string Header(NpcQuestDef d, bool ready, bool low)
    {
        if (ready)
        {
            return "<color=" + Gold + ">- 완료 가능 -</color>";
        }
        string zone = QuestService.ZoneOf(d);
        string label = string.IsNullOrEmpty(zone) ? "기타" : zone;
        return low ? "<color=" + Grey + ">- 레벨 부족 · " + label + " -</color>" : "<color=#88AACC>- " + label + " -</color>";
    }

    /// <summary>목록 한 줄: 상태 표시(! ? *) + 제목 + 오른쪽 끝 유형 한 글자</summary>
    public static string RowLine(NpcQuestDef d, QuestListRules.Tab tab, NpcQuestStatus status, bool tracked)
    {
        bool ready = status == NpcQuestStatus.Ready;
        bool low = tab == QuestListRules.Tab.Available && status == NpcQuestStatus.LevelLow;
        string color = tab == QuestListRules.Tab.Done ? "#AAAAAA" : ready ? Gold : low ? Grey : "#FFFFFF";
        string mark = ready ? "?" : tab == QuestListRules.Tab.Available ? "!" : tab == QuestListRules.Tab.Active && tracked ? "*" : "";
        string markColor = tab == QuestListRules.Tab.Active && !ready ? "#7FE0FF" : color;
        var sb = new StringBuilder();
        sb.Append("<color=").Append(markColor).Append('>').Append(mark.Length > 0 ? mark : " ").Append("</color> ");
        sb.Append("<color=").Append(color).Append('>').Append(d.Title);
        if (low)
        {
            sb.Append(" (Lv ").Append(d.MinLevel).Append(')');
        }
        sb.Append("</color><pos=312><color=#777777>").Append(QuestService.KindTag(d)).Append("</color>");
        return sb.ToString();
    }

    /// <summary>상세 위쪽: 제목·Lv·유형, 요약 대사, 요구 사항</summary>
    public static string Detail(NpcQuestDef d, QuestListRules.Tab tab, NpcQuestStatus status)
    {
        var sb = new StringBuilder();
        sb.Append("<color=").Append(Gold).Append('>').Append(d.Title).Append("</color>  <color=#9ED0FF>Lv ").Append(d.MinLevel)
          .Append("</color>  <color=#BBBBBB>[").Append(QuestService.KindTag(d)).Append(' ').Append(KindName(d)).Append("]</color>\n");
        sb.Append("<color=#CCCCCC>").Append(Summary(d)).Append("</color>\n\n");
        sb.Append("<color=#88AACC>-- 요구 사항 --</color>\n");
        bool ready = status == NpcQuestStatus.Ready;
        sb.Append(ready ? "<color=" + Gold + ">" : "").Append(QuestService.Objective(d)).Append(ready ? "  (달성)</color>" : "");
        string night = tab == QuestListRules.Tab.Active ? QuestNightHint.Line(d, ready, QuestNightHint.SecondsUntilNightNow()) : "";
        if (night.Length > 0)
        {
            sb.Append("\n<color=#9FA8FF>").Append(night).Append("</color>");
        }
        if (d.Objective == NpcQuestObjective.Collect && !d.IsRecordsTarget && tab != QuestListRules.Tab.Done)
        {
            sb.Append("\n<color=#777777>가방에 든 수량만 셉니다. 창고 보관분은 꺼내야 제출됩니다.</color>");
        }
        return sb.ToString();
    }

    /// <summary>요약 대사 한 줄 (description, 비면 목표 문장)</summary>
    public static string Summary(NpcQuestDef d)
    {
        return string.IsNullOrEmpty(d.Description) ? QuestService.Objective(d) : d.Description;
    }

    /// <summary>보상 줄 글자 (아이콘은 따로): 아이템 ×n · EXP · 골드 · 강화석 · 성물</summary>
    public static string RewardLine(NpcQuestDef d)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(d.RewardItem))
        {
            parts.Add(ItemText.DisplayName(d.RewardItem) + " x" + Mathf.Max(1, d.RewardCount));
        }
        if (d.Exp > 0)
        {
            parts.Add("EXP " + d.Exp);
        }
        if (d.Gold > 0)
        {
            parts.Add(d.Gold + " G");
        }
        if (d.Stones > 0)
        {
            parts.Add("강화석 " + d.Stones);
        }
        if (!string.IsNullOrEmpty(d.Artifact))
        {
            parts.Add("성물 해금");
        }
        return parts.Count == 0 ? "-" : string.Join("   ", parts);
    }

    /// <summary>안내 문구: 어디서 수락하나 / 누구에게 보고하나 / 추적 상태</summary>
    public static string Guide(NpcQuestDef d, QuestListRules.Tab tab, NpcQuestStatus status, bool entrance, bool tracked, int trackedCount)
    {
        var sb = new StringBuilder();
        string zone = QuestService.ZoneOf(d);
        string at = string.IsNullOrEmpty(zone) ? "" : " (@" + zone + ")";
        switch (tab)
        {
            case QuestListRules.Tab.Available:
                if (status == NpcQuestStatus.LevelLow)
                {
                    sb.Append("<color=#FF9966>Lv ").Append(d.MinLevel).Append(" 부터 받을 수 있습니다</color>");
                }
                else if (entrance)
                {
                    sb.Append("<color=").Append(Gold).Append(">수락할 수 있습니다</color>");
                }
                else
                {
                    sb.Append("<color=").Append(Gold).Append(">수락: ").Append(NpcQuestCatalog.NpcName(d.Giver)).Append("에게 말을 겁니다</color>");
                }
                break;
            case QuestListRules.Tab.Active:
                sb.Append(status == NpcQuestStatus.Ready
                    ? "<color=" + Gold + ">" + QuestService.ReportTarget(d) + "에게 가서 보고하세요</color>"
                    : "완료 시: " + QuestService.ReportTarget(d) + "에게 보고" + at);
                sb.Append('\n').Append(tracked ? "<color=#7FE0FF>* 추적 중</color>" : "<color=#777777>추적 안 함</color>")
                  .Append("  (").Append(trackedCount).Append('/').Append(NpcQuestTrack.Max).Append(')');
                break;
            default:
                sb.Append("<color=#88DD88>완료한 퀘스트</color>");
                break;
        }
        return sb.ToString();
    }

    public static string Hint(bool entrance)
    {
        return entrance
            ? "↑↓ 선택  ←→ 탭  Enter 수락·보고  Z 추적  Del 포기  Esc 닫기\n패드: ↕ 선택  LB/RB 탭  A 수락·보고  X 추적  Y 포기  B 닫기"
            : "↑↓ 선택  ←→ 탭  Z 추적/접기  Del 포기  H 추적창 접기  Q·Esc 닫기\n패드: ↕ 선택  LB/RB 탭  A·X 추적  Y 포기  LT 추적창  B 닫기";
    }

    public static string Empty(QuestListRules.Tab tab)
    {
        switch (tab)
        {
            case QuestListRules.Tab.Available: return "지금 받을 수 있는 퀘스트가 없습니다.\n레벨을 올리거나 선행 퀘스트를 끝내 보세요.";
            case QuestListRules.Tab.Active: return "진행 중인 퀘스트가 없습니다.\n마을 NPC 머리 위 ! 를 찾아 보세요.";
            default: return "완료한 퀘스트가 없습니다.";
        }
    }
}
