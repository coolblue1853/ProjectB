using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
public class EnemyGoOrigin : EnemyAction
{


    bool isArriveOrigin = false;
    public float desiredSpeed;

    public override void OnStart()
    {
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
        return (isEnd== true && isArriveOrigin == true) ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartGoOrigin()
    {

        if (isArriveOrigin == false)
        {
            if (anim != null)
            {
                anim.SetBool("isWalk", true);
            }
            float originX = originPosition.x; // ������ X ��ǥ
            float currentPositionX = enemyObject.transform.position.x; // ���� ��ġ�� X ��ǥ
            float distanceToMove = Mathf.Abs(originX - currentPositionX); // �̵��ؾ� �� �Ÿ��� ���밪�� ����մϴ�.
            float moveDuration = distanceToMove / desiredSpeed; // �̵��ؾ� �� �Ÿ��� ���ϴ� �ӵ��� �̵��ϴ� �� �ɸ��� �ð��� ����մϴ�.

            if (originX - currentPositionX > 0)
            {
                enemyObject.transform.localScale = new Vector3(chInRommSize, enemyObject.transform.localScale.y, 1);
            }
            else if (originX - currentPositionX < 0)
            {
                enemyObject.transform.localScale = new Vector3(-chInRommSize, enemyObject.transform.localScale.y, 1);
            }
            sequence = DOTween.Sequence()

            .Append(enemyObject.transform.DOMoveX(originX, moveDuration).SetEase(Ease.Linear)) // �������� �̵��մϴ�.

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
