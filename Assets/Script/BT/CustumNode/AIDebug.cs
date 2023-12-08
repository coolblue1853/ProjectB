using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AIDebug : BTNode
{
    public string message;

    public BTBrain ai;

    // Start is called before the first frame update
    public AIDebug(BTBrain brain, string debug)
    {
        message = debug;
        ai = brain;
    }
    public override NodeState Evaluate()
    {
        if(IsWaiting == true)
        {
            if(ai.isEnd == true)
            {
                IsWaiting = false;
                return NodeState.FAILURE;
            }
            return NodeState.SUCCESS;
        }
        else
        {
            Sequence sequence = DOTween.Sequence()
           .AppendInterval(waitTime) // 대기 시간 사용
           .OnComplete(() => ai.StopEvaluateCoroutine())
           .OnComplete(() => OnSequenceComplete());
            Debug.Log("작동중");
            return NodeState.FAILURE;
        }

    }

    private void OnSequenceComplete()
    {
        Debug.Log(message);
        // 시퀀스가 끝나면 SUCCESS로 변경합니다.
        IsWaiting = true;
        ai.restartEvaluate();
    }
}
