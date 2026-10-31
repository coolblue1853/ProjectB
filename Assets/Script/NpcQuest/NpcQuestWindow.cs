using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 창: 진행 중 NPC 퀘스트 목록 + 목표 x/y + 보상. J(패드 왼쪽 스틱 누름)로 열고 닫는다. 위/아래로 선택, Back 으로 닫기.
/// 코드 생성 UI (BestiaryWindow 와 같은 방식). 열려 있는 동안 UiLock "NpcQuest".
/// </summary>
public class NpcQuestWindow : MonoBehaviour
{
    private const int MaxRows = 9;
    private static NpcQuestWindow s_instance;

    private KeyAction _action;
    private InputAction _up, _down, _back;
    private GameObject _panel;
    private TextMeshProUGUI _list, _detail, _title;
    private int _cursor;
    private bool _open;

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
        bool pressed = (kb != null && kb.jKey.wasPressedThisFrame) || (pad != null && pad.leftStickButton.wasPressedThisFrame);
        if (pressed && (_open || !DatabaseManager.isOpenUI))
        {
            Toggle();
            return;
        }
        if (!_open)
        {
            return;
        }
        if (_back.triggered)
        {
            Close();
            return;
        }
        int n = NpcQuestLog.ActiveQuests().Count;
        int prev = _cursor;
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

    private void Redraw()
    {
        var active = NpcQuestLog.ActiveQuests();
        _cursor = Mathf.Clamp(_cursor, 0, Mathf.Max(0, active.Count - 1));
        var list = new StringBuilder();
        if (active.Count == 0)
        {
            list.Append("<color=#999999>진행 중인 퀘스트가 없습니다.\n마을 NPC 머리 위 ! 를 찾아 보세요.</color>");
        }
        for (int i = 0; i < active.Count && i < MaxRows; i++)
        {
            NpcQuestDef d = active[i];
            bool ready = NpcQuestLog.StatusOf(d) == NpcQuestStatus.Ready;
            list.Append(i == _cursor ? "> " : "  ").Append(ready ? "<color=#FFD633>? " : "<color=#FFFFFF>").Append(d.Title).Append("</color>\n");
        }
        _list.text = list.ToString();
        if (active.Count == 0)
        {
            _detail.text = "";
            return;
        }
        NpcQuestDef cur = active[_cursor];
        bool done = NpcQuestLog.StatusOf(cur) == NpcQuestStatus.Ready;
        var sb = new StringBuilder();
        sb.Append("<color=#FFD633>").Append(cur.Title).Append("</color>\n");
        sb.Append("의뢰인  ").Append(NpcQuestCatalog.NpcName(cur.Giver)).Append("   필요 레벨 ").Append(cur.MinLevel).Append("\n\n");
        sb.Append(cur.Description).Append("\n\n");
        sb.Append(done ? "<color=#FFD633>" : "").Append("목표  ").Append(ObjectiveText(cur)).Append(done ? "  (완료! 의뢰인에게 보고)</color>" : "").Append("\n\n");
        sb.Append("보상  EXP ").Append(cur.Exp).Append("   ").Append(cur.Gold).Append(" G");
        if (!string.IsNullOrEmpty(cur.RewardItem))
        {
            sb.Append("   ").Append(ItemText.DisplayName(cur.RewardItem)).Append(" x").Append(cur.RewardCount);
        }
        _detail.text = sb.ToString();
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
        Place(_title.rectTransform, new Vector2(30f, -18f), new Vector2(600f, 54f));
        _title.text = Loc.IsKorean ? "퀘스트" : "QUESTS";
        _title.color = theme.unique;

        TextMeshProUGUI hint = NewText("Hint", pr, theme, 22f);
        Place(hint.rectTransform, new Vector2(30f, -600f), new Vector2(1100f, 30f));
        hint.text = "위/아래 선택    J / BACK 닫기";
        hint.color = new Color(1f, 1f, 1f, 0.55f);

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
