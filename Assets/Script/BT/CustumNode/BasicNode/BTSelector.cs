using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSelector : BTNode
{
    protected List<BTNode> nodes = new List<BTNode>();

    public BTSelector(List<BTNode> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeState Evaluate()
    {

        foreach(var node in nodes)
        {
            switch (node.Evaluate())
            {

                case NodeState.RUNNING:
                    _nodeState = NodeState.RUNNING;
                    return _nodeState;
 
                case NodeState.SUCCESS:
                    _nodeState = NodeState.SUCCESS;
                    return _nodeState;
                case NodeState.FAILURE:
                    break;
                default:
                    break;
            }
        }
        _nodeState = NodeState.FAILURE;
        return _nodeState;
    }

}
