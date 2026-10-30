using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 키 설정 창(BasicFeatures.md 5절): 일시정지 > 설정 > 키 설정에서 연다.
/// 위/아래 행, 좌/우 열(키보드·게임패드), 선택/ENTER 로 재지정(새 키 입력 대기, ESC 취소). 마지막 두 행은 기본값 복원·뒤로.
/// 편집은 InputBindings.Master 에 PerformInteractiveRebinding 으로 받고, 적용·저장은 InputBindings.Assign/Commit.
/// </summary>
public class KeyBindWindow : MonoBehaviour
{
    private const float RowHeight = 40f;
    private const float InputGuard = 0.25f;

    private static KeyBindWindow _instance;

    private readonly List<(Image bg, TextMeshProUGUI label, TextMeshProUGUI kb, TextMeshProUGUI pad)> _rows =
        new List<(Image, TextMeshProUGUI, TextMeshProUGUI, TextMeshProUGUI)>();

    private KeyAction _action;
    private InputAction _up, _down, _left, _right, _select;
    private GameObject _panel;
    private GameObject _dim;
    private TextMeshProUGUI _hint;
    private int _cursor;
    private int _column; // 0 키보드, 1 게임패드
    private bool _open;
    private float _ignoreUntil;
    private InputActionRebindingExtensions.RebindingOperation _op;

    public static bool IsOpen => _instance != null && _instance._open;

    /// <summary>창을 닫은 프레임(같은 프레임의 ESC 를 일시정지 메뉴가 다시 받지 않게)</summary>
    public static int ClosedFrame { get; private set; } = -10;

    private static int RowCount => InputBindings.Rebindable.Length + 2;
    private static int ResetRow => InputBindings.Rebindable.Length;
    private static int BackRow => InputBindings.Rebindable.Length + 1;

