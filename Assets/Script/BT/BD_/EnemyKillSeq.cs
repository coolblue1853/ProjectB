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

        // BehaviorTree���� sequence�� ����
        bt.sequence.Kill();

        // Jump ������ �ʿ��ϴٸ� StartJump ȣ��
        StartJump();
        isEnd = true;
    }

    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    public void StartJump()
    {
        // transform�� ���ε��� Ʈ���� ��� ����
        DOTween.Kill(this.transform);

        // ���� ��ġ���� ��ü ���߱�
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        OnSequenceComplete();
    }

    private void OnSequenceComplete()
    {
        // �׼� ���� �÷��� ����
        isEnd = true;
    }
}
