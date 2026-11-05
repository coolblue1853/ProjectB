using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 창: 받을 수 있는 / 진행 중 / 완료 탭(구역별 묶음, 게시대 현상 의뢰 포함). 진행 중은 완료 가능을 맨 위에, 목표 x/y·목표 구역·보상을 보여 주고 Z 로 추적을 지정한다(최대 3개, 우측 추적 HUD 와 연결).
/// J(패드 오른쪽 스틱 누름)로 열고 닫는다. 위/아래 선택, 좌/우 탭, Back 으로 닫기.
/// 코드 생성 UI (BestiaryWindow 와 같은 방식). 열려 있는 동안 UiLock "NpcQuest".
/// </summary>
public class NpcQuestWindow : MonoBehaviour
{
    private const int MaxRows = 9;
    private static NpcQuestWindow s_instance;

    private KeyAction _action;
    private InputAction _up, _down, _left, _right, _select, _back;
    private GameObject _panel;
    private TextMeshProUGUI _list, _detail, _title, _hint;
    private int _cursor;
    private bool _open;
    private int _count;
    private QuestListRules.Tab _tab;
    private List<NpcQuestDef> _shown = new List<NpcQuestDef>();
    private readonly List<Row> _rows = new List<Row>(); // 커서 단위: 구역 머리글(접기 대상) + 펼쳐진 구역의 퀘스트
    private string _notice = "";

    private sealed class Row
    {
        public NpcQuestDef Quest; // null = 머리글
        public string Key;        // 머리글 접기 키
        public string Text;       // 머리글 표시
        public bool Folded;
        public int Count;         // 머리글 구역의 퀘스트 수
    }

    private NpcQuestDef CurrentQuest()
    {
        return _rows.Count == 0 ? null : _rows[Mathf.Clamp(_cursor, 0, _rows.Count - 1)].Quest;
    }

    public static bool IsOpen => s_instance != null && s_instance._open;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    public static void Toggle()
    {
        if (s_instance == null)
        {
            return;
        }
        if (s_instance._open)
        {
            s_instance.Close();
        }
        else
        {
            s_instance.Open();
        }
    }

    private void Awake()
    {
        s_instance = this;
        _action = new KeyAction();
        _up = _action.UI.UPInventory;
        _down = _action.UI.DownInventory;
        _left = _action.UI.LeftInventory;
        _right = _action.UI.RightInventory;
        _select = _action.UI.Select;
        _back = _action.UI.Back;
    }

    private void OnDestroy()
    {
        _action?.Dispose();
    }

    private void OnEnable()
    {
        NpcQuestLog.Changed += OnChanged;
    }

    private void OnDisable()
    {
        NpcQuestLog.Changed -= OnChanged;
        Close();
    }

