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
        isEnd = false; // 재사용(풀 재활용·BT 재진입) 때 이전 완료 값이 남아 흔들림 없이 끝나지 않게
        ShakeCamera();
    }

    public override TaskStatus OnUpdate()
    {
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    void ShakeCamera()
    {
        // 끝난 시퀀스는 DOTween 이 자동 Kill 하므로 Restart 가 먹지 않는다 — 매번 새로 만든다
        shakeSequence.Kill();
        shakeSequence = DOTween.Sequence()
            .AppendCallback(() => ProCamera2DShake.Instance.Shake(ShakeName))
            .OnComplete(OnSequenceComplete)
            .SetLink(gameObject);


    }

    private void OnSequenceComplete()
    {
        isEnd = true;
    }

}
