using DG.Tweening;
using UnityEngine;

/// <summary>
/// 돌진형 소환수 (몬스터 계보 장비, TowerWorld 12절): 소환될 때 플레이어가 보던 방향으로 달리며 일정 간격으로 물기 판정을 만든다.
/// 수명이 끝나면 흐려지며 사라진다. 피해는 소환 순간 장착 무기의 피해 배열을 쓴다(무기 성장에 비례).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SummonRunner : MonoBehaviour
{
    [SerializeField] private GameObject _attackPrefab;
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _life = 1.6f;
    [SerializeField] private float _firstAttack = 0.15f;
    [SerializeField] private float _attackInterval = 0.35f;
    [SerializeField] private float _damageScale = 0.6f;

    SpriteRenderer sr;
    float dir = 1f, age, nextAttack;
    int[] damage;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Transform player = PlayerController.instance != null ? PlayerController.instance.transform : null;
        dir = player != null && player.localScale.x < 0f ? -1f : 1f;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
        nextAttack = _firstAttack;
        damage = new int[10];
        Weapon w = Weapon.Equipped;
        if (w != null && w.damgeArray != null)
        {
            for (int i = 0; i < damage.Length && i < w.damgeArray.Length; i++)
            {
                damage[i] = i < 2 ? Mathf.Max(1, Mathf.RoundToInt(w.damgeArray[i] * _damageScale)) : w.damgeArray[i];
            }
        }
        else
        {
            damage[0] = 5;
            damage[1] = 10;
        }
        sr.DOFade(0f, 0.3f).SetDelay(_life - 0.3f).SetLink(gameObject);
        Destroy(gameObject, _life);
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += new Vector3(dir * _speed * Time.deltaTime, 0f, 0f);
        if (_attackPrefab != null && age >= nextAttack && age < _life - 0.25f)
        {
            nextAttack += _attackInterval;
            GameObject hit = Instantiate(_attackPrefab, transform.position + new Vector3(dir * 0.6f, 0.4f, 0f), Quaternion.identity);
            DamageObject d = hit.GetComponent<DamageObject>();
            if (d != null)
            {
                d.SetDamge(damage);
                d.isSkill = true;
            }
        }
    }
}
