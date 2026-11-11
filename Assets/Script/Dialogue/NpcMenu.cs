using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// NPC 선택지 창 (REWORK-8). ↑ 로 말을 걸면 열리고, ↑↓/십자/스틱 이동 · Z/Enter/A 선택 · 마우스 호버·클릭 · Esc/B 취소.
/// 열린 동안 UiLock "NpcMenu" 로 조작을 막는다. 임시 UI(기존 UiTheme 패널 재사용).
/// </summary>
public sealed class NpcMenu : MonoBehaviour
{
    private const int MaxRows = 10;
    private const float RowHeight = 52f;

    private static NpcMenu s_instance;

    private KeyAction _action;
    private GameObject _root;
    private RectTransform _panel;
    private TextMeshProUGUI _title;
    private readonly List<Image> _bgs = new List<Image>();
    private readonly List<TextMeshProUGUI> _labels = new List<TextMeshProUGUI>();
    private List<NpcMenuRules.Kind> _kinds;
    private List<string> _rowLabels;   // 행 모드(목적지 목록 등): 항목마다 글자·불가 사유(null=선택 가능), 마지막 줄은 닫기
    private List<string> _rowReasons;
    private Action<int> _onRow;
    private Action<NpcMenuRules.Kind> _onPick;
    private int _cursor;
    private int _openFrame;
    private bool _open;
    private bool _armed; // 열 때 누르고 있던 ↑ 가 선택으로 번지지 않게, 키를 한 번 뗀 뒤부터 입력을 받는다

    public static bool IsOpen => s_instance != null && s_instance._open;

    /// <summary>프로브·테스트용: 커서 위치 (닫혀 있으면 -1)</summary>
    public static int Cursor => IsOpen ? s_instance._cursor : -1;

    /// <summary>프로브·테스트용: 현재 선택지 항목 수</summary>
    public static int Count => s_instance != null && s_instance._kinds != null ? s_instance._kinds.Count : 0;

    public static void Open(string title, List<NpcMenuRules.Kind> kinds, Action<NpcMenuRules.Kind> onPick)
    {
        if (s_instance == null)
        {
            var go = new GameObject("[NpcMenu]");
            DontDestroyOnLoad(go);
            s_instance = go.AddComponent<NpcMenu>();
        }
        s_instance.Show(title, kinds, onPick);
    }

    /// <summary>
    /// 행 모드: 임의 글자 목록. reasons[i] 가 null 이 아니면 그 줄은 고를 수 없고(회색) 고르면 사유만 알린다. 닫기 줄은 자동으로 붙는다.
    /// </summary>
    public static void OpenRows(string title, IList<string> labels, IList<string> reasons, Action<int> onPick)
    {
        var kinds = new List<NpcMenuRules.Kind>();
        var texts = new List<string>();
        var why = new List<string>();
        for (int i = 0; i < labels.Count && i < MaxRows - 1; i++)
        {
            kinds.Add(NpcMenuRules.Kind.Talk);
            texts.Add(labels[i]);
            why.Add(reasons != null && i < reasons.Count ? reasons[i] : null);
        }
        kinds.Add(NpcMenuRules.Kind.Close);
        texts.Add(NpcMenuRules.Label(NpcMenuRules.Kind.Close));
        why.Add(null);
        Open(title, kinds, null);
        s_instance._rowLabels = texts;
        s_instance._rowReasons = why;
        s_instance._onRow = onPick;
        s_instance.Redraw();
    }

