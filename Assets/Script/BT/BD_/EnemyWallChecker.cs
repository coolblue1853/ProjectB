using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;

public class EnemyWallChecker : EnemyConditional
{
    private BehaviorTree bt;
    public GameObject wallCheck;
    public float wallRay = 0.2f;

    public override void OnStart()
    {
        bt = GetComponent<BehaviorTree>();
    }

    public override TaskStatus OnUpdate()
    {
        // ���� ���� ���⿡ ���� ���� ������ �����մϴ�.
        Vector2 rayDirection = enemyObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // �� ���� ����ĳ��Ʈ ����
        RaycastHit2D hit2 = Physics2D.Raycast(wallCheck.transform.position, rayDirection, wallRay, LayerMask.GetMask("Wall"));

        // ����� ���� �׸���
     //   Debug.DrawRay(wallCheck.transform.position, rayDirection * wallRay, Color.blue);

        // ����ĳ��Ʈ ����� ���� TaskStatus ��ȯ
        return hit2.collider != null ? TaskStatus.Success : TaskStatus.Failure;
    }
}
