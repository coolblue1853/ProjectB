using System.Collections;
using System.Collections.Generic;


public class ChasePlayer : BTNode
{


    Brain brain;

    // Start is called before the first frame update
    public ChasePlayer( Brain ai)
    {
     
        brain = ai;
    }
    public override NodeState Evaluate()
    {
        if (!brain.nearPlayer)
        {



        }

        return NodeState.SUCCESS;
    }
}
