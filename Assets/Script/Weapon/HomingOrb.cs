using UnityEngine;

/// <summary>
/// 유도 탄 (몬스터 계보: 도깨비불 → Wisp Lantern). 가까운 적을 향해 방향을 틀며 날아가고, 닿으면 피격 판정을 만들고 사라진다.
/// 피해는 소환 순간 장착 무기의 피해 배열 × 배율.
/// </summary>
public class HomingOrb : MonoBehaviour
{
    [SerializeField] private GameObject _hitPrefab;
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _turnRate = 540f; // 초당 회전 각도
    [SerializeField] private float _life = 2.5f;
    [SerializeField] private float _seekRadius = 9f;
    [SerializeField] private float _damageScale = 0.5f;

    Vector2 dir;
    float age;
    int[] damage;
    Transform target;
    Collider2D targetCol;
    float retarget;

    void Start()
    {
        Transform p = PlayerController.instance != null ? PlayerController.instance.transform : null;
        float face = p != null && p.localScale.x < 0f ? -1f : 1f;
        dir = new Vector2(face, Random.Range(0.3f, 0.9f)).normalized; // 위로 흩어지며 출발
        damage = WeaponDamage(_damageScale);
        Destroy(gameObject, _life);
    }

    public static int[] WeaponDamage(float scale)
    {
        var d = new int[10];
        Weapon w = Weapon.Equipped;
        if (w == null || w.damgeArray == null)
        {
            d[0] = 5;
            d[1] = 10;
            return d;
        }
        for (int i = 0; i < d.Length && i < w.damgeArray.Length; i++)
        {
            d[i] = i < 2 ? Mathf.Max(1, Mathf.RoundToInt(w.damgeArray[i] * scale)) : w.damgeArray[i];
        }
        return d;
    }

    void Update()
    {
        age += Time.deltaTime;
        retarget -= Time.deltaTime;
        if (retarget <= 0f)
        {
            retarget = 0.2f;
            target = Nearest();
            targetCol = target != null ? target.GetComponent<Collider2D>() : null;
        }
        if (target != null && age > 0.15f)
        {
            Vector2 aim = targetCol != null ? (Vector2)targetCol.bounds.center : (Vector2)target.position; // 판정 중심을 노린다
            Vector2 want = (aim - (Vector2)transform.position).normalized;
            float a = Vector2.SignedAngle(dir, want);
            float step = Mathf.Clamp(a, -_turnRate * Time.deltaTime, _turnRate * Time.deltaTime);
            dir = Quaternion.Euler(0f, 0f, step) * dir;
            if (Vector2.Distance(transform.position, aim) < 0.5f)
            {
                Burst();
                return;
            }
        }
        transform.position += (Vector3)(dir * _speed * Time.deltaTime);
    }

    Transform Nearest()
    {
        Transform best = null;
        float bestD = _seekRadius;
        foreach (EnemyHealth e in EnemyHealth.Active)
        {
            if (e == null || e.nowHP <= 0 || !e.IsMonster)
            {
                continue;
            }
            float d = Vector2.Distance(e.transform.position, transform.position);
            if (d < bestD)
            {
                bestD = d;
                best = e.transform;
            }
        }
        return best;
    }

    void Burst()
    {
        if (_hitPrefab != null)
        {
            GameObject hit = Instantiate(_hitPrefab, transform.position, Quaternion.identity);
            DamageObject d = hit.GetComponent<DamageObject>();
            if (d != null)
            {
                d.SetDamge(damage);
                d.isSkill = true;
            }
        }
        Destroy(gameObject);
    }
}
