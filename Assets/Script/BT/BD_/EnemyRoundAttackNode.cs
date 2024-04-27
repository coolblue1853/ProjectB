using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyRoundAttackNode : EnemyAction
{


    public float radius = 5f;       // ���� ������
    public float startAngle = 0f;   // ���� ����
    public float endAngle = 360f;   // �� ����
    public int bulletCount = 30;    // ������ ź�� ����
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
            float angleStep = (endAngle - startAngle) / bulletCount; // ź�� ���� ���

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + i * angleStep; // ���� ź���� ���� ���

                // ������Ʈ�� ���ư��� ������ �������� ȸ���ϱ� ���� ������ ����մϴ�.
                float adjustedAngle = angle;

                // �� ������Ʈ�� ���⿡ ���� ź���� ������ �����մϴ�.
                float enemyDirection = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x);
                if (enemyDirection < 0)
                {
                    // �� ������Ʈ�� ������ ��� ������ ������ŵ�ϴ�.
                    adjustedAngle = 180f - angle;
                }

                // ������ �������� ��ȯ�մϴ�.
                float angleInRadians = adjustedAngle * Mathf.Deg2Rad;

                // ź���� ��ġ ���
                float x = radius * Mathf.Cos(angleInRadians);
                float y = radius * Mathf.Sin(angleInRadians);

                // ź�� ����
                GameObject bullet = GameObject.Instantiate(damageOb, attackPivot.transform.position + new Vector3(x, y, 0f), Quaternion.identity, this.transform);
                // ź�� ���� ����
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
