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
        // sequence가 비활성화된 상태인지 확인하여 종료 여부를 결정
        if (bt.sequence == null || !bt.sequence.IsActive())
        {
            isEnd = true;
        }
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    public void StartChase()
    {
        GameObject target;
        if (isChaseEnemy == false)
        {
            if(behaviorTree.aPC == null)
                target = player;
            else
                target = behaviorTree.aPC;
        }
        else
            target = behaviorTree.enemy;

        bt.sequence = DOTween.Sequence()
.AppendCallback(() => direction = Mathf.Sign(target.transform.position.x - enemyObject.transform.position.x))
.AppendCallback(() => ChangeFace())
.Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
.OnComplete(() => OnSequenceComplete());

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
