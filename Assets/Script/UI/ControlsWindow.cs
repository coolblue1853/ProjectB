using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 조작 안내 창 (11-02): F10 또는 일시정지 > CONTROLS 로 연다. ControlsGuide 표를 현재 키 설정(InputBindings.Master)으로 채워 키보드·게임패드 두 열로 보여 준다.
/// 닫을 때는 Back(ESC/B) 또는 F10. 열려 있는 동안 UiLock "Controls". 키 설정 창은 일시정지 > 설정 > 키 설정.
/// </summary>
public class ControlsWindow : MonoBehaviour
{
    private static ControlsWindow s_instance;

    private KeyAction _action;
    private InputAction _back;
    private GameObject _panel;
    private TextMeshProUGUI _left, _right;
    private bool _open;

    public static bool IsOpen => s_instance != null && s_instance._open;

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
        var go = new GameObject("[ControlsWindow]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<ControlsWindow>();
    }

    public static void Toggle()
    {
        if (s_instance == null)
        {
            return;
        }
        if (s_instance._open)
        {
            s_instance.Close();
        }
        else
        {
            s_instance.Open();
        }
    }

    private void Awake()
    {
        _action = new KeyAction();
        _back = _action.UI.Back;
    }

    private void OnDestroy()
    {
        UiLock.Set("Controls", false);
        _action?.Dispose();
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    private void OnEnable()
    {
        InputBindings.Changed += OnBindingsChanged;
    }

    private void OnDisable()
    {
        InputBindings.Changed -= OnBindingsChanged;
        Close();
    }

    private void OnBindingsChanged()
    {
        if (_open)
        {
            Redraw();
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        bool pressed = kb != null && kb.f10Key.wasPressedThisFrame;
        if (pressed && (_open || !DatabaseManager.isOpenUI))
        {
            Toggle();
            return;
        }
        if (_open && UiLock.TryBack("Controls", _back.triggered))
        {
            Close();
        }
    }

    private void Open()
    {
        if (_panel == null)
        {
            Build();
        }
        _open = true;
        UiLock.Set("Controls", true);
        _action.UI.Enable();
        _panel.SetActive(true);
        Redraw();
        Sfx.Play(SfxId.UiOpen);
    }

    private void Close()
    {
        if (!_open)
        {
            return;
        }
        _open = false;
        UiLock.Set("Controls", false);
        _action.UI.Disable();
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    /// <summary>{이름} 자리를 현재 키 설정 표시로: "Jump" = Player 맵(재지정 가능), "UI/OpenInventory" = UI 맵</summary>
    internal static string Resolve(string name, bool gamepad)
    {
        if (name.IndexOf('/') < 0)
        {
            return InputBindings.Display(name, gamepad);
        }
        InputAction action = InputBindings.Master.asset.FindAction(name);
        int i = action != null ? InputBindings.BindingIndex(action, gamepad) : -1;
        string s = i >= 0 ? action.GetBindingDisplayString(i) : "";
        return string.IsNullOrEmpty(s) ? "-" : s.Replace("Left Stick Press", "LS").Replace("Right Stick Press", "RS");
    }

    private void Redraw()
    {
        var left = new StringBuilder();
        var right = new StringBuilder();
        foreach (ControlsGuide.Section sec in ControlsGuide.Sections)
        {
            StringBuilder sb = sec.RightColumn ? right : left;
            sb.Append("<color=#FFD633>").Append(sec.Title).Append("</color><pos=44%><color=#777777>키보드<pos=76%>패드</color>\n");
            foreach (ControlsGuide.Row row in sec.Rows)
            {
                sb.Append(row.Label)
                  .Append("<pos=44%><color=#9ED0FF>").Append(ControlsGuide.Fill(row.Keyboard, false, Resolve))
                  .Append("</color><pos=76%><color=#BBBBBB>").Append(ControlsGuide.Fill(row.Pad, true, Resolve)).Append("</color>\n");
            }
            sb.Append('\n');
        }
        _left.text = left.ToString();
        _right.text = right.ToString();
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 72;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _panel = theme.NewPanel("Panel", transform).gameObject;
        var pr = (RectTransform)_panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1700f, 880f);

        TextMeshProUGUI title = NewText("Title", pr, theme, 42f);
        Place(title.rectTransform, new Vector2(34f, -18f), new Vector2(900f, 54f));
        title.text = Loc.IsKorean ? "조작 안내" : "CONTROLS";
        title.color = theme.unique;

        _left = NewText("Left", pr, theme, 25f);
        Place(_left.rectTransform, new Vector2(40f, -90f), new Vector2(790f, 720f));
        _right = NewText("Right", pr, theme, 25f);
        Place(_right.rectTransform, new Vector2(870f, -90f), new Vector2(790f, 720f));

        TextMeshProUGUI hint = NewText("Hint", pr, theme, 22f);
        Place(hint.rectTransform, new Vector2(40f, -830f), new Vector2(1600f, 30f));
        hint.text = "F10 / BACK 닫기    키 변경: 일시정지 > 설정 > 키 설정 (재지정 가능한 키는 이 표에 바로 반영)";
        hint.color = new Color(1f, 1f, 1f, 0.55f);
        _panel.SetActive(false);
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.raycastTarget = false;
        t.richText = true;
        t.enableWordWrapping = false;
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(0, 0, 0, 200);
        return t;
    }

    private static void Place(RectTransform rt, Vector2 topLeft, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = topLeft;
        rt.sizeDelta = size;
    }
}
