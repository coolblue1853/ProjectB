using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyHealthChecker : EnemyConditional
{
    public SharedInt healthTreshold;
    public SharedBool isHitChecker;
    public override TaskStatus OnUpdate()
    {
        if(isHitChecker.Value == false)
        {
            return enemyHealth.nowHP < healthTreshold.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
        else
        {
            return enemyHealth.nowHP != enemyHealth.maxHP ? TaskStatus.Success : TaskStatus.Failure;
        }

    }


}
