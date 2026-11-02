using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>첫 경험 힌트 규칙 (순수): 힌트 id·문구, 본 목록 직렬화. 세이브는 ES3 새 키 "hints.seen" (쉼표 문자열)</summary>
public static class FirstHintRules
{
    public const string SaveKey = "hints.seen";
    public const string Kill = "kill", Portal = "portal", Rope = "rope", Gather = "gather", Equip = "equip", Potion = "potion", LevelUp = "levelup";

    public static readonly string[] All = { Kill, Portal, Rope, Gather, Equip, Potion, LevelUp };

    /// <summary>힌트 문구. key(이름) 는 현재 키 설정 표시로 바꿔 주는 함수</summary>
    public static string Text(string id, System.Func<string, string> key)
    {
        switch (id)
        {
            case Kill: return "처치! 떨어진 아이템은 가까이 가면 저절로 주워져요. EXP 가 차면 레벨이 올라요.";
            case Portal: return "포털 앞에서 [↑] 를 누르면 이동해요.";
            case Rope: return "로프: [↑] [↓] 로 오르내려요.";
            case Gather: return "채집물 앞에서 [↑] 를 누르면 채집해요.";
            case Equip: return "장비를 얻었어요! [" + key("UI/OpenInventory") + "] 로 가방을 열어 장착해요.";
            case Potion: return "물약을 얻었어요! 가방에서 Shift+[1~4] 로 퀵슬롯에 등록하면 [1~4] 로 써요.";
            case LevelUp: return "[J] 로 퀘스트를 확인하고 다음 목표를 찾아봐요.";
        }
        return "";
    }

    /// <summary>처음 보는 힌트면 seen 에 넣고 true</summary>
    public static bool TryMarkSeen(HashSet<string> seen, string id)
    {
        return !string.IsNullOrEmpty(id) && seen.Add(id);
    }

    public static string Serialize(HashSet<string> seen)
    {
        return string.Join(",", seen);
    }

    public static HashSet<string> Deserialize(string text)
    {
        var set = new HashSet<string>();
        if (!string.IsNullOrEmpty(text))
        {
            foreach (string s in text.Split(','))
            {
                if (s.Length > 0)
                {
                    set.Add(s);
                }
            }
        }
        return set;
    }

    /// <summary>아이템 종류별 힌트 id (Equip / Consum 만, 나머지는 null)</summary>
    public static string HintForItemType(string type)
    {
        return type == "Equip" ? Equip : type == "Consum" ? Potion : null;
    }
}

/// <summary>
/// 첫 경험 힌트 (11-02, 메이플식 안내): 첫 처치·첫 포털 근처·첫 로프·첫 채집물·첫 장비/물약 획득·첫 레벨업 때 화면 하단에 키 안내를 한 번씩 띄운다.
/// 본 목록은 슬롯 세이브(ES3 "hints.seen"), 설정(일시정지 > HINTS)으로 끌 수 있다. 근접 감지는 0.4초 간격 폴링(목록은 3초마다 갱신).
/// </summary>
public sealed class FirstHints : MonoBehaviour, IMessageHandler
{
    private const float Show = 4.2f, Fade = 0.5f, PollInterval = 0.4f, ListInterval = 3f, NearRange = 3.2f;

    private static FirstHints s_instance;

    private HashSet<string> _seen = new HashSet<string>();
    private readonly Queue<string> _queue = new Queue<string>();
    private Datas _boundDatas;
    private bool _loaded; // 세이브 읽기 전에는 저장하지 않는다(본 목록 덮어쓰기 방지)
    private float _nextPoll, _nextList, _hideAt;
    private Portal[] _portals = new Portal[0];
    private GatherNode[] _nodes = new GatherNode[0];
    private CanvasGroup _group;
    private TextMeshProUGUI _text;
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
        var go = new GameObject("[FirstHints]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<FirstHints>();
        s_instance.Build();
    }

    /// <summary>프로브·테스트용</summary>
    public static bool Visible => s_instance != null && s_instance._group != null && s_instance._group.alpha > 0.01f;
    public static string CurrentText => s_instance != null ? s_instance._text.text : "";
    public static int SeenCount => s_instance != null ? s_instance._seen.Count : 0;

    /// <summary>프로브용: 본 목록을 비운다(저장 안 함)</summary>
    public static void DebugReset()
    {
        if (s_instance != null)
        {
            s_instance._seen.Clear();
            s_instance._queue.Clear();
        }
    }

    /// <summary>프로브용: 힌트 하나를 (처음이면) 띄운다</summary>
    public static void DebugTrigger(string id)
    {
        s_instance?.Trigger(id);
    }

    private void OnEnable()
    {
        MessageSystem.AddListener(this, "Kill", string.Empty);
        LevelSystem.LeveledUp += OnLevelUp;
        PickupLog.Picked += OnPicked;
    }

