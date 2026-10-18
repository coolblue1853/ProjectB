using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using UnityEngine;
public class EnemyRunAway : EnemyAction
{
    BehaviorTree bt;
 //   public RabbitReset rabbitReset;
    public float runTimer;
    bool isEndTime;

    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
      //  rabbitReset = enemyObject.GetComponent<RabbitReset>();
           isEnd = false;
        StartPatrol();
    }
    public override TaskStatus OnUpdate()
    {
        if (bt.sequence == null || !bt.sequence.IsActive()) // 반환 정리로 Kill 된 뒤 재사용돼도 안전 (B-141)
        {
            StartPatrol();
        }

        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartPatrol()
    {


        bt.sequence = DOTween.Sequence()
       .AppendInterval(runTimer)
       .OnComplete(() => OnSequenceComplete());


    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
         //   rabbitReset.ResetRabbit();
        }
    }

}
