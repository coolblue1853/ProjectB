using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// 메이플식 미니맵(BasicFeatures.md 9절): 오른쪽 위에 지금 맵의 지형 실루엣(Ground 콜라이더·타일을 1유닛=1픽셀로 구운 텍스처)과
/// 플레이어(노랑)·포털(하늘)·NPC(초록)·보스(빨강) 점, 맵 이름. N 으로 켜고 끈다(ES3 settings.minimap, 공용 파일).
/// 지형·고정 점은 맵이 바뀔 때만(Portal.MapChanged) 다시 만들고, 플레이어 점은 매 프레임, 보스 점은 1초마다 갱신한다.
/// </summary>
public class MinimapHud : MonoBehaviour
{
    public const string SaveKey = "settings.minimap";
    private const int MaxTexSide = 512; // 1유닛=1픽셀, 더 큰 맵은 줄여 굽는다
    private static readonly Vector2 MaxSize = new Vector2(340f, 150f);
    private const float BossRefresh = 1f;

    private static MinimapHud _instance;

    private GameObject _root;
    private RawImage _terrain;
    private TextMeshProUGUI _title;
    private RectTransform _dotLayer;
    private RectTransform _playerDot;
    private Texture2D _tex;
    private Rect _area;
    private GameObject _map;
    private bool _visible = true;
    private float _bossTimer;
    private readonly List<RectTransform> _staticDots = new List<RectTransform>();
    private readonly List<RectTransform> _bossDots = new List<RectTransform>();
    private readonly List<Transform> _bosses = new List<Transform>();

    /// <summary>미니맵 패널 높이(켜져 있을 때). 아래에 붙는 HUD(제작 핀)가 위치를 맞춘다</summary>
    public static float OccupiedHeight => _instance != null && _instance._visible && _instance._root.activeSelf ? ((RectTransform)_instance._root.transform).sizeDelta.y + 8f : 0f;

