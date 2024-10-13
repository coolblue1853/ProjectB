using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using UnityEngine;

public class EnemyAttackNode : EnemyAction
{
    public EnemySpawner enemySpowner;
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
        enemySpowner = this.transform.GetComponent<EnemyHealth>().enemySpowner;
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

        // 기존 Sequence가 있으면 정리
        if (bt.sequence != null && bt.sequence.IsActive())
        {
            bt.sequence.Kill();
            bt.sequence = null; // GC 수집을 위해 null 할당
        }

        // 새 Sequence 설정
        bt.sequence = DOTween.Sequence()
            .AppendCallback(() => CreatDamageOb())
            .OnComplete(() => OnSequenceComplete());
    }

    void FaceChange()
    {
        if (isApc)
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

        // 방향에 따른 스케일 변경
        enemyObject.transform.localScale = new Vector3(
            direction > 0 ? tfLocalScale : -tfLocalScale,
            enemyObject.transform.localScale.y, 1);
    }

    public void CreatDamageOb()
    {
        GameObject damageObject;
        Vector2 spawnPos = isSummonPlayerPosX
            ? new Vector2(player.transform.position.x, player.transform.position.y - 1f)
            : attackPivot.transform.position;

        var damage = enemySpowner.GetOb(attackName);
        damage.transform.position = spawnPos;
        damage.transform.rotation = attackPivot.transform.rotation;

        if (isSetParent && damage.transform.parent != this.transform)
        {
            damage.transform.SetParent(this.transform);
        }

        damageObject = damage;
        damageObject.transform.localScale = new Vector3(
            Mathf.Abs(damageObject.transform.localScale.x),
            damageObject.transform.localScale.y, 1);

        var enemyDamageObject = damageObject.GetComponent<EnemyDamageObject>();
        if (enemyDamageObject.isLaunch) enemyDamageObject.LaunchObject();
    }

    private void OnSequenceComplete()
    {
        isEnd = true;
    }
}
