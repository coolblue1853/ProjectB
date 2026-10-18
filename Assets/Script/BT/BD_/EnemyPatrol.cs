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
        bt.sequence.Kill(); // 이전 시퀀스 종료
        isEnd = false;
        originPosition = HomePosition; // 리쉬 기준 = 스폰 위치 (매번 현재 위치로 덮어쓰면 리쉬가 동작하지 않음, B-110)
        StartPatrol();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartPatrol()
    {
        movePoint = Random.Range(xMin, xMax);

        // isStop 상태에 따른 동작
        if (isStop)
        {
            movePoint = enemyObject.transform.localScale.x > 0 ? -movePoint : Mathf.Abs(movePoint);
        }
        else
        {
            int direction = Random.Range(0, 2);
            movePoint = direction == 0 ? -movePoint : movePoint;
        }

        // 방향 설정: 이동할 방향에 맞게 로컬 스케일 변경
        if (movePoint > 0)
        {
            enemyObject.transform.localScale = new Vector3(Mathf.Abs(enemyObject.transform.localScale.x), enemyObject.transform.localScale.y, 1);
        }
        else
        {
            enemyObject.transform.localScale = new Vector3(-Mathf.Abs(enemyObject.transform.localScale.x), enemyObject.transform.localScale.y, 1);
        }

        // 애니메이션 설정
        if (anim != null)
        {
            anim.SetBool("isWalk", true);
        }

        // 이동 범위가 originMax를 초과하면 원위치로 돌아옴
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
            .OnComplete(() => OnSequenceComplete())
            .SetLink(enemyObject); // 몹이 파괴되면(풀 초과 등) 같이 끝낸다 — 파괴된 transform 접근 방지
    }

    private void MoveToNewPoint()
    {
        float distanceToMove = Mathf.Abs(movePoint);
        float moveDuration = distanceToMove / desiredSpeed;

        bt.sequence = DOTween.Sequence()
            .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + movePoint, moveDuration).SetEase(Ease.Linear))
            .OnComplete(() => OnSequenceComplete())
            .SetLink(enemyObject); // 몹이 파괴되면(풀 초과 등) 같이 끝낸다 — 파괴된 transform 접근 방지
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
            isEnd = true; // 액션 완료 시 isEnd를 true로 설정
        }
    }

}
