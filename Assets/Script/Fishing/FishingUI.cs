using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 낚시 HUD(코드 생성, 픽셀 UI 테마): 안내 문구, 찌(입질 때 흔들림), 장력 바(안전 구간 표시), 포획 진행도, 결과.
/// FishingSpot.Current 를 읽기만 한다. 문구는 단계가 바뀔 때만 다시 만들고, 바는 매 프레임 크기만 바꾼다.
/// </summary>
public class FishingUI : MonoBehaviour
{
    private const float BarWidth = 360f;

    private static FishingUI _instance;

    private GameObject _panel;
    private TextMeshProUGUI _message;
    private RectTransform _bobber;
    private Image _bobberImage;
    private GameObject _tensionRoot;
    private RectTransform _safeZone;
    private RectTransform _tensionFill;
    private Image _tensionFillImage;
    private RectTransform _progressFill;
    private FishingPhase _shownPhase;
    private FishingSession _shownSession;
    private readonly Color _okColor = new Color(0.55f, 0.85f, 0.45f);
    private readonly Color _dangerColor = new Color(1f, 0.35f, 0.3f);

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
        var go = new GameObject("[FishingUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<FishingUI>();
        _instance.Build();
    }

    private void Update()
    {
        FishingSpot spot = FishingSpot.Current;
        FishingSession s = spot != null ? spot.Session : null;
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
        if (s != _shownSession || s.Phase != _shownPhase)
        {
            _shownSession = s;
            _shownPhase = s.Phase;
            _message.text = MessageFor(s, spot.BagFull);
            _tensionRoot.SetActive(s.Phase == FishingPhase.Reeling);
            if (s.Phase == FishingPhase.Reeling)
            {
                SetSpan(_safeZone, s.Tension.SafeMin, s.Tension.SafeMax);
            }
        }
        UpdateBobber(s);
        if (s.Phase == FishingPhase.Reeling)
        {
            FishingTension t = s.Tension;
            SetSpan(_tensionFill, 0f, t.Tension);
            _tensionFillImage.color = t.InSafeZone ? _okColor : _dangerColor;
            _progressFill.sizeDelta = new Vector2(BarWidth * t.Progress / FishingRules.ProgressGoal, _progressFill.sizeDelta.y);
        }
    }

    private void UpdateBobber(FishingSession s)
    {
        float x = 0f;
        float y = 0f;
        if (s.Phase == FishingPhase.Bite)
        {
            x = Mathf.Sin(Time.time * 60f) * 6f;
            y = -4f;
        }
        else if (s.Phase == FishingPhase.Waiting)
        {
            y = Mathf.Sin(Time.time * 2f) * 2f;
        }
        else if (s.Phase == FishingPhase.Reeling && s.Tension.Resisting)
        {
            x = Mathf.Sin(Time.time * 40f) * 3f;
        }
        _bobber.anchoredPosition = new Vector2(x, y);
        _bobberImage.enabled = s.Phase != FishingPhase.Casting && s.Phase != FishingPhase.Result;
    }

    private static string MessageFor(FishingSession s, bool bagFull)
    {
        switch (s.Phase)
        {
            case FishingPhase.Casting:
                return Loc.T("Hold UP, release to cast");
            case FishingPhase.Waiting:
                return Loc.T(s.Cast == CastQuality.Optimal ? "Perfect cast! Waiting for a bite..." : "Waiting for a bite...");
            case FishingPhase.Bite:
                return Loc.T("BITE!  Press UP");
            case FishingPhase.Reeling:
                return Loc.T("Hold UP to reel  ·  release to ease");
            default:
                return ResultText(s, bagFull);
        }
    }

    private static string ResultText(FishingSession s, bool bagFull)
    {
        switch (s.Result)
        {
            case FishingResult.Caught:
                if (bagFull)
                {
                    return Loc.T("Bag full - released it");
                }
                string color = ColorUtility.ToHtmlStringRGB(UiTheme.Get().RarityColor(s.Fish.Rarity.ToString()));
                return Loc.T("Caught: ") + "<color=#" + color + ">" + ItemText.DisplayName(s.Fish.Id) + "</color> x" + s.Count;
            case FishingResult.Snapped:
                return Loc.T("The line snapped");
            case FishingResult.Escaped:
                return Loc.T("It got away");
            case FishingResult.MissedBite:
                return Loc.T("Missed the bite");
            case FishingResult.ReeledEmpty:
                return Loc.T("Reeled in too early");
            default:
                return Loc.T("Stopped fishing");
        }
    }

    /// <summary>장력 0~100 구간을 바 위 가로 구간으로</summary>
    private static void SetSpan(RectTransform rt, float from, float to)
    {
        float a = from / FishingRules.TensionMax;
        float b = to / FishingRules.TensionMax;
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

        Image panel = theme.NewPanel("FishingPanel", transform, false, 0.9f);
        _panel = panel.gameObject;
        RectTransform pr = panel.rectTransform;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 1f); // 화면 위 가운데(플레이어·퀵슬롯을 가리지 않게)
        pr.anchoredPosition = new Vector2(0f, -120f);
        pr.sizeDelta = new Vector2(420f, 150f);

        _message = NewText(theme, "Message", pr, new Vector2(0f, -14f), 24f);

        Image bobber = NewImage("Bobber", pr, new Color(0.95f, 0.3f, 0.25f));
        _bobberImage = bobber;
        RectTransform bobberHolder = NewRect("BobberHolder", pr);
        bobberHolder.anchorMin = bobberHolder.anchorMax = new Vector2(0.5f, 1f);
        bobberHolder.anchoredPosition = new Vector2(0f, -66f);
        bobber.rectTransform.SetParent(bobberHolder, false);
        _bobber = bobber.rectTransform;
        _bobber.sizeDelta = new Vector2(10f, 14f);

        _tensionRoot = NewRect("Tension", pr).gameObject;
        RectTransform track = NewImage("Track", _tensionRoot.transform, new Color(0.08f, 0.08f, 0.1f, 0.9f)).rectTransform;
        track.anchorMin = track.anchorMax = new Vector2(0.5f, 0f);
        track.anchoredPosition = new Vector2(0f, 40f);
        track.sizeDelta = new Vector2(BarWidth, 18f);
        _safeZone = NewImage("SafeZone", track, new Color(0.45f, 0.75f, 0.9f, 0.45f)).rectTransform;
        _tensionFillImage = NewImage("TensionFill", track, _okColor);
        _tensionFill = _tensionFillImage.rectTransform;
        _tensionFill.localScale = new Vector3(1f, 0.5f, 1f);

        RectTransform progressTrack = NewImage("ProgressTrack", _tensionRoot.transform, new Color(0.08f, 0.08f, 0.1f, 0.9f)).rectTransform;
        progressTrack.anchorMin = progressTrack.anchorMax = new Vector2(0.5f, 0f);
        progressTrack.anchoredPosition = new Vector2(0f, 18f);
        progressTrack.sizeDelta = new Vector2(BarWidth, 8f);
        _progressFill = NewImage("ProgressFill", progressTrack, theme.unique).rectTransform;
        _progressFill.anchorMin = new Vector2(0f, 0f);
        _progressFill.anchorMax = new Vector2(0f, 1f);
        _progressFill.pivot = new Vector2(0f, 0.5f);
        _progressFill.anchoredPosition = Vector2.zero;
        _progressFill.sizeDelta = new Vector2(0f, 0f);

        _panel.SetActive(false);
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
