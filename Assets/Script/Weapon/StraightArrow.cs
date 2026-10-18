using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직선 화살 (⑤구역 산적 궁수 → Bandit Shortbow). 플레이어가 보던 방향으로 곧게 날아가 몬스터에 닿으면 피격 판정을 만든다.
/// HomingOrb 와 같은 피해 규칙(소환 순간 장착 무기 피해 × 배율), 유도 없음. _pierce 만큼 관통한다.
/// 충돌은 트리거(Rigidbody2D Kinematic + Collider2D isTrigger)로 받는다 — 프리팹 레이어는 플레이어 판정 레이어.
/// 지면·벽(Ground·Wall)에 닿으면 사라진다(화살 비가 땅에 꽂힘, 벽 관통 방지). 이동은 Rigidbody2D 속도(물리 단계와 맞춤).
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class StraightArrow : MonoBehaviour
{
    [SerializeField] private GameObject _hitPrefab;
    [SerializeField] private float _speed = 16f;
    [SerializeField] private float _life = 0.7f;
    [SerializeField] private float _damageScale = 0.8f;
    [SerializeField] private int _pierce = 0;           // 추가로 관통하는 적 수
    [SerializeField] private float _angle = 0f;         // 발사 각도(도, 음수 = 아래로; 바라보는 방향 기준) — 화살 비는 -65
    [SerializeField] private float _angleJitter = 0f;   // 여러 발 쏠 때 퍼짐(도)
    [SerializeField] private Vector2 _spawnJitter = Vector2.zero; // 시작 위치 무작위 흩뿌림(화살 비)
    [SerializeField] private bool _isSkill = true;      // 평타 화살은 false

    private Vector2 _dir;
    private int[] _damage;
    private int _hits;
    private readonly HashSet<EnemyHealth> _struck = new HashSet<EnemyHealth>();
    private static int s_terrainMask = -1;

    private void Start()
    {
        Transform p = PlayerController.instance != null ? PlayerController.instance.transform : null;
        float face = p != null && p.localScale.x < 0f ? -1f : 1f;
        float jitter = _angleJitter > 0f ? Random.Range(-_angleJitter, _angleJitter) : 0f;
        _dir = Quaternion.Euler(0f, 0f, (_angle + jitter) * face) * new Vector2(face, 0f);
        if (_spawnJitter != Vector2.zero)
        {
            transform.position += new Vector3(Random.Range(-_spawnJitter.x, _spawnJitter.x), Random.Range(-_spawnJitter.y, _spawnJitter.y), 0f);
        }
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg);
        _damage = HomingOrb.WeaponDamage(_damageScale);
        GetComponent<Rigidbody2D>().linearVelocity = _dir * _speed;
        if (s_terrainMask < 0)
        {
            s_terrainMask = LayerMask.GetMask("Ground", "Wall");
        }
        Destroy(gameObject, _life);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hits > _pierce)
        {
            return; // Destroy 는 프레임 끝까지 미뤄진다 — 같은 물리 단계의 추가 타격 방지
        }
        if (!other.isTrigger && ((1 << other.gameObject.layer) & s_terrainMask) != 0)
        {
            _hits = _pierce + 1;
            Destroy(gameObject);
            return;
        }
        if (!other.CompareTag("Enemy") || !other.TryGetComponent(out EnemyHealth enemy) || !enemy.IsMonster || enemy.nowHP <= 0 || !_struck.Add(enemy))
        {
            return;
        }
        if (_hitPrefab != null)
        {
            GameObject hit = Instantiate(_hitPrefab, other.bounds.center, Quaternion.identity);
            if (hit.TryGetComponent(out DamageObject d))
            {
                d.SetDamge(_damage);
                d.isSkill = _isSkill;
            }
        }
        _hits++;
        if (_hits > _pierce)
        {
            Destroy(gameObject);
        }
    }
}
