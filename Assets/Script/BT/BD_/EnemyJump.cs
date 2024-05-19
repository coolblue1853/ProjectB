using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using UnityEngine;
public class EnemyJump : EnemyAction
{
    public float xJumpForce = 0f;
    public float jumpForce;
    Vector2 jumpDir = new Vector2(1,0);
    BehaviorTree bt;
    public float jumpWaitTime = 0.2f;
    public Vector2 startPos;
    
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();

        StartJump();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartJump()
    {
        body.velocity = new Vector2(body.velocity.x, jumpForce);
        body.AddForce(jumpDir.normalized * xJumpForce, ForceMode2D.Impulse);

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
