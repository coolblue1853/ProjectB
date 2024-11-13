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
    //public EnemyGroundCheck groundCheck;
    int movePoint;
    public float desiredSpeed;
    BehaviorTree bt;
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        bt.sequence.Kill(); // ���� ������ ����
        isEnd = false;
        originPosition = enemyObject.transform.position; // ���� ��ġ ����
        StartPatrol();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartPatrol()
    {
        movePoint = Random.Range(xMin, xMax);

        // isStop ���¿� ���� ����
        if (isStop)
        {
            movePoint = enemyObject.transform.localScale.x > 0 ? -movePoint : Mathf.Abs(movePoint);
        }
        else
        {
            int direction = Random.Range(0, 2);
            movePoint = direction == 0 ? -movePoint : movePoint;
        }

        // ���� ����: �̵��� ���⿡ �°� ���� ������ ����
        if (movePoint > 0)
        {
            enemyObject.transform.localScale = new Vector3(Mathf.Abs(enemyObject.transform.localScale.x), enemyObject.transform.localScale.y, 1);
        }
        else
        {
            enemyObject.transform.localScale = new Vector3(-Mathf.Abs(enemyObject.transform.localScale.x), enemyObject.transform.localScale.y, 1);
        }

        // �ִϸ��̼� ����
        if (anim != null)
        {
            anim.SetBool("isWalk", true);
        }

        // �̵� ������ originMax�� �ʰ��ϸ� ����ġ�� ���ƿ�
        if (Mathf.Abs(enemyObject.transform.position.x - originPosition.x) > originMax)
        {
            ReverseDirection();
            MoveToOrigin();
        }
        else
        {
            MoveToNewPoint();
        }
    }

    private void MoveToOrigin()
    {
        float distanceToMove = Mathf.Abs(enemyObject.transform.position.x - originPosition.x);
        float moveDuration = distanceToMove / desiredSpeed;

        bt.sequence = DOTween.Sequence()
            .Append(enemyObject.transform.DOMoveX(originPosition.x, moveDuration).SetEase(Ease.Linear))
            .OnComplete(() => OnSequenceComplete());
    }

    private void MoveToNewPoint()
    {
        float distanceToMove = Mathf.Abs(movePoint);
        float moveDuration = distanceToMove / desiredSpeed;

        bt.sequence = DOTween.Sequence()
            .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + movePoint, moveDuration).SetEase(Ease.Linear))
            .OnComplete(() => OnSequenceComplete());
    }

    private void ReverseDirection()
    {
        float newScaleX = enemyObject.transform.position.x > originPosition.x ? -Mathf.Abs(enemyObject.transform.localScale.x) : Mathf.Abs(enemyObject.transform.localScale.x);
        enemyObject.transform.localScale = new Vector3(newScaleX, enemyObject.transform.localScale.y, 1);
    }

    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            isEnd = true; // �׼� �Ϸ� �� isEnd�� true�� ����
        }
    }

}
