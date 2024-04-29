using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using BehaviorDesigner.Runtime;
using UnityEngine;
public class EnemyKillSeq : EnemyAction
{
    BehaviorTree bt;
    public override void OnAwake()
    {
        base.OnAwake();
    }
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        bt.sequence.Kill();
        // StartJump();
        isEnd = true;
    }
    public override TaskStatus OnUpdate()
    {


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
