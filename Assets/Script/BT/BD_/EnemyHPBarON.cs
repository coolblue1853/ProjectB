using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
public class EnemyHPBarON : EnemyAction
{
    public GameObject HPObject;
    public GameObject attackPivot;
    float direction;
    public bool isSummonPlayerPosX = false;
    public override void OnStart()
    {
        isEnd = false;
        ONHpBar();

    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    public void ONHpBar()
    {

        HPObject.SetActive(true);
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
