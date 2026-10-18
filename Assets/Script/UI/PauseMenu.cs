using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 일시정지 메뉴 (Esc, 다른 창이 없을 때): 계속하기 · 층계 지도 · 도감 · 마을로 귀환 · 게임 종료.
/// 열리면 Time.timeScale 0 + UiLock "Pause". 코드 생성 UI(공용 픽셀 프레임).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    static PauseMenu instance;

    readonly string[] items = { "RESUME", "TOWER MAP", "BESTIARY", "RETURN TO VILLAGE", "QUIT GAME" };
    readonly List<(Image bg, TextMeshProUGUI text)> rows = new List<(Image, TextMeshProUGUI)>();
    KeyAction action;
    InputAction up, down, select;
    GameObject panel;
    CanvasGroup dim;
    int cursor;
    bool open;
    float openedAt, savedScale = 1f;

    public static bool IsOpen => instance != null && instance.open;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[PauseMenu]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<PauseMenu>();
    }

    void Awake()
    {
        action = new KeyAction();
        up = action.UI.UPInventory;
        down = action.UI.DownInventory;
        select = action.UI.Select;
    }

    void OnDestroy()
    {
        action?.Dispose();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }
        bool esc = kb.escapeKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (!open)
        {
            if (esc && !DatabaseManager.isOpenUI)
            {
                Open();
            }
            return;
        }
        if (Time.unscaledTime - openedAt < 0.15f)
        {
            return;
        }
        if (esc)
        {
            Close();
            return;
        }
        if (up.triggered)
        {
            cursor = (cursor - 1 + items.Length) % items.Length;
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
        else if (down.triggered)
        {
            cursor = (cursor + 1) % items.Length;
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
        else if (select.triggered || kb.enterKey.wasPressedThisFrame)
        {
            Activate();
        }
    }

    public static void OpenNow()
    {
        if (instance != null && !instance.open)
        {
            instance.Open();
        }
    }

    void Open()
    {
        if (panel == null)
        {
            Build();
        }
        open = true;
        openedAt = Time.unscaledTime;
        cursor = 0;
        savedScale = Time.timeScale;
        Time.timeScale = 0f;
        UiLock.Set("Pause", true);
        action.UI.Enable();
        panel.SetActive(true);
        dim.gameObject.SetActive(true);
        Redraw();
        Sfx.Play(SfxId.UiOpen);
        GameLog.Info(GameLog.Cat.Player, "일시정지");
    }

    void Close()
    {
        open = false;
        Time.timeScale = savedScale <= 0f ? 1f : savedScale;
        UiLock.Set("Pause", false);
        action.UI.Disable();
        panel.SetActive(false);
        dim.gameObject.SetActive(false);
    }

    void Activate()
    {
        switch (cursor)
        {
            case 0:
                Close();
                break;
            case 1:
                Close();
                WaypointMap.OpenNow();
                break;
            case 2:
                Close();
                BestiaryWindow.Toggle();
                break;
            case 3:
                Close();
                if (PlayerController.instance != null)
                {
                    Sfx.Play(SfxId.Portal);
                    MapInfo.GoToTown(PlayerController.instance.gameObject);
                }
                break;
            case 4:
                GameLog.Info(GameLog.Cat.Save, "게임 종료 (일시정지 메뉴)");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    void Redraw()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].bg.color = i == cursor ? Color.white : new Color(1f, 1f, 1f, 0f);
            rows[i].text.color = i == cursor ? theme.unique : Color.white;
            rows[i].text.text = (i == cursor ? "> " : "  ") + items[i];
        }
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        dimGo.transform.SetParent(transform, false);
        var dr = (RectTransform)dimGo.transform;
        dr.anchorMin = Vector2.zero;
        dr.anchorMax = Vector2.one;
        dr.offsetMin = dr.offsetMax = Vector2.zero;
        var dimImg = dimGo.GetComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0.02f, 0.6f);
        dimImg.raycastTarget = false;
        dim = dimGo.GetComponent<CanvasGroup>();
        dimGo.SetActive(false);

        panel = theme.NewPanel("Panel", transform).gameObject;
        var pr = (RectTransform)panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(520f, 440f);

        var head = NewText("Title", pr, theme, 40f);
        Place(head.rectTransform, new Vector2(0f, -22f), new Vector2(520f, 50f));
        head.alignment = TextAlignmentOptions.Center;
        head.text = "PAUSED";
        head.color = theme.unique;

        for (int i = 0; i < items.Length; i++)
        {
            Image bg = theme.NewPanel("Row" + i, pr, true, 0f);
            Place(bg.rectTransform, new Vector2(40f, -96f - i * 56f), new Vector2(440f, 48f));
            var text = NewText("Text", bg.transform, theme, 30f);
            Place(text.rectTransform, new Vector2(16f, 0f), new Vector2(410f, 48f));
            text.alignment = TextAlignmentOptions.MidlineLeft;
            rows.Add((bg, text));
        }
        var hint = NewText("Hint", pr, theme, 20f);
        Place(hint.rectTransform, new Vector2(0f, -400f), new Vector2(520f, 28f));
        hint.alignment = TextAlignmentOptions.Center;
        hint.text = "UP/DOWN  ·  SELECT / ENTER  ·  ESC resume";
        hint.color = new Color(1f, 1f, 1f, 0.55f);
        panel.SetActive(false);
    }

    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size)
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
