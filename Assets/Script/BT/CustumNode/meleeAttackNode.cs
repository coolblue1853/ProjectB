using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meleeAttackNode : BTNode
{
    private Brain ai;

    // Start is called before the first frame update
    public meleeAttackNode(Brain brain)
    {
        ai = brain;
    }
    public override NodeState Evaluate()
    {
        ai.enemyAttack.meleeAttack();
        return NodeState.SUCCESS;
    }
}
