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
        StartGoOrigin();
    }
    public override TaskStatus OnUpdate()
    {
        if (bt.sequence == null || !bt.sequence.IsActive())
        {
            StartGoOrigin(); // �������� �ߴܵǾ��ų� �Ϸ�� ��� �ٽ� ����
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

        // �̵� �Ÿ��� �ð� ���
        float distanceToMove = Mathf.Abs(originPosition.x - enemyObject.transform.position.x);
        float moveDuration = distanceToMove / desiredSpeed;

        // ���⿡ ���� ������ ����
        enemyObject.transform.localScale = new Vector3(
            originPosition.x > enemyObject.transform.position.x ? tfLocalScale : -tfLocalScale,
            enemyObject.transform.localScale.y, 1
        );

        // �̵� ������ ����
        bt.sequence = DOTween.Sequence()
            .Append(enemyObject.transform.DOMoveX(originPosition.x, moveDuration).SetEase(Ease.Linear))
            .OnComplete(OnSequenceComplete);

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
