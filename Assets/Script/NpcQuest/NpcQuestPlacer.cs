using UnityEngine;

/// <summary>
/// NPC 퀘스트 배치 (씬 파일 수정 없이 부트 때 만든다, ExcavationSitePlacer 와 같은 방식).
/// - 기존 NPC(촌장 TalkNpc, 상인 Shop, 대장장이 WeaponSmith)에 NpcQuestGiver 를 붙인다.
/// - 신규 NPC 3명: 루엔·세나 = 마을, 이브 = 일반 숲 입구. 도트는 기존 마을 NPC 스프라이트를 색조만 바꾼 임시(PixelLab 안 씀).
/// - 방문 목표 지점 3곳(QuestVisitMarker).
/// </summary>
public static class NpcQuestPlacer
{
    private struct NewNpc
    {
        public string Id, Conversation, Map, Source;
        public float X;
        public float? Y;
        public Color Tint;
    }

    private static readonly NewNpc[] NewNpcs =
    {
        new NewNpc { Id = "ruen", Conversation = StoryDialogues.RuenTalk, Map = "Town", Source = "EquipSmith", X = -402.5f, Tint = new Color(0.72f, 1f, 0.7f) },
        new NewNpc { Id = "sena", Conversation = StoryDialogues.SenaTalk, Map = "Town", Source = "Shop", X = -409.5f, Tint = new Color(0.65f, 1f, 0.9f) },
        new NewNpc { Id = "eve", Conversation = StoryDialogues.EveTalk, Map = "Lv1_NormalForest", Source = "EquipSmith", X = -274f, Y = 1.4f, Tint = new Color(0.9f, 0.75f, 1f) },
    };

    private struct Marker
    {
        public string Id, Map;
        public float X, Y;
    }

    private static readonly Marker[] Markers =
    {
        new Marker { Id = "forest_night_post", Map = "Lv1_NormalForest", X = -300f, Y = 0.4f },
        new Marker { Id = "deep_nest", Map = "Lv1_DeepForest", X = -306f, Y = 0.4f },
        new Marker { Id = "tower_altar", Map = "Lv1_TowerOutskirts", X = -312f, Y = 0.4f },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        MapInfo[] maps = Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        MapInfo town = Find(maps, "Town");
        AttachExisting(town);
        foreach (NewNpc n in NewNpcs)
        {
            MapInfo map = Find(maps, n.Map);
            Transform src = town != null ? town.transform.Find(n.Source) : null;
            if (map == null || src == null)
            {
                Debug.LogWarning("[PB][NpcQuest] NPC 배치 실패: " + n.Id);
                continue;
            }
            CreateNpc(n, map.transform, src);
        }
        CreateCoaches(maps, town);
        foreach (Marker m in Markers)
        {
            MapInfo map = Find(maps, m.Map);
            if (map == null)
            {
                Debug.LogWarning("[PB][NpcQuest] 맵 없음: " + m.Map);
                continue;
            }
            CreateMarker(m, map.transform);
        }
    }

    private static MapInfo Find(MapInfo[] maps, string name)
    {
        foreach (MapInfo m in maps)
        {
            if (m.name == name)
            {
                return m;
            }
        }
        return null;
    }

    private static void AttachExisting(MapInfo town)
    {
        if (town == null)
        {
            return;
        }
        foreach (TalkNpc t in town.GetComponentsInChildren<TalkNpc>(true))
        {
            if (t.Conversation == StoryDialogues.Elder && t.GetComponent<NpcQuestGiver>() == null)
            {
                t.gameObject.AddComponent<NpcQuestGiver>().Init("elder");
            }
        }
        AddGiver(town.transform.Find("Shop"), "merchant");
        AddGiver(town.transform.Find("WeaponSmith"), "smith");
        AddGiver(town.transform.Find("QuestBoard"), QuestBoard.BoardNpcId); // REWORK-5: 게시판 = giver questboard
    }

