using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyWallChecker : EnemyConditional
{
    public GameObject wallCheck;
    RaycastHit2D hit2;
    public float wallRay = 0.2f;
    public override void OnStart()
    {
        chInRommSize = enemyObject.transform.localScale.x;


    }
    public bool isGroundCheck = false;

    public override TaskStatus OnUpdate()
    {
        if(chInRommSize > 0)
        {
            hit2 = Physics2D.Raycast(wallCheck.transform.position, Vector2.right, wallRay, LayerMask.GetMask("Ground"));
            Debug.DrawRay(wallCheck.transform.position, Vector2.right, Color.blue, wallRay);
        }
        else
        {
            hit2 = Physics2D.Raycast(wallCheck.transform.position, Vector2.left, wallRay, LayerMask.GetMask("Ground"));
            Debug.DrawRay(wallCheck.transform.position, Vector2.left, Color.blue, wallRay);
        }

        if (hit2.collider == null) //right
        {

            return TaskStatus.Failure;

        }
        else
        {
            return TaskStatus.Success;
        }
    }


}
