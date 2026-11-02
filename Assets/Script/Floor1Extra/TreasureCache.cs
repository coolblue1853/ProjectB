using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>숨은 보물 상자 한 곳 (Resources/Floor1Treasures.csv: id,map,x,y,gold,exp,item,count,hint)</summary>
public sealed class TreasureDef
{
    public string Id;
    public string Map;
    public Vector2 Position;
    public Floor1Data.Reward Reward;
    public string Hint;
}

/// <summary>상자 열림 기록 (ES3 "f1.caches" — 추가 전용 키, 한 번 연 상자는 다시 열리지 않는다)</summary>
public static class TreasureLedger
{
    public const string SaveKey = "f1.caches";
    private static HashSet<string> s_opened;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_opened = null;
    }

    private static HashSet<string> Opened
    {
        get
        {
            if (s_opened == null)
            {
                s_opened = new HashSet<string>(ES3.KeyExists(SaveKey) ? ES3.Load<List<string>>(SaveKey) : new List<string>());
            }
            return s_opened;
        }
    }

    public static bool IsOpened(string id)
    {
        return Opened.Contains(id);
    }

    public static void MarkOpened(string id)
    {
        if (Opened.Add(id))
        {
            ES3.Save(SaveKey, new List<string>(Opened));
        }
    }

    public static List<TreasureDef> Parse(string text)
    {
        var list = new List<TreasureDef>();
        foreach (string[] c in Floor1Data.Rows(text))
        {
            if (c.Length < 9 || c[0].Trim().Length == 0)
            {
                continue;
            }
            list.Add(new TreasureDef
            {
                Id = c[0].Trim(),
                Map = c[1].Trim(),
                Position = new Vector2(Floor1Data.Float(c[2]), Floor1Data.Float(c[3])),
                Reward = new Floor1Data.Reward
                {
                    Gold = Floor1Data.Int(c[4]),
                    Exp = Floor1Data.Int(c[5]),
                    Item = c[6].Trim(),
                    Count = Floor1Data.Int(c[7]),
                    Label = c[0].Trim(),
                },
                Hint = c[8].Trim(),
            });
        }
        return list;
    }
}

