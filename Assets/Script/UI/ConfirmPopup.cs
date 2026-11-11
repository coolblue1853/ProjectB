using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>공용 확인창 요청(U40). 되돌릴 수 없는 동작은 이 창으로 예/아니오를 묻는다</summary>
public sealed class ConfirmRequest
{
    public string Title = "확인";
    public string Message = "";
    public string YesLabel = "예";
    public string NoLabel = "아니오";
    /// <summary>true 면 처음 선택이 예. 기본(false)은 안전한 쪽인 아니오</summary>
    public bool DefaultYes;
    public Action OnYes;
    /// <summary>아니오·Esc·B 로 닫혔을 때</summary>
    public Action OnNo;
}

/// <summary>확인창 선택 규칙(순수). 0 = 예, 1 = 아니오</summary>
public static class ConfirmRules
{
    public const int Yes = 0;
    public const int No = 1;

    public static int Initial(bool defaultYes)
    {
        return defaultYes ? Yes : No;
    }

    /// <summary>좌우 이동. 두 칸뿐이라 끝에서 멈춘다(dir 이 음수면 왼쪽 = 예, 양수면 오른쪽 = 아니오)</summary>
    public static int Move(int selected, int dir)
    {
        if (dir < 0)
        {
            return Yes;
        }
        return dir > 0 ? No : selected;
    }
}

/// <summary>
/// 공용 확인창(U40, 런타임 생성 모달). 마우스: 버튼. 키보드: ←→ 선택, Z/Enter 결정, Esc 아니오. 패드: 십자 ←→, A 결정, B 아니오.
/// 기본 선택은 아니오(DefaultYes 로 바꿀 수 있다). 열려 있는 동안 UiLock 최상단이라 Esc/B 한 번은 이 창만 닫는다.
/// </summary>
public sealed class ConfirmPopup : MonoBehaviour
{
    private const string LockOwner = "ConfirmPopup";
    private static ConfirmPopup _instance;
    private static int _closedFrame = -1;

    /// <summary>열려 있거나 이번 프레임에 막 닫혔다. 호스트 창은 이 동안 같은 입력을 다시 처리하지 않는다</summary>
    public static bool IsBlocking => (_instance != null && _instance._open) || _closedFrame == Time.frameCount;

    public static bool IsOpen => _instance != null && _instance._open;

    /// <summary>현재 선택(0 예, 1 아니오). 닫혀 있으면 -1</summary>
    public static int Selected => _instance != null && _instance._open ? _instance._sel : -1;

    private KeyAction _action;
    private InputAction _left;
    private InputAction _right;
    private InputAction _select;
    private InputAction _enter;
    private InputAction _back;

    private GameObject _root;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _message;
    private readonly Image[] _btnImg = new Image[2];
    private readonly TextMeshProUGUI[] _btnText = new TextMeshProUGUI[2];

    private ConfirmRequest _req;
    private bool _open;
    private int _sel = ConfirmRules.No;
    private int _openFrame;

    /// <summary>확인창을 연다. 이미 열려 있으면 무시한다</summary>
    public static void Open(ConfirmRequest req)
    {
        if (req == null)
        {
            return;
        }
        if (_instance == null)
        {
            var go = new GameObject("ConfirmPopup");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ConfirmPopup>();
            _instance.Build();
        }
        _instance.Show(req);
    }

    /// <summary>테스트·강제 종료용. 콜백 없이 닫는다</summary>
    public static void ForceClose()
    {
        if (_instance != null && _instance._open)
        {
            _instance.Close();
        }
    }

