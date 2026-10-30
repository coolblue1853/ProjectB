using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 흔적 추적 HUD(코드 생성, 낚시·채집·발굴 HUD 와 같은 자리·테마). TrackingSpot.Current 를 읽기만 한다.
/// 흔적 단계: 목표·흔적 종류, 단계·실수 수, 왼쪽·위·오른쪽 세 칸(흔적 종류 + 신선도, 지운 칸은 X), 남은 시간 막대.
/// 덫 단계: 왕복 커서 막대(좋음 하늘색·완벽 초록·금색 커서). 결과 때는 문구와 덫 품질·처치 보너스만.
/// </summary>
public class TrackingUI : MonoBehaviour
{
    private const float PanelWidth = 500f;
    private const float BarWidth = 380f;
    private const float BoxWidth = 150f;
    private const float BoxHeight = 96f;
    private const float BoxGap = 10f;
    private const float PlayHeight = 250f;
    private const float ResultHeight = 120f;
    private static readonly string[] DirKeys = { "Left", "Up", "Right" };

    private static TrackingUI _instance;

    private readonly Color _box = new Color(0.2f, 0.19f, 0.17f);
    private readonly Color _boxCrossed = new Color(0.1f, 0.09f, 0.09f);
    private readonly Color _good = new Color(0.4f, 0.7f, 0.85f);
    private readonly Color _perfect = new Color(0.45f, 0.85f, 0.45f);
    private readonly Color[] _freshColors =
    {
        new Color(0.55f, 0.9f, 0.55f), new Color(0.95f, 0.82f, 0.4f), new Color(0.55f, 0.55f, 0.58f),
    };

    private GameObject _panel;
    private RectTransform _panelRect;
    private TextMeshProUGUI _message;
    private TextMeshProUGUI _sub;
    private GameObject _play;
    private GameObject _boxes;
    private GameObject _trap;
    private Image[] _boxImages;
    private TextMeshProUGUI[] _boxTexts;
    private RectTransform _trapCursor;
    private RectTransform _timeFill;
    private TextMeshProUGUI _help;
    private TrackingSession _shown;
    private TrackPhase _shownPhase;
    private int _shownStage = -1;
    private int _shownMistakes = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[TrackingUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<TrackingUI>();
        _instance.Build();
    }

    private void Update()
    {
        TrackingSpot spot = TrackingSpot.Current;
        TrackingSession s = spot != null ? spot.Session : null;
        if (s == null)
        {
            if (_panel.activeSelf)
            {
                _panel.SetActive(false);
            }
            _shown = null;
            return;
        }
        if (!_panel.activeSelf)
        {
            _panel.SetActive(true);
        }
        if (s != _shown || s.Phase != _shownPhase)
        {
            _shown = s;
            _shownPhase = s.Phase;
            _shownStage = -1;
            _shownMistakes = -1;
            ShowPhase(spot, s);
        }
        switch (s.Phase)
        {
            case TrackPhase.Trail:
                SetFill(_timeFill, 1f - s.Elapsed / TrackingRules.TimeLimit);
                if (s.Stage != _shownStage || s.Mistakes != _shownMistakes)
                {
                    _shownStage = s.Stage;
                    _shownMistakes = s.Mistakes;
                    RefreshTrail(s);
                }
                break;
            case TrackPhase.Trap:
                SetFill(_timeFill, 1f - s.TrapElapsed / TrackingRules.TrapTimeout);
                _trapCursor.anchoredPosition = new Vector2(BarWidth * (s.TrapCursor / 100f - 0.5f), 0f);
                break;
        }
    }

    private void ShowPhase(TrackingSpot spot, TrackingSession s)
    {
        bool done = s.Done;
        _play.SetActive(!done);
        _panelRect.sizeDelta = new Vector2(PanelWidth, done ? ResultHeight : PlayHeight);
        if (done)
        {
            _message.text = ResultHead(spot, s);
            _sub.text = ResultSub(spot, s);
            return;
        }
        bool trap = s.Phase == TrackPhase.Trap;
        _boxes.SetActive(!trap);
        _trap.SetActive(trap);
        _message.text = trap
            ? string.Format(Loc.T("Set the trap for {0}!"), spot.TargetName)
            : string.Format(Loc.T("Track {0}: {1}"), spot.TargetName, Loc.T(s.Target));
        _help.text = Loc.T(trap ? "X set trap  ·  ESC stop" : "LEFT UP RIGHT pick the trail  ·  ESC stop");
        if (trap)
        {
            _sub.text = Loc.T("Stop in the middle");
        }
    }

    private void RefreshTrail(TrackingSession s)
    {
        _sub.text = string.Format(Loc.T("Trail {0}/{1}  ·  Mistakes {2}/{3}"), s.Stage + 1, TrackingRules.Stages,
            s.Mistakes, TrackingRules.MistakeLimit - 1);
        for (int i = 0; i < TrackingRules.Options; i++)
        {
            TraceOption o = s.OptionAt(i);
            bool crossed = s.IsCrossed(i);
            _boxImages[i].color = crossed ? _boxCrossed : _box;
            string dir = Loc.T(DirKeys[i]);
            if (crossed)
            {
                _boxTexts[i].text = dir + "\n\n<color=#B04040>X</color>";
                continue;
            }
            _boxTexts[i].text = dir + "\n" + Loc.T(o.Trace) + "\n<color=#"
                + ColorUtility.ToHtmlStringRGB(_freshColors[(int)o.Fresh]) + ">" + Loc.T(o.Fresh.ToString()) + "</color>";
        }
    }

