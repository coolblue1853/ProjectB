using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>수량 선택 창 요청(구매·판매·나누기·꺼내기·버리기 공통)</summary>
public sealed class QuantityRequest
{
    public string Title = "수량";
    public string ItemName = "";
    public int Min = 1;
    public int Max = 1;
    public int Initial = 1;
    public string ConfirmLabel = "확인";
    /// <summary>수량이 바뀔 때마다 아래 줄에 보일 글(합계 금액 등). null 이면 "n / 최대"</summary>
    public Func<int, string> Detail;
    public Action<int> OnConfirm;
    public Action OnCancel;
}

/// <summary>
/// 수량 선택 창(REWORK-2 U14, 런타임 생성 모달). 규칙은 QuantityRules.
/// 키보드: ↑↓ ±1, ←→ ±10, 숫자 직접 입력(백스페이스), C 최대, Z/Enter 확인, Esc 취소, 길게 누르면 가속.
/// 패드: 십자 ↑↓ ±1, ←→ ±10, Y 최대, A 확인, B 취소. 마우스: 버튼·휠(±1).
/// 열려 있는 동안 UiLock 최상단이라 Esc/B 한 번은 이 창만 닫는다.
/// </summary>
public sealed class QuantityPopup : MonoBehaviour
{
    private const string LockOwner = "QtyPopup";
    private static QuantityPopup _instance;
    private static int _closedFrame = -1;

    /// <summary>열려 있거나 이번 프레임에 막 닫혔다. 호스트 창은 이 동안 같은 입력을 다시 처리하지 않는다</summary>
    public static bool IsBlocking => (_instance != null && _instance._open) || _closedFrame == Time.frameCount || InventorySearch.IsBlocking;

    public static bool IsOpen => _instance != null && _instance._open;
    public static int CurrentValue => _instance != null ? _instance._value : 0;

    private KeyAction _action;
    private InputAction[] _dirs;
    private readonly int[] _deltas = { 1, -1, 10, -10 };
    private readonly float[] _held = new float[4];
    private InputAction _select;
    private InputAction _enter;
    private InputAction _back;
    private InputAction _maxAction;

    private GameObject _root;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _name;
    private TextMeshProUGUI _count;
    private TextMeshProUGUI _detail;
    private TextMeshProUGUI _confirmText;

    private QuantityRequest _req;
    private bool _open;
    private int _value;
    private bool _fresh;
    private int _openFrame;

    private static Key[] _digitKeys;
    private static Key[] _padKeys;

    /// <summary>창을 연다. 이미 열려 있으면 무시한다</summary>
    public static void Open(QuantityRequest req)
    {
        if (req == null)
        {
            return;
        }
        if (_instance == null)
        {
            var go = new GameObject("QuantityPopup");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<QuantityPopup>();
            _instance.Build();
        }
        _instance.Show(req);
    }

    /// <summary>테스트·강제 종료용. 콜백 없이 닫는다</summary>
    public static void ForceClose()
    {
        if (_instance != null && _instance._open)
        {
            _instance.Close(false);
        }
    }

