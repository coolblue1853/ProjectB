using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyGroundChecker : EnemyConditional
{
    BehaviorTree bt;
    public GameObject groundCheck2;
    RaycastHit2D hitRay;
    public float groundRayLength;
    private LayerMask groundLayerMask;
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        // 레이어 마스크는 한 번만 계산하여 저장
        groundLayerMask = LayerMask.GetMask("Ground");
    }
    public override TaskStatus OnUpdate()
    {
        // groundCheck2 위치에서 아래로 레이캐스트를 쏨
        RaycastHit2D hitRay = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, groundRayLength, groundLayerMask);

        // 디버그용 Ray 출력 (필요할 때만 활성화)
       // Debug.DrawRay(groundCheck2.transform.position, Vector2.down * groundRayLength, Color.red);

        // 바닥이 없고 점프 중이지 않으면 성공 반환
        if (hitRay.collider == null && !bt.isJumping)
        {
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
    }
}
