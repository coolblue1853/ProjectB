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
        // 필요 시 필요한 컴포넌트 캐싱 (최적화)
        enemySpawner = GetComponent<EnemyHealth>().enemySpowner;
        bt = GetComponent<BehaviorTree>();

        // 이전 액션 종료
        StopAction();
        isEnd = false;

        // 공격 시작
        StartAttack();
    }

    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    private void StartAttack()
    {
        FaceChange();

        // 기존 Sequence가 있으면 정리 후 새 Sequence 설정
        if (bt.sequence != null && bt.sequence.IsActive())
        {
            bt.sequence.Kill();
            bt.sequence = null; // GC 수집을 위해 null 할당
        }

        // 새로운 Sequence 생성 및 실행
        bt.sequence = DOTween.Sequence()
            .AppendCallback(CreatDamageOb)
            .OnComplete(OnSequenceComplete);

        bt.sequence.SetLink(enemyObject); // enemyObject가 파괴될 때 자동 해제 설정
    }

    private void FaceChange()
    {
        // 방향 설정
        if (isApc && behaviorTree.enemy != null)
        {
            direction = Mathf.Sign(behaviorTree.enemy.transform.position.x - enemyObject.transform.position.x);
        }
        else
        {
            var target = behaviorTree.aPC != null ? behaviorTree.aPC.transform : player.transform;
            direction = Mathf.Sign(target.position.x - enemyObject.transform.position.x);
        }

        // 방향에 따른 스케일 변경
        enemyObject.transform.localScale = new Vector3(
            direction > 0 ? tfLocalScale : -tfLocalScale,
            enemyObject.transform.localScale.y, 1);
    }

    private void CreatDamageOb()
    {
        // 공격 오브젝트 생성 및 위치 설정
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

        // 참조 저장
        damageOb = damage;
    }

    private void OnSequenceComplete()
    {
        isEnd = true;
    }
}
