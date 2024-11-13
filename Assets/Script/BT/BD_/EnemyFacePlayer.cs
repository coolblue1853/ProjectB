using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyFacePlayer : EnemyAction
{
    public GameObject damageOb;
    public GameObject attackPivot;
    private float direction;
    public bool isSummonPlayerPosX = false;

    public override void OnStart()
    {
        FaceChange();
    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    void FaceChange()
    {
        direction = Mathf.Sign(player.transform.position.x - enemyObject.transform.position.x);
        // ���⿡ ���� ���� ������ ����
        enemyObject.transform.localScale = new Vector3(
            direction > 0 ? tfLocalScale : -tfLocalScale,
            enemyObject.transform.localScale.y, 1
        );

        if (this.transform != null)
        {
            isEnd = true;
        }
    }
}
