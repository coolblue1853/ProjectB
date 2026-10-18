using UnityEngine;

/// <summary>
/// ⑤구역 산적 단검병 계보(Cutthroat Dagger) "뒤로 돌아 찌르기": 판정이 생기는 순간 앞쪽 가장 가까운 몬스터 뒤로 순간 이동해
/// 돌아서서 찌른다. 판정은 플레이어 자식이라 함께 옮겨지고, 뒤를 잡았으므로 이 판정만 치명 확률·치명 피해를 올린다(백어택).
/// 대상이 없거나, 가는 길·대상 뒤·착지 지점이 지형(Ground·Wall)에 막히면 제자리 찌르기. 짧은 시간 만료형 무적으로 기습 순간을 보호.
/// </summary>
[RequireComponent(typeof(DamageObject))]
public class BackstabStep : MonoBehaviour
{
    [SerializeField] private float _range = 6f;        // 앞쪽 탐색 거리
    [SerializeField] private float _maxHeightGap = 1f; // 같은 층만 (다른 높이 발판으로 x 만 옮겨 끼이지 않게)
    [SerializeField] private float _behind = 0.9f;     // 대상 뒤로 설 거리
    [SerializeField] private int _bonusCritRate = 40;  // 백어택 치명 확률 +%
    [SerializeField] private int _bonusCritDmg = 30;   // 백어택 치명 피해 +%
    [SerializeField] private float _invincible = 0.3f;

    private static int s_blockMask = -1;
    private static readonly Vector2 LandingBox = new Vector2(0.4f, 0.6f);

    private DamageObject _damage;
    private bool _behindTarget;
    private int[] _boosted; // 보너스를 넣은 복사본 (나중에 공유 배열로 덮이면 다시 넣는다)

    private static int BlockMask
    {
        get
        {
            if (s_blockMask < 0)
            {
                s_blockMask = LayerMask.GetMask("Ground", "Wall");
            }
            return s_blockMask;
        }
    }

    private void Awake()
    {
        _damage = GetComponent<DamageObject>();
        PlayerController pc = PlayerController.instance;
        if (pc == null)
        {
            return;
        }
        Transform p = pc.transform;
        float face = p.localScale.x < 0f ? -1f : 1f;
        EnemyHealth target = FindTarget(p.position, face);
        if (target == null)
        {
            return;
        }
        Vector2 from = p.position;
        Vector2 to = new Vector2(target.transform.position.x + face * _behind, from.y);
        Vector2 up = Vector2.up * 0.3f;
        if (Physics2D.Linecast(from + up, to + up, BlockMask).collider != null           // 가는 길(대상 앞뒤 포함)에 벽
            || Physics2D.OverlapBox(to + Vector2.up * 0.2f, LandingBox, 0f, BlockMask) != null) // 착지 지점이 지형 속
        {
            return;
        }
        if (pc.mainCharacter != null)
        {
            DashAfterimage.Begin(pc.mainCharacter, _invincible);
        }
        p.position = new Vector3(to.x, p.position.y, p.position.z);
        Vector3 s = p.localScale;
        p.localScale = new Vector3(-s.x, s.y, s.z); // 돌아서서 대상을 본다 (B-129: 방향 API 생기면 교체)
        PlayerStatus.AcquireFor(StatusFlag.Invincible, _invincible, "BackstabStep");
        _behindTarget = true;
    }

    private void Start()
    {
        ApplyBonus();
    }

    private void LateUpdate()
    {
        // Skill.skillWaitTime > 0 이면 SetSkillDmg 가 Start 뒤에 공유 배열을 넣는다 → 그때 다시 보너스
        if (_behindTarget && _damage.damageArr != null && _damage.damageArr != _boosted)
        {
            ApplyBonus();
        }
    }

    private void ApplyBonus()
    {
        if (!_behindTarget || _damage.damageArr == null || _damage.damageArr.Length <= DamageCalculator.CritDmg || _damage.damageArr == _boosted)
        {
            return;
        }
        int[] arr = (int[])_damage.damageArr.Clone(); // 무기 공유 배열은 건드리지 않는다
        arr[DamageCalculator.CritRate] += _bonusCritRate;
        arr[DamageCalculator.CritDmg] += _bonusCritDmg;
        _boosted = arr;
        _damage.SetDamge(arr);
    }

    private EnemyHealth FindTarget(Vector2 origin, float face)
    {
        EnemyHealth best = null;
        float bestD = _range;
        foreach (EnemyHealth e in EnemyHealth.Active)
        {
            if (e == null || e.nowHP <= 0 || !e.IsMonster)
            {
                continue;
            }
            Vector2 to = (Vector2)e.transform.position - origin;
            if (to.x * face <= 0f || Mathf.Abs(to.y) > _maxHeightGap)
            {
                continue;
            }
            if (to.magnitude < bestD)
            {
                bestD = to.magnitude;
                best = e;
            }
        }
        return best;
    }
}
