using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class HitChaseNode : BTNode
{
    Vector2 originPosition;
    GameObject player;
    public GameObject enemyObject;
    public float moveDuration;
    public float moveDistance;
    float direction;
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        originPosition = brain.originPosition;
    }
    // Start is called before the first frame update
    public override NodeState Evaluate()
    {
        if (brain.isAttacked == false)
        {
            return NodeState.SUCCESS;
        }
        else
        {
            brain.StopEvaluateCoroutine();
           // brain.isAttacked = false;
            sequence = DOTween.Sequence()
           .AppendCallback(() => direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x))
           .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
           //.Append(enemyObject.transform.DOMoveX(player.transform.position.x, Mathf.Abs(player.transform.position.x - enemyObject.transform.position.x) / 2))
           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }

    }



    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            brain.restartEvaluate();
        }

    }
}
