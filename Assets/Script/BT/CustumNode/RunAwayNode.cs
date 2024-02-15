using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class RunAwayNode : BTNode
{

    public RabbitReset rabbitReset;
    public GameObject enemyObject;
    public float runTimer;
    bool isEndTime;
    private void Start()
    {

    }
    // Start is called before the first frame update
    public RunAwayNode()
    {
    }
    public override NodeState Evaluate()
    {

        brain.StopEvaluateCoroutine();
        brain.isAttacked = false;
        sequence = DOTween.Sequence()
       .AppendInterval(runTimer)
       .OnComplete(() => OnSequenceComplete());
        return NodeState.FAILURE;

        /*
        if (brain.isAttacked == false)
        {
            return NodeState.SUCCESS;
        }
        else
        {
            brain.StopEvaluateCoroutine();
            brain.isAttacked = false;
            sequence = DOTween.Sequence()
           .Append(enemyObject.transform.DOMoveX(brain.originPosition.x, Mathf.Abs(brain.originPosition.x - enemyObject.transform.position.x)/2))
           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }
        */
    }


    private void OnSequenceComplete()
    {

        if (this.transform != null)
        {
            rabbitReset.ResetRabbit();

        }

    }
}
