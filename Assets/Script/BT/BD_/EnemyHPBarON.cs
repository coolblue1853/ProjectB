using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class EnemyHPBarON : EnemyAction
{
    public GameObject HPObject;

    public override void OnStart()
    {
        // HPBar Ȱ��ȭ
        ShowHpBar();
    }

    public override TaskStatus OnUpdate()
    {
        // �Ϸ�� ���¸� ���� ��ȯ
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    private void ShowHpBar()
    {
        // HPObject�� �̹� Ȱ��ȭ�Ǿ� ���� ������ Ȱ��ȭ
        if (!HPObject.activeSelf)
        {
            HPObject.SetActive(true);
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
