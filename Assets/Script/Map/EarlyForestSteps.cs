using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>초반 필수 동선의 2칸 턱에 기존 지면 타일로 중간 발판을 놓는다. 씬·타일 에셋은 저장하지 않는다.</summary>
public sealed class EarlyForestSteps : MonoBehaviour
{
    private static readonly Vector3[] Steps =
    {
        new Vector3(-339f, -1.1f, 0f),
        new Vector3(-269f, 2.9f, 0f),
    };
    /// <summary>같은 지면 타일이 있는지 보는 기준점(이 칸에 타일이 있는 첫 충돌 타일맵에 놓는다)</summary>
    public static readonly Vector3 ReferencePoint = new Vector3(-335f, -0.1f, 0f);
    /// <summary>각 위치에서 오른쪽으로 놓는 칸 수</summary>
    public const int Width = 2;
    private bool _placed;

    /// <summary>중간 발판 시작 위치(월드). 봇 이동 그래프 굽기(BotNavBaker)가 런타임 지형을 맞추려고 읽는다</summary>
    public static System.Collections.Generic.IReadOnlyList<Vector3> StepPositions => Steps;

    /// <summary>이 맵에 중간 발판을 놓는가</summary>
    public static bool AppliesTo(string mapName)
    {
        return mapName == "Lv1_NearForest_SideRoad" || mapName == "Lv1_NormalForest";
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        foreach (MapInfo map in Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (AppliesTo(map.name))
            {
                map.gameObject.AddComponent<EarlyForestSteps>();
            }
        }
    }

    private void OnEnable()
    {
        if (_placed)
        {
            return;
        }
        foreach (Tilemap map in GetComponentsInChildren<Tilemap>(true))
        {
            TilemapCollider2D collider = map.GetComponent<TilemapCollider2D>();
            if (collider == null)
            {
                continue;
            }
            TileBase tile = map.GetTile(map.WorldToCell(ReferencePoint));
            if (tile == null)
            {
                continue;
            }
            foreach (Vector3 position in Steps)
            {
                Vector3Int first = map.WorldToCell(position);
                for (int i = 0; i < Width; i++)
                {
                    Vector3Int cell = first + Vector3Int.right * i;
                    if (!map.HasTile(cell))
                    {
                        map.SetTile(cell, tile);
                    }
                }
            }
            collider.ProcessTilemapChanges();
            _placed = true;
            return;
        }
    }
}
