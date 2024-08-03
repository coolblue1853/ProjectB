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
    public override void OnStart()
    {
        bt = this.transform.GetComponent<BehaviorTree>();
    }
    public override TaskStatus OnUpdate()
    {
        hitRay = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, groundRayLength, LayerMask.GetMask("Ground"));
        Debug.DrawRay(groundCheck2.transform.position, Vector2.down, Color.red, groundRayLength);
        if ((hitRay.collider == null )&& bt.isJumping == false) 
        {
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
    }
}
