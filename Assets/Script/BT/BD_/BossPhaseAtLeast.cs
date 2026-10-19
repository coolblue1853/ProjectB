using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 보스 페이즈 조건 (AssetIntegrationPlan 6절 8): BossPhase.Phase 가 minPhase 이상이면 Success.
/// 보스 트리의 셀렉터 앞에 두면 페이즈별 패턴을 고를 수 있다(0 = 기본, 1 = HP 50% 이하 격노, 2 = 25% 이하 광폭).
/// 트리 공유 변수 "Phase"(SharedInt)도 BossPhase 가 같은 값으로 갱신한다.
/// </summary>
[TaskCategory("ProjectB")]
[TaskDescription("BossPhase.Phase >= minPhase 이면 Success")]
public class BossPhaseAtLeast : Conditional
{
    public SharedInt minPhase = 1;

    private BossPhase _phase;

    public override void OnAwake()
    {
        _phase = GetComponent<BossPhase>();
    }

    public override TaskStatus OnUpdate()
    {
        if (_phase == null)
        {
            _phase = GetComponent<BossPhase>(); // 보스는 런타임에 BossPhase 가 붙는다
        }
        return _phase != null && _phase.Phase >= minPhase.Value ? TaskStatus.Success : TaskStatus.Failure;
    }
}
