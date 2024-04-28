using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyJump : EnemyAction
{
    
    public float jumpForce;
    Vector2 jumpDir;
    public override void OnStart()
    {

        StartJump();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartJump()
    {

        //StopAction();
       body.velocity = new Vector2(body.velocity.x+3, jumpForce);


        //body.AddForce(jumpDir.normalized * jumpForce, ForceMode2D.Impulse);

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
