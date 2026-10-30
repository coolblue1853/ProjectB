using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 채집·채굴 HUD(코드 생성, 낚시 HUD 와 같은 자리·테마). GatherNode.Current 를 읽기만 한다.
/// 표본: 0~100 막대에 좋음(±15)·완벽(±5) 구간과 왕복 커서. 광맥: 균열 막대 + 약점 보석(빛날 때 정타). 공통: 남은 시간 막대.
/// </summary>
public class GatherUI : MonoBehaviour
{
    private const float BarWidth = 360f;

    private static GatherUI _instance;

    private GameObject _panel;
    private TextMeshProUGUI _message;
    private TextMeshProUGUI _sub;
    private GameObject _specimenRoot;
    private RectTransform _goodZone;
    private RectTransform _perfectZone;
    private RectTransform _cursor;
    private GameObject _oreRoot;
    private GameObject _bars;
    private RectTransform _crackFill;
    private Image _weakGem;
    private RectTransform _timeFill;
    private GatherSession _shownSession;
    private bool _shownDone;
    private float _shownTarget = -1f;
    private int _shownAttempts = -1;
    private int _shownCracks = -1;
    private readonly Color _gemDim = new Color(0.25f, 0.4f, 0.42f);
    private readonly Color _gemLit = new Color(0.75f, 1f, 0.95f);

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
        var go = new GameObject("[GatherUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<GatherUI>();
        _instance.Build();
    }

