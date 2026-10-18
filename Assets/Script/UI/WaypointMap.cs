using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 층계 지도 / 웨이포인트 (M 키): 방문한 구역 목록을 층계별로 보여 주고 선택하면 즉시 이동한다(디아블로 웨이포인트 + 메이플 월드맵).
/// 방문 기록은 ES3 "zones.visited"(맵 이름 목록). 각 줄: FLOOR n · 구역명 · 부하/저항. 미방문은 ??? 잠금.
/// </summary>
public class WaypointMap : MonoBehaviour
{
    const string VisitedKey = "zones.visited";
    static WaypointMap instance;

    class Entry
    {
        public MapInfo map;
        public bool visited;
    }

    readonly List<Entry> entries = new List<Entry>();
    HashSet<string> visited = new HashSet<string>();
    readonly List<(Image bg, TextMeshProUGUI text)> rows = new List<(Image, TextMeshProUGUI)>();
    KeyAction action;
    InputAction up, down, select, back;
    GameObject panel;
    TextMeshProUGUI detail;
    int cursor;
    bool open;
    float openedAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[WaypointMap]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<WaypointMap>();
    }

    void Awake()
    {
        action = new KeyAction();
        up = action.UI.UPInventory;
        down = action.UI.DownInventory;
        select = action.UI.Select;
        back = action.UI.Back;
        if (ES3.KeyExists(VisitedKey))
        {
            visited = new HashSet<string>(ES3.Load<List<string>>(VisitedKey));
        }
        Portal.MapChanged -= OnMapChanged;
        Portal.MapChanged += OnMapChanged;
    }

    void Start()
    {
        // 시작 맵을 방문 처리
        foreach (MapInfo m in FindObjectsByType<MapInfo>(FindObjectsSortMode.None))
        {
            if (m.gameObject.activeInHierarchy)
            {
                MarkVisited(m.gameObject.name);
            }
        }
    }

    void OnDestroy()
    {
        Portal.MapChanged -= OnMapChanged;
        action?.Dispose();
    }

    void OnMapChanged(GameObject map)
    {
        if (map != null)
        {
            MarkVisited(map.name);
        }
    }

    void MarkVisited(string mapName)
    {
        if (visited.Add(mapName))
        {
            ES3.Save(VisitedKey, new List<string>(visited));
            GameLog.Info(GameLog.Cat.Map, "구역 발견: " + mapName);
        }
    }

    public static bool IsVisited(string mapName) => instance != null && instance.visited.Contains(mapName);

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.mKey.wasPressedThisFrame && (open || !DatabaseManager.isOpenUI))
        {
            if (open) Close(); else Open();
            return;
        }
        if (!open || Time.unscaledTime - openedAt < 0.15f)
        {
            return;
        }
        if (back.triggered)
        {
            Close();
            return;
        }
        if (entries.Count == 0)
        {
            return;
        }
        if (up.triggered)
        {
            cursor = (cursor - 1 + entries.Count) % entries.Count;
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
        else if (down.triggered)
        {
            cursor = (cursor + 1) % entries.Count;
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
        else if (select.triggered)
        {
            Travel();
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
        Collect();
        open = true;
        openedAt = Time.unscaledTime;
        UiLock.Set("Waypoint", true);
        action.UI.Enable();
        panel.SetActive(true);
        Redraw();
        Sfx.Play(SfxId.UiOpen);
    }

    void Close()
    {
        open = false;
        UiLock.Set("Waypoint", false);
        action.UI.Disable();
        panel.SetActive(false);
    }

    void Collect()
    {
        entries.Clear();
        var maps = new List<MapInfo>(FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        maps.Sort((a, b) => a.floor != b.floor ? a.floor.CompareTo(b.floor) : a.ascentLoad.CompareTo(b.ascentLoad));
        foreach (MapInfo m in maps)
        {
            if (!string.IsNullOrEmpty(m.zoneTitle))
            {
                entries.Add(new Entry { map = m, visited = visited.Contains(m.gameObject.name) });
            }
        }
        cursor = Mathf.Clamp(cursor, 0, Mathf.Max(0, entries.Count - 1));
    }

    public static bool TravelTo(string mapName)
    {
        if (instance == null || !instance.visited.Contains(mapName) || PlayerController.instance == null)
        {
            return false;
        }
        return MapInfo.GoToMap(mapName, PlayerController.instance.gameObject);
    }

    void Travel()
    {
        Entry e = entries[cursor];
        if (!e.visited)
        {
            Sfx.Play(SfxId.CraftFail, 0.5f);
            return;
        }
        if (e.map.gameObject.activeInHierarchy)
        {
            Close();
            return;
        }
        Close();
        Sfx.Play(SfxId.Portal);
        MapInfo.GoToMap(e.map.gameObject.name, PlayerController.instance.gameObject);
    }

    void Redraw()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < rows.Count; i++)
        {
            bool has = i < entries.Count;
            rows[i].bg.gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }
            Entry e = entries[i];
            bool here = e.map.gameObject.activeInHierarchy;
            string name = e.visited ? e.map.zoneTitle.ToUpperInvariant() : "???";
            string floor = e.map.floor <= 0 ? "VILLAGE" : "FLOOR " + e.map.floor;
            string col = !e.visited ? "#777777" : here ? "#" + ColorUtility.ToHtmlStringRGB(theme.unique) : "#FFFFFF";
            rows[i].text.text = "<color=#9ED0FF>" + floor + "</color>  <color=" + col + ">" + name + "</color>" + (here ? "  <size=70%>(HERE)</size>" : "");
            rows[i].bg.color = i == cursor ? Color.white : new Color(1f, 1f, 1f, 0f);
        }
        if (entries.Count == 0)
        {
            detail.text = "";
            return;
        }
        Entry cur = entries[cursor];
        int d = AscentLoad.DeficitOf(cur.map.ascentLoad, AscentLoad.Rank);
        detail.text = cur.visited
            ? cur.map.zoneTitle + "\n\nAscent load  " + cur.map.ascentLoad + "\nYour resistance  " + AscentLoad.Resistance + "\n" + (d > 0 ? "<color=#" + ColorUtility.ToHtmlStringRGB(theme.warning) + ">Deficit " + d + "  (-" + Mathf.RoundToInt(d * AscentLoad.PenaltyPerPoint * 100f) + "% damage)</color>" : "<color=#8CFF8C>No ascent penalty</color>") + "\n\n<size=80%>SELECT  travel here</size>"
            : "Undiscovered.\nReach it on foot to unlock the waypoint.";
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 70;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        panel = theme.NewPanel("Panel", transform).gameObject;
        var pr = (RectTransform)panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1000f, 560f);

        var head = NewText("Title", pr, theme, 40f);
        Place(head.rectTransform, new Vector2(30f, -18f), new Vector2(600f, 54f));
        head.text = "TOWER MAP";
        head.color = theme.unique;
        var hint = NewText("Hint", pr, theme, 22f);
        Place(hint.rectTransform, new Vector2(30f, -520f), new Vector2(940f, 30f));
        hint.text = "UP/DOWN select    SELECT travel    M / BACK close";
        hint.color = new Color(1f, 1f, 1f, 0.55f);

        for (int r = 0; r < 9; r++)
        {
            Image bg = theme.NewPanel("Row" + r, pr, true, 0f);
            Place(bg.rectTransform, new Vector2(30f, -86f - r * 44f), new Vector2(520f, 40f));
            var text = NewText("Text", bg.transform, theme, 26f);
            Place(text.rectTransform, new Vector2(14f, 0f), new Vector2(500f, 40f));
            text.rectTransform.pivot = new Vector2(0f, 1f);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            rows.Add((bg, text));
        }
        detail = NewText("Detail", pr, theme, 26f);
        Place(detail.rectTransform, new Vector2(590f, -86f), new Vector2(380f, 380f));
        detail.alignment = TextAlignmentOptions.TopLeft;
        detail.enableWordWrapping = true;
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