    private static string ResultHead(TrackingSpot spot, TrackingSession s)
    {
        switch (s.Result)
        {
            case TrackResult.Success:
                return spot.Summoned ? string.Format(Loc.T("Elite {0} appeared!"), spot.TargetName) : Loc.T("The trail went cold");
            case TrackResult.TooManyMistakes:
                return Loc.T("Lost the trail");
            case TrackResult.TimedOut:
                return Loc.T("Time ran out");
            default:
                return Loc.T("Stopped tracking");
        }
    }

    /// <summary>성공이면 덫 품질과 처치 보너스, 아니면 빈 줄</summary>
    private static string ResultSub(TrackingSpot spot, TrackingSession s)
    {
        if (s.Result != TrackResult.Success || !spot.Summoned)
        {
            return "";
        }
        var sb = new StringBuilder();
        sb.Append(Loc.T("Trap: ")).Append(Loc.T("Trap" + s.Quality));
        if (spot.BonusMaterial != null)
        {
            sb.Append("  ·  ").Append(Loc.T("Kill bonus: ")).Append(ItemText.DisplayName(spot.BonusMaterial)).Append(" x").Append(spot.BonusMaterialCount);
        }
        if (spot.BonusWeapon)
        {
            sb.Append(" + ").Append(Loc.T("lineage weapon"));
        }
        return sb.ToString();
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 55;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image panel = theme.NewPanel("TrackingPanel", transform, false, 0.9f);
        _panel = panel.gameObject;
        _panelRect = panel.rectTransform;
        _panelRect.anchorMin = _panelRect.anchorMax = _panelRect.pivot = new Vector2(0.5f, 1f); // 다른 미니게임 HUD 와 같은 자리(동시에 열리지 않음)
        _panelRect.anchoredPosition = new Vector2(0f, -100f);
        _panelRect.sizeDelta = new Vector2(PanelWidth, PlayHeight);

        _message = NewText(theme, "Message", _panelRect, new Vector2(0f, -14f), 22f);
        _sub = NewText(theme, "Sub", _panelRect, new Vector2(0f, -46f), 17f);

        RectTransform play = NewRect("Play", _panelRect); // 결과 때는 통째로 숨긴다
        _play = play.gameObject;
        BuildBoxes(theme, play);
        BuildTrap(theme, play);
        _timeFill = Fill(Bar("TimeTrack", play, 50f, 6f), theme.unique);
        _help = NewText(theme, "Help", play, Vector2.zero, 14f);
        RectTransform hr = _help.rectTransform;
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0f);
        hr.pivot = new Vector2(0.5f, 0.5f);
        hr.anchoredPosition = new Vector2(0f, 26f);
        hr.sizeDelta = new Vector2(PanelWidth - 24f, 20f);

        _panel.SetActive(false);
    }

    private void BuildBoxes(UiTheme theme, RectTransform play)
    {
        RectTransform boxes = NewRect("Boxes", play);
        _boxes = boxes.gameObject;
        _boxImages = new Image[TrackingRules.Options];
        _boxTexts = new TextMeshProUGUI[TrackingRules.Options];
        for (int i = 0; i < TrackingRules.Options; i++)
        {
            Image img = NewImage("Box" + i, boxes, _box);
            RectTransform rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(BoxWidth, BoxHeight);
            rt.anchoredPosition = new Vector2((i - 1) * (BoxWidth + BoxGap), -80f);
            TextMeshProUGUI t = NewText(theme, "Text", rt, Vector2.zero, 15f);
            RectTransform tr = t.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(6f, 4f);
            tr.offsetMax = new Vector2(-6f, -4f);
            t.textWrappingMode = TextWrappingModes.Normal;
            t.alignment = TextAlignmentOptions.Center;
            _boxImages[i] = img;
            _boxTexts[i] = t;
        }
    }

    private void BuildTrap(UiTheme theme, RectTransform play)
    {
        RectTransform track = Bar("TrapTrack", play, 120f, 26f);
        _trap = track.gameObject;
        Zone(track, TrackingRules.TrapGood, _good);
        Zone(track, TrackingRules.TrapPerfect, _perfect);
        _trapCursor = NewImage("Cursor", track, theme.unique).rectTransform;
        _trapCursor.sizeDelta = new Vector2(6f, 34f);
        _trap.SetActive(false);
    }

    /// <summary>막대 가운데 ±half(0~100 눈금) 구간</summary>
    private static void Zone(RectTransform track, float half, Color color)
    {
        RectTransform rt = NewImage("Zone", track, color).rectTransform;
        rt.sizeDelta = new Vector2(BarWidth * half * 2f / 100f, 26f);
    }

    private static void SetFill(RectTransform fill, float ratio)
    {
        fill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(ratio), fill.sizeDelta.y);
    }

    private static RectTransform Fill(RectTransform track, Color color)
    {
        RectTransform rt = NewImage("Fill", track, color).rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(BarWidth, 0f);
        return rt;
    }

    private static RectTransform Bar(string name, RectTransform parent, float y, float height)
    {
        RectTransform rt = NewImage(name, parent, new Color(0.08f, 0.08f, 0.1f, 0.9f)).rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(BarWidth, height);
        return rt;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        var img = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.transform.SetParent(parent, false);
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI NewText(UiTheme theme, string name, RectTransform parent, Vector2 pos, float size)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(-24f, 34f);
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 220);
        t.raycastTarget = false;
        return t;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
