using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndNode : BTNode
{
    public bool isNRepeat = false;
    public string state;

    // Start is called before the first frame update
    public EndNode()
    {

    }
    public override NodeState Evaluate()
    {
        if (isNRepeat)
        {
            brain.EndNodeState(state);
        }
        else
        {
            brain.EndNode();
        }

        return NodeState.SUCCESS;
    }
}
