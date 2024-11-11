using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using UnityEngine;

public class EnemyAttackNode : EnemyAction
{
    private EnemySpawner enemySpawner;
    private BehaviorTree bt;
    private float direction;

    [SerializeField] private string attackName;
    [SerializeField] private bool isApc = false;
    [SerializeField] private bool isSummonPlayerPosX = false;
    [SerializeField] private bool isSetParent = true;
    [SerializeField] private GameObject damageOb;
    [SerializeField] private GameObject attackPivot;

    public override void OnStart()
    {
        // �ʿ� �� �ʿ��� ������Ʈ ĳ�� (����ȭ)
        enemySpawner = GetComponent<EnemyHealth>().enemySpowner;
        bt = GetComponent<BehaviorTree>();

        // ���� �׼� ����
        StopAction();
        isEnd = false;

        // ���� ����
        StartAttack();
    }

    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    private void StartAttack()
    {
        FaceChange();

        // ���� Sequence�� ������ ���� �� �� Sequence ����
        if (bt.sequence != null && bt.sequence.IsActive())
        {
            bt.sequence.Kill();
            bt.sequence = null; // GC ������ ���� null �Ҵ�
        }

        // ���ο� Sequence ���� �� ����
        bt.sequence = DOTween.Sequence()
            .AppendCallback(CreatDamageOb)
            .OnComplete(OnSequenceComplete);

        bt.sequence.SetLink(enemyObject); // enemyObject�� �ı��� �� �ڵ� ���� ����
    }

    private void FaceChange()
    {
        // ���� ����
        if (isApc && behaviorTree.enemy != null)
        {
            direction = Mathf.Sign(behaviorTree.enemy.transform.position.x - enemyObject.transform.position.x);
        }
        else
        {
            var target = behaviorTree.aPC != null ? behaviorTree.aPC.transform : player.transform;
            direction = Mathf.Sign(target.position.x - enemyObject.transform.position.x);
        }

        // ���⿡ ���� ������ ����
        enemyObject.transform.localScale = new Vector3(
            direction > 0 ? tfLocalScale : -tfLocalScale,
            enemyObject.transform.localScale.y, 1);
    }

    private void CreatDamageOb()
    {
        // ���� ������Ʈ ���� �� ��ġ ����
        Vector2 spawnPos = isSummonPlayerPosX
            ? new Vector2(player.transform.position.x, player.transform.position.y - 1f)
            : attackPivot.transform.position;

        var damage = enemySpawner.GetOb(attackName);
        damage.transform.position = spawnPos;
        damage.transform.rotation = attackPivot.transform.rotation;

        if (isSetParent && damage.transform.parent != this.transform)
        {
            damage.transform.SetParent(this.transform);
        }

        damage.transform.localScale = new Vector3(
            Mathf.Abs(damage.transform.localScale.x),
            damage.transform.localScale.y, 1);

        var enemyDamageObject = damage.GetComponent<EnemyDamageObject>();
        enemyDamageObject.enemyOb = this.gameObject;
        if (enemyDamageObject.isLaunch)
        {
            enemyDamageObject.LaunchObject();
        }

        // ���� ����
        damageOb = damage;
    }

    private void OnSequenceComplete()
    {
        isEnd = true;
    }
}
