using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
public class EnemyCameraShake : EnemyAction
{
    public string ShakeName;
    public GameObject damageOb;
    public GameObject attackPivot;
    float direction;
    public bool isSummonPlayerPosX = false;
    public override void OnStart()
    {
        ShakeCamera(); 

    }
    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }


    void ShakeCamera()
    {
        ProCamera2DShake.Instance.Shake(ShakeName);

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
