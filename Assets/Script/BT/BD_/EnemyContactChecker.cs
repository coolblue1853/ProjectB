using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyContactChecker : EnemyConditional
{

    public SharedBool isChecked =false;

    public override TaskStatus OnUpdate()
    {
        if(isChecked.Value == false)
        {
            isChecked = true;
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
    }



}
