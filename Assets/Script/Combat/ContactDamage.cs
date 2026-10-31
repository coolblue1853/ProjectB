using System.Collections.Generic;
using UnityEngine;

/// <summary>몸박(몹 접촉 피해) 순수 규칙 (ClassicMapleDesign.md 3.3 / 구현 2단계)</summary>
public static class ContactRules
{
    /// <summary>몸박 후 플레이어 피격 무적(초)</summary>
    public const float InvincibleTime = 0.9f;
    /// <summary>몸박 넉백 힘·경직 — 작게(일반 공격 넉백보다 약함)</summary>
    public const float KnockForce = 4f;
    public const float StiffTime = 0.12f;
    /// <summary>접촉 검사 주기(초). 몹 20마리 기준 싸게 돈다</summary>
    public const float CheckInterval = 0.1f;

    /// <summary>플레이어가 받는 몸박 원피해(방어 적용 전). 정예 x1.5 는 EnemyHealth.ContactDamage 에서 이미 반영</summary>
    public static int RawDamage(int contactDamage) => Mathf.Max(0, contactDamage);

    /// <summary>몹이 플레이어 오른쪽에 있으면 왼쪽(-1)으로 밀친다</summary>
    public static float KnockSign(float playerX, float mobX) => playerX >= mobX ? 1f : -1f;
}

/// <summary>
/// 몸박 시스템: 0.1초마다 활성 몬스터와 플레이어 콜라이더가 겹치는지 보고, 무적이 아니면 한 번 피해를 준다.
/// 같은 프레임의 몹 공격 판정과는 피격 무적(isNoDamge)을 공유해 이중 피해가 없다. 매 프레임 GetComponent 없음(콜라이더 캐시).
/// </summary>
public sealed class ContactDamage : MonoBehaviour
{
    private readonly Dictionary<EnemyHealth, Collider2D> _colliders = new Dictionary<EnemyHealth, Collider2D>(32);
    private readonly List<EnemyHealth> _stale = new List<EnemyHealth>(8);
    private Collider2D _playerCol;
    private PlayerHealthManager _hp;
    private float _nextCheck;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("[ContactDamage]");
        DontDestroyOnLoad(go);
        go.AddComponent<ContactDamage>();
    }

    private void Update()
    {
        if (Time.time < _nextCheck)
        {
            return;
        }
        _nextCheck = Time.time + ContactRules.CheckInterval;
        PlayerController pc = PlayerController.instance;
        if (pc == null)
        {
            return;
        }
        if (_hp == null)
        {
            _hp = PlayerHealthManager.Instance;
        }
        if (_hp == null || !_hp.CanBeHit)
        {
            return;
        }
        if (_playerCol == null)
        {
            _playerCol = pc.GetComponent<BoxCollider2D>();
            if (_playerCol == null)
            {
                return;
            }
        }
        Bounds pb = _playerCol.bounds;
        EnemyHealth hit = null;
        foreach (EnemyHealth e in EnemyHealth.Active)
        {
            if (e == null || !e.IsMonster || !e.CanContact || e.ContactDamage <= 0)
            {
                continue;
            }
            if (!_colliders.TryGetValue(e, out Collider2D col))
            {
                col = e.GetComponent<Collider2D>();
                _colliders[e] = col;
            }
            if (col == null || !col.enabled || !Overlap2D(col.bounds, pb))
            {
                continue;
            }
            if (hit == null || e.ContactDamage > hit.ContactDamage)
            {
                hit = e; // 동시에 여럿이면 가장 센 하나만 (무적이 곧 걸린다)
            }
        }
        if (hit != null)
        {
            Apply(hit, pc.transform);
        }
        PruneCache();
    }

    /// <summary>z 를 무시한 2D 겹침(맵마다 z 오프셋이 달라 Bounds.Intersects 는 못 쓴다)</summary>
    private static bool Overlap2D(Bounds a, Bounds b)
    {
        return a.min.x <= b.max.x && a.max.x >= b.min.x && a.min.y <= b.max.y && a.max.y >= b.min.y;
    }

    private void Apply(EnemyHealth mob, Transform player)
    {
        int raw = ContactRules.RawDamage(mob.ContactDamage);
        float sign = ContactRules.KnockSign(player.position.x, mob.transform.position.x);
        GameLog.Info(GameLog.Cat.Combat, "몸박 " + mob.name + " 원피해 " + raw + (mob.IsElite ? " [정예]" : ""), mob);
        _hp.damage2Player(raw, ContactRules.StiffTime, ContactRules.KnockForce, new Vector2(sign, 0.5f), mob.transform.position.x, false, ContactRules.InvincibleTime);
    }

    private void PruneCache()
    {
        if (_colliders.Count < 64)
        {
            return;
        }
        _stale.Clear();
        foreach (var kv in _colliders)
        {
            if (kv.Key == null || !EnemyHealth.Active.Contains(kv.Key))
            {
                _stale.Add(kv.Key);
            }
        }
        for (int i = 0; i < _stale.Count; i++)
        {
            _colliders.Remove(_stale[i]);
        }
    }
}
