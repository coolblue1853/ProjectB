#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// Lv2 "마법 절벽" 맵을 코드 레이아웃으로 생성한다 (Lv2ContentPlan 2절, 다시 실행하면 지우고 재생성).
    /// 구조·컴포넌트는 SideRoad 맵의 오브젝트를 복제해 그대로 쓰고(태그·레이어·충돌·스크립트), 그래픽은 Magic Cliffs 타일로 칠한다.
    /// </summary>
    public static class Lv2MapBuilder
    {
        public const string RootName = "Lv2_MagicCliffs";
        const int X0 = -420;      // 맵 왼쪽 끝 (월드 x)
        const int Width = 240;    // 가로 칸 수 (타일 1칸 = 1u)
        const int Y0 = -180;      // 바닥 윗면 (월드 y)
        const string TileDir = "Assets/ThirdParty/MagicCliffs/MagicCliffsArtwork/TileSets/";
        const string ArtDir = "Assets/ThirdParty/MagicCliffs/MagicCliffsArtwork/";

        // (발판 높이, 시작 x오프셋, 길이)
        static readonly int[,] Platforms =
        {
            { 4, 15, 10 }, { 4, 50, 14 }, { 4, 90, 12 }, { 4, 130, 14 }, { 4, 170, 12 },
            { 8, 30, 10 }, { 8, 70, 12 }, { 8, 110, 12 }, { 8, 150, 12 },
            { 12, 95, 20 },                      // 엘리트 원형 발판
        };
        // (x오프셋, 아래 높이, 위 높이) 로프
        static readonly int[,] Ropes =
        {
            { 20, 0, 4 }, { 57, 0, 4 }, { 96, 0, 4 }, { 137, 0, 4 }, { 176, 0, 4 },
            { 35, 4, 8 }, { 76, 4, 8 }, { 116, 4, 8 }, { 156, 4, 8 },
            { 104, 8, 12 },
        };

        public class SpawnDef
        {
            public string Name, TemplateSpawner, Monster, Dead;
            public bool Day;
            public int Count;
            public float Cycle;
            public int X1, X2, Floor; // 영역 x오프셋 범위, 바닥 높이(위치 자식 배치용)
        }

        static readonly SpawnDef[] Spawns =
        {
            new SpawnDef { Name = "L2_A_Shroom", TemplateSpawner = "Mole", Monster = "SporeShroom_BD", Dead = "SporeShroomDeadBody", Day = true, Count = 4, Cycle = 10, X1 = 8, X2 = 60, Floor = 0 },
            new SpawnDef { Name = "L2_B_Goblin", TemplateSpawner = "RatThief", Monster = "CliffGoblin_BD", Dead = "CliffGoblinDeadBody", Day = true, Count = 6, Cycle = 10, X1 = 60, X2 = 150, Floor = 0 },
            new SpawnDef { Name = "L2_B_ShroomUp", TemplateSpawner = "Mole", Monster = "SporeShroom_BD", Dead = "SporeShroomDeadBody", Day = true, Count = 3, Cycle = 10, X1 = 50, X2 = 64, Floor = 4 },
            new SpawnDef { Name = "L2_C_Goblin", TemplateSpawner = "RatThief", Monster = "CliffGoblin_BD", Dead = "CliffGoblinDeadBody", Day = true, Count = 4, Cycle = 10, X1 = 150, X2 = 200, Floor = 0 },
            new SpawnDef { Name = "L2_A_EyeN", TemplateSpawner = "Flowermole1", Monster = "GazeEye_BD", Dead = "GazeEyeDeadBody", Day = false, Count = 3, Cycle = 12, X1 = 8, X2 = 60, Floor = 0 },
            new SpawnDef { Name = "L2_B_SkelN", TemplateSpawner = "RatCheif", Monster = "CliffSkeleton_BD", Dead = "CliffSkeletonDeadBody", Day = false, Count = 4, Cycle = 12, X1 = 60, X2 = 150, Floor = 0 },
            new SpawnDef { Name = "L2_B_GoblinN", TemplateSpawner = "RatThief", Monster = "CliffGoblin_BD", Dead = "CliffGoblinDeadBody", Day = false, Count = 3, Cycle = 10, X1 = 130, X2 = 144, Floor = 4 },
            new SpawnDef { Name = "L2_C_SkelN", TemplateSpawner = "RatCheif", Monster = "CliffSkeleton_BD", Dead = "CliffSkeletonDeadBody", Day = false, Count = 4, Cycle = 12, X1 = 150, X2 = 200, Floor = 0 },
            new SpawnDef { Name = "L2_C_EliteN", TemplateSpawner = "BossSpawoner", Monster = "DeathBringer_BD", Dead = "DeathBringerDeadBody", Day = false, Count = 1, Cycle = 90, X1 = 95, X2 = 115, Floor = 12 },
            new SpawnDef { Name = "L2_D_BossN", TemplateSpawner = "BossSpawoner", Monster = "AbyssWizard_BD", Dead = "AbyssWizardDeadBody", Day = false, Count = 1, Cycle = 180, X1 = 205, X2 = 236, Floor = 0 },
        };

        public static string Build()
        {
            if (EditorApplication.isPlaying)
            {
                return "플레이 중에는 실행 불가";
            }
            var log = new StringBuilder();
            Transform map = GameObject.Find("Map").transform;
            Transform side = map.Find("Lv1_NearForest_SideRoad");
            Transform old = map.Find(RootName);
            if (old != null)
            {
                Object.DestroyImmediate(old.gameObject);
            }
            foreach (var p in Object.FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (p.name == "Portal_ToLv2")
                {
                    Object.DestroyImmediate(p.gameObject);
                }
            }

            var root = new GameObject(RootName);
            root.transform.SetParent(map, false);
            root.transform.position = new Vector3(0, 0, 100);
            var info = root.AddComponent<MapInfo>();
            info.displayName = "마법 절벽";
            info.cameraBottom = Y0 - 8;
            var grid = root.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Dictionary<Vector2Int, TileBase> tiles = LoadTiles("tileset_");

            // 지형 타일맵: SideRoad 오브젝트를 복제(컴포넌트·태그·레이어·정렬 유지) 후 비운다
            Tilemap ground = CloneTilemap(side.Find("BaseTile"), root.transform, "BaseTile");
            Tilemap wall = CloneTilemap(side.Find("BaseTileWall"), root.transform, "BaseTileWall");
            Tilemap visual = CloneTilemap(side.Find("BackTile"), root.transform, "PlatformTiles");

            TileBase[] grassTop = Pick(tiles, (1, 9), (2, 9), (3, 9), (4, 9));
            TileBase[] fill = Pick(tiles, (2, 12), (3, 12), (4, 12), (2, 14), (3, 14), (4, 14));
            for (int x = 0; x < Width; x++)
            {
                ground.SetTile(new Vector3Int(X0 + x, Y0 - 1, 0), grassTop[x % grassTop.Length]);
                for (int y = 2; y <= 8; y++)
                {
                    ground.SetTile(new Vector3Int(X0 + x, Y0 - y, 0), fill[(x + y) % fill.Length]);
                }
            }
            for (int y = -8; y <= 24; y++)
            {
                for (int k = 1; k <= 3; k++)
                {
                    wall.SetTile(new Vector3Int(X0 - k, Y0 + y, 0), fill[((y + k) % fill.Length + fill.Length) % fill.Length]);
                    wall.SetTile(new Vector3Int(X0 + Width - 1 + k, Y0 + y, 0), fill[((y + k) % fill.Length + fill.Length) % fill.Length]);
                }
            }

            // 원웨이 발판: 그래픽은 타일, 충돌은 SideRoad의 PlatformEffector 발판을 복제
            TileBase pl = tiles.GetValueOrDefault(new Vector2Int(10, 12)), pm = tiles.GetValueOrDefault(new Vector2Int(11, 12)), pr = tiles.GetValueOrDefault(new Vector2Int(12, 12));
            TileBase ul = tiles.GetValueOrDefault(new Vector2Int(10, 13)), um = tiles.GetValueOrDefault(new Vector2Int(11, 13)), ur = tiles.GetValueOrDefault(new Vector2Int(12, 13));
            Transform platTemplate = side.Find("NotGround").GetChild(0);
            var platRoot = new GameObject("NotGround").transform;
            platRoot.SetParent(root.transform, false);
            for (int i = 0; i < Platforms.GetLength(0); i++)
            {
                int h = Platforms[i, 0], sx = Platforms[i, 1], len = Platforms[i, 2];
                for (int x = 0; x < len; x++)
                {
                    bool l = x == 0, r = x == len - 1;
                    visual.SetTile(new Vector3Int(X0 + sx + x, Y0 + h - 1, 0), l ? pl : r ? pr : pm);
                    visual.SetTile(new Vector3Int(X0 + sx + x, Y0 + h - 2, 0), l ? ul : r ? ur : um);
                }
                var plat = Object.Instantiate(platTemplate.gameObject, platRoot).transform;
                plat.name = "Platform_" + h + "_" + sx;
                plat.position = new Vector3(X0 + sx + len / 2f, Y0 + h - 0.25f, 110f);
                plat.localScale = new Vector3(len, 0.5f, 1f);
                var sr = plat.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }

            // 로프(사다리)
            Transform ladderTemplate = side.Find("Ladder").GetChild(0);
            var ladderRoot = new GameObject("Ladder").transform;
            ladderRoot.SetParent(root.transform, false);
            float baseH = ladderTemplate.GetComponent<SpriteRenderer>().bounds.size.y;
            for (int i = 0; i < Ropes.GetLength(0); i++)
            {
                int x = Ropes[i, 0], h0 = Ropes[i, 1], h1 = Ropes[i, 2];
                var rope = Object.Instantiate(ladderTemplate.gameObject, ladderRoot).transform;
                rope.name = "Rope_" + x + "_" + h0;
                float height = h1 - h0 + 1.5f;
                Vector3 s = ladderTemplate.localScale;
                rope.localScale = new Vector3(s.x, s.y * height / baseH, s.z);
                rope.position = new Vector3(X0 + x + 0.5f, Y0 + (h0 + h1) / 2f + 0.25f, ladderTemplate.position.z);
            }

            // 배경 (정적 타일 배경: 하늘·구름·원경·바다)
            var bg = new GameObject("Background").transform;
            bg.SetParent(root.transform, false);
            AddBackdrop(bg, "sky", ArtDir + "sky.png", -24, new Vector2(Width + 80, 40), Y0 + 12);
            AddBackdrop(bg, "clouds", ArtDir + "clouds.png", -23, new Vector2(Width + 80, 15), Y0 + 16);
            AddBackdrop(bg, "far-grounds", ArtDir + "far-grounds.png", -22, new Vector2(Width + 80, 7), Y0 + 2.5f);
            AddBackdrop(bg, "sea", ArtDir + "sea.png", -21, new Vector2(Width + 80, 6), Y0 - 1);

            // 스포너
            var day = new GameObject("DayTimeSpawn").transform;
            var night = new GameObject("NightTimeSpawn").transform;
            day.SetParent(root.transform, false);
            night.SetParent(root.transform, false);
            foreach (SpawnDef d in Spawns)
            {
                log.Append(BuildSpawner(side, d, d.Day ? day : night)).Append(' ');
            }
            var pc = root.AddComponent<PoolCheck>();
            pc.dayTimeSpwaner = day.gameObject;
            pc.nightTimeSpwaner = night.gameObject;

            // 카메라 경계 (ProCamera2D Geometry Boundaries가 보는 레이어의 3D 박스)
            Transform camTemplate = side.Find("Cam").GetChild(0);
            var cam = new GameObject("Cam").transform;
            cam.SetParent(root.transform, false);
            AddBound(camTemplate, cam, "Bottom", new Vector3(X0 + Width / 2f, Y0 - 4f, 0), new Vector3(Width + 10, 1, 1));
            AddBound(camTemplate, cam, "Left", new Vector3(X0 - 2f, Y0 + 12f, 0), new Vector3(1, 40, 1));
            AddBound(camTemplate, cam, "Right", new Vector3(X0 + Width + 1f, Y0 + 12f, 0), new Vector3(1, 40, 1));
            AddBound(camTemplate, cam, "Top", new Vector3(X0 + Width / 2f, Y0 + 30f, 0), new Vector3(Width + 10, 1, 1));

            // 포털: SideRoad 서쪽 ↔ Lv2 입구
            Portal sideTemplate = side.GetComponentsInChildren<Portal>(true).First(p => p.openMap != null && p.openMap.name == "Town");
            var portalRoot = new GameObject("Portal").transform;
            portalRoot.SetParent(root.transform, false);
            Portal toLv1 = Object.Instantiate(sideTemplate.gameObject, portalRoot).GetComponent<Portal>();
            toLv1.name = "Portal_ToLv1";
            toLv1.transform.position = new Vector3(X0 + 3f, Y0 + 0.6f, sideTemplate.transform.position.z);
            Portal toLv2 = Object.Instantiate(sideTemplate.gameObject, side.Find("Portal")).GetComponent<Portal>();
            toLv2.name = "Portal_ToLv2";
            Portal sideInner = side.GetComponentsInChildren<Portal>(true).First(p => p.name == "PotalLv1_NearForestSide");
            toLv2.transform.position = sideInner.transform.position + new Vector3(6f, 0f, 0f);
            toLv2.destination = toLv1.transform; toLv2.openMap = root; toLv2.closeMap = side.gameObject;
            toLv1.destination = toLv2.transform; toLv1.openMap = side.gameObject; toLv1.closeMap = root;

            root.SetActive(false);
            EditorSceneManager.MarkSceneDirty(root.scene);
            EditorSceneManager.SaveScene(root.scene);
            log.Append(Lv2ItemBuilder.ApplyDrops()); // 맵을 다시 만들어도 드롭 테이블 유지
            return "Lv2 생성: 발판 " + Platforms.GetLength(0) + ", 로프 " + Ropes.GetLength(0) + ", 스포너 " + Spawns.Length + " | " + log;
        }

        static Dictionary<Vector2Int, TileBase> LoadTiles(string prefix)
        {
            var map = new Dictionary<Vector2Int, TileBase>();
            foreach (string guid in AssetDatabase.FindAssets("t:Tile", new[] { TileDir }))
            {
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(guid));
                if (tile == null || tile.sprite == null || !tile.name.StartsWith(prefix))
                {
                    continue;
                }
                Rect r = tile.sprite.rect;
                int cx = Mathf.RoundToInt(r.x / 16f);
                int cyTop = Mathf.RoundToInt((tile.sprite.texture.height - r.y - 16f) / 16f);
                map[new Vector2Int(cx, cyTop)] = tile;
            }
            return map;
        }

        static TileBase[] Pick(Dictionary<Vector2Int, TileBase> tiles, params (int, int)[] cells)
        {
            var list = new List<TileBase>();
            foreach (var c in cells)
            {
                if (tiles.TryGetValue(new Vector2Int(c.Item1, c.Item2), out TileBase t))
                {
                    list.Add(t);
                }
            }
            return list.ToArray();
        }

        static Tilemap CloneTilemap(Transform template, Transform parent, string name)
        {
            var go = Object.Instantiate(template.gameObject, parent);
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            var tm = go.GetComponent<Tilemap>();
            tm.ClearAllTiles();
            return tm;
        }

        static void AddBackdrop(Transform parent, string name, string spritePath, int order, Vector2 size, float centerY)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                return;
            }
            var imp = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            if (imp.wrapMode != TextureWrapMode.Repeat || imp.spriteImportMode != SpriteImportMode.Single || imp.spritePixelsPerUnit != 16 || imp.textureCompression != TextureImporterCompression.Uncompressed)
            {
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.spritePixelsPerUnit = 16;
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.SaveAndReimport();
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(X0 + Width / 2f, centerY, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = order;
        }

        static void AddBound(Transform template, Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var go = Object.Instantiate(template.gameObject, parent);
            go.name = "CamBoundary_" + name;
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        static string BuildSpawner(Transform side, SpawnDef d, Transform group)
        {
            EnemySpawner tpl = side.GetComponentsInChildren<EnemySpawner>(true).FirstOrDefault(s => s.name == d.TemplateSpawner);
            if (tpl == null)
            {
                return "[템플릿 없음 " + d.TemplateSpawner + "]";
            }
            var go = Object.Instantiate(tpl.gameObject, group);
            go.name = d.Name;
            go.SetActive(false); // PoolCheck가 포털 진입 시 켠다
            var sp = go.GetComponent<EnemySpawner>();
            sp.enemyCount = d.Count;
            sp.cycleFloat = d.Cycle;
            sp.isDaytimeSpowner = d.Day;
            sp.isRandSpawn = d.Count > 1;

            // 영역(트리거)
            float x1 = X0 + d.X1, x2 = X0 + d.X2;
            go.transform.position = new Vector3((x1 + x2) / 2f, Y0 + d.Floor + 6f, tpl.transform.position.z);
            go.transform.localScale = Vector3.one;
            var box = go.GetComponent<BoxCollider2D>();
            box.offset = Vector2.zero;
            box.size = new Vector2(x2 - x1, d.Floor >= 4 ? 8f : 16f);

            // 스폰 위치 자식 (enemyPositon 아래를 비우고 다시 만든다)
            Transform posRoot = sp.enemyPositon != null ? sp.enemyPositon.transform : null;
            if (posRoot == null)
            {
                posRoot = new GameObject("EnemyPosition").transform;
                posRoot.SetParent(go.transform, false);
                sp.enemyPositon = posRoot.gameObject;
            }
            for (int i = posRoot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(posRoot.GetChild(i).gameObject);
            }
            for (int i = 0; i < d.Count; i++)
            {
                var p = new GameObject("Pos" + i).transform;
                p.SetParent(posRoot, false);
                float t = d.Count == 1 ? 0.5f : i / (float)(d.Count - 1);
                p.position = new Vector3(Mathf.Lerp(x1 + 2, x2 - 2, t), Y0 + d.Floor + 1.5f, 0f);
            }

            // 풀 설정: enemy·dead 교체, 공격 키는 템플릿 그대로(BT가 같은 키를 쓰므로)
            var so = new SerializedObject(sp);
            SerializedProperty infos = so.FindProperty("objectInfos");
            var monster = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy_BD/" + d.Monster + ".prefab");
            var dead = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Enemy/Lv2/" + d.Dead + ".prefab");
            for (int i = 0; i < infos.arraySize; i++)
            {
                SerializedProperty e = infos.GetArrayElementAtIndex(i);
                string key = e.FindPropertyRelative("objectName").stringValue;
                if (key == "enemy")
                {
                    e.FindPropertyRelative("perfab").objectReferenceValue = monster;
                    e.FindPropertyRelative("count").intValue = d.Count;
                }
                else if (key == "dead")
                {
                    e.FindPropertyRelative("perfab").objectReferenceValue = dead;
                    e.FindPropertyRelative("count").intValue = Mathf.Max(2, d.Count);
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            // 드롭은 Lv2 아이템 추가 단계(5.6)에서 채운다
            var dm = go.GetComponent<DropManager>();
            if (dm != null)
            {
                var dso = new SerializedObject(dm);
                dso.FindProperty("dropItems").arraySize = 0;
                dso.ApplyModifiedPropertiesWithoutUndo();
            }
            return d.Name;
        }
    }
}
#endif