    public static void Open()
    {
        if (_instance == null)
        {
            var go = new GameObject("[KeyBindWindow]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<KeyBindWindow>();
        }
        _instance.Show();
    }

    /// <summary>프로브·테스트용: 지금 행/열의 재지정을 시작한다</summary>
    public static bool BeginRebind(string actionName, bool gamepad)
    {
        if (!IsOpen)
        {
            return false;
        }
        int row = System.Array.IndexOf(InputBindings.Rebindable, actionName);
        if (row < 0)
        {
            return false;
        }
        _instance._cursor = row;
        _instance._column = gamepad ? 1 : 0;
        _instance.StartRebind();
        return true;
    }

    public static bool IsWaitingForKey => _instance != null && _instance._op != null;

    private void Awake()
    {
        _action = new KeyAction();
        _up = _action.UI.UPInventory;
        _down = _action.UI.DownInventory;
        _left = _action.UI.LeftInventory;
        _right = _action.UI.RightInventory;
        _select = _action.UI.Select;
        Build();
    }

    private void OnDestroy()
    {
        _open = false; // 취소 콜백이 파괴 중인 글자를 다시 그리지 않게
        CancelRebind();
        _action?.Dispose();
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Show()
    {
        _open = true;
        _cursor = 0;
        _column = 0;
        _ignoreUntil = Time.unscaledTime + InputGuard;
        _action.UI.Enable();
        _dim.SetActive(true);
        _panel.SetActive(true);
        Redraw();
    }

    private void Close()
    {
        CancelRebind();
        _open = false;
        ClosedFrame = Time.frameCount;
        _action.UI.Disable();
        _panel.SetActive(false);
        _dim.SetActive(false);
    }

    private void Update()
    {
        if (!_open || _op != null || Time.unscaledTime < _ignoreUntil)
        {
            return;
        }
        Keyboard kb = Keyboard.current;
        bool esc = (kb != null && kb.escapeKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (esc)
        {
            Close();
            return;
        }
        if (_up.triggered)
        {
            _cursor = (_cursor - 1 + RowCount) % RowCount;
            Moved();
        }
        else if (_down.triggered)
        {
            _cursor = (_cursor + 1) % RowCount;
            Moved();
        }
        else if (_left.triggered || _right.triggered)
        {
            _column = 1 - _column;
            Moved();
        }
        else if (_select.triggered || (kb != null && kb.enterKey.wasPressedThisFrame))
        {
            Activate();
        }
    }

    private void Moved()
    {
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    private void Activate()
    {
        if (_cursor == BackRow)
        {
            Close();
            return;
        }
        if (_cursor == ResetRow)
        {
            InputBindings.ResetToDefaults();
            Sfx.Play(SfxId.Craft, 0.6f);
            Redraw();
            return;
        }
        StartRebind();
    }

    private void StartRebind()
    {
        CancelRebind();
        bool gamepad = _column == 1;
        string actionName = InputBindings.Rebindable[_cursor];
        InputAction action = InputBindings.Master.asset.FindAction("Player/" + actionName);
        int index = InputBindings.BindingIndex(action, gamepad);
        if (index < 0)
        {
            return;
        }
        string picked = null;
        _op = action.PerformInteractiveRebinding(index)
            .WithControlsHavingToMatchPath(gamepad ? InputBindings.GamepadLayout : InputBindings.KeyboardLayout)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough(gamepad ? "<Gamepad>/start" : "<Keyboard>/escape")
            .WithTimeout(8f) // 다른 장치만 쓰는 사람도 대기에서 빠져나오게
            .OnApplyBinding((op, path) => picked = path) // 적용은 InputBindings.Assign 이 한다(겹침 교환·고정 키 거절)
            .OnComplete(op => FinishRebind(actionName, gamepad, picked))
            .OnCancel(op => FinishRebind(actionName, gamepad, null));
        _op.Start();
        Redraw();
    }

    private void FinishRebind(string actionName, bool gamepad, string path)
    {
        DisposeOp();
        _ignoreUntil = Time.unscaledTime + InputGuard; // 지정에 쓴 키가 창 조작으로 다시 읽히지 않게
        if (path != null)
        {
            InputBindings.AssignResult r = InputBindings.Assign(InputBindings.Master.asset, actionName, gamepad, path, out string swapped);
            if (r == InputBindings.AssignResult.Reserved)
            {
                PickupLog.Warn(Loc.T("That key is reserved"));
            }
            else if (r != InputBindings.AssignResult.Invalid)
            {
                InputBindings.Commit();
                Sfx.Play(SfxId.UiMove, 0.8f);
                GameLog.Info(GameLog.Cat.Player, "키 지정 " + actionName + (gamepad ? "(패드)" : "") + " = " + path + (swapped != null ? " ↔ " + swapped : ""));
            }
        }
        if (_open)
        {
            Redraw();
        }
    }

    private void CancelRebind()
    {
        if (_op != null)
        {
            _op.Cancel();
        }
        DisposeOp();
    }

    private void DisposeOp()
    {
        if (_op != null)
        {
            var op = _op;
            _op = null;
            op.Dispose();
        }
    }

    private void Redraw()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < _rows.Count; i++)
        {
            bool sel = i == _cursor;
            var row = _rows[i];
            row.bg.color = sel ? Color.white : new Color(1f, 1f, 1f, 0f);
            row.label.color = sel ? theme.unique : Color.white;
            if (i < InputBindings.Rebindable.Length)
            {
                string name = InputBindings.Rebindable[i];
                row.label.text = Loc.T(LabelOf(name));
                row.kb.text = Cell(InputBindings.Display(name, false), sel && _column == 0);
                row.pad.text = Cell(InputBindings.Display(name, true), sel && _column == 1);
            }
            else
            {
                row.label.text = Loc.T(i == ResetRow ? "RESET TO DEFAULTS" : "BACK");
                row.kb.text = row.pad.text = "";
            }
        }
        _hint.text = Loc.T(_op != null ? _column == 1 ? "PRESS A BUTTON  ·  START cancel" : "PRESS A KEY  ·  ESC cancel" : "UP/DOWN  ·  LEFT/RIGHT column  ·  SELECT rebind  ·  ESC back");
    }

    private string Cell(string key, bool selected)
    {
        if (selected && _op != null)
        {
            return "<color=#FFD040>...</color>";
        }
        return selected ? "<color=#FFD040>[" + key + "]</color>" : "<color=#9ED0FF>" + key + "</color>";
    }

    private static string LabelOf(string actionName)
    {
        switch (actionName)
        {
            case "Jump": return "JUMP";
            case "Dash": return "DASH";
            case "Run": return "RUN";
            case "Attack": return "ATTACK";
            case "SkillA": return "SKILL 1";
            case "SkillS": return "SKILL 2";
            case "SkillD": return "SKILL 3";
            case "SkillF": return "SKILL 4";
            case "Quick1": return "QUICK SLOT 1";
            case "Quick2": return "QUICK SLOT 2";
            case "Quick3": return "QUICK SLOT 3";
            case "Quick4": return "QUICK SLOT 4";
            case "Artifact": return "ARTIFACT";
            case "ArtifactNext": return "NEXT ARTIFACT";
            default: return actionName;
        }
    }

    // ── UI ──
    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95; // 일시정지 메뉴(90) 위
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // 뒤의 일시정지 메뉴가 비치지 않게 화면 전체를 어둡게 덮는다
        var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        dim.transform.SetParent(transform, false);
        dim.rectTransform.anchorMin = Vector2.zero;
        dim.rectTransform.anchorMax = Vector2.one;
        dim.rectTransform.offsetMin = dim.rectTransform.offsetMax = Vector2.zero;
        dim.color = new Color(0f, 0f, 0f, 0.85f);
        _dim = dim.gameObject;
        _dim.SetActive(false);

        float height = 130f + RowCount * RowHeight + 50f;
        _panel = theme.NewPanel("Panel", transform).gameObject;
        var pr = (RectTransform)_panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(760f, height);

        TextMeshProUGUI title = NewText("Title", pr, theme, 38f, TextAlignmentOptions.Center);
        Place(title.rectTransform, new Vector2(0f, -20f), new Vector2(760f, 46f));
        title.text = Loc.T("KEY BINDINGS");
        title.color = theme.unique;

        TextMeshProUGUI headKb = NewText("HeadKb", pr, theme, 22f, TextAlignmentOptions.Center);
        Place(headKb.rectTransform, new Vector2(380f, -78f), new Vector2(170f, 30f));
        headKb.text = Loc.T("KEYBOARD");
        TextMeshProUGUI headPad = NewText("HeadPad", pr, theme, 22f, TextAlignmentOptions.Center);
        Place(headPad.rectTransform, new Vector2(560f, -78f), new Vector2(170f, 30f));
        headPad.text = Loc.T("GAMEPAD");

        for (int i = 0; i < RowCount; i++)
        {
            Image bg = theme.NewPanel("Row" + i, pr, true, 0f);
            Place(bg.rectTransform, new Vector2(30f, -116f - i * RowHeight), new Vector2(700f, RowHeight - 4f));
            TextMeshProUGUI label = NewText("Label", bg.transform, theme, 24f, TextAlignmentOptions.MidlineLeft);
            Place(label.rectTransform, new Vector2(14f, 0f), new Vector2(330f, RowHeight - 4f));
            TextMeshProUGUI kb = NewText("Kb", bg.transform, theme, 24f, TextAlignmentOptions.Center);
            Place(kb.rectTransform, new Vector2(350f, 0f), new Vector2(170f, RowHeight - 4f));
            TextMeshProUGUI pad = NewText("Pad", bg.transform, theme, 24f, TextAlignmentOptions.Center);
            Place(pad.rectTransform, new Vector2(530f, 0f), new Vector2(170f, RowHeight - 4f));
            _rows.Add((bg, label, kb, pad));
        }
        _hint = NewText("Hint", pr, theme, 20f, TextAlignmentOptions.Center);
        Place(_hint.rectTransform, new Vector2(0f, -(height - 44f)), new Vector2(760f, 28f));
        _panel.SetActive(false);
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size, TextAlignmentOptions align)
    {
        var t = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    private static void Place(RectTransform rt, Vector2 topLeft, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = topLeft;
        rt.sizeDelta = size;
    }
}
