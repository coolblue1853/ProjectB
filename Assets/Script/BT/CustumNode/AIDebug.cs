using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AIDebug : BTNode
{
    public string message;
    Animator anim;
    public float minWait;
    public float maxWait;

    // Start is called before the first frame update
    public AIDebug(string debug)
    {
        message = debug;
        anim = transform.parent.parent.GetComponent<Animator>();
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
            if (anim != null)
            {
                anim.SetBool("isWalk", false);
            }

            float waitTime = Random.Range(minWait, maxWait);
            brain.StopEvaluateCoroutine();
             sequence = DOTween.Sequence()
           .AppendInterval(waitTime) // 대기 시간 사용

           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }

    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
        //    Debug.Log("CompWait");
            IsWaiting = true;
            brain.restartEvaluate();
        }
    }
}