    /// <summary>프로브·테스트용: 예(true)/아니오(false)를 누른 것과 같다</summary>
    public static void Choose(bool yes)
    {
        if (_instance != null && _instance._open)
        {
            _instance.Decide(yes);
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
        _left = _action.UI.LeftInventory;
        _right = _action.UI.RightInventory;
        _select = _action.UI.Select;
        _enter = _action.UI.Enter;
        _back = _action.UI.Back;
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
        if (_left == null)
        {
            return;
        }
        InputAction[] all = { _left, _right, _select, _enter, _back };
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

    private void Show(ConfirmRequest req)
    {
        if (_open)
        {
            return;
        }
        _req = req;
        _sel = ConfirmRules.Initial(req.DefaultYes);
        _openFrame = Time.frameCount;
        _title.SetIfChanged(req.Title);
        _message.SetIfChanged(req.Message);
        _btnText[ConfirmRules.Yes].SetIfChanged(req.YesLabel);
        _btnText[ConfirmRules.No].SetIfChanged(req.NoLabel);
        _open = true;
        _root.SetActive(true);
        UiLock.Set(LockOwner, true);
        Refresh();
    }

    private void Refresh()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < 2; i++)
        {
            bool on = i == _sel;
            _btnImg[i].color = on ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
            _btnText[i].color = on ? theme.unique : Color.white;
        }
    }

    private void Update()
    {
        if (!_open)
        {
            return;
        }
        // 창을 연 입력(같은 프레임의 확인 키)이 바로 결정으로 이어지지 않게 한다
        if (Time.frameCount <= _openFrame)
        {
            return;
        }
        Gamepad pad = Gamepad.current;
        bool back = _back.triggered || (pad != null && pad.startButton.wasPressedThisFrame);
        if (UiLock.TryBack(LockOwner, back))
        {
            Decide(false);
            return;
        }
        if (_left.triggered)
        {
            Move(-1);
        }
        else if (_right.triggered)
        {
            Move(1);
        }
        else if (_select.triggered || _enter.triggered)
        {
            Decide(_sel == ConfirmRules.Yes);
        }
    }

    private void Move(int dir)
    {
        int next = ConfirmRules.Move(_sel, dir);
        if (next != _sel)
        {
            _sel = next;
            Sfx.Play(SfxId.UiMove, 0.5f);
            Refresh();
        }
    }

    private void Decide(bool yes)
    {
        ConfirmRequest req = _req;
        Close();
        if (req == null)
        {
            return;
        }
        Action cb = yes ? req.OnYes : req.OnNo;
        if (cb != null)
        {
            cb();
        }
    }

    private void Close()
    {
        _open = false;
        _closedFrame = Time.frameCount;
        _root.SetActive(false);
        UiLock.Set(LockOwner, false);
        _req = null;
    }

    // ───────────── UI 생성 ─────────────

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 510; // 수량 창(500) 위
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
        dim.color = new Color(0f, 0f, 0f, 0.6f);
        dim.raycastTarget = true; // 뒤쪽 클릭 차단

        Image panel = theme.NewPanel("Panel", _root.transform);
        RectTransform pr = panel.rectTransform;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0.5f);
        pr.anchoredPosition = Vector2.zero;
        pr.sizeDelta = new Vector2(720f, 340f);

        _title = NewText("Title", pr, theme, 34f, -20f, 46f);
        _title.color = theme.unique;
        _message = NewText("Message", pr, theme, 26f, -76f, 130f);
        _message.textWrappingMode = TextWrappingModes.Normal;
        _message.richText = true;
        NewButton(pr, theme, ConfirmRules.Yes, new Vector2(110f, -222f));
        NewButton(pr, theme, ConfirmRules.No, new Vector2(390f, -222f));

        TextMeshProUGUI hint = NewText("Hint", pr, theme, 20f, -296f, 30f);
        hint.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        hint.text = "←→ 선택   Z/Enter/A 결정   Esc/B 아니오";
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
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.alignment = TextAlignmentOptions.Center;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(660f, height);
        return t;
    }

    private void NewButton(RectTransform parent, UiTheme theme, int index, Vector2 pos)
    {
        Image img = theme.NewPanel("Btn_" + index, parent, true);
        img.raycastTarget = true;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(220f, 60f);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        bool yes = index == ConfirmRules.Yes;
        btn.onClick.AddListener(() =>
        {
            if (_open)
            {
                Decide(yes);
            }
        });
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(rt, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            text.font = theme.pixelFont;
        }
        text.fontSize = 28f;
        text.raycastTarget = false;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        _btnImg[index] = img;
        _btnText[index] = text;
    }
}