    private void OnChanged()
    {
        if (_open)
        {
            Redraw();
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        PadFieldInput.Refresh();
        // 패드는 SELECT 길게(0.8초). R3 는 아티팩트 사용과 겹쳐서 뺐다(AUTO-49). 눌린 순간 UI 가 열려 있었으면(인벤 프리셋 저장 등) 무시
        bool padHold = PadFieldInput.Select == PadGesture.Hold && (_open || !PadFieldInput.SelectInUi);
        bool pressed = (kb != null && kb.jKey.wasPressedThisFrame) || padHold;
        if (pressed && (_open || !DatabaseManager.isOpenUI))
        {
            Toggle();
            return;
        }
        if (!_open)
        {
            return;
        }
        if (UiLock.TryBack("NpcQuest", _back.triggered))
        {
            Close();
            return;
        }
        int n = _count;
        int prev = _cursor;
        if (_left.triggered || _right.triggered)
        {
            _tab = QuestListRules.Step(_tab, _left.triggered ? -1 : 1);
            _cursor = 0;
            _notice = "";
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
            return;
        }
        if (_select.triggered && n > 0)
        {
            Row row = _rows[Mathf.Clamp(_cursor, 0, n - 1)];
            if (row.Quest == null)
            {
                QuestFoldRules.Toggle(row.Key);
                _notice = "";
                Sfx.Play(SfxId.UiMove, 0.6f);
                Redraw();
                return;
            }
            if (_tab == QuestListRules.Tab.Active && !QuestBountyLink.IsBounty(row.Quest))
            {
                ToggleTracking();
                return;
            }
        }
        if (_up.triggered && n > 0)
        {
            _cursor = (_cursor - 1 + n) % n;
        }
        else if (_down.triggered && n > 0)
        {
            _cursor = (_cursor + 1) % n;
        }
        if (_cursor != prev)
        {
            _notice = "";
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
    }

    private void Open()
    {
        if (_panel == null)
        {
            Build();
        }
        _open = true;
        _cursor = 0;
        _tab = QuestListRules.Tab.Active;
        _notice = "";
        UiLock.Set("NpcQuest", true);
        _action.UI.Enable();
        _panel.SetActive(true);
        Redraw();
        Sfx.Play(SfxId.UiOpen);
    }

    private void Close()
    {
        _open = false;
        UiLock.Set("NpcQuest", false);
        _action.UI.Disable();
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    /// <summary>목표 문장: "슬라임 8마리 처치 3/8" 같은 한 줄</summary>
    public static string ObjectiveText(NpcQuestDef d)
    {
        int have = NpcQuestLog.Progress(d);
        string what;
        switch (d.Objective)
        {
            case NpcQuestObjective.Kill: what = MonsterTable.DisplayName(d.Target) + " 처치"; break;
            case NpcQuestObjective.Collect: what = d.IsRecordsTarget ? "아버지의 기록 수집" : ItemText.DisplayName(d.Target) + " 수집"; break;
            case NpcQuestObjective.Craft: what = ItemText.DisplayName(d.Target) + " 제작"; break;
            case NpcQuestObjective.Visit: what = NpcQuestCatalog.VisitName(d.Target) + " 방문"; break;
            case NpcQuestObjective.KillElite: what = (d.Target == NpcQuestObjectiveRules.AnyElite ? "정예" : MonsterTable.DisplayName(d.Target)) + " 처치"; break;
            case NpcQuestObjective.Equip: what = EquipText(d.Target) + " 착용"; break;
            case NpcQuestObjective.Enhance: what = (d.Target == NpcQuestObjectiveRules.WeaponTarget ? "무기" : d.Target == NpcQuestObjectiveRules.ArmorTarget ? "방어구" : "장비") + " 강화 +" + d.Count; break;
            case NpcQuestObjective.Bestiary: what = "도감 몬스터 종류 처치"; break;
            default: what = NpcQuestCatalog.NpcName(d.Target) + "와(과) 대화"; break;
        }
        return what + " " + have + "/" + d.Count;
    }

    private static string EquipText(string target)
    {
        if (target == NpcQuestObjectiveRules.ArmorTarget)
        {
            return "방어구 부위";
        }
        return NpcQuestObjectiveRules.TryParseSets(target, out int min) ? "세트 " + min + "부위 이상 세트 종류" : target + " 세트 부위";
    }

    private void ToggleTracking()
    {
        var active = NpcQuestLog.ActiveQuests();
        NpcQuestDef cur = CurrentQuest();
        if (cur == null)
        {
            return;
        }
        if (NpcQuestTrack.Toggle(cur.Id, active))
        {
            _notice = NpcQuestTrack.IsTracked(cur.Id, active) ? "<color=#FFD633>추적을 켰습니다</color>" : "<color=#BBBBBB>추적을 껐습니다</color>";
            Sfx.Play(SfxId.UiMove, 0.6f);
        }
        else
        {
            _notice = "<color=#FF7766>추적은 최대 " + NpcQuestTrack.Max + "개 — 다른 퀘스트의 추적을 먼저 끄세요</color>";
        }
        NpcQuestLog.NotifyTrackingChanged(); // 표시 갱신(Redraw 포함) + 저장
    }

    private void Redraw()
    {
        var active = NpcQuestLog.ActiveQuests();
        var bountyActive = QuestBountyLink.Active();
        int activeCount = active.Count + bountyActive.Count;
        _shown = BuildList(active, bountyActive);
        BuildRows();
        _count = _rows.Count;
        _cursor = Mathf.Clamp(_cursor, 0, Mathf.Max(0, _count - 1));
        int availableCount = QuestListRules.Available(NpcQuestCatalog.All, NpcQuestLog.StatusOf).Count + QuestBountyLink.Available().Count;
        _title.text = (Loc.IsKorean ? "퀘스트" : "QUESTS") + "   "
            + TabLabel(QuestListRules.Tab.Available, "받을 수 있는 " + availableCount)
            + "  " + TabLabel(QuestListRules.Tab.Active, "진행 중 " + activeCount)
            + "  " + TabLabel(QuestListRules.Tab.Done, "완료 " + CountDone());

        var list = new StringBuilder();
        if (_count == 0)
        {
            list.Append("<color=#999999>" + EmptyText() + "</color>");
        }
        // 줄 = 구분 머리글 + 퀘스트. 커서가 보이도록 아래로 스크롤
        var lines = new List<string>();
        int cursorLine = 0;
        for (int i = 0; i < _count; i++)
        {
            Row row = _rows[i];
            if (row.Quest == null)
            {
                if (i == _cursor)
                {
                    cursorLine = lines.Count;
                }
                lines.Add((i == _cursor ? "> " : "  ") + (row.Folded ? "[+] " : "[-] ") + row.Text + (row.Folded ? " <color=#999999>(" + row.Count + ")</color>" : ""));
                continue;
            }
            NpcQuestDef d = row.Quest;
            bool ready = _tab == QuestListRules.Tab.Active && IsReady(d);
            bool low = _tab == QuestListRules.Tab.Available && IsLevelLow(d);
            if (i == _cursor)
            {
                cursorLine = lines.Count;
            }
            string star = _tab == QuestListRules.Tab.Active && !QuestBountyLink.IsBounty(d) && NpcQuestTrack.IsTracked(d.Id, active) ? "<color=#7FE0FF>*</color>" : " ";
            string color = _tab == QuestListRules.Tab.Done ? "#AAAAAA" : ready ? "#FFD633" : low ? "#888888" : "#FFFFFF";
            string mark = ready ? "? " : _tab == QuestListRules.Tab.Available ? "! " : "";
            lines.Add((i == _cursor ? "> " : "  ") + star + "<color=" + color + ">" + mark + d.Title + (low ? "  (Lv " + d.MinLevel + ")" : "") + "</color>");
        }
        int first = Mathf.Max(0, cursorLine - MaxRows + 1);
        for (int i = first; i < lines.Count && i < first + MaxRows; i++)
        {
            list.Append(lines[i]).Append('\n');
        }
        _list.text = list.ToString();
        _hint.text = HintText(_tab == QuestListRules.Tab.Active);
        if (_count == 0)
        {
            _detail.text = "";
            return;
        }
        NpcQuestDef cur = CurrentQuest();
        _detail.text = cur != null ? DetailText(cur, active) : "<color=#999999>구역 머리글 — Z/Enter 로 접기·펼치기</color>";
    }

    /// <summary>_shown(구역별로 이미 묶임)을 머리글+퀘스트 행으로 펴고, 접힌 구역은 퀘스트를 감춘다</summary>
    private void BuildRows()
    {
        _rows.Clear();
        Row head = null;
        string lastHeader = null;
        foreach (NpcQuestDef d in _shown)
        {
            bool ready = _tab == QuestListRules.Tab.Active && IsReady(d);
            bool low = _tab == QuestListRules.Tab.Available && IsLevelLow(d);
            string header = HeaderOf(d, ready, low);
            if (header != lastHeader)
            {
                lastHeader = header;
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

    private List<NpcQuestDef> BuildList(List<NpcQuestDef> active, List<NpcQuestDef> bountyActive)
    {
        switch (_tab)
        {
            case QuestListRules.Tab.Available:
            {
                // 수락 가능 → 레벨 부족 순, 각각 구역별로 묶는다
                var open = new List<NpcQuestDef>();
                var low = new List<NpcQuestDef>();
                foreach (NpcQuestDef d in QuestListRules.Available(NpcQuestCatalog.All, NpcQuestLog.StatusOf))
                {
                    (IsLevelLow(d) ? low : open).Add(d);
                }
                open.AddRange(QuestBountyLink.Available());
                List<NpcQuestDef> res = QuestListRules.GroupByZone(open, ZoneOf);
                res.AddRange(QuestListRules.GroupByZone(low, ZoneOf));
                return res;
            }
            case QuestListRules.Tab.Active:
            {
                // 완료 가능이 맨 위, 나머지는 구역별
                var all = new List<NpcQuestDef>(active);
                all.AddRange(bountyActive);
                var ready = new List<NpcQuestDef>();
                var rest = new List<NpcQuestDef>();
                foreach (NpcQuestDef d in all)
                {
                    (IsReady(d) ? ready : rest).Add(d);
                }
                ready.AddRange(QuestListRules.GroupByZone(rest, ZoneOf));
                return ready;
            }
            default:
                return QuestListRules.GroupByZone(QuestListRules.Done(NpcQuestCatalog.All, NpcQuestLog.IsDone), ZoneOf);
        }
    }

    private string TabLabel(QuestListRules.Tab tab, string text)
    {
        return _tab == tab ? "<color=#FFFFFF>[" + text + "]</color>" : "<color=#777777>" + text + "</color>";
    }

    private string EmptyText()
    {
        switch (_tab)
        {
            case QuestListRules.Tab.Available: return "지금 받을 수 있는 퀘스트가 없습니다.\n레벨을 올리거나 선행 퀘스트를 끝내 보세요.";
            case QuestListRules.Tab.Active: return "진행 중인 퀘스트가 없습니다.\n마을 NPC 머리 위 ! 를 찾아 보세요.";
            default: return "완료한 퀘스트가 없습니다.";
        }
    }

    private static bool IsReady(NpcQuestDef d)
    {
        return QuestBountyLink.IsBounty(d) ? QuestBountyLink.IsReady(d) : NpcQuestLog.StatusOf(d) == NpcQuestStatus.Ready;
    }

    private static bool IsLevelLow(NpcQuestDef d)
    {
        return !QuestBountyLink.IsBounty(d) && NpcQuestLog.StatusOf(d) == NpcQuestStatus.LevelLow;
    }

    private static string ZoneOf(NpcQuestDef d)
    {
        return QuestBountyLink.IsBounty(d) ? QuestBountyLink.Zone(d) : NpcQuestZone.Of(d);
    }

    private static string HeaderOf(NpcQuestDef d, bool ready, bool low)
    {
        if (ready)
        {
            return "<color=#FFD633>- 완료 가능 -</color>";
        }
        string zone = ZoneOf(d);
        string label = zone.Length > 0 ? zone : "기타";
        if (low)
        {
            return "<color=#888888>- 레벨 부족 · " + label + " -</color>";
        }
        return "<color=#88AACC>- " + label + " -</color>";
    }

    private string DetailText(NpcQuestDef cur, List<NpcQuestDef> active)
    {
        bool bounty = QuestBountyLink.IsBounty(cur);
        bool activeTab = _tab == QuestListRules.Tab.Active;
        bool done = activeTab && IsReady(cur);
        var sb = new StringBuilder();
        sb.Append("<color=#FFD633>").Append(cur.Title).Append("</color>\n");
        sb.Append("의뢰인  ").Append(NpcQuestCatalog.NpcName(cur.Giver)).Append("   필요 레벨 ").Append(cur.MinLevel).Append('\n');
        string zone = ZoneOf(cur);
        if (zone.Length > 0)
        {
            sb.Append("목표 구역  <color=#9ED0FF>").Append(zone).Append("</color>\n");
        }
        sb.Append('\n').Append(cur.Description).Append("\n\n");
        string goal = bounty ? "처치 " + QuestBountyLink.Progress(cur) + "/" + cur.Count : ObjectiveText(cur);
        if (_tab == QuestListRules.Tab.Done)
        {
            sb.Append("<color=#88DD88>완료</color>   목표  ").Append(goal);
        }
        else
        {
            sb.Append(done ? "<color=#FFD633>" : "").Append("목표  ").Append(goal).Append(done ? "  (완료! 의뢰인에게 보고)</color>" : "");
        }
        sb.Append("\n\n보상  EXP ").Append(cur.Exp).Append("   ").Append(cur.Gold).Append(" G");
        if (!string.IsNullOrEmpty(cur.RewardItem))
        {
            sb.Append("   ").Append(ItemText.DisplayName(cur.RewardItem)).Append(" x").Append(cur.RewardCount);
        }
        if (_tab == QuestListRules.Tab.Available)
        {
            sb.Append("\n\n").Append(IsLevelLow(cur) ? "<color=#FF9966>레벨 " + cur.MinLevel + " 부터 받을 수 있습니다</color>"
                : "<color=#FFD633>" + NpcQuestCatalog.NpcName(cur.Giver) + "에게 말을 걸어 수락하세요</color>");
        }
        else if (activeTab && !bounty)
        {
            bool tracked = NpcQuestTrack.IsTracked(cur.Id, active);
            sb.Append("\n\n").Append(tracked ? "<color=#7FE0FF>* 추적 중</color>" : "<color=#777777>추적 안 함</color>")
              .Append("  (").Append(NpcQuestTrack.Tracked(active).Count).Append('/').Append(NpcQuestTrack.Max).Append(')');
            if (_notice.Length > 0)
            {
                sb.Append("   ").Append(_notice);
            }
        }
        return sb.ToString();
    }

    private static string HintText(bool activeTab)
    {
        return activeTab ? "위/아래 선택    좌/우 탭    Z 추적 지정(최대 3) / 구역 접기    J / BACK 닫기" : "위/아래 선택    좌/우 탭(받을 수 있는·진행 중·완료)    Z 구역 접기    J / BACK 닫기";
    }

    private static int CountDone()
    {
        int n = 0;
        foreach (NpcQuestDef d in NpcQuestCatalog.All)
        {
            if (NpcQuestLog.IsDone(d.Id))
            {
                n++;
            }
        }
        return n;
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 70;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _panel = theme.NewPanel("Panel", transform).gameObject;
        var pr = (RectTransform)_panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1180f, 640f);

        _title = NewText("Title", pr, theme, 42f);
        Place(_title.rectTransform, new Vector2(30f, -18f), new Vector2(1100f, 54f));
        _title.color = theme.unique;

        _hint = NewText("Hint", pr, theme, 22f);
        Place(_hint.rectTransform, new Vector2(30f, -600f), new Vector2(1100f, 30f));
        _hint.color = new Color(1f, 1f, 1f, 0.55f);

        Image left = theme.NewPanel("ListBg", pr, true);
        Place(left.rectTransform, new Vector2(30f, -86f), new Vector2(380f, 500f));
        _list = NewText("List", left.transform, theme, 26f);
        Place(_list.rectTransform, new Vector2(12f, -10f), new Vector2(356f, 480f));
        _list.enableWordWrapping = false;
        _list.overflowMode = TextOverflowModes.Truncate;

        _detail = NewText("Detail", pr, theme, 26f);
        Place(_detail.rectTransform, new Vector2(436f, -90f), new Vector2(720f, 490f));
        _detail.enableWordWrapping = true;
        _panel.SetActive(false);
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.raycastTarget = false;
        t.richText = true;
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(0, 0, 0, 200);
        return t;
    }

    private static void Place(RectTransform rt, Vector2 topLeft, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = topLeft;
        rt.sizeDelta = size;
    }
}
