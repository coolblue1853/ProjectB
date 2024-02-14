using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class PatrolNode : BTNode
{
    Vector2 originPosition;

    public GameObject enemyObject;
    public int xMin;
    public int xMax;

    public int originMax;

    int movePoint;
    public float moveDuration;

    private void Start()
    {
        originPosition = brain.originPosition;
    }
    // Start is called before the first frame update
    public PatrolNode()
    {
    }
    public override NodeState Evaluate()
    {
        if (IsWaiting == true)
        {
            if (brain.isEnd == true)
            {
                IsWaiting = false;
                return NodeState.FAILURE;
            }
            return NodeState.SUCCESS;
        }
        else
        {
            do
            {
                movePoint = Random.Range(xMin, xMax);
                int direction = Random.Range(0, 2);
                if (direction == 0)
                {
                    movePoint = -movePoint;
                }
                // 이동 거리를 계산할 때 originPosition.x를 사용하여 현재 위치에서 벗어나지 않도록 함
            } while (Mathf.Abs(enemyObject.transform.position.x + movePoint - originPosition.x) > originMax);


            Debug.Log("move");
             sequence = DOTween.Sequence()
                .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + movePoint, moveDuration).SetEase(Ease.Linear))

           // .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x+ movePoint, Mathf.Abs(movePoint)+ moveDuration))
           .OnComplete(() => brain.StopEvaluateCoroutine())
           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }

    }

    private void OnSequenceComplete()
    {
        if(this.transform != null)
        {
            IsWaiting = true;
            brain.restartEvaluate();
        }

    }
}
