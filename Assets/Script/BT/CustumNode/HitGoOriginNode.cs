using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class HitGoOriginNode : BTNode
{
    Vector2 originPosition;

    public GameObject enemyObject;
    public float moveDuration;

    private void Start()
    {
        originPosition = brain.originPosition;
    }
    // Start is called before the first frame update
    public HitGoOriginNode()
    {
    }
    public override NodeState Evaluate()
    {
        if (brain.isAttacked == false)
        {
            return NodeState.SUCCESS;
        }
        else
        {
            Debug.Log("gotoorigin");
            brain.StopEvaluateCoroutine();
            brain.isAttacked = false;
            sequence = DOTween.Sequence()
           .Append(enemyObject.transform.DOMoveX(brain.originPosition.x, Mathf.Abs(brain.originPosition.x - enemyObject.transform.position.x)/2))
           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }

    }


    private void OnSequenceComplete()
    {
        brain.restartEvaluate();
    }
}
