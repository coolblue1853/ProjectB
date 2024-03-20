using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class HitGoOriginNode : BTNode
{
    Vector2 originPosition;
    float chInRommSize;
    public GameObject enemyObject;
  //  public float moveDuration;
    bool isArriveOrigin = false;
    public float desiredSpeed =1.0f; // 원하는 속도 설정
    private void Start()
    {
        chInRommSize = enemyObject.transform.localScale.x;
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
            float originX = brain.originPosition.x; // 원점의 X 좌표
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
            sequence = DOTween.Sequence()

  .Append(enemyObject.transform.DOMoveX(originX, moveDuration).SetEase(Ease.Linear)) // 원점으로 이동합니다.

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
