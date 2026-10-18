using DG.Tweening;
using UnityEngine;

/// <summary>
/// 몹 근접 판정에 붙여 "짧은 돌진 물기"로 만든다(④구역 모기, B-124).
/// 판정이 풀에서 나온 첫 프레임에 소유 몹(부모 또는 EnemyDamageObject.enemyOb)을 플레이어 쪽으로 짧게 미끄러뜨린다.
/// 판정은 몹 자식이라 함께 움직인다. 이동 거리 = 플레이어까지 거리 − 멈출 간격, 최대 _maxDistance.
/// </summary>
[RequireComponent(typeof(EnemyDamageObject))]
public class EnemyLungeBite : MonoBehaviour
{
    [SerializeField] private float _maxDistance = 3.5f;
    [SerializeField] private float _stopShort = 0.7f;
    [SerializeField] private float _duration = 0.18f;

    private EnemyDamageObject _damage;
    private Tween _lunge;
    private bool _started;

    private void Awake()
    {
        _damage = GetComponent<EnemyDamageObject>();
    }

    private void OnEnable()
    {
        _started = false; // 부모 지정·enemyOb 설정은 풀에서 꺼낸 직후(같은 프레임)라 첫 Update 에서 시작한다
    }

    private void Update()
    {
        if (_started)
        {
            return;
        }
        _started = true;
        // 소유자는 공격 노드가 매번 지정하는 enemyOb 우선(재사용 판정에 이전 부모가 남아 있어도 엉뚱한 몹을 끌지 않게)
        Transform owner = _damage.enemyOb != null ? _damage.enemyOb.transform : transform.parent;
        if (owner == null || _damage.player == null)
        {
            return;
        }
        if (transform.parent == null)
        {
            transform.SetParent(owner, true); // 부모 없이 꺼낸 노드(isSetParent=false)여도 몹과 함께 움직이게. 반환 시 DestroyObject 가 떼어 낸다
        }
        float dx = _damage.player.transform.position.x - owner.position.x;
        float dist = Mathf.Clamp(Mathf.Abs(dx) - _stopShort, 0f, _maxDistance);
        if (dist <= 0.05f)
        {
            return;
        }
        _lunge?.Kill();
        _lunge = owner.DOMoveX(owner.position.x + Mathf.Sign(dx) * dist, _duration)
            .SetEase(Ease.OutQuad)
            .SetLink(owner.gameObject);
    }

    private void OnDisable()
    {
        _lunge?.Kill(); // 반환·부모 몹 반환 때 이동을 끝낸다
        _lunge = null;
    }
}
