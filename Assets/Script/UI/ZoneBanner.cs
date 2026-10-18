using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 지역 배너 (디아블로식 존 타이틀): 포털로 맵이 바뀌거나 게임을 시작할 때 "FLOOR 1" + 구역 이름을 2.5초 띄운다.
/// 데이터는 맵 루트의 MapInfo(floor, zoneTitle). TO THE BEYOND 층계 구조(TowerWorld.md).
/// </summary>
public class ZoneBanner : MonoBehaviour
{
    static ZoneBanner instance;

    CanvasGroup group;
    TextMeshProUGUI floorText, zoneText;
    string lastZone;

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
            Show(best);
        }
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

    public static void Show(MapInfo info)
    {
        if (instance == null || info == null || string.IsNullOrEmpty(info.zoneTitle))
        {
            return;
        }
        instance.Play(info.floor <= 0 ? "TO THE BEYOND" : "FLOOR " + info.floor, info.zoneTitle);
    }

    void Play(string top, string zone)
    {
        if (zone == lastZone && group.alpha > 0.01f)
        {
            return;
        }
        lastZone = zone;
        floorText.text = top;
        zoneText.text = zone.ToUpperInvariant();
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
        var band = new GameObject("Band", typeof(RectTransform), typeof(Image));
        band.transform.SetParent(root.transform, false);
        var bdr = (RectTransform)band.transform;
        bdr.anchorMin = bdr.anchorMax = new Vector2(0.5f, 0.5f);
        bdr.sizeDelta = new Vector2(760f, 122f);
        bdr.anchoredPosition = new Vector2(0f, -9f);
        var bandImg = band.GetComponent<Image>();
        bandImg.color = new Color(0f, 0f, 0f, 0.4f);
        bandImg.raycastTarget = false;

        // 위아래 얇은 금선
        foreach (float y in new[] { 52f, -70f })
        {
            var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(root.transform, false);
            var lr = (RectTransform)line.transform;
            lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0.5f);
            lr.sizeDelta = new Vector2(520f, 3f);
            lr.anchoredPosition = new Vector2(0f, y);
            var img = line.GetComponent<Image>();
            img.color = new Color(theme.unique.r, theme.unique.g, theme.unique.b, 0.8f);
            img.raycastTarget = false;
        }
        floorText = NewText(root.transform, theme, 36f, new Vector2(0f, 24f), theme.unique);
        zoneText = NewText(root.transform, theme, 60f, new Vector2(0f, -26f), Color.white);
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
