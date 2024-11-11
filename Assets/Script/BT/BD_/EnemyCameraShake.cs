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
        // �������� ���ٸ� ���� ����, �ִٸ� �����
        if (shakeSequence == null)
        {
            shakeSequence = DOTween.Sequence()
             .AppendCallback(() => ProCamera2DShake.Instance.Shake(ShakeName))
            .OnComplete(OnSequenceComplete);

        }
        else
        {
            shakeSequence.Restart(); // �̹� �����ϴ� �������� �����
        }


    }

    private void OnSequenceComplete()
    {
        isEnd = true;
    }

}
