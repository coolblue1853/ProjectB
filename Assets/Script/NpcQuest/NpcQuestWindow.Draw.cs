using System.Collections.Generic;
using UnityEngine;

/// <summary>퀘스트 창의 그리기 절반: 목록 정렬·행 만들기·상세·버튼 표시. 입력·상태는 NpcQuestWindow.cs, 화면 부품은 QuestWindowView, 문구는 QuestWindowText.</summary>
public partial class NpcQuestWindow
{
    // ───── 그리기 ─────

    private QuestFilter FilterFor(QuestListRules.Tab tab)
    {
        return Clone(_entrance, tab);
    }

    private void Redraw()
    {
        if (!_open)
        {
            return;
        }
        QuestView availV = QuestService.Query(FilterFor(QuestListRules.Tab.Available));
        QuestView activeV = QuestService.Query(FilterFor(QuestListRules.Tab.Active));
        QuestView doneV = QuestService.Query(FilterFor(QuestListRules.Tab.Done));
        QuestView view = _tab == QuestListRules.Tab.Available ? availV : _tab == QuestListRules.Tab.Active ? activeV : doneV; // 탭마다 Query 를 한 번씩만
        int availN = availV.Items.Count;
        int activeN = activeV.Items.Count;
        int doneN = doneV.Items.Count;
        SetTabLabel(0, "받을 수 있는 " + availN);
        SetTabLabel(1, "진행 중 " + activeN);
        SetTabLabel(2, "완료 " + doneN);
        _v.Title.text = _entrance != null ? "입구" : "퀘스트";

        BuildRows(OrderForTab(view.Items));
        _cursor = Mathf.Clamp(_cursor, 0, Mathf.Max(0, _rows.Count - 1));
        if (_cursor < _first)
        {
            _first = _cursor;
        }
        else if (_cursor >= _first + QuestWindowView.Rows)
        {
            _first = _cursor - QuestWindowView.Rows + 1;
        }
        _first = Mathf.Clamp(_first, 0, Mathf.Max(0, _rows.Count - QuestWindowView.Rows));
        DrawRows();
        DrawDetail();
        _v.Hint.text = QuestWindowText.Hint(_entrance != null);
    }

    private void SetTabLabel(int i, string text)
    {
        bool on = (int)_tab == i;
        _v.TabText[i].text = on ? "<color=#FFFFFF>[" + text + "]</color>" : "<color=#777777>" + text + "</color>";
    }

    /// <summary>받을 수 있는 탭: 수락 가능 → 레벨 부족 순, 진행 중: 완료 가능이 맨 위. 각각 구역별로 묶는다</summary>
    private List<NpcQuestDef> OrderForTab(List<NpcQuestDef> items)
    {
        var first = new List<NpcQuestDef>();
        var rest = new List<NpcQuestDef>();
        foreach (NpcQuestDef d in items)
        {
            NpcQuestStatus s = QuestService.StatusOf(d);
            bool lead = _tab == QuestListRules.Tab.Active ? s == NpcQuestStatus.Ready : s != NpcQuestStatus.LevelLow;
            (_tab == QuestListRules.Tab.Done || lead ? first : rest).Add(d);
        }
        if (_tab == QuestListRules.Tab.Active)
        {
            first.AddRange(QuestListRules.GroupByZone(rest, QuestService.ZoneOf));
            return first;
        }
        List<NpcQuestDef> res = QuestListRules.GroupByZone(first, QuestService.ZoneOf);
        res.AddRange(QuestListRules.GroupByZone(rest, QuestService.ZoneOf));
        return res;
    }

    private void BuildRows(List<NpcQuestDef> ordered)
    {
        _rows.Clear();
        Row head = null;
        string last = null;
        foreach (NpcQuestDef d in ordered)
        {
            NpcQuestStatus s = QuestService.StatusOf(d);
            string header = QuestWindowText.Header(d, _tab == QuestListRules.Tab.Active && s == NpcQuestStatus.Ready, _tab == QuestListRules.Tab.Available && s == NpcQuestStatus.LevelLow);
            if (header != last)
            {
                last = header;
                string key = QuestFoldRules.Key(_tab, header);
                head = new Row { Key = key, Text = header, Folded = QuestFoldRules.IsFolded(key) };
                _rows.Add(head);
            }
            head.Count++;
            if (!head.Folded)
            {
                _rows.Add(new Row { Quest = d });
            }
        }
    }

