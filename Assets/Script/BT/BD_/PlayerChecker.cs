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
        // 주변에 있는 모든 Collider를 가져옴
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f, layerMask);

        // 가져온 Collider를 순회하면서 Player 태그를 가진 오브젝트가 있는지 확인
        foreach (Collider2D col in colliders)
        {

            if(isEnemyCheck == false)
            {
                if (col.CompareTag(playerTag))
                {
                    // Player 태그를 가진 오브젝트가 있으면 Success 반환
                    return TaskStatus.Success;
                }
                else if (col.CompareTag("APC"))
                {
                    behaviorTree.aPC = col.gameObject;
                    // Player 태그를 가진 오브젝트가 있으면 Success 반환
                    return TaskStatus.Success;
                }

            }
            else
            {
                if (col.CompareTag("Enemy"))
                {
                    // Player 태그를 가진 오브젝트가 있으면 Success 반환
                    return TaskStatus.Success;
                }
            }

        }

        // 주변에 Player 태그를 가진 오브젝트가 없으면 Failure 반환
        return TaskStatus.Failure;
    }


}
