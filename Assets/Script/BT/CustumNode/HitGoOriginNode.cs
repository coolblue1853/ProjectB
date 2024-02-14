using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class HitGoOriginNode : BTNode
{
    Vector2 originPosition;

    public GameObject enemyObject;
    public float moveDuration;
    bool isArriveOrigin = false;
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
        if(isArriveOrigin == false)
        {
            Debug.Log("goToOrigin");
            brain.StopEvaluateCoroutine();
            brain.isAttacked = false;
            sequence = DOTween.Sequence()

           .Append(enemyObject.transform.DOMoveX(brain.originPosition.x, moveDuration).SetEase(Ease.Linear))

           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }
        else
        {
            isArriveOrigin = false;
            return NodeState.SUCCESS;
        }


    }


    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            isArriveOrigin = true;
            brain.restartEvaluate();
        }

    }
}
