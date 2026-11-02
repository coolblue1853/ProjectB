using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 던파식 전투 HUD 두 가지(11-02, CombatSpec 9절): 작은 히트 수 카운터("N HIT" + 누적 피해 + 유지 게이지)와 스태미나 부족 안내 문구.
/// 처치 콤보(KillCombo, 우상단)와 별개다. 큰 창이 열려 있으면(HudPolicy.Hidden) 둘 다 숨긴다.
/// 플레이어가 맞으면 히트 수는 0으로 돌아간다. 집계는 EnemyHealth.damage2Enemy 가 OnPlayerHit 로 넘긴다.
/// </summary>
public class CombatHud : MonoBehaviour
{
    const float NoticeTime = 1.1f, NoticeThrottle = 0.6f;
    static readonly string[] SlotKeys = { "A", "S", "D", "F" };

    static CombatHud instance;
    readonly HitComboCounter _hits = new HitComboCounter();
    CanvasGroup _hitGroup, _noticeGroup;
    TextMeshProUGUI _hitText, _damageText, _noticeText;
    Image _gauge;
    bool _wasHurt;
    float _noticeUntil = -10f, _lastNoticeAt = -10f;

    /// <summary>프로브·테스트용 현재 히트 수</summary>
    public static int Hits => instance != null ? instance._hits.Count : 0;
    public static int BestHits => instance != null ? instance._hits.Best : 0;
    /// <summary>프로브·테스트용: 지금 안내 문구가 떠 있는가</summary>
    public static bool NoticeVisible => instance != null && Time.time < instance._noticeUntil;
    /// <summary>프로브·테스트용: 히트 수 카운터가 실제로 보이는가</summary>
    public static bool HitVisible => instance != null && instance._hitGroup != null && instance._hitGroup.alpha > 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[CombatHud]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<CombatHud>();
        instance.Build();
    }

    /// <summary>플레이어가 적을 맞혔다(피해 합계 = 본타 + 추가타)</summary>
    public static void OnPlayerHit(int damage)
    {
        if (instance != null)
        {
            instance.RegisterHit(damage);
        }
    }

    /// <summary>슬롯 스킬이 스태미나 부족으로 안 나갔다 — 슬롯 표시 + 안내 문구(0.6초에 한 번)</summary>
    public static void NotifyStaminaShort(int slot)
    {
        if (SkillCooldown.instance != null)
        {
            SkillCooldown.instance.FlashShort(slot);
        }
        if (instance != null)
        {
            instance.ShowNotice(slot);
        }
    }

    void RegisterHit(int damage)
    {
        _hits.Hit(Time.time, damage);
        if (_hitText == null)
        {
            return;
        }
        _hitText.text = _hits.Count + " <size=60%>" + Loc.T("HIT") + "</size>";
        _hitText.color = TierColor(_hits.Count);
        _damageText.text = _hits.TotalDamage.ToString("N0");
        _hitText.transform.DOKill(true);
        _hitText.transform.localScale = Vector3.one * 1.12f;
        _hitText.transform.DOScale(1f, 0.1f).SetLink(_hitText.gameObject);
    }

    static Color TierColor(int hits)
    {
        return hits >= 30 ? new Color(1f, 0.42f, 0.29f) : hits >= 10 ? new Color(1f, 0.78f, 0.25f) : new Color(0.93f, 0.95f, 1f);
    }

    void ShowNotice(int slot)
    {
        if (_noticeText == null || Time.time - _lastNoticeAt < NoticeThrottle)
        {
            return;
        }
        _lastNoticeAt = Time.time;
        _noticeUntil = Time.time + NoticeTime;
        string key = slot >= 0 && slot < SlotKeys.Length ? SlotKeys[slot] : "?";
        _noticeText.text = "[" + key + "] " + Loc.T("Not enough stamina");
        _noticeGroup.DOKill();
        _noticeGroup.alpha = 1f;
    }

    void Update()
    {
        bool hidden = HudPolicy.Hidden;
        PlayerController pc = PlayerController.instance;
        bool hurt = pc != null && pc.isAttacked;
        if (hurt && !_wasHurt)
        {
            _hits.Reset(); // 맞으면 히트 수 초기화
        }
        _wasHurt = hurt;

        bool hitShown = !hidden && _hits.Active(Time.time);
        float target = hitShown ? 1f : 0f;
        _hitGroup.alpha = Mathf.MoveTowards(_hitGroup.alpha, target, Time.unscaledDeltaTime * (hitShown ? 12f : 3f));
        if (hitShown)
        {
            _gauge.fillAmount = _hits.Remaining01(Time.time);
        }
        float noticeTarget = !hidden && Time.time < _noticeUntil ? 1f : 0f;
        _noticeGroup.alpha = Mathf.MoveTowards(_noticeGroup.alpha, noticeTarget, Time.unscaledDeltaTime * (noticeTarget > 0f ? 12f : 3f));
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 51;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // 히트 수: 화면 왼쪽 가운데 위쪽, 작게
        var hit = new GameObject("Hits", typeof(RectTransform), typeof(CanvasGroup));
        hit.transform.SetParent(transform, false);
        var hr = (RectTransform)hit.transform;
        hr.anchorMin = hr.anchorMax = hr.pivot = new Vector2(0f, 0.5f);
        hr.anchoredPosition = new Vector2(40f, -70f);
        hr.sizeDelta = new Vector2(260f, 84f);
        _hitGroup = hit.GetComponent<CanvasGroup>();
        _hitGroup.alpha = 0f;
        _hitGroup.blocksRaycasts = false;
        _hitGroup.interactable = false;
        _hitText = NewText("Count", hit.transform, theme, 40f, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.38f), new Vector2(1f, 1f));
        _damageText = NewText("Damage", hit.transform, theme, 22f, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.12f), new Vector2(1f, 0.42f));
        _damageText.color = new Color(0.78f, 0.8f, 0.88f);
        Image track = theme.NewPanel("Track", hit.transform, false, 0.85f);
        var tr = track.rectTransform;
        tr.anchorMin = new Vector2(0f, 0f);
        tr.anchorMax = new Vector2(1f, 0f);
        tr.pivot = new Vector2(0f, 0f);
        tr.anchoredPosition = Vector2.zero;
        tr.sizeDelta = new Vector2(0f, 8f);
        track.raycastTarget = false;
        _gauge = new GameObject("Gauge", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        _gauge.transform.SetParent(track.transform, false);
        _gauge.rectTransform.anchorMin = Vector2.zero;
        _gauge.rectTransform.anchorMax = Vector2.one;
        _gauge.rectTransform.offsetMin = new Vector2(2f, 2f);
        _gauge.rectTransform.offsetMax = new Vector2(-2f, -2f);
        _gauge.color = theme.unique;
        _gauge.type = Image.Type.Filled;
        _gauge.fillMethod = Image.FillMethod.Horizontal;
        _gauge.fillOrigin = (int)Image.OriginHorizontal.Left;
        _gauge.raycastTarget = false;

        // 안내 문구: 아래 중앙, 스킬 칸 위
        var notice = new GameObject("Notice", typeof(RectTransform), typeof(CanvasGroup));
        notice.transform.SetParent(transform, false);
        var nr = (RectTransform)notice.transform;
        nr.anchorMin = nr.anchorMax = nr.pivot = new Vector2(0.5f, 0f);
        nr.anchoredPosition = new Vector2(0f, 120f);
        nr.sizeDelta = new Vector2(520f, 44f);
        _noticeGroup = notice.GetComponent<CanvasGroup>();
        _noticeGroup.alpha = 0f;
        _noticeGroup.blocksRaycasts = false;
        _noticeGroup.interactable = false;
        _noticeText = NewText("Text", notice.transform, theme, 30f, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        _noticeText.color = new Color(1f, 0.45f, 0.4f);
    }

    static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size, TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = align;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 230);
        var r = t.rectTransform;
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return t;
    }
}
