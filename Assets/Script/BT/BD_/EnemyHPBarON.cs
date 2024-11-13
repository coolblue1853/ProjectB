using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class EnemyHPBarON : EnemyAction
{
    public GameObject HPObject;

    public override void OnStart()
    {
        // HPBar 활성화
        ShowHpBar();
    }

    public override TaskStatus OnUpdate()
    {
        // 완료된 상태면 성공 반환
        return isEnd ? TaskStatus.Success : TaskStatus.Running;
    }

    private void ShowHpBar()
    {
        // HPObject가 이미 활성화되어 있지 않으면 활성화
        if (!HPObject.activeSelf)
        {
            HPObject.SetActive(true);
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
