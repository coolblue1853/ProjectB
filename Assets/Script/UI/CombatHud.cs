using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 던파식 전투 HUD(11-02, CombatSpec 9절): 작은 히트 수 카운터("N HIT" + 누적 피해 + 유지 게이지).
/// 스킬이 막힌 이유(스태미나 부족·재사용 대기) 안내 문구는 알림 한 곳(PickupLog.Warn, 같은 문구 2초 중복 방지)으로 보낸다.
/// 처치 콤보(KillCombo, 우상단)와 별개다. 큰 창이 열려 있으면(HudPolicy.Hidden) 히트 수를 숨긴다.
/// 플레이어가 맞으면 히트 수는 0으로 돌아간다. 집계는 EnemyHealth.damage2Enemy 가 OnPlayerHit 로 넘긴다.
/// </summary>
public class CombatHud : MonoBehaviour
{
    const float NoticeTime = 1.1f;
    static readonly string[] SlotKeys = { "A", "S", "D", "F" };

    static CombatHud instance;
    readonly HitComboCounter _hits = new HitComboCounter();
    CanvasGroup _hitGroup;
    TextMeshProUGUI _hitText, _damageText;
    Image _gauge;
    bool _wasHurt;
    float _noticeUntil = -10f;

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
        WeaponMasteryService.LevelUp += instance.OnMasteryLevelUp;
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
            instance.ShowNotice(slot, Loc.T("Not enough stamina"));
        }
    }

    /// <summary>슬롯 스킬이 재사용 대기 중이라 안 나갔다(남은 시간이 입력 버퍼보다 길 때만 부른다)</summary>
    public static void NotifyCooldown(int slot)
    {
        if (instance != null)
        {
            instance.ShowNotice(slot, Loc.T("On cooldown"));
        }
    }

    /// <summary>슬롯 스킬이 무기 숙련 레벨 미달로 잠겨 있다(쿨·스태미나 소모 없음)</summary>
    public static void NotifyLocked(int slot, int unlockLevel)
    {
        if (instance != null)
        {
            instance.ShowNotice(slot, Loc.F("Unlocks at weapon mastery Lv {0}", unlockLevel));
        }
    }

    void OnDestroy()
    {
        WeaponMasteryService.LevelUp -= OnMasteryLevelUp;
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>장착 무기의 숙련 레벨이 올랐다 — 토스트(해금된 스킬 이름 포함)</summary>
    void OnMasteryLevelUp(string key, int level)
    {
        AttackManager am = FindFirstObjectByType<AttackManager>();
        Weapon w = am != null ? am.equipWeapon : null;
        string msg = Loc.F("Weapon mastery Lv {0}!", level);
        if (w != null && w.MasteryKey == key)
        {
            for (int i = 0; i < w.skill.Length; i++)
            {
                if (w.skill[i] != null && WeaponMasteryService.UnlockLevelFor(w, i) == level)
                {
                    msg += "  " + Loc.F("Skill unlocked: {0}", Loc.T(w.skill[i].skillName));
                }
            }
        }
        if (PickupLog.Notice(msg))
        {
            _noticeUntil = Time.time + NoticeTime;
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

    void ShowNotice(int slot, string message)
    {
        string key = slot >= 0 && slot < SlotKeys.Length ? SlotKeys[slot] : "?";
        if (PickupLog.Warn("[" + key + "] " + message))
        {
            _noticeUntil = Time.time + NoticeTime;
        }
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
