using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyKillSeq : EnemyAction
{
    public override void OnAwake()
    {
        base.OnAwake();
    }
    public override void OnStart()
    {

       // StartJump();
    }
    public override TaskStatus OnUpdate()
    {
        if (sequence != null && sequence.IsActive())
        {
            sequence.Kill();
        }
        else if(sequence.IsActive() == false)
        {
            OnSequenceComplete();
        }

            return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartJump()
    {

        DOTween.Kill(this.transform);
        sequence.Kill();
        // ÇöÀç À§Ä¡¿¡ °´Ã¼¸¦ ¸ØÃã
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);



        OnSequenceComplete();
    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            isEnd = true;
        }
    }

}
