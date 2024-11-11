using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

public class EnemyCameraShake : EnemyAction
{
    public string ShakeName;
    private DG.Tweening.Sequence shakeSequence;

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
        // 시퀀스가 없다면 새로 생성, 있다면 재시작
        if (shakeSequence == null)
        {
            shakeSequence = DOTween.Sequence()
             .AppendCallback(() => ProCamera2DShake.Instance.Shake(ShakeName))
            .OnComplete(OnSequenceComplete);

        }
        else
        {
            shakeSequence.Restart(); // 이미 존재하는 시퀀스를 재시작
        }


    }

    private void OnSequenceComplete()
    {
        isEnd = true;
    }

}
