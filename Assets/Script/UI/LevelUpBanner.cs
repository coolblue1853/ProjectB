using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 알림 (11-02): 화면 위쪽 가운데에 "LEVEL UP  Lv N" 을 크게 한 번 띄운다(0.25초 튀어나옴 → 1.6초 유지 → 0.6초 페이드).
/// 한 번에 여러 레벨이 올라도 LevelSystem.LeveledUp 은 한 번만 오므로 배너도 한 번이다. 큰 창이 열려 있어도 보인다(대화·보상 중 레벨업).
/// </summary>
public sealed class LevelUpBanner : MonoBehaviour
{
    private const float Pop = 0.25f, Hold = 1.6f, Fade = 0.6f;

    private static LevelUpBanner s_instance;

    private CanvasGroup _group;
    private TextMeshProUGUI _title, _sub;
    private Tween _tween;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (s_instance != null)
        {
            return;
        }
        var go = new GameObject("[LevelUpBanner]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<LevelUpBanner>();
        s_instance.Build();
        LevelSystem.LeveledUp += s_instance.Show;
    }

    /// <summary>프로브·테스트용: 지금 배너가 보이는가</summary>
    public static bool Visible => s_instance != null && s_instance._group != null && s_instance._group.alpha > 0.01f;

    private void OnDestroy()
    {
        LevelSystem.LeveledUp -= Show;
        _tween?.Kill();
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    private void Show(int level)
    {
        _title.text = Loc.T("LEVEL UP");
        _sub.text = "Lv " + level;
        _tween?.Kill();
        _group.alpha = 1f;
        var rt = (RectTransform)_group.transform;
        rt.localScale = Vector3.one * 0.6f;
        _tween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(rt.DOScale(1f, Pop).SetEase(Ease.OutBack))
            .AppendInterval(Hold)
            .Append(_group.DOFade(0f, Fade));
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var root = new GameObject("Banner", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(transform, false);
        var rr = (RectTransform)root.transform;
        rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 0.5f);
        rr.anchoredPosition = new Vector2(0f, 250f);
        rr.sizeDelta = new Vector2(900f, 190f);
        _group = root.GetComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        _title = NewText("Title", rr, theme, 96f, new Vector2(0f, 40f), theme.unique);
        _sub = NewText("Sub", rr, theme, 54f, new Vector2(0f, -50f), Color.white);
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        t.color = color;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 230);
        var r = t.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(900f, size * 1.3f);
        return t;
    }
}
