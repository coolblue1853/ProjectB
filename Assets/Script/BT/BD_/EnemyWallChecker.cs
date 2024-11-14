using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;

public class EnemyWallChecker : EnemyConditional
{
    private BehaviorTree bt;
    public GameObject wallCheck;
    public float wallRay = 0.2f;

    public override void OnStart()
    {
        bt = GetComponent<BehaviorTree>();
    }

    public override TaskStatus OnUpdate()
    {
        // 현재 적의 방향에 따라 레이 방향을 설정합니다.
        Vector2 rayDirection = enemyObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // 벽 검출 레이캐스트 실행
        RaycastHit2D hit2 = Physics2D.Raycast(wallCheck.transform.position, rayDirection, wallRay, LayerMask.GetMask("Wall"));

        // 디버그 레이 그리기
     //   Debug.DrawRay(wallCheck.transform.position, rayDirection * wallRay, Color.blue);

        // 레이캐스트 결과에 따라 TaskStatus 반환
        return hit2.collider != null ? TaskStatus.Success : TaskStatus.Failure;
    }
}
