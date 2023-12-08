using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSequence : BTNode
{
    protected List<BTNode> nodes = new List<BTNode>();

    public enum NODETYPE
    {
        SETDIR, WAIT, FINDMOVE1, FINDMOVE2, MOVE,
    }
    public BTSequence(List<BTNode> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        bool isAnyNodeRunning = false;
        foreach (var node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.RUNNING:
                    isAnyNodeRunning = true;
                    break;
                case NodeState.SUCCESS:
                    break;
                case NodeState.FAILURE:
                    _nodeState = NodeState.FAILURE;
                    return _nodeState;
                case NodeState.WAIT:
                    // WAIT 노드가 있다면 해당 대기 시간을 사용
                    waitTime = node.waitTime;
                    return NodeState.WAIT;
                default:
                    break;
            }
        }
        _nodeState = isAnyNodeRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        return _nodeState;
    }

    public BTNode GetNode(NODETYPE nodeType)
    {
        switch (nodeType)
        {
            case NODETYPE.WAIT:
                return nodes.Find(node => node.nodeState == NodeState.WAIT);
            default:
                return null;
        }
    }
}