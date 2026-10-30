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

    readonly string[] items = { "RESUME", "TOWER MAP", "BESTIARY", "SETTINGS", "RETURN TO VILLAGE", "QUIT GAME" };
    readonly string[] settingItems = { "MASTER VOLUME", "SCREEN SHAKE", "HIT FLASH", "LANGUAGE", "PICKUP", "BACK" };
    bool settingsMode;
    readonly List<(Image bg, TextMeshProUGUI text)> rows = new List<(Image, TextMeshProUGUI)>();
    KeyAction action;
    InputAction up, down, left, right, select;
    GameObject panel;
    CanvasGroup dim;
    TextMeshProUGUI title, hint;
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
        left = action.UI.LeftInventory;
        right = action.UI.RightInventory;
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
            if (settingsMode) { settingsMode = false; cursor = 3; Redraw(); }
            else Close();
            return;
        }
        int count = settingsMode ? settingItems.Length : items.Length;
        if (settingsMode && (left.triggered || right.triggered))
        {
            Adjust(right.triggered ? 1 : -1);
            return;
        }
        if (up.triggered)
        {
            cursor = (cursor - 1 + count) % count;
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
        else if (down.triggered)
        {
            cursor = (cursor + 1) % count;
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
        settingsMode = false;
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

    void Adjust(int dir)
    {
        switch (cursor)
        {
            case 0: GameSettings.SetVolume(GameSettings.Volume + 0.1f * dir); break;
            case 1: GameSettings.SetShake(GameSettings.Shake + 0.25f * dir); break;
            case 2: GameSettings.SetFlash(!GameSettings.Flash); break;
            case 3: Loc.Toggle(); break; // 한국어 ↔ English (I2, Q-407)
            case 4: GameSettings.SetPickup(PickupRules.Next(GameSettings.Pickup)); break; // 줍기 필터 (BasicFeatures 4절)
        }
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    void Activate()
    {
        if (settingsMode)
        {
            if (cursor == settingItems.Length - 1) { settingsMode = false; cursor = 3; Redraw(); }
            else Adjust(1);
            return;
        }
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
                settingsMode = true;
                cursor = 0;
                Redraw();
                break;
            case 4:
                Close();
                if (PlayerController.instance != null)
                {
                    Sfx.Play(SfxId.Portal);
                    MapInfo.GoToTown(PlayerController.instance.gameObject);
                }
                break;
            case 5:
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
        string[] list = settingsMode ? settingItems : items;
        title.text = Loc.T(settingsMode ? "SETTINGS" : "PAUSED");
        for (int i = 0; i < rows.Count; i++)
        {
            bool has = i < list.Length;
            rows[i].bg.gameObject.SetActive(has);
            if (!has) continue;
            string value = "";
            if (settingsMode)
            {
                value = i == 0 ? "  " + Mathf.RoundToInt(GameSettings.Volume * 100f) + "%" : i == 1 ? "  " + Mathf.RoundToInt(GameSettings.Shake * 100f) + "%"
                    : i == 2 ? "  " + Loc.T(GameSettings.Flash ? "ON" : "OFF") : i == 3 ? (Loc.IsKorean ? "  한국어" : "  ENGLISH")
                    : i == 4 ? "  " + Loc.T(GameSettings.Pickup == PickupFilter.All ? "ALL" : "SKIP COMMON MAT.") : "";
            }
            rows[i].bg.color = i == cursor ? Color.white : new Color(1f, 1f, 1f, 0f);
            rows[i].text.color = i == cursor ? theme.unique : Color.white;
            rows[i].text.text = (i == cursor ? "> " : "  ") + Loc.T(list[i]) + "<color=#9ED0FF>" + value + "</color>";
        }
        hint.text = Loc.T(settingsMode ? "UP/DOWN  ·  LEFT/RIGHT adjust  ·  ESC back" : "UP/DOWN  ·  SELECT / ENTER  ·  ESC resume");
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
        pr.sizeDelta = new Vector2(520f, 490f);

        title = NewText("Title", pr, theme, 40f);
        var head = title;
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
        hint = NewText("Hint", pr, theme, 20f);
        Place(hint.rectTransform, new Vector2(0f, -450f), new Vector2(520f, 28f));
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