    /// <summary>프로브·테스트용: 현재 창의 확인을 누른 것과 같다</summary>
    public static void ConfirmNow()
    {
        if (_instance != null && _instance._open)
        {
            _instance.Confirm();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
        _closedFrame = -1;
    }

    private void Awake()
    {
        _action = new KeyAction();
        _dirs = new[] { _action.UI.UPInventory, _action.UI.DownInventory, _action.UI.RightInventory, _action.UI.LeftInventory };
        _select = _action.UI.Select;
        _enter = _action.UI.Enter;
        _back = _action.UI.Back;
        _maxAction = _action.UI.Divide;
        EnableActions(true);
    }

    private void OnEnable()
    {
        EnableActions(true);
    }

    private void OnDisable()
    {
        EnableActions(false);
    }

    private void OnDestroy()
    {
        if (_open)
        {
            UiLock.Set(LockOwner, false);
            _open = false; // 씬 전환 등으로 파괴되면 콜백 없이 정리
        }
        if (_action != null)
        {
            _action.Dispose();
        }
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void EnableActions(bool on)
    {
        if (_dirs == null)
        {
            return;
        }
        InputAction[] all = { _dirs[0], _dirs[1], _dirs[2], _dirs[3], _select, _enter, _back, _maxAction };
        foreach (InputAction a in all)
        {
            if (on)
            {
                a.Enable();
            }
            else
            {
                a.Disable();
            }
        }
    }

    private void Show(QuantityRequest req)
    {
        if (_open)
        {
            return;
        }
        _req = req;
        _value = QuantityRules.Clamp(req.Initial, req.Min, Mathf.Max(req.Min, req.Max));
        _fresh = true;
        _openFrame = Time.frameCount;
        for (int i = 0; i < _held.Length; i++)
        {
            _held[i] = 0f;
        }
        _title.SetIfChanged(req.Title);
        _name.SetIfChanged(req.ItemName);
        _confirmText.SetIfChanged(req.ConfirmLabel);
        _open = true;
        _root.SetActive(true);
        UiLock.Set(LockOwner, true);
        Refresh();
    }

    private int Max
    {
        get { return Mathf.Max(_req.Min, _req.Max); }
    }

    private void Refresh()
    {
        _count.SetIfChanged(_value.ToString());
        string detail = _req.Detail != null ? _req.Detail(_value) : _value + " / " + Max;
        _detail.SetIfChanged(detail);
    }

    private void Change(int next)
    {
        int clamped = QuantityRules.Clamp(next, _req.Min, Max);
        if (clamped != _value)
        {
            _value = clamped;
            Sfx.Play(SfxId.UiMove, 0.5f);
            Refresh();
        }
    }

    private void Add(int delta)
    {
        _fresh = true;
        Change(QuantityRules.Step(_value, delta, _req.Min, Max));
    }

    private void SetMax()
    {
        _fresh = true;
        Change(Max);
    }

    private void Update()
    {
        if (!_open)
        {
            return;
        }
        // 창을 연 입력(같은 프레임의 확인 키)이 바로 확정으로 이어지지 않게 한다
        if (Time.frameCount <= _openFrame)
        {
            return;
        }
        if (UiLock.TryBack(LockOwner, _back.triggered))
        {
            Close(true);
            return;
        }
        ReadDirections(Time.unscaledDeltaTime);
        ReadDigits();
        if (_maxAction.triggered)
        {
            SetMax();
        }
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            float wheel = mouse.scroll.ReadValue().y;
            if (wheel > 0.1f)
            {
                Add(1);
            }
            else if (wheel < -0.1f)
            {
                Add(-1);
            }
        }
        if (_select.triggered || _enter.triggered)
        {
            Confirm();
        }
    }

    private void ReadDirections(float dt)
    {
        for (int i = 0; i < _dirs.Length; i++)
        {
            InputAction a = _dirs[i];
            if (a.WasPressedThisFrame())
            {
                _held[i] = 0f;
                Add(_deltas[i]);
            }
            else if (a.IsPressed())
            {
                float before = _held[i];
                _held[i] += dt;
                int ticks = QuantityRules.RepeatTicks(before, _held[i]);
                if (ticks > 0)
                {
                    Add(_deltas[i] * ticks * QuantityRules.HoldMultiplier(_held[i]));
                }
            }
            else
            {
                _held[i] = 0f;
            }
        }
    }

    private void ReadDigits()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }
        if (_digitKeys == null)
        {
            _digitKeys = new Key[10];
            _padKeys = new Key[10];
            for (int d = 0; d < 10; d++)
            {
                _digitKeys[d] = (Key)Enum.Parse(typeof(Key), "Digit" + d);
                _padKeys[d] = (Key)Enum.Parse(typeof(Key), "Numpad" + d);
            }
        }
        for (int d = 0; d < 10; d++)
        {
            if (kb[_digitKeys[d]].wasPressedThisFrame || kb[_padKeys[d]].wasPressedThisFrame)
            {
                int next = QuantityRules.AppendDigit(_value, d, _fresh, _req.Min, Max);
                _fresh = false;
                Change(next);
            }
        }
        if (kb.backspaceKey.wasPressedThisFrame)
        {
            _fresh = false;
            Change(QuantityRules.Backspace(_value, _req.Min, Max));
        }
    }

    private void Confirm()
    {
        QuantityRequest req = _req;
        int value = _value;
        Close(false);
        if (req != null && req.OnConfirm != null)
        {
            req.OnConfirm(value);
        }
    }

    private void Close(bool cancel)
    {
        QuantityRequest req = _req;
        _open = false;
        _closedFrame = Time.frameCount;
        _root.SetActive(false);
        UiLock.Set(LockOwner, false);
        _req = null;
        if (cancel && req != null && req.OnCancel != null)
        {
            req.OnCancel();
        }
    }

    // ───────────── UI 생성 ─────────────

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        _root = new GameObject("Root", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(transform, false);
        var rr = (RectTransform)_root.transform;
        rr.anchorMin = Vector2.zero;
        rr.anchorMax = Vector2.one;
        rr.offsetMin = rr.offsetMax = Vector2.zero;
        var dim = _root.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        dim.raycastTarget = true; // 뒤쪽 클릭 차단

        Image panel = theme.NewPanel("Panel", _root.transform);
        RectTransform pr = panel.rectTransform;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0.5f);
        pr.anchoredPosition = Vector2.zero;
        pr.sizeDelta = new Vector2(760f, 500f);

        _title = NewText("Title", pr, theme, 36f, -22f, 50f);
        _title.color = theme.unique;
        _name = NewText("Name", pr, theme, 30f, -78f, 44f);
        _count = NewText("Count", pr, theme, 76f, -124f, 96f);
        _detail = NewText("Detail", pr, theme, 28f, -226f, 44f);

        float[] xs = { 56f, 196f, 336f, 476f, 616f };
        int[] deltas = { -10, -1, 1, 10, 0 };
        string[] labels = { "-10", "-1", "+1", "+10", "최대" };
        for (int i = 0; i < labels.Length; i++)
        {
            int delta = deltas[i];
            NewButton(pr, theme, labels[i], new Vector2(xs[i], -290f), new Vector2(88f, 56f), () =>
            {
                if (!_open)
                {
                    return;
                }
                if (delta == 0)
                {
                    SetMax();
                }
                else
                {
                    Add(delta);
                }
            }, out _);
        }
        NewButton(pr, theme, "확인", new Vector2(150f, -366f), new Vector2(220f, 60f), () =>
        {
            if (_open)
            {
                Confirm();
            }
        }, out _confirmText);
        NewButton(pr, theme, "취소", new Vector2(390f, -366f), new Vector2(220f, 60f), () =>
        {
            if (_open)
            {
                Close(true);
            }
        }, out _);

        TextMeshProUGUI hint = NewText("Hint", pr, theme, 20f, -446f, 40f);
        hint.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        hint.text = "↑↓ ±1   ←→ ±10   숫자 직접 입력   C/Y 최대   Z/A 확인   Esc/B 취소";
        _root.SetActive(false);
    }

    private static TextMeshProUGUI NewText(string name, RectTransform parent, UiTheme theme, float size, float y, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        t.alignment = TextAlignmentOptions.Center;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(720f, height);
        return t;
    }

    private static void NewButton(RectTransform parent, UiTheme theme, string label, Vector2 pos, Vector2 size, Action onClick, out TextMeshProUGUI text)
    {
        Image img = theme.NewPanel("Btn_" + label, parent, true);
        img.raycastTarget = true;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(rt, false);
        text = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            text.font = theme.pixelFont;
        }
        text.fontSize = 28f;
        text.raycastTarget = false;
        text.alignment = TextAlignmentOptions.Center;
        text.text = label;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
    }
}
