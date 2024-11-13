using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyGroundChecker : EnemyConditional
{
    BehaviorTree bt;
    public GameObject groundCheck2;
    RaycastHit2D hitRay;
    public float groundRayLength;
    private LayerMask groundLayerMask;
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        // ���̾� ����ũ�� �� ���� ����Ͽ� ����
        groundLayerMask = LayerMask.GetMask("Ground");
    }
    public override TaskStatus OnUpdate()
    {
        // groundCheck2 ��ġ���� �Ʒ��� ����ĳ��Ʈ�� ��
        RaycastHit2D hitRay = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, groundRayLength, groundLayerMask);

        // ����׿� Ray ��� (�ʿ��� ���� Ȱ��ȭ)
       // Debug.DrawRay(groundCheck2.transform.position, Vector2.down * groundRayLength, Color.red);

        // �ٴ��� ���� ���� ������ ������ ���� ��ȯ
        if (hitRay.collider == null && !bt.isJumping)
        {
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
    }
}
