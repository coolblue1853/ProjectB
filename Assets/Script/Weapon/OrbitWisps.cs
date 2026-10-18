using DG.Tweening;
using UnityEngine;

/// <summary>
/// 공전 도깨비불 (몬스터 계보: 도깨비불 → Wisp Lantern R 스킬). 플레이어 주위를 돌며 각 불이 일정 간격으로 작은 피격 판정을 만든다.
/// 불 모양은 프리팹 자식(0번)을 복제해 개수만큼 만든다.
/// </summary>
public class OrbitWisps : MonoBehaviour
{
    [SerializeField] private GameObject _hitPrefab;
    [SerializeField] private int _count = 3;
    [SerializeField] private float _radius = 1.6f;
    [SerializeField] private float _degPerSec = 220f;
    [SerializeField] private float _life = 6f;
    [SerializeField] private float _hitInterval = 0.45f;
    [SerializeField] private float _damageScale = 0.35f;

    Transform[] orbs;
    float angle, nextHit;
    int[] damage;

    void Start()
    {
        damage = HomingOrb.WeaponDamage(_damageScale);
        Transform proto = transform.childCount > 0 ? transform.GetChild(0) : null;
        if (proto == null)
        {
            Destroy(gameObject);
            return;
        }
        orbs = new Transform[_count];
        orbs[0] = proto;
        for (int i = 1; i < _count; i++)
        {
            orbs[i] = Instantiate(proto.gameObject, transform).transform;
        }
        Vector3 baseScale = proto.localScale; // 원본 크기 유지 (1로 키우면 너무 크다)
        foreach (Transform o in orbs)
        {
            o.localScale = Vector3.zero;
            o.DOScale(baseScale, 0.25f).SetLink(o.gameObject);
            o.DOScale(0f, 0.3f).SetDelay(_life - 0.3f).SetLink(o.gameObject);
        }
        nextHit = 0.3f;
        Destroy(gameObject, _life);
    }

    void LateUpdate()
    {
        if (orbs == null)
        {
            return;
        }
        if (PlayerController.instance != null)
        {
            transform.position = PlayerController.instance.transform.position + Vector3.up * 0.8f;
        }
        angle += _degPerSec * Time.deltaTime;
        nextHit -= Time.deltaTime;
        bool hit = nextHit <= 0f;
        if (hit)
        {
            nextHit = _hitInterval;
        }
        for (int i = 0; i < orbs.Length; i++)
        {
            float a = (angle + i * 360f / orbs.Length) * Mathf.Deg2Rad;
            orbs[i].position = transform.position + new Vector3(Mathf.Cos(a) * _radius, Mathf.Sin(a) * _radius * 0.6f, 0f);
            if (hit && _hitPrefab != null)
            {
                GameObject h = Instantiate(_hitPrefab, orbs[i].position, Quaternion.identity);
                DamageObject d = h.GetComponent<DamageObject>();
                if (d != null)
                {
                    d.SetDamge(damage);
                    d.isSkill = true;
                }
            }
        }
    }
}
