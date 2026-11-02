using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 창: 진행 중 / 완료 탭. 진행 중은 완료 가능을 맨 위에, 목표 x/y·목표 구역·보상을 보여 주고 Z 로 추적을 지정한다(최대 3개, 우측 추적 HUD 와 연결).
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
    private string _notice = "";

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
        Gamepad pad = Gamepad.current;
        bool pressed = (kb != null && kb.jKey.wasPressedThisFrame) || (pad != null && pad.rightStickButton.wasPressedThisFrame);
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
            _tab = _tab == QuestListRules.Tab.Active ? QuestListRules.Tab.Done : QuestListRules.Tab.Active;
            _cursor = 0;
            _notice = "";
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
            return;
        }
        if (_select.triggered && _tab == QuestListRules.Tab.Active && n > 0)
        {
            ToggleTracking();
            return;
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
            default: what = NpcQuestCatalog.NpcName(d.Target) + "와(과) 대화"; break;
        }
        return what + " " + have + "/" + d.Count;
    }

    private void ToggleTracking()
    {
        var active = NpcQuestLog.ActiveQuests();
        NpcQuestDef cur = _shown[Mathf.Clamp(_cursor, 0, _shown.Count - 1)];
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
        bool activeTab = _tab == QuestListRules.Tab.Active;
        _shown = activeTab
            ? QuestListRules.Active(active, d => NpcQuestLog.StatusOf(d) == NpcQuestStatus.Ready)
            : QuestListRules.Done(NpcQuestCatalog.All, NpcQuestLog.IsDone);
        _count = _shown.Count;
        _cursor = Mathf.Clamp(_cursor, 0, Mathf.Max(0, _count - 1));
        int doneCount = CountDone();
        _title.text = (Loc.IsKorean ? "퀘스트" : "QUESTS") + "   "
            + (activeTab ? "<color=#FFFFFF>[진행 중 " + active.Count + "]</color>  <color=#777777>완료 " + doneCount + "</color>"
                         : "<color=#777777>진행 중 " + active.Count + "</color>  <color=#FFFFFF>[완료 " + doneCount + "]</color>");

        var list = new StringBuilder();
        if (_count == 0)
        {
            list.Append(activeTab
                ? "<color=#999999>진행 중인 퀘스트가 없습니다.\n마을 NPC 머리 위 ! 를 찾아 보세요.</color>"
                : "<color=#999999>완료한 퀘스트가 없습니다.</color>");
        }
        // 줄 = 구분 머리글 + 퀘스트. 커서가 보이도록 아래로 스크롤
        var lines = new List<string>();
        int cursorLine = 0;
        bool sawReady = false, sawActive = false;
        for (int i = 0; i < _count; i++)
        {
            NpcQuestDef d = _shown[i];
            bool ready = activeTab && NpcQuestLog.StatusOf(d) == NpcQuestStatus.Ready;
            if (ready && !sawReady)
            {
                sawReady = true;
                lines.Add("<color=#FFD633>- 완료 가능 -</color>");
            }
            else if (activeTab && !ready && !sawActive)
            {
                sawActive = true;
                lines.Add("<color=#88AACC>- 진행 중 -</color>");
            }
            if (i == _cursor)
            {
                cursorLine = lines.Count;
            }
            string star = activeTab && NpcQuestTrack.IsTracked(d.Id, active) ? "<color=#7FE0FF>*</color>" : " ";
            string color = !activeTab ? "#AAAAAA" : ready ? "#FFD633" : "#FFFFFF";
            lines.Add((i == _cursor ? "> " : "  ") + star + "<color=" + color + ">" + (ready ? "? " : "") + d.Title + "</color>");
        }
        int first = Mathf.Max(0, cursorLine - MaxRows + 1);
        for (int i = first; i < lines.Count && i < first + MaxRows; i++)
        {
            list.Append(lines[i]).Append('\n');
        }
        _list.text = list.ToString();
        _hint.text = HintText(activeTab);
        if (_count == 0)
        {
            _detail.text = "";
            return;
        }
        NpcQuestDef cur = _shown[_cursor];
        bool done = activeTab && NpcQuestLog.StatusOf(cur) == NpcQuestStatus.Ready;
        var sb = new StringBuilder();
        sb.Append("<color=#FFD633>").Append(cur.Title).Append("</color>\n");
        sb.Append("의뢰인  ").Append(NpcQuestCatalog.NpcName(cur.Giver)).Append("   필요 레벨 ").Append(cur.MinLevel).Append('\n');
        string zone = NpcQuestZone.Of(cur);
        if (zone.Length > 0)
        {
            sb.Append("목표 구역  <color=#9ED0FF>").Append(zone).Append("</color>\n");
        }
        sb.Append('\n').Append(cur.Description).Append("\n\n");
        if (!activeTab)
        {
            sb.Append("<color=#88DD88>완료</color>   목표  ").Append(ObjectiveText(cur));
        }
        else
        {
            sb.Append(done ? "<color=#FFD633>" : "").Append("목표  ").Append(ObjectiveText(cur)).Append(done ? "  (완료! 의뢰인에게 보고)</color>" : "");
        }
        sb.Append("\n\n보상  EXP ").Append(cur.Exp).Append("   ").Append(cur.Gold).Append(" G");
        if (!string.IsNullOrEmpty(cur.RewardItem))
        {
            sb.Append("   ").Append(ItemText.DisplayName(cur.RewardItem)).Append(" x").Append(cur.RewardCount);
        }
        if (activeTab)
        {
            bool tracked = NpcQuestTrack.IsTracked(cur.Id, active);
            sb.Append("\n\n").Append(tracked ? "<color=#7FE0FF>* 추적 중</color>" : "<color=#777777>추적 안 함</color>")
              .Append("  (").Append(NpcQuestTrack.Tracked(active).Count).Append('/').Append(NpcQuestTrack.Max).Append(')');
            if (_notice.Length > 0)
            {
                sb.Append("   ").Append(_notice);
            }
        }
        _detail.text = sb.ToString();
    }

    private static string HintText(bool activeTab)
    {
        return activeTab ? "위/아래 선택    좌/우 탭    Z 추적 지정(최대 3)    J / BACK 닫기" : "위/아래 선택    좌/우 탭    J / BACK 닫기";
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
