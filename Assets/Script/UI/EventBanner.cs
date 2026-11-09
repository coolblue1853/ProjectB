using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상단 이벤트 알림 (11-02): 퀘스트 수락·완료 배너(제목 + 보상 요약)와 사망 안내(EXP 손실 수치·마을 부활 안내).
/// 퀘스트 배너는 위쪽, 레벨업 배너(LevelUpBanner)는 가운데라 겹치지 않는다. 큰 창이 열려 있어도 보이도록 unscaled 시간으로 돈다.
/// </summary>
public sealed class EventBanner : MonoBehaviour
{
    private const float QuestPop = 0.2f, QuestHold = 2.6f, QuestFade = 0.5f;

    private static EventBanner s_instance;

    private CanvasGroup _quest, _death;
    private TextMeshProUGUI _qTitle, _qSub, _dTitle, _dSub;
    private Tween _qTween, _dTween;

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
        var go = new GameObject("[EventBanner]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<EventBanner>();
        s_instance.Build();
    }

    /// <summary>프로브·테스트용: 퀘스트 배너 / 사망 안내가 보이는가</summary>
    public static bool QuestVisible => s_instance != null && s_instance._quest != null && s_instance._quest.alpha > 0.01f;
    public static bool DeathVisible => s_instance != null && s_instance._death != null && s_instance._death.alpha > 0.01f;
    public static string QuestText => s_instance != null ? s_instance._qTitle.text + " | " + s_instance._qSub.text : "";
    public static string DeathText => s_instance != null ? s_instance._dTitle.text + " | " + s_instance._dSub.text : "";

    private void OnEnable()
    {
        NpcQuestLog.Accepted += OnAccepted;
        NpcQuestLog.Completed += OnCompleted;
        PlayerHealthManager.PlayerDied += OnDied;
        PlayerHealthManager.PlayerRespawned += OnRespawned;
    }

    private void OnDisable()
    {
        NpcQuestLog.Accepted -= OnAccepted;
        NpcQuestLog.Completed -= OnCompleted;
        PlayerHealthManager.PlayerDied -= OnDied;
        PlayerHealthManager.PlayerRespawned -= OnRespawned;
    }

