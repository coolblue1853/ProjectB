using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using UnityEngine;
public class EnemyAttackNode : EnemyAction
{
    public EnemySpowner enemySpowner;
    public string attackName;
    public bool isApc = false;
    BehaviorTree bt;
    public GameObject damageOb;
    public GameObject attackPivot;
    float direction;
    public bool isSummonPlayerPosX = false;
    public bool isSetParent = true;
   
    public override void OnStart()
    {
        enemySpowner = this.transform.GetComponent<EnemyHealth>().enemySpowner  ;
        bt = this.transform.GetComponent<BehaviorTree>();
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

        FaceChange();
        bt.sequence = DOTween.Sequence()
       .AppendCallback(() => CreatDamageOb())
       //  .Append(enemyObject.transform.DOMoveX(enemyObject.transform.position.x + moveDistance * direction, moveDuration).SetEase(Ease.Linear))
       .OnComplete(() => OnSequenceComplete());

    }

    void FaceChange()
    {

        if (isApc == true)
        {
            if (behaviorTree.enemy != null)
            {
                direction = Mathf.Sign(behaviorTree.enemy.transform.position.x - enemyObject.transform.position.x);
            }
        }
        else
        {
            if (behaviorTree.aPC == null)
            {
                direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x);
            }
            else
            {
                direction = Mathf.Sign(behaviorTree.aPC.transform.position.x - enemyObject.transform.position.x);
            }
        }



        if (direction > 0)
        {
            enemyObject.transform.localScale = new Vector3(tfLocalScale, enemyObject.transform.localScale.y, 1);
        }
        else if (direction < 0)
        {
            enemyObject.transform.localScale = new Vector3(-tfLocalScale, enemyObject.transform.localScale.y, 1);
        }


    }

    public void CreatDamageOb()
    {
        GameObject damageObject;
        if (isSummonPlayerPosX == true)
        {
            // var damage = Object.Instantiate(damageOb, new Vector2(player.transform.position.x, player.transform.position.y - 1f), attackPivot.transform.rotation);
            var damage = enemySpowner.GetGo(attackName);
            damage.transform.position = new Vector2(player.transform.position.x, player.transform.position.y - 1f);
            damage.transform.rotation = attackPivot.transform.rotation;
            damageObject = damage;
            if (direction > 0)
            {
                damageObject.transform.localScale = new Vector3(damageObject.transform.localScale.x, damageObject.transform.localScale.y, 1);
            }
            else if (direction < 0)
            {
                damageObject.transform.localScale = new Vector3(-Mathf.Abs(damageObject.transform.localScale.x), damageObject.transform.localScale.y, 1);
            }

        }
        else
        {
            //var damage = Object.Instantiate(damageOb, attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
            var damage = enemySpowner.GetGo(attackName);
            damage.transform.position = attackPivot.transform.position;
            damage.transform.rotation = attackPivot.transform.rotation;
            if(isSetParent)
                damage.transform.parent = this.transform;
            damageObject = damage;
            damageObject.transform.localScale = new Vector3(Mathf.Abs(damageObject.transform.localScale.x), damageObject.transform.localScale.y, 1);
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
