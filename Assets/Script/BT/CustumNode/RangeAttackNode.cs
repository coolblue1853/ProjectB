using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttackNode : BTNode
{
    private Brain ai;

    // Start is called before the first frame update
    public RangeAttackNode(Brain brain)
    {
        ai = brain;
    }
    public override NodeState Evaluate()
    {
        ai.enemyAttack.RangeAttack();
        return NodeState.SUCCESS;
    }
}
