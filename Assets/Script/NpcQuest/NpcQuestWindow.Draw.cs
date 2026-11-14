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
        int availN = availV.Items.Count;
        int activeN = activeV.Items.Count;
        int doneN = doneV.Items.Count;
        SetTabLabel(QuestListRules.Tab.All, "전체 " + (availN + activeN + doneN));
        SetTabLabel(QuestListRules.Tab.Available, "받을 수 있는 " + availN);
        SetTabLabel(QuestListRules.Tab.Active, "진행 중 " + activeN);
        SetTabLabel(QuestListRules.Tab.Done, "완료 " + doneN);
        _v.Title.text = QuestWindowText.EntranceTitle(_entrance);

        List<NpcQuestDef> items; // 탭마다 Query 를 한 번씩만 — 전체 탭은 세 결과를 상태순으로 합친다(QuestService.Query(All) 과 같은 규칙)
        switch (_tab)
        {
            case QuestListRules.Tab.Available: items = availV.Items; break;
            case QuestListRules.Tab.Active: items = activeV.Items; break;
            case QuestListRules.Tab.Done: items = doneV.Items; break;
            default:
                items = QuestService.MergeByState(availV.Items, activeV.Items, doneV.Items);
                break;
        }
        BuildRows(OrderForTab(items));
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

    private void SetTabLabel(QuestListRules.Tab tab, string text)
    {
        int i = (int)tab;
        bool on = _tab == tab;
        _v.TabText[i].text = on ? "<color=#FFFFFF>[" + text + "]</color>" : "<color=#777777>" + text + "</color>";
    }

    /// <summary>받을 수 있는 탭: 수락 가능 → 레벨 부족 순, 진행 중: 완료 가능이 맨 위. 각각 구역별로 묶는다. 전체 탭은 이미 상태순(상태별 머리글)</summary>
    private List<NpcQuestDef> OrderForTab(List<NpcQuestDef> items)
    {
        if (_tab == QuestListRules.Tab.All)
        {
            return items;
        }
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
            string header = _tab == QuestListRules.Tab.All ? QuestWindowText.StateHeader(s)
                : QuestWindowText.Header(d, _tab == QuestListRules.Tab.Active && s == NpcQuestStatus.Ready, _tab == QuestListRules.Tab.Available && s == NpcQuestStatus.LevelLow);
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
            Sprite icon = row.Quest != null ? QuestIcon(row.Quest) : null;
            _v.RowIcon[i].sprite = icon;
            _v.RowIcon[i].gameObject.SetActive(icon != null); // 없으면 숨김(흰 네모 금지)
            if (row.Quest == null)
            {
                _v.RowText[i].text = (row.Folded ? "[+] " : "[-] ") + row.Text + (row.Folded ? " <color=#999999>(" + row.Count + ")</color>" : "");
            }
            else
            {
                QuestListRules.Tab tab = RowTab(row.Quest);
                bool tracked = tab == QuestListRules.Tab.Active && NpcQuestTrack.IsTracked(row.Quest.Id, active);
                _v.RowText[i].text = QuestWindowText.RowLine(row.Quest, tab, QuestService.StatusOf(row.Quest), tracked);
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
            _v.Detail.text = "<color=#999999>묶음 머리글 - Z · A · 클릭으로 접기/펼치기</color>";
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
        QuestListRules.Tab tab = RowTab(cur);
        _v.Detail.text = QuestWindowText.Detail(cur, tab, s);
        _v.RewardHeader.gameObject.SetActive(true);
        _v.RewardText.text = QuestWindowText.RewardLine(cur);
        Sprite icon = string.IsNullOrEmpty(cur.RewardItem) ? null : ItemIconLoader.Load(cur.RewardItem);
        _v.RewardIcon.sprite = icon;
        _v.RewardIcon.gameObject.SetActive(icon != null);
        List<NpcQuestDef> active = QuestService.ActiveAll();
        string guide = QuestWindowText.Guide(cur, tab, s, entrance, NpcQuestTrack.IsTracked(cur.Id, active), NpcQuestTrack.Tracked(active).Count);
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

    /// <summary>목록 아이콘: 보상 아이템 아이콘, 없으면 유형별 기존 아이콘 (이름별 캐시)</summary>
    private Sprite QuestIcon(NpcQuestDef d)
    {
        return LoadIcon(QuestWindowText.IconName(d, n => LoadIcon(n) != null));
    }

    private Sprite LoadIcon(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        if (!_icons.TryGetValue(name, out Sprite sp))
        {
            sp = ItemIconLoader.LoadQuiet(name);
            _icons[name] = sp;
        }
        return sp;
    }

    /// <summary>버튼 3개: 입구 모드는 수락/보고 + 추적 + 포기, 일반 모드는 추적 + 포기 (보기만 하는 탭은 숨김)</summary>
    private void ShowActions(NpcQuestDef cur)
    {
        QuestListRules.Tab tab = cur != null ? RowTab(cur) : _tab;
        bool activeTab = tab == QuestListRules.Tab.Active && cur != null;
        bool entrance = _entrance != null;
        bool primaryOn = entrance && cur != null && tab != QuestListRules.Tab.Done;
        _v.Primary.gameObject.SetActive(primaryOn);
        _v.Track.gameObject.SetActive(activeTab);
        _v.Abandon.gameObject.SetActive(activeTab);
        if (primaryOn)
        {
            _v.PrimaryText.text = tab == QuestListRules.Tab.Available ? "수락 (Enter · A)" : "보고 (Enter · A)";
        }
        if (activeTab)
        {
            bool tracked = NpcQuestTrack.IsTracked(cur.Id, QuestService.ActiveAll());
            _v.TrackText.text = (tracked ? "추적 해제" : "추적") + " (Z · X)";
            _v.AbandonText.text = "포기 (Del · Y)";
        }
    }
}
