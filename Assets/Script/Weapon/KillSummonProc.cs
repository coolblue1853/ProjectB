using UnityEngine;

/// <summary>
/// 보스 고유 무기(Primordial Gel): 이 무기를 장착한 채 적을 처치하면 확률로 아군 슬라임을 소환한다.
/// 무기 프리팹 루트에 붙인다. 장착 중인 무기일 때만 반응한다(EnemyHealth.Killed 정적 이벤트 구독).
/// 소환된 슬라임의 처치도 다시 굴린다(의도: 연쇄 가능, _cooldown 으로 폭주 방지).
/// </summary>
[RequireComponent(typeof(Weapon))]
public class KillSummonProc : MonoBehaviour
{
    [SerializeField] private GameObject _summonPrefab;
    [SerializeField, Range(0f, 1f)] private float _chance = 0.35f;
    [SerializeField] private float _cooldown = 0.5f;

    private Weapon _weapon;
    private float _nextTime;

    private void Awake()
    {
        _weapon = GetComponent<Weapon>();
    }

    private void OnEnable()
    {
        EnemyHealth.Killed += OnKilled;
    }

    private void OnDisable()
    {
        EnemyHealth.Killed -= OnKilled;
    }

    private void OnKilled(EnemyHealth enemy)
    {
        if (_summonPrefab == null || enemy == null || Weapon.Equipped != _weapon || Time.time < _nextTime)
        {
            return;
        }
        if (Random.value >= _chance)
        {
            return;
        }
        _nextTime = Time.time + _cooldown;
        Instantiate(_summonPrefab, enemy.transform.position + Vector3.up * 0.2f, Quaternion.identity);
    }
}
