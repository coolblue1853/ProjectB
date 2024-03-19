﻿using System.Collections;
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

    public bool isLeftEnd = false;
    public bool isRightEnd = false;

    int movePoint;
    public float desiredSpeed;
    float chInRommSize;
    private void Start()
    {
        chInRommSize = enemyObject.transform.localScale.x;
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
                if(isLeftEnd == false && isRightEnd == false)
                {
                    int direction = Random.Range(0, 2);
                    if (direction == 0)
                    {
                        movePoint = -movePoint;
                    }
                }
                else if (isLeftEnd)
                {
                    isLeftEnd = false;
                }
                else if (isRightEnd)
                {
                    movePoint = -movePoint;
                    isRightEnd = false;
                }
  
                // 이동 거리를 계산할 때 originPosition.x를 사용하여 현재 위치에서 벗어나지 않도록 함
            } while (Mathf.Abs(enemyObject.transform.position.x + movePoint - originPosition.x) > originMax);

            if (movePoint > 0)
            {
                enemyObject.transform.localScale = new Vector3(chInRommSize, enemyObject.transform.localScale.y, 1);
            }
            else if (movePoint < 0)
            {
                enemyObject.transform.localScale = new Vector3(-chInRommSize, enemyObject.transform.localScale.y, 1);
            }


            float distanceToMove = Mathf.Abs(movePoint); // 이동해야 할 거리의 절대값을 계산합니다.
            float moveDuration = distanceToMove / desiredSpeed; // 이동해야 할 거리를 일정한 속도로 이동하는 데 걸리는 시간을 계산합니다.
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
