using UnityEngine;

/// <summary>
/// 보스 페이즈 진입 시 부하 호출 (⑤구역 산적 대장: 2페이즈에 단검병 2명).
/// BossPhase.PhaseEntered 를 듣고, 자기 스포너(EnemySpawner.SpawnMinion)의 부하 풀에서 꺼내 양옆에 세운다.
/// 한 번 부른 페이즈는 다시 부르지 않는다(풀 재사용 시 OnEnable 에서 초기화).
/// </summary>
public class BossMinionCall : MonoBehaviour
{
    [SerializeField] private string _minionKey = EnemySpawner.MinionKey;
    [SerializeField] private int _atPhase = 1;          // 0-기준: 1 = 2페이즈
    [SerializeField] private int _count = 2;
    [SerializeField] private float _spread = 2.5f;

    private EnemyHealth _health;
    private bool _called;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        _called = false;
        BossPhase.PhaseEntered += OnPhaseEntered;
    }

    private void OnDisable()
    {
        BossPhase.PhaseEntered -= OnPhaseEntered;
    }

    private void OnPhaseEntered(BossPhase boss, int phase)
    {
        if (_called || boss.gameObject != gameObject || phase < _atPhase || _health == null || _health.enemySpowner == null)
        {
            return;
        }
        _called = true;
        for (int i = 0; i < _count; i++)
        {
            float side = i % 2 == 0 ? 1f : -1f;
            float dist = _spread * (1 + i / 2);
            _health.enemySpowner.SpawnMinion(_minionKey, transform.position + new Vector3(side * dist, 0.5f, 0f));
        }
        BossBanner.Show(name.Replace("(Clone)", "").Replace("_BD", "").Trim(), "ENRAGED — Calls the gang!", true); // 페이즈 배너를 대신한다
    }
}