    /// <summary>프로브·테스트용: 커서 위치 항목을 고른다</summary>
    public static void PickCurrent()
    {
        if (IsOpen)
        {
            s_instance.Pick(s_instance._cursor);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    private void Awake()
    {
        _action = new KeyAction();
        Build();
    }

    private void OnDestroy()
    {
        UiLock.Set("NpcMenu", false);
        _action?.Dispose();
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    private void Show(string title, List<NpcMenuRules.Kind> kinds, Action<NpcMenuRules.Kind> onPick)
    {
        _kinds = kinds;
        _onPick = onPick;
        _rowLabels = null;
        _rowReasons = null;
        _onRow = null;
        _cursor = 0;
        _open = true;
        _openFrame = Time.frameCount;
        _armed = false;
        _title.text = title;
        int n = Mathf.Min(kinds.Count, MaxRows);
        _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, 90f + n * RowHeight);
        _root.SetActive(true);
        _action.UI.Enable();
        UiLock.Set("NpcMenu", true);
        Sfx.Play(SfxId.UiOpen, 0.6f);
        Redraw();
    }

    private void Close()
    {
        _open = false;
        _root.SetActive(false);
        _action.UI.Disable();
        UiLock.Set("NpcMenu", false);
    }

    private void Update()
    {
        if (!_open || Time.frameCount == _openFrame)
        {
            return;
        }
        if (UiLock.TryBack("NpcMenu", _action.UI.Back.triggered))
        {
            Close();
            return;
        }
        if (!_armed)
        {
            _armed = !_action.UI.UPInventory.IsPressed() && !_action.UI.DownInventory.IsPressed() && !_action.UI.Select.IsPressed();
            return;
        }
        int n = Mathf.Min(_kinds.Count, MaxRows);
        int delta = _action.UI.UPInventory.triggered ? -1 : _action.UI.DownInventory.triggered ? 1 : 0;
        if (delta != 0)
        {
            _cursor = NpcMenuRules.Step(_cursor, n, delta);
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
            return;
        }
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            int hit = RowUnderMouse(mouse.position.ReadValue(), n);
            if (hit >= 0 && hit != _cursor && mouse.delta.ReadValue().sqrMagnitude > 0.01f)
            {
                _cursor = hit;
                Redraw();
            }
            if (mouse.leftButton.wasPressedThisFrame && hit >= 0)
            {
                Pick(hit);
                return;
            }
        }
        Keyboard kb = Keyboard.current;
        if (_action.UI.Select.triggered || (kb != null && kb.enterKey.wasPressedThisFrame))
        {
            Pick(_cursor);
        }
    }

    private int RowUnderMouse(Vector2 screen, int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)_bgs[i].transform, screen, null))
            {
                return i;
            }
        }
        return -1;
    }

    private void Pick(int index)
    {
        if (!_open || index < 0 || index >= _kinds.Count)
        {
            return;
        }
        NpcMenuRules.Kind kind = _kinds[index];
        if (_rowLabels != null)
        {
            string why = _rowReasons[index];
            if (kind != NpcMenuRules.Kind.Close && why != null)
            {
                PickupLog.Warn(why); // 고를 수 없는 줄: 메뉴는 그대로 두고 사유만 알린다
                Sfx.Play(SfxId.UiMove, 0.6f);
                return;
            }
            Action<int> row = _onRow;
            Close();
            Sfx.Play(SfxId.UiOpen, 0.5f);
            if (kind != NpcMenuRules.Kind.Close)
            {
                row?.Invoke(index);
            }
            return;
        }
        Action<NpcMenuRules.Kind> cb = _onPick;
        Close();
        Sfx.Play(SfxId.UiOpen, 0.5f);
        if (kind != NpcMenuRules.Kind.Close)
        {
            cb?.Invoke(kind);
        }
    }

    private void Redraw()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < _bgs.Count; i++)
        {
            bool has = _kinds != null && i < _kinds.Count && i < MaxRows;
            _bgs[i].gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }
            bool on = i == _cursor;
            _bgs[i].color = on ? Color.white : new Color(1f, 1f, 1f, 0f);
            bool blocked = _rowReasons != null && _rowReasons[i] != null;
            _labels[i].color = blocked ? new Color(0.55f, 0.55f, 0.55f) : on ? theme.unique : Color.white;
            _labels[i].text = (on ? "> " : "  ") + (_rowLabels != null ? _rowLabels[i] : NpcMenuRules.Label(_kinds[i]));
        }
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 81;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _root = new GameObject("Root", typeof(RectTransform));
        _root.transform.SetParent(transform, false);
        var rr = (RectTransform)_root.transform;
        rr.anchorMin = Vector2.zero;
        rr.anchorMax = Vector2.one;
        rr.offsetMin = rr.offsetMax = Vector2.zero;

        Image panel = theme.NewPanel("Panel", _root.transform);
        _panel = panel.rectTransform;
        _panel.anchorMin = _panel.anchorMax = _panel.pivot = new Vector2(0.5f, 0.5f);
        _panel.anchoredPosition = new Vector2(0f, 60f);
        _panel.sizeDelta = new Vector2(560f, 400f);

        _title = NewText("Title", _panel, theme, 30f);
        var tr = _title.rectTransform;
        tr.anchorMin = new Vector2(0f, 1f);
        tr.anchorMax = new Vector2(1f, 1f);
        tr.pivot = new Vector2(0.5f, 1f);
        tr.anchoredPosition = new Vector2(0f, -16f);
        tr.sizeDelta = new Vector2(0f, 50f);
        _title.alignment = TextAlignmentOptions.Center;
        _title.color = theme.unique;

        for (int i = 0; i < MaxRows; i++)
        {
            Image row = theme.NewPanel("Row" + i, _panel, true, 0f);
            var rt = row.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(24f, -70f - i * RowHeight);
            rt.sizeDelta = new Vector2(512f, 48f);
            TextMeshProUGUI text = NewText("Text", row.transform, theme, 28f);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(14f, 0f);
            text.rectTransform.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            _bgs.Add(row);
            _labels.Add(text);
        }
        _root.SetActive(false);
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
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        return t;
    }
}