/// <summary>
/// 숨은 보물 상자: 맵 끝자락에 숨겨 둔 1회성 상자. 가까이 가면 "↑ 상자 열기", 열면 골드·EXP·재료. 가방이 가득 차면 열리지 않는다.
/// 켜고 끄기: Floor1Content.csv 의 treasure. 그림은 코드 도트 임시(Floor1ContentList.md).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class TreasureCache : MonoBehaviour
{
    private TreasureDef _def;
    private SpriteRenderer _sr;
    private TextMeshPro _prompt;
    private bool _near;
    private bool _opened;

    public string Id => _def != null ? _def.Id : null;
    public bool Opened => _opened;

    public void Init(TreasureDef def, SpriteRenderer sr)
    {
        _def = def;
        _sr = sr;
        _opened = TreasureLedger.IsOpened(def.Id);
        _sr.sprite = _opened ? TreasureSprites.Open : TreasureSprites.Closed;
    }

    private void Awake()
    {
        var go = new GameObject("CachePrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        _prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme != null && theme.pixelFont != null)
        {
            _prompt.font = theme.pixelFont;
        }
        _prompt.text = "↑ 상자 열기";
        _prompt.fontSize = 3f;
        _prompt.alignment = TextAlignmentOptions.Center;
        _prompt.color = theme != null ? theme.unique : Color.yellow;
        _prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        _prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    private void OnEnable()
    {
        Floor1Input.Acquire();
    }

    private void OnDisable()
    {
        Floor1Input.Release();
        _near = false;
    }

    private void Update()
    {
        bool show = _near && !_opened;
        if (_prompt.gameObject.activeSelf != show)
        {
            _prompt.gameObject.SetActive(show);
        }
        if (show && Floor1Input.UpPressed && !DatabaseManager.isOpenUI)
        {
            TryOpen();
        }
    }

    /// <summary>열기 시도. 열렸으면 true. 가방이 가득 차면 안내만 띄우고 닫아 둔다</summary>
    public bool TryOpen()
    {
        if (_opened || _def == null)
        {
            return false;
        }
        if (!_def.Reward.CanReceive())
        {
            PickupLog.Warn("가방 공간이 부족해 상자를 열 수 없습니다");
            return false;
        }
        _opened = true;
        TreasureLedger.MarkOpened(_def.Id);
        _sr.sprite = TreasureSprites.Open;
        _def.Reward.Apply();
        PickupLog.Notice("숨은 상자를 열었다: " + _def.Reward.Describe());
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _near = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _near = false;
        }
    }
}

/// <summary>상자 코드 도트 (20×14, PPU 16): 닫힘 = 어두운 나무 + 금속 띠, 열림 = 뚜껑이 젖혀진 빈 상자</summary>
public static class TreasureSprites
{
    private static Sprite s_closed;
    private static Sprite s_open;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_closed = null;
        s_open = null;
    }

    public static Sprite Closed => s_closed != null ? s_closed : (s_closed = Make(false));
    public static Sprite Open => s_open != null ? s_open : (s_open = Make(true));

    private static Sprite Make(bool open)
    {
        const int w = 20;
        const int h = 14;
        var wood = new Color32(98, 70, 48, 255);
        var woodDark = new Color32(62, 44, 34, 255);
        var band = new Color32(140, 112, 64, 255);
        var inside = new Color32(24, 20, 22, 255);
        Color32[] px = MiniGameSprites.Blank(w * h);
        for (int y = 0; y < 7; y++)
        {
            for (int x = 1; x < 19; x++)
            {
                px[y * w + x] = (y == 0 || x == 1 || x == 18) ? woodDark : wood;
            }
        }
        if (open)
        {
            for (int x = 2; x < 18; x++)
            {
                px[6 * w + x] = inside;
            }
            for (int y = 7; y < 13; y++)
            {
                for (int x = 3; x < 17; x++)
                {
                    px[y * w + x] = y == 7 ? woodDark : wood;
                }
            }
        }
        else
        {
            for (int y = 7; y < 11; y++)
            {
                int inset = y >= 9 ? y - 8 : 0;
                for (int x = 1 + inset; x < 19 - inset; x++)
                {
                    px[y * w + x] = y == 7 ? woodDark : wood;
                }
            }
        }
        for (int y = 0; y < (open ? 7 : 11); y++)
        {
            px[y * w + 5] = band;
            px[y * w + 14] = band;
        }
        if (!open)
        {
            for (int y = 5; y < 8; y++)
            {
                px[y * w + 9] = band;
                px[y * w + 10] = band;
            }
        }
        return MiniGameSprites.MakeSprite(px, w, h);
    }
}

/// <summary>상자 배치: 씬 파일을 고치지 않고 부트 때 맵 오브젝트 아래에 만든다 (TrackingSpotPlacer 와 같은 방식)</summary>
public static class TreasureCachePlacer
{
    private const string ResourceName = "Floor1Treasures";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (!Floor1Data.On("treasure"))
        {
            return;
        }
        TextAsset csv = Resources.Load<TextAsset>(ResourceName);
        if (csv == null)
        {
            return;
        }
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TreasureDef d in TreasureLedger.Parse(csv.text))
        {
            MapInfo map = System.Array.Find(maps, m => m.name == d.Map);
            if (map == null)
            {
                Debug.LogWarning("[PB][Floor1] 상자 맵 없음: " + d.Map);
                continue;
            }
            Create(map.transform, d);
        }
    }

    private static void Create(Transform parent, TreasureDef d)
    {
        var go = new GameObject("TreasureCache_" + d.Id);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(d.Position.x, d.Position.y, parent.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(3f, 2.5f);
        box.offset = new Vector2(0f, 1f);
        var art = new GameObject("Art");
        art.transform.SetParent(go.transform, false);
        art.transform.localPosition = Vector3.zero;
        var sr = art.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20;
        go.AddComponent<TreasureCache>().Init(d, sr);
        go.AddComponent<GroundSnapOnce>();
    }
}
