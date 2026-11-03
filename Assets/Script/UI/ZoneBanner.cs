using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 지역 배너 (디아블로식 존 타이틀): 포털로 맵이 바뀌거나 게임을 시작할 때 "FLOOR 1" + 구역 이름을 2.5초 띄운다.
/// 같은 구역은 5분 안에 다시 띄우지 않는다(마을 왕복마다 뜨지 않게, 11-01).
/// 데이터는 맵 루트의 MapInfo(floor, zoneTitle). TO THE BEYOND 층계 구조(TowerWorld.md).
/// </summary>
public class ZoneBanner : MonoBehaviour
{
    static ZoneBanner instance;

    CanvasGroup group;
    TextMeshProUGUI floorText, zoneText, guardText;
    RectTransform bandRect;
    RectTransform bottomLine;
    string lastZone;
    private Coroutine _pendingShow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[ZoneBanner]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ZoneBanner>();
        instance.Build();
        Portal.MapChanged -= instance.OnMapChanged;
        Portal.MapChanged += instance.OnMapChanged;
    }

    void Start()
    {
        // 시작 맵: 활성 MapInfo 중 플레이어와 가장 가까운 것
        MapInfo best = null;
        float bestD = float.MaxValue;
        Vector3 p = PlayerController.instance != null ? PlayerController.instance.transform.position : Vector3.zero;
        foreach (MapInfo m in FindObjectsByType<MapInfo>(FindObjectsSortMode.None))
        {
            float d = Mathf.Abs(m.transform.position.y - p.y) + Mathf.Abs(m.transform.position.x - p.x) * 0.01f;
            if (d < bestD)
            {
                bestD = d;
                best = m;
            }
        }
        if (best != null)
        {
            StartCoroutine(ShowAfterTitle(best));
        }
    }

    System.Collections.IEnumerator ShowAfterTitle(MapInfo info)
    {
        while (UiLock.IsHeld("Title"))
        {
            yield return null; // 타이틀이 걷힌 뒤에 첫 지역 배너
        }
        yield return new WaitForSecondsRealtime(0.4f);
        Show(info);
    }

    void OnDestroy()
    {
        Portal.MapChanged -= OnMapChanged;
    }

    void OnMapChanged(GameObject openMap)
    {
        if (openMap != null && openMap.TryGetComponent(out MapInfo info))
        {
            Show(info);
        }
    }

    private System.Collections.IEnumerator ShowWhenVisible(MapInfo info)
    {
        // 같은 프레임 뒤에 시작하는 FadeInFromBlack도 감지하도록 한 프레임 기다린다.
        yield return null;
        while (ScreenFader.Busy)
        {
            yield return null;
        }
        _pendingShow = null;
        if (info != null && info.gameObject.activeInHierarchy)
        {
            string floor = info.floor <= 0 ? "TO THE BEYOND" : Loc.IsKorean ? info.floor + Loc.T("FLOOR") : "FLOOR " + info.floor;
            ZoneProgress.Zone zone = ZoneProgress.Find(info.gameObject.name);
            Play(floor, Loc.T(info.zoneTitle), ZoneProgress.GuardianLine(zone, ZoneProgress.IsDefeated(zone), Loc.IsKorean));
        }
    }

    public static void Show(MapInfo info)
    {
        if (info != null)
        {
            ZoneProgress.SetCurrent(info.gameObject.name);
        }
        if (instance == null || info == null || string.IsNullOrEmpty(info.zoneTitle))
        {
            return;
        }
        if (instance._pendingShow != null)
        {
            instance.StopCoroutine(instance._pendingShow);
        }
        instance._pendingShow = instance.StartCoroutine(instance.ShowWhenVisible(info));
    }

    /// <summary>같은 구역 배너를 다시 띄우지 않는 시간(11-01 사용자: 마을을 나갈 때마다 배너가 떠서 번거롭다)</summary>
    public const float RepeatCooldown = 300f;
    readonly System.Collections.Generic.Dictionary<string, float> _shownAt = new System.Collections.Generic.Dictionary<string, float>();

    /// <summary>이 구역 배너를 지금 띄울까: 처음이거나 마지막으로 띄운 지 RepeatCooldown 초가 지났을 때</summary>
    public static bool ShouldShow(float lastShown, bool everShown, float now)
    {
        return !everShown || now - lastShown >= RepeatCooldown;
    }

    void Play(string top, string zone, string guardian = "")
    {
        if (zone == lastZone && group.alpha > 0.01f)
        {
            return;
        }
        bool ever = _shownAt.TryGetValue(zone, out float last);
        if (!ShouldShow(last, ever, Time.unscaledTime))
        {
            return;
        }
        _shownAt[zone] = Time.unscaledTime;
        lastZone = zone;
        floorText.text = top;
        zoneText.text = zone.ToUpperInvariant();
        // 수호자가 있는 구역만 띠를 한 줄 늘려 수호자 이름+목표를 보인다(§14-7: 개수 정보는 도감 구역 탭)
        bool hasGuard = !string.IsNullOrEmpty(guardian);
        guardText.gameObject.SetActive(hasGuard);
        guardText.text = guardian;
        bandRect.sizeDelta = new Vector2(760f, hasGuard ? 166f : 122f);
        bandRect.anchoredPosition = new Vector2(0f, hasGuard ? -31f : -9f);
        bottomLine.anchoredPosition = new Vector2(0f, hasGuard ? -114f : -70f);
        group.gameObject.SetActive(true);
        group.DOKill();
        group.alpha = 0f;
        RectTransform zr = zoneText.rectTransform;
        zr.DOKill();
        zr.anchoredPosition = new Vector2(0f, -36f);
        DOTween.Sequence()
            .Append(group.DOFade(1f, 0.5f))
            .Join(zr.DOAnchorPosY(-26f, 0.5f).SetEase(Ease.OutCubic))
            .AppendInterval(2.2f)
            .Append(group.DOFade(0f, 0.8f))
            .OnComplete(() => group.gameObject.SetActive(false))
            .SetLink(gameObject);
        GameLog.Info(GameLog.Cat.Map, "지역 배너: " + top + " / " + zone);
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 55;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var root = new GameObject("Root", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(transform, false);
        var rr = (RectTransform)root.transform;
        rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.8f);
        rr.sizeDelta = new Vector2(1200f, 140f);
        group = root.GetComponent<CanvasGroup>();
        group.alpha = 0f;

        // 반투명 어두운 띠 (밝은 하늘 배경에서도 읽히게)
        var band = theme.NewPanel("Band", root.transform, false, 0.9f).gameObject;
        var bdr = (RectTransform)band.transform;
        bdr.anchorMin = bdr.anchorMax = new Vector2(0.5f, 0.5f);
        bandRect = bdr;
        bdr.sizeDelta = new Vector2(760f, 122f);
        bdr.anchoredPosition = new Vector2(0f, -9f);
        // 위아래 얇은 금선
        foreach (float y in new[] { 52f, -70f })
        {
            var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(root.transform, false);
            var lr = (RectTransform)line.transform;
            lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0.5f);
            lr.sizeDelta = new Vector2(520f, 3f);
            lr.anchoredPosition = new Vector2(0f, y);
            if (y < 0f)
            {
                bottomLine = lr;
            }
            var img = line.GetComponent<Image>();
            img.color = new Color(theme.unique.r, theme.unique.g, theme.unique.b, 0.8f);
            img.raycastTarget = false;
        }
        floorText = NewText(root.transform, theme, 36f, new Vector2(0f, 24f), theme.unique);
        zoneText = NewText(root.transform, theme, 60f, new Vector2(0f, -26f), Color.white);
        guardText = NewText(root.transform, theme, 28f, new Vector2(0f, -90f), new Color(1f, 0.78f, 0.45f));
        guardText.gameObject.SetActive(false);
        root.SetActive(false);
    }

    static TextMeshProUGUI NewText(Transform parent, UiTheme theme, float size, Vector2 pos, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 220);
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1200f, size * 1.4f);
        rt.anchoredPosition = pos;
        return t;
    }
}