    public static Rect Area => _instance != null ? _instance._area : default(Rect);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[MinimapHud]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<MinimapHud>();
    }

    private void Awake()
    {
        try
        {
            _visible = !ES3.KeyExists(SaveKey, SaveSlots.SharedFile) || ES3.Load<bool>(SaveKey, SaveSlots.SharedFile);
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "미니맵 설정 읽기 실패: " + e.Message);
        }
        Build();
        Portal.MapChanged += OnMapChanged;
    }

    private void Start()
    {
        OnMapChanged(FindStartMap()); // 시작 맵(포털 이벤트 없음)
    }

    private void OnDestroy()
    {
        Portal.MapChanged -= OnMapChanged;
        if (_tex != null)
        {
            Destroy(_tex);
        }
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>시작 시 지금 맵: 플레이어와 높이가 가장 가까운 활성 맵(MapInfo.Hook 과 같은 규칙)</summary>
    private static GameObject FindStartMap()
    {
        GameObject root = GameObject.Find("Map");
        PlayerController player = PlayerController.instance;
        if (root == null || player == null)
        {
            return null;
        }
        GameObject best = null;
        float bestDy = float.MaxValue;
        foreach (Transform m in root.transform)
        {
            float dy = Mathf.Abs(m.position.y - player.transform.position.y);
            if (m.gameObject.activeInHierarchy && m.GetComponent<MapInfo>() != null && dy < bestDy)
            {
                bestDy = dy;
                best = m.gameObject;
            }
        }
        return best;
    }

    private void OnMapChanged(GameObject map)
    {
        _map = map;
        BakeTerrain();
        BuildStaticDots();
        _bossTimer = 0f;
        MapInfo info = map != null ? map.GetComponent<MapInfo>() : null;
        _title.text = info != null && !string.IsNullOrEmpty(info.displayName) ? Loc.T(info.displayName) : "";
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.nKey.wasPressedThisFrame && !DatabaseManager.isOpenUI)
        {
            SetVisible(!_visible);
        }
        bool show = _visible && _map != null && !DatabaseManager.isOpenUI;
        if (_root.activeSelf != show)
        {
            _root.SetActive(show);
        }
        if (!show)
        {
            return;
        }
        PlayerController player = PlayerController.instance;
        if (player != null)
        {
            Place(_playerDot, player.transform.position);
        }
        _bossTimer -= Time.unscaledDeltaTime;
        if (_bossTimer <= 0f)
        {
            _bossTimer = BossRefresh;
            RefreshBosses();
        }
    }

    public static void SetVisible(bool visible)
    {
        if (_instance == null)
        {
            return;
        }
        _instance._visible = visible;
        try
        {
            ES3.Save(SaveKey, visible, SaveSlots.SharedFile);
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "미니맵 설정 저장 실패: " + e.Message);
        }
    }

    // ── 지형 ──
    /// <summary>
    /// 지금 맵의 지면(태그 Ground 콜라이더)을 굽는다. 영역 = 크기 있는 지면 콜라이더들의 합(흩어진 장식 타일이 영역을 늘리지 않게),
    /// 타일맵 지면은 칠한 칸을 1칸=1픽셀로, 그 밖의 발판(BoxCollider2D 등)은 경계 사각형으로 칠한다. 지면이 없으면 점만.
    /// </summary>
    private void BakeTerrain()
    {
        var grounds = new List<Collider2D>();
        if (_map != null)
        {
            foreach (Collider2D c in _map.GetComponentsInChildren<Collider2D>(false))
            {
                if (c.CompareTag("Ground") && c.bounds.size.x > 0.5f)
                {
                    grounds.Add(c);
                }
            }
        }
        if (grounds.Count == 0)
        {
            _area = _map != null ? new Rect(_map.transform.position.x - 30f, _map.transform.position.y - 10f, 60f, 25f) : new Rect(0f, 0f, 1f, 1f);
            _terrain.texture = null;
            _terrain.color = new Color(0f, 0f, 0f, 0f);
            Resize(new Vector2(_area.width, _area.height));
            return;
        }
        Bounds wb = grounds[0].bounds;
        foreach (Collider2D c in grounds)
        {
            wb.Encapsulate(c.bounds);
        }
        wb.Expand(new Vector3(2f, 2f, 0f));
        wb.max += new Vector3(0f, 6f, 0f); // 위쪽 여유(점프·발판 위 점이 잘리지 않게)
        _area = new Rect(wb.min.x, wb.min.y, wb.size.x, wb.size.y);
        int w = Mathf.Clamp(Mathf.CeilToInt(wb.size.x), 4, MaxTexSide);
        int h = Mathf.Clamp(Mathf.CeilToInt(wb.size.y), 4, MaxTexSide);
        if (_tex == null || _tex.width != w || _tex.height != h)
        {
            if (_tex != null)
            {
                Destroy(_tex);
            }
            _tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        }
        var pixels = new Color32[w * h];
        var painted = new HashSet<Tilemap>();
        foreach (Collider2D c in grounds)
        {
            Tilemap tm = c.GetComponent<Tilemap>();
            if (tm != null)
            {
                if (painted.Add(tm))
                {
                    PaintTiles(tm, pixels, w, h);
                }
            }
            else
            {
                PaintRect(c.bounds, pixels, w, h);
            }
        }
        _tex.SetPixels32(pixels);
        _tex.Apply(false);
        _terrain.texture = _tex;
        _terrain.color = Color.white;
        Resize(_area.size); // 텍스처가 512 로 잘려도 화면 비율은 월드 크기대로
    }

    private static readonly Color32 Solid = new Color32(200, 196, 180, 230);

    /// <summary>타일 칸을 칠한다. 순회는 타일맵 경계와 미니맵 영역이 겹치는 칸만(멀리 떨어진 장식 타일이 순회를 키우지 않게)</summary>
    private void PaintTiles(Tilemap tm, Color32[] pixels, int w, int h)
    {
        BoundsInt cb = tm.cellBounds;
        Vector3Int lo = tm.WorldToCell(new Vector3(_area.xMin, _area.yMin, 0f));
        Vector3Int hi = tm.WorldToCell(new Vector3(_area.xMax, _area.yMax, 0f));
        int x0 = Mathf.Max(cb.xMin, Mathf.Min(lo.x, hi.x)), x1 = Mathf.Min(cb.xMax, Mathf.Max(lo.x, hi.x) + 1);
        int y0 = Mathf.Max(cb.yMin, Mathf.Min(lo.y, hi.y)), y1 = Mathf.Min(cb.yMax, Mathf.Max(lo.y, hi.y) + 1);
        if ((long)(x1 - x0) * (y1 - y0) > 400000)
        {
            GameLog.Warn(GameLog.Cat.Map, "미니맵: 타일 칸이 너무 많아 건너뜀 " + tm.name);
            return;
        }
        for (int y = y0; y < y1; y++)
        {
            for (int x = x0; x < x1; x++)
            {
                var c = new Vector3Int(x, y, 0);
                if (tm.HasTile(c))
                {
                    SetPixel(tm.GetCellCenterWorld(c), pixels, w, h);
                }
            }
        }
    }

    private void PaintRect(Bounds b, Color32[] pixels, int w, int h)
    {
        Vector2 a = MinimapMath.WorldToUv(b.min, _area);
        Vector2 z = MinimapMath.WorldToUv(b.max, _area);
        int x0 = Mathf.FloorToInt(a.x * w), x1 = Mathf.Max(x0, Mathf.CeilToInt(z.x * w) - 1);
        int y0 = Mathf.FloorToInt(a.y * h), y1 = Mathf.Max(y0, Mathf.CeilToInt(z.y * h) - 1);
        for (int y = Mathf.Max(0, y0); y <= Mathf.Min(h - 1, y1); y++)
        {
            for (int x = Mathf.Max(0, x0); x <= Mathf.Min(w - 1, x1); x++)
            {
                pixels[y * w + x] = Solid;
            }
        }
    }

    private void SetPixel(Vector3 world, Color32[] pixels, int w, int h)
    {
        int x = Mathf.FloorToInt((world.x - _area.xMin) / _area.width * w);
        int y = Mathf.FloorToInt((world.y - _area.yMin) / _area.height * h);
        if (x >= 0 && x < w && y >= 0 && y < h)
        {
            pixels[y * w + x] = Solid;
        }
    }

    private void Resize(Vector2 content)
    {
        Vector2 size = MinimapMath.Fit(content, MaxSize, 40f);
        _terrain.rectTransform.sizeDelta = size;
        var rr = (RectTransform)_root.transform;
        rr.sizeDelta = new Vector2(size.x + 20f, size.y + 44f);
    }

    // ── 점 ──
    private void BuildStaticDots()
    {
        foreach (RectTransform d in _staticDots)
        {
            Destroy(d.gameObject);
        }
        _staticDots.Clear();
        if (_map == null)
        {
            return;
        }
        foreach (Portal p in _map.GetComponentsInChildren<Portal>(false)) // 꺼진(봉인·조건부) 포털은 표시하지 않음
        {
            _staticDots.Add(NewDot(new Color(0.45f, 0.85f, 1f), 7f, p.transform.position));
        }
        foreach (TalkNpc n in _map.GetComponentsInChildren<TalkNpc>(false))
        {
            _staticDots.Add(NewDot(new Color(0.45f, 1f, 0.5f), 6f, n.transform.position));
        }
        foreach (QuestBoard q in _map.GetComponentsInChildren<QuestBoard>(false))
        {
            _staticDots.Add(NewDot(new Color(0.45f, 1f, 0.5f), 6f, q.transform.position));
        }
        _playerDot.SetAsLastSibling();
    }

    /// <summary>
    /// 보스 점: 살아 있는 보스(EnemyHealth.Active 중 isBossMob, HP>0) 가운데 미니맵 영역 안. 1초마다.
    /// 스포너가 몹을 씬 최상위에 만들어서 맵 자식 탐색으로는 못 찾는다(BgmDirector.BossNearPlayer 와 같은 방식)
    /// </summary>
    private void RefreshBosses()
    {
        _bosses.Clear();
        if (_map != null && EnemyHealth.ActiveBossCount > 0)
        {
            foreach (EnemyHealth e in EnemyHealth.Active)
            {
                if (e != null && e.isBossMob && e.nowHP > 0 && _area.Contains(e.transform.position))
                {
                    _bosses.Add(e.transform);
                }
            }
        }
        while (_bossDots.Count < _bosses.Count)
        {
            _bossDots.Add(NewDot(new Color(1f, 0.3f, 0.25f), 9f, Vector3.zero));
        }
        for (int i = 0; i < _bossDots.Count; i++)
        {
            bool on = i < _bosses.Count;
            if (_bossDots[i].gameObject.activeSelf != on)
            {
                _bossDots[i].gameObject.SetActive(on);
            }
            if (on)
            {
                Place(_bossDots[i], _bosses[i].position);
            }
        }
        _playerDot.SetAsLastSibling();
    }

    private RectTransform NewDot(Color color, float size, Vector3 world)
    {
        var img = new GameObject("Dot", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.transform.SetParent(_dotLayer, false);
        img.color = color;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        Place(rt, world);
        return rt;
    }

    private void Place(RectTransform dot, Vector3 world)
    {
        Vector2 uv = MinimapMath.WorldToUv(world, _area);
        Vector2 size = _terrain.rectTransform.sizeDelta;
        dot.anchoredPosition = new Vector2(uv.x * size.x, uv.y * size.y);
    }

    // ── UI ──
    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image panel = theme.NewPanel("Minimap", transform, false, 0.8f);
        _root = panel.gameObject;
        RectTransform pr = panel.rectTransform;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(1f, 1f);
        pr.anchoredPosition = new Vector2(-16f, -16f);
        pr.sizeDelta = MaxSize + new Vector2(20f, 44f);

        _title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        _title.transform.SetParent(pr, false);
        if (theme.pixelFont != null)
        {
            _title.font = theme.pixelFont;
        }
        _title.fontSize = 20f;
        _title.color = theme.unique;
        _title.alignment = TextAlignmentOptions.TopLeft;
        _title.textWrappingMode = TextWrappingModes.NoWrap;
        _title.raycastTarget = false;
        RectTransform tr = _title.rectTransform;
        tr.anchorMin = new Vector2(0f, 1f);
        tr.anchorMax = new Vector2(1f, 1f);
        tr.pivot = new Vector2(0f, 1f);
        tr.anchoredPosition = new Vector2(12f, -6f);
        tr.sizeDelta = new Vector2(-24f, 26f);

        _terrain = new GameObject("Terrain", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        _terrain.transform.SetParent(pr, false);
        _terrain.raycastTarget = false;
        RectTransform terr = _terrain.rectTransform;
        terr.anchorMin = terr.anchorMax = terr.pivot = new Vector2(0.5f, 0f);
        terr.anchoredPosition = new Vector2(0f, 10f);
        terr.sizeDelta = MaxSize;

        _dotLayer = new GameObject("Dots", typeof(RectTransform)).GetComponent<RectTransform>();
        _dotLayer.SetParent(terr, false);
        _dotLayer.anchorMin = Vector2.zero;
        _dotLayer.anchorMax = Vector2.one;
        _dotLayer.offsetMin = _dotLayer.offsetMax = Vector2.zero;

        _playerDot = NewDot(new Color(1f, 0.9f, 0.2f), 9f, Vector3.zero);
        _root.SetActive(false);
    }
}
