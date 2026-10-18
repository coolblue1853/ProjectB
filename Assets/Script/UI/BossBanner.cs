using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스 처치 승리 연출(QualityRoadmap G2): 화면 중앙 상단에 보스 이름 + "DEFEATED" 배너, 승리 효과음, 화면 흔들림.
/// 고유템을 얻었으면 아래 줄에 금색으로 강조한다. 코드 생성 UI(캔버스 1개 재사용).
/// </summary>
public class BossBanner : MonoBehaviour
{
    static BossBanner instance;

    CanvasGroup group;
    TextMeshProUGUI title, sub;
    TextAnimator titleAnim;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        instance = null;
    }

    /// <summary>페이즈 전환 등 임의 제목·부제 (처치 연출 아님)</summary>
    public static void Show(string title, string subtitle, bool custom)
    {
        if (instance == null)
        {
            var go = new GameObject("[BossBanner]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BossBanner>();
            instance.Build();
        }
        instance.PlayCustom(title, subtitle);
    }

    public static void Show(string bossName, string uniqueItem)
    {
        if (instance == null)
        {
            var go = new GameObject("[BossBanner]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BossBanner>();
            instance.Build();
        }
        instance.Play(bossName, uniqueItem);
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var root = new GameObject("Root", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(transform, false);
        var rr = (RectTransform)root.transform;
        rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.72f);
        rr.sizeDelta = new Vector2(1400f, 220f);
        group = root.GetComponent<CanvasGroup>();
        group.alpha = 0f;

        var band = new GameObject("Band", typeof(RectTransform), typeof(Image));
        band.transform.SetParent(root.transform, false);
        var br = (RectTransform)band.transform;
        br.anchorMin = Vector2.zero;
        br.anchorMax = Vector2.one;
        br.offsetMin = br.offsetMax = Vector2.zero;
        var bandImg = band.GetComponent<Image>();
        bandImg.color = new Color(0f, 0f, 0f, 0.55f);
        bandImg.raycastTarget = false;

        title = NewText("Title", root.transform, theme, 84f, new Vector2(0f, 30f));
        title.color = theme.unique;
        titleAnim = title.gameObject.AddComponent<TextAnimator>();
        sub = NewText("Sub", root.transform, theme, 40f, new Vector2(0f, -60f));
        root.SetActive(false);
    }

    static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size, Vector2 pos)
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
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(40, 10, 0, 255);
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1400f, size * 1.4f);
        rt.anchoredPosition = pos;
        return t;
    }

    void PlayCustom(string title, string subtitle)
    {
        UiTheme theme = UiTheme.Get();
        group.gameObject.SetActive(true);
        group.DOKill();
        group.alpha = 0f;
        titleAnim.SetText("<shake a=0.3>" + title.ToUpperInvariant() + "</shake>", false);
        title_SetColor(theme.warning);
        sub.text = subtitle;
        Transform t = group.transform;
        t.DOKill();
        t.localScale = Vector3.one * 1.2f;
        DOTween.Sequence()
            .Append(group.DOFade(1f, 0.2f))
            .Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
            .AppendInterval(1.6f)
            .Append(group.DOFade(0f, 0.5f))
            .OnComplete(() => group.gameObject.SetActive(false))
            .SetLink(gameObject);
    }

    void title_SetColor(Color c)
    {
        this.title.color = c;
    }

    void Play(string bossName, string uniqueItem)
    {
        group.gameObject.SetActive(true);
        group.DOKill();
        group.alpha = 0f;
        title_SetColor(UiTheme.Get().unique);
        titleAnim.SetText("<wave a=0.5>" + bossName.ToUpperInvariant() + " DEFEATED</wave>", false);
        bool unique = !string.IsNullOrEmpty(uniqueItem);
        sub.text = unique ? "<color=#FFC640>Unique: " + uniqueItem + "</color>" : "Rewards dropped";
        Transform t = group.transform;
        t.DOKill();
        t.localScale = Vector3.one * 1.25f;
        DOTween.Sequence()
            .Append(group.DOFade(1f, 0.25f))
            .Join(t.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .AppendInterval(2.4f)
            .Append(group.DOFade(0f, 0.6f))
            .OnComplete(() => group.gameObject.SetActive(false))
            .SetLink(gameObject);
        Sfx.Play(SfxId.Victory);
        if (unique)
        {
            Sfx.Play(SfxId.RareDrop);
        }
        HitFeedback.Shake(HitFeedback.ShakeTier.Heavy);
        GameLog.Info(GameLog.Cat.AI, "보스 처치 배너: " + bossName + (unique ? " / 고유 " + uniqueItem : ""));
    }
}
