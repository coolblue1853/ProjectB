using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모기 계보(Bloodsting Rapier) 흡혈 찌르기: 같은 판정 오브젝트의 DamageObject 가 적을 맞힐 때 피해 일부를 HP 로 회복한다.
/// 회복량 = 무기 평균 피해 × 판정 배율 × _ratio (적 1명당 1회, 판정의 최대 타격 수까지).
/// 실제 최종 피해(방어·치명 반영)는 EnemyHealth 내부 값이라 평균 피해로 근사한다.
/// </summary>
[RequireComponent(typeof(DamageObject))]
public class LifeStealHit : MonoBehaviour
{
    [SerializeField] private float _ratio = 0.2f;
    [SerializeField] private int _minHeal = 1;

    private DamageObject _damage;
    private readonly HashSet<Collider2D> _healed = new HashSet<Collider2D>();

    private void Awake()
    {
        _damage = GetComponent<DamageObject>();
    }

    private void OnEnable()
    {
        _healed.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy") || _healed.Contains(other) || _healed.Count >= Mathf.Max(1, _damage.maxDamagedEnemies))
        {
            return;
        }
        if (_damage.isHealSkill || PlayerHealthManager.Instance == null || !other.TryGetComponent(out EnemyHealth _))
        {
            return;
        }
        _healed.Add(other);
        PlayerHealthManager.Instance.HpHeal(HealAmount());
    }

    private int HealAmount()
    {
        int[] arr = _damage.damageArr;
        if ((arr == null || arr.Length < 2) && Weapon.Equipped != null)
        {
            arr = Weapon.Equipped.damgeArray;
        }
        float avg = arr != null && arr.Length >= 2 ? (arr[0] + arr[1]) * 0.5f : 0f;
        return Mathf.Max(_minHeal, Mathf.RoundToInt(avg * _damage.dmgRatio * _ratio));
    }
}
