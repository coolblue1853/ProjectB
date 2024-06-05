using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
public class EnemyGoOrigin : EnemyAction
{
    BehaviorTree bt;

    bool isArriveOrigin = false;
    public float desiredSpeed;

    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        isEnd = false;
        StartGoOrigin();
    }
    public override TaskStatus OnUpdate()
    {
        if (Mathf.Abs(enemyObject.transform.position.y - originPosition.y) < 1)
        {
            isArriveOrigin = true;
        }
        else
        {
            isArriveOrigin = false;
        }
        return (isEnd == true && isArriveOrigin == true) ? TaskStatus.Success : TaskStatus.Running;
        if (bt.sequence.active == false)
        {
            isArriveOrigin = false;
            StartGoOrigin();
        }


    }
    public void StartGoOrigin()
    {

        if (isArriveOrigin == false)
        {
            if (anim != null)
            {
                anim.SetBool("isWalk", true);
            }
            float originX = originPosition.x; // 원점의 X 좌표
            float currentPositionX = enemyObject.transform.position.x; // 현재 위치의 X 좌표
            float distanceToMove = Mathf.Abs(originX - currentPositionX); // 이동해야 할 거리의 절대값을 계산합니다.
            float moveDuration = distanceToMove / desiredSpeed; // 이동해야 할 거리를 원하는 속도로 이동하는 데 걸리는 시간을 계산합니다.

            if (originX - currentPositionX > 0)
            {
                enemyObject.transform.localScale = new Vector3(chInRommSize, enemyObject.transform.localScale.y, 1);
            }
            else if (originX - currentPositionX < 0)
            {
                enemyObject.transform.localScale = new Vector3(-chInRommSize, enemyObject.transform.localScale.y, 1);
            }

            bt.sequence = DOTween.Sequence()
            .Append(enemyObject.transform.DOMoveX(originX, moveDuration).SetEase(Ease.Linear)) // 원점으로 이동합니다.
           .OnComplete(() => OnSequenceComplete());

        }

    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {

            isEnd = true;
        }
    }

}
