using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSkill : MonoBehaviour
{
    public GameObject player;
    public GameObject summonObject;
    public int summonCount;
    public float[] summonPosition;
    public float yPos;

    //  public GameObject summonPivot;
    public float summonDistance = 0; // üũ
    public float rayDistance = 0; // üũ
     Vector2 direction = new Vector2(1, 0);
    public float ackFloat;
    public LayerMask collisionLayer; // �浹 üũ�� ���� ���̾�
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");

        summonPosition = new float[summonCount];
        for (int i =0; i < summonCount; i++)
        {
            float zPosition = Random.Range(-1f, 1f);
            Vector2 newDir = new Vector2(Mathf.Abs(direction.x), direction.y);
            if (player.transform.localScale.x < 0)
            {
                newDir = new Vector2(-Mathf.Abs(direction.x), direction.y);
            }

            Vector2 currentPosition = new Vector2(player.transform.position.x, player.transform.position.y - 0.55f);
            Vector3 destination = currentPosition + newDir.normalized * summonDistance;
            destination = new Vector3(destination.x, destination.y + 0.55f);
            // ��ǥ ��ġ������ ��θ� �˻�
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, rayDistance, collisionLayer);

            if (hit.collider == null)

            {  // �浹�� ���� ���, ������Ʈ�� ��ǥ ��ġ�� �̵�
      
                GameObject r = Instantiate(summonObject, destination + new Vector3(0, yPos, zPosition), this.transform.rotation);
              

            }
            else
            {
                Vector3 safePosition;
                // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                if (newDir.x < 0)
                {
                     safePosition = hit.point + new Vector2(ackFloat, 0); // �浹 �������� �ణ ������ ��ġ
                }
                else
                {
                     safePosition = hit.point - new Vector2(ackFloat, 0); // �浹 �������� �ణ ������ ��ġ
                }
                safePosition = new Vector3(safePosition.x, safePosition.y + 0.55f);
                GameObject r = Instantiate(summonObject, safePosition + new Vector3(0, yPos, zPosition), this.transform.rotation);

            }



        }

      // Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
