using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyGroundChecker : EnemyConditional
{
    BehaviorTree bt;
    public GameObject groundCheck2;
    RaycastHit2D hit2;
    RaycastHit2D hit3;
    public float groundRayLength;
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
    }

    public override TaskStatus OnUpdate()
    {
        
        hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, groundRayLength, LayerMask.GetMask("Ground"));
      //  hit3 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, groundRayLength, LayerMask.GetMask("Wall"));
        Debug.DrawRay(groundCheck2.transform.position, Vector2.down, Color.red, groundRayLength);
        if ((hit2.collider == null || hit3.collider != null )&& bt.isJumping == false) //right
        {
            return TaskStatus.Success;

        }
        else
        {
            return TaskStatus.Failure;
        }
    }


}
