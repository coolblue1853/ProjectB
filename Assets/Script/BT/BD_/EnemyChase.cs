using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
using BehaviorDesigner.Runtime;
public class EnemyChase : EnemyAction
{
    private BehaviorTree bt;
    private GameObject player;
    [SerializeField] private float moveDuration;
    [SerializeField] private float moveDistance;
    private float direction;
    [SerializeField] private bool isChaseEnemy = false;

    public override void OnStart()
    {
        bt = GetComponent<BehaviorTree>();
        isEnd = false;

        // Player 객체 캐싱
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        StartChase();
    }
    public override TaskStatus OnUpdate()
    {
        // sequence가 비활성화된 상태인지 확인하여 종료 여부를 결정 (다른 노드가 Kill 하면 추적 중단)
        if (bt.sequence == null || !bt.sequence.IsActive())
        {
            isEnd = true;
        }
        else if (chaseTarget != null)
        {
            // 매 프레임 연속 이동 (C3): 예전엔 0.1초 DOMoveX 트윈을 반복해 걸음이 끊겨 보였다. 속도는 그대로(거리/시간)
            float dx = chaseTarget.transform.position.x - enemyObject.transform.position.x;
            if (Mathf.Abs(dx) > TurnDeadZone && Mathf.Sign(dx) != direction)
            {
                direction = Mathf.Sign(dx);
                ChangeFace();
            }
            Vector3 p = enemyObject.transform.position;
            p.x += direction * chaseSpeed * Time.deltaTime;
            enemyObject.transform.position = p;
        }
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    const float TurnDeadZone = 0.25f; // 목표 바로 위에서 좌우로 떨리지 않게
    GameObject chaseTarget;
    float chaseSpeed;

    public void StartChase()
    {
        if (isChaseEnemy == false)
        {
            chaseTarget = behaviorTree.aPC == null ? player : behaviorTree.aPC;
        }
        else
        {
            chaseTarget = behaviorTree.enemy;
        }
        if (chaseTarget == null)
        {
            isEnd = true;
            return;
        }
        direction = Mathf.Sign(chaseTarget.transform.position.x - enemyObject.transform.position.x);
        ChangeFace();
        chaseSpeed = moveDuration > 0f ? moveDistance / moveDuration : 0f;
        bt.sequence?.Kill();
        bt.sequence = DOTween.Sequence()
            .AppendInterval(moveDuration)
            .OnComplete(() => OnSequenceComplete());
        bt.sequence.SetLink(enemyObject);
    }
    void ChangeFace()
    {
        if (direction > 0)
        {
            enemyObject.transform.localScale = new Vector3(tfLocalScale, enemyObject.transform.localScale.y, 1);
        }
        else if (direction < 0)
        {
            enemyObject.transform.localScale = new Vector3(-tfLocalScale, enemyObject.transform.localScale.y, 1);
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
