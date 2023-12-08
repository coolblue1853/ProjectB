using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndNode : BTNode
{

    public BTBrain ai;

    // Start is called before the first frame update
    public EndNode(BTBrain brain)
    {
        ai = brain;
    }
    public override NodeState Evaluate()
    {
         ai.EndNode();
        return NodeState.SUCCESS;
    }
}
