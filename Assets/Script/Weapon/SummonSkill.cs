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
    public float summonDistance = 0; // 체크
    public float rayDistance = 0; // 체크
     Vector2 direction = new Vector2(1, 0);
    public float ackFloat;
    public LayerMask collisionLayer; // 충돌 체크를 위한 레이어
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
            // 목표 위치까지의 경로를 검사
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, rayDistance, collisionLayer);

            if (hit.collider == null)

            {  // 충돌이 없는 경우, 오브젝트를 목표 위치로 이동
      
                GameObject r = Instantiate(summonObject, destination + new Vector3(0, yPos, zPosition), this.transform.rotation);
              

            }
            else
            {
                Vector3 safePosition;
                // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
                if (newDir.x < 0)
                {
                     safePosition = hit.point + new Vector2(ackFloat, 0); // 충돌 지점에서 약간 떨어진 위치
                }
                else
                {
                     safePosition = hit.point - new Vector2(ackFloat, 0); // 충돌 지점에서 약간 떨어진 위치
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
