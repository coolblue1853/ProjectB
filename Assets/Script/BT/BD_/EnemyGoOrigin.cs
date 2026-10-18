using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
public class EnemyGoOrigin : EnemyAction
{
    private BehaviorTree bt;
    private bool isArriveOrigin = false;
    public float desiredSpeed;

    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        isEnd = false;
        originPosition = HomePosition; // 풀 재사용 시에도 이번 스폰 위치로 복귀
        StartGoOrigin();
    }
    public override TaskStatus OnUpdate()
    {
        if (bt.sequence == null || !bt.sequence.IsActive())
        {
            StartGoOrigin(); // 움직임이 중단되었거나 완료된 경우 다시 시작
        }

        return isEnd && Mathf.Abs(enemyObject.transform.position.x - originPosition.x) < 1f
            ? TaskStatus.Success
            : TaskStatus.Running;
    }
    public void StartGoOrigin()
    {

        if (anim != null)
        {
            anim.SetBool("isWalk", true);
        }

        // 이동 거리와 시간 계산
        float distanceToMove = Mathf.Abs(originPosition.x - enemyObject.transform.position.x);
        float moveDuration = distanceToMove / desiredSpeed;

        // 방향에 따라 스케일 변경
        enemyObject.transform.localScale = new Vector3(
            originPosition.x > enemyObject.transform.position.x ? tfLocalScale : -tfLocalScale,
            enemyObject.transform.localScale.y, 1
        );

        // 이동 시퀀스 설정
        bt.sequence = DOTween.Sequence()
            .Append(enemyObject.transform.DOMoveX(originPosition.x, moveDuration).SetEase(Ease.Linear))
            .OnComplete(OnSequenceComplete)
            .SetLink(enemyObject); // 몹이 파괴되면(풀 초과 등) 같이 끝낸다 — 파괴된 transform 접근 방지

    }
    private void OnSequenceComplete()
    {
        if (anim != null)
        {
            anim.SetBool("isWalk", false);
        }
        isEnd = true;
    }

}
