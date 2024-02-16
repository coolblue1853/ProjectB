using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AIDebug : BTNode
{
    public string message;


    // Start is called before the first frame update
    public AIDebug(string debug)
    {
        message = debug;

    }
    public override NodeState Evaluate()
    {
        if(IsWaiting == true)
        {
            if(brain.isEnd == true)
            {
                IsWaiting = false;
                return NodeState.FAILURE;
            }
            return NodeState.SUCCESS;
        }
        else
        {
             sequence = DOTween.Sequence()
           .AppendInterval(waitTime) // 대기 시간 사용
           .OnComplete(() => brain.StopEvaluateCoroutine())
           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }

    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            Debug.Log("CompWait");
            IsWaiting = true;
            brain.restartEvaluate();
        }
    }
}
