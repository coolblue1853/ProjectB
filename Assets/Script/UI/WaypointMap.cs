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
    int cursor, scroll;
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
            MapInfo info = null;
            foreach (MapInfo m in FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (m.gameObject.name == mapName) { info = m; break; }
            }
            if (visited.Count > 1 && info != null && !string.IsNullOrEmpty(info.zoneTitle))
            {
                PickupLog.Report(Loc.T("Discovered: ") + Loc.T(info.zoneTitle), 1, "R"); // 첫 시작 맵은 제외
            }
            GameLog.Info(GameLog.Cat.Map, "구역 발견: " + mapName);
        }
    }

    public static bool IsVisited(string mapName) => instance != null && instance.visited.Contains(mapName);

    void Update()
    {
        Keyboard kb = Keyboard.current;
        PadFieldInput.Refresh();
        if (((kb != null && kb.mKey.wasPressedThisFrame) || PadFieldInput.LtStart) && (open || !DatabaseManager.isOpenUI))
        {
            if (open) Close(); else Open();
            return;
        }
        if (!open || Time.unscaledTime - openedAt < 0.15f)
        {
            return;
        }
        if (UiLock.TryBack("Waypoint", back.triggered))
        {
            Close();
            return;
        }
        if (entries.Count == 0)
        {
            return;
        }
        if (kb != null && kb.homeKey.wasPressedThisFrame)
        {
            SelectVillage(); // 마을 귀환 바로가기 (HOME)
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

    /// <summary>목록에서 마을(floor 0) 칸으로 커서를 옮기고 이동한다. 이미 마을이면 닫기만 한다.</summary>
    private void SelectVillage()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].map.floor <= 0)
            {
                cursor = i;
                Travel();
                return;
            }
        }
    }

    /// <summary>이 구역 포털이 이어 주는 구역 목록(연결도). 미방문 구역은 ???, 봉인 포털은 [봉인]. 같은 맵 안 이동 포털은 건너뛴다.</summary>
    private string BuildConnections(MapInfo map, UiTheme theme)
    {
        var lines = new List<string>();
        var seen = new HashSet<string>();
        foreach (Portal p in map.GetComponentsInChildren<Portal>(true))
        {
            if (p.openMap == null || p.openMap == map.gameObject)
            {
                continue;
            }
            MapInfo dest = p.openMap.GetComponent<MapInfo>();
            if (dest == null || string.IsNullOrEmpty(dest.zoneTitle) || !seen.Add(dest.gameObject.name))
            {
                continue;
            }
            bool known = visited.Contains(dest.gameObject.name);
            PortalSeal seal = p.GetComponent<PortalSeal>();
            bool sealedPath = seal != null && !seal.IsOpen;
            string title = known ? Loc.T(dest.zoneTitle).ToUpperInvariant() : "???";
            lines.Add("<color=#9ED0FF>></color> " + title + (sealedPath ? " <color=#" + ColorUtility.ToHtmlStringRGB(theme.warning) + ">" + "[" + Loc.T("Sealed") + "]" + "</color>" : ""));
        }
        return lines.Count == 0 ? "" : "\n\n<size=80%>" + Loc.T("Connections") + "\n" + string.Join("\n", lines) + "</size>";
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
        // 목록이 줄 수보다 길면 커서가 보이도록 창을 민다(구역이 9개를 넘으면 아래가 잘리던 문제)
        scroll = Mathf.Clamp(scroll, Mathf.Max(0, cursor - rows.Count + 1), cursor);
        scroll = Mathf.Clamp(scroll, 0, Mathf.Max(0, entries.Count - rows.Count));
        for (int i = 0; i < rows.Count; i++)
        {
            int idx = scroll + i;
            bool has = idx < entries.Count;
            rows[i].bg.gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }
            Entry e = entries[idx];
            bool here = e.map.gameObject.activeInHierarchy;
            string name = e.visited ? Loc.T(e.map.zoneTitle).ToUpperInvariant() : "???";
            string floor = e.map.floor <= 0 ? Loc.T("VILLAGE") : Loc.IsKorean ? e.map.floor + Loc.T("FLOOR") : "FLOOR " + e.map.floor;
            string col = !e.visited ? "#777777" : here ? "#" + ColorUtility.ToHtmlStringRGB(theme.unique) : "#FFFFFF";
            rows[i].text.text = "<color=#9ED0FF>" + floor + "</color>  <color=" + col + ">" + name + "</color>" + (here ? "  <size=70%>" + Loc.T("(HERE)") + "</size>" : "");
            rows[i].bg.color = idx == cursor ? Color.white : new Color(1f, 1f, 1f, 0f);
        }
        if (entries.Count == 0)
        {
            detail.text = "";
            return;
        }
        Entry cur = entries[cursor];
        int d = AscentLoad.DeficitOf(cur.map.ascentLoad, AscentLoad.Rank);
        detail.text = cur.visited
            ? Loc.T(cur.map.zoneTitle) + "\n\n" + Loc.T("Ascent load") + "  " + cur.map.ascentLoad + "\n" + Loc.T("Your resistance") + "  " + AscentLoad.Resistance + "\n"
              + (d > 0 ? "<color=#" + ColorUtility.ToHtmlStringRGB(theme.warning) + ">" + Loc.F("Deficit {0}  (-{1}% damage)", d, Mathf.RoundToInt(d * AscentLoad.PenaltyPerPoint * 100f)) + "</color>"
                       : "<color=#8CFF8C>" + Loc.T("No ascent penalty") + "</color>")
              + BuildConnections(cur.map, theme)
              + "\n\n<size=80%>" + Loc.T("SELECT  travel here") + "</size>"
            : Loc.T("Undiscovered.") + "\n" + Loc.T("Reach it on foot to unlock the waypoint.");
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
        head.text = Loc.T("TOWER MAP");
        head.color = theme.unique;
        var hint = NewText("Hint", pr, theme, 22f);
        Place(hint.rectTransform, new Vector2(30f, -520f), new Vector2(940f, 30f));
        hint.text = Loc.T("UP/DOWN select    SELECT travel    HOME village    M / ESC close");
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
