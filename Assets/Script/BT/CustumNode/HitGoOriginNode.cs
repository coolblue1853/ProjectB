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
    public float desiredSpeed =1.0f; // ���ϴ� �ӵ� ����
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
            float originX = brain.originPosition.x; // ������ X ��ǥ
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