    private void OnDisable()
    {
        MessageSystem.RemoveListener(this);
        LevelSystem.LeveledUp -= OnLevelUp;
        PickupLog.Picked -= OnPicked;
    }

    private void OnDestroy()
    {
        _tween?.Kill();
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    public void OnMessage(MessageArgs messageArgs)
    {
        if (messageArgs.message == "Kill")
        {
            Trigger(FirstHintRules.Kill);
        }
    }

    private void OnLevelUp(int level)
    {
        Trigger(FirstHintRules.LevelUp);
    }

    private void OnPicked(string itemName)
    {
        if (string.IsNullOrEmpty(itemName) || itemName == PickupLog.GoldKey || !CsvItemCatalog.Shared.Exists(itemName))
        {
            return;
        }
        string id = FirstHintRules.HintForItemType(CsvItemCatalog.Shared.Get(itemName).Type);
        if (id != null)
        {
            Trigger(id);
        }
    }

    private void Trigger(string id)
    {
        if (!GameSettings.Hints || _seen.Contains(id))
        {
            return;
        }
        if (FirstHintRules.TryMarkSeen(_seen, id))
        {
            _queue.Enqueue(id);
            Save();
        }
    }

    private void Update()
    {
        SaveManager sm = SaveManager.instance;
        if (sm != null && !ReferenceEquals(sm.datas, _boundDatas))
        {
            _boundDatas = sm.datas;
            Load();
        }
        if (Time.unscaledTime >= _nextPoll)
        {
            _nextPoll = Time.unscaledTime + PollInterval;
            Poll();
        }
        if (_queue.Count > 0 && Time.unscaledTime >= _hideAt)
        {
            ShowNext();
        }
    }

    private void Poll()
    {
        PlayerController pc = PlayerController.instance;
        if (!GameSettings.Hints || pc == null)
        {
            return;
        }
        if (pc.isUpLadder)
        {
            Trigger(FirstHintRules.Rope);
        }
        bool needPortal = !_seen.Contains(FirstHintRules.Portal), needGather = !_seen.Contains(FirstHintRules.Gather);
        if (!needPortal && !needGather)
        {
            return;
        }
        if (Time.unscaledTime >= _nextList)
        {
            _nextList = Time.unscaledTime + ListInterval;
            _portals = needPortal ? FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None) : _portals;
            _nodes = needGather ? FindObjectsByType<GatherNode>(FindObjectsInactive.Exclude, FindObjectsSortMode.None) : _nodes;
        }
        Vector2 p = pc.transform.position;
        float sq = NearRange * NearRange;
        if (needPortal)
        {
            foreach (Portal t in _portals)
            {
                if (t != null && t.isActiveAndEnabled && ((Vector2)t.transform.position - p).sqrMagnitude < sq)
                {
                    Trigger(FirstHintRules.Portal);
                    break;
                }
            }
        }
        if (needGather)
        {
            foreach (GatherNode g in _nodes)
            {
                if (g != null && g.isActiveAndEnabled && g.IsReady && ((Vector2)g.transform.position - p).sqrMagnitude < sq)
                {
                    Trigger(FirstHintRules.Gather);
                    break;
                }
            }
        }
    }

    private void ShowNext()
    {
        string id = _queue.Dequeue();
        _text.text = FirstHintRules.Text(id, k => ControlsWindow.Resolve(k, false));
        _hideAt = Time.unscaledTime + Show + Fade + 0.2f;
        _tween?.Kill();
        _group.alpha = 0f;
        _tween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(_group.DOFade(1f, 0.25f))
            .AppendInterval(Show)
            .Append(_group.DOFade(0f, Fade));
    }

    private void Load()
    {
        string text = null;
        try
        {
            text = ES3.KeyExists(FirstHintRules.SaveKey) ? ES3.Load<string>(FirstHintRules.SaveKey) : null;
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "힌트 목록 읽기 실패: " + e.Message);
        }
        _seen = FirstHintRules.Deserialize(text);
        _loaded = true;
    }

    private void Save()
    {
        if (!_loaded)
        {
            return;
        }
        try
        {
            ES3.Save(FirstHintRules.SaveKey, FirstHintRules.Serialize(_seen));
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "힌트 목록 저장 실패: " + e.Message);
        }
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 78;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var root = new GameObject("Hint", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        root.transform.SetParent(transform, false);
        var rr = (RectTransform)root.transform;
        rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 0f);
        rr.anchoredPosition = new Vector2(0f, 190f);
        rr.sizeDelta = new Vector2(1300f, 70f);
        root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        root.GetComponent<Image>().raycastTarget = false;
        _group = root.GetComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;
        _text = LevelUpBanner.NewText("Text", rr, theme, 34f, Vector2.zero, Color.white);
        _text.rectTransform.sizeDelta = new Vector2(1280f, 60f);
    }
}
