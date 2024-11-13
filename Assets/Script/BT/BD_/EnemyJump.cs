using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using UnityEngine;
public class EnemyJump : EnemyAction
{
    public float xJumpForce = 0f;
    public float jumpForce;
    private Vector2 jumpDir;
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
        // ���� ���� �� ����
        body.velocity = new Vector2(body.velocity.x, jumpForce);

        // x ���� ���� �� ����
        float cSize = enemyObject.transform.localScale.x;
        jumpDir = cSize > 0 ? new Vector2(1, 0) : new Vector2(-1, 0);

        body.AddForce(jumpDir.normalized * xJumpForce, ForceMode2D.Impulse);

        // ���� �� �ٷ� �Ϸ� ���·� ����
        OnSequenceComplete();
    }

    private void OnSequenceComplete()
    {
        // ���� �Ϸ� �� ���¸� true�� ����
        isEnd = true;
    }

}
