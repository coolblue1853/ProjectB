
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackNode : BTNode
{
    private Brain ai;
    GameObject player;
    bool isAttack= false;
    public float attackWaitTime;
    public GameObject damageOb;
    public GameObject attackPivot;
    float direction;
    float chInRommSize;
    public GameObject enemyObject;
    public bool isAttackRepeat = true;
    public AttackCheck attackCheck;
    // Start is called before the first frame update
    private void Start()
    {
        chInRommSize = enemyObject.transform.localScale.x;
        player = GameObject.FindWithTag("Player");
        isAttackRepeat = true;
    }
    public AttackNode(Brain brain)
    {

        ai = brain;
    }
    public override NodeState Evaluate()
    {
        if (attackCheck.isNearPlayer == false)
        {
            isAttackRepeat = true;
            return NodeState.SUCCESS;
        }
        else if(isAttack == false)
        {

            Invoke("ReActiveBool", attackWaitTime);
            isAttackRepeat = false;
            brain.StopEvaluateCoroutine();
            sequence = DOTween.Sequence()
           .AppendCallback(() => direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x))
           .AppendCallback(() => FaceChange())
           .AppendCallback(() => CreatDamageOb())
           .AppendInterval(attackWaitTime)
           //  .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
           .OnComplete(() => OnSequenceComplete());
            return NodeState.FAILURE;
        }
        else
        {
            isAttack = false;
            return NodeState.SUCCESS;

        }


    }

    void FaceChange()
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


    public void CreatDamageOb()
    {
        GameObject damageObject = Instantiate(damageOb, attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
    }
    private void ReActiveBool()
    {
        isAttackRepeat = true;
    }
    private void OnSequenceComplete()
    {
        //isAttackRepeat = true;
        if (this.transform != null)
        {
            isAttack = true;
            brain.restartEvaluate();
        }

    }
}
