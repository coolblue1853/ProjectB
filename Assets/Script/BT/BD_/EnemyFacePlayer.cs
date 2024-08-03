using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyFacePlayer : EnemyAction
{
    public GameObject damageOb;
    public GameObject attackPivot;
    float direction;
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
        Debug.Log(direction);
        if (direction > 0)
        {
            enemyObject.transform.localScale = new Vector3(tfLocalScale, enemyObject.transform.localScale.y, 1);
        }
        else if (direction < 0)
        {
            enemyObject.transform.localScale = new Vector3(-tfLocalScale, enemyObject.transform.localScale.y, 1);
        }

        OnSequenceComplete();
    }

    private void OnSequenceComplete()
    {
        if (this.transform != null)
        {
            isEnd = true;
        }
    }

}