    private void OnDestroy()
    {
        _qTween?.Kill();
        _dTween?.Kill();
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    /// <summary>보상 한 줄 요약: "EXP +120   300 G   약초 x3" (0 은 뺀다)</summary>
    public static string RewardText(NpcQuestDef d)
    {
        var parts = new System.Collections.Generic.List<string>(3);
        if (d.Exp > 0) { parts.Add("EXP +" + d.Exp); }
        if (d.Gold > 0) { parts.Add(d.Gold + " G"); }
        if (!string.IsNullOrEmpty(d.RewardItem) && d.RewardCount > 0)
        {
            ItemDef def = CsvItemCatalog.Shared.Exists(d.RewardItem) ? CsvItemCatalog.Shared.Get(d.RewardItem) : null;
            string name = def != null && !string.IsNullOrEmpty(def.DisplayName) ? def.DisplayName : d.RewardItem;
            parts.Add(name + " x" + d.RewardCount);
        }
        return string.Join("   ", parts);
    }

    /// <summary>사망 안내 둘째 줄: EXP 손실 수치와 마을 부활 안내</summary>
    public static string DeathSub(int expLoss)
    {
        string loss = expLoss > 0 ? "EXP -" + expLoss + " (5%)" : "EXP 손실 없음";
        return loss + "   ·   마을에서 부활합니다";
    }

    /// <summary>사망 배너 팁 (결정 11-05): 죽을 때마다 순서대로 순환</summary>
    public static readonly string[] DeathTips =
    {
        "HP 물약은 빠른 슬롯에 등록해 두면 위기에 바로 쓸 수 있어요",
        "귀환 주문서로 위험한 사냥터에서 마을로 빠르게 돌아올 수 있어요",
        "저장 지점(마을 NPC)에서 진행 상황을 자주 저장하세요",
        "레벨 10 이하에서는 사망해도 EXP 를 잃지 않아요",
        "보스는 패턴을 외우면 쉬워져요. 구르기로 공격을 피해 보세요",
    };

    private static int s_tipIndex = -1;

    /// <summary>다음 팁 한 줄 (호출마다 한 칸씩 순환)</summary>
    public static string NextDeathTip()
    {
        if (s_tipIndex < 0)
        {
            s_tipIndex = Random.Range(0, DeathTips.Length);
        }
        string tip = DeathTips[s_tipIndex % DeathTips.Length];
        s_tipIndex = (s_tipIndex + 1) % DeathTips.Length;
        return tip;
    }

    private void OnAccepted(NpcQuestDef d)
    {
        ShowQuest("퀘스트 수락", new Color(1f, 0.85f, 0.2f), d.Title, "Q 로 퀘스트 확인");
    }

    private void OnCompleted(NpcQuestDef d)
    {
        ShowQuest("퀘스트 완료", new Color(0.62f, 0.93f, 0.62f), d.Title, RewardText(d));
    }

    private void ShowQuest(string head, Color color, string title, string sub)
    {
        _qTitle.text = head;
        _qTitle.color = color;
        _qSub.text = title + (string.IsNullOrEmpty(sub) ? "" : "\n<size=70%><color=#DDDDDD>" + sub + "</color></size>");
        _qTween?.Kill();
        _quest.alpha = 1f;
        var rt = (RectTransform)_quest.transform;
        rt.localScale = Vector3.one * 0.7f;
        _qTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(rt.DOScale(1f, QuestPop).SetEase(Ease.OutBack))
            .AppendInterval(QuestHold)
            .Append(_quest.DOFade(0f, QuestFade));
    }

    private void OnDied()
    {
        _dTitle.text = "쓰러졌다";
        _dSub.text = DeathSub(LevelSystem.LastDeathLoss) + "\n<size=70%><color=#BBBBBB>TIP  " + NextDeathTip() + "</color></size>";
        _dTween?.Kill();
        _dTween = _death.DOFade(1f, 0.4f).SetUpdate(true).SetLink(gameObject);
    }

    private void OnRespawned()
    {
        _dTween?.Kill();
        _dTween = _death.DOFade(0f, 0.4f).SetUpdate(true).SetLink(gameObject);
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 79;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _quest = NewGroup("Quest", new Vector2(0f, 400f), new Vector2(1000f, 170f));
        _qTitle = LevelUpBanner.NewText("Head", _quest.transform, theme, 50f, new Vector2(0f, 45f), Color.white);
        _qSub = LevelUpBanner.NewText("Sub", _quest.transform, theme, 46f, new Vector2(0f, -30f), Color.white);
        _qSub.rectTransform.sizeDelta = new Vector2(1000f, 120f);

        _death = NewGroup("Death", new Vector2(0f, 60f), new Vector2(1000f, 200f));
        var deathCanvas = _death.gameObject.AddComponent<Canvas>(); // 사망 암전(FeelFx 900) 위에 보이게
        deathCanvas.overrideSorting = true;
        deathCanvas.sortingOrder = 950;
        _dTitle = LevelUpBanner.NewText("Head", _death.transform, theme, 90f, new Vector2(0f, 40f), new Color(0.9f, 0.25f, 0.25f));
        _dSub = LevelUpBanner.NewText("Sub", _death.transform, theme, 40f, new Vector2(0f, -55f), Color.white);
        _dSub.rectTransform.sizeDelta = new Vector2(1000f, 110f);
    }

    private CanvasGroup NewGroup(string name, Vector2 pos, Vector2 size)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(transform, false);
        var rr = (RectTransform)root.transform;
        rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 0.5f);
        rr.anchoredPosition = pos;
        rr.sizeDelta = size;
        var g = root.GetComponent<CanvasGroup>();
        g.alpha = 0f;
        g.blocksRaycasts = false;
        g.interactable = false;
        return g;
    }
}
