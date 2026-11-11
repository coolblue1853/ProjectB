using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class PlayerChecker : EnemyConditional
{
    public Vector2 boxSize ; // 확인할 직사각형 영역의 크기
    public LayerMask layerMask; // 검출할 레이어
    public string playerTag = "Player"; // 플레이어 태그
    public bool isEnemyCheck = false;
    public override TaskStatus OnUpdate()
    {
        // REWORK-3: 인식 검사(폭 18 이상)는 행동 속성이 상자·선공/후공/비공격을 바꾼다. 속성 없는 몹은 기존 경로
        MobBehavior mb = enemyHealth != null ? enemyHealth.Behavior : null;
        bool detect = mb != null && !isEnemyCheck && MobBehaviorRules.IsDetectBox(boxSize);
        Vector2 box = detect ? MobBehaviorRules.DetectBox(mb, boxSize, enemyHealth.ChaseLatched, enemyHealth.IsProvoked) : boxSize;
        if (detect && !MobBehaviorRules.MayAcquire(mb, enemyHealth.ChaseLatched, enemyHealth.IsProvoked))
        {
            enemyHealth.ChaseLatched = false;
            return TaskStatus.Failure;
        }
        // 주변에 있는 모든 Collider를 가져옴
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, box, 0f, layerMask);

        // 가져온 Collider를 순회하면서 Player 태그를 가진 오브젝트가 있는지 확인
        foreach (Collider2D col in colliders)
        {

            if(isEnemyCheck == false)
            {
                if (col.CompareTag(playerTag))
                {
                    if (detect) { enemyHealth.ChaseLatched = true; }
                    return TaskStatus.Success;
                }
                else if (col.CompareTag("APC"))
                {
                    behaviorTree.aPC = col.gameObject;
                    if (detect) { enemyHealth.ChaseLatched = true; }
                    return TaskStatus.Success;
                }

            }
            else
            {
                if (col.CompareTag("Enemy"))
                {
                    behaviorTree.enemy = col.gameObject;
                    if (detect) { enemyHealth.ChaseLatched = true; }
                    return TaskStatus.Success;
                }
            }

        }

        if (detect) { enemyHealth.ChaseLatched = false; }
        return TaskStatus.Failure;
    }


}
