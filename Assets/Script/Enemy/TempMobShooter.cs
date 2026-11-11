using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 숲 사냥꾼(trait=shoot)·유적 파수꾼(trait=rune): 플레이어가 사거리 안이면 잠깐 멈춰 준비한 뒤(Windup) 화살을 한 발 쏜다.
/// 화살은 Resources/FX/MobArrow(산적 화살 프리팹 복제, 새 그림 없음)를 정적 풀로 돌려 쓴다. 피해는 몸박 x 배율.
/// 준비 동안 추적은 멈춘다(EnemyHealth.MoveSpeedMul). 근접 BT 는 그대로 — 가까이 붙으면 평소처럼 벤다.
/// </summary>
public sealed class TempMobShooter : MonoBehaviour
{
    const string ArrowPath = "FX/MobArrow";
    static ObjectPool<GameObject> s_pool;
    static GameObject s_prefab;

    private EnemyHealth _enemy;
    private MobShootRules.Profile _profile;
    private Cooldown _readyAt, _fireAt;
    private bool _winding;
    private float _dir;

    /// <summary>추적 이동 배율: 준비 중 0</summary>
    public float ChaseMul => _winding ? 0f : 1f;

    public void Init(MobShootRules.Profile profile)
    {
        _profile = profile;
    }

    void Awake()
    {
        _enemy = GetComponent<EnemyHealth>();
    }

    void OnEnable()
    {
        _winding = false;
        _fireAt = default;
        _readyAt.Arm(Time.time, 1f); // 막 나타나자마자 쏘지 않는다
    }

    void Update()
    {
        if (_enemy == null || !_enemy.CanContact || _enemy.isStun)
        {
            _winding = false;
            return;
        }
        if (_winding)
        {
            if (_fireAt.Ready(Time.time))
            {
                _winding = false;
                Fire();
            }
            return;
        }
        PlayerController player = PlayerController.instance;
        if (player == null)
        {
            return;
        }
        Vector3 d = player.transform.position - transform.position;
        if (!MobShootRules.CanStart(_profile, d.x, d.y, _readyAt.Ready(Time.time)))
        {
            return;
        }
        _dir = Mathf.Sign(d.x);
        _winding = true;
        _fireAt.Arm(Time.time, _profile.Windup);
        _readyAt.Arm(Time.time, _profile.Windup + _profile.Cooldown);
    }

    void Fire()
    {
        int dmg = MobShootRules.ShotDamage(_enemy.ContactDamage, _profile.DamageMul);
        GameObject arrow = dmg > 0 ? GetArrow() : null;
        if (arrow == null)
        {
            return;
        }
        arrow.transform.SetParent(null);
        arrow.transform.position = transform.position + new Vector3(_dir * 0.6f, 0.5f, 0f);
        arrow.SetActive(true);
        if (arrow.TryGetComponent(out EnemyDamageObject shot))
        {
            shot.damage = dmg;
            shot.enemyOb = gameObject;
            shot.LaunchObject();
        }
    }

    static GameObject GetArrow()
    {
        if (s_pool == null)
        {
            s_prefab = Resources.Load<GameObject>(ArrowPath);
            if (s_prefab == null)
            {
                return null;
            }
            s_pool = new ObjectPool<GameObject>(Create, null, g => g.SetActive(false), DestroyArrow, true, 4, 16);
        }
        return s_pool.Get();
    }

    static GameObject Create()
    {
        GameObject go = Instantiate(s_prefab);
        go.SetActive(false);
        go.GetComponent<PoolAble>().pool = s_pool;
        return go;
    }

    static void DestroyArrow(GameObject g)
    {
        if (g != null)
        {
            Destroy(g);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        s_pool = null;
        s_prefab = null;
    }
}
