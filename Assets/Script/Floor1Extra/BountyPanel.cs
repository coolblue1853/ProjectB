using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 사냥 의뢰 게시대 화면(코드 생성, 카드 3장). ←/→ 고르기, ↑ 또는 Enter 로 수락·보고, ESC/Back 으로 닫기.
/// 아래에 1층계 졸업 점검(레벨 15·Stoneborn 세트·탑 문지기)을 같이 보여 준다.
/// </summary>
public class BountyPanel : MonoBehaviour
{
    private const int CardCount = BountyRules.OfferCount;
    private const float CardW = 330f;
    private const float CardH = 300f;

    private static BountyPanel s_instance;

    private KeyAction _action;
    private InputAction _left, _right, _confirm, _up, _back;
    private GameObject _panel;
    private readonly Image[] _cards = new Image[CardCount];
    private readonly TextMeshProUGUI[] _texts = new TextMeshProUGUI[CardCount];
    private TextMeshProUGUI _grad;
    private bool _open;
    private int _cursor;
    private float _openedAt;

    public static bool IsOpen => s_instance != null && s_instance._open;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    public static void Open()
    {
        if (s_instance != null)
        {
            s_instance.OpenPanel();
        }
    }

    private void Awake()
    {
        s_instance = this;
        _action = new KeyAction();
        _left = _action.UI.LeftInventory;
        _right = _action.UI.RightInventory;
        _confirm = _action.UI.Enter;
        _up = _action.UI.Up;
        _back = _action.UI.Back;
    }

    private void OnDestroy()
    {
        _action?.Dispose();
    }

    private void Update()
    {
        if (!_open)
        {
            return;
        }
        if (UiLock.TryBack("Bounty", _back.triggered))
        {
            Close();
            return;
        }
        int prev = _cursor;
        if (_left.triggered)
        {
            _cursor = (_cursor + CardCount - 1) % CardCount;
        }
        else if (_right.triggered)
        {
            _cursor = (_cursor + 1) % CardCount;
        }
        if (_cursor != prev)
        {
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
        // 열 때 누른 ↑ 가 곧바로 수락으로 이어지지 않게 0.3초 건너뛴다
        if (Time.unscaledTime > _openedAt + 0.3f && (_confirm.triggered || _up.triggered))
        {
            Activate();
        }
    }

    private void Activate()
    {
        BountyBoard b = BountyBoard.Instance;
        if (b == null || _cursor >= b.State.Offered.Count)
        {
            return;
        }
        string id = b.State.Offered[_cursor];
        BountyDef d = BountyRules.Find(b.Defs, id);
        if (BountyRules.IsReady(b.State, d))
        {
            b.TryReport(id);
        }
        else if (b.Accept(id))
        {
            PickupLog.Notice("사냥 의뢰 수락 - " + d.Title + ": " + d.Targets.Length + "종 " + d.Count + "마리");
            Sfx.Play(SfxId.UiOpen, 0.6f);
        }
        Redraw();
    }

    private void OpenPanel()
    {
        BountyBoard b = BountyBoard.Instance;
        if (b == null || _open || DatabaseManager.isOpenUI)
        {
            return;
        }
        if (_panel == null)
        {
            Build();
        }
        b.EnsureOffers();
        _open = true;
        _cursor = 0;
        _openedAt = Time.unscaledTime;
        UiLock.Set("Bounty", true);
        _action.UI.Enable();
        _panel.SetActive(true);
        Redraw();
        Sfx.Play(SfxId.UiOpen);
    }

    private void Close()
    {
        _open = false;
        UiLock.Set("Bounty", false);
        _action.UI.Disable();
        _panel.SetActive(false);
    }

    private void Redraw()
    {
        BountyBoard b = BountyBoard.Instance;
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < CardCount; i++)
        {
            bool has = i < b.State.Offered.Count;
            _cards[i].gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }
            BountyDef d = BountyRules.Find(b.Defs, b.State.Offered[i]);
            string status = BountyRules.StatusOf(b.State, d);
            bool done = status == "완료";
            string stateColor = status == "보고 가능" ? "#8fe38f" : (done ? "#888888" : "#ffc640");
            _texts[i].text = "<size=120%>" + d.Title + "</size>\n<color=#aaaaaa>[" + d.Zone + "]  Lv " + d.MinLevel + "+</color>\n\n"
                + "처치 " + d.Count + "마리\n\n보상\n" + d.Reward.Describe() + "\n\n<color=" + stateColor + ">" + status + "</color>";
            _cards[i].color = i == _cursor ? new Color(1f, 0.85f, 0.4f, 0.35f) : new Color(1f, 1f, 1f, 0.08f);
        }
        _grad.text = GraduationCheck();
        _grad.color = theme.common;
    }

    /// <summary>1층계 졸업 점검: 레벨 15, Stoneborn 세트 3부위 이상, 탑 문지기 처치</summary>
    public static string GraduationCheck()
    {
        int level = LevelSystem.Level;
        int gate = BestiaryRecord.Kills("Gatekeeper_BD");
        string lv = level >= 15 ? "<color=#8fe38f>Lv " + level + " OK</color>" : "<color=#ffc640>Lv " + level + "/15</color>";
        string boss = gate > 0 ? "<color=#8fe38f>탑 문지기 처치</color>" : "<color=#ffc640>탑 문지기 미처치</color>";
        return "1층계 졸업 점검:  " + lv + "   ·   " + boss + "   ·   Stoneborn 세트는 I 창에서 확인";
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
        pr.sizeDelta = new Vector2(1100f, 560f);

        TextMeshProUGUI head = NewText("Title", pr, theme, 40f);
        Place(head.rectTransform, new Vector2(30f, -16f), new Vector2(800f, 54f));
        head.text = "오늘의 사냥 의뢰";
        head.color = theme.unique;

        for (int i = 0; i < CardCount; i++)
        {
            Image bg = theme.NewPanel("Card" + i, pr, true);
            Place(bg.rectTransform, new Vector2(30f + i * (CardW + 20f), -84f), new Vector2(CardW, CardH));
            TextMeshProUGUI t = NewText("Text", bg.transform, theme, 24f);
            Place(t.rectTransform, new Vector2(16f, -12f), new Vector2(CardW - 32f, CardH - 24f));
            t.alignment = TextAlignmentOptions.TopLeft;
            t.enableWordWrapping = true;
            _cards[i] = bg;
            _texts[i] = t;
        }

        _grad = NewText("Graduation", pr, theme, 22f);
        Place(_grad.rectTransform, new Vector2(30f, -400f), new Vector2(1040f, 60f));
        _grad.enableWordWrapping = true;
        TextMeshProUGUI hint = NewText("Hint", pr, theme, 22f);
        Place(hint.rectTransform, new Vector2(30f, -500f), new Vector2(1040f, 30f));
        hint.text = "←/→ 고르기    ↑ 수락·보고    ESC 닫기    (의뢰는 하루가 지나면 새로 걸린다)";
        hint.color = new Color(1f, 1f, 1f, 0.55f);
        _panel.SetActive(false);
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
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
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        return t;
    }
}
