
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ChaseNode : BTNode
{
    Vector2 originPosition;
    GameObject player;
    public GameObject enemyObject;
    public float moveDuration;
    public float moveDistance;
    float direction;

    bool endChase = false;
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        originPosition = brain.originPosition;
    }
    // Start is called before the first frame update
    public override NodeState Evaluate()
    {
        brain.StopEvaluateCoroutine();
        sequence = DOTween.Sequence()
       .AppendCallback(() => direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x))
       .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
       .OnComplete(() => OnSequenceComplete());
        return NodeState.FAILURE;

    }



    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            endChase = true;
            brain.restartEvaluate();
        }

    }
}
