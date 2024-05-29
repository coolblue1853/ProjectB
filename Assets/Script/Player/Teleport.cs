using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Vector2 direction; // 이동 방향 (예: (1, 0) - 오른쪽, (0, 1) - 위쪽)
    public float distance;    // 이동 거리
    public LayerMask collisionLayer; // 충돌 체크를 위한 레이어
    public GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        if(player.transform.localScale.x < 0)
        {
            direction = new Vector2(-direction.x, direction.y);
        }
        TryTeleport(direction, distance);
    }

    void TryTeleport(Vector2 direction, float distance)
    {
        Vector2 currentPosition = new Vector2(player.transform.position.x, player.transform.position.y -0.55f);
        Vector2 destination = currentPosition + direction.normalized * distance;
        destination = new Vector2(destination.x, destination.y + 0.55f);
        // 목표 위치까지의 경로를 검사
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, distance, collisionLayer);

        if (hit.collider == null)
        {
            // 충돌이 없는 경우, 오브젝트를 목표 위치로 이동
            player.transform.position = destination;

        }
        else
        {
            // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
            Vector2 safePosition = hit.point - direction.normalized * 0.2f; // 충돌 지점에서 약간 떨어진 위치
             safePosition = new Vector2(safePosition.x, safePosition.y + 0.55f);
            player.transform.position = safePosition;

        }
    }
}
