using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAttackNode : BTNode
{
    private Brain ai;

    // Start is called before the first frame update
    public PointAttackNode(Brain brain)
    {
        ai = brain;
    }
    public override NodeState Evaluate()
    {
        ai.enemyAttack.PointAttack();
        return NodeState.SUCCESS;
    }
}
