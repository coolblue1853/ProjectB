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
        // 수직 점프 힘 설정
        body.velocity = new Vector2(body.velocity.x, jumpForce);

        // x 방향 점프 힘 설정
        float cSize = enemyObject.transform.localScale.x;
        jumpDir = cSize > 0 ? new Vector2(1, 0) : new Vector2(-1, 0);

        body.AddForce(jumpDir.normalized * xJumpForce, ForceMode2D.Impulse);

        // 점프 후 바로 완료 상태로 변경
        OnSequenceComplete();
    }

    private void OnSequenceComplete()
    {
        // 점프 완료 시 상태를 true로 설정
        isEnd = true;
    }

}