    private static void AddGiver(Transform t, string id)
    {
        if (t != null && t.GetComponent<NpcQuestGiver>() == null)
        {
            t.gameObject.AddComponent<NpcQuestGiver>().Init(id);
        }
    }

    /// <summary>마차 NPC (FastTravel.md): 정류장 표(FastTravelStops.csv)의 npcName·anchorX 로 각 맵에 배치. 도트는 상인 임시 + 색조</summary>
    private static void CreateCoaches(MapInfo[] maps, MapInfo town)
    {
        Transform src = town != null ? town.transform.Find("Shop") : null;
        foreach (FastTravelStop s in FastTravelStops.All)
        {
            if (string.IsNullOrEmpty(s.NpcId) || !s.AnchorX.HasValue)
            {
                continue;
            }
            MapInfo map = Find(maps, s.Id);
            if (map == null || src == null)
            {
                Debug.LogWarning("[PB][FastTravel] 마차 NPC 배치 실패: " + s.Id);
                continue;
            }
            var n = new NewNpc { Id = s.NpcId, Conversation = StoryDialogues.CoachTalk, Map = s.Id, Source = "Shop", X = s.AnchorX.Value, Y = s.AnchorY, Tint = new Color(1f, 0.85f, 0.55f) };
            CreateNpc(n, map.transform, src).AddComponent<FastTravelNpc>().Init(s.Id);
        }
    }

    private static GameObject CreateNpc(NewNpc n, Transform parent, Transform src)
    {
        var go = new GameObject("NPC_" + n.Id);
        go.transform.SetParent(parent, false);
        bool town = n.Map == "Town";
        go.transform.position = new Vector3(n.X, n.Y ?? src.position.y, town ? src.position.z : parent.position.z);
        Vector3 ls = src.lossyScale;
        go.transform.localScale = new Vector3(Mathf.Abs(ls.x) / Mathf.Max(0.0001f, Mathf.Abs(parent.lossyScale.x)), Mathf.Abs(ls.y) / Mathf.Max(0.0001f, Mathf.Abs(parent.lossyScale.y)), 1f);
        SpriteRenderer srcSr = src.GetComponentInChildren<SpriteRenderer>(true);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = srcSr.sprite;
        sr.sortingLayerID = srcSr.sortingLayerID;
        sr.sortingOrder = srcSr.sortingOrder;
        sr.sharedMaterial = srcSr.sharedMaterial;
        sr.color = n.Tint;
        SpriteFlipbook srcFb = src.GetComponent<SpriteFlipbook>();
        if (srcFb != null && srcFb.Frames != null && srcFb.Frames.Length > 0)
        {
            go.AddComponent<SpriteFlipbook>().Frames = srcFb.Frames;
        }
        float sx = Mathf.Abs(go.transform.lossyScale.x);
        float sy = Mathf.Abs(go.transform.lossyScale.y);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.2f / sx, 2f / sy);
        col.offset = new Vector2(0f, 1f / sy);
        go.AddComponent<TalkNpc>().Configure(n.Conversation, 3.0f / sy);
        go.AddComponent<NpcQuestGiver>().Init(n.Id);
        if (!town)
        {
            go.AddComponent<GroundSnapOnce>();
        }
        return go;
    }

    private static void CreateMarker(Marker m, Transform parent)
    {
        var go = new GameObject("QuestVisit_" + m.Id);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(m.X, m.Y, parent.position.z);
        go.layer = LayerMask.NameToLayer("Default");
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(4f, 3f);
        box.offset = new Vector2(0f, 1f);
        var mark = new GameObject("Mark");
        mark.transform.SetParent(go.transform, false);
        var sr = mark.AddComponent<SpriteRenderer>();
        sr.sprite = QuestVisitMarker.StarSprite();
        sr.sortingOrder = 20;
        sr.enabled = false;
        go.AddComponent<QuestVisitMarker>().Init(m.Id, sr);
    }
}
