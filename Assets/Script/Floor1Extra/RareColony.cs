using System.Collections.Generic;
using UnityEngine;

/// <summary>희귀 군락 한 곳 (Resources/Floor1RareNodes.csv: id,map,x,y,kind,hint). 같은 맵의 후보 중 하나만 그때그때 나타난다</summary>
public sealed class RareNodeDef
{
    public string Id;
    public string Map;
    public Vector2 Position;
    public GatherKind Kind;
    public string Hint;
}

/// <summary>희귀 군락 규칙 (순수 — 테스트 대상). 난수는 호출자가 0~1 값으로 넘긴다</summary>
public static class RareColonyRules
{
    /// <summary>구역에 들어설 때마다 군락이 나타날 확률</summary>
    public const float SpawnChance = 0.2f;
    /// <summary>같은 구역에서 다시 굴리기까지 최소 간격(초) — 들락거려 군락을 뽑는 것을 막는다</summary>
    public const float RollCooldown = 240f;

    public static List<RareNodeDef> Parse(string text)
    {
        var list = new List<RareNodeDef>();
        foreach (string[] c in Floor1Data.Rows(text))
        {
            if (c.Length < 6 || c[0].Trim().Length == 0)
            {
                continue;
            }
            list.Add(new RareNodeDef
            {
                Id = c[0].Trim(),
                Map = c[1].Trim(),
                Position = new Vector2(Floor1Data.Float(c[2]), Floor1Data.Float(c[3])),
                Kind = c[4].Trim() == "Ore" ? GatherKind.Ore : GatherKind.Specimen,
                Hint = c[5].Trim(),
            });
        }
        return list;
    }

    /// <summary>굴리기: 후보 수가 0 이면 -1, 확률에 못 미치면 -1, 아니면 후보 인덱스</summary>
    public static int Pick(int candidates, float chanceRoll01, float pickRoll01, float chance = SpawnChance)
    {
        if (candidates <= 0 || chanceRoll01 >= chance)
        {
            return -1;
        }
        return Mathf.Min(candidates - 1, Mathf.FloorToInt(Mathf.Clamp01(pickRoll01) * candidates));
    }

    public static bool CanRoll(float now, float nextRollAt)
    {
        return now >= nextRollAt;
    }
}

/// <summary>희귀 군락 한 개의 수명: 캐고 쉬는 중이 되면(성공 뒤 쿨타임) 결과 연출이 끝난 다음 사라진다</summary>
public class RareColonyNode : MonoBehaviour
{
    private GatherNode _node;

    public GatherNode Node => _node;

    public void Init(GatherNode node)
    {
        _node = node;
    }

    private void Update()
    {
        if (_node != null && !_node.IsReady && GatherNode.Current != _node)
        {
            gameObject.SetActive(false);
        }
    }
}

