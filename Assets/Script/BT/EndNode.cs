using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndNode : BTNode
{



    // Start is called before the first frame update
    public EndNode()
    {

    }
    public override NodeState Evaluate()
    {
        brain.EndNode();
        return NodeState.SUCCESS;
    }
}
