using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyRunAway : EnemyAction
{
    public RabbitReset rabbitReset;
    public float runTimer;
    bool isEndTime;

    public override void OnStart()
    {
        rabbitReset = enemyObject.GetComponent<RabbitReset>();
           isEnd = false;
        StartPatrol();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartPatrol()
    {


        sequence = DOTween.Sequence()
       .AppendInterval(runTimer)
       .OnComplete(() => OnSequenceComplete());


    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            rabbitReset.ResetRabbit();
        }
    }

}
