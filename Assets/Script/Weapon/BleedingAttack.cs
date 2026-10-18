/// <summary>출혈: 고정 피해 + 출혈 중인 대상에게 장비 출혈 증뎀 적용. 중첩 수를 세어 마지막 출혈이 끝날 때 해제한다.</summary>
public class BleedingAttack : DotEffectBase
{
    protected override void OnBegin()
    {
        if (enemyHealth != null)
        {
            enemyHealth.AddBleedStack();
        }
    }

    protected override void OnEnd()
    {
        if (enemyHealth != null)
        {
            enemyHealth.RemoveBleedStack();
        }
    }
}