    private void Update()
    {
        GatherNode node = GatherNode.Current;
        GatherSession s = node != null ? node.Session : null;
        if (s == null)
        {
            if (_panel.activeSelf)
            {
                _panel.SetActive(false);
            }
            _shownSession = null;
            return;
        }
        if (!_panel.activeSelf)
        {
            _panel.SetActive(true);
        }
        if (s != _shownSession || s.Done != _shownDone)
        {
            _shownSession = s;
            _shownDone = s.Done;
            _shownTarget = -1f;
            _shownAttempts = -1;
            _shownCracks = -1;
            _message.text = s.Done ? ResultText(s, node.BagFull) : Loc.T(s.Kind == GatherKind.Ore ? "Strike when the weak spot glows" : "Press UP inside the mark");
            _bars.SetActive(!s.Done);
            _specimenRoot.SetActive(!s.Done && s.Kind == GatherKind.Specimen);
            _oreRoot.SetActive(!s.Done && s.Kind == GatherKind.Ore);
            _sub.text = "";
        }
        if (s.Done)
        {
            return;
        }
        _timeFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(1f - s.Elapsed / s.TimeLimit), _timeFill.sizeDelta.y);
        if (s is SpecimenSession sp)
        {
            UpdateSpecimen(sp);
        }
        else if (s is MiningSession m)
        {
            UpdateMining(m);
        }
    }

    private void UpdateSpecimen(SpecimenSession s)
    {
        if (!Mathf.Approximately(s.Target, _shownTarget))
        {
            _shownTarget = s.Target;
            SetSpan(_goodZone, s.Target - GatherRules.GoodHalf, s.Target + GatherRules.GoodHalf);
            SetSpan(_perfectZone, s.Target - GatherRules.PerfectHalf, s.Target + GatherRules.PerfectHalf);
        }
        if (s.AttemptsUsed != _shownAttempts)
        {
            _shownAttempts = s.AttemptsUsed;
            _sub.text = string.Format(Loc.T("Tries left {0}"), GatherRules.SpecimenAttempts - s.AttemptsUsed);
        }
        float a = s.Cursor / GatherRules.CursorMax;
        _cursor.anchorMin = new Vector2(a, 0f);
        _cursor.anchorMax = new Vector2(a, 1f);
    }

    private void UpdateMining(MiningSession s)
    {
        _weakGem.color = s.Glowing ? _gemLit : _gemDim;
        if (s.Cracks != _shownCracks)
        {
            _shownCracks = s.Cracks;
            SetSpan(_crackFill, 0f, GatherRules.CursorMax * s.Cracks / GatherRules.CrackGoal);
            _sub.text = string.Format(Loc.T("Cracks {0}/{1}"), s.Cracks, GatherRules.CrackGoal)
                + (s.LastWasCrit && s.Swings > 0 ? "  " + Loc.T("CRITICAL!") : "");
        }
    }

    private static string ResultText(GatherSession s, bool bagFull)
    {
        switch (s.Result)
        {
            case GatherResult.Success:
                if (bagFull)
                {
                    return Loc.T("Bag full - left it");
                }
                string color = ColorUtility.ToHtmlStringRGB(UiTheme.Get().RarityColor(s.Item.Rarity.ToString()));
                string head = s is SpecimenSession sp && sp.LastJudge == SpecimenJudge.Perfect ? Loc.T("Perfect! ") : "";
                return head + Loc.T("Got: ") + "<color=#" + color + ">" + FishingUI.DisplayName(s.Item.Id) + "</color> x" + s.Count;
            case GatherResult.Failed:
                return Loc.T("The specimen was damaged");
            case GatherResult.TimedOut:
                return Loc.T(s.Kind == GatherKind.Ore ? "The vein held firm" : "The specimen was damaged");
            default:
                return Loc.T("Stopped gathering");
        }
    }

    /// <summary>0~100 구간을 막대 위 가로 구간으로</summary>
    private static void SetSpan(RectTransform rt, float from, float to)
    {
        float a = Mathf.Clamp01(from / GatherRules.CursorMax);
        float b = Mathf.Clamp01(to / GatherRules.CursorMax);
        rt.anchorMin = new Vector2(a, 0f);
        rt.anchorMax = new Vector2(b, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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

        Image panel = theme.NewPanel("GatherPanel", transform, false, 0.9f);
        _panel = panel.gameObject;
        RectTransform pr = panel.rectTransform;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 1f); // 낚시 HUD 와 같은 자리(동시에 열리지 않음)
        pr.anchoredPosition = new Vector2(0f, -120f);
        pr.sizeDelta = new Vector2(420f, 150f);

        _message = NewText(theme, "Message", pr, new Vector2(0f, -14f), 24f);
        _sub = NewText(theme, "Sub", pr, new Vector2(0f, -46f), 18f);

        RectTransform bars = NewRect("Bars", pr); // 결과 문구 때는 막대를 통째로 숨긴다
        _bars = bars.gameObject;
        RectTransform track = Bar("Track", bars, 54f, 18f);
        _specimenRoot = NewRect("Specimen", track).gameObject;
        _goodZone = NewImage("Good", _specimenRoot.transform, new Color(0.45f, 0.75f, 0.9f, 0.45f)).rectTransform;
        _perfectZone = NewImage("Perfect", _specimenRoot.transform, new Color(0.55f, 0.85f, 0.45f, 0.9f)).rectTransform;
        _cursor = NewImage("Cursor", _specimenRoot.transform, theme.unique).rectTransform;
        _cursor.offsetMin = new Vector2(-2f, -4f);
        _cursor.offsetMax = new Vector2(2f, 4f);

        _oreRoot = NewRect("Ore", track).gameObject;
        _crackFill = NewImage("Cracks", _oreRoot.transform, new Color(0.85f, 0.6f, 0.3f)).rectTransform;
        _weakGem = NewImage("WeakGem", _oreRoot.transform, _gemDim);
        RectTransform gem = _weakGem.rectTransform;
        gem.anchorMin = gem.anchorMax = new Vector2(1f, 0.5f); // 막대 오른쪽 바깥
        gem.anchoredPosition = new Vector2(16f, 0f);
        gem.sizeDelta = new Vector2(12f, 12f);
        gem.localRotation = Quaternion.Euler(0f, 0f, 45f);

        RectTransform timeTrack = Bar("TimeTrack", bars, 30f, 6f);
        _timeFill = NewImage("TimeFill", timeTrack, theme.unique).rectTransform;
        _timeFill.anchorMin = new Vector2(0f, 0f);
        _timeFill.anchorMax = new Vector2(0f, 1f);
        _timeFill.pivot = new Vector2(0f, 0.5f);
        _timeFill.anchoredPosition = Vector2.zero;
        _timeFill.sizeDelta = new Vector2(BarWidth, 0f);

        _panel.SetActive(false);
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
