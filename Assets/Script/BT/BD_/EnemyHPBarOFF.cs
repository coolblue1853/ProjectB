using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class EnemyHPBarOFF : EnemyAction
{
    public GameObject HPObject;

    public override void OnStart()
    {
        // HPBar ��Ȱ��ȭ
        HideHpBar();
    }

    public override TaskStatus OnUpdate()
    {
        // �Ϸ�� ���¸� ���� ��ȯ
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    private void HideHpBar()
    {
        // HPObject�� �̹� ��Ȱ��ȭ �Ǿ� ���� ������ ��Ȱ��ȭ
        if (HPObject.activeSelf)
        {
            HPObject.SetActive(false);
        }

        // ���� �Ϸ�
        OnSequenceComplete();
    }

    private void OnSequenceComplete()
    {
        // ���� �Ϸ� ó��
        isEnd = true;
    }
}
