using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyAttackNode : EnemyAction
{
    public GameObject damageOb;
    public GameObject attackPivot;
    float direction;
    public bool isSummonPlayerPosX = false;
    public override void OnStart()
    {
        StopAction();
        isEnd = false;
        StartPatrol();

    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void StartPatrol()
    {
        if (anim != null)
        {
            anim.SetBool("isAttack", true);

        }
        FaceChange();
        sequence = DOTween.Sequence()
       .AppendCallback(() => CreatDamageOb())
       //  .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
       .OnComplete(() => OnSequenceComplete());

    }

    void FaceChange()
    {
        direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x);
        Debug.Log(direction);
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
        if (isSummonPlayerPosX == true)
        {
            var damage = Object.Instantiate(damageOb, new Vector2(player.transform.position.x, attackPivot.transform.position.y), attackPivot.transform.rotation);
   

        }
        else
        {
            var damage = Object.Instantiate(damageOb, attackPivot.transform.position, attackPivot.transform.rotation, this.transform);

        }




    }
    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            isEnd = true;
        }
    }

}