    private void DrawRows()
    {
        List<NpcQuestDef> active = QuestService.ActiveAll();
        for (int i = 0; i < QuestWindowView.Rows; i++)
        {
            int idx = _first + i;
            bool has = idx < _rows.Count;
            _v.RowButtons[i].gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }
            Row row = _rows[idx];
            if (row.Quest == null)
            {
                _v.RowText[i].text = (row.Folded ? "[+] " : "[-] ") + row.Text + (row.Folded ? " <color=#999999>(" + row.Count + ")</color>" : "");
            }
            else
            {
                bool tracked = _tab == QuestListRules.Tab.Active && NpcQuestTrack.IsTracked(row.Quest.Id, active);
                _v.RowText[i].text = QuestWindowText.RowLine(row.Quest, _tab, QuestService.StatusOf(row.Quest), tracked);
            }
            _v.RowBg[i].color = idx == _cursor ? new Color(1f, 0.85f, 0.4f, 0.32f) : new Color(1f, 1f, 1f, row.Quest == null ? 0.05f : 0f);
        }
        _v.ListInfo.text = _rows.Count > QuestWindowView.Rows ? (_cursor + 1) + "/" + _rows.Count : "";
    }

    private void DrawDetail()
    {
        NpcQuestDef cur = CurrentQuest();
        bool entrance = _entrance != null;
        if (_rows.Count == 0)
        {
            _v.Detail.text = "<color=#999999>" + QuestWindowText.Empty(_tab) + "</color>";
        }
        else if (cur == null)
        {
            _v.Detail.text = "<color=#999999>구역 머리글 - Z · A · 클릭으로 접기/펼치기</color>";
        }
        ShowActions(cur);
        if (cur == null)
        {
            _v.RewardHeader.gameObject.SetActive(false);
            _v.RewardIcon.gameObject.SetActive(false);
            _v.RewardText.text = "";
            _v.Guide.text = _notice;
            return;
        }
        NpcQuestStatus s = QuestService.StatusOf(cur);
        _v.Detail.text = QuestWindowText.Detail(cur, _tab, s);
        _v.RewardHeader.gameObject.SetActive(true);
        _v.RewardText.text = QuestWindowText.RewardLine(cur);
        Sprite icon = string.IsNullOrEmpty(cur.RewardItem) ? null : Resources.Load<Sprite>("Item/" + cur.RewardItem);
        _v.RewardIcon.sprite = icon;
        _v.RewardIcon.gameObject.SetActive(icon != null);
        List<NpcQuestDef> active = QuestService.ActiveAll();
        string guide = QuestWindowText.Guide(cur, _tab, s, entrance, NpcQuestTrack.IsTracked(cur.Id, active), NpcQuestTrack.Tracked(active).Count);
        if (_notice.Length > 0)
        {
            guide += "\n" + _notice;
        }
        if (entrance && _entrance.Source == "bounty")
        {
            guide += "\n<color=#AAAAAA>" + BountyGraduation.Check() + "</color>";
        }
        _v.Guide.text = guide;
    }

    /// <summary>버튼 3개: 입구 모드는 수락/보고 + 추적 + 포기, 일반 모드는 추적 + 포기 (보기만 하는 탭은 숨김)</summary>
    private void ShowActions(NpcQuestDef cur)
    {
        bool activeTab = _tab == QuestListRules.Tab.Active && cur != null;
        bool entrance = _entrance != null;
        bool primaryOn = entrance && cur != null && _tab != QuestListRules.Tab.Done;
        _v.Primary.gameObject.SetActive(primaryOn);
        _v.Track.gameObject.SetActive(activeTab);
        _v.Abandon.gameObject.SetActive(activeTab);
        if (primaryOn)
        {
            _v.PrimaryText.text = _tab == QuestListRules.Tab.Available ? "수락 (Enter · A)" : "보고 (Enter · A)";
        }
        if (activeTab)
        {
            bool tracked = NpcQuestTrack.IsTracked(cur.Id, QuestService.ActiveAll());
            _v.TrackText.text = (tracked ? "추적 해제" : "추적") + " (Z · X)";
            _v.AbandonText.text = "포기 (Del · Y)";
        }
    }
}
