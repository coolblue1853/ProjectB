using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTInvertor : BTNode
{
    protected BTNode node;

    public BTInvertor(List<BTNode> nodes)
    {
        this.node = node;
    }

    public override NodeState Evaluate()
    {


        switch (node.Evaluate())
        {
            case NodeState.RUNNING:
                _nodeState = NodeState.RUNNING;
                break;
            case NodeState.SUCCESS:
                _nodeState = NodeState.FAILURE;
                break;
            case NodeState.FAILURE:
                _nodeState = NodeState.SUCCESS;
                break;
            case NodeState.WAIT:
                _nodeState = NodeState.WAIT;
                break;
            default:
                break;
        }

        return _nodeState;
    }


}
