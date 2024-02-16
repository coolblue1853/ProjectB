
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackNode : BTNode
{
    private Brain ai;

     bool isAttack= false;
    public float attackWaitTime;
    // Start is called before the first frame update
    public AttackNode(Brain brain)
    {
        ai = brain;
    }
    public override NodeState Evaluate()
    {
        if(isAttack == false)
        {
            brain.StopEvaluateCoroutine();
            sequence = DOTween.Sequence()
           .AppendCallback(() => Debug.Log("attack"))
           .AppendInterval(attackWaitTime)
           //  .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }
        else
        {
            isAttack = false;
            return NodeState.SUCCESS;

        }


    }

    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            isAttack = true;
            brain.restartEvaluate();
        }

    }
}
