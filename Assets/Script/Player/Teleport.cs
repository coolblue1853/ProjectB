using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Vector2 direction; // �̵� ���� (��: (1, 0) - ������, (0, 1) - ����)
    public float distance;    // �̵� �Ÿ�
    public LayerMask collisionLayer; // �浹 üũ�� ���� ���̾�
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
        // ��ǥ ��ġ������ ��θ� �˻�
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, distance, collisionLayer);

        if (hit.collider == null)
        {
            // �浹�� ���� ���, ������Ʈ�� ��ǥ ��ġ�� �̵�
            player.transform.position = destination;

        }
        else
        {
            // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
            Vector2 safePosition = hit.point - direction.normalized * 0.2f; // �浹 �������� �ణ ������ ��ġ
             safePosition = new Vector2(safePosition.x, safePosition.y + 0.55f);
            player.transform.position = safePosition;

        }
    }
}
