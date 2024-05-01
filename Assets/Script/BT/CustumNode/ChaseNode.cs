using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ChaseNode : BTNode
{
    BehaviorTree bt;
    Animator anim;
    Vector2 originPosition;
  
    public GameObject enemyObject;
    GameObject player;
    public float moveDuration;
    public float moveDistance;
    float direction;
    float chInRommSize;
    bool endChase = false;
    private void Start()
    {
        
        anim = transform.parent.parent.GetComponent<Animator>();
        chInRommSize = enemyObject.transform.localScale.x;
        player = GameObject.FindWithTag("Player");
        originPosition = brain.originPosition;
    }
    // Start is called before the first frame update
    public override NodeState Evaluate()
    {
        if (anim != null)
        {
            anim.SetBool("isWalk", true);
        }
        brain.StopEvaluateCoroutine();
        sequence = DOTween.Sequence()
       .AppendCallback(() => direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x))
        .AppendCallback(() => ChangeFace())
       .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
       .OnComplete(() => OnSequenceComplete());
        return NodeState.FAILURE;

    }

    void ChangeFace()
    {
        if (direction > 0)
        {
            enemyObject.transform.localScale = new Vector3(chInRommSize, enemyObject.transform.localScale.y, 1);
        }
        else if (direction < 0)
        {
            enemyObject.transform.localScale = new Vector3(-chInRommSize, enemyObject.transform.localScale.y, 1);
        }
    }

    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            if (anim != null)
            {
                anim.SetBool("isWalk", false);
            }
            endChase = true;
            brain.restartEvaluate();
        }

    }
}
