using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class EnemyKillSeq : EnemyAction
{
    private BehaviorTree bt;

    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();

        // BehaviorTree에서 sequence를 종료
        bt.sequence.Kill();

        // Jump 로직이 필요하다면 StartJump 호출
        StartJump();
        isEnd = true;
    }

    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    public void StartJump()
    {
        // transform에 바인딩된 트윈을 모두 종료
        DOTween.Kill(this.transform);

        // 현재 위치에서 객체 멈추기
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        OnSequenceComplete();
    }

    private void OnSequenceComplete()
    {
        // 액션 종료 플래그 설정
        isEnd = true;
    }
}