/// <summary>
/// 희귀 군락 채집(FloorCoreContent 1층계): 구역에 들어설 때 낮은 확률로 반짝이는 군락이 하나 나타난다. 같은 채집 미니게임이고 보상표만 R/U 가 많다(GatherRules.Rare*).
/// 한 번 캐면 사라진다. 나타나 있는 동안 미니맵에 점(MinimapMarker)이 찍힌다. 끄기: Floor1Content.csv 의 rare.
/// </summary>
public static class RareColonyDirector
{
    private const string ResourceName = "Floor1RareNodes";
    private static readonly Dictionary<string, List<RareNodeDef>> s_defs = new Dictionary<string, List<RareNodeDef>>();
    private static readonly Dictionary<string, List<GameObject>> s_nodes = new Dictionary<string, List<GameObject>>();
    private static readonly Dictionary<string, float> s_nextRoll = new Dictionary<string, float>();
    private static Sprite s_glow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Portal.MapChanged -= OnMapChanged;
        s_defs.Clear();
        s_nodes.Clear();
        s_nextRoll.Clear();
        s_glow = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (!Floor1Data.On("rare"))
        {
            return;
        }
        TextAsset csv = Resources.Load<TextAsset>(ResourceName);
        if (csv == null)
        {
            return;
        }
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (RareNodeDef d in RareColonyRules.Parse(csv.text))
        {
            MapInfo map = System.Array.Find(maps, m => m.name == d.Map);
            if (map == null)
            {
                Debug.LogWarning("[PB][Floor1] 희귀 군락 맵 없음: " + d.Map);
                continue;
            }
            if (!s_defs.TryGetValue(d.Map, out List<RareNodeDef> defs))
            {
                s_defs[d.Map] = defs = new List<RareNodeDef>();
                s_nodes[d.Map] = new List<GameObject>();
            }
            defs.Add(d);
            s_nodes[d.Map].Add(Create(map.transform, d));
        }
        Portal.MapChanged += OnMapChanged;
        GameObject root = GameObject.Find("Map");
        if (root != null)
        {
            foreach (Transform m in root.transform)
            {
                if (m.gameObject.activeSelf)
                {
                    OnMapChanged(m.gameObject);
                }
            }
        }
    }

    private static void OnMapChanged(GameObject map)
    {
        if (map == null || !s_defs.TryGetValue(map.name, out List<RareNodeDef> defs))
        {
            return;
        }
        List<GameObject> nodes = s_nodes[map.name];
        if (!RareColonyRules.CanRoll(Time.time, s_nextRoll.TryGetValue(map.name, out float at) ? at : 0f))
        {
            return; // 방금 굴렸다: 나타나 있던 군락은 그대로 둔다
        }
        s_nextRoll[map.name] = Time.time + RareColonyRules.RollCooldown;
        int pick = RareColonyRules.Pick(nodes.Count, Random.value, Random.value);
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].SetActive(i == pick);
        }
        if (pick >= 0)
        {
            GameLog.Info(GameLog.Cat.Map, "희귀 군락 출현: " + defs[pick].Id + " " + defs[pick].Position);
            PickupLog.Notice("희귀 군락이 보인다: " + defs[pick].Hint);
        }
    }

    /// <summary>검증용: 이 구역의 군락을 강제로 하나 켠다(없는 구역이면 false)</summary>
    public static bool ForceSpawn(string map, int index)
    {
        if (!s_nodes.TryGetValue(map, out List<GameObject> nodes) || nodes.Count == 0)
        {
            return false;
        }
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].SetActive(i == index);
        }
        return true;
    }

    private static GameObject Create(Transform parent, RareNodeDef d)
    {
        var go = new GameObject("RareColony_" + d.Id);
        go.SetActive(false); // 켜질 때 Awake 가 완성된 상태로 돈다
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(d.Position.x, d.Position.y, parent.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(2.5f, 2.5f);
        box.offset = new Vector2(0f, 1f);

        var glow = new GameObject("Glow");
        glow.transform.SetParent(go.transform, false);
        glow.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        var gsr = glow.AddComponent<SpriteRenderer>();
        gsr.sprite = GlowSprite();
        gsr.sortingOrder = 19;
        var mark = new GameObject("Mark");
        mark.transform.SetParent(go.transform, false);
        var sr = mark.AddComponent<SpriteRenderer>();
        sr.sprite = d.Kind == GatherKind.Ore ? GatherNodePlacer.VeinSprite() : GatherNodePlacer.BushSprite();
        sr.sortingOrder = 20;

        var node = go.AddComponent<GatherNode>();
        node.Init(d.Kind, 1, sr, true);
        go.AddComponent<RareColonyNode>().Init(node);
        go.AddComponent<MinimapMarker>().Init(new Color(0.85f, 0.55f, 1f), 8f, null);
        return go;
    }

    /// <summary>군락 뒤 은은한 보라 빛무리 (28×28, PPU 16 — 임시 그림)</summary>
    private static Sprite GlowSprite()
    {
        if (s_glow != null)
        {
            return s_glow;
        }
        const int n = 28;
        Color32[] px = MiniGameSprites.Blank(n * n);
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = (x - 13.5f) / 13.5f;
                float dy = (y - 13.5f) / 13.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < 1f)
                {
                    byte a = (byte)(70f * (1f - d));
                    px[y * n + x] = new Color32(200, 150, 255, a);
                }
            }
        }
        s_glow = MiniGameSprites.MakeSprite(px, n, n);
        return s_glow;
    }
}
