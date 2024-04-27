using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyRoundAttackNode : EnemyAction
{


    public float radius = 5f;       // 원의 반지름
    public float startAngle = 0f;   // 시작 각도
    public float endAngle = 360f;   // 끝 각도
    public int bulletCount = 30;    // 생성할 탄막 개수
    public GameObject damageOb;
    public GameObject attackPivot;
    float direction;
    public bool isSummonPlayerPosX = false;
    public override void OnStart()
    {
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
            float angleStep = (endAngle - startAngle) / bulletCount; // 탄막 간격 계산

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + i * angleStep; // 현재 탄막의 각도 계산

                // 오브젝트가 날아가는 방향을 기준으로 회전하기 위해 각도를 계산합니다.
                float adjustedAngle = angle;

                // 적 오브젝트의 방향에 따라 탄막의 각도를 조정합니다.
                float enemyDirection = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x);
                if (enemyDirection < 0)
                {
                    // 적 오브젝트가 반전된 경우 각도도 반전시킵니다.
                    adjustedAngle = 180f - angle;
                }

                // 각도를 라디안으로 변환합니다.
                float angleInRadians = adjustedAngle * Mathf.Deg2Rad;

                // 탄막의 위치 계산
                float x = radius * Mathf.Cos(angleInRadians);
                float y = radius * Mathf.Sin(angleInRadians);

                // 탄막 생성
                GameObject bullet = GameObject.Instantiate(damageOb, attackPivot.transform.position + new Vector3(x, y, 0f), Quaternion.identity, this.transform);
                // 탄막 방향 설정
                bullet.transform.up = new Vector2(x, y).normalized;
            }

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
