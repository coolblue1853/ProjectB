using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using BehaviorDesigner.Runtime;
using UnityEngine;
public class EnemyPatrol : EnemyAction
{
    public int xMin;
    public int xMax;
    public int originMax;
    public bool isStop = false;
    public bool isGroundCheck = false;
    public EnemyGroundCheck groundCheck;
    int movePoint;
    public float desiredSpeed;
    BehaviorTree bt;
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        bt.sequence.Kill();
        isEnd = false;
        StartPatrol();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartPatrol()
    {

            movePoint = Random.Range(xMin, xMax);
            if (isStop == false)
            {
                   int direction = Random.Range(0, 2);
                if (direction == 0)
                {
                    movePoint = -movePoint;
                }
            }
            else if (isStop == true)
            {
                if (enemyObject.transform.localScale.x > 0)
                {
                    movePoint = -movePoint;
                }
                else
                {
                    movePoint = Mathf.Abs(movePoint);
                }
            }

        if (movePoint > 0)
        {
            enemyObject.transform.localScale = new Vector3(tfLocalScale, enemyObject.transform.localScale.y, 1);
        }
        else if (movePoint < 0)
        {
            enemyObject.transform.localScale = new Vector3(-tfLocalScale, enemyObject.transform.localScale.y, 1);
        }
        if (isGroundCheck == true)
        {
            groundCheck.isGroundCheck = true;
        }
        // 이동애니메이션 바꾸기
        if (anim != null)
        {
            anim.SetBool("isWalk", true);
        }
        if (Mathf.Abs(enemyObject.transform.position.x + movePoint - originPosition.x) > originMax)
        {
            if(enemyObject.transform.position.x > originPosition.x)
            {
                enemyObject.transform.localScale = new Vector3(-tfLocalScale, enemyObject.transform.localScale.y, 1);
            }
            else
            {
                enemyObject.transform.localScale = new Vector3(tfLocalScale, enemyObject.transform.localScale.y, 1);
            }
            float distanceToMove = Mathf.Abs(enemyObject.transform.position.x - originPosition.x); // 이동해야 할 거리의 절대값을 계산합니다.
            float moveDuration = distanceToMove / desiredSpeed; // 이동해야 할 거리를 일정한 속도로 이동하는 데 걸리는 시간을 계산합니다.
            bt.sequence = DOTween.Sequence()
            .Append(enemyObject.transform.DOMoveX(originPosition.x, moveDuration).SetEase(Ease.Linear))
            .OnComplete(() => OnSequenceComplete());
        }
        else
        {
            float distanceToMove = Mathf.Abs(movePoint); // 이동해야 할 거리의 절대값을 계산합니다.
            float moveDuration = distanceToMove / desiredSpeed; // 이동해야 할 거리를 일정한 속도로 이동하는 데 걸리는 시간을 계산합니다.
            bt.sequence = DOTween.Sequence()
                .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + movePoint, moveDuration).SetEase(Ease.Linear))
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
