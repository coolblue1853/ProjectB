using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
public class EnemyWallChecker : EnemyConditional
{
    BehaviorTree bt;
    public GameObject wallCheck;
    RaycastHit2D hit2;
    public float wallRay = 0.2f;
    public Vector2 startPos;
    public Vector2 EndPos;
     bool isEndWait = false;
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
        startPos = this.transform.position;
        /* �� �κ��� �־ kill �� �۵����� �ʴ� ���̾���. �����ÿ� ���ƹ����� ������ �߻��ϸ� ���� �ٲپ���.
       bt.isJumping = true;
       bt.sequence.Kill();
        bt.sequence = DOTween.Sequence()
           .AppendInterval(2f)
         .OnComplete(() => bt.isJumping = false);
        */


    }
    public bool isGroundCheck = false;

    public override TaskStatus OnUpdate()
    {
        chInRommSize = enemyObject.transform.localScale.x;
        if (chInRommSize > 0)
        {
            hit2 = Physics2D.Raycast(wallCheck.transform.position, Vector2.right, wallRay, LayerMask.GetMask("Wall"));
            Debug.DrawRay(wallCheck.transform.position, Vector2.right, Color.blue, wallRay);
        }
        else
        {
            hit2 = Physics2D.Raycast(wallCheck.transform.position, Vector2.left, wallRay, LayerMask.GetMask("Wall"));
            Debug.DrawRay(wallCheck.transform.position, Vector2.left, Color.blue, wallRay);
        }

        if (hit2.collider == null) //right
        {
            return TaskStatus.Failure;

        }
        else
        {

            /*
            EndPos = this.transform.position;
            if(Mathf.Abs(startPos.x)- Mathf.Abs(EndPos.x) < 0.1 || Mathf.Abs(startPos.y) - Mathf.Abs(EndPos.y) < 0.1)
            {
                Debug.Log("���� ����2");
                return TaskStatus.Failure;
            }
            */
            return TaskStatus.Success;
        }
    }


}
