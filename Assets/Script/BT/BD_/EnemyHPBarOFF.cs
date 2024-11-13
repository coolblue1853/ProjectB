using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class EnemyHPBarOFF : EnemyAction
{
    public GameObject HPObject;

    public override void OnStart()
    {
        // HPBar 비활성화
        HideHpBar();
    }

    public override TaskStatus OnUpdate()
    {
        // 완료된 상태면 성공 반환
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    private void HideHpBar()
    {
        // HPObject가 이미 비활성화 되어 있지 않으면 비활성화
        if (HPObject.activeSelf)
        {
            HPObject.SetActive(false);
        }

        // 상태 완료
        OnSequenceComplete();
    }

    private void OnSequenceComplete()
    {
        // 상태 완료 처리
        isEnd = true;
    }
}
