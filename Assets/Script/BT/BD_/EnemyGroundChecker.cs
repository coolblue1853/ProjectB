using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyGroundChecker : EnemyConditional
{
    public GameObject groundCheck2;
    RaycastHit2D hit2;

    public override void OnStart()
    {

    }
    public bool isGroundCheck = false;

    void GroundCheck()
    {
        isGroundCheck = true;

    }
    public override TaskStatus OnUpdate()
    {
        hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(groundCheck2.transform.position, Vector2.down, Color.red, 0.3f);
        if (hit2.collider == null) //right
        {
            StopAllActions();
            return TaskStatus.Success;

        }
        else
        {
            return TaskStatus.Failure;
        }
    }


}
