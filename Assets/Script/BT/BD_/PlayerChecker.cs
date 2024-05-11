using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class PlayerChecker : EnemyConditional
{
    public Vector2 boxSize ; // Ȯ���� ���簢�� ������ ũ��
    public LayerMask layerMask; // ������ ���̾�
    public string playerTag = "Player"; // �÷��̾� �±�

    public override TaskStatus OnUpdate()
    {
        // �ֺ��� �ִ� ��� Collider�� ������
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f, layerMask);

        // ������ Collider�� ��ȸ�ϸ鼭 Player �±׸� ���� ������Ʈ�� �ִ��� Ȯ��
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(playerTag))
            {
                // Player �±׸� ���� ������Ʈ�� ������ Success ��ȯ
                return TaskStatus.Success;
            }
        }

        // �ֺ��� Player �±׸� ���� ������Ʈ�� ������ Failure ��ȯ
        return TaskStatus.Failure;
    }


}