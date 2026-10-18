using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 연속 처치 콤보 (메이플 콤보 킬): 3초 안에 다음 처치가 이어지면 콤보가 쌓인다. 우상단에 "x N COMBO" + 게이지,
/// 5·10·20 콤보마다 드롭률 보너스(+5%씩, 최대 +20%)를 DatabaseManager.playerDropRate 에 임시로 얹는다(끊기면 회수).
/// 처치는 EnemyHealth 가 보내는 MessageSystem "Kill" 로 센다.
/// </summary>
public class KillCombo : MonoBehaviour, PixelCrushers.IMessageHandler
{
    const float Window = 3f;
    static readonly int[] Milestones = { 5, 10, 20 };
    const int BonusPerMilestone = 5, MaxBonus = 20;

    static KillCombo instance;
    int combo, appliedBonus;
    float expireAt;
    CanvasGroup group;
    TextMeshProUGUI text;
    TextAnimator anim;
    Image gauge;

    public static int Current => instance != null ? instance.combo : 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[KillCombo]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<KillCombo>();
        instance.Build();
    }

    void OnEnable()
    {
        PixelCrushers.MessageSystem.AddListener(this, "Kill", string.Empty);
    }

    void OnDisable()
    {
        PixelCrushers.MessageSystem.RemoveListener(this);
        ClearBonus();
    }

    public void OnMessage(PixelCrushers.MessageArgs args)
    {
        combo++;
        expireAt = Time.time + Window;
        int bonus = 0;
        foreach (int m in Milestones)
        {
            if (combo >= m)
            {
                bonus += BonusPerMilestone;
            }
        }
        bonus = Mathf.Min(bonus, MaxBonus);
        if (bonus != appliedBonus)
        {
            DatabaseManager.playerDropRate += bonus - appliedBonus;
            appliedBonus = bonus;
            GameLog.Info(GameLog.Cat.Combat, "콤보 " + combo + " → 드롭률 보너스 +" + bonus + "%");
        }
        Show();
    }

    void Update()
    {
        if (combo == 0)
        {
            return;
        }
        float left = expireAt - Time.time;
        gauge.fillAmount = Mathf.Clamp01(left / Window);
        if (left <= 0f)
        {
            End();
        }
    }

    void Show()
    {
        group.gameObject.SetActive(true);
        group.DOKill();
        group.alpha = 1f;
        bool milestone = System.Array.IndexOf(Milestones, combo) >= 0;
        string color = combo >= 20 ? "#FF6A4A" : combo >= 10 ? "#FFC640" : combo >= 5 ? "#9ED0FF" : "#FFFFFF";
        anim.SetText("<color=" + color + ">" + (milestone ? "<bounce>" : "") + "x" + combo + (milestone ? "</bounce>" : "") + "</color> <size=60%>COMBO</size>", false);
        text.transform.DOKill(true);
        text.transform.localScale = Vector3.one * (milestone ? 1.5f : 1.2f);
        text.transform.DOScale(1f, 0.18f).SetEase(Ease.OutBack).SetLink(text.gameObject);
        if (milestone)
        {
            Sfx.Play(SfxId.Crit, 0.5f);
        }
    }

    void End()
    {
        combo = 0;
        ClearBonus();
        group.DOFade(0f, 0.4f).SetLink(gameObject).OnComplete(() => group.gameObject.SetActive(false));
    }

    void ClearBonus()
    {
        if (appliedBonus != 0)
        {
            DatabaseManager.playerDropRate -= appliedBonus;
            appliedBonus = 0;
        }
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 52;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var root = new GameObject("Root", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(transform, false);
        var rr = (RectTransform)root.transform;
        rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(1f, 1f);
        rr.anchoredPosition = new Vector2(-40f, -150f);
        rr.sizeDelta = new Vector2(420f, 90f);
        group = root.GetComponent<CanvasGroup>();

        text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(root.transform, false);
        if (theme.pixelFont != null)
        {
            text.font = theme.pixelFont;
        }
        text.fontSize = 56f;
        text.alignment = TextAlignmentOptions.MidlineRight;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.outlineWidth = 0.25f;
        text.outlineColor = new Color32(0, 0, 0, 230);
        var tr = text.rectTransform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(0f, 14f);
        tr.offsetMax = Vector2.zero;
        anim = text.gameObject.AddComponent<TextAnimator>();

        Image track = theme.NewPanel("Track", root.transform, false, 0.9f);
        var kr = track.rectTransform;
        kr.anchorMin = new Vector2(0f, 0f);
        kr.anchorMax = new Vector2(1f, 0f);
        kr.pivot = new Vector2(1f, 0f);
        kr.anchoredPosition = Vector2.zero;
        kr.sizeDelta = new Vector2(0f, 14f);
        gauge = new GameObject("Gauge", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        gauge.transform.SetParent(track.transform, false);
        gauge.rectTransform.anchorMin = Vector2.zero;
        gauge.rectTransform.anchorMax = Vector2.one;
        gauge.rectTransform.offsetMin = new Vector2(3f, 3f);
        gauge.rectTransform.offsetMax = new Vector2(-3f, -3f);
        gauge.color = theme.unique;
        gauge.type = Image.Type.Filled;
        gauge.fillMethod = Image.FillMethod.Horizontal;
        gauge.fillOrigin = (int)Image.OriginHorizontal.Right;
        gauge.raycastTarget = false;
        root.SetActive(false);
    }
}
