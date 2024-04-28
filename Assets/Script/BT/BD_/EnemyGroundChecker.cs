using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyGroundChecker : EnemyConditional
{
    public GameObject groundCheck2;
    RaycastHit2D hit2;
    public float groundRayLength;
    public override void OnStart()
    {

    }

    public override TaskStatus OnUpdate()
    {
        
        hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, groundRayLength, LayerMask.GetMask("Ground"));
        Debug.DrawRay(groundCheck2.transform.position, Vector2.down, Color.red, groundRayLength);
        if (hit2.collider == null && isJumping == false) //right
        {
   
            return TaskStatus.Success;

        }
        else
        {
            return TaskStatus.Failure;
        }
    }


}
