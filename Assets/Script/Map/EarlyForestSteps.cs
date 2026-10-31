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
    private bool _placed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        foreach (MapInfo map in Object.FindObjectsByType<MapInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (map.name == "Lv1_NearForest_SideRoad" || map.name == "Lv1_NormalForest")
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
            TileBase tile = map.GetTile(map.WorldToCell(new Vector3(-335f, -0.1f, 0f)));
            if (tile == null)
            {
                continue;
            }
            foreach (Vector3 position in Steps)
            {
                Vector3Int first = map.WorldToCell(position);
                for (int i = 0; i < 2; i++)
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
